// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Framework.WeightedChoices
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Framework
{
	public static class WeightedChoices
	{
		public static T Choose<T>(Random random, IEnumerable<Weighted<T>> weights)
		{
			if (!weights.Any<Weighted<T>>())
				throw new ArgumentException("Nothing to choose.");
			return WeightedChoices.Choose<T>(random.NextDouble(), weights);
		}

		public static T Choose<T>(double normalizedRoll, IEnumerable<Weighted<T>> weights)
		{
			if (!weights.Any<Weighted<T>>())
				throw new ArgumentException("Nothing to choose.");
			long num1 = 0;
			foreach (Weighted<T> weight in weights)
				num1 += (long)weight.Weight;
			long num2 = (long)Math.Ceiling(Math.Max(0.0, Math.Min(normalizedRoll, 1.0)) * (double)num1);
			long num3 = 0;
			foreach (Weighted<T> weight in weights)
			{
				num3 += (long)weight.Weight;
				if (num2 <= num3)
					return weight.Value;
			}
			return weights.Last<Weighted<T>>().Value;
		}
	}
}
