using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toec_Common.Dto;
using Toec_Common.Enum;

namespace Toec_Common.Modules
{
    public class DtoClientWingetModule
    {
        public string Guid { get; set; }
        public string DisplayName { get; set; }
        public string Name { get; set; }
        public string PackageId { get; set; }
        public string PackageVersion { get; set; }
        public string Arguments { get; set; }
        public string Override { get; set; }
        public bool InstallLatest { get; set; }
        public bool KeepUpdated { get; set; }
        public EnumWingetInstallType.WingetInstallType InstallType { get; set; }
        public int Order { get; set; }
        public int Timeout { get; set; }
        public bool RedirectOutput { get; set; }
        public bool RedirectError { get; set; }
        public string RunAs { get; set; }
        public EnumCondition.FailedAction ConditionFailedAction { get; set; }
        public int ConditionNextOrder { get; set; }
        public DtoClientModuleCondition Condition { get; set; }


    }
}
