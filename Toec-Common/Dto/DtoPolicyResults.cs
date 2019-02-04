using System.Collections.Generic;

namespace Toec_Common.Dto
{
    public class DtoPolicyResults
    {
        public List<DtoScriptModuleOutput> CustomInventories;
        public List<DtoServerPolicyHistory> Histories;

        public DtoPolicyResults()
        {
            Histories = new List<DtoServerPolicyHistory>();
            CustomInventories = new List<DtoScriptModuleOutput>();
        }
    }
}