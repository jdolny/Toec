using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using Toec_Common.Dto;
using Toec_Services.ApiCall;

namespace Toec_Services
{
    public class ServiceUpdate
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public bool CheckUpdate(string expectedClientVersion)
        {
            if (!expectedClientVersion.Equals(DtoGobalSettings.ClientVersion))
            {
                var clientIsNewerThanServer = false;
                try
                {
                    var serverVersion = expectedClientVersion.Split('.');
                    var serverMajor = Convert.ToInt32(serverVersion[0]);
                    var serverMinor = Convert.ToInt32(serverVersion[1]);
                    var serverBuild = Convert.ToInt32(serverVersion[2]);


                    var clientVersion = DtoGobalSettings.ClientVersion.Split('.');
                    var clientMajor = Convert.ToInt32(clientVersion[0]);
                    var clientMinor = Convert.ToInt32(clientVersion[1]);
                    var clientBuild = Convert.ToInt32(clientVersion[2]);


                    if (clientMajor > serverMajor)
                        clientIsNewerThanServer = true;

                    else if (clientMajor == serverMajor && clientMinor > serverMinor)
                        clientIsNewerThanServer = true;

                    else if (clientMajor == serverMajor && clientMinor == serverMinor &&
                             clientBuild > serverBuild)
                        clientIsNewerThanServer = true;

                    if (clientIsNewerThanServer)
                    {
                        Logger.Error("Client Version Is Newer Than Server Version.  Server Must Be Upgraded");
                        return false;
                    }
                    //client version is older than server, start the client upgrade process
                    Logger.Info("Client Update Required");
                    var arch = Environment.Is64BitOperatingSystem ? "-x64.msi" : "-x86.msi";
                    return UpdateClient($"Toec-{serverMajor}.{serverMinor}.{serverBuild}{arch}");

                }
                catch (Exception ex)
                {
                    Logger.Error("Could Not Parse Version Information");
                    Logger.Error(ex.Message);
                    return false;
                }
            }

            return true;
        }

        private bool UpdateClient(string msiFile)
        {
            Logger.Info("Updating Toec To: " + msiFile);
         
          
            try
            {
                Directory.CreateDirectory(DtoGobalSettings.BaseCachePath + "ClientUpgrades\\");
            }
            catch
            {
                //ignored
            }

            Logger.Debug($"Obtaining A Download Connection To {DtoGobalSettings.ComServer}");
            var downloadConRequest = new DtoDownloadConRequest();
            downloadConRequest.ComputerGuid = DtoGobalSettings.ClientIdentity.Guid;
            downloadConRequest.ComputerName = DtoGobalSettings.ClientIdentity.Name;
            downloadConRequest.ComServer = DtoGobalSettings.ComServer;
            var downloadConnection = new APICall().PolicyApi.CreateDownloadConnection(downloadConRequest);
            if(downloadConnection == null)
            {
                Logger.Error("Could Not Obtain Download Connection. Response was null.");
                return false;
            }

            if (!downloadConnection.Success)
            {
                Logger.Error("Could Not Obtain Download Connection. " + downloadConnection.ErrorMessage);
                return false;
            }

            if (downloadConnection.QueueIsFull)
            {
                Logger.Debug("Download Connections Are Full");
                return false;
            }

            Logger.Debug("Downloading Files");

            var msiRequest = new DtoClientFileRequest();
            msiRequest.FileName = msiFile;
            Logger.Debug($"Downloading {msiRequest.FileName}");
            if (!new APICall().PolicyApi.GetClientUpgrade(msiRequest, DtoGobalSettings.ClientIdentity.Name))
            {
                Logger.Debug("Releasing The Download Connection.");
                new APICall().PolicyApi.RemoveDownloadConnection(downloadConRequest);
                return false;
            }

            Logger.Debug("Releasing The Download Connection.");
            new APICall().PolicyApi.RemoveDownloadConnection(downloadConRequest);

            if(new FileInfo(DtoGobalSettings.BaseCachePath + "ClientUpgrades\\" + msiFile).Length < 1)
            {
                Logger.Debug("MSI File was invalid.");
                return false;
            }

            var pArgs = new DtoProcessArgs();
            pArgs.RunWith = "msiexec.exe";
            pArgs.RunWithArgs = " /i ";
            pArgs.Command = "\"" + DtoGobalSettings.BaseCachePath + "ClientUpgrades\\" + msiFile + "\"" + $" /q /norestart";         
            pArgs.Timeout = 5;
            pArgs.WorkingDirectory = DtoGobalSettings.BaseCachePath + "ClientUpgrades\\";
            var result = new ServiceProcess(pArgs).RunProcess();

            //Nothing from here on should execute. Updating msi should kill this process.
            Logger.Info(JsonConvert.SerializeObject(result));
            return true;
        }
    }
}
