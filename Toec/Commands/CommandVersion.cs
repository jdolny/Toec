using System;
using Toec_Common.Dto;

namespace Toec.Commands
{
    public class CommandVersion : ICommand
    {
        public void Run()
        {
            Console.WriteLine(DtoGobalSettings.ClientVersion);
        }
    }
}