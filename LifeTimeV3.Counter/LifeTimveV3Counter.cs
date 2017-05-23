using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using LifeTimeV3.Counter.Display;
using LifeTimeV3.Counter.Classes;

namespace LifeTimeV3.Counter
{
    public partial class LifeTimeV3Counter : Form
    {
        #region fields
        private LifeTimeV3Display _display = null;
        private DisplayConfiguration _config = null;
        #endregion

        #region constructor
        public LifeTimeV3Counter(string[] args)
        {
            InitializeComponent();

            DoubleBuffered = true;            
            
            BackColor = Color.Pink;
            TransparencyKey = Color.Pink;

            CreateDisplay(args);

            Timer t = new Timer();
            t.Interval = 1000;
            t.Tick += new EventHandler(updatedisplay);
            t.Start();                        
        }

        private void CreateDisplay(string[] args)
        {
            string src = GetSourceFile(args);

            if(string.IsNullOrEmpty(src))
            {                
                return;
            }

            _config = new DisplayConfiguration(src);

            _display = new LifeTimeV3Display(_config.Settings.Rows, _config.Settings.Columns, _config.Settings.DotSize, _config.Settings.DotStyle, 
                _config.Settings.BackColor, _config.Settings.ForeColor, _config.Settings.DarkDotColor);

            Controls.Add(_display);

            updatedisplay(true);

            Width = _display.Width + 15;
            Height = _display.Height + 15;
        }

        private string GetSourceFile(string[] args)
        {
            string file = "";

            if (args.Length >= 1)
            {
                if (File.Exists(args[0])) file = args[0];
            }

            if (string.IsNullOrEmpty(file))
            {
                OpenFileDialog open = new OpenFileDialog();
                open.Filter = "XML Dateien (*.xml)|*.xml|Alle Dateien (*.*)|*.*";

                if (open.ShowDialog() == DialogResult.OK) file = open.FileName;
            }

            return file;
        }
        #endregion

        #region mathods
        private void updatedisplay(bool UpdateStaticValues)
        {
            foreach (IDisplayObject o in _config.Content)
            {
                if (!UpdateStaticValues && o is DisplayText) continue;
                else
                {
                    _display.SetText(o.GetValue(), o.Position);
                    _display.SetColor(o.CurrentColor, o.Position, o.GetValue().Length);
                }
            }
        }
        #endregion

        #region events
        private void LifeTimeV3Counter_Load(object sender, EventArgs e)
        {
            if (_config == null) this.Close();
            this.Location = new Point(_config.Settings.DisplayPosX, _config.Settings.DisplayPosY);
        }

        private void updatedisplay(object sender, EventArgs e)
        {
            updatedisplay(false);
        }



        private void LifeTimeV3Counter_MouseClick(object sender, MouseEventArgs e)
        {
            if (this.FormBorderStyle == System.Windows.Forms.FormBorderStyle.None)
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            else
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        }

        private void LifeTimeV3Counter_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(Color.Black), 0, 0, 5, _display.Height);
        }
        #endregion
    }
}
