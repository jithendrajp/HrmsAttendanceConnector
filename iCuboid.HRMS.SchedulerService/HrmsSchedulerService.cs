using iCuboid.HRMS.DataConnector;
using log4net;
using System;
using System.Configuration;
using System.Diagnostics;
using System.ServiceProcess;
using System.Timers;

namespace iCuboid.HRMS.SchedulerService
{
    public partial class HrmsSchedulerService : ServiceBase
    {
        ILog Log = LogManager.GetLogger(typeof(HrmsSchedulerService));
        private Timer _timer;
        private DateTime _scheduleTime;
        public HrmsSchedulerService()
        {
            InitializeComponent();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                Log.Info("Service starting");
                _timer = new Timer();
                // Read Scheduletime  for run from appsettings.
                var servicerunningtime = ConfigurationManager.AppSettings["SchedulerStartTime"].ToString();
                _scheduleTime = DateTime.Parse(servicerunningtime);

                _timer.Enabled = true;
                //Test if its a time in the past and protect setting _timer.Interval with a negative number which causes an error.
                double tillNextInterval = _scheduleTime.Subtract(DateTime.Now).TotalSeconds * 1000;
                if (tillNextInterval < 0)
                    tillNextInterval += new TimeSpan(24, 0, 0).TotalSeconds * 1000;
                _timer.Interval = tillNextInterval;
                _timer.Elapsed += new ElapsedEventHandler(Timer_Elapsed);

            }
            catch (Exception ex)
            {
                Log.Error(ex);
                Stop();
            }


        }

        protected override void OnStop()
        {
            try
            {
                Log.Info("Service stopping");
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Log.Info($"Triggered -{DateTime.Now}");
            AMSConnector.Connect().GetAwaiter().GetResult();
            Log.Info($"End -{DateTime.Now} operation");
            //setting timer to next day
            if (_timer.Interval != 24 * 60 * 60 * 1000)
            {
                _timer.Interval = 24 * 60 * 60 * 1000;
            }
        }
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var newExc = new Exception("CurrentDomainOnUnhandledException", e.ExceptionObject as Exception);
            Log.Error("UnhandledException\r\n" + newExc.ToString());

        }
    }
}
