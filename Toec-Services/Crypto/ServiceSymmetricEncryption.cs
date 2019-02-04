//https://cymbeline.ch/2014/02/28/dynamic-aes-key-exchange-through-rsa-encryption/

using System;
using System.IO;
using System.Security.Cryptography;

namespace Toec_Services.Crypto
{
    public class ServiceSymmetricEncryption
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public string Decrypt(byte[] key, byte[] data)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = key;

                // Extract the IV from the data first.
                var iv = new byte[aes.BlockSize/8];
                Array.Copy(data, iv, iv.Length);
                aes.IV = iv;

                // The remainder of the data is the encrypted data we care about.
                var encryptedData = new byte[data.Length - iv.Length];
                Array.Copy(data, iv.Length, encryptedData, 0, encryptedData.Length);

                using (var decryptor = aes.CreateDecryptor())
                {
                    using (var ms = new MemoryStream(encryptedData))
                    {
                        using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (var reader = new StreamReader(cs))
                            {
                                return reader.ReadToEnd();
                            }
                        }
                    }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public byte[] EncryptData(byte[] key, string data)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.GenerateIV();

                using (var encryptor = aes.CreateEncryptor())
                {
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            using (var writer = new StreamWriter(cs))
                            {
                                writer.Write(data);
                            }
                        }

                        var encrypted = ms.ToArray();
                        var result = new byte[aes.BlockSize/8 + encrypted.Length];

                        // Result is built as: IV (plain text) + Encrypted(data)
                        Array.Copy(aes.IV, result, aes.BlockSize/8);
                        Array.Copy(encrypted, 0, result, aes.BlockSize/8, encrypted.Length);

                        return result;
                    }
                }
            }
        }
    }
}