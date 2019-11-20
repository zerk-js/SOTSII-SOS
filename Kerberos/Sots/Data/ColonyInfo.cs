// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ColonyInfo
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Strategy;
using System;

namespace Kerberos.Sots.Data
{
	internal class ColonyInfo : IIDProvider
	{
		public ColonyFactionInfo[] Factions = new ColonyFactionInfo[0];
		public int PlayerID;
		public int OrbitalObjectID;
		public double ImperialPop;
		public float CivilianWeight;
		public int TurnEstablished;
		public float TerraRate;
		public float InfraRate;
		public float ShipConRate;
		public float TradeRate;
		public float OverharvestRate;
		public float EconomyRating;
		public ColonyStage CurrentStage;
		public float OverdevelopProgress;
		public float OverdevelopRate;
		public float SlaveWorkRate;
		public float PopulationBiosphereRate;
		public bool isHardenedStructures;
		public RebellionType RebellionType;
		public int RebellionTurns;
		public int TurnsOverharvested;
		public int RepairPoints;
		public bool DamagedLastTurn;
		public int RepairPointsMax;
		public bool OwningColony;
		public bool ReplicantsOn;
		public int CachedStarSystemID;

		public int ID { get; set; }

		private static int GetEconomicChange(ColonyInfo.EconomicChangeReason reason)
		{
			switch (reason)
			{
				case ColonyInfo.EconomicChangeReason.FreighterKilled:
					return -1;
				case ColonyInfo.EconomicChangeReason.SpoiledGoods:
					return -1;
				case ColonyInfo.EconomicChangeReason.CombatInfrastructureLoss2Points:
					return -1;
				case ColonyInfo.EconomicChangeReason.EnemyInSystem:
					return -3;
				case ColonyInfo.EconomicChangeReason.EnemyInProvince:
					return -2;
				case ColonyInfo.EconomicChangeReason.EmpireInDebt500K:
					return -1;
				case ColonyInfo.EconomicChangeReason.EmpireInDebt1Mil:
					return -1;
				case ColonyInfo.EconomicChangeReason.EmpireInDebt2Mil:
					return -1;
				case ColonyInfo.EconomicChangeReason.MoraleUnder20:
					return -1;
				case ColonyInfo.EconomicChangeReason.PopulationReductionOver50K:
					return -1;
				case ColonyInfo.EconomicChangeReason.Savings500K:
					return 1;
				case ColonyInfo.EconomicChangeReason.Savings1Mil:
					return 1;
				case ColonyInfo.EconomicChangeReason.Savings3Mil:
					return 1;
				case ColonyInfo.EconomicChangeReason.Stimulus300K:
					return 1;
				case ColonyInfo.EconomicChangeReason.ShipsBuiltWithTradeMaxed:
					return 1;
				case ColonyInfo.EconomicChangeReason.MoraleMin80:
					return 1;
				case ColonyInfo.EconomicChangeReason.ConstructionOrUpgradeInSystem:
					return 2;
				case ColonyInfo.EconomicChangeReason.PopulationGrowthOver30K:
					return 1;
				default:
					return 0;
			}
		}

		public void ModifyEconomyRating(
		  GameDatabase db,
		  ColonyInfo.EconomicChangeReason reason,
		  int multiplier)
		{
			multiplier = Math.Max(multiplier, 0);
			bool flag = false;
			if (this.OrbitalObjectID != 0)
				flag = db.GetStarSystemInfo(db.GetOrbitalObjectInfo(this.OrbitalObjectID).StarSystemID).IsOpen;
			else
				App.Log.Warn("ColonyInfo.ModifyEconomyRating: OrbitalObjectID==0", "game");
			int num = ColonyInfo.GetEconomicChange(reason) * multiplier;
			if (!flag)
				num = Math.Max(num - 1, 0);
			float economyRating = this.EconomyRating;
			this.EconomyRating += (float)num * 0.01f;
			this.EconomyRating = Math.Max(this.EconomyRating, 0.0f);
			if (App.Log.Level < Kerberos.Sots.Engine.LogLevel.Verbose)
				return;
			App.Log.Trace(string.Format("ColonyInfo.ModifyEconomyRating pid={0},id={1},x{2},{3},{4}->{5}: {6}", (object)this.PlayerID, (object)this.ID, (object)multiplier, flag ? (object)"open" : (object)"closed", (object)(economyRating * 100f).ToString("N0"), (object)(this.EconomyRating * 100f).ToString("N0"), (object)reason.ToString()), "game", Kerberos.Sots.Engine.LogLevel.Verbose);
		}

		public bool IsIndependentColony(App _app)
		{
			PlayerInfo playerInfo = _app.GameDatabase.GetPlayerInfo(this.PlayerID);
			return _app.AssetDatabase.GetFaction(playerInfo.FactionID).IsIndependent();
		}

		public bool IsIndependentColony(GameSession _app)
		{
			PlayerInfo playerInfo = _app.GameDatabase.GetPlayerInfo(this.PlayerID);
			return _app.AssetDatabase.GetFaction(playerInfo.FactionID).IsIndependent();
		}

		public enum EconomicChangeReason
		{
			FreighterKilled,
			SpoiledGoods,
			CombatInfrastructureLoss2Points,
			EnemyInSystem,
			EnemyInProvince,
			EmpireInDebt500K,
			EmpireInDebt1Mil,
			EmpireInDebt2Mil,
			MoraleUnder20,
			PopulationReductionOver50K,
			Savings500K,
			Savings1Mil,
			Savings3Mil,
			Stimulus300K,
			ShipsBuiltWithTradeMaxed,
			MoraleMin80,
			ConstructionOrUpgradeInSystem,
			PopulationGrowthOver30K,
		}
	}
}
