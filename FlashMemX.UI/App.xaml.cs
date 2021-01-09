using System.Windows;

namespace FlashMemX.UI
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ProgramManager.Setup();
        }
    }
}
