using NLog;
using System.ServiceProcess;

namespace KxRemoteService
{
    public partial class Service1 : ServiceBase
    {
        private ILogger _logger = LogManager.GetCurrentClassLogger();

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _logger.Info("Starting service...");
            KinectX.Network.KxServer.Start();
            _logger.Info("Service started. Waiting for stop...");
            _logger.Info("Service stopped");

        }

        protected override void OnStop()
        {
            _logger.Info("Stopping Service...");
            KinectX.Network.KxServer.Stop();
        }
    }
}
