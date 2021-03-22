using System;
using System.Reflection;
using log4net;
using Toec_Services.Entity;

namespace Toec_Services
{
    public class ServicePrepareImage
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private bool _skipHibernation;
        public bool Run(string[] args)
        {
            Logger.Info("Preparing Computer For Image: ");
            Logger.Info("Checking Toec Service");
            var servResult = new ServiceSystemService().StopToec();
            if (!servResult)
            {
                Logger.Error("Toec Service Must Be Stopped Before Preparing Image.");
                return false;
            }

            foreach(var arg in args)
            {
                if (arg.Equals("--prepareImage", StringComparison.InvariantCultureIgnoreCase))
                    continue;
                else if (arg.Equals("skip_hibernation", StringComparison.InvariantCultureIgnoreCase))
                    _skipHibernation = true;

            }

            DisableHibernation();
            ResetToec();

            Logger.Info("Prepare Image Finished");
            return true;
        }

        private void DisableHibernation()
        {
            if (_skipHibernation) return;
            Logger.Info("Disabling Hibernation");
            System.Diagnostics.Process.Start("powercfg.exe", "/h off ");
        }

        private void ResetToec()
        {
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
        }
    }
}