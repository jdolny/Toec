using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using Toec_Common.Dto;
using Toec_Common.Entity;
using Toec_Common.Enum;
using Toec_Common.Modules;
using Toec_Services.ApiCall;
using Toec_Services.Entity;

namespace Toec_Services.Policy
{
    public class PolicyRunner
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly EnumPolicy.Trigger _trigger;
        private DtoTriggerResponse _policiesToRun;
        private readonly List<DtoPolicyResult> _policyResults;
        private bool _reboot;
        private bool _rebootNoLogins;
        public PolicyRunner(EnumPolicy.Trigger trigger)
        {
            _trigger = trigger;
            _policyResults = new List<DtoPolicyResult>();
            _reboot = false;
            _rebootNoLogins = false;
        }

        private void CleanupCache()
        {
            Logger.Info("Cleaning Policy Cache");
            var fileSystemService = new ServiceFileSystem();
            foreach (var policyResult in _policyResults)
            {
                if (policyResult.ExecutionType == EnumPolicy.ExecutionType.Install && policyResult.DeleteCache &&
                    (policyResult.PolicyResult == EnumPolicy.Result.Success ||
                     policyResult.PolicyResult == EnumPolicy.Result.NotApplicable))
                {
                    var result = policyResult;
                    foreach (
                        var policy in _policiesToRun.Policies.Where(policy => result.PolicyGuid.Equals(policy.Guid)))
                    {
                        foreach (var module in policy.FileCopyModules)
                        {
                            if (policy.Trigger == EnumPolicy.Trigger.Login)
                                new APICall().LocalApi.DeleteModuleCache(module.Guid);
                            else
                                fileSystemService.DeleteDirectory(module.Guid);
                        }

                        foreach (var module in policy.SoftwareModules)
                        {
                            if (policy.Trigger == EnumPolicy.Trigger.Login)
                                new APICall().LocalApi.DeleteModuleCache(module.Guid);
                            else
                                fileSystemService.DeleteDirectory(module.Guid);
                        }

                        foreach (var module in policy.ScriptModules)
                        {
                            if (policy.Trigger == EnumPolicy.Trigger.Login)
                                new APICall().LocalApi.DeleteModuleCache(module.Guid);
                            else
                                fileSystemService.DeleteDirectory(module.Guid);
                        }
                        foreach (var module in policy.WuModules)
                        {
                            if (policy.Trigger == EnumPolicy.Trigger.Login)
                                new APICall().LocalApi.DeleteModuleCache(module.Guid);
                            else
                                fileSystemService.DeleteDirectory(module.Guid);
                        }
                        foreach (var module in policy.CommandModules)
                        {
                            if (policy.Trigger == EnumPolicy.Trigger.Login)
                                new APICall().LocalApi.DeleteModuleCache(module.Guid);
                            else
                                fileSystemService.DeleteDirectory(module.Guid);
                        }
                    }
                }
            }
        }

        private bool IsTriggerStopError(DtoClientPolicy policy)
        {
            if (policy.ErrorAction == EnumPolicy.ErrorAction.AbortCurrentPolicy)
            {
                Logger.Error("Aborting Current Policy");
                return false;
            }
            if (policy.ErrorAction == EnumPolicy.ErrorAction.AbortRemainingPolicies)
            {
                Logger.Error("Aborting All Remaining Policies");
                return true;
            }
            return false;
        }

        private void RecordResults()
        {
            Logger.Info("Submitting Policy Results");
            var serverHistoryList = new List<DtoServerPolicyHistory>();
            var customInventoryList = new List<DtoScriptModuleOutput>();
            foreach (var policyResult in _policyResults)
            {
                var history = new EntityPolicyHistory();
                history.PolicyGUID = policyResult.PolicyGuid;
                history.PolicyHash = policyResult.PolicyHash;
                history.LastRunTime = DateTime.UtcNow;
                history.Username = Environment.UserDomainName + "\\" + Environment.UserName;

                if (!policyResult.SkipServerResult)
                {
                    var serverHistory = new DtoServerPolicyHistory();
                    serverHistory.Hash = policyResult.PolicyHash;
                    if (policyResult.PolicyResult == EnumPolicy.Result.Success)
                        serverHistory.Result = EnumPolicyHistory.RunResult.Success;
                    else if (policyResult.PolicyResult == EnumPolicy.Result.Failed)
                        serverHistory.Result = EnumPolicyHistory.RunResult.Failed;
                    else if (policyResult.PolicyResult == EnumPolicy.Result.Skipped)
                        serverHistory.Result = EnumPolicyHistory.RunResult.Skipped;
                    else
                        serverHistory.Result = EnumPolicyHistory.RunResult.NotApplicable;

                    serverHistory.PolicyGuid = policyResult.PolicyGuid;
                    serverHistory.LastRunTime = DateTime.UtcNow;
                    serverHistory.User = Environment.UserDomainName + "\\" + Environment.UserName;
                    serverHistoryList.Add(serverHistory);
                }
                //Don't record local results if not success, only server
                if (policyResult.PolicyResult == EnumPolicy.Result.Success || policyResult.PolicyResult == EnumPolicy.Result.NotApplicable)
                {
                    if (policyResult.ScriptOutputs.Count > 0)
                        customInventoryList.AddRange(policyResult.ScriptOutputs);
                    if (_trigger == EnumPolicy.Trigger.Login)
                        new APICall().LocalApi.WritePolicyHistory(history);
                    else
                    {
                        new PolicyHistoryServices().AddHistory(history);
                    }
                }
            }

            var results = new DtoPolicyResults();
            results.CustomInventories = customInventoryList;
            results.Histories = serverHistoryList;
            if (results.CustomInventories.Count == 0 && results.Histories.Count == 0)
            {
                Logger.Info("Completed Submitting Policy Results");
                return;
            }

            if (_trigger == EnumPolicy.Trigger.Login)
                new APICall().LocalApi.SendServerResults(results);
            else
                new APICall().PolicyApi.AddHistory(results);
            Logger.Info("Completed Submitting Policy Results");
        }

        public bool Run(DtoClientPolicy singleModulePolicy = null)
        {
            if (singleModulePolicy == null)
            {
                _policiesToRun = _trigger == EnumPolicy.Trigger.Login
                    ? new APICall().LocalApi.GetLoginPolicies(Environment.UserDomainName + "\\" + Environment.UserName)
                    : new PolicySelector(_trigger, Environment.UserDomainName + "\\" + Environment.UserName).GetPoliciesToExecute();
            }
            else
            {
                _policiesToRun = new DtoTriggerResponse();
                _policiesToRun.Policies.Add(singleModulePolicy);
            }

            if (_policiesToRun == null)
            {
                Logger.Error("Error Trying To Parse Policies. Aborting Trigger: " + _trigger);
                return false;
            }

            if (_policiesToRun.Policies.Count == 0)
            {
                Logger.Info(string.Format("No Policies For Trigger {0} Were Found For This Computer", _trigger));
                return true;
            }

            DtoGobalSettings.PolicyIsRunning = true;
            bool cacheFailedWithTriggerStop = false;

            //Check for any conditions that will need cached
            var conditionNeedsCached = false;
            if(_policiesToRun.Policies.Any(x=> x.MessageModules.Any()))
            {
                foreach(var messageModule in _policiesToRun.Policies.Select(x => x.MessageModules))
                {
                    if(messageModule.Any(x => x.Condition.Guid != null))
                    {
                        conditionNeedsCached = true;
                        break;
                    }
                }
            }
            if (_policiesToRun.Policies.Any(x => x.PrinterModules.Any()) && !conditionNeedsCached)
            {
                foreach (var printerModule in _policiesToRun.Policies.Select(x => x.PrinterModules))
                {
                    if (printerModule.Any(x => x.Condition.Guid != null))
                    {
                        conditionNeedsCached = true;
                        break;
                    }
                }
            }

            //Check if install of remote access needs cached
            var remotelyNeedsCached = false;
            if (_policiesToRun.Policies.Any(x => x.RemoteAccess == EnumPolicy.RemoteAccess.ForceReinstall))
                remotelyNeedsCached = true;
            else if (new ServiceSystemService().IsRemotelyInstalled())
                remotelyNeedsCached = false;
            else if (_policiesToRun.Policies.Any(x => x.RemoteAccess == EnumPolicy.RemoteAccess.Enabled))
                remotelyNeedsCached = true;

            //cache all policies first if any need cached
            if (_policiesToRun.Policies.Any(x =>
                x.SoftwareModules.Any() || x.FileCopyModules.Any() || x.WinPeModules.Any() ||  x.WuModules.Any() || x.ScriptModules.Any() || x.CommandModules.Any()) || conditionNeedsCached || _policiesToRun.Policies.Any(x => x.Condition.Guid != null) || remotelyNeedsCached)
            {
                var downloadConResult = new ServiceDownloadConnectionManager().CreateConnection(_trigger);
                if (!downloadConResult)
                    return true; //can't get download connection treat as if policy never ran

                foreach (var policy in _policiesToRun.Policies)
                {
                    SetPolicyLogLevel(policy);

                    var cacheResult = new PolicyCacher(policy).Cache();
                    if (policy.SkipServerResult)
                        cacheResult.SkipServerResult = true;

                    if (policy.ExecutionType == EnumPolicy.ExecutionType.Cache)
                        _policyResults.Add(cacheResult);
                    if (cacheResult.PolicyResult != EnumPolicy.Result.Success)
                    {
                        if (IsTriggerStopError(policy))
                        {
                            cacheFailedWithTriggerStop = true;
                            break;
                        }
                    }
                }
                //release the download slot
                new ServiceDownloadConnectionManager().RemoveConnection(_trigger);

            }

            if (!cacheFailedWithTriggerStop)
            {
                foreach (var policy in _policiesToRun.Policies)
                {
                    SetPolicyLogLevel(policy);

                    if (policy.ExecutionType == EnumPolicy.ExecutionType.Cache)
                    {
                        Logger.Debug("Policy's Execution Type Is Cache Only And Will Not Install");
                        continue;
                    }

                    var policyResult = new DtoPolicyResult();
                    if (policy.WingetUseMaxConnections && (policy.WingetModules.Any() || policy.IsWingetUpdate))
                    {
                        Logger.Debug("A Winget module was found and the policy is enforcing Max connections.");
                        if (!new ServiceDownloadConnectionManager().CreateConnection(_trigger))
                        {
                            policyResult.PolicyResult = EnumPolicy.Result.Failed;
                        }
                        else
                        {
                            policyResult = new PolicyExecutor(policy).Execute();
                            new ServiceDownloadConnectionManager().RemoveConnection(_trigger);
                        }
                    }
                    else
                    {
                        policyResult = new PolicyExecutor(policy).Execute();
                    }
                    

                    if (policy.SkipServerResult)
                        policyResult.SkipServerResult = true;
                    _policyResults.Add(policyResult);

                    if (policyResult.PolicyResult == EnumPolicy.Result.Failed)
                    {
                        if (IsTriggerStopError(policy)) break;
                    }

                    else if (policyResult.PolicyResult == EnumPolicy.Result.Success &&
                        policy.CompletedAction == EnumPolicy.CompletedAction.Reboot)
                    {
                        _reboot = true;
                        break;
                    }
                    else if (policyResult.PolicyResult == EnumPolicy.Result.Success &&
                        policy.CompletedAction == EnumPolicy.CompletedAction.RebootIfNoLogins)
                    {
                        _rebootNoLogins = true;
                        break;
                    }
                }
            }

            Logger.Info("Policies Complete.  Starting Policy Cleanup.");
            DtoGobalSettings.PolicyIsRunning = false;
            RecordResults();

            CleanupCache();

            //restore global log level
            ((Hierarchy) LogManager.GetRepository()).Root.Level = DtoGobalSettings.LogLevel;
            ((Hierarchy) LogManager.GetRepository()).RaiseConfigurationChanged(
                EventArgs.Empty);

            if (_reboot || (singleModulePolicy != null && _policiesToRun.Policies.Any(x => x.WinPeModules.Any()) && _policyResults.First().PolicyResult == EnumPolicy.Result.Success))
            {
                Logger.Info("Policy Initiated Reboot.  Rebooting Now.");
                if (_trigger == EnumPolicy.Trigger.Login)
                    new APICall().LocalApi.LogoutAllUsers();
                else
                    new ServiceUserTracker().LogoutAllUsers();
                Process.Start("shutdown.exe", "/r /t " + DtoGobalSettings.ShutdownDelay);
            }

            if (_rebootNoLogins)
            {
                Logger.Info("Policy Initiated Reboot If No Users Are Logged In.");
                Logger.Info("Checking For Any Logged In Users.");
                if (_trigger != EnumPolicy.Trigger.Login)
                {
                    var users = new ServiceUserLogins().GetUsersLoggedIn();
                    if (users.Count > 0)
                    {
                        Logger.Info("User Found, Reboot Skipped.");
                    }
                    else
                    {
                        Logger.Info("No Users Found, Issuing Reboot Command.");
                        Process.Start("shutdown.exe", "/r /t " + DtoGobalSettings.ShutdownDelay);
                    }
                }
                else
                {
                    Logger.Info("User Found, Policy Is A Login Policy, Reboot Skipped.");
                }
                   
            }
            return true;
        }

        private void SetPolicyLogLevel(DtoClientPolicy policy)
        {
            if (policy.LogLevel == EnumPolicy.LogLevel.Full)
                ((Hierarchy) LogManager.GetRepository()).Root.Level = Level.Debug;
            else if (policy.LogLevel == EnumPolicy.LogLevel.HiddenArguments)
                ((Hierarchy) LogManager.GetRepository()).Root.Level = Level.Info;
            else
                ((Hierarchy) LogManager.GetRepository()).Root.Level = Level.Error;

            ((Hierarchy) LogManager.GetRepository()).RaiseConfigurationChanged(EventArgs.Empty);
        }
    }
}