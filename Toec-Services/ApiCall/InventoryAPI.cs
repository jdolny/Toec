using Newtonsoft.Json;
using RestSharp;
using Toec_Common.Dto;
using Toec_Common.Inventory;

namespace Toec_Services.ApiCall
{
    public class InventoryAPI : BaseAPI
    {
        public InventoryAPI(string resource) : base(resource)
        {
        }

        public DtoBoolResponse SubmitInventory(DtoInventoryCollection collection)
        {
            Request.Method = Method.POST;
            Request.Timeout = 900000;
            Request.Resource = string.Format("ProvisionedComm/{0}/SubmitInventory/", Resource);
            return new ApiRequest(900000).ExecuteSymKeyEncryption<DtoBoolResponse>(Request,
                JsonConvert.SerializeObject(collection));
        }
    }
}