using System;
using Toec_Common.Inventory;

namespace Toec_Services.InventorySearchers
{
    public class Firewall : IInventorySearcher
    {
        const int NET_FW_PROFILE2_DOMAIN = 1;
        const int NET_FW_PROFILE2_PRIVATE = 2;
        const int NET_FW_PROFILE2_PUBLIC = 4;

        public void Search(DtoInventoryCollection collection)
        {
            //http://csharphelper.com/blog/2015/01/access-firewall-information-and-check-firewall-status-using-the-dynamic-keyword-in-c/
            
            Type FWManagerType = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
            dynamic FWManager = Activator.CreateInstance(FWManagerType);
            collection.Firewall.DomainEnabled = FWManager.FirewallEnabled(NET_FW_PROFILE2_DOMAIN);
            collection.Firewall.PrivateEnabled =  FWManager.FirewallEnabled(NET_FW_PROFILE2_PRIVATE);
            collection.Firewall.PublicEnabled = FWManager.FirewallEnabled(NET_FW_PROFILE2_PUBLIC);
        }
    }
}