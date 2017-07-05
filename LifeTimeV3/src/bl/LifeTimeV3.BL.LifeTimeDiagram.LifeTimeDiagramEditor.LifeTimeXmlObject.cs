using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Xml;

namespace LifeTimeV3.BL.LifeTimeDiagram
{
    partial class LifeTimeDiagramEditor
    {
        #region Xml Object Import Export Class
        public class LifeTimeXmlObject
        {
            #region Properties
            public XmlDocument Xml { get; set; }
            #endregion

            #region fields
            LifeTimeDiagramSettings _settings;
            #endregion

            #region constructor
            public LifeTimeXmlObject(XmlDocument xml, LifeTimeDiagramSettings settings) : this(xml)
            {
                _settings = settings;               
            }
            public LifeTimeXmlObject(XmlDocument xml)
            {                
                Xml = xml;
            }
            #endregion

            #region GetXmlFromObject
            /// <summary>
            /// Returns an XMLElement from an ILifeTimeObject
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="o"></param>
            /// <returns></returns>
            public XmlElement GetXmlFromObject(ILifeTimeObject o)
            {
                XmlElement e = Xml.CreateElement(o.XmlNodeName);
                
                foreach (string property in (o.Properties(true)))
                {
                    if (o.GetType().GetProperty(property).GetValue(o) is Color)
                        e.SetAttribute(property, ConvertColorToString((Color)(o.GetType().GetProperty(property).GetValue(o))));
                    else if (o.GetType().GetProperty(property).GetValue(o) is FontFamily)
                        e.SetAttribute(property, (o.GetType().GetProperty(property).GetValue(o) as FontFamily).Name);
                    else if (o.GetType().GetProperty(property).PropertyType == typeof(Color[]))
                    {
                        string colors = "";                        
                        foreach (Color c in o.GetType().GetProperty(property).GetValue(o) as Color[])
                        {
                            string color = c.R + ";" + c.G + ";" + c.B;
                            colors += string.IsNullOrEmpty(colors) ? color : ";" + color;
                        }

                        e.SetAttribute(property, colors);
                    }
                    else
                        e.SetAttribute(property, o.GetType().GetProperty(property).GetValue(o).ToString());
                }

                return e;
            }
            #endregion

            #region GetObjectFromXml
            /// <summary>
            /// Returns an ILifeTimeObject from an XMLNode
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="node"></param>
            /// <returns></returns>
            public ILifeTimeObject GetObjectFromXml(XmlNode node)
            {
                ILifeTimeObject o;

                

                switch (node.Name)
                {
                    case LifeTimeElement.XmlNodeNameDefinition:
                        LifeTimeElement.LifeTimeObjectType type = LifeTimeElement.LifeTimeObjectType.TimeSpan;
                        if (node.Attributes["Type"].Value == "Marker") type = LifeTimeElement.LifeTimeObjectType.Marker;
                        if (node.Attributes["Type"].Value == "Event") type = LifeTimeElement.LifeTimeObjectType.Event;
                        if (node.Attributes["Type"].Value == "TimeSpan") type = LifeTimeElement.LifeTimeObjectType.TimeSpan;
                        if (node.Attributes["Type"].Value == "Text") type = LifeTimeElement.LifeTimeObjectType.Text;
                        
                        o = new LifeTimeElement(_settings, node.Attributes["Name"].Value, type);
                        break;

                    case LifeTimeGroup.XmlNodeNameDefinition:
                        o = new LifeTimeGroup(node.Attributes["Name"].Value, GetColorFromArgbString(node.Attributes["GroupColor"].Value));                        
                        break;

                    case LifeTimeDiagramSettings.XmlNodeNameDefinition:
                        o = new LifeTimeDiagramSettings();                        
                        break;

                    case LifeTimeExportSettings.XmlNodeNameDefinition:
                        o = new LifeTimeExportSettings();                        
                        break;

                    default:
                        return null;
                }

                Type t = o.GetType();

                foreach (string property in o.Properties(true))
                {
                    if (node.Attributes[property] == null) continue;

                    if (t.GetProperty(property).PropertyType == typeof(bool))
                        t.GetProperty(property).SetValue(o, node.Attributes[property].Value.ToString().ToLower() == "true" ? true : false);
                    else if (t.GetProperty(property).PropertyType == typeof(string))
                        t.GetProperty(property).SetValue(o, node.Attributes[property].Value);
                    else if (t.GetProperty(property).PropertyType == typeof(Color[]))
                    {
                        string[] values = node.Attributes[property].Value.Split(new char[] { ';' });

                        List<Color> colors = new List<Color>();

                        for (int i = 0; i < values.Length; i+=3)
                        {
                            colors.Add(Color.FromArgb(0, Convert.ToInt16(values[i]), Convert.ToInt16(values[i + 1]), Convert.ToInt16(values[i + 2])));
                        }

                        t.GetProperty(property).SetValue(o, colors.ToArray());
                    }
                    else if (t.GetProperty(property).PropertyType == typeof(int))
                        t.GetProperty(property).SetValue(o, Convert.ToInt16(node.Attributes[property].Value));
                    else if (t.GetProperty(property).PropertyType == typeof(float))
                        t.GetProperty(property).SetValue(o, Convert.ToSingle(node.Attributes[property].Value));
                    else if (t.GetProperty(property).PropertyType == typeof(double))
                        t.GetProperty(property).SetValue(o, Convert.ToDouble(node.Attributes[property].Value));
                    else if (t.GetProperty(property).PropertyType == typeof(Color))
                        t.GetProperty(property).SetValue(o, GetColorFromArgbString(node.Attributes[property].Value));
                    else if (t.GetProperty(property).PropertyType == typeof(DateTime))
                        t.GetProperty(property).SetValue(o, Convert.ToDateTime(node.Attributes[property].Value));
                    else if (t.GetProperty(property).PropertyType == typeof(FontFamily))
                    {
                        try
                        {
                            t.GetProperty(property).SetValue(o, new FontFamily(node.Attributes[property].Value));
                        }
                        catch
                        {
                            t.GetProperty(property).SetValue(o, SystemFonts.DefaultFont.FontFamily);
                        }                        
                    }
                    else if (t.GetProperty(property).PropertyType == typeof(LifeTimeElement.BondPositionsHorizontally))
                    {
                        foreach (LifeTimeElement.BondPositionsHorizontally e in Enum.GetValues(typeof(LifeTimeElement.BondPositionsHorizontally)))
                        {
                            if (node.Attributes[property].Value == Enum.GetName(typeof(LifeTimeElement.BondPositionsHorizontally), e))
                            {
                                t.GetProperty(property).SetValue(o, e);
                                break;
                            }
                        }                        
                    }
                    else if (t.GetProperty(property).PropertyType == typeof(LifeTimeElement.BondPostionsVertically))
                    {
                        foreach (LifeTimeElement.BondPostionsVertically e in Enum.GetValues(typeof(LifeTimeElement.BondPostionsVertically)))
                        {
                            if (node.Attributes[property].Value == Enum.GetName(typeof(LifeTimeElement.BondPostionsVertically), e))
                            {
                                t.GetProperty(property).SetValue(o, e);
                                break;
                            }
                        }
                    }
                    else if (t.GetProperty(property).PropertyType == typeof(FontStyle))
                    {
                        t.GetProperty(property).SetValue(o, FontStyle.Regular);

                        foreach (FontStyle st in Enum.GetValues(typeof(FontStyle)))
                        {
                            string b = Enum.GetName(typeof(FontStyle), st);
                            if (!string.IsNullOrEmpty(b) && node.Attributes[property].Value.Contains(b))
                            {
                                t.GetProperty(property).SetValue(o, (int)(t.GetProperty(property).GetValue(o)) + (int)st);                                
                            }
                        }
                    }
                }

                return o;
            }
            #endregion

            #region ColorStringConverter
            public static Color GetColorFromArgbString(String s)
            {
                string[] argb = s.Split(new char[] { ',' });
                return Color.FromArgb(Convert.ToInt16(argb[0]),
                    Convert.ToInt16(argb[1]),
                    Convert.ToInt16(argb[2]),
                    Convert.ToInt16(argb[3]));
            }

            public static string ConvertColorToString(Color c)
            {
                return string.Format("{0},{1},{2},{3}", c.A, c.R, c.G, c.B);
            }
            #endregion
        }
        #endregion
    }
}
