using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using Toec_Common.Dto;
using Toec_Common.Enum;
using Toec_Common.Modules;
using Toec_Services.Policy.Modules;

namespace Toec_Services.Policy
{
    public class PolicyExecutor
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly DtoClientPolicy _policy;

        private readonly DtoPolicyResult _policyResult;

        public PolicyExecutor(DtoClientPolicy policy)
        {
            _policyResult = new DtoPolicyResult();
            _policyResult.PolicyName = policy.Name;
            _policyResult.PolicyGuid = policy.Guid;
            _policyResult.PolicyHash = policy.Hash;
            _policyResult.PolicyResult = EnumPolicy.Result.Success;
            _policyResult.DeleteCache = policy.RemoveInstallCache;
            _policyResult.ExecutionType = policy.ExecutionType;
            _policy = policy;
        }

        public DtoPolicyResult Execute()
        {
            Logger.Info(string.Format("Executing Policy {0} ({1})", _policy.Guid, _policy.Name));

            if (_policy.Trigger != EnumPolicy.Trigger.Login)
            {
                if (_policy.IsLoginTracker)
                    new ModuleUserLogins().Run();

                if (_policy.IsApplicationMonitor)
                    new ModuleApplicationMonitor().Run();

                if (_policy.IsInventory == EnumPolicy.InventoryAction.Before || _policy.IsInventory == EnumPolicy.InventoryAction.Both)
                    new ModuleInventory().Run();

                if (_policy.WuType != EnumPolicy.WuType.Disabled)
                    ModuleWuManager.InstallAllUpdates(_policy.WuType);
            }

            var orderCounter = -1;
            foreach (var order in GetPolicyModuleOrder(_policy))
            {
                orderCounter++;
                foreach (var module in _policy.SoftwareModules)
                {
                    if (module.Order != order) continue;
                    var result = new ModuleSoftwareManager(module).Run();
                    if (result.Success) continue;
                    if (IsPolicyStopError(result)) return _policyResult;
                }
                foreach (var module in _policy.ScriptModules)
                {
                    if (module.Order != order) continue;
                    var result = new ModuleScriptManager(module).Run();
                    if (result.Success)
                    {
                        if (result.ScriptOutput != null)
                            _policyResult.ScriptOutputs.Add(result.ScriptOutput);
                        continue;
                    }
                    if (module.IsCondition) //script module has failed by this point
                    {
                        if (orderCounter == 0)
                        {
                            //if module is condition and condition failed, break out of remaining policy
                            Logger.Debug("First Policy Condition Not Met.  Marked As Not Applicable");
                            _policyResult.PolicyResult = EnumPolicy.Result.Skipped;
                            return _policyResult;
                        }
                        else
                        {
                            //Condition was used later in the policy not first, don't marked as skipped, use last result value.
                            Logger.Debug("Policy Condition Not Met.  Skipping Remaining Policy Modules.");
                            return _policyResult;
                        }
                        
                    }
                    if (IsPolicyStopError(result)) return _policyResult;
                }
                foreach (var module in _policy.PrinterModules)
                {
                    if (module.Order != order) continue;
                    var result = new ModulePrintManager(module, _policy.Trigger).Run();
                    if (result.Success) continue;
                    if (IsPolicyStopError(result)) return _policyResult;
                }
                foreach (var module in _policy.FileCopyModules)
                {
                    if (module.Order != order) continue;
                    var result = new ModuleFileCopy(module).Run();
                    if (result.Success) continue;
                    if (IsPolicyStopError(result)) return _policyResult;
                }
                foreach (var module in _policy.CommandModules)
                {
                    if (module.Order != order) continue;
                    var result = new ModuleCommandManager(module).Run();
                    if (result.Success) continue;
                    if (IsPolicyStopError(result)) return _policyResult;
                }
                foreach (var module in _policy.WuModules)
                {
                    if (module.Order != order) continue;
                    var result = new ModuleWuManager(module).Run();
                    if (result.Success) continue;
                    if (IsPolicyStopError(result)) return _policyResult;
                }
            }

            if (_policy.Trigger != EnumPolicy.Trigger.Login)
            {
                if (_policy.IsInventory == EnumPolicy.InventoryAction.After || _policy.IsInventory == EnumPolicy.InventoryAction.Both)
                    new ModuleInventory().Run();
            }

            Logger.Info(string.Format("Finished Executing Policy {0} ({1})", _policy.Guid, _policy.Name));

            return _policyResult;
        }

        private List<int> GetPolicyModuleOrder(DtoClientPolicy policy)
        {
            var list = new List<int>();
            list.AddRange(policy.PrinterModules.Select(module => module.Order));
            list.AddRange(policy.SoftwareModules.Select(module => module.Order));
            list.AddRange(policy.ScriptModules.Select(module => module.Order));
            list.AddRange(policy.FileCopyModules.Select(module => module.Order));
            list.AddRange(policy.CommandModules.Select(module => module.Order));
            list.AddRange(policy.WuModules.Select(module => module.Order));

            var distictList = list.Distinct().ToList();
            distictList.Sort();
            return distictList;
        }

        private bool IsPolicyStopError(DtoModuleResult moduleResult)
        {
            _policyResult.PolicyResult = EnumPolicy.Result.Failed;
            _policyResult.FailedModuleName = moduleResult.Name;
            _policyResult.FailedModuleGuid = moduleResult.Guid;
            _policyResult.FailedModuleExitCode = moduleResult.ExitCode;
            _policyResult.FailedModuleErrorMessage = moduleResult.ErrorMessage;

            Logger.Error(string.Format("An Error Occurred In Module {0}", moduleResult.Name));
            if (_policy.ErrorAction == EnumPolicy.ErrorAction.Continue)
            {
                Logger.Debug("Continuing With Policy Error");
                return false;
            }

            return true;
        }
    }
}