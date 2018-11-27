using Package;
using SocketSearch;
using System;

namespace ATP.SocketSearch
{
    enum DMIBreakOut
    {
        EB=6,
        NoEB=7
    }
    class Atp2Dmi : Atp2OtherSystem
    {
        public delegate UInt16 ComputeProtectSpeed(int MAEndDistance, int limSpeedNum, int limSpeedDistance_1);

        public DMIPackage dmiPackage = new DMIPackage() { PackageType = 3, ActulSpeed = 25, TrainNum = "" };
        
        public override void Initialize()
        {
            CreateSocket("DMI");
        }

        public void SendDMI(int trainID, DcInfo dcInfo, bool isEB, 
            ComputeProtectSpeed ProtectSpeed, SpeedLimit speedLimit,
            bool DMIShow, bool isRealeaseEB, bool isSendZCBool,
            bool isInFault)    //发送DMI消息
        {
            dmiPackage.TrainID = 65536;
            dmiPackage.TrainNum = "T0" + trainID.ToString();
            dmiPackage.HighModel = 1;
            dmiPackage.CurModel = (byte)dcInfo.DCCtrlMode;
            dmiPackage.ActulSpeed = (UInt16)Math.Abs(dcInfo.DCTrainSpeed);
            dmiPackage.BreakOut = (byte)DMIBreakOut.NoEB;
            dmiPackage.Alarm = 1;

            if (isEB == false)
            {
                UInt16 HighSpeed = (UInt16)ProtectSpeed(
                    speedLimit.MAEndDistance, speedLimit.limSpeedNum, speedLimit.limSpeedDistance[0]);
                dmiPackage.HighSpeed =HighSpeed;    //目前得不到速度信息
                dmiPackage.PermitSpeed = (UInt16)(HighSpeed -5);
                dmiPackage.FrontPermSpeed = (UInt16)(HighSpeed - 2);
                dmiPackage.TargetLoca = (UInt16)speedLimit.MAEndDistance;
            }
            else
            {
                dmiPackage.HighSpeed = 0;    //目前得不到速度信息
                dmiPackage.PermitSpeed = 0;
                dmiPackage.FrontPermSpeed = 0;
                dmiPackage.TargetLoca = 0;
            }

            if (isEB == true)
            {
                dmiPackage.BreakOut = (byte)DMIBreakOut.EB;
                dmiPackage.Alarm = 1;
            }

            dmiPackage.Dmishow = (UInt16)(DMIShow ? 2 : 1);
            
            if (!dcInfo.IsCurBaliseEmpty())
            {
                dmiPackage.IsNoZHG = (UInt16)(dcInfo.IsCurStartWith("ZHG")? 0 : 1);
            }

            dmiPackage.IsRealeaseEB = (UInt16)(isRealeaseEB ? 2 : 1);
            dmiPackage.IsCBTC = (UInt16)(isSendZCBool ? 0 : 1);
            
            if (isInFault == true)
            {
                dmiPackage.FaultType = 2;
            }
            else if (isSendZCBool == false)
            {
                dmiPackage.FaultType = 3;
            }
            else
            {
                dmiPackage.FaultType = 1;
            }
            
            byte[] DMISendData = dmiPackage.DMIPackStream();
            client.Send(DMISendData, 1024);
        }
    }
}
