namespace Toec_Common.Dto
{
    public class DtoProcessArgs
    {
        public string Arguments { get; set; }
        public string Command { get; set; }
        public bool RedirectError { get; set; }
        public bool RedirectOutput { get; set; }
        public string RunWith { get; set; }
        public string RunWithArgs { get; set; }
        public int Timeout { get; set; }
        public string WorkingDirectory { get; set; }
    }
}