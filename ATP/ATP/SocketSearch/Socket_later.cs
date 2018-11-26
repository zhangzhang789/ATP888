using ATP.SocketSearch;
using CbtcData;
using ConfigData;
using Package;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using TrainMessageEB;

namespace SocketSearch
{
    enum PackageType
    {
        PackZC = 9,
        PackDMI = 4,
        PackDC = 6,
        PackBalise = 5
    }

    enum Speed
    {
        baliseSpeed = 40,
        quduanSpeed = 70
    }

    public enum ModelType
    {
        AM = 1,
        CM = 2,
        RM = 3,
        EUM = 4
    }

    public enum HandlePos
    {
        HandleNone, Handle_1, Handle_2
    }

    class Socket_later
    {
        private bool isRecvZC = false;
        private bool isCalMA = false;
        private bool isLeftSearch = false;
        private ModelType curModel = ModelType.RM; // 前的控车模式
        private DcInfo dcInfo = new DcInfo();

        private bool isFirstEnter = true; //是不是第一次初始化判断方向信息
        
        private byte CurbaliseID = 0; //当前应答器的ID
        private byte SectionOrSwitch = 0; //当前应答器所处位置是道岔2还是区段1

        private Byte headID;            //是ZC发送的当前位置的id
        private Byte tailSectionOrSwitch; //1是区段，2是道岔
        private Byte tailID; //从ZC接受到的MA的id

        private UInt32 MAEndOff = 0;
        private int obstacleNum = 0;
        private string[] obstacleID = new string[10];
        private byte[] obstacleState = new byte[10];

        private SpeedLimit speedLimit = new SpeedLimit();

        private Atp2Curve curve = new Atp2Curve();
        private Atp2Dmi dmi = new Atp2Dmi();
        private Atp2Zc zc = new Atp2Zc();
        private Atp2Dc dc = new Atp2Dc();
        private Atp2Fault fault = new Atp2Fault();
        
        
        private int trainID;
        private int sendID;
        private Int16 headFault;
        private Byte zhangJieFault;
        private Byte xiaoJieFault;
        private string faultReason;
        private int ZC_Count = 0;
        Comfort comfort = new Comfort();

        EB Socket_EB = new EB();
        TrainMessage trainMessage = new TrainMessage();
        SearchLater searchLater = new SearchLater();
        private System.Timers.Timer timer;
        private System.Timers.Timer timer_receZC;

        const int Distance70_40 = (int)((70 / 3.6 * 70 / 3.6 - 40 / 3.6 * 40 / 3.6) / (2 * 1.2)); //106
        const int Distance70_0 = (int)(70 / 3.6 * 80 / 3.6 / (2 * 1.2)); //除以3.6的以m/s为单位,180

        public void SocketStart()
        {
            GetATPIPAndPort(); //得到绑定ATP的IP和Port

            curve.Initialize();
            dmi.Initialize();
            zc.Initialize();
            dc.Initialize();
            fault.Initialize();

            searchLater.GetHash();
            trainMessage.GetHash(); //trainMessage和searchLater.trainMessage是两个实例化的类
            StartReceData();  //一直接受数据
            SetupTimer();  //隔200ms计算一次MA终点
            SetupTimer_ReceiveZC();
        }

        public void StartReceData()
        {
            CreateRecvThread(dmi.client, Receive_DMI_Data);
            CreateRecvThread(zc.client, Receive_ZC_Data);
            CreateRecvThread(dc.client, Receive_DC_Data);
            CreateRecvThread(fault.client, Receive_Fault_Data);
        }

        private void CreateRecvThread(UdpClient client, Action<byte[]> recvHandler)
        {
            IPEndPoint sender = new IPEndPoint(0, 0);
            Thread recvThread = new Thread(() =>
            {
                while (true)
                {
                    byte[] buf = client.Receive(ref sender);
                    recvHandler(buf);
                }
            })
            { IsBackground = true };
            recvThread.Start();
        }

        public void GetATPIPAndPort()
        {
            foreach (var item in IPConfigure.IPList)
            {
                trainID = item.trainID;
                sendID = item.sendID;
            }
        }

        private void SetupTimer()  //发包的方法
        {
            timer = new System.Timers.Timer(200);
            timer.Elapsed += TimerElapsed;
            timer.AutoReset = false;
            timer.Start();
        }

        private void SetupTimer_ReceiveZC()  //发包的方法
        {
            timer_receZC = new System.Timers.Timer(3000);
            timer_receZC.Elapsed += IsSendZC;
            timer_receZC.AutoReset = false;
            timer_receZC.Start();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                ProcessData();
                curve.SendATPCurve(speedLimit, isInFault, Socket_EB.isEB,
                    zhangJieFault, xiaoJieFault, faultReason, faultRecover, speedFault);                         //隔200ms发送数据
                dmi.SendDMI(trainID, dcInfo, Socket_EB.isEB,
                    ProtectSpeed, speedLimit, DMIShow, isRealeaseEB, isZcAlive, isInFault);
                zc.SendZC(dcInfo, sendID, trainID, curModel);
                dc.SendDC(Socket_EB.isEB);
                calATOATP();                            //计算舒适度
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Socket_later.TimerElapsed: " + ex.Message);
            }
            timer.Start();
        }
        
        public void ProcessData()  //每隔200ms调用一次这个方法，接受数据的一直等待接受，这里隔200ms计算一次MA
        {
            GetCurModel();     //得到目前的控车模式
            StartCalMA();     //开始计算MA
        }

        public void StartCalMA() //刷到正线上的应答器开始计算MA
        {
            dcInfo.SetCurBalise();
            GetMA();    //得到MA信息
        }

        public void GetMA() //用于寻路得到MA信息
        {
            GetFirstDir();  //初始化列车速度方向信息
            GetDir();       //实时判断列车方向，方向不对时则EB
            //if (dcInfo.IsCurBaliseEmpty()) //当目前的应答器不等于空的时候
            if (dcInfo.IsCurBaliseEmpty())
            {
                CalSectionOrSwitchIDOFF(dcInfo.curBalise);
                GetDistanceAndPrint(dcInfo.curBalise);
            }
        }

        public void curBaliseEB(string curBalise, byte tailID, byte tailSectionOrSwitch)
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

            if (Regex.Matches(curBalise, "Z").Count <= 0 && dcInfo.IsCurBaliseEmpty())
            {
                curBaliseEB(curBalise, tailID, tailSectionOrSwitch); //如果在MA终点上就不需要寻路了
            }
            if (isRecvZC && Socket_EB.isEB == false && curModel != ModelType.EUM) //当这四个条件满足时开始处理ATP曲线。当应答器的id和zc发来的id一致时，开始计算信息距离
            {
                //isReleaseEB = false;
                byte currentHeadID = (byte)searchLater.BaliseToID(curBalise);
                if (currentHeadID == headID) //目前应答器给我的ID和ZC发的ID一致
                {

                    int[] value = searchLater.SearchDistance(isLeftSearch, tailSectionOrSwitch, tailID, Convert.ToInt32(MAEndOff), obstacleNum, curBalise, obstacleID, obstacleState);
                    speedLimit.MAEndDistance = value[0];
                    speedLimit.limSpeedNum = value[1];
                    speedLimit.limSpeedDistance[0] = value[2];
                    speedLimit.limSpeedLength[0] = value[3];
                    speedLimit.limSpeedDistance[1] = value[4];
                    speedLimit.limSpeedLength[1] = value[5];
                    speedLimit.limSpeedDistance[2] = value[6];
                    speedLimit.limSpeedLength[2] = value[7];
                    speedLimit.limSpeedDistance[3] = value[8];
                    speedLimit.limSpeedLength[3] = value[9];
                    isCalMA = true;
                    if (Math.Abs(dcInfo.DCTrainSpeed) >= ProtectSpeed(speedLimit.MAEndDistance, 
                        speedLimit.limSpeedNum, speedLimit.limSpeedDistance[0])) //超速就EB Math.Abs(DCTrainSpeed) >= ProtectSpeed()
                    {
                        Socket_EB.Set_EB("超过防护速度");
                    }
                    Console.WriteLine("speedLimit.MAEndDistance " + 
                        Convert.ToString(speedLimit.MAEndDistance) + 
                        " limSpeedNum " + Convert.ToString(speedLimit.limSpeedNum) + 
                        " limSpeedDistance_1 " + Convert.ToString(speedLimit.limSpeedDistance[0]) +
                        " limSpeedLength_1 " + Convert.ToString(speedLimit.limSpeedLength[0]) + 
                        " limSpeedDistance_2 " + Convert.ToString(speedLimit.limSpeedDistance[1]) + 
                        " limSpeedLength_2 " + Convert.ToString(speedLimit.limSpeedLength[1]) +
                        " limSpeedDistance_3 " + Convert.ToString(speedLimit.limSpeedDistance[2]) + 
                        " limSpeedLength_3 " + Convert.ToString(speedLimit.limSpeedLength[2]) + 
                        " limSpeedDistance_4 " + Convert.ToString(speedLimit.limSpeedDistance[3]) + 
                        " limSpeedLength_4 " + Convert.ToString(speedLimit.limSpeedLength[3]));


                }
            }

        }

        public void CalSectionOrSwitchIDOFF(string curBalise) //根据目前应答器判断是区段和道岔，还有ID，和偏移量。车的头部和尾部都是当前应答器
        {
            int ID = searchLater.BaliseToID(curBalise);

            if (dcInfo.IsCurStartWith("Z")) //车头部和车尾部的应答器都是一样的，都是当前应答器发送的，count等于0即进入正线
            {
                UInt32[] value = SectionOrSwitchIDOFF(dcInfo.trainHead, ID);     //利用这个来发送偏移量
                zc.zcPackage.HeadSectionOrSwitch = (byte)value[0];  //区段是1，道岔是2
                zc.zcPackage.HeadID = (byte)value[1];            //根据应答器名字得到节点ID。找到目前的发送给ZC，ZC发送过来的是MA的
                zc.zcPackage.HeadOff = value[2];               //计算偏移量
            }

            if (dcInfo.isTailStartWith("Z"))
            {
                UInt32[] value1 = SectionOrSwitchIDOFF(dcInfo.trainTail, ID); //trainHead trainTail均指当前的应答器。车不分车头和车尾部
                zc.zcPackage.HeadSectionOrSwitch = (byte)value1[0]; // 1是区段，2是道岔
                zc.zcPackage.TailID = (byte)value1[1];
                zc.zcPackage.TailOff = value1[2];
            }
        }

        public UInt32[] SectionOrSwitchIDOFF(string balise, int ID) //寻找偏移量和distance_1
        {
            UInt32[] returnValue = new UInt32[3];

            bool isSwitch = trainMessage.IsRailswitchVoid(balise);
            CurbaliseID = (byte)trainMessage.BaliseToIteam(balise, ID).device.Id;
            UInt32 Off = 0;  //当前应答器的偏移量
            UInt32 Distance_1 = 0;  //当前应答器的距离，120-偏移量

            if (isSwitch)
            {
                SectionOrSwitch = 2;
                trainMessage.SwitchGetOffDis(isLeftSearch, balise, ref Off, ref Distance_1, ID);
            }
            else
            {
                SectionOrSwitch = 1;
                trainMessage.SectionGetOffDis(isLeftSearch, balise, ref Off, ref Distance_1, ID);
            }

            returnValue[0] = SectionOrSwitch;
            returnValue[1] = CurbaliseID;
            returnValue[2] = Off;

            return returnValue;
        }

        public void GetDir() //在行驶过程中实时接收司控器的方向信息判断方向，并且方向不对就EB
        {
            if (curModel != ModelType.EUM) //不在EUM模式下
            {
                if (dcInfo.IsMovingLeft())
                {
                    //ATPPermitDirection = 2;
                    zc.zcPackage.HeadActDirection = 0xAA; //记录下来发送给ZC
                    isLeftSearch = true; //左寻
                }
                else if (dcInfo.IsMovingRight())
                {
                    //ATPPermitDirection = 1;
                    zc.zcPackage.HeadActDirection = 0x55;
                    isLeftSearch = false;
                }
                else if (dcInfo.DCTrainSpeed > 0) //在列车运行的时候判断
                {
                    Socket_EB.Set_EB("行驶过程中方向判断错误");
                }
            }
            else //在EUM模式下
            {
                //ATPPermitDirection = 0;
                zc.zcPackage.HeadActDirection = 0;
            }
        }

        public void GetFirstDir() //得到初始化的方向信息
        {
            if (isFirstEnter)
            {
                if (dcInfo.DCTrainSpeed != 0)
                {
                    isLeftSearch = dcInfo.DCTrainSpeed < 0;
                }

                isFirstEnter = false;
            }
        }

        public void GetCurModel()  //判断目前的控车模式
        {
            if (dcInfo.isHeadTartWith("Z"))
            {
                curModel = ModelType.RM;//正线之前RM
            }
            else
            {
                curModel = dcInfo.DCCtrlMode;
            }

            ModelIsRecvZC(); //判断是否接收消息
        }

        public bool isZcAlive = true;

        private void IsSendZC(object sender, ElapsedEventArgs e)
        {
            isZcAlive = ZC_Count != 0;
            ZC_Count = 0;
            timer_receZC.Start();
        }

        public void Receive_ZC_Data(byte[] ZCData)
        {
            using (MemoryStream receStreamZC = new MemoryStream(ZCData))
            {
                ZC_Count += 1;
                BinaryReader reader = new BinaryReader(receStreamZC);
                UInt16 ZCCycle = reader.ReadUInt16();
                UInt16 ZCPackageType = reader.ReadUInt16();
                byte ZCSendID = reader.ReadByte();
                byte ZCReceiveID = reader.ReadByte();
                UInt16 ZCDataLength = reader.ReadUInt16();
                UInt16 ZCNID_ZC = reader.ReadUInt16();
                UInt16 ZCNID_Train = reader.ReadUInt16();
                byte ZCInfoType = reader.ReadByte();
                byte ZCStopEnsure = reader.ReadByte();
                UInt64 ZCNID_DataBase = reader.ReadUInt64();
                UInt16 ZCNID_ARButton = reader.ReadUInt16();
                byte ZCQ_ARButtonStatus = reader.ReadByte();
                UInt16 ZCNID_LoginZCNext = reader.ReadUInt16();
                byte ZCN_Length = reader.ReadByte();
                byte MAEndType = reader.ReadByte();
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
            if (tailSectionOrSwitch == 3 && tailID == 0 && MAEndOff == 0 && MAEndDir == 0) //EB消息测完
            {
                Socket_EB.Set_EB("ZC发送EB信息");
            }
        }

        private void ModelIsRecvZC() //当model不等于4时接受ZC的消息
        {
            isRecvZC = curModel != ModelType.EUM;
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
                    dc.dcPackage.IsEB = 7; //收到缓解消息发送给DC
                    dmi.dmiPackage.BreakOut = 7; //收到缓解消息发送给dmi
                    Socket_EB.isEB = false;
                }
            }
        }

        public List<double> DCTrainSpeedList = new List<double>();  //存储列车的实时速度

        public List<DateTime> DCTrainSpeedTimeList = new List<DateTime>();//存储列车得到速度的时间

        public void Receive_DC_Data(byte[] DCData) //司控器传来消息
        {
            if (DCData[2] == 5) //在有速度时才会发送
            {
                Receive_Balise_Data(DCData); //应答器也是由司控器的端口传来的
            }
            else if (DCData[2] == 6)
            {
                using (MemoryStream receStreamDC = new MemoryStream(DCData))
                {
                    BinaryReader reader = new BinaryReader(receStreamDC);
                    UInt16 DCCycle = reader.ReadUInt16();
                    UInt16 DCPackageType = reader.ReadUInt16();
                    UInt16 DCLength = reader.ReadUInt16();
                    dcInfo.DCTrainSpeed = reader.ReadInt16(); //解析出列车的实时速度               
                    if (isRecvZC == false && dcInfo.DCTrainSpeed > 40) //超速就EB
                    {
                        Socket_EB.Set_EB("超过防护速度");
                    }
                    dcInfo.DCCtrlMode = (ModelType)(reader.ReadUInt16() + 1);
                    if (dcInfo.DCCtrlMode == ModelType.CM) //计算舒适度
                    {
                        DateTime dt = DateTime.Now;
                        DCTrainSpeedList.Add(dcInfo.DCTrainSpeed);
                        DCTrainSpeedTimeList.Add(dt);
                    }
                    dcInfo.DCHandlePos = (HandlePos)reader.ReadUInt16();
                    UInt16 DCisKeyIn = reader.ReadUInt16();
                }
            }
        }

        public void calATOATP()
        {
            if (DCTrainSpeedList.Count >= 6)   //计算手动的舒适度
            {
                double v = DCTrainSpeedList[DCTrainSpeedList.Count - 1];
                double prev = DCTrainSpeedList[DCTrainSpeedList.Count - 3];
                double v1 = DCTrainSpeedList[DCTrainSpeedList.Count - 4];
                double v2 = DCTrainSpeedList[DCTrainSpeedList.Count - 6];
                DateTime t = DCTrainSpeedTimeList[DCTrainSpeedTimeList.Count - 1];
                DateTime pret = DCTrainSpeedTimeList[DCTrainSpeedTimeList.Count - 3];
                DateTime t1 = DCTrainSpeedTimeList[DCTrainSpeedTimeList.Count - 4];
                DateTime t2 = DCTrainSpeedTimeList[DCTrainSpeedTimeList.Count - 6];
                double a = (v - prev) / (t - pret).TotalSeconds;
                double prea = (v1 - v2) / (t1 - t2).TotalSeconds;
                comfort.CalculateEDa(v, prev, a, prea);

                curve.atpCurvePackage.totalEnergy = (Int32)comfort.totalEnergy;

                if (dcInfo.DCTrainSpeed != 0)
                {
                    curve.atpCurvePackage.Comfort = (int)comfort.da; //不等于0的时候才计算舒适度
                }
                else
                {
                    curve.atpCurvePackage.Comfort = 0;
                }
                DCTrainSpeedList.Clear();
            }
        }
        
        public bool isFirstCalSpeed = true;
        public int nextLimNum = 65535;
        public bool isConvertLimNum = false;
        public int ConvertDistance = 0; // 第一次过障碍物之后到MA终点距离
        public bool isFirstR = true;

        public UInt16 ProtectSpeed(int MAEndDistance, int limSpeedNum, int limSpeedDistance_1) //先这样粗略计算
        {
            if (dcInfo.IsCurBaliseEmpty() && dcInfo.IsCurStartWith("T"))
            {
                if (nextLimNum != limSpeedNum && 
                    isRecvZC == true && 
                    !trainMessage.LeftNextCurBaliseList(dcInfo.curBalise)[0].device.Name.StartsWith('Z') && 
                    isFirstR == true)
                {
                    isConvertLimNum = true;
                    ConvertDistance = speedLimit.MAEndDistance;
                    isFirstR = false;
                }
            }
            else if (dcInfo.IsCurBaliseEmpty())
            {
                if (nextLimNum != limSpeedNum && isRecvZC == true && isFirstR == true)
                {
                    isConvertLimNum = true;
                    ConvertDistance = speedLimit.MAEndDistance;
                    isFirstR = false;
                }
            }

            if (ConvertDistance - speedLimit.MAEndDistance <= Distance70_40 && isConvertLimNum == true)
            {
                if (dcInfo.IsCurStartWith("W"))
                {
                    return 40;
                }
                else
                {
                    if (ConvertDistance < speedLimit.MAEndDistance)
                    {
                        ConvertDistance = speedLimit.MAEndDistance;
                        return (UInt16)(Math.Sqrt(2 * 1.2 * (ConvertDistance - speedLimit.MAEndDistance) + Math.Pow(40 / 3.6, 2)) * 3.6);
                    }

                    if (speedLimit.MAEndDistance <= Distance70_0)
                    {
                        return (UInt16)(Math.Sqrt(2 * 1.2 * speedLimit.MAEndDistance) * 3.6);
                    }

                    return (UInt16)(Math.Sqrt(2 * 1.2 * (ConvertDistance - speedLimit.MAEndDistance) + Math.Pow(40 / 3.6, 2)) * 3.6);
                }
            }
            else
            {
                isConvertLimNum = false;
                nextLimNum = limSpeedNum;
                isFirstR = true;
                
                if (dcInfo.IsCurBaliseEmpty())
                {
                    return 100;
                }
                else
                {
                    if (dcInfo.IsCurStartWith("Z"))
                    {
                        return 40;
                    }
                    else
                    {
                        switch (limSpeedNum)
                        {
                            case 0:    //只能是一直在去段上

                                if (isCalMA == false)
                                {
                                    return 40; //和转换轨的40一样
                                }
                                else if (speedLimit.MAEndDistance > Distance70_0)
                                {
                                    return 70;
                                }
                                else
                                {
                                    return (UInt16)(Math.Sqrt(2 * 1.2 * speedLimit.MAEndDistance) * 3.6);
                                }

                            default:
                                if (dcInfo.IsCurStartWith("W"))
                                {
                                    return 40;
                                }
                                else if (limSpeedDistance_1 > Distance70_40)
                                {
                                    return 70;
                                }
                                else
                                {
                                    return (UInt16)(Math.Sqrt(2 * 1.2 * limSpeedDistance_1 + Math.Pow(40 / 3.6, 2)) * 3.6);
                                }
                        }
                    }
                }

            }

        }
        public bool DCConvertTypeBool = false;//初始化
        public bool speedFault = false;
        public bool DMIShow = true;
        public bool isRealeaseEB = true;
        public bool faultRecover = true;
        public bool isInFault = false;

        public void Receive_Fault_Data(byte[] FaultData) //应答器传来消息
        {
            using (MemoryStream receFaultBalise = new MemoryStream(FaultData))
            {
                BinaryReader reader = new BinaryReader(receFaultBalise);
                headFault = reader.ReadInt16();
                zhangJieFault = reader.ReadByte();
                xiaoJieFault = reader.ReadByte();
                faultReason = reader.ReadString();

                if (headFault == 23000)   //改变包头进行故障恢复
                {
                    DCConvertTypeBool = false;
                    speedFault = false;
                    DMIShow = true;
                    isRealeaseEB = true;
                    faultRecover = true;
                }
                else if (headFault == 23205)  //下发故障
                {
                    faultRecover = false;
                    if (zhangJieFault == 0 && xiaoJieFault == 0) //速度传感器故障
                    {
                        speedFault = true;
                    }
                    if (zhangJieFault == 0 && xiaoJieFault == 1)
                    {
                        DMIShow = false;
                    }
                    if (zhangJieFault == 0 && xiaoJieFault == 7)  //EB后不能缓解
                    {
                        isRealeaseEB = false;
                    }
                    Socket_EB.Set_EB("下发故障");
                    isInFault = true;
                }

            }

        }

        public void Receive_Balise_Data(byte[] BaliseData) //应答器传来消息
        {
            using (MemoryStream receStreamBalise = new MemoryStream(BaliseData))
            {
                BinaryReader reader = new BinaryReader(receStreamBalise);
                UInt16 baliseCycle = reader.ReadUInt16();
                UInt16 balisePackageType = reader.ReadUInt16();
                dcInfo.baliseHead = reader.ReadString();
            }

        }
    }
}
