using System;
using System.Collections.Generic;
using System.IO;
using Toec_Common.Dto;
using Toec_Common.Enum;

namespace Toec_Common.Modules
{
    public class DtoClientWinPeModule
    {
        public DtoClientWinPeModule()
        {
           
            Files = new List<DtoClientFileHash>();
        }

        public string Destination { get; set; }
        public string DisplayName { get; set; }
        public List<DtoClientFileHash> Files { get; set; }
        public string Guid { get; set; }
        public int Order { get; set; }
        public EnumCondition.FailedAction ConditionFailedAction { get; set; }
        public int ConditionNextOrder { get; set; }
        public DtoClientModuleCondition Condition { get; set; }
    }
}