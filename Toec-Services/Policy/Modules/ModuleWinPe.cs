using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using log4net;
using Toec_Common.Dto;
using Toec_Common.Modules;

namespace Toec_Services.Policy.Modules
{
    public class ModuleWinPe
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ServiceFileSystem _fileSystemService;
        private readonly DtoClientWinPeModule _module;
        private readonly DtoModuleResult _moduleResult;
        private readonly string _bcdGuid;

        public ModuleWinPe(DtoClientWinPeModule module)
        {
            _fileSystemService = new ServiceFileSystem();
            _moduleResult = new DtoModuleResult();
            _moduleResult.Name = module.DisplayName;
            _moduleResult.Guid = module.Guid;
            _moduleResult.Success = true;
            _module = module;
            _bcdGuid = Guid.NewGuid().ToString();
        }

        public DtoModuleResult Run()
        {
            Logger.Info("Running WinPE Module: " + _module.DisplayName);

            var fi = new FileInfo(Environment.SpecialFolder.Windows.ToString());
            var destination = Path.GetPathRoot(fi.FullName);
            var partition = destination.Replace("\\", "");


            _module.Destination = Path.Combine(destination, "boot");

            if (!_fileSystemService.CreateDestinationDirectory(_module.Destination))
            {
                _moduleResult.Success = false;
                _moduleResult.ExitCode = "-1";
                _moduleResult.ErrorMessage = "Could Not Create Destination Directory";
                Logger.Error("Could Not Create Destination Directory");
                return _moduleResult;
            }


            foreach (var file in _module.Files)
            {
                Logger.Debug(string.Format("Processing File {0}", file.FileName));
                if (!File.Exists(Path.Combine(DtoGobalSettings.BaseCachePath, _module.Guid,
                    file.FileName)))
                {
                    Logger.Debug("File No Longer Exists: " + Path.Combine(DtoGobalSettings.BaseCachePath, _module.Guid,
                                     file.FileName));
                    _moduleResult.Success = false;
                    _moduleResult.ExitCode = "-1";
                    _moduleResult.ErrorMessage = "The File No Longer Exists.";
                    return _moduleResult;
                }

                if (
                    !_fileSystemService.CopyFile(
                        Path.Combine(DtoGobalSettings.BaseCachePath, _module.Guid, file.FileName),
                        Path.Combine(_module.Destination, file.FileName), true))
                {
                    _moduleResult.Success = false;
                    _moduleResult.ExitCode = "-1";
                    _moduleResult.ErrorMessage = "Could Not Copy File To Destination";
                    Logger.Error("Could Not Copy File To Destination");
                    return _moduleResult;
                }

            }

            var arch = Environment.Is64BitOperatingSystem ? "x64" : "x86";

            if (!File.Exists(Path.Combine(_module.Destination,"boot.sdi")))
            {
                _moduleResult.Success = false;
                _moduleResult.ExitCode = "-1";
                _moduleResult.ErrorMessage = "Required file boot.sdi was not found.";
                Logger.Error("Required file boot.sdi was not found.");
                return _moduleResult;
            }

            if (!File.Exists(Path.Combine(_module.Destination, "WinPE10" + arch + ".wim")))
            {
                _moduleResult.Success = false;
                _moduleResult.ExitCode = "-1";
                _moduleResult.ErrorMessage = "Required file WinPE10" + arch + ".wim was not found.";
                Logger.Error("Required file WinPE10" + arch + ".wim was not found.");
                return _moduleResult;
            }

            var pArgs = new DtoProcessArgs();
            pArgs.RunWith = "cmd.exe";
            pArgs.RunWithArgs = "/c ";
            pArgs.RedirectOutput = true;
            pArgs.RedirectError = true;
            pArgs.Command =
                "bcdedit /create {ramdiskoptions} /d \"Theopenem Imaging\"" +
                " & bcdedit /set {ramdiskoptions} ramdisksdidevice partition=" + partition +
                " & bcdedit /set {ramdiskoptions} ramdisksdipath \\boot\\boot.sdi" +
                " & bcdedit /create {" + _bcdGuid + "} /application osloader /d \"Theopenem Imaging\"" +
                " & bcdedit /set {" + _bcdGuid + "} systemroot \\windows" +
                " & bcdedit /set {" + _bcdGuid + "} detecthal Yes" +
                " & bcdedit /set {" + _bcdGuid + "} winpe Yes" +
                " & bcdedit /set {" + _bcdGuid + "} osdevice ramdisk=[" + partition + "]\\boot\\WinPE10" + arch + ".wim,{ramdiskoptions}" +
                " & bcdedit /set {" + _bcdGuid + "} device ramdisk=[" + partition + "]\\boot\\WinPE10" + arch + ".wim,{ramdiskoptions}" +
                " & bcdedit /bootsequence {" + _bcdGuid + "}";

            new ServiceProcess(pArgs).RunProcess();

            Logger.Info($"WinPE Module {_module.DisplayName} Completed");
            Logger.Info("Computer Will Start Image Deployment At Next Reboot.");
            return _moduleResult;
        }
    }
}