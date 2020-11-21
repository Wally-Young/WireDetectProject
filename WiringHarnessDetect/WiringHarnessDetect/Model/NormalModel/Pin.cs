using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiringHarnessDetect.Model
{
    public class Pin : ValidateModelBase
    {
        //项目编号
        private string projectNO;
        [Required]
        public string ProjectNO
        {
            get => projectNO;
            set
            {
                projectNO = value;
                RaisePropertyChanged("ProjectNO");
            }
        }

        //治具编号
        private string fixtureCode;
        public string FixtureCode
        {

            get => fixtureCode;
            set
            {
                fixtureCode = value;
                RaisePropertyChanged("FixtureCode");
            }
        }

        private bool isSafeBox;
        public bool IsSafeBox
        {
            get => isSafeBox;
            set
            {
                isSafeBox = value;
                RaisePropertyChanged("IsSafeBox");
            }
        }



        //引脚编号
        private string pinCode;

        public string PinCode
        {

            get => pinCode;
            set
            {
                pinCode = value;
                RaisePropertyChanged("PinCode");
            }
        }

        //绘图颜色
        private string drawColor;
        public string DrawColor
        {
            get => drawColor;
            set
            {
                drawColor = value;
                RaisePropertyChanged("DrawColor");
            }
        }



        //继电器名称
        public string safeBoxName;
        public string SafeBoxName
        {
            get => safeBoxName;
            set
            {
                safeBoxName = value;
                RaisePropertyChanged("SafeBoxName");
            }
        }

        //物理地址
        public int physicalChannel;
        public int PhysicalChannel
        {
            get => physicalChannel;
            set
            {
                physicalChannel = value;
                RaisePropertyChanged("PhysicalChannel");
            }
        }

        //控制引脚
        public int relayControlChannel;
        public int RelayControlChannel
        {
            get => relayControlChannel;
            set
            {
                relayControlChannel = value;
                RaisePropertyChanged("RelayControlChannel");
            }
        }

        public double PinSX { get; set; }//坐标X

        public double PinSY { get; set; }//坐标Y

        public double PinEX { get; set; }//画布宽度X

        public double PinEY { get; set; }//画布高度Y

        public double Radius { get; set; } //圆圈半径
    }

    public class PinEx : Pin
    {
        private string projectName;
        public string ProjectName
        {
            get => projectName;
            set
            {
                projectName = value;
                RaisePropertyChanged("ProjectName");
            }
        }



        private string fixtureName;
        public string FixtureName
        {
            get => fixtureName;
            set
            {
                fixtureName = value;
                RaisePropertyChanged("FixtureName");
            }
        }

    }
}
