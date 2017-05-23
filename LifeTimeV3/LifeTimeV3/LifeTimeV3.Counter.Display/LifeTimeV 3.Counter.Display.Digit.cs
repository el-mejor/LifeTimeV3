using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LifeTimeV3.Counter.Display
{
    public class LifeTimeV3DisplayDigit : Panel
    {
        public enum DotStyleEnum { Square, Circle }

        #region properties
        public char Value { get; set; }
        public int DotSize { get; set; }
        public Color DarkDotColor { get; set; }
        public DotStyleEnum DotStyle { get; set; }
        #endregion

        #region fields
        #endregion

        #region constructor
        public LifeTimeV3DisplayDigit()
        {
            this.Paint += new PaintEventHandler(DrawDisplay);            
        }
        #endregion

        #region public methods
        
        #endregion

        #region private methods
        private void DrawDisplay(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            Byte[] DotMatrix = GetDotMatrix5x7(Value);
            g.FillRectangle(new SolidBrush(BackColor), 0, 0, Width, Height);

            int i = 0;

            for (float row = 0.0f; row < Height; row += Height / 7.0f)
            {
                int checkdot = 0x10;
                for (float col = 0.0f; col < Width; col += Width / 5.0f)
                {
                    int dot = checkdot & DotMatrix[i];
                    if (dot > 0) DrawDot(g, new SolidBrush(ForeColor), col, row);
                    else DrawDot(g, new SolidBrush(DarkDotColor), col, row);
                    checkdot = checkdot / 2;
                }
                i++;
            }
        }

        private void DrawDot(Graphics g, SolidBrush b, float col, float row)
        {
            if(DotStyle == DotStyleEnum.Square)
            {
                g.FillRectangle(b, col, row, DotSize , DotSize);                
            }
            else
            {
                g.FillEllipse(b, col, row, DotSize, DotSize);
            }
        }
        #endregion

        #region static methods
        public static byte[] GetDotMatrix5x7(char value)
        {
            switch (value)
            {
                case (char)255:
                    return GetDotMatrix5x7Of_Fill();
                case (char)254:
                    return GetDotMatrix5x7Of_BigDot();
                case (char)253:
                    return GetDotMatrix5x7Of_SmallDot();

                case ' ':
                    return GetDotMatrix5x7Of_WhiteSpace();
                    
                case '0':
                    return GetDotMatrix5x7Of_0();
                case '1':
                    return GetDotMatrix5x7Of_1();
                case '2':
                    return GetDotMatrix5x7Of_2();
                case '3':
                    return GetDotMatrix5x7Of_3();
                case '4':
                    return GetDotMatrix5x7Of_4();
                case '5':
                    return GetDotMatrix5x7Of_5();
                case '6':
                    return GetDotMatrix5x7Of_6();
                case '7':
                    return GetDotMatrix5x7Of_7();
                case '8':
                    return GetDotMatrix5x7Of_8();
                case '9':
                    return GetDotMatrix5x7Of_9();


                case '*':
                    return GetDotMatrix5x7Of_Star();
                case '.':
                    return GetDotMatrix5x7Of_Dot();
                case ':':
                    return GetDotMatrix5x7Of_DoubleDot();
                case '/':
                    return GetDotMatrix5x7Of_Slash();


                case 'A':
                    return GetDotMatrix5x7Of_A();
                case 'B':
                    return GetDotMatrix5x7Of_B();
                case 'C':
                    return GetDotMatrix5x7Of_C();
                case 'D':
                    return GetDotMatrix5x7Of_D();
                case 'E':
                    return GetDotMatrix5x7Of_E();
                case 'F':
                    return GetDotMatrix5x7Of_F();
                case 'G':
                    return GetDotMatrix5x7Of_G();
                case 'H':
                    return GetDotMatrix5x7Of_H();
                case 'I':
                    return GetDotMatrix5x7Of_I();
                case 'J':
                    return GetDotMatrix5x7Of_J();
                case 'K':
                    return GetDotMatrix5x7Of_K();
                case 'L':
                    return GetDotMatrix5x7Of_L();
                case 'M':
                    return GetDotMatrix5x7Of_M();
                case 'N':
                    return GetDotMatrix5x7Of_N();
                case 'O':
                    return GetDotMatrix5x7Of_O();
                case 'P':
                    return GetDotMatrix5x7Of_P();
                case 'R':
                    return GetDotMatrix5x7Of_R();
                case 'S':
                    return GetDotMatrix5x7Of_S();
                case 'T':
                    return GetDotMatrix5x7Of_T();
                case 'U':
                    return GetDotMatrix5x7Of_U();
                case 'V':
                    return GetDotMatrix5x7Of_V();
                case 'W':
                    return GetDotMatrix5x7Of_W();
                case 'X':
                    return GetDotMatrix5x7Of_X();
                case 'Y':
                    return GetDotMatrix5x7Of_Y();
                case 'Z':
                    return GetDotMatrix5x7Of_Z();
                    
                default:
                    break;
            }

            return GetDotMatrix5x7Of_WhiteSpace();
        }
        
        //Fill
        private static byte[] GetDotMatrix5x7Of_Fill()
        {
            return new byte[] { 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f };
        }

        // White Space
        private static byte[] GetDotMatrix5x7Of_WhiteSpace()
        {            
            return new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        }
        // 0
        private static byte[] GetDotMatrix5x7Of_0()
        {            
            return new byte[] { 0x0E, 0x11, 0x13, 0x15, 0x19, 0x11, 0x0E };
        }
        // 1
        private static byte[] GetDotMatrix5x7Of_1()
        {            
            return new byte[] { 0x04, 0x0C, 0x04, 0x04, 0x04, 0x04, 0x0E };
        }
        // 2
        private static byte[] GetDotMatrix5x7Of_2()
        {            
            return new byte[] { 0x0e, 0x11, 0x01, 0x02, 0x04, 0x08, 0x1f };
        }
        // 3
        private static byte[] GetDotMatrix5x7Of_3()
        {            
            return new byte[] { 0x1f, 0x02, 0x04, 0x02, 0x01, 0x11, 0x0e };
        }
        // 4
        private static byte[] GetDotMatrix5x7Of_4()
        {
            return new byte[] { 0x02, 0x06, 0x0a, 0x12, 0x1f, 0x02, 0x02 };
        }
        // 5
        private static byte[] GetDotMatrix5x7Of_5()
        {
            return new byte[] { 0x1f, 0x10, 0x1e, 0x01, 0x01, 0x11, 0x0e };
        }
        // 6
        private static byte[] GetDotMatrix5x7Of_6()
        {
            return new byte[] { 0x06, 0x08, 0x10, 0x1e, 0x11, 0x11, 0x0e };
        }
        // 7
        private static byte[] GetDotMatrix5x7Of_7()
        {
            return new byte[] { 0x1f, 0x01, 0x02, 0x04, 0x08, 0x08, 0x08 };
        }
        // 8
        private static byte[] GetDotMatrix5x7Of_8()
        {
            return new byte[] { 0x0e, 0x11, 0x11, 0x0e, 0x11, 0x11, 0x0e };
        }
        // 9
        private static byte[] GetDotMatrix5x7Of_9()
        {
            return new byte[] { 0x0e, 0x11, 0x11, 0x0f, 0x01, 0x02, 0x0c };
        }

        // Star
        private static byte[] GetDotMatrix5x7Of_Star()
        {
            return new byte[] { 0x00, 0x04, 0xf5, 0x0e, 0xf5, 0x04, 0x00 };
        }

        // Big Dot
        private static byte[] GetDotMatrix5x7Of_BigDot()
        {
            return new byte[] { 0x00, 0x0e, 0x1f, 0x1f, 0x1f, 0x0e, 0x00 };
        }

        // Small Dot
        private static byte[] GetDotMatrix5x7Of_SmallDot()
        {
            return new byte[] { 0x00, 0x06, 0x0f, 0x0f, 0x06, 0x00, 0x00 };
        }

        // Dot
        private static byte[] GetDotMatrix5x7Of_Dot()
        {
            return new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x0c, 0x0c };
        }
        // DoubleDot
        private static byte[] GetDotMatrix5x7Of_DoubleDot()
        {
            return new byte[] { 0x00, 0x0c, 0x0c, 0x00, 0x0c, 0x0c, 0x00 };
        }
        // Slash
        private static byte[] GetDotMatrix5x7Of_Slash()
        {
            return new byte[] { 0x00, 0x01, 0x02, 0x04, 0x08, 0x10, 0x00 };
        }

        // A
        private static byte[] GetDotMatrix5x7Of_A()
        {
            return new byte[] { 0x0e, 0x11, 0x11, 0x11, 0x1f, 0x11, 0x11 };
        }
        // B
        private static byte[] GetDotMatrix5x7Of_B()
        {
            return new byte[] { 0x1e, 0x11, 0x11, 0x1e, 0x11, 0x11, 0x1e };
        }
        // C
        private static byte[] GetDotMatrix5x7Of_C()
        {
            return new byte[] { 0x0e, 0x11, 0x10, 0x10, 0x10, 0x11, 0x0e };
        }
        // D
        private static byte[] GetDotMatrix5x7Of_D()
        {
            return new byte[] { 0x1c, 0x12, 0x11, 0x11, 0x11, 0x12, 0x1c };
        }
        // E
        private static byte[] GetDotMatrix5x7Of_E()
        {
            return new byte[] { 0x1f, 0x10, 0x10, 0x1e, 0x10, 0x10, 0x1f };
        }
        // F
        private static byte[] GetDotMatrix5x7Of_F()
        {
            return new byte[] { 0x1f, 0x10, 0x10, 0x1e, 0x10, 0x10, 0x10 };
        }
        // G
        private static byte[] GetDotMatrix5x7Of_G()
        {
            return new byte[] { 0x0e, 0x11, 0x10, 0x17, 0x11, 0x11, 0x0f };
        }
        // H
        private static byte[] GetDotMatrix5x7Of_H()
        {
            return new byte[] { 0x11, 0x11, 0x11, 0x1f, 0x11, 0x11, 0x11 };
        }
        // I
        private static byte[] GetDotMatrix5x7Of_I()
        {
            return new byte[] { 0x0e, 0x04, 0x04, 0x04, 0x04, 0x04, 0x0e };
        }
        // J
        private static byte[] GetDotMatrix5x7Of_J()
        {
            return new byte[] { 0x07, 0x02, 0x02, 0x02, 0x02, 0x12, 0x0C };
        }
        // K
        private static byte[] GetDotMatrix5x7Of_K()
        {
            return new byte[] { 0x11, 0x12, 0x14, 0x18, 0x14, 0x12, 0x11 };
        }
        // L
        private static byte[] GetDotMatrix5x7Of_L()
        {
            return new byte[] { 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x1f };
        }
        // M
        private static byte[] GetDotMatrix5x7Of_M()
        {
            return new byte[] { 0x11, 0x1b, 0x15, 0x15, 0x11, 0x11, 0x11 };
        }
        // N
        private static byte[] GetDotMatrix5x7Of_N()
        {
            return new byte[] { 0x11, 0x11, 0x19, 0x15, 0x13, 0x11, 0x11 };
        }
        // O
        private static byte[] GetDotMatrix5x7Of_O()
        {
            return new byte[] { 0x0e, 0x11, 0x11, 0x11, 0x11, 0x11, 0x0e };
        }
        // P
        private static byte[] GetDotMatrix5x7Of_P()
        {
            return new byte[] { 0x1e, 0x11, 0x11, 0x1e, 0x10, 0x10, 0x10 };
        }
        // R
        private static byte[] GetDotMatrix5x7Of_R()
        {
            return new byte[] { 0x1e, 0x11, 0x11, 0x1e, 0x14, 0x12, 0x11 };
        }
        // S
        private static byte[] GetDotMatrix5x7Of_S()
        {
            return new byte[] { 0x0f, 0x10, 0x10, 0x0e, 0x01, 0x01, 0x1e };
        }
        // T
        private static byte[] GetDotMatrix5x7Of_T()
        {
            return new byte[] { 0x1f, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04 };
        }
        // U
        private static byte[] GetDotMatrix5x7Of_U()
        {
            return new byte[] { 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x0e };
        }
        // V
        private static byte[] GetDotMatrix5x7Of_V()
        {
            return new byte[] { 0x11, 0x11, 0x11, 0x11, 0x11, 0x0a, 0x04 };
        }
        // W
        private static byte[] GetDotMatrix5x7Of_W()
        {
            return new byte[] { 0x11, 0x11, 0x11, 0x15, 0x15, 0x15, 0x0a };
        }
        // X
        private static byte[] GetDotMatrix5x7Of_X()
        {
            return new byte[] { 0x11, 0x11, 0x0a, 0x04, 0x0a, 0x11, 0x11 };
        }
        // Y
        private static byte[] GetDotMatrix5x7Of_Y()
        {
            return new byte[] { 0x11, 0x11, 0x11, 0x0a, 0x04, 0x04, 0x04 };
        }
        // Z
        private static byte[] GetDotMatrix5x7Of_Z()
        {
            return new byte[] { 0x1f, 0x01, 0x02, 0x04, 0x08, 0x10, 0x1f };
        }
        #endregion
    }
}
