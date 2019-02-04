using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using log4net;
using Toec_Services.Entity;

namespace Toec_Services
{
    public class ServicePortSelector
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public string GenerateTrayAppPort()
        {
            var trayPort = GetOpenPort();
            var activeTrayPorts = new ServiceSetting().GetSetting("active_login_ports");

            if (!string.IsNullOrEmpty(trayPort))
            {
                var trayPortsInUse = new List<string>();
                if (string.IsNullOrEmpty(activeTrayPorts.Value))
                {
                    activeTrayPorts.Value = trayPort;
                    new ServiceSetting().UpdateSettingValue(activeTrayPorts);
                }
                else
                {
                    //check if tray ports are still in use
                    trayPortsInUse.AddRange(activeTrayPorts.Value.Split(',').Where(p => IsPortInUse(Convert.ToInt32(p))));

                    var updatedPorts = trayPort + ",";
                    foreach (var p in trayPortsInUse)
                        updatedPorts += p + ",";

                    activeTrayPorts.Value = updatedPorts.Trim(',');
                    new ServiceSetting().UpdateSettingValue(activeTrayPorts);
                }
            }
            return trayPort;
        }

        //https://www.codeproject.com/Tips/268108/Find-the-open-port-on-a-machine-using-Csharp
        private string GetOpenPort()
        {
            int portStartIndex;
            int portEndIndex;
            try
            {
                var portRange = new ServiceSetting().GetSetting("login_port_range").Value;
                var splitPortRange = portRange.Split('-');
                portStartIndex = Convert.ToInt32(splitPortRange[0]);
                portEndIndex = Convert.ToInt32(splitPortRange[1]);
            }
            catch (Exception ex)
            {
                Logger.Error("Could Not Parse Tray App Port Range");
                Logger.Error(ex.Message);
                return null;
            }


            try
            {
                var properties = IPGlobalProperties.GetIPGlobalProperties();
                var tcpEndPoints = properties.GetActiveTcpListeners();

                var usedPorts = tcpEndPoints.Select(p => p.Port).ToList();
                var unusedPort = 0;

                for (var port = portStartIndex; port < portEndIndex; port++)
                {
                    if (usedPorts.Contains(port)) continue;
                    unusedPort = port;
                    break;
                }
                return unusedPort.ToString();
            }
            catch (Exception ex)
            {
                Logger.Error("Could Not Find Unused Port In Range");
                Logger.Error(ex.Message);
                return null;
            }
        
        }

        private bool IsPortInUse(int portNumber)
        {
            var properties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpEndPoints = properties.GetActiveTcpListeners();

            var usedPorts = tcpEndPoints.Select(p => p.Port).ToList();
            return usedPorts.Contains(portNumber);
        }
    }
}