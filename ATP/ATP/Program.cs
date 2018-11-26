using CbtcData;
using System;
using Package;
using ConfigData;
using SocketSearch;
using TrainMessageEB;

namespace ATP
{
    class Program
    {

        
        static void Main(string[] args)
        {
            Socket_later socket_later = new Socket_later();     
            IPConfigure LoadIPConfig = new IPConfigure();
            socket_later.SocketStart();
       
            Console.ReadKey();

        }
    }
}