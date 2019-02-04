namespace Toec_Common.Inventory
{
    public class DtoProcessorInventory
    {
        public const string Query = "Select * From Win32_Processor";
        public uint MaxClockSpeed { get; set; }
        public string Name { get; set; }
        public uint NumberOfCores { get; set; }
    }
}