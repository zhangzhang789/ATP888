using System;
using System.Xml;

namespace CbtcData
{
    public class Section : Device, IOccupy
    {
        /// <summary>
        /// 默认方向
        /// </summary>
        public enum DefaultDirection
        {
            Upward,     // 上行
            Downward    // 下行
        }

        /// <summary>
        /// 逻辑区段个数
        /// </summary>
        public int LogicCount { get; private set; }

        /// <summary>
        /// 公里标长度
        /// </summary>
        public double Distance { get; private set; }

        /// <summary>
        /// 默认方向
        /// </summary>
        public DefaultDirection Direction { get; set; }

        public override void LoadData(XmlElement element)
        {
            base.LoadData(element);

            LogicCount = int.Parse(element.GetAttribute("LogicCount"));
            Distance = int.Parse(element.GetAttribute("Distance"));
            Direction = (DefaultDirection)Enum.Parse(
                typeof(DefaultDirection), element.GetAttribute("Direction"));
        }
    }
}
