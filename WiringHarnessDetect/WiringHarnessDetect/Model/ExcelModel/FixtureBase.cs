using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiringHarnessDetect.Model
{
    public class FixtureBase:ValidateModelBase
    {
        //治具型号
        public string FixtureType{ get; set; }

        //图纸
        public string ImagePath { get; set; }

        //坐标值
        public string Coordinate { get; set; }

        //LED地址
        public int  LEDAddress { get; set; }

        

    }

    public class FixtureInfo
    {
        //治具型号
        public string FixtureType { get; set; }

        //治具编号
        public string FixtureNO { get; set; }

        //工程编号
        public string ProjectNO { get; set; }

        //引脚索引
        public int PinIndex { get; set; }

        //引脚编号
        public string PinNO { get; set; }

        //物理地址
        public int PhysicalChannel { get; set; }
    }
}
