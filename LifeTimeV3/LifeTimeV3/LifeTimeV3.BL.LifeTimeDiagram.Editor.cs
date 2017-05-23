using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

using LifeTimeV3.LifeTimeDiagram.Controls;
using LifeTimeV3.Src;

namespace LifeTimeV3.BL.LifeTimeDiagram
{   
    /// <summary>
    /// The LifeTimeDiagramEditor main class, containing the diagram and providing the toolbox form (and its controlls) and the diagram viewer.
    /// </summary>
    public partial class LifeTimeDiagramEditor
    {
        #region enums
        public enum DrawComponent { Shadow, Object, Label, All }
        public enum DrawNewRandomColor { Yes, No }
        public enum DrawStyle { WithShadow, WithoutShadow }
        #endregion

        #region Properties
        public String FileName 
        {
            get { return Diagram.FileName; }
            set { Diagram.FileName = value; }
        }
        public LifeTimeDiagram Diagram {get; set;}
        public Boolean DiagramChanged 
        {
            get { return Diagram.Changed; }
            set { Diagram.Changed = value; }
        }
        public DrawNewRandomColor RequestNewRandomColors;
        
        public LifeTimeDiagramBox DiagramViewer;
        public LifeTimeObjectPropertyGrid PropertyGrid;
        public LifeTimeObjectPropertyGrid SettingsGrid;
        public LifeTimeExportPNGPropertyGrid ExportGrid;
        public LifeTimeObjectBrowser ObjectBrowser;
        #endregion

        #region fields
        private LifeTimeToolBoxForm _toolbox;
        #endregion

        #region Constructor
        public LifeTimeDiagramEditor()
        {
            Diagram = new LifeTimeDiagram();

            RequestNewRandomColors = DrawNewRandomColor.Yes;

            CreateDiagramViewer();

            CreateToolBoxControlls();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Load a LifeTimeDiagram
        /// </summary>
        /// <param name="filename"></param>
        public void LoadDiagram(String filename)
        {
            LifeTimeDiagramFileHandler open = new LifeTimeDiagramFileHandler(filename);
            Diagram = open.OpenFile();

            DiagramViewer.Zoom = Diagram.Settings.Zoom;
            DiagramViewer.OffsetX = Diagram.Settings.OffsetX;
            DiagramViewer.OffsetY = Diagram.Settings.OffsetY;

            UpdateControls();
        }

        /// <summary>
        /// Creates a new LifeTimeDiagram
        /// </summary>
        /// <param name="filename"></param>
        public void NewDiagram(String filename)
        {
            Diagram = new LifeTimeDiagram();

            UpdateControls();
        }

        public void SaveDiagram()
        {
            SaveDiagram(Diagram.FileName);
        }
        
        /// <summary>
        /// Save the LifeTimeDiagram
        /// </summary>
        /// <param name="filename"></param>
        public void SaveDiagram(String filename)
        {
            Diagram.Settings.Zoom = DiagramViewer.Zoom;
            Diagram.Settings.OffsetX = DiagramViewer.OffsetX;
            Diagram.Settings.OffsetY = DiagramViewer.OffsetY;

            LifeTimeDiagramFileHandler save = new LifeTimeDiagramFileHandler(filename);
            save.SaveFile(Diagram);
        }

        /// <summary>
        /// Get the ToolBox Form
        /// </summary>
        /// <returns></returns>
        public LifeTimeToolBoxForm GetToolBoxForm()
        {
            if (_toolbox == null || _toolbox.IsDisposed)
            {
                CreateToolBoxControlls();

                _toolbox = new LifeTimeToolBoxForm(
                PropertyGrid,
                SettingsGrid,
                ExportGrid,
                ObjectBrowser
                );
            }

            return _toolbox;
        }

        /// <summary>
        /// Export the LifeTimeDiagram to a PNG file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="randomcolor"></param>
        /// <param name="drawcomponents"></param>
        /// <param name="style"></param>
        public void ExportPNG(String filename, int width, int height, 
            DrawNewRandomColor randomcolor = DrawNewRandomColor.Yes, DrawComponent drawcomponents = DrawComponent.All, DrawStyle style = DrawStyle.WithShadow)
        {
            LifeTimeDiagramFileHandler export = new LifeTimeDiagramFileHandler(filename);
            
            export.ExportPNG(Diagram, width, height, randomcolor, drawcomponents, style);
        }

        /// <summary>
        /// Try to return an object by giving the diagram position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public LifeTimeElement SelectObjectByPosition(int x, int y)
        {
            LifeTimeElement o;

            LifeTimeElement.LifeTimeObjectType[] types = { 
                                                            LifeTimeElement.LifeTimeObjectType.Event, //giving the order of searching
                                                            LifeTimeElement.LifeTimeObjectType.TimeSpan, 
                                                            LifeTimeElement.LifeTimeObjectType.Marker 
                                                        };

            foreach (LifeTimeElement.LifeTimeObjectType type in types)
            {
                Diagram.ObjectFences.TryGetValue(SelectObjectByPositionSeeker(type, x, y), out o);
                if (o != null) return o;
            }

            return null; 
        }
        #endregion

        #region Private Methods
        private void CreateToolBoxControlls()
        {
            PropertyGrid = new LifeTimeObjectPropertyGrid();
            SettingsGrid = new LifeTimeObjectPropertyGrid();
            ExportGrid = new LifeTimeExportPNGPropertyGrid();
            ObjectBrowser = new LifeTimeObjectBrowser();

            ObjectBrowser.UpdateObjectBrowser(Diagram.Groups);

            PropertyGrid.ObjectChanged += new LifeTimeObjectPropertyGrid.ObjectChangedEvent(ObjectChanged);
            ExportGrid.ExportButtonClick += new LifeTimeExportPNGPropertyGrid.ExportButtonClicked(ExportButtonClick);

            ObjectBrowser.ItemSelected += new LifeTimeObjectBrowser.ItemSelectedHandler(ObjectSelectedInObjectBrowser);
            ObjectBrowser.ObjectCollectionChanged += new LifeTimeObjectBrowser.ObjectCollectionChangedHandler(ObjectCollectionChanged);
        }

        private void CreateDiagramViewer()
        {
            DiagramViewer = new LifeTimeDiagramBox();

            DiagramViewer.Paint += new System.Windows.Forms.PaintEventHandler(this.diagramViewer_Paint);
            DiagramViewer.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.diagramViewer_MouseDoubleClick);
            DiagramViewer.MouseDown += new System.Windows.Forms.MouseEventHandler(this.diagramViewer_MouseDown);
            DiagramViewer.MouseMove += new System.Windows.Forms.MouseEventHandler(this.diagramViewer_MouseMove);
            DiagramViewer.MouseUp += new System.Windows.Forms.MouseEventHandler(this.diagramViewer_MouseUp);
            DiagramViewer.Resize += new System.EventHandler(this.diagramViewer_Resize);
        }

        private Rectangle SelectObjectByPositionSeeker(LifeTimeElement.LifeTimeObjectType seekFor, int x, int y)
        {
            Rectangle s = new Rectangle();

            foreach (Rectangle r in Diagram.ObjectFences.Keys)
            {
                LifeTimeElement o;
                Diagram.ObjectFences.TryGetValue(r, out o);
                if (o.Type == seekFor)
                {
                    float refX = (r.X + DiagramViewer.OffsetX) * DiagramViewer.Zoom;
                    float refWidth = r.Width * DiagramViewer.Zoom;
                    float refY = (r.Y + DiagramViewer.OffsetY) * DiagramViewer.Zoom;
                    float refHeight = r.Height * DiagramViewer.Zoom;

                    if ((x > refX && x < refX + refWidth) && (y > refY && y < refY + refHeight))
                    {
                        if(!o.Highlight) return r; //prefer an object which is not already selected
                        else s = r; //if there's no alternate object return the object which is already selected
                    }
                }

            }

            return s; //return no object if none was found (an empty rectangle as key)
        }

        private void UpdateObjectBrowser(bool LoadContent)
        {
            if (LoadContent) ObjectBrowser.UpdateObjectBrowser(Diagram.Groups);
            else ObjectBrowser.UpdateObjectBrowser();
        }

        private void UpdateControls()
        {
            UpdateObjectBrowser(true);

            SettingsGrid.SetObject(Diagram.Settings);
            ExportGrid.SetExportSettings(Diagram.ExportSettings);
        }
        #endregion

        #region DiagramViewer / Property Grid / ObjectBrowser Events
        private void ObjectSelectedInObjectBrowser(object sender, LifeTimeObjectBrowser.ItemSelectedArgs e)
        {
            if (e.LifeTimeObject != null) PropertyGrid.SetObject(e.LifeTimeObject);            
        }

        private void ObjectCollectionChanged(object sender, EventArgs e)
        {
            Diagram.Changed = true;

            DiagramViewer.Refresh();
        }

        /// <summary>
        /// An object was edited, update the diagram
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ObjectChanged(object sender, LifeTimeObjectPropertyGrid.ObjectChangedArgs e)
        {
            Diagram.Changed = true;
            
            UpdateObjectBrowser(false);

            if (e.NewColorsRequested) RequestNewRandomColors = DrawNewRandomColor.Yes;

            DiagramViewer.Refresh();            
        }

        /// <summary>
        /// Redraw the diagram
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void diagramViewer_Paint(object sender, PaintEventArgs e)
        {
            if (Diagram == null) return;

            PictureBox p = sender as PictureBox;

            e.Graphics.ScaleTransform(DiagramViewer.Zoom, DiagramViewer.Zoom);
            e.Graphics.TranslateTransform(DiagramViewer.OffsetX, DiagramViewer.OffsetY);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            Diagram.DrawDiagram(e.Graphics, Diagram.Settings.Width, Diagram.Settings.Height,
                RequestNewRandomColors,
                DrawComponent.All,
                DrawStyle.WithShadow);

            RequestNewRandomColors = DrawNewRandomColor.No;
        }

        /// <summary>
        /// Catch the resize event to redraw the diagram
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void diagramViewer_Resize(object sender, EventArgs e)
        {
            DiagramViewer.Refresh();
        }

        /// <summary>
        /// Catch the mouse down event to start moving of the diagram
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void diagramViewer_MouseDown(object sender, MouseEventArgs e)
        {
            DiagramViewer.ContextMenuStrip = null;
            DiagramViewer.BeginMouse(e);
        }

        /// <summary>
        /// Catch the mouse up event to end the moving of the diagram
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void diagramViewer_MouseUp(object sender, MouseEventArgs e)
        {
            DiagramViewer.EndMouse(e);
            DiagramViewer.Refresh();

            if (!DiagramViewer.Scaling && !DiagramViewer.Moving) 
            {
                ILifeTimeObject o = SelectObjectByPosition(e.X, e.Y);
                
                PropertyGrid.SetObject(o);
                TreeNode t = ObjectBrowser.ShowItemInObjectBrowser(o);

                if (t != null && e.Button == MouseButtons.Right)
                {
                    DiagramViewer.ContextMenuStrip = t.ContextMenuStrip;
                    DiagramViewer.ContextMenuStrip.Show(Cursor.Position);
                }
                else if (DiagramViewer.ContextMenuStrip != null) DiagramViewer.ContextMenuStrip.Hide();
            }            
        }

        /// <summary>
        /// Catch mosue movement to move the diagram while moving is enabled
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void diagramViewer_MouseMove(object sender, MouseEventArgs e)
        {
            DiagramViewer.MoveMouse(e);
            DiagramViewer.Refresh();
        }

        /// <summary>
        /// Catch double click to reset zoom and shift
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void diagramViewer_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            DiagramViewer.Reset();
            DiagramViewer.Refresh();
        }

        /// <summary>
        /// Export PNG was requested
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportButtonClick(object sender, EventArgs e)
        {
            ExportPNG(Diagram.ExportSettings.FileName, Diagram.ExportSettings.Width, Diagram.ExportSettings.Height);
        }
        #endregion       
    }
}
