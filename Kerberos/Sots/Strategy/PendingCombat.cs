// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.PendingCombat
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System.Collections.Generic;

namespace Kerberos.Sots.Strategy
{
	internal class PendingCombat
	{
		public Dictionary<int, ResolutionType> CombatResolutionSelections = new Dictionary<int, ResolutionType>();
		public Dictionary<int, AutoResolveStance> CombatStanceSelections = new Dictionary<int, AutoResolveStance>();
		public Dictionary<int, int> SelectedPlayerFleets = new Dictionary<int, int>();
		public int ConflictID;
		public int SystemID;
		public List<int> FleetIDs;
		public CombatType Type;
		public PostCombatData CombatResults;
		public List<int> PlayersInCombat;
		public List<int> NPCPlayersInCombat;
		public int CardID;
		public bool complete;

		public PendingCombat()
		{
			this.FleetIDs = new List<int>();
			this.NPCPlayersInCombat = new List<int>();
		}
	}
}
