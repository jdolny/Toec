using System;
using Toec_Common.Enum;

namespace Toec_Common.Dto
{
    public class DtoServerPolicyHistory
    {
        public int ComputerId { get; set; }

        public string Hash { get; set; }

        public int Id { get; set; }

        public DateTime LastRunTime { get; set; }

        public string PolicyGuid { get; set; }

        public int PolicyId { get; set; }

        public string User { get; set; }

        public EnumPolicyHistory.RunResult Result { get; set; }
    }
}