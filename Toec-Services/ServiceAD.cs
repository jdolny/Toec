using System;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Reflection;
using log4net;

namespace Toec_Services
{
    public class ServiceAD
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public string GetADGuid(string computerName)
        {
            var domain = "";
            try
            {
                domain = Domain.GetCurrentDomain().Name;

                var sADPath = string.Format("LDAP://{0}", domain);
                var de = new DirectoryEntry(sADPath);

                var sFilter = "(&(objectCategory=computer)(name=" + computerName + "))";
                var directorySearcher = new DirectorySearcher(de, sFilter);
                var DirectorySearchResult = directorySearcher.FindOne();

                if (null != DirectorySearchResult)
                {
                    var deComp = DirectorySearchResult.GetDirectoryEntry();
                    Logger.Info("AD Computer Guid: " + deComp.Guid);
                    return deComp.Guid.ToString();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Active Directory Search Failed: {0}", domain));
                Logger.Error(ex.Message);
                return null;
            }
            return null;
        }
    }
}