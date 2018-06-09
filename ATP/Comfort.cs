using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBTC
{
    class Comfort
    {
  
        public double totalEnergy;
        public double TotalEnergy
        {
            get { return totalEnergy; }
            set { totalEnergy = value; }
        }                                //总能量的值
        public double ATOtotalEnergy;
        public double ATOTotalEnergy
        {
            get { return ATOtotalEnergy; }
            set { ATOtotalEnergy = value; }
        }

        double trainm=2000;
        public double da;              //列车舒适度
        public double ATOda;

        public void CalculateEDa(double v, double prev, double a, double prea)
        {
            if (prev < v)
            {
                totalEnergy += 0.5 * (Math.Pow(v, 2) - Math.Pow(prev, 2)) * trainm;
            }
            else
            {

            }
            da = Math.Abs(a - prea);            
      
           
        }
        public void ATOCalculateEDa(double v, double prev, double a, double prea)
        {
            if (prev < v)
            {
                ATOtotalEnergy += 0.5 * (Math.Pow(v, 2) - Math.Pow(prev, 2)) * trainm;
            }
            else
            {

            }
            ATOda = Math.Abs(a - prea);


        }

    }
}
