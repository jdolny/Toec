namespace Toec_Common.Inventory
{
    public class DtoBiosInventory
    {
        public const string Query = "Select * From Win32_BIOS";
        public string SerialNumber { get; set; }
        public string SMBIOSBIOSVersion { get; set; }
        public string Version { get; set; }
    }
}