﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Decompose.Numerics
{
    public class PrimeCounting
    {
        private const int sizeSmall = 1024;
        private const int chunkSize = 32;
        private int threads;
        private int[] piSmall;
        private int[] tauSumSmall;
        private int[] mobiusSmall;

        public PrimeCounting(int threads)
        {
            this.threads = threads;
            var n = sizeSmall;
            var i = 0;
            var count = 0;
            piSmall = new int[n];
            foreach (var p in new SieveOfErostothones())
            {
                while (i < p && i < n)
                    piSmall[i++] = count;
                if (i < n)
                    piSmall[i++] = ++count;
                if (i == n)
                    break;
            }
            var divisors = new DivisorsCollection(n);
            tauSumSmall = new int[n];
            tauSumSmall[0] = divisors[0];
            for (var j = 1; j < n; j++)
                tauSumSmall[j] = (tauSumSmall[j - 1] + divisors[j]) & 3;
            mobiusSmall = new MobiusCollection(sizeSmall, 0).ToArray();
        }

        public int Pi(int x)
        {
            if (x < piSmall.Length)
                return piSmall[x];
            return new SieveOfErostothones().TakeWhile(p => p <= x).Count();
        }

        public int PiWithPowers(int x)
        {
            var sum = Pi(x);
            for (int j = 2; true; j++)
            {
                var root = IntegerMath.FloorRoot(x, j);
                if (root == 1)
                    break;
                sum += Pi(root);
            }
            return sum;
        }

        public int ParityOfPi(long x)
        {
            // pi(x) mod 2 = SumTwoToTheOmega(x)/2 mod 2- sum(pi(floor(x^(1/j)) mod 2)
            if (x < piSmall.Length)
                return piSmall[x] % 2;
            var parity = SumTwoToTheOmega(x) / 2 % 2;
            for (var j = 2; true; j++)
            {
                var root = IntegerMath.FloorRoot(x, j);
                if (root == 1)
                    break;
                parity ^= ParityOfPi(root);
            }
            return parity;
        }

        public int ParityOfPi(BigInteger x)
        {
            if (x < piSmall.Length)
                return piSmall[(int)x] % 2;
            var parity = SumTwoToTheOmega(x) / 2 % 2;
            for (int j = 2; true; j++)
            {
                var root = IntegerMath.FloorRoot(x, j);
                if (root == 1)
                    break;
                parity ^= ParityOfPi(root);
            }
            return parity;
        }

        private int SumTwoToTheOmega(long x)
        {
            // sum(2^w(d), d=[1,x]) mod 4 = sum(mu(d)TauSum(x/d^2), d=[1,floor(sqrt(x))]) mod 4
            var limit = (int)IntegerMath.FloorSquareRoot(x);
            var sum = 0;
            var mobius = new MobiusCollection(limit + 1, threads);
            for (var d = 1; d <= limit; d++)
            {
                var mu = mobius[d];
                if (mu == 1)
                    sum += TauSum(x / ((long)d * d));
                else if (mu == -1)
                    sum += 4 - TauSum(x / ((long)d * d));
            }
            return sum;
        }

        private int SumTwoToTheOmega(BigInteger x)
        {
            var limit = IntegerMath.FloorSquareRoot(x);
            if (limit <= int.MaxValue)
                return SumTwoToTheOmega((long)x, (int)limit);
            if (limit <= long.MaxValue)
                return SumTwoToTheOmega((UInt128)x, (long)limit);
            var sum = 0;
            var nLast = (BigInteger)0;
            var tauLast = 0;
            for (var d = (BigInteger)1; d <= limit; d++)
            {
                var mu = IntegerMath.Mobius(d);
                if (mu != 0)
                {
                    var n = x / (d * d);
                    var tau = n == nLast ? tauLast : TauSum(n);
                    if (mu == 1)
                        sum += tau;
                    else
                        sum += 4 - tau;
                    tauLast = tau;
                    nLast = n;
                }
            }
            return sum;
        }

        private int SumTwoToTheOmegaSimple(long x, int limit)
        {
            var mobius = new MobiusCollection(limit + 1, 2 * threads);
            var sum = 0;
            var nLast = (long)0;
            var tauLast = 0;
            for (var d = 1; d <= limit; d++)
            {
                var mu = mobius[d];
                if (mu != 0)
                {
                    var n = x / ((long)d * d);
                    var tau = n == nLast ? tauLast : TauSum(n);
                    if (mu == 1)
                        sum += tau;
                    else
                        sum += 4 - tau;
                    tauLast = tau;
                    nLast = n;
                }
            }
            return sum;
        }

        private int blockSize = 1 << 24;
        private int singleLimit1 = 1;
        private int singleLimit2 = 100;

        private struct WorkItem
        {
            public int Min;
            public int Max;
        }

        private int SumTwoToTheOmega(long x, int limit)
        {
            //Console.WriteLine("Sum(2^w), x = {0}", x);
            var mobius = new MobiusRange(limit + 1, 0);
            var queue = new BlockingCollection<WorkItem>();
            var units = limit < (1 << 16) ? 1 : 100;
            var consumers = Math.Max(1, threads);
            var sum = 0;
            var tasks = new Task[consumers];
            for (var consumer = 0; consumer < consumers; consumer++)
            {
                var thread = consumer;
                tasks[consumer] = Task.Factory.StartNew(() => ConsumeItems(thread, queue, mobius, x, ref sum));
            }
            int d;
            for (d = 1; d < singleLimit1; d++)
            {
                var mu = mobiusSmall[d];
                if (mu != 0)
                {
                    var tau = TauSumParallel(x / (d * d));
                    if (mu == 1)
                        sum += tau;
                    else
                        sum += 4 - tau;
                }
            }
            for (d = singleLimit1; d < singleLimit2; d++)
            {
                if (mobiusSmall[d] != 0)
                    queue.Add(new WorkItem { Min = d, Max = d + 1 });
            }
            for (var unit = 0; unit < units; unit++)
            {
                var dmin = d;
                var dmax = unit == units - 1 ? limit + 1 : (int)Math.Exp((unit + 1) * Math.Log(limit + 1) / units);
                if (dmin >= dmax)
                    continue;
                if (dmax - dmin > blockSize)
                    break;
                queue.Add(new WorkItem { Min = dmin, Max = dmax });
                d = dmax;
            }
            while (d < limit + 1)
            {
                var dmin = d;
                var dmax = Math.Min(dmin + blockSize, limit + 1);
                queue.Add(new WorkItem { Min = dmin, Max = dmax });
                d = dmax;
            }
            queue.CompleteAdding();
            Task.WaitAll(tasks);
            return sum & 3;
        }

        private void ConsumeItems(int thread, BlockingCollection<WorkItem> queue, MobiusRange mobius, long x, ref int sum)
        {
            var item = default(WorkItem);
            var values = new sbyte[blockSize];
            while (queue.TryTake(out item, Timeout.Infinite))
            {
                //Console.WriteLine("+ Sum(2^w), dmin = {0}, dmax = {1}, thread = {2}", item.Min, item.Max, thread);
                var timer = new Stopwatch();
                timer.Start();
                if (item.Max == item.Min + 1)
                    values[0] = (sbyte)mobiusSmall[item.Min];
                else
                    mobius.GetValues(item.Min, item.Max, values);
                Interlocked.Add(ref sum, SumTwoToTheOmega(values, x, item.Min, item.Max));
                //Console.WriteLine("- Sum(2^w), dmin = {0}, dmax = {1}, thread = {2}, elapsed = {3:F3} msec",
                //    item.Min, item.Max, thread, (double)timer.ElapsedTicks / Stopwatch.Frequency * 1000);
            }
        }

        private int SumTwoToTheOmega(sbyte[] mobius, long x, int dmin, int dmax)
        {
            //Console.WriteLine("dmin = {0}, dmax = {1}", dmin, dmax);
            var sum = 0;
            var last = (long)0;
            var current = x / ((long)dmax * dmax);
            var delta = dmax == 1 ? (long)0 : x / ((long)(dmax - 1) * (dmax - 1)) - current;
            var d = dmax - 1;
            var count = 0;
            while (d >= dmin)
            {
                var mu = mobius[d - dmin];
                if (mu != 0)
                {
                    var dSquared = (long)d * d;
                    var product = (current + delta) * dSquared;
                    if (product > x)
                    {
                        do
                        {
                            --delta;
                            product -= dSquared;
                        }
                        while (product > x);
                    }
                    else if (product + dSquared <= x)
                    {
                        ++delta;
                        if (product + 2 * dSquared <= x)
                            break;
                    }
                    current += delta;
                    Debug.Assert(x / dSquared == current);
                    if (current != last)
                    {
                        if ((count & 3) != 0)
                        {
                            var tau = TauSum(last);
                            if (count > 0)
                                sum += count * tau;
                            else
                                sum -= count * (4 - tau);
                        }
                        count = 0;
                        last = current;
                    }
                    count += mu;
                }
                --d;
            }
            //Console.WriteLine("d middle = {0}", d);
            while (d >= dmin)
            {
                var mu = mobius[d - dmin];
                if (mu != 0)
                {
                    current = x / ((long)d * d);
                    if (current != last)
                    {
                        var tau = TauSum(last);
                        if (count > 0)
                            sum += count * tau;
                        else
                            sum -= count * (4 - tau);
                        count = 0;
                        last = current;
                    }
                    count += mu;
                }
                --d;
            }
            {
                var tau = TauSum(last);
                if (count > 0)
                    sum += count * tau;
                else
                    sum -= count * (4 - tau);
            }
            return sum;
        }

        private int SumTwoToTheOmega(UInt128 x, long limit)
        {
            throw new NotImplementedException();
        }

        private int TauSumSimple(long y)
        {
            if (y == 0)
                return 0;
            var sum = 0;
            var n = (long)1;
            var squared = y - 1;
            while (true)
            {
                sum ^= (int)((y / n) & 1);
                squared -= 2 * n + 1;
                if (squared < 0)
                    break;
                ++n;
            }
            sum = 2 * sum - (int)((n * n) & 3);
            return sum & 3;
        }

        private int TauSumParallel(long y)
        {
            if (y < tauSumSmall.Length)
                return tauSumSmall[y];
            var sqrt = 0;
            var sum = TauSumInnerParallel(y, out sqrt);
            sum = 2 * sum - (int)((sqrt * sqrt) & 3);
            return sum & 3;
        }

        private int TauSum(long y)
        {
            // sum(tau(d), d=[1,y]) = 2 sum(y/d, d=[1,floor(sqrt(y))]) - floor(sqrt(y))^2
            if (y < tauSumSmall.Length)
                return tauSumSmall[y];
            var sqrt = 0;
            var sum = TauSumInner(y, out sqrt);
            sum = 2 * sum - (int)((sqrt * sqrt) & 3);
            return sum & 3;
        }

        public int TauSum(BigInteger y)
        {
            if (y <= long.MaxValue)
                return TauSum((long)y);
            var sum = 0;
            var n = (BigInteger)1;
            var squared = y - 1;
            while (true)
            {
                sum ^= (int)((y / n) & 1);
                squared -= 2 * n + 1;
                if (squared < 0)
                    break;
                ++n;
            }
            sum = 2 * sum - (int)((n * n) & 3);
            return sum & 3;
        }

        public int TauSumInnerSimple(long y, out int sqrt)
        {
            // Computes sum(floor(y/d), d=[1,floor(sqrt(y))]) mod 2.
            if (y == 0)
            {
                sqrt = 0;
                return 0;
            }
            var sum = 0;
            var n = (long)1;
            var squared = y - 1;
            while (true)
            {
                sum ^= (int)(y / n);
                squared -= 2 * n + 1;
                if (squared < 0)
                    break;
                ++n;
            }
            sqrt = (int)n;
            return sum & 1;
        }

        public int TauSumInner(long y, out int sqrt)
        {
            // Computes sum(floor(y/d), d=[1,floor(sqrt(y))]) mod 2.
            // To avoid division, we start at the
            // end and proceed backwards using multiplication
            // with estimates.  We keep track of the
            // difference between steps and let
            // it increase by at most one each iteration.
            // As soon as it starts changing too quickly
            // we resort to a different method where
            // the quantity floor(y/d) is odd iff y mod 2d >= d.
            if (y <= int.MaxValue)
                return TauSumInnerSmall((int)y, out sqrt);
            return TauSumInnerLarge(y, out sqrt);
        }

        public int TauSumInnerSmall(int y, out int sqrt)
        {
            var limit = (int)Math.Floor(Math.Sqrt(y));
            var sum1 = 0;
            var current = limit - 1;
            var delta = 1;
            var i = limit;
            while (i > 0)
            {
                var product = (current + delta) * i;
                if (product > y)
                    --delta;
                else if (product + i <= y)
                {
                    ++delta;
                    if (product + 2 * i <= y)
                        break;
                }
                current += delta;
                Debug.Assert(y / i == current);
                sum1 ^= current;
                --i;
            }
            sum1 &= 1;
            var sum2 = 0;
            var count2 = 0;
            while (i > 0)
            {
                sum2 ^= (int)(y % (i << 1)) - i;
                --i;
                ++count2;
            }
            sum2 = (sum2 >> 31) & 1;
            if ((count2 & 1) != 0)
                sum2 ^= 1;
            sqrt = limit;
            return sum1 ^ sum2;
        }

        public int TauSumInnerLarge(long y, out int sqrt)
        {
            var limit = (int)Math.Floor(Math.Sqrt(y));
            var sum1 = 0;
            var current = (long)limit - 1;
            var delta = 1;
            var i = limit;
            while (i > 0)
            {
                var product = (current + delta) * i;
                if (product > y)
                    --delta;
                else if (product + i <= y)
                {
                    ++delta;
                    if (product + 2 * i <= y)
                        break;
                }
                current += delta;
                Debug.Assert(y / i == current);
                sum1 ^= (int)current;
                --i;
            }
            sum1 &= 1;
            var sum2 = 0;
            var count2 = 0;
            while (i > 0)
            {
                sum2 ^= (int)(y % (i << 1)) - i;
                --i;
                ++count2;
            }
            sum2 = (sum2 >> 31) & 1;
            if ((count2 & 1) != 0)
                sum2 ^= 1;
            sqrt = limit;
            return sum1 ^ sum2;
        }

        public int TauSumInnerParallel(long y, out int sqrt)
        {
            var limit = (int)Math.Floor(Math.Sqrt(y));

            var sum = TauSumInnerWorker(y, 1, limit + 1);
            sqrt = limit;
            return sum;
        }

        public int TauSumInnerWorker(long y, int imin, int imax)
        {
            var sum1 = 0;
            var current = y / imax;
            var delta = y / (imax - 1) - current;
            var i = imax - 1;
            while (i >= imin)
            {
                var product = (long)(current + delta) * i;
                if (product > y)
                    --delta;
                else if (product + i <= y)
                {
                    ++delta;
                    if (product + 2 * i <= y)
                        break;
                }
                current += delta;
                Debug.Assert(y / i == current);
                sum1 ^= (int)current;
                --i;
            }
            sum1 &= 1;
            var sum2 = 0;
            var count2 = 0;
            while (i >= imin)
            {
                sum2 ^= (int)(y % (i << 1)) - i;
                --i;
                ++count2;
            }
            sum2 = (sum2 >> 31) & 1;
            if ((count2 & 1) != 0)
                sum2 ^= 1;
            return sum1 ^ sum2;
        }

#if false
        private struct WorkItem
        {
            public int Count { get; set; }
            public long Value { get; set; }
        }

        private const int numberOfInitialValues = 1000;

        private int SumTwoToTheOmegaNew(long x, int limit)
        {
            var queue = new BlockingCollection<WorkItem>();
            var mobius = new MobiusCollection(limit + 1, 2 * threads);
            Task.Factory.StartNew(() => ProduceItems(queue, mobius, x, limit));
            var sum = 0;
            if (threads == 0)
            {
                ProcessFirstFewValues(mobius, x, limit, ref sum);
                ConsumeItems(queue, ref sum);
            }
            else
            {
                var initialTask = Task.Factory.StartNew(() => ProcessFirstFewValues(mobius, x, limit, ref sum));
                var tasks = new Task[threads];
                for (int thread = 0; thread < threads; thread++)
                    tasks[thread] = Task.Factory.StartNew(() => ConsumeItems(queue, ref sum));
                Task.WaitAll(tasks);
                initialTask.Wait();
            }
            return sum;
        }

        private void ProcessFirstFewValues(MobiusCollection mobius, long x, int limit, ref int sum)
        {
            var max = Math.Min(limit, numberOfInitialValues);
            var subtotal = 0;
            Parallel.For(1, max + 1,
                d =>
                {
                    //Console.WriteLine("x = {0}, d = {1}", x, d);
                    var mu = mobius[d];
                    if (mu == 1)
                        Interlocked.Add(ref subtotal, TauSum(x / ((long)d * d)));
                    else if (mu == -1)
                        Interlocked.Add(ref subtotal, 4 - TauSum(x / ((long)d * d)));
                });
            Interlocked.Add(ref sum, subtotal);
            //Console.WriteLine("done with {0} values", max);
        }

        private void ConsumeItems(BlockingCollection<WorkItem> queue, ref int sum)
        {
            var item = default(WorkItem);
            while (queue.TryTake(out item, Timeout.Infinite))
                Interlocked.Add(ref sum, ProcessBatch(item.Count, item.Value));
        }

        private void ProduceItems(BlockingCollection<WorkItem> queue, MobiusCollection mobius, long x, int limit)
        {
#if true
            var totalAdded1 = (long)0;
            var totalAdded2 = (long)0;
#endif
            var last = (long)0;
            var current = (long)1;
            var delta = 0;
            var d = limit;
            var count = 0;
            while (d > numberOfInitialValues)
            {
                var mu = mobius[d];
                if (mu != 0)
                {
                    var dSquared = (long)d * d;
                    var product = (current + delta) * dSquared;
                    if (product > x)
                    {
                        do
                        {
                            --delta;
                            product -= dSquared;
                        }
                        while (product > x);
                    }
                    else if (product + dSquared <= x)
                    {
                        ++delta;
                        if (product + 2 * dSquared <= x)
                            break;
                    }
                    current += delta;
                    Debug.Assert(x / dSquared == current);
                    if (current != last)
                    {
#if true
                        ++totalAdded1;
#endif
                        queue.Add(new WorkItem { Count = count, Value = last });
                        count = 0;
                        last = current;
                    }
                    count += mu;
                }
                --d;
            }
            var dmax = d;
            for (d = numberOfInitialValues + 1; d <= dmax; d++)
            {
                var mu = mobius[d];
                if (mu != 0)
                {
                    current = x / ((long)d * d);
                    if (current != last)
                    {
#if true
                        ++totalAdded2;
#endif
                        queue.Add(new WorkItem { Count = count, Value = last });
                        count = 0;
                        last = current;
                    }
                    count += mu;
                }
            }
#if true
            ++totalAdded2;
#endif
            queue.Add(new WorkItem { Count = count, Value = last });
            queue.CompleteAdding();
        }

        private int ProcessBatch(int count, long last)
        {
            if ((count & 3) != 0)
            {
                var tau = TauSum(last);
                //Console.WriteLine("count = {0}, last = {1}, tau = {2}", count, last, tau);
                if (count == 1)
                    return tau;
                else if (count == -1)
                    return 4 - tau;
                else if (count > 0)
                    return (count * tau) & 3;
                else
                    return (-count * (4 - tau)) & 3;
            }
            return 0;
        }
#endif
    }
}
