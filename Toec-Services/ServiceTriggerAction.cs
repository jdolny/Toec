using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using log4net;
using Microsoft.AspNet.SignalR.Client;
using Toec_Common.Dto;
using Toec_Common.Enum;
using Toec_Services.ApiCall;
using Toec_Services.Policy;
using Toec_Services.Socket;

namespace Toec_Services
{
    public class ServiceTriggerAction : IDisposable
    {
        private const int MillisecondsPerMinute = 60000;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private Timer _checkinTimer;
        private Timer _startupRetryTime;
        private HubConnection _hubConnection;
        private ServiceSocket _serviceSocket;
        public ServiceTriggerAction()
        {
            _serviceSocket = new ServiceSocket(_hubConnection);
        }

        public void Checkin()
        {
            //This is only called via a force checkin from the server and does not modify the checkin timer
            Logger.Info("Manual Checkin Request Received");
            Logger.Info("Checking For Checkin Policies");
            new ServiceActiveComServer().Set();
            new PolicyRunner(EnumPolicy.Trigger.Checkin).Run();
            Logger.Info("Trigger Action: Checkin. Complete.");
        }

        public void Login()
        {
            Logger.Info("Checking For Login Policies");
            new PolicyRunner(EnumPolicy.Trigger.Login).Run();
            Logger.Info("Trigger Action: Login. Complete.");
        }

        private void RecurringCheckin(object source, ElapsedEventArgs e)
        {
            if (DtoGobalSettings.PolicyIsRunning)
            {
                Logger.Info("A Previous Policy Is Still Running.  Checkin Skipped.");
                return;
            }

            while (true)
            {
                var startupInfo = new APICall().ProvisionApi.GetStartupInfo(DtoGobalSettings.ClientIdentity.Name);
                if (startupInfo != null)
                {
                    if (startupInfo.ExpectedClientVersion != null)
                    {

                        if (!new ServiceUpdate().CheckUpdate(startupInfo.ExpectedClientVersion))
                        {
                            Task.Delay(300*1000).Wait();
                            continue;
                        }
                        break;
                    }
                    break;
                }
                break;
            }

            Logger.Info("Checking For Checkin Policies");
            new ServiceActiveComServer().Set();
            _hubConnection = _serviceSocket.ConnectSocket();
            new PolicyRunner(EnumPolicy.Trigger.Checkin).Run();
            if (Convert.ToInt32(_checkinTimer.Interval) != DtoGobalSettings.CheckinTime*MillisecondsPerMinute)
            {
                Logger.Info(string.Format("Check In Time Has Been Updated To {0}", DtoGobalSettings.CheckinTime));
                _checkinTimer.Stop();
                _checkinTimer.Interval = DtoGobalSettings.CheckinTime*MillisecondsPerMinute;
                _checkinTimer.Start();
            }
            Logger.Info("Trigger Action: Checkin. Complete.");
        }

        public void Startup()
        {
            _hubConnection = _serviceSocket.ConnectSocket();
            Logger.Info("Checking For Startup Policies");
            var result = new PolicyRunner(EnumPolicy.Trigger.Startup).Run();
            if (result)
            {
               
                Logger.Info(string.Format("Check In Time Set To {0}", DtoGobalSettings.CheckinTime));
                _checkinTimer = new Timer(DtoGobalSettings.CheckinTime*MillisecondsPerMinute);
                _checkinTimer.Elapsed += RecurringCheckin;
                _checkinTimer.Enabled = true;
                Logger.Info("Trigger Action: Startup. Complete.");
            }
            else
            {
                Logger.Error("Startup Checkin Failed.  Trying Again In 1 Minute");
                //start a timer to try and check in every 1 minutes if checkin failed
                _startupRetryTime = new Timer(1*MillisecondsPerMinute);
                _startupRetryTime.Elapsed += StartupRetry;
                _startupRetryTime.Enabled = true;
            }
        }

        private void StartupRetry(object source, ElapsedEventArgs e)
        {
            new ServiceActiveComServer().Set();
            _hubConnection = _serviceSocket.ConnectSocket();
            var result = new PolicyRunner(EnumPolicy.Trigger.Startup).Run();
            if (result)
            {
                _checkinTimer = new Timer(DtoGobalSettings.CheckinTime*MillisecondsPerMinute);
                _checkinTimer.Elapsed += RecurringCheckin;
                _checkinTimer.Enabled = true;
                _startupRetryTime.Stop();
                _startupRetryTime.Enabled = false;

                Logger.Info("Trigger Action: Startup. Complete.");
            }
            else
            {
                Logger.Error("Checkin Failed.  Trying Again In 1 Minute");
            }
        }

        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if(_checkinTimer != null)
                    _checkinTimer.Dispose();
                    if(_startupRetryTime != null)
                    _startupRetryTime.Dispose();
                    if(_hubConnection != null)
                    _hubConnection.Dispose();
                }
            }
            this.disposed = true;
        }
    }
}