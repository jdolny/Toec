using System.ServiceProcess;
using Toec.ServiceHost;

namespace Toec.Commands
{
    public class CommandService : ICommand
    {
        public void Run()
        {
            ServiceBase.Run(new Host());
        }
    }
}