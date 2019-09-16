using iCuboid.HRMS.DataConnector.Enums;
using iCuboid.HRMS.DataConnector.PublicTypes;
using iCuboid.HRMS.DataConnector.WrapperTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iCuboid.HRMS.DataConnector.Models
{
    public class ERPAttendance: ERPNextObjectBase
    {
        public ERPAttendance() : this(new ERPObject(DocType.Attendance)) { }
        public ERPAttendance(ERPObject obj) : base(obj) { }

        public static ERPAttendance Create(string attendancedate, AttendanceStatusEnum status, string depmnt, string employeeid, string empname, string company)
        {
            ERPAttendance obj = new ERPAttendance();
            obj.attendance_date = attendancedate;
            obj.status = status;
            obj.department = depmnt;
            obj.employee = employeeid;
            obj.employee_name = empname;
            obj.company = company;
            return obj;
        }

        public string attendance_date
        {
              get { return data.attendance_date; }
                set
                {
                    data.attendance_date = value;
                }
            
        }
        public string company
        {
            get { return data.company; }
            set
            {
                data.company = value;
            }
        }
        public string department
        {
            get { return data.department; }
            set
            {
                data.department = value;
            }
        }
        public string employee
        {
            get { return data.employee; }
            set
            {
                data.employee = value;
                
            }
        }

        public string employee_name
        {
            get { return data.employee_name; }
            set
            {
                data.employee_name = value;
                data.name = value;
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
        public AttendanceStatusEnum status
        {
            get { return parseEnum<AttendanceStatusEnum>(data.status); }
            set { data.status = value.ToString(); }
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
