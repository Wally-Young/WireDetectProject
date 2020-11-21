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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WiringHarnessDetect.ViewModel;

namespace WiringHarnessDetect.View
{
    /// <summary>
    /// UserManagerView.xaml 的交互逻辑
    /// </summary>
    public partial class UserManagerView : Page
    {
        public UserManagerView()
        {
            InitializeComponent();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            string tag = (sender as CheckBox).Tag.ToString();
            switch (tag)
            {
               case "user": (App.Current.Resources["Locator"] as ViewModelLocator).User.User.Access = Model.Authority.Maintainer; break;
               case "project": (App.Current.Resources["Locator"] as ViewModelLocator).User.User.Access = Model.Authority.Operator; break;
                case "paramter": (App.Current.Resources["Locator"] as ViewModelLocator).User.User.Access = Model.Authority.Admin; break;
                default:
                    break;
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {

        }
    }
}
