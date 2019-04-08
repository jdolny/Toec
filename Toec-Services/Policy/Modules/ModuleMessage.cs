using System.Reflection;
using System.Threading;
using log4net;
using Toec_Common.Dto;
using Toec_Common.Modules;
using Toec_Services.ApiCall;
using Toec_Services.Entity;

namespace Toec_Services.Policy.Modules
{
    public class ModuleMessage
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        private readonly DtoClientMessageModule _module;
        private readonly DtoModuleResult _moduleResult;

        public ModuleMessage(DtoClientMessageModule module)
        {
            _moduleResult = new DtoModuleResult();
            _moduleResult.Name = module.DisplayName;
            _moduleResult.Guid = module.Guid;
            _moduleResult.Success = true;
            _module = module;
        }

        public DtoModuleResult Run()
        {
            Logger.Info("Running Message Module: " + _module.DisplayName);

            var activeTrayPorts = new ServiceSetting().GetSetting("active_login_ports").Value;
            foreach (var port in activeTrayPorts.Split(','))
            {
                var localPort = port;
                var t = new Thread(() => RunMessage(_module.Message, _module.Title, localPort, _module.Timeout));
                t.Start();
            }

            Logger.Info($"Message Module {_module.DisplayName} Completed");
            return _moduleResult;
        }

        private void RunMessage(string message, string title, string port, int timeout)
        {
            new APICall().TrayApi.SendMessage(message, title, port, timeout);
        }
    }
}