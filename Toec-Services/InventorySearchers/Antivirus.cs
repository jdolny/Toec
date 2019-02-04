using System;
using System.Collections.Generic;
using Toec_Common.Inventory;

namespace Toec_Services.InventorySearchers
{
    public class AntiVirus : IInventorySearcher
    {
        public void Search(DtoInventoryCollection collection)
        {
            List<DtoAntiVirusWmi> av;
            using (var wmi = new ServiceWmi<DtoAntiVirusWmi>(new DtoAntiVirusWmi(),"root\\SecurityCenter2"))
            {
                av = wmi.GetObjectList();
            }

            var avInventoryList = new List<DtoAntiVirusInventory>();
            foreach (var a in av)
            {
                var avInventory = new DtoAntiVirusInventory();

                string provider;
                string realTimeScanner;
                string upToDate;

                try
                {
                    avInventory.DisplayName = a.DisplayName;
                    var hex = a.ProductState.ToString("X6");
                    var productState = a.ProductState.ToString();
                    avInventory.ProductState = Convert.ToInt32(productState);
                    provider = hex.Substring(0, 2);
                    realTimeScanner = hex.Substring(2, 2);
                    upToDate = hex.Substring(4, 2);
                }
                catch
                {
                    //ignored
                    continue;
                }
            

                switch (provider)
                {
                    case "05":
                    case "07":
                        avInventory.Provider = "AntiVirus-Firewall";
                        break;
                    case "04":
                    case "06":
                        avInventory.Provider = "AntiVirus";
                        break;
                    default:
                        avInventory.Provider = "Unknown";
                        break;
                }

                switch (realTimeScanner)
                {
                    case "00":
                        avInventory.RealtimeScanner = "Off";
                        break;
                    case "01":
                        avInventory.RealtimeScanner = "Expired";
                        break;
                    case "10":
                        avInventory.RealtimeScanner = "On";
                        break;
                    case "11":
                        avInventory.RealtimeScanner = "Snoozed";
                        break;
                    default:
                        avInventory.Provider = "Unknown";
                        break;

                }
                switch (upToDate)
                {
                    case "00":
                        avInventory.DefinitionStatus = "Up To Date";
                        break;
                    case "10":
                        avInventory.DefinitionStatus = "Out Of Date";
                        break;
                    default:
                        avInventory.DefinitionStatus = "Unknown";
                        break;
                }

                avInventoryList.Add(avInventory);
            }

            collection.AntiVirus = avInventoryList;
            /*
             https://gallery.technet.microsoft.com/scriptcenter/Get-the-status-of-4b748f25#content
             $SECURITY_PROVIDER = switch ($WSC_SECURITY_PROVIDER) 
                { 
                    0  {"NONE"} 
                    1  {"FIREWALL"} 
                    2  {"AUTOUPDATE_SETTINGS"} 
                    4  {"ANTIVIRUS"} 
                    8  {"ANTISPYWARE"} 
                    16 {"INTERNET_SETTINGS"} 
                    32 {"USER_ACCOUNT_CONTROL"} 
                    64 {"SERVICE"} 
                    default {"UNKNOWN"} 
                } 
 
 
                $RealTimeProtectionStatus = switch ($WSC_SECURITY_PRODUCT_STATE) 
                { 
                    "00" {"OFF"}  
                    "01" {"EXPIRED"} 
                    "10" {"ON"} 
                    "11" {"SNOOZED"} 
                    default {"UNKNOWN"} 
                } 
 
                $DefinitionStatus = switch ($WSC_SECURITY_SIGNATURE_STATUS) 
                { 
                    "00" {"UP_TO_DATE"} 
                    "10" {"OUT_OF_DATE"} 
                    default {"UNKNOWN"} 
                }   
             */
        }
    }
}