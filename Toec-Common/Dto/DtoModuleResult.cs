namespace Toec_Common.Dto
{
    public class DtoModuleResult
    {
        public string ErrorMessage { get; set; }
        public string ExitCode { get; set; }
        public string Guid { get; set; }
        public string Name { get; set; }
        public DtoScriptModuleOutput ScriptOutput { get; set; }
        public bool Success { get; set; }
    }
}