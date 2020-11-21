using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using WiringHarnessDetect.Model;
using MessageBox = System.Windows.MessageBox;

namespace WiringHarnessDetect.ViewModel
{
    public class ExcelManagerViewModel : ViewModelBase
    {
        public System.Timers.Timer timer = new System.Timers.Timer();//实例化Timer类，设置间隔时间为1000毫秒；
        string partype = "";

        #region Property
        //车型
        private CarProject car;
        public CarProject Car
        {
            get => car;
            set
            {
                car = value;
                RaisePropertyChanged("Car");
            }
        }
        //线束
        private WireType wire;
        public WireType Wire
        {
            get
            {
            //    if (wire == null)
            //        wire = new WireType();
                return wire;
            }
            set
            {
                wire = value;
                RaisePropertyChanged("Wire");
            }

        }
        //工程
        private ExcelProject project;
        public ExcelProject Project
        {
            get
            {
                
                return project;
            }
            set
            {
                project = value;
                RaisePropertyChanged("Project");
            }
            
        }
            
        //图纸路径
        private string imagePath;
        public string ImagePath
        {
            get => imagePath;
            set
            {
                imagePath = value;
                RaisePropertyChanged("ImagePath");
            }
        }

        private bool isPassive;
        public bool IsPassive //探针测试是否是无源线束
        {
            get => isPassive;
            set
            {
                isPassive = value;
                RaisePropertyChanged("IsPassive");
            }
        }
       
        //是否循环
        private bool isCycle;
        public bool IsCycle 
        {
            get => isCycle;
            set
            {
                isCycle = value;
                RaisePropertyChanged("IsCycle");
            }
        }
   
        //通道数据
        private ObservableCollection<int> channels;
        public ObservableCollection<int> Channels
        {
            get
            {
                if (channels == null)
                    channels = new ObservableCollection<int>();
                return channels;
            }
            set
            {
                channels = value;
                RaisePropertyChanged("Channels");
            }
        }

        //治具类型
        private ObservableCollection<FixtureBase> fixtures;
        public ObservableCollection<FixtureBase> Fixtures
        {
            get
            {
                if (fixtures == null)
                    fixtures = new ObservableCollection<FixtureBase>();
                return fixtures;
            }
            set
            {
                fixtures = value;
                RaisePropertyChanged("Fixtures");
            }
        }

        //选中的治具
        private FixtureBase fixture;
        public FixtureBase Fixture
        {
            get
            {
                if (fixture == null)
                    fixture = new FixtureBase() ;
                return fixture;
            }
            set
            {
                fixture = value;
                RaisePropertyChanged("FixtureLED");
            }
        }


        //连接基本关系
        private ObservableCollection<FixtureInfo> fixtureConnect;
        public ObservableCollection<FixtureInfo> FixtureConnect
        {
            get => fixtureConnect;
            set
            {
                fixtureConnect = value;
                RaisePropertyChanged("fixtureConnect");
            }
        }

        //治具引脚
        private ObservableCollection<ExcelPin> pins;
        public ObservableCollection<ExcelPin> Pins
        {
            get => pins;
            set
            {
                pins = value;
                RaisePropertyChanged("Pins");
            }
        }

        //引脚
        private ExcelPin pin;
        public ExcelPin Pin
        {
            get => pin;
            set
            {
                pin = value;
                RaisePropertyChanged("Pin");
            }
        }

        

        #endregion

        #region Command

        //窗体加载
        public RelayCommand OnLoad
        {

            get
            {
                return new RelayCommand(() =>
                {
                    Fixtures = new ObservableCollection<FixtureBase>(SQliteDbContext.GetAllFixtureBaseInfos());

                });
            }
        }

        //改变线束
        public RelayCommand<string> ChangeWire
        {
            get
            {
                return new RelayCommand<string>(
                    p =>
                    {
                        if (p.Contains("wire"))
                        {
                            if (Car == null)
                            {
                                MessageBox.Show("请选择一个车型!", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }

                        }
                        else if (p.Contains("car"))
                        {

                            return;
                        }
                        (App.Current.Resources["Locator"] as ViewModelLocator).Main.ExProjects =
                        new ObservableCollection<ExcelProject>(SQliteDbContext.GetAllWireExcelProject(Car.CarNO, Wire.WireNO));
                        Fixtures.Clear();
                    });



            }
        }
       

        //改变工程
        public RelayCommand<object> ChangeProject
        {
            get
            {
                return new RelayCommand<object>
                    (
              p =>
              
              {
                  if(p!=null)
                  {
                      ExcelProject excelProject = p as ExcelProject;
                      if(excelProject== null||excelProject.ProjectNO == null)
                      {
                          return;
                      }
                      //= (App.Current.Resources["Locator"] as ViewModelLocator).Main.Fixtures
                      var pinsinfos = SQliteDbContext.GetAFixturePinInfos(excelProject.ProjectNO);
                      var baseinfos = SQliteDbContext.GetAllFixtureBaseInfos().Where(x => pinsinfos.Exists(b => b.FixtureType == x.FixtureType)).ToList();
                      (App.Current.Resources["Locator"] as ViewModelLocator).Main.Fixtures = new ObservableCollection<FixtureInfo>(pinsinfos);
                      Fixtures = new ObservableCollection<FixtureBase>(baseinfos);
                  }
                 
                  
              }
              );
            }
        }

        //改变治具
        public RelayCommand<object> ChangeFixture
        {
            get
            {
                return new RelayCommand<object>(
              p =>
              {
                  try
                  {
                      if (p != null)
                      {
                          FixtureBase fixtureBase = p as FixtureBase;
                          if (fixtureBase.ImagePath == null || fixtureBase.ImagePath.Trim().Length == 0)
                          {
                              ImagePath = AppDomain.CurrentDomain.BaseDirectory + $@"Image\Error.png";
                              Messenger.Default.Send<List<ExcelPin>>(new List<ExcelPin>(), "clearEx");
                          }
                              
                          else
                          {
                              ImagePath = AppDomain.CurrentDomain.BaseDirectory + $@"Image\ExcelProject\{fixtureBase.ImagePath}";
                              Pins = new ObservableCollection<ExcelPin>(SQliteDbContext.GetOneFixtureExcelPins(fixture.FixtureType));
                              Messenger.Default.Send<List<ExcelPin>>(Pins.ToList(), "clearEx");
                          }
                      }
                     
                  }
                  catch (Exception ex)
                  {
                      ImagePath = AppDomain.CurrentDomain.BaseDirectory + $@"Image\Error.png";
                      Messenger.Default.Send<List<ExcelPin>>(new List<ExcelPin>(), "clearEx");
                      Console.WriteLine(ex.Message);
                  }
                 
              });
            }
        }

        //改变治具
        public RelayCommand<object> ChangePart
        {
            get
            {
                return new RelayCommand<object>(
              p =>
              {
                  try
                  {
                      if (p != null)
                      {
                          PartEx part = p as PartEx;
                          
                      }

                  }
                  catch (Exception ex)
                  {
                      ImagePath = AppDomain.CurrentDomain.BaseDirectory + $@"Image\Error.png";
                      Console.WriteLine(ex.Message);
                  }

              });
            }
        }
        //手动
        public RelayCommand Manual
        {
            get => new RelayCommand(new Action(() =>
            {
                IsCycle = false;
                timer.Stop();
                ProbeAction();
            }
            ));
        }

        //循环
        public RelayCommand<string> Cycle
        {
            get => new RelayCommand<string>(
                p => { IsCycle = !IsCycle; ProbeCycle(p); });
        }

        //导出Excel
        public RelayCommand  ExportExcel
        {
            get => new RelayCommand(() => Exportexcel());
        }
        #endregion

        #region Method
        private void ProbeAction()
        {
            if ((App.Current.Resources["Locator"] as ViewModelLocator).Main.IsConnected)
            {
                (App.Current.Resources["Locator"] as ViewModelLocator).Main.OperationName =  "PassiveProbe" ;
              
               
              byte[]  msg = new byte[] { 0xfe, 0xef, 0x30, 0x01, 0x00, 0x02, 0x00, 0x03 };//无源线束探针
                
                

                Messenger.Default.Send<byte[]>(msg, "Send");
            }
            else
            {
                System.Windows.MessageBox.Show("请先连接设备！", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }

        private void ProbeCycle(string s)
        {


            int interval = 0;
            int.TryParse(s, out interval);
            if (interval < 500)
            {
               System.Windows. MessageBox.Show("请输入正确的时间间隔，间隔必须大于500","警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                IsCycle = false;
                return;
            }

            if (IsCycle)
            {
              
                timer.Elapsed += Timer_Tick;
                timer.Interval = interval;
                timer.Start();
                
            }
            else
            {
                timer.Stop();
            }





        }


        private void Timer_Tick(object sender, EventArgs e)
        {
            ProbeAction();
            timer.Stop();
        }


        private void Exportexcel()
        {
            string filePathName = "";//定义图像文件的位置（包括路径及文件名）
            System.Windows.Forms.SaveFileDialog ofd = new System.Windows.Forms.SaveFileDialog(); //添加打开对话框
            ofd.Filter = "Excel文件|*.xls;*.xlsx";  //设置过滤器
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)//如果确定打开图片，则保存文件的路径及文件名到字符串变量filePathName中
            {
                try
                {
                    filePathName = ofd.FileName;  //包括路径和文件名       
                    var workBook = new HSSFWorkbook();// filepath =
                    DataTable dataTable = SQliteDbContext.GetAllFBaseInfos();
                    List<String> titles = new List<string>() { "治具型号", "LED地址", "图片名称" };
                    CreateSheet(workBook, "Sheet1", titles, dataTable);
                    DataTable pintable = SQliteDbContext.GetAllPinsInfo();
                    
                    List<String> pins = new List<string>() { "治具型号", "引脚编号", "引脚编码(物理地址)" };
                    CreateSheet(workBook, "Sheet2", pins, pintable);
                    
                    using (FileStream fs = new FileStream(filePathName + ".xls", FileMode.Create))
                    {
                        workBook.Write(fs);
                    }
                    MessageBox.Show($"导出成功!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导出失败!原因:{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;

                }
               
            }
           

        }

        private void CreateSheet(HSSFWorkbook workBook,string sheetName,List<string>titles,DataTable table) 
        {
            
            #region 设置字体
            IFont font = workBook.CreateFont();
            font.FontHeightInPoints = 11;
            font.FontName = "宋体";
           
            font.Boldweight = (short)FontBoldWeight.Bold;
            font.Color = HSSFColor.Black.Index;
            #endregion
            HSSFPalette palette = workBook.GetCustomPalette(); //调色板实例
            palette.SetColorAtIndex((short)8, (byte)91, (byte)155, (byte)213);//设置表头背景色
            palette.SetColorAtIndex((short)16, (byte)221, (byte)235, (byte)247);//设置内容背景色
            palette.SetColorAtIndex((short)32, (byte)155, (byte)194, (byte)230);//设置下边框线颜色
            palette.SetColorAtIndex((short)48, (byte)212, (byte)212, (byte)212);//设置下边框线颜色
            palette.SetColorAtIndex((short)64, (byte)255, (byte)255, (byte)0);//设置黄色      

            var cellMidStyle = workBook.CreateCellStyle();
            cellMidStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;//设置水平居中
            cellMidStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;//设置垂直居中
            cellMidStyle.FillPattern = FillPattern.SolidForeground;
            cellMidStyle.FillForegroundColor = palette.FindColor((byte)221, (byte)235, (byte)247).Indexed;
            cellMidStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            cellMidStyle.BottomBorderColor = palette.FindColor((byte)155, (byte)194, (byte)230).Indexed;
            cellMidStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            cellMidStyle.LeftBorderColor = palette.FindColor((byte)212, (byte)212, (byte)212).Indexed;
            cellMidStyle.SetFont(font);
            var sheet = workBook.CreateSheet(sheetName);//创建表格
            #region 创建列头
            // 第一行存放列名
            var row = sheet.CreateRow(0);
         
            for (int i = 0; i < titles.Count; i++)
            {
                var cell = row.CreateCell(i);
                cell.SetCellValue(titles[i]);
                int length = Encoding.UTF8.GetBytes(titles[i]).Length;
                sheet.SetColumnWidth(i, length * 256);//设置表格列宽
                cell.CellStyle = cellMidStyle;//设置单元格样式
            }
            int rowIndex = 1;
            foreach (DataRow myRow in table.Rows)
            {
                row = sheet.CreateRow(rowIndex);
                for (int i = 0; i < titles.Count; i++)
                {
                    row.CreateCell(i).SetCellValue(myRow[i].ToString());
                }
                rowIndex++;
            }
            
            #endregion
        }
        #endregion
    }
}
