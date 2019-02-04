using Microsoft.Deployment.WindowsInstaller;

namespace Toec_InstallHelper
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult CreateFirewallException(Session session)
        {
            return new Firewall(session).CreateException();
        }

        [CustomAction]
        public static ActionResult InitializeDatabase(Session session)
        {
            return new Database(session).Initialize();
        }
    }
}