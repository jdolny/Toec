namespace Toec_Common.Inventory
{
    public class DtoLogicalVolumeInventory
    {
        public string Drive { get; set; }
        public int FreeSpaceGB { get; set; }
        public int FreeSpacePercent { get; set; }
        public int SizeGB { get; set; }
    }
}