namespace Toec_Common.Inventory
{
    public class DtoPrinterInventory
    {
        public const string Query = "select * from Win32_Printer";
        public string DriverName { get; set; }
        public bool Local { get; set; }
        public string Name { get; set; }
        public bool Network { get; set; }
        public string ShareName { get; set; }
        public string SystemName { get; set; }
    }
}