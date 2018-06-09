using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBTC
{
    class ATSPackage
    {
        UInt16 trainID_;
        public UInt16 TrainID_ { set { trainID_ = value; } }

        public BinaryWriter ATSPackStream(BinaryWriter ATSPackageStream)
        {
            ///等待填写
            return ATSPackageStream;
        }
    }
}
