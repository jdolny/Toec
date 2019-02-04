using System.Reflection;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using Toec_Common.Dto;
using Toec_Common.Enum;
using Toec_Common.Modules;
using Toec_Services.ApiCall;
using Toec_Services.InventorySearchers;

namespace Toec_Services.Policy.Modules
{
    public class ModulePrintManager
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly DtoClientPrinterModule _module;

        private readonly DtoModuleResult _moduleResult;
        private readonly EnumPolicy.Trigger _trigger;

        public ModulePrintManager(DtoClientPrinterModule module, EnumPolicy.Trigger policyTrigger)
        {
            _moduleResult = new DtoModuleResult();
            _moduleResult.Name = module.DisplayName;
            _moduleResult.Guid = module.Guid;
            _moduleResult.Success = true;
            _module = module;
            _trigger = policyTrigger;
        }

        private string GetPrinterArgs(bool isSecondPass)
        {
            var args = "/q ";

            if (!isSecondPass)
            {
                if (_module.PrinterAction == EnumPrinterModule.ActionType.Delete)
                {
                    if (_trigger == EnumPolicy.Trigger.Login)
                        args += "/dn ";
                    else
                        args += "/gd ";
                }
                else if (_module.PrinterAction == EnumPrinterModule.ActionType.Install)
                {
                    if (_trigger == EnumPolicy.Trigger.Login)
                        args += "/in ";
                    else
                        args += "/ga ";
                }
                else if (_module.PrinterAction == EnumPrinterModule.ActionType.InstallPowershell)
                {
                    args = "-ConnectionName " + "\"" + _module.PrinterPath + "\"";
                    Logger.Debug("Printer Args:" + args);
                    return args;
                }
            }

            if (isSecondPass || _module.PrinterAction == EnumPrinterModule.ActionType.None)
            {
                if (_module.IsDefault)
                    args += "/y ";
            }

            args += "/n " + "\"" + _module.PrinterPath + "\"";

            Logger.Debug("Printer Args:" + args);
            return args;
        }

        public DtoModuleResult Run()
        {
            var printerFound = false;
            Logger.Info("Running Printer Module: " + _module.DisplayName);
            var pArgs = new DtoProcessArgs();
            if (_module.PrinterAction == EnumPrinterModule.ActionType.InstallPowershell)
            {
                pArgs.RunWith = "Powershell.exe";
            }
            else
            {
                pArgs.RunWith = "rundll32.exe";
            }
            
            var printerArgs = GetPrinterArgs(isSecondPass:false);
            if (printerArgs == null)
            {
                _moduleResult.Success = false;
                _moduleResult.ExitCode = "-1";
                _moduleResult.ErrorMessage = "Could Not Determine Printer Action Type";
                return _moduleResult;
            }

            if (_module.PrinterAction == EnumPrinterModule.ActionType.InstallPowershell)
            {
                pArgs.Command = "Add-Printer " + printerArgs;
            }
            else
            {
                pArgs.Command = "printui.dll,PrintUIEntry " + printerArgs;
            }
            
            pArgs.Timeout = 1;
            var result = new ServiceProcess(pArgs).RunProcess();
            _moduleResult.ExitCode = result.ExitCode.ToString();

            if (result.ExitCode != 0 || !string.IsNullOrEmpty(result.StandardError))
            {
                _moduleResult.Success = false;
                _moduleResult.ErrorMessage = result.StandardError;
                Logger.Info("Printer Module: " + _module.DisplayName + "Finished");
                return _moduleResult;
            }


            if (_module.WaitForEnumeration)
            {
                Logger.Debug("Enumerating Printers");
                int count = 0;
                while (true)
                {
                    var tempFound = PrinterFound();
                    if (tempFound && (_module.PrinterAction == EnumPrinterModule.ActionType.Install || _module.PrinterAction == EnumPrinterModule.ActionType.InstallPowershell))
                    {
                        Logger.Debug("Found Printer.");
                        printerFound = true;
                        break;
                    }
                    else if (tempFound && _module.PrinterAction == EnumPrinterModule.ActionType.Delete)
                    {
                        Logger.Debug("Printer Still Exists.");
                        printerFound = true;
                    }
                    else if (!tempFound && (_module.PrinterAction == EnumPrinterModule.ActionType.Install || _module.PrinterAction == EnumPrinterModule.ActionType.InstallPowershell))
                    {
                        Logger.Debug("Could Not Find Printer.");
                        printerFound = false;
                    }
                    else if (!tempFound && _module.PrinterAction == EnumPrinterModule.ActionType.Delete)
                    {
                        Logger.Debug("Could Not Find Printer.");
                        printerFound = false;
                        break;
                    }

                    if (count == 24 && (_module.PrinterAction == EnumPrinterModule.ActionType.Install || _module.PrinterAction == EnumPrinterModule.ActionType.InstallPowershell))
                    {
                        Logger.Debug("Could Not Verify Printer Was Installed.  Giving Up.");
                        break;
                    }
                    else if (count == 24 && _module.PrinterAction == EnumPrinterModule.ActionType.Delete)
                    {
                        Logger.Debug("Could Not Verify Printer Was Deleted.  Giving Up.");
                        break;
                    }

                    Logger.Debug("Waiting...");
                    Task.Delay(5*1000).Wait();
                    count++;
                }
            }
            else
            {
                //if enumeration skipped, must assume printer found
                printerFound = true;
            }

            if (printerFound && _module.IsDefault && _module.PrinterAction != EnumPrinterModule.ActionType.None)
            {

                Logger.Info("Setting Printer As Default.");
                pArgs = new DtoProcessArgs();
                pArgs.RunWith = "rundll32.exe";
                printerArgs = GetPrinterArgs(isSecondPass: true);
                if (printerArgs == null)
                {
                    _moduleResult.Success = false;
                    _moduleResult.ExitCode = "-1";
                    _moduleResult.ErrorMessage = "Could Not Determine Printer Action Type";
                    return _moduleResult;
                }
                pArgs.Command = "printui.dll,PrintUIEntry " + printerArgs;
                pArgs.Timeout = 1;
                result = new ServiceProcess(pArgs).RunProcess();
                _moduleResult.ExitCode = result.ExitCode.ToString();

                if (result.ExitCode != 0 || !string.IsNullOrEmpty(result.StandardError))
                {
                    _moduleResult.Success = false;
                    _moduleResult.ErrorMessage = result.StandardError;
                    Logger.Info("Printer Module: " + _module.DisplayName + "Finished");
                    return _moduleResult;
                }
            }


            if ((_module.PrinterAction == EnumPrinterModule.ActionType.Install || _module.PrinterAction == EnumPrinterModule.ActionType.InstallPowershell) && !printerFound)
            {
                _moduleResult.Success = false;
                _moduleResult.ExitCode = "-1";
                _moduleResult.ErrorMessage = "Unknown Error";
                Logger.Error("Printer Installation Failed.");
            }
            else if (_module.PrinterAction == EnumPrinterModule.ActionType.Delete && printerFound)
            {
                _moduleResult.Success = false;
                _moduleResult.ExitCode = "-1";
                _moduleResult.ErrorMessage = "Unknown Error";
                Logger.Error("Printer Removal Failed.");
            }

            if (_module.RestartSpooler)
            {
                Logger.Info("Restarting Print Spooler");
                var restartResult = _trigger == EnumPolicy.Trigger.Login
                    ? new APICall().LocalApi.RestartPrintSpooler()
                    : new ServiceSystemService().RestartPrintSpooler();
                if (restartResult)
                    Logger.Info("Print Spooler Restarted Successfully");
                else
                    Logger.Error("Print Spooler Failed To Restart.");
            }

            return _moduleResult;
        }

        private bool PrinterFound()
        {
            var printers = _trigger == EnumPolicy.Trigger.Login ? new Printer().GetInstalledPrintersWmiOnly() : new Printer().GetInstalledPrinters();
            Logger.Debug(JsonConvert.SerializeObject(printers));
            if (printers == null) return false;
            foreach (var printer in printers)
            {
                if (string.IsNullOrEmpty(printer.SystemName) || string.IsNullOrEmpty(printer.ShareName))
                    continue;
                var unc = printer.SystemName + "\\" + printer.ShareName;
                if (unc.ToLower().Equals(_module.PrinterPath.ToLower()))
                {
                    return true;
                }
            }

            return false;
        }
    }
}