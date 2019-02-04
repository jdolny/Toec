using System.Collections.Generic;
using Toec_Common.Enum;

namespace Toec_Common.Dto
{
    public class DtoProvisionResponse
    {
        public DtoProvisionResponse()
        {
            ComServers = new List<DtoClientComServers>();
        }

        public string Certificate { get; set; }
        public string ComputerIdentifier { get; set; }
        public List<DtoClientComServers> ComServers { get; set; }
        public string Message { get; set; }
        public EnumProvisionStatus.Status ProvisionStatus { get; set; }
    }
}