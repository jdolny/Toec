using System;
using System.IO;
using System.Reflection;
using log4net;
using Newtonsoft.Json;
using RestSharp;
using Toec_Common.Dto;

namespace Toec_Services.ApiCall
{
    public class PolicyAPI : BaseAPI
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public PolicyAPI(string resource) : base(resource)
        {
        }

        public DtoActionResult AddHistory(DtoPolicyResults results)
        {
            Request.Method = Method.POST;
            Request.Resource = string.Format("ProvisionedComm/{0}/AddHistory/", Resource);
            return new ApiRequest().ExecuteSymKeyEncryption<DtoActionResult>(Request,
                JsonConvert.SerializeObject(results));
        }

        public DtoTriggerResponse GetClientPolicies(DtoPolicyRequest policyRequest)
        {
            Request.Method = Method.POST;
            Request.Resource = string.Format("ProvisionedComm/{0}/GetClientPolicies/", Resource);
            return new ApiRequest().ExecuteSymKeyEncryption<DtoTriggerResponse>(Request,
                JsonConvert.SerializeObject(policyRequest));
        }

        public bool GetFile(DtoClientFileRequest fileRequest)
        {
            var outputPath = string.IsNullOrEmpty(fileRequest.Destination) ? Path.Combine(DtoGobalSettings.BaseCachePath, fileRequest.ModuleGuid, fileRequest.FileName) : fileRequest.Destination;
            Request.Method = Method.POST;
            Request.Timeout = 14400000;
            Request.Resource = string.Format("ProvisionedComm/{0}/GetFile/", Resource);
            var apiRequest = new ApiRequest(14400000);
            return apiRequest.DownloadFile(Request, JsonConvert.SerializeObject(fileRequest),outputPath);
        }

        public bool GetFileForImagePrep(DtoClientFileRequest fileRequest, string outputPath)
        {
            Request.Method = Method.POST;
            Request.Timeout = 14400000;
            Request.Resource = string.Format("ProvisionedComm/{0}/GetFile/", Resource);
            var apiRequest = new ApiRequest(14400000);
            return apiRequest.DownloadFile(Request, JsonConvert.SerializeObject(fileRequest), outputPath);
        }

        public bool GetClientUpgrade(DtoClientFileRequest fileRequest, string computerName)
        {
            Request.Method = Method.POST;

            Request.Resource = string.Format("{0}/GetClientMsi/", Resource);
            var apiRequest = new ApiRequest();
            Request.AddParameter("application/json", JsonConvert.SerializeObject(fileRequest), ParameterType.RequestBody);
            var response = apiRequest.ExecuteRawHmac(Request,computerName);
            if (response == null) return false;
           
            var outputPath = Path.Combine(DtoGobalSettings.BaseCachePath, "ClientUpgrades", fileRequest.FileName);

            try
            {
                File.WriteAllBytes(outputPath, response);
            }
            catch (Exception ex)
            {
                Logger.Error("Could Not Save File: " + outputPath);
                Logger.Error(ex.Message);
                return false;
            }

            return true;
        }

        public string GetScript(string moduleGuid)
        {
            Request.Method = Method.GET;
            Request.AddParameter("moduleGuid", moduleGuid);
            Request.Resource = string.Format("ProvisionedComm/{0}/GetScript/", Resource);
            var responseData = new ApiRequest().ExecuteSymKeyEncryption<DtoStringResponse>(Request, string.Empty);
            return responseData != null ? responseData.Value : string.Empty;
        }


        public string GetRemotelyInstallArgs()
        {
            Request.Method = Method.GET;
            Request.Resource = string.Format("ProvisionedComm/{0}/GetRemotelyInstallArgs/", Resource);
            var responseData = new ApiRequest().ExecuteSymKeyEncryption<DtoStringResponse>(Request, string.Empty);
            return responseData != null ? responseData.Value : string.Empty;
        }

        public DtoActionResult UpdateRemoteAccessId(RemotelyConnectionInfo conInfo)
        {
            Request.Method = Method.POST;
            Request.Resource = string.Format("ProvisionedComm/{0}/UpdateRemoteAccessId/", Resource);
            return new ApiRequest().ExecuteSymKeyEncryption<DtoActionResult>(Request,
                JsonConvert.SerializeObject(conInfo));
        }

        public DtoActionResult UpdateLastSocketResult(DtoStringResponse result)
        {
            Request.Method = Method.POST;
            Request.Resource = string.Format("ProvisionedComm/{0}/UpdateLastSocketResult/", Resource);
            return new ApiRequest().ExecuteSymKeyEncryption<DtoActionResult>(Request,
                JsonConvert.SerializeObject(result));
        }

        public DtoImpersonationAccount GetImpersonationAccount(string impersonationGuid)
        {
            Request.Method = Method.GET;
            Request.AddParameter("impersonationGuid", impersonationGuid);
            Request.Resource = string.Format("ProvisionedComm/{0}/GetImpersonationAccount/", Resource);
            return new ApiRequest().ExecuteSymKeyEncryption<DtoImpersonationAccount>(Request,string.Empty);
        }

        public DtoDomainJoinCredentials GetDomainJoinCredentials()
        {
            Request.Method = Method.GET;
            Request.Resource = string.Format("ProvisionedComm/{0}/GetDomainJoinCredentials/", Resource);
            return new ApiRequest().ExecuteSymKeyEncryption<DtoDomainJoinCredentials>(Request, string.Empty);
        }

        public bool AddToFirstRunGroup()
        {
            Request.Method = Method.GET;
            Request.Resource = string.Format("ProvisionedComm/{0}/AddToFirstRunGroup/", Resource);
            var result = new ApiRequest().ExecuteSymKeyEncryption<DtoBoolResponse>(Request, string.Empty);
            return result != null && result.Value;
        }

        public bool RemoveFromFirstRunGroup()
        {
            Request.Method = Method.GET;
            Request.Resource = string.Format("ProvisionedComm/{0}/RemoveFromFirstRunGroup/", Resource);
            var result = new ApiRequest().ExecuteSymKeyEncryption<DtoBoolResponse>(Request, string.Empty);
            return result != null && result.Value;
        }

        public DtoDownloadConnectionResult CreateDownloadConnection(DtoDownloadConRequest conRequest)
        {
            Request.Method = Method.POST;
            Request.AddParameter("application/json", JsonConvert.SerializeObject(conRequest), ParameterType.RequestBody);
            Request.Resource = string.Format("{0}/CreateDownloadConnection/", Resource);
            //this needs to use hmac or an endpoint that hasn't provisioned yet and needs an upgrade first, won't be able to upgrade
            var result = new ApiRequest().ExecuteHMAC<DtoDownloadConnectionResult>(Request, conRequest.ComputerName);
            return result ?? new DtoDownloadConnectionResult();
        }

        public bool RemoveDownloadConnection(DtoDownloadConRequest conRequest)
        {
            Request.Method = Method.POST;
            Request.AddParameter("application/json", JsonConvert.SerializeObject(conRequest), ParameterType.RequestBody);
            Request.Resource = string.Format("{0}/RemoveDownloadConnection/", Resource);
            //this needs to use hmac or an endpoint that hasn't provisioned yet and needs an upgrade first, won't be able to upgrade
            var result = new ApiRequest().ExecuteHMAC<DtoBoolResponse>(Request, conRequest.ComputerName);
            return result != null && result.Value;
        }
    }
}