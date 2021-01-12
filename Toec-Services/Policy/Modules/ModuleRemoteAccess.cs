using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Toec_Common.Dto;
using Toec_Common.Enum;
using Toec_Common.Modules;

namespace Toec_Services.Policy.Modules
{
    public class ModuleRemoteAccess
    {
        private static readonly ILog Logger =
          LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private DtoClientPolicy _policy;
        private const string _moduleGuid = "99999999-9999-9999-9999-999999999999";

        public ModuleRemoteAccess(DtoClientPolicy policy)
        {
            _policy = policy;
        }

        public void Run()
        {
            Logger.Info("Running Remote Access Module: ");
            if ((_policy.RemoteAccess == EnumPolicy.RemoteAccess.Enabled && !new ServiceSystemService().IsRemotelyInstalled()) ||
                _policy.RemoteAccess == EnumPolicy.RemoteAccess.ForceReinstall)
            {
                Logger.Info("Installing Remote Access Service");
                var installArgs = new ApiCall.APICall().PolicyApi.GetRemotelyInstallArgs();
                if(string.IsNullOrEmpty(installArgs))
                {
                    Logger.Error("Could Not Get Install Args");
                    return;
                }
                if(installArgs.Contains("Error"))
                {
                    Logger.Error("Could Not Get Install Args.  " + installArgs);
                    return;
                }
                var file = string.Empty;
                if (Environment.Is64BitOperatingSystem)
                    file = "Remotely-Win10-x64.zip";
                else
                    file = "Remotely-Win10-x86.zip";

                var module = new DtoClientCommandModule();
                module.Guid = "99999999-9999-9999-9999-999999999999";
                module.Command = Path.Combine(DtoGobalSettings.BaseCachePath, module.Guid, "Remotely_Installer.exe");
                module.DisplayName = "Install Remotely";
                module.Timeout = 5;
                module.RedirectError = true;
                module.RedirectOutput = true;
                module.SuccessCodes = new List<string>() { "0" };
                module.Arguments = $"{installArgs} -path '{Path.Combine(DtoGobalSettings.BaseCachePath, module.Guid,file)}'";
                var result = new ModuleCommandManager(module).Run();
                if(!result.Success)
                    Logger.Error(result.ErrorMessage);

                if (!new ServiceSystemService().DisableRemotelyServiceStartup())
                    Logger.Error("Could Not Change Remotely Startup Type");
                
                var connectionInfo = new ServiceFileSystem().ReadRemotelyConnectionFile();
                if(connectionInfo != null)
                {
                    var res = new ApiCall.APICall().PolicyApi.UpdateRemoteAccessId(connectionInfo);
                    if (res != null)
                    {
                        if(!res.Success)
                            Logger.Error("Could Not Update Client Remote Access Id");
                    }
                    
                }
                return;
                    
            }
            if (_policy.RemoteAccess == EnumPolicy.RemoteAccess.Enabled && new ServiceSystemService().IsRemotelyInstalled())
            {
                Logger.Debug("Remote Access Already Installed. Updating Remote Access Device Id");
                var connectionInfo = new ServiceFileSystem().ReadRemotelyConnectionFile();
                if (connectionInfo != null)
                {
                    var res = new ApiCall.APICall().PolicyApi.UpdateRemoteAccessId(connectionInfo);
                    if (res != null)
                    {
                        if (!res.Success)
                            Logger.Error("Could Not Update Client Remote Access Id");
                    }
                }
                return;
            }

            if (_policy.RemoteAccess == EnumPolicy.RemoteAccess.Disabled && new ServiceSystemService().IsRemotelyInstalled())
            {
                Logger.Info("Removing Remote Access Service");
                var module = new DtoClientCommandModule();
                module.Command = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Remotely", "Remotely_Installer.exe");
                module.Guid = "99999999-9999-9999-9999-999999999999";
                module.DisplayName = "Remove Remotely";
                module.Timeout = 5;
                module.RedirectError = true;
                module.RedirectOutput = true;
                module.SuccessCodes = new List<string>() { "0" };
                module.WorkingDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Remotely");
                module.Arguments = "-uninstall -quiet";

                //remove remote access id from server
                var res = new ApiCall.APICall().PolicyApi.UpdateRemoteAccessId(new RemotelyConnectionInfo() { DeviceID = "", Host = "", OrganizationID = "", ServerVerificationToken ="" });
                if (res != null)
                {
                    if (!res.Success)
                        Logger.Error("Could Not Remmove Client Remote Access Id");
                }

                var result = new ModuleCommandManager(module).Run();
                if (!result.Success)
                    Logger.Error(result.ErrorMessage);
                return;
            }
        }
    }
}
