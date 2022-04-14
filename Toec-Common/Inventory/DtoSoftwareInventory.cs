namespace Toec_Common.Inventory
{
    public class DtoSoftwareInventory
    {
        public int Build { get; set; }
        public int Major { get; set; }
        public int Minor { get; set; }
        public string Name { get; set; }
        public int Revision { get; set; }
        public string Version { get; set; }

        public string UninstallString { get; set; }
    }
}