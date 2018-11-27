using System;
using System.Collections.Generic;
using System.Text;

namespace ATP.SocketSearch
{
    class Atp2Fault : Atp2OtherSystem
    {
        public override void Initialize()
        {
            CreateSocket("Fault");
        }
    }
}
