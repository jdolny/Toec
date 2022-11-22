using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using log4net;
using Toec_Common.Dto;
using Toec_Common.Modules;

namespace Toec_Services.Policy.Modules
{
    public class ModuleFileCopy
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ServiceFileSystem _fileSystemService;
        private readonly DtoClientFileCopyModule _module;
        private readonly DtoModuleResult _moduleResult;

        public ModuleFileCopy(DtoClientFileCopyModule module)
        {
            _fileSystemService = new ServiceFileSystem();
            _moduleResult = new DtoModuleResult();
            _moduleResult.Name = module.DisplayName;
            _moduleResult.Guid = module.Guid;
            _moduleResult.Success = true;
            _module = module;
        }

        public DtoModuleResult Run()
        {
            Logger.Info("Running File Copy Module: " + _module.DisplayName);
            if (_module.Destination.Equals("[toec-appdata]"))
                Logger.Debug("Module Has No Destination.  Module Is Cached Only.");
            else
            {
                if (!_fileSystemService.CreateDestinationDirectory(_module.Destination))
                {
                    _moduleResult.Success = false;
                    _moduleResult.ExitCode = "-1";
                    _moduleResult.ErrorMessage = "Could Not Create Destination Directory";
                    return _moduleResult;
                }
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

                var extension = Path.GetExtension(file.FileName);
                if (_module.Unzip && !string.IsNullOrEmpty(extension) && extension.ToLower().Equals(".zip"))
                {
                    if (_module.Destination.Equals("[toec-appdata]"))
                        _module.Destination = Path.Combine(DtoGobalSettings.BaseCachePath, _module.Guid);
                    try
                    {
                        var path = Path.Combine(DtoGobalSettings.BaseCachePath, _module.Guid, file.FileName);
                        using (FileStream zipToOpen = new FileStream(path, FileMode.Open))
                        {
                            using (ZipArchive archive = new ZipArchive(zipToOpen))
                            {
                                ZipArchiveExtensions.ExtractToDirectory(archive, _module.Destination , _module.Overwrite);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Could Not Unzip File");
                        Logger.Error(ex.Message);
                        _moduleResult.Success = false;
                        _moduleResult.ExitCode = "-1";
                        _moduleResult.ErrorMessage = "Could Not Unzip File To Destination";
                        return _moduleResult;
                    }
                }
                else if (_module.Destination.Equals("[toec-appdata]"))
                {
                    //do nothing, file was already copied during cacheing
                }
                else
                {
                    if (
                        !_fileSystemService.CopyFile(
                            Path.Combine(DtoGobalSettings.BaseCachePath, _module.Guid, file.FileName),
                            Path.Combine(_module.Destination, file.FileName),_module.Overwrite))
                    {
                        _moduleResult.Success = false;
                        _moduleResult.ExitCode = "-1";
                        _moduleResult.ErrorMessage = "Could Not Copy File To Destination";
                        return _moduleResult;
                    }
                }
            }

            Logger.Info($"File Copy Module {_module.DisplayName} Completed");
            return _moduleResult;
        }
    }
}