using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiringHarnessDetect.Model
{

    public class ConnectionInfo : ValidateModelBase
    {
        //工程名称
        public string ProjectNO { get; set; }
        //线束号
        public string WireNum { get; set; }
        //回路信息
        public string CirCuitName { get; set; }
        //治具编号
        public string FixtureNO { get; set; }
        //治具型号
        public string FixtureType { get; set; }
        //引脚编号
        public int PinIndex { get; set; }
        //引脚编号
        public string PinNO { get; set; }
        //物理地址
        //[NotMapped]
        public int PhysicalChannel { get; set; }

        //[NotMapped]
        public string Coordinate { get; set; }

        public override bool Equals(object obj)
        {

            ConnectionInfo td = obj as ConnectionInfo;

            if (td == null)

                return false;

            return PhysicalChannel == td.PhysicalChannel ;
        }

        public override int GetHashCode()
        {
            return PhysicalChannel.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{PhysicalChannel}(XY:{Coordinate},F:{FixtureType}->{PinNO}," +
                $"V:{FixtureNO}->{PinIndex})");
            return sb.ToString();

        }


    }

    public class ConnectionFixtures
    {
        public string FixtureType { get; set; }
      
        //引脚编号
        public string PinNO { get; set; }
        //物理地址
        //[NotMapped]
        public int PhysicalChannel { get; set; }

        public string Coordinate { get; set; }
    }
        


    public class BasePin
    {
        public string FixtureNO { get; set; }
        public int PinIndex { get; set; }
        public int PhysicalChannel { get; set; }


    }

    public class WireCircuit
    {
        public string WireNum { get; set; }

        public string DislayMsg { get; set; }

        public List<ConnectionInfo> Connects { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
           
            foreach (var item in Connects)
            {
                sb.Append($"{item.Coordinate}:({item.FixtureType},{item.PinNO})[{item.PhysicalChannel}]({item.FixtureNO},{item.PinIndex})\n");
                //sb.Append($"{item.PhysicalChannel}(XY:{item.Coordinate},F:{item.FixtureType}->{item.PinNO},V:{item.FixtureNO}->{item.PinIndex})\n");
                sb.Append("\n");
            }
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();


        }

        public override bool Equals(object obj)
        {
            if (obj is WireCircuit)
            {
                WireCircuit data = obj as WireCircuit;
                return this.WireNum==data.WireNum&& Connects.Count == data.Connects.Count && Connects.Count(t => !data.Connects.Contains(t)) == 0;
                
            }
            else
            {
                return false;
            }

        }

        public override int GetHashCode()
        {
            return Connects.GetHashCode();
        }
    }


    public class ErrorWire 
    {
        public string WireNum { get; set; }

        public string DislayMsg { get; set; }

        public List<ConnectionInfo> CorrectConnects { get; set; }

        public List<ConnectionInfo> ErrorConnects { get; set; }

       
    }
    public class ConnectInfoEx
    {
        public string WireNum { get; set; }
        public string FixtureType { get; set; }
        public string FixtureNO { get; set; }
        public string ProjectNO { get; set; }
        public string CirCuitName { get; set; }
        public List<BasePin> Pins { get; set; }
    }

    public class CompareConnectInfo : IEqualityComparer<ConnectionInfo>
    {
        public bool Equals(ConnectionInfo x, ConnectionInfo y)
        {
            return x.PhysicalChannel == y.PhysicalChannel&&x.PinIndex==y.PinIndex;
        }

        public int GetHashCode(ConnectionInfo p)
        {
            if (p == null)
                return 0;
            return p.PhysicalChannel.GetHashCode();
        }
    }

    public class CompareWireCuit : IEqualityComparer<WireCircuit>
    {
        public bool Equals(WireCircuit x, WireCircuit y)
        {
            return x.Connects.Count == y.Connects.Count && x.Connects.Count(t => !y.Connects.Contains(t)) == 0;

        }

        public int GetHashCode(WireCircuit p)
        {
            if (p == null)
                return 0;
            return p.Connects.GetHashCode();
        }
    }

    public class BaseData
    {
        public int Channel { get; set; }
        public List<int>  Connects { get; set; }



        public override bool Equals(object obj)
        {
            if(obj is BaseData)
            {
               BaseData data=   obj as BaseData;
                return Connects.All(data.Connects.Contains) && Connects.Count == data.Connects.Count;
            }
           else
            {
                return false;
            }
        
        }

        public override int GetHashCode()
        {
            return Connects.Count.GetHashCode();
        }
    }

}



