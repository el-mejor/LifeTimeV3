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
                if (Path.GetExtension(FileName).ToLower() == ".xml") return OpenFileFromXml();
                throw new Exception("Unknown file type: " + FileName);
            }
            #endregion

            #region Private Methods
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
