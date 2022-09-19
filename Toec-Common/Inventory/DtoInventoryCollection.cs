using System.Collections.Generic;
using Toec_Common.Dto;

namespace Toec_Common.Inventory
{
    public class DtoInventoryCollection
    {
        public DtoInventoryCollection()
        {
            WindowsUpdates = new List<DtoWindowsUpdateInventory>();
            Software = new List<DtoSoftwareInventory>();
            Gpu = new List<DtoVideoControllerInventory>();
            HardDrives = new List<DtoHardDriveInventory>();
            Printers = new List<DtoPrinterInventory>();
            NetworkAdapters = new List<DtoNicInventory>();
            AntiVirus = new List<DtoAntiVirusInventory>();
            ClientVersion = DtoGobalSettings.ClientVersion;
            Firewall = new DtoFirewallInventory();
            Bitlocker = new List<DtoBitlockerInventory>();
            LogicalVolume = new List<DtoLogicalVolumeInventory>();
            Certificates = new List<DtoCertificateInventory>();
            PushUrl = string.Format("http://{0}{1}:{2}/", DtoGobalSettings.ClientIdentity.Name, DtoGobalSettings.Domain,
                DtoGobalSettings.RemoteApiPort);
        }

        public DtoBiosInventory Bios { get; set; }
        public string ClientVersion { get; set; }
        public DtoComputerSystemInventory ComputerSystem { get; set; }
        public List<DtoVideoControllerInventory> Gpu { get; set; }
        public List<DtoHardDriveInventory> HardDrives { get; set; }
        public List<DtoNicInventory> NetworkAdapters { get; set; }
        public DtoOsInventory Os { get; set; }
        public List<DtoPrinterInventory> Printers { get; set; }
        public DtoProcessorInventory Processor { get; set; }
        public string PushUrl { get; set; }
        public List<DtoSoftwareInventory> Software { get; set; }
        public List<DtoWindowsUpdateInventory> WindowsUpdates { get; set; }
        public List<DtoAntiVirusInventory> AntiVirus { get; set; }
        public DtoFirewallInventory Firewall { get; set; }
        public List<DtoBitlockerInventory> Bitlocker { get; set; }
        public List<DtoLogicalVolumeInventory> LogicalVolume { get; set; } 
        public List<DtoCertificateInventory> Certificates { get; set; }
        public string HardwareUUID { get; set; }
    }
}