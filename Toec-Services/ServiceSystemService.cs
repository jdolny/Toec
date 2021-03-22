using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using log4net;

namespace Toec_Services
{
    public class ServiceSystemService
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public bool IsRemotelyInstalled()
        {
            return ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == "Remotely_Service") != null;
        }

        public bool StopRemotelyService()
        {
            var service = new ServiceController("Remotely_Service");
            try
            {
                var timeout = TimeSpan.FromMilliseconds(30000);
                if (service.Status == ServiceControllerStatus.Running)
                {
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                }
                service.Close();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                return false;
            }
        }

        public bool StartRemotelyService()
        {
            var service = new ServiceController("Remotely_Service");
            try
            {
                var timeout = TimeSpan.FromMilliseconds(30000);
                service.Start();
                //service.WaitForStatus(ServiceControllerStatus.StartPending, timeout);
                service.Close();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                return false;
            }
        }

        public bool DisableRemotelyServiceStartup()
        {
            var service = new ServiceController("Remotely_Service");
            try
            {
                var timeout = TimeSpan.FromMilliseconds(30000);
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                ServiceHelper.ChangeStartMode(service, ServiceStartMode.Manual);
                service.Close();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                return false;
            }
        }

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
                if (service.Status == ServiceControllerStatus.Stopped)
                    return true;
                var timeout = TimeSpan.FromMilliseconds(30000);
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
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

    public static class ServiceHelper
    {
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern Boolean ChangeServiceConfig(
            IntPtr hService,
            UInt32 nServiceType,
            UInt32 nStartType,
            UInt32 nErrorControl,
            String lpBinaryPathName,
            String lpLoadOrderGroup,
            IntPtr lpdwTagId,
            [In] char[] lpDependencies,
            String lpServiceStartName,
            String lpPassword,
            String lpDisplayName);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr OpenService(
            IntPtr hSCManager, string lpServiceName, uint dwDesiredAccess);

        [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr OpenSCManager(
            string machineName, string databaseName, uint dwAccess);

        [DllImport("advapi32.dll", EntryPoint = "CloseServiceHandle")]
        public static extern int CloseServiceHandle(IntPtr hSCObject);

        private const uint SERVICE_NO_CHANGE = 0xFFFFFFFF;
        private const uint SERVICE_QUERY_CONFIG = 0x00000001;
        private const uint SERVICE_CHANGE_CONFIG = 0x00000002;
        private const uint SC_MANAGER_ALL_ACCESS = 0x000F003F;

        public static void ChangeStartMode(ServiceController svc, ServiceStartMode mode)
        {
            var scManagerHandle = OpenSCManager(null, null, SC_MANAGER_ALL_ACCESS);
            if (scManagerHandle == IntPtr.Zero)
            {
                throw new ExternalException("Open Service Manager Error");
            }

            var serviceHandle = OpenService(
                scManagerHandle,
                svc.ServiceName,
                SERVICE_QUERY_CONFIG | SERVICE_CHANGE_CONFIG);

            if (serviceHandle == IntPtr.Zero)
            {
                throw new ExternalException("Open Service Error");
            }

            var result = ChangeServiceConfig(
                serviceHandle,
                SERVICE_NO_CHANGE,
                (uint)mode,
                SERVICE_NO_CHANGE,
                null,
                null,
                IntPtr.Zero,
                null,
                null,
                null,
                null);

            if (result == false)
            {
                int nError = Marshal.GetLastWin32Error();
                var win32Exception = new Win32Exception(nError);
                throw new ExternalException("Could not change service start type: "
                    + win32Exception.Message);
            }

            CloseServiceHandle(serviceHandle);
            CloseServiceHandle(scManagerHandle);
        }
    }
}