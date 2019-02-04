using System;
using Toec_Common.Inventory;

namespace Toec_Services.InventorySearchers
{
    public class LogicalVolume : IInventorySearcher
    {
        public void Search(DtoInventoryCollection collection)
        {
            using (var wmi = new ServiceWmi<DtoLogicalVolumeWmi>(new DtoLogicalVolumeWmi()))
            {
                var volumes = wmi.GetObjectList();
                foreach (var volume in volumes)
                {
                    var lvInventory = new DtoLogicalVolumeInventory();
                    try
                    {
                        string free = (volume.FreeSpace / 1024 / 1024 / 1024).ToString();
                        string size = (volume.Size / 1024 / 1024 / 1024).ToString();
                        var freePercent = Math.Round((Convert.ToDouble(free)/Convert.ToDouble(size) * 100));

                        lvInventory.Drive = volume.DeviceId;
                        lvInventory.FreeSpacePercent = Convert.ToInt32(freePercent);
                        lvInventory.FreeSpaceGB = Convert.ToInt32(free);
                        lvInventory.SizeGB = Convert.ToInt32(size);
                        collection.LogicalVolume.Add(lvInventory);

                    }
                    catch
                    {
                        //ignored
                        
                    }
                }
            }
        }
    }
}