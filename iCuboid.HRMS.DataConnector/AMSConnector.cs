using iCuboid.HRMS.DataConnector.Models;
using iCuboid.HRMS.DataConnector.PublicTypes;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iCuboid.HRMS.DataConnector
{
    public static class AMSConnector
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static SqlConnection connection { get; set; }
        private static SqlCommand command { get; set; }
        private static SqlDataReader dataReader { get; set; }
        private static List<Employee> Employees { get; set; }
        private static readonly string _DOMAIN = ConfigurationManager.AppSettings["ErpNextDomain"];

        private static readonly string _USERNAME = ConfigurationManager.AppSettings["ErpNextUserName"];

        private static readonly string _PASSWORD = ConfigurationManager.AppSettings["ErpNextPassword"];

        private static readonly ERPNextClient Client = new ERPNextClient(_DOMAIN, _USERNAME, _PASSWORD);
        public async static Task Connect()
        {

            try
            {
                Employees = new List<Employee>();
                string connetionString = ConfigurationManager.ConnectionStrings["netxsEntities"].ConnectionString.ToString();
                connection = new SqlConnection(connetionString);
                connection.Open();
                string sql = "SELECT * FROM Emp";
                command = new SqlCommand(sql, connection);
                dataReader = command.ExecuteReader();
                var datatable = new DataTable();
                datatable.Load(dataReader);
                string JsonResponse = string.Empty;
                JsonResponse = JsonConvert.SerializeObject(datatable);
                Employees = JsonConvert.DeserializeObject<List<Employee>>(JsonResponse);
                await MarkTheAttendance(Employees);
                dataReader.Close();
                command.Dispose();


            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private async static Task MarkTheAttendance(List<Employee> Employees)
        {
            DateTime LastProcessedDate = DateTime.MinValue;
            DateTime ProcessingDate = DateTime.MinValue;
            DateTime ProcessingDayCheckIn = DateTime.MinValue;
            DateTime ProcessingDayCheckOut = DateTime.MinValue;
            String EmployeName = "";
            HrmsConnector.doLogin();
            foreach (var emp in Employees)
            {
                if (emp.EmpID != null)
                {
                    var employee = Client.GetObject(DocType.Employee, emp.EmpID);
                    //check employee is in  active state or not
                    if (employee != null && employee.Data.status == "Active")
                    {
                        //Get the Last updated date of this employee
                        LastProcessedDate = await HrmsConnector.CheckTheLastUpdatedAttendance(emp.EmpID);
                        ProcessingDate = LastProcessedDate.AddDays(1);

                        if (LastProcessedDate != DateTime.MinValue && ProcessingDate.Date != DateTime.Now.Date)
                        {
                            while (ProcessingDate.Date < DateTime.Now.Date)
                            {
                                string sql = $"select min(dt) as CheckIn,max(dt) as CheckOut,empname  from [netxs].[dbo].[Trans] where EmpID='{emp.EmpID}' and CAST(dt as date) = CAST('{ProcessingDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}' as date)  group by EmpID,empname";
                                command = new SqlCommand(sql, connection);
                                dataReader = command.ExecuteReader();
                                while (dataReader.Read())
                                {
                                    ProcessingDayCheckIn = (DateTime)dataReader.GetValue(0);
                                    ProcessingDayCheckOut = (DateTime)dataReader.GetValue(1);
                                }
                                await HrmsConnector.UpdateAttendance(ProcessingDate, ProcessingDayCheckIn, ProcessingDayCheckOut, emp.EmpID);
                                ProcessingDate = ProcessingDate.AddDays(1);
                            }
                        }
                    }
                }
            }
        }
        public async static Task<DateTime> GetChekOutTime(string empid,DateTime ProcessingDate)
        {
            var CheckOut = ProcessingDate;
            string sql = $"select min(dt) as CheckIn,max(dt) as CheckOut,empname  from [netxs].[dbo].[Trans] where EmpID='{empid}' and CAST(dt as date) = CAST('{ProcessingDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}' as date)  group by EmpID,empname";
            command = new SqlCommand(sql, connection);
            dataReader = command.ExecuteReader();
            while (dataReader.Read())
            {
                
                CheckOut = (DateTime)dataReader.GetValue(1);
            }
            return CheckOut;
        }


        public async static Task ConnectorClose()
        {

            connection.Close();

        }


    }
}
