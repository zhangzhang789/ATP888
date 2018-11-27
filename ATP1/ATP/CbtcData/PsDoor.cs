using System.Xml;

namespace CbtcData
{
    /// <summary>
    /// 屏蔽门
    /// </summary>
    public class PSDoor : Device
    {
        /// <summary>
        /// 前缀名称（车站）
        /// </summary>
        public string PreName { get; private set; }

        public override void LoadData(XmlElement element)
        {
            base.LoadData(element);

            PreName = element.GetAttribute("PreName");
        }
    }
}
