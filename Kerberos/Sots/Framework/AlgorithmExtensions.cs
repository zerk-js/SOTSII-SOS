// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Framework.AlgorithmExtensions
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Framework
{
	internal static class AlgorithmExtensions
	{
		public static void DistributePercentages<E>(
		  ref Dictionary<E, float> ratios,
		  E lockedVar,
		  float newValue)
		{
			float num1 = 1f;
			bool flag = false;
			if (ratios.ContainsKey(lockedVar))
			{
				if (ratios.Count == 1)
				{
					ratios[lockedVar] = 1f;
					return;
				}
				num1 = 1f - ratios[lockedVar];
				ratios.Remove(lockedVar);
				flag = true;
			}
			Dictionary<E, float> dictionary1 = new Dictionary<E, float>((IDictionary<E, float>)ratios);
			foreach (E key in dictionary1.Keys)
				ratios[key] = (double)num1 == 0.0 ? 0.0f : ratios[key] / num1;
			float num2 = 1f - newValue;
			float num3 = num2 * (1f - ratios.Sum<KeyValuePair<E, float>>((Func<KeyValuePair<E, float>, float>)(x => x.Value)));
			foreach (E key in dictionary1.Keys)
			{
				Dictionary<E, float> dictionary2;
				E index1;
				(dictionary2 = ratios)[index1 = key] = dictionary2[index1] * num2;
				Dictionary<E, float> dictionary3;
				E index2;
				(dictionary3 = ratios)[index2 = key] = dictionary3[index2] + num3 / (float)ratios.Keys.Count;
			}
			if (!flag)
				return;
			ratios.Add(lockedVar, newValue);
		}

		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
		{
			T[] elements = source.ToArray<T>();
			for (int index1 = elements.Length - 1; index1 > 0; --index1)
			{
				int index2 = rng.Next(index1 + 1);
				T obj = elements[index1];
				elements[index1] = elements[index2];
				elements[index2] = obj;
			}
			foreach (T obj in elements)
				yield return obj;
		}

		public static void Shuffle<T>(this IList<T> list)
		{
			Random random = new Random();
			int count = list.Count;
			while (count > 1)
			{
				--count;
				int index = random.Next(count + 1);
				T obj = list[index];
				list[index] = list[count];
				list[count] = obj;
			}
		}

		public static bool ExistsFirst<T>(
		  this IEnumerable<T> source,
		  Func<T, bool> predicate,
		  out T target)
		{
			foreach (T obj in source)
			{
				if (predicate(obj))
				{
					target = obj;
					return true;
				}
			}
			target = default(T);
			return false;
		}

		public static IEnumerable<T> IntersectSet<T>(
		  this IEnumerable<T> first,
		  ISet<T> second)
		{
			foreach (T obj in first)
			{
				if (((ICollection<T>)second).Contains(obj))
					yield return obj;
			}
		}

		public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
		{
			if (val.CompareTo(min) < 0)
				return min;
			if (val.CompareTo(max) > 0)
				return max;
			return val;
		}

		public static float Normalize(this float val, float min, float max)
		{
			if ((double)val > (double)max)
				return 1f;
			if ((double)val < (double)min)
				return 0.0f;
			float num = max - min;
			return (val - min) / num;
		}
	}
}
