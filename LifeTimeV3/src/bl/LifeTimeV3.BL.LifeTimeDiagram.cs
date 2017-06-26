using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;

namespace LifeTimeV3.BL.LifeTimeDiagram
{
    partial class LifeTimeDiagramEditor
    {
        #region LifeTimeDiagram Class
        /// <summary>
        /// The class containing and modelling the diagram itself
        /// </summary>
        public class LifeTimeDiagram : IDisposable
        {
            #region Enums
            #endregion

            #region events
            public delegate void DiagramMessageHandler(object sender, DiagramMessageArgs e);
            public event DiagramMessageHandler DiagramMessage;
            #endregion

            #region Properties
            //Basics
            public string Name { get; set; }
            public string Description { get; set; }
            public string FileName { get; set; }
            public bool Changed { get; set; }
            //public Dictionary<Rectangle, LifeTimeElement> ObjectFences = new Dictionary<Rectangle, LifeTimeElement>();
            public List<ElementFence> ElementFences = new List<ElementFence>();

            //Elements
            public LifeTimeGroup Groups;

            //Settings and Configuration
            public LifeTimeDiagramSettings Settings { get; set; }
            public LifeTimeExportSettings ExportSettings { get; set; }
            #endregion

            #region Fields
            private CreateRandomColor RandomColor;
            #endregion

            #region Cosntructors
            public LifeTimeDiagram()
            {
                Groups = new LifeTimeGroup("Root", Color.White);
                Settings = new LifeTimeDiagramSettings();
                ExportSettings = new LifeTimeExportSettings();
                Changed = false;
                RandomColor = new CreateRandomColor();
            }

            public void Dispose()
            {
                this.Settings = null;
                this.ExportSettings = null;
                this.Groups = null;                
            }
            #endregion

            #region Public Methods
            /// <summary>
            /// Draw the diagram
            /// </summary>
            /// <param name="g"></param>
            /// <param name="width"></param>
            /// <param name="height"></param>
            /// <param name="rndColor"></param>
            /// <param name="components"></param>
            public void DrawDiagram(Graphics g, int width, int height, DrawNewRandomColor rndColor, DrawComponent components, DrawStyle style)
            {
                List<string> exColl = new List<string>();

                ElementFences.Clear();

                DiagramDrawer draw = new DiagramDrawer(width, height, Settings, style);
                List<LifeTimeElement> o = GetAllObjects(true);

                //Draw Diagram Area
                g.FillRectangle(new SolidBrush(Settings.BackColor), 0, 0, width, height);
                
                //Show hints if necessary
                if (this.Groups.Groups.Count == 0)
                {
                    DiagramMessageArgs dma = new DiagramMessageArgs(Src.LifeTimeV3TextList.GetText("[308]"), DiagramMessageArgs.MsgPriorities.Info); //state that there is no group with elements
                    DiagramMessage?.Invoke(this, dma);

                    return;
                }
                else if (o.Count == 0)
                {
                    DiagramMessageArgs dma = new DiagramMessageArgs(Src.LifeTimeV3TextList.GetText("[307]"), DiagramMessageArgs.MsgPriorities.Info); //state that there is nothing to draw
                    DiagramMessage?.Invoke(this, dma);

                    return;
                }
                
                DiagramMessageArgs e = new DiagramMessageArgs(DiagramMessageArgs.MsgPriorities.None); //no info label
                DiagramMessage?.Invoke(this, e);
                

                //Draw Diagram components
                if (components == DrawComponent.All)
                {
                    if (Settings.DrawShadows)
                        exColl.AddRange(DrawDiagramComponent(draw, o, g, rndColor, DrawComponent.Shadow));
                    exColl.AddRange(DrawDiagramComponent(draw, o, g, rndColor, DrawComponent.Object));
                    exColl.AddRange(DrawDiagramComponent(draw, o, g, rndColor, DrawComponent.Label));
                    exColl.AddRange(DrawDiagramComponent(draw, o, g, rndColor, DrawComponent.Text));
                }
                else
                    exColl.AddRange(DrawDiagramComponent(draw, o, g, rndColor, components));

                //show hint that something went wrong
                if (exColl.Count > 0)
                {
                    errOutputOnDiag(g, exColl);

                    DiagramMessageArgs dma = new DiagramMessageArgs($"{Src.LifeTimeV3TextList.GetText("[309]")} {exColl[0]}", DiagramMessageArgs.MsgPriorities.Error); //state that there was an error while drawing
                    DiagramMessage?.Invoke(this, dma);
                }
                else
                {
                    DiagramMessageArgs dma = new DiagramMessageArgs(DiagramMessageArgs.MsgPriorities.None); //no info label
                    DiagramMessage?.Invoke(this, dma);
                }
            }

            private void errOutputOnDiag(Graphics g, List<string> exColl)
            {   
                StringBuilder sb = new StringBuilder();                
                foreach (var ex in exColl)
                    sb.Append(ex).Append(Environment.NewLine);
                
                SizeF s = g.MeasureString(sb.ToString(), new Font(new FontFamily("Arial"), 8.0f, FontStyle.Bold));

                g.FillRectangle(new SolidBrush(Color.Red), Settings.Border, Settings.Border, s.Width + 20, s.Height + 20);

                g.DrawString(sb.ToString(), new Font(new FontFamily("Arial"), 8.0f, FontStyle.Bold), new SolidBrush(Color.White), new PointF(Settings.Border + 10, Settings.Border + 10));
            }

            public void PrintDiagram(PrintDocument prntDoc)
            {   
                prntDoc.PrintPage += PrintPage;
                prntDoc.Print();
            }

            /// <summary>
            /// Returns a collection of all Objects in the Diagram. 
            /// </summary>
            /// <param name="g"></param>
            /// <returns></returns>
            public List<LifeTimeElement> GetAllObjects(Boolean SkipDisabled)
            {
                int row = -2;
                List<LifeTimeElement> c = new List<LifeTimeElement>();

                String path = "Root";

                foreach (LifeTimeGroup _g in Groups.Groups)
                {
                    if (SkipDisabled && !_g.Enabled) continue;
                    row += 2;
                    row = GetAllObjectsDeep(c, _g, row, path, SkipDisabled);
                }

                return c;
            }
            
            public DateTime GetDateTimeFromPos(int x)
            {
                return Settings.Begin.AddDays((Settings.End - Settings.Begin).TotalDays * (Convert.ToDouble(x - Settings.Border) / Convert.ToDouble(Settings.Width - Settings.Border * 2)));
            }
            #endregion

            #region Private Methods
            private List<string> DrawDiagramComponent(DiagramDrawer draw, List<LifeTimeElement> o, Graphics g, DrawNewRandomColor rndColor, DrawComponent components)
            {
                List<string> exColl = new List<string>();
                //New random color
                foreach (LifeTimeElement _o in o)
                {
                    if (rndColor == DrawNewRandomColor.Yes && _o.GetRandomColor) _o.Color = RandomColor.CreateNewRandomColor(_o.BaseColor);
                }

                //Draw the types
                LifeTimeElement.LifeTimeObjectType[] types = { 
                                                                LifeTimeElement.LifeTimeObjectType.Marker, 
                                                                LifeTimeElement.LifeTimeObjectType.TimeSpan, 
                                                                LifeTimeElement.LifeTimeObjectType.Event,
                                                                LifeTimeElement.LifeTimeObjectType.Text
                                                            };

                foreach (LifeTimeElement.LifeTimeObjectType type in types)
                {   
                    exColl.AddRange(DrawObjectsOfType(type, draw, o, g, components));                    
                }

                return exColl;
            }

            private List<string> DrawObjectsOfType(LifeTimeElement.LifeTimeObjectType type, DiagramDrawer draw, List<LifeTimeElement> o, Graphics g, DrawComponent components)
            {
                List<string> exColl = new List<string>();

                foreach (LifeTimeElement _o in o)
                {
                    if (!_o.Deleted && _o.Type == type)
                    {
                        try
                        {
                            foreach (ElementFence fence in draw.DrawObject(g, _o, components))
                                if (fence.FenceRectangle.Width > 0 && fence.FenceRectangle.Height > 0)//components == DrawComponent.Object || components == DrawComponent.Text)
                                {
                                    AddObjectFenceToDictionary(fence);

                                    if (Properties.Settings.Default.DebugMode)
                                    {
                                        Pen fenceMarker = new Pen(Color.OrangeRed);
                                        fenceMarker.DashPattern = new float[] { 3.0f, 3.0f };

                                        g.DrawRectangle(fenceMarker, fence.FenceRectangle);
                                        g.DrawString($"{fence.FenceRectangle.X} {fence.FenceRectangle.Y} {fence.FenceRectangle.Width} {fence.FenceRectangle.Height}", new Font(new FontFamily("Courier New"), 6.0f),
                                        new SolidBrush(Color.OrangeRed), fence.FenceRectangle.X, fence.FenceRectangle.Y);
                                    }
                                }
                        }
                        catch (Exception ex)
                        {
                            exColl.Add($"Element: \"{_o.Name}\" - \"{ex.Message}\"");
                        }

                        if (exColl.Count > 5)
                            break;
                    }
                }

                return exColl;
            }

            private int GetAllObjectsDeep(List<LifeTimeElement> c, LifeTimeGroup g, int row, string path, Boolean SkipDisabled)
            {
                foreach (LifeTimeElement _o in g.Objects)
                {
                    if (SkipDisabled && !_o.Enabled) continue;
                    _o.Row = row;

                    c.Add(_o);
                }

                foreach (LifeTimeGroup _g in g.Groups)
                {
                    if (SkipDisabled && !_g.Enabled) continue;
                    row += 1;
                    row = GetAllObjectsDeep(c, _g, row, path, SkipDisabled);
                }

                return row;
            }

            private void AddObjectFenceToDictionary(ElementFence fence)
            {
                if (!ElementFences.Any(f => f == fence)) ElementFences.Add(fence);
            }

            private void PrintPage(object sender, PrintPageEventArgs e)
            {
                float scaleX = Convert.ToSingle(e.PageBounds.Width) / Convert.ToSingle(Settings.Width);
                float scaleY = Convert.ToSingle(e.PageBounds.Height) / Convert.ToSingle(Settings.Height);
                scaleX = scaleX < scaleY ? scaleX : scaleY;

                e.Graphics.ScaleTransform(scaleX, scaleX);
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                DrawDiagram(e.Graphics, Settings.Width, Settings.Height, DrawNewRandomColor.No, DrawComponent.All, Settings.DrawShadows ? DrawStyle.WithShadow : DrawStyle.WithoutShadow);
            }
            #endregion

            #region DiagramMessageArgs Class
            public class DiagramMessageArgs
            {
                public enum MsgPriorities { None, Info, Tip, Error }

                public string Message
                { get; set; }
                public MsgPriorities MsgPriority
                { get; set; }

                public DiagramMessageArgs(string msg, MsgPriorities msgPrio) : this(msgPrio)
                {
                    Message = msg;                    
                }

                public DiagramMessageArgs(MsgPriorities msgPrio)
                {
                    MsgPriority = msgPrio;
                }

            }
            #endregion

            #region DiagramDrawer Class
            private class DiagramDrawer
            {
                #region enums
                #endregion

                #region Properties
                public int Width { get; set; }
                public int Height { get; set; }
                public LifeTimeDiagramSettings Settings { get; set; }
                public DrawStyle Style { get; set; }
                public Double PixelPerDay
                {
                    get { return Convert.ToDouble(Width - 2 * Settings.Border) / (Convert.ToDouble((Settings.End - Settings.Begin).Days)); }
                }
                #endregion

                #region contructor
                public DiagramDrawer(int width, int height, LifeTimeDiagramSettings s, DrawStyle style)
                {
                    Settings = s;
                    Width = width; Height = height;
                    Style = style;

                }

                #endregion

                #region Public Methods
                public List<ElementFence> DrawObject(Graphics g, LifeTimeElement o, DrawComponent components)
                {
                    List<ElementFence> fences = new List<ElementFence>();

                    if (o.Type == LifeTimeElement.LifeTimeObjectType.Event)
                    {
                        fences.AddRange(DrawObjectEvent(g, o, components));
                    }
                    if (o.Type == LifeTimeElement.LifeTimeObjectType.TimeSpan)
                    {
                        fences.AddRange(DrawObjectTimeSpan(g, o, components));
                    }
                    if (o.Type == LifeTimeElement.LifeTimeObjectType.Marker)
                    {
                        fences.AddRange(DrawObjectMarker(g, o, components));
                    }
                    if (o.Type == LifeTimeElement.LifeTimeObjectType.Text)
                    {
                        fences.AddRange(DrawObjectText(g, o, components));
                    }
                    else
                        fences.Add(new ElementFence());
                    
                    return fences;
                }

                private List<ElementFence> DrawObjectEvent(Graphics g, LifeTimeElement o, DrawComponent components)
                {
                    List<ElementFence> fences = new List<ElementFence>();

                    int x = GetX(o.Begin);
                    int y = GetY(o.Row) + (Settings.BlockHeight / 2) + o.LineDeviation;

                    if ((components == DrawComponent.Shadow || components == DrawComponent.All) && Style == DrawStyle.WithShadow)
                    {
                        g.FillEllipse(new SolidBrush(Color.DarkGray),
                            x - (o.Size / 2) + 2, y - (o.Size / 2) + 2, o.Size, o.Size);
                        g.FillEllipse(new SolidBrush(Settings.BackColor),
                            x - (o.Size / 2), y - (o.Size / 2), o.Size, o.Size);
                    }

                    if (components == DrawComponent.Object || components == DrawComponent.All)
                    {
                        g.FillEllipse(new SolidBrush(GetColorOfObject(o)),
                            x - (o.Size / 2), y - (o.Size / 2), o.Size, o.Size);
                        g.DrawEllipse(GetFramePen(o),
                            x - (o.Size / 2), y - (o.Size / 2), o.Size, o.Size);
                    }

                    if (components == DrawComponent.Label || components == DrawComponent.All)
                        fences.Add(DrawObjectLabel(g, o));

                    Rectangle r = new Rectangle(x - o.Size, y - o.Size, o.Size * 2, o.Size * 2);
                    fences.Add(new ElementFence(r, o, false));

                    return fences;
                }

                private List<ElementFence> DrawObjectTimeSpan(Graphics g, LifeTimeElement o, DrawComponent components)
                {
                    List<ElementFence> fences = new List<ElementFence>();

                    int x = GetX(o.Begin);
                    int y = GetY(o.Row) + o.LineDeviation;

                    if ((components == DrawComponent.Shadow || components == DrawComponent.All) && Style == DrawStyle.WithShadow)
                    {
                        g.FillRectangle(new SolidBrush(Color.DarkGray),
                            x + 2, y + 2, GetWidth(o.Begin, o.End), o.Size);

                        g.FillRectangle(new SolidBrush(Settings.BackColor),
                            x, y, GetWidth(o.Begin, o.End), o.Size);
                    }

                    if (components == DrawComponent.Object || components == DrawComponent.All)
                    {
                        Color c = Color.FromArgb(Convert.ToInt16(255.0 * o.Opacity),
                            GetColorOfObject(o).R,
                            GetColorOfObject(o).G,
                            GetColorOfObject(o).B);

                        g.FillRectangle(new SolidBrush(c),
                            x, y, GetWidth(o.Begin, o.End), o.Size);

                        g.DrawRectangle(GetFramePen(o),
                            x, y, GetWidth(o.Begin, o.End), o.Size);
                    }

                    if (components == DrawComponent.Label || components == DrawComponent.All)
                        fences.Add(DrawObjectLabel(g, o));

                    Rectangle r = new Rectangle(x, y, GetWidth(o.Begin, o.End), o.Size);

                    fences.Add(new ElementFence(r, o, false));

                    return fences;
                }

                private List<ElementFence> DrawObjectMarker(Graphics g, LifeTimeElement o, DrawComponent components)
                {
                    List<ElementFence> fences = new List<ElementFence>();

                    int x = GetX(o.Begin);
                    if (components != DrawComponent.Shadow)
                    {
                        if (components == DrawComponent.Object || components == DrawComponent.All)
                        {
                            g.DrawLine(new Pen(GetColorOfObject(o), 1f),
                                    x, Settings.Border, x, Height - Settings.Border);
                        }

                        if (components == DrawComponent.Label || components == DrawComponent.All)
                            fences.Add(DrawObjectLabel(g, o));
                    }

                    Rectangle r = new Rectangle(x - 4, 0, 8, Height - Settings.Border);

                    fences.Add(new ElementFence(r, o, false));

                    return fences;
                }

                private List<ElementFence> DrawObjectText(Graphics g, LifeTimeElement o, DrawComponent components)
                {
                    List<ElementFence> fences = new List<ElementFence>();

                    SizeF s = g.MeasureString(o.Text, new Font(o.UseDiagFont? Settings.Font : o.Font, Convert.ToSingle(o.Size), o.FontStyle));

                    int x = Settings.Border + o.TextPosX;
                    int y = Settings.Border + o.TextPosY;

                    if (o.HorizontallyBonding == LifeTimeElement.BondPositionsHorizontally.Left)
                        x = Settings.Border;
                    if (o.HorizontallyBonding == LifeTimeElement.BondPositionsHorizontally.Right)
                        x = Settings.Width - Settings.Border - Convert.ToInt32(s.Width);
                    if (o.HorizontallyBonding == LifeTimeElement.BondPositionsHorizontally.Center)
                        x = (Settings.Width / 2) - Convert.ToInt32(s.Width / 2);

                    if (o.VerticallyBonding == LifeTimeElement.BondPostionsVertically.Top)
                        y = Settings.Border;
                    if (o.VerticallyBonding == LifeTimeElement.BondPostionsVertically.Bottom)
                        y = Settings.Height - Settings.Border - Convert.ToInt32(s.Height);
                    if (o.VerticallyBonding == LifeTimeElement.BondPostionsVertically.Middle)
                        y = (Settings.Height / 2) - Convert.ToInt32(s.Height / 2);

                    if (o.HorizontallyBonding != LifeTimeElement.BondPositionsHorizontally.None)
                        o.TextPosX = x - Settings.Border;
                    if (o.VerticallyBonding != LifeTimeElement.BondPostionsVertically.None)
                        o.TextPosY = y - Settings.Border;

                    int border = o.TextInBox ? 2 : 0;

                    Rectangle r = new Rectangle(x, y, Convert.ToInt32(s.Width) + 2 * border, Convert.ToInt32(s.Height) + 2 * border);

                    fences.Add(new ElementFence(r, o, true));

                    Color c = Color.FromArgb(Convert.ToInt16(255.0 * o.Opacity),
                            GetColorOfObject(o).R,
                            GetColorOfObject(o).G,
                            GetColorOfObject(o).B);

                    if (components == DrawComponent.Text || components == DrawComponent.All)
                    {
                        if (o.TextInBox)
                        {
                            g.FillRectangle(new SolidBrush(c), r);
                            g.DrawRectangle(GetFramePen(o), r);
                        }
                        
                        g.DrawString(o.Text, new Font(o.UseDiagFont? Settings.Font : o.Font, o.Size, o.FontStyle), new SolidBrush(GetPenColor(o)), x + border, y + border);
                    }
                    
                    return fences;
                }

                private ElementFence DrawObjectLabel(Graphics g, LifeTimeElement o)
                {
                    if (o.GetLabel() == "")
                        return new ElementFence();
                    
                    int x = GetX(o.Begin);
                    int y = GetY(o.Row) + o.LineDeviation;
                    
                    DrawLineToLabel(g, o, x, y);

                    return DrawLabelText(g, o, x, y);
                }

                private ElementFence DrawLabelText(Graphics g, LifeTimeElement o, int x, int y)
                {
                    g.DrawString(o.GetLabel(), new Font(o.UseDiagFont? Settings.Font : o.Font, Settings.GlobalFontSize, o.FontStyle), new SolidBrush(GetPenColor(o)), 
                        x + o.TextPosX, y + o.TextPosY);

                    SizeF s = g.MeasureString(o.GetLabel(), new Font(o.UseDiagFont ? Settings.Font : o.Font, Settings.GlobalFontSize, o.FontStyle));

                    return new ElementFence(new Rectangle(x + o.TextPosX, y + o.TextPosY, Convert.ToInt32(s.Width), Convert.ToInt32(s.Height)), o, true);
                }

                private void DrawLineToLabel(Graphics g, LifeTimeElement o, int x, int y)
                {
                    if (o.TextPosY < o.Size && o.TextPosY >= 0)
                        return;

                    int origin = y;
                    if (o.Type == LifeTimeElement.LifeTimeObjectType.Event)
                        origin = (o.TextPosY > 0 ? y + o.Size / 2 : y - o.Size / 2) + Settings.BlockHeight / 2;
                    else
                        origin = o.TextPosY > 0 ? y + o.Size : y;

                    int target = o.TextPosY;
                    

                    g.DrawLine(new Pen(GetPenColor(o), 1.0f), x, origin, x + o.TextPosX, y + target + Settings.GlobalFontSize * 2);
                    g.DrawLine(new Pen(GetPenColor(o), 1.0f), x + o.TextPosX, y + target + Settings.GlobalFontSize * 2, x + o.TextPosX + 20, y + target + Settings.GlobalFontSize * 2);
                }

                private Pen GetFramePen(LifeTimeElement o)
                {
                    return o.Highlight ? new Pen(Color.Red, 2.0f) : new Pen(Color.Black, 1.0f);
                }

                private Color GetPenColor(LifeTimeElement o)
                {
                    return o.Highlight ? Color.Red : o.UseGlobalTextColor? Settings.LabelColor : o.TextColor;
                }
                #endregion

                #region Private Methods
                private Color GetColorOfObject(LifeTimeElement o)
                {
                    if (o.GetRandomColor) return o.Color;
                    else return o.FixedColor;
                }
                #endregion

                #region Math
                private int GetX(DateTime d)
                {
                    return Convert.ToInt32(Settings.Border + (d - Settings.Begin).Days * PixelPerDay);
                }

                private int GetWidth(DateTime begin, DateTime end)
                {
                    return Convert.ToInt32((end - begin).Days * PixelPerDay);
                }

                private int GetY(int row)
                {
                    return Settings.Border + Convert.ToInt32(row / 2.0 * Settings.GroupHeight);
                }

                #endregion

                #region statics
                #endregion

                
            }
            #endregion

            #region RandomColor Class
            private class CreateRandomColor
            {
                private Random r = new Random();

                /// <summary>
                /// Returns a new random, based on the given BasicColor, Color.
                /// </summary>
                /// <param name="BaseColor"></param>
                /// <returns></returns>
                public Color CreateNewRandomColor(Color BaseColor)
                {
                    return Color.FromArgb((BaseColor.R + r.Next(255)) / 2, (BaseColor.G + r.Next(255)) / 2, (BaseColor.B + r.Next(255)) / 2);
                }

                /// <summary>
                /// Returns a new random Color.
                /// </summary>
                public Color CreateNewRandomColor()
                {
                    return Color.FromArgb((r.Next(255)), (r.Next(255)), (r.Next(255)));
                }

                /// <summary>
                /// Returns a random Base Color.
                /// </summary>
                /// <returns></returns>
                public Color GetRandomBaseColor()
                {
                    Color[] BaseColors = new Color[] { Color.Red, Color.Green, Color.Blue, Color.Cyan, Color.Magenta, Color.Yellow };
                    return BaseColors[r.Next(6)];
                }
            }
            #endregion

            #region ElementFence Struct
            public class ElementFence
            {
                #region publics
                public Rectangle FenceRectangle;
                public bool Movable;
                public ILifeTimeObject LifeTimeObject;
                #endregion

                public ElementFence(Rectangle fence, ILifeTimeObject element, bool movable)
                {
                    FenceRectangle = fence;
                    LifeTimeObject = element;
                    Movable = movable;
                }

                public ElementFence()
                {
                    FenceRectangle = new Rectangle();
                    LifeTimeObject = null;                    
                }
            }
            #endregion
        }
        #endregion
    }
}
