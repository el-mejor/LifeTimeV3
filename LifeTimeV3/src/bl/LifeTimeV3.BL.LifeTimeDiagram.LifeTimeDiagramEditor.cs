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
        public enum DrawComponent { Shadow, Object, Label, Text, All }
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
        public int ReferenceLine;
        public DateTime ReferenceLineDateTime;
        #endregion

        #region fields        
        private LifeTimeToolBoxForm _toolbox;
        private ILifeTimeObject CurrentObject;
        private MoveObject _moveObject;        
        #endregion

        #region events        
        public event EventHandler ObjectSelected;

        public event EventHandler DiagramChanged;
        public event MouseEventHandler MouseMoved;
        #endregion

        #region Constructor
        public LifeTimeDiagramEditor()
        {
            Diagram = new LifeTimeDiagram();

            RequestNewRandomColors = DrawNewRandomColor.Yes;

            CreateDiagramViewer();
            
            LoadToolbox();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Load a LifeTimeDiagram
        /// </summary>
        /// <param name="filename"></param>
        public void LoadDiagram(String filename)
        {            
            if (_toolbox != null) _toolbox.Dispose();                

            LifeTimeDiagramFileHandler open = new LifeTimeDiagramFileHandler(filename);            

            Diagram = open.OpenFile();

            DiagramViewer.Zoom = Diagram.Settings.Zoom;
            DiagramViewer.OffsetX = Diagram.Settings.OffsetX;
            DiagramViewer.OffsetY = Diagram.Settings.OffsetY;

            LoadToolbox();

            if (DiagramChanged != null) DiagramChanged(this, null);            
        }

        /// <summary>
        /// Creates a new LifeTimeDiagram
        /// </summary>
        /// <param name="filename"></param>
        public void NewDiagram(String filename)
        {
            if (_toolbox != null) _toolbox.Dispose();
            Diagram = new LifeTimeDiagram();

            LoadToolbox();

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

                if (CurrentObject != null) PropertyGrid.SetObject(CurrentObject);
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
        public void ExportPNG(string filename, int width, int height, 
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
                                                            LifeTimeElement.LifeTimeObjectType.Marker,
                                                            LifeTimeElement.LifeTimeObjectType.Text
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
        private void LoadToolbox()
        {
            if(_toolbox != null && !_toolbox.IsDisposed)
                _toolbox.Dispose();

            _toolbox = GetToolBoxForm();
            _toolbox.Visible = false;            
        }

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
            ObjectBrowser.ObjectCollectionChanged += new EventHandler(ObjectCollectionChanged);            
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

        #region DiagramViewer / Property Grid / ObjectBrowser Event Handler
        private void ObjectSelectedInObjectBrowser(object sender, LifeTimeObjectBrowser.ItemSelectedArgs e)
        {
            if (e.Object != null)
            {
                PropertyGrid.SetObject(e.Object);
                ObjectSelected?.Invoke(e.Object, null);
            }

            CurrentObject = e.Object;
        }

        private void ObjectCollectionChanged(object sender, EventArgs e)
        {
            Diagram.Changed = true;

            DiagramViewer.Refresh();

            DiagramChanged?.Invoke(this, null);
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

            //draw reference line
            if (Diagram.Settings.ShowRefLine && ReferenceLine > 0 && ReferenceLine < Diagram.Settings.Width)
                e.Graphics.DrawLine(new Pen(Color.Black, 1.0f), ReferenceLine, 0, ReferenceLine, Diagram.Settings.Height);
            
            RequestNewRandomColors = DrawNewRandomColor.No;
        }

        private void diagramViewer_Resize(object sender, EventArgs e)
        {
            DiagramViewer.Refresh();
        }

        private void diagramViewer_MouseDown(object sender, MouseEventArgs e)
        {
            //DiagramViewer.ContextMenuStrip = null;

            ILifeTimeObject o = SelectObjectByPosition(e.X, e.Y);

            if (o == null || (!(o is LifeTimeElement) || (o as LifeTimeElement).Type != LifeTimeElement.LifeTimeObjectType.Text || (o as LifeTimeElement).Locked))
                DiagramViewer.BeginMouse(e);
            else
            {
                _moveObject = new MoveObject(this, o as LifeTimeElement, DiagramViewer.Zoom);
                _moveObject.MoveObjectBegin(e);
            }
        }

        private void diagramViewer_MouseUp(object sender, MouseEventArgs e)
        {
            if (_moveObject == null || _moveObject.Object == null)
            {
                DiagramViewer.EndMouse(e);
                
                DiagramViewer.Refresh();

                if (!DiagramViewer.Scaling && !DiagramViewer.Moving)
                {
                    ILifeTimeObject o = SelectObjectByPosition(e.X, e.Y);

                    PropertyGrid.SetObject(o);

                    CurrentObject = o;

                    TreeNode t = ObjectBrowser.ShowItemInObjectBrowser(o);

                    if (t != null && e.Button == MouseButtons.Right)
                    {
                        DiagramViewer.ContextMenuStrip = t.ContextMenuStrip;
                        DiagramViewer.ContextMenuStrip.Show(Cursor.Position);
                    }
                    else if (DiagramViewer.ContextMenuStrip != null) DiagramViewer.ContextMenuStrip.Hide();
                }
            }   
            else
            {
                TreeNode t = ObjectBrowser.ShowItemInObjectBrowser(_moveObject.Object);

                if (t != null && e.Button == MouseButtons.Right)
                {
                    DiagramViewer.ContextMenuStrip = t.ContextMenuStrip;
                    DiagramViewer.ContextMenuStrip.Show(Cursor.Position);
                }
                else if (DiagramViewer.ContextMenuStrip != null) DiagramViewer.ContextMenuStrip.Hide();

                _moveObject.MoveObjectEnd(e);
            }        
        }

        private void diagramViewer_MouseMove(object sender, MouseEventArgs e)
        {
            ReferenceLine = Convert.ToInt32((e.X / DiagramViewer.Zoom) - Convert.ToInt32(DiagramViewer.OffsetX));

            if (_moveObject == null || _moveObject.Object == null)
            {
                DiagramViewer.MoveMouse(e);
                DiagramViewer.Refresh();
            }
            else
            {
                _moveObject.MoveObjectMove(e);
            }

            ReferenceLineDateTime = Diagram.GetDateTimeFromPos(ReferenceLine);
            if (Diagram.Settings.ShowRefLine && ReferenceLine > 0 && ReferenceLine < Diagram.Settings.Width)
                MouseMoved?.Invoke(this, e);                  
        }



        private void ExportButtonClick(object sender, EventArgs e)
        {
            ExportPNG(Diagram.ExportSettings.FileName, Diagram.ExportSettings.Width, Diagram.ExportSettings.Height);
        }
        #endregion

        #region Move Object Helping class
        private class MoveObject
        {
            public LifeTimeElement Object;

            private LifeTimeDiagramEditor _editorInstance;
            private int _movingObjectX;
            private int _movingObjectY;
            private int _movingObjectOriginX;
            private int _movingObjectOriginY;
            private float _currZoom;
            
            public MoveObject(LifeTimeDiagramEditor editorInstance, LifeTimeElement obj, float currZoom)
            {
                _editorInstance = editorInstance;
                Object = obj;
                _currZoom = currZoom;
            }

            public void MoveObjectBegin(MouseEventArgs e)
            {   
                _movingObjectX = e.X;
                _movingObjectY = e.Y;
                _movingObjectOriginX = Object.TextPosX;
                _movingObjectOriginY = Object.TextPosY;
            }

            public void MoveObjectMove(MouseEventArgs e)
            {
                Object.TextPosX = _movingObjectOriginX + Convert.ToInt32((e.X - _movingObjectX) / _currZoom);
                Object.TextPosY = _movingObjectOriginY + Convert.ToInt32((e.Y - _movingObjectY) / _currZoom);

                _editorInstance.DiagramViewer.Refresh();
            }

            public void MoveObjectEnd(MouseEventArgs e)
            {
                _editorInstance.CurrentObject = Object;
                _editorInstance.PropertyGrid.SetObject(_editorInstance.CurrentObject);
                TreeNode t = _editorInstance.ObjectBrowser.ShowItemInObjectBrowser(_editorInstance.CurrentObject);

                Object = null;

                _editorInstance.DiagramViewer.Refresh();
            }
        }
        #endregion
    }
}
