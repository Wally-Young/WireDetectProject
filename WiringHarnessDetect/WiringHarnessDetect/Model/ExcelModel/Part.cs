using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiringHarnessDetect.Model
{ 
    public class PartEx: ValidateModelBase
    {

        //工程编号
        private string projectNO;
        public string ProjectNO
        {
            get => projectNO;
            set
            {
                projectNO = value;
                RaisePropertyChanged("ProjectNO");
            }
        }

        //零件类型
        private string partType;
        public string PartType
        {
            get => partType;
            set
            {
                partType = value;
                RaisePropertyChanged("PartType");
            }
        }       
         
        //回路信息
        private List<string> circuits;
        public List<string> Circuits
        {
            get => circuits;
            set
            {
                circuits = value;
                RaisePropertyChanged("Circuits");
            }
        }

    }

    public  class Part
    {
        public string ProjectNO { get; set; }

        public string PartType { get; set; }

        public string Circuit { get; set; }

        public string Author { get; set; }

        public DateTime UpdateTime { get; set; }
    }
  
}
