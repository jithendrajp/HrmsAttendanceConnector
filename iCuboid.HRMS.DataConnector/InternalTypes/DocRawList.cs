using System.Collections.Generic;

namespace iCuboid.HRMS.DataConnector.InternalTypes
{
    internal class DocRawList
    {
        public List<Dictionary<string, object>> data { get; set; }
    }

    internal class DocRaw
    {
        public Dictionary<string, object> data { get; set; }
    }

}