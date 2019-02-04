using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using log4net;
using Microsoft.Owin.Hosting;
using Toec_Common.Dto;
using Toec_Services;
using Toec_Services.ApiCall;

namespace Toec_UI
{
    internal static class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static string GetServicePort()
        {
            try
            {
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "Toec");
                var text = File.ReadAllText(Path.Combine(path, "TraySettings"));
                int value;
                if (!int.TryParse(text, out value))
                {
                    Logger.Error("Could Not Parse Tray App Settings File.  Tray App Will Not Be Available.");
                    Environment.Exit(1);
                }
                else
                {
                    return value.ToString();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Could Not Read Tray App Settings File.  Tray App Will Not Be Available.");
                Logger.Error(ex.Message);
                Environment.Exit(1);
            }

            return "";
        }

        [STAThread]
        private static void Main()
        {
            Logger.Info("Tray App Started");
            Logger.Info("Current User: " + Environment.UserName);
            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            DtoGobalSettings.BaseCachePath = Path.Combine(basePath, "Toec", "AppData") +
                                             Path.DirectorySeparatorChar;
            DtoGobalSettings.LocalApiPort = GetServicePort();

            bool createdNew;
            using (new Mutex(true, "Toec-UI", out createdNew))
            {
                if (!createdNew) return;

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                

                var counter = 0;
                while (counter <= 12)
                {
                    counter++;
                    var trayStartupInfo = new APICall().LocalApi.ServiceStartComplete();
                    if (trayStartupInfo == null)
                    {
                        Logger.Debug("Tray App Is Waiting For Service To Finish Initializing... ");
                        Task.Delay(10*1000).Wait();
                        continue;
                    }
                    if (trayStartupInfo.ServiceStarted)
                    {
                        DtoGobalSettings.ClientIdentity = new DtoClientIdentity();
                        DtoGobalSettings.ComServer = trayStartupInfo.ComServer;
                        DtoGobalSettings.ClientIdentity.Guid = trayStartupInfo.ComputerGuid;
                        DtoGobalSettings.ClientIdentity.Name = trayStartupInfo.ComputerName;
                        DtoGobalSettings.ShutdownDelay = trayStartupInfo.ShutdownDelay;
                        new ServiceLogLevel().Set(trayStartupInfo.LogLevel);
                        int value;
                        if (int.TryParse(trayStartupInfo.TrayAppPort, out value))
                        {
                            WebApp.Start<Startup>("http://localhost:" + value);
                        }
                        new ServiceTriggerAction().Login();
                        break;
                    }

                    Logger.Debug("Tray App Is Waiting For Service To Finish Initializing... ");

                    if (counter == 12)
                    {
                        Logger.Error("Could Not Contact The Toec Service.  Exiting.");
                        Environment.Exit(1);
                    }

                    Task.Delay(10*1000).Wait();
                }
            
                Application.Run();
              

            }
        }
    }
}