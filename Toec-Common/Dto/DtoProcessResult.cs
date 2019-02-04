namespace Toec_Common.Dto
{
    public class DtoProcessResult
    {
        public int ExitCode { get; set; }
        public string StandardError { get; set; }
        public string StandardOut { get; set; }
    }
}