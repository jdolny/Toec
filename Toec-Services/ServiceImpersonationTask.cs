using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using log4net;
using Microsoft.Win32.TaskScheduler;
using Toec_Common.Dto;


namespace Toec_Services
{
    public class ServiceImpersonationTask
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public string ModuleGuid { get; set; }
        public int ExecutionTimeout { get; set; }
        public string Command { get; set; }
        public string Arguments { get; set; }
        public string ImpersonationGuid { get; set; }
        public string WorkingDirectory { get; set; }

        public int RunTask()
        {
            Logger.Debug("Starting Impersonation Task");
            var credentials = new ApiCall.APICall().PolicyApi.GetImpersonationAccount(ImpersonationGuid);
            if (credentials == null)
            {
                Logger.Debug("Could Not Obtain Credentials For Impersonation Account " + ImpersonationGuid);
                return -1;
            }

            //add user to local backup operators group need for batch logon permission
            var pArgs = new DtoProcessArgs();
            pArgs.RunWith = "cmd.exe";
            pArgs.RunWithArgs = "/c ";
            pArgs.RedirectOutput = true;
            pArgs.RedirectError = true;
            pArgs.Command = "\"" + $"net localgroup \"backup operators\" {credentials.Username} /add" + "\"";
            new ServiceProcess(pArgs).RunProcess();




            TaskDefinition td = TaskService.Instance.NewTask();
            td.RegistrationInfo.Description = "Toec Impersonation Task";
            td.Principal.RunLevel = TaskRunLevel.Highest;
            td.Actions.Add(Command,Arguments,WorkingDirectory);
            td.Settings.DisallowStartIfOnBatteries = false;
            td.Settings.StopIfGoingOnBatteries = false;
            
            if(ExecutionTimeout != 0)
            {
                td.Settings.ExecutionTimeLimit = TimeSpan.FromMinutes(ExecutionTimeout);
            }

            try
            {
                var ts = TaskService.Instance.RootFolder.RegisterTaskDefinition(
                    "Toec Impersonation Task " + ModuleGuid, td, TaskCreation.CreateOrUpdate, credentials.Username, credentials.Password, TaskLogonType.Password);

           
                ts.Run();

             
                while (ts.State == TaskState.Running)
                {
                    System.Threading.Tasks.Task.Delay(5 * 1000).Wait();
                }
               
                ts.Stop();
                var exitCode = ts.LastTaskResult;
                
                ts.TaskService.RootFolder.DeleteTask("Toec Impersonation Task " + ModuleGuid);

                return exitCode;
            }
            catch (Exception ex)
            {
                Logger.Error("Could Not Run Impersonation Task");
                Logger.Error(ex.Message);
                return -1;
            }
            finally
            {
                // remove user from backup operators group
                pArgs.Command = "\"" + $"net localgroup \"backup operators\" {credentials.Username} /delete" + "\"";
                new ServiceProcess(pArgs).RunProcess();
            }
        }

       
    }
}
