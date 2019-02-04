using Toec_Common.Inventory;

namespace Toec_Services.InventorySearchers
{
    public class HardDrive : IInventorySearcher
    {
        public void Search(DtoInventoryCollection collection)
        {
            using (var wmi = new ServiceWmi<DtoHardDriveInventory>(new DtoHardDriveInventory()))
            {
                collection.HardDrives = wmi.GetObjectList();
            }
        }
    }
}