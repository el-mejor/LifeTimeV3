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

        public List<LifeTimeDiagramEditor.LifeTimeElement> MultipliedObjectsCollection
        { get { return _multipliedObjects; } }

        public enum PeriodBaseEnum { Days, Month, Years};

        /// <summary>
        /// Base unit of the period (every [Period] [Day/Month/Year])
        /// </summary>
        public PeriodBaseEnum PeriodBase { get; set; }

        /// <summary>
        /// Period (every x [BaseUnit])
        /// </summary>
        public int Period { get; set; }

        /// <summary>
        /// Ammount of copies to add
        /// </summary>
        public int AmmountOfCopies { get; set; }

        /// <summary>
        /// Limit until copies are made
        /// </summary>
        public DateTime LimitForAddingCopies { get; set; }

        /// <summary>
        /// Switch to use either Limit or Ammount for adding copies
        /// </summary>
        public bool UseLimitForAddingCopies { get; set; }
        #endregion

        #region fields
        private List<LifeTimeDiagramEditor.LifeTimeElement> _multipliedObjects = new List<LifeTimeDiagramEditor.LifeTimeElement>();
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
