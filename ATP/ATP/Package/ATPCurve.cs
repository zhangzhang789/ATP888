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

        int limSpeedDistance_1_;
        public int limSpeedDistance_1 { set { limSpeedDistance_1_ = value; } }

        int limSpeedLength_1_;
        public int limSpeedLength_1 { set { limSpeedLength_1_ = value; } }

        int limSpeedDistance_2_;
        public int limSpeedDistance_2 { set { limSpeedDistance_2_ = value; } }

        int limSpeedLength_2_;
        public int limSpeedLength_2 { set { limSpeedLength_2_ = value; } }

        int limSpeedDistance_3_;
        public int limSpeedDistance_3 { set { limSpeedDistance_3_ = value; } }

        int limSpeedLength_3_;
        public int limSpeedLength_3 { set { limSpeedLength_3_ = value; } }

        int limSpeedDistance_4_;
        public int limSpeedDistance_4 { set { limSpeedDistance_4_ = value; } }

        int limSpeedLength_4_;
        public int limSpeedLength_4 { set { limSpeedLength_4_ = value; } }

        string faultPostion_;  //故障位置
        public string faultPostion { set { faultPostion_ = value; } }

        string faultReason_;   //故障原因
        public string faultReason { set { faultReason_ = value; } }

        int totalEnergy_;   //总能耗
        public int totalEnergy { set { totalEnergy_ = value; } }

        int Comfort_;   //不适度
        public int Comfort { set { Comfort_ = value; } }



        public byte[] ATPCurvePackStream()
        {
            byte[] ATPCurveSendData= new byte[1024];
            Stream sendStream = new MemoryStream(ATPCurveSendData);
            BinaryWriter ATPCurvPackageStream = new BinaryWriter(sendStream);
            ATPCurvPackageStream.Write(MAEndDistance_);
            ATPCurvPackageStream.Write(limSpeedNum_);
            ATPCurvPackageStream.Write(limSpeedDistance_1_);
            ATPCurvPackageStream.Write(limSpeedLength_1_);
            ATPCurvPackageStream.Write(limSpeedDistance_2_);
            ATPCurvPackageStream.Write(limSpeedLength_2_);
            ATPCurvPackageStream.Write(limSpeedDistance_3_);
            ATPCurvPackageStream.Write(limSpeedLength_3_);
            ATPCurvPackageStream.Write(limSpeedDistance_4_);
            ATPCurvPackageStream.Write(limSpeedLength_4_);
            ATPCurvPackageStream.Write(faultPostion_);
            ATPCurvPackageStream.Write(faultReason_);
            ATPCurvPackageStream.Write(totalEnergy_);
            ATPCurvPackageStream.Write(Comfort_);
            return ATPCurveSendData;
        }
    }
}
