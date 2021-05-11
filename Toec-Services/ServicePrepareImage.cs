using System;
using System.IO;
using System.Reflection;
using log4net;
using Toec_Common.Dto;
using Toec_Services.Entity;

namespace Toec_Services
{
    public class ServicePrepareImage
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private DtoImagePrepOptions _imagePrepOptions;

        public bool Cleanup()
        {
            try
            {
                File.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)}\\Toec\\image_prepped");
                Microsoft.Win32.Registry.LocalMachine.DeleteSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\PersonalizationCSP", false);
            }
            catch(Exception ex)
            {
                Logger.Debug(ex.Message);
                return false;
            }

            return true;
        }

        public bool Run(DtoImagePrepOptions imagePrepOptions)
        {
            if(imagePrepOptions == null)
            {
                Logger.Info("Image Prep Cancelled.");
                return false;
            }

            if(imagePrepOptions.RunSysprep && string.IsNullOrEmpty(imagePrepOptions.SysprepAnswerPath))
            {
                Logger.Info("A Sysprep Answer File Was Not Defined.  Image Prep Cancelled");
                return false;
            }

            Logger.Info("Preparing Computer For Image: ");
            Logger.Info("Checking Toec Service");
            var servResult = new ServiceSystemService().StopToec();
            if (!servResult)
            {
                Logger.Error("Toec Service Must Be Stopped Before Preparing Image.");
                return false;
            }

            _imagePrepOptions = imagePrepOptions;

            DisableHibernation();
            AddDriverRegistry();
            EnableWinLogonBackground();
            CreateSetupComplete();
            ResetToec();

            File.Create($"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)}\\Toec\\image_prepped");
            RunSysprep();


            Logger.Info("Prepare Image Finished");
            return true;
        }

        private void CreateSetupComplete()
        {
            if (!_imagePrepOptions.CreateSetupComplete) return;
            Logger.Info("Creating Setup Complete Script");
            var winPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            Directory.CreateDirectory(Path.Combine(winPath, "Setup", "Scripts"));
            var scriptPath = Path.Combine(winPath, "Setup", "Scripts","setupcomplete.cmd");
            File.WriteAllText(scriptPath, _imagePrepOptions.SetupCompleteContents);
            Logger.Info("Finished Creating Setup Complete Script");
        }

        private void AddDriverRegistry()
        {
            if (!_imagePrepOptions.AddDriverRegistry) return;
            Logger.Info("Updating Registry DevicePath Locations.");
            Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\", "DevicePath", "%SystemRoot%\\inf;c:\\drivers");
            Logger.Info("Finished Updating Registry DevicePath Locations.");
        }

        private void RunSysprep()
        {
            if (!_imagePrepOptions.RunSysprep) return;
            Logger.Info("Running Sysprep");
            if(!string.IsNullOrEmpty(_imagePrepOptions.SysprepAnswerPath))
            {
                var winPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                var sysPrepPath = Path.Combine(winPath, "System32", "Sysprep");
                File.Copy(_imagePrepOptions.SysprepAnswerPath, Path.Combine(sysPrepPath,"unattend.xml"), true);
                System.Diagnostics.Process.Start(Path.Combine(sysPrepPath,"sysprep.exe"), $"/oobe /generalize /shutdown /unattend:{Path.Combine(sysPrepPath,"unattend.xml")}");
            }
            Logger.Info("Finished Running Sysprep");

        }

        private void EnableWinLogonBackground()
        {
            if (!_imagePrepOptions.EnableFinalizingBackground) return;
            Logger.Info("Setting finalizing background image.");
            Microsoft.Win32.RegistryKey key;
            key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\PersonalizationCSP");
            key.SetValue("LockScreenImagePath", $"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)}\\Toec\\finalizing_lock_image.png", Microsoft.Win32.RegistryValueKind.String);
            key.SetValue("LockScreenImageUrl", $"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)}\\Toec\\finalizing_lock_image.png", Microsoft.Win32.RegistryValueKind.String);
            key.SetValue("DesktopImagePath", $"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)}\\Toec\\finalizing_desktop_image.png", Microsoft.Win32.RegistryValueKind.String);
            key.SetValue("DesktopImageUrl", $"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)}\\Toec\\finalizing_desktop_image.png", Microsoft.Win32.RegistryValueKind.String);
            key.Close();
            Logger.Info("Finished Setting finalizing background image.");
        }

        private void DisableHibernation()
        {
            if (!_imagePrepOptions.RunHibernate) return;
            Logger.Info("Disabling Hibernation");
            System.Diagnostics.Process.Start("powercfg.exe", "/h off ");
            Logger.Info("Finished Disabling Hibernation");
        }

        private void ResetToec()
        {
            if (!_imagePrepOptions.ResetToec) return;
            Logger.Info("Resetting Toec");

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
                Logger.Error("Prepare Image Failed.  Could Not Reset ID's");
            }

            Logger.Info("Finished Resetting Toec");
        }
    }
}