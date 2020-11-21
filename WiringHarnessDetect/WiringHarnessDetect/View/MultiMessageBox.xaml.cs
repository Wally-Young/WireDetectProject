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

namespace WiringHarnessDetect
{
    /// <summary>
    /// MultiMessageBox.xaml 的交互逻辑
    /// </summary>
    public partial class MultiMessageBox : Window
    {

        public bool DiaResult;
        public MultiMessageBox()
        {
            InitializeComponent();
        }
        public MultiMessageBox(string title, string message, MBoxType mBoxType = MBoxType.Confirm)
        {
            InitializeComponent();
            // 准备Binding
            Binding binding = new Binding()
            {
                Source = title,
                //Path = new PropertyPath("Name"),// 需绑定的数据源属性名  
                Mode = BindingMode.OneWay,// 绑定模式
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            // 连接数据源与绑定目标
            BindingOperations.SetBinding(
                titlelb,// 需绑定的控件  ,

                Label.ContentProperty,// 需绑定的控件属性
                binding);

            Binding binding1 = new Binding()
            {
                Source = message,
                Mode = BindingMode.OneWay,// 绑定模式
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };

            BindingOperations.SetBinding(
                tipMessage,
                Label.ContentProperty,// 需绑定的控件属性
                binding1);

            switch (mBoxType)
            {
                case MBoxType.Confirm:
                    this.tipico.Source = new BitmapImage(new Uri("/Image/question.png", UriKind.Relative));
                    this.CanCel.Visibility = Visibility.Visible;
                    this.Confirm.Visibility = Visibility.Visible;
                    this.Confirm.Margin = new Thickness(0, 0, 10, 20);
                    break;
                case MBoxType.Warning:
                    this.tipico.Source = new BitmapImage(new Uri("/Image/warning.png", UriKind.Relative));
                    this.CanCel.Visibility = Visibility.Collapsed;
                    this.Confirm.Margin = new Thickness(30, 0, 0, 20);

                    break;
                case MBoxType.Info:
                    this.tipico.Source = new BitmapImage(new Uri("/Image/info.png", UriKind.Relative));
                    this.CanCel.Visibility = Visibility.Collapsed;
                    this.Confirm.Margin = new Thickness(30, 0, 0, 20);

                    break;
                default:
                    break;
            }
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            this.DiaResult = true;
            this.Close();
        }

        private void CanCel_Click(object sender, RoutedEventArgs e)
        {
            this.DiaResult = false;
            this.Close();
        }

        private void FrmCloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();

        }


    }

    public enum MBoxType
    {
        Confirm = 1,
        Warning = 2,
        Info = 3,

    }
}
