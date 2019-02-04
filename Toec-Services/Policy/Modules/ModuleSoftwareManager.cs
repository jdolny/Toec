using System.IO;
using System.Reflection;
using log4net;
using Newtonsoft.Json;
using Toec_Common.Dto;
using Toec_Common.Enum;
using Toec_Common.Modules;

namespace Toec_Services.Policy.Modules
{
    public class ModuleSoftwareManager
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly DtoClientSoftwareModule _module;

        private readonly DtoModuleResult _moduleResult;

        public ModuleSoftwareManager(DtoClientSoftwareModule module)
        {
            _moduleResult = new DtoModuleResult();
            _moduleResult.Name = module.DisplayName;
            _moduleResult.Guid = module.Guid;
            _moduleResult.Success = true;
            _module = module;
        }

        public DtoModuleResult Run()
        {
            var directory = Path.Combine(DtoGobalSettings.BaseCachePath, _module.Guid) + Path.DirectorySeparatorChar;
            Logger.Info("Running Software Module: " + _module.DisplayName);

            string runWith = "msiexec.exe";
            string runWithArgs = "";
            switch (_module.InstallType)
            {
                case EnumSoftwareModule.MsiInstallType.Install:
                    runWithArgs = " /i ";
                    break;
                case EnumSoftwareModule.MsiInstallType.Uninstall:
                    runWithArgs = " /x ";
                    break;
                case EnumSoftwareModule.MsiInstallType.Patch:
                    runWithArgs = " /p ";
                    break;
            }

            if (!string.IsNullOrEmpty(_module.RunAs))
            {
                var iTask = new ServiceImpersonationTask();
                iTask.Command = runWith;
                iTask.Arguments = runWithArgs + "\"" + directory + _module.Command + "\"" + " " + _module.Arguments;
                iTask.ExecutionTimeout = _module.Timeout;
                iTask.ModuleGuid = _module.Guid;
                iTask.ImpersonationGuid = _module.RunAs;

                var result = iTask.RunTask();
                Logger.Info(JsonConvert.SerializeObject(result));
                Logger.Info("Software Module: " + _module.DisplayName + " Finished");
                _moduleResult.ExitCode = result.ToString();
                if (!_module.SuccessCodes.Contains(result.ToString()))
                {
                    _moduleResult.Success = false;
                    if(result == 259)
                        _moduleResult.ErrorMessage = "Task Timed Out.";
                    else
                        _moduleResult.ErrorMessage = "Impersonation Task Failed.  See Log For Details.";
                }
            }
            else
            {
                var pArgs = new DtoProcessArgs();
                pArgs.RunWith = runWith;
                pArgs.RunWithArgs = runWithArgs;
                pArgs.Command = "\"" + directory + _module.Command + "\"";
                pArgs.Arguments = _module.Arguments;
                pArgs.Timeout = _module.Timeout;
                pArgs.WorkingDirectory = directory;
                pArgs.RedirectError = _module.RedirectError;
                pArgs.RedirectOutput = _module.RedirectOutput;

                var result = new ServiceProcess(pArgs).RunProcess();
                Logger.Info(JsonConvert.SerializeObject(result));
                Logger.Info("Software Module: " + _module.DisplayName + "Finished");

                _moduleResult.ExitCode = result.ExitCode.ToString();
                if (!_module.SuccessCodes.Contains(result.ExitCode.ToString()))
                {
                    _moduleResult.Success = false;
                    _moduleResult.ErrorMessage = result.StandardError;
                }
            }

            return _moduleResult;
        }
    }
}