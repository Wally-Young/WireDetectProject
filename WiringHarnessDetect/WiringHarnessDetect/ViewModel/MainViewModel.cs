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
        bool isFirstF = false;//�Ƿ��ǲ���о���
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
        List<JKSampleData> allSampleData = new List<JKSampleData>();//���еļ̵�������
        List<JKSampleData> allOFFData = new List<JKSampleData>();//���е�ʧ������
        List<JKSampleData> allOutData = new List<JKSampleData>();//���е��������
        List<JKSampleData> dropOneKData = new List<JKSampleData>();//�̵�������ʧ������
        List<JKSampleData> dropOneFData = new List<JKSampleData>();//����о���β������

        List<byte> dropone = new List<byte>();
        List<byte> alloutput = new List<byte>();
        private List<JKSampleData> shortSampleDatas = new List<JKSampleData>();//�̵�����·����
        private List<JKSampleData> cutSampleDatas = new List<JKSampleData>();//�̵�����·����

        JObject jo;
        #endregion
        public List<WireCircuit> cutdata;
        public List<WireCircuit> shortdata;

        private LabelInfo labelInfo;
        #region  Property

        //�û���Ϣ
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

        //�Ƿ��Ǽ̵�������
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

        //���й���
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
        //�����ξ�ͼֽ
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
        //Tcp�Ƿ�����
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

      
        //Excel�ξ���Ϣ
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

        //Excel��·��Ϣ
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

        //�����ͺ�
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

        //�����ͺ�
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


        private string operationStep;//��������
        public string OperationStep
        {
            get => operationStep;
            set
            {
                operationStep = value;
                RaisePropertyChanged("OperationStep");
            }
        }

        private string operationName; //��������
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
        public byte[] ReceiveData { get; set; }//TCP�յ�����Ϣ
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

                    MessageBox.Show("Tcp�޷�����,������������Ƿ���ȷ","����",MessageBoxButton.OK,MessageBoxImage.Error);
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
        /// Tcp����Ϣ
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
        /// ��ʼ������
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

                System.Windows.MessageBox.Show("�����ļ��޷�����,����Config�ļ��µ�ServerInfo.json�Ƿ�������ȷ!", "����", MessageBoxButton.OK, MessageBoxImage.Error);
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
        /// ���豸������Ϣ
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

                MessageBox.Show("�豸δ����!", "����", MessageBoxButton.OK, MessageBoxImage.Error);
            }
               
            
        }

        /// <summary>
        /// ����̽������
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
                //��Դ����̽��
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
                else if(operationName=="ActiveProbe") //��Դ����̽��
                {
                   
                }
                tempdata = data.ToList();
            }
           
        }

        #region ��Դ����

        /// <summary>
        /// ��Դ�������
        /// </summary>
        /// <param name="data"></param>
        private void DealWithPassiveDetect(byte[] data)
        {
            if (Enumerable.SequenceEqual(data, tempdata))
            {
                App.Current.Dispatcher.Invoke(new Action(() => OperationStep = "�ظ������ͬ����������"));
                
                return;
            }
            else
            {
                App.Current.Dispatcher.Invoke(new Action(() => OperationStep = "���ݷ����У����Ժ�"));
                
                if (BitConverter.ToString(data).IndexOf(BitConverter.ToString(endflag)) >= 0)//����������׼
                {
                    connectionDatas.Clear();
                    finaldata.AddRange(data);
                    #region��1����ȡ��Ч����
                    var rows = BitConverter.ToString(finaldata.ToArray()).
                        Replace(BitConverter.ToString(new byte[] { 0xFA, 0xFB, 0x60, 0x01 }), "").
                        Replace(BitConverter.ToString(new byte[] { 0xFC, 0xFD }), "$").
                        Split('$').Select(x => x.TrimEnd('-').TrimStart('-')).
                        Where(x => x.Length > 12);
                    #endregion

                    #region��2�� ��������
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

                    #region��3���ж��Ƿ��Ƕ�����
                    foreach (var item in connectionDatas)
                    {
                        if (!item.Connects.All(t => connectionDatas.Select(x => x.ChannelNum).Any(b => b == t)))
                        {
                            item.IsSingle = true;
                        }
                    }
                    #endregion

                    #region ��4���Ա���������
                    List<ConnectionData> sampleDatas = SQliteDbContext.GetConnectionDatas((App.Current.Resources["Locator"] as ViewModelLocator).Passive.Project.ProjectNO);
                    if(sampleDatas.Count==0)
                    {
                       
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            (App.Current.Resources["Locator"] as ViewModelLocator).Passive.ShortData = new ObservableCollection<string>(DisplayDeal(connectionDatas));
                            OperationStep = "Error---�ù�����������Ϊ��";
                        });
                    }
                    else
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            (App.Current.Resources["Locator"] as ViewModelLocator).Passive.ShortData = new ObservableCollection<string>(DisplayDeal(sampleDatas));
                            OperationStep = "Error---�ù�����������Ϊ��";
                        });

                        var resj = sampleDatas.Intersect(connectionDatas, new CompareConnectData()); // ����
                        if (resj.Count() != sampleDatas.Count)//����������д���
                        {
                            var cutdata = sampleDatas.Except(connectionDatas, new CompareConnectData());//��·�������ݣ������У����Ǽ��û��;
                            var shortdata = connectionDatas.Except(sampleDatas, new CompareConnectData());//��·���ݣ�����û�У����Ǽ���У�
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                (App.Current.Resources["Locator"] as ViewModelLocator).Passive.ShortData = new ObservableCollection<string>(DisplayDeal(shortdata.ToList()));
                                (App.Current.Resources["Locator"] as ViewModelLocator).Passive.CutData = new ObservableCollection<string>(DisplayDeal(cutdata.ToList()));
                                OperationStep = "Error";
                            });



                            #region ���ʹ�����
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
        /// Excel��Դ�������
        /// </summary>
        /// <param name="data"></param>
        private void DealWithExPassiveDetect(byte[] data)
        {
            if (Enumerable.SequenceEqual(data, tempdata))
            {
                App.Current.Dispatcher.Invoke(new Action(() => OperationStep = "�ظ������ͬ����������"));
                return;
            }
            else
            {
                App.Current.Dispatcher.Invoke(new Action(() => OperationStep = "���ݷ����У����Ժ�"));
                // OperationStep = "���ݷ����У����Ժ�";
                if (BitConverter.ToString(data).IndexOf(BitConverter.ToString(endflag)) >= 0)//����������׼
                {
                    connectionDatas.Clear();

                    List<WireCircuit> ActualconnectInfos = new List<WireCircuit>();
                    List<BaseData> baseDatas = new List<BaseData>();
                    finaldata.AddRange(data);

                    #region��1����ȡ��Ч����
                    var rows = BitConverter.ToString(finaldata.ToArray()).
                        Replace(BitConverter.ToString(new byte[] { 0xFA, 0xFB, 0x60, 0x01 }), "").
                        Replace(BitConverter.ToString(new byte[] { 0xFC, 0xFD }), "$").
                        Split('$').Select(x => x.TrimEnd('-').TrimStart('-')).
                        Where(x => x.Length > 12);
                    #endregion

                    #region��2�� ��������
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
               

                    //ȥ�ظ�
                    var actualBaseData=baseDatas.Distinct();
                    #endregion

                    #region ��3��������������

                    //�ҳ�ͬһ�������µ��������ӹ�ϵ
                    var sampleDatas = (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPassive.Connects;
                    if (sampleDatas.Count == 0)
                    {
                        OperationStep = "û����ص���������!";
                        return;
                    }

                    //���������ߺŷ���
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

                    //ʵ�ʲ��Խ����ֵ

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

                    #region��4���Ա���������

                    var rsbase = finalSampleData.Intersect(ActualconnectInfos);
                    if (rsbase.Count() != finalSampleData.Count())
                    {

                        shortdata = finalSampleData.Except(ActualconnectInfos).ToList();
                        cutdata = ActualconnectInfos.Except(finalSampleData).ToList();
                        OperationStep = "Error!";

                        #region ���ʹ�����
                        if ((App.Current.Resources["Locator"] as ViewModelLocator).ExcelPassive.IsLED)
                        {
                            var rc = cutdata.Select(x => x.Connects.Select(r => r.FixtureType)).Distinct().ToList();
                            var rs = shortdata.Select(x => x.Connects.Select(r => r.FixtureType)).Distinct().ToList();
                            rs.AddRange(rc);
                            ExcelProject project = (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPassive.Project;
                            var pinsinfos = SQliteDbContext.GetAFixturePinInfos(project.ProjectNO);

                            var baseinfos1 = SQliteDbContext.GetAllFixtureBaseInfos().Where(x => pinsinfos.Exists(b => b.FixtureType == x.FixtureType));


                            //���ݶ�·�Ͷ�·�б�����ξ��ͺţ��������е����д����ξߵ��ͺ�
                            List<string> errorNames = new List<string>();
                            foreach (var item in rs)
                            {
                                foreach (var subitem in item)
                                {
                                    errorNames.Add(subitem);
                                }


                            }
                            errorNames = errorNames.Distinct().ToList();
                            //�ӻ����ξ߹�ϵ���ҳ���Щ�����ξߵ�LED��ַ
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

                        #region  ��ʾ����
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
                            labelInfo.Result = "�� ��";
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
                        labelInfo.Result = "�� ��";
                       
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
        /// ��Դ����ѧϰ
        /// </summary>
        /// <param name="data"></param>
        private void DealWithPassiveLearn(byte[] data)
        {
            if (Enumerable.SequenceEqual(data, tempdata))
            {
                App.Current.Dispatcher.Invoke(new Action(() => OperationStep = "�ظ������ͬ����������"));
                return;
            }
            else
            {
                App.Current.Dispatcher.Invoke(new Action(() => OperationStep = "���ݷ����У����Ժ�"));
               
                if (BitConverter.ToString(data).IndexOf(BitConverter.ToString(endflag)) >= 0)//����������׼
                {
                    finaldata.AddRange(data);
                    connectionDatas = new List<ConnectionData>();
                    #region��1����ȡ��Ч����
                    var rows = BitConverter.ToString(finaldata.ToArray()).
                        Replace(BitConverter.ToString(new byte[] { 0xFA, 0xFB, 0x60, 0x01 }), "").
                        Replace(BitConverter.ToString(new byte[] { 0xFC, 0xFD }), "$").
                        Split('$').Select(x => x.TrimEnd('-').TrimStart('-')).
                        Where(x => x.Length > 12);
                    #endregion

                    #region��2�� ��������
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

                    #region��3���ж��Ƿ��Ƕ�����
                    foreach (var item in connectionDatas)
                    {
                        if(!item.Connects.All(t=>connectionDatas.Select(x=>x.ChannelNum).Any(b=>b==t)))
                        {
                            item.IsSingle = true;
                        }
                    }
                    #endregion

                    #region��4����ʾ����
                    App.Current.Dispatcher.Invoke(new Action(() => 
                    (App.Current.Resources["Locator"] as ViewModelLocator).Passive.LearningData = new ObservableCollection<string>(DisplayDeal(connectionDatas))));
                    ;
                   
                    finaldata.Clear();
                    App.Current.Dispatcher.Invoke(new Action(() => OperationStep = "ѧϰ����"));
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
        /// ��Դѧϰ����
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
                    App.Current.Dispatcher.Invoke(new Action(() => OperationStep = "�ظ������ͬ����������"));
                    return;
                  
                }
                else
                {
                    App.Current.Dispatcher.Invoke(new Action(() => OperationStep = "���ݷ����У����Ժ�"));
                   // OperationStep = "���ݷ����У����Ժ�";
                    if (BitConverter.ToString(data).IndexOf(BitConverter.ToString(endflagdelay)) >= 0)//����������׼
                    {
                        finaldata.AddRange(data);
                        #region��1����ȡ��Ч����
                        var rows = BitConverter.ToString(finaldata.ToArray()).
                            Replace(BitConverter.ToString(new byte[] { 0xFA, 0xFB, 0x60, 0x03 }), "").
                            Replace(BitConverter.ToString(new byte[] { 0xFC, 0xFD }), "$").
                            Split('$').Select(x => x.TrimEnd('-').TrimStart('-')).
                            Where(x => x.Length > 12);
                        #endregion

                        #region��2�� ��������
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

                        #region��3���ж��Ƿ��Ƕ�����
                        foreach (var item in connectionDatas)
                        {
                            if (!item.Connects.All(t => connectionDatas.Select(x => x.ChannelNum).Any(b => b == t)))
                            {
                                item.IsSingle = true;
                            }
                        }
                        #endregion

                        #region��4����ʾ����
                        //ѧϰģʽ��
                        if ((App.Current.Resources["Locator"] as ViewModelLocator).Atctive.IsLearning)
                        {
                            App.Current.Dispatcher.Invoke(new Action(() => 
                            {
                                (App.Current.Resources["Locator"] as ViewModelLocator).Atctive.LearningData = new ObservableCollection<string>(DisplayDeal(connectionDatas));
                                (App.Current.Resources["Locator"] as ViewModelLocator).Atctive.Step++;//������һ��ѧϰ��
                                OperationStep = "ѧϰ����";
                            }));
                           
                            
                        }
                        //���ģʽ��
                        else
                        {
                            List<ConnectionData> sampleDatas = SQliteDbContext.GetConnectionDatas((App.Current.Resources["Locator"] as ViewModelLocator).Atctive.Project.ProjectNO);
                            (App.Current.Resources["Locator"] as ViewModelLocator).Atctive.LearningData = new ObservableCollection<string>(DisplayDeal(sampleDatas));
                            var resj = sampleDatas.Intersect(connectionDatas, new CompareConnectData()); // ����

                            if (resj.Count() != sampleDatas.Count)//����������д���
                            {
                                var cutdata = sampleDatas.Except(connectionDatas, new CompareConnectData());//��·�������ݣ������У����Ǽ��û��;
                                var shortdata = connectionDatas.Except(sampleDatas, new CompareConnectData());//��·���ݣ�����û�У����Ǽ���У�
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    (App.Current.Resources["Locator"] as ViewModelLocator).Atctive.ShortData = new ObservableCollection<string>(DisplayDeal(shortdata.ToList()));
                                    (App.Current.Resources["Locator"] as ViewModelLocator).Atctive.CutData = new ObservableCollection<string>(DisplayDeal(cutdata.ToList()));
                                    OperationStep = "Error";
                                });
                               


                                #region ���ʹ�����
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
                                    //������һ��
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
                                OperationStep = "��Դ�������ּ�����������!";
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
                    App.Current.Dispatcher.Invoke(new Action(() => OperationStep = "�ظ������ͬ����������"));
                    return;
                }
                else
                {
                    int i = 0;
                    finaldata.AddRange(data);
                    int rcount = data.Length;
                    while (i < rcount)
                    {
                        #region��1����ȡ��Ч����
                        byte[] countbyte = finaldata.Skip(i + 4).Take(2).ToArray();//��Ч����
                        int count = BitConverter.ToUInt16(countbyte, 0);//�ֽ�������
                        List<byte> byets = finaldata.Skip(i + 8).Take(count - 2).ToList();//��ȡ��Ч�ֽ�
                        if (i + count + 8 > finaldata.Count)
                        {
                            finaldata = finaldata.Skip(i).Take(finaldata.Count - i).ToList();
                            break;
                        }
                        #endregion

                        #region��2�� ���ܽ���
                        //��������
                        if (finaldata[i + 6] == 0xF1 && finaldata[i + 7] == 0xF1)
                        {
                             allOFFData.Clear();
                             AnalyFunciton(byets, ref allOFFData, projectNo);//��������
                             delayData.AddRange(allOFFData);
                            //���Ƚ�
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
                        //ȫ�����
                        else if (finaldata[i + 6] == 0xF2 && finaldata[i + 7] == 0xF2)
                        {      
                            if (isFirstF)
                            {
                                isFirstF = false;
                                allOutData.Clear();
                                AnalyFunciton(byets, ref allOutData, projectNo);//��������
                                finaldata.Clear();
                               break;
                            }
                            else
                            {
                                alloutput.Clear();
                                alloutput.AddRange(finaldata.Skip(i + 8).Take(count - 2).ToArray());
                                allOutData.Clear();
                                AnalyFunciton(byets, ref allOutData, projectNo);//��������        
                                //���Ƚ�
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
                        //����о
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
                                        #region �������
                                        JKSampleData jKSampleData = new JKSampleData();
                                        jKSampleData.PhysicalChannel = j + 1;
                                        jKSampleData.ProjectNO = projectNo;
                                        jKSampleData.DetectionStepCode = 4;
                                        jKSampleData.DeviceName = fpins[index].SafeBoxName;
                                        #endregion

                                        #region �Ƚϵó���������
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
                        //����ʧ��
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
                                        #region �������
                                        JKSampleData jKSampleData = new JKSampleData();
                                        jKSampleData.PhysicalChannel = j + 1;
                                        jKSampleData.ProjectNO = projectNo;
                                        jKSampleData.DetectionStepCode = 3;
                                        jKSampleData.DeviceName = kpins[index].SafeBoxName;

                                        #endregion

                                        #region �Ƚϵó���������
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
                                    ///����·�б�
                                    /// //���Ƚ�
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
                    #region��3����ʾ�ξ�����
                    //����������ʾ�ξ�
                    if (step >= 1)
                    {
                        if (!(App.Current.Resources["Locator"] as ViewModelLocator).Atctive.IsLearning)
                        {
                            //��·�б�
                            DealWithDisplayJKData(ref shortSampleDatas );

                            //��·�б�
                            DealWithDisplayJKData(ref cutSampleDatas);

                        }
                    }
                    #endregion

                    #region ��4�� ���ʹ�����
                    if ((App.Current.Resources["Locator"] as ViewModelLocator).Atctive.IsLED)
                    {
                        HashSet<int> errorLEDs = new HashSet<int>();
                        List<byte> cmd = new List<byte> { 0xfe, 0xef, 0x30, 0x04, 0x00, 0x08 };
                        byte[] bits = new byte[8];
                        if (step > 0) //��Դ����
                        {
                            errorLEDs.Union(GetLEDAddress(shortSampleDatas));
                            errorLEDs.Union(GetLEDAddress(cutSampleDatas));
                        }
                        //����ָ���
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

                    #region��5�� ��ʾ

                    App.Current.Dispatcher.Invoke(() => {
                        (App.Current.Resources["Locator"] as ViewModelLocator).Atctive.DelayData = new ObservableCollection<JKSampleData>(delayData);
                        //������һ��
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
        /// ��ӡ��ǩ��Ϣ
        /// </summary>
        /// <param name="port"></param>
        /// <param name="labelInfo"></param>
        private void PrintLabel(string port, LabelInfo labelInfo)
        {

            // �� ��ӡ�� �˿�.
            TSCLIB_DLL.openport("TSC TTP-244 Pro");
            // ���ñ�ǩ ��ȡ��߶� ����Ϣ.
            // �� 94mm  �� 25mm
            // �ٶ�Ϊ4
            // ����Ũ��Ϊ8
            // ʹ�ô�ֱ�g��Мy��(gap sensor)
            // ������ǩ֮���  ���Ϊ 3.5mm
            TSCLIB_DLL.setup("40", "30", "4", "8", "0", "0", "0");
            // ���������Ϣ
            TSCLIB_DLL.clearbuffer();
            // ���� TSPL ָ��.
            // ���� ��ӡ�ķ���.
            // TSCLIB_DLL.sendcommand("DIRECTION 1");
            // ��ӡ�ı���Ϣ.
            // �� (176, 16) ��������
            // ����߶�Ϊ34
            // ��ת�ĽǶ�Ϊ 0 ��
            // 2 ��ʾ ����.
            // ����û���»���.
            // ����Ϊ ����.
            // ��ӡ������Ϊ��title
            TSCLIB_DLL.windowsfont(5, 5, 20, 0, 2, 0, "����", $"��������:{labelInfo.WireName}");
            TSCLIB_DLL.windowsfont(5, 35, 20, 0, 2, 0, "����", $"���������:{labelInfo.PartNum}");
            TSCLIB_DLL.windowsfont(5, 65, 20, 0, 2, 0, "����", $"�������:{labelInfo.DetectTime}");
            TSCLIB_DLL.windowsfont(5, 95, 20, 0, 2, 0, "����", $"��ҵ��ˮ��:{labelInfo.RunningNumber}");
            TSCLIB_DLL.windowsfont(5, 125, 20, 0, 2, 0, "����", $"���Ա:{labelInfo.OperatorNum}");
            TSCLIB_DLL.windowsfont(5, 155, 20, 0, 2, 0, "����", $"���״̬:{labelInfo.Result}");
            // ��ӡ����.
            // �� (176, 66) ��������
            // �� Code39 �����뷽ʽ
            // ����߶� 130
            // ��ӡ�����ͬʱ������ӡ������ı���Ϣ.
            // ��ת�ĽǶ�Ϊ 0 ��
            // ���� �� խ ��������Ϊ 7:12
            // ��������Ϊ:barCode
            //TSCLIB_DLL.barcode("176", "66", "39", "130", "1", "0", "7", "12", barCode);
            // ��ӡ.
            TSCLIB_DLL.printlabel("1", "1");
            // �ر� ��ӡ�� �˿�
            TSCLIB_DLL.closeport();
            //Open specified printer driver
        }


        /// <summary>
        /// �����̵�������
        /// </summary>
        /// <param name="data">Tcp��������Ч�ַ�</param>
        /// <param name="Sdatas">����ļ̵����������е�����</param>
        /// <param name="ProjectNO">��Ŀ���</param>
        private void AnalyFunciton(List<byte> data ,ref List<JKSampleData> Sdatas,string ProjectNO)
        {
            for (int k = 0; k < data.Count; k++)
            {
                if (data[k] == 0x01 || data[k] == 0x02)
                {
                    JKSampleData jKSampleData = new JKSampleData();
                    jKSampleData.PhysicalChannel = k + 1;//�����ַ
                    jKSampleData.ProjectNO = ProjectNO;//��Ŀ��
                    jKSampleData.DetectionStepCode = 1;//��ⲽ��
                    jKSampleData.PinVoltageCode = data[k]; //0x01-12V,0x02-0V
                   //����ξ߱��
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
        /// �Ƚ�����
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
               result.Add("shortData", jKSampleSaveDatas.Except(sampleData, new CompareJKDataJK()).ToList());//��·�б�
               result.Add("shortData", sampleData.Except(jKSampleSaveDatas, new CompareJKDataJK()).ToList());//��·����  
            }
            return result;
        }
        /// <summary>
        /// �ַ���ת16�����ֽ�����
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
        /// ��Դ���������ʾ����
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
        /// ����Bitһλ
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
        /// ����̵����Ų����ݵ���ʾ
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
                item.DeviceName = kfDisplay.ToString().TrimEnd('-'); //�Ƴ�����"-"
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
        /// ��ȡ������
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