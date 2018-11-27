using Package;
using System;
using System.Collections.Generic;
using System.Text;

namespace ATP.SocketSearch
{
    class Atp2Dc : Atp2OtherSystem
    {
        public DCPackage dcPackage = new DCPackage() { PackageType = 5 };

        public override void Initialize()
        {
            CreateSocket("DC");
        }

        public void SendDC(bool isEB)
        {
            if (isEB == true)
            {
                dcPackage.IsEB = 6; //收到缓解后已经赋值7
            }

            byte[] DCSendData = dcPackage.DCPackStream();
            client.Send(DCSendData, 1024);
        }
    }
}
