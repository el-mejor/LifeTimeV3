using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;

using LifeTimeV3.BL.LifeTimeDiagram;

namespace LifeTimeV3
{
    public partial class  FormLifeTimeUI : Form
    {
        #region Fields
        private LifeTimeDiagramEditor DiagramEditor;        
        #endregion

        #region constructor
        public FormLifeTimeUI()
        {
            InitializeComponent();

            DiagramEditor = new LifeTimeDiagramEditor();

            //ToolBox Form
            ShowToolbox();

            //DiagramViewer in Main Form
            this.Controls.Add(DiagramEditor.DiagramViewer);
            DiagramEditor.DiagramViewer.BringToFront();
            DiagramEditor.DiagramViewer.Dock = DockStyle.Fill;
            DiagramEditor.RequestNewRandomColors = LifeTimeDiagramEditor.DrawNewRandomColor.Yes;

            DiagramEditor.NewDiagram(null);

            this.BringToFront();
        }
        #endregion     

        #region private methods
        private void ShowToolbox()
        {
            if (!DiagramEditor.GetToolBoxForm().Visible) DiagramEditor.GetToolBoxForm().Show(this);
        }

        /// <summary>
        /// Check for unsaved changes. Returns true when cancel was chosen by the user.
        /// </summary>
        /// <returns></returns>
        private bool CheckForUnsavedChages()
        {
            if (DiagramEditor.DiagramChanged)
            {
                DialogResult r = MessageBox.Show("There are unsaved changes. Would you save the Diagram?", "Unsaved changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (r == DialogResult.Cancel) return true;
                else if (r == DialogResult.Yes) DiagramEditor.SaveDiagram(DiagramEditor.FileName);
            }

            return false;
        }
        #endregion

        #region eventhandler
        private void FormLifeTimeUI_KeyDown(object sender, KeyEventArgs e)
        {


        }

        private void FormLifeTimeUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = CheckForUnsavedChages();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DiagramEditor.FileName == null) saveAsToolStripMenuItem_Click(sender, e);                            
            else DiagramEditor.SaveDiagram(DiagramEditor.FileName);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(CheckForUnsavedChages()) return;             
            
            DiagramEditor.NewDiagram(null);          
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog SaveLifeTimeFile = new SaveFileDialog();
            SaveLifeTimeFile.Filter = "XML Dateien (*.xml)|*.xml|Alle Dateien (*.*)|*.*";
            if (SaveLifeTimeFile.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            DiagramEditor.FileName = SaveLifeTimeFile.FileName;

            DiagramEditor.SaveDiagram(DiagramEditor.FileName);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog OpenLifeTimeFile = new OpenFileDialog();
            OpenLifeTimeFile.Filter = "XML Dateien (*.xml)|*.xml|CSV Dateien (*.csv)|*.csv|Alle Dateien (*.*)|*.*";
            if (OpenLifeTimeFile.ShowDialog() != DialogResult.OK)
            {
                this.Close();
            }

            DiagramEditor.LoadDiagram(OpenLifeTimeFile.FileName);            
        }

        private void toolboxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowToolbox();
        }
        #endregion
    }
}
