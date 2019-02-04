using Toec_Common.Inventory;

namespace Toec_Services.InventorySearchers
{
    public interface IInventorySearcher
    {
        void Search(DtoInventoryCollection collection);
    }
}