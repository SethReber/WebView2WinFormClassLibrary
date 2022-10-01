using System;
using System.Windows.Forms;
using WebView2WindowsFormsBrowser;

namespace Forms
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                using (BrowserForm browserForm = new BrowserForm())
                {
                    await browserForm.InitializeAsync();
                    browserForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}