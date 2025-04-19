namespace TraverseNetworkAdjustment.DataStruct
{
    public abstract class Point
    {
        public string Name { get; set; }

        protected Point(string name)
        {
            Name = name;
        }
    }
    public class TraversePoint : Point
    {
        public double X { get; private set; }
        public double Y { get; private set; }

        public TraversePoint(string name, double x, double y) : base(name)
        {
            X = x;
            Y = y;
        }
    }
    public class OrientationPoint : Point
    {
        public OrientationPoint(string name, double azimuthAngle) : base(name)
        {
            AzimuthAngle = azimuthAngle;
        }

        public double AzimuthAngle { get; private set; }
    }
}
