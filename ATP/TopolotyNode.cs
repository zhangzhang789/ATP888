 using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;
using System.Text;
using 线路绘图工具;

namespace CBTC
{
    public class TopolotyNode : 线路绘图工具.TopolotyNode, IXmlSerializable
    {
        internal 线路绘图工具.Device FindDeviceByDistance(double distance)
        {
            if ((NodeDevice as ICheckDistance).IsDistanceIn(distance))
            {
                return NodeDevice;
            }
            else
            {
                foreach (TopolotyNode node in RightNodes)
                {
                    线路绘图工具.Device resultDevice = node.FindDeviceByDistance(distance);
                    if (resultDevice != null)
                    {
                        return resultDevice;
                        break;
                    }
                }
            }

            return null;
        }

        public TopolotyNode() { }

        public TopolotyNode(线路绘图工具.IConnectAble device) : base(device)
        {
        }

        System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
        {
            throw new NotImplementedException();
        }

        void IXmlSerializable.ReadXml(System.Xml.XmlReader reader)
        {
            base.ReadXml(reader);

            List<TopolotyNode> leftNodes = new List<TopolotyNode>();
            foreach (var item in LeftNodes)
            {
                leftNodes.Add(new TopolotyNode(item.ConnectableDevice));
            }
            LeftNodes.Clear();
            foreach (var item in leftNodes)
            {
                LeftNodes.Add(item);
            }

            List<TopolotyNode> rightNodes = new List<TopolotyNode>();
            foreach (var item in RightNodes)
            {
                rightNodes.Add(new TopolotyNode(item.ConnectableDevice));
            }
            RightNodes.Clear();
            foreach (var item in rightNodes)
            {
                RightNodes.Add(item);
            }
        }

        void IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
