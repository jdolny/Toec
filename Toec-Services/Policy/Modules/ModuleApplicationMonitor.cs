using System.Reflection;
using System.Timers;
using log4net;
using Toec_Services.Entity;

namespace Toec_Services.Policy.Modules
{
    public class ModuleApplicationMonitor
    {
        public static Timer _timer;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static bool ApplicationMonitorRunning { get; set; }
        private ServiceApplicationMonitor _applicationMonitor;

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            var users = new ServiceUserLogins().GetUsersLoggedIn();
            if (users.Count > 0)
            {
                if (ApplicationMonitorRunning) return;
                Logger.Debug("Discovered Logged In Users.  Starting Application Monitor.");
                _applicationMonitor = new ServiceApplicationMonitor();
                _applicationMonitor.Start();
                ApplicationMonitorRunning = true;
            }
            else
            {
                if (_applicationMonitor == null) return;
                if (!ApplicationMonitorRunning) return;
                Logger.Debug("All Users Have Logged Out.  Stopping Application Monitor.");
                //close any open applications since the service start time, in case they didn't close, should help with correctly tracking sign off instead of closing app
                new ServiceAppMonitor().CloseSinceServiceStart();
                _applicationMonitor.Stop();
                ApplicationMonitorRunning = false;
            }
        }

        public void Run()
        {
            Logger.Debug("Running Application Monitor Module");
            if (ApplicationMonitorRunning)
                return;
            _timer = new Timer();
            _timer.Elapsed += OnTimedEvent;
            _timer.Interval = 10000;
            _timer.Enabled = true;
            OnTimedEvent(null, null);
        }
    }
}