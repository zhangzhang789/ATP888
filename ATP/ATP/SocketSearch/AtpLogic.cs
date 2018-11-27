using ConfigData;
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
        Socket_later socket_later = new Socket_later();
 
        public void Initialize()
        {
            IPConfigure LoadIPConfig = new IPConfigure();
            socket_later.SocketInitialize();          
        }
        public void Start()
        {
            socket_later.SocketReceive();
        }
    }
}
