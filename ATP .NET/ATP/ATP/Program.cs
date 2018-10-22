using CbtcData;
using System;
using Package;
using ConfigData;
using SocketSearch;

namespace ATP
{
    class Program
    {

        public static StationElements data;
        
        static void Main(string[] args)
        {
            Socket_later socket_later = new Socket_later();
            data = new StationElements();
            data.LoadDevices("StationElements.xml");
            data.LoadTopo("StationTopoloty.xml");
            data.LoadRoutes("RouteList.xml");
            data.ClearEmptyNode();
        
            IPConfigure LoadIPConfig = new IPConfigure();
     
            Console.ReadKey();

        }
    }
}