using System.Collections.Generic;
using Toec_Common.Modules;

namespace Toec_Common.Dto
{
    public class DtoTriggerResponse
    {
        public DtoTriggerResponse()
        {
            Policies = new List<DtoClientPolicy>();
        }

        public int CheckinTime { get; set; }
        public int ShutdownDelay { get; set; }
        public List<DtoClientPolicy> Policies { get; set; }
        public bool UserLoginsSubmitted { get; set; }
        public bool AppMonitorSubmitted { get; set; }
    }
}