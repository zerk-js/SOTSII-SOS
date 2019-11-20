// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.PostCombatData
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using System.Collections.Generic;

namespace Kerberos.Sots.Strategy
{
	internal class PostCombatData
	{
		public List<string> AdditionalInfo = new List<string>();
		public List<FleetInfo> FleetsInCombat = new List<FleetInfo>();
		public Dictionary<ShipInfo, int> FleetDamageTable = new Dictionary<ShipInfo, int>();
		public Dictionary<int, Dictionary<int, float>> WeaponDamageTable = new Dictionary<int, Dictionary<int, float>>();
		public List<DestroyedShip> DestroyedShips = new List<DestroyedShip>();
		public List<int> PlayersInCombat = new List<int>();
		public int? SystemId;
	}
}
