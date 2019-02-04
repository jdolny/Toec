using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Toec_Common.Dto
{
    public class DtoIpInfo
    {
        public IPAddress IpAddress { get; set; }
        public IPAddress Netmask { get; set; }
        public IPAddress Gateway { get; set; }
        public IPAddress Broadcast { get; set; }
    }
}
