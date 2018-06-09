using System;
using System.Linq;
using System.Windows;
using System.Collections;
using 线路绘图工具;
using System.Diagnostics;

namespace CBTC
{
    class SearchDistances
    {
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
        int[] limSpeedDistance = new int[10];
        int[] limSpeedLength = new int[10];
        int[] retuenValue = new int[11];
        int[] nodeID = new int[2];//存储每一次索引的区段ID
        int i = 0;
        int j = 0;//用来放道岔DG
        int reverseNum = 0;//经过的处于反位状态的障碍物的数量
        int count = 0;//用来统计经过多少障碍物        
        int offset = 0;//距离区段的当前的偏移量   
        int curNodeID = 0;//道岔时从应答器解的区段ID    
        int distance_1 = 0;//距当前区段端点的距离
        int distance_2 = 0;//当前区段距离MA的距离  
        int tempLink = 0;
        int tempLinkNum = 0;
        int nextLimitSpeed = 0;
        int firstReverseNum = 0;
        int Namelength = 0;//判断当前所在区段是道岔还是区段,在列车是道岔时索引距离时使用   
        string curNodeName = "";//从应答器解的区段     
        string[] nodeName = new string[2];//存储每一次索引的区段
        string[] switchName = new string[2];//存储道岔的区段号
        string tempSwitchName_1 = null;//临时存储道岔的区段号
        string tempSwitchName_2 = null;//临时存储道岔的区段号
        string switchConvert = "";
        string firstSwitchName = "";//在列车经过道岔时索引距离时使用
        string firstRverseObstacle = "";//第一个处于反位的道岔，W0104
        bool isSwitchMAEnd = false;//MA终点是否是道岔                  
        bool isFirstBalise_1 = false;//用来列车在道岔时寻路时使用
        bool isBreak = false;//用来跳出索引循环    
        bool isFirst = true;//列车在道岔时计算偏移量
        bool isNextLimitSpeed = true;
        HashTable hashTable;
        string MAendLinkName = "";
        string MAEndLink = null;
        //curBalise是司控器传进来未经处理的应答器，W开头的
        public int[] SearchDistance(bool isLeftSearch, byte type, byte ID, int MAEndOff, int obstacleNum, string curBalise, string[] obstacleID, byte[] obstacleState)
        {   //计算MA距离

            if (type == 1)//把ID变成障碍物的全称
            {
                Section section = ATP.stationElements_.Elements.Find((GraphicElement element) =>
                {
                    if (element is Section)
                    {
                        if ((element as Section).ID == ID) //MA的ID，MA的type，1表示区段，2表示道岔
                        {
                            return true;
                        }
                    }
                    return false;
                }) as Section;
                MAEndLink = section.Name;
            }

            else if (type == 2)
            {
                RailSwitch railswitch = ATP.stationElements_.Elements.Find((GraphicElement element) =>
                {
                    if (element is RailSwitch)
                    {
                        if ((element as RailSwitch).ID == ID)//MA的ID，MA的type，1表示区段，2表示道岔，找到MA的section
                        {
                            return true;
                        }
                    }
                    return false;
                }) as RailSwitch;
                MAEndLink = railswitch.SectionName;//存的是W开头的
            }

            //Debug.WriteLine("MAEndLink:" + MAEndLink);


            #region 初始化变量
            i = 0;
            j = 0;
            reverseNum = 0;
            count = 0;
            offset = 0;
            distance_1 = 0;
            distance_2 = 0;
            MAEndDistance = 0;
            limSpeedNum = 0;
            limSpeedDistance_1 = 0;
            limSpeedLength_1 = 0;
            limSpeedDistance_2 = 0;
            limSpeedLength_2 = 0;
            limSpeedDistance_3 = 0;
            limSpeedLength_3 = 0;
            nodeID = new int[2];
            nodeName = new string[2];
            switchName = new string[2];
            limSpeedDistance = new int[10];
            limSpeedLength = new int[10];
            retuenValue = new int[11];
            hashTable = new HashTable();
            isBreak = false;
            isSwitchMAEnd = false;
            isFirstBalise_1 = false;
            firstRverseObstacle = "";
            nextLimitSpeed = 0;
            firstReverseNum = 0;
            isNextLimitSpeed = true;
            hashTable = new HashTable();
            hashTable.sectionHashTable();
            hashTable.switchHashTable();
            hashTable.Left2is20();
            hashTable.Left2is5();
            hashTable.Is2();
            #endregion
            byte currentID = 0;

            #region 判断MA终点是否道岔
            if (type == 2)
            {
                isSwitchMAEnd = true;
            }
            #endregion

            #region 障碍物由ID变成W开头的全称
            if (obstacleNum != 0)
            {
                for (int k = 0; k < obstacleNum; k++)
                {
                    if (obstacleID[k].Length == 1 || obstacleID[k].Length == 2)
                    {
                        RailSwitch railswitch = ATP.stationElements_.Elements.Find((GraphicElement element) =>
                        {
                            if (element is RailSwitch)
                            {
                                if ((element as RailSwitch).ID == Convert.ToInt32(obstacleID[k]))
                                {
                                    return true;
                                }
                            }
                            return false;
                        }) as RailSwitch;
                        obstacleID[k] = railswitch.SectionName; //把障碍物由ID变成全称
                        
                    }

                }
                for (int m = 0; m < obstacleNum; m++)
                {
                    if (obstacleState[m] == 2)
                    {
                        firstReverseNum = m + 1;   //得到第一个是反位的道岔，+1后直接break
                        break;
                    }
                }
                if (firstReverseNum > 0)
                {
                    firstRverseObstacle = obstacleID[firstReverseNum - 1]; //表示的经过第一个反位的道岔
                }
            }
            #endregion

            #region 判断所在区段是否道岔
            if (hashTable.ht.Contains(curBalise) == false)//不是道岔
            {
                switchConvert = null;
                isFirst = true;
            }
            else
            {
                foreach (string key in hashTable.ht.Keys) //由当前应答器找nodename和id，用这两个计算左节点和右节点。
                {
                    if (curBalise == key)
                    {
                        switchConvert = (string)hashTable.ht[key];
                    }
                }
                curNodeName = switchConvert.Substring(0, switchConvert.IndexOf("_"));//若是道岔的话是道岔的NodeName
                curNodeID = Convert.ToInt32(switchConvert.Substring(switchConvert.IndexOf("_") + 1));//若是道岔的话就是ID
            }
            #endregion

            #region 需要重新看一下代码，寻找偏移量
            if (switchConvert != null)//所在区段是道岔，可能需要重新在看一下,用来寻找道岔的偏移量
            {
                int curdLink = Convert.ToInt32(curBalise.Substring(1, 4));//curBalise的W0106_1中的106，为了后面的判断
                int curLinkNum = Convert.ToInt32(curBalise.Substring(curBalise.IndexOf("_") + 1)); //最后一位1
                if (obstacleState[0] == 2)//处于反位
                {
                    if (curLinkNum == 1)
                    {
                        if (isLeftSearch)
                        {
                            offset = 5;
                            distance_1 = 20;
                        }
                        else
                        {
                            offset = 20;
                            distance_1 = 5;
                        }
                    }
                    else if (curLinkNum == 3)
                    {
                        if (isLeftSearch)
                        {
                            offset = 20;
                            distance_1 = 5;
                        }
                        else
                        {
                            offset = 5;
                            distance_1 = 20;
                        }

                    }
                    else if (curLinkNum == 2)
                    {
                        if (curdLink == 104 || curdLink == 103 || curdLink == 109 || curdLink == 213 || curdLink == 207 || curdLink == 305 || curdLink == 503 || curdLink == 406 || curdLink == 409)
                        {
                            if (isLeftSearch)
                            {
                                offset = 20;
                                distance_1 = 5;
                            }
                            else
                            {
                                offset = 5;
                                distance_1 = 20;   //修改了这里，偏移量反了，才出现错位。
                            }
                        }
                        else
                        {
                            if (isLeftSearch)
                            {
                                offset = 5;
                                distance_1 = 20;
                            }
                            else
                            {
                                offset = 20;
                                distance_1 = 5;   //修改了这里，偏移量反了，才出现错位。
                            }
                        }


                    }
                    else   //curLinkNum==0,且是反位
                    {
                        if (curdLink == 104 || curdLink == 103 || curdLink == 109 || curdLink == 213 || curdLink == 207 || curdLink == 305 || curdLink == 503 || curdLink == 406 || curdLink == 409)
                        {
                            if (isLeftSearch)
                            {
                                offset = 5;
                                distance_1 = 20;
                            }
                            else
                            {
                                offset = 20;
                                distance_1 = 5;
                            }
                        }
                        else
                        {
                            if (isLeftSearch)
                            {
                                offset = 20;
                                distance_1 = 5;
                            }
                            else
                            {
                                offset = 5;
                                distance_1 = 20;
                            }
                        }


                    }

                }
                else//定位
                {
                    if (curdLink == 105 || curdLink == 106 || curdLink == 411 || curdLink == 414) //不是数字的时候curlink是区段
                    {
                        if (curLinkNum == 1)//应答器后面的1，从左往右布置是先1后2。既左寻先经过1再经过2
                        {
                            if (isLeftSearch)
                            {
                                offset = 5;
                                distance_1 = 45;
                            }
                            else
                            {
                                offset = 45;
                                distance_1 = 5;
                            }
                        }
                        else
                        {
                            if (isLeftSearch)
                            {
                                offset = 45;
                                distance_1 = 5;
                            }
                            else
                            {
                                offset = 5;
                                distance_1 = 45;
                            }

                        }
                    }
                    else
                    {
                        if (curdLink == 104 || curdLink == 103 || curdLink == 109 || curdLink == 213 || curdLink == 207 || curdLink == 305 || curdLink == 503 || curdLink == 406 || curdLink == 409)
                        {
                            if (curLinkNum == 0)
                            {
                                if (isLeftSearch)
                                {
                                    offset = 5;
                                    distance_1 = 20;
                                }
                                else
                                {
                                    offset = 20;
                                    distance_1 = 5;  //要到达的前一区段
                                }
                            }

                            if (curLinkNum == 1)
                            {
                                if (isLeftSearch)
                                {
                                    offset = 20;
                                    distance_1 = 5;
                                }
                                else
                                {
                                    offset = 5;
                                    distance_1 = 20;
                                }
                            }
                        }

                        else
                        {
                            if (curLinkNum == 0)
                            {
                                if (isLeftSearch)
                                {
                                    offset = 20;
                                    distance_1 = 5;
                                }
                                else
                                {
                                    offset = 5;
                                    distance_1 = 20;  //要到达的前一区段
                                }
                            }

                            if (curLinkNum == 1)
                            {
                                if (isLeftSearch)
                                {
                                    offset = 5;
                                    distance_1 = 20;
                                }
                                else
                                {
                                    offset = 20;
                                    distance_1 = 5;
                                }
                            }
                        }
                    }

                }
            }
            else//当是区段时求偏移量和distance_1.
            {
                curNodeName = curBalise.Substring(0, 5);
                if (hashTable.ht_4.ContainsKey(curBalise.Substring(0, 5)) == false)  //四个道岔的时候才用这个
                {
                    foreach (string key in hashTable.ht_1.Keys)
                    {
                        if (key == curBalise.Substring(curBalise.IndexOf("_") + 1))
                        {
                            if (isLeftSearch)
                            {
                                distance_1 = (120 - (int)hashTable.ht_1[key]);
                                offset = 120 - distance_1;
                            }
                            else
                            {
                                distance_1 = (int)hashTable.ht_1[key];
                                offset = (120 - (int)hashTable.ht_1[key]);
                            }
                        }
                    }
                }

                else     //是站台两个道岔的时候
                {
                    if (curBalise.Substring(curBalise.Length - 3, 3) == "1_1")
                    {
                        if (isLeftSearch)
                        {
                            offset = 100;
                            distance_1 = 20;
                        }
                        else
                        {
                            offset = 20;
                            distance_1 = 100;
                        }
                    }

                    if (curBalise.Substring(curBalise.Length - 3, 3) == "1_2")
                    {
                        if (isLeftSearch)
                        {
                            offset = 20;
                            distance_1 = 100;
                        }
                        else
                        {
                            offset = 100;
                            distance_1 = 20;
                        }
                    }
                }

            }
            #endregion

            #region 判断当前区段是否是MA终点

            if (curBalise.Substring(0, 1) == "T")  //由应答器得到currentID
            {
                foreach (var item in ATP.stationTopoloty_.Nodes)
                {
                    if (item.NodeDevice.Name == curBalise.Substring(0, 5))//item.NodeDevice.Name是T0108
                    {
                        currentID = (byte)item.NodeDevice.ID;
                        break;
                    }
                }
            }
            else
            {
                foreach (string key in hashTable.ht.Keys)
                {
                    if (curBalise == key)
                    {
                        string tempID = (string)hashTable.ht[key];//应答器的name和id存到hash表里面了
                        currentID = Convert.ToByte(tempID.Substring(tempID.IndexOf("_") + 1));//每一个应答器都有name和id
                    }
                }
            }
            if (isLeftSearch == false)   //右寻的时候判断EB
            {
                if (curNodeName.Substring(0, 1) == "T")  //当是区段时由curnodename判断是不是过了区段了
                {
                    find_2(curNodeName);
                    foreach (var p in find_2(curNodeName).LeftNodes)
                    {
                        string nodeName1 = p.NodeDevice.Name;
                        int nodeID1 = p.NodeDevice.ID;
                        int type2 = 2;
                        if (nodeName1.Substring(0, 1) == "T")
                        {
                            type2 = 1;
                        }
                        if (nodeID1 == Socket.tailID && type2 == Socket.tailSectionOrSwitch)
                        {
                            Socket.isEB = true;
                            ATP.Write("\r\n" + "EB" + "Search类 423行 当前区段是MA终点（是道岔时）" + "nodeID:" + Convert.ToString(nodeID1) + "tailID:" + Convert.ToString(Socket.tailID) + "type2:" + Convert.ToString(type2) + "tailSectionOrSwitch" + Convert.ToString(Socket.tailSectionOrSwitch) + " " + DateTime.Now.ToString());
                        }
                        break;
                    }

                }
                else
                {
                    foreach (var p in find_1(curNodeName, curNodeID).LeftNodes)//当是道岔时由左节点进行判断是否EB，curNodeName, curNodeID可以唯一决定一个道岔
                    {
                        string nodeName1 = p.NodeDevice.Name;
                        int nodeID1 = p.NodeDevice.ID;
                        int type1 = 2;
                        if (nodeName1.Substring(0, 1) == "T")
                        {
                            type1 = 1;
                        }
                        if (nodeID1 == Socket.tailID && type1 == Socket.tailSectionOrSwitch) // 修改 （||Socket.tailID==0）
                        {
                            Socket.isEB = true;
                            ATP.Write("\r\n" + "EB" + "Search类 443行 当前区段是MA终点（是区段时）" + "nodeID:" + Convert.ToString(nodeID1) + "tailID:" + Convert.ToString(Socket.tailID) + "type1:" + Convert.ToString(type1) + "tailSectionOrSwitch" + Convert.ToString(Socket.tailSectionOrSwitch) + " " + DateTime.Now.ToString());
                        }
                        break;
                    }
                }
            }
            else  //左寻的时候判断EB，用右节点
            {
                if (curNodeName.Substring(0, 1) == "T")  //当是区段时由curnodename判断是不是过了区段了
                {
                    find_2(curNodeName);
                    foreach (var p in find_2(curNodeName).RightNodes)
                    {
                        string nodeName1 = p.NodeDevice.Name;
                        int nodeID1 = p.NodeDevice.ID;
                        int type2 = 2;
                        if (nodeName1.Substring(0, 1) == "T")
                        {
                            type2 = 1;
                        }
                        if (nodeID1 == Socket.tailID && type2 == Socket.tailSectionOrSwitch)
                        {
                            Socket.isEB = true;
                            ATP.Write("\r\n" + "EB" + "Search类 423行 当前区段是MA终点（是道岔时）" + "nodeID:" + Convert.ToString(nodeID1) + "tailID:" + Convert.ToString(Socket.tailID) + "type2:" + Convert.ToString(type2) + "tailSectionOrSwitch" + Convert.ToString(Socket.tailSectionOrSwitch) + " " + DateTime.Now.ToString());
                        }
                        break;
                    }

                }
                else
                {
                    foreach (var p in find_1(curNodeName, curNodeID).RightNodes)//当是道岔时由左节点进行判断是否EB，curNodeName, curNodeID可以唯一决定一个道岔
                    {
                        string nodeName1 = p.NodeDevice.Name;
                        int nodeID1 = p.NodeDevice.ID;
                        int type1 = 2;
                        if (nodeName1.Substring(0, 1) == "T")
                        {
                            type1 = 1;
                        }
                        if (nodeID1 == Socket.tailID && type1 == Socket.tailSectionOrSwitch) // 修改 （||Socket.tailID==0）
                        {
                            Socket.isEB = true;
                            ATP.Write("\r\n" + "EB" + "Search类 443行 当前区段是MA终点（是区段时）" + "nodeID:" + Convert.ToString(nodeID1) + "tailID:" + Convert.ToString(Socket.tailID) + "type1:" + Convert.ToString(type1) + "tailSectionOrSwitch" + Convert.ToString(Socket.tailSectionOrSwitch) + " " + DateTime.Now.ToString());
                        }
                        break;
                    }
                }
            }
            #region 过了道岔就停止

            #endregion
            int cur_type; //1是区段2是道岔
            if (curBalise.Substring(0, 1) == "T")  //有可能有重复的
            {
                cur_type = 1;
            }
            else
            {
                cur_type = 2;
            }

            if (ID == currentID && type == cur_type) //ID表示MA的ID，currentID表示目前的ID，如果MA的ID就是目前的ID，那么直接求就行了。否则就要利用左寻和右寻函数了。MA是当前车到MA故会一直减小。最后肯定会进入到这个方法里面。
            {
                MAEndDistance = MAEndOff - offset;
                nextLimitSpeed = 0;
                retuenValue[0] = MAEndDistance;
                retuenValue[8] = nextLimitSpeed;
            }
            else
            {
                if (isLeftSearch == true)
                {
                    retuenValue = LeftSearch(curBalise, MAEndLink, MAEndOff, obstacleID, obstacleState, obstacleNum, isLeftSearch);
                }
                else
                {
                    retuenValue = RightSearch(curBalise, MAEndLink, MAEndOff, obstacleID, obstacleState, obstacleNum, isLeftSearch);//当没有到当前区段时，用当前应答器，MAlink和障碍物的名称和状态寻路。
                }
            }
            return retuenValue;
        }
        #endregion

        bool isSwitch = false;
        #region 左寻
        public int[] LeftSearch(string curBalise, string MAEndLink, int MAEndOff, string[] obstacleID, byte[] obstacleState, int obstacleNum, bool isLeft)
        {
            if (curBalise.Substring(0, 1) == "W")
            {
                isSwitch = true;
                firstSwitchName = curBalise.Substring(0, 5);
                isFirstBalise_1 = true;
            }
            else
            {
                isSwitch = false;
            }
            while (true)
            {
                if (curNodeName.Substring(0, 1) == "T")
                {
                    find_2(curNodeName);
                    foreach (var p in find_2(curNodeName).LeftNodes)
                    {
                        nodeName[i] = p.NodeDevice.Name;
                        nodeID[i] = p.NodeDevice.ID;
                        if ((MAEndLink == "ZHG1" || MAEndLink == "ZHG2") && isLeft == true &&(nodeName[0]== "ZHG1"|| nodeName[0]== "ZHG2"))
                        {
                            switchName[j] = "ZHG1";
                        }                                                                    //ZHG1左寻没有东西新加
                        else if (p.NodeDevice.Name.Substring(0, 1) != "T" )
                        {
                            switchName[j] = (p.NodeDevice as RailSwitch).SectionName; //zh1没有左节点了
                        }
                       
                           
                        
                        i++;
                        j++;
                    }
                }
                else
                {
                    foreach (var p in find_1(curNodeName, curNodeID).LeftNodes)
                    {
                        nodeName[i] = p.NodeDevice.Name;
                        nodeID[i] = p.NodeDevice.ID;
                        if (p.NodeDevice.Name.Substring(0, 1) != "T")
                        {
                            switchName[j] = (p.NodeDevice as RailSwitch).SectionName;
                        }
                        i++;
                        j++;
                    }
                }

                BreakSearch(MAEndLink, MAEndOff, obstacleState, obstacleNum, isLeft); //要等到右节点是MA终点时，这个才起作用，isbreak为TRUE，跳出循环。
                if (isBreak)//isBreak，没有找到就跳不出循环了
                {
                    MAEndDistance = distance_1 + distance_2 + MAEndOff;
                    break;
                }

                if (isSwitch == true && isFirstBalise_1)
                {
                    count++;
                    tempSwitchName_1 = firstSwitchName;
                    isFirstBalise_1 = false;
                }

                if (curNodeName.Substring(0, 1) == "T")
                {
                    NormalSection(obstacleID, obstacleState);
                }
                else//当前区段是道岔
                {
                    if (obstacleID[count - 1] == tempSwitchName_1)
                    {
                        if (obstacleState[count - 1] == 1)//定位
                        {
                            SwitchInNormal(obstacleID, obstacleState);
                        }
                        else//反位
                        {
                            if (i == 1)
                            {
                                SwitchInReverse_1(obstacleID, obstacleState);
                            }
                            else
                            {
                                SwitchInReverse_2(obstacleID, obstacleState);
                            }
                        }
                    }
                }

                i = 0;
                j = 0;
                tempSwitchName_2 = null;
                nodeID = new int[2];
                nodeName = new string[2];
                switchName = new string[2];
                NextSectionSpeed(obstacleState);
            }

            int[] returnValue = new int[11];
            returnValue[0] = MAEndDistance;
            returnValue[1] = limSpeedNum;
            returnValue[2] = limSpeedDistance_1;
            returnValue[3] = limSpeedLength_1;
            returnValue[4] = limSpeedDistance_2;
            returnValue[5] = limSpeedLength_2;
            returnValue[6] = limSpeedDistance_3;
            returnValue[7] = limSpeedLength_3;
            returnValue[8] = nextLimitSpeed;
            returnValue[9] = limSpeedDistance_4;
            returnValue[10] = limSpeedLength_4;
            return returnValue;

        }
        #endregion 
        #region 右寻
        public int[] RightSearch(string curBalise, string MAEndLink, int MAEndOff, string[] obstacleID, byte[] obstacleState, int obstacleNum, bool isLeft)
        {

            if (curNodeName.Substring(0, 1) != "T")  //是道岔的时候知道第一个道岔的名字
            {
                isSwitch = true;
                firstSwitchName = curBalise.Substring(0, 5);
                isFirstBalise_1 = true;
            }
            else                                 //等于4的时候不是道岔加T
            {
                isSwitch = false;
            }
            while (true)
            {

                if (curNodeName.Substring(0, 1) == "T")
                {
                    foreach (var p in find_2(curNodeName).RightNodes) //记录其右节点，只有一个或者两个，记录name和ID还有是否是道岔
                    {

                        nodeName[i] = p.NodeDevice.Name;
                        nodeID[i] = p.NodeDevice.ID;
                        if (p.NodeDevice.Name.Substring(0, 1) != "T")
                        {
                            switchName[j] = (p.NodeDevice as RailSwitch).SectionName;
                        }
                        i++;
                        j++;
                    }
                }
                else
                {
                    foreach (var p in find_1(curNodeName, curNodeID).RightNodes)
                    {
                        nodeName[i] = p.NodeDevice.Name;//把右节点的name和ID都记下来，有的时候是有两个的。通过curNodeName,里面区段时是T0104，道岔时是name和id，curNodeID这两个值来控制循环。
                        nodeID[i] = p.NodeDevice.ID;
                        if (p.NodeDevice.Name.Substring(0, 1) != "T")
                        {
                            switchName[j] = (p.NodeDevice as RailSwitch).SectionName;
                        }
                        i++;
                        j++;
                    }
                }


                BreakSearch(MAEndLink, MAEndOff, obstacleState, obstacleNum, isLeft);//每一次都右寻，用这个计算障碍物的距离和长度


                if (isBreak)
                {
                    MAEndDistance = distance_1 + distance_2 + MAEndOff; //MA是直接在这里计算的，当distance_1既到达当前区段终点的距离，distance_2是两个距离间隔区段的距离，MAEndOff是MA终点区段的距离
                    break;
                }

                if (isSwitch == true && isFirstBalise_1)
                {
                    count++;
                    tempSwitchName_1 = firstSwitchName;
                    isFirstBalise_1 = false;
                }

                if (curNodeName.Substring(0, 1) == "T") //调换位置
                {
                    NormalSection(obstacleID, obstacleState); //接下来更新curnodename和curnodeid。不断的是右节点，不断的查找。直到找到MA终点。
                }
                else//当前区段是道岔
                {

                    if (obstacleID[count - 1] == tempSwitchName_1)
                    {
                        if (obstacleState[count - 1] == 1)//定位
                        {
                            SwitchInNormal(obstacleID, obstacleState);  //为了得到distanc_2
                        }
                        else//反位
                        {
                            if (i == 1)
                            {
                                SwitchInReverse_1(obstacleID, obstacleState); //处于反位时的distance_2，并将curnodename赋给右节点进行下一次查询
                            }
                            else
                            {
                                SwitchInReverse_2(obstacleID, obstacleState);
                            }
                        }
                    }
                }



                i = 0;
                j = 0;
                tempSwitchName_2 = null;
                nodeID = new int[2];
                nodeName = new string[2];
                switchName = new string[2];
                NextSectionSpeed(obstacleState);
            }

            int[] returnValue = new int[11];
            returnValue[0] = MAEndDistance;
            returnValue[1] = limSpeedNum;
            returnValue[2] = limSpeedDistance_1;
            returnValue[3] = limSpeedLength_1;
            returnValue[4] = limSpeedDistance_2;
            returnValue[5] = limSpeedLength_2;
            returnValue[6] = limSpeedDistance_3;
            returnValue[7] = limSpeedLength_3;
            returnValue[8] = nextLimitSpeed;
            returnValue[9] = limSpeedDistance_4;
            returnValue[10] = limSpeedLength_4;
            return returnValue;
        }
        #endregion
        public void BreakSearch(string MAEndLink, int MAEndOff, byte[] obstacleState, int obstacleNum, bool isLeft) //这个是专门寻找障碍物的长度和位置的
        {
            int obstacleNum1 = 0;
            if (isSwitchMAEnd == true)  //ma终点是道岔
            {
                for (int n = 0; n < j; n++)
                {
                    if (String.Compare(switchName[n], MAEndLink) == 0)
                    {
                        if (firstReverseNum > 0)
                        {
                            ObstacleHandle(obstacleState, isLeft);
                            MAEndIsNotReverse(isLeft);
                        }
                        isBreak = true;
                    }
                }
            }
            else
            {
                if (String.Compare(nodeName[0], MAEndLink) == 0) //MAEndLink T0215,nodeName里面存的是当前的区段T0108,判断当前区段的右节点是不是到达MA，相当的时候是0
                {
                    if (obstacleNum == 0)
                    {
                        obstacleNum1 = 0;
                    }
                    else
                    {
                        obstacleNum1 = obstacleNum - 1;
                    }
                    if (curNodeName.Length != 4 && obstacleState[obstacleNum1] == 2) //死循环即是抛异常
                    {
                        limSpeedDistance[reverseNum] = distance_2 - 25;
                        limSpeedLength[reverseNum] = 25;
                        //reverseNum++;
                    }
                    if (firstReverseNum > 0)
                    {
                        ObstacleHandle(obstacleState, isLeft);   //这两个是处理障碍物的距离和长度
                        MAEndIsNotReverse(isLeft);
                    }
                    isBreak = true;
                }

                else if (String.Compare(nodeName[1], MAEndLink) == 0) //添加这个是为了左寻的时候最后有两个
                {
                    if (obstacleNum == 0)
                    {
                        obstacleNum1 = 0;
                    }
                    else
                    {
                        obstacleNum1 = obstacleNum - 1;
                    }
                    if (curNodeName.Length != 4 && obstacleState[obstacleNum1] == 2) //死循环即是抛异常
                    {
                        limSpeedDistance[reverseNum] = distance_2 - 25;
                        limSpeedLength[reverseNum] = 25;
                        //reverseNum++;
                    }
                    if (firstReverseNum > 0)
                    {
                        ObstacleHandle(obstacleState, isLeft);
                        MAEndIsNotReverse(isLeft);
                    }
                    isBreak = true;
                }

            }
        }

        public void NormalSection(string[] obstacleID, byte[] obstacleState)
        {
            curNodeName = nodeName[0];
            curNodeID = nodeID[0];
            if (curNodeName.Substring(0, 1) != "T")
            {
                count++;
                tempSwitchName_1 = switchName[0];
                if (obstacleID[count - 1] == tempSwitchName_1)
                {
                    if (obstacleState[count - 1] == 1)
                    {
                        if (tempSwitchName_1 == "W0105" || tempSwitchName_1 == "W0106" || tempSwitchName_1 == "W0414" || tempSwitchName_1 == "W0411")
                        {
                            distance_2 = distance_2 + 50;
                        }
                        else
                        {
                            distance_2 = distance_2 + 25;
                        }
                    }
                    else
                    {
                        distance_2 = distance_2 + 25;
                    }
                }
            }
            else
            {
                distance_2 = distance_2 + 120;
            }
        }

        public void SwitchInNormal(string[] obstacleID, byte[] obstacleState)
        {
            curNodeName = nodeName[0];
            curNodeID = nodeID[0];
            if (curNodeName.Substring(0, 1) != "T")
            {
                tempSwitchName_2 = switchName[0];
                if (String.Compare(tempSwitchName_1, tempSwitchName_2) != 0)
                {
                    count++;
                    tempSwitchName_1 = tempSwitchName_2;
                    if (obstacleID[count - 1] == tempSwitchName_1)
                    {
                        if (obstacleState[count - 1] == 1)
                        {
                            if (tempSwitchName_1 == "W0105" || tempSwitchName_1 == "W0106" || tempSwitchName_1 == "W0414" || tempSwitchName_1 == "W0411")
                            {
                                distance_2 = distance_2 + 50;  //四岔时是50
                            }
                            else
                            {
                                distance_2 = distance_2 + 25; //正常的是25，然后不断相加就可以得到最终的distance_2。
                            }
                        }
                        else
                        {
                            distance_2 = distance_2 + 25;
                        }
                    }
                }
            }
            else
            {
                distance_2 = distance_2 + 120;
            }
        }

        public void SwitchInReverse_1(string[] obstacleID, byte[] obstacleState)
        {
            limSpeedDistance[reverseNum] = distance_2 - 25;
            limSpeedLength[reverseNum] = 25;
            reverseNum++;
            curNodeName = nodeName[0];
            curNodeID = nodeID[0];

            if (curNodeName.Substring(0, 1) != "T")
            {
                count++;
                tempSwitchName_1 = switchName[0];
                if (obstacleID[count - 1] == tempSwitchName_1)
                {
                    if (obstacleState[count - 1] == 1)
                    {
                        if (tempSwitchName_1 == "W0105" || tempSwitchName_1 == "W0106" || tempSwitchName_1 == "W0414" || tempSwitchName_1 == "W0411")
                        {
                            distance_2 = distance_2 + 50;
                        }
                        else
                        {
                            distance_2 = distance_2 + 25;
                        }
                    }
                    else
                    {
                        distance_2 = distance_2 + 25;
                    }
                }
            }
            else
            {
                distance_2 = distance_2 + 120;

            }
        }

        public void SwitchInReverse_2(string[] obstacleID, byte[] obstacleState)
        {
            limSpeedDistance[reverseNum] = distance_2 - 25;
            limSpeedLength[reverseNum] = 25;
            reverseNum++;
            curNodeName = nodeName[1]; //在这里赋给下一个值继续进行查询
            curNodeID = nodeID[1];

            switchName[0] = switchName[1];  //用于存左节点的

            if (curNodeName.Substring(0, 1) != "T")
            {
                count++;
                tempSwitchName_1 = switchName[0];
                if (obstacleID[count - 1] == tempSwitchName_1)
                {
                    if (obstacleState[count - 1] == 1)
                    {
                        if (tempSwitchName_1 == "W0105" || tempSwitchName_1 == "W0106" || tempSwitchName_1 == "W0414" || tempSwitchName_1 == "W0411")
                        {
                            distance_2 = distance_2 + 50;
                        }
                        else
                        {
                            distance_2 = distance_2 + 25;
                        }
                    }
                    else
                    {
                        distance_2 = distance_2 + 25;
                    }
                }
            }
            else
            {
                distance_2 = distance_2 + 120;
            }
        }

        public void NextSectionSpeed(byte[] obstacleState)
        {
            if (isNextLimitSpeed)
            {
                if (isSwitch == false)
                {
                    if (curNodeName.Substring(0, 1) != "T")
                    {
                        if (obstacleState[0] == 2)
                        {
                            nextLimitSpeed = 40;
                        }
                        else
                        {
                            nextLimitSpeed = 70;
                        }
                    }
                    else
                    {
                        nextLimitSpeed = 70;
                    }
                }
                else
                {
                    if (curNodeName.Substring(0, 1) != "T")
                    {
                        if (String.Compare(firstSwitchName, tempSwitchName_1) != 0)
                        {
                            if (obstacleState[1] == 2)
                            {
                                nextLimitSpeed = 40;
                            }
                            else
                            {
                                nextLimitSpeed = 70;
                            }
                        }
                        else
                        {
                            nextLimitSpeed = 70;
                        }
                    }
                    else
                    {
                        nextLimitSpeed = 70;
                    }
                }
                isNextLimitSpeed = false;
            }
        }

        public void ObstacleHandle(byte[] obstacleState, bool isLeft) //用这个方法来计算限速区段的位置和距离
        {
            if (MAEndLink == "T0215" && isLeft == false && obstacleState[obstacleState.Length-1]==2)
            {
                reverseNum++;       //在寻到T0215时会忽略前面的反位
            }

            else if (((MAEndLink == "T0107" || MAEndLink == "T0108" || MAEndLink == "T0411") && obstacleState[obstacleState.Length - 1] == 2) && isLeft == false)
            {
                reverseNum++;
            }

            else if (isLeft == true && (MAEndLink == "T0115" || MAEndLink == "T0114" || MAEndLink == "T0215"))
            {
                reverseNum++;
            }





            if (reverseNum == 1)
            {
                limSpeedDistance_1 = limSpeedDistance[0];
                limSpeedLength_1 = limSpeedLength[0];
                limSpeedNum = 1;
            }
            else if (reverseNum == 2)
            {
                if (limSpeedDistance[0] + 25 == limSpeedDistance[1])
                {
                    limSpeedDistance_1 = limSpeedDistance[0];
                    limSpeedLength_1 = limSpeedLength[0] + limSpeedLength[1];
                    limSpeedNum = 1;
                }
                else
                {
                    limSpeedDistance_1 = limSpeedDistance[0];
                    limSpeedLength_1 = limSpeedLength[0];
                    limSpeedDistance_2 = limSpeedDistance[1];
                    limSpeedLength_2 = limSpeedLength[1];
                    limSpeedNum = 2;
                }
            }
            else if (reverseNum == 3)
            {
                if (limSpeedDistance[0] + 25 == limSpeedDistance[1])
                {

                    if (limSpeedDistance[1] + 25 == limSpeedDistance[2])
                    {
                        limSpeedDistance_1 = limSpeedDistance[0];
                        limSpeedLength_1 = limSpeedLength[0] + limSpeedLength[1] + limSpeedLength[2];
                        limSpeedNum = 1;
                    }
                    else
                    {
                        limSpeedDistance_1 = limSpeedDistance[0];
                        limSpeedLength_1 = limSpeedLength[0] + limSpeedLength[1];
                        limSpeedDistance_2 = limSpeedDistance[2];
                        limSpeedLength_2 = limSpeedLength[2];
                        limSpeedNum = 2;
                    }
                }
                else
                {



                    limSpeedDistance_1 = limSpeedDistance[0];
                    limSpeedLength_1 = limSpeedLength[0];
                    limSpeedDistance_2 = limSpeedDistance[1];
                    limSpeedLength_2 = limSpeedLength[1] + limSpeedLength[2];
                    limSpeedNum = 2;


                }
            }
            else if (reverseNum == 4 && obstacleState.Length == 6 && obstacleState[0] == 2 && obstacleState[1] == 1 && obstacleState[2] == 2 && obstacleState[3] == 1)
            {
                limSpeedDistance_1 = limSpeedDistance[0];
                limSpeedLength_1 = limSpeedLength[0] + limSpeedLength[1];
                limSpeedDistance_2 = limSpeedDistance[1];
                limSpeedLength_2 = limSpeedLength[2];
                limSpeedDistance_3 = limSpeedDistance[2];
                limSpeedLength_3 = limSpeedLength[2] + limSpeedLength[1];
                limSpeedNum = 3;
            }

            else if (reverseNum == 4 && obstacleState.Length == 7 && obstacleState[0] == 2 && obstacleState[1] == 1 && obstacleState[2] == 1 && obstacleState[3] == 2)//从ZHG2过W0206
            {
                limSpeedDistance_1 = limSpeedDistance[0];
                limSpeedLength_1 = limSpeedLength[0];
                limSpeedDistance_2 = limSpeedDistance[1];
                limSpeedLength_2 = limSpeedLength[2];
                limSpeedDistance_3 = limSpeedDistance[2];
                limSpeedLength_3 = limSpeedLength[2] + limSpeedLength[1];
                limSpeedNum = 3;
            }
            else if (reverseNum == 4 && obstacleState.Length == 6 && obstacleState[0] == 2 && obstacleState[1] == 2 && obstacleState[2] == 2 && obstacleState[3] == 1 && obstacleState[4] == 2 && obstacleState[5] == 1)//从ZHG2过W0206
            {
                limSpeedDistance_1 = limSpeedDistance[0];
                limSpeedLength_1 = limSpeedLength[0] + limSpeedLength[1] + limSpeedLength[2]; ;
                limSpeedDistance_2 = limSpeedDistance[3];
                limSpeedLength_2 = limSpeedLength[2];
                limSpeedNum = 2;
            }
            else if (reverseNum == 4)
            {
                limSpeedDistance_1 = limSpeedDistance[0];
                limSpeedLength_1 = limSpeedLength[0] + limSpeedLength[1];
                limSpeedDistance_2 = limSpeedDistance[2];
                limSpeedLength_2 = limSpeedLength[2] + limSpeedLength[3];
                limSpeedNum = 2;
            }
            else if (reverseNum == 5)
            {

                if (limSpeedDistance[0] + 25 != limSpeedDistance[1])
                {
                    limSpeedDistance_1 = limSpeedDistance[0];
                    limSpeedLength_1 = limSpeedLength[0];
                    limSpeedDistance_2 = limSpeedDistance[1];
                    limSpeedLength_2 = limSpeedLength[1] + limSpeedLength[2];
                    limSpeedDistance_3 = limSpeedDistance[3];
                    limSpeedLength_3 = limSpeedLength[3] + limSpeedLength[4];
                    limSpeedNum = 3;
                }
                else if (limSpeedDistance[0] + 25 == limSpeedDistance[1] && obstacleState.Length == 7 && obstacleState[0] == 2 && obstacleState[1] == 2 && obstacleState[2] == 1 && obstacleState[3] == 2)//障碍物状态一直不变
                {
                    limSpeedDistance_1 = limSpeedDistance[0];
                    limSpeedLength_1 = limSpeedLength[0] + limSpeedLength[1];
                    limSpeedDistance_2 = limSpeedDistance[2];
                    limSpeedLength_2 = limSpeedLength[1];
                    limSpeedDistance_3 = limSpeedDistance[3];
                    limSpeedLength_3 = limSpeedLength[3] + limSpeedLength[4];
                    limSpeedNum = 3;
                }
                else if (limSpeedDistance[0] + 25 == limSpeedDistance[1])
                {
                    if (limSpeedDistance[1] + 25 == limSpeedDistance[2])
                    {
                        limSpeedDistance_1 = limSpeedDistance[0];
                        limSpeedLength_1 = limSpeedLength[0] + limSpeedLength[1] + limSpeedLength[2];
                        limSpeedDistance_2 = limSpeedDistance[3];
                        limSpeedLength_2 = limSpeedLength[3] + limSpeedLength[4];
                    }
                    else
                    {
                        limSpeedDistance_1 = limSpeedDistance[0];
                        limSpeedLength_1 = limSpeedLength[0] + limSpeedLength[1];
                        limSpeedDistance_2 = limSpeedDistance[2];
                        limSpeedLength_2 = limSpeedLength[2] + limSpeedLength[3] + limSpeedLength[4];
                    }
                    limSpeedNum = 2;
                }
            }
            else if (reverseNum == 6)   //新加的，尽量不改变原来的&& obstacleState.Length==8 && obstacleState[0]==2 && obstacleState[1] == 2 && obstacleState[2] == 2 && obstacleState[3] == 1
            {
                limSpeedDistance_1 = limSpeedDistance[0];  //从第一个区段开始，再加上前面的偏移量
                limSpeedLength_1 = limSpeedLength[0] + limSpeedLength[1] + limSpeedLength[2];
                limSpeedDistance_2 = limSpeedDistance[3];
                limSpeedLength_2 = limSpeedLength[2];
                limSpeedDistance_3 = limSpeedDistance[4];
                limSpeedLength_3 = limSpeedLength[4] + limSpeedLength[5];
                limSpeedNum = 3;
            }
            //else if (reverseNum == 6)  //只有一条路才能经过6个反位
            //{
            //    limSpeedDistance_1 = limSpeedDistance[0];
            //    limSpeedLength_1 = limSpeedLength[0] + limSpeedLength[1];
            //    limSpeedDistance_2 = limSpeedDistance[2];
            //    limSpeedLength_2 = limSpeedLength[2] + limSpeedLength[3];
            //    limSpeedDistance_3 = limSpeedDistance[4];
            //    limSpeedLength_3 = limSpeedLength[4] + limSpeedLength[5];
            //    limSpeedNum = 3;
            //}

            else if (reverseNum == 7)
            {
                if (limSpeedDistance[0] + 25 == limSpeedDistance[1] && limSpeedDistance[1] + 25 == limSpeedDistance[2])
                {
                    limSpeedDistance_1 = limSpeedDistance[0];
                    limSpeedLength_1 = limSpeedLength[0] + limSpeedLength[1] + limSpeedLength[2];
                    limSpeedDistance_2 = limSpeedDistance[3];
                    limSpeedLength_2 = limSpeedLength[3] + limSpeedLength[4];
                    limSpeedDistance_3 = limSpeedDistance[5];
                    limSpeedLength_3 = limSpeedLength[5] + limSpeedLength[6];
                    limSpeedNum = 3;

                }
                else if (limSpeedDistance[4] + 25 == limSpeedDistance[5] && limSpeedDistance[5] + 25 == limSpeedDistance[6])
                {
                    limSpeedDistance_1 = limSpeedDistance[0] + limSpeedDistance[1];
                    limSpeedLength_1 = limSpeedLength[0] + limSpeedLength[1];
                    limSpeedDistance_2 = limSpeedDistance[2] + limSpeedDistance[3];
                    limSpeedLength_2 = limSpeedLength[2] + limSpeedLength[3];
                    limSpeedDistance_3 = limSpeedDistance[4] + limSpeedDistance[5] + limSpeedDistance[6];
                    limSpeedLength_3 = limSpeedLength[4] + limSpeedLength[5] + limSpeedLength[6];
                    limSpeedNum = 3;
                }
                else   //四个限速区段新加,从ZH2G排出
                {

                    limSpeedDistance_1 = limSpeedDistance[0];
                    limSpeedLength_1 = limSpeedLength[0];
                    limSpeedDistance_2 = limSpeedDistance[1];
                    limSpeedLength_2 = limSpeedLength[1] + limSpeedLength[2];
                    limSpeedDistance_3 = limSpeedDistance[3];
                    limSpeedLength_3 = limSpeedLength[3] + limSpeedLength[4];
                    limSpeedDistance_4 = limSpeedDistance[5];
                    limSpeedLength_4 = limSpeedLength[5] + limSpeedLength[6];
                    limSpeedNum = 4;
                }

            }
            else if (reverseNum == 8)   //从zhg1排出来的四道岔
            {
                limSpeedDistance_1 = limSpeedDistance[0];
                limSpeedLength_1 = limSpeedLength[0] + limSpeedLength[1] + limSpeedLength[2];
                limSpeedDistance_2 = limSpeedDistance[3];
                limSpeedLength_2 = limSpeedLength[1] + limSpeedLength[2];
                limSpeedDistance_3 = limSpeedDistance[5];
                limSpeedLength_3 = limSpeedLength[3] + limSpeedLength[4];
                limSpeedDistance_4 = limSpeedDistance[7];
                limSpeedLength_4 = limSpeedLength[5] + limSpeedLength[6];
                limSpeedNum = 4;
            }
            else if (reverseNum == 9)   //从zhg1排出来的四道岔
            {
                limSpeedDistance_1 = limSpeedDistance[0];//只有最开始加100了，后面没有加100
                limSpeedLength_1 = limSpeedLength[0] + limSpeedLength[1] + limSpeedLength[2];
                limSpeedDistance_2 = limSpeedDistance[3];
                limSpeedLength_2 = limSpeedLength[1] + limSpeedLength[2];
                limSpeedDistance_3 = limSpeedDistance[5];
                limSpeedLength_3 = limSpeedLength[3] + limSpeedLength[4];
                limSpeedDistance_4 = limSpeedDistance[7];
                limSpeedLength_4 = limSpeedLength[5] + limSpeedLength[6];
                limSpeedNum = 4;
            }



        }

        public void MAEndIsNotReverse(bool isLeft)  //寻到路了才用这个
        {
            if (curNodeName.Substring(0, 1) != "T" && String.Compare(firstSwitchName, firstRverseObstacle) == 0 && (!((MAEndLink=="T0115" || MAEndLink=="T0114") && isLeft==true))) //curNodeName MA终点的前一个是道岔的时候
            {
                if (limSpeedNum == 2)
                {
                    limSpeedDistance_2 += distance_1;
                }
                else if (limSpeedNum == 3)
                {
                    limSpeedDistance_2 += distance_1;
                    limSpeedDistance_3 += distance_1;
                }

                else if (limSpeedNum == 4)
                {
                    limSpeedDistance_2 += distance_1;
                    limSpeedDistance_3 += distance_1;
                    limSpeedDistance_4 += distance_1;
                }
                limSpeedDistance_1 = 0;
                limSpeedLength_1 = limSpeedLength_1 - offset;  //不断的走就会变
            }
            else                                                                                                 //当MA终点的前一个是区段的时候
            {
                if (limSpeedNum == 2)
                {
                    limSpeedDistance_2 += distance_1;
                }
                else if (limSpeedNum == 3)
                {
                    limSpeedDistance_2 += distance_1;
                    limSpeedDistance_3 += distance_1;
                }
                else if (limSpeedNum == 4)
                {
                    limSpeedDistance_2 += distance_1;
                    limSpeedDistance_3 += distance_1;
                    limSpeedDistance_4 += distance_1;
                }
                limSpeedDistance_1 += distance_1;
            }
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
    }
}
