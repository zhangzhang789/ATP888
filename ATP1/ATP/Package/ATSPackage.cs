using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Package
{
    class ATSPackage
    {
        UInt16 trainID_;
        public UInt16 TrainID_ { set { trainID_ = value; } }

        public byte[] ATSPackStream()
        {
            byte[] ATSSendData = new byte[1024];
            Stream sendStream = new MemoryStream(ATSSendData);
            BinaryWriter ATSPackageStream = new BinaryWriter(sendStream);
            return ATSSendData;
        }
    }
}
