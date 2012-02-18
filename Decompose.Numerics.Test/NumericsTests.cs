﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Decompose.Numerics.Test
{
    [TestClass]
    public class NumericsTests
    {
        [TestMethod]
        public void TestSqrt1()
        {
            var p = BigInteger.Parse("12345678901234567890");
            var n = p * p;
            var p2 = IntegerMath.FloorSquareRoot(n);
            Assert.AreEqual(p, p2);
        }

        [TestMethod]
        public void TestSqrt2()
        {
            Assert.AreEqual(0, (int)IntegerMath.FloorSquareRoot(0));
            Assert.AreEqual(1, (int)IntegerMath.FloorSquareRoot(1));
            Assert.AreEqual(1, (int)IntegerMath.FloorSquareRoot(2));
            Assert.AreEqual(1, (int)IntegerMath.FloorSquareRoot(3));
            Assert.AreEqual(2, (int)IntegerMath.FloorSquareRoot(4));
            Assert.AreEqual(2, (int)IntegerMath.FloorSquareRoot(5));
            Assert.AreEqual(2, (int)IntegerMath.FloorSquareRoot(8));
            Assert.AreEqual(3, (int)IntegerMath.FloorSquareRoot(9));
            Assert.AreEqual(3, (int)IntegerMath.FloorSquareRoot(10));        
        }

        [TestMethod]
        public void TestPrimality1()
        {
            var algorithm = MillerRabin.Create(16, new Int32Reduction());
            var primes = Enumerable.Range(937, 1000 - 937)
                .Where(n => algorithm.IsPrime(n))
                .ToArray();
            var expected = new[] { 937, 941, 947, 953, 967, 971, 977, 983, 991, 997 };
            Assert.IsTrue(((IStructuralEquatable)primes).Equals(expected, EqualityComparer<int>.Default));
        }

        [TestMethod]
        public void TestPrimality2()
        {
            var algorithm = MillerRabin.Create(16, new UInt32MontgomeryReduction());
            Assert.IsTrue(new SieveOfErostothones().Take(10000).All(prime => algorithm.IsPrime((uint)prime)));
        }

        [TestMethod]
        public void TestPrimality3()
        {
            var primalityOld = new OldMillerRabin(16);
            TestPrimality3<int>(0, 10000, primalityOld, new Int32Reduction());
            //TestPrimality3<int>(0, 10000, primalityOld, new Int32MontgomeryReduction());
            TestPrimality3<int>(int.MaxValue - 10000 - 1, 10000, primalityOld, new Int32Reduction());
            //TestPrimality3<int>(int.MaxValue - 10000 - 1, 10000, primalityOld, new Int32MontgomeryReduction());
            TestPrimality3<uint>(0, 10000, primalityOld, new UInt32Reduction());
            TestPrimality3<uint>(0, 10000, primalityOld, new UInt32MontgomeryReduction());
            TestPrimality3<uint>(uint.MaxValue - 10000 - 1, 10000, primalityOld, new UInt32Reduction());
            TestPrimality3<uint>(uint.MaxValue - 10000 - 1, 10000, primalityOld, new UInt32MontgomeryReduction());
            TestPrimality3<long>(0, 10000, primalityOld, new Int64Reduction());
            //TestPrimality3<long>(0, 10000, primalityOld, new Int64MontgomeryReduction());
            TestPrimality3<long>(long.MaxValue - 10000 - 1, 10000, primalityOld, new Int64Reduction());
            //TestPrimality3<long>(long.MaxValue - 10000 - 1, 10000, primalityOld, new Int64MontgomeryReduction());
            TestPrimality3<ulong>(0, 10000, primalityOld, new UInt64Reduction());
            TestPrimality3<ulong>(0, 10000, primalityOld, new UInt64MontgomeryReduction());
            TestPrimality3<ulong>(ulong.MaxValue - 10000 - 1, 10000, primalityOld, new UInt64Reduction());
            TestPrimality3<ulong>(ulong.MaxValue - 10000 - 1, 10000, primalityOld, new UInt64MontgomeryReduction());
            TestPrimality3<BigInteger>(0, 10000, primalityOld, new BigIntegerReduction());
            TestPrimality3<BigInteger>(0, 10000, primalityOld, new BigIntegerMontgomeryReduction());
            TestPrimality3<BigInteger>((BigInteger)ulong.MaxValue * ulong.MaxValue, 10000, primalityOld, new BigIntegerReduction());
            TestPrimality3<BigInteger>((BigInteger)ulong.MaxValue * ulong.MaxValue, 10000, primalityOld, new BigIntegerMontgomeryReduction());
        }

        private void TestPrimality3<T>(T startValue, T countValue, IPrimalityAlgorithm<T> primalityOld, IReductionAlgorithm<T> reduction)
        {
            var start = (Integer<T>)startValue;
            var count = (Integer<T>)countValue;
            var primalityNew = MillerRabin.Create(16, reduction);
            for (var i = (Integer<T>)0; i < count; i++)
            {
                var n = start + i;
                var isPrimeOld = primalityOld.IsPrime(n);
                var isPrimeNew = primalityNew.IsPrime(n);
                Assert.AreEqual(isPrimeOld, isPrimeNew);
            }
        }

        [TestMethod]
        public void TestPollard1()
        {
            var expected = new[] { BigInteger.Parse("274177"), BigInteger.Parse("67280421310721") };
            var n = BigInteger.Parse("18446744073709551617");
            var algorithm = new PollardRho(1, 0);
            var factors = algorithm.Factor(n).OrderBy(factor => factor).ToArray();
            var product = factors.Aggregate((sofar, current) => sofar * current);
            Assert.AreEqual(n, product);
            Assert.IsTrue(((IStructuralEquatable)factors).Equals(expected, EqualityComparer<BigInteger>.Default));
        }

        [TestMethod]
        public void TestPollard2()
        {
            var expected = new[] { BigInteger.Parse("91739369"), BigInteger.Parse("266981831") };
            var n = expected.Aggregate(BigInteger.One, (sofar, factor) => sofar * factor);
            var algorithm = new PollardRho(1, 0);
            var factors = algorithm.Factor(n).OrderBy(factor => factor).ToArray();
            var product = factors.Aggregate((sofar, current) => sofar * current);
            Assert.AreEqual(n, product);
            Assert.IsTrue(((IStructuralEquatable)factors).Equals(expected, EqualityComparer<BigInteger>.Default));
        }

        [TestMethod]
        public void TestPollardLong()
        {
            var expected = new[] { ulong.Parse("400433141"), ulong.Parse("646868797") };
            var n = expected[0] * expected[1];
            var algorithm = new UInt64PollardRhoReduction(new UInt64Reduction());
            var factors = algorithm.Factor(n).OrderBy(factor => factor).ToArray();
            var product = factors.Aggregate((sofar, current) => sofar * current);
            Assert.AreEqual(n, product);
            Assert.IsTrue(((IStructuralEquatable)factors).Equals(expected, EqualityComparer<ulong>.Default));
        }

        [TestMethod]
        public void TestShanksSquareForms()
        {
            var expected = new[] { long.Parse("91739369"), long.Parse("266981831") };
            var n = expected.Aggregate((long)1, (sofar, factor) => sofar * factor);
            var algorithm = new ShanksSquareForms();
            var factors = algorithm.Factor(n).OrderBy(factor => factor).ToArray();
            var product = factors.Aggregate((sofar, current) => sofar * current);
            Assert.AreEqual(n, product);
            Assert.IsTrue(((IStructuralEquatable)factors).Equals(expected, EqualityComparer<long>.Default));
        }

        [TestMethod]
        public void TestQuadraticSieve1()
        {
            var expected = new[] { BigInteger.Parse("274177"), BigInteger.Parse("67280421310721") };
            var n = BigInteger.Parse("18446744073709551617");
            var algorithm = new QuadraticSieve(new QuadraticSieve.Config { Threads = 8 });
            var factors = algorithm.Factor(n).OrderBy(factor => factor).ToArray();
            Assert.IsTrue(factors.Length == 2);
            var product = factors.Aggregate((sofar, current) => sofar * current);
            Assert.AreEqual(n, product);
            Assert.IsTrue(((IStructuralEquatable)factors).Equals(expected, EqualityComparer<BigInteger>.Default));
        }

        [TestMethod]
        public void TestExtendedGreatestCommonDivisor()
        {
            var a = (BigInteger)120;
            var b = (BigInteger)23;
            var cExpected = (BigInteger)(-9);
            var dExpected = (BigInteger)47;
            BigInteger c;
            BigInteger d;
            IntegerMath.ExtendedGreatestCommonDivisor(a, b, out c, out d);
            Assert.AreEqual(cExpected, c);
            Assert.AreEqual(dExpected, d);
        }

        [TestMethod]
        public void TestBigIntegerReduction()
        {
            var p = BigInteger.Parse("10023859281455311421");
            TestReduction(p, new BigIntegerReduction());
        }

        [TestMethod]
        public void TestMutableIntegerIntegerReduction()
        {
            var p = BigInteger.Parse("10023859281455311421");
            TestReduction(p, new MutableIntegerReduction());
        }

        [TestMethod]
        public void TestMontgomeryReduction()
        {
            var p = BigInteger.Parse("10023859281455311421");
            TestReduction(p, new BigIntegerMontgomeryReduction());
        }

        [TestMethod]
        public void TestBarrettReduction()
        {
            var p = BigInteger.Parse("10023859281455311421");
            TestReduction(p, new BarrettReduction());
        }

        [TestMethod]
        public void TestUInt64Reduction()
        {
            var p = ulong.Parse("10023859281455311421");
            TestReduction(p, new UInt64Reduction());
        }

        [TestMethod]
        public void TestUInt32MontgomeryReduction()
        {
            TestReduction(uint.MaxValue, new UInt32MontgomeryReduction());
            TestReduction((uint)1 << 16, new UInt32MontgomeryReduction());
            TestReduction((uint)1 << 8, new UInt32MontgomeryReduction());
        }

        [TestMethod]
        public void TestUInt64MontgomeryReduction()
        {
            TestReduction(ulong.Parse("259027704197601377"), new UInt64MontgomeryReduction());
        }

        private void TestReduction<T>(Integer<T> max, IReductionAlgorithm<T> reduction)
        {
            var random = new RandomInteger<T>(0);
            for (int j = 0; j < 100; j++)
            {
                var p = random.Next(max) | 1;
                var reducer = reduction.GetReducer(p);
                var xBar = reducer.ToResidue(p);
                var yBar = reducer.ToResidue(p);
                var zBar = reducer.ToResidue(p);
                for (int i = 0; i < 100; i++)
                {
                    var x = random.Next(p);
                    var y = random.Next(p);

                    xBar.Set(x);
                    yBar.Set(y);
                    zBar.Set(xBar).Multiply(yBar);
                    Assert.AreEqual((BigInteger)x * y % p, (Integer<T>)zBar.Value);

                    xBar.Set(x);
                    zBar.Set(xBar).Power(y);
                    Assert.AreEqual(BigInteger.ModPow(x, y, p), (Integer<T>)zBar.Value);
                }
            }
        }

        [TestMethod]
        public void TestMutableInteger1()
        {
            var n = BigInteger.Parse("10023859281455311421");
            var store = new MutableIntegerStore(1);
            var a = store.Allocate();
            var b = store.Allocate();
            var x = store.Allocate();
            for (int i = -10; i < 10; i++)
            {
                for (int j = -10; j < 10; j++)
                {
                    a.Set(i);
                    Assert.AreEqual(i, (int)a);
                    Assert.AreEqual((BigInteger)i, (BigInteger)a);
                    Assert.AreEqual(i.GetBitLength(), a.GetBitLength());
                    b.Set(j);
                    Assert.AreEqual(j, (int)b);
                    Assert.AreEqual((BigInteger)j, (BigInteger)b);
                    Assert.AreEqual(j.GetBitLength(), j.GetBitLength());

                    x.Set((BigInteger)i);
                    Assert.AreEqual(i, (int)x);

                    for (int k = 0; k < 10; k++)
                    {
                        x.Set(a).LeftShift(k);
                        Assert.AreEqual(i << k, (int)x);
                        x.Set(a).RightShift(k);
                        Assert.AreEqual(i >> k, (int)x);
                    }

                    x.SetAnd(a, b, store);
                    Assert.AreEqual(i & j, (int)x);
                    x.SetOr(a, b, store);
                    Assert.AreEqual(i | j, (int)x);
                    x.SetExclusiveOr(a, b, store);
                    Assert.AreEqual(i ^ j, (int)x);
                    x.SetNot(a);
                    Assert.AreEqual(~i, (int)x);

                    x.SetSum(a, b);
                    Assert.AreEqual(i + j, (int)x);
                    x.SetDifference(a, b);
                    Assert.AreEqual(i - j, (int)x);
                    x.SetProduct(a, b);
                    Assert.AreEqual(i * j, (int)x);
                    if (j != 0)
                    {
                        x.SetQuotient(a, b, store);
                        Assert.AreEqual(i / j, (int)x);
                        x.SetRemainder(a, b);
                        Assert.AreEqual(i % j, (int)x);
                    }
                    x.Set(a).Negate();
                    Assert.AreEqual(-i, (int)x);
                    x.Set(a).AbsoluteValue();
                    Assert.AreEqual(Math.Abs(i), (int)x);

                    x.Set(a).Increment();
                    Assert.AreEqual(i + 1, (int)x);
                    x.Set(a).Decrement();
                    Assert.AreEqual(i - 1, (int)x);
                    x.SetSum(a, j);
                    Assert.AreEqual(i + j, (int)x);
                    x.SetDifference(a, j);
                    Assert.AreEqual(i - j, (int)x);
                    x.SetProduct(a, j);
                    Assert.AreEqual(i * j, (int)x);
                    x.SetProduct(a, j);
                    Assert.AreEqual(i * j, (int)x);
                    if (j != 0)
                    {
                        x.SetQuotient(a, j, store);
                        Assert.AreEqual(i / j, (int)x);
                        x.SetQuotient(a, j, store);
                        Assert.AreEqual(i / j, (int)x);
                        var remainder = a.GetRemainder(j);
                        Assert.AreEqual(i % j, remainder);
                    }

                    Assert.AreEqual(i == j, a == b);
                    Assert.AreEqual(i != j, a != b);
                    Assert.AreEqual(i < j, a < b);
                    Assert.AreEqual(i <= j, a <= b);
                    Assert.AreEqual(i > j, a > b);
                    Assert.AreEqual(i >= j, a >= b);

                    Assert.AreEqual(i == j, a == j);
                    Assert.AreEqual(i != j, a != j);
                    Assert.AreEqual(i < j, a < j);
                    Assert.AreEqual(i <= j, a <= j);
                    Assert.AreEqual(i > j, a > j);
                    Assert.AreEqual(i >= j, a >= j);

                    Assert.AreEqual(i == j, i == b);
                    Assert.AreEqual(i != j, i != b);
                    Assert.AreEqual(i < j, i < b);
                    Assert.AreEqual(i <= j, i <= b);
                    Assert.AreEqual(i > j, i > b);
                    Assert.AreEqual(i >= j, i >= b);
                }
            }
        }

        [TestMethod]
        public void TestMutableInteger2()
        {
            for (int i = -10; i < 10; i++)
            {
                for (int j = -10; j < 10; j++)
                {
                    Assert.AreEqual((BigInteger)i & j, (int)((MutableInteger)i & j));
                    Assert.AreEqual((BigInteger)i | j, (int)((MutableInteger)i | j));
                    Assert.AreEqual((BigInteger)i ^ j, (int)((MutableInteger)i ^ j));
                    Assert.AreEqual(~(BigInteger)i, (int)(~(MutableInteger)i));

                    Assert.AreEqual((BigInteger)i + j, (int)((MutableInteger)i + j));
                    Assert.AreEqual((BigInteger)i - j, (int)((MutableInteger)i - j));
                    Assert.AreEqual((BigInteger)i * j, (int)((MutableInteger)i * j));
                    if (j != 0)
                    {
                        Assert.AreEqual((BigInteger)i / j, (int)((MutableInteger)i / j));
                        Assert.AreEqual((BigInteger)i % j, (int)((MutableInteger)i % j));
                    }
                    Assert.AreEqual(-(BigInteger)i, (int)(-(MutableInteger)i));
                }
            }
        }

        [TestMethod]
        public void TestMutableInteger3()
        {
            var n = BigInteger.Parse("10023859281455311421");
            var generator = new MersenneTwister(0);
            var random = generator.Create<BigInteger>();
            var smallRandom = generator.Create<uint>();
            var length = (n.GetBitLength() * 2 + 31) / 32 + 3;
            var store = new MutableIntegerStore(1);
            var a = store.Allocate();
            var b = store.Allocate();
            var x = store.Allocate();
            for (int i = 0; i < 1000; i++)
            {
                var aPrime = random.Next(n);
                var bPrime = random.Next(n);
                uint c = smallRandom.Next(0);
                a.Set(aPrime);
                b.Set(bPrime);

                for (int j = 0; j <= 65; j++)
                {
                    x.Set(a).LeftShift(j);
                    Assert.AreEqual(aPrime << j, (BigInteger)x);

                    x.Set(a).RightShift(j);
                    Assert.AreEqual(aPrime >> j, (BigInteger)x);
                }

                x.SetSum(a, b);
                Assert.AreEqual(aPrime + bPrime, (BigInteger)x);

                x.SetSum(a, c);
                Assert.AreEqual(aPrime + c, (BigInteger)x);

                x.SetProduct(a, b);
                Assert.AreEqual(aPrime * bPrime, (BigInteger)x);

                x.SetSquare(a);
                Assert.AreEqual(aPrime * aPrime, (BigInteger)x);

                x.SetProduct(a, c);
                Assert.AreEqual(c * aPrime, (BigInteger)x);

                x.SetQuotient(a, b, store);
                Assert.AreEqual(aPrime / bPrime, (BigInteger)x);

                x.SetRemainder(a, b);
                Assert.AreEqual(aPrime % bPrime, (BigInteger)x);

                x.SetQuotient(a, c, store);
                Assert.AreEqual(aPrime / c, (BigInteger)x);

                x.SetRemainder(a, c);
                Assert.AreEqual(aPrime % c, (BigInteger)x);

                x.SetSquare(a);
                Assert.AreEqual(aPrime * aPrime, (BigInteger)x);

                x.SetDifference(a, b);
                Assert.AreEqual(aPrime - bPrime, (BigInteger)x);

                x.SetGreatestCommonDivisor(a, b, store);
                Assert.AreEqual(BigInteger.GreatestCommonDivisor(aPrime, bPrime), (BigInteger)x);

                if (x.IsOne)
                {
                    x.SetModularInverse(a, b, store);
                    Assert.AreEqual(IntegerMath.ModularInverse(aPrime, bPrime), (BigInteger)x);
                }
            }
        }

        [TestMethod]
        public void ModularSquareRootTest1()
        {
            var random = new MersenneTwister(0).Create<BigInteger>();
            var limit = BigInteger.Parse("10023859281455311421");
            for (int i = 0; i < 100; i++)
            {
                var p = IntegerMath.NextPrime(random.Next(limit));
                var n = random.Next(p);
                if (IntegerMath.JacobiSymbol(n, p) == 1)
                {
                    var r1 = IntegerMath.ModularSquareRoot(n, p);
                    var r2 = p - r1;
                    Assert.AreEqual(n, r1 * r1 % p);
                    Assert.AreEqual(n, r2 * r2 % p);
                }
            }
        }

        [TestMethod]
        public void DivModTest1()
        {
            // Triggers "borrow != 0" special case.
            int length = 20;
            var a = new MutableInteger(length);
            var b = new MutableInteger(length);
            var c = new MutableInteger(length);
            var x = new MutableInteger(length);
            a.Set(BigInteger.Parse("851968723384911158384830467125731460171903460330379450819468842227482878637917031244505597763225"));
            b.Set(BigInteger.Parse("2200761205517100656206929789365760219952611739831"));
            x.SetRemainder(a, b);
            Assert.AreEqual((BigInteger)a % (BigInteger)b, (BigInteger)x);
        }

        [TestMethod]
        public void SieveOfErostothonesTest1()
        {
            var primes = new SieveOfErostothones();
            int iterations = 1000;
            for (int repetition = 1; repetition <= 2; repetition++)
            {
                int n = 0;
                int i = 0;
                foreach (int p in primes)
                {
                    while (n < p)
                    {
                        Assert.IsFalse(IntegerMath.IsPrime(n));
                        ++n;
                    }
                    Assert.IsTrue(IntegerMath.IsPrime(p));
                    n = p + 1;
                    if (++i >= iterations)
                        break;
                }
                Assert.AreEqual(i, iterations);
            }
        }

        [TestMethod]
        public void TrialDivisionTest1()
        {
            var primalityAlgorithm = new TrialDivisionPrimality();
            var factorizationAlgorithm = new TrialDivisionFactorization();
            for (int n = 2; n < 10000; n++)
            {
                var factors = factorizationAlgorithm.Factor(n).ToArray();
                var product = factors.Aggregate((sofar, current) => sofar * current);
                Assert.AreEqual(n, product);
                Assert.IsTrue(factors.All(factor => IntegerMath.IsPrime(factor)));
                Assert.AreEqual(IntegerMath.IsPrime((BigInteger)n), primalityAlgorithm.IsPrime(n));
            }
        }

        const string matrix1 = @"
            1111111
            0000000
            0000011
            0011000
            ";

        const string matrix2 = @"
            11111111111111111000000000000000000000000000
            00000000000000000000000000000000000000000000
            00000111011100011001000010111100011100001110
            00110000010110101010011001110001101101000001
            00101001000000100000001100000100010000011100
            00000100100010000000100000000001000000101100
            01000000101000110000000000000010101011000010
            01010000000001000011000001100100001000010010
            00001000110000010010010010000001001000100010
            00000001100001000000101000000110000000100000
            10001000000000000000000000100000000001000000
            10100100000001000000000000001010000000000000
            00100001101000000100000000000010000000000000
            01010001001100001100000100000000100000000000
            00001000011100000001000100000000000000000000
            00000000000000000000000101000000000100001000
            00000100000000100000000001000000000000000001
            10000000000000000000000000000000000000000000
            10000000000000001001000000001000000100000000
            00000000000010010010000000000000000000000000
            01000001000011000000001000000000000000000000
            00010000000010000000000000000000000000000000
            00000010000000000000000000000000000000000010
            00000100000000001000000000000000000001000000
            00000000000000100000000000000000000100000000
            00000010000000000000000000100000000000010000
            00100010000000000000000000000000010000000000
            00010000000000000000000000010000000000100000
            00000000000000000000000000010000000000000000
            00000000010000000000000000001000000000000000
            00000000000000000000000000000001000000000000
            00000000000000010000000000000000000000001000
            00000000000000000000000000000000000000000000
            00000000000100000000100000000000100000000000
            ";

        [TestMethod]
        public void GaussianEliminationTest()
        {
            var solver = new GaussianElimination<BoolBitArray>(1);
            foreach (var text in new[] { matrix1, matrix2 })
            {
                var origMatrix = GetBitMatrix(GetLines(text));
                var matrix = new BoolBitMatrix(origMatrix);
                foreach (var v in solver.Solve(matrix))
                {
                    Assert.IsTrue(GaussianElimination<BoolBitArray>.IsSolutionValid(origMatrix, v));
                    Assert.IsTrue(GaussianElimination<BoolBitArray>.IsSolutionValid(matrix, v));
                }
                Console.WriteLine();
            }
        }

        [TestMethod]
        public void StructuredGaussianEliminationTest()
        {
            var solver = new StructuredGaussianElimination<BoolBitArray, BoolBitMatrix>(1, 0, false);
            foreach (var text in new[] { matrix1, matrix2 })
            {
                var origMatrix = GetBitMatrix(GetLines(text));
                var matrix = new BoolBitMatrix(origMatrix);
                foreach (var v in solver.Solve(matrix))
                {
                    Assert.IsTrue(GaussianElimination<BoolBitArray>.IsSolutionValid(origMatrix, v));
                    Assert.IsTrue(GaussianElimination<BoolBitArray>.IsSolutionValid(matrix, v));
                }
                Console.WriteLine();
            }
        }

        private string[] GetLines(string text)
        {
            return text.Split('\n').Select(row => row.Trim()).Where(row => row != "").ToArray();
        }

        private IBitMatrix GetBitMatrix(string[] lines)
        {
            int rows = lines.Length;
            int cols = lines[0].Length;
            var matrix = new BoolBitMatrix(rows, cols);
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                for (int j = 0; j < line.Length; j++)
                    matrix[i, j] = line[j] == '1';
            }
            return matrix;
        }

        [TestMethod]
        public void BitLengthTest()
        {
            for (int i = 0; i < 256; i++)
            {
                ulong value = (ulong)i;
                int count = 0;
                while (value != 0)
                {
                    value >>= 1;
                    ++count;
                }
                Assert.AreEqual(count, i.GetBitLength());
                count += 0;
            }
        }

        [TestMethod]
        public void BitCountTest()
        {
            for (int i = 0; i < 256; i++)
            {
                ulong value = (ulong)i;
                int count = 0;
                while (value != 0)
                {
                    if ((value & 1) != 0)
                        ++count;
                    value >>= 1;
                }
                Assert.AreEqual(count, i.GetBitCount());
                count += 0;
            }
        }

        [TestMethod]
        public void ModularInverseTest()
        {
            ModularInverseTest(int.MaxValue);
            ModularInverseTest(uint.MaxValue);
            ModularInverseTest(long.MaxValue);
            ModularInverseTest(ulong.MaxValue);
            ModularInverseTest(BigInteger.Parse("10023859281455311421"));
        }

        public void ModularInverseTest<T>(T max)
        {
            var random = new RandomInteger<T>(0);
            for (int i = 0; i < 1000; i++)
            {
                var q = random.Next(max);
                var p = random.Next(q);
                while (!Integer<T>.GreatestCommonDivisor(p, q).IsOne)
                {
                    q = random.Next(max);
                    p = random.Next(q);
                }
                var r = Integer<T>.FloorRoot(random.Next(q), 2);
                r -= r % p;
                var pInv = Integer<T>.ModularInverse(p, q);
                var result = Integer<T>.ModularProduct(p, pInv, q);
                Assert.AreEqual(Integer<T>.One, result);
                Assert.AreEqual(r / p % q, Integer<T>.ModularProduct(r, pInv, q));
            }
        }

        [TestMethod]
        public void UInt128Test()
        {
            UInt128Test((ulong)1 << 20, (ulong)1 << 20);
            UInt128Test((ulong)1 << 40, (ulong)1 << 20);
            UInt128Test((ulong)1 << 60, (ulong)1 << 20);

            UInt128Test((ulong)1 << 20, (ulong)1 << 40);
            UInt128Test((ulong)1 << 40, (ulong)1 << 40);
            UInt128Test((ulong)1 << 60, (ulong)1 << 40);

            UInt128Test((ulong)1 << 20, (ulong)1 << 60);
            UInt128Test((ulong)1 << 40, (ulong)1 << 60);
            UInt128Test((ulong)1 << 60, (ulong)1 << 60);

            UInt128Test(ulong.MaxValue, ulong.MaxValue);
        }

        private void UInt128Test(ulong factorMax, ulong modulusMax)
        {
            var random = new MersenneTwister(0).Create<ulong>();
            for (int i = 0; i < 10000; i++)
            {
                var n = random.Next(modulusMax - 1) + 1;
                var a = random.Next(factorMax) % n;
                var b = random.Next(factorMax) % n;
                var s = (int)(b % 32);
                Assert.AreEqual((BigInteger)a << s, (UInt128)a << s);
                Assert.AreEqual((BigInteger)a >> s, (UInt128)a >> s);
                Assert.AreEqual((BigInteger)a & b, (UInt128)a & b);
                Assert.AreEqual((BigInteger)a | b, (UInt128)a | b);
                Assert.AreEqual((BigInteger)a ^ b, (UInt128)a ^ b);
                if (a <= long.MaxValue)
                    Assert.AreEqual(~(BigInteger)a, (long)~(UInt128)a);
                Assert.AreEqual((BigInteger)a + b, (UInt128)a + b);
                if (b < a)
                    Assert.AreEqual((BigInteger)a - b, (UInt128)a - b);
                Assert.AreEqual((BigInteger)a * b, (UInt128)a * b);
                Assert.AreEqual((BigInteger)a * b % n, (UInt128)a * b % n);
                Assert.AreEqual((BigInteger)a * b / n, (UInt128)a * b / n);
                Assert.AreEqual(((BigInteger)a + b) % n, UInt128.ModularSum(a, b, n));
                Assert.AreEqual((((BigInteger)a - b) % n + n) % n, UInt128.ModularDifference(a, b, n));
                Assert.AreEqual((BigInteger)a * b % n, UInt128.ModularProduct(a, b, n));
            }
            for (int i = 0; i < 1000; i++)
            {
                var n = random.Next(modulusMax - 1) + 1;
                var a = random.Next(factorMax) % n;
                var b = random.Next(factorMax) % n;
                Assert.AreEqual(BigInteger.ModPow(a, b, n), UInt128.ModularPower(a, b, n));
            }
        }

        [TestMethod]
        public void ModularPowerOfTwo()
        {
            var random = new MersenneTwister(0).Create<ulong>();
            var modulusMax = ulong.MaxValue;
            for (int i = 0; i < 10000; i++)
            {
                var modulus = random.Next(modulusMax - 1);
                var exponent = random.Next(modulus);
                Assert.AreEqual(BigInteger.ModPow(2, exponent, modulus), IntegerMath.ModularPowerOfTwo(exponent, modulus));
            }
        }

        [TestMethod]
        public void FloorRootTest1()
        {
            var max = IntegerMath.Power(BigIntegers.Two, 3 * 64);
            var random = new MersenneTwister(0).Create<BigInteger>();
            for (var i = 0; i < 100000; i++)
            {
                var x = random.Next(max);
                var n = (int)random.Next(20 - 2) + 2;
                var root = IntegerMath.FloorRoot(x, n);
                Assert.IsTrue(IntegerMath.Power(root, n) <= x && IntegerMath.Power(root + 1, n) > x);
            }
        }
    }
}
