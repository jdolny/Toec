namespace Toec_Common.Inventory
{
    public class DtoHardDriveInventory
    {
        public const string Query = "select * from Win32_DiskDrive where size > 0 AND MediaLoaded = TRUE";
        public string FirmwareRevision { get; set; }
        public string Model { get; set; }
        public string SerialNumber { get; set; }
        public ulong Size { get; set; }
        public string Status { get; set; }
    }
}