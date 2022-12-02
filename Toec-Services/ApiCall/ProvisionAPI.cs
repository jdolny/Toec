using System.Collections.Generic;
using Newtonsoft.Json;
using RestSharp;
using Toec_Common.Dto;

namespace Toec_Services.ApiCall
{
    public class ProvisionAPI : BaseAPI
    {
        public ProvisionAPI(string resource) : base(resource)
        {
        }

        public bool ComConnectionTest(string computerName)
        {
            Request.Method = Method.GET;
            Request.Resource = string.Format("{0}/ComConnectionTest/", Resource);
            var response = new ApiRequest().ExecuteHMAC<DtoBoolResponse>(Request, computerName);
            if (response == null) return false;
            return response.Value;
        }

        public List<DtoComServerConnection> ComConnections(string computerName)
        {
            Request.Method = Method.GET;
            Request.Resource = string.Format("{0}/ComConnections/", Resource);
            return new ApiRequest().ExecuteHMAC<List<DtoComServerConnection>>(Request, computerName);
        }

        public DtoProvisionResponse ConfirmProvisionRequest(DtoConfirmProvisionRequest confirmRequest)
        {
            Request.Method = Method.POST;
            Request.Resource = string.Format("ProvisionedComm/{0}/ConfirmProvisionRequest/", Resource);
            return new ApiRequest().ExecuteSymKeyEncryption<DtoProvisionResponse>(Request,
                JsonConvert.SerializeObject(confirmRequest));
        }

        public DtoProvisionResponse GetIntermediateCert(string computerName)
        {
            Request.Method = Method.GET;
            Request.Resource = string.Format("{0}/GetIntermediateCert/", Resource);
            return new ApiRequest().ExecuteHMAC<DtoProvisionResponse>(Request, computerName);
        }

        public DtoClientStartupInfo GetStartupInfo(string computerName)
        {
            Request.Method = Method.GET;
            Request.Resource = string.Format("{0}/GetStartupInfo/", Resource);
            return new ApiRequest().ExecuteHMAC<DtoClientStartupInfo>(Request, computerName);
        }

        public DtoProvisionResponse ProvisionClient(DtoProvisionRequest provisionRequest)
        {
            Request.Method = Method.POST;
            Request.AddParameter("application/json", JsonConvert.SerializeObject(provisionRequest), ParameterType.RequestBody);
            Request.Resource = string.Format("{0}/ProvisionClient/", Resource);
            return new ApiRequest().ExecuteHMAC<DtoProvisionResponse>(Request, provisionRequest.Name);
        }

        public DtoProvisionResponse RenewSymmKey(DtoRenewKeyRequest renewRequest)
        {
            Request.Method = Method.POST;
            Request.Resource = string.Format("ProvisionedComm/{0}/RenewSymmKey/", Resource);
            return new ApiRequest().ExecuteSymKeyEncryption<DtoProvisionResponse>(Request,
                JsonConvert.SerializeObject(renewRequest));
        }
    }
}