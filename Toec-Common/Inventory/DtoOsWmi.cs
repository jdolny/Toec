namespace Toec_Common.Inventory
{
    public class DtoOsWmi
    {
        public const string Query = "Select * From Win32_OperatingSystem";
        public string BuildNumber { get; set; }
        public string Caption { get; set; }
        public string OSArchitecture { get; set; }
        public ushort ServicePackMajorVersion { get; set; }
        public ushort ServicePackMinorVersion { get; set; }
        public string Version { get; set; }
    }
}