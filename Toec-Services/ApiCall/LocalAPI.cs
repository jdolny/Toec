using System.Collections.Generic;
using Newtonsoft.Json;
using RestSharp;
using Toec_Common.Dto;
using Toec_Common.Entity;
using Toec_Common.Inventory;

namespace Toec_Services.ApiCall
{
    public class LocalAPI : BaseAPI
    {
        public LocalAPI(string resource) : base(resource)
        {
        }

        public bool DeleteModuleCache(string moduleGuid)
        {
            Request.Method = Method.DELETE;
            Request.AddParameter("moduleGuid", moduleGuid);
            Request.Resource = string.Format("api/{0}/DeleteModuleCache/", Resource);
            var result =
                new ApiRequest("http://localhost:" + DtoGobalSettings.LocalApiPort + "/").Execute<DtoBoolResponse>(
                    Request);
            return result != null && result.Value;
        }

        public bool GetFile(DtoClientFileRequest fileRequest)
        {
            Request.Method = Method.POST;
            Request.AddParameter("application/json", JsonConvert.SerializeObject(fileRequest), ParameterType.RequestBody);
            Request.Resource = string.Format("api/{0}/GetFile/", Resource);
            var result =
                new ApiRequest("http://localhost:" + DtoGobalSettings.LocalApiPort + "/").Execute<DtoBoolResponse>(
                    Request);
            return result != null && result.Value;
        }

        public DtoTriggerResponse GetLoginPolicies(string currentUser)
        {
            Request.Method = Method.GET;
            Request.AddParameter("currentUser", currentUser);
            Request.Resource = string.Format("api/{0}/GetLoginPolicies/", Resource);
            return
                new ApiRequest("http://localhost:" + DtoGobalSettings.LocalApiPort + "/").Execute<DtoTriggerResponse>(
                    Request);
        }

        public string GetScript(string moduleGuid)
        {
            Request.Method = Method.GET;
            Request.AddParameter("moduleGuid", moduleGuid);
            Request.Resource = string.Format("api/{0}/GetScript/", Resource);
            var result =
                new ApiRequest("http://localhost:" + DtoGobalSettings.LocalApiPort + "/").Execute<DtoStringResponse>(
                    Request);
            return result == null ? string.Empty : result.Value;
        }

        public List<DtoPrinterInventory> GetPrinterCollection()
        {
            Request.Method = Method.GET;
            Request.Resource = string.Format("api/{0}/GetPrinterCollection/", Resource);
            return
                new ApiRequest("http://localhost:" + DtoGobalSettings.LocalApiPort + "/").Execute<List<DtoPrinterInventory>>(
                    Request);
        }

        public bool RestartPrintSpooler()
        {
            Request.Method = Method.GET;
            Request.Resource = string.Format("api/{0}/RestartPrintSpooler/", Resource);
            var result =
                new ApiRequest("http://localhost:" + DtoGobalSettings.LocalApiPort + "/").Execute<DtoBoolResponse>(
                    Request);
            return result != null && result.Value;
        }

        public bool LogoutAllUsers()
        {
            Request.Method = Method.GET;
            Request.Resource = string.Format("api/{0}/LogoutAllUsers/", Resource);
            var result =
                new ApiRequest("http://localhost:" + DtoGobalSettings.LocalApiPort + "/").Execute<DtoBoolResponse>(
                    Request);
            return result != null && result.Value;
        }

        public DtoTrayAppStartupInfo ServiceStartComplete()
        {
            Request.Method = Method.GET;
            Request.Resource = string.Format("api/{0}/ServiceStartComplete/", Resource);
            return
                new ApiRequest("http://localhost:" + DtoGobalSettings.LocalApiPort + "/").Execute<DtoTrayAppStartupInfo>
                    (Request);
        }

        public DtoActionResult WritePolicyHistory(EntityPolicyHistory history)
        {
            Request.Method = Method.POST;
            Request.AddParameter("application/json", JsonConvert.SerializeObject(history), ParameterType.RequestBody);
            Request.Resource = string.Format("api/{0}/WritePolicyHistory/", Resource);
            return
                new ApiRequest("http://localhost:" + DtoGobalSettings.LocalApiPort + "/").Execute<DtoActionResult>(
                    Request);
        }

        public DtoActionResult SendServerResults(DtoPolicyResults results)
        {
            Request.Method = Method.POST;
            Request.AddParameter("application/json", JsonConvert.SerializeObject(results), ParameterType.RequestBody);
            Request.Resource = string.Format("api/{0}/SendServerResults/", Resource);
            return
                new ApiRequest("http://localhost:" + DtoGobalSettings.LocalApiPort + "/").Execute<DtoActionResult>(
                    Request);
        }
        public DtoDownloadConnectionResult CreateDownloadConnection(DtoDownloadConRequest conRequest)
        {
            Request.Method = Method.POST;
            Request.AddParameter("application/json", JsonConvert.SerializeObject(conRequest), ParameterType.RequestBody);
            Request.Resource = string.Format("api/{0}/CreateDownloadConnection/", Resource);
            return
                new ApiRequest("http://localhost:" + DtoGobalSettings.LocalApiPort + "/").Execute<DtoDownloadConnectionResult>(
                    Request);
        }

        public DtoBoolResponse RemoveDownloadConnection(DtoDownloadConRequest conRequest)
        {
            Request.Method = Method.POST;
            Request.AddParameter("application/json", JsonConvert.SerializeObject(conRequest), ParameterType.RequestBody);
            Request.Resource = string.Format("api/{0}/RemoveDownloadConnection/", Resource);
            return
                new ApiRequest("http://localhost:" + DtoGobalSettings.LocalApiPort + "/").Execute<DtoBoolResponse>(
                    Request);
        }
    }
}