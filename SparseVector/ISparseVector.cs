namespace Practical1
{
    public interface ISparseVector
    {
        int Dimension { get; }
        int NonZeroCount { get; }
        double this[int index] { get; }
        SparseVector Add(SparseVector other);
        SparseVector Subtract(SparseVector other);
        SparseVector Multiply(double scalar);
        double Norm();
        double Dot(SparseVector other);
    }
}
