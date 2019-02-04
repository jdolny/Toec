using Toec_Common.Inventory;

namespace Toec_Services.InventorySearchers
{
    public class Processor : IInventorySearcher
    {
        public void Search(DtoInventoryCollection collection)
        {
            using (var wmi = new ServiceWmi<DtoProcessorInventory>(new DtoProcessorInventory()))
            {
                collection.Processor = wmi.Execute();
            }
        }
    }
}