using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using log4net;
using Toec_Common.Dto;
using Toec_Common.Enum;
using Toec_Common.Inventory;
using Toec_Services.ApiCall;
using Toec_Services.Crypto;
using Toec_Services.Entity;
using Toec_Services.InventorySearchers;

namespace Toec_Services
{
    public class ServiceProvision : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ServiceSetting _serviceSetting;

        public ServiceProvision()
        {
            _serviceSetting = new ServiceSetting();
        }

        private string EncryptDataWithIntermediate(AsymmetricAlgorithm publicKey, byte[] data)
        {
            byte[] encryptedKey;
            using (var rsa = (RSACryptoServiceProvider) publicKey)
            {
                encryptedKey = rsa.Encrypt(data, true);
            }
            return Convert.ToBase64String(encryptedKey);
        }

        private byte[] GenerateSymmKey()
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = aes.LegalKeySizes[0].MaxSize;
                return aes.Key;
            }
        }

        public EnumProvisionStatus.Status ProvisionClient()
        {
            var provisionStatusString = _serviceSetting.GetSetting("provision_status");
            EnumProvisionStatus.Status provisionStatus;

            if (string.IsNullOrEmpty(provisionStatusString.Value))
                provisionStatus = EnumProvisionStatus.Status.NotStarted;
            else
                provisionStatus = (EnumProvisionStatus.Status) Convert.ToInt16(provisionStatusString.Value);

            if (provisionStatus == EnumProvisionStatus.Status.NotStarted)
            {
                var stage1Result = ProvisionStage1();
                if (stage1Result == EnumProvisionStatus.Status.IntermediateInstalled)
                {
                    var stage2Result = ProvisionStage2();
                    if (stage2Result == EnumProvisionStatus.Status.PendingConfirmation)
                        return ProvisionStage3();
                    return stage2Result;
                }
                return stage1Result;
            }

            if (provisionStatus == EnumProvisionStatus.Status.IntermediateInstalled ||
                provisionStatus == EnumProvisionStatus.Status.PendingPreProvision ||
                provisionStatus == EnumProvisionStatus.Status.PendingProvisionApproval ||
                provisionStatus == EnumProvisionStatus.Status.PendingReset)
            {
                var stage2Result = ProvisionStage2();
                if (stage2Result == EnumProvisionStatus.Status.PendingConfirmation)
                    return ProvisionStage3();
                return stage2Result;
            }

            if (provisionStatus == EnumProvisionStatus.Status.PendingConfirmation)
            {
                return ProvisionStage3();
            }

            if (provisionStatus == EnumProvisionStatus.Status.Provisioned)
            {
                return RenewSymmKey();
            }

            return EnumProvisionStatus.Status.Error;
        }

        private EnumProvisionStatus.Status ProvisionStage1()
        {
            var response = new APICall().ProvisionApi.GetIntermediateCert(DtoGobalSettings.ClientIdentity.Name);
            if (response == null)
                return EnumProvisionStatus.Status.Error;
            if (response.ProvisionStatus != EnumProvisionStatus.Status.IntermediateInstalled)
            {
                Logger.Error(response.Message);
                return response.ProvisionStatus;
            }
            var bytes = Convert.FromBase64String(response.Certificate);
            var intermediateCert = new X509Certificate2(bytes);
            if (!ServiceCertificate.ValidateCert(intermediateCert))
                return EnumProvisionStatus.Status.Error;

            if (ServiceCertificate.StoreLocalMachine(intermediateCert, StoreName.CertificateAuthority))
            {
                var settingProvisionStatus = _serviceSetting.GetSetting("provision_status");
                settingProvisionStatus.Value =
                    Convert.ToInt16(EnumProvisionStatus.Status.IntermediateInstalled).ToString();
                _serviceSetting.UpdateSettingValue(settingProvisionStatus);
                var intermediateThumbprint = _serviceSetting.GetSetting("intermediate_thumbprint");
                intermediateThumbprint.Value = intermediateCert.Thumbprint;
                _serviceSetting.UpdateSettingValue(intermediateThumbprint);

                return EnumProvisionStatus.Status.IntermediateInstalled;
            }

            return EnumProvisionStatus.Status.Error;
        }

        private EnumProvisionStatus.Status ProvisionStage2()
        {
            var intermediateThumbprint = _serviceSetting.GetSetting("intermediate_thumbprint");
            if (string.IsNullOrEmpty(intermediateThumbprint.Value))
            {
                //assume stage 1 didn't finish
                return EnumProvisionStatus.Status.NotStarted;
            }
            var intermediate = ServiceCertificate.GetCertificateFromStore(intermediateThumbprint.Value,
                StoreName.CertificateAuthority);
            if (intermediate == null) return EnumProvisionStatus.Status.NotStarted;
            var key = GenerateSymmKey();

            var provisionRequest = new DtoProvisionRequest();
            provisionRequest.Name = DtoGobalSettings.ClientIdentity.Name;
            provisionRequest.AdGuid = new ServiceAD().GetADGuid(provisionRequest.Name);
            provisionRequest.SymmKey = EncryptDataWithIntermediate(intermediate.PublicKey.Key, key);
            provisionRequest.InstallationId = DtoGobalSettings.ClientIdentity.InstallationId;

            //include some hardware details
            Logger.Debug("Gathering Hardware Details");
            var inventoryCollection = new DtoInventoryCollection();
            new ComputerSystem().Search(inventoryCollection);
            new Bios().Search(inventoryCollection);
            new Processor().Search(inventoryCollection);
            new Nic().Search(inventoryCollection);
            try
            {
                var m = Convert.ToInt64(inventoryCollection.ComputerSystem.TotalPhysicalMemory);
                provisionRequest.Memory = Convert.ToInt32(m / 1024 / 1024);
            }
            catch
            {
                provisionRequest.Memory = 0;
            }

            try
            {
                provisionRequest.Processor = inventoryCollection.Processor.Name;
            }
            catch
            {
                provisionRequest.Processor = string.Empty;
            }

            try
            {
                provisionRequest.SerialNumber = inventoryCollection.Bios.SerialNumber;
            }
            catch
            {
                provisionRequest.SerialNumber = string.Empty;
            }

            try
            {
                provisionRequest.Model = inventoryCollection.ComputerSystem.Model;
            }
            catch
            {
                provisionRequest.Model = string.Empty;
            }

            try
            {
                foreach (var nic in inventoryCollection.NetworkAdapters)
                {
                    provisionRequest.Macs.Add(nic.Mac);
                }
            }
            catch
            {
                //do nothing
            }


            inventoryCollection = null;

            var response = new APICall().ProvisionApi.ProvisionClient(provisionRequest);
            if (response == null)
                return EnumProvisionStatus.Status.Error;
            if (response.ProvisionStatus == EnumProvisionStatus.Status.Reset)
            {
                Logger.Info("Client Reset Approved.  Starting Reset Process.");
                return EnumProvisionStatus.Status.Reset;
            }
            if (response.ProvisionStatus == EnumProvisionStatus.Status.FullReset)
            {
                Logger.Info("Client Full Reset Requested.  Starting Full Reset Process.");
                return EnumProvisionStatus.Status.FullReset;
            }
            if (response.ProvisionStatus == EnumProvisionStatus.Status.PendingReset)
            {
                Logger.Info("Client Is Pending Reset Approval.");
                return EnumProvisionStatus.Status.PendingReset;
            }
            if (response.ProvisionStatus == EnumProvisionStatus.Status.PendingProvisionApproval)
            {
                Logger.Info("Client Is Pending Provisioning Approval");
                return EnumProvisionStatus.Status.PendingProvisionApproval;
            }
            if (response.ProvisionStatus == EnumProvisionStatus.Status.PendingPreProvision)
            {
                Logger.Info("Client Has Not Been Pre-Provisioned And The Current Security Policy Requires It.");
                return EnumProvisionStatus.Status.PendingPreProvision;
            }
            if (response.ProvisionStatus != EnumProvisionStatus.Status.PendingConfirmation)
                return EnumProvisionStatus.Status.Error;

            var byteCert = Convert.FromBase64String(response.Certificate);
            var base64Cert = new ServiceSymmetricEncryption().Decrypt(key, byteCert);
            var deviceCert = new X509Certificate2(Convert.FromBase64String(base64Cert));
            if (ServiceCertificate.StoreLocalMachine(deviceCert, StoreName.My))
            {
                var deviceThumbprint = _serviceSetting.GetSetting("device_thumbprint");
                deviceThumbprint.Value = deviceCert.Thumbprint;
                _serviceSetting.UpdateSettingValue(deviceThumbprint);

                var computerIdentifier = _serviceSetting.GetSetting("computer_identifier");
                computerIdentifier.Value = response.ComputerIdentifier;
                DtoGobalSettings.ClientIdentity.Guid = response.ComputerIdentifier;
                _serviceSetting.UpdateSettingValue(computerIdentifier);

                var entropy = _serviceSetting.GetSetting("entropy");
                var entropyBytes = ServiceDP.CreateRandomEntropy();
                entropy.Value = Convert.ToBase64String(entropyBytes);
                _serviceSetting.UpdateSettingValue(entropy);

                var encryptedKey = ServiceDP.EncryptData(key, true, entropyBytes);
                var keySetting = _serviceSetting.GetSetting("encryption_key");
                keySetting.Value = Convert.ToBase64String(encryptedKey);
                _serviceSetting.UpdateSettingValue(keySetting);

                var settingProvisionStatus = _serviceSetting.GetSetting("provision_status");
                settingProvisionStatus.Value = Convert.ToInt16(response.ProvisionStatus).ToString();
                _serviceSetting.UpdateSettingValue(settingProvisionStatus);
            }

            return EnumProvisionStatus.Status.PendingConfirmation;
        }

        private EnumProvisionStatus.Status ProvisionStage3()
        {
            var deviceThumbprint = _serviceSetting.GetSetting("device_thumbprint");
            if (string.IsNullOrEmpty(deviceThumbprint.Value))
            {
                //assume stage 2 didn't finish
                return EnumProvisionStatus.Status.NotStarted;
            }
            var deviceCert = ServiceCertificate.GetCertificateFromStore(deviceThumbprint.Value, StoreName.My);
            if (deviceCert == null) return EnumProvisionStatus.Status.Error;

            var confirmRequest = new DtoConfirmProvisionRequest();
            confirmRequest.Name = DtoGobalSettings.ClientIdentity.Name;
            confirmRequest.Guid = _serviceSetting.GetSetting("computer_identifier").Value;
            confirmRequest.DeviceCert = Convert.ToBase64String(deviceCert.RawData);

            var confirmResult = new APICall().ProvisionApi.ConfirmProvisionRequest(confirmRequest);
            if (confirmResult == null) return EnumProvisionStatus.Status.Error;

            if (confirmResult.ProvisionStatus != EnumProvisionStatus.Status.Provisioned)
                return confirmResult.ProvisionStatus;

            UpdateComServers(confirmResult.ComServers);
            var settingProvisionStatus = _serviceSetting.GetSetting("provision_status");
            settingProvisionStatus.Value = Convert.ToInt16(confirmResult.ProvisionStatus).ToString();
            _serviceSetting.UpdateSettingValue(settingProvisionStatus);
            return EnumProvisionStatus.Status.Provisioned;
        }

        private EnumProvisionStatus.Status RenewSymmKey()
        {
            var deviceThumbprint = _serviceSetting.GetSetting("device_thumbprint");

            var deviceCert = ServiceCertificate.GetCertificateFromStore(deviceThumbprint.Value, StoreName.My);
            if (deviceCert == null) return EnumProvisionStatus.Status.Error;

            var key = GenerateSymmKey();

            var renewRequest = new DtoRenewKeyRequest();
            renewRequest.Name = DtoGobalSettings.ClientIdentity.Name;
            renewRequest.Guid = DtoGobalSettings.ClientIdentity.Guid;
            renewRequest.DeviceCert = Convert.ToBase64String(deviceCert.RawData);
            renewRequest.SymmKey = Convert.ToBase64String(key);

            var renewResult = new APICall().ProvisionApi.RenewSymmKey(renewRequest);
            if (renewResult == null) return EnumProvisionStatus.Status.Error;
            if (renewResult.ProvisionStatus != EnumProvisionStatus.Status.Provisioned)
                return renewResult.ProvisionStatus;

            UpdateComServers(renewResult.ComServers);
            var entropy = _serviceSetting.GetSetting("entropy");
            var entropyBytes = ServiceDP.CreateRandomEntropy();
            entropy.Value = Convert.ToBase64String(entropyBytes);
            _serviceSetting.UpdateSettingValue(entropy);

            var encryptedKey = ServiceDP.EncryptData(key, true, entropyBytes);
            var keySetting = _serviceSetting.GetSetting("encryption_key");
            keySetting.Value = Convert.ToBase64String(encryptedKey);
            _serviceSetting.UpdateSettingValue(keySetting);
            return EnumProvisionStatus.Status.Provisioned;
        }

        private void UpdateComServers(List<DtoClientComServers> comServers)
        {
            var settingService = new ServiceSetting();
            var activeString = "";
            foreach (var server in comServers.Where(x => x.Role.Equals("Active")))
            {
                activeString += server.Url + ",";
            }
            var trimmedActive = activeString.Trim(',');

            var passiveString = "";
            foreach (var server in comServers.Where(x => x.Role.Equals("Passive")))
            {
                passiveString += server.Url + ",";
            }
            var trimmedPassive = passiveString.Trim(',');

            if (!string.IsNullOrEmpty(trimmedActive))
            {
                var currentActive = settingService.GetSetting("active_com_servers");
                currentActive.Value = trimmedActive;
                settingService.UpdateSettingValue(currentActive);
            }

            if (!string.IsNullOrEmpty(trimmedPassive))
            {
                var currentPassive = settingService.GetSetting("passive_com_servers");
                currentPassive.Value = trimmedPassive;
                settingService.UpdateSettingValue(currentPassive);
            }

        }

        public bool VerifyProvisionStatus()
        {
            Logger.Info("Verifying Client Provision Status");

            var provisionStatusString = _serviceSetting.GetSetting("provision_status");
            EnumProvisionStatus.Status provisionStatus;

            if (string.IsNullOrEmpty(provisionStatusString.Value))
                provisionStatus = EnumProvisionStatus.Status.NotStarted;
            else
                provisionStatus = (EnumProvisionStatus.Status) Convert.ToInt16(provisionStatusString.Value);

            switch (provisionStatus)
            {
                case EnumProvisionStatus.Status.NotStarted:
                    //Computer is not provisioned, verify the CA exists
                    var caThumbprint = _serviceSetting.GetSetting("ca_thumbprint");
                    var ca = ServiceCertificate.GetCertificateFromStore(caThumbprint.Value, StoreName.Root);
                    if (ca == null)
                    {
                        Logger.Error("Certificate Authority Could Not Be Found.  Application Cannot Continue.");
                        //Provisioning can never complete without the correct CA, don't return anything, just exit.
                        Task.Delay(10*1000).Wait();
                        Environment.Exit(1);
                    }
                    break;
                case EnumProvisionStatus.Status.PendingConfirmation:
                case EnumProvisionStatus.Status.Provisioned:
                    var deviceThumbprint = _serviceSetting.GetSetting("device_thumbprint");
                    var deviceCert = ServiceCertificate.GetCertificateFromStore(deviceThumbprint.Value, StoreName.My);
                    if (deviceCert == null)
                    {
                        Logger.Error("Device Certificate Could Not Be Found.  Restarting Provisioning Process.");
                        return false;
                    }
                    if (!ServiceCertificate.ValidateCert(deviceCert))
                    {
                        return false;
                    }
                    var clientIdentity = deviceCert.Subject;
                    var expectedId = _serviceSetting.GetSetting("computer_identifier");
                    Logger.Debug("Current Expected Identity: " + expectedId.Value);
                    Logger.Debug("Current Identity: " + clientIdentity);
                    if (!clientIdentity.Contains(expectedId.Value))
                    {
                        Logger.Error("The Current Identity Doesn't Match The Expected Identity");
                        return false;
                    }
                    break;
                default:
                    var intermediateThumbprint = _serviceSetting.GetSetting("intermediate_thumbprint");
                    var intermediate = ServiceCertificate.GetCertificateFromStore(intermediateThumbprint.Value,
                        StoreName.CertificateAuthority);
                    if (intermediate == null)
                    {
                        Logger.Error("Intermediate Certificate Could Not Be Found.  Restarting Provisioning Process.");
                        return false;
                    }
                    if (!ServiceCertificate.ValidateCert(intermediate))
                    {
                        return false;
                    }
                    break;
            }

            Logger.Info("Verification Complete");
            return true;
        }


        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if(_serviceSetting != null)
                    _serviceSetting.Dispose();
                }
            }
            this.disposed = true;
        }
    }
}