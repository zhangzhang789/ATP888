using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CBTC
{
    public interface ICheckDistance
    {
        bool IsDistanceIn(double distance);
        void SetLeftDistance(double value);
        void SetRightDistance(double value);
        double GetDistance();
    }
}