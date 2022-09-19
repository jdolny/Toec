using Toec_Common.Inventory;

namespace Toec_Services.InventorySearchers
{
    public class Gpu : IInventorySearcher
    {
        public void Search(DtoInventoryCollection collection)
        {
            using (var wmi = new ServiceWmi<DtoVideoControllerInventory>(new DtoVideoControllerInventory()))
            {
                collection.Gpu = wmi.GetObjectList();
            }
        }
    }
}