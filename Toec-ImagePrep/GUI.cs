using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Toec_Common.Dto;
using Toec_Services;
using Toec_Services.ApiCall;
using Toec_Services.Entity;

namespace Toec_ImagePrep
{
    public partial class GUI : Form
    {
        private DtoImagePrepOptions _imagePrepOptions;
        private bool _serverConnectionSuccessful;
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
            txtSetupComplete.Text += "$args = \\\"/add-driver $($Driver.FullName) /install \\\";^" + Environment.NewLine;
            txtSetupComplete.Text += "$p = (Start-Process -FilePath pnputil.exe -ArgumentList $args -NoNewWindow -Passthru);^" + Environment.NewLine;
            txtSetupComplete.Text += "Wait-Process -Id $p.Id -Timeout 10 >> \"%ProgramFiles%\\Toec\\setupcomplete.log\";^" + Environment.NewLine;
            txtSetupComplete.Text += "};^" + Environment.NewLine;
            txtSetupComplete.Text += "%End PowerShell%" + Environment.NewLine;
            txtSetupComplete.Text += Environment.NewLine;
            txtSetupComplete.Text += "net start toec /y" + Environment.NewLine;



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

            if (imagePrepOptions.RunSysprep && string.IsNullOrEmpty(imagePrepOptions.SysprepAnswerPath) && string.IsNullOrEmpty(txtSysprep.Text))
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
            InstallDrivers();
            RemoveRemoteAccess();
            ResetToec();
            RunSysprep();

            File.Create($"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)}\\Toec\\image_prepped");
            AppendLogText("Prepare Image Finished");
            return true;
        }

        private void InstallDrivers()
        {
            Directory.CreateDirectory("c:\\drivers\\imageprep");
            foreach (var driver in checkedListBoxDrivers.CheckedItems)
            {
                var filesToDownload = new APICall().ImagePrepApi.GetFileCopyModule(Convert.ToInt32(driver.ToString().Split(':').First()));
               
                foreach(var file in filesToDownload)
                {
                    new APICall().PolicyApi.GetFileForImagePrep(file, "c:\\drivers\\imageprep\\" + file.FileName);

                    var extension = Path.GetExtension(file.FileName);
                    var name = Path.GetFileNameWithoutExtension(file.FileName);
                    if (!string.IsNullOrEmpty(extension) && extension.ToLower().Equals(".zip"))
                    {
                        try
                        {
                            var path = Path.Combine("c:\\drivers\\imageprep\\", file.FileName);
                            using (FileStream zipToOpen = new FileStream(path, FileMode.Open))
                            {
                                using (ZipArchive archive = new ZipArchive(zipToOpen))
                                {
                                    Directory.CreateDirectory($"c:\\drivers\\imageprep\\{name}");
                                    ZipArchiveExtensions.ExtractToDirectory(archive, $"c:\\drivers\\imageprep\\{name}", true);
                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }


            System.Diagnostics.Process.Start("pnputil.exe" , $"/add-driver c:\\drivers\\imageprep\\*.inf /install /subdirs");
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
            else
            {
                var winPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                var sysPrepPath = Path.Combine(winPath, "System32", "Sysprep");
                var finalPath = Path.Combine(sysPrepPath, "unattend.xml");
                File.WriteAllText(finalPath, txtSysprep.Text);
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

        private void ToemsConnect_Click(object sender, EventArgs e)
        {
            var serviceSetting = new ServiceSetting();
            var comServers = "";
            comServers = serviceSetting.GetSetting("active_com_servers").Value;
            if(string.IsNullOrEmpty(comServers))
                comServers = serviceSetting.GetSetting("initial_com_servers").Value;
            if(string.IsNullOrEmpty(comServers))
            {
                txtConnectOutput.Text = "Could Not Find A Valid Com Server.  Ensure This Computer Has Finished The Provision Process.";
                return;
            }


            var listComServers = comServers.Split(',').ToList();

            
            foreach (var server in listComServers)
            {

                DtoGobalSettings.ComServer = server;
                if (!DtoGobalSettings.ComServer.EndsWith("/"))
                    DtoGobalSettings.ComServer += "/";

                DtoGobalSettings.ClientIdentity = new DtoClientIdentity();
                DtoGobalSettings.ClientIdentity.Name = Dns.GetHostName();
                DtoGobalSettings.ClientIdentity.Guid = new ServiceSetting().GetSetting("computer_identifier").Value;

                var success = new APICall().ProvisionApi.ComConnectionTest("imageprep");
                if(success)
                {
                    _serverConnectionSuccessful = true;
                    txtConnectOutput.Text = "Successfully Connected To Com Server.  Additional Image Prep Options Now Available.";
                    break;
                }
            }

            if(!_serverConnectionSuccessful)
            {
                txtConnectOutput.Text = "Could Not Connect To The Com Server.  Ensure This Computer Has Finished The Provision Process.  Additional Image Prep Options Will Not Be Available";
            }

            foreach (var driver in new APICall().ImagePrepApi.GetDriverList().Split(',').ToList())
                checkedListBoxDrivers.Items.Add(driver);

            var setupCompleteList = new APICall().ImagePrepApi.GetSetupCompleteList();
            setupCompleteList.Add(new DtoSetupCompleteFile { Id = -1, Name = "__Select A SetupComplete File" });
            var orderedSetup = setupCompleteList.OrderBy(x => x.Name).ToList();
            ddlSetupComplete.DataSource = orderedSetup;
            ddlSetupComplete.DisplayMember = "Name";
            ddlSetupComplete.ValueMember = "Id";

            var list = new APICall().ImagePrepApi.GetSysprepList();
            list.Add(new DtoSysprepAnswerfile { Id = -1, Name = "__Select An Answer File" });
            var ordered = list.OrderBy(x => x.Name).ToList();
            ddlSysprep.DataSource = ordered;
            ddlSysprep.DisplayMember = "Name";
            ddlSysprep.ValueMember = "Id";
            

        }

        private void ddlSetupComplete_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var setupCompleteId = (int)ddlSetupComplete.SelectedValue;
                if (setupCompleteId == -1)
                    return;
                txtSetupComplete.Text = new APICall().ImagePrepApi.GetSetupCompleteFile(setupCompleteId);
            }
            catch { }
        }

        private void ddlSysprep_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var sysprepId = (int)ddlSysprep.SelectedValue;
                if (sysprepId == -1)
                    return;
                txtSysprep.Text = new APICall().ImagePrepApi.GetSysprepFile(sysprepId);
            }
            catch { }

        
        }
    }

   
}
