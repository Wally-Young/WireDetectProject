using Panuon.UI.Silver;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using WiringHarnessDetect.Common;
using WiringHarnessDetect.Model;
using WiringHarnessDetect.ViewModel;

namespace WiringHarnessDetect.View.SubView
{
    /// <summary>
    /// WireWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WireWindow : WindowX
    {
        public WireWindow(WindowOperation operation)
        {
            InitializeComponent();
            if (operation != WindowOperation.Add)
            {
                typeno.IsReadOnly = true;
                this.tempCarNo = (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Wire.WireNO;
            }

            switch (operation)
            {
                case WindowOperation.Add:
                    btnConfirm.Content = "添加";

                    break;
                case WindowOperation.Update:
                    btnConfirm.Content = "更新";
                    this.Title = "修改线束";
                    break;
                case WindowOperation.Delete:
                    btnConfirm.Content = "删除";
                    this.Title = "删除线束";
                    break;
                default:
                    break;
            }

            this.Operation = operation;
        }

        public WindowOperation Operation { get; set; } //操作类型

        private string tempCarNo = "";
       
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WireType car = (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Wire;
            if (Operation == WindowOperation.Add)
            {
                if (QualityCheck())
                {
                    car.Author = (App.Current.Resources["Locator"] as ViewModelLocator).Main.User.UserID;
                    int r = SQliteDbContext.AddWireType(car);
                    if (r > 0)
                    {
                        MessageBox.Show("添加成功!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        (App.Current.Resources["Locator"] as ViewModelLocator).Main.CarTypes = new System.Collections.ObjectModel.ObservableCollection<CarProject>(SQliteDbContext.GetAllCarTypes());
                         
                    }
                }

            }
            else if (Operation == WindowOperation.Update)
            {
                QualityCheck();
                int r = SQliteDbContext.UpdateWireType(car);
                if (r > 0)
                {
                    MessageBox.Show("更新成功!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    (App.Current.Resources["Locator"] as ViewModelLocator).Main.CarTypes = new System.Collections.ObjectModel.ObservableCollection<CarProject>(SQliteDbContext.GetAllCarTypes());
                    this.Close();
                }
            }
            else if (Operation == WindowOperation.Delete)
            {
                QualityCheck();
                int r = SQliteDbContext.DeleteWireType(car);
                if (r > 0)
                {
                    MessageBox.Show("删除成功!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    (App.Current.Resources["Locator"] as ViewModelLocator).Main.CarTypes = new System.Collections.ObjectModel.ObservableCollection<CarProject>(SQliteDbContext.GetAllCarTypes());
                    this.Close();
                }
            }
             (App.Current.Resources["Locator"] as ViewModelLocator).Main.WireTypes= new ObservableCollection<WireType>(SQliteDbContext.GetAllWireTypes());
        }

        private bool QualityCheck()
        {
            if (carname.Text.Trim().Length == 0)
            {
                MessageBox.Show("请输入线束名称", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                carname.Focus();
                return false;
            }
            if (typeno.Text.Trim().Length == 0)
            {
                MessageBox.Show("请输入线束编号", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                carname.Focus();
                return false;
            }
            if (Operation == WindowOperation.Add && SQliteDbContext.CheckWireNOExist(typeno.Text.Trim()))
            {
                MessageBox.Show("线束编号已经存在请重新输入", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                carname.Focus();
                return false;
            }
            if (Operation == WindowOperation.Update && tempCarNo.Trim() != typeno.Text.Trim())
            {
                if (SQliteDbContext.CheckWireNOExist(typeno.Text.Trim()))
                {
                    MessageBox.Show("线束编号已经存在请重新输入", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    carname.Focus();
                    return false;
                }
                else
                {
                    return true;
                }

            }

            return true;
        }

        private void WindowX_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (carname.Text.Trim().Length == 0 || typeno.Text.Trim().Length == 0)
                (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Wire = null;

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
