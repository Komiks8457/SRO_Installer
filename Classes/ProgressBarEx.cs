using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SRO_Installer_Boobies.Classes
{
    internal class ProgressBarEx : UserControl
    {
        private int val, min, max = 100;
        private Color BarColor1 = Color.DarkOrchid;
        private Color BarColor2 = Color.DeepSkyBlue;
        private Image BarImage;

        protected override void OnResize(EventArgs e) => Invalidate();

        protected override void OnPaint(PaintEventArgs e)
        {
            Brush brush;

            var g = e.Graphics;
            
            if (BarImage == null)
            {
                brush = new LinearGradientBrush(new Point(0, 0), new Point(0, 1), BarColor1, BarColor2);
            }
            else brush = new TextureBrush(BarImage);

            var percent = (val - min) / (float)(max - min);

            var rect = ClientRectangle;

            rect.Width = (int)(rect.Width * percent);
            
            g.FillRectangle(brush, rect);

            brush.Dispose();

            g.Dispose();
        }

        public int Minimum
        {
            get => min;

            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                
                if (value > max)
                {
                    max = value;
                }

                min = value;
                
                if (val < min)
                {
                    val = min;
                }
                
                Invalidate();
            }
        }

        public int Maximum
        {
            get => max;

            set
            {
                if (value < min)
                {
                    min = value;
                }

                max = value;
                
                if (val > max)
                {
                    val = max;
                }
                
                Invalidate();
            }
        }

        public int Value
        {
            get => val;

            set
            {
                var oldValue = val;
                
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
                
                var newValueRect = ClientRectangle;
                var oldValueRect = ClientRectangle;
                
                var percent = (val - min) / (float)(max - min);
                newValueRect.Width = (int)(newValueRect.Width * percent);
                
                percent = (oldValue - min) / (float)(max - min);
                oldValueRect.Width = (int)(oldValueRect.Width * percent);

                var updateRect = new Rectangle();
                
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
                
                Invalidate(updateRect);
            }
        }

        public Color ProgressBarColor1
        {
            get => BarColor1;

            set
            {
                BarColor1 = value;
                Invalidate();
            }
        }

        public Color ProgressBarColor2
        {
            get => BarColor2;

            set
            {
                BarColor2 = value;
                Invalidate();
            }
        }

        public Image ProgressBarImage
        {
            get => BarImage;

            set
            {
                BarImage = value;
                Invalidate();
            }
        }
    }
}
