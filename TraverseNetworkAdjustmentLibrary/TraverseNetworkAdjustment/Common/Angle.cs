using static System.Math;
namespace Traverse_Network_Adjustment.Common
{
    public static class Angle
    {
        public const double rou = 180 / PI * 3600;
        public static (int, int, double) GetDMS(double angRad)
        {
            double dms = angRad / PI * 180;
            int d = (int)dms;
            double ms = (dms - d) * 60;
            int m = (int)ms;
            double s = (ms - m) * 60;
            return (d, m, s);
        }
        public static double StandardDMS(double angDMS)
        {
            if (angDMS >= 0 && angDMS < 360)
            {
                return angDMS;
            }
            else if (angDMS < 0)
            {
                return StandardDMS(angDMS + 360);
            }
            else
            {
                return StandardDMS(angDMS - 360);
            }
        }
    }
}
