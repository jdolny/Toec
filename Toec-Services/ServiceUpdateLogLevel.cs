using System.Reflection;
using log4net;
using Toec_Services.Entity;

namespace Toec_Services
{
    public class ServiceUpdateLogLevel
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public bool Update(string logLevel)
        {
            Logger.Info("Updating Log Level");
           

            var serviceSetting = new ServiceSetting();
            var logLevelEntity = new ServiceSetting().GetSetting("log_level");
            logLevelEntity.Value = logLevel;
            serviceSetting.UpdateSettingValue(logLevelEntity);

            Logger.Info("Updating Log Level Finished");
            return true;
        }
    }
}