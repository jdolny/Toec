using System.Collections.Generic;
using System.Web.Http;
using Toec_Common.Dto;
using Toec_Common.Entity;
using Toec_Common.Enum;
using Toec_Common.Inventory;
using Toec_Services;
using Toec_Services.ApiCall;
using Toec_Services.Entity;
using Toec_Services.InventorySearchers;
using Toec_Services.Policy;

namespace Toec_LocalApi.Controllers
{
    public class LocalController : ApiController
    {
        public DtoBoolResponse DeleteModuleCache(string moduleGuid)
        {
            var result = new ServiceFileSystem().DeleteDirectory(moduleGuid);
            return new DtoBoolResponse {Value = result};
        }

        [HttpPost]
        public DtoBoolResponse GetFile(DtoClientFileRequest fileRequest)
        {
            var result = new APICall().PolicyApi.GetFile(fileRequest);
            return new DtoBoolResponse {Value = result};
        }

        public DtoTriggerResponse GetLoginPolicies(string currentUser)
        {
            return new PolicySelector(EnumPolicy.Trigger.Login,currentUser).GetPoliciesToExecute();
        }

        public DtoStringResponse GetScript(string moduleGuid)
        {
            var result = new APICall().PolicyApi.GetScript(moduleGuid);
            return new DtoStringResponse {Value = result};
        }

        public List<DtoPrinterInventory> GetPrinterCollection()
        {
            return new Printer().GetInstalledPrinters();
        }

        [HttpGet]
        public DtoBoolResponse RestartPrintSpooler()
        {
            var result = new ServiceSystemService().RestartPrintSpooler();
            return new DtoBoolResponse {Value = result};
        }

        [HttpGet]
        public DtoBoolResponse LogoutAllUsers()
        {
            new ServiceUserTracker().LogoutAllUsers();
            new ServiceAppMonitor().CloseAllOpen();
            return new DtoBoolResponse { Value = true };
        }

        [HttpGet]
        public DtoTrayAppStartupInfo ServiceStartComplete()
        {
            var startupInfo = new DtoTrayAppStartupInfo();
            startupInfo.ServiceStarted = DtoGobalSettings.ServiceStartupComplete;
            startupInfo.TrayAppPort = new ServicePortSelector().GenerateTrayAppPort();
            startupInfo.LogLevel = DtoGobalSettings.LogLevel.ToString();
            startupInfo.ComputerGuid = DtoGobalSettings.ClientIdentity.Guid;
            startupInfo.ComputerName = DtoGobalSettings.ClientIdentity.Name;
            startupInfo.ComServer = DtoGobalSettings.ComServer;
            startupInfo.ShutdownDelay = DtoGobalSettings.ShutdownDelay;
            return startupInfo;
        }

        [HttpPost]
        public DtoActionResult WritePolicyHistory(EntityPolicyHistory history)
        {
            return new PolicyHistoryServices().AddHistory(history);
        }

        [HttpPost]
        public DtoActionResult SendServerResults(DtoPolicyResults results)
        {
            return new APICall().PolicyApi.AddHistory(results);
        }

        [HttpPost]
        public DtoDownloadConnectionResult CreateDownloadConnection(DtoDownloadConRequest downloadConRequest)
        {
            return new APICall().PolicyApi.CreateDownloadConnection(downloadConRequest);
        }

        [HttpPost]
        public DtoBoolResponse RemoveDownloadConnection(DtoDownloadConRequest downloadConRequest)
        {
            var result = new APICall().PolicyApi.RemoveDownloadConnection(downloadConRequest);
            return new DtoBoolResponse() { Value = result };
        }
    }
}