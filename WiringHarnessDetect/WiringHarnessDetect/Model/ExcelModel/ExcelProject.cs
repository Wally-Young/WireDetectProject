using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiringHarnessDetect.Model
{
    public class ExcelProject
    {
        //车型编号
        public string CarNO { get; set; }

        //线束编号
        public string WireNO { get; set; }

        //工程名称
        public string ProjectName { get; set; }

        //工程编号
        public string ProjectNO { get; set; }

        //创建者
        public string Author { get; set; }

        //修改日期
        public DateTime ModifyDate { get; set; } = DateTime.Now;
    }
}
