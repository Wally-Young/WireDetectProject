
using Panuon.UI.Silver;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
    /// PaperFrm.xaml 的交互逻辑
    /// </summary>
    public partial class PaperWindow : WindowX
    {
        private string projectcode;
        private bool IsModify = false;
        private CablePaper cablePaper;
        public PaperWindow(string projectCode)
        {
            InitializeComponent();
            this.projectcode = projectCode;
            this.cablePaper = new CablePaper();
            this.DataContext = cablePaper;
        }
        public PaperWindow(CablePaper cablePaper)
        {
            InitializeComponent();
            this.projectcode = cablePaper.ProjectNO;
            this.cablePaper = cablePaper;
            this.txtfixcode.IsEnabled = false;
            this.DataContext = cablePaper;

        }

        /// <summary>
        /// 浏览图片按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string tag = (sender as Button).Tag.ToString().ToLower();
            if(tag=="scan")
            {
                string filePathName = "";//定义图像文件的位置（包括路径及文件名）
                System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog(); //添加打开对话框
                ofd.Filter = "图像文件|*jpg;*.bmp;*.png,*.tif|所有文件|*.*";  //设置过滤器
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)//如果确定打开图片，则保存文件的路径及文件名到字符串变量filePathName中
                {
                    filePathName = ofd.FileName;  //包括路径和文件名
                }

                try
                {
                    Bitmap bitmap = new Bitmap(filePathName);
                    string filepath = "";
                    if (folderExist(System.Environment.CurrentDirectory + "\\Image\\" + projectcode))
                        filepath = System.Environment.CurrentDirectory + "\\Image\\" + projectcode + "\\" + System.IO.Path.GetFileName(ofd.FileName);
                    else
                    {
                        folderCreate(System.Environment.CurrentDirectory + "\\Image\\" + projectcode);
                        filepath = System.Environment.CurrentDirectory + "\\Image\\" + projectcode + "\\" + System.IO.Path.GetFileName(ofd.FileName);
                    }

                    bitmap.Save(filepath);
                    bitmap.Dispose();
                    cablePaper.ImagePath = filepath;
                }
                catch (Exception ex)
                {

                    System.Windows.MessageBox.Show("工程中已经存在同名的图片，请将图片重命名后再导入", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else if(tag=="confirm")
            {              
               if(CheckPaper())
                {
                    (App.Current.Resources["Locator"] as ViewModelLocator).Main.Papers.Add(cablePaper);
                    SQliteDbContext.AddPaper(cablePaper);
                    string msg = IsModify ? "修改成功!" : "添加成功!";
                    MessageBox.Show(msg, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }                  
            }
            else if(tag=="cancel")
            {
                if (!IsModify)
                {
                    foreach (var item in grid.Children)
                    {
                        if (item is TextBox)
                        {
                            (item as TextBox).Text = "";
                        }
                    }
                }
            }

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

        private bool CheckPaper()
        {
            if (this.cablePaper.FixtureCode.Trim().Length == 0)
            {
                MessageBox.Show("治具编号不能为空!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                
                return false;
            }
            
            if (cablePaper.FixtureName.Trim().Length == 0)
            {
                MessageBox.Show("治具名称不能为空!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                
                return false;
            }
            if (cablePaper.LEDAddress > 0)
            {
                MessageBox.Show("治具地址不能为空!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                
                return false;
            }
            if(cablePaper.ImagePath.Trim().Length==0)
            {
                MessageBox.Show("治具图纸不能为空!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                return false;
            }
            if(SQliteDbContext.GetPaper(cablePaper)!=null)
            {
                MessageBox.Show("治具编号不能重复!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                return false;
            }

            
            


            return true;
        }
    }
}
