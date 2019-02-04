using System.Collections.Generic;
using Toec_Common.Enum;

namespace Toec_Common.Dto
{
    public class DtoPolicyResult
    {
        public DtoPolicyResult()
        {
            ScriptOutputs = new List<DtoScriptModuleOutput>();
            SkipServerResult = false;
        }

        public bool DeleteCache { get; set; }
        public EnumPolicy.ExecutionType ExecutionType { get; set; }
        public string FailedModuleErrorMessage { get; set; }
        public string FailedModuleExitCode { get; set; }
        public string FailedModuleGuid { get; set; }
        public string FailedModuleName { get; set; }
        public string PolicyGuid { get; set; }
        public string PolicyHash { get; set; }
        public string PolicyName { get; set; }
        public bool SkipServerResult { get; set; }
        public EnumPolicy.Result PolicyResult { get; set; }
        public List<DtoScriptModuleOutput> ScriptOutputs { get; set; }
    }
}