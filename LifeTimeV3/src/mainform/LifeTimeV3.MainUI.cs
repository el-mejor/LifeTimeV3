using System;
using System.Windows.Forms;

using LifeTimeV3.BL.LifeTimeDiagram;
using LifeTimeV3.Src;
using System.Collections.Generic;

namespace LifeTimeV3.MainUI
{
    public partial class  FormLifeTimeMainUI : Form
    {
        #region Fields
        private LifeTimeDiagramEditor _diagramEditor;                
        #endregion

        #region constructor
        public FormLifeTimeMainUI()
        {
            InitializeComponent();

            //GUI
            toolStripMenuItem1.Text = LifeTimeV3TextList.GetText(toolStripMenuItem1.Text);
            newToolStripMenuItem.Text = LifeTimeV3TextList.GetText(newToolStripMenuItem.Text);
            openToolStripMenuItem.Text = LifeTimeV3TextList.GetText(openToolStripMenuItem.Text);
            saveToolStripMenuItem.Text = LifeTimeV3TextList.GetText(saveToolStripMenuItem.Text);
            saveAsToolStripMenuItem.Text = LifeTimeV3TextList.GetText(saveAsToolStripMenuItem.Text);
            exitToolStripMenuItem.Text = LifeTimeV3TextList.GetText(exitToolStripMenuItem.Text);

            _diagramEditor = new LifeTimeDiagramEditor();
            _diagramEditor.ObjectSelected += new EventHandler(ObjectSelected);
            _diagramEditor.DiagramChanged += new EventHandler(DiagramChanged);

            labelInfo.Visible = false;

            ClearAndAddMainMenuElementGenericItems();

            ShowDiagramViewer();
        }
        #endregion     

        #region private methods
        /// <summary>
        /// Show the Diagram in the main UI
        /// </summary>
        private void ShowDiagramViewer()
        {
            this.Controls.Add(_diagramEditor.DiagramViewer);
            _diagramEditor.DiagramViewer.SendToBack();
            _diagramEditor.DiagramViewer.Dock = DockStyle.Fill;
            _diagramEditor.RequestNewRandomColors = LifeTimeDiagramEditor.DrawNewRandomColor.Yes;
            _diagramEditor.DiagramViewer.MouseDoubleClick += new MouseEventHandler(DiagramDoubleClick);
            _diagramEditor.DiagramViewer.DiagramZoomed += new LifeTimeDiagram.DiagramBox.LifeTimeDiagramBox.DiagramZoomedEventHandler(DiagramZoomed);

            _diagramEditor.NewDiagram(null);
        }
        
        /// <summary>
        /// Show the toolbox form
        /// </summary>
        private void ShowToolbox()
        {
            if (!_diagramEditor.GetToolBoxForm().Visible) _diagramEditor.GetToolBoxForm().Show(this);
        }

        /// <summary>
        /// Check for unsaved changes. Returns true when cancel was chosen by the user.
        /// </summary>
        /// <returns></returns>
        private bool CheckForUnsavedChages()
        {
            if (_diagramEditor.DiagramIsChanged)
            {
                DialogResult r = MessageBox.Show("There are unsaved changes. Would you save the Diagram?", "Unsaved changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (r == DialogResult.Cancel) return true;
                else if (r == DialogResult.Yes) _diagramEditor.SaveDiagram(_diagramEditor.FileName);
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
            if (_diagramEditor.FileName == null) saveAsToolStripMenuItem_Click(sender, e);                            
            else _diagramEditor.SaveDiagram(_diagramEditor.FileName);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(CheckForUnsavedChages()) return;             
            
            _diagramEditor.NewDiagram(null);

            zoomSlider.Value = Convert.ToInt16(_diagramEditor.DiagramViewer.Zoom * 50);

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

            _diagramEditor.FileName = SaveLifeTimeFile.FileName;

            _diagramEditor.SaveDiagram(_diagramEditor.FileName);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CheckForUnsavedChages()) return;  

            OpenFileDialog OpenLifeTimeFile = new OpenFileDialog();
            OpenLifeTimeFile.Filter = "XML Dateien (*.xml)|*.xml|CSV Dateien (*.csv)|*.csv|Alle Dateien (*.*)|*.*";
            if (OpenLifeTimeFile.ShowDialog() != DialogResult.OK)
                return;
            
            _diagramEditor.LoadDiagram(OpenLifeTimeFile.FileName);

            zoomSlider.Value = Convert.ToInt16(_diagramEditor.DiagramViewer.Zoom * 50);
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
                
                ClearAndAddMainMenuElementGenericItems();

                LifeTimeDiagram.Toolbox.Controls.LifeTimeObjectBrowser.LifeTimeObjectTreeNode t = _diagramEditor.ObjectBrowser.ShowItemInObjectBrowser(o);

                if (t != null)
                    foreach (ToolStripItem i in t.BuildContextMenu(false, true))
                        eLEMENTToolStripMenuItem.DropDownItems.Add(i);
            }
            else
            {
                toolStripObjectStatusLabel.Text = "";

                ClearAndAddMainMenuElementGenericItems();
            }
        }

        private void ClearAndAddMainMenuElementGenericItems()
        {            
            eLEMENTToolStripMenuItem.DropDownItems.Clear();            
        }
        
        private void DiagramChanged(object sender, EventArgs e)
        {
            if (_diagramEditor.Diagram != null)
                Text = $"LifeTimeV3 - {_diagramEditor.FileName}";
            else
                Text = $"LifeTimeV3";

            if (_diagramEditor.DiagramIsChanged) toolStripDiagramStatusLabel.Text = LifeTimeV3TextList.GetText("[217]"); //unsaved changes
            if (!_diagramEditor.DiagramIsChanged) toolStripDiagramStatusLabel.Text = "";
        }

        private void DiagramDoubleClick(object sender, EventArgs e)
        {
            ShowToolbox();
        }
        
        private void resetZoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _diagramEditor.DiagramViewer.Reset();
            _diagramEditor.DiagramViewer.Refresh();
        }

        private void DiagramZoomed(object sender, LifeTimeDiagram.DiagramBox.LifeTimeDiagramBox.ZoomEventArgs e)
        {
            zoomSlider.Value = Convert.ToInt16(e.Zoom * 50);
        }

        private void zoomSlider_Scroll(object sender, EventArgs e)
        {
            if (zoomSlider.Value == 0) zoomSlider.Value = 1;
            _diagramEditor.DiagramViewer.Zoom = zoomSlider.Value / 50.0f;
            _diagramEditor.DiagramViewer.Refresh();
        }       
        #endregion
    }
}
