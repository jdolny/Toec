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
                Logger.Info("Attempting Upgrade For: " + module.Name);

                RunPrereqs();
                GetWingetCommand();
               

                if (string.IsNullOrEmpty(_wingetFullPath))
                {
                    Logger.Error("Could not find / install Winget, it must be installed manually.");
                    return;
                }
              

                if (!string.IsNullOrEmpty(module.RunAs))
                {
                    var cmd = "winget";

                    cmd += $" upgrade -e --id {module.PackageId} -h --accept-source-agreements --accept-package-agreements --scope machine --force {module.Arguments}";

                    if (!string.IsNullOrEmpty(module.Override))
                        cmd += $" --override {module.Override}";


                    uint timeout = 10 * 60000; //set default 10 minutes
                    if (module.Timeout > 0)
                        timeout = (uint)module.Timeout * 60000; //convert minutes to milliseconds
                    Logger.Debug($"Running Winget command: {cmd}");

                    var iTask = new ServiceImpersonationTask();
                    iTask.Command = "powershell";
                    iTask.Arguments = cmd;
                    iTask.ExecutionTimeout = _module.Timeout;
                    iTask.ModuleGuid = _module.Guid;
                    iTask.ImpersonationGuid = _module.RunAs;

                    var result = iTask.RunTask();
                    Logger.Info(JsonConvert.SerializeObject(result));
                    Logger.Info("Winget Module: " + _module.DisplayName + " Finished");
                    _moduleResult.ExitCode = result.ToString();
                    if (result != 0)
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
                    Logger.Info("Winget Module: " + module.Name + " Finished, Exit code: " + result.ExitCode);
                }

            }
        }

        public DtoModuleResult Run()
        {
            Logger.Info("Running Winget Module: " + _module.DisplayName);

            RunPrereqs();

            GetWingetCommand();
            
            
            if (string.IsNullOrEmpty(_wingetFullPath))
            {
                _moduleResult.ErrorMessage = "Could not find / install Winget, it must be installed manually.";
                _moduleResult.Success = false;
                return _moduleResult;
            }

            if (!string.IsNullOrEmpty(_module.RunAs))
            {
                var cmd = "winget";

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


                var iTask = new ServiceImpersonationTask();
                iTask.Command = "powershell";
                iTask.Arguments = cmd;
                iTask.ExecutionTimeout = _module.Timeout;
                iTask.ModuleGuid = _module.Guid;
                iTask.ImpersonationGuid = _module.RunAs;

                var result = iTask.RunTask();
                Logger.Info(JsonConvert.SerializeObject(result));
                Logger.Info("Winget Module: " + _module.DisplayName + " Finished");
                _moduleResult.ExitCode = result.ToString();
                if (result != 0)
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
        private void RunPrereqs()
        {
            Logger.Debug("Checking WinGet Prereqs");
            try
            {
                File.WriteAllText(DtoGobalSettings.BaseCachePath + "Winget.ps1", GetScriptContents());
            }
            catch(Exception ex) 
            {
                Logger.Error("Could not run winget prereqs");
                Logger.Error(ex.ToString());
            }

            if (!string.IsNullOrEmpty(_module.RunAs))
            {
                var runWith = "Powershell.exe";
                var runWithArgs = " -ExecutionPolicy Bypass -File ";

                var iTask = new ServiceImpersonationTask();
                iTask.Command = runWith;
                iTask.Arguments = runWithArgs + $"\"{DtoGobalSettings.BaseCachePath}Winget.ps1\"";
                iTask.ExecutionTimeout = _module.Timeout;
                iTask.ModuleGuid = _module.Guid + "-wingetpre";
                iTask.ImpersonationGuid = _module.RunAs;

                var result = iTask.RunTask();
                Logger.Info(JsonConvert.SerializeObject(result));
                
                
                if (result != 0)
                {
                    Logger.Debug("prereqs installation failed");
                    if (result == 259)
                        Logger.Debug("Task Timed Out.");
                }
            }
            else
            {
               
                var pArgs = new DtoProcessArgs();
                pArgs.RunWith = "powershell.exe";
                pArgs.RunWithArgs = $" -ExecutionPolicy Bypass -NoProfile -File \"{DtoGobalSettings.BaseCachePath}Winget.ps1\"";
                pArgs.RedirectError = true;
                pArgs.RedirectOutput = true;
                var result = new ServiceProcess(pArgs).RunProcess();
                if (result.ExitCode != 0)
                {
                    Logger.Debug("prereqs installation failed");
                    Logger.Debug(result.StandardError);
                }
            }
        }

        private string GetScriptContents()
        {
            return @"
<#
MIT License

Copyright (c) 2022 Romanitho

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the ""Software""), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
#>


function Write-ToLog ($LogMsg, $LogColor = ""White"") {
    $Log = ""$(Get-Date -UFormat ""%T"") - $LogMsg""
    $Log | Write-host -ForegroundColor $LogColor
}

function Get-WingetCmd {

    $WingetCmd = $null

    #Get WinGet Path
    try {
        #Get Admin Context Winget Location
        $WingetInfo = (Get-Item ""$env:ProgramFiles\WindowsApps\Microsoft.DesktopAppInstaller_*_8wekyb3d8bbwe\winget.exe"").VersionInfo | Sort-Object -Property FileVersionRaw
        #If multiple versions, pick most recent one
        $WingetCmd = $WingetInfo[-1].FileName
    }
    catch {
        #Get User context Winget Location
        if (Test-Path ""$env:LocalAppData\Microsoft\WindowsApps\Microsoft.DesktopAppInstaller_8wekyb3d8bbwe\winget.exe"") {
            $WingetCmd = ""$env:LocalAppData\Microsoft\WindowsApps\Microsoft.DesktopAppInstaller_8wekyb3d8bbwe\winget.exe""
        }
    }

    return $WingetCmd
}


function Install-Prerequisites {

    Write-ToLog ""Checking prerequisites..."" ""Cyan""

    #Check if Visual C++ 2019 or 2022 installed
    $Visual2019 = ""Microsoft Visual C++ 2015-2019 Redistributable*""
    $Visual2022 = ""Microsoft Visual C++ 2015-2022 Redistributable*""
    $path = Get-Item HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*, HKLM:\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\* | Where-Object { $_.GetValue(""DisplayName"") -like $Visual2019 -or $_.GetValue(""DisplayName"") -like $Visual2022 }

    #If not installed, download and install
    if (!($path)) {

        Write-ToLog ""Microsoft Visual C++ 2015-2022 is not installed."" ""Red""

        try {
            #Get proc architecture
            if ($env:PROCESSOR_ARCHITECTURE -eq ""ARM64"") {
                $OSArch = ""arm64""
            }
            elseif ($env:PROCESSOR_ARCHITECTURE -like ""*64*"") {
                $OSArch = ""x64""
            }
            else {
                $OSArch = ""x86""
            }

            #Download and install
            $SourceURL = ""https://aka.ms/vs/17/release/VC_redist.$OSArch.exe""
            $Installer = ""$env:TEMP\VC_redist.$OSArch.exe""
            Write-ToLog ""-> Downloading $SourceURL...""
            Invoke-WebRequest $SourceURL -UseBasicParsing -OutFile $Installer
            Write-ToLog ""-> Installing VC_redist.$OSArch.exe...""
            Start-Process -FilePath $Installer -Args ""/passive /norestart"" -Wait
            Start-Sleep 3
            Remove-Item $Installer -ErrorAction Ignore
            Write-ToLog ""-> MS Visual C++ 2015-2022 installed successfully."" ""Green""
        }
        catch {
            Write-ToLog ""-> MS Visual C++ 2015-2022 installation failed."" ""Red""
        }

    }

    #Check if Microsoft.VCLibs.140.00.UWPDesktop is installed
    if (!(Get-AppxPackage -Name 'Microsoft.VCLibs.140.00.UWPDesktop' -AllUsers)) {
        Write-ToLog ""Microsoft.VCLibs.140.00.UWPDesktop is not installed"" ""Red""
        $VCLibsUrl = ""https://aka.ms/Microsoft.VCLibs.x64.14.00.Desktop.appx""
        $VCLibsFile = ""$env:TEMP\Microsoft.VCLibs.x64.14.00.Desktop.appx""
        Write-ToLog ""-> Downloading $VCLibsUrl...""
        Invoke-RestMethod -Uri $VCLibsUrl -OutFile $VCLibsFile
        try {
            Write-ToLog ""-> Installing Microsoft.VCLibs.140.00.UWPDesktop...""
            Add-AppxProvisionedPackage -Online -PackagePath $VCLibsFile -SkipLicense | Out-Null
            Write-ToLog ""-> Microsoft.VCLibs.140.00.UWPDesktop installed successfully."" ""Green""
        }
        catch {
            Write-ToLog ""-> Failed to intall Microsoft.VCLibs.140.00.UWPDesktop..."" ""Red""
        }
        Remove-Item -Path $VCLibsFile -Force
    }

    #Check available WinGet version, if fail set version to the latest version as of 2023-10-08
    $WingetURL = 'https://api.github.com/repos/microsoft/winget-cli/releases/latest'
    try {
        $WinGetAvailableVersion = ((Invoke-WebRequest $WingetURL -UseBasicParsing | ConvertFrom-Json)[0].tag_name).Replace(""v"", """")
    }
    catch {
        $WinGetAvailableVersion = ""1.6.2771""
    }

    #Get installed Winget version
    try {
        $WingetInstalledVersionCmd = & $Winget -v
        $WinGetInstalledVersion = (($WingetInstalledVersionCmd).Replace(""-preview"", """")).Replace(""v"", """")
        Write-ToLog ""Installed Winget version: $WingetInstalledVersionCmd""
    }
    catch {
        Write-ToLog ""WinGet is not installed"" ""Red""
    }

    #Check if the available WinGet is newer than the installed
    if ($WinGetAvailableVersion -gt $WinGetInstalledVersion) {

        Write-ToLog ""-> Downloading Winget v$WinGetAvailableVersion""
        $WingetURL = ""https://github.com/microsoft/winget-cli/releases/download/v$WinGetAvailableVersion/Microsoft.DesktopAppInstaller_8wekyb3d8bbwe.msixbundle""
        $WingetInstaller = ""$env:TEMP\Microsoft.DesktopAppInstaller_8wekyb3d8bbwe.msixbundle""
        Invoke-RestMethod -Uri $WingetURL -OutFile $WingetInstaller
        try {
            Write-ToLog ""-> Installing Winget v$WinGetAvailableVersion""
            Add-AppxProvisionedPackage -Online -PackagePath $WingetInstaller -SkipLicense | Out-Null
            Write-ToLog ""-> Winget installed."" ""Green""
        }
        catch {
            Write-ToLog ""-> Failed to install Winget!"" ""Red""
        }
        Remove-Item -Path $WingetInstaller -Force
    }

    Write-ToLog ""Checking prerequisites ended.`n"" ""Cyan""

}

#If running as a 32-bit process on an x64 system, re-launch as a 64-bit process
if (""$env:PROCESSOR_ARCHITEW6432"" -ne ""ARM64"") {
    if (Test-Path ""$($env:WINDIR)\SysNative\WindowsPowerShell\v1.0\powershell.exe"") {
        Start-Process ""$($env:WINDIR)\SysNative\WindowsPowerShell\v1.0\powershell.exe"" -Wait -ArgumentList ""-NoProfile -ExecutionPolicy Bypass -Command $($MyInvocation.line)""
        Exit $lastexitcode
    }
}

#Config console output encoding
$null = cmd /c '' #Tip for ISE
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$Script:ProgressPreference = 'SilentlyContinue'

#Check if current process is elevated (System or admin user)
$CurrentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
$Script:IsElevated = $CurrentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)


#Get Winget command
$Script:Winget = Get-WingetCmd

if ($IsElevated -eq $True) {
    Write-ToLog ""Running with admin rights.`n ""
    #Check/install prerequisites
    Install-Prerequisites
    #Reload Winget command
    $Script:Winget = Get-WingetCmd
}
else {
    Write-ToLog ""Running without admin rights.`n ""
}
";
        }
    }
}
