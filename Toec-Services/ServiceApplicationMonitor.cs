using System;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Reflection;
using log4net;
using Toec_Common.Entity;
using Toec_Services.Entity;

namespace Toec_Services
{
    public class ServiceApplicationMonitor : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private bool _disposed;
        private readonly ManagementEventWatcher _creationWatcher;
        private readonly ManagementEventWatcher _deletionWatcher;

        public ServiceApplicationMonitor()
        {
            try
            {
                var scope = new ManagementScope(String.Format("\\\\{0}\\root\\CIMV2", "localhost"), null);
                scope.Connect();

                var creationQuery = "Select * From __InstanceCreationEvent Within 10 " +
                                    "Where TargetInstance ISA 'Win32_Process' AND TargetInstance.SessionId != 0 AND TargetInstance.Name != 'dllHost.exe' AND TargetInstance.Name != 'SearchProtocolHost.exe' AND TargetInstance.Name != 'backgroundTaskHost.exe' AND TargetInstance.Name != 'smartscreen.exe'";

                var deletionQuery = "Select * From __InstanceDeletionEvent Within 10 " +
                                    "Where TargetInstance ISA 'Win32_Process' AND TargetInstance.SessionId != 0 AND TargetInstance.Name != 'dllHost.exe' AND TargetInstance.Name != 'SearchProtocolHost.exe' AND TargetInstance.Name != 'backgroundTaskHost.exe' AND TargetInstance.Name != 'smartscreen.exe'";

                _creationWatcher = new ManagementEventWatcher(scope, new EventQuery(creationQuery));
                _creationWatcher.EventArrived += InstanceEventHandler;

                _deletionWatcher = new ManagementEventWatcher(scope, new EventQuery(deletionQuery));
                _deletionWatcher.EventArrived += InstanceEventHandler;
            }
            catch (Exception ex)
            {
                Logger.Debug("Could Not Monitor Application Usage");
                Logger.Error(ex.Message);
            }
        }

        public void Start()
        {
            _creationWatcher.Start();
            _deletionWatcher.Start();
        }

        public void Stop()
        {
            Dispose();
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
                case "__InstanceCreationEvent":  
                    SaveCreateProcess(eventProperties);
                    break;
                case "__InstanceDeletionEvent":
                   SaveDeleteProcess(eventProperties);
                    break;
                default:
                    return;
            }
        }

        private void SaveCreateProcess(ManagementBaseObject mbo)
        {
            var appMon = new EntityAppMonitor();
            try
            {
                int sessionId;
                if (!int.TryParse(mbo["SessionId"].ToString(), out sessionId))
                    return;

                int processId;
                if (!int.TryParse(mbo["ProcessId"].ToString(), out processId))
                    return;

                
                appMon.Name = mbo["Name"].ToString();
                appMon.Path = mbo["ExecutablePath"].ToString();

                var path = appMon.Path.Substring(2).ToLower();
                if (path.StartsWith(@"\users\"))
                {
                    appMon.Path = @"%userprofile%\" + String.Join("\\", path.Split('\\').Skip(3));
                  
                }
                appMon.StartDateTime = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
                appMon.Pid = processId;
                appMon.UserName = ServiceUserLogins.GetUserNameFromSessionId(sessionId, true);
                if (appMon.UserName == "SYSTEM") return;
            }
            catch
            {
                //ignored
                return;
            }
       

            new ServiceAppMonitor().AddAppEvent(appMon);

        }

        private void SaveDeleteProcess(ManagementBaseObject mbo)
        {
            var appMon = new EntityAppMonitor();
            try
            {
                int sessionId;
                if (!int.TryParse(mbo["SessionId"].ToString(), out sessionId))
                    return;

                int processId;
                if (!int.TryParse(mbo["ProcessId"].ToString(), out processId))
                    return;
                
                appMon.Name = mbo["Name"].ToString();
                appMon.Path = mbo["ExecutablePath"].ToString();
                appMon.EndDateTime = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
                appMon.Pid = processId;
                appMon.UserName = ServiceUserLogins.GetUserNameFromSessionId(sessionId, true);
                if (appMon.UserName == "SYSTEM") return;
            }
            catch
            {
                //ignored
                return;
            }

            new ServiceAppMonitor().CloseAppEvent(appMon);
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
                _creationWatcher.Stop();
                _deletionWatcher.Stop();
                _creationWatcher.Dispose();
                _deletionWatcher.Dispose();
            }

            _disposed = true;
        }
    }
}
