using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Toec_Common.Inventory;

namespace Toec_Services.InventorySearchers
{
    public class Nic : IInventorySearcher
    {
        public void Search(DtoInventoryCollection collection)
        {
            foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                try
                {
                    var nic = new DtoNicInventory();
                    nic.Name = adapter.Name;
                    nic.Description = adapter.Description;
                    nic.Type = adapter.NetworkInterfaceType.ToString();
                    nic.Mac = adapter.GetPhysicalAddress().ToString();
                    nic.Status = adapter.OperationalStatus.ToString();
                    nic.Speed = adapter.Speed;
                    nic.Ips = string.Empty;
                    nic.Gateways = string.Empty;

                    foreach (
                        var ip in
                            adapter.GetIPProperties()
                                .UnicastAddresses.Where(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork))
                    {
                        nic.Ips += ip.Address + ",";
                    }
                    nic.Ips = nic.Ips.Trim(',');

                    foreach (
                      var gateway in
                          adapter.GetIPProperties()
                              .GatewayAddresses.Where(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork))
                    {
                        nic.Gateways += gateway.Address + ",";
                    }
                    nic.Gateways = nic.Gateways.Trim(',');

                    collection.NetworkAdapters.Add(nic);
                }
                catch
                {
                    //Ignore
                }
            }
        }
    }
}