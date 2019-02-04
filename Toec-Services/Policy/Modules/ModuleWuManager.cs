using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using Toec_Common.Dto;
using Toec_Common.Enum;
using Toec_Common.Inventory;
using Toec_Common.Modules;
using WUApiLib;

namespace Toec_Services.Policy.Modules
{
    public class ModuleWuManager
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly DtoClientWuModule _module;

        private readonly DtoModuleResult _moduleResult;

        public ModuleWuManager(DtoClientWuModule module)
        {
            _moduleResult = new DtoModuleResult();
            _moduleResult.Name = module.DisplayName;
            _moduleResult.Guid = module.Guid;
            _moduleResult.Success = true;
            _module = module;
        }

        public DtoModuleResult Run()
        {
            Logger.Info("Running Windows Update Module: " + _module.DisplayName);
            var file = _module.Files.FirstOrDefault();
            if (file == null)
            {
                _moduleResult.Success = false;
                _moduleResult.ErrorMessage = "File Was Null";
                return _moduleResult;
            }

            var ext = Path.GetExtension(file.FileName);
            if (ext == null)
            {
                _moduleResult.Success = false;
                _moduleResult.ErrorMessage = "Missing File Extension";
                return _moduleResult;
            }

            switch (ext.ToLower())
            {
                case ".msu":
                    InstallMsu(GetFileForArch());
                    break;
                case ".cab":
                    InstallCab(GetFileForArch());
                    break;
                default:
                    _moduleResult.Success = false;
                    _moduleResult.ErrorMessage = "An Unrecognized File Extension Was Found.";
                    return _moduleResult;
            }


            return _moduleResult;
        }

        private DtoClientFileHash GetFileForArch()
        {
            string osArch;
            using (var wmi = new ServiceWmi<DtoOsWmi>(new DtoOsWmi()))
            {
                var wmiInfo = wmi.Execute();
                osArch = wmiInfo.OSArchitecture;
            }

            if (osArch.Contains("64"))
            {
                foreach (var file in _module.Files)
                {
                    if (file.FileName.ToLower().Contains("-x64"))
                    {
                        return file;
                    }
                }
            }
            else if (osArch.Contains("32"))
            {
                foreach (var file in _module.Files)
                {
                    if (file.FileName.ToLower().Contains("-x86"))
                    {
                        return file;
                    }
                }
            }
            else
            {
                Logger.Debug("Could Not Determine Current Os Architecture For Update Selection.  Using First File.");
                return _module.Files.OrderBy(x => x.FileName).FirstOrDefault();
            }

            Logger.Debug("Could Not Find A File Designated For This Architecture.  Using First File.");
            return _module.Files.OrderBy(x => x.FileName).FirstOrDefault();
        }

        private void InstallCab(DtoClientFileHash file)
        {
            var directory = Path.Combine(DtoGobalSettings.BaseCachePath, _module.Guid) + Path.DirectorySeparatorChar;
            var pArgs = new DtoProcessArgs();
            pArgs.RunWith = "cmd.exe";
            pArgs.RunWithArgs = " /c";
            pArgs.Command = "dism.exe /Online /Add-Package /PackagePath:" + "\"" + directory + file.FileName + "\"";
            pArgs.Arguments = "/Quiet /NoRestart " +_module.Arguments;
            pArgs.Timeout = _module.Timeout;
            pArgs.WorkingDirectory = directory;
            pArgs.RedirectError = _module.RedirectError;
            pArgs.RedirectOutput = _module.RedirectOutput;

            var result = new ServiceProcess(pArgs).RunProcess();
            Logger.Info(JsonConvert.SerializeObject(result));
            Logger.Info("Windows Update Module: " + _module.DisplayName + "Finished");

            _moduleResult.ExitCode = result.ExitCode.ToString();
            if (!_module.SuccessCodes.Contains(result.ExitCode.ToString()))
            {
                _moduleResult.Success = false;
                _moduleResult.ErrorMessage = result.StandardError;
            }
        }

        private void InstallMsu(DtoClientFileHash file)
        {
            var directory = Path.Combine(DtoGobalSettings.BaseCachePath, _module.Guid) + Path.DirectorySeparatorChar;
            var pArgs = new DtoProcessArgs();
            pArgs.RunWith = "wusa.exe";
            pArgs.Command = "\"" + directory + file.FileName + "\"";
            pArgs.Arguments = "/quiet /norestart " + _module.Arguments;
            pArgs.Timeout = _module.Timeout;
            pArgs.WorkingDirectory = directory;
            pArgs.RedirectError = _module.RedirectError;
            pArgs.RedirectOutput = _module.RedirectOutput;

            var result = new ServiceProcess(pArgs).RunProcess();
            Logger.Info(JsonConvert.SerializeObject(result));
            Logger.Info("Windows Update Module: " + _module.DisplayName + "Finished");

            _moduleResult.ExitCode = result.ExitCode.ToString();
            if (!_module.SuccessCodes.Contains(result.ExitCode.ToString()))
            {
                _moduleResult.Success = false;
                _moduleResult.ErrorMessage = result.StandardError;
            }
        }

        public static void InstallAllUpdates(EnumPolicy.WuType wuType)
        {
            Logger.Info("Running Windows Update.");
            var updateSession = new UpdateSession();
            var updateSearchResult = updateSession.CreateUpdateSearcher();
            var updateCollection = new UpdateCollection();
            var installCollection = new UpdateCollection();
            updateSearchResult.Online = true;
            if(wuType == EnumPolicy.WuType.Microsoft || wuType == EnumPolicy.WuType.MicrosoftSkipUpgrades)
                updateSearchResult.ServerSelection = ServerSelection.ssWindowsUpdate;
            else if (wuType == EnumPolicy.WuType.Wsus || wuType == EnumPolicy.WuType.WsusSkipUpgrades)
                updateSearchResult.ServerSelection = ServerSelection.ssManagedServer;
            else
            {
                Logger.Debug("Could Not Determine Windows Update Server Selection.");
                return;
            }

            updateSearchResult.IncludePotentiallySupersededUpdates = false;

            try
            {
                Logger.Debug("Searching For Available Windows Updates. ");
                var searchResults = updateSearchResult.Search("IsInstalled=0 and Type='Software'");

                if (wuType == EnumPolicy.WuType.MicrosoftSkipUpgrades || wuType == EnumPolicy.WuType.WsusSkipUpgrades)
                {
                    foreach (IUpdate u in searchResults.Updates)
                    {
                        var isFeatureUpgrade = false;
                        var unknownCategory = false;
                        foreach (ICategory ic in u.Categories)
                        {
                            if (string.IsNullOrEmpty(ic.Name))
                            {
                                Logger.Debug("Could Not Determine Windows Update Category.  Skipping Update.");
                                Logger.Debug(u.Title + " " + u.Identity.UpdateID + " ");
                                unknownCategory = true;
                                break;
                            }
                            if (ic.Name.Equals("Upgrades"))
                                isFeatureUpgrade = true;
                            break;
                        }

                        if (isFeatureUpgrade || unknownCategory) continue;
                        Logger.Debug(u.Title + " " + u.Identity.UpdateID + " ");
                        u.AcceptEula();
                        updateCollection.Add(u);
                    }
                }
                else //include feature upgrades
                {
                    foreach (IUpdate u in searchResults.Updates)
                    {
                        Logger.Debug(u.Title + " " + u.Identity.UpdateID + " ");
                        u.AcceptEula();
                        updateCollection.Add(u);
                    }
                }

                if (updateCollection.Count == 0)
                {
                    Logger.Info("No Updates Found.");
                    return;
                }

                UpdateDownloader downloader = updateSession.CreateUpdateDownloader();
                downloader.Updates = updateCollection;
                downloader.Download();
              
                foreach (IUpdate update in updateCollection)
                {
                    if (update.IsDownloaded)
                        installCollection.Add(update);
                }

                IUpdateInstaller installer = updateSession.CreateUpdateInstaller();
                installer.Updates = installCollection;

                IInstallationResult result = installer.Install();
                
                Logger.Debug("Windows Update Result: " + result.ResultCode);
                Logger.Debug("Reboot Required: " + result.RebootRequired);

                for(int i = 0; i < installCollection.Count; i++)
                {
                    Logger.Debug(installCollection[i].Title + " " + result.GetUpdateResult(i).ResultCode);
                }
            }
            catch
            {
                //Ignored
            }
        }

    }
}
