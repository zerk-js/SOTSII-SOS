// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.ShipCombatInfo
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;

namespace Kerberos.Sots.Combat
{
	internal class ShipCombatInfo
	{
		public ShipInfo shipInfo;
		public float trackingFireFactor;
		public float directFireFactor;
		public float armorFactor;
		public float structureFactor;
		public float pdFactor;
		public int battleRiders;
		public int drones;
		public float bombFactorPopulation;
		public float bombFactorInfrastructure;
		public float bombFactorHazard;
		public bool shipDead;
	}
}
