using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
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

            #region Properties
            //Basics
            public string Name { get; set; }
            public string Description { get; set; }
            public string FileName { get; set; }
            public bool Changed { get; set; }
            public Dictionary<Rectangle, LifeTimeElement> ObjectFences = new Dictionary<Rectangle, LifeTimeElement>();

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
                ObjectFences.Clear();

                DiagramDrawer draw = new DiagramDrawer(width, height, Settings, style);
                List<LifeTimeElement> o = GetAllObjects(true);

                //Draw Diagram Area
                g.FillRectangle(new SolidBrush(Settings.BackColor), 0, 0, width, height);

                //Draw Diagram components
                if (components == DrawComponent.All)
                {
                    if (Settings.DrawShadows) DrawDiagramComponent(draw, o, g, rndColor, DrawComponent.Shadow);
                    DrawDiagramComponent(draw, o, g, rndColor, DrawComponent.Object);
                    DrawDiagramComponent(draw, o, g, rndColor, DrawComponent.Label);
                    DrawDiagramComponent(draw, o, g, rndColor, DrawComponent.Text);
                }
                else
                    DrawDiagramComponent(draw, o, g, rndColor, components);
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
            private void DrawDiagramComponent(DiagramDrawer draw, List<LifeTimeElement> o, Graphics g, DrawNewRandomColor rndColor, DrawComponent components)
            {
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
                foreach (LifeTimeElement.LifeTimeObjectType type in types) DrawObjectsOfType(type, draw, o, g, components);
            }

            private void DrawObjectsOfType(LifeTimeElement.LifeTimeObjectType type, DiagramDrawer draw, List<LifeTimeElement> o, Graphics g, DrawComponent components)
            {
                Rectangle fence = new Rectangle();

                foreach (LifeTimeElement _o in o)
                {
                    if (!_o.Deleted && _o.Type == type)
                    {
                        fence = draw.DrawObject(g, _o, components);
                        if (components == DrawComponent.Object || components == DrawComponent.Text)
                            AddObjectFenceToDictionary(fence, _o);
                    }
                }
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

            private void AddObjectFenceToDictionary(Rectangle fence, LifeTimeElement _o)
            {
                if (!ObjectFences.Keys.Any(n => n == fence)) ObjectFences.Add(fence, _o);
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
                public Rectangle DrawObject(Graphics g, LifeTimeElement o, DrawComponent components)
                {
                    if (o.Type == LifeTimeElement.LifeTimeObjectType.Event)
                    {
                        return DrawObjectEvent(g, o, components);
                    }
                    if (o.Type == LifeTimeElement.LifeTimeObjectType.TimeSpan)
                    {
                        return DrawObjectTimeSpan(g, o, components);
                    }
                    if (o.Type == LifeTimeElement.LifeTimeObjectType.Marker)
                    {
                        return DrawObjectMarker(g, o, components);
                    }
                    if (o.Type == LifeTimeElement.LifeTimeObjectType.Text)
                    {
                        return DrawObjectText(g, o, components);
                    }
                    return new Rectangle();
                }

                private Rectangle DrawObjectEvent(Graphics g, LifeTimeElement o, DrawComponent components)
                {
                    Int32 x = GetX(o.Begin);
                    Int32 y = GetY(o.Row) + (Settings.BlockHeight / 2) + o.LineDeviation;

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
                        DrawObjectLabel(g, o);

                    return new Rectangle(x - o.Size, y - o.Size, o.Size * 2, o.Size * 2);
                }

                private Rectangle DrawObjectTimeSpan(Graphics g, LifeTimeElement o, DrawComponent components)
                {
                    Int32 x = GetX(o.Begin);
                    Int32 y = GetY(o.Row) + o.LineDeviation;

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
                        DrawObjectLabel(g, o);

                    return new Rectangle(x, y, GetWidth(o.Begin, o.End), o.Size);
                }

                private Rectangle DrawObjectMarker(Graphics g, LifeTimeElement o, DrawComponent components)
                {
                    Int32 x = GetX(o.Begin);
                    if (components != DrawComponent.Shadow)
                    {
                        if (components == DrawComponent.Object || components == DrawComponent.All)
                        {
                            g.DrawLine(new Pen(GetColorOfObject(o), 1f),
                                    x, Settings.Border, x, Height - Settings.Border);
                        }

                        if (components == DrawComponent.Label || components == DrawComponent.All)
                            DrawObjectLabel(g, o);
                    }

                    return new Rectangle(x - 4, 0, 8, Height - Settings.Border);
                }

                private Rectangle DrawObjectText(Graphics g, LifeTimeElement o, DrawComponent components)
                {
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

                    Color c = Color.FromArgb(Convert.ToInt16(255.0 * o.Opacity),
                            GetColorOfObject(o).R,
                            GetColorOfObject(o).G,
                            GetColorOfObject(o).B);

                    if (components == DrawComponent.Text || components == DrawComponent.All)
                    {
                        if (o.TextInBox)
                        {
                            g.FillRectangle(new SolidBrush(c), r);
                            g.DrawRectangle(new Pen(Settings.LabelColor, 1.0f), r);
                        }
                        
                        g.DrawString(o.Text, new Font(o.UseDiagFont? Settings.Font : o.Font, o.Size, o.FontStyle), new SolidBrush(GetPenColor(o)), x + border, y + border);
                    }
                    
                    return r;
                }

                private void DrawObjectLabel(Graphics g, LifeTimeElement o)
                {
                    if (o.GetLabel() == "") return;

                    int x = GetX(o.Begin);
                    int y = GetY(o.Row) + o.LineDeviation;
                    DrawLineToLabel(g, o, x, y);
                    DrawLabelText(g, o, x, y);
                }

                private void DrawLabelText(Graphics g, LifeTimeElement o, int x, int y)
                {
                    g.DrawString(o.GetLabel(), new Font(o.UseDiagFont? Settings.Font : o.Font, Settings.GlobalFontSize, o.FontStyle), new SolidBrush(GetPenColor(o)), x + o.TextPosX, y + o.TextPosY);
                }

                private void DrawLineToLabel(Graphics g, LifeTimeElement o, int x, int y)
                {
                    g.DrawLine(new Pen(Settings.LabelColor, 1.0f), x, y, x + o.TextPosX, y + o.TextPosY + 14);
                    g.DrawLine(new Pen(Settings.LabelColor, 1.0f), x + o.TextPosX, y + o.TextPosY + 14, x + o.TextPosX + 20, y + o.TextPosY + 14);
                }

                private Pen GetFramePen(LifeTimeElement o)
                {
                    return o.Highlight ? new Pen(Color.Red, 2.0f) : new Pen(Color.Black, 1.0f);
                }

                private Color GetPenColor(LifeTimeElement o)
                {
                    return o.Highlight ? Color.Red : Settings.LabelColor;
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
        }
        #endregion
    }
}
