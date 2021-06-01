using System;

namespace Toec.Commands
{
    public class CommandFactory
    {
        public static ICommand GetCommand(string[] args)
        {
            if (args == null || args.Length == 0)
                return new CommandService();

            var str = args[0];
            switch (str.ToLower())
            {
                case "--version":
                    return new CommandVersion();
                case "--console":
                    return new CommandConsole();
                case "--resetfull":
                    return new CommandResetFull();
                case "--resetpartial":
                    return new CommandResetPartial();
                case "--prepareimage":
                    return new CommandPrepareImage(args);
                case "--prepareimagegui":
                    return new CommandPrepareImageGui(args);
                case "--resetkey":
                    return new CommandResetKey(args);
                case "--loglevel":
                    return new CommandLogLevel(args);
                case "--comservers":
                    return new CommandComServer(args);
                default:
                    DisplayHelpMenu();
                    Environment.Exit(1);
                    return null;
            }
        }

        private static void DisplayHelpMenu()
        {
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("Displaying Toec Help Menu.");
            Console.WriteLine();
            Console.Write("--version\t\t\t\t");
            Console.WriteLine("Displays the current version of Toec");
            Console.Write("--console\t\t\t\t");
            Console.WriteLine("Runs Toec in console mode for debugging purposes");
            Console.Write("--logLevel [INFO|DEBUG|ERROR]\t\t");
            Console.WriteLine("Updates the log level");
            Console.Write("--resetFull\t\t\t\t");
            Console.WriteLine("Resets Toec back to a new installation state");
            Console.Write("--resetPartial\t\t\t\t");
            Console.WriteLine("Tells endpoint to re-provision with server");
            Console.Write("--resetKey [SERVER_KEY] [CA_THUMBPRINT] ");
            Console.WriteLine("Changes the server key and certificate thumbprint for the endpoint");
            Console.Write("--prepareImage\t\t\t\t");
            Console.WriteLine("Prepares the Toec client for image capture");
            Console.Write("--comServers [COM_SERVERS]\t\t");
            Console.WriteLine("Manually update the endpoint's com servers");

            Console.WriteLine();
            Console.WriteLine();

        }
    }
}