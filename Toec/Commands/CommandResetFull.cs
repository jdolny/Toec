using System;
using System.Security.Principal;
using Toec_Services;

namespace Toec.Commands
{
    public class CommandResetFull : ICommand
    {
        public void Run()
        {
            if (!HasAdministrativeRight())
            {
                Console.WriteLine("Administrative Privileges Required To Reset Toec");
            }
            else
            {
                Console.WriteLine("Toec Full Reset Initiated");
                new ServiceReset().HardReset("Full");
            }
        }

        public static bool HasAdministrativeRight()
        {
            var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}