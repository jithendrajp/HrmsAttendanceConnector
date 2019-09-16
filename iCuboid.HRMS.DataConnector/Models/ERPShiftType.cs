using iCuboid.HRMS.DataConnector.PublicTypes;
using iCuboid.HRMS.DataConnector.WrapperTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iCuboid.HRMS.DataConnector.Models
{
    public class ERPShiftType: ERPNextObjectBase
    {
        public ERPShiftType() : this(new ERPObject(DocType.ShiftType)) { }
        public ERPShiftType(ERPObject obj) : base(obj) { }

        public static ERPShiftType Create(string starttime, string endtime)
        {
            ERPShiftType obj = new ERPShiftType();
            obj.start_time = starttime;
            obj.end_time = endtime;
            return obj;
        }

       
        public string start_time
        {
            get { return data.start_time; }
            set
            {
                data.start_time = value;
            }
        }
        public string end_time
        {
            get { return data.end_time; }
            set
            {
                data.end_time = value;
            }
        }
     
        public string creation
        {
            get { return data.creation; }
            set
            {
                data.creation = value;
            }
        }
        public DateTime modified
        {
            get { return data.modified; }
            set
            {
                data.modified = DateTime.Now;
            }
        }
        public int docstatus
        {
            get { return data.docstatus; }
            set
            {
                data.docstatus = value;
            }
        }
    }
}
