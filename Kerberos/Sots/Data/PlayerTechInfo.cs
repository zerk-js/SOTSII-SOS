// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.PlayerTechInfo
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.Data
{
	internal class PlayerTechInfo
	{
		public int PlayerID;
		public int TechID;
		public string TechFileID;
		public TechStates State;
		public int Progress;
		public int ResearchCost;
		public float Feasibility;
		public float PlayerFeasibility;
		public int? TurnResearched;

		public PlayerTechInfo.PrimaryKey ID
		{
			get
			{
				return new PlayerTechInfo.PrimaryKey()
				{
					PlayerID = this.PlayerID,
					TechID = this.TechID
				};
			}
		}

		public override string ToString()
		{
			return this.TechFileID ?? string.Empty;
		}

		public struct PrimaryKey
		{
			public int PlayerID;
			public int TechID;
		}
	}
}
