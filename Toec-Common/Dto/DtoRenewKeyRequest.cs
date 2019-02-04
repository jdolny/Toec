namespace Toec_Common.Dto
{
    public class DtoRenewKeyRequest
    {
        public string DeviceCert { get; set; }
        public string Guid { get; set; }
        public string Name { get; set; }
        public string SymmKey { get; set; }
    }
}