using Toec_Common.Inventory;

namespace Toec_Services.InventorySearchers
{
    public class Bitlocker : IInventorySearcher
    {
        public void Search(DtoInventoryCollection collection)
        {
            using (var wmi = new ServiceWmi<DtoBitlockerInventory>(new DtoBitlockerInventory(), "root\\cimv2\\security\\microsoftvolumeencryption"))
            {
                collection.Bitlocker = wmi.GetObjectList();
            }
        }
    }
}