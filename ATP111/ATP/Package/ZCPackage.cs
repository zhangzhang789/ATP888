using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Package
{
    class ZCPackage
    {


        UInt16 cycle_;
       public UInt16 Cycle { set { cycle_ = value; } }

        UInt16 type_;
        public UInt16 PackageType { set { type_ = value; } }

        Byte sendID_;
        public Byte SendID { set { sendID_ = value; } }

        Byte doorState_;
        public Byte DoorState_ { set { doorState_ = value; } }



        Byte receiveID_;
        public Byte ReceiveID { set { receiveID_ = value; } }

        UInt16 length_;
        public UInt16 Length { set { length_ = value; } }

        UInt16 trainID_;
        public UInt16 TrainID { set { trainID_ = value; } }

        UInt16 zcID_;
        public UInt16 ZCID { set { zcID_ = value; } }

        Byte runInformation_;
        public Byte RunInformation { set { runInformation_ = value; } }

        Byte stopFlag_;
        public Byte StopFlag { set { stopFlag_ = value; } }

        UInt16 stopMALink_;
        public UInt16 StopMAlink { set { stopMALink_ = value; } }

        UInt32 stopMAOff_;
        public UInt32 StopMAOfff { set { stopMAOff_ = value; } }

        Byte headSectionOrSwitch_;
        public Byte HeadSectionOrSwitch { get { return headSectionOrSwitch_; } set { headSectionOrSwitch_ = value; } }

        Byte headID_;
        public Byte HeadID { get { return headID_; }  set { headID_ = value; } }   //拆开的两个

        UInt32 headOff_;
        public UInt32 HeadOff { set { headOff_ = value; } }

        Byte tailSectionOrSwitch_;
        public Byte TailSectionOrSwitch { set { tailSectionOrSwitch_ = value; } }

        Byte tailID_;
        public Byte TailID { set { tailID_ = value; } }   //拆开的两个

        UInt32 tailOff_;
        public UInt32 TailOff { set { tailOff_ = value; } }

        Byte headExpDirection_;
        public Byte HeadExpDirection { set { headExpDirection_ = value; } }

        Byte headActDirection_;
        public Byte HeadActDirection { set { headActDirection_ = value; } }

        Byte runModel_;
        public Byte RunModel { set { runModel_ = value; } }

        Byte runLevel_;
        public Byte RunLevel { set { runLevel_ = value; } }

        UInt16 actSpeed_;
        public UInt16 ACtSpeed { set { actSpeed_ = value; } }

        Byte stopInfo_;
        public Byte StopInfo { set { stopInfo_ = value; } }

        UInt16 error_;
        public UInt16 Error { set { error_ = value; } }

        UInt16 back_;
        public UInt16 Back { set { back_ = value; } }

        UInt16 limitSpeed_;
        public UInt16 LimitSpeed { set { limitSpeed_ = value; } }

        Byte integrity_;
        public Byte Integrity { set { integrity_ = value; } }

        Byte emergenvy_;
        public Byte Emergenvy { set { emergenvy_ = value; } }

        UInt16 arlamp_;
        public UInt16 Arlamp { set { arlamp_ = value; } }

        Byte arlampCmd_;
        public Byte ArlampCmd { set { arlampCmd_ = value; } }

        Byte mode_;
        public Byte Mode { set { mode_ = value; } }

        UInt32 vobc_;
        public UInt32 VOBC { set { vobc_ = value; } }

        UInt16 controlZC_;
        public UInt16 ControlZC { set { controlZC_ = value; } }

        UInt32 sendTime_;
        public UInt32 SendTime { set { sendTime_ = value; } }

        UInt32 reserved_;
        public UInt32 Reserved { set { reserved_ = value; } }


        public byte[] ZCPackStream()
        {
            byte[] ZCSendData = new byte[1024];
            Stream sendStream = new MemoryStream(ZCSendData);
            BinaryWriter ZCPackageStream = new BinaryWriter(sendStream);
            ZCPackageStream.Write((UInt16)0); //包头
            ZCPackageStream.Write(type_);
            ZCPackageStream.Write(sendID_);
            ZCPackageStream.Write(receiveID_);
            ZCPackageStream.Write(length_);
            ZCPackageStream.Write(mode_);
            ZCPackageStream.Write(trainID_);
            ZCPackageStream.Write(zcID_);
            ZCPackageStream.Write(runInformation_);
            ZCPackageStream.Write(stopFlag_);
            ZCPackageStream.Write(stopMALink_);
            ZCPackageStream.Write(stopMAOff_);
            ZCPackageStream.Write(headSectionOrSwitch_);
            ZCPackageStream.Write(headID_);
            ZCPackageStream.Write(headOff_);
            ZCPackageStream.Write(tailSectionOrSwitch_);
            ZCPackageStream.Write(tailID_);
            ZCPackageStream.Write(tailOff_);
            ZCPackageStream.Write(headExpDirection_);
            ZCPackageStream.Write(headActDirection_);
            ZCPackageStream.Write(runModel_);
            ZCPackageStream.Write(runLevel_);
            ZCPackageStream.Write(actSpeed_);
            ZCPackageStream.Write(doorState_);
            ZCPackageStream.Write(stopInfo_);
            ZCPackageStream.Write(error_);
            ZCPackageStream.Write(back_);
            ZCPackageStream.Write(limitSpeed_);
            ZCPackageStream.Write(integrity_);
            ZCPackageStream.Write(emergenvy_);
            ZCPackageStream.Write(arlamp_);
            ZCPackageStream.Write(arlampCmd_);
            ZCPackageStream.Write(vobc_);
            ZCPackageStream.Write(controlZC_);
            ZCPackageStream.Write(sendTime_);
            ZCPackageStream.Write(reserved_);

            return ZCSendData;
        }
    }
}
