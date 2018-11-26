﻿using Package;
using SocketSearch;
using System;

namespace ATP.SocketSearch
{
    class Atp2Zc : Atp2OtherSystem
    {
        public ZCPackage zcPackage = new ZCPackage() { PackageType = 8, ReceiveID = 3, ZCID = 3 };

        public override void Initialize()
        {
            CreateSocket("ZC");
        }

        public void SendZC(DcInfo dcInfo, int sendID, int trainID, ModelType curModel) //测完
        {
            if (!dcInfo.IsCurStartWith("Z") && !dcInfo.IsCurBaliseEmpty())
            {
                zcPackage.SendID = (byte)sendID;
                zcPackage.TrainID = (UInt16)trainID;
                zcPackage.RunInformation = 0x01;
                zcPackage.TailSectionOrSwitch = zcPackage.HeadSectionOrSwitch;
                zcPackage.TailID = zcPackage.HeadID;
                //zcPackage.TailOff = zcPackage.HeadOff; //大写的是自己找的                                            
                zcPackage.ACtSpeed = (UInt16)Math.Abs(dcInfo.DCTrainSpeed);
                zcPackage.Mode = (byte)curModel;

                byte[] ZCSendData = zcPackage.ZCPackStream();
                client.Send(ZCSendData, 1024);
            }
        }
    }
}