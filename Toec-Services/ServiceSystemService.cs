using System;
using System.Reflection;
using System.ServiceProcess;
using log4net;

namespace Toec_Services
{
    public class ServiceSystemService
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public bool RestartPrintSpooler()
        {
            var service = new ServiceController("Spooler");
            try
            {
                var timeout = TimeSpan.FromMilliseconds(30000);
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                return false;
            }
        }

        public bool StopToec()
        {
            var service = new ServiceController("Toec");
            try
            {
                if (service.Status != ServiceControllerStatus.Stopped)
                    return false;
                else
                    return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                return false;
            }
        }

        public bool RestartToec()
        {
            var service = new ServiceController("Toec");
            try
            {
                var timeout = TimeSpan.FromMilliseconds(30000);
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                return false;
            }
        }
    }
}