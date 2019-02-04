using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using log4net;
using Toec_Services.Crypto;
using Toec_Services.Entity;

namespace Toec_Services
{
    public class ServiceResetServerKey
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public bool Reset(string key, string thumbprint)
        {
            Logger.Info("Resetting Server Key");
            var entropy = new byte[16];
            new RNGCryptoServiceProvider().GetBytes(entropy);
            var serverKeyBytes = Encoding.ASCII.GetBytes(key);
            var encryptedKey = ServiceDP.EncryptData(serverKeyBytes, true, entropy);

            var serviceSetting = new ServiceSetting();
            var serverKeyEntropy = serviceSetting.GetSetting("server_key_entropy");
            serverKeyEntropy.Value = Convert.ToBase64String(entropy);
            serviceSetting.UpdateSettingValue(serverKeyEntropy);

            var serverKey = serviceSetting.GetSetting("server_key");
            serverKey.Value = Convert.ToBase64String(encryptedKey);
            serviceSetting.UpdateSettingValue(serverKey);


            var caThumbprint = serviceSetting.GetSetting("ca_thumbprint");
            caThumbprint.Value = thumbprint;
            serviceSetting.UpdateSettingValue(caThumbprint);

            Logger.Info("Resetting Server Key Finished");
            return true;
        }
    }
}