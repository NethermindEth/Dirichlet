﻿using System;
using System.Numerics;

namespace Decompose.Numerics
{
    public struct Integer<T> : IComparable<Integer<T>>, IEquatable<Integer<T>>
    {
        private static IOperations<T> ops;
        private static Integer<T> zero;
        private static Integer<T> one;
        private static Integer<T> two;

        static Integer()
        {
            ops = Operations.Get<T>();
            zero = new Integer<T>(ops.Zero);
            one = new Integer<T>(ops.One);
            two = new Integer<T>(ops.Two);
        }

        private T value;
        public Integer(T value) { this.value = value; }
        public T Value { get { return value; } }
        public static Integer<T> Zero { get { return zero; } }
        public static Integer<T> One { get { return one; } }
        public static Integer<T> Two { get { return two; } }
        public bool IsZero { get { return ops.IsZero(value); } }
        public bool IsOne { get { return ops.IsOne(value); } }
        public bool IsEven { get { return ops.IsEven(value); } }
        public static implicit operator Integer<T>(int value) { return new Integer<T>(ops.Convert(value)); }
        public static implicit operator Integer<T>(T value) { return new Integer<T>(value); }
        public static implicit operator T(Integer<T> integer) { return integer.value; }
        public static implicit operator BigInteger(Integer<T> integer) { return ops.ToBigInteger(integer.value); }
        public static Integer<T> operator +(Integer<T> a, Integer<T> b) { return new Integer<T>(ops.Add(a.value, b.value)); }
        public static Integer<T> operator -(Integer<T> a, Integer<T> b) { return new Integer<T>(ops.Subtract(a.value, b.value)); }
        public static Integer<T> operator *(Integer<T> a, Integer<T> b) { return new Integer<T>(ops.Multiply(a.value, b.value)); }
        public static Integer<T> operator /(Integer<T> a, Integer<T> b) { return new Integer<T>(ops.Divide(a.value, b.value)); }
        public static Integer<T> operator %(Integer<T> a, Integer<T> b) { return new Integer<T>(ops.Modulus(a.value, b.value)); }
        public static Integer<T> operator -(Integer<T> a) { return new Integer<T>(ops.Negate(a.value)); }
        public static Integer<T> operator ++(Integer<T> a) { return new Integer<T>(ops.Add(a.value, one.value)); }
        public static Integer<T> operator --(Integer<T> a) { return new Integer<T>(ops.Subtract(a.value, one.value)); }
        public static Integer<T> operator <<(Integer<T> a, int b) { return new Integer<T>(ops.LeftShift(a.value, b)); }
        public static Integer<T> operator >>(Integer<T> a, int b) { return new Integer<T>(ops.RightShift(a.value, b)); }
        public static Integer<T> operator &(Integer<T> a, Integer<T> b) { return new Integer<T>(ops.And(a.value, b.value)); }
        public static Integer<T> operator |(Integer<T> a, Integer<T> b) { return new Integer<T>(ops.Or(a.value, b.value)); }
        public static Integer<T> operator ^(Integer<T> a, Integer<T> b) { return new Integer<T>(ops.ExclusiveOr(a.value, b.value)); }
        public static Integer<T> operator ~(Integer<T> a) { return new Integer<T>(ops.Not(a.value)); }
        public static bool operator ==(Integer<T> a, Integer<T> b) { return ops.Equals(a.value, b.value); }
        public static bool operator !=(Integer<T> a, Integer<T> b) { return !ops.Equals(a.value, b.value); }
        public static bool operator <(Integer<T> a, Integer<T> b) { return ops.Compare(a.value, b.value) < 0; }
        public static bool operator <=(Integer<T> a, Integer<T> b) { return ops.Compare(a.value, b.value) <= 0; }
        public static bool operator >(Integer<T> a, Integer<T> b) { return ops.Compare(a.value, b.value) > 0; }
        public static bool operator >=(Integer<T> a, Integer<T> b) { return ops.Compare(a.value, b.value) >= 0; }
        public static Integer<T> SquareRoot(Integer<T> a) { return new Integer<T>(ops.SquareRoot(a.value)); }
        public static Integer<T> GreatestCommonDivisor(Integer<T> a, Integer<T> b) { return new Integer<T>(ops.GreatestCommonDivisor(a.value, b.value)); }
        public static Integer<T> ModularSum(Integer<T> a, Integer<T> b, Integer<T> modulus) { return new Integer<T>(ops.ModularSum(a.value, b.value, modulus.value)); }
        public static Integer<T> ModularDifference(Integer<T> a, Integer<T> b, Integer<T> modulus) { return new Integer<T>(ops.ModularDifference(a.value, b.value, modulus.value)); }
        public static Integer<T> ModularProduct(Integer<T> a, Integer<T> b, Integer<T> modulus) { return new Integer<T>(ops.ModularProduct(a.value, b.value, modulus.value)); }
        public static Integer<T> ModularPower(Integer<T> a, Integer<T> exponent, Integer<T> modulus) { return new Integer<T>(ops.ModularPower(a.value, exponent.value, modulus.value)); }
        public static Integer<T> ModularInverse(Integer<T> a, Integer<T> modulus) { return new Integer<T>(ops.ModularInverse(a.value, modulus.value)); }
        public int CompareTo(Integer<T> other) { return ops.Compare(value, other.value); }
        public bool Equals(Integer<T> other) { return ops.Equals(value, other.value); }
        public override string ToString() { return value.ToString(); }
        public override bool Equals(object obj) { return obj is Integer<T> && ops.Equals(value, ((Integer<T>)obj).value); }
        public override int GetHashCode() { return value.GetHashCode(); }
    }
}