using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using log4net;
using Newtonsoft.Json;
using Toec_Common.Dto;
using Toec_Common.Enum;
using Toec_Common.Modules;
using Toec_Services.ApiCall;

namespace Toec_Services.Policy
{
    public class PolicyCacher
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly APICall _call;
        private readonly ServiceFileSystem _fileSystemService;
        private readonly DtoClientPolicy _policy;
        private readonly DtoPolicyResult _policyResult;

        public PolicyCacher(DtoClientPolicy policy)
        {
            _policy = policy;
            _call = new APICall();
            _fileSystemService = new ServiceFileSystem();
            _policyResult = new DtoPolicyResult();
            _policyResult.PolicyResult = EnumPolicy.Result.Success;
            _policyResult.PolicyGuid = policy.Guid;
            _policyResult.PolicyName = policy.Name;
            _policyResult.PolicyHash = policy.Hash;
        }

        public DtoPolicyResult Cache()
        {
            Logger.Info(string.Format("Caching Policy {0} ({1})", _policy.Guid, _policy.Name));

            var moduleResult = new DtoModuleResult();
            moduleResult.Success = true;

            if (!CacheCondition(_policy.Condition))
                return _policyResult;

            foreach (var module in _policy.FileCopyModules)
            {
                moduleResult.Name = module.DisplayName;
                moduleResult.Guid = module.Guid;

                if (!CreateDirectory(module.Guid))
                {
                    moduleResult.ErrorMessage = "Could Not Create Cache Directory";
                    if (IsStopError(moduleResult)) return _policyResult;
                }
                if (!DownloadFiles(module.Files,module.Guid,module.DisplayName))
                {
                    moduleResult.ErrorMessage = "Could Not Download File";
                    if (IsStopError(moduleResult)) return _policyResult;
                }

                if (!CacheCondition(module.Condition))
                    return _policyResult;
            }

            foreach (var module in _policy.SoftwareModules)
            {
                moduleResult.Name = module.DisplayName;
                moduleResult.Guid = module.Guid;

                if (!CreateDirectory(module.Guid))
                {
                    moduleResult.ErrorMessage = "Could Not Create Cache Directory";
                    if (IsStopError(moduleResult)) return _policyResult;
                }
                if (!DownloadFiles(module.Files,module.Guid,module.DisplayName))
                {
                    moduleResult.ErrorMessage = "Could Not Download File";
                    if (IsStopError(moduleResult)) return _policyResult;
                }

                if (!CacheCondition(module.Condition))
                    return _policyResult;
            }

            foreach (var module in _policy.CommandModules)
            {
                moduleResult.Name = module.DisplayName;
                moduleResult.Guid = module.Guid;

                if (!CreateDirectory(module.Guid))
                {
                    moduleResult.ErrorMessage = "Could Not Create Cache Directory";
                    if (IsStopError(moduleResult)) return _policyResult;
                }
                if (!DownloadFiles(module.Files, module.Guid, module.DisplayName))
                {
                    moduleResult.ErrorMessage = "Could Not Download File";
                    if (IsStopError(moduleResult)) return _policyResult;
                }

                if (!CacheCondition(module.Condition))
                    return _policyResult;
            }

            foreach (var module in _policy.WuModules)
            {
                moduleResult.Name = module.DisplayName;
                moduleResult.Guid = module.Guid;

                if (!CreateDirectory(module.Guid))
                {
                    moduleResult.ErrorMessage = "Could Not Create Cache Directory";
                    if (IsStopError(moduleResult)) return _policyResult;
                }
                if (!DownloadFiles(module.Files,module.Guid,module.DisplayName))
                {
                    moduleResult.ErrorMessage = "Could Not Download File";
                    if (IsStopError(moduleResult)) return _policyResult;
                }

                if (!CacheCondition(module.Condition))
                    return _policyResult;
            }


            foreach (var module in _policy.ScriptModules)
            {
                moduleResult.Name = module.DisplayName;
                moduleResult.Guid = module.Guid;

                if (!CreateDirectory(module.Guid))
                {
                    moduleResult.ErrorMessage = "Could Not Create Cache Directory";
                    if (IsStopError(moduleResult)) return _policyResult;
                }
                if (!DownloadScriptFile(module))
                {
                    moduleResult.ErrorMessage = "Could Not Download Script";
                    if (IsStopError(moduleResult)) return _policyResult;
                }

                if (!CacheCondition(module.Condition))
                    return _policyResult;
            }

            foreach (var module in _policy.MessageModules)
            {
                moduleResult.Name = module.DisplayName;
                moduleResult.Guid = module.Guid;
                if (!CacheCondition(module.Condition))
                    return _policyResult;
            }

            foreach (var module in _policy.PrinterModules)
            {
                moduleResult.Name = module.DisplayName;
                moduleResult.Guid = module.Guid;
                if (!CacheCondition(module.Condition))
                    return _policyResult;
            }

            Logger.Info(string.Format("Finished Caching Policy {0} ({1})", _policy.Guid, _policy.Name));
            return _policyResult;
        }

        private bool CacheCondition(DtoClientModuleCondition condition)
        {
            if(condition.Guid == null)
            {
                //no condition
                return true;
            }
            //treat condition as script module
            var conditionJson = JsonConvert.SerializeObject(condition);
            var scriptModule = JsonConvert.DeserializeObject<DtoClientScriptModule>(conditionJson);

            var moduleResult = new DtoModuleResult();
            moduleResult.Success = true;

            moduleResult.Name = scriptModule.DisplayName;
            moduleResult.Guid = scriptModule.Guid;

            if (!CreateDirectory(scriptModule.Guid))
            {
                moduleResult.ErrorMessage = "Could Not Create Cache Directory";
                if (IsStopError(moduleResult)) return false;
            }
            if (!DownloadScriptFile(scriptModule))
            {
                moduleResult.ErrorMessage = "Could Not Download Script";
                if (IsStopError(moduleResult)) return false;
            }
            return true;
        }

        private bool CreateDirectory(string moduleGuid)
        {
            Logger.Debug("Creating directory for cached files");
            if (!new ServiceFileSystem().CreateDirectory(moduleGuid)) return false;
            return true;
        }

        private bool DownloadScriptFile(DtoClientScriptModule module)
        {
            string ext;
            if (module.ScriptType == EnumScriptModule.ScriptType.Powershell)
            {
                ext = ".ps1";
            }
            else if (module.ScriptType == EnumScriptModule.ScriptType.VbScript)
            {
                ext = ".vbs";
            }
            else if (module.ScriptType == EnumScriptModule.ScriptType.Batch)
            {
                ext = ".bat";
            }
            else
            {
                Logger.Error("Could Not Determine Script Type.  Exiting.");
                return false;
            }

            var scriptContents = _policy.Trigger == EnumPolicy.Trigger.Login
                ? new APICall().LocalApi.GetScript(module.Guid)
                : new APICall().PolicyApi.GetScript(module.Guid);
            if (string.IsNullOrEmpty(scriptContents))
                return false;
            try
            {
                using (
                    var file =
                        new StreamWriter(Path.Combine(DtoGobalSettings.BaseCachePath, module.Guid, module.Guid + ext)))
                {
                    file.WriteLine(scriptContents);
                }
                return true;
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                return false;
            }
        }

        private bool DownloadFiles(List<DtoClientFileHash> files, string moduleGuid, string moduleName)
        {
            foreach (var file in files)
            {
                Logger.Debug(string.Format("Download File {0}", file.FileName));

                if (File.Exists(Path.Combine(DtoGobalSettings.BaseCachePath, moduleGuid, file.FileName)))
                {
                    var hash =
                        _fileSystemService.GetFileHash(Path.Combine(DtoGobalSettings.BaseCachePath, moduleGuid,
                            file.FileName));
                    if (hash.Equals(file.FileHash))
                    {
                        Logger.Debug("File Is Already Cached.  Skipping.");
                        continue;
                    }
                }

                var fileRequest = new DtoClientFileRequest();
                fileRequest.FileName = file.FileName;
                fileRequest.ModuleGuid = moduleGuid;
                if (_policy.Trigger == EnumPolicy.Trigger.Login)
                {
                    //need to call the windows service to download file, tray app cannot authenticate to the server

                    fileRequest.Destination = Path.Combine(DtoGobalSettings.BaseCachePath, fileRequest.ModuleGuid,
                        fileRequest.FileName);
                    if (!_call.LocalApi.GetFile(fileRequest))
                    {
                        Logger.Error("Could Not Cache Module: " + moduleName);
                        return false;
                    }
                }
                else
                {
                    if (!_call.PolicyApi.GetFile(fileRequest))
                    {
                        Logger.Error("Could Not Cache Module: " + moduleName);
                        return false;
                    }
                }
                Logger.Debug(string.Format("Download Complete {0}", file.FileName));
            }
            return true;
        }

       

        private bool IsStopError(DtoModuleResult moduleResult)
        {
            _policyResult.PolicyResult = EnumPolicy.Result.Failed;
            _policyResult.FailedModuleGuid = moduleResult.Guid;
            _policyResult.FailedModuleName = moduleResult.Name;
            _policyResult.FailedModuleErrorMessage = moduleResult.ErrorMessage;
            _policyResult.FailedModuleExitCode = "-1";
            Logger.Error(string.Format("An Error Occurred In Module {0}", moduleResult.Name));
            if (_policy.ErrorAction == EnumPolicy.ErrorAction.Continue)
            {
                Logger.Debug("Continuing With Policy Error");
                return false;
            }

            return true;
        }
    }
}