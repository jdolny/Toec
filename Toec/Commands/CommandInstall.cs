using System;
using System.Configuration.Install;
using System.Reflection;
using System.Security.Principal;

namespace Toec.Commands
{
    internal class CommandInstall : ICommand
    {
        private readonly string _installType;

        public CommandInstall(string type)
        {
            _installType = type;
        }

        public void Run()
        {
            if (_installType.Equals("install"))
                Install();
            else if (_installType.Equals("uninstall"))
                Uninstall();
        }

        public static bool HasAdministrativeRight()
        {
            var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public void Install()
        {
            if (!HasAdministrativeRight())
            {
                Console.WriteLine("Administrative Privileges Required To Install Service");
            }
            else
            {
                Console.WriteLine("Installing Toec");
                try
                {
                    var installer = new AssemblyInstaller(Assembly.GetExecutingAssembly(), new string[] {});
                    installer.UseNewContext = true;
                    installer.Install(null);
                    installer.Commit(null);
                    Console.WriteLine();
                    Console.WriteLine("Successfully Installed Toec");
                    Console.WriteLine("The Service Must Manually Be Started");
                }
                catch (Exception ex)
                {
                    Console.WriteLine();
                    Console.WriteLine("Could Not Install Toec");
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private static void Uninstall()
        {
            if (!HasAdministrativeRight())
            {
                Console.WriteLine("Administrative Privileges Required To Remove Service");
            }
            else
            {
                Console.WriteLine("UnInstalling Toec");
                try
                {
                    var installer = new AssemblyInstaller(Assembly.GetExecutingAssembly(), new string[] {});
                    installer.UseNewContext = true;
                    installer.Uninstall(null);
                    installer.Commit(null);
                    Console.WriteLine();
                    Console.WriteLine("Successfully UnInstalled Toec");
                    Console.WriteLine("The Service Must Manually Be Started");
                }
                catch (Exception ex)
                {
                    Console.WriteLine();
                    Console.WriteLine("Could Not UnInstall Toec");
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}