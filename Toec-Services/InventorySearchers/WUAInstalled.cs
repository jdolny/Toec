using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Toec_Common.Inventory;
using WUApiLib;

namespace Toec_Services.InventorySearchers
{
    //Getting a list of installed Windows updates seems much more difficult than it should be.  This tries to get as many as possible using various methods.

    public class WUAInstalled : IInventorySearcher
    {

        public void Search(DtoInventoryCollection collection)
        {
            var list = new List<DtoWindowsUpdateInventory>();
            try
            {
                var updateSession = new UpdateSession();
                var updateSearchResult = updateSession.CreateUpdateSearcher();
                var count = updateSearchResult.GetTotalHistoryCount();
                var history = updateSearchResult.QueryHistory(0, count);

                for (var i = 0; i < count; ++i)
                {
                    var update = new DtoWindowsUpdateInventory {IsInstalled = true};
                    update.Title = history[i].Title;
                    update.UpdateId = history[i].UpdateIdentity.UpdateID;
                    update.LastDeploymentChangeTime = history[i].Date.ToString(CultureInfo.InvariantCulture);
                    if (!string.IsNullOrEmpty(update.Title))
                        list.Add(update);
                }
            }
            catch
            {
                //ignored
            }

            try
            {
                var uSession = new UpdateSession();
                var uSearcher = uSession.CreateUpdateSearcher();
                uSearcher.Online = true;
                uSearcher.ServerSelection = ServerSelection.ssWindowsUpdate;
                var searchResults = uSearcher.Search("IsInstalled=1 AND IsHidden=0");
                foreach (IUpdate u in searchResults.Updates)
                {
                    var update = new DtoWindowsUpdateInventory() { IsInstalled = true };
                    update.Title = u.Title;
                    update.LastDeploymentChangeTime = u.LastDeploymentChangeTime.ToString(CultureInfo.InvariantCulture);
                    update.UpdateId = u.Identity.UpdateID;
                    list.Add(update);
                }
            }
            catch
            {
                //ignored               
            }

            collection.WindowsUpdates.AddRange(list.GroupBy(x => new {x.Title}).Select(g => g.First()).ToList());

            using (var wmi = new ServiceWmi<DtoInstalledUpdatesWmi>(new DtoInstalledUpdatesWmi()))
            {
                var wmiUpdates = wmi.GetObjectList();
                foreach (var wmiUpdate in wmiUpdates)
                {
                    if (collection.WindowsUpdates.Any(wu => wu.Title.Contains(wmiUpdate.HotFixID))) continue;
                    var update = new DtoWindowsUpdateInventory() { IsInstalled = true };
                    update.Title = wmiUpdate.HotFixID;
                    update.LastDeploymentChangeTime = wmiUpdate.InstalledOn;
                    collection.WindowsUpdates.Add(update);
                }
            }
           
        }
    }
}