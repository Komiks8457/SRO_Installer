using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SRO_Installer_Boobies.Classes
{
    internal class ProgressBarEx : UserControl
    {
        private int min;// Minimum value for progress range
        private int max = 100;// Maximum value for progress range
        private int val;// Current progress
        private Color BarColor1 = Color.DarkOrchid;// Color of progress meter
        private Color BarColor2 = Color.DeepSkyBlue;// Color of progress meter

        protected override void OnResize(EventArgs e)
        {
            // Invalidate the control to get a repaint.
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            var brush = new LinearGradientBrush(new Point(0, 0), new Point(0, 1), BarColor1, BarColor2);
            var percent = (val - min) / (float)(max - min);
            var rect = ClientRectangle;
            // Calculate area for drawing the progress.
            rect.Width = (int)(rect.Width * percent);

            // Draw the progress meter.
            g.FillRectangle(brush, rect);

            // Draw a three-dimensional border around the control.
            //Draw3DBorder(g);

            // Clean up.
            brush.Dispose();
            g.Dispose();
        }

        public int Minimum
        {
            get => min;

            set
            {
                // Prevent a negative value.
                if (value < 0)
                {
                    value = 0;
                }

                // Make sure that the minimum value is never set higher than the maximum value.
                if (value > max)
                {
                    max = value;
                }

                min = value;

                // Ensure value is still in range
                if (val < min)
                {
                    val = min;
                }

                // Invalidate the control to get a repaint.
                Invalidate();
            }
        }

        public int Maximum
        {
            get => max;

            set
            {
                // Make sure that the maximum value is never set lower than the minimum value.
                if (value < min)
                {
                    min = value;
                }

                max = value;

                // Make sure that value is still in range.
                if (val > max)
                {
                    val = max;
                }

                // Invalidate the control to get a repaint.
                Invalidate();
            }
        }

        public int Value
        {
            get => val;

            set
            {
                var oldValue = val;

                // Make sure that the value does not stray outside the valid range.
                if (value < min)
                {
                    val = min;
                }
                else if (value > max)
                {
                    val = max;
                }
                else
                {
                    val = value;
                }

                // Invalidate only the changed area.

                var newValueRect = ClientRectangle;
                var oldValueRect = ClientRectangle;

                // Use a new value to calculate the rectangle for progress.
                var percent = (val - min) / (float)(max - min);
                newValueRect.Width = (int)(newValueRect.Width * percent);

                // Use an old value to calculate the rectangle for progress.
                percent = (oldValue - min) / (float)(max - min);
                oldValueRect.Width = (int)(oldValueRect.Width * percent);

                var updateRect = new Rectangle();

                // Find only the part of the screen that must be updated.
                if (newValueRect.Width > oldValueRect.Width)
                {
                    updateRect.X = oldValueRect.Size.Width;
                    updateRect.Width = newValueRect.Width - oldValueRect.Width;
                }
                else
                {
                    updateRect.X = newValueRect.Size.Width;
                    updateRect.Width = oldValueRect.Width - newValueRect.Width;
                }

                updateRect.Height = Height;

                // Invalidate the intersection region only.
                Invalidate(updateRect);
            }
        }

        public Color ProgressBarColor1
        {
            get => BarColor1;

            set
            {
                BarColor1 = value;

                // Invalidate the control to get a repaint.
                Invalidate();
            }
        }

        public Color ProgressBarColor2
        {
            get => BarColor2;

            set
            {
                BarColor2 = value;

                // Invalidate the control to get a repaint.
                Invalidate();
            }
        }

        private void Draw3DBorder(Graphics g)
        {
            var PenWidth = (int)Pens.White.Width;

            g.DrawLine(Pens.DarkGray,
            new Point(ClientRectangle.Left, ClientRectangle.Top),
            new Point(ClientRectangle.Width - PenWidth, ClientRectangle.Top));
            g.DrawLine(Pens.DarkGray,
            new Point(ClientRectangle.Left, ClientRectangle.Top),
            new Point(ClientRectangle.Left, ClientRectangle.Height - PenWidth));
            g.DrawLine(Pens.White,
            new Point(ClientRectangle.Left, ClientRectangle.Height - PenWidth),
            new Point(ClientRectangle.Width - PenWidth, ClientRectangle.Height - PenWidth));
            g.DrawLine(Pens.White,
            new Point(ClientRectangle.Width - PenWidth, ClientRectangle.Top),
            new Point(ClientRectangle.Width - PenWidth, ClientRectangle.Height - PenWidth));
        }
    }
}
