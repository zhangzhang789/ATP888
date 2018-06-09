using System;
using System.IO;

namespace CBTC
{
    
    class DMIPackage
    {

        UInt16 cycle_;
        public UInt16 Cycle { set { cycle_ = value; } }

        UInt16 type_;
        public UInt16 PackageType { set { type_ = value; } }

        UInt16 length_;
        public UInt16 Length { set { length_ = value; } }

        String trainNum_;
        public string TrainNum { get { return trainNum_; } set { trainNum_ = value; } }

        UInt32 trainID_;
        public UInt32 TrainID { set { trainID_ = value; } }

        Byte highModel_;
        public Byte HighModel { set { highModel_ = value; } }

        Byte curModel_;
        public Byte CurModel { set { curModel_ = value; } }

        Byte curRate_;
        public Byte CurRate { set { curRate_ = value; } }

        Byte breakOut_;
        public Byte BreakOut { set { breakOut_ = value; } }

        UInt32 trainStation_;
        public UInt32 TrainStation { set { trainStation_ = value; } }

        UInt32 trainHeadLoca_;
        public UInt32 TrainHeadLoca { set { trainHeadLoca_ = value; } }

        UInt16 targetLoca_;
        public UInt16 TargetLoca { set { targetLoca_ = value; } }

        UInt16 startLoca_;
        public UInt16 StartLoca {  set { startLoca_ = value; } }

        public UInt16 actulSpeed_;
        public UInt16 ActulSpeed {  set { actulSpeed_ = value; } }   //因为要访问，改成公共的

        UInt16 highSpeed_;
        public UInt16 HighSpeed { set { highSpeed_ = value; } }

        UInt16 openSpeed_;
        public UInt16 OpenSpeed { set { openSpeed_ = value; } }

        UInt16 permitSpeed_;
        public UInt16 PermitSpeed { set { permitSpeed_ = value; } }

        UInt16 interSpeed_;
        public UInt16 InterSpeed { set { interSpeed_ = value; } }

        UInt16 targetSpeed_;
        public UInt16 TargetSpeed { set { targetSpeed_ = value; } }

        Byte alarm_;
        public Byte Alarm { set { alarm_ = value; } }

        UInt16 tempLimitSpeedStart1_;
        public UInt16 TempLimitSpeedStart1 { set { tempLimitSpeedStart1_ = value; } }

        UInt16 tempLimitSpeedEnd1_;
        public UInt16 TempLimitSpeedEnd1 { set { tempLimitSpeedEnd1_ = value; } }

        UInt16 tempLimitSpeed1_;
        public UInt16 TempLimitSpeed1 { set { tempLimitSpeed1_ = value; } }

        UInt16 tempLimitSpeedStart2_;
        public UInt16 TempLimitSpeedStart2 { set { tempLimitSpeedStart2_ = value; } }

        UInt16 tempLimitSpeedEnd2_;
        public UInt16 TempLimitSpeedEnd2 { set { tempLimitSpeedEnd2_ = value; } }

        UInt16 tempLimitSpeed2_;
        public UInt16 TempLimitSpeed2 { set { tempLimitSpeed2_ = value; } }

        UInt16 tempLimitSpeedStart3_;
        public UInt16 TempLimitSpeedStart3 { set { tempLimitSpeedStart3_ = value; } }

        UInt16 tempLimitSpeedEnd3_;
        public UInt16 TempLimitSpeedEnd3 { set { tempLimitSpeedEnd3_ = value; } }

        UInt16 tempLimitSpeed3_;
        public UInt16 TempLimitSpeed3 { set { tempLimitSpeed3_ = value; } }

        Byte runLocation_;
        public Byte RunLocation { set { runLocation_ = value; } }

        Byte runDirection_;
        public Byte RunDirection { set { runDirection_ = value; } }

        Byte hint_;
        public Byte Hint { set { hint_ = value; } }

        UInt16 frontPermSpeed_;
        public UInt16 FrontPermSpeed { set { frontPermSpeed_ = value; } }

        UInt16 isCBTC_;
        public UInt16 IsCBTC { set { isCBTC_ = value; } }

        UInt16 faultType_;
        public UInt16 FaultType { set { faultType_ = value; } }

        UInt16 isSendTrain_;
        public UInt16 IsSendTrain { set { isSendTrain_ = value; } }

        UInt16 isNoZHG_;
        public UInt16 IsNoZHG { set { isNoZHG_ = value; } }

        UInt16 isSendZC_;
        public UInt16 IsSendZC { set { isSendZC_ = value; } }

        UInt16 dmishow_;
        public UInt16 Dmishow { set { dmishow_ = value; } }

        UInt16 isRealeaseEB_;
        public UInt16 IsRealeaseEB { set { isRealeaseEB_ = value; } }


        public BinaryWriter DMIPackStream(BinaryWriter DMIPackageStream)
        {
            DMIPackageStream.Write((UInt16)0);
            DMIPackageStream.Write(type_);
            DMIPackageStream.Write(length_);
            DMIPackageStream.Write(trainNum_);
            DMIPackageStream.Write(trainID_);
            DMIPackageStream.Write(highModel_);
            DMIPackageStream.Write(curModel_);
            DMIPackageStream.Write(curRate_);
            DMIPackageStream.Write(breakOut_);
            DMIPackageStream.Write(trainStation_);
            DMIPackageStream.Write(trainHeadLoca_);
            DMIPackageStream.Write(targetLoca_);
            DMIPackageStream.Write(startLoca_);
            DMIPackageStream.Write(actulSpeed_);
            DMIPackageStream.Write(highSpeed_);
            DMIPackageStream.Write(openSpeed_);
            DMIPackageStream.Write(permitSpeed_);
            DMIPackageStream.Write(interSpeed_);
            DMIPackageStream.Write(targetSpeed_);
            DMIPackageStream.Write(alarm_);
            DMIPackageStream.Write(tempLimitSpeedStart1_);
            DMIPackageStream.Write(tempLimitSpeedEnd1_);
            DMIPackageStream.Write(tempLimitSpeed1_);
            DMIPackageStream.Write(tempLimitSpeedStart2_);
            DMIPackageStream.Write(tempLimitSpeedEnd2_);
            DMIPackageStream.Write(tempLimitSpeed2_);
            DMIPackageStream.Write(tempLimitSpeedStart3_);
            DMIPackageStream.Write(tempLimitSpeedEnd3_);
            DMIPackageStream.Write(tempLimitSpeed3_);
            DMIPackageStream.Write(runLocation_);
            DMIPackageStream.Write(runDirection_);
            DMIPackageStream.Write(hint_);
            DMIPackageStream.Write(frontPermSpeed_);
            DMIPackageStream.Write(isCBTC_);
            DMIPackageStream.Write(faultType_);
            DMIPackageStream.Write(isSendTrain_);
            DMIPackageStream.Write(isNoZHG_);
            DMIPackageStream.Write(isSendZC_);
            DMIPackageStream.Write(dmishow_);
            DMIPackageStream.Write(isRealeaseEB_);

            return DMIPackageStream;
        }
    }
}
