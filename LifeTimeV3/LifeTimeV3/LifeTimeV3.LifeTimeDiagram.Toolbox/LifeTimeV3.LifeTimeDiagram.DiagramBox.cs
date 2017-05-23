using System;
using System.Windows.Forms;
using LifeTimeV3.Src;

namespace LifeTimeV3.LifeTimeDiagram.DiagramBox
{
    /// <summary>
    /// Diagram Box
    /// </summary>
    public class LifeTimeDiagramBox : PictureBox
    {
        #region Properties
        public float OffsetX { get; set; }
        public float OffsetY { get; set; }
        public float Zoom { get; set; }
        public bool Moving { get; private set; }
        public bool Scaling { get; private set; }
        #endregion

        #region Fields
        private int _currMousePosX;
        private int _currMousePosY;
        private bool _rightButton { get; set; }

        #endregion

        #region Cunstructor
        public LifeTimeDiagramBox()
        {
            Reset();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Reset to Zoom = 1 and no shift
        /// </summary>
        public void Reset()
        {
            OffsetX = 0; OffsetY = 0; Zoom = 1; Moving = false;
            ZoomEventArgs e = new ZoomEventArgs();
            e.Zoom = Zoom;
            if (DiagramZoomed != null) DiagramZoomed(this, e);
        }

        /// <summary>
        /// Increase Zoom by 0.1
        /// </summary>
        public void ZoomIn()
        {
            Zoom += 0.1f;
        }

        /// <summary>
        /// Decrease Zoom by 0.1
        /// </summary>
        public void ZoomOut()
        {
            Zoom -= 0.1f;
            if (Zoom <= 0) Zoom = 0.1f;
        }

        /// <summary>
        /// Start any Mouseaction in the Diagram (Move or Zoom)
        /// </summary>
        /// <param name="e"></param>
        public void BeginMouse(MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right) _rightButton = true;
            if (e.Button == System.Windows.Forms.MouseButtons.Left && !_rightButton) BeginMove(e.X, e.Y);
            if (e.Button == System.Windows.Forms.MouseButtons.Left && _rightButton) BeginZoom(e.Y);
        }

        /// <summary>
        /// Update when Mouse is moved (Move or Zoom)
        /// </summary>
        /// <param name="e"></param>
        public void MoveMouse(MouseEventArgs e)
        {
            if (Moving)
            {
                EndMove(e.X, e.Y);
                BeginMove(e.X, e.Y);

            }
            if (Scaling)
            {
                EndZoom(e.Y);
                BeginZoom(e.Y);
            }
        }

        /// <summary>
        /// Finalize Mouseaction (Move or Zoom)
        /// </summary>
        /// <param name="e"></param>
        public void EndMouse(MouseEventArgs e)
        {
            _rightButton = false;

            if (Moving)
            {
                EndMove(e.X, e.Y);
            }
            if (Scaling)
            {
                EndZoom(e.Y);
            }
        }
        #endregion

        #region private methods
        /// <summary>
        /// Begin of movement
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void BeginMove(int x, int y)
        {
            //if (Zoom == 1) return;
            _currMousePosX = x; _currMousePosY = y;
            Moving = true;
        }

        /// <summary>
        /// End of movement
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void EndMove(int x, int y)
        {
            Moving = false;
            OffsetX -= (_currMousePosX - x) / Zoom; OffsetY -= (_currMousePosY - y) / Zoom;
        }

        /// <summary>
        /// Zooming by pressing both Mousebuttons and moving
        /// </summary>
        /// <param name="p"></param>
        private void BeginZoom(int y)
        {
            if (Moving) return;
            _currMousePosY = y;
            Scaling = true;
        }

        /// <summary>
        /// End Zooming by pressing both Mousebuttons
        /// </summary>
        /// <param name="x"></param>
        private void EndZoom(int y)
        {
            Scaling = false;
            Zoom += (_currMousePosY - y) * 0.01f;

            ZoomEventArgs e = new ZoomEventArgs();
            e.Zoom = Zoom;
            if (DiagramZoomed != null) DiagramZoomed(this, e);
        }
        #endregion

        #region eventhandler
        public delegate void DiagramZoomedEventHandler(object sender, ZoomEventArgs e);
        public event DiagramZoomedEventHandler DiagramZoomed;
        public class ZoomEventArgs : EventArgs
        {
            public float Zoom { get; set; } 
        }
        #endregion
    }
}
