using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml;

namespace LifeTimeV3.BL.LifeTimeDiagram
{
    partial class LifeTimeDiagramEditor
    {
        #region LifeTimeObjects
        /// <summary>
        /// The interface of LifeTimeObjects
        /// </summary>
        public interface ILifeTimeObject: ICloneable
        {
            bool Deleted
            {
                get;
                set;
            }
            bool Enabled
            {
                get;
                set;
            }
            string Name
            {
                get;
                set;
            }            
            string XmlNodeName
            {
                get;
            }

            List<LifeTimeElement> Objects
            {
                get;
                set;
            }
            List<LifeTimeGroup> Groups
            {
                get;
                set;
            }

            List<string> Properties(bool All);
        }

        /// <summary>
        /// The abstraction of LifeTimeObjects
        /// </summary>
        public abstract class LifeTimeObjectBase: ILifeTimeObject
        {
            public bool Deleted
            {
                get;
                set;
            }
            public bool Enabled
            {
                get;
                set;
            }
            public string Name
            {
                get;
                set;
            }
            public abstract string XmlNodeName
            {
                get;
            }

            public List<LifeTimeElement> Objects
            {
                get;
                set;
            }
            public List<LifeTimeGroup> Groups
            {
                get;
                set;
            }

            public abstract List<string> Properties(bool All);

            public abstract object Clone();            
        }

        /// <summary>
        /// LifeTimeElement - The displayed objects in the diagram
        /// </summary>
        public class LifeTimeElement : LifeTimeObjectBase
        {
            #region Enumerators
            public enum LifeTimeObjectType { TimeSpan, Event, Marker, Text };
            public enum TimeSpanBase { Minutes, Days };
            public enum BondPositionsHorizontally { None = 0, Left = 1, Center = 2, Right = 3 }
            public enum BondPostionsVertically { None = 0, Top = 1, Middle = 2, Bottom = 3}
            #endregion

            #region properties
            /// <summary>
            /// Returns a list of available properties
            /// </summary>
            /// <returns></returns>
            public override List<String> Properties(bool All)
            {
                List<String> properties = new List<String>();

                properties.Add("Name");
                if (All) properties.Add("Enabled");
                properties.Add("Type");
                if(All || Type != LifeTimeObjectType.Text) properties.Add("BeginsToday");
                if (All || (!BeginsToday && Type != LifeTimeObjectType.Text)) properties.Add("Begin");
                if (Type == LifeTimeObjectType.TimeSpan || All) properties.Add("EndsToday");
                if ((Type == LifeTimeObjectType.TimeSpan && !EndsToday) || All) properties.Add("End");
                properties.Add("GetRandomColor");
                if (All || !GetRandomColor) properties.Add("FixedColor");
                properties.Add("Opacity");
                properties.Add("UseDiagFont");
                if (All || !UseDiagFont) properties.Add("Font");
                properties.Add("FontStyle");
                properties.Add("UseGlobalTextColor");
                if (All || !UseGlobalTextColor) properties.Add("TextColor");
                if (All || Type != LifeTimeObjectType.Marker) properties.Add("Size");
                if (All || Type != LifeTimeObjectType.Text) properties.Add("LineDeviation");
                if (Type == LifeTimeObjectType.Text || All) properties.Add("HorizontallyBonding");
                if (Type == LifeTimeObjectType.Text || All) properties.Add("VerticallyBonding");
                properties.Add("TextPosX");
                properties.Add("TextPosY");
                if (All) properties.Add("Color");
                if (All) properties.Add("BaseColor");                
                if (Type == LifeTimeObjectType.Text || All) properties.Add("TextInBox");
                properties.Add("Locked");
                if (Type == LifeTimeObjectType.Text || All) properties.Add("Text");

                return properties;
            }
            public const String XmlNodeNameDefinition = "Object";
            public override string XmlNodeName
            {
                get
                {
                    return XmlNodeNameDefinition;
                }
            }

            //Data        
            public LifeTimeObjectType Type
            {
                get
                {
                    return _type;
                }
                set
                {
                    _type = value;
                }
            }
            public bool Highlight { get; set; }
            public DateTime Begin
            {
                get
                {
                    return BeginsToday ? DateTime.Now : _begin;
                }
                set { _begin = value; }
            }
            public DateTime End
            {
                get
                {
                    return EndsToday ? DateTime.Now : _end;
                }
                set { _end = value; }
            }
            public bool BeginsToday { get; set; }
            public bool EndsToday { get; set; }
            public Color BaseColor
            {
                get
                {
                    return _baseColor;
                }
                set
                {
                    _baseColor = value;
                    CheckColorBehavior();
                }
            }
            public Color Color
            {
                get { return _color; }
                set
                {
                    _color = value;
                    CheckColorBehavior();
                }
            }
            public Color FixedColor
            {
                get { return _fixedColor; }
                set
                {
                    _fixedColor = value;
                    CheckColorBehavior();
                }
            }
            public Color TextColor { get; set; }
            public bool UseGlobalTextColor { get; set; }
            public bool GetRandomColor { get; set; }
            public double Opacity
            {
                get { return _opacity; }
                set
                {
                    if (value >= 0 && value <= 1) _opacity = value;
                    else _opacity = 1;
                }
            }
            public int Size { get; set; }
            public bool UseDiagFont { get; set; }
            public FontFamily Font { get; set; }
            public FontStyle FontStyle { get; set; }
            public int LineDeviation { get; set; }
            public int Row { get; set; }
            public int TextPosX { get; set; }
            public int TextPosY { get; set; }
            public bool TextInBox { get; set; }
            public string Text { get; set; }
            public bool Locked { get; set; }
            public BondPositionsHorizontally HorizontallyBonding { get; set; }
            public BondPostionsVertically VerticallyBonding { get; set; }
            #endregion

            #region Fields
            private DateTime _begin;
            private DateTime _end;
            private LifeTimeObjectType _type;

            private Color _baseColor;
            private Color _color;
            private Color _fixedColor;
            private Double _opacity;
            #endregion

            #region Constructor
            public LifeTimeElement(LifeTimeDiagramSettings settings, string name, LifeTimeObjectType type)
            {                
                Name = name;
                Enabled = true;
                _type = type;
                Highlight = false;
                _begin = DateTime.Now;
                _end = Begin.AddSeconds(1);
                _baseColor = Color.White;
                _color = Color.White;
                _fixedColor = Color.White;
                GetRandomColor = false;
                Color = Color.Red;
                UseGlobalTextColor = true;
                TextColor = settings != null? settings.LabelColor : Color.Black;
                Opacity = 1.0;
                Size = settings != null ? settings.BlockHeight : 10;
                Font = settings != null ? settings.Font : new FontFamily("Arial");
                FontStyle = FontStyle.Regular;
                UseDiagFont = true;
                LineDeviation = 0;
                TextPosX = 0;
                TextPosY = Size;
                Text = "Text";
                TextInBox = true;
                Locked = false;
                HorizontallyBonding = BondPositionsHorizontally.None;
                VerticallyBonding = BondPostionsVertically.None;

                Objects = new List<LifeTimeElement>();
                Groups = new List<LifeTimeGroup>();
            }
            #endregion

            #region Public Methods
            /// <summary>
            /// /// Returns the timespan if the type is TimeSpan. If not TimeSpan(0) will be returned.
            /// </summary>
            /// <param name="unit">State the base of the returned value.</param>
            /// <returns></returns>
            public long GetTimeSpan(TimeSpanBase unit = TimeSpanBase.Days)
            {
                TimeSpan duration = (EndsToday? DateTime.Now : _end) - _begin;

                if (_type != LifeTimeObjectType.TimeSpan) return 0;
                if (unit == TimeSpanBase.Minutes) return (duration).Minutes + (duration).Hours * 60 + (duration).Days * 24 * 60;
                if (unit == TimeSpanBase.Days) return (duration).Days;
                return 0;
            }

            /// <summary>
            /// Returns the elements label, replacing placeholders for duration information. 
            /// </summary>
            /// <returns></returns>
            public string GetLabel()
            {
                return Name.Replace("##years#", (GetTimeSpan(TimeSpanBase.Days) / 365.0f).ToString("F1"))
                    .Replace("##days#", GetTimeSpan(TimeSpanBase.Days).ToString())
                    .Replace("##hours#", (GetTimeSpan(TimeSpanBase.Minutes) / 60).ToString())
                    .Replace("##minutes#", GetTimeSpan(TimeSpanBase.Minutes).ToString());
            }

            /// <summary>
            /// Clone the object
            /// </summary>
            /// <returns></returns>
            public override object Clone()
            {
                return MemberwiseClone();
            }

            #endregion

            #region Private Methods
            private void CheckColorBehavior()
            {
                if (!GetRandomColor) _color = _fixedColor;
            }
            #endregion
        }

        /// <summary>
        /// LifeTimeGroup - Groups containing LifeTimeElements or further groups
        /// </summary>
        public class LifeTimeGroup : LifeTimeObjectBase
        {
            #region Properties
            /// <summary>
            /// Returns a list of available properties
            /// </summary>
            /// <returns></returns>
            public override List<String> Properties(bool All)
            {
                List<String> properties = new List<String>();

                properties.Add("Name");
                if (All) properties.Add("Enabled");
                properties.Add("OwnColor");
                if (OwnColor || All) properties.Add("GroupColor");

                return properties;
            }
            public const String XmlNodeNameDefinition = "Group";
            public override string XmlNodeName
            {
                get
                {
                    return XmlNodeNameDefinition;
                }
            }

            //Basics        

            //Data
            public Color GroupColor
            {
                get { return _groupColor; }
                set
                {
                    _groupColor = value;
                    SetGroupColor();
                }
            }
            public Boolean OwnColor { get; set; }


            #endregion

            #region Fields
            private Color _groupColor;
            #endregion

            #region Constructors
            public LifeTimeGroup(String name, Color color)
            {
                Name = name;
                Enabled = true;
                _groupColor = color;
                OwnColor = true;
                Objects = new List<LifeTimeElement>();
                Groups = new List<LifeTimeGroup>();
            }
            #endregion

            #region Public Methods
            /// <summary>
            /// Set a new groups color. All objects basecolors in this group will be changed as well.
            /// </summary>
            /// <param name="color"></param>
            public void SetGroupColor()
            {
                foreach (LifeTimeElement o in Objects)
                {
                    o.BaseColor = _groupColor;
                }
                foreach (LifeTimeGroup g in Groups)
                {
                    if (!g.OwnColor) g.SetGroupColor();
                }
            }

            /// <summary>
            /// Adds a LifeTimeObject to the group
            /// </summary>
            /// <param name="o"></param>
            public void Add(LifeTimeElement o)
            {
                o.BaseColor = _groupColor;
                Objects.Add(o);
            }

            /// <summary>
            ///  Adds a LifeTimeGroup to the group
            /// </summary>
            /// <param name="g"></param>
            public void Add(LifeTimeGroup g)
            {
                if (!g.OwnColor) g.GroupColor = _groupColor;
                Groups.Add(g);
            }

            /// <summary>
            /// Clone the object
            /// </summary>
            /// <returns></returns>
            public override object Clone()
            {
                return MemberwiseClone();
            }
            #endregion

            #region Private Methods
            #endregion
        }

        /// <summary>
        /// LifeTimeDiagramSettings - The Diagram Settings
        /// </summary>
        public class LifeTimeDiagramSettings : LifeTimeObjectBase
        {
            #region Enumerators
            #endregion

            #region Properties
            /// <summary>
            /// Returns a list of available properties
            /// </summary>
            /// <returns></returns>
            public override List<String> Properties(bool All)
            {
                List<String> properties = new List<String>();

                properties.Add("Begin");
                properties.Add("End");
                properties.Add("Width");
                properties.Add("Height");
                if (All) properties.Add("Zoom");
                if (All) properties.Add("OffsetX");
                if (All) properties.Add("OffsetY");
                properties.Add("GroupHeight");
                properties.Add("BlockHeight");
                if (All) properties.Add("GroupAreaWidth");
                properties.Add("Border");
                properties.Add("DrawShadows");
                properties.Add("BackColor");
                properties.Add("LabelColor");                
                properties.Add("Font");
                properties.Add("GlobalFontSize");
                properties.Add("Locked");
                properties.Add("ShowRefLine");
                return properties;
            }
            public const String XmlNodeNameDefinition = "DiagramSettings";
            public override string XmlNodeName
            {
                get
                {
                    return XmlNodeNameDefinition;
                }
            }

            public DateTime Begin { get; set; }
            public DateTime End { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public float Zoom { get; set; }
            public float OffsetX { get; set; }
            public float OffsetY { get; set; }
            public int GroupHeight { get; set; }
            public int BlockHeight { get; set; }
            public int GroupAreaWidth { get; set; }
            public int Border { get; set; }
            public bool DrawShadows { get; set; }
            public Color BackColor { get; set; }
            public Color LabelColor { get; set; }
            public bool ShowRefLine { get; set; }
            public FontFamily Font { get; set; }
            public int GlobalFontSize { get; set; }
            public bool Locked { get; set; }
            #endregion

            #region Constructors
            public LifeTimeDiagramSettings()
            {
                Begin = DateTime.Now;
                End = DateTime.Now.AddDays(1);
                Width = 1900;
                Height = 1200;
                Zoom = 1.0f;
                OffsetX = 0;
                OffsetY = 0;
                GroupHeight = 30;
                BlockHeight = 10;
                GroupAreaWidth = 100;
                Border = 20;
                BackColor = Color.White;
                LabelColor = Color.Black;
                DrawShadows = true;
                Font = new FontFamily("Arial");
                GlobalFontSize = 8;
                Locked = true;
            }
            #endregion

            #region Public Methods
            /// <summary>
            /// Clone the object
            /// </summary>
            /// <returns></returns>
            public override object Clone()
            {
                return MemberwiseClone();
            }
            #endregion

            #region Private Methods
            #endregion
        }

        /// <summary>
        /// LifeTimeExportSettings - The Settings for exporting the diagram
        /// </summary>
        public class LifeTimeExportSettings : LifeTimeObjectBase
        {
            #region Enumerators
            #endregion

            #region Properties
            /// <summary>
            /// Returns a list of available properties
            /// </summary>
            /// <returns></returns>
            public override List<String> Properties(bool All)
            {
                List<String> properties = new List<String>();
                properties.Add("FileName");
                properties.Add("OverwriteExisting");
                properties.Add("Width");
                properties.Add("Height");

                return properties;
            }
            public const String XmlNodeNameDefinition = "ExportSettings";
            public override string XmlNodeName
            {
                get
                {
                    return XmlNodeNameDefinition;
                }
            }

            public String FileName { get; set; }
            public bool OverwriteExisting { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            #endregion

            #region Constructors
            public LifeTimeExportSettings()
            {
                FileName = "";
                OverwriteExisting = false;
                Width = 1024;
                Height = 768;
            }
            #endregion

            #region Public Methods
            /// <summary>
            /// Clone the object
            /// </summary>
            /// <returns></returns>
            public override object Clone()
            {
                return MemberwiseClone();
            }
            #endregion

            #region Private Methods
            #endregion
        }
        #endregion
    }
}
