using Toec_Common.Inventory;

namespace Toec_Services.InventorySearchers
{
    public class ComputerSystem : IInventorySearcher
    {
        public void Search(DtoInventoryCollection collection)
        {
            using (var wmi = new ServiceWmi<DtoComputerSystemInventory>(new DtoComputerSystemInventory()))
            {
                collection.ComputerSystem = wmi.Execute();
            }

            using (var wmi = new ServiceWmi<DtoComputerSystemProduct>(new DtoComputerSystemProduct()))
            {
                var computerSystemProduct = wmi.Execute();
                if (computerSystemProduct != null)
                    collection.HardwareUUID = computerSystemProduct.UUID;
                else
                    collection.HardwareUUID = string.Empty;
            }
        }
    }
}