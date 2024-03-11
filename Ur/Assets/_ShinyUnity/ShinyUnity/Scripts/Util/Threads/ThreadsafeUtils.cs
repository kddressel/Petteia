// The MIT License (MIT)
// 
// Copyright (c) 2024 Shiny Dolphin Games LLC
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// NOTE: Although UnityEngine is included, you can only use the subset of functions that can be used off the main thread (Mathf functions, Vector2 etc)
namespace Shiny.Threads
{
    /// <summary>
    /// As subset of Utils that are threadsafe. They are copy pasted, which is bad, but it would be annoying to have to remember where each function lives.
    /// </summary>
    public static class ThreadsafeUtils
    {
        public static ThreadsafeRandom Random { get; private set; } = new ThreadsafeRandom();

        public static IEnumerable<T> RepeatFactory<T>(Func<int, T> factory, int count) => Enumerable.Range(0, count).Select(factory);

        public static float AverageSafe<T>(this T[] self, Func<T, float> selector)
        {
            if (self.Length == 0) return default;

            var sum = 0f;
            foreach (var val in self)
            {
                sum += selector(val);
            }
            return sum / self.Length;
        }

        public static float AverageSafe<T>(this List<T> self, Func<T, float> selector)
        {
            if (self.Count == 0) return default;

            var sum = 0f;
            foreach (var val in self)
            {
                sum += selector(val);
            }
            return sum / self.Count;
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> self)
        {
            List<T> shuffledList = self.ToList();
            int n = shuffledList.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Next(0, n + 1);
                T value = shuffledList[k];
                shuffledList[k] = shuffledList[n];
                shuffledList[n] = value;
            }
            return shuffledList;
        }

        public static T RandomElement<T>(this Array self)
        {
            return (T)self.GetValue(Random.Next(0, self.Length));
        }

        public static T RandomElement<T>(this T[] self)
        {
            return self.ElementAtOrDefault(Random.Next(0, self.Length));
        }

        public static T WeightedRandomElement<T>(this IEnumerable<T> self, IEnumerable<float> weights)
        {
            var totalWeight = weights.Sum();
            var random = Random.NextDouble() * totalWeight;
            var currWeight = 0f;
            for (var i = 0; i < weights.Count(); i++)
            {
                currWeight += weights.ElementAt(i);
                if (random < currWeight)
                {
                    return self.ElementAt(i);
                }
            }

            return default(T);
        }

        public static IEnumerable<T> WithReplacement<T>(this IEnumerable<T> self, T old, T replacement) where T : class
        {
            var working = self.ToList();
            var idx = working.IndexOf(old);
            working.Remove(old);
            working.Insert(idx, replacement);
            return working;
        }

        public static float RemapAndClampRange(float val, float windowStart, float windowEnd, float newMin, float newMax)
        {
            float windowSize = windowEnd - windowStart;
            float clamped = Mathf.Clamp(val, windowStart, windowEnd);
            float newSize = newMax - newMin;
            float t = (clamped - windowStart) / windowSize;
            return newMin + t * newSize;
        }

        public static float NormalizedDistance(Vector2 a, Vector2 b, float min, float max) => RemapAndClampRange(Vector2.Distance(a, b), min, max, 0, 1);
    }
}
