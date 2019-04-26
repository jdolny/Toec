using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Toec_Common.Dto;

namespace Toec_Services.RemoteAccess
{
    public class Install
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public bool CheckInstallation()
        {
            return true;
            //not implemented yet
            /*
            Logger.Debug("Checking Remote Access Installation Status");
            if (string.IsNullOrEmpty(DtoGobalSettings.ExpectedRemoteAccessVersion))
            {
                //remote access is not enabled, remove if installed or do nothing if not installed
                var services = ServiceController.GetServices();
                var service = services.FirstOrDefault(x => x.ServiceName == "Mesh Agent");
                if (service != null)
                {

                    try
                    {
                        if (service.ServiceName == null)
                        {
                            var timeout = TimeSpan.FromMilliseconds(30000);
                            service.Stop();
                            service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);

                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex.Message);
                    }


                }
                return true;
            }*/
        }
        
    }
}
