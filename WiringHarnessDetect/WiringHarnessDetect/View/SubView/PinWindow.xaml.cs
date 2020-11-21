using GalaSoft.MvvmLight.Messaging;
using Panuon.UI.Silver;
using System;
using System.Collections.Generic;
using System.Globalization;
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
using WiringHarnessDetect.Model;
using WiringHarnessDetect.ViewModel;

namespace WiringHarnessDetect.View
{
    /// <summary>
    /// CodeWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CodeWindow : WindowX
    {
        
        private bool isRelayPaper;
        private Pin pin;

        public bool IsModify;
        public CodeWindow(bool isRalay)
        {
            InitializeComponent();
            isRelayPaper = isRalay;
            this.type.Visibility = isRalay ? Visibility.Visible : Visibility.Collapsed;
            
        }

       

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

            phy.Visibility = Visibility.Visible;
            contr.Visibility = Visibility.Visible;
            this.Height = this.Height + 55;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            phy.Visibility = Visibility.Collapsed;
            contr.Visibility = Visibility.Collapsed;
            this.Height = this.Height - 55;
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            if (CheckPin())
            {
                if (IsModify)
                    SQliteDbContext.UpdatOnePin(pin);
                else
                    SQliteDbContext.AddPin(pin);
                string msg = IsModify ? "修改成功!" : "添加成功!";
                MessageBox.Show(msg, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                Messenger.Default.Send<string>("update", "UpdateCanavs");
                this.Close();
            }

        }

        private bool CheckPin()
        {
            this.pin = (App.Current.Resources["Locator"] as ViewModelLocator).Paper.Pin;
            if (pin.PinCode.Trim().Length == 0)
            {
                MessageBox.Show("引脚编号不能为空!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                return false;
            }
            if (pin.physicalChannel <= 0)
            {
                MessageBox.Show("物理地址必须为大于零的整数!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                return false;
            }
            if(pin.IsSafeBox&&pin.SafeBoxName.Trim().Length==0)
            {
                MessageBox.Show("元件名称不能为空!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (pin.IsSafeBox && !(pin.SafeBoxName.Trim().Contains("F")|| pin.SafeBoxName.Trim().Contains("K")))
            {
                MessageBox.Show("请输入正确的元件名称（继电器—K 保险芯—F）!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if(pin.IsSafeBox )
            {
                if(!IsModify&& SQliteDbContext.CheckSafeBoxNameSingle(pin)>0)
                {
                    MessageBox.Show("元器件名称不能重复", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
                else if(IsModify )
                {
                    if(SQliteDbContext.GetOnePin(pin.PinCode,pin.ProjectNO).safeBoxName!=pin.safeBoxName&& SQliteDbContext.CheckSafeBoxNameSingle(pin) > 0)
                    {
                        MessageBox.Show("元器件名称不能重复", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                   
                }
             
                   
            }

            if (SQliteDbContext.CheckCodeSingle(pin)&&!IsModify)
            {
                MessageBox.Show("引脚编号不重复!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                return false;
            }
            if (SQliteDbContext.CheckAddressSingle(pin)>=1)
            {
                MessageBox.Show("物理地址已经被占用!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                return false;
            }
            if (SQliteDbContext.CheckAddressSingle(pin) > 1&& SQliteDbContext.GetOnePin(pin.PinCode, pin.ProjectNO).physicalChannel!=pin.physicalChannel&&IsModify)
            {
                MessageBox.Show("物理地址已经被占用!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                return false;
            }
            return true;
        }
    }

    public class BoolConverterToInt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return 0;
            }


            return ((bool)value) ? 1 : 0;
        }

       

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return false;
            }
            int val = System.Convert.ToInt32(value);
            return val > 0;
            
        }

       
    }

}
