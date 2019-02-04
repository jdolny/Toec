using System;

namespace Toec_Common.Inventory
{
    public class DtoBitlockerInventory
    {
        public const string Query = "select * from win32_encryptablevolume";
        public string DriveLetter { get; set; }
        public UInt32 ProtectionStatus { get; set; }      
    }
}