using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiringHarnessDetect.Model
{
    public class WireType:ValidateModelBase
    {
        //线束名称
        private string wireName;
        public string WireName
        {
            get => wireName;
            set
            {
                wireName = value;
                RaisePropertyChanged("WireName");
            }
        }


        //线束编号
        private string wireNO;
        public string WireNO
        {
            get => wireNO;
            set
            {
                wireNO = value;
                RaisePropertyChanged("WireNO");
            }
        }


        //修改日期
        public DateTime ModifyDate { get; set; } = DateTime.Now;

        //创建人
        public string Author { get; set; }
    }
}
