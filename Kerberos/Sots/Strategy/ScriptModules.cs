// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.ScriptModules
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.StarMapFramework;
using Kerberos.Sots.Encounters;
using Kerberos.Sots.Framework;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Strategy
{
	internal class ScriptModules
	{
		public VonNeumann VonNeumann { get; private set; }

		public Swarmers Swarmers { get; private set; }

		public Gardeners Gardeners { get; private set; }

		public AsteroidMonitor AsteroidMonitor { get; private set; }

		public MorrigiRelic MorrigiRelic { get; private set; }

		public Slaver Slaver { get; private set; }

		public Pirates Pirates { get; private set; }

		public Spectre Spectre { get; private set; }

		public GhostShip GhostShip { get; private set; }

		public MeteorShower MeteorShower { get; private set; }

		public SystemKiller SystemKiller { get; private set; }

		public Locust Locust { get; private set; }

		public Comet Comet { get; private set; }

		public NeutronStar NeutronStar { get; private set; }

		public SuperNova SuperNova { get; private set; }

		public static ScriptModules New(
		  Random random,
		  GameDatabase db,
		  AssetDatabase assetdb,
		  GameSession game,
		  NamesPool namesPool,
		  GameSetup gameSetup)
		{
			ScriptModules scriptModules = new ScriptModules();
			scriptModules.VonNeumann = VonNeumann.InitializeEncounter(db, assetdb);
			scriptModules.Swarmers = Swarmers.InitializeEncounter(db, assetdb);
			scriptModules.Gardeners = Gardeners.InitializeEncounter(db, assetdb);
			scriptModules.AsteroidMonitor = AsteroidMonitor.InitializeEncounter(db, assetdb);
			scriptModules.MorrigiRelic = MorrigiRelic.InitializeEncounter(db, assetdb);
			scriptModules.Slaver = Slaver.InitializeEncounter(db, assetdb);
			scriptModules.Pirates = Pirates.InitializeEncounter(db, assetdb);
			scriptModules.Spectre = Spectre.InitializeEncounter(db, assetdb);
			scriptModules.GhostShip = GhostShip.InitializeEncounter(db, assetdb);
			scriptModules.MeteorShower = MeteorShower.InitializeEncounter(db, assetdb);
			scriptModules.SystemKiller = SystemKiller.InitializeEncounter(db, assetdb);
			scriptModules.Locust = Locust.InitializeEncounter(db, assetdb);
			scriptModules.Comet = Comet.InitializeEncounter(db, assetdb);
			if (db.HasEndOfFleshExpansion())
			{
				scriptModules.NeutronStar = NeutronStar.InitializeEncounter(db, assetdb);
				scriptModules.SuperNova = SuperNova.InitializeEncounter();
			}
			scriptModules.AddEasterEggs(random, db, assetdb, game, namesPool, gameSetup);
			return scriptModules;
		}

		public static ScriptModules Resume(GameDatabase db)
		{
			ScriptModules scriptModules = new ScriptModules();
			scriptModules.VonNeumann = VonNeumann.ResumeEncounter(db);
			scriptModules.Swarmers = Swarmers.ResumeEncounter(db);
			scriptModules.Gardeners = Gardeners.ResumeEncounter(db);
			scriptModules.AsteroidMonitor = AsteroidMonitor.ResumeEncounter(db);
			scriptModules.MorrigiRelic = MorrigiRelic.ResumeEncounter(db);
			scriptModules.Slaver = Slaver.ResumeEncounter(db);
			scriptModules.Pirates = Pirates.ResumeEncounter(db);
			scriptModules.Spectre = Spectre.ResumeEncounter(db);
			scriptModules.GhostShip = GhostShip.ResumeEncounter(db);
			scriptModules.MeteorShower = MeteorShower.ResumeEncounter(db);
			scriptModules.SystemKiller = SystemKiller.ResumeEncounter(db);
			scriptModules.Locust = Locust.ResumeEncounter(db);
			scriptModules.Comet = Comet.ResumeEncounter(db);
			if (db.HasEndOfFleshExpansion())
			{
				scriptModules.NeutronStar = NeutronStar.ResumeEncounter(db);
				scriptModules.SuperNova = SuperNova.ResumeEncounter();
			}
			List<PlayerInfo> list = db.GetPlayerInfos().Where<PlayerInfo>((Func<PlayerInfo, bool>)(x =>
		   {
			   if (!x.isStandardPlayer)
				   return !x.includeInDiplomacy;
			   return false;
		   })).ToList<PlayerInfo>();
			foreach (int playerID in db.GetStandardPlayerIDs().ToList<int>())
			{
				foreach (PlayerInfo playerInfo in list)
				{
					DiplomacyInfo diplomacyInfo = db.GetDiplomacyInfo(playerID, playerInfo.ID);
					if (diplomacyInfo.State != DiplomacyState.WAR)
						db.UpdateDiplomacyState(playerID, playerInfo.ID, DiplomacyState.WAR, diplomacyInfo.Relations, true);
				}
			}
			return scriptModules;
		}

		public void UpdateEasterEggs(GameSession game)
		{
			foreach (EncounterInfo encounterInfo in game.GameDatabase.GetEncounterInfos().ToList<EncounterInfo>())
			{
				switch (encounterInfo.Type)
				{
					case EasterEgg.EE_SWARM:
						if (this.Swarmers != null)
						{
							this.Swarmers.UpdateTurn(game, encounterInfo.Id);
							continue;
						}
						continue;
					case EasterEgg.EE_ASTEROID_MONITOR:
						if (this.AsteroidMonitor != null)
						{
							this.AsteroidMonitor.UpdateTurn(game, encounterInfo.Id);
							continue;
						}
						continue;
					case EasterEgg.EE_VON_NEUMANN:
						if (this.VonNeumann != null)
						{
							this.VonNeumann.UpdateTurn(game, encounterInfo.Id);
							continue;
						}
						continue;
					case EasterEgg.EE_GARDENERS:
					case EasterEgg.GM_GARDENER:
						if (this.Gardeners != null)
						{
							this.Gardeners.UpdateTurn(game, encounterInfo.Id);
							continue;
						}
						continue;
					case EasterEgg.EE_MORRIGI_RELIC:
						if (this.MorrigiRelic != null)
						{
							this.MorrigiRelic.UpdateTurn(game, encounterInfo.Id);
							continue;
						}
						continue;
					case EasterEgg.GM_SYSTEM_KILLER:
						if (this.SystemKiller != null)
						{
							this.SystemKiller.UpdateTurn(game, encounterInfo.Id);
							continue;
						}
						continue;
					case EasterEgg.GM_LOCUST_SWARM:
						if (this.Locust != null)
						{
							this.Locust.UpdateTurn(game, encounterInfo.Id);
							continue;
						}
						continue;
					case EasterEgg.GM_NEUTRON_STAR:
						if (this.NeutronStar != null)
						{
							this.NeutronStar.UpdateTurn(game, encounterInfo.Id);
							continue;
						}
						continue;
					case EasterEgg.GM_SUPER_NOVA:
						if (this.SuperNova != null)
						{
							this.SuperNova.UpdateTurn(game, encounterInfo.Id);
							continue;
						}
						continue;
					default:
						continue;
				}
			}
		}

		private void AddEasterEggs(
		  Random random,
		  GameDatabase gamedb,
		  AssetDatabase assetdb,
		  GameSession game,
		  NamesPool namesPool,
		  GameSetup gameSetup)
		{
			List<StarSystemInfo> list1 = gamedb.GetStarSystemInfos().ToList<StarSystemInfo>();
			foreach (StarSystemInfo starSystemInfo in new List<StarSystemInfo>((IEnumerable<StarSystemInfo>)list1))
			{
				List<OrbitalObjectInfo> list2 = gamedb.GetStarSystemOrbitalObjectInfos(starSystemInfo.ID).ToList<OrbitalObjectInfo>();
				if (list2.Count<OrbitalObjectInfo>() == 0)
					list1.Remove(starSystemInfo);
				bool flag = false;
				foreach (OrbitalObjectInfo orbitalObjectInfo in list2)
				{
					if (gamedb.GetColonyInfoForPlanet(orbitalObjectInfo.ID) != null)
					{
						flag = true;
						break;
					}
				}
				if (flag)
					list1.Remove(starSystemInfo);
			}
			using (List<StarSystemInfo>.Enumerator enumerator = list1.GetEnumerator())
			{
			label_43:
				while (enumerator.MoveNext())
				{
					StarSystemInfo current = enumerator.Current;
					foreach (OrbitalObjectInfo orbit in gamedb.GetStarSystemOrbitalObjectInfos(current.ID).ToList<OrbitalObjectInfo>())
					{
						PlanetInfo planetInfo = gamedb.GetPlanetInfo(orbit.ID);
						if (planetInfo != null && !(planetInfo.Type == "gaseous") && (gamedb.GetLargeAsteroidInfo(orbit.ID) == null && gamedb.GetAsteroidBeltInfo(orbit.ID) == null) && random.CoinToss((double)assetdb.RandomEncOddsPerOrbital * ((double)gameSetup._randomEncounterFrequency / 100.0)))
						{
							int maxValue = game.GetAvailableEEOdds().Sum<KeyValuePair<EasterEgg, int>>((Func<KeyValuePair<EasterEgg, int>, int>)(x => x.Value));
							if (maxValue == 0)
								return;
							int num1 = random.Next(maxValue);
							int num2 = 0;
							EasterEgg easterEgg = EasterEgg.EE_SWARM;
							foreach (KeyValuePair<EasterEgg, int> easterEggOdd in assetdb.EasterEggOdds)
							{
								num2 += easterEggOdd.Value;
								if (num2 > num1)
								{
									easterEgg = easterEggOdd.Key;
									break;
								}
							}
							App.Log.Warn(string.Format("Spawning {0} at {1}", (object)easterEgg.ToString(), (object)current.ID), nameof(game));
							switch (easterEgg)
							{
								case EasterEgg.EE_SWARM:
									if (this.Swarmers != null)
									{
										this.Swarmers.AddInstance(gamedb, assetdb, current.ID, orbit.ID);
										goto label_43;
									}
									else
										goto label_43;
								case EasterEgg.EE_ASTEROID_MONITOR:
									if (this.AsteroidMonitor != null)
									{
										this.AsteroidMonitor.AddInstance(gamedb, assetdb, current.ID, orbit.ID);
										goto label_43;
									}
									else
										goto label_43;
								case EasterEgg.EE_PIRATE_BASE:
									if (this.Pirates != null)
									{
										this.Pirates.AddInstance(gamedb, assetdb, game, current.ID, orbit.ID);
										goto label_43;
									}
									else
										goto label_43;
								case EasterEgg.EE_VON_NEUMANN:
									if (this.VonNeumann != null)
									{
										this.VonNeumann.AddInstance(gamedb, assetdb, namesPool);
										goto label_43;
									}
									else
										goto label_43;
								case EasterEgg.EE_GARDENERS:
									if (this.Gardeners != null)
									{
										this.Gardeners.AddInstance(gamedb, assetdb, current.ID);
										goto label_43;
									}
									else
										goto label_43;
								case EasterEgg.EE_INDEPENDENT:
									ScriptModules.InsertIndependentSystem(random, current, orbit, gamedb, assetdb);
									goto label_43;
								case EasterEgg.EE_MORRIGI_RELIC:
									if (this.MorrigiRelic != null)
									{
										this.MorrigiRelic.AddInstance(gamedb, assetdb, current.ID, orbit.ID);
										goto label_43;
									}
									else
										goto label_43;
								default:
									goto label_43;
							}
						}
					}
				}
			}
		}

		private static void GenerateIndependentRace(
		  Random random,
		  StarSystemInfo system,
		  OrbitalObjectInfo orbit,
		  GameDatabase gamedb,
		  AssetDatabase assetdb)
		{
			List<Faction> list = assetdb.Factions.Where<Faction>((Func<Faction, bool>)(x => x.IsIndependent())).ToList<Faction>();
			List<PlayerInfo> players = gamedb.GetPlayerInfos().ToList<PlayerInfo>();
			players.RemoveAll((Predicate<PlayerInfo>)(x =>
		   {
			   if (!x.isStandardPlayer)
				   return !x.includeInDiplomacy;
			   return true;
		   }));
			list.RemoveAll((Predicate<Faction>)(x =>
		   {
			   if (x.IndyDescrition != null)
				   return players.Any<PlayerInfo>((Func<PlayerInfo, bool>)(y => y.Name == x.Name));
			   return true;
		   }));
			if (list.Count == 0)
				return;
			Faction faction1 = random.Choose<Faction>((IList<Faction>)list);
			IndyDesc indyDescrition = faction1.IndyDescrition;
			double num1 = indyDescrition.TechLevel != 1 ? (indyDescrition.TechLevel != 2 ? (indyDescrition.TechLevel != 3 ? (double)random.NextInclusive(1750, 10000) * (double)indyDescrition.BasePopulationMod * 1000000.0 : (double)random.NextInclusive(750, 2000) * (double)indyDescrition.BasePopulationMod * 1000000.0) : (double)random.NextInclusive(1, 750) * (double)indyDescrition.BasePopulationMod * 1000000.0) : (double)random.NextInclusive(30, 200) * (double)indyDescrition.BasePopulationMod * 1000.0;
			FactionInfo factionInfo = gamedb.GetFactionInfo(faction1.ID);
			factionInfo.IdealSuitability = gamedb.GetFactionSuitability(indyDescrition.BaseFactionSuitability) + random.NextInclusive(-indyDescrition.Suitability, indyDescrition.Suitability);
			gamedb.UpdateFaction(factionInfo);
			gamedb.RemoveOrbitalObject(orbit.ID);
			PlanetOrbit planetOrbit = new PlanetOrbit();
			if (indyDescrition.MinPlanetSize != 0 && indyDescrition.MaxPlanetSize != 0)
				planetOrbit.Size = new int?(random.NextInclusive(indyDescrition.MinPlanetSize, indyDescrition.MaxPlanetSize));
			PlanetInfo pi1 = StarSystemHelper.InferPlanetInfo((Kerberos.Sots.Data.StarMapFramework.Orbit)planetOrbit);
			pi1.Suitability = factionInfo.IdealSuitability;
			pi1.Biosphere = (int)((double)pi1.Biosphere * (double)indyDescrition.BiosphereMod);
			pi1.ID = gamedb.InsertPlanet(orbit.ParentID, orbit.StarSystemID, orbit.OrbitalPath, orbit.Name, indyDescrition.StellarBodyType, new int?(), pi1.Suitability, pi1.Biosphere, pi1.Resources, pi1.Size);
			double num2 = Math.Min(num1 + (double)(1000 * pi1.Biosphere), Colony.GetMaxCivilianPop(gamedb, pi1));
			string avatarPath = "";
			if (((IEnumerable<string>)faction1.AvatarTexturePaths).Count<string>() > 0)
				avatarPath = faction1.AvatarTexturePaths[0];
			int insertIndyPlayerId = gamedb.GetOrInsertIndyPlayerId(gamedb, faction1.ID, faction1.Name, avatarPath);
			players = gamedb.GetPlayerInfos().ToList<PlayerInfo>();
			foreach (PlayerInfo playerInfo in players)
			{
				PlayerInfo pi = playerInfo;
				Faction faction2 = assetdb.Factions.FirstOrDefault<Faction>((Func<Faction, bool>)(x => x.ID == pi.FactionID));
				gamedb.InsertDiplomaticState(insertIndyPlayerId, pi.ID, pi.isStandardPlayer || pi.includeInDiplomacy ? DiplomacyState.NEUTRAL : DiplomacyState.WAR, faction1.GetDefaultReactionToFaction(faction2), false, true);
			}
			gamedb.InsertColony(pi1.ID, insertIndyPlayerId, num2 / 2.0, 0.5f, 0, 1f, true);
			gamedb.InsertColonyFaction(pi1.ID, faction1.ID, num2 / 2.0, 1f, 0);
			if (indyDescrition.TechLevel < 4)
				return;
			foreach (PlanetInfo systemPlanetInfo in gamedb.GetStarSystemPlanetInfos(system.ID))
			{
				if (systemPlanetInfo.ID != pi1.ID)
				{
					PlanetInfo planetInfo = gamedb.GetPlanetInfo(systemPlanetInfo.ID);
					float num3 = Math.Abs(factionInfo.IdealSuitability - planetInfo.Suitability);
					if ((double)num3 < 200.0)
					{
						double impPop = (double)(random.NextInclusive(100, 200) * 100) * (double)indyDescrition.BasePopulationMod;
						gamedb.InsertColony(pi1.ID, insertIndyPlayerId, impPop, 0.5f, 0, 1f, true);
						gamedb.InsertColonyFaction(pi1.ID, faction1.ID, impPop / 2.0, 1f, 0);
					}
					else if ((double)num3 < 600.0 && (double)planetInfo.Suitability != 0.0)
					{
						float num4 = 100f + (float)random.Next(150);
						if (random.Next(2) == 0)
							num4 *= -1f;
						planetInfo.Suitability = factionInfo.IdealSuitability + num4;
						gamedb.UpdatePlanet(planetInfo);
						double num5 = (double)(random.NextInclusive(50, 100) * 100) * (double)indyDescrition.BasePopulationMod;
						gamedb.InsertColony(pi1.ID, insertIndyPlayerId, num5 / 2.0, 0.5f, 0, 1f, true);
						gamedb.InsertColonyFaction(pi1.ID, faction1.ID, num5 / 2.0, 1f, 0);
					}
				}
			}
		}

		private static void InsertIndependentSystem(
		  Random random,
		  StarSystemInfo system,
		  OrbitalObjectInfo orbit,
		  GameDatabase gamedb,
		  AssetDatabase assetdb)
		{
			ScriptModules.GenerateIndependentRace(random, system, orbit, gamedb, assetdb);
		}

		public bool IsEncounterPlayer(int playerID)
		{
			return this.VonNeumann.PlayerID == playerID || this.Swarmers.PlayerID == playerID || (this.Gardeners.PlayerID == playerID || this.AsteroidMonitor.PlayerID == playerID) || (this.MorrigiRelic.PlayerID == playerID || this.Slaver.PlayerID == playerID || (this.Pirates.PlayerID == playerID || this.Spectre.PlayerID == playerID)) || (this.GhostShip.PlayerID == playerID || this.MeteorShower.PlayerID == playerID || (this.SystemKiller.PlayerID == playerID || this.Locust.PlayerID == playerID) || this.Comet.PlayerID == playerID);
		}

		public EasterEgg GetEasterEggTypeForPlayer(int playerID)
		{
			if (this.VonNeumann.PlayerID == playerID)
				return EasterEgg.EE_VON_NEUMANN;
			if (this.Swarmers.PlayerID == playerID)
				return EasterEgg.EE_SWARM;
			if (this.Gardeners.PlayerID == playerID)
				return EasterEgg.EE_GARDENERS;
			if (this.AsteroidMonitor.PlayerID == playerID)
				return EasterEgg.EE_ASTEROID_MONITOR;
			if (this.MorrigiRelic.PlayerID == playerID)
				return EasterEgg.EE_MORRIGI_RELIC;
			if (this.SystemKiller.PlayerID == playerID)
				return EasterEgg.GM_SYSTEM_KILLER;
			if (this.Locust.PlayerID == playerID)
				return EasterEgg.GM_LOCUST_SWARM;
			if (this.Comet.PlayerID == playerID)
				return EasterEgg.GM_COMET;
			return this.Pirates.PlayerID == playerID ? EasterEgg.EE_PIRATE_BASE : EasterEgg.UNKNOWN;
		}
	}
}
