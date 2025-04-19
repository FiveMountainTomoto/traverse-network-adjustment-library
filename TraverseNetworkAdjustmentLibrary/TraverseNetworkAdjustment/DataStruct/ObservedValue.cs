namespace TraverseNetworkAdjustment.DataStruct
{
    // 观测值基类
    public abstract class ObservedValue
    {
        protected ObservedValue(string stationName, string stationPointName, double value)
        {
            StationName = stationName;
            StationPointName = stationPointName;
            Value = value;
        }

        public string StationName { get; private set; } = null!;
        public string StationPointName { get; private set; } = null!;
        public double Value { get; }
    }

    // 边长观测值
    public class LengthObservedValue : ObservedValue
    {
        public LengthObservedValue(string stationName, string stationPointName, double value, string obsPointName) : base(stationName, stationPointName, value)
        {
            ObsPointName = obsPointName;
        }

        public string ObsPointName { get; private set; } = null!;
    }

    // 角度观测值
    public class AngleObservedValue : ObservedValue
    {

        public AngleObservedValue(string stationName, string stationPointName, double value, string obsPoint1Name, string obsPoint2Name) : base(stationName, stationPointName, value)
        {
            ObsPoint1Name = obsPoint1Name;
            ObsPoint2Name = obsPoint2Name;
        }

        public string ObsPoint1Name { get; private set; } = null!;
        public string ObsPoint2Name { get; private set; } = null!;
    }


}
