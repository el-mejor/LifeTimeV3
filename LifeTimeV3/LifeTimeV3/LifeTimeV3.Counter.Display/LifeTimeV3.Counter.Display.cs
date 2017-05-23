using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LifeTimeV3.Counter.Display
{
    public class LifeTimeV3Display : Panel
    {
        #region properties
        public int Rows { get; set; }
        public int Columns { get; set; }
        public int DotSize { get; set; }
        public LifeTimeV3DisplayDigit.DotStyleEnum DotStyle { get; set; }
        public Color DarkDotColor { get; set; } 
        #endregion

        #region fields
        #endregion

        #region constructor
        public LifeTimeV3Display(int rows, int columns, int dotsize, LifeTimeV3DisplayDigit.DotStyleEnum dotstyle, Color backcolor, Color forecolor, Color darkcolor)
        {
            Rows = rows; Columns = columns; DotSize = dotsize; DotStyle = dotstyle;
            this.BackColor = backcolor; 
            this.ForeColor = forecolor; 
            this.DarkDotColor = darkcolor;

            this.Height = 4 + Rows * (8 * DotSize);
            this.Width = 4 + Columns * (6 * DotSize);

            for (short r = 0; r < Rows; r++)
            {
                for (short c = 0; c < Columns; c++)
                {
                    LifeTimeV3DisplayDigit d = new LifeTimeV3DisplayDigit();
                    d.DotSize = DotSize;
                    d.DotStyle = DotStyle;
                    d.BackColor = this.BackColor;
                    d.ForeColor = this.ForeColor;
                    d.DarkDotColor = this.DarkDotColor;
                    d.DotSize = DotSize;
                    d.Width = 5 * DotSize;
                    d.Height = 7 * DotSize;
                    d.Top = 2 + r * (8 * DotSize);
                    d.Left = 2 + c * (6 * DotSize);
                    d.Value = ' ';
                    this.Controls.Add(d);
                }
            }
        }
        #endregion

        #region public methods
        /// <summary>
        /// Show text on the display, starting on the given row/columns
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        public void SetText(String Text, DisplayPosition pos)
        {
            char[] text = Text.ToCharArray();

            int i = pos.Row * Columns + pos.Column;

            for(int j = i; j < i + text.Length && j < this.Controls.Count; j++)
            {
                LifeTimeV3DisplayDigit d = this.Controls[j] as LifeTimeV3DisplayDigit;
                if (d.Value != text[j-i])
                {
                    d.Value = text[j-i];
                    d.Refresh();
                }                
            }
        }

        public void SetColor(Color forecolor, DisplayPosition pos, int length)
        {
            int i = pos.Row * Columns + pos.Column;

            for(int j = i; j < i + length && j < this.Controls.Count; j++)
            {
                if (!this.Controls[j].ForeColor.Equals(forecolor))
                {
                    this.Controls[j].ForeColor = forecolor;
                    this.Controls[j].Refresh();
                }
            }
        }

        /// <summary>
        /// Clear the entire display
        /// </summary>
        public void Clear()
        {
            foreach(LifeTimeV3DisplayDigit d in this.Controls)
            {
                d.Value = ' ';
                d.Refresh();
            }
        }
        #endregion

        #region private methods
        #endregion
    }

    public class DisplayPosition
    {
        #region properties
        public int Row { get; set; }
        public int Column { get; set; }
        #endregion

        #region constructor
        public DisplayPosition(int r, int c)
        {
            Row = r; Column = c;
        }
        #endregion
    }
}
