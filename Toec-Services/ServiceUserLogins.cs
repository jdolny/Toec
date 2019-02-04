using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;
using log4net;

namespace Toec_Services
{
    //Code from the fog project
    public class ServiceUserLogins
    {
        private class NativeMethods
        {
            [DllImport("user32.dll")]
            public static extern bool GetLastInputInfo(ref Lastinputinfo plii);

            [DllImport("Wtsapi32.dll")]
            public static extern void WTSFreeMemory(IntPtr pointer);

            [DllImport("Wtsapi32.dll")]
            public static extern bool WTSQuerySessionInformation(IntPtr hServer, int sessionId, WtsInfoClass wtsInfoClass,
                out IntPtr ppBuffer, out int pBytesReturned);
        }

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public enum WtsInfoClass
        {
            WTSInitialProgram,
            WTSApplicationName,
            WTSWorkingDirectory,
            WTSOEMId,
            WTSSessionId,
            WTSUserName,
            WTSWinStationName,
            WTSDomainName,
            WTSConnectState,
            WTSClientBuildNumber,
            WTSClientName,
            WTSClientDirectory,
            WTSClientProductId,
            WTSClientHardwareId,
            WTSClientAddress,
            WTSClientDisplay,
            WTSClientProtocolType,
            WTSIdleTime,
            WTSLogonTime,
            WTSIncomingBytes,
            WTSOutgoingBytes,
            WTSIncomingFrames,
            WTSOutgoingFrames,
            WTSClientInfo,
            WTSSessionInfo
        }

        public int GetInactivityTime()
        {
            var lastInputInfo = new Lastinputinfo();
            lastInputInfo.CbSize = (uint) Marshal.SizeOf(lastInputInfo);
            lastInputInfo.DwTime = 0;

            var envTicks = (uint) Environment.TickCount;

            if (!NativeMethods.GetLastInputInfo(ref lastInputInfo))
                return 0;

            var lastInputTick = lastInputInfo.DwTime;
            var idleTime = envTicks - lastInputTick;

            return (int) idleTime/1000;
        }

    

        /// <summary>
        ///     Get all active session IDs
        /// </summary>
        /// <returns>A list of session IDs</returns>
        private static List<int> GetSessionIds()
        {
            var sessionIds = new List<int>();
            var properties = new[] {"SessionId"};

            var query = new SelectQuery("Win32_Process", "", properties); //SessionId
            var searcher = new ManagementObjectSearcher(query);

            foreach (var envVar in searcher.Get())
            {
                try
                {
                    if (!sessionIds.Contains(int.Parse(envVar["SessionId"].ToString())))
                        sessionIds.Add(int.Parse(envVar["SessionId"].ToString()));
                }
                catch (Exception ex)
                {
                    Logger.Error("Could Not Read User Session Ids");
                    Logger.Error(ex.Message);
                }
            }

            return sessionIds;
        }

        /// <summary>
        ///     Convert a session ID to its correlating username
        /// </summary>
        /// <param name="sessionId">The session ID to use</param>
        /// <param name="prependDomain">If the user's domain should be prepended</param>
        /// <returns>The username</returns>
        //https://stackoverflow.com/questions/19487541/get-windows-user-name-from-sessionid
        public static string GetUserNameFromSessionId(int sessionId, bool prependDomain)
        {
            IntPtr buffer;
            int strLen;
            var username = "SYSTEM";
            if (!NativeMethods.WTSQuerySessionInformation(IntPtr.Zero, sessionId, WtsInfoClass.WTSUserName, out buffer, out strLen) ||
                strLen <= 1) return username;
            username = Marshal.PtrToStringAnsi(buffer);
            NativeMethods.WTSFreeMemory(buffer);
            if (!prependDomain) return username;
            if (
                !NativeMethods.WTSQuerySessionInformation(IntPtr.Zero, sessionId, WtsInfoClass.WTSDomainName, out buffer, out strLen) ||
                strLen <= 1) return username;
            username = Marshal.PtrToStringAnsi(buffer) + "\\" + username;
            NativeMethods.WTSFreeMemory(buffer);
            return username;
        }

        public List<string> GetUsersLoggedIn()
        {
            var sessionIds = GetSessionIds();

            return (from sessionId in sessionIds
                select GetUserNameFromSessionId(sessionId, true)).Where(x => !x.Equals("SYSTEM")).Distinct().ToList();
        }

       

       

        internal struct Lastinputinfo
        {
            public uint CbSize;
            public uint DwTime;
        }
    }

}