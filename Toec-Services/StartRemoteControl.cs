using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using log4net;
using System.Reflection;

namespace Toec_Services
{
    public class StartRemoteControl
    {
        public StartRemoteControl()
        {
            try
            {
                var scope = new ManagementScope(String.Format("\\\\{0}\\root\\CIMV2", "localhost"), null);
                scope.Connect();

                var deletionQuery = "Select * From __InstanceDeletionEvent Within 1 " +
                                    "Where TargetInstance ISA 'Win32_Process' AND TargetInstance.SessionId != 0 AND TargetInstance.Name = 'Remotely_Desktop.exe'";

                _deletionWatcher = new ManagementEventWatcher(scope, new EventQuery(deletionQuery));
                _deletionWatcher.EventArrived += InstanceEventHandler;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
            _systemServices = new ServiceSystemService();
        }

        public static Timer _timer;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private bool _disposed;
        private readonly ManagementEventWatcher _deletionWatcher;
        private ServiceSystemService _systemServices;


        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {

            var process = Process.GetProcessesByName("Remotely_Desktop").FirstOrDefault();
            if (process == null)
            {
                Logger.Debug("No Active Remote Sessions Found.  Stopping Remotely Service.");
                _systemServices.StopRemotelyService();
                StopWatcher();
            }

        }

        public void StartWatcher()
        {
            _timer = new Timer();
            _deletionWatcher.Start();
        }

        public void StopWatcher()
        {
            Dispose();
        }

        public void Run()
        {
            StartWatcher();

            Logger.Debug("Closing Any Existing Remote Control Sessions");
            try
            {
                foreach (var process in Process.GetProcessesByName("Remotely_Desktop"))
                {
                    process.Kill();
                }

                foreach (var process in Process.GetProcessesByName("Remotely_Agent"))
                {
                    process.Kill();
                }
            }
            catch { }

            _systemServices.StopRemotelyService();

            Logger.Debug("Starting Remotely Service");
            if (_systemServices.StartRemotelyService())
            {
                new ApiCall.APICall().PolicyApi.UpdateLastSocketResult(new Toec_Common.Dto.DtoStringResponse() { Value = "Ready" });
                _timer.Elapsed += OnTimedEvent;
                _timer.Interval = 60000;
                _timer.Enabled = true;

            }
            else
                new ApiCall.APICall().PolicyApi.UpdateLastSocketResult(new Toec_Common.Dto.DtoStringResponse() { Value = "Error:  Could Not Start Remotely Service" });

        }

        private void InstanceEventHandler(object sender, EventArrivedEventArgs e)
        {
            if (e.NewEvent == null) return;

            ManagementBaseObject eventProperties;

            try
            {
                eventProperties = (ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value;
            }
            catch
            {
                return;
            }


            string eventType = e.NewEvent.ClassPath.ClassName;
            switch (eventType)
            {
                case "__InstanceDeletionEvent":
                    Logger.Debug("Remote Session Closed.  Stopping Remotely Service");
                    _systemServices.StopRemotelyService();
                    StopWatcher();
                    break;
                default:
                    return;
            }
        }



        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if(_deletionWatcher != null)
                {
                    _deletionWatcher.Stop();
                    _deletionWatcher.Dispose();
                }
                if(_timer != null)
                {
                    _timer.Stop();
                    _timer.Dispose();
                }
            }
            _disposed = true;
        }


    }
}
