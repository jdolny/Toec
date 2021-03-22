namespace Toec.Commands
{
    public class CommandFactory
    {
        public static ICommand GetCommand(string[] args)
        {
            if (args == null || args.Length == 0)
                return new CommandService();

            var str = args[0];
            switch (str)
            {
                case "--version":
                    return new CommandVersion();
                case "--console":
                    return new CommandConsole();
                case "--resetFull":
                    return new CommandResetFull();
                case "--resetPartial":
                    return new CommandResetPartial();
                case "--prepareImage":
                    return new CommandPrepareImage(args);
                case "--resetKey":
                    return new CommandResetKey(args);
                case "--logLevel":
                    return new CommandLogLevel(args);
                case "--comServers":
                    return new CommandComServer(args);
                default:
                    return null;
            }
        }
    }
}