using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using WiringHarnessDetect.Model;
using WiringHarnessDetect.View;

namespace WiringHarnessDetect.ViewModel
{
    public class PaperManagerViewModel: ViewModelBase
    {

        public PaperManagerViewModel()
        {
            Messenger.Default.Register<string>(this, "MenuClick", p =>ProjectAndPaperCURD(p));
            Messenger.Default.Register<string>(this, "UpdateCanavs", p => UpdateCanvas(p));//清空点
            
           
        }


       public System.Timers.Timer timer = new System.Timers.Timer();//实例化Timer类，设置间隔时间为1000毫秒； 
       
       
        #region  Property



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

      

        //治具图纸
        private CablePaper cablePaper;
        public CablePaper CablePaper
        {
            get=>cablePaper;
            set
            {
                cablePaper = value;
                RaisePropertyChanged("CablePaper");
            }
        }

        //引脚
        private Pin pin;
        public Pin Pin
        {
            get => pin;
            set
            {
                pin = value;
                RaisePropertyChanged("Pin");
            }
        }

        private ObservableCollection<PinEx> pins;

        public ObservableCollection<PinEx> Pins
        {
            get => pins;
            set
            {
                pins = value;
                RaisePropertyChanged("Pins");
            }
        }


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

        #endregion

        #region Command

        /// <summary>
        /// 工程改变
        /// </summary>
        public RelayCommand<object> ChangeProject
        {
            get { return new RelayCommand<object>(p=>ProjectSelectChange(p.ToString())); }
        }

        /// <summary>
        /// 图纸改变
        /// </summary>
        public RelayCommand<object> ChangePaper
        {
            get { return new RelayCommand<object>(p => PaperChange(p)); }
        }

        public RelayCommand Manual
        {
            get => new RelayCommand(new Action(()=> 
            {
                IsCycle = false;
                timer.Stop();
                ProbeAction();
            }
            ));
        }

        public RelayCommand<string> Cycle
        {
            get => new RelayCommand<string>(
                p=> { IsCycle = !IsCycle; ProbeCycle(p); });
        }
        #endregion

        #region  Method


        private void ProjectSelectChange(string projectNO)
        {
            (App.Current.Resources["Locator"] as ViewModelLocator).Main.Papers = new ObservableCollection<CablePaper>(SQliteDbContext.GetPapers(projectNO));
        }

        private void PaperChange(object paper )
        {
            CablePaper = paper as CablePaper;
            if(CablePaper!=null)
            {
                ImagePath = AppDomain.CurrentDomain.BaseDirectory + $@"Image\{cablePaper.ProjectNO}\{cablePaper.ImagePath}";
                List<Pin> pins = SQliteDbContext.GetAllFixturePins(this.CablePaper.FixtureCode);
                Messenger.Default.Send<List<Pin>>(pins , "clear");
                List<PinEx> pine = SQliteDbContext.GetExPins(CablePaper.FixtureCode, Project.ProjectNO);
                Pins = new ObservableCollection<PinEx>( pine);
 
            }
            else
            {
                ImagePath =AppDomain.CurrentDomain.BaseDirectory + $@"Image\Error.png";
            }
            
                
        }

        private void ProjectAndPaperCURD(string header)
        {
            if (header == "新建工程")
            {
                ProjectWindow projectWindow = new ProjectWindow();
                projectWindow.ShowDialog();
            }
            else if (header == "删除工程")
            {
                MessageBoxResult result = MessageBox.Show("您确定要删除此工程吗?", "询问", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if(result==MessageBoxResult.OK)
                {
                    int rs = SQliteDbContext.DeleteProject(project);
                    (App.Current.Resources["Locator"] as ViewModelLocator).Main.Projects.Remove(Project);
                    
                    if (rs > 0)
                        MessageBox.Show("删除成功!", "通知", MessageBoxButton.OK, MessageBoxImage.Information);
                }
              
            }
            else if (header == "修改工程")
            {
                ProjectWindow projectWindow = new ProjectWindow(Project);
               
                projectWindow.IsModify = true;
                projectWindow.ShowDialog();
            }
            else if (header == "新建图纸")
            {
                if(Project==null)
                {
                    MessageBox.Show("请先选择一个所属工程!", "通知", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                PaperWindow paperWindow = new PaperWindow(Project.ProjectNO);
                paperWindow.ShowDialog();
            }
            else if (header == "修改图纸")
            {
                PaperWindow paperWindow = new PaperWindow(CablePaper);
                paperWindow.ShowDialog();
            }
            else if (header == "删除图纸")
            {
                MessageBoxResult result = MessageBox.Show("您确定要删除此图纸吗?", "询问", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.OK)
                {
                    int rs = SQliteDbContext.DeletePaper(CablePaper);
                    (App.Current.Resources["Locator"] as ViewModelLocator).Main.Papers.Remove(CablePaper);

                    if (rs > 0)
                        MessageBox.Show("删除成功!", "通知", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else if (header == "全部清空")
            {

            }
        }


        private void UpdateCanvas(string s)
        {
            List<Pin> pins = SQliteDbContext.GetAllFixturePins(this.CablePaper.FixtureCode);
            Messenger.Default.Send<List<Pin>>(pins, "clear");
            Pins = new ObservableCollection<PinEx>(SQliteDbContext.GetExPins(CablePaper.FixtureCode, Project.ProjectNO));
           
        }


        private void ProbeAction()
        {
            if((App.Current.Resources["Locator"] as ViewModelLocator).Main.IsConnected)
            {
                (App.Current.Resources["Locator"] as ViewModelLocator).Main.OperationName = IsPassive?"PassiveProbe":"ActiveProbe";
                byte[] msg;
                if (IsPassive)
                {
                    msg = new byte[] { 0xfe, 0xef, 0x30, 0x01, 0x00, 0x02, 0x00, 0x02 };//无源线束探针
                }
                else
                {
                    msg = new byte[] { 0xfe, 0xef, 0x30, 0x02, 0x00, 0x02, 0x00, 0x05 };//有源线束探针
                }

                Messenger.Default.Send<byte[]>(msg, "Send");
            }
            else
            {
                MessageBox.Show("请先连接设备！");
            }
            
        }

        private void ProbeCycle(string s)
        {
            int interval = 0;
            int.TryParse(s, out interval);
            if (interval < 10)
            {
                MessageBox.Show("请输入正确的时间间隔，间隔必须大于10");
                IsCycle = false;
                return;
            }

            if (IsCycle)
            {
                timer.Interval = interval;
                timer.Elapsed += Timer_Tick;
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

       
        #endregion



    }

   
}
