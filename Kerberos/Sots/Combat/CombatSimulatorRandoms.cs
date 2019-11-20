// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.CombatSimulatorRandoms
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Encounters;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameTriggers;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Combat
{
	internal static class CombatSimulatorRandoms
	{
		private static Random _rand = new Random();

		public static bool Simulate(GameSession game, int systemId, List<FleetInfo> fleets)
		{
			if (ScriptHost.AllowConsole)
				App.Log.Trace(string.Format("Simulating RANDOM AI combat at: {0}", (object)systemId), "combat");
			bool flag1 = true;
			Dictionary<PlayerInfo, List<FleetInfo>> dictionary = new Dictionary<PlayerInfo, List<FleetInfo>>();
			foreach (FleetInfo fleet in fleets)
			{
				PlayerInfo playerInfo = game.GameDatabase.GetPlayerInfo(fleet.PlayerID);
				if (playerInfo != null)
				{
					if (!dictionary.ContainsKey(playerInfo))
					{
						dictionary.Add(playerInfo, new List<FleetInfo>());
						if (!playerInfo.isStandardPlayer && !CombatSimulatorRandoms.IsValidSimulateEncounterPlayer(game, playerInfo.ID))
							flag1 = false;
					}
					dictionary[playerInfo].Add(fleet);
				}
			}
			List<ColonyInfo> colonyInfoList = new List<ColonyInfo>();
			PlanetInfo[] systemPlanetInfos = game.GameDatabase.GetStarSystemPlanetInfos(systemId);
			if (systemPlanetInfos != null)
			{
				foreach (PlanetInfo planetInfo in systemPlanetInfos)
				{
					ColonyInfo colonyInfoForPlanet = game.GameDatabase.GetColonyInfoForPlanet(planetInfo.ID);
					if (colonyInfoForPlanet != null && colonyInfoForPlanet.IsIndependentColony(game.App))
						colonyInfoList.Add(colonyInfoForPlanet);
				}
			}
			if (!flag1 && colonyInfoList.Count == 0 || dictionary.Keys.Count<PlayerInfo>((Func<PlayerInfo, bool>)(x => x.isStandardPlayer)) != 1)
				return false;
			bool flag2 = false;
			PlayerInfo index1 = dictionary.Keys.First<PlayerInfo>((Func<PlayerInfo, bool>)(x => x.isStandardPlayer));
			List<FleetInfo> aiPlayerFleets = dictionary[index1];
			if (game.ScriptModules.Swarmers != null)
			{
				PlayerInfo index2 = dictionary.Keys.FirstOrDefault<PlayerInfo>((Func<PlayerInfo, bool>)(x => x.ID == game.ScriptModules.Swarmers.PlayerID));
				if (index2 != null)
				{
					FleetInfo randomsFleet1 = (FleetInfo)null;
					FleetInfo randomsFleet2 = (FleetInfo)null;
					foreach (FleetInfo fleetInfo in dictionary[index2])
					{
						List<ShipInfo> list = game.GameDatabase.GetShipInfoByFleetID(fleetInfo.ID, false).ToList<ShipInfo>();
						if (list.Count != 0)
						{
							if (list.Any<ShipInfo>((Func<ShipInfo, bool>)(x => x.DesignID == game.ScriptModules.Swarmers.SwarmQueenDesignID)))
								randomsFleet1 = fleetInfo;
							else
								randomsFleet2 = fleetInfo;
						}
					}
					if (randomsFleet1 != null)
						CombatSimulatorRandoms.SimulateSwarmerQueen(CombatSimulatorRandoms._rand, game, systemId, randomsFleet1, index1.ID, aiPlayerFleets);
					if (randomsFleet2 != null)
						CombatSimulatorRandoms.SimulateSwarmerNest(CombatSimulatorRandoms._rand, game, systemId, randomsFleet2, index1.ID, aiPlayerFleets);
					flag2 = true;
				}
			}
			if (game.ScriptModules.Gardeners != null)
			{
				PlayerInfo index2 = dictionary.Keys.FirstOrDefault<PlayerInfo>((Func<PlayerInfo, bool>)(x => x.ID == game.ScriptModules.Gardeners.PlayerID));
				if (index2 != null)
				{
					FleetInfo randomsFleet = dictionary[index2].FirstOrDefault<FleetInfo>();
					if (randomsFleet != null)
						CombatSimulatorRandoms.SimulateProteans(CombatSimulatorRandoms._rand, game, systemId, randomsFleet, index1.ID, aiPlayerFleets);
					flag2 = true;
				}
			}
			if (game.ScriptModules.AsteroidMonitor != null)
			{
				PlayerInfo index2 = dictionary.Keys.FirstOrDefault<PlayerInfo>((Func<PlayerInfo, bool>)(x => x.ID == game.ScriptModules.AsteroidMonitor.PlayerID));
				if (index2 != null)
				{
					FleetInfo randomsFleet = dictionary[index2].FirstOrDefault<FleetInfo>();
					if (randomsFleet != null)
						CombatSimulatorRandoms.SimulateAsteroidMonitors(CombatSimulatorRandoms._rand, game, systemId, randomsFleet, index1.ID, aiPlayerFleets);
					flag2 = true;
				}
			}
			if (game.ScriptModules.MorrigiRelic != null)
			{
				PlayerInfo index2 = dictionary.Keys.FirstOrDefault<PlayerInfo>((Func<PlayerInfo, bool>)(x => x.ID == game.ScriptModules.MorrigiRelic.PlayerID));
				if (index2 != null)
				{
					FleetInfo randomsFleet = dictionary[index2].FirstOrDefault<FleetInfo>();
					if (randomsFleet != null)
						CombatSimulatorRandoms.SimulateMorrigiRelics(CombatSimulatorRandoms._rand, game, systemId, randomsFleet, index1.ID, aiPlayerFleets);
					flag2 = true;
				}
			}
			if (game.ScriptModules.Pirates != null)
			{
				PlayerInfo index2 = dictionary.Keys.FirstOrDefault<PlayerInfo>((Func<PlayerInfo, bool>)(x => x.ID == game.ScriptModules.Pirates.PlayerID));
				if (index2 != null)
				{
					FleetInfo randomsFleet = dictionary[index2].FirstOrDefault<FleetInfo>();
					if (randomsFleet != null)
						CombatSimulatorRandoms.SimulatePirateBase(CombatSimulatorRandoms._rand, game, systemId, randomsFleet, index1.ID, aiPlayerFleets);
					flag2 = true;
				}
			}
			foreach (ColonyInfo indyColony in colonyInfoList)
			{
				Dictionary<PlayerInfo, List<FleetInfo>> enemyPlayers = new Dictionary<PlayerInfo, List<FleetInfo>>();
				foreach (KeyValuePair<PlayerInfo, List<FleetInfo>> keyValuePair in dictionary)
				{
					if (keyValuePair.Key.isStandardPlayer && game.GameDatabase.GetDiplomacyStateBetweenPlayers(indyColony.PlayerID, keyValuePair.Key.ID) == DiplomacyState.WAR)
						enemyPlayers.Add(keyValuePair.Key, keyValuePair.Value);
				}
				if (enemyPlayers.Keys.Count != 0)
				{
					CombatSimulatorRandoms.SimulateIndyColony(CombatSimulatorRandoms._rand, game, systemId, indyColony, enemyPlayers);
					flag2 = true;
				}
			}
			return flag2;
		}

		private static bool IsValidSimulateEncounterPlayer(GameSession game, int playerId)
		{
			return game.ScriptModules != null && (game.ScriptModules.Swarmers != null && game.ScriptModules.Swarmers.PlayerID == playerId || game.ScriptModules.Gardeners != null && game.ScriptModules.Gardeners.PlayerID == playerId || (game.ScriptModules.AsteroidMonitor != null && game.ScriptModules.AsteroidMonitor.PlayerID == playerId || game.ScriptModules.MorrigiRelic != null && game.ScriptModules.MorrigiRelic.PlayerID == playerId) || game.ScriptModules.Pirates != null && game.ScriptModules.Pirates.PlayerID == playerId);
		}

		private static Dictionary<FleetInfo, List<ShipInfo>> GetShipsInFleets(
		  GameSession game,
		  List<FleetInfo> playerFleets)
		{
			Dictionary<FleetInfo, List<ShipInfo>> dictionary = new Dictionary<FleetInfo, List<ShipInfo>>();
			foreach (FleetInfo playerFleet in playerFleets)
			{
				if (playerFleet.Type == FleetType.FL_NORMAL || (playerFleet.Type & FleetType.FL_ALL_COMBAT) != (FleetType)0)
				{
					dictionary.Add(playerFleet, new List<ShipInfo>());
					foreach (ShipInfo shipInfo in game.GameDatabase.GetShipInfoByFleetID(playerFleet.ID, true).ToList<ShipInfo>())
					{
						if (!ShipSectionAsset.IsBattleRiderClass(shipInfo.DesignInfo.GetRealShipClass().Value))
							dictionary[playerFleet].Add(shipInfo);
					}
				}
			}
			return dictionary;
		}

		private static void SimulateSwarmerQueen(
		  Random rand,
		  GameSession game,
		  int systemId,
		  FleetInfo randomsFleet,
		  int aiPlayerID,
		  List<FleetInfo> aiPlayerFleets)
		{
			int numToKill = rand.NextInclusive(4, 6);
			Dictionary<FleetInfo, List<ShipInfo>> shipsInFleets = CombatSimulatorRandoms.GetShipsInFleets(game, aiPlayerFleets);
			shipsInFleets.Sum<KeyValuePair<FleetInfo, List<ShipInfo>>>((Func<KeyValuePair<FleetInfo, List<ShipInfo>>, int>)(x => x.Value.Count));
			if (shipsInFleets.Sum<KeyValuePair<FleetInfo, List<ShipInfo>>>((Func<KeyValuePair<FleetInfo, List<ShipInfo>>, int>)(x =>
		   {
			   if (x.Value.Count <= 0)
				   return 0;
			   return x.Value.Sum<ShipInfo>((Func<ShipInfo, int>)(y =>
		 {
				 if (y.DesignInfo == null)
					 return 0;
				 return CombatAI.GetShipStrength(y.DesignInfo.Class) / 3;
			 }));
		   })) <= 4)
			{
				foreach (KeyValuePair<FleetInfo, List<ShipInfo>> keyValuePair in shipsInFleets)
				{
					foreach (ShipInfo shipInfo in keyValuePair.Value)
						game.GameDatabase.RemoveShip(shipInfo.ID);
					CombatSimulatorRandoms.FleetDestroyed(game, randomsFleet.PlayerID, keyValuePair.Key, (ShipInfo)null);
					game.GameDatabase.RemoveFleet(keyValuePair.Key.ID);
				}
				if (game.GameDatabase.GetShipInfoByFleetID(randomsFleet.ID, false).ToList<ShipInfo>().Count == 0)
					game.GameDatabase.RemoveFleet(randomsFleet.ID);
			}
			else
			{
				CombatSimulatorRandoms.UpdateShipsKilled(game, rand, shipsInFleets, randomsFleet.PlayerID, numToKill);
				List<ShipInfo> list = game.GameDatabase.GetShipInfoByFleetID(randomsFleet.ID, false).ToList<ShipInfo>();
				if (list.Count > 0)
					CombatSimulatorRandoms.FleetDestroyed(game, aiPlayerID, randomsFleet, list.First<ShipInfo>());
				game.GameDatabase.RemoveFleet(randomsFleet.ID);
			}
			foreach (SwarmerInfo si in game.GameDatabase.GetSwarmerInfos().ToList<SwarmerInfo>())
			{
				if (si.QueenFleetId.HasValue)
				{
					FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(si.QueenFleetId.Value);
					if (fleetInfo != null && fleetInfo.SystemID == systemId)
						Swarmers.ClearTransform(game.GameDatabase, si);
				}
			}
		}

		private static void SimulateSwarmerNest(
		  Random rand,
		  GameSession game,
		  int systemId,
		  FleetInfo randomsFleet,
		  int aiPlayerID,
		  List<FleetInfo> aiPlayerFleets)
		{
			int numToKill = rand.NextInclusive(2, 4);
			Dictionary<FleetInfo, List<ShipInfo>> shipsInFleets = CombatSimulatorRandoms.GetShipsInFleets(game, aiPlayerFleets);
			shipsInFleets.Sum<KeyValuePair<FleetInfo, List<ShipInfo>>>((Func<KeyValuePair<FleetInfo, List<ShipInfo>>, int>)(x => x.Value.Count));
			if (shipsInFleets.Sum<KeyValuePair<FleetInfo, List<ShipInfo>>>((Func<KeyValuePair<FleetInfo, List<ShipInfo>>, int>)(x =>
		   {
			   if (x.Value.Count <= 0)
				   return 0;
			   return x.Value.Sum<ShipInfo>((Func<ShipInfo, int>)(y =>
		 {
				 if (y.DesignInfo == null)
					 return 0;
				 return CombatAI.GetShipStrength(y.DesignInfo.Class) / 3;
			 }));
		   })) <= 2)
			{
				foreach (KeyValuePair<FleetInfo, List<ShipInfo>> keyValuePair in shipsInFleets)
				{
					foreach (ShipInfo shipInfo in keyValuePair.Value)
						game.GameDatabase.RemoveShip(shipInfo.ID);
					CombatSimulatorRandoms.FleetDestroyed(game, randomsFleet.PlayerID, keyValuePair.Key, (ShipInfo)null);
					game.GameDatabase.RemoveFleet(keyValuePair.Key.ID);
				}
				if (game.GameDatabase.GetShipInfoByFleetID(randomsFleet.ID, false).ToList<ShipInfo>().Count != 0)
					return;
				game.GameDatabase.RemoveFleet(randomsFleet.ID);
			}
			else
			{
				CombatSimulatorRandoms.UpdateShipsKilled(game, rand, shipsInFleets, randomsFleet.PlayerID, numToKill);
				List<ShipInfo> list = game.GameDatabase.GetShipInfoByFleetID(randomsFleet.ID, false).ToList<ShipInfo>();
				if (list.Count > 0)
					CombatSimulatorRandoms.FleetDestroyed(game, aiPlayerID, randomsFleet, list.First<ShipInfo>());
				game.GameDatabase.RemoveFleet(randomsFleet.ID);
			}
		}

		private static void SimulateProteans(
		  Random rand,
		  GameSession game,
		  int systemId,
		  FleetInfo randomsFleet,
		  int aiPlayerID,
		  List<FleetInfo> aiPlayerFleets)
		{
			int numToKill = rand.NextInclusive(3, 5);
			Dictionary<FleetInfo, List<ShipInfo>> shipsInFleets = CombatSimulatorRandoms.GetShipsInFleets(game, aiPlayerFleets);
			if (shipsInFleets.Sum<KeyValuePair<FleetInfo, List<ShipInfo>>>((Func<KeyValuePair<FleetInfo, List<ShipInfo>>, int>)(x => x.Value.Count)) == 0)
				return;
			CombatSimulatorRandoms.UpdateShipsKilled(game, rand, shipsInFleets, randomsFleet.PlayerID, numToKill);
			CombatSimulatorRandoms.FleetDestroyed(game, aiPlayerID, randomsFleet, (ShipInfo)null);
			game.GameDatabase.RemoveFleet(randomsFleet.ID);
		}

		private static void SimulateAsteroidMonitors(
		  Random rand,
		  GameSession game,
		  int systemId,
		  FleetInfo randomsFleet,
		  int aiPlayerID,
		  List<FleetInfo> aiPlayerFleets)
		{
			int numToKill = rand.NextInclusive(1, 3);
			Dictionary<FleetInfo, List<ShipInfo>> shipsInFleets = CombatSimulatorRandoms.GetShipsInFleets(game, aiPlayerFleets);
			if (shipsInFleets.Sum<KeyValuePair<FleetInfo, List<ShipInfo>>>((Func<KeyValuePair<FleetInfo, List<ShipInfo>>, int>)(x => x.Value.Count)) == 0)
				return;
			CombatSimulatorRandoms.UpdateShipsKilled(game, rand, shipsInFleets, randomsFleet.PlayerID, numToKill);
			int encounterIdAtSystem = game.GameDatabase.GetEncounterIDAtSystem(EasterEgg.EE_ASTEROID_MONITOR, systemId);
			ShipInfo shipInfo = game.GameDatabase.GetShipInfoByFleetID(randomsFleet.ID, false).ToList<ShipInfo>().FirstOrDefault<ShipInfo>((Func<ShipInfo, bool>)(x => x.DesignID == game.ScriptModules.AsteroidMonitor.MonitorCommandDesignId));
			if (shipInfo != null)
			{
				AsteroidMonitorInfo asteroidMonitorInfo = game.GameDatabase.GetAsteroidMonitorInfo(encounterIdAtSystem);
				if (asteroidMonitorInfo != null)
				{
					asteroidMonitorInfo.IsAggressive = false;
					game.GameDatabase.UpdateAsteroidMonitorInfo(asteroidMonitorInfo);
				}
				shipInfo.DesignInfo = game.GameDatabase.GetDesignInfo(shipInfo.DesignID);
				List<DesignModuleInfo> source = new List<DesignModuleInfo>();
				foreach (DesignSectionInfo designSection in shipInfo.DesignInfo.DesignSections)
					source.AddRange((IEnumerable<DesignModuleInfo>)designSection.Modules.ToList<DesignModuleInfo>());
				if (source.Any<DesignModuleInfo>())
				{
					foreach (SectionInstanceInfo sectionInstanceInfo in game.GameDatabase.GetShipSectionInstances(shipInfo.ID).ToList<SectionInstanceInfo>())
					{
						foreach (ModuleInstanceInfo module in game.GameDatabase.GetModuleInstances(sectionInstanceInfo.ID).ToList<ModuleInstanceInfo>())
						{
							module.Structure = 0;
							game.GameDatabase.UpdateModuleInstance(module);
						}
					}
					game.InsertNewMonitorSpecialProject(aiPlayerID, encounterIdAtSystem, randomsFleet.ID);
				}
				else
				{
					game.GameDatabase.RemoveFleet(randomsFleet.ID);
					game.GameDatabase.RemoveEncounter(encounterIdAtSystem);
				}
			}
			else
			{
				game.GameDatabase.RemoveFleet(randomsFleet.ID);
				game.GameDatabase.RemoveEncounter(encounterIdAtSystem);
			}
		}

		private static void SimulateIndyColony(
		  Random rand,
		  GameSession game,
		  int systemId,
		  ColonyInfo indyColony,
		  Dictionary<PlayerInfo, List<FleetInfo>> enemyPlayers)
		{
			StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(systemId);
			PlanetInfo planetInfo = game.GameDatabase.GetPlanetInfo(indyColony.OrbitalObjectID);
			List<PlayerInfo> standardPlayers = game.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>();
			foreach (FleetInfo fleetInfo in game.GameDatabase.GetFleetInfoBySystemID(systemId, FleetType.FL_NORMAL).ToList<FleetInfo>())
			{
				MissionInfo missionByFleetId = game.GameDatabase.GetMissionByFleetID(fleetInfo.ID);
				if (missionByFleetId != null && missionByFleetId.Type == MissionType.INVASION && missionByFleetId.TargetOrbitalObjectID == planetInfo.ID)
				{
					game.GameDatabase.InsertGovernmentAction(fleetInfo.PlayerID, App.Localize("@GA_INDEPENDANTCONQUERED"), "IndependantConquered", 0, 0);
					foreach (PlayerInfo playerInfo in standardPlayers)
					{
						if (game.GameDatabase.GetDiplomacyInfo(fleetInfo.PlayerID, playerInfo.ID).isEncountered)
							game.GameDatabase.ApplyDiplomacyReaction(playerInfo.ID, fleetInfo.PlayerID, StratModifiers.DiplomacyReactionInvadeIndependentWorld, 1);
					}
				}
			}
			game.GameDatabase.InsertTurnEvent(new TurnEvent()
			{
				EventType = TurnEventType.EV_COLONY_DESTROYED,
				EventMessage = TurnEventMessage.EM_COLONY_DESTROYED,
				PlayerID = indyColony.PlayerID,
				ColonyID = indyColony.ID,
				SystemID = systemId,
				TurnNumber = game.GameDatabase.GetTurnCount(),
				ShowsDialog = false
			});
			StationTypeFlags stationTypeFlags = StationTypeFlags.CIVILIAN | StationTypeFlags.DIPLOMATIC | StationTypeFlags.DEFENCE;
			foreach (StationInfo stationInfo in game.GameDatabase.GetStationForSystemAndPlayer(systemId, indyColony.PlayerID).ToList<StationInfo>())
			{
				int? parentId = stationInfo.OrbitalObjectInfo.ParentID;
				int id = planetInfo.ID;
				if ((parentId.GetValueOrDefault() != id ? 0 : (parentId.HasValue ? 1 : 0)) != 0 && ((StationTypeFlags)(1 << (int)(stationInfo.DesignInfo.StationType & (StationType)31)) & stationTypeFlags) != (StationTypeFlags)0)
					game.GameDatabase.DestroyStation(game, stationInfo.ID, 0);
			}
			game.GameDatabase.RemoveColonyOnPlanet(planetInfo.ID);
			if (enemyPlayers.Keys.Any<PlayerInfo>((Func<PlayerInfo, bool>)(x => standardPlayers.Any<PlayerInfo>((Func<PlayerInfo, bool>)(y => y.ID == x.ID)))))
				GameSession.ApplyMoralEvent(game.App, MoralEvent.ME_LOSE_WORLD_ENEMY, indyColony.PlayerID, new int?(), starSystemInfo.ProvinceID, new int?());
			foreach (int num in enemyPlayers.Keys.Select<PlayerInfo, int>((Func<PlayerInfo, int>)(x => x.ID)).ToList<int>())
			{
				int i = num;
				game.App.GameDatabase.ApplyDiplomacyReaction(indyColony.PlayerID, i, StratModifiers.DiplomacyReactionKillColony, 1);
				int factionId = game.App.GameDatabase.GetPlayerFactionID(indyColony.PlayerID);
				foreach (PlayerInfo playerInfo in standardPlayers.Where<PlayerInfo>((Func<PlayerInfo, bool>)(x =>
			   {
				   if (x.FactionID == factionId)
					   return x.ID != i;
				   return false;
			   })).ToList<PlayerInfo>())
				{
					if (game.App.GameDatabase.GetDiplomacyInfo(i, playerInfo.ID).isEncountered)
						game.App.GameDatabase.ApplyDiplomacyReaction(playerInfo.ID, i, StratModifiers.DiplomacyReactionKillRaceWorld, 1);
				}
			}
			if (indyColony.CurrentStage == Kerberos.Sots.Data.ColonyStage.GemWorld)
			{
				GameSession.ApplyMoralEvent(game.App, MoralEvent.ME_LOSE_GEM, indyColony.PlayerID, new int?(), starSystemInfo.ProvinceID, new int?());
			}
			else
			{
				if (indyColony.CurrentStage != Kerberos.Sots.Data.ColonyStage.ForgeWorld)
					return;
				GameSession.ApplyMoralEvent(game.App, MoralEvent.ME_LOSE_FORGE, indyColony.PlayerID, new int?(), starSystemInfo.ProvinceID, new int?());
			}
		}

		private static void SimulateMorrigiRelics(
		  Random rand,
		  GameSession game,
		  int systemId,
		  FleetInfo randomsFleet,
		  int aiPlayerID,
		  List<FleetInfo> aiPlayerFleets)
		{
			int numToKill = rand.NextInclusive(5, 6);
			Dictionary<FleetInfo, List<ShipInfo>> shipsInFleets = CombatSimulatorRandoms.GetShipsInFleets(game, aiPlayerFleets);
			if (shipsInFleets.Sum<KeyValuePair<FleetInfo, List<ShipInfo>>>((Func<KeyValuePair<FleetInfo, List<ShipInfo>>, int>)(x => x.Value.Count)) == 0)
				return;
			CombatSimulatorRandoms.UpdateShipsKilled(game, rand, shipsInFleets, randomsFleet.PlayerID, numToKill);
			MorrigiRelicInfo relicInfo = game.GameDatabase.GetMorrigiRelicInfos().ToList<MorrigiRelicInfo>().FirstOrDefault<MorrigiRelicInfo>((Func<MorrigiRelicInfo, bool>)(x => x.FleetId == randomsFleet.ID));
			if (relicInfo != null && relicInfo.IsAggressive)
			{
				List<ShipInfo> list = game.GameDatabase.GetShipInfoByFleetID(randomsFleet.ID, false).ToList<ShipInfo>();
				game.ScriptModules.MorrigiRelic.ApplyRewardsToPlayers(game.App, relicInfo, list, new List<Player>()
		{
		  game.GetPlayerObject(aiPlayerID)
		});
			}
			else
			{
				CombatSimulatorRandoms.FleetDestroyed(game, aiPlayerID, randomsFleet, (ShipInfo)null);
				game.GameDatabase.RemoveFleet(randomsFleet.ID);
			}
		}

		private static void SimulatePirateBase(
		  Random rand,
		  GameSession game,
		  int systemId,
		  FleetInfo randomsFleet,
		  int aiPlayerID,
		  List<FleetInfo> aiPlayerFleets)
		{
			int numToKill = rand.NextInclusive(5, 6);
			Dictionary<FleetInfo, List<ShipInfo>> shipsInFleets = CombatSimulatorRandoms.GetShipsInFleets(game, aiPlayerFleets);
			int num1 = shipsInFleets.Sum<KeyValuePair<FleetInfo, List<ShipInfo>>>((Func<KeyValuePair<FleetInfo, List<ShipInfo>>, int>)(x => x.Value.Count));
			if (num1 == 0)
				return;
			PirateBaseInfo pirateBaseInfo = game.GameDatabase.GetPirateBaseInfos().FirstOrDefault<PirateBaseInfo>((Func<PirateBaseInfo, bool>)(x => x.SystemId == systemId));
			if (pirateBaseInfo == null)
				return;
			CombatSimulatorRandoms.UpdateShipsKilled(game, rand, shipsInFleets, randomsFleet.PlayerID, numToKill);
			if (num1 < numToKill)
				return;
			int bounty = game.AssetDatabase.GlobalPiracyData.Bounties[0];
			List<FleetInfo> list = game.GameDatabase.GetFleetsByPlayerAndSystem(randomsFleet.PlayerID, systemId, FleetType.FL_NORMAL).ToList<FleetInfo>();
			int maxValue = 0;
			foreach (FleetInfo fleetInfo in list)
				maxValue = game.GameDatabase.GetShipInfoByFleetID(fleetInfo.ID, true).Count<ShipInfo>((Func<ShipInfo, bool>)(x => x.DesignInfo.Class != ShipClass.Station));
			if (maxValue > 0)
			{
				int num2 = rand.NextInclusive(0, maxValue);
				int num3 = bounty + num2 * game.AssetDatabase.GlobalPiracyData.Bounties[1];
			}
			foreach (int num2 in game.GameDatabase.GetStandardPlayerIDs().ToList<int>())
			{
				if (num2 != aiPlayerID)
				{
					string factionName = game.GameDatabase.GetFactionName(game.GameDatabase.GetPlayerFactionID(num2));
					int reactionAmount = 0;
					game.AssetDatabase.GlobalPiracyData.ReactionBonuses.TryGetValue(factionName, out reactionAmount);
					game.GameDatabase.ApplyDiplomacyReaction(num2, aiPlayerID, reactionAmount, new StratModifiers?(StratModifiers.DiplomacyReactionElimPirates), 1);
				}
			}
			game.GameDatabase.InsertTurnEvent(new TurnEvent()
			{
				EventType = TurnEventType.EV_PIRATE_BASE_DESTROYED,
				EventMessage = TurnEventMessage.EM_PIRATE_BASE_DESTROYED,
				PlayerID = aiPlayerID,
				SystemID = systemId,
				TurnNumber = game.GameDatabase.GetTurnCount(),
				ShowsDialog = false
			});
			game.GameDatabase.DestroyStation(game, pirateBaseInfo.BaseStationId, 0);
		}

		private static void UpdateShipsKilled(
		  GameSession game,
		  Random rand,
		  Dictionary<FleetInfo, List<ShipInfo>> aiPlayerShips,
		  int randomsPlayerID,
		  int numToKill)
		{
			int num1 = numToKill;
			for (int index = 0; index < numToKill && num1 > 0 && aiPlayerShips.Keys.Count > 0; ++index)
			{
				bool flag = false;
				while (!flag && aiPlayerShips.Keys.Count > 0)
				{
					int num2 = 0;
					foreach (KeyValuePair<FleetInfo, List<ShipInfo>> aiPlayerShip in aiPlayerShips)
					{
						num2 += aiPlayerShip.Value.Count;
						foreach (ShipInfo shipInfo in aiPlayerShip.Value)
						{
							ShipInfo ship = shipInfo;
							if (rand.CoinToss(50))
							{
								num1 -= CombatAI.GetShipStrength(ship.DesignInfo.Class) / 3;
								if (ship.DesignInfo.IsSuulka())
								{
									TurnEvent turnEvent = game.GameDatabase.GetTurnEventsByTurnNumber(game.GameDatabase.GetTurnCount(), aiPlayerShip.Key.PlayerID).FirstOrDefault<TurnEvent>((Func<TurnEvent, bool>)(x => x.ShipID == ship.ID));
									if (turnEvent != null)
										game.GameDatabase.RemoveTurnEvent(turnEvent.ID);
									game.GameDatabase.InsertTurnEvent(new TurnEvent()
									{
										EventType = TurnEventType.EV_SUULKA_DIES,
										EventMessage = TurnEventMessage.EM_SUULKA_DIES,
										PlayerID = randomsPlayerID,
										SystemID = aiPlayerShip.Key.SystemID,
										ShipID = ship.ID,
										DesignID = ship.DesignID,
										TurnNumber = game.GameDatabase.GetTurnCount(),
										ShowsDialog = false
									});
									SuulkaInfo suulkaByShipId = game.GameDatabase.GetSuulkaByShipID(ship.ID);
									if (suulkaByShipId != null)
										game.GameDatabase.RemoveSuulka(suulkaByShipId.ID);
								}
								game.GameDatabase.RemoveShip(ship.ID);
								aiPlayerShip.Value.Remove(ship);
								flag = true;
								break;
							}
						}
						if (flag)
						{
							if (aiPlayerShip.Value.Count == 0)
							{
								CombatSimulatorRandoms.FleetDestroyed(game, randomsPlayerID, aiPlayerShip.Key, (ShipInfo)null);
								game.GameDatabase.RemoveFleet(aiPlayerShip.Key.ID);
								aiPlayerShips.Remove(aiPlayerShip.Key);
								break;
							}
							break;
						}
					}
					if (num2 == 0)
						break;
				}
			}
			foreach (KeyValuePair<FleetInfo, List<ShipInfo>> aiPlayerShip in aiPlayerShips)
			{
				if (aiPlayerShip.Value.Count > 0)
					CombatSimulatorRandoms.CheckFleetCommandPoints(game, aiPlayerShip.Key, aiPlayerShip.Value);
			}
		}

		private static void FleetDestroyed(
		  GameSession game,
		  int killerPlayerID,
		  FleetInfo fleet,
		  ShipInfo fleetCommander = null)
		{
			if (fleet.PlayerID == game.ScriptModules.Gardeners.PlayerID)
				game.GameDatabase.InsertTurnEvent(new TurnEvent()
				{
					EventType = TurnEventType.EV_PROTEANS_REMOVED,
					EventMessage = TurnEventMessage.EM_PROTEANS_REMOVED,
					PlayerID = killerPlayerID,
					SystemID = fleet.SystemID,
					TurnNumber = game.GameDatabase.GetTurnCount(),
					ShowsDialog = false
				});
			else if (fleet.PlayerID == game.ScriptModules.Swarmers.PlayerID)
			{
				if (fleetCommander != null && fleetCommander.DesignID == game.ScriptModules.Swarmers.SwarmQueenDesignID)
					game.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_SWARM_QUEEN_DESTROYED,
						EventMessage = TurnEventMessage.EM_SWARM_QUEEN_DESTROYED,
						PlayerID = killerPlayerID,
						SystemID = fleet.SystemID,
						TurnNumber = game.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
				else
					game.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_SWARM_DESTROYED,
						EventMessage = TurnEventMessage.EM_SWARM_DESTROYED,
						PlayerID = killerPlayerID,
						SystemID = fleet.SystemID,
						TurnNumber = game.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
			}
			else if (fleet.PlayerID == game.ScriptModules.MorrigiRelic.PlayerID)
				game.GameDatabase.InsertTurnEvent(new TurnEvent()
				{
					EventType = TurnEventType.EV_TOMB_DESTROYED,
					EventMessage = TurnEventMessage.EM_TOMB_DESTROYED,
					PlayerID = killerPlayerID,
					SystemID = fleet.SystemID,
					TurnNumber = game.GameDatabase.GetTurnCount(),
					ShowsDialog = false
				});
			else
				game.GameDatabase.InsertTurnEvent(new TurnEvent()
				{
					EventType = TurnEventType.EV_FLEET_DESTROYED,
					EventMessage = TurnEventMessage.EM_FLEET_DESTROYED,
					PlayerID = killerPlayerID,
					SystemID = fleet.SystemID,
					FleetID = fleet.ID,
					TurnNumber = game.GameDatabase.GetTurnCount(),
					ShowsDialog = false
				});
			GameTrigger.PushEvent(EventType.EVNT_FLEETDIED, (object)fleet.Name, game);
		}

		private static void CheckFleetCommandPoints(
		  GameSession game,
		  FleetInfo fleet,
		  List<ShipInfo> ships)
		{
			if (game.GameDatabase.GetFactionName(game.GameDatabase.GetPlayerFactionID(fleet.PlayerID)) == "loa" || ships.Count == 0 || (fleet.Type != FleetType.FL_NORMAL || ships.Max<ShipInfo>((Func<ShipInfo, int>)(x => game.GameDatabase.GetShipCommandPointQuota(x.ID))) != 0))
				return;
			int num = game.GameDatabase.InsertFleet(fleet.PlayerID, 0, fleet.SystemID, fleet.SupportingSystemID, App.Localize("@FLEET_RETREAT"), FleetType.FL_NORMAL);
			int missionID = game.GameDatabase.InsertMission(num, MissionType.RETREAT, 0, 0, 0, 0, false, new int?());
			game.GameDatabase.InsertWaypoint(missionID, WaypointType.ReturnHome, new int?());
			game.GameDatabase.InsertWaypoint(missionID, WaypointType.DisbandFleet, new int?());
			foreach (ShipInfo ship in ships)
				game.GameDatabase.TransferShip(ship.ID, num);
			game.GameDatabase.RemoveFleet(fleet.ID);
		}
	}
}
