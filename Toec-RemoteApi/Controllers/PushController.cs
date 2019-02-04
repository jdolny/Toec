using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Web.Http;
using log4net;
using Toec_Common.Dto;
using Toec_RemoteApi.Controllers.Authorization;
using Toec_Services;
using Toec_Services.ApiCall;
using Toec_Services.Entity;
using Toec_Services.Policy.Modules;

namespace Toec_RemoteApi.Controllers
{
    public class PushController : ApiController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [SignatureAuth]
        [HttpGet]
        public DtoStringResponse GetLoggedInUsers()
        {
            var listUsers = new ServiceUserLogins().GetUsersLoggedIn();
            var users = string.Empty;
            foreach (var user in listUsers)
            {
                users += user + ",";
            }
            return new DtoStringResponse() {Value = users.Trim(',')};
        }

        [SignatureAuth]
        [HttpGet]
        public DtoBoolResponse GetStatus()
        {
            return new DtoBoolResponse { Value = true };
        }

        [SignatureAuth]
        [HttpPost]
        public DtoBoolResponse Reboot(DtoStringResponse delay)
        {
            Logger.Info("Server Issued Reboot Request.  Rebooting.");
            var t = new Thread(() => RunReboot(delay.Value));
            t.Start();
            return new DtoBoolResponse { Value = true };
        }

        [SignatureAuth]
        [HttpPost]
        public DtoBoolResponse Shutdown(DtoStringResponse delay)
        {
            Logger.Info("Server Issued Shutdown Request.  Shutting Down.");
            var t = new Thread(() => RunShutdown(delay.Value));
            t.Start();
            return new DtoBoolResponse { Value = true };
        }

        [SignatureAuth]
        [HttpGet]
        public DtoBoolResponse Checkin()
        {
            var t = new Thread(RunCheckin);
            t.Start();
            return new DtoBoolResponse {Value = true};
        }

        [SignatureAuth]
        [HttpGet]
        public DtoBoolResponse Inventory()
        {
            var t = new Thread(RunInventory);
            t.Start();
            return new DtoBoolResponse { Value = true };
        }

        [SignatureAuth]
        [HttpPost]
        public DtoBoolResponse Message(DtoMessage message)
        {
            var activeTrayPorts = new ServiceSetting().GetSetting("active_login_ports").Value;
            foreach (var port in activeTrayPorts.Split(','))
            {
                var localPort = port;
                var t = new Thread(() => RunMessage(message.Message, message.Title, localPort,message.Timeout));
                t.Start();
            }
            return new DtoBoolResponse {Value = true};
        }

        [SignatureAuth]
        [HttpPost]
        public DtoBoolResponse WolTask(DtoWolTask wolTask)
        {
            var t = new Thread(() => RunWakeup(wolTask));
            t.Start();
            return new DtoBoolResponse() { Value = true };
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

        private void RunMessage(string message,string title, string port, int timeout)
        {
            new APICall().TrayApi.SendMessage(message,title, port,timeout);
        }

        private void RunReboot(string delay)
        {
            new ServiceUserTracker().LogoutAllUsers();
            new ServiceAppMonitor().CloseAllOpen();
            Process.Start("shutdown.exe", "/r /t " + delay);
        }

        private void RunShutdown(string delay)
        {
            new ServiceUserTracker().LogoutAllUsers();
            new ServiceAppMonitor().CloseAllOpen();
            Process.Start("shutdown.exe", "/s /t " + delay);
        }
    }
}