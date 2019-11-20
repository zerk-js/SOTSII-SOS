// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.ShipInclude
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.Strategy
{
	internal class ShipInclude
	{
		public ShipInclusionType InclusionType { get; set; }

		public int Amount { get; set; }

		public ShipRole ShipRole { get; set; }

		public Kerberos.Sots.Strategy.WeaponRole? WeaponRole { get; set; }

		public string Faction { get; set; }

		public string FactionExclusion { get; set; }
	}
}
