﻿using iCuboid.HRMS.DataConnector.PublicTypes;
using iCuboid.HRMS.DataConnector.WrapperTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iCuboid.HRMS.DataConnector.Models
{
    public class ERPTimesheet: ERPNextObjectBase
    {
        public ERPTimesheet() : this(new ERPObject(DocType.Timesheet)) { }
        public ERPTimesheet(ERPObject obj) : base(obj) { }

        public static ERPTimesheet Create(float totalhours, string depmnt, string employeeid, string empname, string company)
        {
            ERPTimesheet obj = new ERPTimesheet();
            obj.total_hours = totalhours;
            obj.department = depmnt;
            obj.employee = employeeid;
            obj.employee_name = empname;
            obj.company = company;
            return obj;
        }

        public float total_hours
        {
            get { return data.total_hours; }
            set
            {
                data.total_hours = value;
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
        public IList<TimeSheetLog> time_logs
        {
            get { return data.time_logs; }

            set
            {
                data.time_logs = value;
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
