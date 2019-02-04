using System;

namespace Toec_Common.Inventory
{
    public class DtoAntiVirusWmi
    {
        public const string Query = "select * from AntiVirusProduct";
        public string DisplayName { get; set; }
        public UInt32 ProductState { get; set; }      
    }
}