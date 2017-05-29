using System;
using System.Windows.Forms;

using LifeTimeV3.BL.LifeTimeDiagram;
using LifeTimeV3.Src;

namespace LifeTimeV3.MainUI
{
    public partial class  FormLifeTimeMainUI : Form
    {
        #region Fields
        private LifeTimeDiagramEditor DiagramEditor;        
        #endregion

        #region constructor
        public FormLifeTimeMainUI()
        {
            InitializeComponent();

            DiagramEditor = new LifeTimeDiagramEditor();
            DiagramEditor.ObjectSelected += new LifeTimeDiagramEditor.ObjectSelectedEventHandler(ObjectSelected);
            DiagramEditor.DiagramChanged += new LifeTimeDiagramEditor.DiagramChangedEventHandler(DiagramChanged);

            ShowDiagramViewer();
        }
        #endregion     

        #region private methods
        /// <summary>
        /// Show the Diagram in the main UI
        /// </summary>
        private void ShowDiagramViewer()
        {
            this.Controls.Add(DiagramEditor.DiagramViewer);
            DiagramEditor.DiagramViewer.SendToBack();
            DiagramEditor.DiagramViewer.Dock = DockStyle.Fill;
            DiagramEditor.RequestNewRandomColors = LifeTimeDiagramEditor.DrawNewRandomColor.Yes;
            DiagramEditor.DiagramViewer.MouseDoubleClick += new MouseEventHandler(DiagramDoubleClick);
            DiagramEditor.DiagramViewer.DiagramZoomed += new LifeTimeDiagram.DiagramBox.LifeTimeDiagramBox.DiagramZoomedEventHandler(DiagramZoomed);

            DiagramEditor.NewDiagram(null);
        }
        
        /// <summary>
        /// Show the toolbox form
        /// </summary>
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
            if (DiagramEditor.DiagramIsChanged)
            {
                DialogResult r = MessageBox.Show("There are unsaved changes. Would you save the Diagram?", "Unsaved changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (r == DialogResult.Cancel) return true;
                else if (r == DialogResult.Yes) DiagramEditor.SaveDiagram(DiagramEditor.FileName);
            }

            return false;
        }
        #endregion

        #region eventhandler
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

            zoomSlider.Value = Convert.ToInt16(DiagramEditor.DiagramViewer.Zoom * 50);

            ShowToolbox();
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
            if (CheckForUnsavedChages()) return;  

            OpenFileDialog OpenLifeTimeFile = new OpenFileDialog();
            OpenLifeTimeFile.Filter = "XML Dateien (*.xml)|*.xml|CSV Dateien (*.csv)|*.csv|Alle Dateien (*.*)|*.*";
            if (OpenLifeTimeFile.ShowDialog() != DialogResult.OK)
                return;
            
            DiagramEditor.LoadDiagram(OpenLifeTimeFile.FileName);

            zoomSlider.Value = Convert.ToInt16(DiagramEditor.DiagramViewer.Zoom * 50);
        }

        private void toolboxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowToolbox();
        }

        private void ObjectSelected(object sender, EventArgs e)
        {
            //ShowToolbox();
            if (sender is LifeTimeDiagramEditor.LifeTimeElement)
            {
                LifeTimeDiagramEditor.LifeTimeElement o = sender as LifeTimeDiagramEditor.LifeTimeElement;

                toolStripObjectStatusLabel.Text =
                    string.Format(LifeTimeV3TextList.GetText("[303]"), //Element name, time ago, duration
                    o.Name,
                    ((DateTime.Now - o.Begin).Days / 365.0).ToString("F1"),
                    (DateTime.Now - o.Begin).Days.ToString(),
                    (o.GetTimeSpan(LifeTimeDiagramEditor.LifeTimeElement.TimeSpanBase.Days) / 365.0).ToString("F1"),
                    o.GetTimeSpan(LifeTimeDiagramEditor.LifeTimeElement.TimeSpanBase.Days).ToString());
            }
            else
            {
                toolStripObjectStatusLabel.Text = "";
            }
        }

        private void DiagramChanged(object sender, EventArgs e)
        {
            if (DiagramEditor.Diagram != null)
                Text = $"LifeTimeV3 - {DiagramEditor.FileName}";
            else
                Text = $"LifeTimeV3";

            if (DiagramEditor.DiagramIsChanged) toolStripDiagramStatusLabel.Text = LifeTimeV3TextList.GetText("[217]"); //unsaved changes
            if (!DiagramEditor.DiagramIsChanged) toolStripDiagramStatusLabel.Text = "";
        }

        private void DiagramDoubleClick(object sender, EventArgs e)
        {
            ShowToolbox();
        }
        
        private void resetZoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DiagramEditor.DiagramViewer.Reset();
            DiagramEditor.DiagramViewer.Refresh();
        }

        private void DiagramZoomed(object sender, LifeTimeDiagram.DiagramBox.LifeTimeDiagramBox.ZoomEventArgs e)
        {
            zoomSlider.Value = Convert.ToInt16(e.Zoom * 50);
        }

        private void zoomSlider_Scroll(object sender, EventArgs e)
        {
            if (zoomSlider.Value == 0) zoomSlider.Value = 1;
            DiagramEditor.DiagramViewer.Zoom = zoomSlider.Value / 50.0f;
            DiagramEditor.DiagramViewer.Refresh();
        }
        #endregion
    }
}
