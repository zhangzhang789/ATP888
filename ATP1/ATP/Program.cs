using CbtcData;
using System;
using Package;
using ConfigData;
using SocketSearch;
using TrainMessageEB;
using ATP.SocketSearch;

namespace ATP
{
    class Program
    {

        
        static void Main(string[] args)
        {
            AtpLogic atpLogic = new AtpLogic();       
            atpLogic.Initialize();
            atpLogic.Start();
            
            Console.ReadKey();

        }
    }
}