using Toec_Common.Inventory;

namespace Toec_Services.InventorySearchers
{
    public class Bios : IInventorySearcher
    {
        public void Search(DtoInventoryCollection collection)
        {
            using (var wmi = new ServiceWmi<DtoBiosInventory>(new DtoBiosInventory()))
            {
                collection.Bios = wmi.Execute();
            }
        }
    }
}