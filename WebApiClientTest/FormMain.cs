using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Windows.Forms;
using WebApiSelfHostTest;
using WebApiSelfHostTest.Models;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using System.Drawing;
using System.Net.Http.Headers;

namespace WebApiClientTest
{
    public partial class FormMain : Form
    {
        private AppSetting appCfg;
        private AppSettingUI appCfgUI;

        private BindingSource bsProd = null;
        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            try
            {
                appCfg = AppSetting.Load();
                appCfgUI = AppSettingUI.Load();

                // restore window position
                if (appCfgUI.WindowPosition != Rectangle.Empty)
                {
                    DesktopBounds = appCfgUI.WindowPosition;
                    if (appCfgUI.WindowState == FormWindowState.Maximized)
                        WindowState = FormWindowState.Maximized;
                    else
                        WindowState = FormWindowState.Normal;
                }

                if (chbOnTop.Checked == appCfgUI.AlwaysOnTop)
                    chbOnTop_CheckedChanged(null, null);
                else
                    chbOnTop.Checked = appCfgUI.AlwaysOnTop;  // envoke chbOnTop_CheckedChanged automatically
            }
            catch (Exception ex) { GM.ShowErrorMessageBox(this, "Initialization of application setting failed!", ex); }

            tbURL.Text = new Uri(new Uri(appCfg.UrlBase), "/api/product/").ToString();

            dgProd.AutoGenerateColumns = false;
            dgProd.DataSource = bsProd = new BindingSource(new List<Product>(), "");
            foreach (DataGridViewColumn col in dgProd.Columns)
            {
                //                col.HeaderCell.ContextMenuStrip = cmuGridHdr;  // add context menu to header
                col.HeaderCell.Style.Alignment = col.DefaultCellStyle.Alignment;  // ensure right alignment for headers
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // save window position
                if (appCfgUI == null)
                    appCfgUI = new AppSettingUI();

                appCfgUI.WindowPosition = (WindowState == FormWindowState.Normal) ? DesktopBounds : RestoreBounds;
                appCfgUI.WindowState = WindowState;
                appCfgUI.AlwaysOnTop = chbOnTop.Checked;
                appCfgUI.Save();
            }
            catch { }
        }

        private void chbOnTop_CheckedChanged(object sender, EventArgs e)
        {
            TopMost = chbOnTop.Checked;
        }

        private void onAdd_Click(object sender, EventArgs e)
        {
            string strUrl = tbURL.Text;

            using ( DlgProduct dlg = new DlgProduct() )
            {
                // propose new ID
                try
                {
                    List<Product> arrProd = (bsProd.DataSource as List<Product>);
                    if ( arrProd != null && arrProd.Count > 0 )
                        dlg.Data.ID = arrProd.Max(p => p.ID)+1;
                }
                catch {}

                if ( dlg.ShowDialog(this) != DialogResult.OK )
                    return;

                try
                {
                    dgProd.EndEdit();
                    bsProd.EndEdit();

                    string strJObj = JsonConvert.SerializeObject(dlg.Data);
                    HttpResponseMessage response = sendRequest(strUrl, HttpMethod.Post, new StringContent(strJObj, Encoding.UTF8, "application/json"));
                    string respContent = response.Content.ReadAsStringAsync().Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        GM.ShowErrorMessageBox(this, formatRespMessage(respContent, string.Format("Adding product on server failed with error {0}", response.StatusCode)));
                        return;
                    }
                
                    bsProd.Add(new Product(dlg.Data));
    //                UpdateGrid(rowIdx);
                }
                catch (Exception ex) { GM.ShowErrorMessageBox(this, "Error occured when adding new product!", ex); }
            }
        }
        private HttpResponseMessage sendRequest(string strUrl, HttpMethod method, HttpContent content = null)
		{
            if ( string.IsNullOrEmpty(strUrl) )
            {
                tbURL.Focus();
                throw new Exception("Enter URL");
			}

            HttpClient client = new HttpClient();
            // general way - create request and send it; 
            // for specific HTTP methods, there are specific HttpClient functions, see. 
            //      client.PostAsync(strUrl, new StringContent(strJObj, Encoding.UTF8, "application/json"));
            //      client.DeleteAsync(new Uri(strUrl)).Result;
            //      ...
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(strUrl);
            request.Method = method;
            if ( !string.IsNullOrEmpty(appCfg.AuthorizationKeyBase64))             // basic authorization - do not use in real application
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", appCfg.AuthorizationKeyBase64);
//              client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", appCfg.AuthorizationKeyBase64);

            if ( content != null )
                request.Content = content;

            return client.SendAsync(request).Result;    
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            int rowIdx = (dgProd.CurrentRow == null) ? -1 : dgProd.CurrentRow.Index;  // gridLines.CurrentCell.RowIndex;
            if (rowIdx < 0 || rowIdx >= dgProd.Rows.Count)
            {
                GM.ShowErrorMessageBox(this, "No product selected!");
                return;
            }
            editProduct(rowIdx);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            int rowIdx = (dgProd.CurrentRow == null) ? -1 : dgProd.CurrentRow.Index;  // gridLines.CurrentCell.RowIndex;
            if (rowIdx < 0 || rowIdx >= bsProd.Count) //dgProd.Rows.Count)
            {
                GM.ShowErrorMessageBox(this, "No product selected!");
                return;
            }
            try
            {
                dgProd.EndEdit();
                bsProd.EndEdit();

                Product delProd = bsProd[rowIdx] as Product;

                string strUrl = new Uri(new Uri(tbURL.Text), delProd.ID.ToString()).ToString();
                HttpResponseMessage response = sendRequest(strUrl, HttpMethod.Delete);
                string respContent = response.Content.ReadAsStringAsync().Result;
                if (!response.IsSuccessStatusCode)
                {
                    GM.ShowErrorMessageBox(this, formatRespMessage(respContent, string.Format("Server returned error {0} when deleting product ID = {1}", response.StatusCode, delProd.ID)));
                    return;
                }
                
                bsProd.RemoveAt(rowIdx);
                UpdateGrid(rowIdx);
            }
            catch (Exception ex) { GM.ShowErrorMessageBox(this, "Error occured when deleting product!", ex); }
        }

        private void btnLoadProduct_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                string strUrl = tbURL.Text;
                HttpResponseMessage response = sendRequest(strUrl, HttpMethod.Get);
                string respContent = response.Content.ReadAsStringAsync().Result;
                if (!response.IsSuccessStatusCode)
                {
                    GM.ShowErrorMessageBox(this, formatRespMessage(respContent, string.Format("Server returned error when loadig product list: {0}", response.StatusCode)));
                    return;
                }
                if (string.IsNullOrEmpty(respContent))
                    GM.ShowInfoMessageBox(this, string.Format("No data returned from server!{0}URL: {1}", Environment.NewLine, strUrl));
                else
                {
                    dgProd.EndEdit();
                    bsProd.EndEdit();

                    bsProd.SuspendBinding();
                    bsProd.Clear();
                    try
                    {
                        JArray ar = JArray.Parse(respContent);
                        foreach (JObject t in ar.Children())
                            bsProd.Add(new Product(Convert.ToInt32(t["id"]), Convert.ToString(t["code"]), Convert.ToString(t["name"])));
                    }
                    catch (Exception ex)
                    {
                        GM.ShowErrorMessageBox(this, string.Format("Reading response failed{0}URL: {1}{0}Response: {2}", Environment.NewLine, strUrl, respContent), ex);
                        return;
                    }
                    bsProd.SuspendBinding();
                    dgProd.Refresh();
                }
            }
            catch (Exception ex) { GM.ShowErrorMessageBox(this, "Error occured when loading product list!", ex); }
            finally { Cursor = Cursors.Default; }
        }

        private void dgProd_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            // set selection after right mouse click - to hightlight row
            if (e.Button == MouseButtons.Right)
            {
                DataGridView dg = sender as DataGridView;
                if (!dg.Focused)
                    dg.Focus();
                if (dg != null && e.RowIndex >= 0 && e.RowIndex < dg.Rows.Count && !dg.Rows[e.RowIndex].Selected) // already selected 
                    dg.CurrentCell = dg.Rows[e.RowIndex].Cells[dg.CurrentCell == null ? 0 : dg.CurrentCell.ColumnIndex];
            }
        }

        private void dgProd_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgProd.Rows.Count)
                return;

            editProduct(e.RowIndex);
        }
        private void editProduct(int rowIdx)
        {
            if (rowIdx < 0 || rowIdx >= dgProd.Rows.Count)
                return;

            try
            {
                dgProd.EndEdit();
                bsProd.EndEdit();

                Product prod = bsProd[rowIdx] as Product;

                string strUrl = new Uri(new Uri(tbURL.Text), prod.ID.ToString()).ToString();
                // reload product from server
                HttpResponseMessage response = sendRequest(strUrl, HttpMethod.Get);
                string respContent = response.Content.ReadAsStringAsync().Result;
                if (!response.IsSuccessStatusCode)
                {
                    GM.ShowErrorMessageBox(this, formatRespMessage(respContent, string.Format("Server returned error {0} when loadig product ID = {1}", response.StatusCode, prod.ID)));
                    return;
                }
                if (string.IsNullOrEmpty(respContent))
                    GM.ShowInfoMessageBox(this, string.Format("Return value is empty!{0}URL: {1}", Environment.NewLine, strUrl));
                else // reload product with given ID from server and edit it
                {
                    try
                    {
                        JObject jProd = JObject.Parse(respContent);
                        prod.Code = Convert.ToString(jProd["code"]);
                        prod.Name = Convert.ToString(jProd["name"]);
                    }
                    catch (Exception ex)
                    {
                        GM.ShowErrorMessageBox(this, string.Format("Reading response failed{0}URL: {1}{0}Response: {2}", Environment.NewLine, strUrl, respContent), ex);
                        return;
                    }

                    using (DlgProduct dlg = new DlgProduct(prod))
                    {
                        if (dlg.ShowDialog(this) != DialogResult.OK)
                            return;

                        string strJObj = JsonConvert.SerializeObject(dlg.Data);
                        // Update == PUT
                        response = sendRequest(strUrl, HttpMethod.Put, new StringContent(strJObj, Encoding.UTF8, "application/json"));
                        respContent = response.Content.ReadAsStringAsync().Result;
                        if (!response.IsSuccessStatusCode)
                        {
                            string s = string.Format("Updating product on server failed with error {0}", response.StatusCode);
                            if (!string.IsNullOrEmpty(respContent))
                                s += string.Format("{0}{0}{1}", Environment.NewLine, respContent);

                            GM.ShowErrorMessageBox(this, s);
                            return;
                        }

                        prod.CopyFrom(dlg.Data);
                    }
                    UpdateGrid(rowIdx);
                }
            }
            catch (Exception ex) { GM.ShowErrorMessageBox(this, "Error occured when editing product!", ex); }
//            finally { dgProd.Focus(); }
        }
        private string formatRespMessage(string respContent, string msg)
		{
            if (!string.IsNullOrEmpty(respContent))
                try
                {
                    JObject retObj = JObject.Parse(respContent);
                    if (retObj["Message"] != null)
                        msg += string.Format("{0}{0}{1}", Environment.NewLine, retObj["Message"]);
                }
                catch { }

            return msg;
        }

        private void UpdateGrid(int selRow)
        {
            try
            {
                if (selRow >= 0 && selRow < dgProd.RowCount)
                {
                    //                dgProd.ClearSelection();
                    dgProd.FirstDisplayedScrollingRowIndex = selRow;
                    dgProd.CurrentCell = dgProd.Rows[selRow].Cells[(dgProd.CurrentCell==null)?0: dgProd.CurrentCell.ColumnIndex];
                }
                else if (dgProd.CurrentCell != null && dgProd.CurrentCell.RowIndex >= 0)
                    dgProd.CurrentCell = dgProd.Rows[dgProd.CurrentCell.RowIndex].Cells[dgProd.CurrentCell.ColumnIndex];
                else if (dgProd.RowCount > 0)
                    dgProd.CurrentCell = dgProd.Rows[0].Cells[(dgProd.CurrentCell == null) ? 0 : dgProd.CurrentCell.ColumnIndex];

                if (dgProd.CurrentCell != null &&
                     (dgProd.SelectedCells == null || dgProd.SelectedCells.Count == 0 || dgProd.SelectedCells[0] != dgProd.CurrentCell))
                    dgProd.CurrentCell.Selected = true;
            }
            catch { }    // FirstDisplayedScrollingRowIndex vyhodi exception, pokud je okno prilis male a DataGridView ma malou velikost
                         // dgProd.CurrentCell.Selected = true vyhodi nekdy chybu DataGridView InvalidOperationException reentrant call to SetCurrentCellAddressCore

            dgProd.Refresh();
        }
    }
}
