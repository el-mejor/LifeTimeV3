using System;
using System.IO;
using System.Drawing;
using System.Xml;

namespace LifeTimeV3.BL.LifeTimeDiagram
{
    partial class LifeTimeDiagramEditor
    {   
        #region LifeTimeDiagram FileHandler Class
        /// <summary>
        /// File handling class to open, save and export the diagram
        /// </summary>
        private class LifeTimeDiagramFileHandler
        {
            #region Properties
            public String FileName;
            #endregion

            #region Fields
            private XmlDocument xml;
            private LifeTimeDiagram d;
            #endregion

            #region constructor
            public LifeTimeDiagramFileHandler(String filename)
            {
                FileName = filename;
            }
            #endregion

            #region Save File
            #region Public Methods
            /// <summary>
            /// Save Diagram to Xml File.
            /// </summary>
            public void SaveFile(LifeTimeDiagram diagram)
            {
                xml = new XmlDocument();

                d = diagram;
                CheckIfFileExistsAndMakeBackup();
                AddRootNode();
                AddSettings();
                AddObjects();
                xml.Save(FileName);

                diagram.Changed = false;
            }
            #endregion

            #region Private Methods
            private void CheckIfFileExistsAndMakeBackup()
            {
                if (File.Exists(FileName))
                {
                    if (File.Exists(FileName + "_old")) File.Delete(FileName + "_old");
                    File.Copy(FileName, FileName + "_old");
                    File.Delete(FileName);
                }
            }

            private void AddRootNode()
            {
                XmlElement e = xml.CreateElement("LifeTimeDiagram");
                xml.AppendChild(e);
            }

            private void AddSettings()
            {
                LifeTimeXmlObject get = new LifeTimeXmlObject(xml);

                XmlElement s = get.GetXmlFromObject(d.Settings);

                xml.SelectSingleNode("LifeTimeDiagram").AppendChild(s);

                XmlElement e = get.GetXmlFromObject(d.ExportSettings);

                xml.SelectSingleNode("LifeTimeDiagram").AppendChild(e);
            }

            private void AddObjects()
            {
                foreach (LifeTimeGroup g in d.Groups.Groups)
                {
                    AddGroupToRoot(g);
                }
            }

            private void AddGroupToRoot(LifeTimeGroup g)
            {
                if (!g.Deleted) xml.SelectSingleNode("LifeTimeDiagram").AppendChild(AddGroupDeep(g));
            }

            private XmlElement AddGroupDeep(LifeTimeGroup g)
            {
                if (!g.Deleted)
                {
                    LifeTimeXmlObject get = new LifeTimeXmlObject(xml);

                    XmlElement e = get.GetXmlFromObject(g);

                    foreach (LifeTimeElement deepObject in g.Objects)
                    {
                        if (!deepObject.Deleted) e.AppendChild(get.GetXmlFromObject(deepObject));
                    }


                    foreach (LifeTimeGroup deepGroup in g.Groups)
                    {
                        XmlNode n = AddGroupDeep(deepGroup);

                        if (n != null) e.AppendChild(n);
                    }

                    return e;
                }

                return null;
            }
            #endregion
            #endregion

            #region Open File
            #region Public Methods
            /// <summary>
            /// Open and returns a LifeTimeDiagram
            /// </summary>
            /// <returns></returns>
            public LifeTimeDiagram OpenFile()
            {
                if (Path.GetExtension(FileName).ToLower() == ".csv") return OpenFileFromCSV();
                if (Path.GetExtension(FileName).ToLower() == ".xml") return OpenFileFromXml();
                throw new Exception("Unknown file type: " + FileName);
            }
            #endregion

            #region Private Methods
            private LifeTimeDiagram OpenFileFromCSV()
            {
                LifeTimeDiagram d = new LifeTimeDiagram();
                d.FileName = FileName;
                //this is a poor copy of the old version, a bit modified to match the new modell

                //load a lifetimediagramfile
                string newline;
                try
                {
                    using (StreamReader rdline = new StreamReader(FileName))
                    {
                        //read first line and check if this is a lifetimediagram
                        newline = rdline.ReadLine();
                        if (newline != "LifeTime Diagram File")
                        {
                            throw new Exception("Content seems not to be a valid LifeTime CSV");
                        }
                        //read 2nd line and get the Diagram Name
                        newline = rdline.ReadLine();

                        d.Name = newline;

                        newline = rdline.ReadLine();
                        //skip until "Settings:" appears
                        while (newline != "Settings:")
                        {
                            newline = rdline.ReadLine();
                        }
                        newline = rdline.ReadLine(); //skip table headers
                        newline = rdline.ReadLine(); //here are the settings. now call Element[0].csv_to_settings

                        string[] settings = newline.Split(new char[] { ';' }, StringSplitOptions.None);
                        d.Settings.Begin = Convert.ToDateTime(settings[0]);
                        d.Settings.End = Convert.ToDateTime(settings[1]);

                        newline = rdline.ReadLine();
                        //skip until "Groups:" appears
                        while (newline != "Groups:")
                        {
                            newline = rdline.ReadLine();
                        }

                        d.Groups.Add(new LifeTimeGroup("Unassigned", Color.White));
                        for (int group_index = 1; group_index <= 24; group_index++)
                        {
                            newline = rdline.ReadLine();
                            String[] groupinfo = newline.Split(new char[] { ';' });

                            d.Groups.Add(new LifeTimeGroup(groupinfo[1], Color.White));
                        }

                        newline = rdline.ReadLine();
                        //skip until "Elements:" appears
                        while (newline != "Elements:")
                        {
                            newline = rdline.ReadLine();
                        }

                        newline = rdline.ReadLine(); //skip table headers
                        newline = rdline.ReadLine(); //this is the first element
                        while (newline != null)
                        {
                            string[] element = newline.Split(new char[] { ';' });

                            LifeTimeElement.LifeTimeObjectType type = LifeTimeElement.LifeTimeObjectType.Marker;
                            if (element[1] == "marker") type = LifeTimeElement.LifeTimeObjectType.Marker;
                            if (element[1] == "span") type = LifeTimeElement.LifeTimeObjectType.TimeSpan;
                            if (element[1] == "event") type = LifeTimeElement.LifeTimeObjectType.Event;

                            int group;

                            LifeTimeElement o = new LifeTimeElement(element[0], type);

                            o.Begin = Convert.ToDateTime(element[2]);
                            o.End = Convert.ToDateTime(element[3]);
                            group = Convert.ToInt16(element[4]);
                            o.LineDeviation = Convert.ToInt16(Convert.ToDouble(element[5].Replace(",", ".")) * 10);
                            if (element[6] == "False") o.GetRandomColor = false;
                            else o.GetRandomColor = true;
                            o.Color = Color.FromArgb(Convert.ToInt16(element[8]), Convert.ToInt16(element[9]), Convert.ToInt16(element[10]));
                            o.FixedColor = o.Color;
                            o.Opacity = Convert.ToDouble(element[11]) / 100.0;
                            if (element[12] == "False") o.BeginsToday = false;
                            else o.BeginsToday = true;
                            if (element[13] == "False") o.EndsToday = false;
                            else o.EndsToday = true;

                            d.Groups.Groups[group].Add(o);

                            newline = rdline.ReadLine(); //a new element string
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                return d;
            }

            private LifeTimeDiagram OpenFileFromXml()
            {
                xml = new XmlDocument();
                xml.Load(FileName);

                LifeTimeDiagram d = new LifeTimeDiagram();
                d.FileName = FileName;

                d.Settings = GetSettings(settingsType.Diagram) as LifeTimeDiagramSettings;
                d.ExportSettings = GetSettings(settingsType.Export) as LifeTimeExportSettings;
                GetObjects(d);

                return d;
            }

            private void GetObjects(LifeTimeDiagram d)
            {
                foreach (XmlNode group in xml.SelectNodes("/LifeTimeDiagram/Group"))
                    GetGroupDeep(group, d.Groups);
            }

            private void GetGroupDeep(XmlNode group, LifeTimeGroup d)
            {
                LifeTimeXmlObject get = new LifeTimeXmlObject(xml);
                LifeTimeGroup g = get.GetObjectFromXml(group) as LifeTimeGroup;

                foreach (XmlNode child in group.ChildNodes)
                {
                    if (child.Name.ToLower() == "object")
                        g.Add(get.GetObjectFromXml(child) as LifeTimeElement);

                    if (child.Name.ToLower() == "group")
                        GetGroupDeep(child, g);
                }

                d.Add(g);
            }

            private enum settingsType
            {
                Diagram, Export
            }
            private ILifeTimeObject GetSettings(settingsType settingsType)
            {
                LifeTimeXmlObject get = new LifeTimeXmlObject(xml);

                string XmlNodeName = "";

                if (settingsType == settingsType.Diagram) XmlNodeName = LifeTimeDiagramSettings.XmlNodeNameDefinition;
                if (settingsType == settingsType.Export) XmlNodeName = LifeTimeExportSettings.XmlNodeNameDefinition;

                return get.GetObjectFromXml(xml.SelectSingleNode("LifeTimeDiagram/" + XmlNodeName));
            }
            #endregion
            #endregion

            #region ExportPNG
            #region Public Methods
            public void ExportPNG(LifeTimeDiagram diagram, int width, int height, DrawNewRandomColor NewRandomColor, DrawComponent DrawComponents, DrawStyle Style)
            {
                d = diagram;

                Bitmap b = new Bitmap(width, height);

                d.DrawDiagram(Graphics.FromImage(b), width, height,
                    LifeTimeDiagramEditor.DrawNewRandomColor.Yes, LifeTimeDiagramEditor.DrawComponent.All, LifeTimeDiagramEditor.DrawStyle.WithShadow);

                b.Save(FileName, System.Drawing.Imaging.ImageFormat.Png);
            }
            #endregion
            #endregion
        }
        #endregion
    }
}
