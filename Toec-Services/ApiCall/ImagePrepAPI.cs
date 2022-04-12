using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using log4net;
using Newtonsoft.Json;
using RestSharp;
using Toec_Common.Dto;

namespace Toec_Services.ApiCall
{
    public class ImagePrepAPI : BaseAPI
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ImagePrepAPI(string resource) : base(resource)
        {

        }

        public string GetDriverList()
        {
            Request.Method = Method.GET;
            Request.Resource = string.Format("ProvisionedComm/{0}/GetDriverList/", Resource);
            var responseData = new ApiRequest().ExecuteSymKeyEncryption<DtoStringResponse>(Request, string.Empty);
            return responseData != null ? responseData.Value : string.Empty;
        }

        public List<DtoSysprepAnswerfile> GetSysprepList()
        {
            Request.Method = Method.GET;
            Request.Resource = string.Format("ProvisionedComm/{0}/GetSysprepList/", Resource);
            return new ApiRequest().ExecuteSymKeyEncryption<List<DtoSysprepAnswerfile>>(Request, string.Empty);
          
        }

        public List<DtoSetupCompleteFile> GetSetupCompleteList()
        {
            Request.Method = Method.GET;
            Request.Resource = string.Format("ProvisionedComm/{0}/GetSetupCompleteList/", Resource);
            return new ApiRequest().ExecuteSymKeyEncryption<List<DtoSetupCompleteFile>>(Request, string.Empty);
           
        }

        public string GetSetupCompleteFile(int id)
        {
            Request.Method = Method.GET;
            Request.Resource = string.Format("ProvisionedComm/{0}/GetSetupCompleteFile/{1}", Resource,id);
            var responseData = new ApiRequest().ExecuteSymKeyEncryption<DtoStringResponse>(Request, string.Empty);
            return responseData != null ? responseData.Value : string.Empty;
        }

        public string GetSysprepFile(int id)
        {
            Request.Method = Method.GET;
            Request.Resource = string.Format("ProvisionedComm/{0}/GetSysprepFile/{1}", Resource, id);
            var responseData = new ApiRequest().ExecuteSymKeyEncryption<DtoStringResponse>(Request, string.Empty);
            return responseData != null ? responseData.Value : string.Empty;
        }

        public List<DtoClientFileRequest> GetFileCopyModule(int id)
        {
            Request.Method = Method.GET;
            Request.Resource = string.Format("ProvisionedComm/{0}/GetFileCopyModule/{1}", Resource,id);
            return new ApiRequest().ExecuteSymKeyEncryption<List<DtoClientFileRequest>>(Request, string.Empty);

        }

    }
}