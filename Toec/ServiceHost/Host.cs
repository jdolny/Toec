using System;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using log4net;
using Microsoft.Owin.Hosting;
using Toec_Common.Dto;
using Toec_Services;
using Toec_Services.Entity;

namespace Toec.ServiceHost
{
    internal partial class Host : ServiceBase
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static Thread _serviceStartThread;
        private static Thread _serviceStopThread;
        private IDisposable _localServer;

        public Host()
        {
            InitializeComponent();
            CanShutdown = true;
            _serviceStartThread = new Thread(DoWork);
            _serviceStopThread = new Thread(LogOutUsers);
        }

        private void DoWork()
        {
            var startupResult = new ServiceInitialize().OnStartTasks();
            if (startupResult)
            {
                try
                {
                    if (!string.IsNullOrEmpty(DtoGobalSettings.LocalApiPort))
                        _localServer =
                            WebApp.Start<Toec_LocalApi.Startup>("http://localhost:" + DtoGobalSettings.LocalApiPort);
                    else
                        Logger.Info("Local API Calls To This Device Have Been Disabled.  Port Is Not Defined.");
                }
                catch (Exception ex)
                {
                    Logger.Error("Error Starting Local API", ex);
                }
                new ServiceTriggerAction().Startup();
            }
        }

        private void LogOutUsers()
        {
            Logger.Debug("Marking All Users As Logged Out.");
            new ServiceUserTracker().LogoutAllUsers();
            new ServiceAppMonitor().CloseAllOpen();
        }

        public void ManualStart()
        {
            OnStart(new string[0]);
        }

        public void ManualStop()
        {
            OnStop();
        }

        protected override void OnStart(string[] args)
        {
            Logger.Info("Starting Toec Service");
            _serviceStartThread.Start();
        }

        protected override void OnStop()
        {
            Logger.Debug("Service Is Stopping.");
            _serviceStopThread.Start();
          
            if (_localServer != null)
            {
                _localServer.Dispose();
            }
            base.OnStop();
        }

        protected override void OnShutdown()
        {
            Logger.Debug("System Is Shutting Down.");
            _serviceStopThread.Start();
            base.OnShutdown();
        }
    }
}