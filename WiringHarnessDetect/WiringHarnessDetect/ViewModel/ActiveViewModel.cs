using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WiringHarnessDetect.Common;
using WiringHarnessDetect.Model;

namespace WiringHarnessDetect.ViewModel
{
    public class ActiveViewModel:ValidateModelBase
    {

        public int JkIndex = 0;
        public int FkIndex = 0;
        public string operationStep = "";
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

        private bool isPause;

        public bool IsPause //是否暂停
        {
            get => isPause;
            set
            {
                isPause = value;
                RaisePropertyChanged("IsPause");
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

        private string limagePath;//左边图纸路径
        public string LImagePath
        {
            get => limagePath;
            set
            {
                limagePath = value;
                RaisePropertyChanged("LImagePath");
            }
        }

        private string rimagePath;//右边图纸路径
        public string RImagePath
        {
            get => rimagePath;
            set
            {
                rimagePath = value;
                RaisePropertyChanged("RImagePath");
            }
        }

        private int step;//操作步骤
        public int Step
        {
            get => step;
            set
            {
                step = value;
                RaisePropertyChanged("Step");
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

        private ObservableCollection<JKSampleData> delayData;
        public ObservableCollection<JKSampleData> DelayData
        {
            get
            {
                if (delayData == null)
                    delayData = new ObservableCollection<JKSampleData>();
                return delayData;
            }
            set
            {
                delayData = value;
                RaisePropertyChanged("DelayData");
            }
        }


        private ObservableCollection<JKSampleData> currentDelayData;
        public ObservableCollection<JKSampleData> CurrentDelayData
        {
            get
            {
                if (currentDelayData == null)
                    currentDelayData = new ObservableCollection<JKSampleData>();
                return currentDelayData;
            }
            set
            {
                currentDelayData = value;
                RaisePropertyChanged("CurrentDelayData");
            }
        }

        private ObservableCollection<JKSampleData> jkShortData;
        public ObservableCollection<JKSampleData> JKShortData
        {
            get
            {
                if (jkShortData == null)
                    jkShortData = new ObservableCollection<JKSampleData>();
                return jkShortData;
            }
            set
            {
                jkShortData = value;
                RaisePropertyChanged("JKShortData");
            }
        }
        private ObservableCollection<JKSampleData> jkCutData;
        public ObservableCollection<JKSampleData> JKCutData
        {
            get
            {
                if (jkCutData == null)
                    jkCutData = new ObservableCollection<JKSampleData>();
                return jkCutData;
            }
            set
            {
                jkCutData = value;
                RaisePropertyChanged("JKCutData");
            }
        }

        public List<Pin> KPins { get; set; }

        public List<Pin> FPins { get; set; }

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
        #endregion

        #region Command
        public RelayCommand BeginDetect //开始检测
        {
            get => new RelayCommand(Detect);
        }
        public RelayCommand CycleDetect//循环检测
        {
            get => new RelayCommand(CyCle);
        }

        public RelayCommand NextStep//循环检测
        {
            get => new RelayCommand(()=> 
            {
                (App.Current.Resources["Locator"] as ViewModelLocator).Main.finaldata.Clear();
                (App.Current.Resources["Locator"] as ViewModelLocator).Main.tempdata.Clear();
                Step++;
            });
        }

        public RelayCommand Pause//循环检测
        {
            get => new RelayCommand(() =>
            {
                IsPause = !IsPause;    
            });
        }

        public RelayCommand Save//保存数据
        {
            get => new RelayCommand(SaveData);
        }
        public RelayCommand ExportExcel//保存数据
        {
            get => new RelayCommand(() =>
            {
                Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
                result.Add("无源部分学习数据", LearningData.ToList());
                result.Add("无源部分短路数据", ShortData.ToList());
                result.Add("无源部分断路数据", CutData.ToList());
               
                Dictionary<string, List<JKSampleData>> resultd = new Dictionary<string, List<JKSampleData>>();
                resultd.Add("有源源部分学习数据", DelayData.ToList());
                resultd.Add("有源部分短路数据", JKShortData.ToList());
                resultd.Add("有源部分断路数据", JKCutData.ToList());
                ExcelHelper.WriteActive(resultd,result);
            }
            );
        }
        #endregion

        #region Method
        private void Detect()
        {
            if (Project==null)
            {
                MessageBox.Show("请选择一个工程!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
                (App.Current.Resources["Locator"] as ViewModelLocator).Main.OperationStep = "开始测量，初始化开始，请稍等";
            if (!(App.Current.Resources["Locator"] as ViewModelLocator).Main.IsConnected)
            {
                MessageBox.Show("请先连接设备");
                return;
            }
            CablePaper cablePaper = (App.Current.Resources["Locator"] as ViewModelLocator).Main.Papers.ToList().Find(x => x.IsSafeBox == 1);
            //找到保险盒图片
            if (Project.ProjectType==1)
            {
              
                LImagePath = AppDomain.CurrentDomain.BaseDirectory + $@"Image\{cablePaper.ProjectNO}\{cablePaper.ImagePath}";
               
            }
            Step = 0;

            (App.Current.Resources["Locator"] as ViewModelLocator).Main.OperationStep = IsLearning ? "线束无源部分学习" : "线束无源部分检测";
            (App.Current.Resources["Locator"] as ViewModelLocator).Main.OperationName = "ActiveDetectLearning";
            //找到所有的Pin
            if (IsLearning)
            {
                List<Pin> pins=  SQliteDbContext.GetAllFixturePins(cablePaper.FixtureCode);
                Pins = new ObservableCollection<Pin>(pins);
                if (pins.Exists(x => x.SafeBoxName != null && x.SafeBoxName.Contains("K")))
                    KPins = pins.FindAll(x => x.SafeBoxName.Contains("K"));
                else
                    KPins = new List<Pin>();
                if (pins.Exists(x => x.SafeBoxName != null && x.SafeBoxName.Contains("F")))
                    FPins = pins.FindAll(x => x.SafeBoxName.Contains("F"));
                else
                    FPins = new List<Pin>();
            }
            //发送测试执行
            byte[] msg = new byte[] { 0xfe, 0xef, 0x30, 0x02, 0x00, 0x02, 0x00, 0x00 };
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
                Messenger.Default.Send<string>("disenable" ,"NextDisable");
                timer.Elapsed += Timer_Tick;
                timer.Interval = 1000;
                timer.Start();
            }
            else
            {
                Messenger.Default.Send<string>("enable", "NextDisable");
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
            if(IsPause)
            {
                (App.Current.Resources["Locator"] as ViewModelLocator).Main.OperationStep = "暂停检测中";
                return;
            }
            byte[] msg=new byte[] { };
           
            if (IsLearning)
            {              
                switch (step)
                {
                    case 0: operationStep = "线束无源部分检测"; msg=new byte[] { 0xfe, 0xef, 0x30, 0x02, 0x00, 0x02, 0x00, 0x00 }; break;
                    case 1: operationStep = "线束上电检测";    msg= new byte[] { 0xfe, 0xef, 0x30, 0x02, 0x00, 0x02, 0x00, 0x01 }; break;
                    case 2: operationStep = "继电器全ON检测";  msg=new byte[] { 0xfe, 0xef, 0x30, 0x02, 0x00, 0x02, 0x00, 0x02 }; break;//继电器全输出
                    case 3:                       
                        if (KPins != null && KPins.Count != 0)
                        {
                            operationStep = $"{KPins[JkIndex].SafeBoxName}失电检测";
                            if (JkIndex >= 1)
                            {
                                Pins.ToList().Find(x => x.SafeBoxName == KPins[JkIndex - 1].SafeBoxName).DrawColor = "#7CFC00";
                            }
                            else
                            {
                                Pins.ToList().Find(x => x.SafeBoxName == KPins[JkIndex].SafeBoxName).DrawColor = "#7CFC00";
                            }
                            List<byte> cmd = new List<byte> { 0xfe, 0xef, 0x30, 0x02, 0x00, 0x04, 0x00, 0x03 };//继电器依次失电
                            if (KPins.Count > 0)
                            {
                                cmd.AddRange(BitConverter.GetBytes((short)(KPins[JkIndex].RelayControlChannel)).Reverse());

                                msg = cmd.ToArray();
                            }
                        }
                        else
                        {
                            operationStep = "失电检测";
                            step = 4;
                            MessageBox.Show("没有编辑导入继电器数据","提示",MessageBoxButton.OK,MessageBoxImage.Warning);
                        }
                        break;
                    case 4:                      
                        if (FPins != null && FPins.Count != 0)
                        {
                            msg=new byte[] { 0xfe, 0xef, 0x30, 0x02, 0x00, 0x02, 0x00, 0x04 };
                            operationStep = $"正在检测,{FPins[FkIndex].SafeBoxName}拆除检测  请拆除{FPins[FkIndex].SafeBoxName}保险芯！";
                            if (JkIndex >= 1)
                            {
                                Pins.ToList().Find(x => x.SafeBoxName == FPins[FkIndex - 1].SafeBoxName).DrawColor = "#7CFC00";
                            }
                            else
                            {
                                Pins.ToList().Find(x => x.SafeBoxName == FPins[FkIndex].SafeBoxName).DrawColor = "#7CFC00";
                            }
                        }
                        else
                        {
                            operationStep = "保险芯检测！";
                            MessageBox.Show("没有编辑导入保险芯数据", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                            step = 5;
                        }
                        break;//保险盒拆除
                    case 5: operationStep = "所有步骤执行完成！"; step = 0; timer.Stop(); break;
                }
            }
            else 
            {
                switch (step)
                {
                    case 0: operationStep = "线束无源部分检测"; msg=new byte[] { 0xfe, 0xef, 0x30, 0x02, 0x00, 0x02, 0x00, 0x00 }; break;
                    case 1: operationStep = "线束上电检测";  msg=new byte[] { 0xfe, 0xef, 0x30, 0x02, 0x00, 0x02, 0x00, 0x01 }; break;
                    case 2: operationStep = "继电器全ON检测"; msg=new byte[] { 0xfe, 0xef, 0x30, 0x02, 0x00, 0x02, 0x00, 0x02 }; break;//继电器全输出
                    case 3:
                        if (KPins != null && KPins.Count != 0)
                        {
                            operationStep = $"{KPins[JkIndex].SafeBoxName}失电检测";
                            if (JkIndex >= 1)
                            {
                                Pins.ToList().Find(x => x.SafeBoxName == KPins[JkIndex - 1].SafeBoxName).DrawColor = "#7CFC00";
                            }
                            else
                            {
                                Pins.ToList().Find(x => x.SafeBoxName == KPins[JkIndex].SafeBoxName).DrawColor = "#7CFC00";
                            }
                            List<byte> cmd = new List<byte> { 0xfe, 0xef, 0x30, 0x02, 0x00, 0x04, 0x00, 0x03 };//继电器依次失电
                            if (KPins.Count > 0)
                            {
                                cmd.AddRange(BitConverter.GetBytes((short)(KPins[JkIndex].RelayControlChannel)).Reverse());
                                msg = cmd.ToArray();
                            }
                        }
                        else
                        {
                            operationStep = "继电器失电检测无数据";
                            step = 4;
                            Thread.Sleep(100);
                           
                        }             
                        break;
                    case 4:
                        operationStep = "所有步骤执行完成！";
                        if (IsCycle)
                        {
                            operationStep = "线束无源部分检测！";
                            Detect();
                        }
                        break;
                }
            }
            //发送指令消息
            if(msg.Length>0)
                 Messenger.Default.Send<byte[]>(msg, "Send");
            //更新步骤名称
            (App.Current.Resources["Locator"] as ViewModelLocator).Main.OperationStep = operationStep;
            timer.Stop();
        }

        /// <summary>
        /// 更新图纸
        /// </summary>
        /// <param name="result"></param>
        private void UpdatePicture(string result)
        {
            string[] s = new string[2];
            Dictionary<string, List<Pin>> info = new Dictionary<string, List<Pin>>();

            #region 更新左边图
            if (result.Contains('+'))
            {
                s = result.Split('+');
                if (s[0].Contains('('))
                {
                    int start = s[0].IndexOf('(') ;
                    
                    s[0] = s[0].Substring(0,start);
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

                    s[0] = s[0].Substring(0, start );
                }
                if (s[1].Contains('('))
                {
                    int start = s[1].IndexOf('(');

                    s[1] = s[1].Substring(0, start );
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
                modelPins.Find(x => x.PhysicalChannel == int.Parse( s[0])).DrawColor = "#7CFC00";
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

            Messenger.Default.Send<Dictionary<string, List<Pin>>>(info, "UpdateCanvaPin");
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        private void SaveData()
        {
            var sampleDatas = (App.Current.Resources["Locator"] as ViewModelLocator).Main.connectionDatas.SelectMany(
                p => p.Connects.Select(
                    d =>
                    new BaseSampleData
                    {
                        ProjectNO = p.ProjectNO,
                        PinCode = p.PinCode,
                        FixtureCode = p.FixtureCode,
                        IsSingle = p.IsSingle,
                        ChannelNum = p.ChannelNum,
                        ConnectTo = d,

                    })
                    ).ToList();
            if (SQliteDbContext.IsExitConnectInfos(sampleDatas.First().ProjectNO))
            {
                SQliteDbContext.DeletePassiveConnectInfos(sampleDatas.First().ProjectNO);
            }
            string sql = "Insert into PassiveSampleData (ProjectNO,PinCode,FixtureCode,ChannelNum,ConnectTo,IsSingle) values (@ProjectNO,@PinCode,@FixtureCode,@ChannelNum,@ConnectTo,@IsSingle)";
            int rs = SQliteDbContext.MultiInsert<BaseSampleData>(sql, sampleDatas);

            if (SQliteDbContext.IsExitConnectInfos(DelayData.First().ProjectNO))
            {
                SQliteDbContext.DeletePassiveConnectInfos(DelayData.First().ProjectNO);
            }
            string sqla = "Insert into ActiveSampleData (ProjectNO,DetectionStepCode,PinVoltageCode,PinNO,DeviceName,PhysicalChannel,FixtureName,FixtureCode) values" +
                " (@ProjectNO,@DetectionStepCode,@PinVoltageCode,@PinNO,@DeviceName,@PhysicalChannel,@FixtureName,@FixtureCode)";
            int rsa = SQliteDbContext.MultiInsert<BaseSampleData>(sql, sampleDatas);
            if (rs > 0&&rsa>0)
            {
                MessageBox.Show("保存成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("数据未保存成功！", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        #endregion
    }
}
