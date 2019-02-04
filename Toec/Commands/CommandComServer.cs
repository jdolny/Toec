using System;
using System.Security.Principal;
using Toec_Services;

namespace Toec.Commands
{
    public class CommandComServer : ICommand
    {
        private readonly string[] _args;
        public CommandComServer(string[] args)
        {
            _args = args;
        }
        public void Run()
        {
            if (!HasAdministrativeRight())
            {
                Console.WriteLine("Administrative Privileges Required To Update Com Servers");
            }
            else
            {
                if (_args.Length != 2) return;
                Console.WriteLine("Toec Com Server Update Initiated");
                new ServiceUpdateComServer().Update(_args[1]);
            }
        }

        public static bool HasAdministrativeRight()
        {
            var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}