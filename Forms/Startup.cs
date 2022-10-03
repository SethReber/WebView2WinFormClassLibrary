using System;
using System.Windows.Forms;

namespace Forms
{
    public class Startup
    {
        [STAThread]
        public static void Main(string[] commandLineParameters)
        {
            Application.EnableVisualStyles();
            Application.Run(new Login());
        }
    }
}