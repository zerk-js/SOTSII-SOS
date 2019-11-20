// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.StrategicTaskComparision
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System.Collections.Generic;

namespace Kerberos.Sots.Strategy
{
	internal class StrategicTaskComparision : IComparer<StrategicTask>
	{
		public int Compare(StrategicTask alpha, StrategicTask beta)
		{
			if (alpha == beta)
				return 0;
			if (alpha.Score == beta.Score)
				return alpha.Mission < beta.Mission || alpha.Mission <= beta.Mission && alpha.SubScore > beta.SubScore ? -1 : 1;
			if (alpha.Score > beta.Score)
				return -1;
			return alpha.Score < beta.Score ? 1 : 0;
		}
	}
}
