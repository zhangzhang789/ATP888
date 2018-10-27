using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace TrainMessageEB
{
    class HashTable
    {
        public Hashtable ht_1 = new Hashtable();
        public Hashtable ht_2 = new Hashtable();


        #region 区段距离哈希表
        public void sectionHashTable()   //当在正常区段有四个应答器时
        {
            ht_1.Add("1_1", 100);
            ht_1.Add("1_2", 80);
            ht_1.Add("2_1", 40);
            ht_1.Add("2_2", 20);
        }
        public void sikai()   //当在正常区段有四个应答器时
        {
            ht_2.Add("W0106", "03_05");
            ht_2.Add("W0105", "02_04");
            ht_2.Add("W0411", "15_17");
            ht_2.Add("W0414", "16_18");
        }
        #endregion


    }
}
