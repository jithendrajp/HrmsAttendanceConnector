using iCuboid.HRMS.DataConnector.Enums;
using iCuboid.HRMS.DataConnector.Models;
using iCuboid.HRMS.DataConnector.PublicTypes;
using log4net;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iCuboid.HRMS.DataConnector
{
    public static class HrmsConnector
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly string _DOMAIN= ConfigurationManager.AppSettings["ErpNextDomain"];
        private static readonly string _USERNAME = ConfigurationManager.AppSettings["ErpNextUserName"];
        private static readonly string _PASSWORD = ConfigurationManager.AppSettings["ErpNextPassword"];

        private static readonly ERPNextClient Client = new ERPNextClient(_DOMAIN, _USERNAME,_PASSWORD);

        public static void doLogin()
        {
            Client.Login(_USERNAME, _PASSWORD);
        }

        public async  static Task<DateTime> CheckTheLastUpdatedAttendance(string empid)
        {
            Log.Info($"Checking LastUpdatedAttendanceDate of employee:{empid}");
            DateTime LastUpdatedAttendanceDt = DateTime.MinValue;
            FetchListOption listOption = new FetchListOption();
            listOption.Filters.Add(new ERPFilter(DocType.Employee, "name", OperatorFilter.Equals, empid));
            listOption.IncludedFields.AddRange(new string[] { "name", "employee_name", "date_of_joining", "last_attendance_date" });
            var employees= Client.ListObjects(DocType.Employee, listOption);          
            if (employees != null && employees.Count > 0) {
                //For the first time ReadOnly field  last_attendance_date will be null
                if (employees[0].Data.last_attendance_date == null)
                {
                    LastUpdatedAttendanceDt = DateTime.Now.AddDays(-2);
                }
                else
                {
                    LastUpdatedAttendanceDt = DateTime.Parse(employees[0].Data.last_attendance_date);
                }
            }
            Log.Info($"LastUpdatedAttendanceDate of employee:{empid} is {LastUpdatedAttendanceDt}");
            return LastUpdatedAttendanceDt;
        }
        public async static Task UpdateAttendance(DateTime ProcessingDate, DateTime ProcessingDayCheckIn, DateTime ProcessingDayCheckOut, string empid)
        {

            try
            {
                var employee = Client.GetObject(DocType.Employee, empid);
                if (employee != null)
                {
                    Log.Info($"updating attendance of employee:{empid} for the day {ProcessingDate} with chekin {ProcessingDayCheckIn} and checkout{ProcessingDayCheckOut}");
                    

                    int ShiftOneHalfDayThresholdHour = 0;
                    int ShiftTwoHalfDayThresholdHour = 0;
                    int ShiftThreeHalfDayThresholdHour = 0;
                    float workinghours = 0;
                    float othours = 0;
                    ERPAttendance attendance = new ERPAttendance();
                    List<TimeSheetLog> TimeSheetLogs = new List<TimeSheetLog>();
                    TimeSheetLog timeSheetLog = new TimeSheetLog();
                    ShiftOneHalfDayThresholdHour = Convert.ToInt32(ConfigurationManager.AppSettings["ShiftOneHalfDayThresholdHour"]);
                    ShiftTwoHalfDayThresholdHour = Convert.ToInt32(ConfigurationManager.AppSettings["ShiftTwoHalfDayThresholdHour"]);
                    ShiftThreeHalfDayThresholdHour = Convert.ToInt32(ConfigurationManager.AppSettings["ShiftThreeHalfDayThresholdHour"]);

                    //fetching shift timings from webconfig
                    IList<string> ShiftOneStartList = ConfigurationManager.AppSettings["ShiftOneStartTime"].Split(':');
                    TimeSpan ShiftOneStartTs = new TimeSpan(Convert.ToInt32(ShiftOneStartList[0]), Convert.ToInt32(ShiftOneStartList[1]), 0);
                    var ShiftOneStartdate = ProcessingDate.Date.Add(ShiftOneStartTs);

                    IList<string> ShiftOneEndList = ConfigurationManager.AppSettings["ShiftOneEndTime"].Split(':');
                    TimeSpan ShiftOneEndTs = new TimeSpan(Convert.ToInt32(ShiftOneEndList[0]), Convert.ToInt32(ShiftOneEndList[1]), 0);
                    var ShiftOneEnddate = ProcessingDate.Date.Add(ShiftOneEndTs);

                    IList<string> ShiftTwoStartList = ConfigurationManager.AppSettings["ShiftTwoStartTime"].Split(':');
                    TimeSpan ShiftTwoStartTs = new TimeSpan(Convert.ToInt32(ShiftTwoStartList[0]), Convert.ToInt32(ShiftTwoStartList[1]), 0);
                    var ShiftTwoStartdate = ProcessingDate.Date.Add(ShiftTwoStartTs);

                    IList<string> ShiftTwoEndList = ConfigurationManager.AppSettings["ShiftTwoEndTime"].Split(':');
                    TimeSpan ShiftTwoEndTs = new TimeSpan(Convert.ToInt32(ShiftTwoEndList[0]), Convert.ToInt32(ShiftTwoEndList[1]), 0);
                    var ShiftTwoEnddate = ProcessingDate.Date.Add(ShiftTwoEndTs).AddDays(1);

                    IList<string> ShiftThreeStartList = ConfigurationManager.AppSettings["ShiftThreeStartTime"].Split(':');
                    TimeSpan ShiftThreeStartTs = new TimeSpan(Convert.ToInt32(ShiftThreeStartList[0]), Convert.ToInt32(ShiftThreeStartList[1]), 0);
                    var ShiftThreeStartdate = ProcessingDate.Date.Add(ShiftThreeStartTs);

                    IList<string> ShiftThreeEndList = ConfigurationManager.AppSettings["ShiftThreeEndTime"].Split(':');
                    TimeSpan ShiftThreeEndTs = new TimeSpan(Convert.ToInt32(ShiftThreeEndList[0]), Convert.ToInt32(ShiftThreeEndList[1]), 0);
                    var ShiftThreeEnddate = ProcessingDate.Date.Add(ShiftThreeEndTs);

                    
                    //If there is no checkin records in AMS for the day
                    if (ProcessingDayCheckIn == DateTime.MinValue)
                    {
                        //Check the employee is on leave or not
                        Log.Info($"Checking employee {empid} is leave or not");
                        FetchListOption listOption = new FetchListOption();
                        listOption.Filters.Add(new ERPFilter(DocType.Attendance, "employee_id", OperatorFilter.Equals, empid));
                        listOption.Filters.Add(new ERPFilter(DocType.Attendance, "attendance_date", OperatorFilter.Equals, ProcessingDate.ToString("yyyy-MM-dd")));
                        listOption.IncludedFields.AddRange(new string[] { "name", "status", "attendance_date" });
                        var documents = Client.ListObjects(DocType.Attendance, listOption);

                        if (documents.Count > 0 && documents != null)
                        {
                            attendance.status = (AttendanceStatusEnum)Enum.Parse(typeof(AttendanceStatusEnum), documents[0].Data.status.Replace(" ", ""));
                            attendance.attendance_date = documents[0].Data.attendance_date.ToString();
                            Log.Info($"Employee {empid} is on leave with status {attendance.status}");
                        }
                        else
                        {
                            //Not on Leave and also there is no swipe
                            attendance.status = AttendanceStatusEnum.Absent;
                            attendance.attendance_date = ProcessingDate.ToString("yyyy-MM-dd");
                            Log.Info($"Employee {empid} is Not on leave and there is no swipe so status :{attendance.status}");
                        }
                    }

                    if (ProcessingDayCheckIn <= ShiftOneStartdate && ProcessingDayCheckOut <= ShiftOneEnddate )
                    {
                        Log.Info($"Employee {empid} is found in shift1 so processing attendance");
                        //if checkout is before HalfDayThreshold
                        if (ProcessingDayCheckOut < ProcessingDate.Date.AddHours(ShiftOneHalfDayThresholdHour))
                        {
                            attendance.status = AttendanceStatusEnum.Absent;
                            attendance.attendance_date = ProcessingDayCheckOut.ToString("yyyy-MM-dd");
                            Log.Info($"Employee {empid} checkout {ProcessingDayCheckOut} before HalfDayThreshold so marking as leave with status {attendance.status}");

                        }
                        //if checkout is before FullDayThreshold 
                        else if (ProcessingDayCheckOut < ShiftOneEnddate)
                        {
                            attendance.status = AttendanceStatusEnum.HalfDay;
                            attendance.attendance_date = ProcessingDayCheckOut.ToString("yyyy-MM-dd");
                            Log.Info($"Employee {empid}  chekout {ProcessingDayCheckOut} before Full Day Threshold so marking attendance status as {attendance.status}");

                        }
                        else
                        {
                            attendance.status = AttendanceStatusEnum.Present;
                            attendance.attendance_date = ProcessingDayCheckOut.ToString("yyyy-MM-dd");
                            var ts = (ProcessingDayCheckOut - ProcessingDayCheckIn);
                            var h = Math.Floor(ts.TotalHours);
                            var m = (ts.TotalHours - h) * 60;
                            workinghours = (float)(h + m / 60);
                            timeSheetLog.from_time = ProcessingDayCheckIn.ToString("yyyy-MM-dd");
                            timeSheetLog.to_time = ProcessingDayCheckOut.ToString("yyyy-MM-dd");
                            if (workinghours > 8.35)
                            {

                                othours = workinghours - (float)8.35;
                                timeSheetLog.hours = (float)Math.Round(othours);
                                timeSheetLog.activity_type = "Execution";
                                TimeSheetLogs.Add(timeSheetLog);
                            }


                            Log.Info($"Employee {empid} checkout {ProcessingDayCheckOut} properly and marking attendance status {attendance.status} and workinghours {timeSheetLog.hours}");
                        }

                    }
                    else if (ProcessingDayCheckIn <= ShiftTwoStartdate && ProcessingDayCheckOut <= ShiftTwoEnddate)
                    {
                        Log.Info($"Employee {empid} is found in shift2 so processing attendance");
                        //this shift is ending in next day so feteching chekouttime again
                        if (ProcessingDayCheckOut== ProcessingDayCheckIn)
                        {
                            ProcessingDayCheckOut = await AMSConnector.GetChekOutTime(empid, ProcessingDate.AddDays(1));
                        }
                        //if checkout is before HalfDayThreshold
                        if (ProcessingDayCheckOut < ProcessingDate.Date.AddHours(ShiftTwoHalfDayThresholdHour))
                        {
                            attendance.status = AttendanceStatusEnum.Absent;
                            attendance.attendance_date = ProcessingDayCheckOut.ToString("yyyy-MM-dd");
                            Log.Info($"Employee {empid} checkout {ProcessingDayCheckOut} before HalfDayThreshold so marking as leave with status {attendance.status}");

                        }
                        //if checkout is before FullDayThreshold 
                        else if (ProcessingDayCheckOut < ShiftTwoEnddate)
                        {
                            attendance.status = AttendanceStatusEnum.HalfDay;
                            attendance.attendance_date = ProcessingDayCheckOut.ToString("yyyy-MM-dd");
                            Log.Info($"Employee {empid}  chekout {ProcessingDayCheckOut} before Full Day Threshold so marking attendance status as {attendance.status}");

                        }
                        else
                        {
                            attendance.status = AttendanceStatusEnum.Present;
                            attendance.attendance_date = ProcessingDayCheckOut.ToString("yyyy-MM-dd");
                            var ts = (ProcessingDayCheckOut - ProcessingDayCheckIn);
                            var h = Math.Floor(ts.TotalHours);
                            var m = (ts.TotalHours - h) * 60;
                            workinghours = (float)(h + m / 60);
                            timeSheetLog.from_time = ProcessingDayCheckIn.ToString("yyyy-MM-dd");
                            timeSheetLog.to_time = ProcessingDayCheckOut.ToString("yyyy-MM-dd");
                            if (workinghours > 8.35)
                            {

                                othours = workinghours - (float)8.35;
                                timeSheetLog.hours = (float)Math.Round(othours);
                                timeSheetLog.activity_type = "Execution";
                                TimeSheetLogs.Add(timeSheetLog);
                            }


                            Log.Info($"Employee {empid} checkout {ProcessingDayCheckOut} properly and marking attendance status {attendance.status} and workinghours {timeSheetLog.hours}");
                        }

                    }
                    else if (ProcessingDayCheckIn <= ShiftThreeStartdate && ProcessingDayCheckOut <= ShiftThreeEnddate)
                    {
                        Log.Info($"Employee {empid} is found in shift3 so processing attendance");
                        //if checkout is before HalfDayThreshold
                        if (ProcessingDayCheckOut < ProcessingDate.Date.AddHours(ShiftThreeHalfDayThresholdHour))
                        {
                            attendance.status = AttendanceStatusEnum.Absent;
                            attendance.attendance_date = ProcessingDayCheckOut.ToString("yyyy-MM-dd");
                            Log.Info($"Employee {empid} checkout {ProcessingDayCheckOut} before HalfDayThreshold so marking as leave with status {attendance.status}");

                        }
                        //if checkout is before FullDayThreshold 
                        else if (ProcessingDayCheckOut < ShiftThreeEnddate)
                        {
                            attendance.status = AttendanceStatusEnum.HalfDay;
                            attendance.attendance_date = ProcessingDayCheckOut.ToString("yyyy-MM-dd");
                            Log.Info($"Employee {empid}  chekout {ProcessingDayCheckOut} before Full Day Threshold so marking attendance status as {attendance.status}");

                        }
                        else
                        {
                            attendance.status = AttendanceStatusEnum.Present;
                            attendance.attendance_date = ProcessingDayCheckOut.ToString("yyyy-MM-dd");
                            var ts = (ProcessingDayCheckOut - ProcessingDayCheckIn);
                            var h = Math.Floor(ts.TotalHours);
                            var m = (ts.TotalHours - h) * 60;
                            workinghours = (float)(h + m / 60);
                            timeSheetLog.from_time = ProcessingDayCheckIn.ToString("yyyy-MM-dd");
                            timeSheetLog.to_time = ProcessingDayCheckOut.ToString("yyyy-MM-dd");
                            if (workinghours > 8.35)
                            {

                                othours = workinghours - (float)8.35;
                                timeSheetLog.hours = (float)Math.Round(othours);
                                timeSheetLog.activity_type = "Execution";
                                TimeSheetLogs.Add(timeSheetLog);
                            }


                            Log.Info($"Employee {empid} checkout {ProcessingDayCheckOut} properly and marking attendance status {attendance.status} and workinghours {timeSheetLog.hours}");
                        }

                    }


                    
                    attendance.employee_name = employee.Data.employee_name;
                    attendance.employee = employee.Data.employee;
                    attendance.company = employee.Data.company;
                    attendance.department = employee.Data.department;                    
                    attendance.docstatus = 1;

                    //mark the attendance
                    Client.InsertObject(attendance.Object);
                    Log.Info($"Marked the attendance for the eemployee {empid} successfully");

                    //create the time sheet for the employee
                    if (TimeSheetLogs.Count > 0)
                    {
                        ERPTimesheet timesheet = new ERPTimesheet();
                        timesheet.employee_name = employee.Data.employee_name;
                        timesheet.employee = employee.Data.employee;
                        timesheet.company = employee.Data.company;
                        timesheet.department = employee.Data.department;
                        timesheet.time_logs = TimeSheetLogs;
                        timesheet.docstatus = 0;
                        Client.InsertObject(timesheet.Object);
                        Log.Info($"created OT for the employee {empid}  successfully");
                    }

                    //finally update Employee with lastattendance date
                    ERPObject updated_obj = new ERPObject(DocType.Employee);
                    updated_obj.Data.last_attendance_date = ProcessingDate.ToString("yyyy-MM-dd");
                    Client.UpdateObject(DocType.Employee, empid, updated_obj);
                    Log.Info($"updated the processed date for the employee {empid} successfully");
                }
            }

            catch (Exception ex)
            {

                Log.Error(ex);
            }
        }
    }

}

