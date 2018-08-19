using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using 线路绘图工具;

namespace CBTC
{
    class TrainMessage
    {
        public string MAEndLink;
        HashTable hashTable = new HashTable();
        public string IDTypeConvertName(byte type, byte ID) //用type和id得到名字。当ZC发送MA的type和ID时可以用到
        {
            if (type == 1) //区段
            {
                Section section = ATP.stationElements_.Elements.Find((GraphicElement element) =>
                {
                    if (element is Section)
                    {
                        if ((element as Section).ID == ID) //区段ID唯一
                        {
                            return true;
                        }
                    }
                    return false;
                }) as Section;
                MAEndLink = section.Name;  // T0301
                return MAEndLink;
            }

            else if (type == 2)  //道岔
            {
                RailSwitch railswitch = ATP.stationElements_.Elements.Find((GraphicElement element) =>
                {
                    if (element is RailSwitch)
                    {
                        if ((element as RailSwitch).ID == ID)
                        {
                            return true;
                        }
                    }
                    return false;
                }) as RailSwitch;
                MAEndLink = railswitch.SectionName;//W0414
                return MAEndLink;
            }
            return null;
        }

        public TopolotyNode BaliseToIteam(string balise) //根据当前的应答器找到节点,道岔根据sectionName即可
        {
            if (balise.Substring(0, 1) == "T")
            {
                foreach (var item in ATP.stationTopoloty_.Nodes)
                {
                    if (item.NodeDevice.Name == balise.Substring(0, 5)) //区段时只需要根据Name判断就可以,nameT0103
                    {
                        return item;
                    }
                }
            }

            else
            {
                TopolotyNode node = ATP.stationTopoloty_.Nodes.Find((TopolotyNode toponode) =>   //node是寻找到的节点，返回符合条件的
                {
                    if (toponode.NodeDevice is RailSwitch)
                    {
                        RailSwitch railSwitch = toponode.NodeDevice as RailSwitch;
                        return railSwitch.SectionName == balise.Substring(0, 5);
                    }
                    return false;
                });
                if (node != null)
                {
                    return node;
                }
            }
            return null;

        }

        public List<线路绘图工具.TopolotyNode> RightNextCurBaliseList(string curBalise) //由当前的应答器右寻得到一个列表。当前是直轨下一个只有一个元素，当前是道岔则有两个元素
        {
            List<线路绘图工具.TopolotyNode> nodes_list = BaliseToIteam(curBalise).RightNodes;
            return nodes_list;
        }

        public List<线路绘图工具.TopolotyNode> LeftNextCurBaliseList(string curBalise) //由当前的应答器右寻得到一个列表。当前是直轨下一个只有一个元素，当前是道岔则有两个元素
        {
            List<线路绘图工具.TopolotyNode> nodes_list = BaliseToIteam(curBalise).LeftNodes;
            return nodes_list;
        }

        public string NextCurBaliseList(线路绘图工具.TopolotyNode Node) //根据节点得到Name,有可能是道岔是sectionName，有可能是直轨是Name
        {
            string NextName = "";
            if (Node.NodeDevice is Section)
            {
                NextName = (Node.NodeDevice as Section).Name;
            }
            else if (Node.NodeDevice is RailSwitch)
            {
                NextName = (Node.NodeDevice as RailSwitch).SectionName;
            }
            return NextName;

        }



        public bool IsRailswitchVoid(string curbalise) //根据传进来的curbalise(T0104,W0233)判断是不是道岔
        {
            if (curbalise.Substring(0, 1) == "T")
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void SectionGetOffDis(bool isLeftSearch,string curBalise,ref UInt32 offset,ref UInt32 distance_1)
        {
            int LogicCount = (BaliseToIteam(curBalise).NodeDevice as Section).LogicCount; //当是2时不是站台，有4个应答器
            hashTable.sectionHashTable();
            string Key = curBalise.Substring(curBalise.IndexOf("_") + 1); //当前应答器是1_1
            if (LogicCount == 1)
            {
                if (curBalise.Substring(curBalise.Length - 3, 3) == "1_1")
                {
                    if (isLeftSearch)
                    {
                        offset = 100;
                        distance_1 = 120-offset;
                    }
                    else
                    {
                        offset = 20;
                        distance_1 = 120-offset;
                    }
                }

                if (curBalise.Substring(curBalise.Length - 3, 3) == "1_2")
                {
                    if (isLeftSearch)
                    {
                        offset = 20;
                        distance_1 = 120-offset;
                    }
                    else
                    {
                        offset = 100;
                        distance_1 = 120-offset;
                    }
                }
            }
            else
            {
                if (isLeftSearch)
                {
                    offset = Convert.ToUInt16(hashTable.ht_1[Key]);
                    distance_1 = 120 - offset;
                }
                else
                {
                    offset = Convert.ToUInt16(120 - (int)hashTable.ht_1[Key]);
                    distance_1 = 120 - offset;
                }
            }
            
        }

        public void SwitchGetOffDis(bool isLeftSearch, string curBalise, ref UInt32 offset, ref UInt32 distance_1)
        {
            bool isUp = (BaliseToIteam(curBalise).NodeDevice as RailSwitch).IsUp;
            bool isLeft = (BaliseToIteam(curBalise).NodeDevice as RailSwitch).IsLeft;
            string Key = curBalise.Substring(curBalise.IndexOf("_") + 1); //取最后一个数字
            if (isLeft)
            {
                if (isLeftSearch == false)
                {
                    if (Key == "1")
                    {
                        offset = 20;
                        distance_1 = 25 - offset;
                    }
                    else if (Key == "2")
                    {
                        offset = 20;
                        distance_1 = 25 - offset;
                    }
                    else if (Key == "0")
                    {
                        offset = 5;
                        distance_1 = 25 - offset;
                    }
                }
                else //右寻
                {
                    if (Key == "1")
                    {
                        offset = 5;
                        distance_1 = 25 - offset;
                    }
                    else if (Key == "2")
                    {
                        offset = 5;
                        distance_1 = 25 - offset;
                    }
                    else if (Key == "0")
                    {
                        offset = 20;
                        distance_1 = 25 - offset;
                    }
                }
            }
            else  //方向偏右，有可能是4开道岔
            {
                if (isLeftSearch == false)
                {
                    if (Key == "1")
                    {
                        offset = 5;
                        distance_1 = 25 - offset;
                    }
                    else if (Key == "2")
                    {
                        offset = 5;
                        distance_1 = 25 - offset;
                    }
                    else if (Key == "0")
                    {
                        offset = 20;
                        distance_1 = 25 - offset;
                    }
                    else if (Key == "3")
                    {
                        offset = 20;
                        distance_1 = 25 - offset;
                    }
                }
                else //右寻
                {
                    if (Key == "1")
                    {
                        offset = 20;
                        distance_1 = 25 - offset;
                    }
                    else if (Key == "2")
                    {
                        offset = 20;
                        distance_1 = 25 - offset;
                    }
                    else if (Key == "0")
                    {
                        offset = 5;
                        distance_1 = 25 - offset;
                    }
                    else if (Key == "3")
                    {
                        offset = 5;
                        distance_1 = 25 - offset;
                    }
                }
            }

        }



    }
}
