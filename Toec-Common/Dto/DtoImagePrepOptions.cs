using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toec_Common.Dto
{
    public class DtoImagePrepOptions
    {
        public bool RunHibernate { get; set; }
        public bool AddDriverRegistry { get; set; }
        public bool EnableFinalizingBackground { get; set; }
        public bool CreateSetupComplete { get; set; }
        public bool RunSysprep { get; set; }
        public bool ResetToec { get; set; }
        public string SetupCompleteContents { get; set; }
        public string SysprepAnswerPath { get; set; }
    }
}
