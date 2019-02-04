using System.Collections.Generic;
using Toec_Common.Enum;

namespace Toec_Common.Modules
{
    public class DtoClientSoftwareModule
    {
        public DtoClientSoftwareModule()
        {
            Files = new List<DtoClientFileHash>();
            SuccessCodes = new List<string>();
        }

        public string Arguments { get; set; }
        public string Command { get; set; }
        public string DisplayName { get; set; }
        public List<DtoClientFileHash> Files { get; set; }
        public string Guid { get; set; }
        public EnumSoftwareModule.MsiInstallType InstallType { get; set; }
        public int Order { get; set; }
        public bool RedirectError { get; set; }
        public bool RedirectOutput { get; set; }
        public List<string> SuccessCodes { get; set; }
        public int Timeout { get; set; }
        public string RunAs { get; set; }
    }
}