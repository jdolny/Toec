namespace Toec_Common.Inventory
{
    public class DtoWindowsUpdateInventory
    {
        public bool IsInstalled { get; set; }
        public string LastDeploymentChangeTime { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string UpdateId { get; set; }
    }
}