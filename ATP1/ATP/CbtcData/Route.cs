using System;
using System.Collections.Generic;
using System.Xml;

namespace CbtcData
{
    public class Route
    {
        /// <summary>
        /// 进路中的道岔
        /// </summary>
        public class RouteSwitch
        {
            /// <summary>
            /// 道岔
            /// </summary>
            public RailSwitch RSwitch { get; private set; }

            /// <summary>
            /// 道岔应该所处位置
            /// </summary>
            public RailSwitch.SwitchPosition Position { get; private set; }

            public RouteSwitch(RailSwitch rSwitch, RailSwitch.SwitchPosition position)
            {
                RSwitch = rSwitch;
                Position = position;
            }
        }

        /// <summary>
        /// 进路号
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// 始端信号机
        /// </summary>
        public Signal Start { get; private set; }

        /// <summary>
        /// 终端信号机
        /// </summary>
        public Signal End { get; private set; }

        /// <summary>
        /// 进路开放信号灯色
        /// </summary>
        public Signal.SignalColor Color { get; private set; }

        /// <summary>
        /// 进路方向
        /// </summary>
        public Section.DefaultDirection Direction { get; private set; }

        /// <summary>
        /// 接近区段
        /// </summary>
        public List<IOccupy> Incoming { get; } = new List<IOccupy>();

        /// <summary>
        /// 内方区段
        /// </summary>
        public List<IOccupy> In { get; } = new List<IOccupy>();

        /// <summary>
        /// 离去区段
        /// </summary>
        public List<IOccupy> Leaving { get; } = new List<IOccupy>();

        /// <summary>
        /// 进路包含道岔（防护和带动道岔）
        /// </summary>
        public List<RouteSwitch> Switches { get; } = new List<RouteSwitch>();

        /// <summary>
        /// 接近区段防护门
        /// </summary>
        public List<PSDoor> ApprochingPSDoors { get; } = new List<PSDoor>();

        /// <summary>
        /// 进路内方防护门
        /// </summary>
        public List<PSDoor> InDoors { get; } = new List<PSDoor>();

        /// <summary>
        /// 进路关联的继电器
        /// </summary>
        public List<RelayButton> Relays { get; } = new List<RelayButton>();

        public void LoadData(XmlElement element, StationElements allDevices)
        {
            Id = int.Parse(element.GetAttribute("ID"));
            Start = FindDevice(element.GetAttribute("Start"), allDevices) as Signal;
            End = FindDevice(element.GetAttribute("End"), allDevices) as Signal;
            Color = (Signal.SignalColor)Enum.Parse(typeof(Signal.SignalColor),
                element.GetAttribute("Color").Replace("Signal", ""));

            Direction = element.GetAttribute("Direction") == "DIRUP" ? 
                Section.DefaultDirection.Upward : Section.DefaultDirection.Downward;

            LoadOccupyDevice("Incoming", Incoming, element, allDevices);
            LoadOccupyDevice("In", In, element, allDevices);
            LoadOccupyDevice("Leaving", Leaving, element, allDevices);
            LoadSwitch(element, allDevices);
            LoadPSDoor(element, "IncomingDoors", ApprochingPSDoors, allDevices);
            LoadPSDoor(element, "InDoors", InDoors, allDevices);
            LoadRelay(element, "Relays", Relays, allDevices);
        }

        private void LoadRelay(XmlElement element, string attributeName, List<RelayButton> devices, StationElements allDevices)
        {
            string strTemp = element.GetAttribute(attributeName).TrimEnd();

            if (!string.IsNullOrEmpty(strTemp))
            {
                string[] strDevices = strTemp.Split(' ');
                foreach (var item in strDevices)
                {
                    devices.Add(FindDevice(item, allDevices) as RelayButton);
                }
            }
        }

        private void LoadPSDoor(XmlElement element, string attributeName, List<PSDoor> devices, StationElements allDevices)
        {
            string strTemp = element.GetAttribute(attributeName).TrimEnd();

            if (!string.IsNullOrEmpty(strTemp))
            {
                string[] strDevices = strTemp.Split(' ');
                foreach (var item in strDevices)
                {
                    devices.Add(FindDevice(item, allDevices) as PSDoor);
                }
            }
        }

        private Device FindDevice(string nameId, StationElements allDevice)
        {
            if (nameId.Contains('('))
            {
                int start = nameId.IndexOf('(') + 1;
                int length = nameId.IndexOf(')') - start;
                nameId = nameId.Substring(start, length);
            }
            string[] subs = nameId.Split('_');
            if (subs.Length > 1)
            {
                return allDevice.FindDevice(subs[subs.Length - 2], int.Parse(subs[subs.Length -1]));
            }

            return null;
        }

        private void LoadSwitch(XmlElement element, StationElements allDevices)
        {
            string strSwitch = element.GetAttribute("InSwitches").TrimEnd();
            string strPosition = element.GetAttribute("Positions").TrimEnd();

            if (!string.IsNullOrEmpty(strSwitch))
            {
                string[] names = strSwitch.Split(' ');
                string[] positions = strPosition.Split(' ');

                for (int i = 0; i < names.Length; i++)
                {
                    Switches.Add(new RouteSwitch(
                        FindDevice(names[i], allDevices) as RailSwitch,
                        (RailSwitch.SwitchPosition)Enum.Parse(typeof(RailSwitch.SwitchPosition), positions[i])));
                }
            }
        }

        private void LoadOccupyDevice(string attributeName, List<IOccupy> devices, XmlElement element, StationElements allDevices)
        {
            string strTemp = element.GetAttribute(attributeName).TrimEnd();

            if (!string.IsNullOrEmpty(strTemp))
            {
                string[] strDevices = strTemp.Split(' ');
                foreach (var item in strDevices)
                {
                    IOccupy device = FindDevice(item, allDevices) as IOccupy;
                    devices.Add(FindDevice(item, allDevices) as IOccupy);
                }
            }
        }
    }
}
