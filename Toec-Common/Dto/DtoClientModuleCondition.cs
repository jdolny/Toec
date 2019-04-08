using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toec_Common.Enum;

namespace Toec_Common.Dto
{
    public class DtoClientModuleCondition
    {
        public DtoClientModuleCondition()
        {
            SuccessCodes = new List<string>();
            RunAs = string.Empty;
        }
        public string Guid { get; set; }
        public string DisplayName { get; set; }
        public string Arguments { get; set; }
        public int Timeout { get; set; }
        public bool RedirectOutput { get; set; }
        public bool RedirectError { get; set; }
        public EnumScriptModule.ScriptType ScriptType { get; set; }
        public List<string> SuccessCodes { get; set; }
        public string WorkingDirectory { get; set; }
        public string RunAs { get; set; }
    }
}
