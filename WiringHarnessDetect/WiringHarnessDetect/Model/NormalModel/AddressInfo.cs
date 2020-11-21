using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiringHarnessDetect.Model
{
    public  class AddressInfo:ValidateModelBase
    {

        //头板卡组号
        private int startGroupNum;

        public int StartGroupNum
        {
            get => startGroupNum;
            set 
            {
                startGroupNum = value;
                RaisePropertyChanged("StartGroupNum");
            }
        }


        //尾板卡组号
        private int endGroupNum;
        public int EndGroupNum
        {
            get =>endGroupNum;
            set
            {
                endGroupNum = value;
                RaisePropertyChanged("EndGroupNum");
            }
        }


        //头板卡号
        private int startBoardNum;

        public int StartBoradNum
        {
            get => startBoardNum;
            set
            {
                startBoardNum = value;
                RaisePropertyChanged("StartBoradNum");
            }
        }

        //尾板卡号
        private int endBoardNum;

        public int EndBoradNum
        {
            get => endBoardNum;
            set
            {
                endBoardNum = value;
                RaisePropertyChanged("EndBoradNum");
            }
        }


        //头物理地址
        private int startAddress;

        public int StartAddress
        {
            get => startAddress;
            set
            {
                startAddress = value;
                RaisePropertyChanged("StartAddress");
            }
        }

        //尾物理地址
        private int endAddress;

        public int EndAddress
        {
            get => endAddress;
            set
            {
                endAddress = value;
                RaisePropertyChanged("EndAddress");
            }
        }
    }
}
