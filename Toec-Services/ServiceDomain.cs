using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Toec_Services
{
    public static class ServiceDomain
    {
        [DllImport("netapi32.dll", CharSet = CharSet.Unicode)]
        static extern uint NetJoinDomain(string lpServer, string lpDomain, string lpAccountOU, string lpAccount, string lpPassword,JoinOptions NameType);

        [Flags]
        enum JoinOptions
        {
            NETSETUP_JOIN_DOMAIN = 0x00000001,
            NETSETUP_ACCT_CREATE = 0x00000002,
            NETSETUP_ACCT_DELETE = 0x00000004,
            NETSETUP_WIN9X_UPGRADE = 0x00000010,
            NETSETUP_DOMAIN_JOIN_IF_JOINED = 0x00000020,
            NETSETUP_JOIN_UNSECURE = 0x00000040,
            NETSETUP_MACHINE_PWD_PASSED = 0x00000080,
            NETSETUP_DEFER_SPN_SET = 0x10000000
        }

        public static uint JoinDomain(string server, string domain, string OU, string account, string password)
        {
            try
            {
                uint value1 = NetJoinDomain(server, domain, OU, account, password, (JoinOptions.NETSETUP_JOIN_DOMAIN | JoinOptions.NETSETUP_DOMAIN_JOIN_IF_JOINED | JoinOptions.NETSETUP_ACCT_CREATE));
                return value1;
            }
            catch (Exception ex)
            {
                return 11;
            }
        }



    }
}
