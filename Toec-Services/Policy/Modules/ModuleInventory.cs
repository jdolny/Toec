using System;
using System.Linq;
using System.Reflection;
using log4net;
using Toec_Common.Inventory;
using Toec_Services.ApiCall;
using Toec_Services.InventorySearchers;

namespace Toec_Services.Policy.Modules
{
    public class ModuleInventory
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public bool Run()
        {
            Logger.Info("Running Inventory Module");
            var collection = new DtoInventoryCollection();
            var instances = from t in Assembly.GetExecutingAssembly().GetTypes()
                where t.GetInterfaces().Contains(typeof (IInventorySearcher))
                      && t.GetConstructor(Type.EmptyTypes) != null
                select Activator.CreateInstance(t) as IInventorySearcher;

            foreach (var instance in instances)
            {
                Logger.Debug("Scanning " + instance);
                instance.Search(collection);
            }

            var result = new APICall().InventoryApi.SubmitInventory(collection);
            if (result != null)
                return result.Value;
            else
                return false;
        }
    }
}