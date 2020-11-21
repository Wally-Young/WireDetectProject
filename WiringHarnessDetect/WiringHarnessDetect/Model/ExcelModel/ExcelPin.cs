using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiringHarnessDetect.Model
{
    public class ExcelPin : ValidateModelBase
    {
        //治具型号
        private string fixtureType;
        public string FixtureType
        {

            get => fixtureType;
            set
            {
                fixtureType = value;
                RaisePropertyChanged("FixtureType");
            }
        }

        //引脚编号
        private string pinNO;
        public string PinNO
        {

            get => pinNO;
            set
            {
                pinNO = value;
                RaisePropertyChanged("PinNO");
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

        public double PinSX { get; set; }//坐标X

        public double PinSY { get; set; }//坐标Y

        public double PinEX { get; set; }//画布宽度X

        public double PinEY { get; set; }//画布高度Y

        public double Radius { get; set; } //圆圈半径

    }
}
