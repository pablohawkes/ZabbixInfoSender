using System;
using System.Reflection;
using System.ServiceProcess;
using System.Timers;

namespace ZabbixInfoSender
{
    public partial class SvcMain : ServiceBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType); //log4net declare
        private static System.Timers.Timer aTimer;
        private static ZabbixInfoSenderInformation ZabbixInfoSenderInformation;

        public SvcMain()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            //UNCOMMENT TO ENABLE DEBUGGER:
            //System.Diagnostics.Debugger.Launch();

            log.Info("***********************************************************************************************");
            log.Info("***********************************************************************************************");
            log.Info("Service Started - Version: " + Assembly.GetExecutingAssembly().GetName().Version);

            try
            {
                ZabbixInfoSenderInformation = new ZabbixInfoSenderInformation();
            }
            catch (Exception exc)
            {
                log.Error("Error on constructor ZabbixInfoSenderInformation()" + Environment.NewLine, exc);
                Environment.Exit(-1);
            }


            var TimerSeconds = ZabbixInfoSenderInformation.ControlIntervalSeconds;
            if (TimerSeconds <= 0 || TimerSeconds >= 3600)
            {
                log.Error("Parameter ControlIntervalSeconds has wrong value: " + TimerSeconds.ToString());
                Environment.Exit(-2);
            }

            // Create timer for ControlIntervalSeconds.
            aTimer = new System.Timers.Timer(TimerSeconds * 1000);


            // Hook up the Elapsed event for the timer.
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Enabled = true;
        }

        protected override void OnStop()
        {
            log.Info("Service Stopped");
        }

        //protected override void OnPause()
        //{
        //    log.Info("OnPause event");
        //}

        //protected override void OnContinue()
        //{
        //    log.Info("Service Continued");
        //}

        protected override void OnShutdown()
        {
            log.Info("Service Stopped due Shutdown");
        }

        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            aTimer.Stop();
            log.Info("--------------------------------------------------------------------");
            log.Info("OnTimedEvent Started");
            try
            {

                ZabbixInfoSenderInformation.ExecuteInfoGatheringAndSendToZabbix();
                //log.Debug("OnTimedEvent Executed OK");

            }
            catch (Exception exc)
            {
                log.Error("Error on OnTimedEvent" + Environment.NewLine, exc);
            }
            finally
            {
                log.Info("OnTimedEvent Finished");
                aTimer.Start();
            }
        }
    }
}
