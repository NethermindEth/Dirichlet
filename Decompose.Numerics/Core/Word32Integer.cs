﻿using System;
using System.Linq;
using System.Numerics;
using System.Diagnostics;

namespace Decompose.Numerics
{
    public class Word32Integer : IComparable<Word32Integer>, IEquatable<Word32Integer>
    {
        private const int wordLength = 32;

        private uint[] bits;
        private int last;
        private int sign;

        public uint[] Bits { get { return bits; } }
        public int Length { get { return bits.Length; } }
        public int Last { get { return last; } set { last = value; } }

        public int GetBitLength()
        {
            CheckValid();
            if (last == 0 && bits[0] == 0)
                return 0;
            var b = bits[last];
            return 32 * last + bits[last].GetBitLength();
        }

        public int Sign
        {
            get { return last == 0 && bits[0] == 0 ? 0 : sign; }
            set { sign = value; }
        }

        public bool IsZero
        {
            get { return last == 0 && bits[0] == 0; }
        }

        public bool IsOne
        {
            get { return last == 0 && bits[0] == 1 && sign == 1; }
        }

        public bool IsEven
        {
            get { return (bits[0] & 1) == 0; }
        }

        public uint LeastSignificantWord
        {
            get { return bits[0]; }
        }

        public static int WordLength
        {
            get { return wordLength; }
        }

        public Word32Integer(int length)
            : this(new uint[length], 1)
        {
        }

        private Word32Integer(uint[] bits, int sign)
        {
            Debug.Assert(bits != null && bits.Length > 0);
            Debug.Assert(sign == 1 || sign == -1);
            this.bits = bits;
            this.sign = sign;
            SetLast(Length - 1);
        }

        private void CheckLast(int newLast)
        {
            if (bits.Length < newLast + 1)
                Resize(newLast + 1);
        }

        public void Resize(int length)
        {
            var newBits = new uint[length];
            bits.CopyTo(newBits, 0);
            bits = newBits;
        }

        public Word32Integer Clear()
        {
            CheckValid();
            for (int i = 0; i <= last; i++)
                bits[i] = 0;
            last = 0;
            CheckValid();
            return this;
        }

        public Word32Integer Set(int a)
        {
            CheckValid();
            bits[0] = (uint)(a < 0 ? -a : a);
            for (int i = 1; i <= last; i++)
                bits[i] = 0;
            last = 0;
            sign = a < 0 ? -1 : 1;
            CheckValid();
            return this;
        }

        public Word32Integer Set(uint a)
        {
            CheckValid();
            bits[0] = a;
            for (int i = 1; i <= last; i++)
                bits[i] = 0;
            last = 0;
            sign = 1;
            CheckValid();
            return this;
        }

        public Word32Integer Set(long a)
        {
            CheckValid();
            var aAbs = a < 0 ? -a : a;
            bits[0] = (uint)aAbs;
            bits[1] = (uint)(aAbs >> 32);
            for (int i = 2; i <= last; i++)
                bits[i] = 0;
            last = bits[1] != 0 ? 1 : 0;
            sign = a < 0 ? -1 : 1;
            CheckValid();
            return this;
        }

        public Word32Integer Set(ulong a)
        {
            CheckValid();
            bits[0] = (uint)a;
            bits[1] = (uint)(a >> 32);
            for (int i = 2; i <= last; i++)
                bits[i] = 0;
            last = bits[1] != 0 ? 1 : 0;
            sign = 1;
            CheckValid();
            return this;
        }

        public Word32Integer Set(BigInteger a)
        {
            CheckValid();
            var asign = a.Sign == -1 ? -1 : 1;
            a = BigInteger.Abs(a);
            var nBits = GetBits(a);
            CheckLast(nBits.Length - 1);
            nBits.CopyTo(bits, 0);
            for (int i = nBits.Length; i <= last; i++)
                bits[i] = 0;
            last = Math.Max(nBits.Length - 1, 0);
            sign = asign;
            CheckValid();
            return this;
        }

        public Word32Integer Set(Word32Integer a)
        {
            CheckValid();
            CheckLast(a.last);
            Debug.Assert(!object.ReferenceEquals(this, a));
            var abits = a.bits;
            int alast = a.last;
            for (int i = 0; i <= alast; i++)
                bits[i] = abits[i];
            for (int i = alast + 1; i <= last; i++)
                bits[i] = 0;
            last = alast;
            sign = a.sign;
            CheckValid();
            return this;
        }

#if false
        public Word32Integer SetMasked(Word32Integer a, int n)
        {
            CheckValid();
            Debug.Assert(Length == a.Length);
            Debug.Assert(n % 32 == 0 && n > 0);
            int clast = (n + 31) / 32 - 1;
            int alast = Math.Min(a.last, clast);
            for (int i = 0; i <= alast; i++)
                bits[i] = a.bits[i];
            for (int i = alast + 1; i <= last; i++)
                bits[i] = 0;
            return SetLast(alast);
        }
#endif

        public Word32Integer Copy()
        {
            CheckValid();
            var newBits = new uint[bits.Length];
            Array.Copy(bits, 0, newBits, 0, last + 1);
            return new Word32Integer(newBits, sign);
        }

        public static implicit operator Word32Integer(int a)
        {
            return new Word32Integer(1).Set(a);
        }

        public static implicit operator Word32Integer(uint a)
        {
            return new Word32Integer(1).Set(a);
        }

        public static implicit operator Word32Integer(long a)
        {
            return new Word32Integer(2).Set(a);
        }

        public static implicit operator Word32Integer(ulong a)
        {
            return new Word32Integer(2).Set(a);
        }

        public static implicit operator Word32Integer(BigInteger a)
        {
            return new Word32Integer(4).Set(a);
        }

        public static explicit operator int(Word32Integer a)
        {
            CheckValid(a);
            Debug.Assert(a.last == 0);
            return a.sign == -1 ? -(int)a.bits[0] : (int)a.bits[0];
        }

        public static explicit operator uint(Word32Integer a)
        {
            CheckValid(a);
            Debug.Assert(a.last == 0);
            return a.bits[0];
        }

        public static explicit operator ulong(Word32Integer a)
        {
            CheckValid(a);
            Debug.Assert(a.last < 2);
            return a.last == 0 ? a.bits[0] : (ulong)a.bits[1] << 32 | a.bits[0];
        }

        public static implicit operator BigInteger(Word32Integer a)
        {
            CheckValid(a);
            var bytes = new byte[(a.last + 1) * 4 + 1];
            for (int i = 0; i <= a.last; i++)
                BitConverter.GetBytes(a.bits[i]).CopyTo(bytes, i * 4);
            if (a.sign == -1)
            {
                int carry = 1;
                for (int i = 0; i < bytes.Length; i++)
                {
                    carry += (byte)~bytes[i];
                    bytes[i] = (byte)carry;
                    carry >>= 8;
                }
            }
            return new BigInteger(bytes);
        }

        public override string ToString()
        {
            return ((BigInteger)this).ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj is Word32Integer)
                return CompareTo((Word32Integer)obj) == 0;
            if (obj is int)
                return CompareTo((int)obj) == 0;
            if (obj is uint)
                return CompareTo((uint)obj) == 0;
            if (obj is long)
                return CompareTo((long)obj) == 0;
            if (obj is ulong)
                return CompareTo((ulong)obj) == 0;
            if (obj is BigInteger)
                return CompareTo((BigInteger)obj) == 0;
            return false;
        }

        public bool Equals(Word32Integer other)
        {
            if ((object)other == null)
                return false;
            return CompareTo(other) == 0;
        }

        public static bool operator ==(Word32Integer a, Word32Integer b)
        {
            if ((object)a == (object)b)
                return true;
            if ((object)a == null || (object)b == null)
                return false;
            return a.CompareTo(b) == 0;
        }

        public static bool operator ==(Word32Integer a, int b)
        {
            if ((object)a == null)
                return false;
            return a.CompareTo(b) == 0;
        }

        public static bool operator ==(Word32Integer a, uint b)
        {
            if ((object)a == null)
                return false;
            return a.CompareTo(b) == 0;
        }

        public static bool operator ==(Word32Integer a, long b)
        {
            if ((object)a == null)
                return false;
            return a.CompareTo(b) == 0;
        }

        public static bool operator ==(Word32Integer a, ulong b)
        {
            if ((object)a == null)
                return false;
            return a.CompareTo(b) == 0;
        }

        public static bool operator ==(int a, Word32Integer b)
        {
            if ((object)b == null)
                return false;
            return b.CompareTo(a) == 0;
        }

        public static bool operator ==(uint a, Word32Integer b)
        {
            if ((object)b == null)
                return false;
            return b.CompareTo(a) == 0;
        }

        public static bool operator ==(long a, Word32Integer b)
        {
            if ((object)b == null)
                return false;
            return b.CompareTo(a) == 0;
        }

        public static bool operator ==(ulong a, Word32Integer b)
        {
            if ((object)b == null)
                return false;
            return b.CompareTo(a) == 0;
        }

        public static bool operator !=(Word32Integer a, Word32Integer b)
        {
            return !(a == b);
        }

        public static bool operator !=(Word32Integer a, int b)
        {
            return !(a == b);
        }

        public static bool operator !=(Word32Integer a, uint b)
        {
            return !(a == b);
        }

        public static bool operator !=(Word32Integer a, ulong b)
        {
            return !(a == b);
        }

        public static bool operator !=(Word32Integer a, long b)
        {
            return !(a == b);
        }

        public static bool operator !=(int a, Word32Integer b)
        {
            return !(a == b);
        }

        public static bool operator !=(uint a, Word32Integer b)
        {
            return !(a == b);
        }

        public static bool operator !=(long a, Word32Integer b)
        {
            return !(a == b);
        }

        public static bool operator !=(ulong a, Word32Integer b)
        {
            return !(a == b);
        }

        public static bool operator <(Word32Integer a, Word32Integer b)
        {
            return a.CompareTo(b) < 0;
        }

        public static bool operator <(Word32Integer a, int b)
        {
            return a.CompareTo(b) < 0;
        }

        public static bool operator <(Word32Integer a, uint b)
        {
            return a.CompareTo(b) < 0;
        }

        public static bool operator <(Word32Integer a, long b)
        {
            return a.CompareTo(b) < 0;
        }

        public static bool operator <(Word32Integer a, ulong b)
        {
            return a.CompareTo(b) < 0;
        }

        public static bool operator <(int a, Word32Integer b)
        {
            return b.CompareTo(a) > 0;
        }

        public static bool operator <(uint a, Word32Integer b)
        {
            return b.CompareTo(a) > 0;
        }

        public static bool operator <(long a, Word32Integer b)
        {
            return b.CompareTo(a) > 0;
        }

        public static bool operator <(ulong a, Word32Integer b)
        {
            return b.CompareTo(a) > 0;
        }

        public static bool operator <=(Word32Integer a, Word32Integer b)
        {
            return a.CompareTo(b) <= 0;
        }

        public static bool operator <=(Word32Integer a, int b)
        {
            return a.CompareTo(b) <= 0;
        }

        public static bool operator <=(Word32Integer a, uint b)
        {
            return a.CompareTo(b) <= 0;
        }

        public static bool operator <=(Word32Integer a, long b)
        {
            return a.CompareTo(b) <= 0;
        }

        public static bool operator <=(Word32Integer a, ulong b)
        {
            return a.CompareTo(b) <= 0;
        }

        public static bool operator <=(int a, Word32Integer b)
        {
            return b.CompareTo(a) >= 0;
        }

        public static bool operator <=(uint a, Word32Integer b)
        {
            return b.CompareTo(a) >= 0;
        }

        public static bool operator <=(long a, Word32Integer b)
        {
            return b.CompareTo(a) >= 0;
        }

        public static bool operator <=(ulong a, Word32Integer b)
        {
            return b.CompareTo(a) >= 0;
        }

        public static bool operator >(Word32Integer a, Word32Integer b)
        {
            return a.CompareTo(b) > 0;
        }

        public static bool operator >(Word32Integer a, int b)
        {
            return a.CompareTo(b) > 0;
        }

        public static bool operator >(Word32Integer a, uint b)
        {
            return a.CompareTo(b) > 0;
        }

        public static bool operator >(Word32Integer a, long b)
        {
            return a.CompareTo(b) > 0;
        }

        public static bool operator >(Word32Integer a, ulong b)
        {
            return a.CompareTo(b) > 0;
        }

        public static bool operator >(int a, Word32Integer b)
        {
            return b.CompareTo(a) < 0;
        }

        public static bool operator >(uint a, Word32Integer b)
        {
            return b.CompareTo(a) < 0;
        }

        public static bool operator >(long a, Word32Integer b)
        {
            return b.CompareTo(a) < 0;
        }

        public static bool operator >(ulong a, Word32Integer b)
        {
            return b.CompareTo(a) < 0;
        }

        public static bool operator >=(Word32Integer a, Word32Integer b)
        {
            return a.CompareTo(b) >= 0;
        }

        public static bool operator >=(Word32Integer a, int b)
        {
            return a.CompareTo(b) >= 0;
        }

        public static bool operator >=(Word32Integer a, uint b)
        {
            return a.CompareTo(b) >= 0;
        }

        public static bool operator >=(Word32Integer a, long b)
        {
            return a.CompareTo(b) >= 0;
        }

        public static bool operator >=(Word32Integer a, ulong b)
        {
            return a.CompareTo(b) >= 0;
        }

        public static bool operator >=(int a, Word32Integer b)
        {
            return b.CompareTo(a) <= 0;
        }

        public static bool operator >=(uint a, Word32Integer b)
        {
            return b.CompareTo(a) <= 0;
        }

        public static bool operator >=(long a, Word32Integer b)
        {
            return b.CompareTo(a) <= 0;
        }

        public static bool operator >=(ulong a, Word32Integer b)
        {
            return b.CompareTo(a) <= 0;
        }

        public static Word32Integer operator +(Word32Integer a, Word32Integer b)
        {
            return new Word32Integer(Math.Max(a.last, b.last) + 2).SetSum(a, b);
        }

        public static Word32Integer operator -(Word32Integer a, Word32Integer b)
        {
            return new Word32Integer(Math.Max(a.last, b.last) + 2).SetDifference(a, b);
        }

        public static Word32Integer operator *(Word32Integer a, Word32Integer b)
        {
            return new Word32Integer(a.last + b.last + 1).SetProduct(a, b);
        }

        public static Word32Integer operator /(Word32Integer a, Word32Integer b)
        {
            return new Word32Integer(a.last + 1).SetQuotient(a, b, new Word32Integer(a.last + 1));
        }

        public static Word32Integer operator %(Word32Integer a, Word32Integer b)
        {
            return new Word32Integer(a.last + b.last + 1).SetRemainder(a, b);
        }

        public override int GetHashCode()
        {
            if (IsZero)
                return 0;
            int hash = 0;
            for (int i = 0; i <= last; i++)
                hash ^= (int)bits[i];
            return hash ^ sign;
        }

        public int CompareTo(Word32Integer other)
        {
            if (sign != other.sign)
                return IsZero && other.IsZero ? 0 : sign;
            var result = UnsignedCompareTo(other);
            return sign == -1 ? -result : result;
        }

        public int UnsignedCompareTo(Word32Integer other)
        {
            CheckValid();
            var diff = last - other.last;
            if (diff != 0)
                return diff;
            var obits = other.bits;
            for (int i = last; i >= 0; i--)
            {
                uint wi = bits[i];
                uint oi = obits[i];
                if (wi < oi)
                    return -1;
                if (wi > oi)
                    return 1;
            }
            return 0;
        }

        public int CompareTo(int other)
        {
            CheckValid();
            if (last > 0)
                return 1;
            if (sign != (other < 0 ? -1 : 1))
            {
                if (IsZero && other == 0)
                    return 0;
                return sign;
            }
            var result = bits[0].CompareTo((uint)Math.Abs(other));
            return sign == -1 ? -result : result;
        }

        public int CompareTo(uint other)
        {
            CheckValid();
            if (sign == -1)
            {
                if (IsZero && other == 0)
                    return 0;
                return -1;
            }
            if (last > 0)
                return 1;
            return bits[0].CompareTo(other);
        }

        public int UnsignedCompareTo(uint other)
        {
            CheckValid();
            if (last > 0)
                return 1;
            return bits[0].CompareTo(other);
        }

        public int CompareTo(long other)
        {
            CheckValid();
            if (last > 1)
                return 1;
            if (sign != (other < 0 ? -1 : 1))
            {
                if (IsZero && other == 0)
                    return 0;
                return sign;
            }
            var result = ((ulong)bits[1] << 32 | bits[0]).CompareTo(other);
            return sign == -1 ? -result : result;
        }

        public int CompareTo(ulong other)
        {
            CheckValid();
            if (sign == -1)
            {
                if (IsZero && other == 0)
                    return 0;
                return -1;
            }
            if (last > 1)
                return 1;
            return ((ulong)bits[1] << 32 | bits[0]).CompareTo(other);
        }

        public int UnsignedCompareTo(ulong other)
        {
            CheckValid();
            if (last > 1)
                return 1;
            return ((ulong)bits[1] << 32 | bits[0]).CompareTo(other);
        }

        public Word32Integer Mask(int n)
        {
            CheckValid();
            int i = n / 32;
            int j = n - 32 * i;
            if (j == 0)
            {
                for (int k = last; k >= i; k--)
                    bits[k] = 0;
                if (i > 0)
                    --i;
            }
            else
            {
                for (int k = last; k > i; k--)
                    bits[k] = 0;
                bits[i] &= (1u << j) - 1;
            }
            while (i > 0 && bits[i] == 0)
                --i;
            last = i;
            CheckValid();
            return this;
        }

        public Word32Integer LeftShift(int n)
        {
            CheckValid();
            if (n == 0)
                return this;
            int i = n / 32;
            int j = n - 32 * i;
            CheckLast(last + i + 1);
            if (j == 0)
            {
                for (int k = last; k >= 0; k--)
                    bits[k + i] = bits[k];
                for (int k = 0; k < i; k++)
                    bits[k] = 0;
                last += i;
                CheckValid();
                return this;
            }
            else
            {
                int jneg = 32 - j;
                bits[last + i + 1] = bits[last] >> jneg;
                for (int k = last - 1; k >= 0; k--)
                    bits[k + i + 1] = bits[k + 1] << j | bits[k] >> jneg;
                bits[i] = bits[0] << j;
                for (int k = 0; k < i; k++)
                    bits[k] = 0;
                return SetLast(last + i + 1);
            }
        }

        public Word32Integer RightShift(int n)
        {
            if (sign == 1)
                return UnsignedRightShift(n);
            SetUnsignedDifference(this, 1);
            UnsignedRightShift(n);
            SetUnsignedSum(this, 1);
            return this;
        }

        public Word32Integer UnsignedRightShift(int n)
        {
            CheckValid();
            if (n == 0)
                return this;
            int i = n / 32;
            if (i > last)
            {
                Clear();
                return this;
            }
            int j = n - 32 * i;
            int limit = last - i;
            if (j == 0)
            {
                for (int k = 0; k <= limit; k++)
                    bits[k] = bits[k + i];
                for (int k = limit + 1; k <= last; k++)
                    bits[k] = 0;
            }
            else
            {
                int jneg = 32 - j;
                for (int k = 0; k < limit; k++)
                    bits[k] = bits[i + k + 1] << jneg | bits[i + k] >> j;
                bits[limit] = bits[i + limit] >> j;
                for (int k = limit + 1; k <= last; k++)
                    bits[k] = 0;
            }
            return SetLast(limit);
        }

        public Word32Integer SetBit(int n)
        {
            CheckValid();
            Debug.Assert(n % 32 == 0);
            int i = n / 32;
            int j = n - 32 * i;
            CheckLast(i);
            ++bits[i];
            last = Math.Max(last, i);
            CheckValid();
            return this;
        }

        public Word32Integer Add(Word32Integer a)
        {
            return SetSum(this, a);
        }

        public Word32Integer SetSum(Word32Integer a, Word32Integer b)
        {
            SetSignedSum(a, b, false);
            return this;
        }

        private void SetUnsignedSum(Word32Integer a, Word32Integer b)
        {
            CheckValid();
            int limit = Math.Max(a.last, b.last);
            CheckLast(limit);
            a.CheckLast(limit);
            b.CheckLast(limit);
            var abits = a.bits;
            var bbits = b.bits;
            ulong carry = 0;
            for (int i = 0; i <= limit; i++)
            {
                carry += (ulong)abits[i] + bbits[i];
                bits[i] = (uint)carry;
                carry >>= 32;
            }
            if (carry != 0)
            {
                CheckLast(++limit);
                bits[limit] = (uint)carry;
            }
            for (int i = limit + 1; i <= last; i++)
                bits[i] = 0;
            last = limit;
            sign = a.sign;
            CheckValid();
        }

        public Word32Integer Increment()
        {
            return SetSum(this, 1);
        }

        public Word32Integer Add(int a)
        {
            return SetSum(this, a);
        }

        public Word32Integer SetSum(Word32Integer a, int b)
        {
            SetSignedSum(a, b, false);
            return this;
        }

        public Word32Integer Add(uint a)
        {
            return SetSum(this, a);
        }

        public Word32Integer SetSum(Word32Integer a, uint b)
        {
            if (a.sign == -1)
                return SetDifference(a, b);
            SetUnsignedSum(a, b);
            return this;
        }

        private void SetUnsignedSum(Word32Integer a, uint b)
        {
            CheckValid();
            var abits = a.bits;

            // Add the word.
            int limit = a.last;
            CheckLast(limit);
            ulong carry = (ulong)abits[0] + b;
            bits[0] = (uint)carry;
            carry >>= 32;

            // Propagate carry.
            int j = 1;
            while (j <= limit && carry != 0)
            {
                carry += abits[j];
                bits[j] = (uint)carry;
                carry >>= 32;
                ++j;
            }

            // Check for overflow.
            if (carry != 0)
            {
                // Add a new word.
                CheckLast(++limit);
                bits[limit] = (uint)carry;
            }
            else if (!object.ReferenceEquals(a, this))
            {
                // Copy unchanged words.
                while (j <= limit)
                {
                    bits[j] = abits[j];
                    ++j;
                }
            }

            // Clear old words.
            for (int i = limit + 1; i <= last; i++)
                bits[i] = 0;

            // Update last and sign.
            last = limit;
            sign = a.sign;

            CheckValid();
        }

        public Word32Integer AddModulo(Word32Integer a, Word32Integer n)
        {
            CheckValid();
            CheckLast(n.last);
            Debug.Assert(a.last <= n.last);
            ulong carry = 0;
            int limit = Math.Max(last, a.last);
            var abits = a.bits;
            var nbits = n.bits;
            for (int i = 0; i <= limit; i++)
            {
                carry += (ulong)bits[i] + abits[i];
                bits[i] = (uint)carry;
                carry >>= 32;
            }
            if (carry != 0)
            {
                Debug.Assert(limit + 1 < Length);
                ++limit;
                bits[limit] = (uint)carry;
            }
            last = limit;
            if (CompareTo(n) >= 0)
                SetDifference(this, n);
            CheckValid();
            return this;
        }

        public Word32Integer Subtract(Word32Integer a)
        {
            return SetDifference(this, a);
        }

        public Word32Integer SetDifference(Word32Integer a, Word32Integer b)
        {
            SetSignedSum(a, b, true);
            return this;
        }

        private void SetUnsignedDifference(Word32Integer a, Word32Integer b)
        {
            CheckValid();
            var limit = a.last;
            CheckLast(limit);
            b.CheckLast(limit);
            var abits = a.bits;
            var bbits = b.bits;
            ulong borrow = 0;
            for (int i = 0; i <= limit; i++)
            {
                borrow += (ulong)abits[i] - bbits[i];
                bits[i] = (uint)borrow;
                borrow = (ulong)((long)borrow >> 32);
            }
            for (int i = limit + 1; i <= last; i++)
                bits[i] = 0;
            while (limit > 0 && bits[limit] == 0)
                --limit;
            last = limit;
            CheckValid();
        }

        public Word32Integer Decrement()
        {
            return SetDifference(this, 1);
        }

        public Word32Integer Subtract(int a)
        {
            return SetDifference(this, a);
        }

        public Word32Integer SetDifference(Word32Integer a, int b)
        {
            SetSignedSum(a, b, true);
            return this;
        }

        public Word32Integer Subtract(uint a)
        {
            return SetDifference(this, a);
        }

        public Word32Integer SetDifference(Word32Integer a, uint b)
        {
            if (a.sign == -1)
                return SetSum(a, b);
            SetUnsignedDifference(a, b);
            return this;
        }

        private void SetUnsignedDifference(Word32Integer a, uint b)
        {
            CheckValid();
            CheckLast(a.last);
            var abits = a.bits;
            ulong borrow = (ulong)abits[0] - b;
            bits[0] = (uint)borrow;
            borrow = (ulong)((long)borrow >> 32);
            var limit = a.last;
            int j = 1;
            while (j <= limit && borrow != 0)
            {
                borrow += (ulong)abits[j];
                bits[j] = (uint)borrow;
                borrow = (ulong)((long)borrow >> 32);
                ++j;
            }
            if (!object.ReferenceEquals(a, this))
            {
                while (j <= limit)
                {
                    bits[j] = abits[j];
                    ++j;
                }
            }
            for (int i = limit + 1; i <= last; i++)
                bits[i] = 0;
            while (limit > 0 && bits[limit] == 0)
                --limit;
            last = limit;
            sign = a.sign;
            CheckValid();
        }

        public Word32Integer SubtractModulo(Word32Integer a, Word32Integer n)
        {
            CheckValid();
            CheckLast(n.last);
            Debug.Assert(a.last <= n.last);
            if (CompareTo(a) < 0)
                SetSum(this, n);
            var abits = a.bits;
            var nbits = n.bits;
            ulong borrow = 0;
            var limit = Math.Max(last, a.last);
            for (int i = 0; i <= limit; i++)
            {
                borrow += (ulong)bits[i] - abits[i];
                bits[i] = (uint)borrow;
                borrow = (ulong)((long)borrow >> 32);
            }
            for (int i = limit + 1; i <= last; i++)
                bits[i] = 0;
            while (limit > 0 && bits[limit] == 0)
                --limit;
            last = limit;
            CheckValid();
            return this;
        }

        public void SetSignedSum(Word32Integer a, int b, bool subtraction)
        {
            var asign = a.sign;
            var bsign = 1;
            var bAbs = (uint)b;
            if (b < 0)
            {
                bsign = -1;
                bAbs = (uint)-b;
            }
            if (subtraction)
                bsign = -bsign;
            if (asign == bsign)
            {
                SetUnsignedSum(a, bAbs);
                sign = asign;
            }
            else
            {
                if (a.UnsignedCompareTo(bAbs) < 0)
                {
                    Set(bAbs - a.LeastSignificantWord);
                    sign = -asign;
                }
                else
                {
                    SetUnsignedDifference(a, bAbs);
                    sign = asign;
                }
            }
        }

        public void SetSignedSum(Word32Integer a, Word32Integer b, bool subtraction)
        {
            var asign = a.sign;
            var bsign = subtraction ? -b.sign : b.sign;
            if (asign == bsign)
            {
                SetUnsignedSum(a, b);
                sign = asign;
            }
            else
            {
                if (a.UnsignedCompareTo(b) < 0)
                {
                    SetUnsignedDifference(b, a);
                    sign = -asign;
                }
                else
                {
                    SetUnsignedDifference(a, b);
                    sign = asign;
                }
            }
        }

        public void Negate()
        {
            sign = sign == -1 ? 1 : -1;
        }

        public void AbsoluteValue()
        {
            sign = 1;
        }

        public Word32Integer Multiply(Word32Integer a, Word32IntegerStore store)
        {
            var reg1 = store.Allocate().Set(this);
            if (object.ReferenceEquals(this, a))
                SetSquare(reg1);
            else
                SetProduct(reg1, a);
            store.Release(reg1);
            return this;
        }

        public Word32Integer SetSquare(Word32Integer a)
        {
            return SetProduct(a, a);
        }

#if false
        public Word32Integer SetSquareSlow(Word32Integer a)
        {
            // Use operand scanning algorithm.
            CheckValid();
            CheckLast(2 * a.last + 1);
            var abits = a.bits;
            for (int i = 0; i <= last; i++)
                bits[i] = 0;
            int alast = a.last;
            for (int i = 0; i <= alast; i++)
            {
                ulong avalue = abits[i];
                ulong carry = avalue * avalue + bits[2 * i];
                bits[2 * i] = (uint)carry;
                carry >>= 32;
                for (int j = i + 1; j <= alast; j++)
                {
                    ulong value = avalue * abits[j];
                    ulong eps = value >> 63;
                    value <<= 1;
                    carry += value + bits[i + j];
                    bits[i + j] = (uint)carry;
                    carry >>= 32;
                    carry += eps << 32;
                }
                int k = i + alast + 1;
                carry += bits[k];
                bits[k] = (uint)carry;
                carry >>= 32;
                bits[k + 1] = (uint)carry;
            }
            int limit = 2 * alast + 1;
            while (limit > 0 && bits[limit] == 0)
                --limit;
            last = limit;
            CheckValid();
            return this;
        }
#endif

        public Word32Integer SetProduct(Word32Integer a, Word32Integer b)
        {
            // Use operand scanning algorithm.
            CheckValid();
            CheckLast(a.last + b.last + 1);
            Debug.Assert(!object.ReferenceEquals(this, a) && !object.ReferenceEquals(this, b));

            var abits = a.bits;
            var bbits = b.bits;
            int wlast = last;
            int alast = a.last;
            int blast = b.last;
            ulong ai = abits[0];
            ulong carry = 0;
            for (int j = 0; j <= blast; j++)
            {
                carry += ai * bbits[j];
                bits[j] = (uint)carry;
                carry >>= 32;
            }
            bits[blast + 1] = (uint)carry;
            for (int i = 1; i <= alast; i++)
            {
                ai = abits[i];
                carry = 0;
                for (int j = 0; j <= blast; j++)
                {
                    carry += (ulong)bits[i + j] + ai * bbits[j];
                    bits[i + j] = (uint)carry;
                    carry >>= 32;
                }
                bits[i + blast + 1] = (uint)carry;
            }
            wlast = alast + blast + 1;
            for (int i = wlast + 1; i <= last; i++)
                bits[i] = 0;
            while (wlast > 0 && bits[wlast] == 0)
                --wlast;
            last = wlast;
            sign = a.sign == b.sign ? 1 : -1;

            CheckValid();
            return this;
        }

        public Word32Integer Multiply(uint a)
        {
            return SetProduct(this, a);
        }

        public Word32Integer SetProduct(Word32Integer a, int b)
        {
            CheckValid();
            Debug.Assert(a.GetBitLength() + b.GetBitLength() <= 32 * Length);
            SetProduct(a, (uint)Math.Abs(b));
            if (b < 0)
                sign = -sign;
            return SetLast(a.last + 1);
        }

        public Word32Integer SetProduct(Word32Integer a, uint b)
        {
            // Use operand scanning algorithm.
            CheckValid();
            CheckLast(a.last + 1);
            ulong carry = 0;
            for (int j = 0; j <= a.last; j++)
            {
                carry += (ulong)b * a.bits[j];
                bits[j] = (uint)carry;
                carry >>= 32;
            }
            bits[a.last + 1] = (uint)carry;
            for (int j = a.last + 2; j <= last; j++)
                bits[j] = 0;
            sign = a.sign;
            return SetLast(a.last + 1);
        }

        public Word32Integer SetProductMasked(Word32Integer a, Word32Integer b, int n)
        {
            CheckValid();
            Debug.Assert(n % 32 == 0);
            Clear();
            int clast = (n + 31) / 32 - 1;
            CheckLast(clast);
            int alast = Math.Min(a.last, clast);
            for (int i = 0; i <= alast; i++)
            {
                int blast = Math.Min(b.last, clast - i);
                ulong carry = 0;
                for (int j = 0; j <= blast; j++)
                {
                    carry += (ulong)bits[i + j] + (ulong)a.bits[i] * b.bits[j];
                    bits[i + j] = (uint)carry;
                    carry >>= 32;
                }
                if (i + blast < clast)
                    bits[i + blast + 1] = (uint)carry;
            }
            return SetLast(clast);
        }

#if false
        /// <summary>
        /// Evaluate the product of two numbers and discard some of the lower bits.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        /// <remarks>
        /// Note: the result may be less than the result of separate multiplication
        /// and shifting operations by at most one.
        /// </remarks>
        public Word32Integer SetProductShifted(Word32Integer a, Word32Integer b, int n)
        {
            // Use product scanning algorithm.
            CheckValid();
            Debug.Assert(n % 32 == 0 && n > 0);
            int shifted = n / 32;
            Clear();
            CheckLast(a.last + b.last + 1 - shifted);
            ulong r0 = 0;
            ulong r1 = 0;
            ulong r2 = 0;
            ulong eps = 0;
            var clast = a.last + b.last + 1;
            for (int k = shifted - 1; k < clast; k++)
            {
                var min = Math.Max(k - b.last, 0);
                var max = Math.Min(k, a.last);
                for (int i = min; i <= max; i++)
                {
                    int j = k - i;
                    ulong uv = (ulong)a.bits[i] * b.bits[j];
                    r0 += (uint)uv;
                    eps = r0 >> 32;
                    r0 = (uint)r0;
                    r1 += (uv >> 32) + eps;
                    eps = r1 >> 32;
                    r1 = (uint)r1;
                    r2 += eps;
                }
                if (k >= shifted)
                    bits[k - shifted] = (uint)r0;
                r0 = r1;
                r1 = r2;
                r2 = 0;
            }
            bits[clast - shifted] = (uint)r0;
            return SetLast(clast - shifted);
        }
#endif

        public Word32Integer Divide(Word32Integer a, Word32IntegerStore store)
        {
            var reg1 = store.Allocate().Set(this);
            reg1.Set(this);
            reg1.ModuloWithQuotient(a, this);
            store.Release(reg1);
            return this;
        }

        public Word32Integer Modulo(Word32Integer a)
        {
            ModuloWithQuotient(a, null);
            return this;
        }

        public Word32Integer SetQuotient(Word32Integer a, Word32Integer b, Word32IntegerStore store)
        {
            var reg1 = store.Allocate();
            reg1.Set(a).ModuloWithQuotient(b, this);
            store.Release(reg1);
            return this;
        }

        public Word32Integer SetQuotient(Word32Integer a, Word32Integer b, Word32Integer reg1)
        {
            reg1.Set(a).ModuloWithQuotient(b, this);
            return this;
        }

        public Word32Integer SetQuotientWithRemainder(Word32Integer a, Word32Integer b, Word32Integer remainder)
        {
            remainder.Set(a).ModuloWithQuotient(b, this);
            return this;
        }

        public Word32Integer SetRemainder(Word32Integer a, Word32Integer b)
        {
            if (!object.ReferenceEquals(this, a))
                Set(a);
            ModuloWithQuotient(b, null);
            return this;
        }

        public Word32Integer ModuloWithQuotient(Word32Integer v, Word32Integer q)
        {
            if (UnsignedCompareTo(v) < 0)
            {
                if (q != null)
                    q.Clear();
                return this;
            }
            if (v.IsZero)
                throw new DivideByZeroException();
            int n = v.last + 1;
            if (n == 1)
            {
                ModuloWithQuotient(v.bits[0], q);
                if (v.sign == -1 && q != null)
                    q.sign = -q.sign;
                return this;
            }
            int dneg = v.bits[v.last].GetBitLength();
            int d = 32 - dneg;
            int m = last + 1 - n;
            CheckLast(last + 1);
            if (q != null)
                q.CheckLast(m + 1);
            var ubits = bits;
            var vbits = v.bits;
            uint v1 = vbits[v.last];
            uint v2 = vbits[v.last - 1];
            if (d != 0)
            {
                uint v3 = n > 2 ? vbits[v.last - 2] : 0;
                v1 = v1 << d | v2 >> dneg;
                v2 = v2 << d | v3 >> dneg;
            }
            for (int j = 0; j <= m; j++)
            {
                int left = n + m - j;
                uint u0 = ubits[left];
                uint u1 = ubits[left - 1];
                uint u2 = ubits[left - 2];
                if (d != 0)
                {
                    uint u3 = j < m ? ubits[left - 3] : 0;
                    u0 = u0 << d | u1 >> dneg;
                    u1 = u1 << d | u2 >> dneg;
                    u2 = u2 << d | u3 >> dneg;
                }
                ulong u0u1 = (ulong)u0 << 32 | u1;
                ulong qhat = u0 == v1 ? (1ul << 32) - 1 : u0u1 / v1;
                ulong r = u0u1 - qhat * v1;
                if (r == (uint)r && v2 * qhat > (r << 32 | u2))
                {
                    --qhat;
                    r = u0u1 - qhat * v1;
                    if (r == (uint)r && v2 * qhat > (r << 32 | u2))
                        --qhat;
                }
                ulong carry = 0;
                ulong borrow = 0;
                for (int i = 0; i < n; i++)
                {
                    carry += qhat * vbits[i];
                    borrow += (ulong)ubits[left - n + i] - (uint)carry;
                    carry >>= 32;
                    ubits[left - n + i] = (uint)borrow;
                    borrow = (ulong)((long)borrow >> 32);
                }
                borrow += ubits[left] - carry;
                ubits[left] = 0;

                if (borrow != 0)
                {
                    --qhat;
                    carry = 0;
                    for (int i = 0; i < n; i++)
                    {
                        carry += (ulong)ubits[left - n + i] + vbits[i];
                        ubits[left - n + i] = (uint)carry;
                        carry >>= 32;
                    }
                }
                if (q != null)
                    q.bits[m - j] = (uint)qhat;
            }
            if (q != null)
            {
                for (int i = m + 1; i <= q.last; i++)
                    q.bits[i] = 0;
                q.SetLast(m);
                q.sign = sign == v.sign ? 1 : -1;
            }
            SetLast(n - 1);
            return this;
        }

        public Word32Integer Divide(uint a, Word32IntegerStore store)
        {
            var reg1 = store.Allocate();
            reg1.Set(this).ModuloWithQuotient(a, this);
            store.Release(reg1);
            return this;
        }

        public Word32Integer Modulo(uint a)
        {
            ModuloWithQuotient(a, null);
            return this;
        }

        public Word32Integer SetQuotient(Word32Integer a, int b, Word32IntegerStore store)
        {
            var reg1 = store.Allocate();
            reg1.Set(a).ModuloWithQuotient((uint)Math.Abs(b), this);
            if (b < 0)
                sign = -sign;
            store.Release(reg1);
            return this;
        }

        public Word32Integer SetQuotient(Word32Integer a, uint b, Word32IntegerStore store)
        {
            var reg1 = store.Allocate();
            reg1.Set(a).ModuloWithQuotient(b, this);
            store.Release(reg1);
            return this;
        }

        public Word32Integer SetQuotientWithRemainder(Word32Integer a, uint b, Word32Integer remainder)
        {
            remainder.Set(a).ModuloWithQuotient(b, this);
            return this;
        }

        public Word32Integer SetRemainder(Word32Integer a, uint b)
        {
            return Set(a.GetRemainder(b));
        }

        public int GetRemainder(int v)
        {
            var result = (int)GetRemainder((uint)Math.Abs(v));
            if (sign == -1)
                result = -result;
            return result;
        }

        public uint GetRemainder(uint v)
        {
            if (v == 0)
                throw new DivideByZeroException();
            var u0 = (ulong)(bits[last] % v);
            for (int j = last - 1; j >= 0; j--)
                u0 = (u0 << 32 | bits[j]) % v;
            Debug.Assert(BigInteger.Abs(this) % v == u0);
            return (uint)u0;
        }

        public Word32Integer ModuloWithQuotient(uint v, Word32Integer q)
        {
            if (q == null)
            {
                var result = GetRemainder(v);
                Clear();
                bits[0] = result;
                return this;
            }
            if (v == 0)
                throw new DivideByZeroException();
            int m = last;
            CheckLast(m + 1);
            q.CheckLast(m + 1);
            var ubits = bits;
            for (int j = 0; j <= m; j++)
            {
                int left = 1 + m - j;
                uint u0 = ubits[left];
                uint u1 = ubits[left - 1];
                ulong u0u1 = (ulong)u0 << 32 | u1;
                ulong qhat = u0 == v ? (1ul << 32) - 1 : u0u1 / v;
                ubits[left - 1] = (uint)(u0u1 - qhat * v);
                ubits[left] = 0;
                q.bits[m - j] = (uint)qhat;
            }
            for (int i = m + 1; i <= q.last; i++)
                q.bits[i] = 0;
            q.SetLast(m);
            last = 0;
            q.sign = sign;
            return this;
        }

#if true
        public Word32Integer BarrettReduction(Word32Integer z, Word32Integer mu, int k)
        {
            // Use product scanning algorithm.
            CheckValid();
            Clear();
            CheckLast(z.last + mu.last + 1);
            var abits = z.bits;
            var mubits = mu.bits;
            ulong r0 = 0;
            ulong r1 = 0;
            ulong r2 = 0;
            ulong eps = 0;
            var clast = z.last + mu.last + 1;
            for (int ij = k - 2; ij < clast; ij++)
            {
                var min = Math.Max(ij - mu.last, 0);
                var max = Math.Min(ij, z.last - (k - 1));
                for (int i = min; i <= max; i++)
                {
                    int j = ij - i;
                    ulong uv = (ulong)bits[i + k - 1] * mubits[j];
                    r0 += (uint)uv;
                    eps = r0 >> 32;
                    r0 = (uint)r0;
                    r1 += (uv >> 32) + eps;
                    eps = r1 >> 32;
                    r1 = (uint)r1;
                    r2 += eps;
                }
                if (ij >= k - 1)
                    bits[ij - k - 1] = (uint)r0;
                r0 = r1;
                r1 = r2;
                r2 = 0;
            }
            bits[clast - (k - 1)] = (uint)r0;
            return SetLast(clast - (k - 1));
        }
#endif

        public Word32Integer MontgomerySOS(Word32Integer n, uint k0)
        {
            // SOS Method - Separated Operand Scanning
            CheckValid();
            var s = n.last + 1;
            CheckLast(2 * s + 1);
            var nbits = n.bits;
            for (int i = 0; i < s; i++)
            {
                var m = bits[i] * k0;
                var carry = bits[i] + (ulong)m * nbits[0];
                bits[i] = (uint)carry;
                for (int j = 1; j < s; j++)
                {
                    carry = (carry >> 32) + bits[i + j] + (ulong)m * nbits[j];
                    bits[i + j] = (uint)carry;
                }
                for (int ij = i + s; (carry >>= 32) != 0; ij++)
                {
                    carry += bits[ij];
                    bits[ij] = (uint)carry;
                }
            }
            for (int i = 0; i <= s; i++)
            {
                bits[i] = bits[i + s];
                bits[i + s] = 0;
            }
            while (s > 0 && bits[s] == 0)
                --s;
            last = s;
            return this;
        }

        public Word32Integer MontgomeryCIOS(Word32Integer u, Word32Integer v, Word32Integer n, uint k0)
        {
            // CIOS Method - Coarsely Integrated Operand Scanning
            CheckValid();
            var s = n.last + 1;
            CheckLast(2 * s + 1);
            var ubits = u.bits;
            var vbits = v.bits;
            var nbits = n.bits;
            for (int i = 0; i <= last; i++)
                bits[i] = 0;
            for (int i = 0; i < s; i++)
            {
                var ui = ubits[i];
                var carry = bits[0] + (ulong)ui * vbits[0];
                bits[0] = (uint)carry;
                for (int j = 1; j < s; j++)
                {
                    carry = (carry >> 32) + bits[j] + (ulong)ui * vbits[j];
                    bits[j] = (uint)carry;
                }
                carry = (carry >> 32) + bits[s];
                bits[s] = (uint)carry;
                bits[s + 1] = (uint)(carry >> 32);
                var m = bits[0] * k0;
                carry = bits[0] + (ulong)m * nbits[0];
                for (int j = 1; j < s; j++)
                {
                    carry = (carry >> 32) + bits[j] + (ulong)m * nbits[j];
                    bits[j - 1] = (uint)carry;
                }
                carry = (carry >> 32) + bits[s];
                bits[s - 1] = (uint)carry;
                bits[s] = bits[s + 1] + (uint)(carry >> 32);
            }
            bits[s + 1] = 0;
            while (s > 0 && bits[s] == 0)
                --s;
            last = s;
            return this;
        }

        private Word32Integer SetLast(int n)
        {
            if (n < 0)
                last = 0;
            else
            {
                int i = n;
                if (i == Length)
                    --i;
                while (i > 0 && bits[i] == 0)
                    --i;
                last = i;
            }
            CheckValid();
            return this;
        }

        private static uint[] GetBits(BigInteger n)
        {
            var bytes = n.ToByteArray();
            var byteLength = bytes.Length;
            if (bytes[byteLength - 1] == 0)
                --byteLength;
            int Length = (byteLength + 3) / 4;
            if (4 * Length > bytes.Length)
            {
                var newBytes = new byte[4 * Length];
                bytes.CopyTo(newBytes, 0);
                bytes = newBytes;
            }
            var bits = new uint[Length];
            for (int i = 0; i < Length; i++)
                bits[i] = (uint)BitConverter.ToInt32(bytes, 4 * i);
            return bits;
        }

        [Conditional("DEBUG")]
        private void CheckValid()
        {
            CheckValid(this);
        }

        [Conditional("DEBUG")]
        private static void CheckValid(Word32Integer x)
        {
            if (x.sign != -1 && x.sign != 1)
                throw new InvalidOperationException("invalid sign");
            if (x.last == 0 && x.bits[0] == 0)
                return;
            if (x.last >= x.Length)
                throw new InvalidOperationException("overrun");
            if (x.bits[x.last] == 0)
                throw new InvalidOperationException("last miscount");
            for (int i = x.last + 1; i < x.Length; i++)
                if (x.bits[i] != 0)
                    throw new InvalidOperationException("not zeroed");
        }
    }
}
