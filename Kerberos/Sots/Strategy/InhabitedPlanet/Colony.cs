// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.InhabitedPlanet.Colony
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Framework;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Steam;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Strategy.InhabitedPlanet
{
	internal class Colony
	{
		public const float INDSYS_IMPERIAL_POPULATION_MOD = 0.2f;
		public const long BLEEDPOP_MIN_REMAINING = 10000;
		public const float POPULATION_GROWTH_MOD = 1f;
		public const float POPULATION_GROWTH_EXP = 2f;

		public static float GetConstructionPoints(GameSession sim, ColonyInfo colony)
		{
			if (GameSession.InstaBuildHackEnabled)
				return 1E+09f;
			return (float)Colony.GetIndustrialOutput(sim, colony, sim.GameDatabase.GetPlanetInfo(colony.OrbitalObjectID)) * colony.ShipConRate;
		}

		public static double GetTaxRevenue(App game, ColonyInfo colony)
		{
			PlayerInfo playerInfo = game.GameDatabase.GetPlayerInfo(colony.PlayerID);
			return Colony.GetTaxRevenue(game, playerInfo, colony);
		}

		public static double GetTaxRevenue(App game, PlayerInfo player, ColonyInfo colony)
		{
			double num1 = (double)player.RateTax != 0.0 ? (double)player.RateTax * 100.0 / (double)game.AssetDatabase.TaxDivider : 0.0;
			double num2 = 0.0;
			bool flag = game.AssetDatabase.GetFaction(player.FactionID).HasSlaves();
			if (colony.RebellionType != RebellionType.None)
				return num2;
			double num3 = num2 + colony.ImperialPop * num1;
			foreach (ColonyFactionInfo faction in colony.Factions)
			{
				if (!flag || faction.FactionID == player.FactionID)
				{
					double num4 = faction.CivilianPop * num1 * 2.0;
					if (faction.FactionID != player.FactionID)
						num4 *= (double)game.GameDatabase.GetStratModifierFloatToApply(StratModifiers.AlienCivilianTaxRate, player.ID);
					num3 += num4;
				}
			}
			float num5 = game.GameDatabase.GetStratModifierFloatToApply(StratModifiers.AIRevenueBonus, player.ID) * game.GameDatabase.GetStratModifierFloatToApply(StratModifiers.AIBenefitBonus, player.ID) + (game.GameDatabase.GetStratModifierFloatToApply(StratModifiers.TaxRevenue, player.ID) - 1f) + game.AssetDatabase.GetAIModifier(game, DifficultyModifiers.RevenueBonus, player.ID);
			double num6 = num3 + num3 * (double)num5;
			if (!game.GameDatabase.GetStarSystemInfo(colony.CachedStarSystemID).IsOpen)
				num6 *= 0.899999976158142;
			HomeworldInfo playerHomeworld = game.GameDatabase.GetPlayerHomeworld(colony.PlayerID);
			if (playerHomeworld != null && playerHomeworld.ColonyID == colony.ID)
				num6 *= (double)game.AssetDatabase.HomeworldTaxBonusMod;
			return num6;
		}

		public static double CalcOverharvestRate(
		  AssetDatabase assetdb,
		  GameDatabase gamedb,
		  ColonyInfo colony,
		  PlanetInfo planet)
		{
			double val2 = Math.Min((double)planet.MaxResources * ((double)assetdb.MaxOverharvestRate * (double)gamedb.GetStratModifier<float>(StratModifiers.StripMiningMaximum, colony.PlayerID)), (double)planet.Resources);
			double val1 = 0.0;
			if ((double)colony.OverharvestRate > 0.0)
				val1 = Math.Max(1.0, (double)Math.Max(gamedb.GetStratModifier<float>(StratModifiers.MinOverharvestRate, colony.PlayerID), colony.OverharvestRate) * val2 * Math.Max(Math.Min(gamedb.GetTotalPopulation(colony) * 1E-05, 1.0), 0.0));
			double num = Math.Min(val1, val2);
			if (assetdb.GetFaction(gamedb.GetPlayerInfo(colony.PlayerID).FactionID).Name == "zuul" && val2 >= 5.0 && num < 5.0)
				num = 5.0;
			return num;
		}

		private static double CalcOverharvestFromOverpopulation(
		  GameDatabase db,
		  ColonyInfo colony,
		  PlanetInfo planet)
		{
			Faction faction = db.AssetDatabase.GetFaction(db.GetPlayerFactionID(colony.PlayerID));
			double num1 = 0.0;
			double maxCivilianPop = Colony.GetMaxCivilianPop(db, db.GetPlanetInfo(colony.OrbitalObjectID));
			double num2 = (double)db.GetStratModifierFloatToApply(StratModifiers.OverPopulationPercentage, colony.PlayerID) * maxCivilianPop;
			double civilianPopulation = db.GetCivilianPopulation(colony.OrbitalObjectID, faction.ID, faction.HasSlaves());
			if (civilianPopulation > num2)
			{
				double num3 = Math.Max(Math.Min((civilianPopulation - num2) / (maxCivilianPop - num2), 1.0), 0.0);
				double val2 = (double)db.GetStratModifierFloatToApply(StratModifiers.OverharvestFromPopulationModifier, colony.PlayerID) * num3;
				num1 = Math.Min((double)planet.Resources, val2);
			}
			return num1;
		}

		public static bool CanBeOverdeveloped(
		  AssetDatabase assetdb,
		  GameDatabase gamedb,
		  ColonyInfo colony,
		  PlanetInfo planet)
		{
			if (colony.CurrentStage == Kerberos.Sots.Data.ColonyStage.Developed && (double)planet.Size >= (double)assetdb.SuperWorldSizeConstraint)
				return gamedb.GetStratModifier<bool>(StratModifiers.AllowSuperWorlds, colony.PlayerID);
			return false;
		}

		public static double GetCivilianIndustrialOutput(GameSession sim, ColonyInfo colony)
		{
			double num1 = 0.0;
			foreach (ColonyFactionInfo faction in colony.Factions)
			{
				double num2 = 1.0;
				if (faction.Morale > 80)
					num2 = 1.5;
				else if (faction.Morale < 20)
					num2 = 0.5;
				num1 += faction.CivilianPop / 1000000.0 * num2 * (double)sim.AssetDatabase.CivilianProductionMultiplier;
			}
			return num1;
		}

		public static double GetSlaveIndustrialOutput(GameSession sim, ColonyInfo colony)
		{
			double num1 = 0.0;
			foreach (ColonyFactionInfo faction in colony.Factions)
			{
				if (faction.FactionID != sim.GameDatabase.GetPlayerFactionID(colony.PlayerID))
				{
					num1 += faction.CivilianPop / 1000000.0 * (double)sim.AssetDatabase.SlaveProductionMultiplier * (1.0 + (double)colony.SlaveWorkRate);
				}
				else
				{
					double num2 = 1.0;
					if (faction.Morale > 80)
						num2 = 1.5;
					else if (faction.Morale < 20)
						num2 = 0.5;
					num1 += faction.CivilianPop / 1000000.0 * num2 * (double)sim.AssetDatabase.CivilianProductionMultiplier;
				}
			}
			return num1;
		}

		public static double GetIndustrialOutput(GameSession sim, ColonyInfo colony, PlanetInfo planet)
		{
			if (colony.RebellionType != RebellionType.None)
				return 0.0;
			int resources = planet.Resources;
			int playerFactionId = sim.GameDatabase.GetPlayerFactionID(colony.PlayerID);
			bool flag = sim.AssetDatabase.GetFaction(playerFactionId).HasSlaves();
			double num1 = colony.ImperialPop / 1000000.0 * (double)sim.AssetDatabase.ImperialProductionMultiplier + (flag ? Colony.GetSlaveIndustrialOutput(sim, colony) : Colony.GetCivilianIndustrialOutput(sim, colony));
			if (num1 < 1.0)
				num1 = 1.0;
			double num2 = (double)planet.Infrastructure * (double)planet.Resources;
			double num3 = num1 + num2;
			OrbitalObjectInfo orbitalObjectInfo = sim.GameDatabase.GetOrbitalObjectInfo(planet.ID);
			int num4 = 0;
			foreach (StationInfo stationInfo in sim.GameDatabase.GetStationForSystem(orbitalObjectInfo.StarSystemID).Where<StationInfo>((Func<StationInfo, bool>)(x => x.DesignInfo.StationType == StationType.MINING)).ToList<StationInfo>())
				num4 += stationInfo.DesignInfo.StationLevel * sim.AssetDatabase.MiningStationIOBonus;
			int num5 = sim.GameDatabase.GetColonyInfosForSystem(orbitalObjectInfo.StarSystemID).Count<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == colony.PlayerID));
			if (num5 > 0)
				num3 += (double)(num4 / num5);
			double num6 = (num3 + Colony.CalcOverharvestRate(sim.AssetDatabase, sim.GameDatabase, colony, planet) * (double)sim.GameDatabase.GetStratModifierFloatToApply(StratModifiers.OverharvestModifier, colony.PlayerID)) * ((double)sim.GameDatabase.GetStratModifier<float>(StratModifiers.IndustrialOutputModifier, colony.PlayerID) * (double)sim.AssetDatabase.GlobalProductionModifier);
			float num7 = sim.GameDatabase.GetStratModifierFloatToApply(StratModifiers.AIProductionBonus, colony.PlayerID) * sim.GameDatabase.GetStratModifierFloatToApply(StratModifiers.AIBenefitBonus, colony.PlayerID) + sim.AssetDatabase.GetAIModifier(sim.App, DifficultyModifiers.ProductionBonus, colony.PlayerID);
			double num8 = num6 + num6 * (double)num7 + (!colony.isHardenedStructures || sim.GameDatabase.GetStarSystemInfo(colony.CachedStarSystemID).IsOpen ? 1.0 : 0.899999976158142);
			double num9 = colony.DamagedLastTurn ? num8 / 2.0 : num8;
			if (colony.CurrentStage == Kerberos.Sots.Data.ColonyStage.ForgeWorld)
				num9 *= (double)sim.AssetDatabase.ForgeWorldIOBonus;
			return num9;
		}

		public static float GetOverdevelopmentTarget(GameSession sim, PlanetInfo planet)
		{
			return (float)planet.Resources * planet.Size * sim.AssetDatabase.SuperWorldModifier;
		}

		public static float GetOverdevelopmentDelta(
		  GameSession sim,
		  ColonyInfo colony,
		  PlanetInfo planet)
		{
			return (float)Colony.GetIndustrialOutput(sim, colony, planet) * colony.OverdevelopRate;
		}

		public static float GetBiosphereDelta(
		  GameSession sim,
		  ColonyInfo colony,
		  PlanetInfo planet,
		  double terraformingBonus)
		{
			PlayerInfo playerInfo = sim.GameDatabase.GetPlayerInfo(colony.PlayerID);
			string factionName = sim.GameDatabase.GetFactionName(playerInfo.FactionID);
			float factionSuitability = sim.GameDatabase.GetFactionSuitability(factionName);
			float num1 = Math.Abs(planet.Suitability - factionSuitability);
			float num2 = Colony.GetTerraformingDelta(sim, colony, planet, terraformingBonus);
			if ((double)num2 > (double)num1)
				num2 = num1;
			float modifierFloatToApply = sim.GameDatabase.GetStratModifierFloatToApply(StratModifiers.BiosphereDestructionModifier, colony.PlayerID);
			if (sim.GameDatabase.GetGardenerInfos().Where<GardenerInfo>((Func<GardenerInfo, bool>)(x => x.IsGardener)).Select<GardenerInfo, FleetInfo>((Func<GardenerInfo, FleetInfo>)(x => sim.GameDatabase.GetFleetInfo(x.FleetId))).ToList<FleetInfo>().Any<FleetInfo>((Func<FleetInfo, bool>)(x =>
		 {
			 if (x != null)
				 return x.SystemID == colony.CachedStarSystemID;
			 return false;
		 })))
				modifierFloatToApply += sim.AssetDatabase.GlobalGardenerData.BiosphereDamage;
			return -((float)(int)((double)Math.Min((int)Math.Abs(num2) * 10, planet.Biosphere) * (double)modifierFloatToApply) + (float)(int)Math.Max(0.0, (((IEnumerable<ColonyFactionInfo>)colony.Factions).Sum<ColonyFactionInfo>((Func<ColonyFactionInfo, double>)(x => x.CivilianPop)) / Colony.GetMaxCivilianPop(sim.GameDatabase, planet) * (double)planet.Biosphere - (double)planet.Biosphere) / 10.0));
		}

		public static double GetInfrastructureDelta(
		  GameSession sim,
		  ColonyInfo colony,
		  PlanetInfo planet)
		{
			double val1 = Colony.GetIndustrialOutput(sim, colony, planet) / 50000.0 * (double)colony.InfraRate;
			if (!sim.GameDatabase.GetPlayerInfo(colony.PlayerID).isStandardPlayer)
				val1 += 0.02;
			return Math.Min(val1, 1.0 - (double)planet.Infrastructure);
		}

		public static float GetBiosphereBurnDelta(
		  GameSession sim,
		  ColonyInfo colony,
		  PlanetInfo planet,
		  FleetInfo fleet)
		{
			return (float)(Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(sim, fleet.ID) / 100);
		}

		public static float GetTerraformingDelta(
		  GameSession sim,
		  ColonyInfo colony,
		  PlanetInfo planet,
		  double terraformingSpace)
		{
			PlayerInfo playerInfo = sim.GameDatabase.GetPlayerInfo(colony.PlayerID);
			string factionName = sim.GameDatabase.GetFactionName(playerInfo.FactionID);
			float factionSuitability = sim.GameDatabase.GetFactionSuitability(factionName);
			float num1 = Math.Abs(planet.Suitability - factionSuitability);
			double industrialOutput = Colony.GetIndustrialOutput(sim, colony, planet);
			float modifierFloatToApply = sim.GameDatabase.GetStratModifierFloatToApply(StratModifiers.TerraformingModifier, colony.PlayerID);
			if (sim.GameDatabase.GetGardenerInfos().Where<GardenerInfo>((Func<GardenerInfo, bool>)(x => x.IsGardener)).Select<GardenerInfo, FleetInfo>((Func<GardenerInfo, FleetInfo>)(x => sim.GameDatabase.GetFleetInfo(x.FleetId))).ToList<FleetInfo>().Any<FleetInfo>((Func<FleetInfo, bool>)(x =>
		 {
			 if (x != null)
				 return x.SystemID == colony.CachedStarSystemID;
			 return false;
		 })))
				modifierFloatToApply += sim.AssetDatabase.GlobalGardenerData.Terrforming;
			float num2 = (float)(industrialOutput * 0.01) * colony.TerraRate * modifierFloatToApply;
			float num3 = 0.0f;
			StationInfo systemPlayerAndType = sim.GameDatabase.GetStationForSystemPlayerAndType(sim.GameDatabase.GetOrbitalObjectInfo(planet.ID).StarSystemID, colony.PlayerID, StationType.CIVILIAN);
			if (systemPlayerAndType != null)
			{
				foreach (DesignModuleInfo module in systemPlayerAndType.DesignInfo.DesignSections[0].Modules)
				{
					if (module.StationModuleType.Value == ModuleEnums.StationModuleType.Terraform)
						num3 += num2 * 0.25f;
				}
			}
			float num4 = num2 + (num3 + (float)terraformingSpace);
			if (!playerInfo.isStandardPlayer)
				num4 += 10f;
			if ((double)num4 > (double)num1)
				num4 = num1;
			if ((double)planet.Suitability > (double)factionSuitability)
				num4 *= -1f;
			return num4;
		}

		public static float GetShipConstResources(
		  GameSession sim,
		  ColonyInfo colony,
		  PlanetInfo planet)
		{
			return (float)Colony.GetIndustrialOutput(sim, colony, planet) * colony.ShipConRate;
		}

		public static double EstimateColonyDevelopmentCost(
		  GameSession game,
		  int planetId,
		  int playerId)
		{
			ColonyInfo colonyInfo = new ColonyInfo()
			{
				ID = 0,
				PlayerID = playerId,
				OrbitalObjectID = planetId,
				ImperialPop = 100.0,
				CivilianWeight = 0.0f,
				TurnEstablished = game.GameDatabase.GetTurnCount(),
				TerraRate = 0.5f,
				InfraRate = 0.5f,
				ShipConRate = 0.0f,
				TradeRate = 0.0f,
				DamagedLastTurn = false
			};
			return Colony.GetColonySupportCost(game.AssetDatabase, game.GameDatabase, colonyInfo);
		}

		public static double GetClimateHazard(FactionInfo factionInfo, PlanetInfo planetInfo)
		{
			return (double)Math.Abs(factionInfo.IdealSuitability - planetInfo.Suitability);
		}

		public static ColonyStage GetColonyStage(
		  GameDatabase gamedb,
		  int playerId,
		  double hazard)
		{
			if (hazard < 100.0)
				return ColonyStage.Open;
			return hazard < (double)(300 + gamedb.GetStratModifier<int>(StratModifiers.DomeStageModifier, playerId)) ? ColonyStage.Domed : ColonyStage.Underground;
		}

		public static double GetColonyStageModifier(GameDatabase gamedb, int playerId, double hazard)
		{
			switch (Colony.GetColonyStage(gamedb, playerId, hazard))
			{
				case ColonyStage.Open:
					return 1.0;
				case ColonyStage.Domed:
					return 2.0;
				default:
					return 3.0 + (double)gamedb.GetStratModifier<float>(StratModifiers.CavernDmodModifier, playerId);
			}
		}

		public static double GetColonyDistanceModifier(
		  double supportDistance,
		  double driveSpeed,
		  bool gatePresent)
		{
			if (gatePresent)
				return 1.0;
			driveSpeed = Math.Max(driveSpeed, 0.001);
			return Math.Max(supportDistance / driveSpeed, 1.0);
		}

		public static double GetColonySupportCost(
		  GameDatabase gamedb,
		  AssetDatabase assetdb,
		  PlayerInfo playerInfo,
		  FactionInfo playerFactionInfo,
		  OrbitalObjectInfo targetOrbitalObjectInfo,
		  PlanetInfo targetPlanetInfo,
		  StarSystemInfo targetStarSystemInfo,
		  Dictionary<int, OrbitalObjectInfo> playerOrbitalObjectInfos,
		  Dictionary<int, PlanetInfo> playerPlanetInfos,
		  Dictionary<int, StarSystemInfo> playerStarSystemInfos,
		  bool gateAtSystem,
		  double playerDriveSpeed)
		{
			if (playerFactionInfo.Name == "loa")
				return 0.0;
			double idealSuitability = (double)playerFactionInfo.IdealSuitability;
			double climateHazard = Colony.GetClimateHazard(playerFactionInfo, targetPlanetInfo);
			if (climateHazard == 0.0)
				return 0.0;
			double? distanceForColony = Colony.FindSupportDistanceForColony(playerInfo, playerFactionInfo, targetOrbitalObjectInfo, targetPlanetInfo, targetStarSystemInfo, playerOrbitalObjectInfos, playerPlanetInfos, playerStarSystemInfos);
			if (!distanceForColony.HasValue)
				return -1.0;
			double distanceModifier = Colony.GetColonyDistanceModifier(distanceForColony.Value, playerDriveSpeed, gateAtSystem);
			double colonyStageModifier = Colony.GetColonyStageModifier(gamedb, playerInfo.ID, climateHazard);
			return Math.Pow(climateHazard / 1.5, 1.89999997615814) * distanceModifier * colonyStageModifier * (double)assetdb.ColonySupportCostFactor;
		}

		public static double GetColonySupportCost(
		  AssetDatabase assetdb,
		  GameDatabase db,
		  ColonyInfo colonyInfo)
		{
			PlayerInfo playerInfo = db.GetPlayerInfo(colonyInfo.PlayerID);
			OrbitalObjectInfo orbitalObjectInfo = db.GetOrbitalObjectInfo(colonyInfo.OrbitalObjectID);
			PlanetInfo planetInfo = db.GetPlanetInfo(colonyInfo.OrbitalObjectID);
			StarSystemInfo starSystemInfo = db.GetStarSystemInfo(orbitalObjectInfo.StarSystemID);
			return Colony.GetColonySupportCost(assetdb, db, playerInfo, orbitalObjectInfo, planetInfo, starSystemInfo);
		}

		public static double GetColonySupportCost(
		  AssetDatabase assetdb,
		  GameDatabase db,
		  PlayerInfo playerInfo,
		  OrbitalObjectInfo targetOrbitalObjectInfo,
		  PlanetInfo targetPlanetInfo,
		  StarSystemInfo targetStarSystemInfo)
		{
			FactionInfo factionInfo = db.GetFactionInfo(playerInfo.FactionID);
			IEnumerable<ColonyInfo> coloniesByPlayerId = db.GetPlayerColoniesByPlayerId(playerInfo.ID);
			Dictionary<int, PlanetInfo> infoMapForColonies1 = Colony.GetPlanetInfoMapForColonies(db, coloniesByPlayerId);
			Dictionary<int, OrbitalObjectInfo> infoMapForColonies2 = Colony.GetOrbitalObjectInfoMapForColonies(db, coloniesByPlayerId);
			Dictionary<int, StarSystemInfo> forOrbitalObjects = Colony.GetStarSystemInfoMapForOrbitalObjects(db, (IEnumerable<OrbitalObjectInfo>)infoMapForColonies2.Values);
			bool gateAtSystem = db.SystemHasGate(targetStarSystemInfo.ID, playerInfo.ID);
			double driveSpeedForPlayer = (double)db.FindCurrentDriveSpeedForPlayer(playerInfo.ID);
			return Colony.GetColonySupportCost(db, assetdb, playerInfo, factionInfo, targetOrbitalObjectInfo, targetPlanetInfo, targetStarSystemInfo, infoMapForColonies2, infoMapForColonies1, forOrbitalObjects, gateAtSystem, driveSpeedForPlayer) * (double)db.GetStratModifier<float>(StratModifiers.ColonySupportCostModifier, playerInfo.ID);
		}

		public static double GetColonySupportCost(GameDatabase db, int playerId, int planetId)
		{
			PlayerInfo playerInfo = db.GetPlayerInfo(playerId);
			OrbitalObjectInfo orbitalObjectInfo = db.GetOrbitalObjectInfo(planetId);
			PlanetInfo planetInfo = db.GetPlanetInfo(planetId);
			StarSystemInfo starSystemInfo = db.GetStarSystemInfo(orbitalObjectInfo.StarSystemID);
			return Colony.GetColonySupportCost(db.AssetDatabase, db, playerInfo, orbitalObjectInfo, planetInfo, starSystemInfo);
		}

		private static bool IsColonyDeveloped(FactionInfo playerFactionInfo, PlanetInfo planetInfo)
		{
			if ((double)planetInfo.Suitability == (double)playerFactionInfo.IdealSuitability)
				return (double)planetInfo.Infrastructure >= 1.0;
			return false;
		}

		private static double? FindSupportDistanceForColony(
		  PlayerInfo playerInfo,
		  FactionInfo playerFactionInfo,
		  OrbitalObjectInfo targetOrbitalObjectInfo,
		  PlanetInfo targetPlanetInfo,
		  StarSystemInfo targetStarSystemInfo,
		  Dictionary<int, OrbitalObjectInfo> playerOrbitalObjectInfos,
		  Dictionary<int, PlanetInfo> playerPlanetInfos,
		  Dictionary<int, StarSystemInfo> playerStarSystemInfos)
		{
			if (Colony.IsColonyDeveloped(playerFactionInfo, targetPlanetInfo))
				return new double?(0.0);
			double? nullable1 = new double?();
			foreach (PlanetInfo planetInfo in playerPlanetInfos.Values)
			{
				if (Colony.IsColonyDeveloped(playerFactionInfo, planetInfo))
				{
					OrbitalObjectInfo orbitalObjectInfo = playerOrbitalObjectInfos[planetInfo.ID];
					StarSystemInfo playerStarSystemInfo = playerStarSystemInfos[orbitalObjectInfo.StarSystemID];
					double length = (double)(targetStarSystemInfo.Origin - playerStarSystemInfo.Origin).Length;
					if (nullable1.HasValue)
					{
						double num = length;
						double? nullable2 = nullable1;
						if ((num >= nullable2.GetValueOrDefault() ? 0 : (nullable2.HasValue ? 1 : 0)) == 0)
							continue;
					}
					nullable1 = new double?(length);
				}
			}
			return nullable1;
		}

		public static Dictionary<int, PlanetInfo> GetPlanetInfoMapForColonies(
		  GameDatabase db,
		  IEnumerable<ColonyInfo> colonyInfos)
		{
			Dictionary<int, PlanetInfo> dictionary = new Dictionary<int, PlanetInfo>();
			foreach (PlanetInfo planetInfo in colonyInfos.Select<ColonyInfo, PlanetInfo>((Func<ColonyInfo, PlanetInfo>)(x => db.GetPlanetInfo(x.OrbitalObjectID))).ToList<PlanetInfo>())
				dictionary[planetInfo.ID] = planetInfo;
			return dictionary;
		}

		public static Dictionary<int, OrbitalObjectInfo> GetOrbitalObjectInfoMapForColonies(
		  GameDatabase db,
		  IEnumerable<ColonyInfo> colonyInfos)
		{
			Dictionary<int, OrbitalObjectInfo> dictionary = new Dictionary<int, OrbitalObjectInfo>();
			foreach (OrbitalObjectInfo orbitalObjectInfo in colonyInfos.Select<ColonyInfo, OrbitalObjectInfo>((Func<ColonyInfo, OrbitalObjectInfo>)(x => db.GetOrbitalObjectInfo(x.OrbitalObjectID))).ToList<OrbitalObjectInfo>())
				dictionary[orbitalObjectInfo.ID] = orbitalObjectInfo;
			return dictionary;
		}

		public static Dictionary<int, StarSystemInfo> GetStarSystemInfoMapForOrbitalObjects(
		  GameDatabase db,
		  IEnumerable<OrbitalObjectInfo> orbitalObjectInfos)
		{
			Dictionary<int, StarSystemInfo> dictionary = new Dictionary<int, StarSystemInfo>();
			foreach (OrbitalObjectInfo orbitalObjectInfo in orbitalObjectInfos)
			{
				if (!dictionary.ContainsKey(orbitalObjectInfo.StarSystemID))
					dictionary[orbitalObjectInfo.StarSystemID] = db.GetStarSystemInfo(orbitalObjectInfo.StarSystemID);
			}
			return dictionary;
		}

		public static bool IsColonySelfSufficient(
		  GameSession sim,
		  ColonyInfo colony,
		  PlanetInfo planet)
		{
			if (!sim.GameDatabase.GetPlayerInfo(colony.PlayerID).isStandardPlayer)
				return true;
			ColonyInfo colony1 = new ColonyInfo()
			{
				OrbitalObjectID = colony.OrbitalObjectID,
				ImperialPop = colony.ImperialPop,
				CivilianWeight = colony.CivilianWeight,
				PlayerID = colony.PlayerID,
				DamagedLastTurn = false
			};
			colony1.CachedStarSystemID = sim.GameDatabase.GetOrbitalObjectInfo(colony1.OrbitalObjectID).StarSystemID;
			PlanetInfo planet1 = new PlanetInfo();
			planet1.Biosphere = planet.Biosphere;
			planet1.Size = planet.Size;
			planet1.Suitability = planet.Suitability;
			planet1.Resources = planet.Resources;
			planet1.Infrastructure = planet.Infrastructure;
			planet1.ID = planet.ID;
			List<PlagueInfo> plagues = new List<PlagueInfo>();
			List<ColonyFactionInfo> civPopulation;
			bool achievedSuperWorld;
			return Colony.MaintainColony(sim, ref colony1, ref planet1, ref plagues, 0.0, 0.0, (FleetInfo)null, out civPopulation, out achievedSuperWorld, true);
		}

		public static int SupportTripsTillSelfSufficient(
		  GameSession sim,
		  ColonyInfo colony,
		  PlanetInfo planet,
		  double colSpace,
		  double terSpace,
		  FleetInfo fleet)
		{
			if (sim.GameDatabase.GetPlayerFaction(fleet.PlayerID).Name == "loa" && sim.GetPlayerObject(fleet.PlayerID).IsAI())
				return 1;
			List<PlagueInfo> plagues = new List<PlagueInfo>();
			int num1 = Kerberos.Sots.StarFleet.StarFleet.GetNumColonizationShips(sim, fleet.ID);
			if (sim.GameDatabase.GetPlayerFaction(fleet.PlayerID).Name == "loa" && sim.GetPlayerObject(fleet.PlayerID).IsAI())
				num1 = 2;
			if (colSpace <= 0.0 || num1 <= 0)
				return -1;
			int num2 = 0;
			while (!Colony.IsColonySelfSufficient(sim, colony, planet))
			{
				++num2;
				List<ColonyFactionInfo> civPopulation;
				bool achievedSuperWorld;
				Colony.MaintainColony(sim, ref colony, ref planet, ref plagues, colSpace, terSpace, fleet, out civPopulation, out achievedSuperWorld, false);
				if (num2 > 50)
					return -1;
			}
			return num2;
		}

		public static bool IsColonyPhase1Complete(
		  GameSession sim,
		  ColonyInfo colony,
		  PlanetInfo planet)
		{
			if ((double)planet.Infrastructure >= 1.0 && colony.ImperialPop > 0.0)
			{
				PlayerInfo playerInfo = sim.GameDatabase.GetPlayerInfo(colony.PlayerID);
				string factionName = sim.GameDatabase.GetFactionName(playerInfo.FactionID);
				float factionSuitability = sim.GameDatabase.GetFactionSuitability(factionName);
				if ((double)planet.Suitability == (double)factionSuitability)
					return true;
			}
			return false;
		}

		public static int PredictTurnsToPhase1Completion(
		  GameSession sim,
		  ColonyInfo colony,
		  PlanetInfo planet)
		{
			List<PlagueInfo> plagues = new List<PlagueInfo>();
			if (!Colony.IsColonySelfSufficient(sim, colony, planet))
				return -1;
			int num = 0;
			while (!Colony.IsColonyPhase1Complete(sim, colony, planet))
			{
				List<ColonyFactionInfo> civPopulation;
				bool achievedSuperWorld;
				Colony.MaintainColony(sim, ref colony, ref planet, ref plagues, 0.0, 0.0, (FleetInfo)null, out civPopulation, out achievedSuperWorld, false);
				++num;
				if (num > 500)
					return -1;
			}
			return num;
		}

		public static void SetOutputRate(
		  GameDatabase gamedb,
		  AssetDatabase assetdb,
		  ref ColonyInfo colony,
		  PlanetInfo planet,
		  Colony.OutputRate rate,
		  float value)
		{
			Dictionary<Colony.OutputRate, float> ratios = new Dictionary<Colony.OutputRate, float>()
	  {
		{
		  Colony.OutputRate.Trade,
		  colony.TradeRate
		},
		{
		  Colony.OutputRate.Infra,
		  colony.InfraRate
		},
		{
		  Colony.OutputRate.ShipCon,
		  colony.ShipConRate
		},
		{
		  Colony.OutputRate.Terra,
		  colony.TerraRate
		},
		{
		  Colony.OutputRate.OverDev,
		  colony.OverdevelopRate
		}
	  };
			if ((double)planet.Infrastructure >= 1.0)
				ratios.Remove(Colony.OutputRate.Infra);
			if ((double)gamedb.GetPlanetHazardRating(colony.PlayerID, planet.ID, false) == 0.0)
				ratios.Remove(Colony.OutputRate.Terra);
			if (!Colony.CanBeOverdeveloped(assetdb, gamedb, colony, planet))
				ratios.Remove(Colony.OutputRate.OverDev);
			AlgorithmExtensions.DistributePercentages<Colony.OutputRate>(ref ratios, rate, value);
			colony.InfraRate = ratios.Keys.Contains<Colony.OutputRate>(Colony.OutputRate.Infra) ? ratios[Colony.OutputRate.Infra] : 0.0f;
			colony.ShipConRate = ratios.Keys.Contains<Colony.OutputRate>(Colony.OutputRate.ShipCon) ? ratios[Colony.OutputRate.ShipCon] : 0.0f;
			colony.TerraRate = ratios.Keys.Contains<Colony.OutputRate>(Colony.OutputRate.Terra) ? ratios[Colony.OutputRate.Terra] : 0.0f;
			colony.TradeRate = ratios.Keys.Contains<Colony.OutputRate>(Colony.OutputRate.Trade) ? ratios[Colony.OutputRate.Trade] : 0.0f;
			colony.OverdevelopRate = ratios.Keys.Contains<Colony.OutputRate>(Colony.OutputRate.OverDev) ? ratios[Colony.OutputRate.OverDev] : 0.0f;
		}

		public static double CalcSuitMod(FactionInfo faction, double suitability)
		{
			double val2 = 0.0;
			suitability = Math.Min(20.0, Math.Max(suitability, val2));
			double idealSuitability = (double)faction.IdealSuitability;
			double suitabilityTolerance = Player.GetSuitabilityTolerance();
			return Math.Min(Math.Abs(idealSuitability - suitability), suitabilityTolerance);
		}

		public static double CalcPopulationGrowth(
		  GameSession sim,
		  ColonyInfo colony,
		  double curPopulation,
		  double growthModifier,
		  int populationSupply,
		  FactionInfo populationFaction)
		{
			if (colony.DamagedLastTurn)
			{
				colony.DamagedLastTurn = false;
				return 0.0;
			}
			if (sim.GameDatabase.GetMissionInfos().Any<MissionInfo>((Func<MissionInfo, bool>)(x =>
		   {
			   if (x.TargetSystemID == colony.CachedStarSystemID)
				   return x.Type == MissionType.EVACUATE;
			   return false;
		   })))
				return 0.0;
			PlanetInfo planetInfo = sim.GameDatabase.GetPlanetInfo(colony.OrbitalObjectID);
			FactionInfo factionInfo = sim.GameDatabase.GetFactionInfo(sim.GameDatabase.GetPlayerFactionID(colony.PlayerID));
			double num1 = (double)Math.Abs(planetInfo.Suitability - populationFaction.IdealSuitability);
			double num2 = 1.0;
			if (sim.GameDatabase.GetStratModifier<bool>(StratModifiers.AllowIdealAlienGrowthRate, colony.PlayerID) && populationFaction.ID != sim.GameDatabase.GetPlayerFactionID(colony.PlayerID))
				num1 = 0.0;
			if (sim.GameDatabase.GetStratModifier<bool>(StratModifiers.RequiresSterileEnvironment, colony.PlayerID))
			{
				num1 = 0.0;
				num2 = (double)Colony.GetLoaGrowthPotential(sim, planetInfo.ID, colony.CachedStarSystemID, colony.PlayerID);
			}
			double num3 = (curPopulation * (num1 > 0.0 ? Math.Min(20.0 / num1, 1.0) : 1.0) + (double)populationSupply) * num2 * growthModifier;
			StarSystemInfo starSystemInfo = sim.GameDatabase.GetStarSystemInfo(colony.CachedStarSystemID);
			if (starSystemInfo != (StarSystemInfo)null)
			{
				if (starSystemInfo.IsOpen && populationFaction.ID == factionInfo.ID)
					num3 *= sim.AssetDatabase.GetGlobal<double>("OpenSystemNativeModifier");
				else if (starSystemInfo.IsOpen && populationFaction.ID != factionInfo.ID)
					num3 *= sim.AssetDatabase.GetGlobal<double>("OpenSystemAlienModifier");
			}
			float num4 = sim.GameDatabase.GetStratModifierFloatToApply(StratModifiers.PopulationGrowthModifier, colony.PlayerID) - 1f + sim.AssetDatabase.GetAIModifier(sim.App, DifficultyModifiers.PopulationGrowthBonus, colony.PlayerID);
			if (colony.ReplicantsOn && curPopulation < sim.AssetDatabase.GetGlobal<double>("ReplicantGrowthLimit"))
			{
				PlayerTechInfo playerTechInfo = sim.GameDatabase.GetPlayerTechInfos(colony.PlayerID).FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == "CYB_Replicant"));
				if (playerTechInfo != null && playerTechInfo.State == TechStates.Researched)
					num4 += sim.AssetDatabase.GetTechBonus<float>(playerTechInfo.TechFileID, "popgrowth_u100m");
			}
			double num5 = Math.Round(num3 + num3 * (double)num4, MidpointRounding.AwayFromZero);
			if (sim.AssetDatabase.GetFaction(sim.GameDatabase.GetPlayerInfo(colony.PlayerID).FactionID).Name == "loa" && populationFaction.Name == "loa")
			{
				float num6 = 1f;
				if (starSystemInfo != (StarSystemInfo)null)
					num6 = 1f / Math.Max((float)new StellarClass(starSystemInfo.StellarClass).GetInterference() - 4f, 1f);
				num5 *= (1.0 - (double)sim.GameDatabase.GetPlayerInfo(colony.PlayerID).RateTax * 10.0) * (double)num6;
			}
			return num5;
		}

		public static float GetLoaGrowthPotential(
		  GameSession sim,
		  int Planetid,
		  int starsystemid,
		  int playerid)
		{
			PlanetInfo planetInfo = sim.GameDatabase.GetPlanetInfo(Planetid);
			StarSystemInfo starSystemInfo = sim.GameDatabase.GetStarSystemInfo(starsystemid);
			double num1 = Math.Max(1.0 - (double)planetInfo.Biosphere / sim.AssetDatabase.GetGlobal<double>("SterilizationIndex"), 0.01);
			if (sim.AssetDatabase.GetFaction(sim.GameDatabase.GetPlayerInfo(playerid).FactionID).Name == "loa")
			{
				float num2 = 1f;
				if (starSystemInfo != (StarSystemInfo)null)
					num2 = 1f / Math.Max((float)new StellarClass(starSystemInfo.StellarClass).GetAverageInterference() - 4f, 1f);
				num1 *= (1.0 - (double)sim.GameDatabase.GetPlayerInfo(playerid).RateTax * 10.0) * (double)num2;
			}
			return (float)num1;
		}

		public static double DoPopulationChange(
		  GameSession sim,
		  double value,
		  ColonyInfo colony,
		  double maxPop,
		  double maxDelta,
		  double minDelta,
		  int populationSupply,
		  FactionInfo populationFaction,
		  double growthModifier = 1.0)
		{
			return Math.Max(0.0, sim.GameDatabase.GetPlayerInfo(colony.PlayerID).isAIRebellionPlayer ? 50000000.0 : Math.Min(value + Math.Min(Colony.CalcPopulationGrowth(sim, colony, value, growthModifier, populationSupply, populationFaction), maxDelta), maxPop));
		}

		public static double GetMaxImperialPop(GameDatabase gamedb, PlanetInfo pi)
		{
			double num = 0.0;
			ColonyInfo colonyInfoForPlanet = gamedb.GetColonyInfoForPlanet(pi.ID);
			if (colonyInfoForPlanet != null)
				num = (double)gamedb.GetStratModifier<int>(StratModifiers.AdditionalMaxImperialPopulation, colonyInfoForPlanet.PlayerID) * 1000000.0;
			return (double)pi.Size * 100000000.0 + num;
		}

		public static double GetMaxCivilianPop(GameDatabase gamedb, PlanetInfo pi)
		{
			double num = 0.0;
			ColonyInfo colonyInfoForPlanet = gamedb.GetColonyInfoForPlanet(pi.ID);
			if (colonyInfoForPlanet != null)
				num = (double)gamedb.GetStratModifier<int>(StratModifiers.AdditionalMaxCivilianPopulation, colonyInfoForPlanet.PlayerID) * 1000000.0;
			return (double)pi.Size * 200000000.0 + num;
		}

		public static double GetMaxSlavePop(GameDatabase gamedb, PlanetInfo pi)
		{
			return (double)pi.Size * 50000000.0;
		}

		public static double GetKilledSlavePopulation(
		  GameSession sim,
		  ColonyInfo colony,
		  double currentSlaves)
		{
			return Math.Floor(Math.Min(currentSlaves, Math.Max(1000.0, currentSlaves * ((double)sim.AssetDatabase.MinSlaveDeathRate + ((double)sim.AssetDatabase.MaxSlaveDeathRate - (double)sim.AssetDatabase.MinSlaveDeathRate) * (double)colony.SlaveWorkRate))));
		}

		public static void UpdatePopulation(
		  GameSession sim,
		  ref ColonyInfo colony,
		  int populationSupply,
		  ref List<ColonyFactionInfo> civPopulation,
		  bool useSlaveRules)
		{
			int playerFactionId = sim.GameDatabase.GetPlayerFactionID(colony.PlayerID);
			FactionInfo faction = sim.GameDatabase.GetFactionInfo(playerFactionId);
			PlanetInfo planetInfo = sim.GameDatabase.GetPlanetInfo(colony.OrbitalObjectID);
			if (colony.ImperialPop > 0.0)
				colony.ImperialPop = Colony.DoPopulationChange(sim, colony.ImperialPop, colony, colony.CurrentStage == Kerberos.Sots.Data.ColonyStage.ForgeWorld ? Colony.GetMaxImperialPop(sim.GameDatabase, planetInfo) * (double)sim.AssetDatabase.ForgeWorldImpMaxBonus : Colony.GetMaxImperialPop(sim.GameDatabase, planetInfo), 50000000.0, 50000000.0, populationSupply, faction, 1.0);
			PlayerInfo playerInfo1 = sim.GameDatabase.GetPlayerInfo(colony.PlayerID);
			if (civPopulation.Where<ColonyFactionInfo>((Func<ColonyFactionInfo, bool>)(x =>
		   {
			   if (useSlaveRules)
				   return x.FactionID == faction.ID;
			   return true;
		   })).Sum<ColonyFactionInfo>((Func<ColonyFactionInfo, double>)(y => y.CivilianPop)) == 0.0 && colony.ImperialPop > (double)sim.AssetDatabase.CivilianPopulationTriggerAmount && !playerInfo1.isAIRebellionPlayer)
			{
				if (!civPopulation.Any<ColonyFactionInfo>((Func<ColonyFactionInfo, bool>)(x => x.FactionID == faction.ID)))
					civPopulation.Add(new ColonyFactionInfo()
					{
						CivilianPop = (double)sim.AssetDatabase.CivilianPopulationStartAmount,
						FactionID = faction.ID,
						CivPopWeight = 1f,
						TurnEstablished = sim.GameDatabase.GetTurnCount(),
						OrbitalObjectID = planetInfo.ID,
						Morale = sim.AssetDatabase.CivilianPopulationStartMoral
					});
				else
					civPopulation.First<ColonyFactionInfo>((Func<ColonyFactionInfo, bool>)(x => x.FactionID == faction.ID)).CivilianPop = (double)sim.AssetDatabase.CivilianPopulationStartAmount;
				colony.ImperialPop -= (double)sim.AssetDatabase.CivilianPopulationStartAmount;
			}
			List<int> intList = new List<int>();
			foreach (PlayerInfo playerInfo2 in sim.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>())
			{
				if (!intList.Contains(playerInfo2.FactionID) && sim.AssetDatabase.GetFaction(playerInfo2.FactionID).Name != "zuul")
					intList.Add(playerInfo2.FactionID);
			}
			double val1_1 = (colony.CurrentStage == Kerberos.Sots.Data.ColonyStage.GemWorld ? Colony.GetMaxCivilianPop(sim.GameDatabase, planetInfo) * (double)sim.AssetDatabase.GemWorldCivMaxBonus : Colony.GetMaxCivilianPop(sim.GameDatabase, planetInfo)) * (double)colony.CivilianWeight;
			if (useSlaveRules)
			{
				foreach (ColonyFactionInfo colonyFactionInfo in civPopulation)
				{
					if (colonyFactionInfo.FactionID != faction.ID && colonyFactionInfo.CivilianPop > 0.0)
						colonyFactionInfo.CivilianPop -= Colony.GetKilledSlavePopulation(sim, colony, colonyFactionInfo.CivilianPop);
				}
				OrbitalObjectInfo orbitalObjectInfo = sim.GameDatabase.GetOrbitalObjectInfo(planetInfo.ID);
				StationInfo systemPlayerAndType = sim.GameDatabase.GetStationForSystemPlayerAndType(orbitalObjectInfo.StarSystemID, colony.PlayerID, StationType.CIVILIAN);
				if (systemPlayerAndType != null)
				{
					StarSystemInfo starSystemInfo1 = sim.GameDatabase.GetStarSystemInfo(orbitalObjectInfo.StarSystemID);
					float range = sim.GameDatabase.FindCurrentDriveSpeedForPlayer(colony.PlayerID) * 2f;
					List<StarSystemInfo> list = sim.GameDatabase.GetSystemsInRange(starSystemInfo1.Origin, range).ToList<StarSystemInfo>();
					List<SlaveNode> source = new List<SlaveNode>();
					foreach (StarSystemInfo starSystemInfo2 in list)
					{
						float distanceMod = (starSystemInfo2.Origin - starSystemInfo1.Origin).Length / range;
						if ((double)distanceMod != 0.0)
						{
							foreach (ColonyInfo colonyInfo in sim.GameDatabase.GetColonyInfosForSystem(starSystemInfo2.ID).ToList<ColonyInfo>())
							{
								int targetFactionId = sim.GameDatabase.GetPlayerFactionID(colonyInfo.PlayerID);
								if (targetFactionId != sim.GameDatabase.GetPlayerFactionID(colony.PlayerID) && !source.Any<SlaveNode>((Func<SlaveNode, bool>)(x =>
							   {
								   if ((double)x.DistanceMod == (double)distanceMod)
									   return x.FactionId == targetFactionId;
								   return false;
							   })))
									source.Add(new SlaveNode()
									{
										DistanceMod = distanceMod,
										FactionId = targetFactionId
									});
							}
						}
					}
					if (source.Count > 0)
					{
						SlaveNode chosenTarget = App.GetSafeRandom().Choose<SlaveNode>((IList<SlaveNode>)source);
						double num = Math.Min(Math.Floor((double)(systemPlayerAndType.DesignInfo.StationLevel * 50000) / (double)chosenTarget.DistanceMod), Math.Max(Math.Floor(val1_1 - civPopulation.Sum<ColonyFactionInfo>((Func<ColonyFactionInfo, double>)(x => x.CivilianPop))), 0.0));
						if (num > 0.0)
						{
							if (civPopulation.Any<ColonyFactionInfo>((Func<ColonyFactionInfo, bool>)(x => x.FactionID == chosenTarget.FactionId)))
								civPopulation.First<ColonyFactionInfo>((Func<ColonyFactionInfo, bool>)(x => x.FactionID == chosenTarget.FactionId)).CivilianPop += num;
							else
								civPopulation.Add(new ColonyFactionInfo()
								{
									FactionID = chosenTarget.FactionId,
									Morale = 100,
									OrbitalObjectID = colony.OrbitalObjectID,
									TurnEstablished = sim.GameDatabase.GetTurnCount(),
									CivPopWeight = 0.0f,
									CivilianPop = num
								});
						}
					}
				}
				double maxSlavePop = Colony.GetMaxSlavePop(sim.GameDatabase, planetInfo);
				if (sim.GameDatabase.GetSlavePopulation(planetInfo.ID, faction.ID) > maxSlavePop)
				{
					foreach (ColonyFactionInfo colonyFactionInfo in civPopulation)
					{
						if (colonyFactionInfo.FactionID == playerFactionId)
							colonyFactionInfo.CivilianPop = Math.Min(colonyFactionInfo.CivilianPop, maxSlavePop * (double)colonyFactionInfo.CivPopWeight);
					}
				}
			}
			else if (sim.GameDatabase.GetStratModifier<bool>(StratModifiers.AllowAlienImmigration, colony.PlayerID) && !playerInfo1.isAIRebellionPlayer && sim.GameDatabase.GetStarSystemInfo(sim.GameDatabase.GetOrbitalObjectInfo(colony.OrbitalObjectID).StarSystemID).IsOpen)
			{
				float num1 = sim.GameDatabase.GetPlayerInfo(colony.PlayerID).RateImmigration * 100f * sim.AssetDatabase.CitizensPerImmigrationPoint;
				foreach (int num2 in intList)
				{
					int i = num2;
					if (!(sim.AssetDatabase.GetFaction(i).Name == "zuul"))
					{
						if (!civPopulation.Any<ColonyFactionInfo>((Func<ColonyFactionInfo, bool>)(x => x.FactionID == i)))
							civPopulation.Add(new ColonyFactionInfo()
							{
								FactionID = i,
								CivilianPop = 0.0,
								CivPopWeight = 1f,
								TurnEstablished = sim.GameDatabase.GetTurnCount(),
								OrbitalObjectID = planetInfo.ID,
								Morale = sim.AssetDatabase.CivilianPopulationStartMoral
							});
						double val2 = Math.Floor(val1_1 - sim.GameDatabase.GetCivilianPopulations(colony.OrbitalObjectID).Sum<ColonyFactionInfo>((Func<ColonyFactionInfo, double>)(x => x.CivilianPop)));
						if (val2 > 0.0)
							civPopulation.First<ColonyFactionInfo>((Func<ColonyFactionInfo, bool>)(x => x.FactionID == i)).CivilianPop += Math.Min((double)num1 / (double)intList.Count, val2);
					}
				}
			}
			foreach (ColonyFactionInfo colonyFactionInfo in civPopulation)
			{
				if ((!useSlaveRules || colonyFactionInfo.FactionID == faction.ID) && colonyFactionInfo.CivilianPop > 0.0)
				{
					float num1 = colonyFactionInfo.CivPopWeight;
					if (useSlaveRules && colonyFactionInfo.FactionID != faction.ID)
						num1 = 1f;
					double val1_2 = colonyFactionInfo.CivilianPop;
					double num2 = Math.Max(Math.Floor(val1_1 - sim.GameDatabase.GetCivilianPopulations(colony.OrbitalObjectID).Sum<ColonyFactionInfo>((Func<ColonyFactionInfo, double>)(x => x.CivilianPop))), 0.0);
					if (num2 > 0.0)
						val1_2 = val1_1 * (double)num1 * (double)sim.AssetDatabase.GetFaction(playerFactionId).GetImmigrationPopBonusValueForFaction(sim.AssetDatabase.GetFaction(colonyFactionInfo.FactionID));
					double maxPop = Math.Min(Math.Max(val1_2, colonyFactionInfo.CivilianPop), Math.Min(val1_1, colonyFactionInfo.CivilianPop + num2));
					double num3 = Colony.DoPopulationChange(sim, colonyFactionInfo.CivilianPop, colony, maxPop, 20000000.0, 50000000.0, 0, sim.GameDatabase.GetFactionInfo(colonyFactionInfo.FactionID), (double)sim.AssetDatabase.CivilianPopulationGrowthRateMod);
					colonyFactionInfo.CivilianPop = num3;
				}
			}
		}

		public static int CalcColonyRepairPoints(GameSession sim, ColonyInfo colony)
		{
			return (int)(colony.ImperialPop / 100000000.0 * (0.200000002980232 * Colony.GetIndustrialOutput(sim, colony, sim.GameDatabase.GetPlanetInfo(colony.OrbitalObjectID))) * (double)sim.GetStationRepairModifierForSystem(sim.GameDatabase.GetOrbitalObjectInfo(colony.OrbitalObjectID).StarSystemID, colony.PlayerID));
		}

		public static void SimulatePlagues(
		  GameSession sim,
		  ref ColonyInfo ci,
		  ref PlanetInfo pi,
		  ref List<PlagueInfo> plagues)
		{
			Random safeRandom = App.GetSafeRandom();
			double totalPopulation = sim.GameDatabase.GetTotalPopulation(ci);
			foreach (PlagueInfo plagueInfo in plagues)
			{
				if (!sim.AssetDatabase.GetFaction(sim.GameDatabase.GetPlayerFactionID(ci.PlayerID)).IsPlagueImmune(plagueInfo.PlagueType))
				{
					double num1 = 0.0;
					double num2 = 0.0;
					float num3 = 0.0f;
					float minValue = 0.0f;
					float maxValue = 0.0f;
					float num4 = 0.0f;
					bool flag1 = false;
					switch (plagueInfo.PlagueType)
					{
						case WeaponEnums.PlagueType.BASIC:
							num4 = 0.0f;
							minValue = 0.2f;
							maxValue = 0.4f;
							flag1 = false;
							break;
						case WeaponEnums.PlagueType.RETRO:
							num4 = 0.0f;
							minValue = 0.1f;
							maxValue = 0.3f;
							flag1 = false;
							break;
						case WeaponEnums.PlagueType.BEAST:
							num4 = 0.0f;
							minValue = 0.05f;
							maxValue = 0.2f;
							flag1 = true;
							break;
						case WeaponEnums.PlagueType.ASSIM:
							num4 = -0.1f;
							minValue = 0.0f;
							maxValue = 0.0f;
							flag1 = true;
							break;
						case WeaponEnums.PlagueType.NANO:
							++plagueInfo.InfectionRate;
							pi.Infrastructure = Math.Max(plagueInfo.InfectionRate - plagueInfo.InfectionRate, 0.0f);
							if (safeRandom.CoinToss(0.200000002980232))
							{
								plagueInfo.InfectedPopulationImperial = 0.0;
								plagueInfo.InfectedPopulationCivilian = 0.0;
								continue;
							}
							continue;
						case WeaponEnums.PlagueType.XOMBIE:
							num4 = -0.1f;
							minValue = 0.0f;
							maxValue = 0.0f;
							flag1 = true;
							break;
						case WeaponEnums.PlagueType.ZUUL:
							num4 = 0.0f;
							minValue = 0.0f;
							maxValue = 0.0f;
							flag1 = false;
							break;
					}
					plagueInfo.InfectionRate = Math.Max(plagueInfo.InfectionRate - num4, 1f);
					plagueInfo.InfectedPopulationCivilian *= (double)plagueInfo.InfectionRate;
					plagueInfo.InfectedPopulationImperial *= (double)plagueInfo.InfectionRate;
					float num5 = safeRandom.NextInclusive(minValue, maxValue);
					double num6 = plagueInfo.InfectedPopulationCivilian * (double)num5;
					double num7 = plagueInfo.InfectedPopulationImperial * (double)num5;
					plagueInfo.InfectedPopulationCivilian = Math.Max(plagueInfo.InfectedPopulationCivilian - num6, 0.0);
					plagueInfo.InfectedPopulationImperial = Math.Max(plagueInfo.InfectedPopulationImperial - num7, 0.0);
					double num8 = num2 + num6;
					double num9 = num1 + num7;
					bool flag2 = false;
					if (flag1)
					{
						switch (plagueInfo.PlagueType)
						{
							case WeaponEnums.PlagueType.BEAST:
								double num10 = plagueInfo.InfectedPopulation * 3.0;
								double num11 = num8 + num10 * 0.949999988079071;
								double num12 = num9 + num10 * 0.0500000007450581;
								double num13 = Math.Floor((totalPopulation - num11 - num12 - plagueInfo.InfectedPopulation) / 100000.0);
								plagueInfo.InfectedPopulationCivilian = Math.Max(plagueInfo.InfectedPopulationCivilian - Math.Floor(num13 / 2.0), 0.0);
								plagueInfo.InfectedPopulationImperial = Math.Max(plagueInfo.InfectedPopulationImperial - Math.Floor(num13 / 2.0), 0.0);
								num8 = num11 + Math.Floor(num13 / 2.0);
								num9 = num12 + Math.Floor(num13 / 2.0);
								num3 += safeRandom.NextInclusive(1f, 3f) * (float)Math.Ceiling(plagueInfo.InfectedPopulation / 100000.0);
								break;
							case WeaponEnums.PlagueType.ASSIM:
								double num14 = plagueInfo.InfectedPopulation * 2.0;
								double num15 = num8 + num14 * 0.75;
								double num16 = num9 + num14 * 0.25;
								double num17 = Math.Floor((totalPopulation - num15 - num16 - plagueInfo.InfectedPopulation) / 50000.0);
								plagueInfo.InfectedPopulationCivilian = Math.Max(plagueInfo.InfectedPopulationCivilian - Math.Floor(num17 / 2.0), 0.0);
								plagueInfo.InfectedPopulationImperial = Math.Max(plagueInfo.InfectedPopulationImperial - Math.Floor(num17 / 2.0), 0.0);
								num8 = num15 + Math.Floor(num17 / 2.0);
								num9 = num16 + Math.Floor(num17 / 2.0);
								num3 += safeRandom.NextInclusive(1f, 2f) * (float)Math.Ceiling(plagueInfo.InfectedPopulation / 500000.0);
								if (plagueInfo.InfectedPopulationCivilian > 0.0 && (plagueInfo.InfectedPopulationCivilian > ((IEnumerable<ColonyFactionInfo>)ci.Factions).Sum<ColonyFactionInfo>((Func<ColonyFactionInfo, double>)(x => x.CivilianPop)) - num8 && plagueInfo.InfectedPopulationImperial > ci.ImperialPop - num9))
								{
									sim.GameDatabase.InsertTurnEvent(new TurnEvent()
									{
										EventType = TurnEventType.EV_ASSIMILATION_PLAGUE_PLANET_GAINED,
										EventMessage = TurnEventMessage.EM_ASSIMILATION_PLAGUE_PLANET_GAINED,
										ColonyID = ci.ID,
										PlayerID = plagueInfo.LaunchingPlayer,
										TurnNumber = sim.GameDatabase.GetTurnCount(),
										ShowsDialog = false
									});
									sim.GameDatabase.InsertTurnEvent(new TurnEvent()
									{
										EventType = TurnEventType.EV_ASSIMILATION_PLAGUE_PLANET_LOST,
										EventMessage = TurnEventMessage.EM_ASSIMILATION_PLAGUE_PLANET_LOST,
										ColonyID = ci.ID,
										PlayerID = ci.PlayerID,
										TargetPlayerID = plagueInfo.LaunchingPlayer,
										TurnNumber = sim.GameDatabase.GetTurnCount(),
										ShowsDialog = false
									});
									sim.GameDatabase.GetPlayerFactionID(ci.PlayerID);
									((IEnumerable<ColonyFactionInfo>)ci.Factions).First<ColonyFactionInfo>().CivilianPop += ci.ImperialPop;
									ci.ImperialPop = 1000.0;
									ci.PlayerID = plagueInfo.LaunchingPlayer;
									num8 = 0.0;
									num9 = 0.0;
									num3 = 0.0f;
									flag2 = true;
									using (List<StationInfo>.Enumerator enumerator = sim.GameDatabase.GetStationForSystem(ci.CachedStarSystemID).ToList<StationInfo>().GetEnumerator())
									{
										while (enumerator.MoveNext())
										{
											StationInfo current = enumerator.Current;
											OrbitalObjectInfo orbitalObjectInfo = sim.GameDatabase.GetOrbitalObjectInfo(current.OrbitalObjectID);
											if (orbitalObjectInfo != null)
											{
												int? parentId = orbitalObjectInfo.ParentID;
												int orbitalObjectId = ci.OrbitalObjectID;
												if ((parentId.GetValueOrDefault() != orbitalObjectId ? 0 : (parentId.HasValue ? 1 : 0)) != 0)
													sim.GameDatabase.DestroyStation(sim, current.ID, 0);
											}
										}
										break;
									}
								}
								else
									break;
							case WeaponEnums.PlagueType.XOMBIE:
								double num18 = plagueInfo.InfectedPopulation * 10.0;
								plagueInfo.InfectedPopulationCivilian += num18 * 0.75;
								plagueInfo.InfectedPopulationImperial += num18 * 0.25;
								double num19 = Math.Floor((totalPopulation - num8 - num9 - plagueInfo.InfectedPopulation) / 100000.0);
								plagueInfo.InfectedPopulationCivilian = Math.Max(plagueInfo.InfectedPopulationCivilian - Math.Floor(num19 / 2.0), 0.0);
								plagueInfo.InfectedPopulationImperial = Math.Max(plagueInfo.InfectedPopulationImperial - Math.Floor(num19 / 2.0), 0.0);
								num8 += Math.Floor(num19 / 2.0);
								num9 += Math.Floor(num19 / 2.0);
								num3 += safeRandom.NextInclusive(1f, 5f) * (float)Math.Ceiling(plagueInfo.InfectedPopulation / 50000.0);
								break;
						}
					}
					ci.ImperialPop = Math.Max(ci.ImperialPop - num9, 0.0);
					double num20 = ((IEnumerable<ColonyFactionInfo>)ci.Factions).Sum<ColonyFactionInfo>((Func<ColonyFactionInfo, double>)(x => x.CivilianPop));
					if (num20 > 0.0)
					{
						foreach (ColonyFactionInfo faction in ci.Factions)
							faction.CivilianPop = Math.Max(faction.CivilianPop - num8 * (faction.CivilianPop / num20), 0.0);
					}
					double val2 = ((IEnumerable<ColonyFactionInfo>)ci.Factions).Sum<ColonyFactionInfo>((Func<ColonyFactionInfo, double>)(x => x.CivilianPop));
					pi.Infrastructure = Math.Max(pi.Infrastructure - num3, 0.0f);
					plagueInfo.InfectedPopulationImperial = Math.Min(plagueInfo.InfectedPopulationImperial, ci.ImperialPop);
					plagueInfo.InfectedPopulationCivilian = Math.Min(plagueInfo.InfectedPopulationCivilian, val2);
					plagueInfo.InfectionRate += num4;
					if (!flag2)
					{
						TurnEventMessage turnEventMessage = num9 == 0.0 && num8 == 0.0 || (double)num3 == 0.0 ? (num9 != 0.0 || num8 != 0.0 ? TurnEventMessage.EM_PLAGUE_DAMAGE_POP : TurnEventMessage.EM_PLAGUE_DAMAGE_STRUCT) : TurnEventMessage.EM_PLAGUE_DAMAGE_POP_STRUCT;
						sim.GameDatabase.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_PLAGUE_DAMAGE,
							EventMessage = turnEventMessage,
							PlagueType = plagueInfo.PlagueType,
							ImperialPop = (float)num9,
							CivilianPop = (float)num8,
							ColonyID = plagueInfo.ColonyId,
							PlayerID = ci.PlayerID,
							TurnNumber = sim.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
					}
				}
			}
		}

		public static bool MaintainColony(
		  GameSession sim,
		  ref ColonyInfo colony,
		  ref PlanetInfo planet,
		  ref List<PlagueInfo> plagues,
		  double fleetCapacity,
		  double terraformingBonus,
		  FleetInfo fleet,
		  out List<ColonyFactionInfo> civPopulation,
		  out bool achievedSuperWorld,
		  bool isSupplyRun = false)
		{
			OrbitalObjectInfo orbitalObjectInfo = sim.GameDatabase.GetOrbitalObjectInfo(planet.ID);
			if (!isSupplyRun && (double)colony.OverharvestRate > (double)sim.GameDatabase.GetStratModifier<float>(StratModifiers.MinOverharvestRate, colony.PlayerID))
			{
				if (colony.TurnsOverharvested > 5)
				{
					sim.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_OVERHARVEST_WARNING,
						EventMessage = TurnEventMessage.EM_OVERHARVEST_WARNING,
						PlayerID = colony.PlayerID,
						SystemID = orbitalObjectInfo.StarSystemID,
						OrbitalID = colony.OrbitalObjectID,
						ColonyID = colony.ID,
						TurnNumber = sim.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
					colony.TurnsOverharvested = 0;
				}
				else
					++colony.TurnsOverharvested;
			}
			Random safeRandom = App.GetSafeRandom();
			achievedSuperWorld = false;
			bool flag1 = false;
			civPopulation = sim.GameDatabase.GetCivilianPopulations(planet.ID).ToList<ColonyFactionInfo>();
			PlayerInfo playerInfo1 = sim.GameDatabase.GetPlayerInfo(colony.PlayerID);
			bool flag2 = sim.AssetDatabase.GetFaction(playerInfo1.FactionID).HasSlaves();
			if (!playerInfo1.isAIRebellionPlayer)
			{
				Colony.SimulatePlagues(sim, ref colony, ref planet, ref plagues);
				civPopulation = ((IEnumerable<ColonyFactionInfo>)colony.Factions).ToList<ColonyFactionInfo>();
				if (colony.RebellionType != RebellionType.None)
				{
					double civilianPopulation = sim.GameDatabase.GetCivilianPopulation(planet.ID, playerInfo1.FactionID, flag2);
					double val1_1 = Math.Min(colony.ImperialPop, 50000000.0);
					double val1_2 = Math.Min(civilianPopulation, 50000000.0 * (double)colony.CivilianWeight);
					double num1 = 0.0;
					double num2 = 0.0;
					while (val1_1 > 0.0 && val1_2 > 0.0)
					{
						if (safeRandom.CoinToss(0.75))
						{
							int num3 = (int)Math.Min(val1_2, 1000000.0);
							num2 += (double)num3;
							val1_2 -= (double)num3;
						}
						else
						{
							int num3 = (int)Math.Min(val1_1, 1000000.0);
							num1 += (double)num3;
							val1_1 -= (double)num3;
						}
					}
					int num4 = 0;
					int num5 = 0;
					foreach (FleetInfo fleetInfo in sim.GameDatabase.GetFleetInfoBySystemID(orbitalObjectInfo.StarSystemID, FleetType.FL_ALL).ToList<FleetInfo>())
					{
						if (fleetInfo.PlayerID == colony.PlayerID && !fleetInfo.IsReserveFleet)
							num4 += sim.GameDatabase.GetFleetCruiserEquivalent(fleetInfo.ID);
						foreach (ShipInfo shipInfo in sim.GameDatabase.GetShipInfoByFleetID(fleetInfo.ID, false).ToList<ShipInfo>())
						{
							if (shipInfo.IsPoliceShip())
								++num5;
						}
					}
					double num6 = num2 + (double)(1000000 * num4);
					colony.ImperialPop -= num1;
					colony.ImperialPop = Math.Max(0.0, colony.ImperialPop);
					foreach (ColonyFactionInfo colonyFactionInfo in civPopulation)
					{
						if (civilianPopulation == 0.0)
						{
							colonyFactionInfo.CivilianPop = 0.0;
						}
						else
						{
							colonyFactionInfo.CivilianPop -= Math.Ceiling(num6 * (colonyFactionInfo.CivilianPop / civilianPopulation));
							colonyFactionInfo.CivilianPop = Math.Max(0.0, colonyFactionInfo.CivilianPop);
						}
					}
					planet.Infrastructure -= (float)Math.Floor((num6 + num1) / 1000000000.0);
					double num7 = civPopulation.Sum<ColonyFactionInfo>((Func<ColonyFactionInfo, double>)(x => x.CivilianPop));
					float num8 = num7 != 0.0 ? (float)((num6 - num1) / num7) + (float)num4 * (1f / 400f) + (float)num5 * 0.02f : 1f;
					if (safeRandom.CoinToss((double)num8))
					{
						sim.GameDatabase.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_REBELLION_ENDED_WIN,
							EventMessage = TurnEventMessage.EM_REBELLION_ENDED_WIN,
							PlayerID = colony.PlayerID,
							SystemID = orbitalObjectInfo.StarSystemID,
							OrbitalID = colony.OrbitalObjectID,
							ColonyID = colony.ID,
							TurnNumber = sim.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
						colony.RebellionType = RebellionType.None;
						foreach (ColonyFactionInfo colonyFactionInfo in civPopulation)
							colonyFactionInfo.Morale = 50;
					}
					else if (colony.ImperialPop <= 0.0)
					{
						sim.GameDatabase.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_REBELLION_ENDED_LOSS,
							EventMessage = TurnEventMessage.EM_REBELLION_ENDED_LOSS,
							PlayerID = colony.PlayerID,
							SystemID = orbitalObjectInfo.StarSystemID,
							OrbitalID = colony.OrbitalObjectID,
							ColonyID = colony.ID,
							TurnNumber = sim.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
						string factionName = string.Format("{0} {1}", (object)orbitalObjectInfo.Name, (object)App.Localize("@UI_PLAYER_NAME_REBELLION_COLONY"));
						Faction faction = sim.AssetDatabase.GetFaction(playerInfo1.FactionID);
						int insertIndyPlayerId = sim.GameDatabase.GetOrInsertIndyPlayerId(sim, playerInfo1.FactionID, factionName, faction.SplinterAvatarPath());
						sim.GameDatabase.UpdateDiplomacyState(insertIndyPlayerId, playerInfo1.ID, DiplomacyState.WAR, 500, true);
						foreach (PlayerInfo playerInfo2 in sim.GameDatabase.GetPlayerInfos().ToList<PlayerInfo>())
						{
							DiplomacyInfo diplomacyInfo = sim.GameDatabase.GetDiplomacyInfo(playerInfo1.ID, playerInfo2.ID);
							if (sim.GameDatabase.GetFactionName(playerInfo2.FactionID) == "morrigi")
								sim.GameDatabase.UpdateDiplomacyState(insertIndyPlayerId, playerInfo2.ID, diplomacyInfo.State, diplomacyInfo.Relations + 300, true);
							else
								sim.GameDatabase.UpdateDiplomacyState(insertIndyPlayerId, playerInfo2.ID, diplomacyInfo.State, diplomacyInfo.Relations, true);
						}
						sim.GameDatabase.DuplicateStratModifiers(insertIndyPlayerId, playerInfo1.ID);
						colony.ShipConRate = 0.0f;
						colony.TerraRate = 0.75f;
						colony.InfraRate = 0.25f;
						colony.TradeRate = 0.0f;
						colony.PlayerID = insertIndyPlayerId;
						colony.ImperialPop = 500.0;
						colony.RebellionType = RebellionType.None;
						foreach (ColonyFactionInfo colonyFactionInfo in civPopulation)
							colonyFactionInfo.Morale = 100;
					}
					else
					{
						sim.GameDatabase.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_REBELLION_ONGOING,
							EventMessage = TurnEventMessage.EM_REBELLION_ONGOING,
							PlayerID = colony.PlayerID,
							SystemID = orbitalObjectInfo.StarSystemID,
							OrbitalID = colony.OrbitalObjectID,
							ColonyID = colony.ID,
							TurnNumber = sim.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
						++colony.RebellionTurns;
					}
					colony.RepairPointsMax = Colony.CalcColonyRepairPoints(sim, colony);
					colony.RepairPoints = 0;
					return false;
				}
			}
			string factionName1 = sim.GameDatabase.GetFactionName(playerInfo1.FactionID);
			float factionSuitability = sim.GameDatabase.GetFactionSuitability(factionName1);
			float val1_3 = playerInfo1.isAIRebellionPlayer || factionName1 == "loa" ? 0.0f : Math.Abs(planet.Suitability - factionSuitability);
			float biosphere = (float)planet.Biosphere;
			int resources = planet.Resources;
			if (isSupplyRun || Colony.IsColonySelfSufficient(sim, colony, planet))
			{
				double num1 = colony.ImperialPop + civPopulation.Sum<ColonyFactionInfo>((Func<ColonyFactionInfo, double>)(x => x.CivilianPop));
				double num2 = num1 * (2.0 / (double)Math.Max(val1_3, 1f)) * (100.0 + (double)biosphere / 100.0);
				double val2 = Math.Truncate(Math.Max(0.0, num1 - num2));
				if (val2 > 0.0)
				{
					flag1 = false;
					if (fleetCapacity > 0.0)
					{
						double num3 = Math.Min(fleetCapacity * 1.5, val2);
						fleetCapacity -= num3 / 1.5;
						val2 -= num3;
					}
				}
				else
				{
					flag1 = true;
					if (colony.CurrentStage == Kerberos.Sots.Data.ColonyStage.Colony)
						colony.CurrentStage = Kerberos.Sots.Data.ColonyStage.Developed;
				}
				if (val2 > 0.0)
				{
					double num3 = val2 / 5.0;
					if (playerInfo1.isStandardPlayer && sim.GameDatabase.GetStratModifier<bool>(StratModifiers.ColonyStarvation, playerInfo1.ID))
					{
						if (num1 != 0.0)
						{
							foreach (ColonyFactionInfo colonyFactionInfo in civPopulation)
							{
								double num4 = Math.Truncate(colonyFactionInfo.CivilianPop / num1 * num3 * 2.0);
								colonyFactionInfo.CivilianPop -= num4;
								num3 -= num4;
								colonyFactionInfo.CivilianPop = Math.Max(colonyFactionInfo.CivilianPop, 0.0);
							}
							colony.ImperialPop -= colony.ImperialPop / num1 * num3;
						}
					}
					else if (plagues.Count == 0)
						Colony.UpdatePopulation(sim, ref colony, (int)(fleetCapacity * (1.0 - (double)sim.AssetDatabase.InfrastructureSupplyRatio)) * 10, ref civPopulation, flag2);
				}
				else if (plagues.Count == 0)
					Colony.UpdatePopulation(sim, ref colony, (int)(fleetCapacity * (1.0 - (double)sim.AssetDatabase.InfrastructureSupplyRatio)) * 10, ref civPopulation, flag2);
				colony.ImperialPop = Math.Truncate(colony.ImperialPop);
			}
			if (isSupplyRun && fleet != null)
			{
				float colonizationShips = (float)Kerberos.Sots.StarFleet.StarFleet.GetNumColonizationShips(sim, fleet.ID);
				double num1 = fleetCapacity * (double)sim.AssetDatabase.InfrastructureSupplyRatio / 2.0 + (double)colonizationShips * 1.0;
				planet.Infrastructure += (float)(num1 * 0.001);
				planet.Infrastructure = Math.Min(planet.Infrastructure, 1f);
				if (sim.App.GetStratModifier<bool>(StratModifiers.RequiresSterileEnvironment, playerInfo1.ID))
				{
					planet.Biosphere -= (int)Colony.GetBiosphereBurnDelta(sim, colony, planet, fleet);
					planet.Biosphere = Math.Max(planet.Biosphere, 0);
				}
				else
				{
					float num2 = Colony.GetTerraformingDelta(sim, colony, planet, terraformingBonus);
					if ((double)num2 > (double)val1_3)
						num2 = val1_3;
					planet.Suitability += num2;
				}
			}
			else
			{
				double infrastructureDelta = Colony.GetInfrastructureDelta(sim, colony, planet);
				double infrastructure = (double)planet.Infrastructure;
				planet.Infrastructure += (float)infrastructureDelta;
				planet.Infrastructure = Math.Min(planet.Infrastructure, 1f);
				if ((double)planet.Infrastructure == 1.0)
					Colony.SetOutputRate(sim.GameDatabase, sim.AssetDatabase, ref colony, planet, Colony.OutputRate.Infra, 0.0f);
				if (Colony.CanBeOverdeveloped(sim.AssetDatabase, sim.GameDatabase, colony, planet))
				{
					colony.OverdevelopProgress += Colony.GetOverdevelopmentDelta(sim, colony, planet);
					if ((double)colony.OverdevelopProgress >= (double)Colony.GetOverdevelopmentTarget(sim, planet))
						achievedSuperWorld = true;
				}
				else
				{
					colony.OverdevelopProgress = 0.0f;
					colony.OverdevelopRate = 0.0f;
				}
				if (!sim.App.GetStratModifier<bool>(StratModifiers.RequiresSterileEnvironment, playerInfo1.ID))
				{
					float num = Colony.GetTerraformingDelta(sim, colony, planet, terraformingBonus);
					if ((double)num > (double)val1_3)
						num = val1_3;
					planet.Suitability += num;
					if ((double)val1_3 > 0.0 && (double)val1_3 - (double)num <= 0.0)
						Colony.SetOutputRate(sim.GameDatabase, sim.AssetDatabase, ref colony, planet, Colony.OutputRate.Terra, 0.0f);
					if ((double)val1_3 > 0.0 && (double)val1_3 - (double)num <= 0.0 && playerInfo1.ID == sim.LocalPlayer.ID)
						sim.App.SteamHelper.DoAchievement(AchievementType.SOTS2_HOT_COLD);
					planet.Biosphere += (int)Colony.GetBiosphereDelta(sim, colony, planet, terraformingBonus);
					planet.Biosphere = Math.Max(0, planet.Biosphere);
				}
				planet.Resources -= (int)Colony.CalcOverharvestRate(sim.AssetDatabase, sim.GameDatabase, colony, planet);
				planet.Resources -= (int)Colony.CalcOverharvestFromOverpopulation(sim.GameDatabase, colony, planet);
				if (planet.Resources == 0 && playerInfo1.ID == sim.LocalPlayer.ID)
					sim.App.SteamHelper.DoAchievement(AchievementType.SOTS2_NOTHIN);
				int val1_1 = 100;
				foreach (ColonyFactionInfo faction in colony.Factions)
					val1_1 = Math.Min(val1_1, faction.Morale);
				if (!flag2)
				{
					int num1 = 20;
					StarSystemInfo ssi = sim.GameDatabase.GetStarSystemInfo(colony.CachedStarSystemID);
					if (ssi != (StarSystemInfo)null && ssi.ProvinceID.HasValue)
					{
						int num2 = sim.GameDatabase.GetStarSystemInfos().Count<StarSystemInfo>((Func<StarSystemInfo, bool>)(x =>
					   {
						   if (x.ProvinceID.HasValue)
							   return ssi.ProvinceID.Value == x.ProvinceID.Value;
						   return false;
					   }));
						num1 = Math.Max(num1 - 2 * num2, 0);
					}
					if (val1_1 < num1)
					{
						if (safeRandom.CoinToss((double)(100 - val1_1) * 0.75))
						{
							colony.RebellionType = RebellionType.Civilian;
							colony.RebellionTurns = 0;
							sim.GameDatabase.InsertTurnEvent(new TurnEvent()
							{
								EventType = TurnEventType.EV_REBELLION_STARTED,
								EventMessage = TurnEventMessage.EM_REBELLION_STARTED,
								PlayerID = colony.PlayerID,
								SystemID = orbitalObjectInfo.StarSystemID,
								OrbitalID = colony.OrbitalObjectID,
								ColonyID = colony.ID,
								TurnNumber = sim.GameDatabase.GetTurnCount(),
								ShowsDialog = false
							});
						}
						else
							sim.GameDatabase.InsertTurnEvent(new TurnEvent()
							{
								EventType = TurnEventType.EV_REBELLION_STARTING,
								EventMessage = TurnEventMessage.EM_REBELLION_STARTING,
								PlayerID = colony.PlayerID,
								SystemID = orbitalObjectInfo.StarSystemID,
								OrbitalID = colony.OrbitalObjectID,
								ColonyID = colony.ID,
								TurnNumber = sim.GameDatabase.GetTurnCount(),
								ShowsDialog = false
							});
					}
				}
			}
			colony.RepairPoints = Colony.CalcColonyRepairPoints(sim, colony);
			colony.RepairPointsMax = colony.RepairPoints;
			if (sim.App.GetStratModifier<bool>(StratModifiers.RequiresSterileEnvironment, playerInfo1.ID))
				flag1 = planet.Biosphere == 0;
			return flag1;
		}

		public enum OutputRate
		{
			Trade,
			Infra,
			Terra,
			ShipCon,
			OverDev,
		}
	}
}
