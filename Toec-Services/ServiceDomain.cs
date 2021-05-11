using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Toec_Services
{
    public static class ServiceDomain
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [DllImport("netapi32.dll", CharSet = CharSet.Unicode)]
        static extern uint NetJoinDomain(string lpServer, string lpDomain, string lpAccountOU, string lpAccount, string lpPassword,JoinOptions NameType);

        [Flags]
        enum JoinOptions
        {
            NETSETUP_JOIN_DOMAIN = 0x00000001,
            NETSETUP_ACCT_CREATE = 0x00000002,
        }

        public static void JoinDomain(string domain, string OU, string account, string password)
        {
            Logger.Info("Joining Domain " + domain);
            try
            {
                var resultValue = NetJoinDomain(null, domain, OU, account, password, (JoinOptions.NETSETUP_JOIN_DOMAIN | JoinOptions.NETSETUP_ACCT_CREATE));
                Logger.Info("Successfully Joined Domain");
                
            }
            catch (Exception ex)
            {
                Logger.Error("Domain join failed.");
                Logger.Error(ex.Message);
            }
        }



    }
}
