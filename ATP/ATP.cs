using System;
using System.Windows.Forms;
using System.Threading;
using System.Timers;
using System.Diagnostics;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace CBTC
{
    public partial class ATP : Form
    {
        public static string ATPIP = "";
        public static string FalultIP = "";  //定义接收故障类型的IP
        public static string ATPPort = "";
        public static string FalultPort = ""; //定义接收故障类型的端口
        public static string desZCIP = "";
        public static string desZCPort = "";
        public static string desDMIIP = "";
        public static string desDMIPort = "";
        public static string desDCIP = "";
        public static string desDCPort = "";
        public static string sendID = "";
        public static string trainID = "";
        public static string timeATSIP = "";
        public static string timeATSPort = "";   //发往ATS的端口
        public static string desATSIP = "";
        public static string desATSPort = "";    //接收ATS的端口
        ConfigData configdata = new ConfigData();
        Socket socket = new Socket();
        DCPackage DCPackage_;
        DCPackage DCPackageATO_=new DCPackage();
        DMIPackage DMIPackage_;
        ZCPackage ZCPackage_;
        ATSPackage ATSPackage_;
        public double[] x = new double[100];
        public double[] xATO = new double[100];  //为了隔离代码，用来存ATO的位置数据，ATP的三条线只有一个x来存，实际上是一样的
        public double[] y = new double[100];
        public double[] yATO = new double[100];   //用于存ATO的速度数据
        public static StationElements stationElements_;
        public static StationTopoloty stationTopoloty_;
        public List<DateTime> ATOSpeedTimeList = new List<DateTime>();
        bool isCancel = false;
        int limSpeedNum = 0;
        int limSpeedDistance_1 = 0;
        int limSpeedDistance_1_first = 0;
        int limSpeedLength_1 = 0;
        int limSpeedDistance_2 = 0;
        int limSpeedLength_2 = 0;
        int limSpeedDistance_3 = 0;
        int limSpeedLength_3 = 0;
        int limSpeedDistance_4 = 0;
        int limSpeedLength_4 = 0;
        int MAEndDistance = 0;
        int frontLimit = 0;
        int deDistance = (int)((80 / 3.6 * 80 / 3.6 - 40 / 3.6 * 40 / 3.6) / (2 * 1.2));//从80km/h降到40km/h距离，106
        int brDistance = (int)(80 / 3.6 * 80 / 3.6 / (2 * 1.2));//从80km/h降到0的距离，158
        double fourtyDistance = 40 / 3.6 * 40 / 3.6 / 2 / 1.2;//从40km/h降为0的距离，52
        double node_1 = (80 - 40) / 4;//用来产生ATP曲线的弧形
        double node_2 = 80 / 7;//用来产生ATP曲线的弧形
        double node_3 = 80 / 4;//用来产生ATP曲线的弧形  
        double node_4 = 40 / 7;
        public static int curProtectionSpeed = 100;//防护曲线的实时速度
        public static int curProtectionSpeedATO = 0; //当前的ATO速度，用于计算舒适度
        double frontProtectionSpeed = 25;//下一个区段的防护速度
        double speed_1 = 0;//有一个限速区段时未到80km/h顶尖的速度
        double speed_2 = 0;//两个限速区段中间的速度，，过了第一个限速区段后不能够加速到80km/h，只能升一小段然后要下降到第二个限速区间
        double speed_3 = 0;//第二个限速区段和MA之间的速度，过了第二个限速区段后不能加速到80km/h，只能升一小段然后要下降
        double speed_4 = 0;//两个限速区段中间的速度，过了第一个限速区段后不能够加速到80km/h，只能升一小段然后要下降
        double speed_4_1 = 0;
        double speed_5 = 0;//两个限速区段中间的速度，过了第二个限速区段后不能够加速到80km/h，只能升一小段然后要下降到第三个限速区间
        double speed_5_1 = 0;
        double speed_9 = 0;//两个限速区段中间的速度，过了第三个限速区段后不能够加速到80km/h，只能升一小段然后要下降到第四个限速区间
        double speed_9_1 = 0;
        double speed_6 = 0;//第三个限速区段和MA之间的中间速度，过了第三个限速区段后不能加速到80km/h，只能升一小段然后要下降
        double speed_6_1 = 0;
        double speed_10 = 0;//第四个限速区段和MA之间的中间速度，过了第三个限速区段后不能加速到80km/h，只能升一小段然后要下降
        double speed_10_1 = 0;
        double speed_7 = 0;//从起点到第一个限速区间，来不及上升到70中间的速度。
        double speed_8 = 0; //无障碍物
        double speed_8_0 = 0;
        double speed_12 = 0; //无障碍物    
        double speed_13 = 0; //无障碍物  
        double speed_14 = 0; //无障碍物  
        double speed_15 = 0;
        double speed_16 = 0; //无障碍物  
        double speed_17 = 0;
        double speed_18 = 0;
        double distance_first = 0;
        int lockFlag = 1; //0锁定，1开锁   
        int RMPoint = 0;
        double inialProtectSpeed = 15;
        bool isBtnStart = true;
        bool isBtnCancle = true;
        bool isFirst = false;
        bool isConvertCM = false;
        bool isFirstMe = true;
        bool isUPCase0_right = true;         //在Case0是否上升
        bool isUPCase1_right = true;
        bool isUPCase2_right = true;
        bool isUPCase3_right = true;
        bool isUPCase4_right = true;
        bool isUPCase0_left = true;         //在Case0是否上升
        bool isUPCase1_left = true;
        bool isUPCase2_left = true;
        bool isUPCase3_left = true;
        bool isUPCase4_left = true;
        bool is1to0 = true;
        bool is2to0 = true;
        bool is3to0 = true;
        bool is4to0 = true;
        bool isSendDC = true;
        bool isFirst25 = true;
        bool isFirst26 = true;
        bool isSendATOSpeed0 = false;
        bool firstDirectionLeft = false;
        public bool isSendZCBool = true;
        public double totalEnergy;//总共消耗的能源值
        public double trainW=1000;//列车的重量
        public double da;//当前列车的舒适度
        public List<double> EnergyList=new List<double>();//存放实时总能源的列表
        public List<double> ATOEnergyList = new List<double>();//存放ATO总能源的列表
        public List<double> curProtectionSpeedATOList ; //当前的ATO速度，用于计算舒适度
        public List<double> DaList = new List<double>(); //存放实时舒适度的列表
        public List<double> ATODaList = new List<double>(); //存放ATO舒适度的列表
        public static bool isSendTrain;
        double halfToFourtyDis=0;
        double zeroToHalfDis=0;
        double lineDisCase1 = 0;       //在只有一个障碍物时，最开始的直线位置
        double lineDisCase0 = 0;
        double upDownCase0 = 0;
        double upDownCase0_L = 0;
        public static int coutSendZC = 0;
        Comfort comfort = new Comfort();
        public List<double> ATOSpeedList = new List<double>();  //记录ATO实时速度的列表
        Thread stopThread;
        HashTable hashTable_is2_20=new HashTable();

        public ATP()
        {
            InitializeComponent();
            StationElements ele = StationElements.Open("StationElements.xml", null);
            LoadGraphicElements("StationElements.xml");
            LoadStationTopo("StationTopoloty.xml");
            LoadSecondStation();
            CheckForIllegalCrossThreadCalls = false;
            configdata.ReadConfigData();
            this.KeyPreview = true;  //构造函数加载信息          
        }
        public static void Write(string text)
        {
            try
            {
                FileStream fs = new FileStream(".\\log.txt", FileMode.Append);
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(text);
                sw.Close();
                fs.Close();
            }
            catch
            {

            }
           
        }
        private void ATP_Load(object sender, EventArgs e)    //加载ATP控件
        {
            DCPackage_ = new DCPackage() { PackageType = 5 };
            DMIPackage_ = new DMIPackage() { PackageType = 3, ActulSpeed = 25, TrainNum = "" };
            ZCPackage_ = new ZCPackage() { PackageType = 8, ReceiveID = 3, ZCID = 3 };
            ATSPackage_ = new ATSPackage() ;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (isBtnStart)
            {
                hashTable_is2_20.Is2();
                hashTable_is2_20.Left2is20();
                hashTable_is2_20.Left2is5();
                IsSendZCTimer();    //给ZC计时
                socket.Start(ATPIP, Convert.ToInt32(ATPPort));   //开始绑定端口并接受线路信息
                socket.StartFault(FalultIP, Convert.ToInt32(FalultPort));//开始绑定端口，接收故障信息的IP和端口
                socket.StartATS(desATSIP, Convert.ToInt32(desATSPort));  //从des这个端口接收ATS的停站信息
                SetupTimer();                                   //开启一个定时器线程用于发包
                timer1.Enabled = true;
                timer1.Start();                               //用系统自带的定时器用于主线程的画图
                btnStart.ForeColor = Color.Red;
                isBtnStart = false;                          //只能用一次              
            }
        }     
                                                                 
        private void btnCancel_Click(object sender, EventArgs e) //注销模式
        {
            isBtnCancle = false;
            btnCancel.ForeColor = Color.Red;
        }

        private void SetupTimer()  //发包的方法
        {
            System.Timers.Timer timer = new System.Timers.Timer(200);
            timer.Elapsed += TimerElapsed;
            timer.Start();
        }
        public void IsSendZCTimer()
        {
            System.Timers.Timer timer = new System.Timers.Timer(3000);
            timer.Elapsed += IsSendZC;
            timer.Start();
        }
        int inTimer = 0;
        private void TimerElapsed(object sender, ElapsedEventArgs e) //委托给发包的方法，定时器不断判断，在那里recv[2]收到数据赋值，设置标志位要停车，然后在这里停车
        {
            if (Interlocked.Exchange(ref inTimer, 1) == 0)
            {
                if (Socket.isInFault == true)
                {
                    textBox6.Text = "故障章节 ：" + Convert.ToString(socket.zhangJieFault) + "\r\n" + "故障小节：" + Convert.ToString(socket.xiaoJieFault) + "\r\n" + "故障原因：" + Convert.ToString(socket.faultReason);
                }
                else
                {
                    textBox6.Text="";
                }

                if (socket.faultRecover == true)
                {
                    textBox7.Text = "";
                }
                else if (socket.speedFault==true)
                {
                    textBox7.Text = "速度传感器故障，请切入EUM模式回库";
                }

                else if (socket.zhangJieFault == 0 && socket.xiaoJieFault == 1)
                {
                    textBox7.Text = "DMI故障，请切入EUM模式回库";
                }

                else if(socket.zhangJieFault==0 && socket.xiaoJieFault == 2)
                {
                    textBox7.Text = "ATP主机故障，请切入EUM模式回库";
                }

                else if (socket.zhangJieFault == 0 && socket.xiaoJieFault == 4)
                {
                    textBox7.Text = "应答器主机故障，请切入RM模式回库";
                }

                else if (socket.zhangJieFault == 0 && socket.xiaoJieFault == 5)
                {
                    textBox7.Text = "雷达传感器故障，请切入EUM模式回库";
                }

                else if (socket.zhangJieFault == 0 && socket.xiaoJieFault == 6)
                {
                    textBox7.Text = "ATP主机故障，请切入EUM模式回库";
                }

                else if (socket.zhangJieFault == 0 && socket.xiaoJieFault == 7)
                {
                    textBox7.Text = "ATP主机故障，请切入EUM模式回库";
                }

                if (socket.curModel != 3)
                {
                    DCPackage_.HighSpeed = (UInt16)curProtectionSpeed;
                }
                else
                {
                    DCPackage_.HighSpeed = 15;
                    if (Math.Abs(socket.DCTrainSpeed) > 15)
                    {
                        if((socket.curBalise.Substring(0,3)!="ZHG" && socket.curBalise != "T0115_1_1" && socket.curBalise != "T0114_1_1") && socket.isLeftSearch == true)
                        {
                            Socket.isEB = true;
                            Write("\r\n" + "EB" + " " + "ATP类 276行 CM模式下速度超过15" + " " + DateTime.Now.ToString());
                        }
                        
                    }
                }
             
                DCPackage_.PermitSpeed = (UInt16)(curProtectionSpeed - 5);
                DCPackage_.InterSpeed = (UInt16)(curProtectionSpeed - 2);
                DCPackage_.Direction = socket.ATPPermitDirection;

                DMIPackage_.TrainID = 65536;
                DMIPackage_.TrainNum = "T0" + trainID;
                DMIPackage_.HighModel = 1;
                DMIPackage_.CurModel = socket.curModel;

                if (isCancel == true || socket.speedFault==true)  //当速度传感器故障发生时，DMI显示速度是0
                {
                    DMIPackage_.ActulSpeed = 0;
                }
                else
                {
                    DMIPackage_.ActulSpeed = (UInt16)System.Math.Abs(socket.DCTrainSpeed);
                }

                ZCPackage_.SendID = Convert.ToByte(sendID);
                //ZCPackage_.TrainID = Convert.ToByte(trainID);
                ZCPackage_.TrainID = Convert.ToByte(trainID);

                if (isBtnCancle == false || socket.isUnRegister == true)
                {
                    socket.isFirstEnter = true;
                    //socket.isSendToZC = false;
                    socket.isDCFirst = false;
                    socket.isBaliseFirst = false;
                    ZCPackage_.RunInformation = 0x03;
                }
                else
                {
                    ZCPackage_.RunInformation = socket.runInfoType;
                }
  

                if (socket.DMIRelieveOrder == 2)  //缓解后发这个
                {
                    DCPackage_.IsEB = 7;
                    DMIPackage_.BreakOut = 7;
                    RMPoint = 0;
                    isFirst = true;
                    Socket.isEB = false;
                   
                }
                //if (socket.curBalise == socket.tailID)
                //{
                //    socket.isEB = true;
                //}

                if (frontLimit == 40)
                {
                    DCPackage_.NextSpeed = (40 - 5);
                }
                //Write("\r\n" + "ATP类 240行 速度防护" + " " + "ATP速度：" + Convert.ToString(curProtectionSpeed) + " " + "司控器速度：" + Convert.ToString(System.Math.Abs(socket.DCTrainSpeed)) );
                if (curProtectionSpeed< (UInt16)System.Math.Abs(socket.DCTrainSpeed) && socket.curModel != 4 && socket.curModel!=1)  //司控器那边为了不让初始值为0加了一个1
                {
                    Socket.isEB = true;   //手柄目前没有作用
                    Write("\r\n"+"EB"+ " " + "ATP类 234行 速度超过ATP速度" + " " + "ATP速度：" + Convert.ToString(curProtectionSpeed + 20) + " " + "司控器速度：" + Convert.ToString(System.Math.Abs(socket.DCTrainSpeed)) + " " + "socket模式：" + Convert.ToString(socket.curModel) + " " + DateTime.Now.ToString());
                    DCPackage_.IsEB = 6;
                    DMIPackage_.BreakOut = 6;
                    DMIPackage_.Alarm = 1;                    
                }


                if (Socket.isEB == true || isCancel == true || socket.isReleaseEB == true)
                {
                    if (socket.curModel != 4 && isCancel == false && socket.isReleaseEB == false)
                    {
                        DCPackage_.IsEB = 6;
                        DMIPackage_.BreakOut = 6;
                        DMIPackage_.Alarm = 1;
                    }
                    DMIPackage_.HighSpeed = 0;
                    DMIPackage_.PermitSpeed = 0;
                    DMIPackage_.FrontPermSpeed = 0;
                    DMIPackage_.TargetLoca = 0;
                }
                else
                {
                    if (curProtectionSpeed - 5 < 0)
                    {
                        DMIPackage_.PermitSpeed = 0;
                        DMIPackage_.FrontPermSpeed = 0;
                    }
                    else
                    {
                        DMIPackage_.PermitSpeed = (UInt16)(curProtectionSpeed - 5);
                        DMIPackage_.FrontPermSpeed = (UInt16)(frontProtectionSpeed - 5);
                    }
                    if (socket.curModel != 3)
                    {
                        DMIPackage_.HighSpeed = (UInt16)curProtectionSpeed;
                    }
                    else
                    {
                        DMIPackage_.HighSpeed = 15;
                        if (Math.Abs(socket.DCTrainSpeed) > 15)
                        {
                            if ((socket.curBalise.Substring(0, 3) != "ZHG" && socket.curBalise != "T0115_1_1" && socket.curBalise != "T0114_1_1") && socket.isLeftSearch == true)
                            {
                                Socket.isEB = true;
                                Write("\r\n" + "EB" + " " + "ATP类 276行 CM模式下速度超过15" + " " + DateTime.Now.ToString());
                            }
                        }
                    }
                    DMIPackage_.TargetLoca = (UInt16)Socket.MAEndDistance;
                }

               
                if (isBtnCancle == false && socket.ZCInfoType == 0x03)
                {
                    isCancel = true;
                    socket.isSendToZC = false;
                }
                if (socket.curModel == 4)
                {
                    DMIPackage_.HighSpeed = 0;
                    DMIPackage_.PermitSpeed = 0;
                    DMIPackage_.FrontPermSpeed = 0;
                }
                if (socket.DMIShow == false) //DMI不显示的时候是发1
                {
                    DMIPackage_.Dmishow = 1;
                }
                else
                {
                    DMIPackage_.Dmishow = 2;
                }
                
                
                
                int DCPackageSize = DCPackage_.Pack(socket.SendBuf);
                if (isSendDC)
                {
                    socket.Send(DCPackageSize, desDCIP, Convert.ToInt32(desDCPort));
                    //Write("\r\n308发送司控器速度");
                }

                if (socket.isStop == true)
                {
                    ZCPackage_.DoorState_ = 1;
                    //ATP.Write("\r\n" + "开门状态" + Convert.ToString(1));
                    DMIPackage_.Hint = 0;  //0的时候开门  是否开门
                    DMIPackage_.IsSendTrain = 0;
                }
                else
                {
                    ZCPackage_.DoorState_ = 2;
                    //ATP.Write("\r\n" + "关门状态" + Convert.ToString(2));
                    DMIPackage_.Hint = 1;
                    DMIPackage_.IsSendTrain = 1;
                }

                if (socket.curBalise != "")
                {
                    if (socket.curBalise.Substring(0, 3) == "ZHG")
                    {
                        DMIPackage_.IsNoZHG = 0;
                    }
                    else
                    {
                        DMIPackage_.IsNoZHG = 1;
                    }
                }
                

                if (isSendZCBool == true)
                {
                    DMIPackage_.IsCBTC = 0;
                }
                else
                {
                    DMIPackage_.IsCBTC = 1;
                }

                DMIPackage_.FaultType = 1;
                if (Socket.isInFault == true)
                {
                    DMIPackage_.FaultType = 2;
                }
                else if (isSendZCBool == false)
                {
                    DMIPackage_.FaultType = 3;
                }
                if(Socket.isInFault==true && Socket.isEB == false)
                {
                    Socket.isInFault = false;
                }
                if (socket.isRealeaseEB == false)  //等于1的时候不能缓解
                {
                    DMIPackage_.IsRealeaseEB = 1;
                }
                else
                {
                    DMIPackage_.IsRealeaseEB = 2;
                }

               
               
                int DMIPackageSize = DMIPackage_.Pack(socket.SendBuf);
                socket.Send(DMIPackageSize,desDMIIP, Convert.ToInt32(desDMIPort));
                ZCPackage_.HeadSectionOrSwitch = socket.HeadSectionOrSwitch;
                ZCPackage_.HeadID = socket.HeadID;
                ZCPackage_.HeadOff = socket.HeadOff;  //发送给ZC偏移量
                //ATP.Write("\r\n" + "发送ZC偏移量" + "ATP类 326行" + "off:" +" "+ Convert.ToString(socket.HeadOff));
                Debug.Write(socket.HeadSectionOrSwitch+" "+ socket.HeadID+" "+ socket.HeadOff+"\r\n");
                ZCPackage_.TailSectionOrSwitch = socket.TailSectionOrSwitch;
                if (Socket.baliseHead != "")
                {
                    if (Socket.baliseHead.Substring(0, 4) == "ZHG1" && socket.isLeftSearch == true)
                    {
                        socket.TailID = 9;
                        if(Socket.baliseHead.Substring(5, 3) == "2_2")
                        {
                            socket.TailOff=20;
                        }
                        else if (Socket.baliseHead.Substring(5, 3) == "2_1")
                        {
                            socket.TailOff = 40;
                        }
                        else if (Socket.baliseHead.Substring(5, 3) == "1_2")
                        {
                            socket.TailOff = 80;
                        }
                        else if (Socket.baliseHead.Substring(5, 3) == "1_1")
                        {
                            socket.TailOff = 100;
                        }
                    }
                    if (Socket.baliseHead.Substring(0, 4) == "ZHG2" && socket.isLeftSearch == true)
                    {
                        socket.TailID = 10;
                        if (Socket.baliseHead.Substring(5, 3) == "2_2")
                        {
                            socket.TailOff = 20;
                        }
                        else if (Socket.baliseHead.Substring(5, 3) == "2_1")
                        {
                            socket.TailOff = 40;
                        }
                        else if (Socket.baliseHead.Substring(5, 3) == "1_2")
                        {
                            socket.TailOff = 80;
                        }
                        else if (Socket.baliseHead.Substring(5, 3) == "1_1")
                        {
                            socket.TailOff = 100;
                        }
                    }
                }              
                ZCPackage_.TailID = socket.TailID;
                ZCPackage_.TailOff = socket.TailOff; //大写的是自己找的
                
                ZCPackage_.ACtSpeed = (UInt16)Math.Abs(socket.DCTrainSpeed);
                ZCPackage_.HeadActDirection = socket.actualDirection;
                ZCPackage_.Mode = socket.curModel;
   

                
                int ZCPackageSize = ZCPackage_.Pack(socket.SendBuf);
                if (socket.isSendToZC == true)//&& socket.curModel!=4
                {
                    socket.Send(ZCPackageSize, desZCIP, Convert.ToInt32(desZCPort));
                    if (isBtnCancle == false)//注销后这个等于false
                    {
                        socket.isSendToZC = false;
                    }
                }






 



            }


            Interlocked.Exchange(ref inTimer, 0);

        }

        
        private void ATP_KeyUp(object sender, KeyEventArgs e)  //是否退出程序
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult dr = MessageBox.Show("确定要退出程序吗？", "退出", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.Yes)
                {
                    try
                    {
                        socket.CloseThread();
                    }
                    catch (Exception a)
                    {
                    }
                    this.Close();
                }
            }
        }

        private void LoadSecondStation()  //扩谱图的连接
        {
            StationElements elements_1 = StationElements.Open("StationElements.xml", null);
            stationTopoloty_ = new StationTopoloty();
            stationTopoloty_.Open("StationTopoloty.xml", elements_1.Elements);

        }


        private void LoadStationTopo(string path)
        {
            stationTopoloty_ = new StationTopoloty();
            stationTopoloty_.Open(path, stationElements_.Elements);
        }

        private void LoadGraphicElements(string path)
        {
            stationElements_ = StationElements.Open(path, null);
        }


        private void saveLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            socket.isSaveLog = true;
            socket.isPrintConsole = true;
        }

        private void notSaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            socket.isSaveLog = false;
            socket.isPrintConsole = true;
        }



        private void button1_Click(object sender, EventArgs e)
        {

                      
                AssessForm assessForm = new AssessForm(this, 20, 10, 2, 20);
                assessForm.ShowDialog();
         
            
        }

        public void calATOATP()
        {
            if (socket.DCTrainSpeedList.Count >= 6)   //计算手动的舒适度
            {
                double v = socket.DCTrainSpeedList[socket.DCTrainSpeedList.Count - 1];
                double prev = socket.DCTrainSpeedList[socket.DCTrainSpeedList.Count - 3];
                double v1 = socket.DCTrainSpeedList[socket.DCTrainSpeedList.Count - 4];
                double v2 = socket.DCTrainSpeedList[socket.DCTrainSpeedList.Count - 6];
                DateTime t = socket.DCTrainSpeedTimeList[socket.DCTrainSpeedTimeList.Count - 1];
                DateTime pret = socket.DCTrainSpeedTimeList[socket.DCTrainSpeedTimeList.Count - 3];
                DateTime t1 = socket.DCTrainSpeedTimeList[socket.DCTrainSpeedTimeList.Count - 4];
                DateTime t2 = socket.DCTrainSpeedTimeList[socket.DCTrainSpeedTimeList.Count - 6];
                double a = (v - prev) / (t - pret).TotalSeconds;
                double prea = (v1 - v2) / (t1 - t2).TotalSeconds;
                comfort.CalculateEDa(v, prev, a, prea);
                textBox1.Text = Convert.ToString((Int64)comfort.totalEnergy);
                if (socket.DCTrainSpeed != 0)
                {
                    textBox2.Text = Convert.ToString((int)comfort.da);
                }
                else
                {
                    textBox2.Text = Convert.ToString(0);
                }                  
                 
                EnergyList.Add(comfort.totalEnergy);
                DaList.Add(comfort.da);
                socket.DCTrainSpeedList.Clear();

            }


            if (ATOSpeedList.Count >= 10) //计算在ATO情况下列车的舒适度
            {
                double v = ATOSpeedList[ATOSpeedList.Count - 1];
                double prev = ATOSpeedList[ATOSpeedList.Count - 4];
                double v1 = ATOSpeedList[ATOSpeedList.Count - 5];
                double v2 = ATOSpeedList[ATOSpeedList.Count - 9];
                double a = (v - prev);
                double prea = (v1 - v2);
                comfort.ATOCalculateEDa(v, prev, a, prea);
                textBox3.Text = Convert.ToString((int)comfort.ATOtotalEnergy);
                ATOEnergyList.Add(comfort.ATOtotalEnergy);
                if (socket.DCTrainSpeed != 0)
                {
                    textBox4.Text = Convert.ToString((int)comfort.ATOda);
                }
                else
                {
                    textBox4.Text = Convert.ToString(0);
                }
               
                ATODaList.Add(comfort.ATOda);
                ATOSpeedList.Clear();
                socket.DCTrainSpeedList.Clear();
                socket.DCTrainSpeedTimeList.Clear();
            }

        }

        private void IsSendZC(object sender, ElapsedEventArgs e)
        {
            if (coutSendZC == 0)
            {
                isSendZCBool = false;
            }
            else
            {
                isSendZCBool = true;
            }
            coutSendZC = 0;
        }

      
    }
}
