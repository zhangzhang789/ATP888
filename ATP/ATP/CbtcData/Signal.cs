using System;
using System.Xml;

namespace CbtcData
{
    public class Signal : Device
    {
        /// <summary>
        /// 信号机灯色
        /// </summary>
        public enum SignalColor
        {
            Red,            // 红
            Green,          // 绿
            Yellow,         // 黄
            RedYellow,      // 红黄
            DSFail,         // 灯丝断丝
            DS2Fail,        // 2灯丝断丝
            White,          // 白
            RedWhite,       // 红白
            DoubleYellow,   // 双黄
            Blue,           // 蓝
            DoubleGreen,    // 2绿
            GreenYellow     // 绿黄
        }

        /// <summary>
        /// 信号机类型
        /// </summary>
        public enum SignalType
        {
            L1,
            L2_蓝白, L2_红白, L2_红黄, L2_红绿,
            L3_绿白, L3_黄引, L_3黄白,
            L4_调, L4_引,
            L5,
            L6_黄绿, L6_双黄,
            L7,
            三方向出站,
            L5_黄闪黄,
        }

        /// <summary>
        /// 是否朝左
        /// </summary>
        public bool IsLeft { get; private set; }

        /// <summary>
        /// 信号机驱采类型
        /// </summary>
        public SignalType IoType { get; private set; }

        public override void LoadData(XmlElement element)
        {
            base.LoadData(element);

            IsLeft = bool.Parse(element.GetAttribute("IsLeft"));
            IoType = (SignalType)Enum.Parse(typeof(SignalType), element.GetAttribute("IOType"));
        }
    }
}
