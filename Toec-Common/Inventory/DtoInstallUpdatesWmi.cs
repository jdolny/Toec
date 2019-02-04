namespace Toec_Common.Inventory
{
    public class DtoInstalledUpdatesWmi
    {
        public const string Query = "select * from Win32_QuickfixEngineering";
        public string HotFixID { get; set; }
        public string InstalledOn { get; set; }      
    }
}