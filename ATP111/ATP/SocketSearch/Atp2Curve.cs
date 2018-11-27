using ConfigData;
using Package;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ATP.SocketSearch
{
    class Atp2Curve : Atp2OtherSystem
    {
        //public UdpClient ATPToATPCurveClient;
        public ATPCurvePackage atpCurvePackage = new ATPCurvePackage();

        public override void Initialize()
        {
            CreateSocket("ATPCurve");
        }

        internal void SetFaultReason(byte zhangJieFault, byte xiaoJieFault)
        {
            string[] strFaultReasons = new string[]
            {
                "",
                "DMI故障，请切入EUM模式回库",
                "ATP主机故障，请切入EUM模式回库",
                "",
                "应答器主机故障，请切入RM模式回库",
                "雷达传感器故障，请切入EUM模式回库",
                "ATP主机故障，请切入EUM模式回库",
                "ATP主机故障，请切入EUM模式回库",
            };

            if (zhangJieFault == 0 && (xiaoJieFault > 0 && xiaoJieFault < 8))
            {
                atpCurvePackage.faultReason = strFaultReasons[xiaoJieFault];
            }
        }

        public void SendATPCurve(SpeedLimit speedLimit, bool isInFault, bool isEb, 
            byte zhangJieFault, byte xiaoJieFault, string faultReason, bool faultRecover, bool speedFault)
        {
            Atp2Curve curve = this;

            curve.atpCurvePackage.MAEndDistance = speedLimit.MAEndDistance;
            curve.atpCurvePackage.limSpeedNum = speedLimit.limSpeedNum;

            for (int i = 0; i < 4; i++)
            {
                curve.atpCurvePackage.limSpeedDistance[i] = speedLimit.limSpeedDistance[i];
                curve.atpCurvePackage.limSpeedLength[i] = speedLimit.limSpeedLength[i];
            }

            if (isInFault == true && isEb == false) //收到后并且不EB了说明不处于故障了
            {
                isInFault = false;
            }

            if (isInFault == true)
            {
                curve.atpCurvePackage.faultPostion = "故障章节 ：" +
                    Convert.ToString(zhangJieFault) + "\r\n" + "故障小节：" +
                    Convert.ToString(xiaoJieFault) + "\r\n" + "故障原因：" + Convert.ToString(faultReason);
            }
            else
            {
                curve.atpCurvePackage.faultPostion = "";
            }

            if (faultRecover == true)
            {
                curve.atpCurvePackage.faultReason = "";
            }
            else if (speedFault == true)
            {
                curve.atpCurvePackage.faultReason = "速度传感器故障，请切入EUM模式回库";
            }

            curve.SetFaultReason(zhangJieFault, xiaoJieFault);
            byte[] ATPCurveSendData = curve.atpCurvePackage.ATPCurvePackStream();
            client.Send(ATPCurveSendData, 1024);
        }
    }
}
