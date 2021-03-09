using System;
using System.Windows.Forms;

namespace WebApiClientTest
{
    public class GM
    {
#region Message boxes
        public static void ShowErrorMessageBox(IWin32Window wndOwner, String text, Exception ex = null)
        {
            if (string.IsNullOrEmpty(text))
                text = "Error occured!";
            if (ex != null)
            {
                text += string.Format("{0}{0}{1}", Environment.NewLine, ex.Message);
                for (Exception subEx = ex.InnerException; subEx != null; subEx = subEx.InnerException)
                    text += string.Format("{0}{1}", Environment.NewLine, subEx.Message);

                text += string.Format("{0}{0}{1}", Environment.NewLine, ex.ToString());
            }

            MessageBox.Show(wndOwner, text, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
        public static void ShowInfoMessageBox(IWin32Window wndOwner, String text)
        {
            MessageBox.Show(wndOwner, text, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public static DialogResult ShowQuestionMessageBox(IWin32Window wndOwner, String text)
        {
            return MessageBox.Show(wndOwner, text, Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }
#endregion
    }
}
