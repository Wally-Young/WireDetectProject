using Panuon.UI.Silver;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace WiringHarnessDetect.View.SubView
{
    /// <summary>
    /// ExcelPinWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ExcelPinWindow : WindowX
    {

        string tempaddress = "";
        public ExcelPinWindow(bool ismodify)
        {
            InitializeComponent();
            if(ismodify)
            {
                codepart.IsReadOnly = true;
                tempaddress = address.Text;
            }
            else
            {

            }
            this.IsModify = ismodify;

        }

        public bool IsAdd { get; set; }
        public bool IsModify { get; internal set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!IsModify)
            {
                ExcelPin p = (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Pin;
                p.PinNO = this.codepart.Text;
                if (QualityCheck())
                {
                    int rs = SQliteDbContext.AddExPin(p);
                    if (rs > 0)
                    {
                        MessageBox.Show("添加成功", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Pins.Add(p);
                        IsAdd = true;
                        this.Close();
                    }
                }


            }
            else
            {
                ExcelPin p = (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Pin;
                p.PinNO = this.codepart.Text;
                if(QualityCheck())
                {
                    int rs = SQliteDbContext.UpdatOneExPin(p);
                    if (rs > 0)
                    {
                        MessageBox.Show("修改成功!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.Close();
                    }
                }
               
            }
            (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Pins =
                new ObservableCollection<ExcelPin>(SQliteDbContext.
                GetOneFixtureExcelPins((App.Current.Resources["Locator"] as ViewModelLocator).
                ExcelPaper.Fixture.FixtureType));
            
        }

        private bool QualityCheck()
        {
            if (codepart.Text.Trim().Length == 0)
            {
                MessageBox.Show("请输入引脚编号", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                codepart.Focus();
                return false;
            }
            if (address.Text.Trim().Length == 0)
            {
                MessageBox.Show("请输入物理地址", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                address.Focus();
                return false;
            }
            if (!IsModify && SQliteDbContext.CheckPinNOExist(cmbfixture.Text.Trim(), codepart.Text.Trim()))
            {
                MessageBox.Show("引脚编号已经存在请重新输入", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                codepart.Focus();
                return false;
            }
            if(!IsModify&&SQliteDbContext.CheckPinAddressExist(address.Text.Trim()))
            {

                MessageBox.Show("物理地址已经存在请重新输入", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                address.Focus();
                return false;
            }
            if (IsModify && address.Text.Trim() != tempaddress)
            {
                if (SQliteDbContext.CheckPinAddressExist(address.Text.Trim()))
                {
                    MessageBox.Show("物理地址已经存在请重新输入", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    address.Focus();
                    return false;
                }
                else
                {
                    return true;
                }

            }

            return true;
        }
        /// <summary>
        /// 判断字符串是否是数字
        /// </summary>
        public static bool IsNumber(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            const string pattern = "^[0-9]*$";
            Regex rx = new Regex(pattern);
            return rx.IsMatch(s);
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key==Key.Enter)
            {
                Button_Click(null, null);
            }
        }
    }
}
