using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiringHarnessDetect.Model
{
    public class LabelInfo
    {
        public string WireName { get; set; }

        public string PartNum { get; set; }

        public String DetectTime { get; set; } = DateTime.Now.ToString("yyyy/MM/dd HH:mm");

        public String RunningNumber  { get; set; }

        public string OperatorNum { get; set; }

        public string Result { get; set; }
    }
}
