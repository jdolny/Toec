using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Toec_Common.Dto;
using Toec_Services;
using Toec_Services.Entity;

namespace Toec_ImagePrep
{
    public partial class GUI : Form
    {
        private DtoImagePrepOptions _imagePrepOptions;

        public GUI()
        {
            this.ShowInTaskbar = true;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Padding = new Padding(3);
            this.Width = 400;
            this.BringToFront();
            //this.TopMost = true;
            this.Focus();
            this.Activate();

         
            InitializeComponent();

            txtSetupComplete.Text = "mkdir \"%ProgramFiles%\\Toec\"" + Environment.NewLine;
            txtSetupComplete.Text += "net stop toec /y >> \"%ProgramFiles%\\Toec\\setupcomplete.log\"" + Environment.NewLine;
            txtSetupComplete.Text += "powercfg.exe /h off >> \"%ProgramFiles%\\Toec\\setupcomplete.log\"" + Environment.NewLine;
            txtSetupComplete.Text += "copy NUL \"%ProgramFiles%\\Toec\\setupcompletecmd_complete\" >> \"%ProgramFiles%\\Toec\\setupcomplete.log\"" + Environment.NewLine;
            txtSetupComplete.Text += "del /Q /F c:\\windows\\system32\\sysprep\\unattend.xml >> \"%ProgramFiles%\\Toec\\setupcomplete.log\" " + Environment.NewLine;
            txtSetupComplete.Text += "del /Q /F c:\\windows\\panther\\unattend.xml >> \"%ProgramFiles%\\Toec\\setupcomplete.log\"" + Environment.NewLine;
            //txtSetupComplete.Text += @"pnputil.exe /add-driver c:\drivers\*.inf /subdirs /install" + Environment.NewLine;
            txtSetupComplete.Text += Environment.NewLine;
            txtSetupComplete.Text += "REM ####### Driver install ###########" + Environment.NewLine;
            txtSetupComplete.Text += @"PowerShell ^" + Environment.NewLine;
            txtSetupComplete.Text += "$Drivers = Get-ChildItem \"C:\\drivers\" -Recurse -Filter \"*.inf\" ;^" + Environment.NewLine;
            txtSetupComplete.Text += "ForEach($Driver in $Drivers) {;^" + Environment.NewLine;
            txtSetupComplete.Text += "Write-Host Installing $Driver.FullName;^" + Environment.NewLine;
            txtSetupComplete.Text += "$p = (Start-Process -FilePath pnputil.exe -ArgumentList \\\"/add-driver $Driver.FullName /install\\\" -Passthru);^" + Environment.NewLine;
            txtSetupComplete.Text += "Wait-Process -Id $p.Id -Timeout 120;^" + Environment.NewLine;
            txtSetupComplete.Text += "taskkill /pid $p.Id /f /t;^" + Environment.NewLine;
            txtSetupComplete.Text += "} >> \"%ProgramFiles%\\Toec\\setupcomplete.log\";^" + Environment.NewLine;
            txtSetupComplete.Text += "%End PowerShell%" + Environment.NewLine;
            txtSetupComplete.Text += "REM ####### End Driver install ###########" + Environment.NewLine;
            txtSetupComplete.Text += Environment.NewLine;
            txtSetupComplete.Text += "net start toec /y" + Environment.NewLine + Environment.NewLine;


        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            txtOutput.Text = "";
            var imagePrepOptions = new DtoImagePrepOptions();
            if (chkDisableHibernate.Checked)
                imagePrepOptions.RunHibernate = true;

            if (chkDriversReg.Checked)
                imagePrepOptions.AddDriverRegistry = true;

            if (chkEnableBackground.Checked)
                imagePrepOptions.EnableFinalizingBackground = true;

            if (chkCreateSetupComplete.Checked)
            {
                imagePrepOptions.CreateSetupComplete = true;
                imagePrepOptions.SetupCompleteContents = txtSetupComplete.Text;
            }

            if (chkRunSysprep.Checked)
            {
                imagePrepOptions.RunSysprep = true;
                imagePrepOptions.SysprepAnswerPath = txtSysprepAnswerFile.Text;
            }

            if (chkResetToec.Checked)
                imagePrepOptions.ResetToec = true;

            if (chkRemoveRemoteAccess.Checked)
                imagePrepOptions.RemoveRemoteAccess = true;

            Run(imagePrepOptions);

        }


        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            txtSysprepAnswerFile.Text = openFileDialog1.FileName;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Console.Write("");
            this.Close();
        }

        private void AppendLogText(string text)
        {
            txtOutput.Text += text + Environment.NewLine;
        }

        public bool Run(DtoImagePrepOptions imagePrepOptions)
        {
            if (imagePrepOptions == null)
            {
                AppendLogText("Image Prep Cancelled.");
                return false;
            }

            if (imagePrepOptions.RunSysprep && string.IsNullOrEmpty(imagePrepOptions.SysprepAnswerPath))
            {
                AppendLogText("A Sysprep Answer File Was Not Defined.  Image Prep Cancelled");
                return false;
            }

            AppendLogText("Preparing Computer For Image: ");
            AppendLogText("Checking Toec Service");
            var servResult = new ServiceSystemService().StopToec();
            if (!servResult)
            {
                AppendLogText("Toec Service Must Be Stopped Before Preparing Image.");
                return false;
            }

            _imagePrepOptions = imagePrepOptions;

            DisableHibernation();
            AddDriverRegistry();
            EnableWinLogonBackground();
            CreateSetupComplete();
            RemoveRemoteAccess();
            ResetToec();
            RunSysprep();

            File.Create($"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)}\\Toec\\image_prepped");
            AppendLogText("Prepare Image Finished");
            return true;
        }

        private void RemoveRemoteAccess()
        {
            if (!_imagePrepOptions.RemoveRemoteAccess) return;
            AppendLogText("Removing Remove Access Client");
            var remotelyPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\Remotely\\Remotely_Installer.exe";
            if(File.Exists(remotelyPath))
            {
                System.Diagnostics.Process.Start(remotelyPath, "-uninstall -quiet");
            }
            else
            {
                AppendLogText("Remotely Not Found.");
            }
            AppendLogText("Finished Removing Access Client.");
        }
        private void CreateSetupComplete()
        {
            if (!_imagePrepOptions.CreateSetupComplete) return;
            AppendLogText("Creating Setup Complete Script");
            var winPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            Directory.CreateDirectory(Path.Combine(winPath, "Setup", "Scripts"));
            var scriptPath = Path.Combine(winPath, "Setup", "Scripts", "setupcomplete.cmd");
            File.WriteAllText(scriptPath, _imagePrepOptions.SetupCompleteContents);
            AppendLogText("Finished Creating Setup Complete Script");
        }

        private void AddDriverRegistry()
        {
            if (!_imagePrepOptions.AddDriverRegistry) return;
            AppendLogText("Updating Registry DevicePath Locations.");
            Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\", "DevicePath", "%SystemRoot%\\inf;c:\\drivers");
            AppendLogText("Finished Updating Registry DevicePath Locations.");
        }

        private void RunSysprep()
        {
            if (!_imagePrepOptions.RunSysprep) return;
            AppendLogText("Running Sysprep");
            if (!string.IsNullOrEmpty(_imagePrepOptions.SysprepAnswerPath))
            {
                var winPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                var sysPrepPath = Path.Combine(winPath, "System32", "Sysprep");
                File.Copy(_imagePrepOptions.SysprepAnswerPath, Path.Combine(sysPrepPath, "unattend.xml"), true);
                System.Diagnostics.Process.Start(Path.Combine(sysPrepPath, "sysprep.exe"), $"/oobe /generalize /shutdown /unattend:{Path.Combine(sysPrepPath, "unattend.xml")}");
            }
            AppendLogText("Finished Running Sysprep");

        }

        private void EnableWinLogonBackground()
        {
            if (!_imagePrepOptions.EnableFinalizingBackground) return;
            AppendLogText("Setting finalizing background image.");
            Microsoft.Win32.RegistryKey key;
            key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\PersonalizationCSP");
            key.SetValue("LockScreenImagePath", $"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)}\\Toec\\finalizing_lock_image.png", Microsoft.Win32.RegistryValueKind.String);
            key.SetValue("LockScreenImageUrl", $"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)}\\Toec\\finalizing_lock_image.png", Microsoft.Win32.RegistryValueKind.String);
            key.SetValue("DesktopImagePath", $"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)}\\Toec\\finalizing_desktop_image.png", Microsoft.Win32.RegistryValueKind.String);
            key.SetValue("DesktopImageUrl", $"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)}\\Toec\\finalizing_desktop_image.png", Microsoft.Win32.RegistryValueKind.String);
            key.Close();
            AppendLogText("Finished Setting finalizing background image.");
        }

        private void DisableHibernation()
        {
            if (!_imagePrepOptions.RunHibernate) return;
            AppendLogText("Disabling Hibernation");
            System.Diagnostics.Process.Start("powercfg.exe", "/h off ");
            AppendLogText("Finished Disabling Hibernation");
        }

        private void ResetToec()
        {
            if (!_imagePrepOptions.ResetToec) return;
            AppendLogText("Resetting Toec");

            ServiceCertificate.DeleteAllDeviceCertificates();
            ServiceCertificate.DeleteIntermediate();

            var serviceSetting = new ServiceSetting();

            var installationId = serviceSetting.GetSetting("installation_id");
            installationId.Value = null;
            serviceSetting.UpdateSettingValue(installationId);


            var encryptionKey = serviceSetting.GetSetting("encryption_key");
            encryptionKey.Value = null;
            serviceSetting.UpdateSettingValue(encryptionKey);

            var entropy = serviceSetting.GetSetting("entropy");
            entropy.Value = null;
            serviceSetting.UpdateSettingValue(entropy);

            var computerIdentifier = serviceSetting.GetSetting("computer_identifier");
            computerIdentifier.Value = null;
            serviceSetting.UpdateSettingValue(computerIdentifier);

            var deviceThumbprint = serviceSetting.GetSetting("device_thumbprint");
            deviceThumbprint.Value = null;
            serviceSetting.UpdateSettingValue(deviceThumbprint);

            var intermediateThumbprint = serviceSetting.GetSetting("intermediate_thumbprint");
            intermediateThumbprint.Value = null;
            serviceSetting.UpdateSettingValue(intermediateThumbprint);

            new PolicyHistoryServices().DeleteAll();
            new ServiceUserTracker().DeleteAll();
            new ServiceAppMonitor().DeleteAll();

            var provisionStatus = serviceSetting.GetSetting("provision_status");
            provisionStatus.Value = "0";
            serviceSetting.UpdateSettingValue(provisionStatus);


            var updatedStatus = serviceSetting.GetSetting("provision_status");
            var updatedId = installationId = serviceSetting.GetSetting("installation_id");

            if (!updatedStatus.Value.Equals("0") && !string.IsNullOrEmpty(updatedId.Value))
            {
                AppendLogText("Prepare Image Failed.  Could Not Reset ID's");
            }

            AppendLogText("Finished Resetting Toec");
        }

    }

   
}
