using System.ComponentModel;
using System.Configuration.Install;

namespace Toec.ServiceHost
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }
    }
}