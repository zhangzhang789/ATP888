using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using System.Xml.Serialization;
using 线路绘图工具;

namespace CBTC
{
    public class StationElements
    {
        [XmlElement("Section", typeof(Section))]
        [XmlElement("RailSwitch", typeof(RailSwitch))]
        [XmlElement("Signal", typeof(Signal))]
        [XmlElement("CommandButton", typeof(CommandButton))]
        [XmlElement("SmallButton", typeof(SmallButton))]
        [XmlElement("Station", typeof(Station))]
        [XmlElement("GraphicElement", typeof(GraphicElement))]
        public List<GraphicElement> Elements { get; set; }

        public static StationElements Open(string path, Canvas canvas)
        {
            using (StreamReader sr = new StreamReader(path))
            {
                StationElements elements = new XmlSerializer(typeof(StationElements)).Deserialize(sr) as StationElements;
                return elements;
            }
        }
    }
}
