using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiringHarnessDetect.Model
{
    public class CarProject:ValidateModelBase
    {
        //车型名称
        private string carName;
        public string CarName
        {
            get => carName;
            set
            {
                carName = value;
                RaisePropertyChanged("CarName");
            }
        }


        //车型ID
        private string carNO;

        public string CarNO
        {
            get => carNO;
            set
            {
                carNO = value;
                RaisePropertyChanged("CarNO");
            }
        }

        //修改日期
        public DateTime ModifyDate { get; set; } = DateTime.Now;

        //创建人
        public string Author { get; set; }

    }
}
