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

        public static bool JoinDomain(string OU)
        {
            var credentials = new ApiCall.APICall().PolicyApi.GetDomainJoinCredentials();
            if (credentials == null)
            {
                Logger.Debug("Could Not Obtain Credentials To Join The Domain.");
                return false;
            }

            if (string.IsNullOrEmpty(OU))
                OU = null; //set ou to null if it comes through as empty

            Logger.Info("Joining Domain " + credentials.Domain);
            Logger.Debug("Username: " + credentials.Username);
            Logger.Debug("OU: " + OU);
            try
            {
                var resultValue = NetJoinDomain(null, credentials.Domain, OU, credentials.Username, credentials.Password, (JoinOptions.NETSETUP_JOIN_DOMAIN | JoinOptions.NETSETUP_ACCT_CREATE));
                if (resultValue == 0 || resultValue == 2691) //2691 = already joined, return success as to not hold up the policy
                {
                    Logger.Info("Successfully Joined Domain");
                    return true;
                }
                else if(resultValue == 2224)
                {
                    Logger.Info("Computer Already Exists In A Different OU.  Cannot Join To Specified OU");
                    resultValue = NetJoinDomain(null, credentials.Domain, null, credentials.Username, credentials.Password, (JoinOptions.NETSETUP_JOIN_DOMAIN | JoinOptions.NETSETUP_ACCT_CREATE));
                    if (resultValue == 0)
                    {
                        Logger.Info("Successfully Joined Domain");
                        return true;
                    }
                    else
                    {
                        Logger.Error("Domain Join Failed.");
                        Logger.Info("Domain Join Result: " + resultValue);
                        return false;
                    }
                }
                else
                {
                    Logger.Error("Domain Join Failed.");
                    Logger.Info("Domain Join Result: " + resultValue);
                    return false;
                }
                   
                
                
            }
            catch (Exception ex)
            {
                Logger.Error("Domain join failed.");
                Logger.Error(ex.Message);
                return false;
            }
        }



    }
}
