using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CbtcData;
using ATP;
using ATP.TrainMessageEB;
namespace TrainMessageEB
{
    class TrainMessage
    {
        public string MAEndLink;
        public HashTable hashTable = new HashTable();
        public Getxml getxml=new Getxml();
        public void GetHash()
        {
            hashTable.sectionHashTable();
            hashTable.sikai();
            getxml.XMLInitialize();
        }
     
        public string IDTypeConvertName(byte type, byte ID) //用type和id得到名字。当ZC发送MA的type和ID时可以用到。OK
        {
            if (type == 1) //区段
            {
                Section section = getxml.data.Topo.Find((TopolotyNode toponode) =>
                {
                    if (toponode.device is Section)
                    {
                        if ((toponode.device as Section).Id == ID) //区段ID唯一
                        {
                            return true;
                        }
                    }
                    return false;
                }).device as Section;
                MAEndLink = section.Name;  // T0301
                return MAEndLink;
            }

            else if (type == 2)  //道岔
            {
                RailSwitch railswitch = getxml.data.Topo.Find((TopolotyNode toponode) =>
                {
                    if (toponode.device is RailSwitch)
                    {
                        if ((toponode.device as RailSwitch).Id == ID) //type和id可以唯一确定道岔
                        {
                            return true;
                        }
                    }
                    return false;
                }).device as RailSwitch;
                MAEndLink = railswitch.section.Name;//W0414
                return MAEndLink;
            }
            return null;
        }

        public TopolotyNode IDTypeConvertTopo(byte type, byte ID) //用type和id得到名字。当ZC发送MA的type和ID时可以用到。OK
        {
            if (type == 1) //区段
            {
                TopolotyNode topo = getxml.data.Topo.Find((TopolotyNode toponode) =>
                {
                    if (toponode.device is Section)
                    {
                        if ((toponode.device as Section).Id == ID) //区段ID唯一
                        {
                            return true;
                        }
                    }
                    return false;
                });
                return topo;


            }

            else if (type == 2)  //道岔
            {
                TopolotyNode topo = getxml.data.Topo.Find((TopolotyNode toponode) =>
                {
                    if (toponode.device is RailSwitch)
                    {
                        if ((toponode.device as RailSwitch).Id == ID) //type和id可以唯一确定道岔
                        {
                            return true;
                        }
                    }
                    return false;
                });
               
                return topo;
            }
            return null;
        }

        public TopolotyNode BaliseToIteam(string balise,int ID) //根据当前的应答器找到节点,道岔根据sectionName即可
        {
            if (balise.Substring(0, 1) == "T")
            {
                foreach (var item in getxml.data.Topo)
                {
                    if (item.device.Name == balise.Substring(0, 5)) //区段时只需要根据Name判断就可以,nameT0103
                    {
                        return item;
                    }
                }
            }

            else if (balise.Substring(0, 1) == "Z")
            {
                foreach (var item in getxml.data.Topo)
                {
                    if (item.device.Name == balise.Substring(0, 4)) //区段时只需要根据Name判断就可以,nameT0103
                    {
                        return item;
                    }
                }
            }

            else
            {
                TopolotyNode node = getxml.data.Topo.Find((TopolotyNode toponode) =>   //node是寻找到的节点，返回符合条件的
                {
                    if (toponode.device is RailSwitch)
                    {
                        RailSwitch railSwitch = toponode.device as RailSwitch;
                        return railSwitch.section.Name == balise.Substring(0, 5) && railSwitch.Id==ID;
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

        public TopolotyNode CurBaliseToIteam(string balise) //根据当前的应答器找到节点,道岔根据sectionName即可
        {
            if (balise.Substring(0, 1) == "T")
            {
                foreach (var item in getxml.data.Topo)
                {
                    if (item.device.Name == balise.Substring(0, 5)) //区段时只需要根据Name判断就可以,nameT0103
                    {
                        return item;
                    }
                }
            }

            else
            {
                TopolotyNode node = getxml.data.Topo.Find((TopolotyNode toponode) =>   //node是寻找到的节点，返回符合条件的
                {
                    if (toponode.device is RailSwitch)
                    {
                        RailSwitch railSwitch = toponode.device as RailSwitch;
                        return railSwitch.section.Name == balise.Substring(0, 5);
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

        public TopolotyNode SectionBaliseToIteam(string balise) //根据当前的应答器找到节点,道岔根据sectionName即可
        {
            if (balise.Substring(0, 1) == "T")
            {
                foreach (var item in getxml.data.Topo)
                {
                    if (item.device.Name == balise.Substring(0, 5)) //区段时只需要根据Name判断就可以,nameT0103
                    {
                        return item;
                    }
                }
            }
            return null;         
        }

        public List<TopolotyNode> RightNextCurBaliseList(string curBalise,int id) //由当前的应答器右寻得到一个列表。当前是直轨下一个只有一个元素，当前是道岔则有两个元素
        {
            List<TopolotyNode> nodes_list = BaliseToIteam(curBalise,id).Right;
            return nodes_list;
        }

        public List<TopolotyNode> RightNextCurBaliseList(string curBalise) //由当前的应答器右寻得到一个列表。当前是直轨下一个只有一个元素，当前是道岔则有两个元素
        {
            List<TopolotyNode> nodes_list = SectionBaliseToIteam(curBalise).Right;
            return nodes_list;
        }
        public List<TopolotyNode> LeftNextCurBaliseList(string curBalise, int id) //由当前的应答器右寻得到一个列表。当前是直轨下一个只有一个元素，当前是道岔则有两个元素
        {
            List<TopolotyNode> nodes_list = BaliseToIteam(curBalise,id).Left; //left本省就是一个列表
            return nodes_list;
        }
        public List<TopolotyNode> LeftNextCurBaliseList(string curBalise) //由当前的应答器右寻得到一个列表。当前是直轨下一个只有一个元素，当前是道岔则有两个元素
        {
            List<TopolotyNode> nodes_list = SectionBaliseToIteam(curBalise).Left; //left本省就是一个列表
            return nodes_list;
        }

        public string NextCurBaliseList(TopolotyNode Node) //根据节点得到Name,有可能是道岔是sectionName，有可能是直轨是Name
        {
            string NextName = "";
            if (Node.device is Section)
            {
                NextName = (Node.device as Section).Name;
            }
            else if (Node.device is RailSwitch)
            {
                NextName = (Node.device as RailSwitch).section.Name; //section的name是原来的section那么
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

        public void SectionGetOffDis(bool isLeftSearch, string curBalise, ref UInt32 offset, ref UInt32 distance_1,int ID)
        {
            int LogicCount = (BaliseToIteam(curBalise,ID).device as Section).LogicCount; //当是2时不是站台，有4个应答器
            string Key = curBalise.Substring(curBalise.IndexOf("_") + 1); //当前应答器是1_1
            if (LogicCount == 1)
            {
                if (curBalise.Substring(curBalise.Length - 3, 3) == "1_1")
                {
                    if (isLeftSearch)
                    {
                        offset = 100;
                        distance_1 = 120 - offset;
                    }
                    else
                    {
                        offset = 20;
                        distance_1 = 120 - offset;
                    }
                }

                if (curBalise.Substring(curBalise.Length - 3, 3) == "1_2")
                {
                    if (isLeftSearch)
                    {
                        offset = 20;
                        distance_1 = 120 - offset;
                    }
                    else
                    {
                        offset = 100;
                        distance_1 = 120 - offset;
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

        public void SwitchGetOffDis(bool isLeftSearch, string curBalise, ref UInt32 offset, ref UInt32 distance_1,int ID)
        {
            bool isUp = (BaliseToIteam(curBalise,ID).device as RailSwitch).IsUp;
            bool isLeft = (BaliseToIteam(curBalise,ID).device as RailSwitch).IsLeft;
            string Key = curBalise.Substring(curBalise.IndexOf("_") + 1); //取最后一个数字
            if (isLeft)
            {
                if (isLeftSearch == false) //右寻
                {
                    if (Key == "1")
                    {
                        if (hashTable.ht_2.Contains(curBalise.Substring(0, 5)))
                        {
                            offset = 20;
                            distance_1 = 25 - offset;
                        }
                        else
                        {
                            offset = 5;
                            distance_1 = 25 - offset;
                        }                    
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
                        offset = 5;
                        distance_1 = 25 - offset;
                    }
                }
                else //左寻
                {
                    if (Key == "1")
                    {
                        if (hashTable.ht_2.Contains(curBalise.Substring(0, 5)))
                        {
                            offset = 5;
                            distance_1 = 25 - offset;
                        }
                        else
                        {
                            offset = 20;
                            distance_1 = 25 - offset;
                        }
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
                        offset = 20;
                        distance_1 = 25 - offset;
                    }
                }
            }
            else  //方向偏右，有可能是4开道岔
            {
                if (isLeftSearch == false) //右寻
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
                else 
                //左寻
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

        }



    }
}
