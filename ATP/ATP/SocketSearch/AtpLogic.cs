using SocketSearch;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TrainMessageEB;

namespace ATP.SocketSearch
{
    class AtpLogic
    {
        public Atp2Curve curve = new Atp2Curve();
        public Atp2Dmi dmi = new Atp2Dmi();
        public Atp2Zc zc = new Atp2Zc();
        public Atp2Dc dc = new Atp2Dc();
        public Atp2Fault fault = new Atp2Fault();
        public TrainMessage trainMessage = new TrainMessage();
        public SearchLater searchLater = new SearchLater();
        public delegate void Receive_DMI_Data(byte[] DMIData);
        public delegate void Receive_ZC_Data(byte[] ZCData);
        public delegate void Receive_DC_Data(byte[] DCData);
        public delegate void Receive_Fault_Data(byte[] FaultData);
        public void Initialize()
        {
            curve.Initialize();
            dmi.Initialize();
            zc.Initialize();
            dc.Initialize();
            fault.Initialize();
            searchLater.GetHash();
            trainMessage.GetHash(); 
        }     
    }
}
