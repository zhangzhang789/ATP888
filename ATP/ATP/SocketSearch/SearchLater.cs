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
        HashTable hashTable = new HashTable();
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
        int privateIndexObstacle = 0;
        bool isFirstSearch = true;
        public void GetHash()
        {
            trainMessage.GetHash();
            hashTable.sikai();
        }
        public int[] SearchDistance(bool isLeftSearch, byte type, byte ID, int MAEndOff, int obstacleNum, string curBalise, string[] obstacleID, byte[] obstacleState) //type,id即MA终点的类型和ID
        {
            curBalise = "T0114_1_1"; //"T0114_1_1"
            int curbalise_id = 0;
            int[] returnValue = new int[9];
            MAEndLink = trainMessage.IDTypeConvertName(type, ID); //由type和ID得到区段或道岔名字。如W0103
            string[] obstacleIDName = new string[obstacleNum];  //存放转换成全名的障碍物的名字
            obstacleIDName = ConvertObstacaleIDTOName(obstacleNum, obstacleID);
            curbalise_id = BaliseToID(curBalise);
            returnValue =GetMAAndObstacleDistance(curBalise, curbalise_id, MAEndLink, MAEndOff, obstacleID, obstacleState, obstacleNum, isLeftSearch);           
            return returnValue;
        }

        private int BaliseToID(string curNowBalise)
        {
            int ID = 0;
            if (!trainMessage.IsRailswitchVoid(curNowBalise)) //如果是直轨
            {
                ID = (trainMessage.CurBaliseToIteam(curNowBalise).device as Section).Id;
            }
            else
            {
                if(hashTable.ht_2.Contains(curNowBalise.Substring(0, 5)))
                {
                    if(curNowBalise.Substring(curNowBalise.IndexOf("_")+1,1)=="0" || curNowBalise.Substring(curNowBalise.IndexOf("_")+1, 1) == "2")
                    {
                        ID =Convert.ToInt32(hashTable.ht_2[curNowBalise.Substring(0, 5)].ToString().Substring(0,2));
                    }
                    else
                    {
                        ID = Convert.ToInt32(hashTable.ht_2[curNowBalise.Substring(0, 5)].ToString().Substring(3, 2));
                    }
                }
                else
                {
                    ID = (trainMessage.CurBaliseToIteam(curNowBalise).device as RailSwitch).Id;
                }
              
            }          
            return ID;
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

        private int[] GetMAAndObstacleDistance(string curBalise,int curBalise_id, string MAEndLink, int MAEndOff, string[] obstacleID, byte[] obstacleState, int obstacleNum, bool isLeftSearch) //寻找障碍物的长度，距离
        {

            string NowSearchBalise = curBalise.Substring(0, 5);
            int[] obstacle_distance = new int[obstacleNum]; //存放每一个障碍物的位置
            string[] obstacle_name = new string[obstacleNum]; //存放每一个障碍物的名字，用于判断每一个障碍物
            int obstacle_length_private=0;
            int obstacle_index_private = 0;
            int obstacle_count = 0;
            bool isFirstNoSame = false;
            int last_obstacle_ma_count = 0;
            GetCurbaliseOff(curBalise, isLeftSearch, curBalise_id); //计算开始的偏移量

            if (isLeftSearch == false) //右寻
            {
                List<TopolotyNode> NextNodesList;

                while (true)
                {
                    
                    if (NowSearchBalise.Substring(0,1) == "T")
                    {
                        NextNodesList = trainMessage.RightNextCurBaliseList(NowSearchBalise);
                        NextSearchBalise = trainMessage.NextCurBaliseList(NextNodesList[0]);
                        if (index_obstacle < obstacleState.Count())
                        {
                            GetObstacleLengthDis(NextNodesList, obstacleState, obstacle_distance, obstacle_name, NowSearchBalise);
                        }
                        else
                        {
                            last_obstacle_ma_count += 1;
                        }
                    }
                    else
                    {
                        NextNodesList = trainMessage.RightNextCurBaliseList(NowSearchBalise, Convert.ToInt32(obstacleID[index_obstacle]));                        
                        if (NextNodesList.Count == 2)
                        {
                            if (obstacleState[index_obstacle] == 1)
                            {
                                NextSearchBalise = trainMessage.NextCurBaliseList(NextNodesList[0]);
                                if (NextSearchBalise == NowSearchBalise)
                                {
                                    GetObstacleLengthDis(NextNodesList, obstacleState, obstacle_distance, obstacle_name, NowSearchBalise);
                                    obstacleID[index_obstacle] = Convert.ToString(NextNodesList[0].device.Id);
                                }
                                else
                                {
                                    
                                    GetObstacleLengthDis(NextNodesList, obstacleState, obstacle_distance, obstacle_name, NowSearchBalise);
                                    if (index_obstacle < obstacleState.Count())
                                    {
                                        index_obstacle += 1;
                                    }
                                }
                            }
                            else if (obstacleState[index_obstacle] == 2)
                            {
                                NextSearchBalise = trainMessage.NextCurBaliseList(NextNodesList[1]);
                                if (NextSearchBalise == NowSearchBalise)
                                {
                                    GetObstacleLengthDis(NextNodesList, obstacleState, obstacle_distance, obstacle_name, NowSearchBalise);
                                    obstacleID[index_obstacle] = Convert.ToString(NextNodesList[1].device.Id);
                                }
                                else
                                {
                                    GetObstacleLengthDis(NextNodesList, obstacleState, obstacle_distance, obstacle_name, NowSearchBalise);
                                    if (index_obstacle < obstacleState.Count())
                                    {
                                        index_obstacle += 1;
                                    }
                                }

                            }                            
                        }
                        else
                        {
                            NextSearchBalise = trainMessage.NextCurBaliseList(NextNodesList[0]);
                            GetObstacleLengthDis(NextNodesList, obstacleState, obstacle_distance, obstacle_name, NowSearchBalise);
                            if( index_obstacle<obstacleState.Count())
                            {
                                index_obstacle += 1;
                            }                            
                          
                        }
                    }
                    if (NowSearchBalise.Substring(0, 1) == "W" && NextSearchBalise.Substring(0,1)!="W")
                    {
                        result_obstacle_distance[obstacle_index_private] = MAEndDistance; //到终点的距离
                        obstacle_count += 1;
                    }
                    if(NowSearchBalise.Substring(0,1)=="W")
                    {
                        obstacle_length_private += 25;
                        isFirstNoSame = true;
                    }
                    else if(isFirstNoSame)
                    {
                        result_obstacle_length[obstacle_index_private] = obstacle_length_private;
                        obstacle_length_private = 0;
                        obstacle_index_private += 1;
                        isFirstNoSame = false;
                    }
                    NowSearchBalise = NextSearchBalise;
                    if (NextSearchBalise == MAEndLink)
                    {
                        index_obstacle = 0;
                        isFirstSearch = true;
                        break;
                    }
                }
                MAEndDistance= MAEndDistance+120* last_obstacle_ma_count;
                int[] Value = GetResultList(MAEndDistance, result_obstacle_distance, obstacle_count, MAEndOff, startDistance, result_obstacle_length);
                ToZero();
                return Value;
            }

            else   //左寻
            {
                List<TopolotyNode> NextNodesList;
                while (true)
                {
                    if (NowSearchBalise.Substring(0, 1) == "T")
                    {
                        NextNodesList = trainMessage.LeftNextCurBaliseList(NowSearchBalise);
                        NextSearchBalise = trainMessage.NextCurBaliseList(NextNodesList[0]);
                        if (index_obstacle < obstacleState.Count())
                        {
                            GetObstacleLengthDis(NextNodesList, obstacleState, obstacle_distance, obstacle_name, NowSearchBalise);
                        }
                        else
                        {
                            last_obstacle_ma_count += 1;
                        }
                    }
                    else
                    {
                        NextNodesList = trainMessage.LeftNextCurBaliseList(NowSearchBalise, Convert.ToInt32(obstacleID[index_obstacle]));
                        if (NextNodesList.Count == 2)
                        {
                            if (obstacleState[index_obstacle] == 1)
                            {
                                NextSearchBalise = trainMessage.NextCurBaliseList(NextNodesList[0]);
                                if (NextSearchBalise == NowSearchBalise)
                                {
                                    GetObstacleLengthDis(NextNodesList, obstacleState, obstacle_distance, obstacle_name, NowSearchBalise);
                                    obstacleID[index_obstacle] = Convert.ToString(NextNodesList[0].device.Id);
                                }
                                else
                                {

                                    GetObstacleLengthDis(NextNodesList, obstacleState, obstacle_distance, obstacle_name, NowSearchBalise);
                                    if (index_obstacle < obstacleState.Count())
                                    {
                                        index_obstacle += 1;
                                    }
                                }
                            }
                            else if (obstacleState[index_obstacle] == 2)
                            {
                                NextSearchBalise = trainMessage.NextCurBaliseList(NextNodesList[1]);
                                if (NextSearchBalise == NowSearchBalise)
                                {
                                    GetObstacleLengthDis(NextNodesList, obstacleState, obstacle_distance, obstacle_name, NowSearchBalise);
                                    obstacleID[index_obstacle] = Convert.ToString(NextNodesList[1].device.Id);
                                }
                                else
                                {
                                    GetObstacleLengthDis(NextNodesList, obstacleState, obstacle_distance, obstacle_name, NowSearchBalise);
                                    if (index_obstacle < obstacleState.Count())
                                    {
                                        index_obstacle += 1;
                                    }
                                }

                            }
                        }
                        else
                        {
                            NextSearchBalise = trainMessage.NextCurBaliseList(NextNodesList[0]);
                            GetObstacleLengthDis(NextNodesList, obstacleState, obstacle_distance, obstacle_name, NowSearchBalise);
                            if (index_obstacle < obstacleState.Count())
                            {
                                index_obstacle += 1;
                            }

                        }
                    }
                    if (NowSearchBalise.Substring(0, 1) == "W" && NextSearchBalise.Substring(0, 1) != "W")
                    {
                        result_obstacle_distance[obstacle_index_private] = MAEndDistance; //到终点的距离
                        obstacle_count += 1;
                    }
                    if (NowSearchBalise.Substring(0, 1) == "W")
                    {
                        obstacle_length_private += 25;
                        isFirstNoSame = true;
                    }
                    else if (isFirstNoSame)
                    {
                        result_obstacle_length[obstacle_index_private] = obstacle_length_private;
                        obstacle_length_private = 0;
                        obstacle_index_private += 1;
                        isFirstNoSame = false;
                    }
                    NowSearchBalise = NextSearchBalise;
                    if (NextSearchBalise == MAEndLink)
                    {
                        index_obstacle = 0;
                        isFirstSearch = true;
                        break;
                    }

                }
                
                int[] Value = GetResultList(MAEndDistance, result_obstacle_distance, result_index, MAEndOff, startOff, result_obstacle_length);
                ToZero();
                return Value;
            }          
        }

        public void ToZero()
        {
            limSpeedNum = 0;
            limSpeedDistance_1 = 0;
            limSpeedLength_1 = 0;
            limSpeedDistance_2 = 0;
            limSpeedLength_2 = 0;
            limSpeedDistance_3 = 0;
            limSpeedLength_3 = 0;
            limSpeedDistance_4 = 0;
            limSpeedLength_4 = 0;
            MAEndDistance = 0;
        }

        public void GetObstacleLengthDis(List<TopolotyNode> NextNodesList, byte[] obstacleState, int[] obstacle_distance, string[] obstacle_name,string NowSearchBalise) //while循环用于寻路
        {
            if (!isFirstSearch)
            {
                if (NextNodesList.Count == 1) //下一个节点是1
                {
                    NextSearchBalise = trainMessage.NextCurBaliseList(NextNodesList[0]); //根据topo节点得到当前名字
                    bool NowisSwitch = trainMessage.IsRailswitchVoid(NowSearchBalise);
                    bool NextisSwitch = trainMessage.IsRailswitchVoid(NextSearchBalise);
                    if (NowisSwitch) //当数量是1时，即可能是道岔也可能是区段
                    {
                        MAEndDistance += 25;
                        obstacle_distance[index_obstacle] = MAEndDistance;
                        obstacle_name[index_obstacle] = NowSearchBalise; //长度有可能是多个道岔连在一起，因此障碍物的长度会变，个数也会减少                                   
                    }

                    else
                    {
                        MAEndDistance += 120;
                        obstacle_distance[index_obstacle] = MAEndDistance;
                    }
                }

                else  //下一个节点是2
                {

                    if (obstacleState[index_obstacle] == 1)//1是定位，2是反位。定位在第一个，反位在第二个
                    {
                        NextSearchBalise = trainMessage.NextCurBaliseList(NextNodesList[0]); //定位是在第一个位置
                        MAEndDistance += 25;  //目前的Distance没有加上偏移量
                        obstacle_distance[index_obstacle] = MAEndDistance;  //遇见一个障碍物存起来此时的距离
                        obstacle_name[index_obstacle] = NowSearchBalise;

                    }
                    else
                    {
                        NextSearchBalise = trainMessage.NextCurBaliseList(NextNodesList[1]); //反位是在第二个位置
                        MAEndDistance += 25;
                        obstacle_distance[index_obstacle] = MAEndDistance;
                        obstacle_name[index_obstacle] = NowSearchBalise;

                    }
                }
            }
            isFirstSearch = false;
        }



        public void GetCurbaliseOff(string curBalise,bool isLeftSearch,int ID)
        {
            bool isSwitch = trainMessage.IsRailswitchVoid(curBalise); //判断是否道岔
            if (isSwitch)
            {
                trainMessage.SwitchGetOffDis(isLeftSearch, curBalise,ref startOff,ref startDistance,ID);
            }
            else
            {
                trainMessage.SectionGetOffDis(isLeftSearch, curBalise, ref startOff, ref startDistance,ID);
            }
        }

        public int[] GetResultList(int MAEndDistance,int[] obstacle_distance,int limSpeedNum, int MAEndOff, UInt32 startDistance, int[] obstacle_length)
        {
            for(int i = 0; i < 4; i++)
            {
                obstacle_distance[i] = obstacle_distance[i] - obstacle_length[i];
            }
            int[] returnValue = new int[10];
            returnValue[0] = MAEndDistance+ MAEndOff+ (int)startDistance;
            returnValue[1] = limSpeedNum;
            returnValue[2] = obstacle_distance[0] + (int)startDistance;
            returnValue[3] = obstacle_length[0] ;
            returnValue[4] = obstacle_distance[1] + (int)startDistance;
            returnValue[5] = obstacle_length[1];
            returnValue[6] = obstacle_distance[2] + (int)startDistance;
            returnValue[7] = obstacle_length[2] ;
            returnValue[8] = obstacle_distance[3] + (int)startDistance;
            returnValue[9] = obstacle_length[3];
            return returnValue;
        }

    }
}
