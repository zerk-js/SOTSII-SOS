﻿// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.MeteorShowerGlobalData
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots
{
	public class MeteorShowerGlobalData
	{
		public CombatAIDamageData[] Damage = new CombatAIDamageData[3];
		public int[] ResourceBonuses = new int[3];
		public int MinMeteors;
		public int MaxMeteors;
		public int LargeMeteorChance;
		public int NumBreakoffMeteors;

		public enum MeteorSizes
		{
			Small,
			Medium,
			Large,
			NumMeteors,
		}
	}
}
