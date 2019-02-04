using System;
using System.Linq;
using Microsoft.Deployment.WindowsInstaller;
using NetFwTypeLib;

namespace Toec_InstallHelper
{
    public class Firewall
    {
        private readonly string _port;
        private readonly string _remoteAddresses;
        private readonly Session _session;
        private readonly string _skipFirewall;

        public Firewall(Session session)
        {
            _port = session.CustomActionData["REMOTE_API_PORT"];
            _skipFirewall = session.CustomActionData["SKIP_FIREWALL"];
            _remoteAddresses = session.CustomActionData["REMOTE_ADDRESSES"];
            _session = session;
        }

        public ActionResult CreateException()
        {
            _session.Log("Creating Firewall Exception");

            if (!string.IsNullOrEmpty(_skipFirewall))
                if (_skipFirewall.ToLower().Equals("true"))
                    return ActionResult.Success;

            try
            {
                var tNetFwPolicy2 = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
                var fwPolicy2 = (INetFwPolicy2) Activator.CreateInstance(tNetFwPolicy2);
                foreach (
                    var rule in
                        fwPolicy2.Rules.Cast<INetFwRule>().Where(rule => rule.Name.Equals("Toems Push")))
                {
                    fwPolicy2.Rules.Remove(rule.Name);
                }
                var currentProfiles = 7; //All profiles
                var inboundRule = (INetFwRule2) Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
                inboundRule.Enabled = true;
                inboundRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
                inboundRule.Protocol = 6; // TCP
                if (!string.IsNullOrEmpty(_port))
                {
                    int value;
                    if (!int.TryParse(_port, out value))
                        DisplayError("Could Not Parse REMOTEAPIPORT For Firewall Exception");
                    else
                        inboundRule.LocalPorts = value.ToString();
                }
                else
                    inboundRule.LocalPorts = "3913";

                inboundRule.Name = "Toems Push";
                inboundRule.Profiles = currentProfiles;
                if (!string.IsNullOrEmpty(_remoteAddresses))
                    inboundRule.RemoteAddresses = _remoteAddresses;

                var firewallPolicy =
                    (INetFwPolicy2) Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
                firewallPolicy.Rules.Add(inboundRule);
            }
            catch (Exception ex)
            {
                _session.Log("Could Not Create Firewall Exception.");
                _session.Log(ex.Message);
                //don't throw, installer can continue without exception
            }

            _session.Log("Finished Creating Firewall Exception");

            return ActionResult.Success;
        }

        private void DisplayError(string errorMessage)
        {
            _session.Log(errorMessage);
            var record = new Record();
            record.SetString(0, errorMessage);
            _session.Message(InstallMessage.Error, record);
        }
    }
}