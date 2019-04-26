using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toec_Common.Dto;
using Toec_Services.ApiCall;
using Toec_Services.Entity;
using Toec_Services.Policy.Modules;

namespace Toec_Services.Socket
{
    public class ServiceHubAction
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public void Process(DtoHubAction action)
        {
            Logger.Debug("Received Hub Action");
            Logger.Debug(action.Action);
            switch (action.Action)
            {
                case "Collect_Inventory":
                    var t = new Thread(RunInventory);
                    t.Start();
                    break;

                case "Message":
                    var message = JsonConvert.DeserializeObject<DtoMessage>(action.Message);
                    var activeTrayPorts = new ServiceSetting().GetSetting("active_login_ports").Value;
                    foreach (var port in activeTrayPorts.Split(','))
                    {
                        var localPort = port;
                        var m = new Thread(() => RunMessage(message.Message, message.Title, localPort, message.Timeout));
                        m.Start();
                    }
                    break;

                case "Force_Checkin":
                    var f = new Thread(RunCheckin);
                    f.Start();
                    break;

                case "Reboot":
                    Logger.Info("Server Issued Reboot Request.  Rebooting.");
                    var r = new Thread(() => RunReboot(action.Message));
                    r.Start();
                    break;

                case "Shutdown":
                    Logger.Info("Server Issued Shutdown Request.  Shutting Down.");
                    var s = new Thread(() => RunShutdown(action.Message));
                    s.Start();
                    break;

                case "Start_Mesh":
                    var sm = new Thread(() => RunStartMesh());
                    sm.Start();
                    break;

                case "Wol_Task":
                    var wolTask = JsonConvert.DeserializeObject<DtoWolTask>(action.Message);
                    var w = new Thread(() => RunWakeup(wolTask));
                    w.Start();
                    break;

                default:
                    Logger.Info("Action Was Not Recognized.");
                    break;
            }
        }

        private void RunStartMesh()
        {
            new RemoteAccess.Install();
        }

        private void RunWakeup(DtoWolTask wolTask)
        {
            ServiceWolRelay.WakeUp(wolTask);
        }

        private void RunInventory()
        {
            new ModuleInventory().Run();
        }

        private void RunCheckin()
        {
            new ServiceTriggerAction().Checkin();
        }

        private void RunMessage(string message, string title, string port, int timeout)
        {
            new APICall().TrayApi.SendMessage(message, title, port, timeout);
        }

        private void RunReboot(string delay)
        {
            new ServiceUserTracker().LogoutAllUsers();
            new ServiceAppMonitor().CloseAllOpen();
            System.Diagnostics.Process.Start("shutdown.exe", "/r /t " + delay);
        }

        private void RunShutdown(string delay)
        {
            new ServiceUserTracker().LogoutAllUsers();
            new ServiceAppMonitor().CloseAllOpen();
            System.Diagnostics.Process.Start("shutdown.exe", "/s /t " + delay);
        }
    }
}
