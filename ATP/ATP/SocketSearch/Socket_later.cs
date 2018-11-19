using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;
using TrainMessageEB;
using System.Net;
using System.Net.Sockets;
using ConfigData;
using System.Timers;
using Package;
using System.Diagnostics;
using CbtcData;

namespace SocketSearch
{
    enum PackageType
    {
        PackZC = 9,
        PackDMI = 4,
        PackDC = 6,
        PackBalise=5
    }

    enum ModelType
    {
        AM = 1,
        CM = 2,
        RM = 3,
        EUM =4
    }
    
    class Socket_later
    {
        
        public Socket ZCsocket;
        public bool isRecvZC;
        public bool isBaliseFirst=false;//是否最开始不起模的应答器
        public bool isLeftSearch = false;
        public byte curModel = 3; //目前的控车模式
        public Socket RecvSocket;
        public UInt16 DCCtrlMode = 0; //司控器的模式
        public string baliseHead = "";
        public string curBalise = "";
        public string baliseTail = "";
        public string trainHead = "";
        public string trainTail = "";    //都等于目前应答器
        public bool isFirstEnter = true; //是不是第一次初始化判断方向信息
        public Int16 DCTrainSpeed = 0;  //实时列车速度，司控器发送，区分大于0和小于0
        public UInt16 ATPPermitDirection = 0; //列车运行方向，初始化是0，速度大于0是1，速度小于0是2
        public UInt16 DCHandlePos = 0; //司控器实时传送的把柄方向，默认是0,速度大于0是1，速度小于0是2
        public byte actualDirection = 0; //发送给ZC实时的方向
        public Byte HeadSectionOrSwitch; //根据目前应答器判断是区段还是道岔
        public Byte TailSectionOrSwitch; //头部和尾部的应答器都是当前的应答器
        public Byte HeadID; //当前应答器的节点ID
        public Byte TailID;
        public UInt32 HeadOff; //根据当前应答器得到当前的偏移量
        public UInt32 TailOff;
        public byte CurbaliseID = 0; //当前应答器的ID
        public byte SectionOrSwitch = 0; //当前应答器所处位置是道岔2还是区段1
        public UInt32 Off = 0;  //当前应答器的偏移量
        public UInt32 Distance_1 = 0;  //当前应答器的距离，120-偏移量
        public byte runInfoType = 0x01;
        public bool isReleaseEB = false; //是否缓解了EB，用于DMI
        public Byte headID;            //是ZC发送的当前位置的id
        public byte ZCInfoType;
        public byte MAEndType;
        public Byte tailSectionOrSwitch; //1是区段，2是道岔
        public Byte tailID; //从ZC接受到的MA的id
        public UInt32 MAEndOff = 0;
        public int obstacleNum = 0;
        public string[] obstacleID = new string[10];
        public byte[] obstacleState = new byte[10];
        public  int MAEndDistance = 0; 
        public  int limSpeedNum = 0;
        public  int limSpeedDistance_1 = 0;
        public  int limSpeedLength_1 = 0;
        public  int limSpeedDistance_2 = 0;
        public  int limSpeedLength_2 = 0;
        public  int limSpeedDistance_3 = 0;
        public  int limSpeedLength_3 = 0;
        public  int limSpeedDistance_4 = 0;
        public  int limSpeedLength_4 = 0;
        public int limitSpeed = 0;
        public UdpClient ATPToATPCurveClient;
        public UdpClient ATPToDMIClient;
        public UdpClient ATPToZCClient;
        public UdpClient ATPToDCClient;
        public IPAddress ATPIP;
        public int ATPPort;
        public IPAddress ZCIP;
        public int ZCPort;
        public IPAddress ATPZCIP;
        public int ATPZCPort;
        public IPAddress DMIIP;
        public int DMIPort;
        public IPAddress ATPDMIIP;
        public int ATPDMIPort;
        public IPAddress DCIP;
        public int DCPort;
        public IPAddress ATPDCIP;
        public int ATPDCPort;
        public IPAddress ATPCurveIP;
        public int ATPCurvePort;
        public IPAddress ATPATPCurveIP;
        public int ATPATPCurvePort;
        ATPCurvePackage atpCurvePackage=new ATPCurvePackage();
        ZCPackage zcPackage=new ZCPackage() { PackageType = 8, ReceiveID = 3, ZCID = 3 };
        DMIPackage dmiPackage=new DMIPackage() { PackageType = 3, ActulSpeed = 25, TrainNum = "" };
        DCPackage dcPackage=new DCPackage() { PackageType = 5 };
        public int trainID ;
        public int sendID;

        EB Socket_EB = new EB();
        TrainMessage trainMessage = new TrainMessage();
        SearchLater searchLater = new SearchLater();
        private Timer timer;

        public void SocketStart()
        {
            GetATPIPAndPort(); //得到绑定ATP的IP和Port
            ATPToATPCurveClient = new UdpClient(new IPEndPoint(ATPATPCurveIP, ATPATPCurvePort));
            ATPToATPCurveClient.Connect(ATPCurveIP, ATPCurvePort);
            ATPToDMIClient = new UdpClient(new IPEndPoint(ATPDMIIP, ATPDMIPort));
            ATPToDMIClient.Connect(DMIIP, DMIPort);
            ATPToZCClient = new UdpClient(new IPEndPoint(ATPZCIP, ATPZCPort));
            ATPToZCClient.Connect(ZCIP, ZCPort);
            ATPToDCClient = new UdpClient(new IPEndPoint(ATPDCIP, ATPDCPort));
            ATPToDCClient.Connect(DCIP, DCPort);
            searchLater.GetHash();
            trainMessage.GetHash(); //trainMessage和searchLater.trainMessage是两个实例化的类
            StartReceData();  //一直接受数据
            SetupTimer();  //隔200ms计算一次MA终点
        }

        public void StartReceData()
        {
            const uint IOC_IN = 0x80000000;
            int IOC_VENDOR = 0x18000000;
            int SIO_UDP_CONNRESET = (int)(IOC_IN | IOC_VENDOR | 12);
            ATPToDMIClient.Client.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);
            ATPToZCClient.Client.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);
            ATPToDCClient.Client.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);
            Task.Run(() =>
            {
 
                IPEndPoint sender = new IPEndPoint(0, 0);
                while (true)
                {
                    //try
                    //{
                        byte[] buf = ATPToDMIClient.Receive(ref sender);
                        Receive_DMI_Data(buf);
                    //}
                    //catch (Exception e)
                    //{
                    //    Debug.WriteLine(e.Message);
                    //}
                }
            });
           
            Task.Run(() =>
            {
              
               
                IPEndPoint sender1 = new IPEndPoint(0, 0);
                while (true)
                {
                    //try
                    //{
                        byte[] buf = ATPToZCClient.Receive(ref sender1);
                        Receive_ZC_Data(buf);
                    //}
                    //catch (Exception e)
                    //{
                    //    Debug.WriteLine(e.Message);
                    //}
                }
            });
           
            Task.Run(() =>
            {

               
                IPEndPoint sender2 = new IPEndPoint(0, 0);
                while (true)
                {
                    //try
                    //{
                        byte[] buf = ATPToDCClient.Receive(ref sender2); //测完
                        if (buf[2] == 6)
                        {
                            Receive_DC_Data(buf);
                        }
                        else if (buf[2] == 5) //在有速度时才会发送
                        {
                            Receive_Balise_Data(buf); //应答器也是由司控器的端口传来的
                        }
                    //}

                    //catch (Exception e)
                    //{
                    //    Debug.WriteLine(e.Message);
                    //}
                }
            });
        }
      

        public void GetATPIPAndPort()
        {
            foreach (var item in IPConfigure.IPList)
            {
                if (item.DeviceName == "ZC")
                {
                    ZCIP = item.IP;
                    ZCPort = item.Port;
                    ATPZCIP = item.ATPIP;
                    ATPZCPort = item.ATPPort;
                    trainID = item.trainID;
                    sendID = item.sendID;
                }
                else if (item.DeviceName == "DMI")
                {
                    DMIIP = item.IP;
                    DMIPort = item.Port;
                    ATPDMIIP = item.ATPIP;
                    ATPDMIPort = item.ATPPort;
                    trainID = item.trainID;
                    sendID = item.sendID;
                }
                else if (item.DeviceName == "DC")
                {
                    DCIP = item.IP;
                    DCPort = item.Port;
                    ATPDCIP = item.ATPIP;
                    ATPDCPort = item.ATPPort;
                    trainID = item.trainID;
                    sendID = item.sendID;
                }
                else if (item.DeviceName == "ATPCurve")
                {
                    ATPCurveIP = item.IP;
                    ATPCurvePort = item.Port;
                    ATPATPCurveIP = item.ATPIP;
                    ATPATPCurvePort = item.ATPPort;
                    trainID = item.trainID;
                    sendID = item.sendID;
                }
            }
        }
        private void SetupTimer()  //发包的方法
        {
            timer = new Timer(200);
            timer.Elapsed += TimerElapsed;
            timer.AutoReset = false;
            timer.Start();
        }
        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {

            ProcessData();
            SendATPCurve();                         //隔200ms发送数据
            SendDMI();
            SendZC();
            SendDC();
            timer.Start();
        }

        public void SendATPCurve()
        {
            atpCurvePackage.MAEndDistance = MAEndDistance;
            atpCurvePackage.limSpeedNum = limSpeedNum;
            atpCurvePackage.limSpeedDistance_1 = limSpeedDistance_1;
            atpCurvePackage.limSpeedLength_1 = limSpeedLength_1;
            atpCurvePackage.limSpeedDistance_2 = limSpeedDistance_2;
            atpCurvePackage.limSpeedLength_2 = limSpeedLength_2;
            atpCurvePackage.limSpeedDistance_3 = limSpeedDistance_3;
            atpCurvePackage.limSpeedLength_3 = limSpeedLength_3;
            atpCurvePackage.limSpeedDistance_4 = limSpeedDistance_4;
            atpCurvePackage.limSpeedLength_4 = limSpeedLength_4;
            byte[] ATPCurveSendData = atpCurvePackage.ATPCurvePackStream();
            ATPToATPCurveClient.Send(ATPCurveSendData, 1024);
        }
        public void SendDMI()
        {
            dmiPackage.TrainID = 65536;
            dmiPackage.TrainNum = "T0" + Convert.ToString(trainID);
            dmiPackage.HighModel = 1;
            dmiPackage.CurModel = (byte)DCCtrlMode;
            dmiPackage.ActulSpeed = (UInt16)DCTrainSpeed;
            dmiPackage.BreakOut = 7;
            dmiPackage.Alarm = 1;
            dmiPackage.HighSpeed = 100;    //目前得不到速度信息
            dmiPackage.PermitSpeed = 95;
            dmiPackage.FrontPermSpeed = 95;
            dmiPackage.TargetLoca = (UInt16)MAEndDistance;
            if (Socket_EB.isEB == true)
            {
                dmiPackage.BreakOut = 6;
                dmiPackage.Alarm = 1;
            }




            byte[] DMISendData = dmiPackage.DMIPackStream();
            ATPToDMIClient.Send(DMISendData, 1024);
        }

        public void SendDC()
        {
            if (Socket_EB.isEB == true)
            {
                dcPackage.IsEB = 6; //收到缓解后已经赋值7
            }
            
            byte[] DCSendData = dcPackage.DCPackStream();
            ATPToDCClient.Send(DCSendData, 1024);
        }

        public void SendZC() //测完
        {
            if (Regex.Matches(baliseHead,"Z").Count <= 0 && baliseHead!="")
            {
            zcPackage.SendID = (byte)sendID;
            zcPackage.TrainID = (UInt16)trainID;
            zcPackage.RunInformation = 0x01;  
            //if (MAEndType == 0x02 && ZCInfoType != 0x04)
            //{
            //        zcPackage.RunInformation = 0x04;
            // }
            //    //注销模式0x03,默认是0x01,折返端换端是0x55
            //    //zcPackage.DoorState_ = 1;

            zcPackage.HeadSectionOrSwitch = HeadSectionOrSwitch;
            zcPackage.HeadID = HeadID;
            zcPackage.HeadOff = HeadOff;  //发送给ZC偏移量
            zcPackage.TailSectionOrSwitch = HeadSectionOrSwitch;
            zcPackage.TailID = HeadID;
            zcPackage.TailOff = HeadOff; //大写的是自己找的                                            
            zcPackage.ACtSpeed = (UInt16)Math.Abs(DCTrainSpeed);
            zcPackage.HeadActDirection = actualDirection;
            zcPackage.Mode = curModel;

            byte[] ZCSendData = zcPackage.ZCPackStream();
            ATPToZCClient.Send(ZCSendData, 1024);
            }

        }

        public void ProcessData()  //每隔200ms调用一次这个方法，接受数据的一直等待接受，这里隔200ms计算一次MA
        {
            GetCurModel();     //得到目前的控车模式
            StartCalMA();     //开始计算MA
        }

        public void StartCalMA() //刷到正线上的应答器开始计算MA
        {
            curBalise = baliseHead;
            //curBalise = "T0114_1_1"; //测试模拟接受应答器
            trainHead = baliseHead;
            trainTail = baliseTail;  //目前都等于当前应答器传来的消息
            GetMA();    //得到MA信息
        }

        public void GetMA() //用于寻路得到MA信息
        {
            GetFirstDir();  //初始化列车速度方向信息
            GetDir();       //实时判断列车方向，方向不对时则EB
            if (curBalise != "") //当目前的应答器不等于空的时候
            {
                CalSectionOrSwitchIDOFF(curBalise);
                GetDistanceAndPrint(curBalise);
            }        
        }

        public void curBaliseEB(string curBalise,byte tailID,byte tailSectionOrSwitch)
        {
            int curID = searchLater.BaliseToID(curBalise);
            TopolotyNode curTopolotyNode = trainMessage.BaliseToIteam(curBalise, curID);
            int curleftType;
            int curleftID;
            if (!isLeftSearch)
            {
                foreach (var p in curTopolotyNode.Left)
                {
                    if (p.device.Name.Substring(0, 1) == "T" || p.device.Name.Substring(0, 1) == "Z")
                    {
                        curleftType = 1;
                        curleftID = searchLater.BaliseToID(p.device.Name);
                    }
                    else
                    {
                        curleftType = 2;
                        curleftID = searchLater.BaliseToID((p.device as RailSwitch).section.Name);
                    }

                    if (curleftID == tailID && curleftType == tailSectionOrSwitch)
                    {
                        Socket_EB.Set_EB("超过MA终点");
                        break;
                    }

                }
            }
            else
            {
                foreach (var p in curTopolotyNode.Right)
                {
                    if (p.device.Name.Substring(0, 1) == "T" || p.device.Name.Substring(0, 1) == "Z")
                    {
                        curleftType = 1;
                        curleftID = searchLater.BaliseToID(p.device.Name);
                    }
                    else
                    {
                        curleftType = 2;
                        curleftID = searchLater.BaliseToID((p.device as RailSwitch).section.Name);
                    }

                    if (curleftID == tailID && curleftType == tailSectionOrSwitch)
                    {
                        Socket_EB.Set_EB("超过MA终点");
                        break;
                    }

                }
            }
            
        }
        public void GetDistanceAndPrint(string curBalise)
        {
        
            if (Regex.Matches(curBalise, "Z").Count <= 0 && curBalise != "")
            {
                curBaliseEB(curBalise, tailID, tailSectionOrSwitch); //如果在MA终点上就不需要寻路了
            }
            if (isRecvZC && runInfoType == 0x01 && Socket_EB.isEB == false && curModel != 4) //当这四个条件满足时开始处理ATP曲线。当应答器的id和zc发来的id一致时，开始计算信息距离
            {
                isReleaseEB = false;
                byte currentHeadID = (byte)searchLater.BaliseToID(curBalise);
                if (currentHeadID == headID) //目前应答器给我的ID和ZC发的ID一致
                {
                  
                        int[] value = searchLater.SearchDistance(isLeftSearch, tailSectionOrSwitch, tailID, Convert.ToInt32(MAEndOff), obstacleNum, curBalise, obstacleID, obstacleState);
                        MAEndDistance = value[0];
                        limSpeedNum = value[1];
                        limSpeedDistance_1 = value[2];
                        limSpeedLength_1 = value[3];
                        limSpeedDistance_2 = value[4];
                        limSpeedLength_2 = value[5];
                        limSpeedDistance_3 = value[6];
                        limSpeedLength_3 = value[7];
                        limSpeedDistance_4 = value[8];
                        limSpeedLength_4 = value[9];
                        Console.WriteLine("MAEndDistance " + Convert.ToString(MAEndDistance) + " limSpeedNum " + Convert.ToString(limSpeedNum) + " limSpeedDistance_1 " + Convert.ToString(limSpeedDistance_1) +
                                       " limSpeedLength_1 " + Convert.ToString(limSpeedLength_1) + " limSpeedDistance_2 " + Convert.ToString(limSpeedDistance_2) + " limSpeedLength_2 " + Convert.ToString(limSpeedLength_2) +
                                       " limSpeedDistance_3 " + Convert.ToString(limSpeedDistance_3) + " limSpeedLength_3 " + Convert.ToString(limSpeedLength_3) + " limSpeedDistance_4 " + Convert.ToString(limSpeedDistance_4) + " limSpeedLength_4 " +
                                        Convert.ToString(limSpeedLength_4));


        }
    }

}

        public void CalSectionOrSwitchIDOFF(string curBalise) //根据目前应答器判断是区段和道岔，还有ID，和偏移量。车的头部和尾部都是当前应答器
        {
            int ID = searchLater.BaliseToID(curBalise);
            if (trainHead.Length != 0 && Regex.Matches(trainHead, "Z").Count == 0) //车头部和车尾部的应答器都是一样的，都是当前应答器发送的，count等于0即进入正线
            {
                UInt32[] value = SectionOrSwitchIDOFF(trainHead,ID);     //利用这个来发送偏移量
                HeadSectionOrSwitch = (byte)value[0];  //区段是1，道岔是2
                HeadID = (byte)value[1];            //根据应答器名字得到节点ID。找到目前的发送给ZC，ZC发送过来的是MA的
                HeadOff = value[2];               //计算偏移量
            }
            if (trainTail.Length != 0 && Regex.Matches(trainTail, "Z").Count == 0)
            {
                UInt32[] value1 = SectionOrSwitchIDOFF(trainTail, ID); //trainHead trainTail均指当前的应答器。车不分车头和车尾部
                TailSectionOrSwitch = (byte)value1[0]; // 1是区段，2是道岔
                TailID = (byte)value1[1];
                TailOff = value1[2];
            }
        }

        public UInt32[] SectionOrSwitchIDOFF(string balise,int ID) //寻找偏移量和distance_1
        {
            UInt32[] returnValue = new UInt32[3];
            bool isSwitch = trainMessage.IsRailswitchVoid(balise);
            CurbaliseID = (byte)trainMessage.BaliseToIteam(balise,ID).device.Id;

            if (isSwitch)
            {
                SectionOrSwitch = 2;
                trainMessage.SwitchGetOffDis(isLeftSearch, balise, ref Off, ref Distance_1,ID);
            }
            else
            {
                SectionOrSwitch = 1;
                trainMessage.SectionGetOffDis(isLeftSearch,balise,ref Off,ref Distance_1,ID);
            }
            returnValue[0] = SectionOrSwitch;
            returnValue[1] = CurbaliseID;
            returnValue[2] = Off;
            return returnValue;
        }


        public void GetDir() //在行驶过程中实时接收司控器的方向信息判断方向，并且方向不对就EB
        {
            if (curModel != 4) //不在EUM模式下
            {
                    if (DCTrainSpeed < 0 && DCHandlePos == 2)
                    {
                        ATPPermitDirection = 2;
                        actualDirection = 0xAA; //记录下来发送给ZC
                        isLeftSearch = true; //左寻
                    }
                    else if (DCTrainSpeed > 0 && DCHandlePos == 1)
                    {
                        ATPPermitDirection = 1;
                        actualDirection = 0x55;    
                        isLeftSearch = false;
                    }
                    else if(DCTrainSpeed>0) //在列车运行的时候判断
                    {
                        Socket_EB.Set_EB("行驶过程中方向判断错误");
                    }              
            }
            else //在EUM模式下
            {
                ATPPermitDirection = 0;
                actualDirection = 0;
            }
        }

        public void GetFirstDir() //得到初始化的方向信息
        {
            if (isFirstEnter)
            {
                if (DCTrainSpeed > 0)//1端有数据
                {
                    isLeftSearch = false;
                    ATPPermitDirection = 1; //默认是0，速度大于0是1，速度小于0是2
                }
                else if (DCTrainSpeed < 0)//2端有数据
                {
                    isLeftSearch = true;
                    ATPPermitDirection = 2;
                }

                isFirstEnter = false;

            }
        }

        public void GetCurModel()  //判断目前的控车模式
        {
            if (Regex.Matches(baliseHead, "Z").Count > 0)
            {
                curModel = (byte)ModelType.RM;//正线之前RM
            }
            else
            {
                switch (DCCtrlMode)
                {
                    case (UInt16)ModelType.AM-1:
                        curModel = (byte)ModelType.AM;
                        break;
                    case (UInt16)ModelType.CM-1:
                        curModel = (byte)ModelType.CM;
                        break;
                    case (UInt16)ModelType.RM-1:
                        curModel = (byte)ModelType.RM;
                        break;
                    case (UInt16)ModelType.EUM-1:
                        curModel = (byte)ModelType.EUM;
                        break;
                    default:
                        break;
                }              
            }
            ModelIsRecvZC(curModel); //判断是否接收消息
        }
        public void Receive_ZC_Data(byte[] ZCData)
        {
            using (MemoryStream receStreamZC = new MemoryStream(ZCData))
            {
                BinaryReader reader = new BinaryReader(receStreamZC);
                UInt16 ZCCycle = reader.ReadUInt16();
                UInt16 ZCPackageType = reader.ReadUInt16();
                byte ZCSendID = reader.ReadByte();
                byte ZCReceiveID = reader.ReadByte();
                UInt16 ZCDataLength = reader.ReadUInt16();
                UInt16 ZCNID_ZC = reader.ReadUInt16();
                UInt16 ZCNID_Train = reader.ReadUInt16();
                ZCInfoType = reader.ReadByte();
                byte ZCStopEnsure = reader.ReadByte();
                UInt64 ZCNID_DataBase = reader.ReadUInt64();
                UInt16 ZCNID_ARButton = reader.ReadUInt16();
                byte ZCQ_ARButtonStatus = reader.ReadByte();
                UInt16 ZCNID_LoginZCNext = reader.ReadUInt16();
                byte ZCN_Length = reader.ReadByte();
                MAEndType = reader.ReadByte();
                byte headSectionOrSwitch = reader.ReadByte();
                headID = reader.ReadByte();
                UInt32 ZCD_D_MAHeadOff = reader.ReadUInt32();
                byte ZCQ_MAHeadDir = reader.ReadByte();
                tailSectionOrSwitch = reader.ReadByte();  //1是直轨，2是道岔 
                tailID = reader.ReadByte();              //MA终点的ID
                MAEndOff = reader.ReadUInt32();
                byte MAEndDir = reader.ReadByte();
                obstacleNum = reader.ReadByte();

                obstacleID = new string[obstacleNum];
                obstacleState = new byte[obstacleNum];

                if (obstacleNum != 0) //障碍物等于0和不等于0会发送不同的包
                {
                    string State = "";
                    string ID = "";
                    byte[] obstacleType = new byte[obstacleNum];
                    obstacleID = new string[obstacleNum]; //障碍物ID会用到
                    obstacleState = new byte[obstacleNum]; //障碍物状态会用到
                    byte[] obstacleLogicState = new byte[obstacleNum];
                    for (int i = 0; i < obstacleNum; i++)
                    {
                        obstacleType[i] = reader.ReadByte();
                        obstacleID[i] = (reader.ReadUInt16()).ToString();
                        obstacleState[i] = reader.ReadByte();
                        obstacleLogicState[i] = reader.ReadByte();
                        State = State + obstacleState[i] + " ";
                        ID = ID + obstacleID[i] + " ";
                    }
                }

                byte ZCN_TSR = reader.ReadByte();
                UInt32 ZCQ_ZC = reader.ReadUInt32();
                byte ZCEB_Type = reader.ReadByte();
                byte ZCEB_DEV_Typ = reader.ReadByte();
                UInt16 ZCEB_DEV_Name = reader.ReadUInt16();

                ZCSendEB(tailSectionOrSwitch, tailID, MAEndOff, MAEndDir); //ZC发送EB消息

            }          
        }
        

        private void ZCSendEB(byte tailSectionOrSwitch, byte tailID, UInt32 MAEndOff, byte MAEndDir) //由于ZC发送EB消息
        {
            if(tailSectionOrSwitch == 3 && tailID == 0 && MAEndOff == 0 && MAEndDir == 0) //EB消息测完
            {
                Socket_EB.Set_EB("ZC发送EB信息");
            }
        }
        
        private void ModelIsRecvZC(byte curModel) //当model不等于4时接受ZC的消息
        {
            if (curModel == 4)
            {
                isRecvZC = false;
            }
            else
            {
                isRecvZC = true;
            }
        }

        public void Receive_DMI_Data(byte[] DMIData) //DMI传来消息
        {
            using (MemoryStream receStreamDMI = new MemoryStream(DMIData))
            {
                BinaryReader reader = new BinaryReader(receStreamDMI);
                UInt16 DMICycle = reader.ReadUInt16();
                UInt16 DMIPackageType = reader.ReadUInt16();
                UInt16 DMILength = reader.ReadUInt16();
                string DMITrainOrder = reader.ReadString();
                UInt32 DMITrainNumber = reader.ReadUInt32();
                UInt16 DMIDriverNumber = reader.ReadUInt16();
                byte DMITestOrder = reader.ReadByte();
                byte DMIRelieveOrder = reader.ReadByte();
                if (DMIRelieveOrder == 2)
                {
                    dcPackage.IsEB = 7; //收到缓解消息发送给DC
                    dmiPackage.BreakOut = 7; //收到缓解消息发送给dmi
                    Socket_EB.isEB = false;
                }
            }
        }

        public void Receive_DC_Data(byte[] DCData) //司控器传来消息
        {
            using (MemoryStream receStreamDC = new MemoryStream(DCData))
            {
                BinaryReader reader = new BinaryReader(receStreamDC);
                UInt16 DCCycle = reader.ReadUInt16();
                UInt16 DCPackageType = reader.ReadUInt16();
                UInt16 DCLength = reader.ReadUInt16();
                DCTrainSpeed = reader.ReadInt16(); //解析出列车的实时速度
                if (Math.Abs(DCTrainSpeed) >= ProtectSpeed()) //超速就EB
                {
                    Socket_EB.isEB = true;
                }
                DCCtrlMode = reader.ReadUInt16();
                DCHandlePos = reader.ReadUInt16();
                UInt16 DCisKeyIn = reader.ReadUInt16();
            }
            

        }

        public int ProtectSpeed() //先这样粗略计算
        {
            if (curBalise != "")
            {
                if (curBalise.Substring(0, 1) == "W")
                {
                    return 40;
                }
                else
                {
                    return 70;
                }
            }
            else
            {
                return 70;
            }
          

        }

        public void Receive_Balise_Data(byte[] BaliseData) //应答器传来消息
        {
            using (MemoryStream receStreamBalise = new MemoryStream(BaliseData))
            {
                BinaryReader reader = new BinaryReader(receStreamBalise);
                UInt16 baliseCycle = reader.ReadUInt16();
                UInt16 balisePackageType = reader.ReadUInt16();
                baliseHead = reader.ReadString();
                string baliseTail = baliseHead;       //头部应答器和尾部应答器默认是一个
                GetBaliseLater(baliseHead, isLeftSearch);
            }

        }

        private void GetBaliseLater(string baliseHead,bool isLeftSearch)    //是否到达最开始的应答器
        {
            if (Regex.Matches(baliseHead, "Z").Count > 0 && isLeftSearch == false)
            {
                isBaliseFirst = false;//还没到正轨时是false，表示还未起模
            }
            else
            {
                isBaliseFirst = true;
            }
        }
        


    }
}
