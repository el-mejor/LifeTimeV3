namespace LifeTimeV3.LifeTimeDiagram.DiagramBox.CopyPeriodicDialog
{
    partial class FormCopyPeriodicDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.labelElement = new System.Windows.Forms.Label();
            this.labelPeriod = new System.Windows.Forms.Label();
            this.numericUpDownPeriod = new System.Windows.Forms.NumericUpDown();
            this.comboBoxPeriodBase = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.numericUpDownAmmount = new System.Windows.Forms.NumericUpDown();
            this.radioButtonLimit = new System.Windows.Forms.RadioButton();
            this.radioButtonAmmount = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPeriod)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownAmmount)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(327, 145);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 0;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(12, 145);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // labelElement
            // 
            this.labelElement.AutoSize = true;
            this.labelElement.Location = new System.Drawing.Point(12, 9);
            this.labelElement.Name = "labelElement";
            this.labelElement.Size = new System.Drawing.Size(35, 13);
            this.labelElement.TabIndex = 2;
            this.labelElement.Text = "label1";
            // 
            // labelPeriod
            // 
            this.labelPeriod.AutoSize = true;
            this.labelPeriod.Location = new System.Drawing.Point(12, 35);
            this.labelPeriod.Name = "labelPeriod";
            this.labelPeriod.Size = new System.Drawing.Size(31, 13);
            this.labelPeriod.TabIndex = 2;
            this.labelPeriod.Text = "[401]";
            // 
            // numericUpDownPeriod
            // 
            this.numericUpDownPeriod.Location = new System.Drawing.Point(224, 33);
            this.numericUpDownPeriod.Name = "numericUpDownPeriod";
            this.numericUpDownPeriod.Size = new System.Drawing.Size(51, 20);
            this.numericUpDownPeriod.TabIndex = 3;
            this.numericUpDownPeriod.ValueChanged += new System.EventHandler(this.numericUpDownPeriod_ValueChanged);
            // 
            // comboBoxPeriodBase
            // 
            this.comboBoxPeriodBase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPeriodBase.FormattingEnabled = true;
            this.comboBoxPeriodBase.Location = new System.Drawing.Point(281, 33);
            this.comboBoxPeriodBase.Name = "comboBoxPeriodBase";
            this.comboBoxPeriodBase.Size = new System.Drawing.Size(121, 21);
            this.comboBoxPeriodBase.TabIndex = 4;
            this.comboBoxPeriodBase.SelectedIndexChanged += new System.EventHandler(this.comboBoxPeriodBase_SelectedIndexChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.dateTimePicker1);
            this.groupBox1.Controls.Add(this.numericUpDownAmmount);
            this.groupBox1.Controls.Add(this.radioButtonLimit);
            this.groupBox1.Controls.Add(this.radioButtonAmmount);
            this.groupBox1.Location = new System.Drawing.Point(12, 60);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(390, 71);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Location = new System.Drawing.Point(184, 39);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(200, 20);
            this.dateTimePicker1.TabIndex = 3;
            this.dateTimePicker1.ValueChanged += new System.EventHandler(this.dateTimePickerLimit_ValueChanged);
            // 
            // numericUpDownAmmount
            // 
            this.numericUpDownAmmount.Location = new System.Drawing.Point(184, 16);
            this.numericUpDownAmmount.Name = "numericUpDownAmmount";
            this.numericUpDownAmmount.Size = new System.Drawing.Size(79, 20);
            this.numericUpDownAmmount.TabIndex = 2;
            this.numericUpDownAmmount.ValueChanged += new System.EventHandler(this.numericUpDownAmmount_ValueChanged);
            // 
            // radioButtonLimit
            // 
            this.radioButtonLimit.AutoSize = true;
            this.radioButtonLimit.Location = new System.Drawing.Point(6, 39);
            this.radioButtonLimit.Name = "radioButtonLimit";
            this.radioButtonLimit.Size = new System.Drawing.Size(49, 17);
            this.radioButtonLimit.TabIndex = 1;
            this.radioButtonLimit.TabStop = true;
            this.radioButtonLimit.Text = "[403]";
            this.radioButtonLimit.UseVisualStyleBackColor = true;
            this.radioButtonLimit.CheckedChanged += new System.EventHandler(this.radioButtonLimit_CheckedChanged);
            // 
            // radioButtonAmmount
            // 
            this.radioButtonAmmount.AutoSize = true;
            this.radioButtonAmmount.Location = new System.Drawing.Point(6, 16);
            this.radioButtonAmmount.Name = "radioButtonAmmount";
            this.radioButtonAmmount.Size = new System.Drawing.Size(49, 17);
            this.radioButtonAmmount.TabIndex = 0;
            this.radioButtonAmmount.TabStop = true;
            this.radioButtonAmmount.Text = "[402]";
            this.radioButtonAmmount.UseVisualStyleBackColor = true;
            this.radioButtonAmmount.CheckedChanged += new System.EventHandler(this.radioButtonAmmount_CheckedChanged);
            // 
            // FormCopyPeriodicDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(414, 180);
            this.ControlBox = false;
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.comboBoxPeriodBase);
            this.Controls.Add(this.numericUpDownPeriod);
            this.Controls.Add(this.labelPeriod);
            this.Controls.Add(this.labelElement);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "FormCopyPeriodicDialog";
            this.Text = "[400]";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPeriod)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownAmmount)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelElement;
        private System.Windows.Forms.Label labelPeriod;
        private System.Windows.Forms.NumericUpDown numericUpDownPeriod;
        private System.Windows.Forms.ComboBox comboBoxPeriodBase;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButtonLimit;
        private System.Windows.Forms.RadioButton radioButtonAmmount;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.NumericUpDown numericUpDownAmmount;
    }
}