// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Framework.RandomExtensions
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Framework
{
	public static class RandomExtensions
	{
		public static float NextSingle(this Random random)
		{
			return (float)random.NextDouble();
		}

		public static int NextInclusive(this Random random, Range<int> range)
		{
			return random.NextInclusive(range.Min, range.Max);
		}

		public static float NextInclusive(this Random random, Range<float> range)
		{
			return random.NextInclusive(range.Min, range.Max);
		}

		public static double NextInclusive(this Random random, Range<double> range)
		{
			return random.NextInclusive(range.Min, range.Max);
		}

		public static int NextInclusive(this Random random, int minValue, int maxValue)
		{
			return random.Next(minValue, maxValue + 1);
		}

		public static double NextInclusive(this Random random, double minValue, double maxValue)
		{
			return ScalarExtensions.Lerp(minValue, maxValue, random.NextDouble());
		}

		public static float NextInclusive(this Random random, float minValue, float maxValue)
		{
			return ScalarExtensions.Lerp(minValue, maxValue, (float)random.NextDouble());
		}

		public static T Choose<T>(this Random random, IList<T> choices)
		{
			if (choices.Count == 0)
				throw new InvalidOperationException("Cannot choose item from an empty list.");
			int index = random.Next(choices.Count);
			return choices[index];
		}

		public static T Choose<T>(this Random random, IEnumerable<T> choices)
		{
			if (!choices.Any<T>())
				throw new InvalidOperationException("Cannot choose item from an empty enumeration.");
			int index = random.Next(choices.Count<T>());
			return choices.ElementAt<T>(index);
		}

		public static bool CoinToss(this Random random, double odds)
		{
			if (odds <= 0.0)
				return false;
			if (odds >= 1.0)
				return true;
			return random.NextDouble() <= odds;
		}

		public static bool CoinToss(this Random random, int odds)
		{
			if (odds <= 0)
				return false;
			if (odds >= 100)
				return true;
			return random.NextInclusive(0, 100) <= odds;
		}

		public static double NextNormal(this Random random)
		{
			double num1;
			double d;
			do
			{
				num1 = 2.0 * random.NextDouble() - 1.0;
				double num2 = 2.0 * random.NextDouble() - 1.0;
				d = num1 * num1 + num2 * num2;
			}
			while (d >= 1.0);
			double num3 = Math.Sqrt(-2.0 * Math.Log(d) / d);
			return num1 * num3;
		}

		public static double NextNormal(this Random random, double min, double max)
		{
			double mean = (min + max) / 2.0;
			return random.NextNormal(min, max, mean);
		}

		public static double NextNormal(this Random random, double min, double max, double mean)
		{
			int num1 = 3;
			double num2 = (max - mean) / (double)num1;
			double num3;
			do
			{
				num3 = num2 * random.NextNormal() + mean;
			}
			while (num3 < min || num3 > max);
			return num3;
		}

		public static double NextNormal(this Random random, Range<double> range)
		{
			return random.NextNormal(range.Min, range.Max);
		}

		public static float NextNormal(this Random random, Range<float> range)
		{
			return (float)random.NextNormal((double)range.Min, (double)range.Max);
		}

		public static Vector3 PointInSphere(this Random random, float radius)
		{
			Vector3 vector3;
			do
			{
				vector3 = new Vector3(random.NextInclusive(-radius, radius), random.NextInclusive(-radius, radius), random.NextInclusive(-radius, radius));
			}
			while ((double)vector3.Length > (double)radius);
			return vector3;
		}
	}
}
