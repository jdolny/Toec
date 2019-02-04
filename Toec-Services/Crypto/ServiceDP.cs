//https://docs.microsoft.com/en-us/dotnet/standard/security/how-to-use-data-protection

using System;
using System.Security.Cryptography;

namespace Toec_Services.Crypto
{
    public class ServiceDP
    {
        public static byte[] CreateRandomEntropy()
        {
            var entropy = new byte[16];
            new RNGCryptoServiceProvider().GetBytes(entropy);
            return entropy;
        }

        public static byte[] DecryptData(byte[] data, bool isSystem, byte[] entropy)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (data.Length <= 0)
                throw new ArgumentException("data");

            try
            {
                return ProtectedData.Unprotect(data, entropy,
                    isSystem ? DataProtectionScope.LocalMachine : DataProtectionScope.CurrentUser);
            }
            catch
            {
                return null;
            }
        }

        public static byte[] EncryptData(byte[] data, bool isSystem, byte[] entropy)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (data.Length <= 0)
                throw new ArgumentException("data");

            return ProtectedData.Protect(data, entropy,
                isSystem ? DataProtectionScope.LocalMachine : DataProtectionScope.CurrentUser);
        }
    }
}