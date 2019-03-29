using log4net;
using System;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Toec_Common.Inventory;

namespace Toec_Services.InventorySearchers
{
    public class Certificate : IInventorySearcher
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Search(DtoInventoryCollection collection)
        {
            foreach (var s in (StoreName[])Enum.GetValues(typeof(StoreName)))
            {
                if (s != StoreName.My && s != StoreName.Root && s != StoreName.CertificateAuthority) continue;
                var store = new X509Store(s, StoreLocation.LocalMachine);

                try
                {
                    store.Open(OpenFlags.ReadOnly);
                    var certs = store.Certificates;
                    foreach (var cert in certs)
                    {
                        var certItem = new DtoCertificateInventory();
                        certItem.Store = s.ToString();
                        certItem.Subject = cert.Subject;
                        certItem.FriendlyName = cert.FriendlyName;
                        certItem.Thumbprint = cert.Thumbprint;
                        certItem.Serial = cert.SerialNumber;
                        certItem.Issuer = cert.Issuer;
                        certItem.NotBefore = cert.NotBefore;
                        certItem.NotAfter = cert.NotAfter;
                        collection.Certificates.Add(certItem);
                    }

                }
                catch (Exception ex)
                {
                    Logger.Error($"Could Not Get Certificate Inventory For {s.ToString()} Store");
                    Logger.Error(ex.Message);
                }
                finally
                {
                    store.Close();
                }
            }
        }
    }
}