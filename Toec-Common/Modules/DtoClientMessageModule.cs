using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toec_Common.Dto;
using Toec_Common.Enum;

namespace Toec_Common.Modules
{
    public class DtoClientMessageModule
    {
        public string Guid { get; set; }
        public string DisplayName { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public int Timeout { get; set; }
        public int Order { get; set; }
        public EnumCondition.FailedAction ConditionFailedAction { get; set; }
        public int ConditionNextOrder { get; set; }
        public DtoClientModuleCondition Condition { get; set; }
    }
}
