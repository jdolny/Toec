using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toec_Common.Modules
{
    public class DtoClientWuModule
    {
        public DtoClientWuModule()
        {
            Files = new List<DtoClientFileHash>();
            SuccessCodes = new List<string>();

        }
        public string Guid { get; set; }
        public string DisplayName { get; set; }
        public string Arguments { get; set; }
        public int Order { get; set; }
        public int Timeout { get; set; }
        public bool RedirectOutput { get; set; }
        public bool RedirectError { get; set; }
        public List<DtoClientFileHash> Files { get; set; }
        public List<string> SuccessCodes { get; set; }
    }
}
