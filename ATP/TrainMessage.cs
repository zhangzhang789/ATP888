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

    }
}
