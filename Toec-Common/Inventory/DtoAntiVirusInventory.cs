namespace Toec_Common.Inventory
{
    public class DtoAntiVirusInventory
    {
        public string DisplayName { get; set; }
        public string Provider { get; set; }
        public string RealtimeScanner { get; set; }
        public string DefinitionStatus { get; set; }
        public int ProductState { get; set; }
    }
}