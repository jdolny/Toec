using System;
using System.Security.Principal;
using Toec_Services;

namespace Toec.Commands
{
    public class CommandLogLevel : ICommand
    {
        private readonly string[] _args;
        public CommandLogLevel(string[] args)
        {
            _args = args;
        }
        public void Run()
        {
            if (!HasAdministrativeRight())
            {
                Console.WriteLine("Administrative Privileges Required To Update Log Level");
            }
            else
            {
                if (_args.Length != 2) return;
                Console.WriteLine("Toec Log Level Update Initiated");
                new ServiceUpdateLogLevel().Update(_args[1]);
            }
        }

        public static bool HasAdministrativeRight()
        {
            var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}