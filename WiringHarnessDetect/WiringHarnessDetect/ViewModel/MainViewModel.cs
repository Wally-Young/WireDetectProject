using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using WiringHarnessDetect.Model;
using System;
using System.Collections.ObjectModel;
using SimpleTcp;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using System.Data;
using System.Data.OleDb;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Linq;
using MessageBox = System.Windows.MessageBox;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using WiringHarnessDetect.Common;

namespace WiringHarnessDetect.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        /// 
        private string ip;
        private int port;
        bool isFirstF = false;//是否是拆保险芯输出
        public MainViewModel()
        {

            Messenger.Default.Register<byte[]>(this, "Send", SendToDevice);
            OperationName = "Text";
        }

        #region Fields
        public List<ConnectionData> connectionDatas = new List<ConnectionData>();
        public List<byte> tempdata = new List<byte>();
        public List<byte> finaldata = new List<byte>();
        readonly byte[] endflag = { 0xfa, 0xfb, 0x60, 0x01, 0x00, 0x04, 0xc1, 0xc1, 0xfc, 0xfd };
        readonly byte[] endflagdelay = { 0xfa, 0xfb, 0x60, 0x03, 0x00, 0x04, 0xc1, 0xc1, 0xfc, 0xfd };   
        private TcpClient client;
        private static int count = 0;
        List<JKSampleData> delayData = new List<JKSampleData>();
        List<JKSampleData> allSampleData = new List<JKSampleData>();//所有的继电器数据
        List<JKSampleData> allOFFData = new List<JKSampleData>();//所有的失电数据
        List<JKSampleData> allOutData = new List<JKSampleData>();//所有的输出数据
        List<JKSampleData> dropOneKData = new List<JKSampleData>();//继电器依次失电数据
        List<JKSampleData> dropOneFData = new List<JKSampleData>();//保险芯依次拆除数据

        List<byte> dropone = new List<byte>();
        List<byte> alloutput = new List<byte>();
        private List<JKSampleData> shortSampleDatas = new List<JKSampleData>();//继电器短路数据
        private List<JKSampleData> cutSampleDatas = new List<JKSampleData>();//继电器断路数据

        JObject jo;
        #endregion
        public List<WireCircuit> cutdata;
        public List<WireCircuit> shortdata;

        private LabelInfo labelInfo;
        #region  Property

        //用户信息
        private User _user;
        public User User
        {
            get => _user;
            set
            {
                _user = value;
                RaisePropertyChanged("User");
            }
        }

        //是否是继电器工程
        public bool isProject=true;
        public bool IsTestProject
        {
            get => isProject;
            set
            {
                isProject = value;
                RaisePropertyChanged("IsTestProject");
            }
        }

        //所有工程
        private ObservableCollection<Project> projects;
        public ObservableCollection<Project> Projects
        {
            get => projects;
            set
            {
                projects = value;
                RaisePropertyChanged("Projects");
            }
        }
        //所有治具图纸
        private ObservableCollection<CablePaper> papers;
        public ObservableCollection<CablePaper> Papers
        {
            get => papers;
            set
            {
                papers = value;
                RaisePropertyChanged("Papers");
            }
        }
        //Tcp是否连接
        private bool isConnected;
        public bool  IsConnected
        {
            get => isConnected;
            set
            {
                isConnected = value;
                RaisePropertyChanged("IsConnected");
            }
        }

        //ExcelProjects
        private ObservableCollection<ExcelProject> exProjects;
        public ObservableCollection<ExcelProject> ExProjects
        {
            get => exProjects;
            set
            {
                exProjects = value;
                RaisePropertyChanged("ExProjects");
            }
        }

      
        //Excel治具信息
        private ObservableCollection<FixtureInfo> fixtures;
        public ObservableCollection<FixtureInfo> Fixtures
        {
            get => fixtures;
            set
            {
                fixtures = value;
                RaisePropertyChanged("Fixtures");
            }
        }

        //Excel回路信息
        private ObservableCollection<WireCircuit> wireCircuits;
        public ObservableCollection<WireCircuit> WireCircuits
        {
            get => wireCircuits;
            set
            {
                wireCircuits = value;
                RaisePropertyChanged("WireCircuits");
            }
        }

        //车型型号
        private ObservableCollection<CarProject> carTypes;
        public ObservableCollection<CarProject> CarTypes
        {
            get => carTypes;
            set
            {
                carTypes = value;
                RaisePropertyChanged("CarTypes");
            }
        }

        //线束型号
        private ObservableCollection<WireType> wireTypes;
        public ObservableCollection<WireType> WireTypes
        {
            get => wireTypes;
            set
            {
                wireTypes = value;
                RaisePropertyChanged("WireTypes");
            }
        }


        private string operationStep;//操作步骤
        public string OperationStep
        {
            get => operationStep;
            set
            {
                operationStep = value;
                RaisePropertyChanged("OperationStep");
            }
        }

        private string operationName; //操作名称
        public string OperationName
        {
            get =>operationName;
            set
            {
                if(value!=operationName)
                {
                    finaldata.Clear();
                    tempdata.Clear();
                    connectionDatas.Clear();
                    operationName = value;
                }
            }
        }
        public byte[] ReceiveData { get; set; }//TCP收到的信息
        #endregion

        #region  Command
        public RelayCommand  Connect
        {
            get => new RelayCommand(() =>
            {
                try
                {
                    
                    if (client.IsConnected)
                    {
                        client.Dispose();
                        client = new TcpClient(ip, port, false, null, null);
                        client.DataReceived += Client_DataReceived;
                        tempdata.Clear();
                        finaldata.Clear();
                        if ((App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.IsCycle)
                            (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Cycle.Execute(null);
                        if ((App.Current.Resources["Locator"] as ViewModelLocator).ExcelPassive.IsCycle)
                            (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPassive.CycleDetect.Execute(null);
                    }
                    else

                        client.Connect();
                    IsConnected = client.IsConnected;
                }
                catch (Exception ex)
                {

                    MessageBox.Show("Tcp无法连接,请检查参数设置是否正确","错误",MessageBoxButton.OK,MessageBoxImage.Error);
                }
                
            }

            );
               
        }
        public RelayCommand OnLoad
        {

            get { return new RelayCommand(Initial); }
        }

        public RelayCommand Print => new RelayCommand(() =>
                                     {
                                         if (labelInfo != null)
                                         {
                                             PrintLabel(jo["printCOM"].ToString(), labelInfo);
                                             count = count++;
                                             jo["Count"] = count + 1;
                                             File.WriteAllText("Config/ServerInfo.json", JsonConvert.SerializeObject(jo));

                                         }

                                     }
                );
        #endregion

        #region Method

        /// <summary>
        /// Tcp发消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_DataReceived(object sender, DataReceivedFromServerEventArgs e)
        {
           
            if(OperationName.Contains("Probe"))
            {
                DealWithProbe(e.Data);
                if ((App.Current.Resources["Locator"] as ViewModelLocator).Paper.IsCycle && IsConnected)
                {
                    (App.Current.Resources["Locator"] as ViewModelLocator).Paper.timer.Start();
                }
                if ((App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.IsCycle && IsConnected)
                {
                    (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.timer.Start();
                }
            }
            else if(OperationName=="PassiveLearning")
            {
                DealWithPassiveLearn(e.Data);
                if((App.Current.Resources["Locator"] as ViewModelLocator).Passive.IsCycle && IsConnected)
                {
                    (App.Current.Resources["Locator"] as ViewModelLocator).Passive.timer.Start();
                }
            }
            else if(OperationName=="PassiveDetect")
            {
                DealWithPassiveDetect(e.Data);
                if ((App.Current.Resources["Locator"] as ViewModelLocator).Passive.IsCycle&&IsConnected)
                {
                    (App.Current.Resources["Locator"] as ViewModelLocator).Passive.timer.Start();
                }
            }
            else if (OperationName == "ExPassiveDetect")
            {
                DealWithExPassiveDetect(e.Data);
                if ((App.Current.Resources["Locator"] as ViewModelLocator).ExcelPassive.IsCycle && IsConnected)
                {
                    (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPassive.timer.Start();
                }
            } 
            else if (OperationName == "ActiveDetectLearning")
            {
                DealWithActiveLearning(e.Data);
                if ((App.Current.Resources["Locator"] as ViewModelLocator).Atctive.IsCycle && IsConnected)
                {
                    (App.Current.Resources["Locator"] as ViewModelLocator).Atctive.timer.Start();
                }
                else if(IsConnected)
                {
                    if((App.Current.Resources["Locator"] as ViewModelLocator).Atctive.Step<5)
                        (App.Current.Resources["Locator"] as ViewModelLocator).Atctive.timer.Start();
                }
            }
          

        }

        /// <summary>
        /// 初始化操作
        /// </summary>
        private void Initial()
        {
            #region ServerInitial

            try
            {
                string jsontxt = File.ReadAllText("Config/ServerInfo.json");
                 jo = JObject.Parse(jsontxt);
                ip = (string)jo["IP"];
                port = (int)jo["Port"];
                if (jo["Date"] != null)
                {
                    if (jo["Date"].ToString() == DateTime.Now.ToString("yyyy-MM-dd"))
                    {
                        count = (int)jo["Count"] + 1;
                    }


                }
                jo["Date"] = DateTime.Now.ToString("yyyy-MM-dd");
                File.WriteAllText("Config/ServerInfo.json", JsonConvert.SerializeObject(jo));
                client = new TcpClient(ip, port, false, null, null);
            }
            catch (Exception ex)
            {

                System.Windows.MessageBox.Show("配置文件无法加载,请检查Config文件下的ServerInfo.json是否配置正确!", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                App.Current.Shutdown();
            }

            client.DataReceived += Client_DataReceived;
            #endregion

            if (!isProject)
                Projects = new ObservableCollection<Project>(SQliteDbContext.GetProjects());
            else
            {

            }
            CarTypes = new ObservableCollection<CarProject>(SQliteDbContext.GetAllCarTypes());
            WireTypes = new ObservableCollection<WireType>(SQliteDbContext.GetAllWireTypes());
            ExProjects = new ObservableCollection<ExcelProject>(SQliteDbContext.GetAllExcelProject());

        }
      

        /// <summary>
        /// 给设备发送信息
        /// </summary>
        /// <param name="msg"></param>
        private void SendToDevice(byte[] msg)
        {

            try
            {
                if(client.IsConnected)
                    client.Send(msg);
            }
            catch (Exception ex)
            {

                MessageBox.Show("设备未连接!", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
               
            
        }

        /// <summary>
        /// 处理探针数据
        /// </summary>
        /// <param name="data"></param>
        private void DealWithProbe(byte[] data)
        {
            if (Enumerable.SequenceEqual(data, tempdata)||data.Length<=10)
            {
                tempdata = data.ToList();
                return;
            }
            else
            {
                finaldata.Clear();
                App.Current.Dispatcher.Invoke(new Action(() =>

               (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Channels.Clear()));
                App.Current.Dispatcher.Invoke(() => (App.Current.Resources["Locator"] as ViewModelLocator).Paper.Channels.Clear());
                //无源线束探针
                if (OperationName=="PassiveProbe")
                {
                    if (BitConverter.ToString(data).IndexOf(BitConverter.ToString(endflag)) >= 0)
                    {
                        finaldata.AddRange(data);
                        var rows = BitConverter.ToString(finaldata.ToArray()).
                            Replace(BitConverter.ToString(new byte[] { 0xFA, 0xFB, 0x60, 0x01 }), "").
                            Replace(BitConverter.ToString(new byte[] { 0xFC, 0xFD }), "$").
                            Split('$').Select(x => x.TrimEnd('-').TrimStart('-')).
                            Where(x => x.Length > 12);

                        foreach (var row in rows)
                        {
                          var arr= strToToHexByte(row.Replace("-", ""));
                          byte[] bytechannel = arr.Skip(4).Take(arr.Length - 4).Reverse().ToArray();
                          for (int i = 0; i < bytechannel.Length; i = i + 2)
                          {
                               App.Current.Dispatcher.Invoke(new Action(()=>
                               {
                                   
                                   if(IsTestProject)
                                        (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Channels.Add(BitConverter.ToInt16(bytechannel, i));
                                   else
                                       (App.Current.Resources["Locator"] as ViewModelLocator).Paper.Channels.Add(BitConverter.ToInt16(bytechannel, i));
                               }
                             
                               ));
                                
                          }
                        }
                        finaldata.Clear();
                    }
                    else
                    {
                        finaldata.AddRange(data);
                    }
                }
                else if(operationName=="ActiveProbe") //有源线束探针
                {
                   
                }
                tempdata = data.ToList();
            }
           
        }

        #region 无源方法

        /// <summary>
        /// 无源线束检测
        /// </summary>
        /// <param name="data"></param>
        private void DealWithPassiveDetect(byte[] data)
        {
            if (Enumerable.SequenceEqual(data, tempdata))
            {
                App.Current.Dispatcher.Invoke(new Action(() => OperationStep = "重复检测相同，重新启动"));
                
                return;
            }
            else
            {
                App.Current.Dispatcher.Invoke(new Action(() => OperationStep = "数据分析中，请稍后"));
                
                if (BitConverter.ToString(data).IndexOf(BitConverter.ToString(endflag)) >= 0)//包含结束标准
                {
                    connectionDatas.Clear();
                    finaldata.AddRange(data);
                    #region【1】提取有效数据
                    var rows = BitConverter.ToString(finaldata.ToArray()).
                        Replace(BitConverter.ToString(new byte[] { 0xFA, 0xFB, 0x60, 0x01 }), "").
                        Replace(BitConverter.ToString(new byte[] { 0xFC, 0xFD }), "$").
                        Split('$').Select(x => x.TrimEnd('-').TrimStart('-')).
                        Where(x => x.Length > 12);
                    #endregion

                    #region【2】 解析数据
                    foreach (var row in rows)
                    {
                        var arr = strToToHexByte(row.Replace("-", ""));
                        ConnectionData CData = new ConnectionData();
                        byte[] sd = arr.Skip(2).Take(2).Reverse().ToArray();
                        CData.ChannelNum = BitConverter.ToUInt16(sd, 0);
                        CData.ProjectNO = (App.Current.Resources["Locator"] as ViewModelLocator).Passive.Project.ProjectNO;
                        var pin = (App.Current.Resources["Locator"] as ViewModelLocator).Passive.Pins.ToList().Find(x => x.physicalChannel == CData.ChannelNum);
                        if (pin != null)
                        {
                            CData.PinCode = pin.PinCode;
                            CData.FixtureCode = pin.FixtureCode;
                        }
                        byte[] bytechannel = arr.Skip(4).Take(arr.Length - 4).Reverse().ToArray();
                        for (int i = 0; i < bytechannel.Length; i = i + 2)
                        {

                            int chanel = BitConverter.ToUInt16(bytechannel.Skip(i).Take(2).ToArray(), 0);
                            if (chanel != CData.ChannelNum)
                            {
                                CData.Connects.Add(chanel);
                            }


                        }

                        connectionDatas.Add(CData);
                    }
                    #endregion

                    #region【3】判断是否是二极管
                    foreach (var item in connectionDatas)
                    {
                        if (!item.Connects.All(t => connectionDatas.Select(x => x.ChannelNum).Any(b => b == t)))
                        {
                            item.IsSingle = true;
                        }
                    }
                    #endregion

                    #region 【4】对比样本数据
                    List<ConnectionData> sampleDatas = SQliteDbContext.GetConnectionDatas((App.Current.Resources["Locator"] as ViewModelLocator).Passive.Project.ProjectNO);
                    if(sampleDatas.Count==0)
                    {
                       
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            (App.Current.Resources["Locator"] as ViewModelLocator).Passive.ShortData = new ObservableCollection<string>(DisplayDeal(connectionDatas));
                            OperationStep = "Error---该工程样本数据为空";
                        });
                    }
                    else
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            (App.Current.Resources["Locator"] as ViewModelLocator).Passive.ShortData = new ObservableCollection<string>(DisplayDeal(sampleDatas));
                            OperationStep = "Error---该工程样本数据为空";
                        });

                        var resj = sampleDatas.Intersect(connectionDatas, new CompareConnectData()); // 交集
                        if (resj.Count() != sampleDatas.Count)//交集不相等有错误
                        {
                            var cutdata = sampleDatas.Except(connectionDatas, new CompareConnectData());//断路数据数据，样本有，但是检测没有;
                            var shortdata = connectionDatas.Except(sampleDatas, new CompareConnectData());//短路数据，样本没有，但是检测有；
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                (App.Current.Resources["Locator"] as ViewModelLocator).Passive.ShortData = new ObservableCollection<string>(DisplayDeal(shortdata.ToList()));
                                (App.Current.Resources["Locator"] as ViewModelLocator).Passive.CutData = new ObservableCollection<string>(DisplayDeal(cutdata.ToList()));
                                OperationStep = "Error";
                            });



                            #region 发送错误编号
                            if ((App.Current.Resources["Locator"] as ViewModelLocator).Passive.IsLED)
                            {
                                HashSet<int> errorLEDs = new HashSet<int>();
                                foreach (var item in cutdata)
                                {
                                    int addr = Papers.ToList().Find(x => x.FixtureCode == item.FixtureCode).LEDAddress;
                                    errorLEDs.Add(addr);
                                }

                                foreach (var item in shortdata)
                                {
                                    int addr = Papers.ToList().Find(x => x.FixtureCode == item.FixtureCode).LEDAddress;
                                    errorLEDs.Add(addr);
                                }
                                Thread.Sleep(50);

                                List<byte> cmd = new List<byte> { 0xfe, 0xef, 0x30, 0x04, 0x00, 0x08, };
                                //short ledstartAddress = (short)numericUpDown1.Value;
                                //cmd.AddRange(BitConverter.GetBytes(ledstartAddress).Reverse());
                                byte[] bits = new byte[8];
                                foreach (var item in errorLEDs)
                                {
                                    // MessageBox.Show(item.ToString());
                                    if (item != 999)
                                    {
                                        int index = item / 8;

                                        if (item % 8 == 0)
                                        {
                                            bits[index - 1] = set_bit(bits[index - 1], 8, true);
                                        }
                                        else
                                        {
                                            bits[index] = set_bit(bits[index], item % 8, true);
                                        }
                                    }
                                }
                                cmd.AddRange(bits);
                                client.Send(cmd.ToArray());
                            }

                            #endregion
                        }
                        else
                        {
                            App.Current.Dispatcher.Invoke(new Action(() => OperationStep = "PASS"));
                        }
                    }
                  


                    #endregion

                    
                    finaldata.Clear();
                }
                else
                {

                    finaldata.AddRange(data);
                }
                
                tempdata = data.ToList();
            }
        }

        /// <summary>
        /// Excel无源线束检测
        /// </summary>
        /// <param name="data"></param>
        private void DealWithExPassiveDetect(byte[] data)
        {
            if (Enumerable.SequenceEqual(data, tempdata))
            {
                App.Current.Dispatcher.Invoke(new Action(() => OperationStep = "重复检测相同，重新启动"));
                return;
            }
            else
            {
                App.Current.Dispatcher.Invoke(new Action(() => OperationStep = "数据分析中，请稍后"));
                // OperationStep = "数据分析中，请稍后";
                if (BitConverter.ToString(data).IndexOf(BitConverter.ToString(endflag)) >= 0)//包含结束标准
                {
                    connectionDatas.Clear();

                    List<WireCircuit> ActualconnectInfos = new List<WireCircuit>();
                    List<BaseData> baseDatas = new List<BaseData>();
                    finaldata.AddRange(data);

                    #region【1】提取有效数据
                    var rows = BitConverter.ToString(finaldata.ToArray()).
                        Replace(BitConverter.ToString(new byte[] { 0xFA, 0xFB, 0x60, 0x01 }), "").
                        Replace(BitConverter.ToString(new byte[] { 0xFC, 0xFD }), "$").
                        Split('$').Select(x => x.TrimEnd('-').TrimStart('-')).
                        Where(x => x.Length > 12);
                    #endregion

                    #region【2】 解析数据
                    foreach (var row in rows)
                    {

                        var arr = strToToHexByte(row.Replace("-", ""));
                        byte[] sd = arr.Skip(2).Take(2).Reverse().ToArray();
                        UInt16 chanel = BitConverter.ToUInt16(sd, 0);
                        List<int> connectitem = new List<int>();
                        BaseData baseData = new BaseData();
                        baseData.Channel = chanel;
                        baseData.Connects = new List<int>();
                        byte[] bytechannel = arr.Skip(4).Take(arr.Length - 4).Reverse().ToArray();
                        for (int i = 0; i < bytechannel.Length; i = i + 2)
                        {
                           ConnectionInfo connectInfo = new ConnectionInfo();
                            int subch = BitConverter.ToUInt16(bytechannel.Skip(i).Take(2).ToArray(), 0);
                            //connectInfo.PhysicalChannel = subch;
                           // ActualconnectInfos.Add(connectInfo);
                            
                             baseData.Connects.Add(subch);
                        }
                        baseDatas.Add(baseData);
                    }
               

                    //去重复
                    var actualBaseData=baseDatas.Distinct();
                    #endregion

                    #region 【3】整合样本数据

                    //找出同一个工程下的所有连接关系
                    var sampleDatas = (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPassive.Connects;
                    if (sampleDatas.Count == 0)
                    {
                        OperationStep = "没有相关的样本数据!";
                        return;
                    }

                    //样本根据线号分类
                    //var finalSampleData = sampleDatas.GroupBy(r => Regex.Replace(r.WireNum, @"[^\d]*", "")).Select(group => new WireCircuit
                    //{
                    //    WireNum = group.Key,
                    //    Connects = new List<ConnectionInfo> (group.ToList()),
                    //}).ToList(); 
                    var finalSampleData = sampleDatas.GroupBy(r => r.WireNum).Select(group => new WireCircuit
                    {
                        WireNum = group.Key,
                        Connects = new List<ConnectionInfo>(group.ToList()),
                    }).ToList();

                    #region 

                    //实际测试结果赋值

                    foreach (var item in actualBaseData)
                    {
                        WireCircuit wireCircuit = new WireCircuit();                     
                        var fd = sampleDatas.ToList().Select(x=>new ConnectionInfo()
                            {
                                ProjectNO = x.ProjectNO,
                                FixtureNO = x.FixtureNO,
                                FixtureType=x.FixtureType,
                                WireNum = x.WireNum,
                                PinIndex = x.PinIndex,
                                PhysicalChannel = x.PhysicalChannel,
                                PinNO = x.PinNO,
                                CirCuitName = x.CirCuitName,
                                Coordinate = x.Coordinate,
                            }).Where(x=>item.Connects.Contains(x.PhysicalChannel));

                        wireCircuit.Connects = new List<ConnectionInfo>(fd.ToList());
                        if (wireCircuit.Connects.Count!=item.Connects.Count)
                        {
                            var d = item.Connects.ToList().FindAll(x => !wireCircuit.Connects.Select(y => y.PhysicalChannel).Contains(x));
                            if(d.Count()>0)
                            {
                                foreach (var subitem in d)
                                {
                                    ConnectionInfo connectionInfo = new ConnectionInfo()
                                    {
                                        PhysicalChannel = subitem,
                                    };
                                    var pin = SQliteDbContext.GetOneExPin(subitem);
                                    if (pin != null)
                                    {
                                        connectionInfo.PinNO = pin.PinNO;
                                        connectionInfo.FixtureType = pin.FixtureType;

                                    }
                                    var p = SQliteDbContext.GetOneFixtureBaseInfo(connectionInfo.FixtureType);
                                    if (p != null)
                                    {
                                        connectionInfo.Coordinate = p.Coordinate;
                                    }
                                    wireCircuit.Connects.Add(connectionInfo);
                                }
                            }
                        }
                       
                        ActualconnectInfos.Add(wireCircuit);
                    }

                    #endregion
                    #endregion

                    #region【4】对比样本数据

                    var rsbase = finalSampleData.Intersect(ActualconnectInfos);
                    if (rsbase.Count() != finalSampleData.Count())
                    {

                        shortdata = finalSampleData.Except(ActualconnectInfos).ToList();
                        cutdata = ActualconnectInfos.Except(finalSampleData).ToList();
                        OperationStep = "Error!";

                        #region 发送错误编号
                        if ((App.Current.Resources["Locator"] as ViewModelLocator).ExcelPassive.IsLED)
                        {
                            var rc = cutdata.Select(x => x.Connects.Select(r => r.FixtureType)).Distinct().ToList();
                            var rs = shortdata.Select(x => x.Connects.Select(r => r.FixtureType)).Distinct().ToList();
                            rs.AddRange(rc);
                            ExcelProject project = (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPassive.Project;
                            var pinsinfos = SQliteDbContext.GetAFixturePinInfos(project.ProjectNO);

                            var baseinfos1 = SQliteDbContext.GetAllFixtureBaseInfos().Where(x => pinsinfos.Exists(b => b.FixtureType == x.FixtureType));


                            //根据断路和断路列表里的治具型号，查找所有的所有错误治具的型号
                            List<string> errorNames = new List<string>();
                            foreach (var item in rs)
                            {
                                foreach (var subitem in item)
                                {
                                    errorNames.Add(subitem);
                                }


                            }
                            errorNames = errorNames.Distinct().ToList();
                            //从基本治具关系中找出这些错误治具的LED地址
                            var errorLEDS = baseinfos1.Where(x => errorNames.Contains(x.FixtureType)).Select(x => x.LEDAddress).ToList();
                            HashSet<int> errorLEDs = new HashSet<int>(errorLEDS);
                         ;
                            Thread.Sleep(50);

                            List<byte> cmd = new List<byte> { 0xfe, 0xef, 0x30, 0x0C,  };
                            List<byte> datas = new List<byte>();
                            datas.AddRange( BitConverter.GetBytes((short)(errorLEDs.Count*2)).Reverse());
                            foreach (var item in errorLEDs)
                            {
                                datas.AddRange(BitConverter.GetBytes((short)item).Reverse());
                            }
                            cmd.AddRange(datas);
                            client.Send(cmd.ToArray());
                        }

                        #endregion

                        #region  显示处理
                        List<string> learningDis = new List<string>();
                        List<string> shortDis = new List<string>();
                        List<string> cutDis = new List<string>();
                        List<string> actualDis = new List<string>();

                        foreach (var item in cutdata)
                        {
                            item.DislayMsg = item.ToString();
                        }
                        foreach (var item in shortdata)
                        {
                            item.DislayMsg = item.ToString();
                        }
                        foreach (var item in finalSampleData)
                        {
                            item.DislayMsg = item.ToString();
                        }
                        finalSampleData.ForEach(x => learningDis.AddRange(ExDisplayDeal(x.DislayMsg.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))));
                        cutdata.ToList().ForEach(x => shortDis.AddRange(ExDisplayDeal(x.DislayMsg.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))));
                        shortdata.ToList().ForEach(x => cutDis.AddRange(ExDisplayDeal(x.DislayMsg.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))));
                        ActualconnectInfos.ToList().ForEach(x => actualDis.AddRange(ExDisplayDeal(x.DislayMsg.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))));

                        cutDis =  cutDis.Where(x => learningDis.Contains(x)&&!actualDis.Contains(x)).ToList();
                       
                        shortDis = shortDis.Where(x => !learningDis.Contains(x)).ToList();
                        cutDis = cutDis.Where(x => !shortDis.Contains(x)).ToList();
                        if(cutDis.Count==0&&shortDis.Count==0)
                        {
                            OperationStep = "Pass!";
                            labelInfo = new LabelInfo();
                            labelInfo.OperatorNum = User.UserID;
                            labelInfo.PartNum = (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPassive.Part.PartType;
                            labelInfo.WireName = (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPassive.Wire.WireName;
                            labelInfo.Result = "合 格";
                            labelInfo.RunningNumber = DateTime.Now.ToString("yyyyMMdd") + (count + 1);
                           

                        }
                        else
                        {
                            labelInfo = new LabelInfo();
                            labelInfo.OperatorNum = User.UserID;
                            labelInfo.PartNum = (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPassive.Part.PartType;
                            labelInfo.WireName = (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPassive.Wire.WireName;
                            labelInfo.Result = "NG";
                            labelInfo.RunningNumber = DateTime.Now.ToString("yyyyMMdd") + (count + 1);
                          
                        }
                        App.Current.Dispatcher.Invoke(() =>
                        {

                            (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPassive.LearningData = new ObservableCollection<string>(learningDis);
                            (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPassive.CutData = new ObservableCollection<string>(cutDis);
                            (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPassive.ShortData = new ObservableCollection<string>(shortDis);
                        });

                        #endregion
                    }
                    else
                    {
                        OperationStep = "Pass!";
                        labelInfo = new LabelInfo();
                        labelInfo.OperatorNum = User.UserID;
                        labelInfo.PartNum = (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPassive.Part.PartType;
                        labelInfo.WireName = (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPassive.Wire.WireName;
                        labelInfo.Result = "合 格";
                       
                    }

                    

                    #endregion

                    finaldata.Clear();

                }
                else
                {

                    finaldata.AddRange(data);
                   
                }
                tempdata = data.ToList();
            }
        }

        /// <summary>
        /// 无源线束学习
        /// </summary>
        /// <param name="data"></param>
        private void DealWithPassiveLearn(byte[] data)
        {
            if (Enumerable.SequenceEqual(data, tempdata))
            {
                App.Current.Dispatcher.Invoke(new Action(() => OperationStep = "重复检测相同，重新启动"));
                return;
            }
            else
            {
                App.Current.Dispatcher.Invoke(new Action(() => OperationStep = "数据分析中，请稍后"));
               
                if (BitConverter.ToString(data).IndexOf(BitConverter.ToString(endflag)) >= 0)//包含结束标准
                {
                    finaldata.AddRange(data);
                    connectionDatas = new List<ConnectionData>();
                    #region【1】提取有效数据
                    var rows = BitConverter.ToString(finaldata.ToArray()).
                        Replace(BitConverter.ToString(new byte[] { 0xFA, 0xFB, 0x60, 0x01 }), "").
                        Replace(BitConverter.ToString(new byte[] { 0xFC, 0xFD }), "$").
                        Split('$').Select(x => x.TrimEnd('-').TrimStart('-')).
                        Where(x => x.Length > 12);
                    #endregion

                    #region【2】 解析数据
                    foreach (var row in rows)
                    {
                        var arr = strToToHexByte(row.Replace("-", ""));
                        ConnectionData CData = new ConnectionData();
                        byte[] sd = arr.Skip(2).Take(2).Reverse().ToArray();
                        CData.ChannelNum = BitConverter.ToUInt16(sd, 0);
                        CData.ProjectNO = (App.Current.Resources["Locator"] as ViewModelLocator).Passive.Project.ProjectNO;
                        var pin = (App.Current.Resources["Locator"] as ViewModelLocator).Passive.Pins.ToList().Find(x => x.physicalChannel ==CData.ChannelNum);
                        if (pin != null)
                        {
                            CData.PinCode = pin.PinCode;
                            CData.FixtureCode = pin.FixtureCode;
                        }
                        byte[] bytechannel = arr.Skip(4).Take(arr.Length - 4).Reverse().ToArray();
                        for (int i = 0; i < bytechannel.Length; i = i + 2)
                        {

                            int chanel = BitConverter.ToUInt16(bytechannel.Skip(i).Take(2).ToArray(), 0);
                            if (chanel != CData.ChannelNum)
                            {
                                CData.Connects.Add(chanel);
                            }


                        }

                        connectionDatas.Add(CData);
                    }
                    #endregion

                    #region【3】判断是否是二极管
                    foreach (var item in connectionDatas)
                    {
                        if(!item.Connects.All(t=>connectionDatas.Select(x=>x.ChannelNum).Any(b=>b==t)))
                        {
                            item.IsSingle = true;
                        }
                    }
                    #endregion

                    #region【4】显示处理
                    App.Current.Dispatcher.Invoke(new Action(() => 
                    (App.Current.Resources["Locator"] as ViewModelLocator).Passive.LearningData = new ObservableCollection<string>(DisplayDeal(connectionDatas))));
                    ;
                   
                    finaldata.Clear();
                    App.Current.Dispatcher.Invoke(new Action(() => OperationStep = "学习结束"));
                    #endregion

                }
                else 
                {
                    
                    finaldata.AddRange(data);
                }
                tempdata = data.ToList();
            }
        }

        #endregion

        /// <summary>
        /// 有源学习处理
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        private void DealWithActiveLearning(byte[] data)
        {
            int step = (App.Current.Resources["Locator"] as ViewModelLocator).Atctive.Step;
            string projectNo = (App.Current.Resources["Locator"] as ViewModelLocator).Atctive.Project.ProjectNO;
         
            if (step == 0)
            {
                if (Enumerable.SequenceEqual(data, tempdata))
                {
                    App.Current.Dispatcher.Invoke(new Action(() => OperationStep = "重复检测相同，重新启动"));
                    return;
                  
                }
                else
                {
                    App.Current.Dispatcher.Invoke(new Action(() => OperationStep = "数据分析中，请稍后"));
                   // OperationStep = "数据分析中，请稍后";
                    if (BitConverter.ToString(data).IndexOf(BitConverter.ToString(endflagdelay)) >= 0)//包含结束标准
                    {
                        finaldata.AddRange(data);
                        #region【1】提取有效数据
                        var rows = BitConverter.ToString(finaldata.ToArray()).
                            Replace(BitConverter.ToString(new byte[] { 0xFA, 0xFB, 0x60, 0x03 }), "").
                            Replace(BitConverter.ToString(new byte[] { 0xFC, 0xFD }), "$").
                            Split('$').Select(x => x.TrimEnd('-').TrimStart('-')).
                            Where(x => x.Length > 12);
                        #endregion

                        #region【2】 解析数据
                        foreach (var row in rows)
                        {
                            var arr = strToToHexByte(row.Replace("-", ""));
                            ConnectionData CData = new ConnectionData();
                            byte[] sd = arr.Skip(2).Take(2).Reverse().ToArray();
                            CData.ChannelNum = BitConverter.ToUInt16(sd, 0);
                            CData.ProjectNO = (App.Current.Resources["Locator"] as ViewModelLocator).Passive.Project.ProjectNO;
                            var pin = (App.Current.Resources["Locator"] as ViewModelLocator).Passive.Pins.ToList().Find(x => x.physicalChannel == CData.ChannelNum);
                            if (pin != null)
                            {
                                CData.PinCode = pin.PinCode;
                                CData.FixtureCode = pin.FixtureCode;
                            }
                            byte[] bytechannel = arr.Skip(4).Take(arr.Length - 4).Reverse().ToArray();
                            for (int i = 0; i < bytechannel.Length; i = i + 2)
                            {

                                int chanel = BitConverter.ToUInt16(bytechannel.Skip(i).Take(2).ToArray(), 0);
                                if (chanel != CData.ChannelNum)
                                {
                                    CData.Connects.Add(chanel);
                                }


                            }

                            connectionDatas.Add(CData);
                        }
                        #endregion

                        #region【3】判断是否是二极管
                        foreach (var item in connectionDatas)
                        {
                            if (!item.Connects.All(t => connectionDatas.Select(x => x.ChannelNum).Any(b => b == t)))
                            {
                                item.IsSingle = true;
                            }
                        }
                        #endregion

                        #region【4】显示处理
                        //学习模式下
                        if ((App.Current.Resources["Locator"] as ViewModelLocator).Atctive.IsLearning)
                        {
                            App.Current.Dispatcher.Invoke(new Action(() => 
                            {
                                (App.Current.Resources["Locator"] as ViewModelLocator).Atctive.LearningData = new ObservableCollection<string>(DisplayDeal(connectionDatas));
                                (App.Current.Resources["Locator"] as ViewModelLocator).Atctive.Step++;//进行下一步学习。
                                OperationStep = "学习结束";
                            }));
                           
                            
                        }
                        //检测模式下
                        else
                        {
                            List<ConnectionData> sampleDatas = SQliteDbContext.GetConnectionDatas((App.Current.Resources["Locator"] as ViewModelLocator).Atctive.Project.ProjectNO);
                            (App.Current.Resources["Locator"] as ViewModelLocator).Atctive.LearningData = new ObservableCollection<string>(DisplayDeal(sampleDatas));
                            var resj = sampleDatas.Intersect(connectionDatas, new CompareConnectData()); // 交集

                            if (resj.Count() != sampleDatas.Count)//交集不相等有错误
                            {
                                var cutdata = sampleDatas.Except(connectionDatas, new CompareConnectData());//断路数据数据，样本有，但是检测没有;
                                var shortdata = connectionDatas.Except(sampleDatas, new CompareConnectData());//短路数据，样本没有，但是检测有；
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    (App.Current.Resources["Locator"] as ViewModelLocator).Atctive.ShortData = new ObservableCollection<string>(DisplayDeal(shortdata.ToList()));
                                    (App.Current.Resources["Locator"] as ViewModelLocator).Atctive.CutData = new ObservableCollection<string>(DisplayDeal(cutdata.ToList()));
                                    OperationStep = "Error";
                                });
                               


                                #region 发送错误编号
                                if ((App.Current.Resources["Locator"] as ViewModelLocator).Passive.IsLED)
                                {
                                    HashSet<int> errorLEDs = new HashSet<int>();
                                    foreach (var item in cutdata)
                                    {
                                        int addr = Papers.ToList().Find(x => x.FixtureCode == item.FixtureCode).LEDAddress;
                                        errorLEDs.Add(addr);
                                    }

                                    foreach (var item in shortdata)
                                    {
                                        int addr = Papers.ToList().Find(x => x.FixtureCode == item.FixtureCode).LEDAddress;
                                        errorLEDs.Add(addr);
                                    }
                                    Thread.Sleep(50);

                                    List<byte> cmd = new List<byte> { 0xfe, 0xef, 0x30, 0x04, 0x00, 0x08, };
                                    //short ledstartAddress = (short)numericUpDown1.Value;
                                    //cmd.AddRange(BitConverter.GetBytes(ledstartAddress).Reverse());
                                    byte[] bits = new byte[8];
                                    foreach (var item in errorLEDs)
                                    {
                                        // MessageBox.Show(item.ToString());
                                        if (item != 999)
                                        {
                                            int index = item / 8;

                                            if (item % 8 == 0)
                                            {
                                                bits[index - 1] = set_bit(bits[index - 1], 8, true);
                                            }
                                            else
                                            {
                                                bits[index] = set_bit(bits[index], item % 8, true);
                                            }
                                        }
                                    }
                                    cmd.AddRange(bits);
                                    client.Send(cmd.ToArray());
                                }

                                #endregion
                                
                            }
                            else
                            {
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    //进行下一步
                                    if ((App.Current.Resources["Locator"] as ViewModelLocator).Atctive.IsCycle)
                                    {
                                        (App.Current.Resources["Locator"] as ViewModelLocator).Atctive.Step++;
                                    }
                               (App.Current.Resources["Locator"] as ViewModelLocator).Atctive.Step++;
                                    OperationStep = "PASS";
                                }
                                );  
                            }
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                OperationStep = "无源线束部分检测出错，检测结束!";
                            });
                           
                        }

                       
                        #endregion

                    }
                    else
                    {

                        finaldata.AddRange(data);
                    }
                    tempdata = data.ToList();
                }
            }
            else if (step >= 1)
            {
                if (Enumerable.SequenceEqual(data, tempdata))
                {
                    App.Current.Dispatcher.Invoke(new Action(() => OperationStep = "重复检测相同，重新启动"));
                    return;
                }
                else
                {
                    int i = 0;
                    finaldata.AddRange(data);
                    int rcount = data.Length;
                    while (i < rcount)
                    {
                        #region【1】提取有效数据
                        byte[] countbyte = finaldata.Skip(i + 4).Take(2).ToArray();//有效数字
                        int count = BitConverter.ToUInt16(countbyte, 0);//字节总数量
                        List<byte> byets = finaldata.Skip(i + 8).Take(count - 2).ToList();//截取有效字节
                        if (i + count + 8 > finaldata.Count)
                        {
                            finaldata = finaldata.Skip(i).Take(finaldata.Count - i).ToList();
                            break;
                        }
                        #endregion

                        #region【2】 功能解析
                        //不输出检测
                        if (finaldata[i + 6] == 0xF1 && finaldata[i + 7] == 0xF1)
                        {
                             allOFFData.Clear();
                             AnalyFunciton(byets, ref allOFFData, projectNo);//解析数据
                             delayData.AddRange(allOFFData);
                            //检测比较
                            CompareData(projectNo, 1, allOFFData);
                            i = i + count + 8;
                            if (i == finaldata.Count)
                            {
                                finaldata.Clear();
                                i = 0;
                                break;
                            }
                            App.Current.Dispatcher.Invoke(() =>
                                    (
                                       (App.Current.Resources["Locator"] as ViewModelLocator).Atctive.CurrentDelayData = new ObservableCollection<JKSampleData>(allOFFData))
                                    );
                            
                        }
                        //全部输出
                        else if (finaldata[i + 6] == 0xF2 && finaldata[i + 7] == 0xF2)
                        {      
                            if (isFirstF)
                            {
                                isFirstF = false;
                                allOutData.Clear();
                                AnalyFunciton(byets, ref allOutData, projectNo);//解析数据
                                finaldata.Clear();
                               break;
                            }
                            else
                            {
                                alloutput.Clear();
                                alloutput.AddRange(finaldata.Skip(i + 8).Take(count - 2).ToArray());
                                allOutData.Clear();
                                AnalyFunciton(byets, ref allOutData, projectNo);//解析数据        
                                //检测比较
                                CompareData(projectNo, 2, allOutData);
                                delayData.AddRange(allOutData);
                                App.Current.Dispatcher.Invoke(() => 
                                {
                                    (App.Current.Resources["Locator"] as ViewModelLocator).Atctive.CurrentDelayData = new ObservableCollection<JKSampleData>(allOutData);
                                });
                                i = i + count + 8;
                                if (i == finaldata.Count)
                                {
                                    finaldata.Clear();
                                    i = 0;
                                }
                            }
                            
                        }
                        //保险芯
                        else if (finaldata[i + 6] == 0xF3 && finaldata[i + 7] == 0xF3)
                        {
                            dropOneFData.Clear();
                            dropone.AddRange(finaldata.Skip(i + 8).Take(count - 2).ToArray());
                            i = i + count + 8;
                            if (i == finaldata.Count)
                            {
                                finaldata.Clear();
                                dropOneFData.Clear();
                                if (dropone.Count == alloutput.Count)
                                {
                                    var fpins = (App.Current.Resources["Locator"] as ViewModelLocator).Atctive.FPins;
                                    var index= (App.Current.Resources["Locator"] as ViewModelLocator).Atctive.FkIndex;
                                    for (int j = 0; j < dropone.Count; j++)
                                    {
                                        #region 逐个解析
                                        JKSampleData jKSampleData = new JKSampleData();
                                        jKSampleData.PhysicalChannel = j + 1;
                                        jKSampleData.ProjectNO = projectNo;
                                        jKSampleData.DetectionStepCode = 4;
                                        jKSampleData.DeviceName = fpins[index].SafeBoxName;
                                        #endregion

                                        #region 比较得出控制引脚
                                        if (alloutput[j] == 0x01 && dropone[j] == 0x00)//NO12V
                                        {
                                            jKSampleData.PinVoltageCode = 3;
                                        }
                                        else if (alloutput[j] == 0x02 && dropone[j] == 0x00)//NO0V
                                        {
                                            jKSampleData.PinVoltageCode = 4;
                                        }
                                        else if (alloutput[j] == 0x00 && dropone[j] == 0x01)//NC12V
                                        {
                                            jKSampleData.PinVoltageCode = 5;
                                        }
                                        else if (alloutput[j] == 0x00 && dropone[j] == 0x02)//NC0V
                                        {
                                            jKSampleData.PinVoltageCode = 6;
                                        }
                                        if (!dropOneFData.Exists(x => x.PhysicalChannel == j + 1) && jKSampleData.PinVoltageCode > 0)
                                        {
                                            try
                                            {
                                                Pin modelPin = SQliteDbContext.GetOnePin(projectNo, j + 1);
                                                if (modelPin != null)
                                                {
                                                    jKSampleData.PinNO = modelPin.PinCode;
                                                    jKSampleData.FixtureCode = modelPin.FixtureCode;
                                                    CablePaper cablePaper = SQliteDbContext.GetPaper(modelPin.FixtureCode);
                                                    if (cablePaper != null)
                                                    {
                                                        jKSampleData.FixtureName = cablePaper.FixtureName;
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {

                                                MessageBox.Show(ex.Message);
                                            }
                                            dropOneFData.Add(jKSampleData);
                                        }
                                        #endregion
                                    }
                                    delayData.AddRange(dropOneFData);
                                    App.Current.Dispatcher.Invoke(() =>
                                    (
                                        App.Current.Resources["Locator"] as ViewModelLocator).Atctive.CurrentDelayData = new ObservableCollection<JKSampleData>(dropOneFData)
                                    );
                                    dropone.Clear();
                                    finaldata.Clear();
                                    break;
                                }
                                else
                                {
                                   break;
                                }
                            }
                        }
                        //依次失电
                        else
                        {
                            dropOneKData.Clear();
                            dropone.Clear();
                            dropone.AddRange(finaldata.Skip(i + 8).Take(count - 2).ToArray());

                            i = i + count + 8;
                            if (i == finaldata.Count)
                            {
                                finaldata.Clear();
                                if (dropone.Count == alloutput.Count)
                                {
                                    var kpins = (App.Current.Resources["Locator"] as ViewModelLocator).Atctive.KPins;
                                    var index = (App.Current.Resources["Locator"] as ViewModelLocator).Atctive.JkIndex;
                                    for (int j = 0; j < dropone.Count; j++)
                                    {
                                        #region 逐个解析
                                        JKSampleData jKSampleData = new JKSampleData();
                                        jKSampleData.PhysicalChannel = j + 1;
                                        jKSampleData.ProjectNO = projectNo;
                                        jKSampleData.DetectionStepCode = 3;
                                        jKSampleData.DeviceName = kpins[index].SafeBoxName;

                                        #endregion

                                        #region 比较得出控制引脚
                                        bool datAddFlag = false;
                                        if (alloutput[j] == 0x01 && dropone[j] == 0x00)//NO12V
                                        {
                                            jKSampleData.PinVoltageCode = 3;
                                            datAddFlag = true;

                                        }
                                        else if (alloutput[j] == 0x02 && dropone[j] == 0x00)//NO0V
                                        {

                                            jKSampleData.PinVoltageCode = 4;
                                            datAddFlag = true;

                                        }
                                        else if (alloutput[j] == 0x00 && dropone[j] == 0x01)//NC12V
                                        { 
                                            jKSampleData.PinVoltageCode = 5;
                                            datAddFlag = true;
                                        }
                                        else if (alloutput[j] == 0x00 && dropone[j] == 0x02)//NC0V
                                        {
                                            jKSampleData.PinVoltageCode = 6;
                                            datAddFlag = true;
                                        }
                                       else if (alloutput[j] == 0x01 && dropone[j] == 0x02)//12V>0V
                                        {
                                            jKSampleData.PinVoltageCode = 7;
                                            datAddFlag = true;
                                        }
                                        else if (alloutput[j] == 0x02 && dropone[j] == 0x01)//0V>12V
                                        {

                                            jKSampleData.PinVoltageCode = 8;
                                            datAddFlag = true;

                                        }
                                        if (datAddFlag)
                                        {
                                            try
                                            {
                                                Pin modelPin = SQliteDbContext.GetOnePin(projectNo, j + 1);
                                                if (modelPin != null)
                                                {
                                                    jKSampleData.PinNO = modelPin.PinCode;
                                                    jKSampleData.FixtureCode = modelPin.FixtureCode;
                                                    CablePaper cablePaper = SQliteDbContext.GetPaper(modelPin.FixtureCode);
                                                    if (cablePaper != null)
                                                    {
                                                        jKSampleData.FixtureName = cablePaper.FixtureName;
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {

                                                MessageBox.Show(ex.Message);
                                            }
                                            dropOneKData.Add(jKSampleData);
                                            App.Current.Dispatcher.Invoke(new Action(() => 
                                            (App.Current.Resources["Locator"] as ViewModelLocator).Atctive.CurrentDelayData = new ObservableCollection<JKSampleData>(dropOneKData)));
                                        }

                                        #endregion


                                    }
                                    ///检测断路列表
                                    /// //检测比较
                                    CompareData(projectNo, 3, dropOneKData);
                                    delayData.AddRange(dropOneKData);
                                    dropone.Clear();
                                    finaldata.Clear();
                                    break;
                                }
                                else
                                {
                                    break;
                                }

                            }
                        }
                        #endregion
                    }
                    #region【3】显示治具名称
                    //查引脚下显示治具
                    if (step >= 1)
                    {
                        if (!(App.Current.Resources["Locator"] as ViewModelLocator).Atctive.IsLearning)
                        {
                            //短路列表
                            DealWithDisplayJKData(ref shortSampleDatas );

                            //断路列表
                            DealWithDisplayJKData(ref cutSampleDatas);

                        }
                    }
                    #endregion

                    #region 【4】 发送错误编号
                    if ((App.Current.Resources["Locator"] as ViewModelLocator).Atctive.IsLED)
                    {
                        HashSet<int> errorLEDs = new HashSet<int>();
                        List<byte> cmd = new List<byte> { 0xfe, 0xef, 0x30, 0x04, 0x00, 0x08 };
                        byte[] bits = new byte[8];
                        if (step > 0) //有源线束
                        {
                            errorLEDs.Union(GetLEDAddress(shortSampleDatas));
                            errorLEDs.Union(GetLEDAddress(cutSampleDatas));
                        }
                        //生成指令发送
                        foreach (var item in errorLEDs)
                        {
                            if (item != 999)
                            {
                                int index = item / 8;

                                if (item % 8 == 0)
                                {
                                    bits[index - 1] = set_bit(bits[index - 1], 8, true);
                                }
                                else
                                {
                                    bits[index] = set_bit(bits[index], item % 8, true);
                                }
                            }
                        }
                        cmd.AddRange(bits);
                        client.Send(cmd.ToArray());
                    }

                    #endregion

                    #region【5】 显示

                    App.Current.Dispatcher.Invoke(() => {
                        (App.Current.Resources["Locator"] as ViewModelLocator).Atctive.DelayData = new ObservableCollection<JKSampleData>(delayData);
                        //进行下一步
                        if ((App.Current.Resources["Locator"] as ViewModelLocator).Atctive.IsCycle && step < 5)
                        {
                            if (shortSampleDatas.Count == 0 && cutSampleDatas.Count == 0)
                                (App.Current.Resources["Locator"] as ViewModelLocator).Atctive.Step++;
                        }
                    });
                   
                   
                    #endregion
                }
            }
       
 
        }
        #endregion

        #region ToolMethod

        /// <summary>
        /// 打印标签信息
        /// </summary>
        /// <param name="port"></param>
        /// <param name="labelInfo"></param>
        private void PrintLabel(string port, LabelInfo labelInfo)
        {

            // 打开 打印机 端口.
            TSCLIB_DLL.openport("TSC TTP-244 Pro");
            // 设置标签 宽度、高度 等信息.
            // 宽 94mm  高 25mm
            // 速度为4
            // 字体浓度为8
            // 使用垂直間距感測器(gap sensor)
            // 两个标签之间的  间距为 3.5mm
            TSCLIB_DLL.setup("40", "30", "4", "8", "0", "0", "0");
            // 清除缓冲信息
            TSCLIB_DLL.clearbuffer();
            // 发送 TSPL 指令.
            // 设置 打印的方向.
            // TSCLIB_DLL.sendcommand("DIRECTION 1");
            // 打印文本信息.
            // 在 (176, 16) 的坐标上
            // 字体高度为34
            // 旋转的角度为 0 度
            // 2 表示 粗体.
            // 文字没有下划线.
            // 字体为 黑体.
            // 打印的内容为：title
            TSCLIB_DLL.windowsfont(5, 5, 20, 0, 2, 0, "宋体", $"线束名称:{labelInfo.WireName}");
            TSCLIB_DLL.windowsfont(5, 35, 20, 0, 2, 0, "宋体", $"线束零件号:{labelInfo.PartNum}");
            TSCLIB_DLL.windowsfont(5, 65, 20, 0, 2, 0, "宋体", $"检测日期:{labelInfo.DetectTime}");
            TSCLIB_DLL.windowsfont(5, 95, 20, 0, 2, 0, "宋体", $"作业流水号:{labelInfo.RunningNumber}");
            TSCLIB_DLL.windowsfont(5, 125, 20, 0, 2, 0, "宋体", $"检测员:{labelInfo.OperatorNum}");
            TSCLIB_DLL.windowsfont(5, 155, 20, 0, 2, 0, "宋体", $"检测状态:{labelInfo.Result}");
            // 打印条码.
            // 在 (176, 66) 的坐标上
            // 以 Code39 的条码方式
            // 条码高度 130
            // 打印条码的同时，还打印条码的文本信息.
            // 旋转的角度为 0 度
            // 条码 宽 窄 比例因子为 7:12
            // 条码内容为:barCode
            //TSCLIB_DLL.barcode("176", "66", "39", "130", "1", "0", "7", "12", barCode);
            // 打印.
            TSCLIB_DLL.printlabel("1", "1");
            // 关闭 打印机 端口
            TSCLIB_DLL.closeport();
            //Open specified printer driver
        }


        /// <summary>
        /// 解析继电器数据
        /// </summary>
        /// <param name="data">Tcp结束的有效字符</param>
        /// <param name="Sdatas">保存的继电器检测过程中的数据</param>
        /// <param name="ProjectNO">项目编号</param>
        private void AnalyFunciton(List<byte> data ,ref List<JKSampleData> Sdatas,string ProjectNO)
        {
            for (int k = 0; k < data.Count; k++)
            {
                if (data[k] == 0x01 || data[k] == 0x02)
                {
                    JKSampleData jKSampleData = new JKSampleData();
                    jKSampleData.PhysicalChannel = k + 1;//物理地址
                    jKSampleData.ProjectNO = ProjectNO;//项目号
                    jKSampleData.DetectionStepCode = 1;//检测步骤
                    jKSampleData.PinVoltageCode = data[k]; //0x01-12V,0x02-0V
                   //添加治具编号
                    if (!allOFFData.Exists(x => x.PhysicalChannel == k + 1))
                    {
                        try
                        {
                            Pin modelPin = SQliteDbContext.GetOnePin(ProjectNO, k + 1);
                            if (modelPin != null)
                            {
                                jKSampleData.PinNO = modelPin.PinCode;
                                jKSampleData.FixtureCode = modelPin.FixtureCode;
                                CablePaper cablePaper = SQliteDbContext.GetPaper(modelPin.FixtureCode);
                                if (cablePaper != null)
                                {
                                    jKSampleData.FixtureName = cablePaper.FixtureName;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        Sdatas.Add(jKSampleData);
                    }
                }
            }
        }

        /// <summary>
        /// 比较数据
        /// </summary>
        /// <param name="projectNo"></param>
        /// <param name="step"></param>
        /// <param name="sampleData"></param>
        private Dictionary<string,List<JKSampleData>> CompareData(string projectNo,int step,List<JKSampleData> sampleData)
        {
            Dictionary<string, List<JKSampleData>> result = new Dictionary<string, List<JKSampleData>>();
            if (!(App.Current.Resources["Locator"] as ViewModelLocator).Atctive.IsLearning)
            {
               List<JKSampleData> jKSampleSaveDatas = SQliteDbContext.GetAllJkSampleDatas(projectNo, step);
               result.Add("shortData", jKSampleSaveDatas.Except(sampleData, new CompareJKDataJK()).ToList());//短路列表
               result.Add("shortData", sampleData.Except(jKSampleSaveDatas, new CompareJKDataJK()).ToList());//断路数据  
            }
            return result;
        }
        /// <summary>
        /// 字符串转16进制字节数组
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        private static byte[] strToToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        /// <summary>
        /// 无源线束结果显示处理
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        private List<string> DisplayDeal(List<ConnectionData> datas)
        {
            List<string> rs = new List<string>();
            foreach (var item in datas)
            {
                string cap = item.IsSingle ? "<" : "+";
                string left = "";
                if (item.PinCode != null && item.PinCode != "")
                {
                    left = $"{item.ChannelNum}(F:{item.FixtureCode}-P:{item.PinCode})";
                }
                else
                {
                    left = $"{item.ChannelNum}";
                }
                foreach (var subitem in item.Connects)
                {
                    var pin = (App.Current.Resources["Locator"] as ViewModelLocator).Passive.Pins.ToList().Find(x => x.physicalChannel == subitem);
                    if (pin != null)
                    {
                        rs.Add(left + cap + $"{subitem}(F:{pin.FixtureCode}-P:{pin.PinCode})");
                    }
                    else
                    {
                        rs.Add(left + cap + $"{subitem}");
                    }
                }

            }
            return rs;
        }

        private List<string> ExDisplayDeal(string[] rs)
        {
            rs = rs.Distinct().ToArray();
            List<string> result = new List<string>();
            for (int i = 0; i < rs.Length; i++)
            {
                for (int j = i + 1; j < rs.Length; j++)
                {
                    if (rs[i] != rs[j])
                        result.Add(rs[i] + "+" + rs[j]);
                }
            }
           
            return result;
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
        /// 处理继电器遗产数据的显示
        /// </summary>
        /// <param name="jKSamples"></param>
        private void DealWithDisplayJKData(ref List<JKSampleData> jKSamples)
        {
            foreach (var item in jKSamples)
            {

                List<JKSampleData> kfDates = new List<JKSampleData>();
                kfDates = SQliteDbContext.GetAllKFPin(item.ProjectNO, item.PhysicalChannel);
                StringBuilder kfDisplay = new StringBuilder();
                kfDisplay.Clear();
                foreach (var sk in kfDates)
                {
                    kfDisplay.Append(sk.DeviceName);
                    kfDisplay.Append("-");
                }
                item.DeviceName = kfDisplay.ToString().TrimEnd('-'); //移除最后的"-"
                try
                {
                    Pin modelPin = SQliteDbContext.GetOnePin(item.ProjectNO, item.PhysicalChannel);
                    if (modelPin != null)
                    {
                        item.PinNO = modelPin.PinCode;
                        item.FixtureCode = modelPin.FixtureCode;
                        CablePaper cablePaper = SQliteDbContext.GetPaper(modelPin.FixtureCode);
                        if (cablePaper != null)
                        {
                            item.FixtureName = cablePaper.FixtureName;
                        }
                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message);
                }
            }
        }

        /// <summary>
        /// 获取错误编号
        /// </summary>
        /// <param name="sampleDatas"></param>
        /// <returns></returns>
        private HashSet<int> GetLEDAddress(List<JKSampleData> sampleDatas)
        {
            HashSet<int> errorLEDs = new HashSet<int>();
            for (int k = 0; k < sampleDatas.Count; k++)
            {
                if (sampleDatas[k].FixtureCode != null)
                {
                    CablePaper cablePaper = null;
                    try
                    {
                        cablePaper = SQliteDbContext.GetPaper(sampleDatas[k].FixtureCode);

                    }
                    catch (Exception ex)
                    {

                        cablePaper = null;
                    }

                    if (cablePaper != null)
                    {
                        errorLEDs.Add(cablePaper.LEDAddress);
                    }
                }

            }
            return errorLEDs;
        }
        #endregion
    }
}