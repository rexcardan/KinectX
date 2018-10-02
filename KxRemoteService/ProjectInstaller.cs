using System.ComponentModel;
using System.Configuration.Install;

namespace KxRemoteService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }

        private void serviceInstaller1_AfterInstall(object sender, InstallEventArgs e)
        {
            using (System.ServiceProcess.ServiceController sc = new System.ServiceProcess.ServiceController(serviceInstaller1.ServiceName))
            {
                sc.Start();
            }

        }
    }
}
