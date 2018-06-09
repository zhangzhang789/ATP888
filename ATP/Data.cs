using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBTC
{
    class Data
    {
        public Hashtable htOffDis = new Hashtable();
        public void sectionHashTable()   //当在正常区段有四个应答器时
        {
            htOffDis.Add("1_1", 100);
            htOffDis.Add("1_2", 80);
            htOffDis.Add("2_1", 40);
            htOffDis.Add("2_2", 20);

        }
    }
}
