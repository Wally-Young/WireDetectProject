using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiringHarnessDetect.Model
{
    public class JKSampleData
    {
        public string ProjectNO { get; set; }

        public int DetectionStepCode { get; set; }

        public int PinVoltageCode { get; set; }

        public string PinNO { get; set; }

        public string DeviceName { get; set; } = "";

        public int PhysicalChannel { get; set; }

        public string FixtureName { get; set; } = "";

        public string FixtureCode { get; set; }
    }

    public class CompareJKData : IEqualityComparer<JKSampleData>
    {
        public bool Equals(JKSampleData x, JKSampleData y)
        {
            return (x.ProjectNO == y.ProjectNO) && (x.PinVoltageCode == y.PinVoltageCode) &&
                 (x.PhysicalChannel == y.PhysicalChannel) && (x.DeviceName == y.DeviceName) && (x.DetectionStepCode == y.DetectionStepCode);
        }

        public int GetHashCode(JKSampleData p)
        {
            if (p == null)
                return 0;
            return p.ProjectNO.GetHashCode() | p.DetectionStepCode.GetHashCode() | p.PinVoltageCode.GetHashCode() | p.PhysicalChannel.GetHashCode() | p.DeviceName.GetHashCode();
        }
    }

    public class CompareJKDataJK : IEqualityComparer<JKSampleData>
    {
        public bool Equals(JKSampleData x, JKSampleData y)
        {
            return (x.ProjectNO == y.ProjectNO) && (x.PinVoltageCode == y.PinVoltageCode) &&
                 (x.PhysicalChannel == y.PhysicalChannel) && (x.DetectionStepCode == y.DetectionStepCode);
        }

        public int GetHashCode(JKSampleData p)
        {
            if (p == null)
                return 0;
            return p.ProjectNO.GetHashCode() | p.DetectionStepCode.GetHashCode() | p.PinVoltageCode.GetHashCode() | p.PhysicalChannel.GetHashCode();
        }
    }
}
