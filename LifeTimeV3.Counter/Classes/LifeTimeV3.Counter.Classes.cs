using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml;

using LifeTimeV3.Counter.Display;

namespace LifeTimeV3.Counter.Classes
{
    #region DisplayConfiguration and Content
    /// <summary>
    /// Containing all information for the display, stored in the given XML
    /// </summary>
    public class DisplayConfiguration
    {
        #region properties
        public DisplaySettings Settings { get; set; }
        public List<IDisplayObject> Content { get; set; }
        #endregion

        #region constructor
        public DisplayConfiguration(String file)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(file);

            Settings = new DisplaySettings(xml);
            DisplayContent(xml);
        }
        #endregion

        #region public methods
        #endregion

        #region private methods
        private void DisplayContent(XmlDocument xml)
            {
                Content = new List<IDisplayObject>();

                foreach (XmlNode n in xml.SelectNodes("LifeTimeCounter/content/*"))
                {
                    if (n.Name == "text") Content.Add(GetContentObject(new DisplayText(), n));
                    else if (n.Name == "value") Content.Add(GetContentObject(new DisplayDynamicValue(), n));
                }
            }

        private IDisplayObject GetContentObject(IDisplayObject o, XmlNode n)
        {
            //<text String="F/A: " PositionRow="0" PositionColumn="0" ForeColor="255,255,255,255"/>
            //<value ValueStyle="1"" TargetTime="01.01.2016 00:00:00" PositionRow="0" PositionColumn="0" ForeColor="255,0,255,0"  Limit1="24.12.2015 00:00:00"
            //   Limit2="01.01.2099 00:00:00" ForeColorLimit1Exceeded="255,255,255,0" ForeColorLimit2Exceeded="255,255,0,0"/>

            foreach (XmlAttribute a in n.Attributes)
            {
                switch (a.Name)
                {
                    case "String":
                        o.Text = a.Value;
                        break;
                    case "PositionRow":
                        o.Position.Row = Convert.ToInt16(a.Value);
                        break;
                    case "PositionColumn":
                        o.Position.Column = Convert.ToInt16(a.Value);
                        break;
                    case "ForeColor":
                        o.ForeColor = DisplayConfiguration.GetColorFromArgbString(a.Value);
                        break;
                    case "TargetTime":
                        o.TargetTime = Convert.ToDateTime(a.Value);
                        break;
                    case "Limit1":
                        o.Limit1 = Convert.ToDateTime(a.Value);
                        break;
                    case "Limit2":
                        o.Limit2 = Convert.ToDateTime(a.Value);
                        break;
                    case "ForeColorLimit1Exceeded":
                        o.ForeColorLimit1Exceeded = DisplayConfiguration.GetColorFromArgbString(a.Value);
                        break;
                    case "ForeColorLimit2Exceeded":
                        o.ForeColorLimit2Exceeded = DisplayConfiguration.GetColorFromArgbString(a.Value);
                        break;
                    case "ValueStyle":
                        if (a.Value == "1") o.CountDownStyle = IDisplayObject.CountDownStyleEnum.Days;
                        else if (a.Value == "2") o.CountDownStyle = IDisplayObject.CountDownStyleEnum.Time;
                        else if (a.Value == "3") o.CountDownStyle = IDisplayObject.CountDownStyleEnum.DaysAndTime;
                        break;
                    case "Format":
                        o.Format = a.Value;
                        break;
                    case "Daily":
                        if (a.Value.ToLower() == "true") o.Daily = true;
                        else o.Daily = false;
                        break;
                }
            }

            if (o.Daily)
            {
                if (o.TargetTime != DateTime.MinValue) o.TargetTime = ConvertToToday(o.TargetTime);
                if (o.Limit1 != DateTime.MinValue) o.Limit1 = ConvertToToday(o.Limit1);
                if (o.Limit2 != DateTime.MinValue) o.Limit2 = ConvertToToday(o.Limit2);
            }

            return o;
        }

        private DateTime ConvertToToday(DateTime d)
        {
            return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, d.Hour, d.Minute, d.Second);
        }
        #endregion

        #region ColorStringConverter Static
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

        /// <summary>
        /// The display settings
        /// </summary>
        public class DisplaySettings
        {
            #region properties
            public int DisplayPosX { get; set; }
            public int DisplayPosY { get; set; }
            public int DotSize { get; set; }
            public LifeTimeV3DisplayDigit.DotStyleEnum DotStyle { get; set; }
            public int Rows { get; set; }
            public int Columns { get; set; }
            public Color BackColor { get; set; }
            public Color ForeColor { get; set; }
            public Color DarkDotColor { get; set; }
            #endregion

            #region constructor
            public DisplaySettings(XmlDocument xml)
            {                
                foreach (XmlAttribute a in xml.SelectSingleNode("LifeTimeCounter/display").Attributes)
                {
                    //<Display DisplayPosX ="100" DisplayPosY="0" Cols="2" Rows="30" BackColor="255,10,10,10" DarkDotColor="255,20,20,20" ForeColor="255,255,255,255"/>
                    switch (a.Name)
                    {
                        case "DisplayPosX":
                            DisplayPosX = Convert.ToInt16(a.Value);
                            break;
                        case "DisplayPosY":
                            DisplayPosY = Convert.ToInt16(a.Value);
                            break;
                        case "DotSize":
                            DotSize = Convert.ToInt16(a.Value);
                            break;
                        case "DotStyle":
                            if (a.Value.ToLower() == "circle") DotStyle = LifeTimeV3DisplayDigit.DotStyleEnum.Circle;
                            else DotStyle = LifeTimeV3DisplayDigit.DotStyleEnum.Square;
                            break;
                        case "Cols":
                            Columns = Convert.ToInt16(a.Value);
                            break;
                        case "Rows":
                            Rows = Convert.ToInt16(a.Value);
                            break;
                        case "BackColor":
                            BackColor = DisplayConfiguration.GetColorFromArgbString(a.Value);
                            break;
                        case "DarkDotColor":
                            DarkDotColor = DisplayConfiguration.GetColorFromArgbString(a.Value);
                            break;
                        case "ForeColor":
                            ForeColor = DisplayConfiguration.GetColorFromArgbString(a.Value);
                            break;
                    }
                }
            }
            #endregion
        }
    }
    #endregion

    #region display objects
    public class IDisplayObjectBase
    {
        public enum CountDownStyleEnum { Days = 1, Time = 2, DaysAndTime = 3 }

        #region constructor
        public IDisplayObjectBase()
        {
            Position = new DisplayPosition(0, 0);
        }
        #endregion

        #region properties
        public DisplayPosition Position { get; set; }
        public Color BackColor { get; set; }
        public Color DarkDotColor { get; set; }
        public Color ForeColor { get; set; }
        public Color CurrentColor { get; set; }
        public String Text { get; set; }
        public DateTime TargetTime { get; set; }
        public bool Daily { get; set; }
        public CountDownStyleEnum CountDownStyle { get; set; }
        public String Format { get; set; }
        public DateTime Limit1 { get; set; }
        public Color ForeColorLimit1Exceeded { get; set; }
        public DateTime Limit2 { get; set; }
        public Color ForeColorLimit2Exceeded { get; set; }
        #endregion

        #region methods
        public void CheckLimits()
        {
            CurrentColor = ForeColor;
            if (TargetTime == null || Limit1 == null || Limit2 == null) return;

            if (CheckLimit(Limit1)) CurrentColor = ForeColorLimit1Exceeded;
            if (CheckLimit(Limit2)) CurrentColor = ForeColorLimit2Exceeded;
        }

        private bool CheckLimit(DateTime limit)
        {
            if (limit == DateTime.MinValue) return false;
            if (DateTime.Compare(limit, DateTime.Now) < 0) return true;
            else return false;
        }
        #endregion
    }

    public abstract class IDisplayObject : IDisplayObjectBase
    {        
        public abstract string GetValue();        
    }

    /// <summary>
    /// Show a static string on the display
    /// </summary>
    public class DisplayText : IDisplayObject
    {
        #region public methods
        public override string GetValue()
        {
            CheckLimits();
            return Text;
        }
        #endregion        
    }

    /// <summary>
    /// Show a dynamic value (Countdown / Duration)
    /// </summary>
    public class DisplayDynamicValue : IDisplayObject
    {
        #region public methods
        public override string GetValue()
        {
            CheckLimits();

            int d = (TargetTime - DateTime.Now).Days;
            int h = (TargetTime - DateTime.Now).Hours;
            int m = (TargetTime - DateTime.Now).Minutes;
            int s = (TargetTime - DateTime.Now).Seconds;

            if (d < 0) d *= -1;
            if (h < 0) h *= -1;
            if (m < 0) m *= -1;
            if (s < 0) s *= -1;

            switch (CountDownStyle)
            {
                case CountDownStyleEnum.Days:
                    return d.ToString(Format);                    

                case CountDownStyleEnum.DaysAndTime:
                    return string.Format("{0}:{1}:{2}:{3}", 
                        d.ToString(Format),
                        h.ToString("D2"),
                        m.ToString("D2"),
                        s.ToString("D2"));                    

                case CountDownStyleEnum.Time:
                    return string.Format("{0}:{1}:{2}",                        
                        h.ToString("D2"),
                        m.ToString("D2"),
                        s.ToString("D2"));                    
            }

            return "ERR";
        }
        #endregion        
    }
    #endregion
}
