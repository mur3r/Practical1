using System;
using System.Globalization;
using System.Text;

namespace Practical1
{
    public sealed class SparseVector : ISparseVector
    {
        private sealed class Node
        {
            public readonly int Index;
            public readonly double Value;
            public Node Next;

            public Node(int index, double value)
            {
                Index = index;
                Value = value;
            }
        }

        private Node head;
        private Node tail;
        public int Dimension { get; }
        public int NonZeroCount { get; private set; }

        public SparseVector() : this(1) { }

        public SparseVector(int dimension)
        {
            if (dimension <= 0)
                throw new ArgumentOutOfRangeException(nameof(dimension));
            Dimension = dimension;
        }

        public SparseVector(double[] values)
            : this(values == null ? 1 : values.Length)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            for (int i = 0; i < values.Length; i++)
            {
                RequireFinite(values[i], nameof(values));
                Append(i, values[i]);
            }
        }

        public SparseVector(SparseVector source)
            : this(source == null ? 1 : source.Dimension)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            for (Node p = source.head; p != null; p = p.Next)
                Append(p.Index, p.Value);
        }

        private static void RequireFinite(double value, string name)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentException("Finite value required.", name);
        }

        private void Append(int index, double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new OverflowException("Non-finite result.");
            if (value == 0)
                return;
            var node = new Node(index, value);
            if (head == null)
                head = node;
            else
                tail.Next = node;
            tail = node;
            NonZeroCount++;
        }

#if RED
        public double this[int index]
        {
            get { throw new NotImplementedException(); }
        }

        public SparseVector Add(SparseVector other)
        {
            throw new NotImplementedException();
        }

        public SparseVector Subtract(SparseVector other)
        {
            throw new NotImplementedException();
        }

        public SparseVector Multiply(double scalar)
        {
            throw new NotImplementedException();
        }

        public double Norm()
        {
            throw new NotImplementedException();
        }

        public double Dot(SparseVector other)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }
#else
        private void CheckOther(SparseVector other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            if (Dimension != other.Dimension)
                throw new ArgumentException("Dimensions must match.");
        }

        public double this[int index]
        {
            get
            {
                if (index < 0 || index >= Dimension)
                    throw new ArgumentOutOfRangeException(nameof(index));
                Node p = head;
                while (p != null && p.Index < index)
                    p = p.Next;
                return p != null && p.Index == index ? p.Value : 0;
            }
        }

        public SparseVector Add(SparseVector other)
        {
            return Merge(other, 1);
        }

        public SparseVector Subtract(SparseVector other)
        {
            return Merge(other, -1);
        }

        private SparseVector Merge(SparseVector other, int sign)
        {
            CheckOther(other);
            var result = new SparseVector(Dimension);
            Node a = head;
            Node b = other.head;
            while (a != null || b != null)
            {
                if (b == null || (a != null && a.Index < b.Index))
                {
                    result.Append(a.Index, a.Value);
                    a = a.Next;
                }
                else if (a == null || b.Index < a.Index)
                {
                    result.Append(b.Index, sign * b.Value);
                    b = b.Next;
                }
                else
                {
                    result.Append(a.Index, a.Value + sign * b.Value);
                    a = a.Next;
                    b = b.Next;
                }
            }
            return result;
        }

        public SparseVector Multiply(double scalar)
        {
            RequireFinite(scalar, nameof(scalar));
            var result = new SparseVector(Dimension);
            if (scalar == 0)
                return result;
            for (Node p = head; p != null; p = p.Next)
                result.Append(p.Index, p.Value * scalar);
            return result;
        }

        public double Norm()
        {
            double scale = 0;
            double sum = 1;
            for (Node p = head; p != null; p = p.Next)
            {
                double value = Math.Abs(p.Value);
                if (scale < value)
                {
                    double ratio = scale / value;
                    sum = 1 + sum * ratio * ratio;
                    scale = value;
                }
                else
                {
                    double ratio = value / scale;
                    sum += ratio * ratio;
                }
            }
            double result = scale == 0 ? 0 : scale * Math.Sqrt(sum);
            if (double.IsInfinity(result))
                throw new OverflowException("Non-finite norm.");
            return result;
        }

        public double Dot(SparseVector other)
        {
            CheckOther(other);
            double result = 0;
            double correction = 0;
            Node a = head;
            Node b = other.head;
            while (a != null && b != null)
            {
                if (a.Index < b.Index)
                    a = a.Next;
                else if (b.Index < a.Index)
                    b = b.Next;
                else
                {
                    double product = a.Value * b.Value;
                    double next = result + product;
                    if (Math.Abs(result) >= Math.Abs(product))
                        correction += (result - next) + product;
                    else
                        correction += (product - next) + result;
                    result = next;
                    a = a.Next;
                    b = b.Next;
                }
            }
            result += correction;
            if (double.IsNaN(result) || double.IsInfinity(result))
                throw new OverflowException("Non-finite dot product.");
            return result;
        }

        public override bool Equals(object obj)
        {
            var other = obj as SparseVector;
            if (other == null || Dimension != other.Dimension)
                return false;
            Node a = head;
            Node b = other.head;
            while (a != null && b != null)
            {
                if (a.Index != b.Index || a.Value != b.Value)
                    return false;
                a = a.Next;
                b = b.Next;
            }
            return a == null && b == null;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = Dimension;
                for (Node p = head; p != null; p = p.Next)
                {
                    hash = hash * 31 + p.Index;
                    hash = hash * 31 + p.Value.GetHashCode();
                }
                return hash;
            }
        }

        public override string ToString()
        {
            var text = new StringBuilder("(");
            Node p = head;
            for (int i = 0; i < Dimension; i++)
            {
                if (i > 0)
                    text.Append("; ");
                double value = p != null && p.Index == i ? p.Value : 0;
                text.Append(value.ToString("G17", CultureInfo.InvariantCulture));
                if (p != null && p.Index == i)
                    p = p.Next;
            }
            return text.Append(")").ToString();
        }
#endif
    }

    internal static class Program
    {
        private static void Main()
        {
            var a = new SparseVector(new double[] { 3, 0, 4 });
            var b = new SparseVector(new double[] { 0, 2, -4 });
            Console.WriteLine("a = " + a);
            Console.WriteLine("b = " + b);
            Console.WriteLine("a + b = " + a.Add(b));
            Console.WriteLine("a - b = " + a.Subtract(b));
            Console.WriteLine("2 * a = " + a.Multiply(2));
            Console.WriteLine("|a| = " + a.Norm());
            Console.WriteLine("a . b = " + a.Dot(b));
        }
    }
}
