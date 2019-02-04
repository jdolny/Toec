using System;
using System.Device.Location;
using System.Globalization;
using System.Reflection;
using log4net;
using Microsoft.Win32;
using Toec_Common.Inventory;

namespace Toec_Services.InventorySearchers
{
    public class Os : IInventorySearcher
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public void Search(DtoInventoryCollection collection)
        {
            using (var wmi = new ServiceWmi<DtoOsWmi>(new DtoOsWmi()))
            {
                var wmiInfo = wmi.Execute();
                var osInventory = new DtoOsInventory();
                try
                {
                    osInventory.BuildNumber = wmiInfo.BuildNumber;
                    osInventory.Caption = wmiInfo.Caption;
                    osInventory.OSArchitecture = wmiInfo.OSArchitecture;
                    osInventory.ServicePackMajorVersion = wmiInfo.ServicePackMajorVersion;
                    osInventory.ServicePackMinorVersion = wmiInfo.ServicePackMinorVersion;
                    osInventory.Version = wmiInfo.Version;
                    osInventory.LocalTimeZone = TimeZone.CurrentTimeZone.StandardName;
                    collection.Os = osInventory;
                }
                catch(Exception ex)
                {
                    Logger.Error("Could Not Parse OS Inventory");
                    Logger.Error(ex.Message);
                }

                try
                {
                    //get release id from registry
                    var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                    if (key != null)
                    {
                        osInventory.ReleaseId = Convert.ToString(key.GetValue("ReleaseId"));
                    }

                    //get uac status from registry
                    var uacKey =
                        Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System");
                    if (uacKey != null)
                    {

                        var consentPromptBehavior = uacKey.GetValue("ConsentPromptBehaviorAdmin");
                        var promptOnSecureDesktop = uacKey.GetValue("PromptOnSecureDesktop");

                        if (consentPromptBehavior != null && promptOnSecureDesktop != null)
                        {
                            var stringConsent = consentPromptBehavior.ToString();
                            var stringPrompt = promptOnSecureDesktop.ToString();
                            if (stringConsent.Equals("0") && stringPrompt.Equals("0"))
                                collection.Os.UacStatus = "Never Notify";
                            else if (stringConsent.Equals("5") && stringPrompt.Equals("0"))
                                collection.Os.UacStatus = "Notify Changes";
                            else if (stringConsent.Equals("5") && stringPrompt.Equals("1"))
                                collection.Os.UacStatus = "Notify Changes (Dim)";
                            else if (stringConsent.Equals("2") && stringPrompt.Equals("0"))
                                collection.Os.UacStatus = "Always Notify";
                            else
                            {
                                collection.Os.UacStatus = "Unknown";
                            }
                        }
                    }

                    //get sus server from registry
                    var susKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate");
                    if (susKey != null)
                    {
                        osInventory.UpdateServer = Convert.ToString(susKey.GetValue("WUServer"));
                        osInventory.SUStargetGroup = Convert.ToString(susKey.GetValue("TargetGroup"));
                    }

                    collection.Os = osInventory;

                }
                catch(Exception ex)
                {
                    Logger.Error("Could Not Parse OS Inventory");
                    Logger.Error(ex.Message);
                }

                try
                {
                    
                    GeoCoordinateWatcher watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
                    
                    watcher.PositionChanged += (sender, e) =>
                    {
                        var coordinate = e.Position.Location;
                        collection.Os.Latitude = coordinate.Latitude.ToString(CultureInfo.InvariantCulture);
                        collection.Os.Longitude = coordinate.Longitude.ToString(CultureInfo.InvariantCulture);
                        collection.Os.LastLocationUpdateUtc = e.Position.Timestamp.DateTime;
                    };

                    watcher.TryStart(true,TimeSpan.FromMilliseconds(10000));
                    collection.Os.LocationEnabled = watcher.Permission == GeoPositionPermission.Granted;


                }
                catch
                {
                    //ignored
                }
              
            }
        }

       
    }
}