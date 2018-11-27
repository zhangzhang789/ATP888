using System.Collections.Generic;
using System.Xml;

namespace CbtcData
{
    /// <summary>
    /// 设备类
    /// </summary>
    public class Device
    {
        /// <summary>
        /// 设备名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 设备Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 联锁区Id
        /// </summary>
        public int CiId { get; set; }

        /// <summary>
        /// 从 xml 节点中读取数据
        /// </summary>
        /// <param name="element">xml 节点</param>
        public virtual void LoadData(XmlElement element)
        {
            Name = element.GetAttribute("Name");
            Id = int.Parse(element.GetAttribute("ID"));
            CiId = int.Parse(element.GetAttribute("StationID"));
        }

        /// <summary>
        /// 初始化设备间关联关系
        /// </summary>
        /// <param name="devices">设备列表</param>
        public virtual void Initialize(List<Device> devices) { }

        public override string ToString()
        {
            return string.Format("{0, -6}({1, -2}, {2})", Name, Id, CiId);
        }
    }
}
