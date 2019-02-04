using System.Reflection;
using log4net;
using Toec_Services.Entity;

namespace Toec_Services
{
    public class ServiceUpdateComServer
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public bool Update(string comServers)
        {
            Logger.Info("Updating Com Servers");
           

            var serviceSetting = new ServiceSetting();
            var activeComServers = new ServiceSetting().GetSetting("active_com_servers");
            activeComServers.Value = comServers;
            serviceSetting.UpdateSettingValue(activeComServers);

            var initialComServers = new ServiceSetting().GetSetting("initial_com_servers");
            initialComServers.Value = comServers;
            serviceSetting.UpdateSettingValue(initialComServers);

            Logger.Info("Updating Com Server Finished");
            return true;
        }
    }
}