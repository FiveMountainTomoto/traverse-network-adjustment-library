using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;


namespace TraverseNetworkAdjustment.Common
{
    public class IndirectAdjustment
    {
        public Vector<double> V { get; }// 观测值改正数
        public Vector<double> x { get; }// 参数改正数
        public Matrix<double> P { get; }// 观测值权阵
        public Matrix<double> Q { get; }// 观测值协因数阵
        public int FreedomCount { get; }// 自由度
        public double VTPV { get; }// 改正数二次型
        public double d0 { get; }// 单位权中误差
        public Matrix<double> Q_xx { get; }// 参数平差值协因数阵
        public Matrix<double> Q_vv { get; }// 改正数协因数阵
        public Matrix<double> Q_LL { get; }// 观测值平差值协因数阵
        public double[] L_SquareError { get; }// 观测值平差值方差
        public double[] X_SquareError { get; }// 参数平差值方差
        public IndirectAdjustment(Matrix<double> B, Matrix<double> P, Vector<double> l)
        {
            FreedomCount = B.RowCount - B.ColumnCount;
            if (FreedomCount <= 0)
            {
                throw new Exception($"自由度小于等于0，n = {B.RowCount}，t = {B.ColumnCount}, r = {FreedomCount}");
            }
            this.P = P;
            Q = P.Inverse();
            var NBBi = (B.Transpose() * P * B).Inverse();
            x = NBBi * B.Transpose() * P * l;
            V = B * x - l;
            L_SquareError = new double[B.RowCount];
            X_SquareError = new double[B.ColumnCount];
            VTPV = (V.ToRowMatrix() * P * V)[0];
            d0 = Math.Sqrt(VTPV / FreedomCount);
            Q_xx = NBBi;
            Q_vv = Q - B * NBBi * B.Transpose();
            Q_LL = B * NBBi * B.Transpose();
            for (int i = 0; i < B.RowCount; i++)
            {
                L_SquareError[i] = d0 * d0 * Q_LL[i, i];
                if (i < B.ColumnCount)
                    X_SquareError[i] = d0 * d0 * Q_xx[i, i];
            }
        }
    }
}
