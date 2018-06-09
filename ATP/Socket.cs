using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Timers;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using 线路绘图工具;
using System.Diagnostics;
using System.Text;

namespace CBTC
{
    class Socket
    {

        public IPEndPoint ipLocalPoint;
        public IPEndPoint ipLocalPoint1;  //处理故障
        public IPEndPoint ipLocalPoint2;  //接收时间
        public EndPoint RemotePoint;
        public EndPoint RemotePoint1;
        public EndPoint RemotePoint2;
        public System.Net.Sockets.Socket socket;
        public System.Net.Sockets.Socket socket1;
        public System.Net.Sockets.Socket socket2;
        public bool runningFlag = false;
        public int localPort = 0;
        public int sendPort = 0;
        public byte[] recv = new byte[1024];
        public byte[] recvFault = new byte[1024]; //接收故障注入的信息
        public byte[] recvATS = new byte[1024]; //接收故障注入的信息
        public string curBalise = "";
        public string number_1 = "";
        public string number_2 = "";
        public int obstacleNum = 0;
        public byte[] obstacleType = new byte[10];
        public string[] obstacleID = new string[10];
        public byte[] obstacleState = new byte[10];
        public byte[] obstacleLogicState = new byte[10];
        public byte[] sendBuf_ = new byte[1024];
        public byte runInfoType = 0x01;
        public byte MAEndType = 0;
        public UInt16 MAEndLink = 0;
        public UInt32 MAEndOff = 0;
        public UInt16 ZCD_MAHeadLink = 0;
        public Byte HeadID;
        public Byte headID;
        public UInt32 HeadOff;
        public Byte TailID;
        public static Byte tailID;  //小写的是自己从ZC收到的，大写的是自己找到的，给别人发的
        public UInt32 TailOff;
        public Byte HeadSectionOrSwitch;
        public Byte headSectionOrSwitch;
        public Byte TailSectionOrSwitch;
        public static Byte tailSectionOrSwitch;
        public int MAEndDir = 0;
        public int curNodeName = 0;
        public Int16 DCTrainSpeed = 0;  //实时列车速度
        public static UInt16 DCCtrlMode = 0;
        public static UInt16 DCHandlePos = 0;
        public byte DMIRelieveOrder = 0;
        public static string baliseHead = "";
        public static string baliseTail = "";
        public static int MAEndDistance = 0;
        public static int limSpeedNum = 0;
        public static int limSpeedDistance_1 = 0;
        public static int limSpeedLength_1 = 0;
        public static int limSpeedDistance_2 = 0;
        public static int limSpeedLength_2 = 0;
        public static int limSpeedDistance_3 = 0;
        public static int limSpeedLength_3 = 0;
        public static int limSpeedDistance_4 = 0;
        public static int limSpeedLength_4 = 0;
        public static int limitSpeed = 0;
        public static UInt16 stopTime = 0;
        public string trainHead = "";
        public string trainTail = "";
        public static bool isInFault=false;
        public bool isDCFirst = false;
        public bool isBaliseFirst = false;
        public bool isLeftSearch = false;
        public bool isRecvZC = false;
        bool isFirst = true;
        public bool isReleaseEB = false;
        public static bool isEB = false;
        public bool isSendToZC = false;
        public bool isSendToATS = true;   //停稳后给ATS发送请求信息   
        UInt32 Link = 0;
        UInt32 Off = 0;
        public byte ZCInfoType = 0;
        public byte actualDirection = 0;
        public UInt16 ATPPermitDirection = 0;
        ZCPackage ZCPackage_;
        SearchDistances Search;
        HashTable hashTable;
        UInt32 tempLink = 0;
        int tempHeadLinkNum = 0;
        Thread thread;
        Thread thread1; //故障注入接口
        Thread thread2;
        MyStruct ZCStruct = new MyStruct();
        MyStruct DMIStruct = new MyStruct();
        MyStruct DCStruct = new MyStruct();
        MyStruct BaliseStruct_ = new MyStruct();
        MyStruct ATSStruct = new MyStruct();
        public List<double> DCTrainSpeedList = new List<double>();  //存储列车的实时速度
        public List<DateTime> DCTrainSpeedTimeList = new List<DateTime>();//存储列车得到速度的时间
        bool isReentry = true;
        bool isBreak = true;
        public bool isFirstEnter = true;
        public byte curModel = 3;
        DateTime recTime = DateTime.Now;
        DateTime firstTime = DateTime.Now;
        public bool isAuthority = true;
        public byte[] sendAts_ = new byte[1024];
        public byte[] SendBuf { get { return sendBuf_; } }
        public bool isPrintATP = false;
        public bool isUnRegister = false;
        public string State = "";
        public string ID = "";
        public string time = "";
        public bool _IsEB = false;
        public Int16 _DCSpeed = 0;
        public UInt16 _DCModel = 0;
        public UInt16 _DCHandle = 0;
        public string _Balise = "";
        public UInt16 _tailID = 0;
        public int _ProtectSpeed = 0;
        public int _Num = 0;
        public string _State = "";
        public string _ID = "";
        public UInt16 _ATPDirection = 0;
        public bool isSaveLog = false;
        public bool isPrintConsole = false;
        string logPath = Application.StartupPath + @"\log\" + DateTime.Today.ToString("yyyyMMdd") + ".txt";
        public int stopNewTime;
        public bool isStop;
        public bool isRecvStopTime=false;
        public string curIsRecvStopTime;
        public  byte zhangJieFault;
        public  byte xiaoJieFault;
        public  Int16 headFault;
        public  string faultReason;
        public byte _zhangJieFault;  //判断是否打印
        public byte _xiaoJieFault;
        public Int16 _headFault;
        public string _faultReason;
        public bool speedFault=false; //速度传感器故障
        public string DCConvertType;
        public bool DCConvertTypeBool = false;
        public bool DMIShow = true;
        public bool isRealeaseEB = true;
        public bool faultRecover = false;
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool AllocConsole();

        // 释放控制台  
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool FreeConsole();

        private static ConsoleColor GetConsoleColor(string time, bool isEB, Int16 DCSpeed, UInt16 DCModel, UInt16 DCHandle, string Balise, int MAEndLink, int ProtectSpeed, int Num, string ID, string State, UInt16 ATPDirection)
        {
            if (isEB == true)
                return ConsoleColor.Red;
            else
                return ConsoleColor.Green;
        }

        public void Start(string ip, int port)
        {
            ZCPackage_ = new ZCPackage();
            Search = new SearchDistances();
            hashTable = new HashTable();
            localPort = port;
            ipLocalPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            RemotePoint = ipLocalPoint;
            socket = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            try
            {
                socket.Bind(ipLocalPoint);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            runningFlag = true;
            thread = new Thread(new ThreadStart(this.ReceiveHandle));
            thread.IsBackground = true;
            thread.Start();
            hashTable.sectionHashTable();
            hashTable.switchHashTable();
            hashTable.Left2is20();
            hashTable.Left2is5();
            hashTable.Is2();
        }

        public void StartFault(string ip, int port)    //故障处理接收的端口
        {
            ipLocalPoint1 = new IPEndPoint(IPAddress.Parse(ip), port);
            RemotePoint1 = ipLocalPoint1;
            socket1 = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            try
            {
                socket1.Bind(ipLocalPoint1);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            thread1 = new Thread(new ThreadStart(this.ReceiveHandleFault));
            thread1.IsBackground = true;
            thread1.Start();
        }

        public void StartATS(string ip, int port)    
        {
            ipLocalPoint2 = new IPEndPoint(IPAddress.Parse(ip), port);
            RemotePoint2 = ipLocalPoint1;
            socket2 = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            try
            {
                socket2.Bind(ipLocalPoint2);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            thread2 = new Thread(new ThreadStart(this.ReceiveHandleATS));   //接收时间
            thread2.IsBackground = true;
            thread2.Start();
        }


        public void ReceiveDataFault(byte[] data)
        {
            
            Stream receStream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(receStream);
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
                if(zhangJieFault == 0 && xiaoJieFault == 0) //速度传感器故障
                {
                    speedFault =true;                  
                }    
                if(zhangJieFault==0 && xiaoJieFault == 1)
                {
                    DMIShow = false;
                }
                if(zhangJieFault==0 && xiaoJieFault == 7)  //EB后不能缓解
                {
                    isRealeaseEB = false;
                }
                isEB = true;
                ATP.Write("\r\n" + "EB" + " " + "Socket类 271行 故障下发" + " " + DateTime.Now.ToString());
                isInFault = true;

            }
           
            //ATP.Write("\r\n" + "故障章节: " + Convert.ToString(zhangJieFault) + "故障小节: " + Convert.ToString(xiaoJieFault) + "故障原因: " + Convert.ToString(falutReason) +"时间: "+DateTime.Now.ToString());
        }

        //public void ReceiveDataATS(byte[] data)    以前方案，没有调用
        //{
        //    ATSStruct.PackedSize = 0;
        //    stopTime = ATSStruct.UnpackUint16(recv);
        //    if (stopTime > 0)
        //    {
        //        isSendToATS = false;
        //    }


        //}
        public void ReceiveHandleFault()
        {
            while (runningFlag)
            {
                try
                {
                    int length1 = socket1.ReceiveFrom(recvFault, ref RemotePoint1); //需要重新定义socket和接口
                    if (length1 > 0)
                    {
                        ReceiveDataFault(recvFault);
                        Array.Clear(recvFault, 0, 1024);
                    }
                }
                catch (Exception e)
                {
                }
            }
        }

        public void ReceiveHandleATS()    //接收时间
        {
            while (runningFlag)
            {
                try
                {
                    EndPoint RemotePoint = new IPEndPoint(IPAddress.Any, 0);
                    int length2 = socket2.ReceiveFrom(recvATS, ref RemotePoint); //需要重新定义socket和接口
                   // ATP.Write("\r\n收到ATS时间");
                    if (length2 > 0)
                    {
                        if (recvATS[2] == 10)
                        {
                            isStop = true;
                            isRecvStopTime = true;//收到停车信息
                            stopNewTime = BitConverter.ToInt32(recvATS, 3);
                            //ATP.Write("\r\n" + "停车信息" + "socket类 281行" + "stopNewTime:" + Convert.ToString(stopNewTime) + " " + DateTime.Now.ToString());
                        }
                        Array.Clear(recvATS, 0, 1024);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
        }

        public void Send(int packageSize, string dIP, int dPort)
        {
            sendPort = dPort;
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(dIP), dPort);
            RemotePoint = (EndPoint)(ipep);
            socket.SendTo(sendBuf_, packageSize, SocketFlags.None, RemotePoint);
        }

        public void SendATS(string dIP, int dPort)
        {

            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(dIP), dPort);
            RemotePoint = (EndPoint)(ipep);
            socket2.SendTo(sendAts_, sendAts_.Length, SocketFlags.None, RemotePoint);  //从哪个socket发出去，就从这个socket绑定的端口发出数据
        }

        public void ReceiveHandle()
        {
            while (runningFlag)
            {
                try
                {
                    int length = socket.ReceiveFrom(recv, ref RemotePoint); //一直在循环，可以不断收到
                    if (length > 0)
                    {
                        ReceiveData(recv);
                        Array.Clear(recv, 0, 1024);
                    }
                }
                catch (Exception e)
                {
                    //MessageBox.Show(Convert.ToString(e));
                   //ATP.Write("\r\n"+"寻路死循环"+" "+"socket类 行"+" "+Convert.ToString(e)+ " " + " " + DateTime.Now.ToString());
                }
            }
        }


        public void ReceiveData(byte[] data)  //所有的消息都发到这一个socket里面，设置[2]，然后设置标志位，收到时间。
        {
            if (recv[2] == 9)//收到ZC数据
            {
                ATP.coutSendZC += 1;
                ZCStruct.PackedSize = 0;
                UInt16 ZCCycle = ZCStruct.UnpackUint16(recv);
                UInt16 ZCPackageType = ZCStruct.UnpackUint16(recv);
                byte ZCSendID = ZCStruct.UnpackByte(recv);
                byte ZCReceiveID = ZCStruct.UnpackByte(recv);
                UInt16 ZCDataLength = ZCStruct.UnpackUint16(recv);
                UInt16 ZCNID_ZC = ZCStruct.UnpackUint16(recv);
                UInt16 ZCNID_Train = ZCStruct.UnpackUint16(recv);
                ZCInfoType = ZCStruct.UnpackByte(recv);
                byte ZCStopEnsure = ZCStruct.UnpackByte(recv);
                UInt64 ZCNID_DataBase = ZCStruct.UnpackUint64(recv);
                UInt16 ZCNID_ARButton = ZCStruct.UnpackUint16(recv);
                byte ZCQ_ARButtonStatus = ZCStruct.UnpackByte(recv);
                UInt16 ZCNID_LoginZCNext = ZCStruct.UnpackUint16(recv);
                byte ZCN_Length = ZCStruct.UnpackByte(recv);
                MAEndType = ZCStruct.UnpackByte(recv);
                //ZCD_MAHeadLink = ZCStruct.UnpackUint16(recv);//
                headSectionOrSwitch = ZCStruct.UnpackByte(recv);
                headID = ZCStruct.UnpackByte(recv);
                UInt32 ZCD_D_MAHeadOff = ZCStruct.UnpackUint32(recv);
                byte ZCQ_MAHeadDir = ZCStruct.UnpackByte(recv);
                //MAEndLink = ZCStruct.UnpackUint16(recv);//
                tailSectionOrSwitch = ZCStruct.UnpackByte(recv);  //1是直轨，2是道岔
                tailID = ZCStruct.UnpackByte(recv);              //MA终点的ID
                //ATP.Write("\r\n" + "MA终点" + "socket类 359行" + "ma终点tailID:" + " " + Convert.ToString(tailID));
                MAEndOff = ZCStruct.UnpackUint32(recv);
                MAEndDir = ZCStruct.UnpackByte(recv);
                obstacleNum = ZCStruct.UnpackByte(recv);

                if(tailSectionOrSwitch==3 && tailID==0 && MAEndOff==0 && MAEndDir == 0)
                {
                    Socket.isEB = true;
                   ATP.Write("\r\n" + "EB" + "socket类 367行 ZC发送3，0，0，0" + " " +Convert.ToString(tailID) + DateTime.Now.ToString());
                }
                if (obstacleNum != 0)
                {
                    State = "";
                    ID = "";
                    obstacleType = new byte[obstacleNum];
                    obstacleID = new string[obstacleNum];
                    obstacleState = new byte[obstacleNum];
                    obstacleLogicState = new byte[obstacleNum];
                    for (int i = 0; i < obstacleNum; i++)
                    {
                        obstacleType[i] = ZCStruct.UnpackByte(recv);
                        obstacleID[i] = (ZCStruct.UnpackUint16(recv)).ToString();
                        obstacleState[i] = ZCStruct.UnpackByte(recv);
                        obstacleLogicState[i] = ZCStruct.UnpackByte(recv);
                        State = State + obstacleState[i] + " ";
                        ID = ID + obstacleID[i] + " ";
                    }
                }
                byte ZCN_TSR = ZCStruct.UnpackByte(recv);
                UInt32 ZCQ_ZC = ZCStruct.UnpackUint32(recv);
                byte ZCEB_Type = ZCStruct.UnpackByte(recv);
                byte ZCEB_DEV_Typ = ZCStruct.UnpackByte(recv);
                UInt16 ZCEB_DEV_Name = ZCStruct.UnpackUint16(recv);
                if (curModel != 4)
                {
                    isRecvZC = true;
                    recTime = DateTime.Now;
                }
            }
            else if (recv[2] == 4)//DMI传来的数据
            {
                DMIStruct.PackedSize = 0;
                UInt16 DMICycle = DMIStruct.UnpackUint16(recv);
                UInt16 DMIPackageType = DMIStruct.UnpackUint16(recv);
                UInt16 DMILength = DMIStruct.UnpackUint16(recv);
                string DMITrainOrder = DMIStruct.UnPackString(recv);
                UInt32 DMITrainNumber = DMIStruct.UnpackDMIUint32(recv);
                UInt16 DMIDriverNumber = DMIStruct.UnpackUint16(recv);
                byte DMITestOrder = DMIStruct.UnpackByte(recv);
                DMIRelieveOrder = DMIStruct.UnpackByte(recv);
            }
            else if (recv[2] == 6)//司控器传来的数据
            {
                DCStruct.PackedSize = 0;
                UInt16 DCCycle = DCStruct.UnpackUint16(recv);
                UInt16 DCPackageType = DCStruct.UnpackUint16(recv);
                UInt16 DCLength = DCStruct.UnpackUint16(recv);
                DCTrainSpeed = (Int16)DCStruct.UnpackUint16(recv);  //解析出列车的实时速度
                DCCtrlMode = DCStruct.UnpackUint16(recv);
                DCHandlePos = DCStruct.UnpackUint16(recv);
                UInt16 DCisKeyIn = DCStruct.UnpackByte(recv);
                if (DCCtrlMode == 1)
                {
                    DateTime dt = DateTime.Now;
                    DCTrainSpeedList.Add(DCTrainSpeed);
                    DCTrainSpeedTimeList.Add(dt);
                }
                if (DCTrainSpeed != 0)
                {
                    isDCFirst = true;
                }
            }
            else if (recv[2] == 5)//应答器传来的数据
            {
                #region 联合大学
                //BaliseStruct_.PackedSize = 0;
                //UInt16 baliseCycle = BaliseStruct_.UnpackUint16(recv);
                //UInt16 balisePackageType = BaliseStruct_.UnpackUint16(recv);
                //string Head = BaliseStruct_.UnPackString(recv);
                //string Tail = BaliseStruct_.UnPackTailString(recv);
                //baliseHead = Regex.Replace(Head, "[T,W]", "", RegexOptions.IgnoreCase);
                //baliseTail = Regex.Replace(Tail, "[T,W]", "", RegexOptions.IgnoreCase);
                #endregion

                #region 沈阳地铁
                BinaryReader br = new BinaryReader(new MemoryStream(recv));
                UInt16 baliseCycle = br.ReadUInt16();
                UInt16 balisePackageType = br.ReadUInt16();
                baliseHead = br.ReadString();
                baliseTail = baliseHead;
                #endregion

                if (Regex.Matches(baliseHead, "Z").Count > 0 && isLeftSearch==false)
                {
                    isBaliseFirst = false;//过滤掉正线之前的应答器，满足这个的时候开始起模
                }
                else
                {
                    isBaliseFirst = true;
                }


            }

            

            //判断ZC是否在规定时间内发来移动授权
            if (isRecvZC == true)
            {
                if (recTime.AddSeconds(1) < DateTime.Now)
                {
                    isAuthority = false;
                }
                else
                {
                    isAuthority = true;
                }
            }

            //选择控车模式
            if (Regex.Matches(baliseHead, "Z").Count > 0 )
            {
                curModel = 3;//正线之前RM
            }
            else
            {
                if (DCCtrlMode == 0)
                {
                    curModel = 1; //AM
                    
                }
                else if (DCCtrlMode == 1)
                {
                    curModel = 2; //CM  
                   
                }
                else if (DCCtrlMode == 2)
                {
                    curModel = 3; //RM                         
                }
                else
                {
                    curModel = 4; //EUM
                  
                }
            }

            //刷到应答器且始计算MA
            if (isBaliseFirst && isDCFirst)//当经过正轨并且司控器的速度不是0的时候刷新
            {
                curBalise = baliseHead;
                trainHead = baliseHead;
                trainTail = baliseTail;
                ReceiceDataHandling();    //刷到应答器才开始画图寻路
            }

            if (isPrintConsole == true)
            {
                //控制台输出
                if (_IsEB != isEB || _DCSpeed != DCTrainSpeed || _DCModel != DCCtrlMode || _DCHandle != DCHandlePos || _Balise != curBalise || _tailID != tailID || _ProtectSpeed != ATP.curProtectionSpeed || _Num != obstacleNum || _ID != ID || _State != State || _ATPDirection != ATPPermitDirection ||_headFault!=headFault || _xiaoJieFault!=xiaoJieFault||_zhangJieFault!=zhangJieFault||_faultReason!=faultReason)
                {
                    AllocConsole();
                    Console.ForegroundColor = GetConsoleColor(time, isEB, DCTrainSpeed, DCCtrlMode, DCHandlePos, curBalise, MAEndLink, _ProtectSpeed, obstacleNum, ID, State, ATPPermitDirection);
                    Console.WriteLine("[{0}] IsEB:{1}  DCSpeed:{2}  DCModel:{3}  DCHandle:{4}  HeadBalise:{5}  tailID:{6}  ProtectSpeed:{7}  Num:{8}  ID:{9}  State:{10}  ATPDirection:{11}  FaultHead:{12} FaultZhangJie:{13} FaultXiaoJie:{14} FaultReason:{15}", DateTime.Now, isEB, DCTrainSpeed, DCCtrlMode, DCHandlePos, curBalise, tailID, ATP.curProtectionSpeed, obstacleNum, ID, State, ATPPermitDirection, headFault, zhangJieFault, xiaoJieFault, faultReason);
                    _IsEB = isEB;
                    _DCSpeed = DCTrainSpeed;
                    _DCModel = DCCtrlMode;
                    _DCHandle = DCHandlePos;
                    _Balise = curBalise;
                    _tailID = tailID;
                    _ProtectSpeed = ATP.curProtectionSpeed;
                    _Num = obstacleNum;
                    _ID = ID;
                    _State = State;
                    _ATPDirection = ATPPermitDirection;
                    _zhangJieFault = zhangJieFault;
                    _xiaoJieFault = xiaoJieFault;
                    _headFault = headFault;
                    if (isSaveLog)
                    {
                        using (StreamWriter sw = File.AppendText(logPath))
                        {
                            sw.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " IsEB: " + isEB + " DCSpeed: " + DCTrainSpeed + " DCModel: " + DCCtrlMode + " DCHandle: " + DCHandlePos + " HeadBalise: " + curBalise + " tailID: " + tailID + " ProtectSpeed: " + ATP.curProtectionSpeed + " Num: " + obstacleNum + " ID: " + ID + " State: " + State + " ATPPermitDirection: " + ATPPermitDirection+ "FaultHead: "+ headFault + "FaultZhangJie: " + zhangJieFault + "FaultXiaoJie: "+ xiaoJieFault + "FaultReason: "+ faultReason);
                        };
                    }
                }
            }

        }

        public void ReceiceDataHandling()
        {
            //列车初始化确定方向和寻路
            if (isFirstEnter)
            {
                if (DCTrainSpeed > 0)//1端有数据
                {
                    isLeftSearch = false;
                    ATPPermitDirection = 1;
                }
                else if (DCTrainSpeed < 0)//2端有数据
                {
                    isLeftSearch = true;
                    ATPPermitDirection = 2;
                }

                isFirstEnter = false;
                isSendToZC = true;
            }

            //在行驶过程中判断方向，若方向不对则EB
            if (curModel != 4)
            {
                if (ATPPermitDirection != 0 && DCTrainSpeed != 0)
                {
                    if (DCTrainSpeed < 0 && DCHandlePos == 2)
                    {
                        ATPPermitDirection = 2;
                        actualDirection = 0xAA;
                        //ATP.Write("\r\n" + "方向变左"+Convert.ToString(574));
                        isLeftSearch = true;
                    }
                    else if (DCTrainSpeed < 0 && DCHandlePos == 1)
                    {
                        isEB = true;
                       ATP.Write("\r\n" + "EB" + "Socket类 573行 行驶方向判断错误" + "DCTrainSpeed:" + Convert.ToString(DCTrainSpeed) + " " + DateTime.Now.ToString());
                    }
                    else if (DCTrainSpeed > 0 && DCHandlePos == 1)
                    {
                        ATPPermitDirection = 1;
                        actualDirection = 0x55;     //由速度得到运行方向
                        isLeftSearch = false;
                    }
                    else
                    {
                        isEB = true;
                       ATP.Write("\r\n" + "EB" + "Socket类 584行 行驶方向判断错误" + " " + DateTime.Now.ToString());
                    }
                }
                if (ATPPermitDirection == 0)
                {
                    if (DCTrainSpeed >= 0 && DCHandlePos == 1)  //只大于0会到else下面，DCHandlePos表示方向，1向右，2向左
                    {
                        ATPPermitDirection = 1;
                        actualDirection = 0x55;
                        isLeftSearch = false;
                    }
                    else if (DCTrainSpeed > 0 && DCHandlePos == 2)
                    {
                        isEB = true;
                        ATP.Write("\r\n" + "EB" + "Socket类 598行 行驶方向判断错误" + "DCTrainSpeed:" + Convert.ToString(DCTrainSpeed) + " " + DateTime.Now.ToString());
                    }
                    else if (DCTrainSpeed < 0 && DCHandlePos == 1)
                    {
                        isEB = true;
                        ATP.Write("\r\n" + "EB" + "Socket类 602行 行驶方向判断错误" + "DCTrainSpeed:" + Convert.ToString(DCTrainSpeed) + " " + DateTime.Now.ToString());
                    }
                    else
                    {
                        ATPPermitDirection = 2;
                        actualDirection = 0xAA;
                    //    ATP.Write("\r\n" + "方向变左" + Convert.ToString(616));
                        isLeftSearch = true;
                    }
                }
            }
            else
            {
                ATPPermitDirection = 0;
                actualDirection = 0;
            }
            CaculateHeadorTailOffandSection();

            if (isRecvStopTime == true)
            {
                if (curIsRecvStopTime.Substring(0,5) != curBalise.Substring(0,5))
                {
                    isRecvStopTime = false;
                 //   ATP.Write("\r\n" + "恢复标志位一次");
                }


            }
            
            if (isRecvStopTime==false && hashTable.ht_4.ContainsKey(curBalise.Substring(0, 5)))
            {
                SendATSFirstCurbalise(curBalise, isLeftSearch);
               // ATP.Write("\r\n"+"请求到站1一次");
            }

            byte currentHeadID = 0;
            //ZC通信后且VOBC在申请MA时，开始处理ATP曲线需要的数据
            if (isRecvZC && runInfoType == 0x01 && isEB == false && curModel != 4)
            {
                isReleaseEB = false;
               
                if (curBalise.Substring(0, 1) == "T")  //如果是T的话在拓扑里面直接是的，可以找到ID
                {
                    foreach (var item in ATP.stationTopoloty_.Nodes)
                    {
                        if (item.NodeDevice.Name == curBalise.Substring(0,5))
                        {
                            currentHeadID = (byte)item.NodeDevice.ID;  //正常区段由应答器位置找到头部对应的ID
                            break;
                        }
                    }
                }
                else
                {
                    foreach (string key in hashTable.ht.Keys) // 当不是T是W的话通过哈希表来找
                    {
                        if (curBalise == key)
                        {
                            string tempID = (string)hashTable.ht[key];
                            currentHeadID = Convert.ToByte(tempID.Substring(tempID.IndexOf("_") + 1)); //道岔的时候找到对应的ID
                        }
                    }
                }
                //if (currentHeadID == tailID)   //应该左寻过了这个节点再EB
                //{
                //    isEB = true;
                //}

                if (currentHeadID==headID)  //目前应答器给我的ID和ZC发的ID一致
                {
                    int[] value = Search.SearchDistance(isLeftSearch, tailSectionOrSwitch, tailID, Convert.ToInt32(MAEndOff), obstacleNum, curBalise, obstacleID, obstacleState);
                    MAEndDistance = value[0];
                    limSpeedNum = value[1];
                    limSpeedDistance_1 = value[2];
                    limSpeedLength_1 = value[3];
                    limSpeedDistance_2 = value[4];
                    limSpeedLength_2 = value[5];
                    limSpeedDistance_3 = value[6];
                    limSpeedLength_3 = value[7];
                    limSpeedDistance_4 = value[9];
                    limSpeedLength_4 = value[10];
                    limitSpeed = value[8];
                   // ATP.Write("\r\nMAENDISTANCE " + Convert.ToString(MAEndDistance) + " " + Convert.ToString(limSpeedDistance_1) + " " + Convert.ToString(limSpeedLength_1)+" "+Convert.ToString(limSpeedNum)+" "+Convert.ToString(tailID));
                }
                //for (int i = 0; i < obstacleNum; i++)
                //{
                //    obstacleID[i] = (ATP.stationElements_.Elements.Find((GraphicElement element) =>
                //    {
                //        if (element is RailSwitch)
                //        {
                //            return (element as RailSwitch).ID == Convert.ToInt32(obstacleID[i]);
                //        }
                //        return false;
                //    }) as RailSwitch).SectionName;


                //    obstacleID[i] = obstacleID[i].Substring(1, 4);
                //}
                if (MAEndDistance > 0)
                {
                    isPrintATP = true;
                }

            }

            //列车正常停车后的处理
            if (currentHeadID== tailID && isSendToZC == true && isEB == false)
            {
                if ((MAEndType == 0x01 || ZCInfoType == 0x05) && isBreak)
                {
                    isBreak = false;
                    SetupTimerBreak();
                }
                else if (MAEndType == 0x02 && ZCInfoType != 0x04)
                {
                    runInfoType = 0x04;
                }
                else if (MAEndType == 0x02 && ZCInfoType == 0x04 && isReentry)
                {
                    isReentry = false;
                    SetupTimerChange();
                }
            }





            //紧急制动在DMI缓解后标志位初始化


            if (DMIRelieveOrder == 2)
            {
                isFirstEnter = true;
                isEB = false;
                isReleaseEB = true;
                isBreak = true;
                isReentry = true;
            }

            if (isReentry == false || isBreak == false)
            {
                if (tailID != currentHeadID)
                {
                    isBreak = true;
                    isReentry = true;
                }
            }

        }
        

        //停车重新启动5s
        System.Timers.Timer timerBreak = new System.Timers.Timer(5000);
        private void SetupTimerBreak()
        {
            timerBreak.Elapsed += TimerBreak_Elapsed;
            timerBreak.AutoReset = false;
            timerBreak.Start();
        }
        private void TimerBreak_Elapsed(object sender, ElapsedEventArgs e)
        {
            runInfoType = 0x01;
            timerBreak.Stop();
        }

        //折返换端5s
        System.Timers.Timer timerChange = new System.Timers.Timer(5000);
        private void SetupTimerChange()
        {
            timerChange.Elapsed += TimerChange_Elapsed;
            timerChange.AutoReset = false;
            timerChange.Start();
        }
        private void TimerChange_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (MAEndType == 0x02)
            {
                if (isLeftSearch == true)
                {
                    isLeftSearch = false;
                    actualDirection = 0x55;
                    ATPPermitDirection = 1;
                }
                else
                {
                    isLeftSearch = true;
                    actualDirection = 0xAA;
                    ATPPermitDirection = 2;
                  //  ATP.Write("\r\n" + "方向变左" + Convert.ToString(782));
                }
                runInfoType = 0x05;
            }
            timerChange.Stop();
        }


        //计算车头车尾区段号，偏移量
        public void CaculateHeadorTailOffandSection()
        {
            if (trainHead.Length != 0 && Regex.Matches(trainHead, "Z").Count == 0)
            {
                UInt32[] value = SectionAndOff(trainHead);     //利用这个来发送偏移量
                HeadSectionOrSwitch = (byte)value[0];
                HeadID = (byte)value[1];
                HeadOff = value[2];               //计算头ID
            }
            if (trainTail.Length != 0 && Regex.Matches(trainTail, "Z").Count == 0)
            {
                UInt32[] value = SectionAndOff(trainTail);
                TailSectionOrSwitch = (byte)value[0];
                TailID = (byte)value[1];
                TailOff = value[2];             //计算尾部ID
            }
        }

        List<string> stationBalise = new List<string>() { "T0107", "T0108", "T0113", "T0201", "T0112", "T0202", "T0211", "T0208", "T0301", "T0302", "T0303", "T0304","T0307","T0401","T0310","T0402","T0405","T0407","T0408","T0410" }; //到站的应答器

        public void SendATSFirstCurbalise(string nowCurbalise, bool isLeft) //发送给ATS数据
        {
            if (isLeft == true)
            {
                if (stationBalise.Contains(nowCurbalise.Substring(0, 5)))
                {

                    if (curModel != 4)
                    {
                        sendAts_ = Encoding.UTF8.GetBytes(ATP.trainID + " " + nowCurbalise.Substring(0, 5) + " " + Convert.ToString(TailOff));
                        //ATP.Write("\r\n" + "发送给ATS偏移量" + "Socket类 826行" + Convert.ToString(TailOff));
                        SendATS(ATP.timeATSIP, Convert.ToInt32(ATP.timeATSPort));
                        curIsRecvStopTime = curBalise;
                        Debug.WriteLine(Convert.ToString(sendAts_));
                    }                        
 
                }
            }
            else
            {
                if (stationBalise.Contains(nowCurbalise.Substring(0, 5)) )
                {
                    if (curModel != 4)
                    {
                        sendAts_ = Encoding.UTF8.GetBytes(ATP.trainID + " " + nowCurbalise.Substring(0, 5) + " " + Convert.ToString(TailOff));
                        //ATP.Write("\r\n" + "发送给ATS偏移量" + "Socket类 826行"+Convert.ToString(TailOff));
                        SendATS(ATP.timeATSIP, Convert.ToInt32(ATP.timeATSPort));
                        curIsRecvStopTime = curBalise;
                        Debug.WriteLine(Convert.ToString(sendAts_));
                    }                      
                    
                }
            }
        }

        //用来寻找ID
        public TopolotyNode findID(string balise)
        {
            if (balise.Substring(0, 1) == "T")
            {
                foreach (var item in ATP.stationTopoloty_.Nodes)//nodes是拓扑图里面的所有节点，由应答器传来的数据来返回相应的节点
                {
                    if (item.NodeDevice.Name == balise.Substring(0, 5))
                    {
                        return item;
                    }
                }
            }
            else
            {
                string name = "";
                foreach (string key in hashTable.ht.Keys)
                {
                    if (curBalise == key)
                    {
                        string tempname = (string)hashTable.ht[key];
                        name = tempname.Substring(0, tempname.IndexOf("_"));
                        break;
                    }
                }

                TopolotyNode node = ATP.stationTopoloty_.Nodes.Find((TopolotyNode toponode) =>   //node是寻找到的节点，返回符合条件的
                {
                    if (toponode.NodeDevice is RailSwitch)  
                    {
                        RailSwitch railSwitch = toponode.NodeDevice as RailSwitch;
                        return railSwitch.Name == name && railSwitch.SectionName == curBalise.Substring(0, 5);
                    }
                    if (toponode.NodeDevice is Section)
                    {
                        Section section = toponode.NodeDevice as Section;
                        return section.Name == name;
                    }
                    return false;
                });

                if (node != null)
                {
                    return node;
                }



                //foreach (var item in ATP.stationTopoloty_.Nodes)
                //{
                //    if (item.NodeDevice.Name == name )
                //    {
                //        return item;
                //    }
                //}
            }
            return null;
        }


        public byte ID_1 = 0;
        public byte SectionOrSwitch = 0;


        public UInt32[] SectionAndOff(string balise)     //寻找偏移量发送
        {
            string balise_1 = balise.Substring(0, 5);
            ID_1 = (byte)findID(balise).NodeDevice.ID;     //在这里找ID

            if (hashTable.ht.Contains(balise) == false)//不是道岔
            {
                SectionOrSwitch = 1;
                foreach (string key in hashTable.ht_1.Keys)
                {
                    if (key == balise.Substring(balise.IndexOf("_") + 1))
                    {
                        if(hashTable.ht_4.ContainsKey(balise.Substring(0, 5)))   //两个道岔，有站台的是两个应答器，没有站台的是4个应答器，对于区段来说
                        {
                            if (isLeftSearch)
                            {
                                if (key == "1_1")
                                {
                                    Off = 100;
                                }
                                else
                                {
                                    Off = 20;
                                }
                            }
                            else
                            {
                                if (key == "1_1")  //修改
                                {
                                    Off = 20;
                                }
                                else
                                {
                                    Off = 100;
                                }
                            }
                        }

                        else        // 直股上四个道岔
                        {
                            if (isLeftSearch)
                            {
                                Off = Convert.ToUInt16(hashTable.ht_1[key]);
                            }
                            else
                            {
                                Off = Convert.ToUInt16(120 - (int)hashTable.ht_1[key]);
                            }
                        }
                        
                    }
                }
            }
            else//是道岔
            {
                SectionOrSwitch = 2;
                Int32 LinkNum = Convert.ToInt32(balise.Substring(balise.IndexOf("_") + 1));
                if (isLeftSearch == false)   //右移的时候
                {
                    if (obstacleState[0] == 2)  //处于反位
                    {

                        if (LinkNum == 2)
                        {
                            if (hashTable.ht_2.ContainsKey(balise.Substring(0, 5)))
                            {
                                Off = 5;
                            }
                            if (hashTable.ht_3.ContainsKey(balise.Substring(0, 5)))
                            {
                                Off = 20;
                            }
                        }
                        else if (LinkNum == 0)
                        {
                            if (hashTable.ht_2.ContainsKey(balise.Substring(0, 5)))
                            {
                                Off = 20;
                            }
                            if (hashTable.ht_3.ContainsKey(balise.Substring(0, 5)))
                            {
                                Off = 5;
                            }
                        }
                        else if (LinkNum == 3)
                        {
                            Off = 5;
                        }
                        else   //等于1时  只有四开的时候定位才经过1
                        {
                            Off = 20;
                        }
                    }
                    else                      //处于定位
                    {
                        if (LinkNum == 1)
                        {
                            if (hashTable.ht_2.ContainsKey(balise.Substring(0, 5)))
                            {
                                Off = 5;
                            }
                            if (hashTable.ht_3.ContainsKey(balise.Substring(0, 5)))
                            {
                                Off = 20;

                            }
                        }
                        else    //linknum是0的时候
                        {
                            if (hashTable.ht_2.ContainsKey(balise.Substring(0, 5)))
                            {
                                Off = 20;
                            }
                            if (hashTable.ht_3.ContainsKey(balise.Substring(0, 5)))
                            {
                                Off = 5;
                            }
                        }
                    }
                }
                else   //左寻
                {
                    if (obstacleState[0] == 2)  //处于反位
                    {
                        if (LinkNum == 2)
                        {
                            if (hashTable.ht_2.ContainsKey(balise.Substring(0, 5)))
                            {
                                Off = 20;    //与右寻相反
                            }
                            if (hashTable.ht_3.ContainsKey(balise.Substring(0, 5)))
                            {
                                Off = 5;
                            }
                        }
                        else if (LinkNum == 0)
                        {
                            if (hashTable.ht_2.ContainsKey(balise.Substring(0, 5)))
                            {
                                Off = 5;
                            }
                            if (hashTable.ht_3.ContainsKey(balise.Substring(0, 5)))
                            {
                                Off = 20;
                            }
                        }
                        else if (LinkNum == 3)
                        {
                            Off = 20;
                        }
                        else   //等于1时  只有四开的时候定位才经过1
                        {
                            Off = 5;
                        }
                    }
                    else                          //处于定位，只会遇见1和0
                    {
                        if (LinkNum == 1)
                        {
                            if (hashTable.ht_2.ContainsKey(balise.Substring(0, 5)))
                            {
                                Off = 20;
                            }
                            if (hashTable.ht_3.ContainsKey(balise.Substring(0, 5)))
                            {
                                Off = 5;

                            }
                        }
                        else    //linknum是0的时候
                        {
                            if (hashTable.ht_2.ContainsKey(balise.Substring(0, 5)))
                            {
                                Off = 5;
                            }
                            if (hashTable.ht_3.ContainsKey(balise.Substring(0, 5)))
                            {
                                Off = 20;
                            }
                        }
                    }
                }
                


            }
            UInt32[] returnValue = new UInt32[3];
            returnValue[0] = SectionOrSwitch;
            returnValue[1] = ID_1;
            returnValue[2] = Off;
            return returnValue;
        }

        public void CloseThread()
        {
            thread.Abort();
        }
        public TopolotyNode find_1(string nodeDeviceName, int nodeDeviceID) //道岔由ID和name找节点，然后就可以左右寻了
        {
            foreach (var item in ATP.stationTopoloty_.Nodes)
            {
                if (item.NodeDevice.Name == nodeDeviceName && item.NodeDevice.ID == nodeDeviceID)
                {
                    return item;
                }
            }
            return null;
        }

        public TopolotyNode find_2(string nodeDeviceName)     //
        {
            foreach (var item in ATP.stationTopoloty_.Nodes)
            {
                if (item.NodeDevice.Name == nodeDeviceName)
                {
                    return item;
                }
            }
            return null;
        }

        public TopolotyNode find_ZG_DC(string nodeDeviceName)     // 为了给ATP用
        {
            foreach (var item in ATP.stationTopoloty_.Nodes)
            {
                if (item.NodeDevice.Name == nodeDeviceName)
                {
                    return item;
                }
            }
            return ATP.stationTopoloty_.Nodes[0];
        }
    }
}
