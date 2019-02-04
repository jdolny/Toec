using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using log4net.Repository.Hierarchy;
using Toec_Common.Dto;
using Toec_Common.Enum;
using Toec_Services.ApiCall;
using Toec_Services.Entity;
using Toec_Services.Policy.Modules;

namespace Toec_Services
{
    public class ServiceInitialize
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private void GetStartupSettings()
        {
            int threshold;
            while (true)
            {
                new ServiceActiveComServer().Set();
                var startupInfo = new APICall().ProvisionApi.GetStartupInfo(DtoGobalSettings.ClientIdentity.Name);
                if (startupInfo != null)
                {
                    if (startupInfo.IsError)
                    {
                        Logger.Error(startupInfo.ErrorMessage);
                        Task.Delay(60*1000).Wait();
                        continue;
                    }

                    if (!new ServiceUpdate().CheckUpdate(startupInfo.ExpectedClientVersion))
                    {
                        Task.Delay(300 * 1000).Wait();
                        continue;
                    }


                    threshold = Convert.ToInt32(startupInfo.ThresholdWindow);
                    if (startupInfo.DelayType == EnumStartupDelay.DelayType.None)
                        break;
                    if (startupInfo.DelayType == EnumStartupDelay.DelayType.Seconds)
                    {
                        Logger.Info(string.Format("Startup Delay Enabled.  Delaying For {0} Seconds.",
                            startupInfo.SubDelay));
                        Task.Delay(Convert.ToInt32(startupInfo.SubDelay)*1000).Wait();
                        break;
                    }
                    if (startupInfo.DelayType == EnumStartupDelay.DelayType.File)
                    {
                        Logger.Info("Startup Delay Enabled.  Waiting For File Condition To Be Met.");
                        if (File.Exists(startupInfo.SubDelay))
                            break;
                    }
                }
                else
                {
                    Logger.Error("Could Not Reach Server To Obtain Startup Info.  Retrying In 1 Minute.");
                }

                Task.Delay(60*1000).Wait();
            }

            if (threshold > 0)
            {
                var delayTimeSeconds = DtoGobalSettings.Rnd.Next(0, threshold + 1);
                Logger.Info(string.Format("Threshold Window Enabled.  Delaying For {0} Seconds.", delayTimeSeconds));
                Task.Delay(delayTimeSeconds*1000).Wait();
            }
        }

        public bool OnStartTasks()
        {
            Logger.Info("Toec Version: " + DtoGobalSettings.ClientVersion);
            VerifyDbConn();
            VerifyInstallationId();

            var logLevelEntity = new ServiceSetting().GetSetting("log_level");
            if (logLevelEntity != null)
                new ServiceLogLevel().Set(logLevelEntity.Value);
            else
                DtoGobalSettings.LogLevel =
                  ((Hierarchy)LogManager.GetRepository()).Root.Level;

            VerifyComServersDefined();

            //Set some global params
            DtoGobalSettings.ClientIdentity = new DtoClientIdentity();
            DtoGobalSettings.BaseCachePath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            DtoGobalSettings.BaseCachePath = Path.Combine(DtoGobalSettings.BaseCachePath, "Toec", "AppData") +
                                             Path.DirectorySeparatorChar;
            DtoGobalSettings.ClientIdentity.Name = Dns.GetHostName();
            DtoGobalSettings.ClientIdentity.Guid = new ServiceSetting().GetSetting("computer_identifier").Value;
            DtoGobalSettings.ClientIdentity.InstallationId = new ServiceSetting().GetSetting("installation_id").Value;
            DtoGobalSettings.RemoteApiPort = new ServiceSetting().GetSetting("remote_api_port").Value;
            DtoGobalSettings.LocalApiPort = new ServiceSetting().GetSetting("local_api_port").Value;
            DtoGobalSettings.ServiceStartTime = DateTime.UtcNow;

            var domain = IPGlobalProperties.GetIPGlobalProperties().DomainName;
            if (!string.IsNullOrEmpty(domain))
                DtoGobalSettings.Domain = "." + domain;
            else
                DtoGobalSettings.Domain = string.Empty;

            if (!string.IsNullOrEmpty(DtoGobalSettings.LocalApiPort))
            {
                //Create text file with port for tray app to read, tray doesn't have access to database
                try
                {
                    var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                        "Toec");
                    Directory.CreateDirectory(path);
                    File.Delete(Path.Combine(path, "TraySettings"));
                    File.WriteAllText(Path.Combine(path, "TraySettings"), DtoGobalSettings.LocalApiPort);
                }
                catch (Exception ex)
                {
                    Logger.Error("Could Not Create Tray App Settings File.  Tray App Will Not Be Available.");
                    Logger.Error(ex.Message);
                }
            }
            else
            {
                Logger.Debug("Local Api Port Is Not Populated.  Tray App Will Not Be Available.");
            }

            GetStartupSettings();

            VerifyProvisionStatus();

            ProvisionClient();

            DtoGobalSettings.ServiceStartupComplete = true;

            return true;
        }

        private void ProvisionClient()
        {
            var counter = 0;
            while (true)
            {
                var provisionStatus = new ServiceProvision().ProvisionClient();
                Logger.Debug("Provision Client: " + provisionStatus.ToString());
                if (provisionStatus == EnumProvisionStatus.Status.Provisioned)
                    break;
                counter++;
                if (provisionStatus == EnumProvisionStatus.Status.FullReset)
                {
                    new ServiceReset().HardReset("Full");
                    new ServiceSystemService().RestartToec();
                }
                if (provisionStatus == EnumProvisionStatus.Status.Reset ||
                    provisionStatus == EnumProvisionStatus.Status.NotStarted)
                {
                    new ServiceReset().HardReset("Partial");
                    new ServiceSystemService().RestartToec();
                }

                if (counter == 10 && provisionStatus == EnumProvisionStatus.Status.Error)
                {
                    Logger.Debug("Provision Failed 10 Times.  Performing Partial Reset.");
                    counter = 0;
                    new ServiceReset().HardReset("Partial");
                    new ServiceSystemService().RestartToec();
                }

                Task.Delay(30*1000).Wait();
            }
        }

        private void VerifyComServersDefined()
        {
            var serviceSetting = new ServiceSetting();
            //no active com servers found, check for initial com servers
            var initialComServers = serviceSetting.GetSetting("initial_com_servers").Value;
            if (!string.IsNullOrEmpty(initialComServers))
                return;

            var activeComServers = serviceSetting.GetSetting("active_com_servers").Value;
            if (!string.IsNullOrEmpty(activeComServers))
                return;

            Logger.Error("No Client Com Servers Defined.  Service Cannot Continue.  Exiting....");
            Task.Delay(10*1000).Wait();
            Environment.Exit(1);
        }

        private void VerifyInstallationId()
        {
            var serviceSetting = new ServiceSetting();
            Logger.Info("Verifying Installation ID");
            var status = serviceSetting.GetSetting("provision_status");
            var installID = serviceSetting.GetSetting("installation_id");
            Logger.Info("Provision Status: " + status.Value);
            Logger.Info("Installation ID: " + installID.Value);
            if(string.IsNullOrEmpty(installID.Value) && status.Value.Equals("0"))
            {
                //from prepare image arg, generate new id
                Logger.Info("Generating New Installation ID");
                installID.Value = Guid.NewGuid().ToString();
                serviceSetting.UpdateSettingValue(installID);
            }
            Logger.Info("Verification Complete");
        }

        private void VerifyDbConn()
        {
            //Verify Database access
            try
            {
                var writeTest = new ServiceSetting().GetSetting("write_test");
                writeTest.Value = Guid.NewGuid().ToString();
                new ServiceSetting().UpdateSettingValue(writeTest);
            }
            catch (Exception ex)
            {
                Logger.Error("Database Access Failed.  Service Cannot Continue.  Exiting....");
                Logger.Error(ex.Message);
                Task.Delay(10*1000).Wait();
                Environment.Exit(1);
            }
        }

        private void VerifyProvisionStatus()
        {
            //Verify the current provision status is actually what it claims to be.
            var verificationCounter = 0;
            while (verificationCounter <= 5)
            {
                verificationCounter++;
                if (new ServiceProvision().VerifyProvisionStatus())
                    break;
                Logger.Info("Verification Failed.");
                if (verificationCounter == 5)
                {
                    //reset provision status
                    new ServiceReset().HardReset("Partial");
                    break;
                }
                Task.Delay(5*1000).Wait();
            }
        }
    }
}