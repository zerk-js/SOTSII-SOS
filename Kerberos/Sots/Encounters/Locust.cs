// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Encounters.Locust
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Encounters
{
	internal class Locust
	{
		private int PlayerId = -1;
		private const string FactionName = "locusts";
		private const string PlayerName = "Locust Swarm";
		private const string PlayerAvatar = "\\base\\factions\\locusts\\avatars\\Locusts_Avatar.tga";
		private const string FleetName = "Locust Swarm";
		private const string ScoutFleetName = "Locust Swarm Scout";
		private const string WorldShipDesignFile = "lv_locust_worldship.section";
		private const string HeraldMoonDesignFile = "dn_locust_heraldmoon.section";
		private const string NeedleShipDesignFile = "locust_needleship.section";
		private int _worldShipDesignId;
		private int _heraldMoonDesignId;
		private int _needleShipDesignId;

		public int PlayerID
		{
			get
			{
				return this.PlayerId;
			}
		}

		public int WorldShipDesignId
		{
			get
			{
				return this._worldShipDesignId;
			}
		}

		public int HeraldMoonDesignId
		{
			get
			{
				return this._heraldMoonDesignId;
			}
		}

		public int NeedleShipDesignId
		{
			get
			{
				return this._needleShipDesignId;
			}
		}

		public static Locust InitializeEncounter(GameDatabase gamedb, AssetDatabase assetdb)
		{
			Locust locust = new Locust();
			locust.PlayerId = gamedb.InsertPlayer("Locust Swarm", "locusts", new int?(), assetdb.RandomEncounterPrimaryColor, new Vector3(0.0f), "", "\\base\\factions\\locusts\\avatars\\Locusts_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal);
			DesignInfo design1 = new DesignInfo(locust.PlayerId, "World Ship", new string[1]
			{
		string.Format("factions\\{0}\\sections\\{1}", (object) "locusts", (object) "lv_locust_worldship.section")
			});
			DesignInfo design2 = new DesignInfo(locust.PlayerId, "Herald Moon", new string[1]
			{
		string.Format("factions\\{0}\\sections\\{1}", (object) "locusts", (object) "dn_locust_heraldmoon.section")
			});
			DesignInfo design3 = new DesignInfo(locust.PlayerId, "Needle Ship", new string[1]
			{
		string.Format("factions\\{0}\\sections\\{1}", (object) "locusts", (object) "locust_needleship.section")
			});
			locust._worldShipDesignId = gamedb.InsertDesignByDesignInfo(design1);
			locust._heraldMoonDesignId = gamedb.InsertDesignByDesignInfo(design2);
			locust._needleShipDesignId = gamedb.InsertDesignByDesignInfo(design3);
			return locust;
		}

		public static Locust ResumeEncounter(GameDatabase gamedb)
		{
			Locust locust = new Locust();
			PlayerInfo playerInfo = gamedb.GetPlayerInfos().ToList<PlayerInfo>().FirstOrDefault<PlayerInfo>((Func<PlayerInfo, bool>)(x =>
		   {
			   if (!x.isStandardPlayer)
				   return x.Name.Contains("Locust Swarm");
			   return false;
		   }));
			locust.PlayerId = playerInfo.ID;
			List<DesignInfo> list = gamedb.GetDesignInfosForPlayer(locust.PlayerId).ToList<DesignInfo>();
			locust._worldShipDesignId = list.First<DesignInfo>((Func<DesignInfo, bool>)(x => x.DesignSections[0].FilePath.EndsWith("lv_locust_worldship.section"))).ID;
			locust._heraldMoonDesignId = list.First<DesignInfo>((Func<DesignInfo, bool>)(x => x.DesignSections[0].FilePath.EndsWith("dn_locust_heraldmoon.section"))).ID;
			locust._needleShipDesignId = list.First<DesignInfo>((Func<DesignInfo, bool>)(x => x.DesignSections[0].FilePath.EndsWith("locust_needleship.section"))).ID;
			return locust;
		}

		public void ExecuteInstance(GameDatabase gamedb, AssetDatabase assetdb, int systemid)
		{
			int fleetId = gamedb.InsertFleet(this.PlayerId, 0, systemid, systemid, "Locust Swarm", FleetType.FL_NORMAL);
			gamedb.InsertLocustSwarmInfo(new LocustSwarmInfo()
			{
				FleetId = new int?(fleetId),
				NumDrones = assetdb.GlobalLocustData.MaxDrones
			});
			gamedb.InsertShip(fleetId, this._worldShipDesignId, null, (ShipParams)0, new int?(), 0);
			if (!gamedb.HasEndOfFleshExpansion())
				return;
			int id = gamedb.GetLocustSwarmInfos().First<LocustSwarmInfo>((Func<LocustSwarmInfo, bool>)(x =>
		   {
			   int? fleetId1 = x.FleetId;
			   int num = fleetId;
			   if (fleetId1.GetValueOrDefault() == num)
				   return fleetId1.HasValue;
			   return false;
		   })).Id;
			for (int index = 0; index < assetdb.GlobalLocustData.InitialLocustScouts; ++index)
				gamedb.InsertLocustSwarmScoutInfo(new LocustSwarmScoutInfo()
				{
					LocustInfoId = id,
					NumDrones = assetdb.GlobalLocustData.MaxDrones,
					TargetSystemId = systemid,
					ShipId = gamedb.InsertShip(fleetId, this._heraldMoonDesignId, null, (ShipParams)0, new int?(), 0)
				});
		}

		public void AddInstance(GameDatabase gamedb, AssetDatabase assetdb, int? targetSystem = null)
		{
			Random safeRandom = App.GetSafeRandom();
			int id;
			if (!targetSystem.HasValue)
			{
				List<KeyValuePair<StarSystemInfo, Vector3>> list = EncounterTools.GetOutlyingStars(gamedb).ToList<KeyValuePair<StarSystemInfo, Vector3>>();
				List<KeyValuePair<StarSystemInfo, Vector3>> range = list.GetRange(0, (int)Math.Ceiling((double)list.Count / 3.0));
				if (range.Count == 0)
					return;
				id = safeRandom.Choose<KeyValuePair<StarSystemInfo, Vector3>>((IList<KeyValuePair<StarSystemInfo, Vector3>>)range).Key.ID;
			}
			else
				id = targetSystem.Value;
			gamedb.InsertIncomingGM(id, EasterEgg.GM_LOCUST_SWARM, gamedb.GetTurnCount() + 1);
			foreach (PlayerInfo playerInfo in gamedb.GetStandardPlayerInfos().ToList<PlayerInfo>())
			{
				if (gamedb.GetStratModifier<int>(StratModifiers.GrandMenaceWarningTime, playerInfo.ID) > 0)
					gamedb.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_INCOMING_LOCUST,
						EventMessage = TurnEventMessage.EM_INCOMING_LOCUST,
						PlayerID = playerInfo.ID,
						TurnNumber = gamedb.GetTurnCount()
					});
			}
		}

		public void UpdateTurn(GameSession game, int id)
		{
			LocustSwarmInfo locustSwarmInfo1 = game.GameDatabase.GetLocustSwarmInfo(id);
			if (locustSwarmInfo1 == null || !locustSwarmInfo1.FleetId.HasValue)
			{
				game.GameDatabase.RemoveEncounter(id);
			}
			else
			{
				FleetInfo fleetInfo1 = game.GameDatabase.GetFleetInfo(locustSwarmInfo1.FleetId.Value);
				StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(fleetInfo1.SystemID);
				if (starSystemInfo != (StarSystemInfo)null && !starSystemInfo.IsDeepSpace)
					locustSwarmInfo1 = this.SpendResources(game, locustSwarmInfo1, fleetInfo1);
				game.GameDatabase.GetMissionByFleetID(fleetInfo1.ID);
				if (game.GameDatabase.GetMoveOrderInfoByFleetID(fleetInfo1.ID) != null || game.isHostilesAtSystem(this.PlayerId, fleetInfo1.SystemID))
				{
					game.GameDatabase.UpdateLocustSwarmInfo(locustSwarmInfo1);
				}
				else
				{
					if (locustSwarmInfo1.Resources >= game.AssetDatabase.GlobalLocustData.MinResourceSpawnAmount)
					{
						this.AddInstance(game.GameDatabase, game.AssetDatabase, new int?(starSystemInfo.ID));
						locustSwarmInfo1.Resources -= game.AssetDatabase.GlobalLocustData.MinResourceSpawnAmount;
					}
					int num1 = 0;
					List<PlanetInfo> list = ((IEnumerable<PlanetInfo>)game.GameDatabase.GetStarSystemPlanetInfos(starSystemInfo.ID)).ToList<PlanetInfo>();
					foreach (PlanetInfo planet in list)
					{
						int num2 = Math.Min(planet.Resources, game.AssetDatabase.GlobalLocustData.MaxSalvageRate - num1);
						planet.Resources -= num2;
						num1 += num2;
						game.GameDatabase.UpdatePlanet(planet);
						if (num1 == game.AssetDatabase.GlobalLocustData.MaxSalvageRate)
							break;
					}
					locustSwarmInfo1.Resources += num1;
					int num3 = list.Sum<PlanetInfo>((Func<PlanetInfo, int>)(x => x.Resources));
					bool flag = false;
					foreach (LocustSwarmInfo locustSwarmInfo2 in game.GameDatabase.GetLocustSwarmInfos().ToList<LocustSwarmInfo>())
					{
						if (locustSwarmInfo2.Id != id && id >= locustSwarmInfo2.Id && locustSwarmInfo2.FleetId.HasValue)
						{
							FleetInfo fleetInfo2 = game.GameDatabase.GetFleetInfo(locustSwarmInfo2.FleetId.Value);
							if (fleetInfo2 != null && fleetInfo2.SystemID == fleetInfo1.SystemID)
							{
								flag = true;
								break;
							}
						}
					}
					if (game.GameDatabase.HasEndOfFleshExpansion())
					{
						bool nestWaitingToGroupUp = num3 == 0 && game.GameDatabase.GetLocustSwarmScoutTargetInfos().Count<LocustSwarmScoutTargetInfo>() > 0 || flag;
						this.UpdateScoutMission(game, locustSwarmInfo1, fleetInfo1, starSystemInfo, nestWaitingToGroupUp);
					}
					if ((num3 == 0 || flag) && !this.SetNextWorldTarget(game, locustSwarmInfo1, fleetInfo1, starSystemInfo))
					{
						game.GameDatabase.RemoveFleet(fleetInfo1.ID);
						game.GameDatabase.RemoveEncounter(locustSwarmInfo1.Id);
					}
					else
						game.GameDatabase.UpdateLocustSwarmInfo(locustSwarmInfo1);
				}
			}
		}

		private LocustSwarmInfo SpendResources(
		  GameSession game,
		  LocustSwarmInfo info,
		  FleetInfo fi)
		{
			foreach (ShipInfo ship in game.GameDatabase.GetShipInfoByFleetID(fi.ID, true).ToList<ShipInfo>())
			{
				int[] healthAndHealthMax = Kerberos.Sots.StarFleet.StarFleet.GetHealthAndHealthMax(game, ship.DesignInfo, ship.ID);
				int points = Math.Min(healthAndHealthMax[1] - healthAndHealthMax[0], info.Resources);
				if (points > 0)
				{
					info.Resources -= points;
					Kerberos.Sots.StarFleet.StarFleet.RepairShip(game.App, ship, points);
				}
			}
			if (info.NumDrones < game.AssetDatabase.GlobalLocustData.MaxDrones)
			{
				int num = Math.Min(game.AssetDatabase.GlobalLocustData.MaxDrones - info.NumDrones, info.Resources / game.AssetDatabase.GlobalLocustData.DroneCost);
				info.NumDrones += num;
				info.Resources -= num * game.AssetDatabase.GlobalLocustData.DroneCost;
			}
			if (game.GameDatabase.HasEndOfFleshExpansion() && info.Resources > 0)
			{
				List<LocustSwarmScoutInfo> list = game.GameDatabase.GetLocustSwarmScoutsForLocustNest(info.Id).ToList<LocustSwarmScoutInfo>();
				int num = Math.Max(game.AssetDatabase.GlobalLocustData.MinLocustScouts - list.Count, 0);
				info.Resources = this.RepairScouts(game, list, info.Resources);
				info.Resources = this.RepairScoutDrones(game, list, info.Resources);
				for (int index = 0; index < num && info.Resources >= game.AssetDatabase.GlobalLocustData.LocustScoutCost; ++index)
				{
					game.GameDatabase.InsertLocustSwarmScoutInfo(new LocustSwarmScoutInfo()
					{
						LocustInfoId = info.Id,
						NumDrones = game.AssetDatabase.GlobalLocustData.MaxDrones,
						TargetSystemId = fi.SystemID,
						ShipId = game.GameDatabase.InsertShip(info.FleetId.Value, this._heraldMoonDesignId, null, (ShipParams)0, new int?(), 0)
					});
					info.Resources -= game.AssetDatabase.GlobalLocustData.LocustScoutCost;
				}
			}
			return info;
		}

		private int RepairScouts(GameSession game, List<LocustSwarmScoutInfo> scouts, int resources)
		{
			if (resources > 0 && scouts.Count > 0)
			{
				foreach (LocustSwarmScoutInfo scout in scouts)
				{
					ShipInfo shipInfo = game.GameDatabase.GetShipInfo(scout.ShipId, true);
					int[] healthAndHealthMax = Kerberos.Sots.StarFleet.StarFleet.GetHealthAndHealthMax(game, shipInfo.DesignInfo, shipInfo.ID);
					int points = Math.Min(healthAndHealthMax[1] - healthAndHealthMax[0], resources);
					if (points > 0)
					{
						resources -= points;
						Kerberos.Sots.StarFleet.StarFleet.RepairShip(game.App, shipInfo, points);
					}
					if (resources == 0)
						break;
				}
			}
			return resources;
		}

		private int RepairScoutDrones(
		  GameSession game,
		  List<LocustSwarmScoutInfo> scouts,
		  int resources)
		{
			if (resources > 0 && scouts.Count > 0)
			{
				scouts.Sort((Comparison<LocustSwarmScoutInfo>)((x, y) => x.NumDrones.CompareTo(y.NumDrones)));
				List<LocustSwarmScoutInfo> locustSwarmScoutInfoList = new List<LocustSwarmScoutInfo>();
				int val2 = 10;
				int num1 = game.AssetDatabase.GlobalLocustData.MaxDrones / val2 + 1;
				for (bool flag = false; !flag && num1 > 0; --num1)
				{
					locustSwarmScoutInfoList.Clear();
					foreach (LocustSwarmScoutInfo scout in scouts)
					{
						if (resources > 0)
						{
							int val1 = Math.Min(game.AssetDatabase.GlobalLocustData.MaxDrones - scout.NumDrones, val2);
							if (scout.NumDrones < game.AssetDatabase.GlobalLocustData.MaxDrones)
							{
								int num2 = Math.Min(val1, resources / game.AssetDatabase.GlobalLocustData.DroneCost);
								scout.NumDrones += num2;
								resources -= num2 * game.AssetDatabase.GlobalLocustData.DroneCost;
							}
						}
						if (scout.NumDrones == game.AssetDatabase.GlobalLocustData.MaxDrones || resources == 0)
							locustSwarmScoutInfoList.Add(scout);
					}
					foreach (LocustSwarmScoutInfo ls in locustSwarmScoutInfoList)
					{
						scouts.Remove(ls);
						game.GameDatabase.UpdateLocustSwarmScoutInfo(ls);
					}
					flag = scouts.Count == 0 || resources == 0;
				}
			}
			return resources;
		}

		private void UpdateScoutMission(
		  GameSession game,
		  LocustSwarmInfo info,
		  FleetInfo fleet,
		  StarSystemInfo currentSystem,
		  bool nestWaitingToGroupUp)
		{
			List<LocustSwarmScoutInfo> list1 = game.GameDatabase.GetLocustSwarmScoutsForLocustNest(info.Id).ToList<LocustSwarmScoutInfo>();
			if (list1.Count == 0)
				return;
			List<int> scoutedSystems = new List<int>();
			List<LocustSwarmScoutTargetInfo> list2 = game.GameDatabase.GetLocustSwarmScoutTargetInfos().ToList<LocustSwarmScoutTargetInfo>();
			List<LocustSwarmScoutInfo> list3 = game.GameDatabase.GetLocustSwarmScoutInfos().ToList<LocustSwarmScoutInfo>();
			scoutedSystems.AddRange((IEnumerable<int>)list2.Select<LocustSwarmScoutTargetInfo, int>((Func<LocustSwarmScoutTargetInfo, int>)(x => x.SystemId)).ToList<int>());
			foreach (LocustSwarmScoutInfo locustSwarmScoutInfo in list3)
			{
				if (locustSwarmScoutInfo.TargetSystemId != 0 && !scoutedSystems.Contains(locustSwarmScoutInfo.TargetSystemId))
					scoutedSystems.Add(locustSwarmScoutInfo.TargetSystemId);
			}
			int num1 = list1.Where<LocustSwarmScoutInfo>((Func<LocustSwarmScoutInfo, bool>)(x => x.TargetSystemId == currentSystem.ID)).Count<LocustSwarmScoutInfo>();
			foreach (LocustSwarmScoutInfo locustSwarmScoutInfo in list1)
			{
				ShipInfo shipInfo = game.GameDatabase.GetShipInfo(locustSwarmScoutInfo.ShipId, false);
				if (nestWaitingToGroupUp)
				{
					if (shipInfo.FleetID != fleet.ID && game.GameDatabase.GetMissionByFleetID(shipInfo.FleetID) == null)
					{
						game.GameDatabase.TransferShip(locustSwarmScoutInfo.ShipId, fleet.ID);
						Vector3 vector3 = new Vector3();
						vector3.Y = 0.0f;
						float num2 = (float)(((num1 + 1) % 5 + 1) / 2);
						float num3 = (num1 + 1) % 2 == 0 ? 1f : -1f;
						vector3.Z = -300f * num2;
						vector3.X = num3 * 500f * num2;
						game.GameDatabase.UpdateShipFleetPosition(locustSwarmScoutInfo.ShipId, new Vector3?(vector3));
						locustSwarmScoutInfo.TargetSystemId = fleet.SystemID;
						game.GameDatabase.UpdateLocustSwarmScoutInfo(locustSwarmScoutInfo);
						game.GameDatabase.RemoveFleet(shipInfo.FleetID);
						++num1;
					}
				}
				else if (shipInfo.FleetID == fleet.ID || game.GameDatabase.GetMissionByFleetID(shipInfo.FleetID) == null)
				{
					int num2 = this.SetNextScoutTarget(game, info, currentSystem, locustSwarmScoutInfo, scoutedSystems);
					if (!scoutedSystems.Contains(num2))
						scoutedSystems.Add(num2);
				}
			}
		}

		private int SetNextScoutTarget(
		  GameSession game,
		  LocustSwarmInfo info,
		  StarSystemInfo currentSystem,
		  LocustSwarmScoutInfo scout,
		  List<int> scoutedSystems)
		{
			List<int> previousTargets = game.GameDatabase.GetLocustSwarmTargets().ToList<int>();
			List<StarSystemInfo> closestStars = EncounterTools.GetClosestStars(game.GameDatabase, currentSystem);
			closestStars.RemoveAll((Predicate<StarSystemInfo>)(x => previousTargets.Contains(x.ID)));
			closestStars.RemoveAll((Predicate<StarSystemInfo>)(x => scoutedSystems.Contains(x.ID)));
			if (closestStars.Count > 0)
			{
				scout.TargetSystemId = closestStars.First<StarSystemInfo>().ID;
				int num = game.GameDatabase.InsertFleet(this.PlayerId, 0, currentSystem.ID, currentSystem.ID, "Locust Swarm Scout", FleetType.FL_NORMAL);
				game.GameDatabase.TransferShip(scout.ShipId, num);
				int missionID = game.GameDatabase.InsertMission(num, MissionType.SURVEY, scout.TargetSystemId, 0, 0, 0, false, new int?());
				Kerberos.Sots.StarFleet.StarFleet.SetWaypointsForMission(game, MissionType.SURVEY, missionID, num, scout.TargetSystemId, 0, new int?(currentSystem.ID));
				game.GameDatabase.UpdateLocustSwarmScoutInfo(scout);
			}
			return scout.TargetSystemId;
		}

		private bool SetNextWorldTarget(
		  GameSession game,
		  LocustSwarmInfo info,
		  FleetInfo fleet,
		  StarSystemInfo currentSystem)
		{
			List<int> previousTargets = game.GameDatabase.GetLocustSwarmTargets().ToList<int>();
			List<StarSystemInfo> source = new List<StarSystemInfo>();
			if (game.GameDatabase.HasEndOfFleshExpansion())
			{
				List<LocustSwarmScoutInfo> list = game.GameDatabase.GetLocustSwarmScoutsForLocustNest(info.Id).ToList<LocustSwarmScoutInfo>();
				if (list.Count > 0)
				{
					bool flag1 = true;
					foreach (LocustSwarmScoutInfo locustSwarmScoutInfo in list)
					{
						ShipInfo shipInfo = game.GameDatabase.GetShipInfo(locustSwarmScoutInfo.ShipId, false);
						if (shipInfo != null)
						{
							int fleetId1 = shipInfo.FleetID;
							int? fleetId2 = info.FleetId;
							if ((fleetId1 != fleetId2.GetValueOrDefault() ? 1 : (!fleetId2.HasValue ? 1 : 0)) != 0)
							{
								flag1 = false;
								break;
							}
						}
					}
					if (!flag1)
						return true;
					bool flag2 = false;
					List<StarSystemInfo> starSystemInfoList = new List<StarSystemInfo>();
					foreach (LocustSwarmScoutTargetInfo swarmScoutTargetInfo in game.GameDatabase.GetLocustSwarmScoutTargetInfos().ToList<LocustSwarmScoutTargetInfo>())
					{
						if (swarmScoutTargetInfo.SystemId != currentSystem.ID)
						{
							flag2 = true;
							int num = 0;
							foreach (PlanetInfo planetInfo in ((IEnumerable<PlanetInfo>)game.GameDatabase.GetStarSystemPlanetInfos(swarmScoutTargetInfo.SystemId)).ToList<PlanetInfo>())
								num += planetInfo.Resources;
							if (num > 0)
							{
								StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(swarmScoutTargetInfo.SystemId);
								if (swarmScoutTargetInfo.IsHostile)
									starSystemInfoList.Add(starSystemInfo);
								else
									source.Add(starSystemInfo);
							}
						}
					}
					if (!flag2)
						source = EncounterTools.GetClosestStars(game.GameDatabase, currentSystem);
					if (source.Count == 0)
						source.AddRange((IEnumerable<StarSystemInfo>)starSystemInfoList);
					source.OrderBy<StarSystemInfo, float>((Func<StarSystemInfo, float>)(x => (x.Origin - currentSystem.Origin).LengthSquared));
				}
				else
					source = EncounterTools.GetClosestStars(game.GameDatabase, currentSystem);
			}
			else
				source = EncounterTools.GetClosestStars(game.GameDatabase, currentSystem);
			source.RemoveAll((Predicate<StarSystemInfo>)(x => previousTargets.Contains(x.ID)));
			if (source.Count <= 0)
				return false;
			game.GameDatabase.InsertLocustSwarmTarget(info.Id, fleet.SystemID);
			int id = source.First<StarSystemInfo>().ID;
			int missionID = game.GameDatabase.InsertMission(fleet.ID, MissionType.STRIKE, source.First<StarSystemInfo>().ID, 0, 0, 0, false, new int?());
			game.GameDatabase.InsertWaypoint(missionID, WaypointType.TravelTo, new int?(id));
			this.UpdateScoutedSystems(game, id);
			return true;
		}

		public void UpdateScoutedSystems(GameSession game, int systemID)
		{
			if (game.GameDatabase.GetLocustSwarmScoutTargetInfos().Any<LocustSwarmScoutTargetInfo>((Func<LocustSwarmScoutTargetInfo, bool>)(x => x.SystemId == systemID)))
				return;
			game.GameDatabase.InsertLocustSwarmScoutTargetInfo(new LocustSwarmScoutTargetInfo()
			{
				SystemId = systemID,
				IsHostile = game.isHostilesAtSystem(this.PlayerId, systemID)
			});
		}

		public static Matrix GetBaseEnemyFleetTrans(App app, int systemID)
		{
			return Locust.GetSpawnTransform(app, systemID);
		}

		public static Matrix GetSpawnTransform(App app, int systemId)
		{
			bool flag = false;
			float num1 = 0.0f;
			float num2 = 0.0f;
			OrbitalObjectInfo orbitalObjectInfo1 = (OrbitalObjectInfo)null;
			Vector3 vector3_1 = Vector3.Zero;
			Vector3? nullable = new Vector3?();
			foreach (OrbitalObjectInfo orbitalObjectInfo2 in app.GameDatabase.GetStarSystemOrbitalObjectInfos(systemId))
			{
				ColonyInfo colonyInfoForPlanet = app.GameDatabase.GetColonyInfoForPlanet(orbitalObjectInfo2.ID);
				if (!flag || colonyInfoForPlanet != null)
				{
					PlanetInfo planetInfo = app.GameDatabase.GetPlanetInfo(orbitalObjectInfo2.ID);
					float num3 = 1000f;
					if (planetInfo != null)
						num3 = StarSystemVars.Instance.SizeToRadius(planetInfo.Size);
					Vector3 position = app.GameDatabase.GetOrbitalTransform(orbitalObjectInfo2.ID).Position;
					float num4 = position.Length + num3;
					if ((double)num4 > (double)num1 || !flag && colonyInfoForPlanet != null)
					{
						orbitalObjectInfo1 = orbitalObjectInfo2;
						num1 = num4;
						flag = colonyInfoForPlanet != null;
						vector3_1 = position;
						num2 = num3 + 10000f;
						nullable = !orbitalObjectInfo2.ParentID.HasValue || orbitalObjectInfo2.ParentID.Value == 0 ? new Vector3?() : new Vector3?(app.GameDatabase.GetOrbitalTransform(orbitalObjectInfo2.ID).Position);
						if (flag)
							break;
					}
				}
			}
			if (orbitalObjectInfo1 == null)
				return Matrix.Identity;
			Vector3 vector3_2 = Vector3.Zero;
			if (nullable.HasValue)
			{
				Matrix world = Matrix.CreateWorld(Vector3.Zero, Vector3.Normalize(nullable.Value), Vector3.UnitY);
				Vector3 v1 = Vector3.Normalize(vector3_1 - nullable.Value);
				vector3_2 = world.Right * num2;
				if ((double)Vector3.Dot(world.Right, v1) < 0.0)
					vector3_2 *= -1f;
			}
			Vector3 vector3_3 = -vector3_1;
			double num5 = (double)vector3_3.Normalize();
			Vector3 position1 = vector3_1 - vector3_3 * num2 + vector3_2;
			return Matrix.CreateWorld(position1, Vector3.Normalize(vector3_1 - position1), Vector3.UnitY);
		}
	}
}
