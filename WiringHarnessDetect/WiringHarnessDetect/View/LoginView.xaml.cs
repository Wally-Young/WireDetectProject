using GalaSoft.MvvmLight.Messaging;
using WiringHarnessDetect.Model;
using WiringHarnessDetect.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Panuon.UI.Silver;

namespace WiringHarnessDetect.View
{
    /// <summary>
    /// LoginView.xaml 的交互逻辑
    /// </summary>
    public partial class LoginView : WindowX
    {
        public LoginView()
        {
            InitializeComponent();
            Messenger.Default.Register<User>(this, "LogSuccess", LogSuccess);
            //卸载当前(this)对象注册的所有MVVMLight消息
            //this.Unloaded += (sender, e) => Messenger.Default.Unregister(this);
        }

        private void LogSuccess(User user)
        {

            var mainview = CommonServiceLocator.ServiceLocator.Current.GetInstance<MainViewModel>();
            mainview.User = user;
            var home = new MainWindow();
            if((App.Current.Resources["Locator"] as ViewModelLocator).Main.IsTestProject)
            {
                home.mainframe.Navigate(new Uri($"View/ExcelDetectView.xaml", UriKind.Relative));
                home.ActiveButton.Visibility = Visibility.Collapsed;
            }
            home.Show();
            this.Close();
        }
    }
}
