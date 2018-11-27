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

        public int ATSPackStream(byte[] ATSSendData)
        {
      
            using (Stream sendStream = new MemoryStream(ATSSendData))
            using (BinaryWriter ATSPackageStream = new BinaryWriter(sendStream))
            {
                return (int)ATSPackageStream.BaseStream.Position;
            }
            
        }
    }
}
