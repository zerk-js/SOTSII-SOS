// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.FleetTemplate
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.StarFleet;
using System.Collections.Generic;

namespace Kerberos.Sots.Strategy
{
	internal class FleetTemplate
	{
		public List<MissionType> MissionTypes = new List<MissionType>();
		public List<ShipInclude> ShipIncludes = new List<ShipInclude>();
		public Dictionary<AIStance, int> StanceWeights = new Dictionary<AIStance, int>();
		public Dictionary<AIStance, int> MinFleetsForStance = new Dictionary<AIStance, int>();
		public List<string> AllowableFactions = new List<string>();
		public int TemplateID;
		public string Name;
		public string FleetName;
		public bool Initial;

		public override string ToString()
		{
			return this.Name;
		}

		public bool CanFactionUse(string factionName)
		{
			if (this.AllowableFactions.Count == 0)
				return true;
			return this.AllowableFactions.Contains(factionName);
		}
	}
}
