using Toec.Commands;

namespace Toec
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CommandFactory.GetCommand(args).Run();
        }
    }
}