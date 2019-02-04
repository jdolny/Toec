using RestSharp;
using Toec_Common.Dto;

namespace Toec_Services.ApiCall
{
    public class TrayAPI : BaseAPI
    {
        public TrayAPI(string resource) : base(resource)
        {
        }

        public bool SendMessage(string message, string title, string port,int timeout)
        {
            Request.Method = Method.GET;
            Request.Resource = "ToecUI/Message/DisplayMessage/";
            Request.AddParameter("message", message);
            Request.AddParameter("title", title);
            Request.AddParameter("timeout", timeout);
            try
            {
                var result = new ApiRequest("http://localhost:" + port + "/").Execute<DtoBoolResponse>(Request);
                return result != null && result.Value;
            }
            catch
            {
                return false;
            }
            
           
        }
    }
}