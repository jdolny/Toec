using RestSharp;

namespace Toec_Services.ApiCall
{
    public class BaseAPI
    {
        protected readonly RestRequest Request;
        protected readonly string Resource;

        public BaseAPI(string resource)
        {
            Request = new RestRequest();
            Request.Timeout = 120000;
            Resource = resource;
        }
    }
}