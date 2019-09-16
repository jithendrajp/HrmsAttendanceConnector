using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iCuboid.HRMS.DataConnector.Models
{
    public class Employee
    {
        public string EmpID { get; set; }
        public string CardID { get; set; }
        public string empname { get; set; }
        public string TzXsGrp { get; set; }
        public string WoXsGrp { get; set; }
        public string HoXsGrp { get; set; }
        public string loccode { get; set; }
        public string FPImage1 { get; set; }
        public string FPImage2 { get; set; }
        public string emppin { get; set; }
        public DateTime? expirydate { get; set; }
        public string photo { get; set; }
        public string deptid { get; set; }
        public string type { get; set; }
        public string sitecode { get; set; }
        public string empstatus { get; set; }
        public string location { get; set; }
        public string email { get; set; }
        public string XsInfo { get; set; }
        public string SapId { get; set; }
        public DateTime? Dob { get; set; }
        public string gender { get; set; }
        public string address { get; set; }
        public string tel1 { get; set; }
        public string tel2 { get; set; }
        public DateTime? doj { get; set; }
        public DateTime? dol { get; set; }
        public string remark { get; set; }
        public string MachineName { get; set; }
        public string BFlag { get; set; }
        public string MFlag { get; set; }

    }
}
