using System.Drawing;
using System.Windows.Forms;

using LifeTimeV3.BL.LifeTimeDiagram;
using LifeTimeV3.LifeTimeDiagram.Toolbox.Controls;
using LifeTimeV3.Src;
using System;

namespace LifeTimeV3.LifeTimeDiagram.Toolbox
{
    /// <summary>
    /// Toolbox Form
    /// </summary>
    public class LifeTimeToolBoxForm : Form
    {
        #region Properties
        public LifeTimeObjectBrowser ObjectBrowser { get; private set; }
        #endregion

        #region Fields
        private LifeTimeObjectPropertyGrid _propertyGrid { get; set; }
        private LifeTimeObjectPropertyGrid _settingsGrid { get; set; }
        private LifeTimeExportPNGPropertyGrid _exportGrid { get; set; }        
        #endregion

        #region constructor
        public LifeTimeToolBoxForm(LifeTimeObjectPropertyGrid propertyGrid, LifeTimeObjectPropertyGrid settingsGrid, LifeTimeExportPNGPropertyGrid exportGrid, LifeTimeObjectBrowser objectBrowser)
        {
            _propertyGrid = propertyGrid;
            _settingsGrid = settingsGrid;
            _exportGrid = exportGrid;
            ObjectBrowser = objectBrowser;

            this.Text = LifeTimeV3TextList.GetText("[213]"); //Toolbox
            this.Font = new Font("Arial", 8.0f);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            //this.ControlBox = false;
            this.TopMost = true;
            this.Width = 350; this.Height = 700;
            this.StartPosition = FormStartPosition.Manual;
            this.ShowInTaskbar = false;
            this.FormClosing += new FormClosingEventHandler(Toolbox_Closing);

            _propertyGrid.Dock = DockStyle.Fill;
            _propertyGrid.SetObject(null);

            SplitContainer _split = new SplitContainer();
            _split.Dock = DockStyle.Fill;
            _split.Orientation = Orientation.Horizontal;

            _split.Panel1.Controls.Add(PropertyGridTab());

            ObjectBrowser.Dock = DockStyle.Fill;
            ObjectBrowser.FindObjectControl.Dock = DockStyle.Bottom;

            _split.Panel2.Controls.Add(ObjectBrowser);
            _split.Panel2.Controls.Add(ObjectBrowser.FindObjectControl);
            

            this.Controls.Add(_split);

            _split.SplitterDistance = 420;
        }
        #endregion

        #region private methods
        private TabControl PropertyGridTab()
        {
            TabControl tabs = new TabControl();
            tabs.Dock = DockStyle.Fill;
            this.Controls.Add(tabs);

            TabPage ObjPropGrid = new TabPage();
            tabs.TabPages.Add(ObjPropGrid);
            ObjPropGrid.Text = LifeTimeV3TextList.GetText("[214]"); //Element
            _propertyGrid.Dock = DockStyle.Fill;
            _propertyGrid.SetObject(null);
            ObjPropGrid.Controls.Add(_propertyGrid);

            TabPage DiagSetGrid = new TabPage();
            tabs.TabPages.Add(DiagSetGrid);
            DiagSetGrid.Text = LifeTimeV3TextList.GetText("[215]"); //Settings
            _settingsGrid.Dock = DockStyle.Fill;
            DiagSetGrid.Controls.Add(_settingsGrid);

            TabPage ExpPNGGrid = new TabPage();
            tabs.TabPages.Add(ExpPNGGrid);
            ExpPNGGrid.Text = LifeTimeV3TextList.GetText("[216]"); //Export
            _exportGrid.Dock = DockStyle.Fill;
            ExpPNGGrid.Controls.Add(_exportGrid);

            return tabs;
        }
        #endregion

        #region eventhandler
        private void Toolbox_Closing(object sender, FormClosingEventArgs e)
        {
            Visible = false;
            e.Cancel = true;            
        }
        #endregion
    }
}
