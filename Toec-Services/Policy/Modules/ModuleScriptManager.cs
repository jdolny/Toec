using System;
using System.IO;
using System.Reflection;
using log4net;
using Newtonsoft.Json;
using Toec_Common.Dto;
using Toec_Common.Enum;
using Toec_Common.Modules;

namespace Toec_Services.Policy.Modules
{
    public class ModuleScriptManager
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly DtoClientScriptModule _module;

        private readonly DtoModuleResult _moduleResult;

        public ModuleScriptManager(DtoClientScriptModule module)
        {
            _moduleResult = new DtoModuleResult();
            _moduleResult.Name = module.DisplayName;
            _moduleResult.Guid = module.Guid;
            _moduleResult.Success = true;
            _module = module;
        }

        private string GetLastStdOutLine(string stdOut)
        {
            try
            {
                var lines = stdOut.Split(
                    new[] {"\r\n", "\r", "\n"},
                    StringSplitOptions.None
                    );

                for (var i = lines.Length - 1; i >= 0; i--)
                {
                    if (!string.IsNullOrEmpty(lines[i]))
                        return lines[i];
                }
                return null;
            }
            catch
            {
                Logger.Error("Could Not Determine Custom Inventory Result Line");
                return null;
            }
        }

        public DtoModuleResult Run()
        {
            Logger.Info("Running Script Module: " + _module.DisplayName);

            string runWith = "";
            string runWithArgs = "";
            string ext;
            if (_module.ScriptType == EnumScriptModule.ScriptType.Powershell)
            {
                runWith = "Powershell.exe";
                runWithArgs = " -ExecutionPolicy Bypass -NoProfile -File ";
                ext = ".ps1";
            }
            else if (_module.ScriptType == EnumScriptModule.ScriptType.VbScript)
            {
                runWith = "cscript.exe";
                ext = ".vbs";
            }
            else if (_module.ScriptType == EnumScriptModule.ScriptType.Batch)
            {
                runWith = "cmd.exe";
                runWithArgs = " /c ";
                ext = ".bat";
            }
            else
            {
                var msg = "Could Not Determine Script Type.  Exiting.";
                Logger.Error(msg);
                _moduleResult.Success = false;
                _moduleResult.ExitCode = "1";
                _moduleResult.ErrorMessage = msg;
                return _moduleResult;
            }

            if (!string.IsNullOrEmpty(_module.RunAs))
            {
                var iTask = new ServiceImpersonationTask();
                iTask.Command = runWith;
                iTask.Arguments = runWithArgs + "\"" + Path.Combine(DtoGobalSettings.BaseCachePath, _module.Guid, _module.Guid + ext) +
                                  "\"" + " " + _module.Arguments;
                iTask.ExecutionTimeout = _module.Timeout;
                iTask.ModuleGuid = _module.Guid;
                if (!string.IsNullOrEmpty(_module.WorkingDirectory))
                    iTask.WorkingDirectory = _module.WorkingDirectory;
                else
                    iTask.WorkingDirectory = Path.Combine(DtoGobalSettings.BaseCachePath, _module.Guid);
                iTask.ImpersonationGuid = _module.RunAs;
                var result = iTask.RunTask();
                Logger.Info(JsonConvert.SerializeObject(result));
                Logger.Info("Script Module: " + _module.DisplayName + " Finished");
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
                pArgs.RunWith = runWith;
                pArgs.RunWithArgs = runWithArgs;
                pArgs.Command = "\"" + Path.Combine(DtoGobalSettings.BaseCachePath, _module.Guid, _module.Guid + ext) +
                                "\"";
                pArgs.Arguments = _module.Arguments;
                pArgs.Timeout = _module.Timeout;
                pArgs.RedirectError = _module.RedirectError;
                pArgs.RedirectOutput = _module.RedirectOutput;
                if (!string.IsNullOrEmpty(_module.WorkingDirectory))
                    pArgs.WorkingDirectory = _module.WorkingDirectory;
                else
                    pArgs.WorkingDirectory = Path.Combine(DtoGobalSettings.BaseCachePath, _module.Guid);
                if (_module.AddToInventory)
                    pArgs.RedirectOutput = true;
                var result = new ServiceProcess(pArgs).RunProcess();
                Logger.Info(JsonConvert.SerializeObject(result));

                if (_module.AddToInventory)
                {
                    var resultLine = GetLastStdOutLine(result.StandardOut);
                    if (!string.IsNullOrEmpty(resultLine))
                    {
                        _moduleResult.ScriptOutput = new DtoScriptModuleOutput();
                        _moduleResult.ScriptOutput.ModuleGuid = _module.Guid;
                        _moduleResult.ScriptOutput.Value = resultLine;
                    }
                }

                Logger.Info("Script Module: " + _module.DisplayName + " Finished");

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