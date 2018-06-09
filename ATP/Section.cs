using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using 线路绘图工具;

namespace CBTC
{
    public class Section : 线路绘图工具.Section, ICheckDistance
    {
        static Pen RedPen_ = new Pen(Brushes.Red, 3);
        static Pen DefaultPen_ = new Pen(Brushes.Cyan, 3);
        static double DefualtDistance_ = 120;

        public double LeftDistance { get; set; }
        public double RightDistance { get; set; }
        public double Distance { get; set; }

        public bool IsDistanceIn(double distance)
        {
            return distance <= LeftDistance && distance >= RightDistance;
        }

        protected override void OnRender(System.Windows.Media.DrawingContext dc)
        {
            foreach (Graphic graphic in graphics_)
            {
                if (graphic is Line)
                {
                    Line line = graphic as Line;
                    dc.DrawLine(RedPen_, line.Points[0], line.Points[1]);
                }
            }
        }


        public void SetLeftDistance(double value)
        {
            LeftDistance = value;
        }

        public void SetRightDistance(double value)
        {
            RightDistance = value;
        }


        public double GetDistance()
        {
            return Distance;
        }

    }
}
