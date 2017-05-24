﻿using System;
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
        public LifeTimeDiagramEditor.ILifeTimeObject Object
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
        public FormCopyPeriodicDialog(LifeTimeDiagramEditor.ILifeTimeObject Object)
        {
            InitializeComponent();

            this.Object = Object;

            //Setup default values
            PeriodBase = PeriodBaseEnum.Years;
            Period = 1;
            AmmountOfCopies = 10;
            LimitForAddingCopies = DateTime.Now.AddYears(10);
            UseLimitForAddingCopies = true;

            //GUI
            Text = LifeTimeV3TextList.GetText(Text);
            labelElement.Text = $"Element: {Object.Name}";
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
        }

        private void numericUpDownAmmount_ValueChanged(object sender, EventArgs e)
        {
            AmmountOfCopies = Convert.ToInt16(numericUpDownAmmount.Value);
        }
    }
}
