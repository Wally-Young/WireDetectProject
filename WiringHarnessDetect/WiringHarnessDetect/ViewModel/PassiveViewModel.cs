using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using WiringHarnessDetect.Common;
using WiringHarnessDetect.Model;

namespace WiringHarnessDetect.ViewModel
{
    public class PassiveViewModel : ValidateModelBase
    {
        public PassiveViewModel()
        {
           
          
        }



        public System.Timers.Timer timer = new System.Timers.Timer();//实例化Timer类，设置间隔时间为1000毫秒； 
        #region Property
        //选择的工程
        private Project project;
        public Project Project
        {
            get => project;
            set
            {
                project = value;
                RaisePropertyChanged("Project");
            }
        }

        private bool isLearning;
        public bool IsLearning
        {
            get => isLearning;
            set
            {
                isLearning = value;
                RaisePropertyChanged("IsLearning");
            }
        }


        private bool isCycle;

        public bool IsCycle //是否循环
        {
            get => isCycle;
            set
            {
                isCycle = value;
                RaisePropertyChanged("IsCycle");
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
                RaisePropertyChanged("CutData");
            }
        }

        private ObservableCollection<Pin> pins;
        public ObservableCollection<Pin> Pins
        {
            get => pins;
            set
            {
                pins = value;
                RaisePropertyChanged("Pins");
            }
        }

        private string limagePath;
        public string LImagePath
        {
            get => limagePath;
            set
            {
                limagePath = value;
                RaisePropertyChanged("LImagePath");
            }
        }

        private string rimagePath;
        public string RImagePath
        {
            get => rimagePath;
            set
            {
                rimagePath = value;
                RaisePropertyChanged("RImagePath");
            }
        }
        #endregion

        #region Command
        public RelayCommand<object> ChangeProject
        {
            get { return new RelayCommand<object>(p => ProjectSelectChange(p.ToString())); }
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
        public RelayCommand  Save//保存数据
        {
            get => new RelayCommand(SaveData);
        }
        public RelayCommand Clear//保存数据
        {
            get => new RelayCommand(()=> 
            {
                LearningData.Clear();
                ShortData.Clear();
                CutData.Clear();
            }
            );
        }

        public RelayCommand ExportExcel//保存数据
        {
            get => new RelayCommand(()=>
            {
                Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
                result.Add("学习数据", LearningData.ToList());
                result.Add("短路数据", ShortData.ToList());
                result.Add("断路数据", CutData.ToList());
                ExcelHelper.WritePassive(result);
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
            if (project==null)
            {
                MessageBox.Show("请先选择一个工程", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if(!(App.Current.Resources["Locator"] as ViewModelLocator).Main.IsConnected)
            {
                MessageBox.Show("请先连接设备","提示",MessageBoxButton.OK,MessageBoxImage.Warning);
                return;
            }
            (App.Current.Resources["Locator"] as ViewModelLocator).Main.OperationName = IsLearning ? "PassiveLearning":"PassiveDetect" ;
            byte[] msg = IsLearning ? new byte[] { 0xfe, 0xef, 0x30, 0x01, 0x00, 0x02, 0x00, 0x00 } : new byte[] { 0xfe, 0xef, 0x30, 0x01, 0x00, 0x02, 0x00, 0x00 };
            Messenger.Default.Send<byte[]>(msg, "Send");
        }

        /// <summary>
        /// 打开循环
        /// </summary>
        private void CyCle()
        {
            if (Project == null)
            {
                MessageBox.Show("请选择一个工程!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            IsCycle = !IsCycle;
            if (IsCycle)
            {
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
                List<CablePaper> cablePapers = SQliteDbContext.GetPapers(this.Project.ProjectNO);
                List<byte> cmd = new List<byte> { 0xfe, 0xef, 0x30, 0x04, 0x00, 0x08 };
                byte[] bits = new byte[8];
                foreach (var item in cablePapers)
                {
                    if (item.LEDAddress != 999)
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

        /// <summary>
        /// 工程改变
        /// </summary>
        /// <param name="projectNO"></param>
        private void ProjectSelectChange(string projectNO)
        {
            (App.Current.Resources["Locator"] as ViewModelLocator).Main.Papers = new ObservableCollection<CablePaper>(SQliteDbContext.GetPapers(projectNO));
            Pins = new ObservableCollection<Pin>(SQliteDbContext.GetAllProjectPins(projectNO));
        }
        /// <summary>
        /// 保存数据
        /// </summary>
        private void SaveData()
        {
            var sampleDatas=  (App.Current.Resources["Locator"] as ViewModelLocator).Main.connectionDatas.SelectMany(
                p => p.Connects.Select(
                    d =>
                    new BaseSampleData
                    {
                        ProjectNO =p.ProjectNO,
                        PinCode = p.PinCode,
                        FixtureCode = p.FixtureCode,
                        IsSingle = p.IsSingle,
                        ChannelNum = p.ChannelNum,
                        ConnectTo = d,
                      
                    })
                    ).ToList();
            if(SQliteDbContext.IsExitConnectInfos(sampleDatas.First().ProjectNO))
            {
                SQliteDbContext.DeletePassiveConnectInfos(sampleDatas.First().ProjectNO);
            }
            string sql = "Insert into PassiveSampleData (ProjectNO,PinCode,FixtureCode,ChannelNum,ConnectTo,IsSingle) values (@ProjectNO,@PinCode,@FixtureCode,@ChannelNum,@ConnectTo,@IsSingle)";
            int rs= SQliteDbContext.MultiInsert<BaseSampleData>(sql, sampleDatas);
            if(rs>0)
            {
                MessageBox.Show("保存成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("数据未保存成功！", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// 更新图纸
        /// </summary>
        /// <param name="result"></param>
        public Dictionary<string, List<Pin>> UpdatePicture(string result)
        {
            string[] s = new string[2];
            Dictionary<string, List<Pin>> info = new Dictionary<string, List<Pin>>();
            #region 更新左边图
            if (result.Contains('+'))
            {
                s = result.Split('+');
                if (s[0].Contains('('))
                {
                    int start = s[0].IndexOf('(');

                    s[0] = s[0].Substring(0, start);
                }
                if (s[1].Contains('('))
                {
                    int start = s[1].IndexOf('(');

                    s[1] = s[1].Substring(0, start);
                }
            }
            else if (result.Contains('<'))
            {
                s = result.Split('<');
                if (s[0].Contains('('))
                {
                    int start = s[0].IndexOf('(');

                    s[0] = s[0].Substring(0, start);
                }
                if (s[1].Contains('('))
                {
                    int start = s[1].IndexOf('(');

                    s[1] = s[1].Substring(0, start);
                }
            }
            Pin modelPin = Pins.ToList().Find(x => x.ProjectNO == Project.ProjectNO && x.physicalChannel == int.Parse(s[0]));
            CablePaper cablePaper = null;
            try
            {
                cablePaper = (App.Current.Resources["Locator"] as ViewModelLocator).Main.Papers.ToList().Find(x => x.FixtureCode == modelPin.FixtureCode);
            }
            catch (Exception ex)
            {
                if (!(ex is NullReferenceException))
                    MessageBox.Show(ex.Message);
            }
            if (cablePaper != null)
            {
                LImagePath = AppDomain.CurrentDomain.BaseDirectory + $@"Image\{cablePaper.ProjectNO}\{cablePaper.ImagePath}";
                var modelPins = SQliteDbContext.GetAllProjectPins(Project.ProjectNO).FindAll(x => x.FixtureCode == modelPin.FixtureCode);
                modelPins.Find(x => x.PhysicalChannel == int.Parse(s[0])).DrawColor = "#7CFC00";
                info.Add("Left", modelPins.ToList());
            }
            else
            {
                LImagePath = AppDomain.CurrentDomain.BaseDirectory + $@"Image\Error.png";
            }
            #endregion

            #region 更新右图
            Pin modelPinR = Pins.ToList().Find(x => x.ProjectNO == Project.ProjectNO && x.physicalChannel == int.Parse(s[1]));
            CablePaper cablePaperR = null;
            try
            {
                cablePaperR = (App.Current.Resources["Locator"] as ViewModelLocator).Main.Papers.ToList().Find(x => x.FixtureCode == modelPinR.FixtureCode);
            }
            catch (Exception ex)
            {
                if (!(ex is NullReferenceException))
                    MessageBox.Show(ex.Message);
            }

            if (cablePaperR != null)
            {
                RImagePath = AppDomain.CurrentDomain.BaseDirectory + $@"Image\{cablePaperR.ProjectNO}\{cablePaperR.ImagePath}";
                var modelPinsr = SQliteDbContext.GetAllProjectPins(Project.ProjectNO).FindAll(x => x.FixtureCode == modelPinR.FixtureCode);
                modelPinsr.Find(x => x.PhysicalChannel == int.Parse(s[1])).DrawColor = "#7CFC00";
                info.Add("Right", modelPinsr.ToList());

            }
            else
            {
                RImagePath = AppDomain.CurrentDomain.BaseDirectory + $@"Image\Error.png";
            }

            #endregion

            return info;

        }
        #endregion
    }
}
