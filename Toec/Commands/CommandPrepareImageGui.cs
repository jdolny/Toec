using Newtonsoft.Json;
using System;
using System.Security.Principal;
using Toec_Common.Dto;
using Toec_Services;

namespace Toec.Commands
{
    
    public class CommandPrepareImageGui : ICommand
    {
        private readonly string[] _args;
        public CommandPrepareImageGui(string[] args)
        {
            _args = args;
        }

        
        public void Run()
        {
            if (!HasAdministrativeRight())
            {
                Console.WriteLine("Administrative Privileges Required To Prepare For Image");
            }
            else
            {
                Console.WriteLine("Toec Prepare Image Initiated");
                new ServicePrepareImage().Run(GetImagePrepOptions());
            }
        }

        private DtoImagePrepOptions GetImagePrepOptions()
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory;
            var imagePrepOptions = new DtoImagePrepOptions();

            try
            {
                System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
                pProcess.StartInfo.FileName = $"{dir}\\Toec-ImagePrep.exe";
                pProcess.StartInfo.UseShellExecute = false;
                pProcess.StartInfo.RedirectStandardOutput = true;
                pProcess.StartInfo.WorkingDirectory = dir;
                pProcess.Start();
                imagePrepOptions = JsonConvert.DeserializeObject<DtoImagePrepOptions>(pProcess.StandardOutput.ReadToEnd());
                pProcess.WaitForExit();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Could Not Start Image Prep Application.");
                Console.WriteLine(ex.Message);
            }

            return imagePrepOptions;
        }

        public static bool HasAdministrativeRight()
        {
            var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}