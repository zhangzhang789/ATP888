using System;
using System.Collections.Generic;
using System.Xml;

namespace CbtcData
{
    public class RailSwitch : Device, IOccupy
    {
        public enum SwitchPosition
        {
            PosNormal,
            PosReverse,
            PosNeither,
            PosFailure,
        }

        /// <summary>
        /// 所属区段
        /// </summary>
        public Section section { get; private set; }

        /// <summary>
        /// 双动道岔
        /// </summary>
        public RailSwitch DoubleSwitch { get; private set; }

        /// <summary>
        /// 默认方向
        /// </summary>
        public Section.DefaultDirection Direction { get; private set; }

        /// <summary>
        /// 定位公里标长度
        /// </summary>
        public double NormalDistance { get; private set; }

        /// <summary>
        /// 反位公里标长度
        /// </summary>
        public double ReverseDistance { get; private set; }

        /// <summary>
        /// 是否朝上
        /// </summary>
        public bool IsUp { get; private set; }

        /// <summary>
        /// 是否朝左
        /// </summary>
        public bool IsLeft { get; private set; }

        public override void LoadData(XmlElement element)
        {
            base.LoadData(element);

            section = new Section()
            {
                Name = element.GetAttribute("SectionName"),
                Id = int.Parse(element.GetAttribute("SectionID")),
                CiId = this.CiId,
            };

            string doubleName = element.GetAttribute("DoubleName");
            if (!string.IsNullOrEmpty(doubleName))
            {
                DoubleSwitch = new RailSwitch()
                {
                    Name = doubleName,
                    Id = int.Parse(element.GetAttribute("DoubleID"))
                };
            }

            Direction = (Section.DefaultDirection)Enum.Parse(
                typeof(Section.DefaultDirection), element.GetAttribute("Direction"));
            section.Direction = Direction;
            NormalDistance = int.Parse(element.GetAttribute("NormalDistance"));
            ReverseDistance = int.Parse(element.GetAttribute("ReverseDistance"));
            IsUp = bool.Parse(element.GetAttribute("IsUp"));
            IsLeft = bool.Parse(element.GetAttribute("IsLeft"));
        }

        public override void Initialize(List<Device> devices)
        {
            if (DoubleSwitch != null)
            {
                DoubleSwitch = (RailSwitch)devices.Find((Device device) =>
                {
                    if (device is RailSwitch)
                    {
                        return device.Name == DoubleSwitch.Name && device.Id == DoubleSwitch.Id;
                    }
                    return false;
                });
            }
        }
    }
}
