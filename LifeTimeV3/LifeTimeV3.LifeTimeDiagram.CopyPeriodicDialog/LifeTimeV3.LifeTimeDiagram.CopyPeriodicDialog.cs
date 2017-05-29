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
using LifeTimeV3.Src;

namespace LifeTimeV3.LifeTimeDiagram.CopyPeriodicDialog
{
    public partial class FormCopyPeriodicDialog : Form
    {
        #region properties
        public LifeTimeDiagramEditor.LifeTimeElement Element
        { get; set; }

        /// <summary>
        /// Base unit of the period (every [Period] [Day/Month/Year])
        /// </summary>
        public LifeTimeDiagramEditor.PeriodBaseEnum PeriodBase { get; set; }

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
        #endregion

        #region constructor
        public FormCopyPeriodicDialog(LifeTimeDiagramEditor.ILifeTimeObject element)
        {
            InitializeComponent();

            Element = element as LifeTimeDiagramEditor.LifeTimeElement;

            //Setup default values
            PeriodBase = LifeTimeDiagramEditor.PeriodBaseEnum.Years;
            Period = 1;
            AmmountOfCopies = 10;
            LimitForAddingCopies = Element.Begin.AddYears(10);
            UseLimitForAddingCopies = true;

            //GUI

            dateTimePicker1.Value = LimitForAddingCopies;

            comboBoxPeriodBase.Items.Add(LifeTimeV3TextList.GetText("[404]"));
            comboBoxPeriodBase.Items.Add(LifeTimeV3TextList.GetText("[405]"));
            comboBoxPeriodBase.Items.Add(LifeTimeV3TextList.GetText("[406]"));
            comboBoxPeriodBase.SelectedIndex = 2;

            Text = LifeTimeV3TextList.GetText(Text);
            labelElement.Text = $"Element: {Element.Name}";
            labelPeriod.Text = LifeTimeV3TextList.GetText(labelPeriod.Text);
            radioButtonAmmount.Text = LifeTimeV3TextList.GetText(radioButtonAmmount.Text);
            radioButtonLimit.Text = LifeTimeV3TextList.GetText(radioButtonLimit.Text);
            radioButtonLimit.Checked = UseLimitForAddingCopies ? true : false;
            radioButtonAmmount.Checked = !radioButtonLimit.Checked ? true : false;

            numericUpDownPeriod.Value = Period;

        }
        #endregion

        #region private
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        #endregion

        private void numericUpDownPeriod_ValueChanged(object sender, EventArgs e)
        {
            Period = Convert.ToInt16(numericUpDownPeriod.Value);
        }

        private void radioButtonAmmount_CheckedChanged(object sender, EventArgs e)
        {
            UseLimitForAddingCopies = false;
        }

        private void radioButtonLimit_CheckedChanged(object sender, EventArgs e)
        {
            UseLimitForAddingCopies = true;
        }

        private void dateTimePickerLimit_ValueChanged(object sender, EventArgs e)
        {
            LimitForAddingCopies = dateTimePicker1.Value;
            radioButtonLimit.Checked = true;
        }

        private void numericUpDownAmmount_ValueChanged(object sender, EventArgs e)
        {
            AmmountOfCopies = Convert.ToInt16(numericUpDownAmmount.Value);
            radioButtonAmmount.Checked = true;
        }

        private void comboBoxPeriodBase_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((string)comboBoxPeriodBase.SelectedItem == LifeTimeV3TextList.GetText("[404]"))
                PeriodBase = LifeTimeDiagramEditor.PeriodBaseEnum.Days;
            if ((string)comboBoxPeriodBase.SelectedItem == LifeTimeV3TextList.GetText("[405]"))
                PeriodBase = LifeTimeDiagramEditor.PeriodBaseEnum.Month;
            if ((string)comboBoxPeriodBase.SelectedItem == LifeTimeV3TextList.GetText("[406]"))
                PeriodBase = LifeTimeDiagramEditor.PeriodBaseEnum.Years;
        }
    }
}
