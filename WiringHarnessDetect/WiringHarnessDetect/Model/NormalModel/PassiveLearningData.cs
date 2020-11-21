using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiringHarnessDetect.Model
{
    public class PassiveData
    {
        public List<ConnectionData> Datas { get; set; }

        public string ProjectNO { get; set; } 

        
    }

    public class FaultData:PassiveData
    {
        public HashSet<int> LEDs { get; set; }
    }



    public class ConnectionData
    {
        public string ProjectNO { get; set; }
        public string FixtureCode { get; set; } = "";

        public string PinCode { get; set; } = "";
        public int  ChannelNum { get; set; }

        private List<int> connects;
        public  List<int> Connects
        {   get
            {
                if (connects == null)
                {
                    connects = new List<int>();
                }
                return connects;
            }
            set
            {
                connects = value;
            }
        }
        public bool IsSingle { get; set; }
       
        public string DislayInfo { get; set; }
    }

    public class CompareConnectData : IEqualityComparer<ConnectionData>
    {
        public bool Equals(ConnectionData x, ConnectionData y)
        {
            return x.ChannelNum == y.ChannelNum&&x.Connects.Count==y.Connects.Count;
        }

        public int GetHashCode(ConnectionData p)
        {
            if (p == null)
                return 0;
            return (p.ChannelNum+p.Connects.Count).GetHashCode();
        }
    }
    public class BaseSampleData
    {
        public string ProjectNO { get; set; }
        public string FixtureCode { get; set; } = "";

        public string PinCode { get; set; } = "";
        public int ChannelNum { get; set; }

        public int ConnectTo { get; set; }
        public bool IsSingle { get; set; }
    }
    
}
