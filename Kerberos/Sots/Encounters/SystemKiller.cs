// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Encounters.SystemKiller
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Encounters
{
	internal class SystemKiller
	{
		private int PlayerId = -1;
		private const string FactionName = "grandmenaces";
		private const string PlayerName = "System Killer";
		private const string PlayerAvatar = "\\base\\factions\\grandmenaces\\avatars\\Systemkiller_Avatar.tga";
		private const string FleetName = "System Killer";
		private const string SystemKillerDesignFile = "systemkiller.section";
		private int _systemKillerDesignId;

		public int PlayerID
		{
			get
			{
				return this.PlayerId;
			}
		}

		public int SystemKillerDesignId
		{
			get
			{
				return this._systemKillerDesignId;
			}
		}

		public static SystemKiller InitializeEncounter(
		  GameDatabase gamedb,
		  AssetDatabase assetdb)
		{
			SystemKiller systemKiller = new SystemKiller();
			systemKiller.PlayerId = gamedb.InsertPlayer("System Killer", "grandmenaces", new int?(), assetdb.RandomEncounterPrimaryColor, new Vector3(0.0f), "", "\\base\\factions\\grandmenaces\\avatars\\Systemkiller_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal);
			DesignInfo design = new DesignInfo(systemKiller.PlayerId, "System Killer", new string[1]
			{
		string.Format("factions\\{0}\\sections\\{1}", (object) "grandmenaces", (object) "systemkiller.section")
			});
			systemKiller._systemKillerDesignId = gamedb.InsertDesignByDesignInfo(design);
			return systemKiller;
		}

		public static SystemKiller ResumeEncounter(GameDatabase gamedb)
		{
			SystemKiller systemKiller = new SystemKiller();
			PlayerInfo playerInfo = gamedb.GetPlayerInfos().ToList<PlayerInfo>().FirstOrDefault<PlayerInfo>((Func<PlayerInfo, bool>)(x =>
		   {
			   if (!x.isStandardPlayer)
				   return x.Name.Contains("System Killer");
			   return false;
		   }));
			systemKiller.PlayerId = playerInfo.ID;
			List<DesignInfo> list = gamedb.GetDesignInfosForPlayer(systemKiller.PlayerId).ToList<DesignInfo>();
			systemKiller._systemKillerDesignId = list.First<DesignInfo>((Func<DesignInfo, bool>)(x => x.DesignSections[0].FilePath.EndsWith("systemkiller.section"))).ID;
			return systemKiller;
		}

		public void ExecuteInstance(GameDatabase gamedb, AssetDatabase assetdb, int systemid)
		{
			StarSystemInfo starSystemInfo1 = gamedb.GetStarSystemInfo(systemid);
			StarSystemInfo starSystemInfo2 = EncounterTools.GetClosestStars(gamedb, starSystemInfo1).Last<StarSystemInfo>();
			int fleetID = gamedb.InsertFleet(this.PlayerId, 0, 0, 0, "System Killer", FleetType.FL_NORMAL);
			gamedb.InsertShip(fleetID, this._systemKillerDesignId, null, (ShipParams)0, new int?(), 0);
			int missionID = gamedb.InsertMission(fleetID, MissionType.STRIKE, starSystemInfo1.ID, 0, 0, 0, false, new int?());
			gamedb.InsertWaypoint(missionID, WaypointType.TravelTo, new int?(starSystemInfo1.ID));
			gamedb.InsertMoveOrder(fleetID, 0, starSystemInfo1.Origin - Vector3.Normalize(starSystemInfo2.Origin - starSystemInfo1.Origin) * 10f, starSystemInfo1.ID, Vector3.Zero);
			gamedb.InsertSystemKillerInfo(new SystemKillerInfo()
			{
				Target = starSystemInfo2.Origin,
				FleetId = new int?(fleetID)
			});
		}

		public void AddInstance(GameDatabase gamedb, AssetDatabase assetdb, int? targetSystem = null)
		{
			Random safeRandom = App.GetSafeRandom();
			List<KeyValuePair<StarSystemInfo, Vector3>> outlyingStars = EncounterTools.GetOutlyingStars(gamedb);
			StarSystemInfo starSystemInfo;
			if (targetSystem.HasValue)
			{
				starSystemInfo = gamedb.GetStarSystemInfo(targetSystem.Value);
			}
			else
			{
				int count = outlyingStars.Count / 3;
				if (count <= 0)
					return;
				List<KeyValuePair<StarSystemInfo, Vector3>> range = outlyingStars.GetRange(0, count);
				starSystemInfo = safeRandom.Choose<KeyValuePair<StarSystemInfo, Vector3>>((IList<KeyValuePair<StarSystemInfo, Vector3>>)range).Key;
			}
			gamedb.InsertIncomingGM(starSystemInfo.ID, EasterEgg.GM_SYSTEM_KILLER, gamedb.GetTurnCount() + 1);
			foreach (PlayerInfo playerInfo in gamedb.GetStandardPlayerInfos().ToList<PlayerInfo>())
			{
				if (gamedb.GetStratModifier<int>(StratModifiers.GrandMenaceWarningTime, playerInfo.ID) > 0)
					gamedb.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_INCOMING_SYSTEMKILLER,
						EventMessage = TurnEventMessage.EM_INCOMING_SYSTEMKILLER,
						PlayerID = playerInfo.ID,
						TurnNumber = gamedb.GetTurnCount()
					});
			}
		}

		public void UpdateTurn(GameSession game, int id)
		{
			SystemKillerInfo si = game.GameDatabase.GetSystemKillerInfo(id);
			FleetInfo fleetInfo = si.FleetId.HasValue ? game.GameDatabase.GetFleetInfo(si.FleetId.Value) : (FleetInfo)null;
			if (fleetInfo == null)
			{
				game.GameDatabase.RemoveEncounter(si.Id);
			}
			else
			{
				MissionInfo missionByFleetId = game.GameDatabase.GetMissionByFleetID(fleetInfo.ID);
				StarSystemInfo systemInfo = game.GameDatabase.GetStarSystemInfo(fleetInfo.SystemID);
				if (game.GameDatabase.GetMoveOrderInfoByFleetID(fleetInfo.ID) != null)
					return;
				List<OrbitalObjectInfo> list = game.GameDatabase.GetStarSystemOrbitalObjectInfos(fleetInfo.SystemID).ToList<OrbitalObjectInfo>();
				list.RemoveAll((Predicate<OrbitalObjectInfo>)(x =>
			   {
				   if (game.GameDatabase.GetAsteroidBeltInfo(x.ID) == null && game.GameDatabase.GetLargeAsteroidInfo(x.ID) == null)
					   return game.GameDatabase.GetStationInfo(x.ID) != null;
				   return true;
			   }));
				if (list.Any<OrbitalObjectInfo>())
				{
					if (game.isHostilesAtSystem(this.PlayerId, fleetInfo.SystemID))
						return;
					OrbitalObjectInfo orbitalObjectInfo = list.OrderBy<OrbitalObjectInfo, float>((Func<OrbitalObjectInfo, float>)(x => x.OrbitalPath.Scale.Length)).First<OrbitalObjectInfo>();
					game.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_PLANET_DESTROYED,
						EventMessage = TurnEventMessage.EM_PLANET_DESTROYED,
						PlayerID = this.PlayerId,
						SystemID = orbitalObjectInfo.StarSystemID,
						OrbitalID = orbitalObjectInfo.ID,
						TurnNumber = game.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
					game.GameDatabase.DestroyOrbitalObject(game, orbitalObjectInfo.ID);
				}
				else
				{
					if (missionByFleetId != null)
						game.GameDatabase.RemoveMission(missionByFleetId.ID);
					List<StarSystemInfo> closestStars = EncounterTools.GetClosestStars(game.GameDatabase, fleetInfo.SystemID);
					double maxCos = Math.Cos(Math.PI / 3.0);
					StarSystemInfo starSystemInfo = closestStars.FirstOrDefault<StarSystemInfo>((Func<StarSystemInfo, bool>)(x => (double)Vector3.Dot(Vector3.Normalize(si.Target - systemInfo.Origin), Vector3.Normalize(x.Origin - systemInfo.Origin)) > maxCos));
					if (starSystemInfo == (StarSystemInfo)null)
					{
						foreach (int standardPlayerId in game.GameDatabase.GetStandardPlayerIDs())
						{
							if (StarMap.IsInRange(game.GameDatabase, standardPlayerId, game.GameDatabase.GetFleetLocation(fleetInfo.ID, false).Coords, 1f, (Dictionary<int, List<ShipInfo>>)null))
								game.GameDatabase.InsertTurnEvent(new TurnEvent()
								{
									EventType = TurnEventType.EV_SYS_KILLER_LEAVING,
									EventMessage = TurnEventMessage.EM_SYS_KILLER_LEAVING,
									PlayerID = this.PlayerID,
									TurnNumber = game.GameDatabase.GetTurnCount()
								});
						}
						game.GameDatabase.RemoveFleet(fleetInfo.ID);
						game.GameDatabase.RemoveEncounter(si.Id);
					}
					else
					{
						int missionID = game.GameDatabase.InsertMission(fleetInfo.ID, MissionType.STRIKE, starSystemInfo.ID, 0, 0, 0, false, new int?());
						game.GameDatabase.InsertWaypoint(missionID, WaypointType.TravelTo, new int?(starSystemInfo.ID));
						game.GameDatabase.InsertMoveOrder(fleetInfo.ID, 0, game.GameDatabase.GetStarSystemOrigin(fleetInfo.SystemID), starSystemInfo.ID, Vector3.Zero);
						game.GameDatabase.UpdateFleetLocation(fleetInfo.ID, 0, new int?());
					}
					if (systemInfo != (StarSystemInfo)null)
						game.GameDatabase.DestroyStarSystem(game, systemInfo.ID);
					if (!(game.App.CurrentState is StarMapState))
						return;
					((StarMapState)game.App.CurrentState).ClearSelectedObject();
					((StarMapState)game.App.CurrentState).RefreshStarmap(StarMapState.StarMapRefreshType.REFRESH_ALL);
				}
			}
		}

		public static Matrix GetBaseEnemyFleetTrans(
		  App app,
		  int skFleetId,
		  Kerberos.Sots.GameStates.StarSystem starSystem,
		  OrbitalObjectInfo[] orbitalObjects)
		{
			return SystemKiller.GetSpawnTransform(app, skFleetId, starSystem, orbitalObjects);
		}

		public static Matrix GetSpawnTransform(
		  App app,
		  int fleetID,
		  Kerberos.Sots.GameStates.StarSystem starSystem,
		  OrbitalObjectInfo[] orbitalObjects)
		{
			DesignInfo designInfo = app.GameDatabase.GetDesignInfo(app.Game.ScriptModules.SystemKiller.SystemKillerDesignId);
			float val2 = 50000f;
			if (designInfo != null)
				val2 = ((IEnumerable<DesignSectionInfo>)designInfo.DesignSections).Sum<DesignSectionInfo>((Func<DesignSectionInfo, float>)(x => x.ShipSectionAsset.Maneuvering.LinearSpeed)) * 120f;
			float num1 = 0.0f;
			float num2 = 5000f;
			Vector3 vector3_1 = Vector3.Zero;
			Vector3 vector3_2 = -Vector3.UnitZ;
			int num3 = 0;
			foreach (OrbitalObjectInfo orbitalObject in orbitalObjects)
			{
				PlanetInfo planetInfo = app.GameDatabase.GetPlanetInfo(orbitalObject.ID);
				if (planetInfo != null)
				{
					Matrix orbitalTransform = app.GameDatabase.GetOrbitalTransform(orbitalObject.ID);
					float lengthSquared = orbitalTransform.Position.LengthSquared;
					if ((double)lengthSquared > (double)num1)
					{
						num1 = lengthSquared;
						vector3_1 = orbitalTransform.Position;
						vector3_2 = -orbitalTransform.Position;
						num2 = StarSystemVars.Instance.SizeToRadius(planetInfo.Size) + val2;
						num3 = orbitalObject.ID;
					}
				}
			}
			if (num3 == 0)
			{
				MoveOrderInfo orderInfoByFleetId = app.GameDatabase.GetMoveOrderInfoByFleetID(fleetID);
				if (orderInfoByFleetId != null)
				{
					Vector3 forward = orderInfoByFleetId.ToCoords - orderInfoByFleetId.FromCoords;
					forward.Y = 0.0f;
					if ((double)forward.Normalize() < 1.40129846432482E-45)
						forward = -Vector3.UnitZ;
					float num4 = 0.0f;
					if (starSystem != null)
						num4 = Math.Min((float)(((double)starSystem.GetBaseOffset() + (double)Kerberos.Sots.GameStates.StarSystem.CombatZoneMapRadii[Math.Max(starSystem.GetFurthestRing() - 1, 1)]) * 5700.0), val2);
					return Matrix.CreateWorld(forward * -num4, forward, Vector3.UnitY);
				}
				float num5 = starSystem != null ? starSystem.GetStarRadius() : 5000f;
				Vector3 position = Vector3.UnitZ * (float)-((double)num5 + (double)val2);
				return Matrix.CreateWorld(position, Vector3.Normalize(-position), Vector3.UnitY);
			}
			double num6 = (double)vector3_2.Normalize();
			IEnumerable<OrbitalObjectInfo> moons = app.GameDatabase.GetMoons(num3);
			if (moons.Count<OrbitalObjectInfo>() > 0)
			{
				float num4 = 5000f;
				Vector3 vector3_3 = Vector3.Zero;
				Vector3 forward = -Vector3.UnitZ;
				int num5 = 0;
				float num7 = 0.0f;
				foreach (OrbitalObjectInfo orbitalObjectInfo in moons)
				{
					PlanetInfo planetInfo = app.GameDatabase.GetPlanetInfo(orbitalObjectInfo.ID);
					if (planetInfo != null)
					{
						Matrix orbitalTransform = app.GameDatabase.GetOrbitalTransform(orbitalObjectInfo.ID);
						float lengthSquared = orbitalTransform.Position.LengthSquared;
						if ((double)lengthSquared > (double)num7)
						{
							num7 = lengthSquared;
							vector3_3 = orbitalTransform.Position;
							forward = -orbitalTransform.Position;
							num4 = StarSystemVars.Instance.SizeToRadius(planetInfo.Size) + val2;
							num5 = orbitalObjectInfo.ID;
						}
					}
				}
				if (num5 != 0)
				{
					Vector3 vector3_4 = Vector3.Normalize(vector3_3 - vector3_1);
					if ((double)Vector3.Dot(vector3_4, vector3_2) > 0.0)
					{
						double num8 = (double)forward.Normalize();
						return Matrix.CreateWorld(vector3_3 - forward * (num4 + val2), forward, Vector3.UnitY);
					}
					Vector3 v0 = Vector3.Cross(vector3_2, Vector3.UnitY);
					float num9 = (double)Vector3.Dot(v0, vector3_4) > 0.0 ? 1f : -1f;
					return Matrix.CreateWorld(vector3_3 + v0 * ((num4 + val2) * num9), forward, Vector3.UnitY);
				}
			}
			Vector3 position1 = vector3_1 - vector3_2 * num2;
			if (starSystem != null)
			{
				Vector3 forward = -Vector3.UnitZ;
				float num4 = -1f;
				foreach (Ship ship in starSystem.GetStationsAroundPlanet(num3))
				{
					Vector3 v0 = vector3_1 - ship.Maneuvering.Position;
					double num5 = (double)v0.Normalize();
					float num7 = Vector3.Dot(v0, vector3_2);
					if ((double)num7 > 0.75 && (double)num7 > (double)num4)
					{
						num4 = num7;
						forward = v0;
					}
				}
				if ((double)num4 > 0.0)
				{
					Matrix world = Matrix.CreateWorld(Vector3.Zero, forward, Vector3.UnitY);
					Vector3 vector3_3 = vector3_1 + world.Right * num2;
					Vector3 vector3_4 = vector3_1 - world.Right * num2;
					vector3_2 = (double)(position1 - vector3_3).LengthSquared >= (double)(position1 - vector3_4).LengthSquared ? (forward - world.Right) * 0.5f : (forward + world.Right) * 0.5f;
					double num5 = (double)vector3_2.Normalize();
					position1 = vector3_1 - vector3_2 * num2;
				}
			}
			return Matrix.CreateWorld(position1, vector3_2, Vector3.UnitY);
		}
	}
}
