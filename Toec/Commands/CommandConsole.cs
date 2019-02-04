using System;
using Toec.ServiceHost;

namespace Toec.Commands
{
    public class CommandConsole : ICommand
    {
        public void Run()
        {
            var serviceHost = new Host();
            serviceHost.ManualStart();

            Console.WriteLine();
            Console.WriteLine("Toec Running");
            Console.WriteLine("Press [Enter] to Exit.");
            Console.Read();
            serviceHost.ManualStop();
        }
    }
}