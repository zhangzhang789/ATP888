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
        public void IDTypeConvertName(byte type, byte ID,ref Section nowSection,ref string MAEndLink, ref RailSwitch nowrailswitch) //用type和id得到名字。是在element，element有section和railswitch。topology图是用于左寻还是右寻的。socket传来的是type和id，为了和应答器的做比较，需要由type和ID得到应答器名字。
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
                nowSection = section;
                MAEndLink = section.Name;  // T0301 。区段的名字是Name
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
                nowrailswitch = railswitch;
                MAEndLink = railswitch.SectionName;//W0414。道岔的名字是SectionName
            
            }
        }

        public void SectionDeviceNameGetTop(string deviceName,ref TopolotyNode nowTopolotyNode) //当是区段时由deviceName得到拓扑节点，可以左寻和右寻。区段的deviceName是T0307,由应答器前五位得到
        {
            foreach (var item in ATP.stationTopoloty_.Nodes)
            {
                if (item.NodeDevice.Name == deviceName)
                {
                    nowTopolotyNode = item;
                }
            }
        } 

        public void RailSwitchDeviceNameIDGetTop(string deviceName, int deviceID,ref TopolotyNode nowTopolotyNode)//当是道岔时需要由name和id。道岔的name和id是2,14.原来是由hash表得来，由着两个可以唯一确定道岔节点。注意有的右节点name是null
        {
            foreach (var item in ATP.stationTopoloty_.Nodes)
            {
                if (item.NodeDevice.Name == deviceName && item.NodeDevice.ID == deviceID) //取实际的线路中看看是什么东西
                {
                    nowTopolotyNode = item;
                }
            }
        } 
            
        public bool IsRailswitchVoid(byte type) //输入type判断是否是道岔
        {
            if (type == 1)
            {
                return false;
            }
            else
            {
                return true;
            }
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

        public void SectionGetOffDis_2(bool isLeftSearch,string curBalise,ref int offset,ref int distance_1) //站台区段两个应答器时的偏移量
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
        Data data = new Data();
        public void SectionGetOffDis_4(bool isLeftSearch, string curBalise, ref int offset, ref int distance_1) //非站台区段四个应答器的偏移量
        {
            data.sectionHashTable();
            string curBalise0_5 = curBalise.Substring(0, 5);
            foreach (string key in data.htOffDis.Keys)
            {
                if (key == curBalise.Substring(curBalise.IndexOf("_") + 1))
                {
                    if (isLeftSearch)
                    {
                        distance_1 = (120 - (int)data.htOffDis[key]);
                        offset = 120 - distance_1;
                    }
                    else
                    {
                        distance_1 = (int)data.htOffDis[key];
                        offset = (120 - (int)data.htOffDis[key]);
                    }
                }
            }

        }

        public void RailswitchDanOffDis(RailSwitch railSwitch,bool isLeftSearch,string curBalise,bool dingWei, ref int offset, ref int distance_1) //单动道岔的偏移量和距离
        {
            int RailswitchID = Convert.ToInt32(curBalise.Substring(curBalise.IndexOf("_") + 1)); //道岔的各段号。W0106_0取最后一个
            if ((railSwitch.IsLeft==true && railSwitch.IsUp==true) || (railSwitch.IsLeft==true && railSwitch.IsUp == false)) //第一种情况
            {
                if (isLeftSearch==true)  //第一种情况下的左寻
                {
                    if (dingWei == true)
                    {
                        if (RailswitchID == 0)
                        {
                            offset = 5;
                            distance_1 = 20;
                        }
                        else if (RailswitchID == 1)
                        {
                            offset = 20;
                            distance_1 = 5;
                        }
                    }
                    else
                    {
                        if (RailswitchID == 0)
                        {
                            offset = 5;
                            distance_1 = 20;
                        }
                        else if (RailswitchID == 2)
                        {
                            offset = 20;
                            distance_1 = 5;
                        }
                    }
                }
                else      //第一种情况下的右寻
                {
                    if (dingWei == true)
                    {
                        if (RailswitchID == 1)
                        {
                            offset = 5;
                            distance_1 = 20;
                        }
                        else if (RailswitchID == 0)
                        {
                            offset = 20;
                            distance_1 = 5;
                        }
                    }
                    else
                    {
                        if (RailswitchID == 2)
                        {
                            offset = 5;
                            distance_1 = 20;
                        }
                        else if (RailswitchID == 0)
                        {
                            offset = 20;
                            distance_1 = 5;
                        }
                    }
                }
            }
            else    //第二种情况下
            {
                if (isLeftSearch == true)  //第二种情况下的左寻
                {
                    if (dingWei == true)
                    {
                        if (RailswitchID == 0)
                        {
                            offset = 20;
                            distance_1 = 5;
                        }
                        else if (RailswitchID == 1)
                        {
                            offset = 5;
                            distance_1 = 20;
                        }
                    }
                    else
                    {
                        if (RailswitchID == 2)
                        {
                            offset = 5;
                            distance_1 = 20;
                        }
                        else if (RailswitchID == 0)
                        {
                            offset = 20;
                            distance_1 = 5;
                        }
                    }
                }
                else      //第二种情况下的右寻
                {
                    if (dingWei == true)
                    {
                        if (RailswitchID == 0)
                        {
                            offset = 5;
                            distance_1 = 20;
                        }
                        else if (RailswitchID == 1)
                        {
                            offset = 20;
                            distance_1 = 5;
                        }
                    }
                    else
                    {
                        if (RailswitchID == 0)
                        {
                            offset = 5;
                            distance_1 = 20;
                        }
                        else if (RailswitchID == 2)
                        {
                            offset = 20;
                            distance_1 = 5;
                        }
                    }
                }
            }
        }

        public void RailswitchSiKaiOffDis(RailSwitch railSwitch, bool isLeftSearch, string curBalise, bool dingWei, ref int offset, ref int distance_1) //四开道岔寻找道岔和偏移量
        {
            int RailswitchID = Convert.ToInt32(curBalise.Substring(curBalise.IndexOf("_") + 1)); //道岔的各段号。W0106_0取最后一个
            if(dingWei == true) //方向朝上的四开道岔的定位。方向朝上和方向朝下都是一样的。
             {
                    if (isLeftSearch == true)
                    {
                        if (RailswitchID == 1)
                        {
                            offset = 5;
                            distance_1 = 20;
                        }
                        else if (RailswitchID == 0)
                        {
                            offset = 20;
                            distance_1 = 5;
                        }
                    }
                    else
                    {
                         if (RailswitchID == 0)
                        {
                            offset = 5;
                            distance_1 = 20;
                        }
                        else if (RailswitchID == 1)
                        {
                            offset = 20;
                            distance_1 = 5;
                        }
                    }
              }
                else   //方向朝上的四开道岔的反位
              {
                    if (isLeftSearch == true)
                    {
                        if (RailswitchID == 1)
                        {
                            offset = 5;
                            distance_1 = 20;
                        }
                        else if (RailswitchID == 3)
                        {
                            offset = 20;
                            distance_1 = 5;
                        }
                    }
                    else
                    {
                        if (RailswitchID == 0)
                        {
                            offset = 5;
                            distance_1 = 20;
                        }
                        else if (RailswitchID == 2)
                        {
                            offset = 20;
                            distance_1 = 5;
                        }
                    }
               }            
        }




    }
}
