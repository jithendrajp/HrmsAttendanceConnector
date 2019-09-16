using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iCuboid.HRMS.DataConnector.Models
{
    public class TimeSheetLog
    {
        public string from_time { get; set; }

        public float hours { get; set; }

        public string to_time { get; set; }

        public string activity_type { get; set; }
    }
}
