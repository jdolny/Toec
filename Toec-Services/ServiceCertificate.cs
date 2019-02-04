using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using log4net;
using Toec_Services.Entity;

namespace Toec_Services
{
    public class ServiceCertificate
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static bool DeleteAllDeviceCertificates()
        {
            Logger.Debug("Deleting All Existing Device Certificates");
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            try
            {
                store.Open(OpenFlags.ReadWrite);
                var collection = store.Certificates.Find(X509FindType.FindByIssuerName, "Toems Intermediate", false);
                foreach (var cert in collection)
                {
                    store.Remove(cert);
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Could Not Delete Existing Device Certificates");
                Logger.Error(ex.Message);
                return false;
            }
            finally
            {
                store.Close();
            }
        }

        public static bool DeleteIntermediate()
        {
            Logger.Debug("Deleting Intermediate Certificate");
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            try
            {
                store.Open(OpenFlags.ReadWrite);
                var collection = store.Certificates.Find(X509FindType.FindBySubjectName, "Toems Intermediate",
                    false);
                foreach (var cert in collection)
                {
                    store.Remove(cert);
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Could Not Delete Intermediate Certificate");
                Logger.Error(ex.Message);
                return false;
            }
            finally
            {
                store.Close();
            }
        }

        public static X509Certificate2 GetCertificateFromStore(string thumbprint, StoreName storeName)
        {
            var store = new X509Store(storeName, StoreLocation.LocalMachine);
            try
            {
                store.Open(OpenFlags.ReadOnly);
                var collection = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, true);

                if (collection.Count == 0)
                {
                    if (storeName == StoreName.Root)
                    {
                        Logger.Error("Could Not Find Certificate Authority With Thumbprint: " + thumbprint);
                    }
                    else if (storeName == StoreName.CertificateAuthority)
                        Logger.Error("Could Not Find Intermediate Certificate With Thumbprint: " + thumbprint);
                    else
                    {
                        Logger.Error("Could Not Find Device Certificate With Thumbprint: " + thumbprint);
                    }

                    return null;
                }
                var cert = new X509Certificate2();
                if (collection[0] != null)
                {
                    cert = collection[0];
                }

                return cert;
            }
            catch (Exception ex)
            {
                Logger.Error("Could Not Find A Certificate With The Following Thumbprint: " + thumbprint);
                Logger.Error(ex.Message);
                return null;
            }
            finally
            {
                store.Close();
            }
        }

        public static bool StoreLocalMachine(X509Certificate2 certificate, StoreName storeName)
        {
            var store = new X509Store(storeName, StoreLocation.LocalMachine);
            try
            {
                store.Open(OpenFlags.ReadWrite);
                store.Add(certificate);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Could Not Store The Certificate");
                Logger.Error(ex.Message);
                return false;
            }
            finally
            {
                store.Close();
            }
        }

        public static bool ValidateCert(X509Certificate2 cert)
        {
            var chain = new X509Chain();
            var chainPolicy = new X509ChainPolicy
            {
                RevocationMode = X509RevocationMode.NoCheck,
                RevocationFlag = X509RevocationFlag.EntireChain
            };
            chain.ChainPolicy = chainPolicy;

            try
            {
                if (chain.Build(cert))
                {
                    var caThumbprint = new ServiceSetting().GetSetting("ca_thumbprint");
                    var ca = GetCertificateFromStore(caThumbprint.Value, StoreName.Root);
                    if (ca == null) return false;
                    var correctCaInChain =
                        chain.ChainElements.Cast<X509ChainElement>().Any(x => x.Certificate.Thumbprint == ca.Thumbprint);
                    if (!correctCaInChain)
                    {
                        Logger.Error("Could Not Find CA In Certificate Chain With Thumbprint : " + ca.Thumbprint);
                        return false;
                    }
                    return true;
                }

                Logger.Error("Could Not Validate Certificate: " + cert.Subject);

                foreach (var chainElement in chain.ChainElements)
                {
                    foreach (var chainStatus in chainElement.ChainElementStatus)
                    {
                        Logger.Error(chainStatus.StatusInformation);
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Could Not Validate Certificate: " + cert.Subject);
                Logger.Error(ex.Message);
                return false;
            }
        }

        public static bool VerifySignature(X509Certificate2 cert, byte[] signature, string message)
        {
            var csp = (RSACryptoServiceProvider) cert.PublicKey.Key;
            var sha1 = new SHA1Managed();
            var encoding = new UnicodeEncoding();
            var data = encoding.GetBytes(message);
            var hash = sha1.ComputeHash(data);
            return csp.VerifyHash(hash, CryptoConfig.MapNameToOID("SHA1"), signature);
        }
    }
}