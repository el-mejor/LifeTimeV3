using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace LifeTimeV3.BL.LifeTimeDiagram
{
    partial class LifeTimeDiagramEditor
    {
        #region LifeTimeDiagram Class
        /// <summary>
        /// The class containing and modelling the diagram itself
        /// </summary>
        public class LifeTimeDiagram
        {
            #region Enums
            #endregion

            #region Properties
            //Basics
            public String Name { get; set; }
            public String Description { get; set; }
            public String FileName { get; set; }
            public Boolean Changed { get; set; }
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
                }
                else
                    DrawDiagramComponent(draw, o, g, rndColor, components);
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
                                                                LifeTimeElement.LifeTimeObjectType.Event 
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
                        if (components == DrawComponent.Object)
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

                private void DrawObjectLabel(Graphics g, LifeTimeElement o)
                {
                    if (o.GetLabel() == "") return;

                    Int32 x = GetX(o.Begin);
                    Int32 y = GetY(o.Row) + o.LineDeviation;

                    g.DrawLine(new Pen(Settings.LabelColor, 1.0f), x, y, x + o.TextPosX, y + o.TextPosY + 14);
                    g.DrawLine(new Pen(Settings.LabelColor, 1.0f), x + o.TextPosX, y + o.TextPosY + 14, x + o.TextPosX + 20, y + o.TextPosY + 14);
                    g.DrawString(o.GetLabel(), new Font("Arial Narrow", 8.0f), new SolidBrush(GetPenColor(o)), x + o.TextPosX, y + o.TextPosY);
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
                private Int32 GetX(DateTime d)
                {
                    return Convert.ToInt32(Settings.Border + (d - Settings.Begin).Days * PixelPerDay);
                }

                private Int32 GetWidth(DateTime begin, DateTime end)
                {
                    return Convert.ToInt32((end - begin).Days * PixelPerDay);
                }

                private Int32 GetY(int row)
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
