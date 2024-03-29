﻿using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toec_Common.Dto;
using Toec_Common.Modules;
using Toec_Services.ApiCall;
using Toec_Services.Entity;
using Toec_Services.Policy;
using Toec_Services.Policy.Modules;

namespace Toec_Services.Socket
{
    public class ServiceHubAction
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public void Process(DtoHubAction action)
        {
            Logger.Debug("Received Hub Action");
            Logger.Debug(action.Action);
            switch (action.Action)
            {
                case "Collect_Inventory":
                    var t = new Thread(RunInventory);
                    t.Start();
                    
                    break;

                case "Current_Users":
                    var cu = new Thread(GetLoggedInUsers);
                    cu.Start();
                    break;

                case "Get_Status":
                    var gs = new Thread(GetStatus);
                    gs.Start();
                    break;

                case "Message":
                    var message = JsonConvert.DeserializeObject<DtoMessage>(action.Message);
                    var activeTrayPorts = new ServiceSetting().GetSetting("active_login_ports").Value;
                    foreach (var port in activeTrayPorts.Split(','))
                    {
                        var localPort = port;
                        var m = new Thread(() => RunMessage(message.Message, message.Title, localPort, message.Timeout));
                        m.Start();
                    }
                    new APICall().PolicyApi.UpdateLastSocketResult(new DtoStringResponse() { Value = "Message Sent" });
                    break;

                case "Force_Checkin":
                    var f = new Thread(RunCheckin);
                    f.Start();
                    break;

                case "Reboot":
                    Logger.Info("Server Issued Reboot Request.  Rebooting.");
                    var r = new Thread(() => RunReboot(action.Message));
                    r.Start();
                    break;

                case "Shutdown":
                    Logger.Info("Server Issued Shutdown Request.  Shutting Down.");
                    var s = new Thread(() => RunShutdown(action.Message));
                    s.Start();
                    break;

                case "Start_Remote_Control":
                    var sm = new Thread(() => RunStartRemoteControl());
                    sm.Start();
                    break;

                case "Wol_Task":
                    var wolTask = JsonConvert.DeserializeObject<DtoWolTask>(action.Message);
                    var w = new Thread(() => RunWakeup(wolTask));
                    w.Start();
                    break;

                case "Run_Module":
                    var policy = JsonConvert.DeserializeObject<DtoClientPolicy>(action.Message);
                    var rsm = new Thread(() => RunSingleModule(policy));
                    rsm.Start();
                    break;

                case "Logs":
                    var logType = action.Message;
                    var l = new Thread(() => SendLog(logType));
                    l.Start();
                    break;

                case "System_Uptime":
                    var uptime = new Thread(GetUptime);
                    uptime.Start();
                    break;
                case "WinPE_Image":
                    var policyWinPe = JsonConvert.DeserializeObject<DtoClientPolicy>(action.Message);
                    var winpeImaging = new Thread(() => RunStartImagingFromWindows(policyWinPe));
                    winpeImaging.Start();
                    break;

                default:
                    Logger.Info("Action Was Not Recognized.");
                    break;
            }
        }

        private void SendLog(string logType)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            path = Path.Combine(path, "Toec", "logs",logType);
            try
            {
                var log = File.ReadLines(path).Reverse().Take(10000).Reverse().ToList();

                var sb = new StringBuilder();
                foreach (var line in log)
                {
                    sb.Append(line);
                    sb.Append(Environment.NewLine);
                }
                new APICall().PolicyApi.UpdateLastSocketResult(new DtoStringResponse() { Value = sb.ToString() });
            }
            catch
            {
                new APICall().PolicyApi.UpdateLastSocketResult(new DtoStringResponse() { Value = "Error: Could Not Open Log" });
            }
        }
        private void RunSingleModule(DtoClientPolicy policy)
        {
            new APICall().PolicyApi.UpdateLastSocketResult(new DtoStringResponse() { Value = "Instant Module Started Successfully" });
            new PolicyRunner(Toec_Common.Enum.EnumPolicy.Trigger.Checkin).Run(policy);
           
        }

        private void RunStartImagingFromWindows(DtoClientPolicy policy)
        {
            new APICall().PolicyApi.UpdateLastSocketResult(new DtoStringResponse() { Value = "Image From Windows Task Started" });
            new PolicyRunner(Toec_Common.Enum.EnumPolicy.Trigger.Checkin).Run(policy);
        }

        private void RunStartRemoteControl()
        {
            new StartRemoteControl().Run();
        }

        private void RunWakeup(DtoWolTask wolTask)
        {
            new APICall().PolicyApi.UpdateLastSocketResult(new DtoStringResponse() { Value = "WOL Task Started" });
            ServiceWolRelay.WakeUp(wolTask);
        }

        private void RunInventory()
        {
            new APICall().PolicyApi.UpdateLastSocketResult(new DtoStringResponse() { Value = "Inventory Collection Started" });
            new ModuleInventory().Run();
        }

        private void RunCheckin()
        {
            new APICall().PolicyApi.UpdateLastSocketResult(new DtoStringResponse() { Value = "Force Checkin Started" });
            new ServiceTriggerAction().Checkin();
        }

        private void RunMessage(string message, string title, string port, int timeout)
        {
            new APICall().TrayApi.SendMessage(message, title, port, timeout);
        }

        private void RunReboot(string delay)
        {
            new APICall().PolicyApi.UpdateLastSocketResult(new DtoStringResponse() { Value = "Reboot Initiated" });
            new ServiceUserTracker().LogoutAllUsers();
            new ServiceAppMonitor().CloseAllOpen();
            System.Diagnostics.Process.Start("shutdown.exe", "/r /t " + delay);
        }

        private void RunShutdown(string delay)
        {
            new APICall().PolicyApi.UpdateLastSocketResult(new DtoStringResponse() { Value = "Shutdown Initiated" });
            new ServiceUserTracker().LogoutAllUsers();
            new ServiceAppMonitor().CloseAllOpen();
            System.Diagnostics.Process.Start("shutdown.exe", "/s /t " + delay);
        }

        private void GetLoggedInUsers()
        {
            var listUsers = new ServiceUserLogins().GetUsersLoggedIn();
            var users = string.Empty;
            foreach (var user in listUsers)
            {
                users += user + ",";
            }
            if (string.IsNullOrEmpty(users))
                users = "There Are No Users Currently Logged On";
            new APICall().PolicyApi.UpdateLastSocketResult(new DtoStringResponse() { Value = users.Trim(',') });
        }

        public void GetStatus()
        {
            new APICall().PolicyApi.UpdateLastSocketResult(new DtoStringResponse() { Value = "Connected" });
        }

        public void GetUptime()
        {
            try
            {
                using (var uptime = new PerformanceCounter("System", "System Up Time"))
                {
                    TimeSpan systemUptime;
                    uptime.NextValue();
                    systemUptime = TimeSpan.FromSeconds(uptime.NextValue());
                    var str = systemUptime.ToString(@"d\ \D\a\y\s\ h\ \H\o\u\r\s\ m\ \M\i\n\u\t\e\s\ s\ \S\e\c\o\n\d\s");
                    new APICall().PolicyApi.UpdateLastSocketResult(new DtoStringResponse() { Value = str});
                }
            }
            catch
            {

            }
            
        }
    }
}
