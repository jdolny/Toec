using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toec_Common.Dto
{
    public class DtoWolTask
    {
        public DtoWolTask()
        {
            Macs = new List<string>();
        }

        public string Gateway { get; set; }
        public List<string> Macs { get; set; }
    }
}
