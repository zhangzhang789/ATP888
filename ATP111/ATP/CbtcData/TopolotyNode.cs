using System;
using System.Collections.Generic;
using System.Xml;

namespace CbtcData
{
    public class TopolotyNode
    {
        public Device device { get; private set; }

        public List<TopolotyNode> Left { get; } = new List<TopolotyNode>();

        public List<TopolotyNode> Right { get; } = new List<TopolotyNode>();

        public TopolotyNode(XmlElement element, List<Device> devices)
        {
            string name = element.GetAttribute("DeviceName");
            int id = int.Parse(element.GetAttribute("DeviceID"));
            int ciId = int.Parse(element.GetAttribute("StationID"));

            device = devices.Find((Device device) =>
            {
                return device.Name == name && device.Id == id && device.CiId == ciId;
            });
        }

        /// <summary>
        /// 读取左右节点
        /// </summary>
        /// <param name="element">XML 元素</param>
        /// <param name="topo">所有节点列表</param>
        public void LoadSideNodes(XmlElement element, List<TopolotyNode> topo)
        {
            var leftNodes = element.SelectNodes("LeftNodes/Node");
            var rightNodes = element.SelectNodes("RightNodes/Node");

            LoadSideNodes(leftNodes, Left, topo);
            LoadSideNodes(rightNodes, Right, topo);
        }

        /// <summary>
        /// 读取边节点
        /// </summary>
        /// <param name="xmlNodes">左/右节点</param>
        /// <param name="sideNodes">左/右节点列表</param>
        /// <param name="allNodes">所有节点列表</param>
        private void LoadSideNodes(XmlNodeList xmlNodes, List<TopolotyNode> sideNodes, List<TopolotyNode> allNodes)
        {
            foreach (XmlElement item in xmlNodes)
            {
                string name = item.GetAttribute("DeviceName");
                int id = int.Parse(item.GetAttribute("DeviceID"));
                int ciId = int.Parse(item.GetAttribute("StationID"));

                if (!(name == "NULL" && id == 0 && ciId == 0))
                {
                    TopolotyNode sideNode = allNodes.Find((TopolotyNode node) =>
                    {
                        return node.device.Name == name &&
                            node.device.Id == id &&
                            node.device.CiId == ciId;
                    });

                    if (sideNode != null)
                    {
                        sideNodes.Add(sideNode);
                    }
                }
            }
        }
    }
}
