﻿// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Framework.Generators
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System.Collections.Generic;

namespace Kerberos.Sots.Framework
{
	internal static class Generators
	{
		public static IEnumerable<int> Sequence(int first, int count, int step)
		{
			int value = first;
			for (int i = 0; i < count; ++i)
			{
				yield return value;
				value += step;
			}
		}
	}
}
