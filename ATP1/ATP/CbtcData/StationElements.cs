using System;
using System.Collections.Generic;
using System.Xml;

namespace CbtcData
{
    public class StationElements
    {
        /// <summary>
        /// 设备列表
        /// </summary>
        public List<Device> Devices { get; } = new List<Device>();
        
        /// <summary>
        /// 拓扑列表
        /// </summary>
        public List<TopolotyNode> Topo { get; } = new List<TopolotyNode>();

        /// <summary>
        /// 进路表
        /// </summary>
        public List<Route> Routes { get; } = new List<Route>();

        /// <summary>
        /// 读取设备文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        public void LoadDevices(string fileName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);

            LoadDevices(doc, typeof(Signal));
            LoadDevices(doc, typeof(RailSwitch));
            LoadDevices(doc, typeof(Section));
            LoadDevices(doc, typeof(PSDoor));
            LoadDevices(doc, typeof(RelayButton));

            foreach (var device in Devices)
            {
                device.Initialize(Devices);
            }
        }

        /// <summary>
        /// 读取拓扑文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        public void LoadTopo(string fileName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);

            var nodes = doc.SelectNodes("StationTopoloty/TopolotyNode");

            // 读取 Topo 节点
            foreach (XmlElement item in nodes)
            {
                Topo.Add(new TopolotyNode(item, Devices));
            }

            // 读取每个节点的左右节点
            for (int i = 0; i < Topo.Count; i++)
            {
                Topo[i].LoadSideNodes(nodes[i] as XmlElement, Topo);
            }
        }

        /// <summary>
        /// 读取进路表
        /// </summary>
        /// <param name="fileName">文件名</param>
        public void LoadRoutes(string fileName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);

            var nodes = doc.SelectNodes("RouteCreator/Route");
            foreach (XmlElement item in nodes)
            {
                Route route = new Route();
                route.LoadData(item, this);
                Routes.Add(route);
            }
        }

        public Device FindDevice(string name, int id)
        {
            return Devices.Find((Device device) =>
            {
                if (device.Name == name && device.Id == id)
                {
                    return true;
                }
                else if (device is RailSwitch)
                {
                    RailSwitch rs = device as RailSwitch;
                    return rs.section.Name == name && rs.section.Id == id;
                }

                return false;
            });
        }

        /// <summary>
        /// 查找拓扑节点
        /// </summary>
        /// <param name="sideNodes">左边或右边的拓扑节点</param>
        private void FillNodes(List<TopolotyNode> sideNodes)
        {
            for (int i = 0; i < sideNodes.Count; i++)
            {
                sideNodes[i] = Topo.Find((TopolotyNode node) =>
                {
                    return node.device.Name == sideNodes[i].device.Name &&
                        node.device.Id == sideNodes[i].device.Id &&
                        node.device.CiId == sideNodes[i].device.CiId;
                });
            }
        }

        /// <summary>
        /// 按类型读取设备配置
        /// </summary>
        /// <param name="doc">xml doc</param>
        /// <param name="deviceType">设备类型</param>
        private void LoadDevices(XmlDocument doc, Type deviceType)
        {
            var devieNodes = doc.SelectNodes("StationElements/" + deviceType.Name);

            foreach (XmlElement item in devieNodes)
            {
                Device device = (Device)Activator.CreateInstance(deviceType);
                device.LoadData(item);
                Devices.Add(device);

                if (device is RailSwitch)
                {
                    RailSwitch rs = device as RailSwitch;
                    if (!Devices.Contains(rs.section))
                    {
                        Devices.Add(rs.section);
                    }
                }
            }
        }

        /// <summary>
        /// 清除没有用到的节点
        /// </summary>
        public void ClearEmptyNode()
        {
            Topo.RemoveAll((TopolotyNode node) =>
            {
                return node.device == null;
            });
        }
    }
}
