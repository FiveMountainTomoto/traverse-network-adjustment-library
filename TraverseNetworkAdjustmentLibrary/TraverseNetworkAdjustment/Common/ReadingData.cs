using System.IO;
using TraverseNetworkAdjustment.DataStruct;

namespace TraverseNetworkAdjustment.Common
{
    internal static class ReadingData
    {
        internal static List<ObservedValue> ReadingObsValue(string filePath)
        {
            List<ObservedValue> obsValues = new List<ObservedValue>();
            try
            {
                using StreamReader sr = new StreamReader(filePath);
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine()!;
                    if (string.IsNullOrEmpty(line)) continue;
                    string[] split = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (split[1] == "LENGTH")
                    {
                        LengthObservedValue lenObsVal = new(split[0], split[2], double.Parse(split[4]), split[3]);
                        obsValues.Add(lenObsVal);
                    }
                    else if (split[1] == "ANG")
                    {
                        AngleObservedValue AngObsVal = new(split[0], split[3], double.Parse(split[5]), split[2], split[4]);
                        obsValues.Add(AngObsVal);
                    }
                    else continue;
                }
            }
            catch (Exception)
            {

                throw;
            }
            return obsValues;
        }

        internal static readonly char[] separator = [' ', '\t'];

        internal static Dictionary<string, Point> ReadingKnownPoint(string filePath)
        {
            Dictionary<string, Point> points = new();
            try
            {
                using StreamReader sr = new StreamReader(filePath);
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine()!;
                    if (string.IsNullOrEmpty(line)) continue;
                    string[] split = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    Point point;
                    if (split.Length == 3)
                    {
                        point = new TraversePoint(split[0], double.Parse(split[1]), double.Parse(split[2]));
                    }
                    else if (split.Length == 2)
                    {
                        point = new OrientationPoint(split[0], double.Parse(split[1]));
                    }
                    else continue;
                    points.Add(point.Name, point);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return points;
        }
        internal static SortedList<string, TraversePoint> ReadingUnknownPoint(string filePath)
        {
            SortedList<string, TraversePoint> points = new();
            try
            {
                using StreamReader sr = new StreamReader(filePath);
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine()!;
                    if (string.IsNullOrEmpty(line)) continue;
                    string[] split = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    TraversePoint point = new(split[0], double.Parse(split[1]), double.Parse(split[2]));
                    points.Add(point.Name, point);
                }
            }
            catch (Exception)
            {

                throw;
            }
            return points;
        }
    }
}