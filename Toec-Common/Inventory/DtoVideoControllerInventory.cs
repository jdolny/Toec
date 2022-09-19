namespace Toec_Common.Inventory
{
    public class DtoVideoControllerInventory
    {
        public const string Query = "select * from Win32_VideoController";
        public string Name { get; set; }
        public ulong AdapterRam { get; set; }

    }
}