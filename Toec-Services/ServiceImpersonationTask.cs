using System;
using System.Reflection;
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

                //Not sure if this actually helps in any way.
                //Anyone care to share a better way?
                for (int i = 0; i < 20; i++)
                {
                    credentials.Username = "000000";
                    credentials.Password = "111111";
                    credentials.Password = "000000";
                    credentials.Username = "111111";
                }

                ts.Run();

                //Give the task some time to start
                //If it hasn't started in 5 minutes skip for now
               /* int counter = 1;
                while (ts.State == TaskState.Queued || ts.State == TaskState.Ready)
                {
                    if (counter == 61)
                    {
                        ts.Stop();
                        ts.TaskService.RootFolder.DeleteTask("Toec Impersonation Task " + ModuleGuid );
                        return -1;
                    }

                    System.Threading.Tasks.Task.Delay(5 * 1000).Wait();
                    counter++;
                }
                */
                //Wait for task to finish up to Execution Timeout - handled by windows task scheduler
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
        }

       
    }
}
