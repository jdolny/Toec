using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toec_Common.Enum
{
    public class EnumCondition
    {
        public enum FailedAction
        {
            MarkNotApplicable = 0,
            MarkSkipped = 1,
            MarkSuccess = 2,
            MarkFailed = 3,
            GotoModule = 4

        }
    }
}
