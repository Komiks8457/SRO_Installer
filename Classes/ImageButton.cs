using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

// ReSharper disable AssignNullToNotNullAttribute
// ReSharper disable ConditionIsAlwaysTrueOrFalse
// ReSharper disable PossibleLossOfFraction
// ReSharper disable NotAccessedField.Local

namespace SRO_Installer_Boobies.Classes
{
    public class ImageButton : PictureBox, IButtonControl
    {

        #region IButtonControl Members

        public DialogResult DialogResult { get; set; }

        public void NotifyDefault(bool value)
        {
            isDefault = value;
        }

        public void PerformClick()
        {
            base.OnClick(EventArgs.Empty);
        }

        #endregion

        #region HoverImage
        private Image m_HoverImage;

        [Category("Appearance")]
        [Description("Image to show when the button is hovered over.")]
        public Image HoverImage
        {
            get => m_HoverImage;
            set { m_HoverImage = value; if (hover) Image = value; }
        }
        #endregion
        #region DownImage
        private Image m_DownImage;

        [Category("Appearance")]
        [Description("Image to show when the button is depressed.")]
        public Image DownImage
        {
            get => m_DownImage;
            set { m_DownImage = value; if (down) Image = value; }
        }
        #endregion
        #region NormalImage
        private Image m_NormalImage;

        [Category("Appearance")]
        [Description("Image to show when the button is not in any other state.")]
        public Image NormalImage
        {
            get => m_NormalImage;
            set { m_NormalImage = value; if (!(hover || down)) Image = value; }
        }
        #endregion

        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private bool hover;
        private bool down;
        private bool isDefault;

        #region Overrides

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Category("Appearance")]
        [Description("The text associated with the control.")]
        public override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Category("Appearance")]
        [Description("The font used to display text in the control.")]
        public override Font Font
        {
            get => base.Font;
            set => base.Font = value;
        }

        #endregion

        #region Description Changes
        [Description("Controls how the ImageButton will handle image placement and control sizing.")]
        public new PictureBoxSizeMode SizeMode { get => base.SizeMode;
            set => base.SizeMode = value;
        }

        [Description("Controls what type of border the ImageButton should have.")]
        public new BorderStyle BorderStyle { get => base.BorderStyle;
            set => base.BorderStyle = value;
        }
        #endregion

        #region Hiding

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Image Image { get => base.Image;
            set => base.Image = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ImageLayout BackgroundImageLayout { get => base.BackgroundImageLayout;
            set => base.BackgroundImageLayout = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Image BackgroundImage { get => base.BackgroundImage;
            set => base.BackgroundImage = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string ImageLocation { get => base.ImageLocation;
            set => base.ImageLocation = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Image ErrorImage { get => base.ErrorImage;
            set => base.ErrorImage = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Image InitialImage { get => base.InitialImage;
            set => base.InitialImage = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool WaitOnLoad { get => base.WaitOnLoad;
            set => base.WaitOnLoad = value;
        }
        #endregion

        #region Events
        protected override void OnMouseMove(MouseEventArgs e)
        {
            hover = true;
            if (down && e.Button == MouseButtons.Left)
            {
                if (m_DownImage != null && Image != m_DownImage)
                    Image = m_DownImage;
            }
            else
                if (m_HoverImage != null)
                    Image = m_HoverImage;
                else
                    Image = m_NormalImage;
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            hover = false;
            Image = m_NormalImage;
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            Focus();
            OnMouseUp(null);
            down = true;
            if (m_DownImage != null && e.Button == MouseButtons.Left)
                Image = m_DownImage;
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            down = false;
            if (hover)
            {
                if (m_HoverImage != null)
                    Image = m_HoverImage;
            }
            else
                Image = m_NormalImage;
            base.OnMouseUp(e);
        }

        private bool holdingSpace;

        public override bool PreProcessMessage(ref Message msg)
        {
            switch (msg.Msg)
            {
                case WM_KEYUP:
                    switch (holdingSpace)
                    {
                        case true:
                            switch ((int)msg.WParam)
                            {
                                case (int)Keys.Space:
                                    OnMouseUp(null);
                                    PerformClick();
                                    break;
                                case (int)Keys.Escape:
                                case (int)Keys.Tab:
                                    holdingSpace = false;
                                    OnMouseUp(null);
                                    break;
                            }

                            break;
                    }

                    return true;
                case WM_KEYDOWN:
                    switch ((int)msg.WParam)
                    {
                        case (int)Keys.Space:
                            holdingSpace = true;
                            OnMouseDown(null);
                            break;
                        case (int)Keys.Enter:
                            PerformClick();
                            break;
                    }

                    return true;
                default:
                    return base.PreProcessMessage(ref msg);
            }
        }

        protected override void OnLostFocus(EventArgs e)
        {
            holdingSpace = false;
            OnMouseUp(null);
            base.OnLostFocus(e);
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            if (string.IsNullOrEmpty(Text) || pe == null || base.Font == null) return;
            var drawBrush = new SolidBrush(base.ForeColor);
            var drawStringSize = pe.Graphics.MeasureString(base.Text, base.Font);
            var drawPoint = base.Image != null ? new PointF(base.Image.Width / 2 - drawStringSize.Width / 2, base.Image.Height / 2 - drawStringSize.Height / 2) : new PointF(Width / 2 - drawStringSize.Width / 2, Height / 2 - drawStringSize.Height / 2);
            pe.Graphics.DrawString(base.Text, base.Font, drawBrush, drawPoint);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            Refresh();
            base.OnTextChanged(e);
        }
        #endregion
    }
}
