// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Framework.Range`1
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;

namespace Kerberos.Sots.Framework
{
	public struct Range<T> where T : IComparable<T>
	{
		public T Min { get; private set; }

		public T Max { get; private set; }

		public unsafe Range(T min, T max)
		{
			this = default;
			if (min.CompareTo(max) <= 0)
			{
				this.Min = min;
				this.Max = max;
			}
			else
			{
				this.Min = max;
				this.Max = min;
			}
		}
	}
}
