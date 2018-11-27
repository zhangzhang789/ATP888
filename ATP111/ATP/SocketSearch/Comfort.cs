using System;
using System.Collections.Generic;
using System.Text;

namespace ATP.SocketSearch
{
    class Comfort
    {
        public double totalEnergy;
        double trainm = 2000;
        public double da;        //列车舒适度
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

    }
}
