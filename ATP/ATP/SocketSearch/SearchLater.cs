using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainMessageEB;
using CbtcData;
using ATP;
using System.Text.RegularExpressions;
using ATP.SocketSearch;

namespace SocketSearch
{
    enum SwitchState
    {
        Normal=1,
        Reverse=2
    }
    enum Length
    {
        switchLength=25,
        railLength=120
    }
    class SearchLater
    {
        TrainMessage trainMessage = new TrainMessage();
        HashTable hashTable_search = new HashTable();
        int MAEndDistance = 0;
        UInt32 startOff = 0;
        UInt32 startDistance = 0;
        public string MAEndLink = null;
        int[] result_obstacle_length = new int[4];
        int[] result_obstacle_distance = new int[4];
        string NextSearchBalise = "";
        int index_obstacle = 0; //用于判断目前在哪个障碍物上
        bool isFirstSearch = true;
        private SocketSearchInfo SearchInfo = new SocketSearchInfo();

        public void GetHash()
        {
            trainMessage.GetHash();
            hashTable_search.sikai();
        }

        public int[] SearchDistance(bool isLeftSearch, byte type, byte ID, int MAEndOff, int obstacleNum, string curBalise, string[] obstacleID, byte[] obstacleState) //type,id即MA终点的类型和ID
        {
            
            int curbalise_id = 0;
            int[] returnValue = new int[9];
            MAEndLink = trainMessage.IDTypeConvertName(type, ID); //由type和ID得到区段或道岔名字。如W0103
            string[] obstacleIDName = new string[obstacleNum];  //存放转换成全名的障碍物的名字
            obstacleIDName = ConvertObstacaleIDTOName(obstacleNum, obstacleID);
            curbalise_id = BaliseToID(curBalise);
        
            returnValue =GetMAAndObstacleDistance(curBalise, curbalise_id, MAEndLink, MAEndOff, obstacleID, obstacleState, obstacleNum, isLeftSearch);           
            return returnValue;
        }
        
        public int BaliseToID(string curNowBalise) //由balise可以得到ID
        {
            int ID = 0;           
            if (SearchInfo.IsCurStartWith(curNowBalise, "Z") && !SearchInfo.IsCurBaliseEmpty(curNowBalise))
            {
                ID= Convert.ToInt32(hashTable_search.ht_2[curNowBalise.Substring(0, 4)]);
            }
            else if (!trainMessage.IsRailswitchVoid(curNowBalise)) //如果是直轨
            {
                ID = (trainMessage.CurBaliseToIteam(curNowBalise).device as Section).Id;
            }
            else
            {
                if(hashTable_search.ht_2.Contains(curNowBalise.Substring(0, 5)))
                {
                    if(curNowBalise.Substring(curNowBalise.IndexOf("_")+1,1)=="0" || curNowBalise.Substring(curNowBalise.IndexOf("_")+1, 1) == "2")
                    {
                        ID =Convert.ToInt32(hashTable_search.ht_2[curNowBalise.Substring(0, 5)].ToString().Substring(0,2));
                    }
                    else
                    {
                        ID = Convert.ToInt32(hashTable_search.ht_2[curNowBalise.Substring(0, 5)].ToString().Substring(3, 2));
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

        private int[] allSearch(bool isLeftSearch,string curBalise, byte[] obstacleState, string[] obstacleID, int obstacleNum, int curBalise_id, int MAEndOff)
        {
            int[] obstacle_distance = new int[obstacleNum]; //存放每一个障碍物的位置
            string[] obstacle_name = new string[obstacleNum]; //存放每一个障碍物的名字，用于判断每一个障碍物
            int obstacle_length_private = 0;
            int obstacle_index_private = 0;
            int obstacle_count = 0;
            bool isFirstNoSame = false;
            int last_obstacle_ma_count = 0;
            index_obstacle = 0;
            string NowSearchBalise = curBalise.Substring(0, 5);
            List<TopolotyNode> NextNodesList;
            GetCurbaliseOff(curBalise, isLeftSearch, curBalise_id); //计算开始的偏移量
            while (true)
            {
                if (SearchInfo.IsCurStartWith(NowSearchBalise, "T")) //当目前位置是区段时
                {
                    if (isLeftSearch)
                    {
                        NextNodesList = trainMessage.LeftNextCurBaliseList(NowSearchBalise);  //在区段时肯定只能有一个右节点，道岔可能有两个节点
                    }
                    else
                    {
                        NextNodesList = trainMessage.RightNextCurBaliseList(NowSearchBalise);  //在区段时肯定只能有一个右节点，道岔可能有两个节点
                    }                  
                    NextSearchBalise = trainMessage.NextCurBaliseList(NextNodesList[0]); //取出唯一的一个右节点
                    if (index_obstacle < obstacleState.Count())
                    {
                        GetObstacleLengthDis(NextNodesList, obstacleState, obstacle_distance, obstacle_name, NowSearchBalise);
                    }
                    else
                    {

                        last_obstacle_ma_count += 1; //如果道岔都寻完了，剩下的只剩下区段，算最后的区段数量
                    }
                }
                else
                {
                    if (isLeftSearch)
                    {
                        NextNodesList = trainMessage.LeftNextCurBaliseList(NowSearchBalise, Convert.ToInt32(obstacleID[index_obstacle]));  //在道岔上就需要ID了                     
                    }
                    else
                    {
                        NextNodesList = trainMessage.RightNextCurBaliseList(NowSearchBalise, Convert.ToInt32(obstacleID[index_obstacle]));  //在道岔上就需要ID了                     
                    }
                    
                    if (NextNodesList.Count == 2)
                    {
                        if (obstacleState[index_obstacle] == (UInt16)SwitchState.Normal) //处于定位
                        {
                            searchNextNode2(NextNodesList, NowSearchBalise, obstacleState, obstacleID, obstacle_distance, obstacle_name, 0);
                        }
                        else if (obstacleState[index_obstacle] == (UInt16)SwitchState.Reverse)
                        {
                            searchNextNode2(NextNodesList, NowSearchBalise, obstacleState, obstacleID, obstacle_distance, obstacle_name, 1);
                        }
                    }
                    else //也有可能在道岔上由寻右节点是1
                    {
                        NextSearchBalise = trainMessage.NextCurBaliseList(NextNodesList[0]);
                        GetObstacleLengthDis(NextNodesList, obstacleState, obstacle_distance, obstacle_name, NowSearchBalise);
                        if (index_obstacle < obstacleState.Count())
                        {
                            index_obstacle += 1;
                        }

                    }
                }
                if ((SearchInfo.IsCurStartWith(NowSearchBalise, "W") && !SearchInfo.IsCurStartWith(NextSearchBalise, "W")) ||
                    (SearchInfo.IsCurStartWith(NowSearchBalise, "W") && SearchInfo.Is0_5Maendlink(NextSearchBalise, MAEndLink)) ||
                    (SearchInfo.IsCurStartWith(NowSearchBalise, "T") && SearchInfo.Is0_5Maendlink(NextSearchBalise, MAEndLink) && SearchInfo.IsCurStartWith(MAEndLink, "W"))) //防止W0106
                {
                    result_obstacle_distance[obstacle_index_private] = MAEndDistance; //到终点的距离
                    obstacle_count += 1; //只有这样障碍物的数量才加1
                }
                if (NowSearchBalise != MAEndLink)
                {
                    NowSearchBalise = NextSearchBalise;
                }
                if (SearchInfo.IsCurStartWith(NowSearchBalise, "W") && !SearchInfo.Is0_5Maendlink(NowSearchBalise, MAEndLink))
                {
                    obstacle_length_private += (UInt16)Length.switchLength;
                    isFirstNoSame = true;
                }
                else if (isFirstNoSame)
                {
                    result_obstacle_length[obstacle_index_private] = obstacle_length_private;
                    obstacle_length_private = 0;
                    obstacle_index_private += 1;
                    isFirstNoSame = false;
                }
                if (NowSearchBalise == MAEndLink) //如果下一个是MA终点就终止查询
                {
                    index_obstacle = 0;
                    isFirstSearch = true;
                    break;
                }
            }
            MAEndDistance = MAEndDistance + (UInt16)Length.railLength * last_obstacle_ma_count;
            int[] Value = GetResultList(MAEndDistance, result_obstacle_distance, obstacle_count, MAEndOff, startDistance, result_obstacle_length, curBalise, MAEndLink);
            ToZero();
            return Value;
        }

        private void searchNextNode2(List<TopolotyNode> NextNodesList,string NowSearchBalise, byte[] obstacleState, string[] obstacleID, int[] obstacle_distance, string[] obstacle_name,int index)
        {
            NextSearchBalise = trainMessage.NextCurBaliseList(NextNodesList[index]); //除了下一个应答器==1，做同样的处理
            if (NextSearchBalise == NowSearchBalise)
            {
                GetObstacleLengthDis(NextNodesList, obstacleState, obstacle_distance, obstacle_name, NowSearchBalise);
                obstacleID[index_obstacle] = Convert.ToString(NextNodesList[index].device.Id);
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
        private int[] GetMAAndObstacleDistance(string curBalise,int curBalise_id, string MAEndLink, int MAEndOff, string[] obstacleID, byte[] obstacleState, int obstacleNum, bool isLeftSearch) //寻找障碍物的长度，距离
        {

            int[] endResult = allSearch(isLeftSearch, curBalise, obstacleState, obstacleID, obstacleNum, curBalise_id, MAEndOff);
            return endResult;
        }

        private void ToZero()
        {

            MAEndDistance = 0;
            result_obstacle_distance = new int[4];
            result_obstacle_length = new int[4];
        }

        private void GetObstacleLengthDis(List<TopolotyNode> NextNodesList, byte[] obstacleState, int[] obstacle_distance, string[] obstacle_name,string NowSearchBalise) //while循环用于寻路
        {
            if (!isFirstSearch) //第一次是所在区段，用偏移量算
            {
                if (NextNodesList.Count == 1) //下一个节点是1
                {
                    NextSearchBalise = trainMessage.NextCurBaliseList(NextNodesList[0]); //根据topo节点得到当前名字
                    bool NowisSwitch = trainMessage.IsRailswitchVoid(NowSearchBalise); //目前的位置是不是道岔
                    if (NowisSwitch) //当数量是1时，即可能是道岔也可能是区段
                    {
                        MAEndDistance += (UInt16)Length.switchLength; //不算第一个了
                        obstacle_distance[index_obstacle] = MAEndDistance;
                        obstacle_name[index_obstacle] = NowSearchBalise; //长度有可能是多个道岔连在一起，因此障碍物的长度会变，个数也会减少                                   
                    }

                    else
                    {
                        MAEndDistance += (UInt16)Length.railLength; //如果当前位置不是道岔，那么加120
                        obstacle_distance[index_obstacle] = MAEndDistance;
                    }
                }

                else  //下一个节点是2，只有当前位于道岔上，下一节点才可能是2
                {
                    if (obstacleState[index_obstacle] == (UInt16)SwitchState.Normal)//1是定位，2是反位。定位在第一个，反位在第二个
                    {
                        NextNodesList2(NextNodesList, NowSearchBalise, obstacle_distance, obstacle_name, 0);
                    }
                    else
                    {
                        NextNodesList2(NextNodesList, NowSearchBalise, obstacle_distance, obstacle_name, 1);                    
                    }
                }
            }
            isFirstSearch = false;
        }

        private void NextNodesList2(List<TopolotyNode> NextNodesList, string NowSearchBalise, int[] obstacle_distance, string[] obstacle_name,int index)
        {
            NextSearchBalise = trainMessage.NextCurBaliseList(NextNodesList[index]); //反位是在第二个位置
            MAEndDistance += (UInt16)Length.switchLength;
            obstacle_distance[index_obstacle] = MAEndDistance;
            obstacle_name[index_obstacle] = NowSearchBalise;
        }


        private void GetCurbaliseOff(string curBalise,bool isLeftSearch,int ID)
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

        
        private bool RightISMA(string curbalise,string MAendbalise)
        {
            
            foreach (var one_right in trainMessage.RightNextCurBaliseList(curbalise)) //如果右节点
            {
                if (SearchInfo.IsCurStartWith(one_right.device.Name, "W"))
                {
                    if (MAendbalise == (one_right.device as RailSwitch).section.Name)
                    {
                        return true;
                    }
                }
                else
                {
                    if (MAendbalise == one_right.device.Name)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private int[] GetResultList(int MAEndDistance,int[] obstacle_distance,int limSpeedNum, int MAEndOff, UInt32 startDistance, int[] obstacle_length,string curbalise ,string MAENDlink)
        {
            if(hashTable_search.ht_2.Contains(MAEndLink.Substring(0, 5)))
            {
                MAEndOff = MAEndOff - (UInt16)Length.switchLength;
            }
            for (int i = 0; i < 4 ; i++)
            {
                obstacle_distance[i] = obstacle_distance[i] - obstacle_length[i];//障碍物的距离到的是距离起点的距离，以前的距离包含了障碍物的长度，因此要减去
            }
            int[] returnValue = new int[10];
            switch (limSpeedNum)
            {
                case 0:
                    if (MAENDlink == curbalise.Substring(0,5))
                    {
                        returnValue[0] = (int)startDistance; //距离左边是offset，右边是distance，以前的madistance只是中间的长度
                        returnValue[1] = limSpeedNum;
                    }
                    else if (RightISMA(curbalise, MAENDlink))
                    {
                        returnValue[0] = MAEndOff + (int)startDistance; //距离左边是offset，右边是distance，以前的madistance只是中间的长度
                        returnValue[1] = limSpeedNum;
                    }
                    else
                    {
                        returnValue[0] = MAEndDistance + MAEndOff + (int)startDistance- (UInt16)Length.railLength; //距离左边是offset，右边是distance，以前的madistance只是中间的长度
                        returnValue[1] = limSpeedNum;
                    }   
                    break;
                    
                case 1:
                    GetstartSwitch(returnValue, MAEndDistance, MAEndOff, startDistance, limSpeedNum);
                    if (SearchInfo.IsCurStartWith(curbalise, "W") && !SearchInfo.IsCurBaliseEmpty(curbalise))
                    {
                        returnValue[2] = 0;
                        returnValue[3] = obstacle_length[0]+ (int)startDistance;
                    }
                    
                    else if (SearchInfo.IsCurStartWith(MAEndLink, "W"))
                    {
                        returnValue[2] = obstacle_distance[0] + (int)startDistance;
                        returnValue[3] = obstacle_length[0]+MAEndOff;
                    }
                    else
                    {
                        returnValue[2] = obstacle_distance[0] + (int)startDistance;
                        returnValue[3] = obstacle_length[0];
                    }               
                    break;
                case 2:

                    GetstartSwitch(returnValue, MAEndDistance, MAEndOff, startDistance, limSpeedNum);
                    GetFirstSwitch(curbalise, returnValue, obstacle_distance, obstacle_length,limSpeedNum);
                    GetLastSwitch(curbalise, returnValue, obstacle_distance, obstacle_length,
                         MAEndOff, limSpeedNum);
                   
                    break;
                case 3:
                    GetstartSwitch(returnValue, MAEndDistance, MAEndOff, startDistance, limSpeedNum);
                    GetFirstSwitch(curbalise, returnValue, obstacle_distance, obstacle_length,limSpeedNum);
                    GetindexSwitch(returnValue, 4, 1, obstacle_distance, obstacle_length);
                    GetLastSwitch(curbalise, returnValue, obstacle_distance, obstacle_length,
                        MAEndOff, limSpeedNum);
           
                
                    break;
                case 4:
                    GetstartSwitch(returnValue, MAEndDistance, MAEndOff, startDistance, limSpeedNum);
                    GetFirstSwitch(curbalise, returnValue, obstacle_distance, obstacle_length,limSpeedNum);
                    GetindexSwitch(returnValue, 4, 1, obstacle_distance, obstacle_length);
                    GetindexSwitch(returnValue, 5, 2, obstacle_distance, obstacle_length);             
                    GetLastSwitch(curbalise, returnValue, obstacle_distance, obstacle_length,
                                 MAEndOff, limSpeedNum);                                    
                    break;
                default:
                    break;
            } 
            return returnValue;
        }

        private void GetFirstSwitch(string curbalise, int[] returnValue, int[] obstacle_distance, int[] obstacle_length,int limSpeedNum)
        {
            if (SearchInfo.IsCurStartWith(curbalise, "W") && !SearchInfo.IsCurBaliseEmpty(curbalise))
            {
                if (obstacle_distance[limSpeedNum-1] == 0)
                {
                    returnValue[2] = 0;
                    returnValue[3] = (int)startDistance;
                }
                else
                {
                    returnValue[2] = 0;
                    returnValue[3] = obstacle_length[0] + (int)startDistance;
                }            
            }
            else
            {
                returnValue[2] = obstacle_distance[0] + (int)startDistance;
                returnValue[3] = obstacle_length[0];
            }
        }

        private void GetLastSwitch(string curbalise, int[] returnValue, int[] obstacle_distance, int[] obstacle_length, int MAEndOff,int limSpeedNum)
        {
            if (SearchInfo.IsCurStartWith(MAEndLink, "W"))
            {
                returnValue[2*limSpeedNum] = obstacle_distance[limSpeedNum-1] + (int)startDistance;
                returnValue[2 * limSpeedNum+1] = obstacle_length[limSpeedNum - 1] + MAEndOff;
            }
            else
            {
                if (obstacle_distance[limSpeedNum - 1] == 0)
                {
                    returnValue[2 * limSpeedNum] = obstacle_distance[limSpeedNum - 2] + (int)startDistance;
                    returnValue[2 * limSpeedNum + 1] = obstacle_length[limSpeedNum - 2];
                }
                else
                {
                    returnValue[2 * limSpeedNum] = obstacle_distance[limSpeedNum - 1] + (int)startDistance;
                    returnValue[2 * limSpeedNum + 1] = obstacle_length[limSpeedNum - 1];
                }
            }
        }

        private void GetindexSwitch(int[] returnValue,int listindex,int obstacleindex, int[] obstacle_distance, int[] obstacle_length)
        {
            returnValue[listindex] = obstacle_distance[obstacleindex] + (int)startDistance;
            returnValue[listindex+1] = obstacle_length[obstacleindex];
        }

        private void GetstartSwitch(int[] returnValue,int MAEndDistance, int MAEndOff, UInt32 startDistance,int limSpeedNum)
        {
            returnValue[0] = MAEndDistance + MAEndOff + (int)startDistance; //距离左边是offset，右边是distance，以前的madistance只是中间的长度
            returnValue[1] = limSpeedNum;                                  //障碍物的数量也是传入得到
        }


    }
}
