using Toec_Common.Dto;
using Toec_Common.Enum;

namespace Toec_Common.Modules
{
    public class DtoClientPrinterModule
    {
        public string DisplayName { get; set; }
        public string Guid { get; set; }
        public bool IsDefault { get; set; }
        public int Order { get; set; }
        public EnumPrinterModule.ActionType PrinterAction { get; set; }
        public string PrinterPath { get; set; }
        public bool RestartSpooler { get; set; }
        public bool WaitForEnumeration { get; set; }
        public EnumCondition.FailedAction ConditionFailedAction { get; set; }
        public int ConditionNextOrder { get; set; }
        public DtoClientModuleCondition Condition { get; set; }
    }
}