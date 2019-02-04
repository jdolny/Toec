namespace Toec_Common.Dto
{
    public class DtoTrayAppStartupInfo
    {
        public string LogLevel { get; set; }
        public bool ServiceStarted { get; set; }
        public string TrayAppPort { get; set; }
        public string ComServer { get; set; }
        public string ComputerName { get; set; }
        public string ComputerGuid { get; set; }
        public int ShutdownDelay { get; set; }
    }
}