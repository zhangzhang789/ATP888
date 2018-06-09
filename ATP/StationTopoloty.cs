using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace CBTC
{
    public class StationTopoloty
    {
        [XmlElement("TopolotyNode", typeof(TopolotyNode))]
        public List<TopolotyNode> Nodes { get; set; }

        internal void Open(string path, List<线路绘图工具.GraphicElement> elements)
        {
            TopolotyNode.Elements = elements;
            using (StreamReader sr = new StreamReader(path))
            {
                StationTopoloty newStationTopoloty = new XmlSerializer(typeof(StationTopoloty)).Deserialize(sr) as StationTopoloty;
                if (newStationTopoloty != null)
                {
                    Nodes = newStationTopoloty.Nodes;
                }
            }
        }
    }
}
