using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiringHarnessDetect.Model
{
    //治具连接关系的Pin
    public class FixtureLED:ValidateModelBase
    {
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

        private int LedAddress;
        public int LEDAddress
        {
            get => LedAddress;
            set
            {
                RaisePropertyChanged("LEDAddress");
            }
        }

        private string imagepath;
        public string ImagePath
        {
            get => imagepath;
            set
            {
                imagepath = value;
                RaisePropertyChanged("ImagePath");

            }
        }

    }
     
    //连接关系表里的治具实体
    public class FixtureEx
    {
        public string ProjectNO { get; set; }

        public string FixtureName { get; set; }

        public string FixtureCode { get; set; }

        public int PinIndex { get; set; }

        public int PhysicalChannel { get; set; }
    }

    //显示名称的Fixtures
    public class FixtureDisplay
    {
        public string ProjectNO { get; set; }
        public string FixtureName { get; set; }
        public string FixtureCode { get; set; }
        public List<ExcelPin> Pins { get; set; }
    }

   
}
