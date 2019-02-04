using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using Toec_Common.Inventory;

namespace Toec_Services.InventorySearchers
{
    public class Printer : IInventorySearcher
    {
        public void Search(DtoInventoryCollection collection)
        {
            using (var wmi = new ServiceWmi<DtoPrinterInventory>(new DtoPrinterInventory()))
            {
                collection.Printers = wmi.GetObjectList();
            }

            //Per computer printers aren't included in wmi when ran as the system account
            //add them via registry
            var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Print\Connections");
            if (key == null) return;
            foreach (var subKey in key.GetSubKeyNames())
            {
                var printerConnection = key.OpenSubKey(subKey);
                if (printerConnection == null) continue;
                var printerPath = Convert.ToString(printerConnection.GetValue("Printer"));
                var printerName = printerPath.Split('\\').Last();
                var server = Convert.ToString(printerConnection.GetValue("Server"));
                collection.Printers.Add(new DtoPrinterInventory
                {
                    SystemName = server,
                    Name = printerPath,
                    ShareName = printerName
                });
            }

            //collection.Printers = collection.Printers.GroupBy(x => new {x.Name, x.DriverName}).Select(g => g.First()).ToList();
        }

        //This is called from the printer install module to verify the printer installed
        public List<DtoPrinterInventory> GetInstalledPrinters()
        {
            var printers = new List<DtoPrinterInventory>();
            using (var wmi = new ServiceWmi<DtoPrinterInventory>(new DtoPrinterInventory()))
            {
                printers = wmi.GetObjectList();
            }

            var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Print\Connections");
            if (key == null) return printers;
            foreach (var subKey in key.GetSubKeyNames())
            {
                var printerConnection = key.OpenSubKey(subKey);
                if (printerConnection == null) continue;
                var printerPath = Convert.ToString(printerConnection.GetValue("Printer"));
                var printerName = printerPath.Split('\\').Last();
                var server = Convert.ToString(printerConnection.GetValue("Server"));
                printers.Add(new DtoPrinterInventory
                {
                    SystemName = server,
                    Name = printerPath,
                    ShareName = printerName
                });
            }

            return printers;
        }

        public List<DtoPrinterInventory> GetInstalledPrintersWmiOnly()
        {
            var printers = new List<DtoPrinterInventory>();
            using (var wmi = new ServiceWmi<DtoPrinterInventory>(new DtoPrinterInventory()))
            {
                printers = wmi.GetObjectList();
            }
            return printers;
        }
    }
}