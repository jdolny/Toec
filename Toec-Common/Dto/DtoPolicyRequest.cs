using System.Collections.Generic;
using Toec_Common.Entity;
using Toec_Common.Enum;

namespace Toec_Common.Dto
{
    public class DtoPolicyRequest
    {
        public DtoClientIdentity ClientIdentity;

        public DtoPolicyRequest()
        {
            ClientIdentity = new DtoClientIdentity();
            PushURL = string.Format("http://{0}{1}:{2}/", DtoGobalSettings.ClientIdentity.Name, DtoGobalSettings.Domain,
                DtoGobalSettings.RemoteApiPort);
            ClientVersion = DtoGobalSettings.ClientVersion;
            CurrentComServer = DtoGobalSettings.ComServer;
        }

        public EnumPolicy.Trigger Trigger { get; set; }
        public List<EntityUserLogin> UserLogins { get; set; }
        public List<EntityAppMonitor> AppMonitors { get; set; }
        public string ClientVersion { get; set; }
        public string PushURL { get; set; }
        public string CurrentComServer { get; set; }
    }
}