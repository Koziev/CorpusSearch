using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CorpusSearch
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void App_Startup(object sender, StartupEventArgs e)
        {
            log4net.Config.XmlConfigurator.Configure();

            UserConfigManager user_cfg_manager = new UserConfigManager();

            user_cfg_manager.AllowSaving = false;
            MainWindow mainWindow = new MainWindow(user_cfg_manager);
            mainWindow.Show();
            user_cfg_manager.AllowSaving = true;
        }
    }
}
