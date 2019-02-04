using System.IO;
using System.Reflection;
using log4net;
using Newtonsoft.Json;
using Toec_Common.Dto;
using Toec_Common.Modules;

namespace Toec_Services.Policy.Modules
{
    internal class ModuleCommandManager
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly DtoClientCommandModule _module;

        private readonly DtoModuleResult _moduleResult;

        public ModuleCommandManager(DtoClientCommandModule module)
        {
            _moduleResult = new DtoModuleResult();
            _moduleResult.Name = module.DisplayName;
            _moduleResult.Guid = module.Guid;
            _moduleResult.Success = true;
            _module = module;
        }

        public DtoModuleResult Run()
        {
            Logger.Info("Running Command Module: " + _module.DisplayName);
            if (!string.IsNullOrEmpty(_module.RunAs))
            {
                var iTask = new ServiceImpersonationTask();
                iTask.Command = "cmd.exe";
                if (_module.Command.StartsWith("[unquote]"))
                {
                    _module.Command = _module.Command.Replace("[unquote]", "");
                    iTask.Arguments = "/c " + _module.Command + " " + _module.Arguments;
                }
                else
                {
                    iTask.Arguments = "/c " + "\"" + _module.Command + "\"" + " " + _module.Arguments;
                }
                iTask.ExecutionTimeout = _module.Timeout;
                iTask.ModuleGuid = _module.Guid;
                if (!string.IsNullOrEmpty(_module.WorkingDirectory))
                    iTask.WorkingDirectory = _module.WorkingDirectory;
                else
                    iTask.WorkingDirectory = Path.Combine(DtoGobalSettings.BaseCachePath, _module.Guid);
                iTask.ImpersonationGuid = _module.RunAs;
                var result = iTask.RunTask();
                Logger.Info(JsonConvert.SerializeObject(result));
                Logger.Info("Command Module: " + _module.DisplayName + " Finished");
                _moduleResult.ExitCode = result.ToString();
                if (!_module.SuccessCodes.Contains(result.ToString()))
                {
                    _moduleResult.Success = false;
                    if (result == 259)
                        _moduleResult.ErrorMessage = "Task Timed Out.";
                    else
                        _moduleResult.ErrorMessage = "Impersonation Task Failed.  See Log For Details.";
                }
            }
            else
            {
                var pArgs = new DtoProcessArgs();
                pArgs.RunWith = "cmd.exe";
                pArgs.RunWithArgs = "/c ";
                if (_module.Command.StartsWith("[unquote]"))
                {
                    _module.Command = _module.Command.Replace("[unquote]", "");
                    pArgs.Command = _module.Command;
                }
                else
                {
                    pArgs.Command = "\"" + _module.Command + "\"";
                }
                pArgs.Arguments = _module.Arguments;
                pArgs.Timeout = _module.Timeout;
                pArgs.RedirectError = _module.RedirectError;
                pArgs.RedirectOutput = _module.RedirectOutput;
                if (!string.IsNullOrEmpty(_module.WorkingDirectory))
                    pArgs.WorkingDirectory = _module.WorkingDirectory;
                else
                    pArgs.WorkingDirectory = Path.Combine(DtoGobalSettings.BaseCachePath, _module.Guid);
               
                var result = new ServiceProcess(pArgs).RunProcess();
                Logger.Info(JsonConvert.SerializeObject(result));
                Logger.Info("Command Module: " + _module.DisplayName + " Finished");
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