using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiringHarnessDetect.Model
{
    public class CablePaper: ValidateModelBase
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

        //治具名称
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

        //治具编号
        private string fixtureCode;
        public string   FixtureCode
        {

            get => fixtureCode;
            set
            {
                fixtureCode = value;
                RaisePropertyChanged("FixtureCode");
            }
        }

        //图纸路径
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

        //LEDAddress
        private int ledAddress=999;
        public int LEDAddress 
        {
            get => ledAddress;
            set
            {
                ledAddress = value;
                RaisePropertyChanged("LEDAddress");
            }
        }

        private int isSafeBox;

        public int  IsSafeBox
        {
            get => isSafeBox;
            set
            {
                isSafeBox = value;
                RaisePropertyChanged("IsSafeBox");
            }
        }

    }
}
