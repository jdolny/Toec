using System;

namespace Toec_Common.Inventory
{
    public class DtoLogicalVolumeWmi
    {
        public const string Query = "select * from win32_logicaldisk";
        public string DeviceId { get; set; }
        public UInt64 FreeSpace { get; set; }
        public UInt64 Size { get; set; }
    }
}