using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LifeTimeV3.BL.LifeTimeDiagram;

namespace LifeTimeV3.LifeTimeDiagram.CopyPeriodicDialog
{
    public partial class FormCopyPeriodicDialog : Form
    {
        #region properties
        public LifeTimeDiagramEditor.LifeTimeElement Object
        { get; set; }

        #endregion

        #region fields
        #endregion

        #region constructor
        public FormCopyPeriodicDialog()
        {
            InitializeComponent();
        }
        #endregion

        #region private
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
        #endregion
    }
}
