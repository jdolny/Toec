﻿using System.Collections.Generic;
using Toec_Common.Dto;
using Toec_Common.Enum;

namespace Toec_Common.Modules
{
    public class DtoClientPolicy
    {
        public DtoClientPolicy()
        {
            CommandModules = new List<DtoClientCommandModule>();
            FileCopyModules = new List<DtoClientFileCopyModule>();
            PrinterModules = new List<DtoClientPrinterModule>();
            ScriptModules = new List<DtoClientScriptModule>();
            SoftwareModules = new List<DtoClientSoftwareModule>();
            WuModules = new List<DtoClientWuModule>();
            MessageModules = new List<DtoClientMessageModule>();
            WinPeModules = new List<DtoClientWinPeModule>();
            WingetModules = new List<DtoClientWingetModule>();
            SkipServerResult = false;
        }

        public List<DtoClientCommandModule> CommandModules { get; set; }
        public List<DtoClientWingetModule> WingetModules { get; set; }
        public EnumPolicy.CompletedAction CompletedAction { get; set; }
        public EnumPolicy.ErrorAction ErrorAction { get; set; }
        public EnumPolicy.ExecutionType ExecutionType { get; set; }
        public List<DtoClientFileCopyModule> FileCopyModules { get; set; }
        public EnumPolicy.FrequencyMissedAction FrequencyMissedAction { get; set; }
        public EnumPolicy.Frequency Frequency { get; set; }
        public string Guid { get; set; }
        public string Hash { get; set; }
        public EnumPolicy.InventoryAction IsInventory { get; set; }
        public EnumPolicy.RemoteAccess RemoteAccess { get; set; }
        public bool IsLoginTracker { get; set; }
        public bool IsApplicationMonitor { get; set; }
        public bool JoinDomain { get; set; }
        public string DomainOU { get; set; }
        public bool ImagePrepCleanup { get; set; }
        public EnumPolicy.LogLevel LogLevel { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public List<DtoClientPrinterModule> PrinterModules { get; set; }
        public bool RemoveInstallCache { get; set; }
        public bool ReRunExisting { get; set; }
        public List<DtoClientScriptModule> ScriptModules { get; set; }
        public List<DtoClientSoftwareModule> SoftwareModules { get; set; }
        public List<DtoClientWuModule> WuModules { get; set; }
        public List<DtoClientMessageModule> MessageModules { get; set; }
        public List<DtoClientWinPeModule> WinPeModules { get; set; }
        public int SubFrequency { get; set; }
        public bool SkipServerResult { get; set; }
        public EnumPolicy.Trigger Trigger { get; set; }
        public EnumPolicy.WuType WuType { get; set; }
        public EnumCondition.FailedAction ConditionFailedAction { get; set; }
        public DtoClientModuleCondition Condition { get; set; }
        public bool IsWingetUpdate { get; set; }
        public bool WingetUseMaxConnections { get; set; }
    }
}