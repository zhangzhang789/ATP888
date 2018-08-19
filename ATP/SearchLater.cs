using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBTC
{
    class SearchLater
    {
        TrainMessage trainMessage = new TrainMessage();
        int limSpeedNum = 0;
        int limSpeedDistance_1 = 0;
        int limSpeedLength_1 = 0;
        int limSpeedDistance_2 = 0;
        int limSpeedLength_2 = 0;
        int limSpeedDistance_3 = 0;
        int limSpeedLength_3 = 0;
        int limSpeedDistance_4 = 0;
        int limSpeedLength_4 = 0;
        int MAEndDistance = 0;
        string MAEndLink = null;
        public int[] SearchDistance(bool isLeftSearch, byte type, byte ID, int MAEndOff, int obstacleNum, string curBalise, string[] obstacleID, byte[] obstacleState)//type,id即MA终点的类型和ID
        {
            MAEndLink = trainMessage.IDTypeConvertName(type, ID); //由type和ID得到区段或道岔名字。如W0103
            string[] obstacleIDName = new string[obstacleNum];  //存放转换成全名的障碍物的名字
            obstacleIDName = ConvertObstacaleIDTOName(obstacleNum, obstacleID);
            RightGetMAAndObstacleDistance(curBalise, MAEndLink, MAEndOff, obstacleID, obstacleState, obstacleNum, isLeftSearch);
 
            int[] returnValue = new int[9];
            return returnValue;
        }

        private string[] ConvertObstacaleIDTOName(int obstacleNum, string[] obstacleID) //将障碍物名字转换成全名
        {
            string[] obstacleIDName = new string[obstacleNum];
            if (obstacleNum > 0)
            {
                for(int i = 0; i < obstacleNum; i++)
                {
                    obstacleIDName[i] = trainMessage.IDTypeConvertName(2,Convert.ToByte(obstacleID[i]));
                }
            }
            return obstacleIDName;          
        }

        private int[] RightGetMAAndObstacleDistance(string curBalise, string MAEndLink, int MAEndOff, string[] obstacleID, byte[] obstacleState, int obstacleNum, bool isLeftSearch) //寻找障碍物的长度，距离
        {
            int[] returnValue = new int[11];
            string NowSearchBalise= curBalise.Substring(0,5);
            string NextSearchBalise = "";

            if (isLeftSearch == false) //右寻
            {
                while (true)
                {
                    List<线路绘图工具.TopolotyNode> nodes = trainMessage.BaliseToIteam(NowSearchBalise).RightNodes;
                    foreach (var node in nodes)
                    {
                        if(node.NodeDevice is Section)
                        {
                            string x = (node.NodeDevice as Section).Name;
                        }
                        else if (node.NodeDevice is RailSwitch)
                        {
                            string y = (node.NodeDevice as RailSwitch).SectionName;
                        }
                    }
                }
            }
            else   //左寻
            {
                while (true)
                {

                }
            }
         
            return returnValue;
        }




    }
}
