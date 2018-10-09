using NLog;
using System;
using System.ServiceProcess;
using System.Threading;
using System.Timers;

namespace KxRemoteService
{
    public partial class Service1 : ServiceBase
    {
        private ILogger _logger = LogManager.GetCurrentClassLogger();
        private System.Timers.Timer timer;

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            // Set up a timer to trigger at next scheduled time
            timer = new System.Timers.Timer();
            //We are going to come back in 20 seconds to do something so the service doesn't hang
            var nextTime = 20000; //20 seconds from now
            timer.Interval = nextTime;
            timer.AutoReset = false;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
            timer.Start();
        }

        private void OnTimer(object sender, ElapsedEventArgs e)
        {
            _logger.Info("Starting service...");
            KinectX.Network.KxServer.Start();
            _logger.Info("Service started. Waiting for stop...");
            _logger.Info("Service stopped");
            timer.Stop();
        }

        protected override void OnStop()
        {
            _logger.Info("Stopping Service...");
            KinectX.Network.KxServer.Stop();
        }
    }
}
