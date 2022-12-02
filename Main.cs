using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using SevenZipExtractor;
using SRO_Installer_Boobies.Classes;
using SRO_Installer_Boobies.Properties;

namespace SRO_Installer_Boobies
{
    public partial class Main : Form
    {
        private ImageButton InstallBtn, StartBtn, SearchBtn, CancelBtn_1, CancelBtn_2, CancelBtn_3, CompleteBtn;
        private ProgressBarEx InstallProgressBar;
        private Panel Panel_1, Panel_2, Panel_3, Panel_0;
        private Label InstallDirectoryLabel, ExtractingFileLabel;
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            Closing += Main_Closing;
            InstallBtn = ImgBtn(345, 390, Resources.install_normal, Resources.install_press, InstallBtn_MouseClick);
            StartBtn = ImgBtn(345, 390, Resources.start_normal, Resources.start_press, StartBtn_MouseClick);
            SearchBtn = ImgBtn(413, 349, Resources.search_normal, Resources.search_press, SearchBtn_MouseClick);
            CancelBtn_1 = ImgBtn(413, 390, Resources.cancel_normal, Resources.cancel_press, CancelBtn_MouseClick);
            CancelBtn_2 = ImgBtn(413, 390, Resources.cancel_normal, Resources.cancel_press, CancelBtn_MouseClick);
            CancelBtn_3 = ImgBtn(413, 390, Resources.cancel_normal, Resources.cancel_press, CancelBtn_MouseClick);
            CompleteBtn = ImgBtn(318, 13, Resources.ok_normal, Resources.ok_press, CancelBtn_MouseClick);

            InstallDirectoryLabel = TextLabel(372, 17, 34, 360, @"C:\Program Files (x86)\Silkroad");
            ExtractingFileLabel = TextLabel(200, 17, 99, 402, "Data.pk2");
            
            InstallProgressBar = new ProgressBarEx()
            {
                Size = new Size(423, 20),
                BackColor = Color.Transparent,
                Location = new Point(39, 364),
                Visible = true
            };

            Panel_0 = new Panel()
            {
                Dock = DockStyle.None,
                Visible = false,
                Size = new Size(389, 59),
                Location = new Point(55, 199),
                BackgroundImage = Resources.install_complete
            };

            Panel_1 = new Panel()
            {
                Dock = DockStyle.Fill,
                Visible = true,
                BackgroundImage = Resources.bg_1
            };

            Panel_2 = new Panel()
            {
                Dock = DockStyle.Fill,
                Visible = false,
                BackgroundImage = Resources.bg_2
            };

            Panel_3 = new Panel()
            {
                Dock = DockStyle.Fill,
                Visible = false,
                BackgroundImage = Resources.bg_3
            };
            
            Controls.Add(Panel_1);
            Controls.Add(Panel_2);
            Controls.Add(Panel_3);

            Panel_0.Controls.Add(CompleteBtn);
            Panel_1.Controls.AddRange(new Control[] { CancelBtn_1, InstallBtn });
            Panel_2.Controls.AddRange(new Control[] { CancelBtn_2, StartBtn, SearchBtn, InstallDirectoryLabel });
            Panel_3.Controls.AddRange(new Control[] { CancelBtn_3, InstallProgressBar, ExtractingFileLabel, Panel_0 });

            Panel_1.MouseDown += DragMove;
            Panel_2.MouseDown += DragMove;
            Panel_3.MouseDown += DragMove;
        }

        private void Main_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                File.Delete(Path.Combine(InstallDirectoryLabel.Text, "7z.dll"));
            }
            catch { /* ignore */ }
        }

        private void DragMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            ReleaseCapture();
            SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }

        private void InstallBtn_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            ControlUpdateVisibility(Panel_1, false);
            ControlUpdateVisibility(Panel_2, true);
        }

        private void SearchBtn_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            var folderSelect = new FolderBrowserDialog();
            if (folderSelect.ShowDialog() == DialogResult.OK)
            {
                ControlTxtUpdate(InstallDirectoryLabel , folderSelect.SelectedPath + @"\Silkroad");
            }
        }

        private void CancelBtn_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            Close();
        }

        private void StartBtn_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            Panel_3.Controls.Add(InstallDirectoryLabel);
            InstallDirectoryLabel.Location = new Point(99, 332);
            ControlUpdateVisibility(Panel_2, false);
            ControlUpdateVisibility(Panel_3, true);
            
            ExtractZip();
        }

        private async void ExtractZip()
        {
            await Task.Run(() =>
            {
                var Silkroad = Resources.Silkroad; //<---Your silkroad.zip in resource
                var SevenZipDll = Path.Combine(InstallDirectoryLabel.Text, "7z.dll");
                File.WriteAllBytes(SevenZipDll, IntPtr.Size == 4 ? Resources.x86_7z : Resources.x64_7z);
                using (var archiveFile = new ArchiveFile(new MemoryStream(Silkroad), SevenZipFormat.Zip, SevenZipDll))
                {
                    foreach (var entry in archiveFile.Entries)
                    {
                        ControlTxtUpdate(ExtractingFileLabel, entry.FileName);
                        entry.Extract(Path.Combine(InstallDirectoryLabel.Text, entry.FileName), true, ProgressEventHandler);
                    }
                }
                ControlUpdateVisibility(Panel_0, true);
                ControlEnableDisable(CancelBtn_3, false);
            });
        }

        private void ProgressEventHandler(object sender, EntryExtractionProgressEventArgs e)
        {
            UpdateProgressBar((int)(100 * e.Completed / e.Total));
        }

        private async void ControlEnableDisable(Control control, bool enable)
        {
            await Task.Run(() =>
            {
                Invoke((MethodInvoker)delegate
                {
                    control.Enabled = enable;
                });
            });
        }

        private async void ControlUpdateVisibility(Control control, bool visible)
        {
            await Task.Run(() =>
            {
                Invoke((MethodInvoker)delegate
                {
                    control.Visible = visible;
                });
            });
        }

        private async void ControlTxtUpdate(Control control, string fileName)
        {
            await Task.Run(() =>
            {
                Invoke((MethodInvoker)delegate
                {
                    control.Text = fileName;
                });
            });
        }

        private async void UpdateProgressBar(int value)
        {
            await Task.Run(() =>
            {
                Invoke((MethodInvoker)delegate
                {
                    InstallProgressBar.Value = value;
                });
            });
        }

        private static Label TextLabel(int w, int h, int x, int y, string text)
        {
            return new Label()
            {
                AutoSize = false,
                Size = new Size(w, h),
                Location = new Point(x, y),
                Text = text,
                Font = new Font(DefaultFont.FontFamily, 9.75f, FontStyle.Bold),
                BackColor = Color.Transparent,
                ForeColor = Color.Aqua
            };
        }

        private static ImageButton ImgBtn(int x, int y, Image normal, Image press, MouseEventHandler sender)
        {
            var btn = new ImageButton()
            {
                SizeMode = PictureBoxSizeMode.AutoSize,
                NormalImage = normal,
                DownImage = press,
                Location = new Point(x, y),
                TabStop = false
            };

            btn.MouseClick += sender;
            return btn;
        }
    }
}
