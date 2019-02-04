using System.Collections.Generic;

namespace Toec_Common.Modules
{
    public class DtoClientFileCopyModule
    {
        public DtoClientFileCopyModule()
        {
            Files = new List<DtoClientFileHash>();
        }

        public string Destination { get; set; }
        public string DisplayName { get; set; }

        public List<DtoClientFileHash> Files { get; set; }
        public string Guid { get; set; }
        public int Order { get; set; }
        public bool Unzip { get; set; }
    }
}