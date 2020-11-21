using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WiringHarnessDetect.Model;

namespace WiringHarnessDetect.ViewModel
{
    public class ParameterViewModel : ValidateModelBase
    {

        public ParameterViewModel()
        {

        }

        #region Property
        public Dictionary<string, AddressInfo> AddressInfos
        {
            get
            {
                return new Dictionary<string, AddressInfo>()
                {
                    ["Passive"] = new AddressInfo() { StartBoradNum = 12, StartAddress = 12 },
                    ["LED"] = new AddressInfo(),
                    ["ActiveDetect"] = new AddressInfo(),
                    ["ActiveControl"] = new AddressInfo(),
                    ["ActiveRelay"] = new AddressInfo(),

                };
            }

        }

        private AddressInfo passive;
        public AddressInfo Passive
        {
            get
            {
                if (passive == null)
                    passive = new AddressInfo();
                return passive;
            }
            set
            {
                AddressInfos["Passive"] = value;
                RaisePropertyChanged("Passive");
            }
        }

        private AddressInfo led;
        public AddressInfo LED
        {
            get
            {
                if (led == null)
                    led = new AddressInfo();
                return led;
            }
            set
            {
                led = value;
                RaisePropertyChanged("LED");
            }
        }

        private AddressInfo activeDetect;
        public AddressInfo ActiveDetect
        {
            get
            {
                if (activeDetect == null)
                    activeDetect = new AddressInfo();
                return activeDetect;
            }
            set
            {
                activeDetect = value;
                RaisePropertyChanged("ActiveDetect");
            }
        }

        private AddressInfo activeControl;
        public AddressInfo ActiveControl
        {
            get
            {
                if (activeControl == null)
                    activeControl = new AddressInfo();
                return activeControl;
            }
            set
            {
                activeControl = value;
                RaisePropertyChanged("ActiveControl");
            }
        }

        private AddressInfo activeRalay;
        public AddressInfo ActiveRalay
        {
            get
            {

                if (activeRalay == null)
                    activeRalay = new AddressInfo();
                return activeRalay;

            }
            set
            {
                activeRalay = value;
                RaisePropertyChanged("ActiveRalay");
            }
        }


        private int passiveDetectTime;
        public int PassiveDetectTime
        {
            get => passiveDetectTime;
            set
            {
                passiveDetectTime = value;
                RaisePropertyChanged("PassiveDetectTime");
            }
        }


        private int activeDetectTime;
        public int ActiveDetectTime
        {
            get => activeDetectTime;
            set
            {
                activeDetectTime = value;
                RaisePropertyChanged("ActiveDetectTime");
            }
        }
        #endregion

        #region RelayCommand
        public RelayCommand Write
        {
            get => new RelayCommand(SaveData);

        }

        public RelayCommand Initial
        {
            get => new RelayCommand(LoadData);

        }
        #endregion

        #region Method

        private void SaveData()
        {
         
            List<byte> cmdp = new List<byte> { 0xFE, 0xEF, 0x30, 0x03, };

            Messenger.Default.Send<byte[]>(new byte[4] , "Send");
            if (!Directory.Exists("ConfigJson"))
            {
                Directory.CreateDirectory("ConfigJson");
            }
            string fp = AppDomain.CurrentDomain.BaseDirectory + "\\ConfigJson\\";
            AddressInfos["Passive"] = Passive;
            AddressInfos["LED"] = LED;
            AddressInfos["ActiveDetect"] = ActiveDetect;
            AddressInfos["ActiveControl"] = ActiveControl;
            AddressInfos["ActiveRelay"] = ActiveRalay;
            var parameter = new { Infos = AddressInfos, PassiveTime = passiveDetectTime };
            File.WriteAllText(fp + "parameters.json", JsonConvert.SerializeObject(parameter, new JsonSerializerSettings() { StringEscapeHandling = StringEscapeHandling.Default }));
            MessageBox.Show("保存成功！");
        }
        private void LoadData()
        {

            string fp = AppDomain.CurrentDomain.BaseDirectory + "\\ConfigJson\\";
            var definition = new { Infos = new Dictionary<string, AddressInfo>(), ATime = 1, PTime = 2 };
            var json1 = File.ReadAllText(fp + "parameters.json");
            var param = JsonConvert.DeserializeAnonymousType(json1, definition);
            Passive = param.Infos["Passive"];
            LED = param.Infos["LED"];
            ActiveDetect = param.Infos["ActiveDetect"];
            ActiveControl = param.Infos["ActiveControl"];
            ActiveRalay = param.Infos["ActiveRelay"];
            ActiveDetectTime = param.ATime;
            PassiveDetectTime = param.PTime;
        }
        #endregion

    }
}
