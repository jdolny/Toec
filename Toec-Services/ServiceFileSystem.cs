using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using log4net;
using Newtonsoft.Json;
using Toec_Common.Dto;

namespace Toec_Services
{
    public class ServiceFileSystem
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public RemotelyConnectionInfo ReadRemotelyConnectionFile()
        {
            Logger.Debug("Reading Remotely Connection File");

            try
            {
                using (var reader = new StreamReader(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Remotely", "ConnectionInfo.json")))
                {
                    var text = reader.ReadLine() ?? "";
                    var conInfo = JsonConvert.DeserializeObject<RemotelyConnectionInfo>(text);
                    if (string.IsNullOrEmpty(conInfo.DeviceID))
                        return null;
                    else
                        return conInfo;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Could Not Read Connection File");
                Logger.Error(ex.Message);
            }
            return null;
        }

        public bool CopyFile(string source, string destination, bool overwrite=true)
        {
            try
            {
                File.Copy(source, destination, overwrite);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Could Not Copy File: " + ex.Message);
                return false;
            }
        }

        public bool CreateDestinationDirectory(string destination)
        {
            Logger.Debug("Creating Directory " + destination);
            if (Directory.Exists(destination))
            {
                Logger.Debug("Directory Already Exists");
                return true;
            }

            try
            {
                Directory.CreateDirectory(destination);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Could Not Create Directory: " + ex.Message);
                return false;
            }
        }

        public bool CreateDirectory(string folderName)
        {
            var baseDirectory = DtoGobalSettings.BaseCachePath;
            Logger.Debug("Creating Directory " + baseDirectory + folderName);
            if (Directory.Exists(baseDirectory + folderName))
            {
                Logger.Debug("Directory Already Exists");
                return true;
            }

            try
            {
                Directory.CreateDirectory(baseDirectory + folderName);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Could Not Create Directory: " + ex.Message);
                return false;
            }
        }

        public bool DeleteDirectory(string folderName)
        {
            if (string.IsNullOrEmpty(folderName)) return false;
            
            var baseDirectory = DtoGobalSettings.BaseCachePath;
            Logger.Debug("Deleting Directory " + baseDirectory + folderName);
            if (!Directory.Exists(baseDirectory + folderName))
            {
                Logger.Debug("Folder Does Not Exist");
                return true;
            }

            try
            {
                Directory.Delete(baseDirectory + folderName, true);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Could Not Delete Directory: " + ex.Message);
                return false;
            }
        }

        public string GetFileHash(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = new BufferedStream(File.OpenRead(filePath), 1200000))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}