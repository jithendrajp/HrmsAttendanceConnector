using iCuboid.HRMS.DataConnector.PublicTypes;
using iCuboid.HRMS.DataConnector.WrapperTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iCuboid.HRMS.DataConnector.Models
{
    public class ERPShiftAssignment: ERPNextObjectBase
    {
        public ERPShiftAssignment() : this(new ERPObject(DocType.ShiftAssignment)) { }
        public ERPShiftAssignment(ERPObject obj) : base(obj) { }

        public static ERPShiftAssignment Create(string time, string shiftType)
        {
            ERPShiftAssignment obj = new ERPShiftAssignment();
            obj.date = time;
            obj.shift_type = shiftType;
            return obj;
        }

       
        public string date
        {
            get { return data.date; }
            set
            {
                data.date = value;
            }
        }
        public string shift_type
        {
            get { return data.shift_type; }
            set
            {
                data.shift_type = value;
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
