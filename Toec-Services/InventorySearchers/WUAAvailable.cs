using Toec_Common.Inventory;
using WUApiLib;

namespace Toec_Services.InventorySearchers
{
    public class WUAAvailable : IInventorySearcher
    {
        public void Search(DtoInventoryCollection collection)
        {
            var updateSession = new UpdateSession();
            var updateSearchResult = updateSession.CreateUpdateSearcher();
            updateSearchResult.Online = true;
            updateSearchResult.ServerSelection = ServerSelection.ssWindowsUpdate;
            updateSearchResult.IncludePotentiallySupersededUpdates = false;

            try
            {
                var searchResults = updateSearchResult.Search("IsInstalled=0 and Type='Software'");
                foreach (IUpdate u in searchResults.Updates)
                {
                    var update = new DtoWindowsUpdateInventory {IsInstalled = false};
                    update.Title = u.Title;
                    update.UpdateId = u.Identity.UpdateID;

                    foreach (ICategory ic in u.Categories)
                    {
                        update.Category = ic.Name;
                        break;
                    }
                    if (!string.IsNullOrEmpty(update.Title))
                        collection.WindowsUpdates.Add(update);
                }
            }
            catch
            {
                //Ignored
            }
        }
    }
}