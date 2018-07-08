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
        public string IDTypeConvertName(byte type, byte ID) //用type和id得到名字
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

        public void SectionGetOffDis(bool isLeftSearch,string curBalise,ref int offset,ref int distance_1)
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
}
