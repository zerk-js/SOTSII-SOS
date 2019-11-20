// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.PlayerBranchInfo
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.Data
{
	internal class PlayerBranchInfo
	{
		public int PlayerID;
		public int FromTechID;
		public int ToTechID;
		public int ResearchCost;
		public float Feasibility;

		public PlayerBranchInfo.PrimaryKey ID
		{
			get
			{
				return new PlayerBranchInfo.PrimaryKey()
				{
					FromTechID = this.FromTechID,
					ToTechID = this.ToTechID,
					PlayerID = this.PlayerID
				};
			}
		}

		public override string ToString()
		{
			return string.Format("${3} %{4}", (object)this.ResearchCost, (object)this.Feasibility);
		}

		public struct PrimaryKey
		{
			public int FromTechID;
			public int ToTechID;
			public int PlayerID;

			public override string ToString()
			{
				return string.Format("{0}: {1}->{2}", (object)this.PlayerID, (object)this.FromTechID, (object)this.ToTechID);
			}
		}
	}
}
