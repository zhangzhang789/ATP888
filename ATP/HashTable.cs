using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace CBTC
{
    class HashTable
    {
        public Hashtable ht = new Hashtable();
        public Hashtable ht_1 = new Hashtable();
        public Hashtable ht_2 = new Hashtable();
        public Hashtable ht_3 = new Hashtable();
        public Hashtable ht_4 = new Hashtable();


        #region 区段距离哈希表
        public void sectionHashTable()   //当在正常区段有四个应答器时
        {
            ht_1.Add("1_1", 100);
            ht_1.Add("1_2", 80);
            ht_1.Add("2_1", 40);
            ht_1.Add("2_2", 20);

        }
        #endregion

        #region 应答器道岔标号哈希表
        public void switchHashTable()
        {
            ht.Add("W0103_0", "3_0");
            ht.Add("W0103_1", "3_0");
            ht.Add("W0103_2", "3_0");

            ht.Add("W0104_0", "4_1");
            ht.Add("W0104_1", "4_1");
            ht.Add("W0104_2", "4_1");

            ht.Add("W0105_0", "5_2");
            ht.Add("W0105_1", "7_4");
            ht.Add("W0105_2", "5_2");
            ht.Add("W0105_3", "7_4");

            ht.Add("W0106_0", "6_3");
            ht.Add("W0106_1", "8_5");
            ht.Add("W0106_2", "6_3");
            ht.Add("W0106_3", "8_5");

            ht.Add("W0109_0", "9_6");
            ht.Add("W0109_1", "9_6");
            ht.Add("W0109_2", "9_6");

            ht.Add("W0110_0", "10_7");
            ht.Add("W0110_1", "10_7");
            ht.Add("W0110_2", "10_7");

            ht.Add("W0203_0", "1_8");
            ht.Add("W0203_1", "1_8");
            ht.Add("W0203_2", "1_8");

            ht.Add("W0206_0", "2_9");
            ht.Add("W0206_1", "2_9");
            ht.Add("W0206_2", "2_9");

            ht.Add("W0213_0", "3_10");
            ht.Add("W0213_1", "3_10");
            ht.Add("W0213_2", "3_10");


            ht.Add("W0207_0", "7_12");
            ht.Add("W0207_1", "7_12");
            ht.Add("W0207_2", "7_12");

            ht.Add("W0217_0", "5_11");
            ht.Add("W0217_1", "5_11");
            ht.Add("W0217_2", "5_11");

            ht.Add("W0305_0", "1_13");
            ht.Add("W0305_1", "1_13");
            ht.Add("W0305_2", "1_13");

            ht.Add("W0308_0", "2_14");  //在element里面，前面是name，后面是id
            ht.Add("W0308_1", "2_14");
            ht.Add("W0308_2", "2_14");

            ht.Add("W0411_0", "1_15");
            ht.Add("W0411_1", "3_17");
            ht.Add("W0411_2", "1_15");
            ht.Add("W0411_3", "3_17");

            ht.Add("W0414_0", "2_16");
            ht.Add("W0414_1", "4_18");
            ht.Add("W0414_2", "2_16");
            ht.Add("W0414_3", "4_18");

            ht.Add("W0406_0", "6_20");
            ht.Add("W0406_1", "6_20");
            ht.Add("W0406_2", "6_20");

            ht.Add("W0409_0", "7_21");
            ht.Add("W0409_1", "7_21");
            ht.Add("W0409_2", "7_21");

            ht.Add("W0412_0", "8_22");
            ht.Add("W0412_1", "8_22");
            ht.Add("W0412_2", "8_22");

            ht.Add("W0403_0", "5_19");
            ht.Add("W0403_1", "5_19");
            ht.Add("W0403_2", "5_19");


        }
        #endregion
        public void Left2is5()  //写socket发的OFF时候用了，经过2是是5或者经过2时是20这两类
        {
            ht_2.Add("W0103", 5);
            ht_2.Add("W0104", 5);
            ht_2.Add("W0109", 5);
            ht_2.Add("W0207", 5);
            ht_2.Add("W0213", 5);
            ht_2.Add("W0305", 5);
            ht_2.Add("W0409", 5);
            ht_2.Add("W0406", 5);
            ht_2.Add("W0403", 5);
        }

        public void Left2is20()
        {
            ht_3.Add("W0110", 20);
            ht_3.Add("W0206", 20);
            ht_3.Add("W0203", 20);
            ht_3.Add("W0308", 20);
            ht_3.Add("W0105", 20);
            ht_3.Add("W0106", 20);          //四开道岔在这里
            ht_3.Add("W0414", 20);
            ht_3.Add("W0411", 20);
            ht_3.Add("W0412", 20);
            ht_3.Add("W0217", 20);
        }

        public void Is2()  //有站台的是两个应答器，没有站台的是四个应答器
        {
            ht_4.Add("T0107", 1);
            ht_4.Add("T0108", 1);
            ht_4.Add("T0113", 1);
            ht_4.Add("T0201", 1);
            ht_4.Add("T0112", 1);
            ht_4.Add("T0202", 1);
            ht_4.Add("T0211", 1);
            ht_4.Add("T0301", 1);
            ht_4.Add("T0303", 1);
            ht_4.Add("T0208", 1);
            ht_4.Add("T0302", 1);
            ht_4.Add("T0304", 1);
            ht_4.Add("T0307", 1);
            ht_4.Add("T0401", 1);
            ht_4.Add("T0310", 1);
            ht_4.Add("T0402", 1);
            ht_4.Add("T0405", 1);
            ht_4.Add("T0407", 1);
            ht_4.Add("T0408", 1);
            ht_4.Add("T0410", 1);            
        }

      
    }
}
