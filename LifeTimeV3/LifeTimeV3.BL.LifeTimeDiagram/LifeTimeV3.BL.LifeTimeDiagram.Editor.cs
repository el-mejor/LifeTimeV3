using System;
using System.Windows.Forms;
using System.Drawing;

using LifeTimeV3.LifeTimeDiagram.Toolbox;
using LifeTimeV3.LifeTimeDiagram.Toolbox.Controls;
using LifeTimeV3.LifeTimeDiagram.DiagramBox;
using LifeTimeV3.Src;
using System.Collections.Generic;

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
        public enum PeriodBaseEnum { Days, Month, Years };
        #endregion

        #region Properties
        public String FileName 
        {
            get { return Diagram.FileName; }
            set { Diagram.FileName = value; }
        }
        public LifeTimeDiagram Diagram {get; set;}
        public Boolean DiagramIsChanged 
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
        private ILifeTimeObject _currObj;
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
            if (_toolbox != null) _toolbox.Close();                

            LifeTimeDiagramFileHandler open = new LifeTimeDiagramFileHandler(filename);            

            Diagram = open.OpenFile();

            DiagramViewer.Zoom = Diagram.Settings.Zoom;
            DiagramViewer.OffsetX = Diagram.Settings.OffsetX;
            DiagramViewer.OffsetY = Diagram.Settings.OffsetY;

            UpdateControls();

            if (DiagramChanged != null) DiagramChanged(this, null);            
        }

        /// <summary>
        /// Creates a new LifeTimeDiagram
        /// </summary>
        /// <param name="filename"></param>
        public void NewDiagram(String filename)
        {
            if (_toolbox != null) _toolbox.Close();
            Diagram = new LifeTimeDiagram();

            UpdateControls();

            if (DiagramChanged != null) DiagramChanged(this, null);
        }

        /// <summary>
        /// Save the LifeTimeDiagram
        /// </summary>
        public void SaveDiagram()
        {
            SaveDiagram(Diagram.FileName);
        }
        
        /// <summary>
        /// Save the LifeTimeDiagram to a given file
        /// </summary>
        /// <param name="filename"></param>
        public void SaveDiagram(String filename)
        {
            Diagram.Settings.Zoom = DiagramViewer.Zoom;
            Diagram.Settings.OffsetX = DiagramViewer.OffsetX;
            Diagram.Settings.OffsetY = DiagramViewer.OffsetY;

            LifeTimeDiagramFileHandler save = new LifeTimeDiagramFileHandler(filename);
            save.SaveFile(Diagram);

            if (DiagramChanged != null) DiagramChanged(this, null);
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
                UpdateControls();

                _toolbox = new LifeTimeToolBoxForm(
                PropertyGrid,
                SettingsGrid,
                ExportGrid,
                ObjectBrowser
                );

                ObjectBrowser.UpdateObjectBrowser(Diagram.Groups);
                SettingsGrid.SetObject(Diagram.Settings);

                if (_currObj != null) PropertyGrid.SetObject(_currObj);
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



        public static List<LifeTimeElement> MultiplyElements(ILifeTimeObject element, PeriodBaseEnum periodBase, int period, int ammount)
        {
            LifeTimeElement e = element as LifeTimeElement;

            List<LifeTimeElement> l = new List<LifeTimeElement>();

            int offset = 0;

            for (int i = ammount; i > 0; i--)
            {                
                _addElementAndAddPeriod(e.Clone() as LifeTimeElement, periodBase, period + offset, l);
                offset += period;
            }
            
            return l;
        }

        public static List<LifeTimeElement> MultiplyElements(ILifeTimeObject element, PeriodBaseEnum periodBase, int period, DateTime limit)
        {
            LifeTimeElement e = element as LifeTimeElement;

            List<LifeTimeElement> l = new List<LifeTimeElement>();
            DateTime approachingLimit;

            int offset = 0;

            do
            {
                approachingLimit = _addElementAndAddPeriod(e.Clone() as LifeTimeElement, periodBase, period + offset, l);
                offset += period;
            } while (DateTime.Compare(approachingLimit, limit) < 0);

            return l;
        }
        #endregion

        #region Private Methods
        private static DateTime _addElementAndAddPeriod(LifeTimeElement element, PeriodBaseEnum periodBase, int value, List<LifeTimeElement> l)
        {
            element.Highlight = false;
            
            if (periodBase == PeriodBaseEnum.Days)
            {
                element.Begin = element.Begin.AddDays(value);
                element.End = element.End.AddDays(value);
            }
            if (periodBase == PeriodBaseEnum.Month)
            {
                element.Begin = element.Begin.AddMonths(value);
                element.End = element.End.AddMonths(value);
            }
            if (periodBase == PeriodBaseEnum.Years)
            {
                element.Begin = element.Begin.AddYears(value);
                element.End = element.End.AddYears(value);
            }

            l.Add(element);

            return element.Begin;        
        }

        private void CreateToolBoxControlls()
        {
            PropertyGrid = new LifeTimeObjectPropertyGrid();
            SettingsGrid = new LifeTimeObjectPropertyGrid();
            ExportGrid = new LifeTimeExportPNGPropertyGrid();
            ObjectBrowser = new LifeTimeObjectBrowser();

            PropertyGrid.ObjectChanged += new LifeTimeObjectPropertyGrid.ObjectChangedEvent(ObjectChanged);
            SettingsGrid.ObjectChanged += new LifeTimeObjectPropertyGrid.ObjectChangedEvent(ObjectChanged);
            ExportGrid.ExportButtonClick += new LifeTimeExportPNGPropertyGrid.ExportButtonClicked(ExportButtonClick);

            ObjectBrowser.ItemSelected += new LifeTimeObjectBrowser.ItemSelectedHandler(ObjectSelectedInObjectBrowser);
            ObjectBrowser.ObjectCollectionChanged += new LifeTimeObjectBrowser.ObjectCollectionChangedHandler(ObjectCollectionChanged);            
        }

        private void CreateDiagramViewer()
        {
            DiagramViewer = new LifeTimeDiagramBox();

            DiagramViewer.Paint += new System.Windows.Forms.PaintEventHandler(this.diagramViewer_Paint);
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

        private void UpdateObjectBrowser(bool ReloadContent)
        {
            if (ReloadContent) ObjectBrowser.UpdateObjectBrowser(Diagram.Groups);
            else ObjectBrowser.UpdateObjectBrowser();
        }

        private void UpdateControls()
        {
            CreateToolBoxControlls();
            UpdateObjectBrowser(true);
            
            SettingsGrid.SetObject(Diagram.Settings);
            
            ExportGrid.SetExportSettings(Diagram.ExportSettings);
        }
        #endregion

        #region DiagramViewer / Property Grid / ObjectBrowser Events
        private void ObjectSelectedInObjectBrowser(object sender, LifeTimeObjectBrowser.ItemSelectedArgs e)
        {
            if (e.LifeTimeObject != null)
            {
                PropertyGrid.SetObject(e.LifeTimeObject);                
                if (ObjectSelected != null) ObjectSelected(e.LifeTimeObject, null);
            }

            _currObj = e.LifeTimeObject;
        }

        private void ObjectCollectionChanged(object sender, EventArgs e)
        {
            Diagram.Changed = true;

            DiagramViewer.Refresh();

            if (DiagramChanged != null) DiagramChanged(this, null);
        }

        private void ObjectChanged(object sender, LifeTimeObjectPropertyGrid.ObjectChangedArgs e)
        {   
            UpdateObjectBrowser(false);

            if (e.NewColorsRequested) RequestNewRandomColors = DrawNewRandomColor.Yes;

            DiagramViewer.Refresh();

            if (DiagramChanged != null && e.DiagramChanged)
            {
                DiagramChanged(this, null);
                Diagram.Changed = true;
            }
        }

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

        private void diagramViewer_Resize(object sender, EventArgs e)
        {
            DiagramViewer.Refresh();
        }

        private void diagramViewer_MouseDown(object sender, MouseEventArgs e)
        {
            DiagramViewer.ContextMenuStrip = null;
            DiagramViewer.BeginMouse(e);
        }

        private void diagramViewer_MouseUp(object sender, MouseEventArgs e)
        {
            DiagramViewer.EndMouse(e);
            DiagramViewer.Refresh();

            if (!DiagramViewer.Scaling && !DiagramViewer.Moving) 
            {
                ILifeTimeObject o = SelectObjectByPosition(e.X, e.Y);

                if (o != null && ObjectSelected != null) ObjectSelected(o, e);

                PropertyGrid.SetObject(o);

                _currObj = o;

                TreeNode t = ObjectBrowser.ShowItemInObjectBrowser(o);                

                if (t != null && e.Button == MouseButtons.Right)
                {
                    DiagramViewer.ContextMenuStrip = t.ContextMenuStrip;
                    DiagramViewer.ContextMenuStrip.Show(Cursor.Position);
                }
                else if (DiagramViewer.ContextMenuStrip != null) DiagramViewer.ContextMenuStrip.Hide();
            }            
        }

        private void diagramViewer_MouseMove(object sender, MouseEventArgs e)
        {
            DiagramViewer.MoveMouse(e);
            DiagramViewer.Refresh();
        }

        private void ExportButtonClick(object sender, EventArgs e)
        {
            ExportPNG(Diagram.ExportSettings.FileName, Diagram.ExportSettings.Width, Diagram.ExportSettings.Height);
        }
        #endregion       

        #region EventHandler
        public delegate void ObjectSelectedEventHandler(object sender, EventArgs e);
        public event ObjectSelectedEventHandler ObjectSelected;

        public delegate void DiagramChangedEventHandler(object sender, EventArgs e);
        public event DiagramChangedEventHandler DiagramChanged;
        #endregion
    }
}
