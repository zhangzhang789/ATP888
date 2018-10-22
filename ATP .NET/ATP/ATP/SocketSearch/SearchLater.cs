using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainMessageEB;
using CbtcData;
using ATP;

namespace SocketSearch
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
        UInt32 startOff = 0;
        UInt32 startDistance = 0;
        string MAEndLink = null;
        int[] result_obstacle_length = new int[4];
        int[] result_obstacle_distance = new int[4];
        int result_index = 0;  //记录指针
        string NextSearchBalise = "";
        int index_obstacle = 0; //用于判断目前在哪个障碍物上
        

        public int[] SearchDistance(bool isLeftSearch, byte type, byte ID, int MAEndOff, int obstacleNum, string curBalise, string[] obstacleID, byte[] obstacleState)//type,id即MA终点的类型和ID
        {
            MAEndLink = trainMessage.IDTypeConvertName(type, ID); //由type和ID得到区段或道岔名字。如W0103
            string[] obstacleIDName = new string[obstacleNum];  //存放转换成全名的障碍物的名字
            obstacleIDName = ConvertObstacaleIDTOName(obstacleNum, obstacleID);
            GetMAAndObstacleDistance(curBalise, MAEndLink, MAEndOff, obstacleID, obstacleState, obstacleNum, isLeftSearch);
 
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

        private int[] GetMAAndObstacleDistance(string curBalise, string MAEndLink, int MAEndOff, string[] obstacleID, byte[] obstacleState, int obstacleNum, bool isLeftSearch) //寻找障碍物的长度，距离
        {

            string NowSearchBalise = curBalise.Substring(0, 5);
            int[] obstacle_distance = new int[obstacleNum]; //存放每一个障碍物的位置
            string[] obstacle_name = new string[obstacleNum]; //存放每一个障碍物的名字，用于判断每一个障碍物



            GetCurbaliseOff(curBalise, isLeftSearch); //计算开始的偏移量

            if (isLeftSearch == false) //右寻
            {
                while (true)
                {
                    List<TopolotyNode> NextNodesList = trainMessage.RightNextCurBaliseList(NowSearchBalise);
                    GetObstacleLengthDis(NextNodesList,obstacleState, obstacle_distance, obstacle_name); //和最后要输出的障碍物距离是不一样的，障碍物有可能是连在一起的

                    if (NextSearchBalise == MAEndLink)
                    {
                        break;
                    }
                }
                GetObstacleLength(obstacle_name, obstacle_distance, obstacleNum,ref result_obstacle_length,ref result_obstacle_distance);
                int[] Value = GetResultList(MAEndDistance, result_obstacle_distance, result_index, MAEndOff, startOff, result_obstacle_length);
                return Value;
            }

            else   //左寻
            {
                while (true)
                {
                    List<TopolotyNode> NextNodesList = trainMessage.LeftNextCurBaliseList(NowSearchBalise);
                    GetObstacleLengthDis(NextNodesList, obstacleState, obstacle_distance, obstacle_name);
                    if (NextSearchBalise == MAEndLink)
                    {
                        break;
                    }

                }
                GetObstacleLength(obstacle_name, obstacle_distance, obstacleNum, ref result_obstacle_length, ref result_obstacle_distance);
                int[] Value = GetResultList(MAEndDistance, result_obstacle_distance, result_index, MAEndOff, startOff, result_obstacle_length);
                return Value;
            }          
        }
               
        public void GetObstacleLengthDis(List<TopolotyNode> NextNodesList, byte[] obstacleState, int[] obstacle_distance, string[] obstacle_name) //while循环用于寻路
        {
            if (NextNodesList.Count == 1) //在直轨上，下一个节点肯定是1个
            {
                NextSearchBalise = trainMessage.NextCurBaliseList(NextNodesList[0]);
                bool isSwitch = trainMessage.IsRailswitchVoid(NextSearchBalise);
                if (isSwitch) //当数量是1时，即可能是道岔也可能是区段
                {
                    MAEndDistance += 25;
                    obstacle_name[index_obstacle] = NextSearchBalise; //长度有可能是多个道岔连在一起，因此障碍物的长度会变，个数也会减少
                    obstacle_distance[index_obstacle] = MAEndDistance;
                }
                else
                {
                    MAEndDistance += 120;
                    obstacle_distance[index_obstacle] = MAEndDistance;
                }
            }

            else  //在道岔上根据定反位判断道岔
            {

                if (obstacleState[index_obstacle] == 1)//1是定位，2是反位。定位在第一个，反位在第二个
                {
                    NextSearchBalise = trainMessage.NextCurBaliseList(NextNodesList[0]); //定位是在第一个位置
                    MAEndDistance += 25;  //目前的Distance没有加上偏移量
                    obstacle_distance[index_obstacle] = MAEndDistance;  //遇见一个障碍物存起来此时的距离
                    obstacle_name[index_obstacle] = NextSearchBalise;
                }
                else
                {
                    NextSearchBalise = trainMessage.NextCurBaliseList(NextNodesList[1]); //反位是在第二个位置
                    MAEndDistance += 25;
                    obstacle_distance[index_obstacle] = MAEndDistance;
                    obstacle_name[index_obstacle] = NextSearchBalise;
                }
            }
        }

        public int[] GetObstacleLength(string[] obstacle_name, int[] obstacle_distance, int obstacleNum,ref int[] result_obstacle_length, ref int[] result_obstacle_distance)
        {
            
            int startIndex = 0;
            int result_obstacleNum = 0;
            
            for(int i = 0; i < obstacleNum; i++) //当两个障碍物在连着时，需要把障碍物加起来
            {
                string nextObstacle = (trainMessage.BaliseToIteam(obstacle_name[i]).device as RailSwitch).section.Name; //障碍物肯定是道岔
                if (nextObstacle != obstacle_name[startIndex])
                {
                    int one_obstacle_length = 25 * (i - startIndex);
                    int one_obstacle_distance = obstacle_distance[startIndex];
                    result_obstacle_length[result_index] = one_obstacle_length;
                    result_obstacle_distance[result_index] = one_obstacle_distance;
                    result_index += 1;
                }

            }
            return result_obstacle_distance;
        }

        public void GetCurbaliseOff(string curBalise,bool isLeftSearch)
        {
            bool isSwitch = trainMessage.IsRailswitchVoid(curBalise); //判断是否道岔
            if (isSwitch)
            {
                trainMessage.SwitchGetOffDis(isLeftSearch, curBalise,ref startOff,ref startDistance);
            }
        }

        public int[] GetResultList(int MAEndDistance,int[] obstacle_distance,int limSpeedNum, int MAEndOff, UInt32 startOff, int[] obstacle_length)
        {
            int[] returnValue = new int[10];
            returnValue[0] = MAEndDistance+ MAEndOff+ (int)startOff;
            returnValue[1] = limSpeedNum;
            returnValue[2] = obstacle_distance[0] + MAEndOff + (int)startOff;
            returnValue[3] = obstacle_length[0] + MAEndOff + (int)startOff;
            returnValue[4] = obstacle_distance[1] + MAEndOff + (int)startOff;
            returnValue[5] = obstacle_length[1] + MAEndOff + (int)startOff;
            returnValue[6] = obstacle_distance[2] + MAEndOff + (int)startOff;
            returnValue[7] = obstacle_length[2] + MAEndOff + (int)startOff;
            returnValue[8] = obstacle_distance[3] + MAEndOff + (int)startOff;
            returnValue[9] = obstacle_length[3] + MAEndOff + (int)startOff;
            return returnValue;
        }

    }
}
