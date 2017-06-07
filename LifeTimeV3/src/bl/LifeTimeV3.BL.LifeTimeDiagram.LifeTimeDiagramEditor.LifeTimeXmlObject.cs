using System;
using System.Collections.Generic;
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

            #region constructor
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
                        
                        o = new LifeTimeElement(node.Attributes["Name"].Value, type);
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
                        t.GetProperty(property).SetValue(o, node.Attributes[property].Value.ToString().ToLower() == "true" ? true: false);
                    else if (t.GetProperty(property).PropertyType == typeof(string))
                        t.GetProperty(property).SetValue(o, node.Attributes[property].Value);
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
                    else if (t.GetProperty(property).PropertyType == typeof(LifeTimeElement.BondPositionsHorizontally))
                    {   
                        if (node.Attributes[property].Value == Enum.GetName(typeof(LifeTimeElement.BondPositionsHorizontally), 0))
                            t.GetProperty(property).SetValue(o, 0);
                        if (node.Attributes[property].Value == Enum.GetName(typeof(LifeTimeElement.BondPositionsHorizontally), 1))
                            t.GetProperty(property).SetValue(o, 1);
                        if (node.Attributes[property].Value == Enum.GetName(typeof(LifeTimeElement.BondPositionsHorizontally), 2))
                            t.GetProperty(property).SetValue(o, 2);
                        if (node.Attributes[property].Value == Enum.GetName(typeof(LifeTimeElement.BondPositionsHorizontally), 3))
                            t.GetProperty(property).SetValue(o, 3);
                    }
                    else if (t.GetProperty(property).PropertyType == typeof(LifeTimeElement.BondPostionsVertically))
                    {
                        if (node.Attributes[property].Value == Enum.GetName(typeof(LifeTimeElement.BondPostionsVertically), 0))
                            t.GetProperty(property).SetValue(o, 0);
                        if (node.Attributes[property].Value == Enum.GetName(typeof(LifeTimeElement.BondPostionsVertically), 1))
                            t.GetProperty(property).SetValue(o, 1);
                        if (node.Attributes[property].Value == Enum.GetName(typeof(LifeTimeElement.BondPostionsVertically), 2))
                            t.GetProperty(property).SetValue(o, 2);
                        if (node.Attributes[property].Value == Enum.GetName(typeof(LifeTimeElement.BondPostionsVertically), 3))
                            t.GetProperty(property).SetValue(o, 3);
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
