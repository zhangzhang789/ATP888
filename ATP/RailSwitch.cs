using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using 线路绘图工具;

namespace CBTC
{
    public class RailSwitch : 线路绘图工具.RailSwitch, ICheckDistance
    {
        static Pen RedPen_ = new Pen(Brushes.Red, 3);
        static Pen DefaultPen_ = new Pen(Brushes.Cyan, 3);
        public static double DefaultDistance_ = 20;

        public bool IsPositionNormal { get; set; }
        public bool IsPositionReverse { get; set; }

        public double LeftDistance { get; set; }
        public double RightDistance { get; set; }
        public double Distance { get; set; }

        RailSwitch()
        {
            IsPositionNormal = true;
            IsPositionReverse = false;
        }

        protected override void OnRender(System.Windows.Media.DrawingContext dc)
        {
            List<int> sectionIndexs = this.SectionIndexList[0];
            List<int> normalIndexs = SectionIndexList[1];
            List<int> reverseIndexs = SectionIndexList[2];

            foreach (int index in sectionIndexs)
            {
                Line line = graphics_[index] as Line;
                dc.DrawLine(RedPen_, line.Points[0], line.Points[1]);
            }

            if (IsPositionNormal)
            {
                foreach (int index in normalIndexs)
                {
                    Line line = graphics_[index] as Line;
                    dc.DrawLine(RedPen_, line.Points[0], line.Points[1]);
                }

                foreach (int index in reverseIndexs)
                {
                    Line line = graphics_[index] as Line;
                    dc.DrawLine(DefaultPen_, line.Points[0], line.Points[1]);
                }
            }
            else
            {
                foreach (int index in reverseIndexs)
                {
                    Line line = graphics_[index] as Line;
                    dc.DrawLine(RedPen_, line.Points[0], line.Points[1]);
                }

                foreach (int index in normalIndexs)
                {
                    Line line = graphics_[index] as Line;
                    dc.DrawLine(DefaultPen_, line.Points[0], line.Points[1]);
                }
            }
        }

        public bool IsDistanceIn(double distance)
        {
            return distance <= LeftDistance && distance >= RightDistance;
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
