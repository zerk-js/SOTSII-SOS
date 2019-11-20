// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.Budget
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Strategy
{
	internal class Budget
	{
		public readonly double CurrentSavings;
		public readonly double SavingsInterest;
		public readonly double DebtInterest;
		public readonly double TradeRevenue;
		public readonly double TaxRevenue;
		public readonly double IORevenue;
		public readonly double CurrentShipUpkeepExpenses;
		public readonly double CurrentStationUpkeepExpenses;
		public readonly double AdditionalUpkeepExpenses;
		public readonly double ColonySupportExpenses;
		public readonly double CorruptionExpenses;
		public readonly int RequiredSecurity;
		public readonly double TotalRevenue;
		public readonly double UpkeepExpenses;
		public readonly double TotalExpenses;
		public readonly double OperatingBudget;
		public readonly double DisposableIncome;
		public readonly double NetSavingsLoss;
		public readonly double RequestedGovernmentSpending;
		public readonly ResearchSpending ResearchSpending;
		public readonly SecuritySpending SecuritySpending;
		public readonly StimulusSpending StimulusSpending;
		public readonly double ProjectedGovernmentSpending;
		public readonly double UnspentIncome;
		public readonly double NetSavingsIncome;
		public readonly double ProjectedSavings;
		public readonly double PendingBuildShipsCost;
		public readonly double PendingBuildStationsCost;
		public readonly double PendingStationsModulesCost;
		public readonly double TotalBuildShipCosts;
		public readonly double TotalBuildStationsCost;
		public readonly double TotalStationsModulesCost;

		private Budget(
		  GameDatabase gamedb,
		  AssetDatabase assetdb,
		  GameSession game,
		  PlayerInfo playerInfo,
		  FactionInfo playerFactionInfo,
		  double maxDriveSpeed,
		  double incomeFromTrade,
		  SpendingCaps spendingCaps,
		  IEnumerable<ColonyInfo> colonyInfos,
		  Dictionary<int, PlanetInfo> planetInfos,
		  Dictionary<int, OrbitalObjectInfo> orbitalObjectInfos,
		  Dictionary<int, StarSystemInfo> starSystemInfos,
		  HashSet<int> starSystemsWithGates,
		  IEnumerable<StationInfo> stationInfos,
		  IEnumerable<DesignInfo> reserveShipDesignInfos,
		  IEnumerable<DesignInfo> shipDesignInfos,
		  IEnumerable<DesignInfo> eliteShipDesignInfos,
		  IEnumerable<DesignInfo> additionalShipDesignInfos)
		{
			this.CurrentSavings = playerInfo.Savings;
			this.SavingsInterest = playerFactionInfo.Name != "loa" ? GameSession.CalculateSavingsInterest(game, playerInfo) : 0.0;
			this.DebtInterest = playerFactionInfo.Name != "loa" ? GameSession.CalculateDebtInterest(game, playerInfo) : 0.0;
			this.TradeRevenue = incomeFromTrade;
			this.TaxRevenue = colonyInfos.Sum<ColonyInfo>((Func<ColonyInfo, double>)(x => Colony.GetTaxRevenue(game.App, playerInfo, x)));
			float num1 = gamedb.GetNameValue<float>("EconomicEfficiency") / 100f;
			this.TradeRevenue *= (double)num1;
			this.TradeRevenue *= (double)game.GameDatabase.GetStratModifierFloatToApply(StratModifiers.TradeRevenue, playerInfo.ID);
			this.TaxRevenue *= (double)num1;
			Player playerObject1 = game.GetPlayerObject(playerInfo.ID);
			if (playerObject1 == null || !playerObject1.IsAI())
			{
				this.CurrentStationUpkeepExpenses = GameSession.CalculateStationUpkeepCosts(gamedb, assetdb, stationInfos);
				this.CurrentShipUpkeepExpenses = GameSession.CalculateFleetUpkeepCosts(assetdb, reserveShipDesignInfos, shipDesignInfos, eliteShipDesignInfos);
				this.AdditionalUpkeepExpenses = GameSession.CalculateShipUpkeepCosts(assetdb, additionalShipDesignInfos, 1f, false);
			}
			this.ColonySupportExpenses = colonyInfos.Sum<ColonyInfo>((Func<ColonyInfo, double>)(x =>
		   {
			   OrbitalObjectInfo orbitalObjectInfo = orbitalObjectInfos[x.OrbitalObjectID];
			   PlanetInfo planetInfo = planetInfos[x.OrbitalObjectID];
			   StarSystemInfo starSystemInfo = starSystemInfos[orbitalObjectInfo.StarSystemID];
			   return Colony.GetColonySupportCost(gamedb, assetdb, playerInfo, playerFactionInfo, orbitalObjectInfos[x.OrbitalObjectID], planetInfos[x.OrbitalObjectID], starSystemInfos[orbitalObjectInfos[x.OrbitalObjectID].StarSystemID], orbitalObjectInfos, planetInfos, starSystemInfos, starSystemsWithGates.Contains(orbitalObjectInfos[x.OrbitalObjectID].StarSystemID), maxDriveSpeed);
		   }));
			this.IORevenue = 0.0;
			List<int> list1 = gamedb.GetPlayerColonySystemIDs(playerInfo.ID).ToList<int>();
			int num2 = list1.Where<int>((Func<int, bool>)(x => gamedb.GetStarSystemInfo(x).IsOpen)).Count<int>();
			Player playerObject2 = game.GetPlayerObject(playerInfo.ID);
			float num3 = 0.0f;
			foreach (int num4 in list1)
			{
				List<BuildOrderInfo> list2 = gamedb.GetBuildOrdersForSystem(num4).ToList<BuildOrderInfo>();
				float num5 = 0.0f;
				foreach (ColonyInfo colony in gamedb.GetColonyInfosForSystem(num4).ToList<ColonyInfo>())
				{
					if (colony.PlayerID == playerInfo.ID)
						num5 += Colony.GetConstructionPoints(game, colony);
				}
				float num6 = num5 * game.GetStationBuildModifierForSystem(num4, playerInfo.ID);
				foreach (BuildOrderInfo buildOrderInfo in list2)
				{
					DesignInfo designInfo = gamedb.GetDesignInfo(buildOrderInfo.DesignID);
					if (designInfo.PlayerID == playerInfo.ID)
					{
						int num7 = designInfo.SavingsCost;
						if (designInfo.IsLoaCube())
							num7 = buildOrderInfo.LoaCubes * assetdb.LoaCostPerCube;
						int num8 = buildOrderInfo.ProductionTarget - buildOrderInfo.Progress;
						float num9 = 0.0f;
						if (!designInfo.isPrototyped)
						{
							num9 = (float)(int)((double)num6 * ((double)gamedb.GetStratModifierFloatToApply(StratModifiers.PrototypeTimeModifier, playerInfo.ID) - 1.0));
							switch (designInfo.Class)
							{
								case ShipClass.Cruiser:
									num7 = (int)((double)designInfo.SavingsCost * (double)gamedb.GetStratModifierFloatToApply(StratModifiers.PrototypeSavingsCostModifierCR, playerInfo.ID));
									break;
								case ShipClass.Dreadnought:
									num7 = (int)((double)designInfo.SavingsCost * (double)gamedb.GetStratModifierFloatToApply(StratModifiers.PrototypeSavingsCostModifierDN, playerInfo.ID));
									break;
								case ShipClass.Leviathan:
									num7 = (int)((double)designInfo.SavingsCost * (double)gamedb.GetStratModifierFloatToApply(StratModifiers.PrototypeSavingsCostModifierLV, playerInfo.ID));
									break;
								case ShipClass.Station:
									RealShipClasses? realShipClass = designInfo.GetRealShipClass();
									if ((realShipClass.GetValueOrDefault() != RealShipClasses.Platform ? 0 : (realShipClass.HasValue ? 1 : 0)) != 0)
									{
										num7 = (int)((double)designInfo.SavingsCost * (double)gamedb.GetStratModifierFloatToApply(StratModifiers.PrototypeSavingsCostModifierPF, playerInfo.ID));
										break;
									}
									break;
							}
						}
						if (playerInfo.isStandardPlayer && playerObject2.IsAI() && playerObject2.Faction.Name == "loa")
							num7 = (int)((double)num7 * 1.0);
						if ((double)num8 <= (double)num6 - (double)num9)
						{
							num3 += (float)num7;
							num6 -= (float)num8;
						}
						this.TotalBuildShipCosts += (double)num7;
					}
				}
				this.IORevenue += (double)num6;
			}
			this.IORevenue *= (double)game.GameDatabase.GetStratModifierFloatToApply(StratModifiers.IORevenue, playerInfo.ID);
			this.PendingBuildShipsCost = (double)num3;
			foreach (MissionInfo missionInfo in game.GameDatabase.GetMissionInfos().Where<MissionInfo>((Func<MissionInfo, bool>)(x => x.Type == MissionType.CONSTRUCT_STN)))
			{
				FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(missionInfo.FleetID);
				if (fleetInfo.PlayerID == playerInfo.ID)
				{
					MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(game, missionInfo.Type, (StationType)missionInfo.StationType.Value, fleetInfo.ID, missionInfo.TargetSystemID, missionInfo.TargetOrbitalObjectID, (List<int>)null, 1, false, new float?(), new float?());
					this.TotalBuildStationsCost += (double)missionEstimate.ConstructionCost;
					if (missionEstimate.TotalTurns - 1 - missionEstimate.TurnsToReturn <= 1)
						this.PendingBuildStationsCost += (double)missionEstimate.ConstructionCost;
				}
			}
			foreach (MissionInfo missionInfo in game.GameDatabase.GetMissionInfos().Where<MissionInfo>((Func<MissionInfo, bool>)(x =>
		   {
			   if (x.Type == MissionType.UPGRADE_STN)
				   return x.Duration > 0;
			   return false;
		   })))
			{
				FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(missionInfo.FleetID);
				if (game.GameDatabase.GetStationInfo(missionInfo.TargetOrbitalObjectID) != null && fleetInfo.PlayerID == playerInfo.ID && game.GameDatabase.GetWaypointsByMissionID(missionInfo.ID).First<WaypointInfo>().Type != WaypointType.ReturnHome)
				{
					StationInfo stationInfo = game.GameDatabase.GetStationInfo(missionInfo.TargetOrbitalObjectID);
					if (stationInfo.DesignInfo.StationLevel + 1 <= 5)
					{
						MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(game, missionInfo.Type, stationInfo.DesignInfo.StationType, fleetInfo.ID, missionInfo.TargetSystemID, missionInfo.TargetOrbitalObjectID, (List<int>)null, stationInfo.DesignInfo.StationLevel + 1, false, new float?(), new float?());
						DesignInfo stationDesignInfo = DesignLab.CreateStationDesignInfo(game.AssetDatabase, game.GameDatabase, fleetInfo.PlayerID, stationInfo.DesignInfo.StationType, stationInfo.DesignInfo.StationLevel + 1, false);
						this.TotalBuildStationsCost += (double)stationDesignInfo.SavingsCost;
						if (missionEstimate.TotalTurns - 1 - missionEstimate.TurnsToReturn <= 1)
							this.PendingBuildStationsCost += (double)stationDesignInfo.SavingsCost;
					}
				}
			}
			foreach (StationInfo stationInfo in game.GameDatabase.GetStationInfosByPlayerID(playerInfo.ID))
			{
				List<DesignModuleInfo> queuedModules = game.GameDatabase.GetQueuedStationModules(stationInfo.DesignInfo.DesignSections[0]).ToList<DesignModuleInfo>();
				if (queuedModules.Count > 0)
				{
					LogicalModule logicalModule1 = game.AssetDatabase.Modules.First<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == game.GameDatabase.GetModuleAsset(queuedModules.First<DesignModuleInfo>().ModuleID)));
					if (logicalModule1 != null)
						this.PendingStationsModulesCost += (double)logicalModule1.SavingsCost;
					foreach (DesignModuleInfo designModuleInfo in queuedModules)
					{
						DesignModuleInfo dmi = designModuleInfo;
						LogicalModule logicalModule2 = game.AssetDatabase.Modules.First<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == game.GameDatabase.GetModuleAsset(dmi.ModuleID)));
						if (logicalModule2 != null)
							this.TotalStationsModulesCost += (double)logicalModule2.SavingsCost;
					}
				}
			}
			this.TotalRevenue = this.TradeRevenue + this.TaxRevenue + this.IORevenue + this.SavingsInterest;
			this.RequiredSecurity = (int)Math.Ceiling(((double)num2 / (double)list1.Count != 0.0 ? ((double)assetdb.BaseCorruptionRate + 0.0199999995529652 * ((double)playerInfo.RateImmigration * 10.0)) / (2.0 * (double)playerInfo.RateGovernmentResearch) : 0.0) * 100.0);
			if (playerFactionInfo.Name == "loa")
				this.RequiredSecurity = 0;
			this.CorruptionExpenses = this.TotalRevenue * (double)Math.Max(0.0f, (float)(((double)assetdb.BaseCorruptionRate + 0.0199999995529652 * ((double)playerInfo.RateImmigration * 10.0) - 2.0 * ((double)playerInfo.RateGovernmentResearch * (double)playerInfo.RateGovernmentSecurity)) * ((double)num2 / (double)list1.Count)));
			if (playerFactionInfo.Name == "loa")
				this.CorruptionExpenses = 0.0;
			this.UpkeepExpenses = this.CurrentShipUpkeepExpenses + this.CurrentStationUpkeepExpenses + this.AdditionalUpkeepExpenses;
			this.TotalExpenses = this.UpkeepExpenses + this.ColonySupportExpenses + this.DebtInterest + this.CorruptionExpenses;
			this.OperatingBudget = this.TotalRevenue - this.TotalExpenses;
			this.DisposableIncome = Math.Max(this.OperatingBudget, 0.0);
			this.NetSavingsLoss = Math.Max(-this.OperatingBudget, 0.0) + this.PendingBuildShipsCost + this.PendingBuildStationsCost + this.PendingStationsModulesCost;
			this.RequestedGovernmentSpending = this.DisposableIncome * (double)playerInfo.RateGovernmentResearch;
			SpendingPool pool = new SpendingPool();
			this.ResearchSpending = new ResearchSpending(playerInfo, this.DisposableIncome - this.RequestedGovernmentSpending, pool, spendingCaps);
			this.SecuritySpending = new SecuritySpending(playerInfo, this.RequestedGovernmentSpending * (double)playerInfo.RateGovernmentSecurity, pool, spendingCaps);
			this.StimulusSpending = new StimulusSpending(playerInfo, this.RequestedGovernmentSpending * (double)playerInfo.RateGovernmentStimulus, pool, spendingCaps);
			this.ProjectedGovernmentSpending = this.SecuritySpending.ProjectedTotal + this.StimulusSpending.ProjectedTotal;
			this.UnspentIncome = pool.Excess;
			this.NetSavingsIncome = this.DisposableIncome - this.ResearchSpending.RequestedTotal - this.SecuritySpending.RequestedTotal - this.StimulusSpending.RequestedTotal + this.UnspentIncome;
			this.ProjectedSavings = this.CurrentSavings + this.NetSavingsIncome - this.NetSavingsLoss;
		}

		public static Budget GenerateBudget(
		  GameSession sim,
		  PlayerInfo playerInfo,
		  IEnumerable<DesignInfo> additionalShipDesignInfos,
		  BudgetProjection budgetProjection)
		{
			FactionInfo factionInfo = sim.GameDatabase.GetFactionInfo(playerInfo.FactionID);
			double driveSpeedForPlayer = (double)sim.GameDatabase.FindCurrentDriveSpeedForPlayer(playerInfo.ID);
			double playerIncomeFromTrade = sim.GetPlayerIncomeFromTrade(playerInfo.ID);
			SpendingCaps spendingCaps = new SpendingCaps(sim.GameDatabase, playerInfo, budgetProjection);
			List<StationInfo> list1 = sim.GameDatabase.GetStationInfosByPlayerID(playerInfo.ID).ToList<StationInfo>();
			List<ColonyInfo> list2 = sim.GameDatabase.GetPlayerColoniesByPlayerId(playerInfo.ID).ToList<ColonyInfo>();
			foreach (TreatyInfo treatyInfo in sim.GameDatabase.GetTreatyInfos().ToList<TreatyInfo>().Where<TreatyInfo>((Func<TreatyInfo, bool>)(x =>
		   {
			   if (x.Type == TreatyType.Protectorate)
				   return x.InitiatingPlayerId == playerInfo.ID;
			   return false;
		   })).ToList<TreatyInfo>())
			{
				if (treatyInfo.Active)
					list2.AddRange(sim.GameDatabase.GetPlayerColoniesByPlayerId(treatyInfo.ReceivingPlayerId));
			}
			Dictionary<int, PlanetInfo> infoMapForColonies1 = Colony.GetPlanetInfoMapForColonies(sim.GameDatabase, (IEnumerable<ColonyInfo>)list2);
			Dictionary<int, OrbitalObjectInfo> infoMapForColonies2 = Colony.GetOrbitalObjectInfoMapForColonies(sim.GameDatabase, (IEnumerable<ColonyInfo>)list2);
			Dictionary<int, StarSystemInfo> forOrbitalObjects = Colony.GetStarSystemInfoMapForOrbitalObjects(sim.GameDatabase, (IEnumerable<OrbitalObjectInfo>)infoMapForColonies2.Values);
			HashSet<int> systemsWithGates = GameSession.GetStarSystemsWithGates(sim.GameDatabase, playerInfo.ID, (IEnumerable<int>)forOrbitalObjects.Keys);
			List<FleetInfo> list3 = sim.GameDatabase.GetFleetInfosByPlayerID(playerInfo.ID, FleetType.FL_ALL).ToList<FleetInfo>();
			List<FleetInfo> eliteFleetInfos = list3.Where<FleetInfo>((Func<FleetInfo, bool>)(x => sim.GameDatabase.GetAdmiralTraits(x.AdmiralID).Contains<AdmiralInfo.TraitType>(AdmiralInfo.TraitType.Elite))).ToList<FleetInfo>();
			if (eliteFleetInfos.Count > 0)
				list3.RemoveAll((Predicate<FleetInfo>)(x => eliteFleetInfos.Contains(x)));
			List<DesignInfo> designInfoList1 = GameSession.MergeShipDesignInfos(sim.GameDatabase, (IEnumerable<FleetInfo>)list3, false);
			List<DesignInfo> designInfoList2 = GameSession.MergeShipDesignInfos(sim.GameDatabase, (IEnumerable<FleetInfo>)eliteFleetInfos, false);
			List<DesignInfo> designInfoList3 = GameSession.MergeShipDesignInfos(sim.GameDatabase, (IEnumerable<FleetInfo>)list3, true);
			return new Budget(sim.GameDatabase, sim.AssetDatabase, sim, playerInfo, factionInfo, driveSpeedForPlayer, playerIncomeFromTrade, spendingCaps, (IEnumerable<ColonyInfo>)list2, infoMapForColonies1, infoMapForColonies2, forOrbitalObjects, systemsWithGates, (IEnumerable<StationInfo>)list1, (IEnumerable<DesignInfo>)designInfoList3, (IEnumerable<DesignInfo>)designInfoList1, (IEnumerable<DesignInfo>)designInfoList2, additionalShipDesignInfos);
		}
	}
}
