// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.PiracyGlobalData
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System.Collections.Generic;

namespace Kerberos.Sots
{
	public class PiracyGlobalData
	{
		public int[] Bounties = new int[4];
		public Dictionary<string, int> ReactionBonuses = new Dictionary<string, int>();
		public float PiracyBaseOdds;
		public float PiracyBaseMod;
		public float PiracyModPolice;
		public float PiracyModNavalBase;
		public float PiracyModNoNavalBase;
		public float PiracyModZuulProximity;
		public float PiracyMinZuulProximity;
		public int PiracyMinShips;
		public int PiracyMaxShips;
		public int PiracyMinBaseShips;
		public int PiracyTotalMaxShips;
		public int PiracyBaseRange;
		public int PiracyBaseShipBonus;
		public int PiracyBaseTurnShipBonus;
		public int PiracyBaseTurnsPerUpdate;

		public enum PiracyBountyType
		{
			PirateBaseDestroyed,
			PirateShipDestroyed,
			FreighterDestroyed,
			FreighterCaptured,
			MaxBountyTypes,
		}
	}
}
