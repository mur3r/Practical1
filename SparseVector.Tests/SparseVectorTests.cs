using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Practical1;

namespace Practical1.Tests
{
    [TestClass]
    public class SparseVectorTests
    {
        private static SparseVector V(params double[] values)
        { return new SparseVector(values); }

        private static void AssertVector(
            SparseVector actual, params double[] expected)
        {
            Assert.AreEqual(expected.Length, actual.Dimension);
            int count = 0;
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], actual[i], 1e-12);
                if (expected[i] != 0) count++;
            }
            Assert.AreEqual(count, actual.NonZeroCount);
        }

        [TestMethod] // t01 додавання та вилучення нульової суми
        public void Add_MixedIndices_MergesAndRemovesZeros()
        {
            var a = V(1, 0, 3, 4, 6); // arrange
            var b = V(0, 2, -3, 5, 0);
            var result = a.Add(b); // act
            AssertVector(result, 1, 2, 0, 9, 6); // assert
        }

        [TestMethod] // t02 нульовий вектор і конструктор без параметрів
        public void Add_DefaultConstructor_UsesOneDimension()
        {
            var a = new SparseVector(); // arrange
            var result = a.Add(V(2)); // act
            AssertVector(result, 2); // assert
        }

        [TestMethod] // t03 різні розмірності при додаванні
        public void Add_DifferentDimensions_ThrowsArgument()
        {
            var a = V(1, 0, 3); // arrange
            Action action = () => a.Add(V(1, 2)); // act
            Assert.ThrowsException<ArgumentException>(action); // assert
        }

        [TestMethod] // t04 відсутній доданок
        public void Add_Null_ThrowsArgumentNull()
        {
            var a = V(1, 0, 3); // arrange
            Action action = () => a.Add(null); // act
            Assert.ThrowsException<ArgumentNullException>(action); // assert
        }

        [TestMethod] // t05 віднімання розріджених векторів
        public void Subtract_CommonIndices_ReturnsDifference()
        {
            var a = V(1, 0, 3); // arrange
            var b = V(0, 2, 4);
            var result = a.Subtract(b); // act
            AssertVector(result, 1, -2, -1); // assert
        }

        [TestMethod] // t06 віднімання вектора від себе
        public void Subtract_SameVector_ReturnsZero()
        {
            var a = V(1, 0, 3); // arrange
            var result = a.Subtract(a); // act
            AssertVector(result, 0, 0, 0); // assert
        }

        [TestMethod] // t07 різні розмірності при відніманні
        public void Subtract_DifferentDimensions_ThrowsArgument()
        {
            var a = V(1, 0, 3); // arrange
            Action action = () => a.Subtract(V(1, 2)); // act
            Assert.ThrowsException<ArgumentException>(action); // assert
        }

        [TestMethod] // t08 відсутній від’ємник
        public void Subtract_Null_ThrowsArgumentNull()
        {
            var a = V(1, 0, 3); // arrange
            Action action = () => a.Subtract(null); // act
            Assert.ThrowsException<ArgumentNullException>(action); // assert
        }

        [TestMethod] // t09 множення на від’ємне число без зміни джерела
        public void Multiply_Negative_ReturnsScaledVector()
        {
            var a = V(1, 0, -3); // arrange
            var result = a.Multiply(-2); // act
            AssertVector(result, -2, 0, 6); // assert
            AssertVector(a, 1, 0, -3);
        }

        [TestMethod] // t10 множення на 0
        public void Multiply_Zero_ReturnsScaledVector()
        {
            var a = V(1, 0, -3); // arrange
            var result = a.Multiply(0); // act
            AssertVector(result, 0, 0, 0); // assert
        }

        [TestMethod] // t11 некоректний множник nan
        public void Multiply_NaN_ThrowsArgument()
        {
            var a = V(1); // arrange
            Action action = () => a.Multiply(double.NaN); // act
            Assert.ThrowsException<ArgumentException>(action); // assert
        }

        [TestMethod] // t12 нескінченний множник
        public void Multiply_Infinity_ThrowsArgument()
        {
            var a = V(1); // arrange
            Action action = () => a.Multiply(double.PositiveInfinity); // act
            Assert.ThrowsException<ArgumentException>(action); // assert
        }

        [TestMethod] // t13 переповнення при множенні
        public void Multiply_Overflow_ThrowsOverflow()
        {
            var a = V(double.MaxValue); // arrange
            Action action = () => a.Multiply(2); // act
            Assert.ThrowsException<OverflowException>(action); // assert
        }

        [TestMethod] // t14 модуль вектора з різними знаками координат
        public void Norm_MixedSigns_ReturnsLength()
        {
            var a = V(-3, 0, 4); // arrange
            double result = a.Norm(); // act
            Assert.AreEqual(5, result, 1e-12); // assert
        }

        [TestMethod] // t15 обчислення модуля (0; 0; 0)
        public void Norm_Zero_ReturnsLength()
        {
            var a = new SparseVector(3); // arrange
            double result = a.Norm(); // act
            Assert.AreEqual(0, result, 1e-12); // assert
        }

        [TestMethod] // t16 компенсація похибки скалярного добутку
        public void Dot_Cancellation_PreservesSmallTerm()
        {
            var a = V(1e16, 1, -1e16); // arrange
            var b = V(2, 3, 2);
            double result = a.Dot(b); // act
            Assert.AreEqual(3, result, 1e-12); // assert
        }

        [TestMethod] // t17 скалярний добуток без спільних координат
        public void Dot_DisjointIndices_ReturnsProduct()
        {
            var a = V(1, 0, 3); // arrange
            var b = V(0, 2, 0);
            double result = a.Dot(b); // act
            Assert.AreEqual(0, result, 1e-12); // assert
        }

        [TestMethod] // t18 різні розмірності скалярного добутку
        public void Dot_DifferentDimensions_ThrowsArgument()
        {
            var a = V(1, 0, 3); // arrange
            Action action = () => a.Dot(V(1, 2)); // act
            Assert.ThrowsException<ArgumentException>(action); // assert
        }

        [TestMethod] // t19 відсутній другий множник
        public void Dot_Null_ThrowsArgumentNull()
        {
            var a = V(1, 0, 3); // arrange
            Action action = () => a.Dot(null); // act
            Assert.ThrowsException<ArgumentNullException>(action); // assert
        }

        [TestMethod] // t20 переповнення скалярного добутку
        public void Dot_Overflow_ThrowsOverflow()
        {
            var a = V(double.MaxValue); // arrange
            Action action = () => a.Dot(V(2)); // act
            Assert.ThrowsException<OverflowException>(action); // assert
        }

        [TestMethod] // t21 переповнення суми координат
        public void Add_Overflow_ThrowsOverflow()
        {
            var a = V(double.MaxValue); // arrange
            Action action = () => a.Add(a); // act
            Assert.ThrowsException<OverflowException>(action); // assert
        }

        [TestMethod] // t22 зчитування координати 1
        public void Indexer_Missing_ReturnsCoordinate()
        {
            var a = V(1, 0, 3); // arrange
            double result = a[1]; // act
            Assert.AreEqual(0, result, 1e-12); // assert
        }

        [TestMethod] // t23 індекс за межами вектора
        public void Indexer_Negative_ThrowsOutOfRange()
        {
            var a = V(1, 0, 3); // arrange
            Action action = () => { var value = a[-1]; }; // act
            Assert.ThrowsException<ArgumentOutOfRangeException>(action); // assert
        }

        [TestMethod] // t24 індекс за межами вектора
        public void Indexer_PastEnd_ThrowsOutOfRange()
        {
            var a = V(1, 0, 3); // arrange
            Action action = () => { var value = a[3]; }; // act
            Assert.ThrowsException<ArgumentOutOfRangeException>(action); // assert
        }

        [TestMethod] // t25 порівняння з копією вектора
        public void Equals_CopiedVector_ReturnsTrue()
        {
            var a = V(1, 0, 3); // arrange
            var b = new SparseVector(a);
            bool result = a.Equals(b); // act
            Assert.IsTrue(result); // assert
        }

        [TestMethod] // t26 порівняння різних координат
        public void Equals_DifferentValues_ReturnsFalse()
        {
            var a = V(1, 0, 3); // arrange
            bool result = a.Equals(V(1, 0, 4)); // act
            Assert.IsFalse(result); // assert
        }

        [TestMethod] // t27 порівняння різних розмірностей
        public void Equals_DifferentDimensions_ReturnsFalse()
        {
            var a = V(1, 0, 3); // arrange
            bool result = a.Equals(V(1, 0, 3, 0)); // act
            Assert.IsFalse(result); // assert
        }

        [TestMethod] // t28 порівняння з null
        public void Equals_Null_ReturnsFalse()
        {
            var a = V(1, 0, 3); // arrange
            bool result = a.Equals(null); // act
            Assert.IsFalse(result); // assert
        }

        [TestMethod] // t29 порівняння з іншим типом
        public void Equals_WrongType_ReturnsFalse()
        {
            var a = V(1, 0, 3); // arrange
            bool result = a.Equals("text"); // act
            Assert.IsFalse(result); // assert
        }

        [TestMethod] // t30 хеші рівних векторів
        public void GetHashCode_EqualVectors_ReturnsEqualHash()
        {
            var a = V(1, 0, 3); // arrange
            var b = new SparseVector(a);
            int result = a.GetHashCode(); // act
            Assert.AreEqual(result, b.GetHashCode()); // assert
        }

        [TestMethod] // t31 точні координати й сталий десятковий роздільник
        public void ToString_PreciseValues_UsesInvariantCulture()
        {
            var a = V(1.5, 0, -2, 1.0000000000000002); // arrange
            string expected = "(1.5; 0; -2; 1.0000000000000002)";
            var saved = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("uk-UA");
                string result = a.ToString(); // act
                Assert.AreEqual(expected, result); // assert
            }
            finally { CultureInfo.CurrentCulture = saved; }
        }

        [TestMethod] // t32 незалежність від вхідного масиву
        public void Add_SourceArrayChanged_PreservesStoredData()
        {
            double[] values = { 1, 0, 3 }; // arrange
            var a = new SparseVector(values);
            values[0] = 99;
            var result = a.Add(new SparseVector(3)); // act
            AssertVector(result, 1, 0, 3); // assert
        }

        [TestMethod] // t33 переповнення модуля вектора
        public void Norm_Overflow_ThrowsOverflow()
        {
            var a = V(double.MaxValue, double.MaxValue); // arrange
            Action action = () => a.Norm(); // act
            Assert.ThrowsException<OverflowException>(action); // assert
        }

    }
}
