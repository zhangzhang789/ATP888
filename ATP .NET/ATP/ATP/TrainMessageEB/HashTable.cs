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



        #region 区段距离哈希表
        public void sectionHashTable()   //当在正常区段有四个应答器时
        {
            ht_1.Add("1_1", 100);
            ht_1.Add("1_2", 80);
            ht_1.Add("2_1", 40);
            ht_1.Add("2_2", 20);

        }
        #endregion

      
    }
}
