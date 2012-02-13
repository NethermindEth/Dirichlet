﻿using System;
using System.Linq;
using System.Numerics;

namespace Decompose.Scripting
{
    public static class Cast
    {
        public static BigInteger ToBigInteger(object value)
        {
            if (value is int)
                return (BigInteger)(int)value;
            if (value is uint)
                return (BigInteger)(uint)value;
            if (value is long)
                return (BigInteger)(long)value;
            if (value is ulong)
                return (BigInteger)(ulong)value;
            if (value is BigInteger)
                return (BigInteger)(BigInteger)value;
            if (value is double)
                return (BigInteger)(double)value;
            throw new NotImplementedException();
        }

        public static int ToInt32(object value)
        {
            if (value is int)
                return (int)(int)value;
            if (value is uint)
                return (int)(uint)value;
            if (value is long)
                return (int)(long)value;
            if (value is ulong)
                return (int)(ulong)value;
            if (value is BigInteger)
                return (int)(BigInteger)value;
            if (value is double)
                return (int)(double)value;
            throw new NotImplementedException();
        }

        public static uint ToUInt32(object value)
        {
            if (value is int)
                return (uint)(int)value;
            if (value is uint)
                return (uint)(uint)value;
            if (value is long)
                return (uint)(long)value;
            if (value is ulong)
                return (uint)(ulong)value;
            if (value is BigInteger)
                return (uint)(BigInteger)value;
            if (value is double)
                return (uint)(double)value;
            throw new NotImplementedException();
        }

        public static long ToInt64(object value)
        {
            if (value is int)
                return (long)(int)value;
            if (value is uint)
                return (long)(uint)value;
            if (value is long)
                return (long)(long)value;
            if (value is ulong)
                return (long)(ulong)value;
            if (value is BigInteger)
                return (long)(BigInteger)value;
            if (value is double)
                return (long)(double)value;
            throw new NotImplementedException();
        }

        public static ulong ToUInt64(object value)
        {
            if (value is int)
                return (ulong)(int)value;
            if (value is uint)
                return (ulong)(uint)value;
            if (value is long)
                return (ulong)(long)value;
            if (value is ulong)
                return (ulong)(ulong)value;
            if (value is BigInteger)
                return (ulong)(BigInteger)value;
            if (value is double)
                return (ulong)(double)value;
            throw new NotImplementedException();
        }

        public static double ToDouble(object value)
        {
            if (value is int)
                return (double)(int)value;
            if (value is uint)
                return (double)(uint)value;
            if (value is long)
                return (double)(long)value;
            if (value is ulong)
                return (double)(ulong)value;
            if (value is BigInteger)
                return (double)(BigInteger)value;
            if (value is double)
                return (double)(double)value;
            throw new NotImplementedException();
        }

        public static object[] ConvertToCompatibleTypes(object[] args)
        {
            var types = args.Select(arg => arg.GetType()).ToArray();
            if (types.Any(type => type == typeof(double)))
                return args.Select(arg => Cast.ToDouble(arg)).Cast<object>().ToArray();
            if (types.Any(type => type == typeof(BigInteger)))
                return args.Select(arg => Cast.ToBigInteger(arg)).Cast<object>().ToArray();
            if (types.Any(type => type == typeof(ulong)))
                return args.Select(arg => Cast.ToUInt64(arg)).Cast<object>().ToArray();
            if (types.Any(type => type == typeof(long)))
                return args.Select(arg => Cast.ToInt64(arg)).Cast<object>().ToArray();
            if (types.Any(type => type == typeof(uint)))
                return args.Select(arg => Cast.ToUInt32(arg)).Cast<object>().ToArray();
            if (types.Any(type => type == typeof(int)))
                return args.Select(arg => Cast.ToInt32(arg)).Cast<object>().ToArray();
            throw new NotImplementedException();
        }

        public static T[] ToArray<T>(object[] args)
        {
            var array = new T[args.Length];
            for (int i = 0; i < args.Length; i++)
                array[i] = (T)args[i];
            return array;
        }
    }
}
