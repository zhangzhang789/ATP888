using System;
using System.IO;

namespace Package
{
    class ATPCurvePackage     //将所有定义的属性都定义在这个类下面，方便调用，而不是结构体
    {
        int MAEndDistance_;
        public int MAEndDistance { set { MAEndDistance_ = value; } }

        int limSpeedNum_;
        public int limSpeedNum { set { limSpeedNum_ = value; } }

        public int[] limSpeedDistance { get; } = new int[4];
        public int[] limSpeedLength { get; } = new int[4];

        string faultPostion_;  //故障位置
        public string faultPostion { set { faultPostion_ = value; } }

        string faultReason_;   //故障原因
        public string faultReason { set { faultReason_ = value; } }

        int totalEnergy_;   //总能耗
        public int totalEnergy { set { totalEnergy_ = value; } }

        int Comfort_;   //不适度
        public int Comfort { set { Comfort_ = value; } }



        public int ATPCurvePackStream(byte[] ATPCurveSendData)
        {
            using (Stream sendStream = new MemoryStream(ATPCurveSendData))
            using (BinaryWriter ATPCurvPackageStream = new BinaryWriter(sendStream))
            {
                ATPCurvPackageStream.Write(MAEndDistance_);
                ATPCurvPackageStream.Write(limSpeedNum_);
                for (int i = 0; i < 4; i++)
                {
                    ATPCurvPackageStream.Write(limSpeedDistance[i]);
                    ATPCurvPackageStream.Write(limSpeedLength[i]);
                }
                ATPCurvPackageStream.Write(faultPostion_);
                ATPCurvPackageStream.Write(faultReason_);
                ATPCurvPackageStream.Write(totalEnergy_);
                ATPCurvPackageStream.Write(Comfort_);
                return (int)ATPCurvPackageStream.BaseStream.Position;
            }

            
        }
    }
}
