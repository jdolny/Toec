namespace Toec_Common.Inventory
{
    public class DtoComputerSystemInventory
    {
        public const string Query = "select * from Win32_ComputerSystem";
        public string Domain { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string Name { get; set; }
        public ulong TotalPhysicalMemory { get; set; }
        public string Workgroup { get; set; }
    }
}