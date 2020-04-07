namespace Toec_Common.Inventory
{
    public class DtoComputerSystemProduct
    {
        public const string Query = "Select * From Win32_ComputerSystemProduct";
        public string UUID { get; set; }
      
    }
}