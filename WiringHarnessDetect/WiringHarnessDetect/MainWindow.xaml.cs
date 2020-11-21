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
using Panuon.UI;
using Panuon.UI.Silver;
using WiringHarnessDetect.Model;
using WiringHarnessDetect.ViewModel;

namespace WiringHarnessDetect
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : WindowX
    {

        public MainWindow()
        {
            InitializeComponent();
            this.grid.MouseLeftButtonDown += (o, e) => { DragMove(); };
        }

       
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string tag = button.Tag.ToString().ToLower();
            if(tag=="min")
            {
                this.WindowState = WindowState.Minimized;
            }
            else if(tag=="max")
            {
                this.WindowState = this.WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
            }
            else if(tag=="close"||tag=="exit")
            {
                MessageBoxResult dialogResult = MessageBox.Show("您确定要退出系统吗?", "问询", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (dialogResult == MessageBoxResult.OK)
                {
                    App.Current.Shutdown();
                }
            }
            else
            {
                if ((App.Current.Resources["Locator"] as ViewModelLocator).Main.IsTestProject)
                {
                    if (tag == "papermanager")
                        tag = "ExcelManager";
                    else if (tag == "passive")
                        tag = "ExcelDetect";
                }
                
                if((App.Current.Resources["Locator"] as ViewModelLocator).Main.IsConnected)
                    (App.Current.Resources["Locator"] as ViewModelLocator).Main.Connect.Execute(null);
                    this.mainframe.Navigate(new Uri($"View/{tag}View.xaml", UriKind.Relative));
            }
        }
    }
}
