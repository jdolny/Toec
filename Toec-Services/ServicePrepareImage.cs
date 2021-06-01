using System;
using System.IO;
using System.Reflection;
using log4net;
using Toec_Common.Dto;
using Toec_Services.ApiCall;
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
                File.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)}\\Toec\\setupcompletecmd_complete");
                Microsoft.Win32.Registry.LocalMachine.DeleteSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\PersonalizationCSP", false);
                new APICall().PolicyApi.RemoveFromFirstRunGroup();
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
            if (imagePrepOptions == null)
            {
                Logger.Info("Image Prep Cancelled.");
                return false;
            }


            Logger.Info("Preparing Computer For Image: ");
            Logger.Info("Checking Toec Service");
            var servResult = new ServiceSystemService().StopToec();
            if (!servResult)
            {
                Logger.Info("Toec Service Must Be Stopped Before Preparing Image.");
                return false;
            }

            _imagePrepOptions = imagePrepOptions;

            ResetToec();

            File.Create($"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)}\\Toec\\image_prepped");
            Logger.Info("Prepare Image Finished");
            return true;
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