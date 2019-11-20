// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.SystemDefendInfoComparision
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System.Collections.Generic;

namespace Kerberos.Sots.Strategy
{
	internal class SystemDefendInfoComparision : IComparer<SystemDefendInfo>
	{
		public int Compare(SystemDefendInfo a, SystemDefendInfo b)
		{
			if (a == b)
				return 0;
			if (a.IsHomeWorld && !b.IsHomeWorld)
				return -1;
			if (b.IsHomeWorld && !a.IsHomeWorld)
				return 1;
			if (a.IsCapital && !b.IsCapital)
				return -1;
			if (b.IsCapital && !a.IsCapital)
				return 1;
			if ((double)a.ProductionRate > (double)b.ProductionRate)
				return -1;
			if ((double)a.ProductionRate < (double)b.ProductionRate)
				return 1;
			if (a.NumColonies > b.NumColonies)
				return -1;
			return a.NumColonies < b.NumColonies ? 1 : 0;
		}
	}
}
