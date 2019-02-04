using System;
using System.Security.Principal;
using Toec_Services;

namespace Toec.Commands
{
    public class CommandResetKey : ICommand
    {
        private readonly string[] _args;
        public CommandResetKey(string[] args)
        {
            _args = args;
        }
        public void Run()
        {
            if (!HasAdministrativeRight())
            {
                Console.WriteLine("Administrative Privileges Required To Reset Server Key");
            }
            else
            {
                if (_args.Length != 3) return;
                Console.WriteLine("Toec Server Key Reset Initiated");
                new ServiceResetServerKey().Reset(_args[1],_args[2]);
            }
        }

        public static bool HasAdministrativeRight()
        {
            var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}