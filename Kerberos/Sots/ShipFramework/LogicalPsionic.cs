// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.ShipFramework.LogicalPsionic
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ShipFramework;

namespace Kerberos.Sots.ShipFramework
{
	internal class LogicalPsionic
	{
		public string Name = "";
		public string Model = "";
		public string PsionicTitle = "";
		public string Description = "";
		public string Icon = "";
		public SectionEnumerations.PsionicAbility Ability;
		public int MinPower;
		public int MaxPower;
		public int BaseCost;
		public float Range;
		public float BaseDamage;
		public LogicalEffect CastorEffect;
		public LogicalEffect CastEffect;
		public LogicalEffect ApplyEffect;
		public string RequiredTechID;
		public bool RequiresSuulka;

		public bool IsAvailable(GameDatabase db, int playerId, bool forSuulka)
		{
			if (this.Ability == SectionEnumerations.PsionicAbility.AbaddonLaser || !forSuulka && this.RequiresSuulka)
				return false;
			if (!string.IsNullOrEmpty(this.RequiredTechID))
			{
				PlayerTechInfo playerTechInfo = db.GetPlayerTechInfo(playerId, db.GetTechID(this.RequiredTechID));
				if (playerTechInfo == null || playerTechInfo.State != TechStates.Researched)
					return false;
			}
			return true;
		}
	}
}
