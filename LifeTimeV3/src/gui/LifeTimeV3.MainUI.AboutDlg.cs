using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Reflection;

namespace LifeTimeV3.MainUI
{
    public partial class LifeTimeV3AboutDlg : Form
    {
        #region constructor
        public LifeTimeV3AboutDlg()
        {
            InitializeComponent();

            labelVersion.Text = $"V{Assembly.GetEntryAssembly().GetName().Version.Major}.{Assembly.GetEntryAssembly().GetName().Version.Minor}.{Assembly.GetEntryAssembly().GetName().Version.Build}";
            if (Assembly.GetEntryAssembly().GetName().Version.Revision > 0)
                labelVersion.Text += $"-beta{Assembly.GetEntryAssembly().GetName().Version.Revision}";

            using (StreamReader s = new StreamReader("License.md"))
            {
                while(!s.EndOfStream)
                    textBoxLicense.Text += s.ReadLine() + Environment.NewLine;
            }
        }
        #endregion

        #region eventhandler
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion

        private void label5_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(label5.Text);
        }

        private void label6_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(label6.Text);
        }

        private void label8_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(label8.Text);
        }
    }
}
