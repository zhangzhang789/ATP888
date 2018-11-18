using System;
using System.Collections.Generic;
using System.Text;
using CbtcData;

namespace ATP.TrainMessageEB
{
    class Getxml
    {
        public StationElements data;
        public void XMLInitialize()
        {
            data = new StationElements();
            data.LoadDevices("StationElements.xml");
            data.LoadTopo("StationTopoloty.xml");
            data.LoadRoutes("RouteList.xml");
            data.ClearEmptyNode();

        }
    }
}
