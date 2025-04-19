using MathNet.Numerics.LinearAlgebra;
using TraverseNetworkAdjustment.Common;
using TraverseNetworkAdjustment.DataStruct;
using static Traverse_Network_Adjustment.Common.Angle;
using static TraverseNetworkAdjustment.Common.CommonCalculate;
using static TraverseNetworkAdjustment.DataStruct.AdjustmentValue;
using TraversePoint = TraverseNetworkAdjustment.DataStruct.TraversePoint;

namespace TraverseNetworkAdjustment
{
    public class TraverseNetwork
    {
        private readonly Dictionary<string, Point> _knPoints;// 已知点
        private readonly SortedList<string, TraversePoint> _unknPoints;// 未知点
        private readonly List<ObservedValue> _obsVals;// 观测值列表
        private readonly double _angAccu;// 先验测角精度
        private readonly (double, double) _disAccu;// 先验测距精度 (固定误差, 比例误差系数) 1 + 0.1ppm = (1, 0.1)
        private readonly int n;// 观测数
        private readonly int t;// 必要观测数
        private readonly int r;// 多余观测数

        private IndirectAdjustment? _adj;// 平差类
        private int[]? _errEquaTypes;// 记录误差方程的类型（0 角度观测值、1 距离观测值）
        private List<TraversePoint>? _adUnknPoints;// 平差后点位
        public bool IsCalculate { get; private set; }
        public double AngleMSE { get; private set; }// 测角单位权中误差

        public IndirectAdjustment Adjustment
        {
            get
            {
                if (IsCalculate) return _adj!;
                else throw new Exception("请先进行计算！");
            }
        }

        public TraverseNetwork(Dictionary<string, Point> knPoints, SortedList<string, TraversePoint> unknPoints, List<ObservedValue> obsVals, double angleAccu, (double, double) disAccu)
        {
            _knPoints = knPoints;
            _unknPoints = unknPoints;
            _obsVals = obsVals;
            _angAccu = angleAccu;
            _disAccu = disAccu;
            n = obsVals.Count;
            t = unknPoints.Count * 2;
            r = n - t;
            IsCalculate = false;
        }
        public TraverseNetwork(string obsPath, string kpPath, string ukpPath, double angleAccu, (double, double) disAccu)
        {
            _knPoints = ReadingData.ReadingKnownPoint(kpPath);
            _unknPoints = ReadingData.ReadingUnknownPoint(ukpPath);
            _obsVals = ReadingData.ReadingObsValue(obsPath);
            _angAccu = angleAccu;
            _disAccu = disAccu;
            n = _obsVals.Count;
            t = _unknPoints.Count * 2;
            r = n - t;
            IsCalculate = false;
        }

        public void AdjustmentCalculate()
        {
            try
            {
                double[,] _B = new double[n, t];// 误差方程系数矩阵B
                double[] _l = new double[n];// 误差方程闭合差向量 l
                double[,] _P = new double[n, n];// 权阵 P
                _errEquaTypes = new int[n];
                int ni = 0;// 表示一条误差方程在 B 中的行索引
                foreach (var obs in _obsVals)// 遍历观测值，列误差方程
                {
                    if (obs is LengthObservedValue)// 边长误差方程
                    {
                        LengthObservedValue _obs = (LengthObservedValue)obs;
                        bool isP1Known = SearchPointIsKnown(_obs.StationPointName, out Point _p1, out int i1);
                        bool isP2Known = SearchPointIsKnown(_obs.ObsPointName, out Point _p2, out int i2);
                        TraversePoint p1 = (TraversePoint)_p1;
                        TraversePoint p2 = (TraversePoint)_p2;
                        double xDiff = GetDiffX(p1, p2);
                        double yDiff = GetDiffY(p1, p2);
                        double s0 = GetDistance(p1, p2);
                        if (!isP1Known)
                        {
                            _B[ni, i1] = -(xDiff / s0);
                            _B[ni, i1 + 1] = -(yDiff / s0);
                        }
                        if (!isP2Known)
                        {
                            _B[ni, i2] = xDiff / s0;
                            _B[ni, i2 + 1] = yDiff / s0;
                        }
                        _l[ni] = (_obs.Value - s0) * 1000;
                        double accu = _disAccu.Item1 + _disAccu.Item2 * _obs.Value / 1000;
                        _P[ni, ni] = _angAccu * _angAccu / (accu * accu);
                        _errEquaTypes[ni] = 1;
                    }
                    if (obs is AngleObservedValue)// 角度误差方程
                    {
                        AngleObservedValue _obs = (AngleObservedValue)obs;
                        bool isP1Known = SearchPointIsKnown(_obs.ObsPoint1Name, out Point _p1, out int i1);
                        bool isP2Known = SearchPointIsKnown(_obs.StationPointName, out Point _p2, out int i2);
                        bool isP3Known = SearchPointIsKnown(_obs.ObsPoint2Name, out Point _p3, out int i3);
                        TraversePoint p2 = (TraversePoint)_p2;
                        double a21 = 0, b21 = 0, a23 = 0, b23 = 0, alpha21 = 0, alpha23 = 0;
                        if (_p1 is TraversePoint)
                        {
                            TraversePoint p1 = (TraversePoint)_p1;
                            double dis = GetDistance(p2, p1) * 1000;
                            alpha21 = GetAlpha(p2, p1);
                            a21 = rou * Math.Sin(alpha21) / dis;
                            b21 = -rou * Math.Cos(alpha21) / dis;
                        }
                        else
                        {
                            OrientationPoint p1 = (OrientationPoint)_p1;
                            alpha21 = p1.AzimuthAngle > double.Pi ? p1.AzimuthAngle - double.Pi : p1.AzimuthAngle + double.Pi;
                        }
                        if (_p3 is TraversePoint)
                        {
                            TraversePoint p3 = (TraversePoint)_p3;
                            double dis = GetDistance(p2, p3) * 1000;
                            alpha23 = GetAlpha(p2, p3);
                            a23 = rou * Math.Sin(alpha23) / dis;
                            b23 = -rou * Math.Cos(alpha23) / dis;
                        }
                        else
                        {
                            OrientationPoint p3 = (OrientationPoint)_p3;
                            alpha23 = p3.AzimuthAngle > double.Pi ? p3.AzimuthAngle - double.Pi : p3.AzimuthAngle + double.Pi;
                        }

                        if (!isP1Known)
                        {
                            _B[ni, i1] = a21;
                            _B[ni, i1 + 1] = b21;
                        }
                        if (!isP2Known)
                        {
                            _B[ni, i2] = a23 - a21;
                            _B[ni, i2 + 1] = b23 - b21;
                        }
                        if (!isP3Known)
                        {
                            _B[ni, i3] = -a23;
                            _B[ni, i3 + 1] = -b23;
                        }
                        double ang0 = Math.Abs(alpha21 - alpha23);// 观测角的近似值
                        double ang0i = 2 * Math.PI - ang0;
                        // 不知道观测的是左角还是右角，所以都算出来，哪个接近观测值就是哪个
                        double l_value1 = (_obs.Value - ang0) * rou;
                        double l_value2 = (_obs.Value - ang0i) * rou;
                        _l[ni] = Math.Abs(l_value1) < Math.Abs(l_value2) ? l_value1 : l_value2;
                        _P[ni, ni] = 1;
                        _errEquaTypes[ni] = 0;
                    }
                    ++ni;
                }
                Matrix<double> B = Matrix<double>.Build.DenseOfArray(_B);
                Vector<double> l = Vector<double>.Build.DenseOfArray(_l);
                Matrix<double> P = Matrix<double>.Build.DenseOfArray(_P);
                // 平差计算
                _adj = new(B, P, l);

                // 平差后点位坐标
                _adUnknPoints = new();
                int i = 0;
                foreach (var p in _unknPoints.Values)
                {
                    TraversePoint adP = new(p.Name, p.X + _adj.x[2 * i] / 1000, p.Y + _adj.x[2 * i + 1] / 1000);
                    ++i;
                    _adUnknPoints.Add(adP);
                }
                AngleMSE = _adj.d0;
                IsCalculate = true;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public void GetResult(out List<ObsAdj> obsAdjs, out List<ParmAdj> parmAdjs)
        {
            if (!IsCalculate) throw new InvalidOperationException("请先进行计算！");
            var v = _adj!.V.ToArray();
            obsAdjs = new();
            for (int i = 0; i < v.Length; i++)
            {
                var obs = _obsVals[i];
                ObsAdj obsAdj = new()
                {
                    StationName = obs.StationName,
                    Value = obs.Value,
                    Correction = v[i],
                    Accuracy = Math.Sqrt(_adj.L_SquareError[i]),
                };
                if (_errEquaTypes![i] == 0)
                {
                    obsAdj.IsDistance = false;
                    obsAdj.AdjustmentValue = obsAdj.Value + obsAdj.Correction / 3600 / 180 * double.Pi;
                }
                else
                {
                    obsAdj.IsDistance = true;
                    obsAdj.AdjustmentValue = obs.Value + v[i] / 1000;
                }
                obsAdjs.Add(obsAdj);
            }

            parmAdjs = [];
            for (int i = 0; i < _unknPoints.Count; i++)
            {
                var p = _unknPoints.Values[i];
                var adp = _adUnknPoints![i];
                ParmAdj parmAdj = new()
                {
                    Name = p.Name,
                    X = p.X,
                    Y = p.Y,
                    Xcorrection = _adj.x[2 * i],
                    Ycorrection = _adj.x[2 * i + 1],
                    Xadjust = adp.X,
                    Yadjust = adp.Y,
                    Xaccuracy = _adj.d0 * Math.Sqrt(_adj.Q_xx[2 * i, 2 * i]),
                    Yaccuracy = _adj.d0 * Math.Sqrt(_adj.Q_xx[2 * i + 1, 2 * i + 1]),
                    Accuracy = _adj.d0 * Math.Sqrt(_adj.Q_xx[2 * i, 2 * i] + _adj.Q_xx[2 * i + 1, 2 * i + 1])
                };
                parmAdjs.Add(parmAdj);
            }
        }
        private bool SearchPointIsKnown(string pointName, out Point point, out int xIndex)// 已知点返回true，未知点返回false
        {
            bool isKnownPoint;
            TraversePoint traversePoint;
            // 尝试从未知点列表里找
            if (_unknPoints.TryGetValue(pointName, out traversePoint!))
            {
                isKnownPoint = false;
                point = traversePoint;
                // 找到了就再找出该点x坐标的在参数向量中的位置
                xIndex = _unknPoints.IndexOfKey(pointName) * 2;
            }
            // 没找到，尝试从已知点字典里找
            else if (_knPoints.TryGetValue(pointName, out point!))
            {
                isKnownPoint = true;
                xIndex = -1;
            }
            else throw new Exception("找不到点：" + pointName);
            return isKnownPoint;
        }
    }
}
