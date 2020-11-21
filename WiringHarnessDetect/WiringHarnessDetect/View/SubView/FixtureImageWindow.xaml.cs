using Panuon.UI.Silver;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WiringHarnessDetect.Common;
using WiringHarnessDetect.Model;
using WiringHarnessDetect.ViewModel;
using MessageBox = System.Windows.MessageBox;

namespace WiringHarnessDetect.View.SubView
{
    /// <summary>
    /// PartImageWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ExFixtureWindow : WindowX
    {
        private string filepath="";
        string templed = "";
        public WindowOperation Operation { get; set; } //操作类型

        public ExFixtureWindow(WindowOperation operation)
        {
            InitializeComponent();

            if (operation != WindowOperation.Add)
            {

                txtFixture.IsReadOnly = true;
                txtFixture.Visibility = Visibility.Visible;
                txtFixture.IsReadOnly = true;
                cmbFixtures.Visibility = Visibility.Collapsed;
                isInput.Visibility = Visibility.Collapsed;
            }
            else
            {
                txtFixture.Visibility = Visibility.Collapsed;
            }

            switch (operation)
            {
                case WindowOperation.Add:
                    btnConfirm.Content = "添加";
                    break;
                case WindowOperation.Update:
                    btnConfirm.Content = "更新";
                    this.Title = "修改工程";
                    this.templed = ledAddr.Text;
                    break;
                case WindowOperation.Delete:
                    btnConfirm.Content = "删除";
                    this.Title = "删除工程";
                    break;
                default:
                    break;
            }

            this.Operation = operation;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string tag = (sender as System.Windows.Controls.Button).Tag.ToString().ToLower();
            if (tag == "scan")
            {
                string filePathName = "";//定义图像文件的位置（包括路径及文件名）
                System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog(); //添加打开对话框
                ofd.Filter = "图像文件|*jpg;*.bmp;*.png,*.tif|所有文件|*.*";  //设置过滤器
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)//如果确定打开图片，则保存文件的路径及文件名到字符串变量filePathName中
                {
                    filePathName = ofd.FileName;  //包括路径和文件名
                                                  // filepath =
                    filepath = ofd.FileName;
                    imagetxt.Text = System.IO.Path.GetFileName(ofd.FileName);
                }
            }
            else if (tag == "confirm")
            {
                Add();
            }
            else if(tag=="cancel")
            {
                this.Close();
            }
            
            
        }

        private void Add()
        {
            FixtureBase fixture = (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Fixture;
            switch (Operation)
            {
                case WindowOperation.Add:
                    if (QualityCheck())
                    {

                        try
                        {
                            Bitmap bitmap = new Bitmap(filepath);
                            if (folderExist(System.Environment.CurrentDirectory + "\\Image\\" + "ExcelProject"))
                                filepath = System.Environment.CurrentDirectory + "\\Image\\" + "ExcelProject" + "\\" + System.IO.Path.GetFileName(filepath);
                            else
                            {
                                folderCreate(System.Environment.CurrentDirectory + "\\Image\\" + "ExcelProject");
                                filepath = System.Environment.CurrentDirectory + "\\Image\\" + "ExcelProject" + "\\" + System.IO.Path.GetFileName(filepath);
                            }
                            bitmap.Save(filepath);
                            bitmap.Dispose();
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show("工程中已经存在同名的图片，请将图片重命名后再导入", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        int row = SQliteDbContext.AddFixtureBaseInfo(fixture);
                        if (row > 0)
                        {
                            System.Windows.MessageBox.Show("添加成功!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            this.Close();
                        }
                    }

                    break;
                case WindowOperation.Update:
                    if (QualityCheck())
                    {
                        int rs = SQliteDbContext.UpdateFixtureBaseInfo(fixture);
                        if (rs > 0)
                        {
                            System.Windows.MessageBox.Show("更新成功!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            this.Close();
                        }
                    }
                    break;
                case WindowOperation.Delete:
                    int rd = SQliteDbContext.DeleteFixtureBaseInfo(fixture);
                    if (rd > 0)
                    {
                        System.Windows.MessageBox.Show("删除成功!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.Close();
                    }
                    break;
                default:
                    break;
            }
                (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Fixtures = new System.Collections.ObjectModel.ObservableCollection<FixtureBase>(SQliteDbContext.GetAllFixtureBaseInfos());
        }

        public bool QualityCheck()
        {

            if ( cmbFixtures.SelectedIndex == -1 && txtFixture.Text.Trim().Length == 0)
            {
                MessageBox.Show("请输入一个治具型号!", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (Operation == WindowOperation.Add && SQliteDbContext.CheckFixturetype((App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Fixture.FixtureType))
            {
                MessageBox.Show("治具型号不能重复!", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (imagetxt.Text.Trim().Length == 0)
            {
                MessageBox.Show("请导入图片!", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (this.ledAddr.Text.Trim().Length == 0)
            {
                MessageBox.Show("请输入LED地址!", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                ledAddr.Focus();
                return false; 
            }
            if (!IsNumber(this.ledAddr.Text.Trim()))
            {
                MessageBox.Show("LED地址只能为整数!", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                ledAddr.Focus();
                return false;
            }
            if (Operation == WindowOperation.Add && SQliteDbContext. CheckLEDAddress(ledAddr.Text.Trim()))
            {
                MessageBox.Show("LED地址不能重复!", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                ledAddr.Focus();
                return false;
            }
            if(Operation == WindowOperation.Update&&templed!=ledAddr.Text)
            {
                if(SQliteDbContext.CheckLEDAddress(ledAddr.Text.Trim()))
                {
                    MessageBox.Show("LED地址不能重复!", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    ledAddr.Focus();
                    return false;
                }
            }
            return true;

        }
        /// <summary>
        /// 判断文件夹是否存在
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns></returns>
        public static bool folderExist(string path)
        {
            bool response = false;

            if (Directory.Exists(path))
            {
                response = true;
            }
            return response;
        }

        /// <summary>
        /// 创建文件夹
        /// </summary>
        /// <param name="path">路径</param>
        public static void folderCreate(string path)
        {
            Directory.CreateDirectory(path);
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


        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // this.prjName.Text = (this.projcmb.SelectedItem as ExcelProject).ProjectName;
            //projetNO = (this.projcmb.SelectedItem as ExcelProject).ProjectNO;
            // this.cmbpart.ItemsSource = SQliteDbContext.GetAllFixtureEx((this.projcmb.SelectedItem as ExcelProject).ProjectNO).Select(x=>x.FixtureName).Distinct().ToList();

            //(App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.FixtureLED = (this.cmbpart.SelectedItem as FixtureLED);
            //(App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.ImagePath = (this.cmbpart.SelectedItem as FixtureLED).ImagePath;

        }

        private void IsInput_Checked(object sender, RoutedEventArgs e)
        {
           
                cmbFixtures.Visibility = Visibility.Collapsed;
                txtFixture.Visibility = Visibility.Visible;
            
            
        }

        private void IsInput_Unchecked(object sender, RoutedEventArgs e)
        {
            
            
                cmbFixtures.Visibility = Visibility.Visible;
                txtFixture.Visibility = Visibility.Collapsed;
            
        }

        private void WindowX_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (imagetxt.Text.Trim().Length == 0 || txtFixture.Text.Trim().Length == 0)
                (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Fixture = null;

        }

        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
             if(e.Key==Key.Enter)
            {
                Add();
            }
        }
    }
}
