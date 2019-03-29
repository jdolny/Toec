using System;

namespace Toec_Common.Inventory
{
    public class DtoCertificateInventory
    {
        public string Store { get; set; }
        public string FriendlyName { get; set; }
        public string Subject { get; set; }
        public string Serial { get; set; }
        public string Thumbprint { get; set; }
        public string Issuer { get; set; }
        public DateTime NotBefore { get; set; }
        public DateTime NotAfter { get; set; }
    }
}