using System;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Runtime.InteropServices;

// Uygulama yapay zeka ile desteklenmiştir, kellebyte

namespace InternetimiRahatBirak
{
    public partial class MainForm : Form
    {
        private Button btnNormalDns;
        private Button btnGoodbyeDpi;
        private Button btnSuperonline;
        private Button btnServisKaldir;
        private Button btnTempGoodbyeDpi;
        private Button btnTempSuperonline;
        private ComboBox cmbDnsSelect;
        private ComboBox cmbNetworkAdapters;
        private Label lblNetworkAdapter;
        private Label lblDnsSelect;
        private static bool alreadyRunning = false;

        [STAThread]
        static void Main()
        {
            if (alreadyRunning) return;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            alreadyRunning = true;

            var mainForm = new MainForm();
            if (!mainForm.IsRunAsAdministrator())
            {
                mainForm.RestartAsAdmin();
                return;
            }

            Application.Run(mainForm);
        }

        public MainForm()
        {
            InitializeUI();
            LoadNetworkAdapters();
            CheckAndSetDNS();
        }

        private bool IsRunAsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        private void RestartAsAdmin()
        {
            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                Verb = "runas",
                FileName = Application.ExecutablePath,
                UseShellExecute = true
            };

            try
            {
                Process.Start(processInfo);
            }
            catch
            {
                MessageBox.Show(
                    "Program DNS ayarlarını değiştirmek için yönetici izinlerine ihtiyaç duyuyor.",
                    "Yönetici İzni Gerekli",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void InitializeUI()
        {
            this.Text = "İnternetimi Rahat Bırak";
            this.Size = new Size(450, 500);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);

            // Başlık
            Label lblTitle = new Label
            {
                Text = "DNS Ayarları",
                Font = new Font("Segoe UI Semibold", 16, FontStyle.Regular),
                ForeColor = Color.White,
                Location = new Point(20, 15),
                AutoSize = true
            };
            this.Controls.Add(lblTitle);

            // Ağ Adaptörü Label
            lblNetworkAdapter = new Label
            {
                Text = "Ağ Adaptörü",
                AutoSize = true,
                Location = new Point(20, 55),
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.White
            };
            this.Controls.Add(lblNetworkAdapter);

            // Ağ Adaptörü ComboBox
            cmbNetworkAdapters = new ComboBox
            {
                Location = new Point(20, 75),
                Size = new Size(405, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9, FontStyle.Regular)
            };
            this.Controls.Add(cmbNetworkAdapters);

            // DNS Seçim Label
            lblDnsSelect = new Label
            {
                Text = "DNS Sunucusu",
                AutoSize = true,
                Location = new Point(20, 115),
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.White
            };
            this.Controls.Add(lblDnsSelect);

            // DNS Seçim ComboBox
            cmbDnsSelect = new ComboBox
            {
                Location = new Point(20, 135),
                Size = new Size(405, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9, FontStyle.Regular)
            };
            cmbDnsSelect.Items.AddRange(new string[] {
                "Cloudflare (1.1.1.1, 1.0.0.1)",
                "Google DNS (8.8.8.8, 8.8.4.4)",
                "Quad9 (Önerilir) (9.9.9.9, 149.112.112.112)"
            });
            cmbDnsSelect.SelectedIndex = 0;
            this.Controls.Add(cmbDnsSelect);

            // DNS Değiştir Butonu
            btnNormalDns = new Button
            {
                Text = "DNS Ayarlarını Değiştir",
                Location = new Point(20, 180),
                Size = new Size(405, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(13, 110, 253),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            btnNormalDns.Click += BtnNormalDns_Click;
            this.Controls.Add(btnNormalDns);

            // GoodbyeDPI Butonu
            btnGoodbyeDpi = new Button
            {
                Text = "GoodbyeDPI Kullan",
                Location = new Point(20, 230),
                Size = new Size(405, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            btnGoodbyeDpi.Click += BtnGoodbyeDpi_Click;
            this.Controls.Add(btnGoodbyeDpi);

            // Superonline Butonu
            btnSuperonline = new Button
            {
                Text = "Superonline Kullan",
                Location = new Point(20, 280),
                Size = new Size(405, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            btnSuperonline.Click += BtnSuperonline_Click;
            this.Controls.Add(btnSuperonline);

            // Servis Kaldır Butonu
            btnServisKaldir = new Button
            {
                Text = "Servis Kaldır",
                Location = new Point(20, 330),
                Size = new Size(405, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            btnServisKaldir.Click += BtnServisKaldir_Click;
            this.Controls.Add(btnServisKaldir);

            // Geçici GoodbyeDPI Butonu
            btnTempGoodbyeDpi = new Button
            {
                Text = "Geçici GoodbyeDPI",
                Location = new Point(20, 380),
                Size = new Size(195, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            btnTempGoodbyeDpi.Click += BtnTempGoodbyeDpi_Click;
            this.Controls.Add(btnTempGoodbyeDpi);

            // Geçici Superonline Butonu
            btnTempSuperonline = new Button
            {
                Text = "Geçici Superonline",
                Location = new Point(225, 380),
                Size = new Size(195, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            btnTempSuperonline.Click += BtnTempSuperonline_Click;
            this.Controls.Add(btnTempSuperonline);
        }

        private void LoadNetworkAdapters()
        {
            cmbNetworkAdapters.Items.Clear();

            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces()
                .Where(adapter => adapter.OperationalStatus == OperationalStatus.Up &&
                                (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                                 adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211))
                .ToArray();

            foreach (NetworkInterface adapter in adapters)
            {
                cmbNetworkAdapters.Items.Add(adapter.Name);
            }

            if (cmbNetworkAdapters.Items.Count > 0)
            {
                cmbNetworkAdapters.SelectedIndex = 0;
            }
            else
            {
                MessageBox.Show("Aktif ağ adaptörü bulunamadı!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnGoodbyeDpi_Click(object sender, EventArgs e)
        {
            RunBatFile("servisyukle.cmd");
        }

        private void BtnSuperonline_Click(object sender, EventArgs e)
        {
            RunBatFile("servisyuklesuperonline.cmd");
            SetDNS("9.9.9.9", "149.112.112.112");
            RestartComputer();
        }

        private void BtnServisKaldir_Click(object sender, EventArgs e)
        {
            RunBatFile("serviskaldir.cmd");
        }

        private void BtnTempGoodbyeDpi_Click(object sender, EventArgs e)
        {
            RunBatFile("gecici.cmd");
        }

        private void BtnTempSuperonline_Click(object sender, EventArgs e)
        {
            RunBatFile("gecicisuperonline.cmd");
            SetDNS("9.9.9.9", "149.112.112.112");
            RestartComputer();
        }

        private void RunBatFile(string fileName)
        {
            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {fileName}",
                    UseShellExecute = true,
                    Verb = "runas",
                    CreateNoWindow = true
                };

                Process process = Process.Start(processInfo);
                process.WaitForExit();

                // Otomatik olarak Y tuşuna basma
                Thread.Sleep(1000); // BAT dosyasının çalışması için bekleyin
                SendKeys.SendWait("Y");

                MessageBox.Show($"{fileName} başarıyla çalıştırıldı!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnNormalDns_Click(object sender, EventArgs e)
        {
            if (cmbNetworkAdapters.SelectedItem == null)
            {
                MessageBox.Show("Lütfen bir ağ adaptörü seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string[] dns = GetSelectedDNS();
                SetDNS(dns[0], dns[1]);
                MessageBox.Show("DNS ayarları başarıyla değiştirildi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string[] GetSelectedDNS()
        {
            switch (cmbDnsSelect.SelectedIndex)
            {
                case 0: // Cloudflare
                    return new string[] { "1.1.1.1", "1.0.0.1" };
                case 1: // Google DNS
                    return new string[] { "8.8.8.8", "8.8.4.4" };
                case 2: // Quad9
                    return new string[] { "9.9.9.9", "149.112.112.112" };
                default:
                    throw new Exception("Lütfen bir DNS seçin!");
            }
        }

        private void SetDNS(string primaryDNS, string secondaryDNS)
        {
            string adapter = cmbNetworkAdapters.SelectedItem.ToString();

            using (var process = new Process())
            {
                process.StartInfo.FileName = "netsh";
                process.StartInfo.Arguments = $"interface ipv4 set dns name=\"{adapter}\" static {primaryDNS} primary";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.WaitForExit();

                process.StartInfo.Arguments = $"interface ipv4 add dns name=\"{adapter}\" {secondaryDNS} index=2";
                process.Start();
                process.WaitForExit();
            }
        }

        private void CheckAndSetDNS()
        {
            string adapter = cmbNetworkAdapters.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(adapter)) return;

            using (var process = new Process())
            {
                process.StartInfo.FileName = "netsh";
                process.StartInfo.Arguments = $"interface ipv4 show dns name=\"{adapter}\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                if (output.Contains("9.9.9.9") && output.Contains("149.112.112.112"))
                {
                    cmbDnsSelect.SelectedIndex = 2; // Quad9
                }
            }
        }

        private void RestartComputer()
        {
            Process.Start("shutdown", "/r /t 0");
        }
    }
}
