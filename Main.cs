using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SevenZipExtractor;
using SRO_Installer_Boobies.Classes;
using SRO_Installer_Boobies.Properties;

namespace SRO_Installer_Boobies
{
    public partial class Main : Form
    {
        private bool IsExtracting;
        private PictureBox SlideShow;
        private string SevenZipDll, MyExecutable;
        private ImageButton InstallBtn, StartBtn, SearchBtn, CancelBtn_1, CancelBtn_2, CancelBtn_3, CompleteBtn;
        private ProgressBarEx InstallProgressBar;
        private Panel Panel_1, Panel_2, Panel_3, Panel_4;
        private Label InstallDirectoryLabel, ExtractingFileLabel;
        private Dictionary<string, ulong> FileList;
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
            FileList = new Dictionary<string, ulong>();
            
            InstallBtn = ImgBtn(345, 390, Resources.install_normal, Resources.install_press, InstallBtn_MouseClick);
            StartBtn = ImgBtn(345, 390, Resources.start_normal, Resources.start_press, StartBtn_MouseClick);
            SearchBtn = ImgBtn(413, 349, Resources.search_normal, Resources.search_press, SearchBtn_MouseClick);
            CancelBtn_1 = ImgBtn(413, 390, Resources.cancel_normal, Resources.cancel_press, CancelBtn_MouseClick);
            CancelBtn_2 = ImgBtn(413, 390, Resources.cancel_normal, Resources.cancel_press, CancelBtn_MouseClick);
            CancelBtn_3 = ImgBtn(413, 390, Resources.cancel_normal, Resources.cancel_press, CancelBtn_MouseClick);
            CompleteBtn = ImgBtn(318, 13, Resources.ok_normal, Resources.ok_press, CompleteBtn_MousClick);
            
            InstallDirectoryLabel = TextLabel(372, 17, 34, 360, @"C:\Program Files (x86)\Silkroad");
            ExtractingFileLabel = TextLabel(300, 17, 99, 402, "Please wait...");
            
            SlideShow = new PictureBox
            {
                Size = new Size(500, 317),
                Location = new Point(0, 0),
                BackColor = Color.Transparent
            };

            InstallProgressBar = new ProgressBarEx
            {
                Size = new Size(421, 18),
                BackColor = Color.Transparent,
                Location = new Point(40, 365)
            };
            
            Panel_1 = new Panel
            {
                Dock = DockStyle.Fill,
                Visible = true,
                BackgroundImage = Resources.bg_1
            };

            Panel_2 = new Panel
            {
                Dock = DockStyle.Fill,
                Visible = false,
                BackgroundImage = Resources.bg_2
            };

            Panel_3 = new Panel
            {
                Dock = DockStyle.Fill,
                Visible = false,
                BackgroundImage = Resources.bg_3
            };

            Panel_4 = new Panel
            {
                Dock = DockStyle.None,
                Visible = false,
                Size = new Size(389, 59),
                Location = new Point(55, 199),
                BackgroundImage = Resources.install_complete
            };

            Controls.Add(Panel_1);
            Controls.Add(Panel_2);
            Controls.Add(Panel_3);

            Panel_1.Controls.AddRange(new Control[] { CancelBtn_1, InstallBtn });
            Panel_2.Controls.AddRange(new Control[] { CancelBtn_2, StartBtn, SearchBtn, InstallDirectoryLabel });
            Panel_3.Controls.AddRange(new Control[] { CancelBtn_3, InstallProgressBar, ExtractingFileLabel, Panel_4, SlideShow });
            Panel_4.Controls.Add(CompleteBtn);

            Panel_1.MouseDown += DragMove;
            Panel_2.MouseDown += DragMove;
            Panel_3.MouseDown += DragMove;
            Panel_4.MouseDown += DragMove;
            SlideShow.MouseDown += DragMove;

            InstallDirectoryLabel.MouseDown += DragMove;
            ExtractingFileLabel.MouseDown += DragMove;
            InstallProgressBar.MouseDown += DragMove;

            Closing += Main_Closing;
        }

        private void Main_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DeleteSevenZipDll();
            if (!IsExtracting) return;
            try
            {
                Directory.Delete(InstallDirectoryLabel.Text);
            }
            catch { /* dont force it */ }
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
            if (IsExtracting && MessageBox.Show(@"Would you stop install Silkroad Online?", Text, MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                Close();
            }
            else return;
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

        private void CompleteBtn_MousClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = InstallDirectoryLabel.Text,
                    FileName = MyExecutable
                };
                process.Start();
            }
            Close();
        }

        private async void ExtractZip()
        {
            await Task.Run(() =>
            {
                IsExtracting = true;
                Directory.CreateDirectory(InstallDirectoryLabel.Text);
                SevenZipDll = Path.Combine(InstallDirectoryLabel.Text, "7z.dll");
                File.WriteAllBytes(SevenZipDll, IntPtr.Size == 4 ? Resources.x86_7z : Resources.x64_7z);
                using (var archiveFile = new ArchiveFile(new MemoryStream(Resources.Silkroad), SevenZipFormat.Zip, SevenZipDll))
                {
                    UpdateSlideShow();

                    foreach (var entry in archiveFile.Entries)
                    {
                        FileList.Add(entry.FileName, entry.Size);
                    }

                    foreach (var entry in archiveFile.Entries)
                    {
                        ControlTxtUpdate(ExtractingFileLabel, entry.FileName);

                        entry.Extract(Path.Combine(InstallDirectoryLabel.Text, entry.FileName), true, ProgressEventHandler);

                        if (entry.IsFolder) continue;

                        if (FileList[entry.FileName] == GetFileSize(Path.Combine(InstallDirectoryLabel.Text, entry.FileName)).Result)
                            continue;

                        MessageBox.Show($@"{entry.FileName} is corrupt, please re-run the installer.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

                        Close();
                    }
                }
                ControlUpdateVisibility(SlideShow, false);
                ControlEnableDisable(CancelBtn_3, false);
                ControlUpdateVisibility(Panel_4, true);
                CreateShortcut("Silkroad.exe");
                DeleteSevenZipDll();
                IsExtracting = false;
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

        private async void UpdateSlideShow()
        {
            await Task.Run(() =>
            {
                var random = new Random();

                var images = new Dictionary<int, Image>
                {
                    { 0, Resources.img_01 },
                    { 1, Resources.img_02 },
                    { 2, Resources.img_03 },
                    { 3, Resources.img_04 },
                    { 4, Resources.img_05 },
                    { 5, Resources.img_06 },
                    { 6, Resources.img_07 },
                    { 7, Resources.img_08 },
                    { 8, Resources.img_09 },
                    { 9, Resources.img_10 }
                };

                while (IsExtracting)
                {
                    Invoke((MethodInvoker)delegate
                    {
                        SlideShow.Image = images[random.Next(10)];
                        SlideShow.Refresh();
                    });
                    Thread.Sleep(5000);
                }
            });
        }

        private static Label TextLabel(int w, int h, int x, int y, string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = false,
                Size = new Size(w, h),
                Location = new Point(x, y),
                Font = new Font(DefaultFont.FontFamily, 9.75f, FontStyle.Bold),
                BackColor = Color.Transparent,
                ForeColor = Color.Aqua
            };
        }

        private static ImageButton ImgBtn(int x, int y, Image normal, Image press, MouseEventHandler sender)
        {
            var btn = new ImageButton
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

        private void DeleteSevenZipDll()
        {
            try
            {
                File.Delete(SevenZipDll);
            }
            catch { /* ignore */ }
        }

        [SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
        private void CreateShortcut(string executable)
        {
            var fullPath = Path.Combine(InstallDirectoryLabel.Text, executable);

            var link = (IShellLink) new ShellLink();
            
            link.SetPath(fullPath);
            link.SetDescription("SROR-Development");
            link.SetWorkingDirectory(Path.GetDirectoryName(fullPath));
            
            var file = (IPersistFile) link;
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            file.Save(Path.Combine(desktopPath, Path.GetFileNameWithoutExtension(fullPath) + ".lnk"), false);

            MyExecutable = fullPath;
        }

        private static async Task<ulong> GetFileSize(string filepath)
        {
            return await Task.Run(() =>
            {
                using(var fileStream = File.OpenRead(filepath))
                {
                    var bytes = new byte[1024];
                    ulong totalBytesRead = 0;
                    int bytesRead;

                    do
                    {
                        bytesRead = fileStream.Read(bytes, 0, bytes.Length);
                        totalBytesRead += (ulong)bytesRead;
                    } while(bytesRead != 0);

                    return totalBytesRead;
                }
            });
        }
    }
}
