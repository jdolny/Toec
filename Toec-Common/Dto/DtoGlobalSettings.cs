using System;
using log4net.Core;

namespace Toec_Common.Dto
{
    public static class DtoGobalSettings
    {
        public static DtoClientIdentity ClientIdentity;

        public static Random Rnd = new Random();
        public static string BaseCachePath { get; set; }
        public static int CheckinTime { get; set; }
        public static int ShutdownDelay { get; set; }

        public static string ClientVersion
        {
            get { return "1.4.4.0"; }
        }

        public static string ComServer { get; set; }
        public static string Domain { get; set; }
        public static string LocalApiPort { get; set; }
        public static Level LogLevel { get; set; }
        public static bool PolicyIsRunning { get; set; }
        public static string RemoteApiPort { get; set; }
        public static bool ServiceStartupComplete { get; set; }
        public static DateTime ServiceStartTime { get; set; }

    }
}