using System;
using System.Windows.Forms;
using WebApiSelfHostTest.Models;

namespace WebApiClientTest
{
    public partial class DlgProduct : Form
    {
        public Product Data = new Product();
        public DlgProduct(Product data = null)
        {
            InitializeComponent();

            nudID.Enabled = (data == null); // new product, 

            Data.CopyFrom(data);

			Shown += delegate(object sender, EventArgs e) { tbCode.Focus(); };
		}

		private void DlgProduct_Load(object sender, System.EventArgs e)
        {
			try 
            {
                tbCode.Text = Data.Code;
                tbName.Text = Data.Name;

                nudID.Minimum = 1;
                nudID.Maximum = int.MaxValue;
                nudID.Value = Data.ID;
            } 
            catch(Exception ex)
            {
                GM.ShowErrorMessageBox(this, "Initiazation product form failed", ex);
            }
        }

        private void DlgProduct_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ( DialogResult != DialogResult.OK )
                return;

            Data.ID = (int)nudID.Value;
            //
            Data.Code = tbCode.Text.Trim();
            if ( string.IsNullOrEmpty(Data.Code) )
            {
                GM.ShowErrorMessageBox(this, "Enter code");
                tbCode.Focus();
                e.Cancel = true;
                return;
            }
            //
            Data.Name = tbName.Text.Trim();
            if (string.IsNullOrEmpty(Data.Name))
            {
                GM.ShowErrorMessageBox(this, "Enter name");
                tbName.Focus();
                e.Cancel = true;
                return;
            }
        }
	}
}
