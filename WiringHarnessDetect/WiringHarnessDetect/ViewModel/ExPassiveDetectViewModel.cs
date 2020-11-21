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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using WiringHarnessDetect.Common;
using WiringHarnessDetect.Model;

namespace WiringHarnessDetect.ViewModel
{
    public class ExPassiveDetectViewModel: ViewModelBase
    {
       public System.Timers.Timer timer = new System.Timers.Timer();


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
            get => wire;
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
            get => project;

            set
            {
                project = value;
                RaisePropertyChanged("Project");
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
                    fixture = new FixtureBase();
                return fixture;
            }
            set
            {
                fixture = value;
                RaisePropertyChanged("FixtureLED");
            }
        }

        //选中的零件
        public PartEx part;
        public PartEx Part
        {
            get => part;
            set
            {
                part = value;
                RaisePropertyChanged("Part");
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

        //图片路径
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


        //右边图片路径
        private string imagePathr;
        public string ImagePathr
        {
            get => imagePathr;
            set
            {
                imagePathr = value;
                RaisePropertyChanged("ImagePathr");
            }
        }
        private bool isLED;
        public bool IsLED //是否启用LED
        {
            get => isLED;
            set
            {
                isLED = value;
                RaisePropertyChanged("IsLED");
            }
        }


        private ObservableCollection<PartEx> partTypes;
        public ObservableCollection<PartEx> PartTypes
        {
            get
            {
                if (partTypes == null)
                    partTypes = new ObservableCollection<PartEx>();
                return partTypes;
            }
            set
            {
                partTypes = value;
                RaisePropertyChanged("PartTypes");
            }
        }

        #region 检测数据

        private ObservableCollection<string> learningData;
        public ObservableCollection<string> LearningData
        {
            get
            {
                if (learningData == null)
                    learningData = new ObservableCollection<string>();
                return learningData;
            }
            set
            {
                learningData = value;
                RaisePropertyChanged("LearningData");
            }
        }

        private ObservableCollection<string> shortData;
        public ObservableCollection<string> ShortData
        {
            get
            {
                if (shortData == null)
                    shortData = new ObservableCollection<string>();
                return shortData;
            }
            set
            {
                shortData = value;
                if (shortData.Count > 0)
                {
                    Dictionary<string, List<ExcelPin>> excelPins = UpdatePicture(shortData[0]);
                    Messenger.Default.Send<Dictionary<string, List<ExcelPin>>>(excelPins, "UpdateExPCanvas");
                }
                RaisePropertyChanged("ShortData");
            }
        }


        private ObservableCollection<string> cutData;
        public ObservableCollection<string> CutData
        {
            get
            {
                if (cutData == null)
                    cutData = new ObservableCollection<string>();
                return cutData;
            }
            set
            {
                cutData = value;
                if(cutData.Count>0)
                {
                    Dictionary<string,List<ExcelPin>> excelPins=  UpdatePicture(cutData[0]);
                    Messenger.Default.Send<Dictionary<string, List<ExcelPin>>>(excelPins, "UpdateExPCanvas");
                }
                RaisePropertyChanged("CutData");
            }
        }
        #endregion 

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


       

        private ObservableCollection<ConnectionInfo> connects;
        public  ObservableCollection<ConnectionInfo> Connects
        {
            get => connects;
            set
            {
                connects = value;
                RaisePropertyChanged("Connects");
            }
        }

        #endregion

        #region  Command

        //改变线束
        public RelayCommand<string> ChangeWire
        {
            get
            {
                return new RelayCommand<string>(
                    p =>
                    {   
                        if(p.Contains("wire"))
                        {
                            if (Car == null)
                            {
                                MessageBox.Show("请选择一个车型!", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }
                           
                        }
                        else if(p.Contains("car"))
                        {
                           
                                return;
                        }
                        (App.Current.Resources["Locator"] as ViewModelLocator).Main.ExProjects =
                        new ObservableCollection<ExcelProject>(SQliteDbContext.GetAllWireExcelProject(Car.CarNO, Wire.WireNO));
                    });


            }
        }

        //改变工程
        public RelayCommand<object> ChangeProject
        {
            get {
                return new RelayCommand<object>(p =>
                {

                    ExcelProject excelProject = p as ExcelProject;

                    if (excelProject != null)
                    {
                        
                        if (excelProject.ProjectNO == null || excelProject == null)
                        {
                            return;
                        }
                        Project = excelProject;
                       
                        //1/找出此工程下的所有治具型号
                        var baseinfos = SQliteDbContext.GetAllFixtureBaseInfos(project.ProjectNO);

                        //2 获取这个工程下的型号下的回路号
                        var parts = SQliteDbContext.GetAllParts(Project.ProjectNO).GroupBy(r => r.PartType).Select(r => new PartEx()
                        {
                            ProjectNO = r.First().ProjectNO,
                            PartType = r.Key,
                            Circuits = r.Select(x => x.Circuit).ToList(),
                        }).ToList();

                        
                        Connects = new ObservableCollection<ConnectionInfo>(SQliteDbContext.GetExConnectionInfo(Project.ProjectNO));
                        PartTypes = new ObservableCollection<PartEx>(parts);
                        Fixtures = new ObservableCollection<FixtureBase>(baseinfos);
                    }

                });
               
            }
        }

        //3/改变零件图号选择
        public RelayCommand<object> ChangePart
        {

            get
            {
                return new RelayCommand<object>(
              p =>
              {
                  if (p == null && p.ToString().Trim().Length == 0)
                      return;

                  PartEx part = p as PartEx;
                  if(Part!=null)
                  {
                      try
                      {
                          //4从当前工程中选择出来所有的当前选中零件号回路信息中含有的连接关系
                          Connects = new ObservableCollection<ConnectionInfo>(SQliteDbContext.GetExConnectionInfo(Project.ProjectNO));
                          var rs = Connects.Where(x => part.Circuits.ToList().Contains(x.CirCuitName)).ToList();
                          Connects = new ObservableCollection<ConnectionInfo>(rs);
                      }
                      catch (Exception ex)
                      {
                          MessageBox.Show($"无法筛选出当前零件号的信息,请重新现在工程和线束号，或者查看数据导入.具体原因为:{ex.Message}!", "错误", MessageBoxButton.OK, MessageBoxImage.Error );
                      }
                      
                  }

                 
              });
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
                          FixtureBase fixture = p as FixtureBase;
                          FixtureConnect = new ObservableCollection<FixtureInfo>(SQliteDbContext.GetAFixturePinInfos(Project.ProjectNO).Where(x => x.FixtureType == fixture.FixtureType).ToList());
                          FixtureBase fixtureBase = Fixtures.Where(x => x.FixtureType == fixture.FixtureType).First();
                          if (fixtureBase.ImagePath == null || fixtureBase.ImagePath.Trim().Length == 0)
                              ImagePath = AppDomain.CurrentDomain.BaseDirectory + $@"Image\Error.png";
                          else
                          {
                              ImagePath = AppDomain.CurrentDomain.BaseDirectory + $@"Image\ExcelProject\{fixtureBase.ImagePath}";
                              Pins = new ObservableCollection<ExcelPin>(SQliteDbContext.GetOneFixtureExcelPins(fixture.FixtureType));
                              
                          }
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

        public RelayCommand BeginDetect //开始检测
        {
            get => new RelayCommand(Detect);
        }

        public RelayCommand CycleDetect//循环检测
        {
            get => new RelayCommand(CyCle);
        }
        public RelayCommand OpenLED//打开治具LED
        {
            get => new RelayCommand(OpenLed);
        }

        public RelayCommand CloseLED//关闭LED
        {
            get => new RelayCommand(CloseLed);
        }
        public RelayCommand Save//保存数据
        {
            get => new RelayCommand(SaveData);
        }

        public RelayCommand Export
        {
            get => new RelayCommand(() =>
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

                          //List<String> titles = new List<string>() { "线束零件号","线束号", "线束名称", "测试时间", "治具型号","物理地址","治具坐标", "治具引脚编号","虚拟编号","虚拟引脚序号" };
                          //CreateSheet(workBook, "短路表", titles, ShortData.ToList());

                          //CreateSheet(workBook, "断路表", titles, CutData.ToList());
                          List<String> titles = new List<string>() { "工程数据","断路列表", "短路列表"};
                          Dictionary<string, List<string>> dicts = new Dictionary<string, List<string>>();
                          dicts.Add("project", LearningData.ToList());
                          dicts.Add("short", ShortData.ToList());
                          dicts.Add("cut", CutData.ToList());
                          CreateSheet(workBook, "结果数据",titles, dicts);
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
              });
        }
        

       public  RelayCommand Clear
        {
            get=>
            
                new RelayCommand(() =>
                {
                    LearningData.Clear();
                    CutData.Clear();
                    ShortData.Clear();
                }
                );
            
        }

      

        #endregion

        #region Method


        /// <summary>
        /// 开始检测
        /// </summary>
        private void Detect()
        {


            (App.Current.Resources["Locator"] as ViewModelLocator).Main.finaldata.Clear();
            (App.Current.Resources["Locator"] as ViewModelLocator).Main.tempdata.Clear();
            if (!(App.Current.Resources["Locator"] as ViewModelLocator).Main.IsConnected)
            {
                MessageBox.Show("请先连接设备");
                return;
            }
            if (project == null)
            {
                MessageBox.Show("请先选择一个工程", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            //(App.Current.Resources["Locator"] as ViewModelLocator).Main.OperationName = IsLearning ? "PassiveLearning" : "PassiveDetect";
            (App.Current.Resources["Locator"] as ViewModelLocator).Main.OperationName ="ExPassiveDetect";
            // byte[] msg = IsLearning ? new byte[] { 0xfe, 0xef, 0x30, 0x01, 0x00, 0x02, 0x00, 0x00 } : new byte[] { 0xfe, 0xef, 0x30, 0x01, 0x00, 0x02, 0x00, 0x00 };
            byte[] msg =  new byte[] { 0xfe, 0xef, 0x30, 0x01, 0x00, 0x02, 0x00, 0x00 };
            Messenger.Default.Send<byte[]>(msg, "Send"); 
        }

        /// <summary>
        /// 打开循环
        /// </summary>
        private void CyCle()
        {
            IsCycle = !IsCycle;
            if (IsCycle)
            {
                (App.Current.Resources["Locator"] as ViewModelLocator).Main.OperationName = "ExPassiveCycle";
                timer.Elapsed += Timer_Tick;
                timer.Interval = 1000;
                timer.Start();
            }
            else
            {
                timer.Stop();
            }
        }

        /// <summary>
        /// 循环发消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            Detect();
            timer.Stop();
        }

        /// <summary>
        /// 打开LED
        /// </summary>
        private void OpenLed()
        {
            if (!(App.Current.Resources["Locator"] as ViewModelLocator).Main.IsConnected)
            {
                MessageBox.Show("请先连接设备");
                return;
            }

            var pinsinfos = SQliteDbContext.GetAFixturePinInfos(Project.ProjectNO);

            var baseinfos = SQliteDbContext.GetAllFixtureBaseInfos().Where(x => pinsinfos.Exists(b => b.FixtureType == x.FixtureType));

            //从这里连接信息，去基本的治具信息，找治具的名称
            var finalbase=  baseinfos.Where(x => Connects.ToList().Exists(r => r.FixtureType == x.FixtureType)).ToList();
            List<byte> cmd = new List<byte> { 0xfe, 0xef, 0x30, 0x04, 0x00, 0x08 };
            byte[] bits = new byte[8];
            foreach (var item in finalbase)
            {
                if (item.LEDAddress != 999||item.LEDAddress!=0)
                {
                    int index = item.LEDAddress / 8;

                    if (item.LEDAddress % 8 == 0)
                    {
                        bits[index - 1] = set_bit(bits[index - 1], 8, true);
                    }
                    else
                    {
                        bits[index] = set_bit(bits[index], item.LEDAddress % 8, true);
                    }
                }

            }
            cmd.AddRange(bits);
            Messenger.Default.Send<byte[]>(cmd.ToArray(), "Send");

        }

        /// <summary>
        /// CloseLED
        /// </summary>
        private void CloseLed()
        {
            if (!(App.Current.Resources["Locator"] as ViewModelLocator).Main.IsConnected)
            {
                MessageBox.Show("请先连接设备");
                return;
            }
            List<byte> cmd = new List<byte> { 0xfe, 0xef, 0x30, 0x04, 0x00, 0x08 };
            byte[] bits = new byte[8];
            cmd.AddRange(bits);
            Messenger.Default.Send<byte[]>(cmd.ToArray(), "Send");

        }

        /// <summary>
        /// 设置Bit一位
        /// </summary>
        /// <param name="data"></param>
        /// <param name="index"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        byte set_bit(byte data, int index, bool flag)
        {
            if (index > 8 || index < 1)
                throw new ArgumentOutOfRangeException();
            int v = index < 2 ? index : (2 << (index - 2));
            return flag ? (byte)(data | v) : (byte)(data & ~v);
        }

        private void SaveData()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 更新图纸
        /// </summary>
        /// <param name="result"></param>
        public Dictionary<string, List<ExcelPin>> UpdatePicture(string tagno)
        {
            Dictionary<string, List<ExcelPin>> Allpins = new Dictionary<string, List<ExcelPin>>();

            //LeftImage;
            List<string>leftCodes=    GetCodes(tagno.Split('+')[0]);
            if (leftCodes[2].Trim().Length>0)
            {
                try
                {
                    FixtureConnect = new ObservableCollection<FixtureInfo>(SQliteDbContext.GetAFixturePinInfos(Project.ProjectNO).Where(x => x.FixtureType == leftCodes[2]).ToList());
                    FixtureBase fixtureBase = SQliteDbContext.GetOneFixtureBaseInfo(leftCodes[2]);
                    if (fixtureBase.ImagePath == null || fixtureBase.ImagePath.Trim().Length == 0)
                        ImagePath = AppDomain.CurrentDomain.BaseDirectory + $@"Image\Error.png";
                    else
                    {
                        ImagePath = AppDomain.CurrentDomain.BaseDirectory + $@"Image\ExcelProject\{fixtureBase.ImagePath}";
                        var pins = SQliteDbContext.GetOneFixtureExcelPins(fixtureBase.FixtureType);
                        ExcelPin p = pins.Find(x => x.PinNO == leftCodes[3]);
                        if (p != null)
                        {
                            p.DrawColor = "#00EE00";
                        }
                        Pins = new ObservableCollection<ExcelPin>(pins);
                        Allpins["Left"] = pins;
                    }
                }
                catch (Exception ex)
                {

                    ImagePath = AppDomain.CurrentDomain.BaseDirectory + $@"Image\Error.png";
                    Allpins["Left"] = new List<ExcelPin>();
                }
               
            }
            else
            {
                ImagePath = AppDomain.CurrentDomain.BaseDirectory + $@"Image\Error.png";
                Allpins["Left"] = new List<ExcelPin>();
            }

            //RightImage
            List<string> rightCodes = GetCodes(tagno.Split('+')[1]);
            if (rightCodes[2].Trim().Length > 0)
            {
                try
                {
                    FixtureConnect = new ObservableCollection<FixtureInfo>(SQliteDbContext.GetAFixturePinInfos(Project.ProjectNO).Where(x => x.FixtureType ==rightCodes[2]).ToList());
                    FixtureBase fixtureBase = SQliteDbContext.GetOneFixtureBaseInfo(rightCodes[2]);
                    if (fixtureBase.ImagePath == null || fixtureBase.ImagePath.Trim().Length == 0)
                        ImagePathr = AppDomain.CurrentDomain.BaseDirectory + $@"Image\Error.png";
                    else
                    {
                        ImagePathr = AppDomain.CurrentDomain.BaseDirectory + $@"Image\ExcelProject\{fixtureBase.ImagePath}";
                        var pins = SQliteDbContext.GetOneFixtureExcelPins(fixtureBase.FixtureType);
                        ExcelPin p = pins.Find(x => x.PinNO == rightCodes[3]);
                        if (p != null)
                        {
                            p.DrawColor = "#00EE00";
                        }
                        Pins = new ObservableCollection<ExcelPin>(pins);
                        Allpins["Right"] = pins;
                    }
                }
                catch (Exception ex)
                {

                    ImagePathr = AppDomain.CurrentDomain.BaseDirectory + $@"Image\Error.png";
                    Allpins["Right"] = new List<ExcelPin>();
                }

            }
            else
            {
                ImagePathr = AppDomain.CurrentDomain.BaseDirectory + $@"Image\Error.png";
                Allpins["Right"] = new List<ExcelPin>();
            }
            Messenger.Default.Send<Dictionary<string,List<ExcelPin>>>(Allpins, "clearEx");
            return Allpins;


        }


        private void CreateSheet(HSSFWorkbook workBook, string sheetName, List<string> titles, List<string> table)
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
            foreach (var myRow in table)
            {
                row = sheet.CreateRow(rowIndex);
                //for (int i = 0; i < myRow.Connects.Count; i++)
                //{
                //    row.CreateCell(i).SetCellValue(myRow[i].ToString());
                //}
                row.CreateCell(0).SetCellValue(this.Part.PartType);
                row.CreateCell(1).SetCellValue(this.Wire.WireNO);
                row.CreateCell(2).SetCellValue(this.Wire.WireName);
                row.CreateCell(3).SetCellValue(DateTime.Now.ToString("yyyy-mm-dd HH:MM:ss"));
                var result= myRow.Split('+');
                var first= GetCodes(result[0]);
                row.CreateCell(4).SetCellValue(first[2]);//治具型号
                row.CreateCell(5).SetCellValue(first[0]);//物理地址
                row.CreateCell(6).SetCellValue(first[1]);//坐标号
                row.CreateCell(7).SetCellValue(first[3]);//引脚编号
                row.CreateCell(8).SetCellValue(first[4]);//虚拟编号
                row.CreateCell(9).SetCellValue(first[5]);//虚拟引脚号
                rowIndex++;
                row = sheet.CreateRow(rowIndex);
                row.CreateCell(0).SetCellValue(this.Part.PartType);
                row.CreateCell(1).SetCellValue(this.Wire.WireNO);
                row.CreateCell(2).SetCellValue(this.Wire.WireName);
                row.CreateCell(3).SetCellValue(DateTime.Now.ToString("yyyy-mm-dd HH:MM:ss"));
                var seconde = GetCodes(result[1]);
                row.CreateCell(4).SetCellValue(seconde[2]);//治具型号
                row.CreateCell(5).SetCellValue(seconde[0]);//物理地址
                row.CreateCell(6).SetCellValue(seconde[1]);//坐标号
                row.CreateCell(7).SetCellValue(seconde[3]);//引脚编号
                row.CreateCell(8).SetCellValue(seconde[4]);//虚拟编号
                row.CreateCell(9).SetCellValue(seconde[5]);//虚拟引脚号
                rowIndex++;
            }

            #endregion
        }

        private void CreateSheet(HSSFWorkbook workBook, string sheetName, List<string> titles,Dictionary<string,List<string>> dicts )
        {

            #region 设置字体
            IFont font = workBook.CreateFont();
            font.FontHeightInPoints = 12;
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
            int max = Math.Max(dicts["project"].Count, dicts["short"].Count);
            max = Math.Max(max, dicts["cut"].Count);
            for (int i = 0; i < max; i++)
            {
                    row = sheet.CreateRow(rowIndex);   
                    rowIndex++;
            }
            rowIndex = 1;
            foreach (var item in dicts["project"])
            {
                row = sheet.GetRow(rowIndex);
                row.CreateCell(0).SetCellValue(item);
                rowIndex++;
            }
            rowIndex = 1;
            foreach (var item in dicts["cut"])
            {
                row = sheet.GetRow(rowIndex);
                row.CreateCell(1).SetCellValue(item);
                rowIndex++;
            }
            rowIndex = 1;
            foreach (var item in dicts["short"])
            {
                row = sheet.GetRow(rowIndex);
                row.CreateCell(2).SetCellValue(item);
                rowIndex++;
            }
           
           
            //设置每列的宽度 15个字符宽度
            for (int i = 0; i < 3; i++)
            {
                sheet.SetColumnWidth(i, 80* 256);
            }
            #endregion
        }
        private  string[] getFixtureInfo(string info)
        {
            int length = info.Length;
            int index = info.IndexOf('(');
            int end = info.IndexOf(',');
            var pinindex = info.Substring(0, index);
          
            var fcode =info.Substring(index,length-end);
            fcode = fcode.Substring(3, fcode.Length - 4);
            var pcode = info.Substring(end, length - end - 1);
            pcode = pcode.Substring(3, pcode.Length - 3);
            return new string[] { fcode, pinindex,  pcode };
        }

        private  List<string> GetCodes(string tag)
        {

            List<string> result = new List<string>();

            int start = tag.IndexOf('(');
            int mid = tag.IndexOf(',');
            int end = tag.IndexOf(')');
            int left = tag.IndexOf('[');
            int right = tag.IndexOf(']');
            string FNO = tag.Substring(start + 1, mid - start - 1);
            string FIndex = tag.Substring(mid + 1, end - mid - 1);
            string PAddress = tag.Substring(left + 1, right - left - 1);
            string Axis = tag.Substring(0, tag.IndexOf(':'));
            int vstart = tag.IndexOf('(', right);
            int vend = tag.IndexOf(',', vstart);
            string VNO = tag.Substring(vstart + 1, vend - vstart - 1);
            int vrightend = tag.IndexOf(')', vend);
            string VIndex = tag.Substring(vend + 1, vrightend - vend - 1);
            result.AddRange(new string[] { PAddress, Axis, FNO, FIndex, VNO, VIndex });
            return result;
        }


    
        #endregion
    }
}
