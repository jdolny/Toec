using System.Collections.Generic;
using Toec_Common.Enum;

namespace Toec_Common.Modules
{
    public class DtoClientScriptModule
    {
        public DtoClientScriptModule()
        {
            SuccessCodes = new List<string>();
        }

        public bool AddToInventory { get; set; }
        public string Arguments { get; set; }
        public string DisplayName { get; set; }
        public string Guid { get; set; }
        public bool IsCondition { get; set; }
        public int Order { get; set; }
        public bool RedirectError { get; set; }
        public bool RedirectOutput { get; set; }
        public EnumScriptModule.ScriptType ScriptType { get; set; }
        public List<string> SuccessCodes { get; set; }
        public int Timeout { get; set; }
        public string WorkingDirectory { get; set; }
        public string RunAs { get; set; }
    }
}