namespace Toec_Services.ApiCall
{
    public class APICall : IAPICall
    {
        public InventoryAPI InventoryApi
        {
            get { return new InventoryAPI("Inventory"); }
        }

        public LocalAPI LocalApi
        {
            get { return new LocalAPI("Local"); }
        }

        public ProvisionAPI ProvisionApi
        {
            get { return new ProvisionAPI("Provision"); }
        }

        public TrayAPI TrayApi
        {
            get { return new TrayAPI("Tray"); }
        }

        public PolicyAPI PolicyApi
        {
            get { return new PolicyAPI("Policy"); }
        }

        public ImagePrepAPI ImagePrepApi
        {
            get { return new ImagePrepAPI("ImagePrep"); }
        }
    }
}