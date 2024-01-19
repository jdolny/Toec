using log4net;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Toec_Common.Dto;
using Toec_Common.Enum;
using Toec_Common.Modules;


namespace Toec_Services.Policy.Modules
{
    internal class ModuleWingetManager
    {
        private static readonly ILog Logger =
           LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private DtoClientWingetModule _module;

        private readonly DtoModuleResult _moduleResult;
        private readonly EnumPolicy.Trigger _trigger;
        private readonly string[] _successCodes = { "0" };
        private string _wingetFullPath;
        private string _powershellFullPath;
        private string _username;
        private string _password;
        private string _domain;
        private bool _notAdminError = false;

        public ModuleWingetManager(DtoClientWingetModule module, EnumPolicy.Trigger policyTrigger)
        {
            _moduleResult = new DtoModuleResult();
            _moduleResult.Name = module.DisplayName;
            _moduleResult.Guid = module.Guid;
            _moduleResult.Success = true;
            _module = module;
            _trigger = policyTrigger;
        }

        public ModuleWingetManager(EnumPolicy.Trigger policyTrigger)
        {
            _trigger = policyTrigger;
        }

        public void Update()
        {
            Logger.Info("Running Winget Upgrades");
           

            var winGetModules = new ApiCall.APICall().PolicyApi.GetWingetModuleUpdates();
            if(!winGetModules.Any())
            {
                Logger.Info("No Winget Modules were found to update");
                return;
            }
            foreach (var module in winGetModules.Where(x => x.InstallType == EnumWingetInstallType.WingetInstallType.Install && x.KeepUpdated))
            {
                _module = module;
                GetWingetCommand();

                if (string.IsNullOrEmpty(_wingetFullPath))
                {
                    InstallWinget();
                    GetWingetCommand();
                }

                if (string.IsNullOrEmpty(_wingetFullPath))
                {
                    Logger.Error("Could not find / install Winget, it must be installed manually.");
                    return;
                }
                Logger.Info("Attempting Upgrade For: " +  module.DisplayName);
                if (!GetRunAsCredentials())
                {
                    Logger.Debug("Error while obtaining impersonation credentials");
                    continue;
                }

                if (!string.IsNullOrEmpty(module.RunAs))
                {
                    //when running as a user and the service is running as system, winget must be called from powershell and not directly
                    //or there will be an access error
                    var cmd = _powershellFullPath + " " + _wingetFullPath;

                    cmd += $" upgrade -e --id {module.PackageId} -h --accept-source-agreements --accept-package-agreements --scope machine --force {module.Arguments}";

                    if (!string.IsNullOrEmpty(module.Override))
                        cmd += $" --override {module.Override}";


                    uint timeout = 10 * 60000; //set default 10 minutes
                    if (module.Timeout > 0)
                        timeout = (uint)module.Timeout * 60000; //convert minutes to milliseconds
                    Logger.Debug($"Running Winget command: {cmd}");

                    var result = new RunasUser().RunCmdAsUser(_username, _password, _domain, cmd, timeout, 2, true);

                    Logger.Info("Winget Module: " + module.DisplayName + " Finished, Exit code: " + result.ExitCode);
                }
                else
                {
                    //when installing from the system account, winget can be called directly, no need to use powershell
                    var scope = "machine";
                    if (_trigger == EnumPolicy.Trigger.Login)
                        scope = "user";
                    var pArgs = new DtoProcessArgs();
                    pArgs.RunWith = _wingetFullPath;

                    pArgs.RunWithArgs = $" upgrade -e --id {module.PackageId} -h --accept-source-agreements --accept-package-agreements --scope {scope} --force {module.Arguments}";

                    if (!string.IsNullOrEmpty(module.Override))
                        pArgs.RunWithArgs += $" --override {module.Override}";


                    pArgs.Timeout = module.Timeout;
                    pArgs.RedirectError = module.RedirectError;
                    pArgs.RedirectOutput = module.RedirectOutput;

                    var result = new ServiceProcess(pArgs).RunProcess();
                    Logger.Info("Winget Module: " + module.DisplayName + " Finished, Exit code: " + result.ExitCode);
                }

            }
        }

        public DtoModuleResult Run()
        {
            Logger.Info("Running Winget Module: " + _module.DisplayName);

            if(!GetRunAsCredentials())
            {
                _moduleResult.ErrorMessage = "Error while obtaining impersonation credentials";
                _moduleResult.Success = false;
                return _moduleResult;
            }

            GetWingetCommand();
            if (string.IsNullOrEmpty(_wingetFullPath))
            {
                InstallWinget();
                GetWingetCommand();
            }
            
            if (string.IsNullOrEmpty(_wingetFullPath))
            {
                _moduleResult.ErrorMessage = "Could not find / install Winget, it must be installed manually.";
                _moduleResult.Success = false;
                return _moduleResult;
            }

            if (!string.IsNullOrEmpty(_module.RunAs))
            {
                //when running as a user and the service is running as system, winget must be called from powershell and not directly
                //or there will be an access error
                var cmd = _powershellFullPath + " " + _wingetFullPath;
                if (_module.InstallType == EnumWingetInstallType.WingetInstallType.Install)
                {
                    cmd += $" install -e --id {_module.PackageId} -h --accept-source-agreements --accept-package-agreements --scope machine --force {_module.Arguments}";
                    if (!_module.InstallLatest)
                        cmd += $" -v {_module.PackageVersion}";
                    if (!string.IsNullOrEmpty(_module.Override))
                        cmd += $" --override {_module.Override}";
                }
                else
                    cmd += $" uninstall -e --id {_module.PackageId} -h --accept-source-agreements --scope machine --force {_module.Arguments}";

                uint timeout = 10 * 60000; //set default 10 minutes
                if(_module.Timeout > 0)
                    timeout = (uint)_module.Timeout * 60000; //convert minutes to milliseconds
                Logger.Debug($"Running Winget command: {cmd}");

                var result = new RunasUser().RunCmdAsUser(_username, _password, _domain, cmd, timeout,2,true);

                Logger.Info("Winget Module: " + _module.DisplayName + " Finished");
                _moduleResult.ExitCode = result.ExitCode.ToString();
                if (result.ExitCode != 0)
                {
                    _moduleResult.Success = false;
                    _moduleResult.ErrorMessage = result.Output;
                }
            }
            else
            {
                //when installing from the system account, winget can be called directly, no need to use powershell
                var scope = "machine";
                if (_trigger == EnumPolicy.Trigger.Login)
                    scope = "user";
                var pArgs = new DtoProcessArgs();
                pArgs.RunWith = _wingetFullPath;
                if (_module.InstallType == Toec_Common.Enum.EnumWingetInstallType.WingetInstallType.Install)
                {
                    pArgs.RunWithArgs = $" install -e --id {_module.PackageId} -h --accept-source-agreements --accept-package-agreements --scope {scope} --force {_module.Arguments}";
                    if (!_module.InstallLatest)
                        pArgs.RunWithArgs += $" -v {_module.PackageVersion}";
                    if (!string.IsNullOrEmpty(_module.Override))
                        pArgs.RunWithArgs += $" --override {_module.Override}";
                }
                else 
                    pArgs.RunWithArgs = $" uninstall -e --id {_module.PackageId} -h --accept-source-agreements --scope {scope} --force {_module.Arguments} ";

                pArgs.Timeout = _module.Timeout;
                pArgs.RedirectError = _module.RedirectError;
                pArgs.RedirectOutput = _module.RedirectOutput;

                var result = new ServiceProcess(pArgs).RunProcess();
                Logger.Info("Winget Module: " + _module.DisplayName + " Finished");
                _moduleResult.ExitCode = result.ExitCode.ToString();
                if (!_successCodes.Contains(result.ExitCode.ToString()))
                {
                    _moduleResult.Success = false;
                    _moduleResult.ErrorMessage = result.StandardError;
                }
            }

            return _moduleResult;
        }

        public void GetWingetCommand()
        {
            Logger.Debug("Locating Winget");
            if (_notAdminError) return;
            if(_trigger == EnumPolicy.Trigger.Login)
            {
                var pArgs = new DtoProcessArgs();
                pArgs.RunWith = "winget.exe";
                pArgs.RunWithArgs = $" --version ";
                pArgs.RedirectError = true;
                pArgs.RedirectOutput = true;

                var result = new ServiceProcess(pArgs).RunProcess();
                if (result.ExitCode != 0)
                {
                    Logger.Debug("Winget version lookup failed");
                    Logger.Debug(result.StandardError);
                }
                else
                {
                    _wingetFullPath = "winget.exe";
                    Logger.Debug("Found Winget version: " + result.StandardOut);
                }
            }
            else if (!string.IsNullOrEmpty(_module.RunAs))
            {
                //winget modules run as a user must have admin rights, check for rights first
                var wingetPath = new RunasUser().RunCmdAsUser(_username, _password, _domain, "where winget.exe", 10000, 2, true);
                if (wingetPath.Output.Contains("[admin-error]"))
                {
                    _notAdminError = true;
                    Logger.Debug("Impersonation accounts used with winget must have Admin privileges");
                    return;
                }

                var powershellPath = new RunasUser().RunCmdAsUser(_username, _password, _domain, "where powershell.exe", 10000, 2, true);
                if (powershellPath.ExitCode != 0)
                {
                    Logger.Debug("Could not find powershell path, cannot continue");
                    return;
                }
                _powershellFullPath = powershellPath.Output;

                var versionResult = new RunasUser().RunCmdAsUser(_username, _password, _domain, $"{_powershellFullPath} {wingetPath.Output} --version", 10000,2,true);
                if (versionResult.ExitCode == 0)
                {
                   _wingetFullPath = wingetPath.Output;
                    Logger.Debug("Found Winget version: " + versionResult.Output);
                    return;
                }
            }
            else
            {
                var pfwa = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "WindowsApps");
                var directories = Directory.GetDirectories(pfwa, "Microsoft.DesktopAppInstaller_*_*__8wekyb3d8bbwe");
                Array.Reverse(directories);
                foreach (string directory in directories)
                {
                    var wingetFullPath = Path.Combine(directory, "winget.exe");
                    if (File.Exists(wingetFullPath))
                    {
                        _wingetFullPath = wingetFullPath;
                        //check version
                        var pArgs = new DtoProcessArgs();
                        pArgs.RunWith = wingetFullPath;
                        pArgs.RunWithArgs = $" --version ";
                        pArgs.RedirectError = true;
                        pArgs.RedirectOutput = true;
                        pArgs.WorkingDirectory = DtoGobalSettings.BaseCachePath;
                        var result = new ServiceProcess(pArgs).RunProcess();
                        if (result.ExitCode != 0)
                        {
                            Logger.Debug("Winget version lookup failed");
                            Logger.Debug(result.StandardError);
                        }
                        else
                        {
                            Logger.Debug("Found Winget version: " + result.StandardOut);
                        }

                        return;
                    }
                }
            }
        }

        private void InstallWinget()
        {
            if (_notAdminError) return;
            Logger.Debug("Installing Winget");
            var request = new RestRequest();
            request.Method = Method.GET;
            var client = new RestClient();
            client.BaseUrl = new Uri("https://github.com");
            request.Resource = "/microsoft/winget-cli/releases/latest/download/Microsoft.DesktopAppInstaller_8wekyb3d8bbwe.msixbundle";
            var destination = string.Empty;
            if(_trigger == EnumPolicy.Trigger.Login)
                destination = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),"winget.msix");
            else
                destination = Path.Combine(DtoGobalSettings.BaseCachePath, "winget.msix");
            try
            {
                using (var stream = File.Create(destination, 4096))
                {
                    request.ResponseWriter = (responseStream) => responseStream.CopyTo(stream);
                    client.DownloadData(request);
                    if (stream.Length == 0)
                    {
                        Logger.Debug($"Could not download winget from {client.BaseUrl}{request.Resource}");
                        //something went wrong, rest sharp can't display any other info with downloaddata, so we don't know why
                        return;

                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Error("Could Not Save File: " + destination);
                Logger.Error(ex.Message);
                return;
            }

            if(!string.IsNullOrEmpty(_module.RunAs))
            {
               
                var installResult = new RunasUser().RunCmdAsUser(_username, _password, _domain, $"{_powershellFullPath} -ExecutionPolicy Bypass -NoProfile Add-AppxPackage -Path '{destination}' ", 5 * 60000,2,true);
                var installSourceResult = new RunasUser().RunCmdAsUser(_username, _password, _domain, $"{_powershellFullPath} -ExecutionPolicy Bypass -NoProfile Add-AppxPackage -RegisterByFamilyName -MainPackage Microsoft.Winget.Source_8wekyb3d8bbwe ", 5 * 60000,2,true);
            }
            else
            {
                var pArgs = new DtoProcessArgs();
                pArgs.RunWith = "powershell.exe";
                pArgs.RunWithArgs = $" -ExecutionPolicy Bypass -NoProfile Add-AppxProvisionedPackage -Online -PackagePath '{destination}' -SkipLicense ";
                pArgs.RedirectError = true;
                pArgs.RedirectOutput = true;
                var result = new ServiceProcess(pArgs).RunProcess();
                if(result.ExitCode != 0)
                {
                    Logger.Debug("Winget installation failed");
                    Logger.Debug(result.StandardError);
                }

                pArgs.RunWithArgs = $" -ExecutionPolicy Bypass -NoProfile Add-AppxPackage -RegisterByFamilyName -MainPackage Microsoft.Winget.Source_8wekyb3d8bbwe ";
                new ServiceProcess(pArgs).RunProcess();
            }
        }

        private bool GetRunAsCredentials()
        {
            if (!string.IsNullOrEmpty(_module.RunAs))
            {
                var credentials = new ApiCall.APICall().PolicyApi.GetImpersonationAccount(_module.RunAs);
                if (credentials == null)
                {
                    Logger.Debug("Could Not Obtain Credentials For Impersonation Account " + _module.RunAs);
                    return false;
                }
                _domain = string.Empty;
                if (credentials.Username.Contains("\\"))
                {
                    var tmp = credentials.Username.Split('\\');
                    if (tmp.Length != 2)
                    {
                        Logger.Debug("Could Not Parse Username " + _module.RunAs);
                        return false;
                    }
                    _domain = tmp[0];
                    _username = tmp[1];
                    _password = credentials.Password;
                }
                else
                {
                    _username = credentials.Username;
                    _password = credentials.Password;
                }
            }
            return true;
        }
    }
}
