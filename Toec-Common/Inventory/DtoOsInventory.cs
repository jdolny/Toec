using System;

namespace Toec_Common.Inventory
{
    public class DtoOsInventory
    {
        public string BuildNumber { get; set; }
        public string Caption { get; set; }
        public string OSArchitecture { get; set; }
        public ushort ServicePackMajorVersion { get; set; }
        public ushort ServicePackMinorVersion { get; set; }
        public string Version { get; set; }
        
        public string ReleaseId { get; set; }
        public string UacStatus { get; set; }
        public string LocalTimeZone { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public bool LocationEnabled { get; set; }
        public DateTime LastLocationUpdateUtc { get; set; }
        public string UpdateServer { get; set; }
        public string SUStargetGroup { get; set; }
    }
}