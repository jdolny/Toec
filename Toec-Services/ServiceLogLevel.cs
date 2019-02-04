using System;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using Toec_Common.Dto;

namespace Toec_Services
{
    public class ServiceLogLevel
    {
        public void Set(string logLevel)
        {
            if (!string.IsNullOrEmpty(logLevel))
            {
                if (logLevel.ToLower().Equals("debug"))
                {
                    ((Hierarchy)LogManager.GetRepository()).Root.Level = Level.Debug;
                    DtoGobalSettings.LogLevel = Level.Debug;
                }
                else if (logLevel.ToLower().Equals("info"))
                {
                    ((Hierarchy)LogManager.GetRepository()).Root.Level = Level.Info;
                    DtoGobalSettings.LogLevel = Level.Info;
                }
                else if (logLevel.ToLower().Equals("error"))
                {
                    ((Hierarchy)LogManager.GetRepository()).Root.Level = Level.Error;
                    DtoGobalSettings.LogLevel = Level.Error;
                }
                else
                    DtoGobalSettings.LogLevel =
                        ((Hierarchy)LogManager.GetRepository()).Root.Level;

                ((Hierarchy)LogManager.GetRepository()).RaiseConfigurationChanged(
                    EventArgs.Empty);
            }
            else
            {
                DtoGobalSettings.LogLevel =
                    ((Hierarchy)LogManager.GetRepository()).Root.Level;
            }
        }

    }
}
