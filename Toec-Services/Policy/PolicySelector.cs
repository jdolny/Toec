using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using Newtonsoft.Json;
using Toec_Common.Dto;
using Toec_Common.Entity;
using Toec_Common.Enum;
using Toec_Common.Modules;
using Toec_Services.ApiCall;
using Toec_Services.Entity;

namespace Toec_Services.Policy
{
    public class PolicySelector : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly EnumPolicy.Trigger _currentTrigger;

        private readonly string _currentUser;
        private readonly List<DtoClientPolicy> _policiesToRun;
        private readonly PolicyHistoryServices _policyHistoryServices;

        public PolicySelector(EnumPolicy.Trigger triggerType, string currentUser)
        {
            _currentTrigger = triggerType;
            _policiesToRun = new List<DtoClientPolicy>();
            _policyHistoryServices = new PolicyHistoryServices();
            _currentUser = currentUser;
        }

        private DtoTriggerResponse GetAllPoliciesForTriggerType(EnumPolicy.Trigger triggerType)
        {
            var policyRequest = new DtoPolicyRequest();

            policyRequest.ClientIdentity.Name = DtoGobalSettings.ClientIdentity.Name;
            policyRequest.ClientIdentity.Guid = DtoGobalSettings.ClientIdentity.Guid;
            policyRequest.ClientIdentity.InstallationId = DtoGobalSettings.ClientIdentity.InstallationId;
            policyRequest.Trigger = triggerType;
            //user login history and application usage is submitted for every startup or checkin trigger
            if (triggerType != EnumPolicy.Trigger.Login)
            {
                var serviceUserTracker = new ServiceUserTracker();
                serviceUserTracker.CleanupOldEvents();
                policyRequest.UserLogins = serviceUserTracker.GetUserCompletedEvents();

                var serviceAppMonitor = new ServiceAppMonitor();
                //Clean up duplicates before sending data to server
                serviceAppMonitor.CleanupOldEvents();
                serviceAppMonitor.RemoveDuplicates();
                policyRequest.AppMonitors = serviceAppMonitor.GetCompletedEvents();
            }

            var response = new APICall().PolicyApi.GetClientPolicies(policyRequest);
            if (response == null) return null;

            //only update checkin time for startup and checkin
            if (triggerType != EnumPolicy.Trigger.Login)
            {
                DtoGobalSettings.ShutdownDelay = response.ShutdownDelay;
                //Cannot have a checkin time less than 1 minute
                DtoGobalSettings.CheckinTime = response.CheckinTime >= 1 ? response.CheckinTime : 1;
            }
            if (response.UserLoginsSubmitted && policyRequest.UserLogins != null)
                new ServiceUserTracker().DeleteEvents(policyRequest.UserLogins);

            if (response.AppMonitorSubmitted && policyRequest.AppMonitors != null)
                new ServiceAppMonitor().DeleteEvents(policyRequest.AppMonitors);

            if (!ValidatePolicies(response.Policies)) return null;

            return response;
        }

        public DtoTriggerResponse GetPoliciesToExecute()
        {
            var triggerData = GetAllPoliciesForTriggerType(_currentTrigger);
            if (triggerData == null) return null;

            foreach (var policy in triggerData.Policies)
            {
                //should never happen, but added check anyway
                if (_currentTrigger == EnumPolicy.Trigger.Login && policy.Trigger != EnumPolicy.Trigger.Login)
                    continue;
                if (policy.Trigger == EnumPolicy.Trigger.Login)
                {
                    Logger.Debug("Login Policy Selector User: " + _currentUser);
                    var policyHistoryUser = _policyHistoryServices.GetLastPolicyRunForUserFromGuid(policy.Guid,
                        _currentUser);
                    switch (policy.Frequency)
                    {
                        //don't need to check for rerun flag because ongoing always runs
                        case EnumPolicy.Frequency.Ongoing:
                            Logger.Debug(policy.Name + " Is Set To Run Ongoing.  Adding To Run List.");
                            _policiesToRun.Add(policy);
                            break;
                        case EnumPolicy.Frequency.OncePerUserPerComputer:
                            OncePerComputerPerUser(policy, policyHistoryUser);
                            break;
                    }
                }
                else
                {
                    var policyHistory = _policyHistoryServices.GetLastPolicyRunFromGuid(policy.Guid);
                    switch (policy.Frequency)
                    {
                        //don't need to check for rerun flag because ongoing always runs
                        case EnumPolicy.Frequency.Ongoing:
                            Logger.Debug(policy.Name + " Is Set To Run Ongoing.  Adding To Run List.");
                            _policiesToRun.Add(policy);
                            break;
                        case EnumPolicy.Frequency.OncePerComputer:
                            OncePerComputer(policy, policyHistory);
                            break;

                        case EnumPolicy.Frequency.OncePerDay:
                            OncePerDay(policy, policyHistory);
                            break;

                        case EnumPolicy.Frequency.OncePerWeek:
                            OncePerWeek(policy, policyHistory);
                            break;

                        case EnumPolicy.Frequency.OncePerMonth:
                            OncePerMonth(policy, policyHistory);
                            break;

                        case EnumPolicy.Frequency.EveryXdays:
                            EveryXDays(policy,policyHistory);
                            break;

                        case EnumPolicy.Frequency.EveryXhours:
                            EveryXHours(policy,policyHistory);
                            break;
                    }
                }
            }

            triggerData.Policies = _policiesToRun;
            return triggerData;
        }

        private void OncePerComputer(DtoClientPolicy policy, EntityPolicyHistory history)
        {
            Logger.Debug(policy.Name + " Is Set To Run Once Per Computer.");
            if (history == null)
            {
                Logger.Debug("Policy Has Never Been Run.  Adding To Run List.");
                _policiesToRun.Add(policy);
            }
            else if (policy.ReRunExisting)
            {
                Logger.Debug("Policy Is Set ReRun On Previously Completed Computers.");
                var hashHistory = _policyHistoryServices.GetLastPolicyRunFromHash(policy.Hash);
                if (hashHistory == null)
                {
                    Logger.Debug("This Policy's Hash Was Not Found In The History.  Adding To Run List.");
                    _policiesToRun.Add(policy);
                }
                else
                {
                    Logger.Debug("Policy Last Ran On " + hashHistory.LastRunTime);
                    Logger.Debug("This Policy Has Already Ran With Forced ReRun.  Skipping");
                }
            }
            else
            {
                Logger.Debug("Policy Has Already Ran And Has Not Been Forced Run Again.  Skipping.");
                Logger.Debug("Policy Last Ran On " + history.LastRunTime);
            }
        }

        private void OncePerComputerPerUser(DtoClientPolicy policy, EntityPolicyHistory history)
        {
            Logger.Debug(policy.Name + " Is Set To Run Once Per Computer Per User.");
            if (history == null)
            {
                Logger.Debug("Policy Has Never Been Run.  Adding To Run List.");
                _policiesToRun.Add(policy);
            }
            else if (policy.ReRunExisting)
            {
                Logger.Debug("Policy Is Set ReRun On Previously Completed Computers.");
                var hashHistory = _policyHistoryServices.GetLastPolicyRunForUserFromHash(policy.Hash, _currentUser);
                if (hashHistory == null)
                {
                    Logger.Debug("This Policy's Hash Was Not Found In The History.  Adding To Run List.");
                    _policiesToRun.Add(policy);
                }
                else
                {
                    Logger.Debug("Policy Last Ran On " + hashHistory.LastRunTime);
                    Logger.Debug("This Policy Has Already Ran With Forced ReRun.  Skipping");
                }
            }
            else
            {
                Logger.Debug("Policy Has Already Ran And Has Not Been Forced Run Again.  Skipping.");
                Logger.Debug("Policy Last Ran On " + history.LastRunTime);
            }
        }

        private void EveryXDays(DtoClientPolicy policy, EntityPolicyHistory history)
        {
            Logger.Debug(policy.Name + " Is Set To Run Every " + policy.SubFrequency + " Days.");
            if (policy.ReRunExisting)
            {
                Logger.Debug("Policy Is Set ReRun On Previously Completed Computers.");
                var hashHistory = _policyHistoryServices.GetLastPolicyRunFromHash(policy.Hash);
                if (hashHistory == null)
                {
                    Logger.Debug("This Policy's Hash Was Not Found In The History.  Adding To Run List.");
                    _policiesToRun.Add(policy);
                    return;
                }
                else
                {
                    Logger.Debug("Policy Last Ran On " + hashHistory.LastRunTime);
                    Logger.Debug("This Policy Has Already Ran With Forced ReRun.  Skipping");
                    return;
                }
            }
            if (history == null)
            {
                Logger.Debug("Policy Has Never Been Run.  Adding To Run List.");
                _policiesToRun.Add(policy);
            }
            else
            {
                Logger.Debug("Policy Last Ran On " + history.LastRunTime);

                var limit = DateTime.UtcNow - TimeSpan.FromDays(policy.SubFrequency);

                if (history.LastRunTime.ToUniversalTime() < limit)
                {
                    Logger.Debug("It Has Been More Than " + policy.SubFrequency + " Days Since The Last Run, Adding To Run List.");
                    _policiesToRun.Add(policy);
                }
                else
                {
                    Logger.Debug("This Policy Has Already Ran Within The Last " + policy.SubFrequency + " Days.  Skipping");
                }
            }
        }

        private void EveryXHours(DtoClientPolicy policy, EntityPolicyHistory history)
        {
            Logger.Debug(policy.Name + " Is Set To Run Every " + policy.SubFrequency + " Hours.");
            if (policy.ReRunExisting)
            {
                Logger.Debug("Policy Is Set ReRun On Previously Completed Computers.");
                var hashHistory = _policyHistoryServices.GetLastPolicyRunFromHash(policy.Hash);
                if (hashHistory == null)
                {
                    Logger.Debug("This Policy's Hash Was Not Found In The History.  Adding To Run List.");
                    _policiesToRun.Add(policy);
                    return;
                }
                else
                {
                    Logger.Debug("Policy Last Ran On " + hashHistory.LastRunTime);
                    Logger.Debug("This Policy Has Already Ran With Forced ReRun.  Skipping");
                    return;
                }
            }
            if (history == null)
            {
                Logger.Debug("Policy Has Never Been Run.  Adding To Run List.");
                _policiesToRun.Add(policy);
            }
            else
            {
                Logger.Debug("Policy Last Ran On " + history.LastRunTime);

                var limit = DateTime.UtcNow - TimeSpan.FromHours(policy.SubFrequency);

                if (history.LastRunTime.ToUniversalTime() < limit)
                {
                    Logger.Debug("It Has Been More Than " + policy.SubFrequency + " Hours Since The Last Run, Adding To Run List.");
                    _policiesToRun.Add(policy);
                }
                else
                {
                    Logger.Debug("This Policy Has Already Ran Within The Last " + policy.SubFrequency + " Hours.  Skipping");
                }
            }
        }

        private void OncePerDay(DtoClientPolicy policy, EntityPolicyHistory history)
        {
            Logger.Debug(policy.Name + " Is Set To Run Once Per Day.");
            if (policy.ReRunExisting)
            {
                Logger.Debug("Policy Is Set ReRun On Previously Completed Computers.");
                var hashHistory = _policyHistoryServices.GetLastPolicyRunFromHash(policy.Hash);
                if (hashHistory == null)
                {
                    Logger.Debug("This Policy's Hash Was Not Found In The History.  Adding To Run List.");
                    _policiesToRun.Add(policy);
                    return;
                }
                else
                {
                    Logger.Debug("Policy Last Ran On " + hashHistory.LastRunTime);
                    Logger.Debug("This Policy Has Already Ran With Forced ReRun.  Skipping");
                    return;
                }
            }
            if (history == null)
            {
                Logger.Debug("Policy Has Never Been Run.  Adding To Run List.");
                _policiesToRun.Add(policy);
            }
            else
            {
                Logger.Debug("Policy Last Ran On " + history.LastRunTime);
                //A new day is considered midnight, not necessarily 24 hours
                var todayStartTime = Convert.ToDateTime(DateTime.UtcNow.ToShortDateString()).ToUniversalTime();
                if (history.LastRunTime.ToUniversalTime() < todayStartTime)
                {
                    Logger.Debug("Midnight Has Passed Since Last Run, Adding To Run List.");
                    _policiesToRun.Add(policy);
                }
                else
                {
                    Logger.Debug("This Policy Has Already Ran Within The Last Day.  Skipping");
                }
            }
        }

        private void OncePerMonth(DtoClientPolicy policy, EntityPolicyHistory history)
        {
            Logger.Debug(policy.Name + " Is Set To Run Once Per Month.");
            var lastDayOfMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
            var currentDayOfMonth = DateTime.Now.Day;

            if (policy.ReRunExisting)
            {
                Logger.Debug("Policy Is Set ReRun On Previously Completed Computers.");
                var hashHistory = _policyHistoryServices.GetLastPolicyRunFromHash(policy.Hash);
                if (hashHistory == null)
                {
                    Logger.Debug("This Policy's Hash Was Not Found In The History.");
                    if (policy.FrequencyMissedAction == EnumPolicy.FrequencyMissedAction.NextOpportunity)
                    {
                        Logger.Debug("Policy Missed Action Is Next Opportunity.  Adding To Run List.");
                        _policiesToRun.Add(policy);
                    }
                    else
                    {
                        Logger.Debug("Current Day: " + currentDayOfMonth);
                        Logger.Debug("Policy Run Day: " + policy.SubFrequency);
                        if (policy.SubFrequency == currentDayOfMonth ||
                            (policy.SubFrequency == 31 && currentDayOfMonth == lastDayOfMonth))
                        {
                            Logger.Debug("Policy Run Day Matches Current Day.  Adding To Run List.");
                            _policiesToRun.Add(policy);
                        }
                        else
                        {
                            Logger.Debug("Policy's Run Day And Current Day Do Not Match.  Skipping");
                        }
                    }
                    return;
                }
            }

            if (history == null)
            {
                Logger.Debug("Policy Has Never Been Run");
                if (policy.FrequencyMissedAction == EnumPolicy.FrequencyMissedAction.NextOpportunity)
                {
                    Logger.Debug("Missed Action Is Next Opportunity.  Adding To Run List.");
                    _policiesToRun.Add(policy);
                }
                else
                {
                    Logger.Debug("Current Day: " + currentDayOfMonth);
                    Logger.Debug("Policy Run Day: " + policy.SubFrequency);
                    if (policy.SubFrequency == currentDayOfMonth ||
                        (policy.SubFrequency == 31 && currentDayOfMonth == lastDayOfMonth))
                    {
                        Logger.Debug("Policy Run Day Matches Current Day.  Adding To Run List.");
                        _policiesToRun.Add(policy);
                    }
                    else
                    {
                        Logger.Debug("Policy's Run Day And Current Day Do Not Match.  Skipping");
                    }
                }
            }
            else
            {
                Logger.Debug("Policy Last Ran On " + history.LastRunTime);
                var lastMonth = DateTime.Now.AddMonths(-1);
                var lastDayLastMonth = DateTime.DaysInMonth(DateTime.Now.Year, lastMonth.Month);
                if (DateTime.UtcNow - history.LastRunTime.ToUniversalTime() >= TimeSpan.FromDays(1))
                {
                    Logger.Debug("Policy Has Not Run Today.  Checking If It Is Scheduled For Today.");
                    if (policy.FrequencyMissedAction == EnumPolicy.FrequencyMissedAction.ScheduleDayOnly &&
                             (policy.SubFrequency == currentDayOfMonth ||
                              policy.SubFrequency == 31 && currentDayOfMonth == lastDayOfMonth))
                    {
                        Logger.Debug("Policy Run Day Matches Current Day.  Adding To Run List.");
                        _policiesToRun.Add(policy);
                        return;
                    }
                    else
                    {
                        Logger.Debug("Policy's Run Day And Current Day Do Not Match.  Skipping");
                    }
                }
                if (DateTime.UtcNow - history.LastRunTime.ToUniversalTime() >= TimeSpan.FromDays(lastDayLastMonth))
                {
                    Logger.Debug("Policy Has Not Run In Over A Month.  Checking Missed Action.");
                    Logger.Debug("Current Day: " + currentDayOfMonth);
                    Logger.Debug("Policy Run Day: " + policy.SubFrequency);
                    if (policy.FrequencyMissedAction == EnumPolicy.FrequencyMissedAction.NextOpportunity)
                    {
                        Logger.Debug("Policy Missed Action Is Next Opportunity.  Adding To Run List.");
                        _policiesToRun.Add(policy);
                    }
                    
                }
            }
        }

        private void OncePerWeek(DtoClientPolicy policy, EntityPolicyHistory history)
        {
            Logger.Debug(policy.Name + " Is Set To Run Once Per Week.");
            var currentDayOfWeek = (int) DateTime.Now.DayOfWeek;

            if (policy.ReRunExisting)
            {
                Logger.Debug("Policy Is Set ReRun On Previously Completed Computers.");
                var hashHistory = _policyHistoryServices.GetLastPolicyRunFromHash(policy.Hash);
                if (hashHistory == null)
                {
                    Logger.Debug("This Policy's Hash Was Not Found In The History.");
                    if (policy.FrequencyMissedAction == EnumPolicy.FrequencyMissedAction.NextOpportunity)
                    {
                        Logger.Debug("Policy Missed Action Is Set To Next Opportunity.  Adding To Run List.");
                        _policiesToRun.Add(policy);
                    }
                    else
                    {
                        Logger.Debug("Current Day Of Week: " + currentDayOfWeek);
                        Logger.Debug("Policy Run Day: " + policy.SubFrequency);
                        if (policy.SubFrequency == currentDayOfWeek)
                        {
                            Logger.Debug("Policy Matches Current Run Day.  Adding To Run List.");
                            _policiesToRun.Add(policy);
                        }
                        else
                        {
                            Logger.Debug("Policy's Run Day And Current Day Do Not Match.  Skipping");
                        }
                       
                    }
                    return;
                }
            }

            if (history == null)
            {
                Logger.Debug("Policy Has Never Been Run");
                if (policy.FrequencyMissedAction == EnumPolicy.FrequencyMissedAction.NextOpportunity)
                {
                    Logger.Debug("Missed Action Is Next Opportunity.  Adding To Run List.");
                    _policiesToRun.Add(policy);
                }
                else
                {
                    Logger.Debug("Current Day Of Week: " + currentDayOfWeek);
                    Logger.Debug("Policy Run Day: " + policy.SubFrequency);
                    if (policy.SubFrequency == currentDayOfWeek)
                    {
                        Logger.Debug("Policy Matches Current Run Day.  Adding To Run List.");
                        _policiesToRun.Add(policy);
                    }
                    else
                    {
                        Logger.Debug("Policy's Run Day And Current Day Do Not Match.  Skipping");
                    }
                }
            }
            else
            {
                Logger.Debug("Policy Last Ran On " + history.LastRunTime);
                Logger.Debug("Current Day Of Week: " + currentDayOfWeek);
                Logger.Debug("Policy Run Day: " + policy.SubFrequency);
                if (DateTime.UtcNow - history.LastRunTime.ToUniversalTime() >= TimeSpan.FromDays(1))
                {
                    Logger.Debug("Policy Has Not Run Today.  Checking If It Is Scheduled For Today.");
                    if (policy.FrequencyMissedAction == EnumPolicy.FrequencyMissedAction.ScheduleDayOnly &&
                        policy.SubFrequency == currentDayOfWeek)
                    {
                        Logger.Debug("Policy's Scheduled Run Day Matches Today.  Adding To Run List.");
                        _policiesToRun.Add(policy);
                        return;
                    }
                    else
                    {
                        Logger.Debug("Policy's Run Day And Current Day Do Not Match.  Skipping");
                    }
                }
                if (DateTime.UtcNow - history.LastRunTime.ToUniversalTime() >= TimeSpan.FromDays(7))
                {
                    Logger.Debug("Policy Has Not Run In Over A Week.  Checking Missed Action.");
                    if (policy.FrequencyMissedAction == EnumPolicy.FrequencyMissedAction.NextOpportunity)
                    {
                        Logger.Debug("Missed Action Is Next Opportunity.  Adding To Run List.");
                        _policiesToRun.Add(policy);
                    }
                    else
                    {
                        Logger.Debug("Policy's Run Day And Current Day Do Not Match.  Skipping");
                    }

                }
            }
        }

        private bool ValidatePolicies(List<DtoClientPolicy> policies)
        {
            if (policies == null)
            {
                Logger.Error("Could Not Get Client Policies");
                return false;
            }

            try
            {
                Logger.Debug(JsonConvert.SerializeObject(policies));
                foreach (var policy in policies)
                {
                    if (string.IsNullOrEmpty(policy.Name))
                        return false;
                    if (string.IsNullOrEmpty(policy.Guid))
                        return false;
                    if (string.IsNullOrEmpty(policy.Hash))
                        return false;
                    if (policy.Frequency != EnumPolicy.Frequency.OncePerComputer &&
                        policy.Frequency != EnumPolicy.Frequency.OncePerDay
                        && policy.Frequency != EnumPolicy.Frequency.OncePerMonth &&
                        policy.Frequency != EnumPolicy.Frequency.OncePerUserPerComputer
                        && policy.Frequency != EnumPolicy.Frequency.OncePerWeek &&
                        policy.Frequency != EnumPolicy.Frequency.Ongoing &&
                        policy.Frequency != EnumPolicy.Frequency.EveryXdays &&
                        policy.Frequency != EnumPolicy.Frequency.EveryXhours)
                        return false;
                    if (policy.Trigger != EnumPolicy.Trigger.Checkin && policy.Trigger != EnumPolicy.Trigger.Login &&
                        policy.Trigger != EnumPolicy.Trigger.Startup &&
                        policy.Trigger != EnumPolicy.Trigger.StartupOrCheckin)
                        return false;
                    int value;
                    if (!int.TryParse(policy.SubFrequency.ToString(), out value))
                        return false;
                    if (policy.CompletedAction != EnumPolicy.CompletedAction.DoNothing &&
                        policy.CompletedAction != EnumPolicy.CompletedAction.Reboot && policy.CompletedAction != EnumPolicy.CompletedAction.RebootIfNoLogins)
                        return false;
                    if (policy.ExecutionType != EnumPolicy.ExecutionType.Cache &&
                        policy.ExecutionType != EnumPolicy.ExecutionType.Install)
                        return false;
                    if (policy.ErrorAction != EnumPolicy.ErrorAction.AbortCurrentPolicy &&
                        policy.ErrorAction != EnumPolicy.ErrorAction.AbortRemainingPolicies &&
                        policy.ErrorAction != EnumPolicy.ErrorAction.Continue)
                        return false;
                    if (!int.TryParse(policy.Order.ToString(), out value))
                        return false;
                    if (policy.FrequencyMissedAction != EnumPolicy.FrequencyMissedAction.NextOpportunity &&
                        policy.FrequencyMissedAction != EnumPolicy.FrequencyMissedAction.ScheduleDayOnly)
                        return false;
                    if (policy.LogLevel != EnumPolicy.LogLevel.Full &&
                        policy.LogLevel != EnumPolicy.LogLevel.HiddenArguments &&
                        policy.LogLevel != EnumPolicy.LogLevel.None)
                        return false;
                    if (policy.WuType != EnumPolicy.WuType.Disabled && policy.WuType != EnumPolicy.WuType.Microsoft &&
                        policy.WuType != EnumPolicy.WuType.MicrosoftSkipUpgrades &&
                        policy.WuType != EnumPolicy.WuType.Wsus && policy.WuType != EnumPolicy.WuType.WsusSkipUpgrades)
                        return false;
                    if (policy.RemoteAccess != EnumPolicy.RemoteAccess.Disabled && policy.RemoteAccess != EnumPolicy.RemoteAccess.Enabled && policy.RemoteAccess != EnumPolicy.RemoteAccess.NotConfigured && policy.RemoteAccess != EnumPolicy.RemoteAccess.ForceReinstall)
                        return false;
                    if (policy.Condition.Guid != null)
                    {
                        if (policy.ConditionFailedAction != EnumCondition.FailedAction.MarkFailed && policy.ConditionFailedAction != EnumCondition.FailedAction.MarkNotApplicable
                            && policy.ConditionFailedAction != EnumCondition.FailedAction.MarkSkipped && policy.ConditionFailedAction != EnumCondition.FailedAction.MarkSuccess)
                        {
                            return false;
                        }
                    }
                
                    foreach (var module in policy.CommandModules)
                    {
                        if (string.IsNullOrEmpty(module.Guid))
                            return false;
                        if (string.IsNullOrEmpty(module.DisplayName))
                            return false;
                        if (string.IsNullOrEmpty(module.Command))
                            return false;
                        if (module.Command.StartsWith("[module-"))
                        {
                            var command = module.Command.Split(']')[1];
                            if (command.StartsWith("\\"))
                                command = command.Substring(1);
                            var guid = module.Command.Replace("[module-", "");
                            guid = guid.Split(']').First();
                            module.Command = Path.Combine(DtoGobalSettings.BaseCachePath, guid, command);
                        }

                        if (!int.TryParse(module.Order.ToString(), out value))
                            return false;
                        if (!int.TryParse(module.Timeout.ToString(), out value))
                            return false;
                        if (module.SuccessCodes.Count == 0)
                            return false;
                        if (module.SuccessCodes.Any(code => !int.TryParse(code, out value)))
                        {
                            return false;
                        }
                    }
                    foreach (var module in policy.FileCopyModules)
                    {
                        if (string.IsNullOrEmpty(module.Destination))
                            return false;
                        if (module.Destination.StartsWith("[module-"))
                        {
                            var guid = module.Destination.Replace("[module-", "");
                            guid = guid.Split(']').First();
                            module.Destination = Path.Combine(DtoGobalSettings.BaseCachePath, guid);
                        }
                        if (string.IsNullOrEmpty(module.Guid))
                            return false;
                        if (string.IsNullOrEmpty(module.DisplayName))
                            return false;

                        if (!int.TryParse(module.Order.ToString(), out value))
                            return false;
                        foreach (var file in module.Files)
                        {
                            if (string.IsNullOrEmpty(file.FileName))
                                return false;
                            if (string.IsNullOrEmpty(file.FileHash))
                                return false;
                        }
                    }
                    foreach (var module in policy.WinPeModules)
                    {
                        if (string.IsNullOrEmpty(module.Guid))
                            return false;
                        if (string.IsNullOrEmpty(module.DisplayName))
                            return false;

                        if (!int.TryParse(module.Order.ToString(), out value))
                            return false;
                        foreach (var file in module.Files)
                        {
                            if (string.IsNullOrEmpty(file.FileName))
                                return false;
                            if (string.IsNullOrEmpty(file.FileHash))
                                return false;
                        }
                    }
                    foreach (var module in policy.PrinterModules)
                    {
                        if (string.IsNullOrEmpty(module.Guid))
                            return false;
                        if (string.IsNullOrEmpty(module.DisplayName))
                            return false;
                        if (string.IsNullOrEmpty(module.PrinterPath))
                            return false;
                        if (!int.TryParse(module.Order.ToString(), out value))
                            return false;
                        if (module.PrinterAction != EnumPrinterModule.ActionType.Delete &&
                            module.PrinterAction != EnumPrinterModule.ActionType.Install &&
                            module.PrinterAction != EnumPrinterModule.ActionType.None &&
                            module.PrinterAction != EnumPrinterModule.ActionType.InstallPowershell)
                            return false;
                    }

                    foreach (var module in policy.ScriptModules)
                    {
                        if (string.IsNullOrEmpty(module.Guid))
                            return false;
                        if (string.IsNullOrEmpty(module.DisplayName))
                            return false;
                      
                        if (!int.TryParse(module.Order.ToString(), out value))
                            return false;
                        if (!int.TryParse(module.Timeout.ToString(), out value))
                            return false;
                        if (module.ScriptType != EnumScriptModule.ScriptType.Batch &&
                            module.ScriptType != EnumScriptModule.ScriptType.Powershell &&
                            module.ScriptType != EnumScriptModule.ScriptType.VbScript)
                            return false;
                        if (module.SuccessCodes.Any(code => !int.TryParse(code, out value)))
                        {
                            return false;
                        }
                    }

                    foreach (var module in policy.SoftwareModules)
                    {
                        if (string.IsNullOrEmpty(module.Guid))
                            return false;
                        if (string.IsNullOrEmpty(module.DisplayName))
                            return false;
                        if (string.IsNullOrEmpty(module.Command))
                            return false;
                        if (string.IsNullOrEmpty(module.Arguments))
                            return false;
                        if (!int.TryParse(module.Order.ToString(), out value))
                            return false;
                        if (!int.TryParse(module.Timeout.ToString(), out value))
                            return false;
                        if (module.InstallType != EnumSoftwareModule.MsiInstallType.Install &&
                            module.InstallType != EnumSoftwareModule.MsiInstallType.Patch &&
                            module.InstallType != EnumSoftwareModule.MsiInstallType.Uninstall)
                            return false;
                        if (module.SuccessCodes.Any(code => !int.TryParse(code, out value)))
                        {
                            return false;
                        }
                        foreach (var file in module.Files)
                        {
                            if (string.IsNullOrEmpty(file.FileName))
                                return false;
                            if (string.IsNullOrEmpty(file.FileHash))
                                return false;
                        }
                    }
                    foreach (var module in policy.WuModules)
                    {
                        if (string.IsNullOrEmpty(module.Guid))
                            return false;
                        if (string.IsNullOrEmpty(module.DisplayName))
                            return false;
                        if (!int.TryParse(module.Order.ToString(), out value))
                            return false;
                        if (!int.TryParse(module.Timeout.ToString(), out value))
                            return false;
                        if (module.SuccessCodes.Any(code => !int.TryParse(code, out value)))
                        {
                            return false;
                        }
                        foreach (var file in module.Files)
                        {
                            if (string.IsNullOrEmpty(file.FileName))
                                return false;
                            if (string.IsNullOrEmpty(file.FileHash))
                                return false;
                        }
                    }
                    foreach (var module in policy.MessageModules)
                    {
                        if (string.IsNullOrEmpty(module.Guid))
                            return false;
                        if (string.IsNullOrEmpty(module.DisplayName))
                            return false;
                        if (!int.TryParse(module.Order.ToString(), out value))
                            return false;
                        if (!int.TryParse(module.Timeout.ToString(), out value))
                            return false;
                        if (string.IsNullOrEmpty(module.Title))
                            return false;
                        if (string.IsNullOrEmpty(module.Message))
                            return false;
                        
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                return false;
            }

            return true;
        }


        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if(_policyHistoryServices != null)
                    _policyHistoryServices.Dispose();
                }
            }
            this.disposed = true;
        }
    }
}