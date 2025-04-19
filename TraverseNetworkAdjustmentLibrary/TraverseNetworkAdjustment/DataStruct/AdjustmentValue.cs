namespace TraverseNetworkAdjustment.DataStruct
{
    public static class AdjustmentValue
    {
        // 观测值平差值
        public class ObsAdj
        {
            public string StationName { get; set; } = null!;
            public double Value { get; set; }
            public double Correction { get; set; }
            public double AdjustmentValue { get; set; }
            public bool IsDistance { get; set; }
            public double Accuracy { get; set; }
        }
        // 参数平差值和精度
        public class ParmAdj
        {
            public string Name { get; set; } = null!;
            public double X { get; set; }
            public double Y { get; set; }

            public double Xcorrection { get; set; }
            public double Ycorrection { get; set; }

            public double Xadjust { get; set; }
            public double Yadjust { get; set; }

            public double Xaccuracy { get; set; }
            public double Yaccuracy { get; set; }
            public double Accuracy { get; set; }
        }
    }
}
