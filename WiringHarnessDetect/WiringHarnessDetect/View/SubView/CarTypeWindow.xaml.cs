using Panuon.UI.Silver;
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
using WiringHarnessDetect.Common;
using WiringHarnessDetect.Model;
using WiringHarnessDetect.ViewModel;

namespace WiringHarnessDetect.View.SubView
{
    /// <summary>
    /// CarTypeWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CarTypeWindow : WindowX
    {
        public WindowOperation Operation { get; set; } //操作类型

        private string tempCarNo = "";
        public CarTypeWindow(WindowOperation operation)
        {
            InitializeComponent();
            if(operation!=WindowOperation.Add)
            {
                typeno.IsReadOnly = true;
                this.tempCarNo = (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Car.CarNO;
            }
           
            switch (operation)
            {
                case WindowOperation.Add:
                    btnConfirm.Content = "添加";
                    
                    break;
                case WindowOperation.Update:
                    btnConfirm.Content = "更新";
                    this.Title = "修改车型";
                    break;
                case WindowOperation.Delete:
                    btnConfirm.Content = "删除";
                    this.Title = "删除车型";
                    break;
                default:
                    break;
            }
           
            this.Operation = operation;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string tag = (sender as Button).Tag.ToString();
            if(tag=="confirm")
            {
                CarProject car = (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Car;
                if (Operation == WindowOperation.Add)
                {
                    if (QualityCheck())
                    {
                        car.Author = (App.Current.Resources["Locator"] as ViewModelLocator).Main.User.UserID;
                        int r = SQliteDbContext.AddCarType(car);
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
                    int r = SQliteDbContext.UpdateCarType(car);
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
                    int r = SQliteDbContext.DeleteCarType(car);
                    if (r > 0)
                    {
                        MessageBox.Show("删除成功!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        (App.Current.Resources["Locator"] as ViewModelLocator).Main.CarTypes = new System.Collections.ObjectModel.ObservableCollection<CarProject>(SQliteDbContext.GetAllCarTypes());
                        this.Close();
                    }
                }
            }
            else if(tag=="cancel")
            {
                this.Close();
            }
        }   

        private bool  QualityCheck()
        {
            if (carname.Text.Trim().Length == 0)
            {
                MessageBox.Show("请输入车型名称", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                carname.Focus();
                return false;
            }
            if (typeno.Text.Trim().Length == 0)
            {
                MessageBox.Show("请输入车型编号", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                carname.Focus();
                return false;
            }
            if (Operation==WindowOperation.Add&& SQliteDbContext.CheckCarNOExist(typeno.Text.Trim()))
            {
                MessageBox.Show("车型编号已经存在请重新输入", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                carname.Focus();
                return false;
            }
            if (Operation == WindowOperation.Update &&tempCarNo.Trim()!=typeno.Text.Trim())
            {
                if(SQliteDbContext.CheckCarNOExist(typeno.Text.Trim()))
                {
                    MessageBox.Show("车型编号已经存在请重新输入", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Car = null;

        }
    }
}
