using static System.Math;
using TraversePoint = TraverseNetworkAdjustment.DataStruct.TraversePoint;

namespace TraverseNetworkAdjustment.Common
{
    internal static class CommonCalculate
    {
        private const double trans = 1000;// 单位转换参数
        public static double GetDistance(TraversePoint p1, TraversePoint p2)// 两点距离
        {
            return Sqrt(Pow(GetDiffX(p1, p2), 2) + Pow(GetDiffY(p1, p2), 2));
        }
        public static double GetDiffX(TraversePoint p1, TraversePoint p2)// 两点x坐标差
        {
            return p2.X - p1.X;
        }
        public static double GetDiffY(TraversePoint p1, TraversePoint p2)// 两点y坐标差
        {
            return p2.Y - p1.Y;
        }

        public static double GetAlpha(TraversePoint p1, TraversePoint p2)// 获取两点的正方位角
        {
            double dy = p2.Y - p1.Y;
            if (dy == 0) return 0;
            double dx = p2.X - p1.X;
            if (dx == 0)
            {
                if (dy > 0) return PI / 2;
                else return PI / 2 * 3;
            }

            double alpha = Atan(dy / dx);
            if (dx > 0 && dy > 0)
                return alpha;// 第一象限
            else if ((dx < 0 && dy > 0) || (dx < 0 && dy < 0))
                return alpha + PI;// 第二象限、第三象限
            else if (dx > 0 && dy < 0)
                return alpha + 2 * PI;// 第四象限

            else throw new Exception("?!");
        }

    }
}
