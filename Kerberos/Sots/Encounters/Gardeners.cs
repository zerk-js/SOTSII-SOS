// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Encounters.Gardeners
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.StarMapFramework;
using Kerberos.Sots.Framework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Encounters
{
	internal class Gardeners
	{
		private int PlayerId = -1;
		private const string FactionName = "protean";
		private const string PlayerName = "Protean";
		private const string PlayerAvatar = "\\base\\factions\\protean\\avatars\\Protean_Avatar.tga";
		private const string FleetName = "Protean Pod";
		private const string FollowFleetName = "Protean Follow";
		private const string GardenerFleetName = "Gardener";
		private const string ProteanDesignFile = "protean.section";
		private const string GardenerDesignFile = "Rama.section";
		private int _proteanDesignId;
		private int _gardenerDesignId;

		public int PlayerID
		{
			get
			{
				return this.PlayerId;
			}
		}

		public int ProteanDesignId
		{
			get
			{
				return this._proteanDesignId;
			}
		}

		public int GardenerDesignId
		{
			get
			{
				return this._gardenerDesignId;
			}
		}

		public static Gardeners InitializeEncounter(GameDatabase gamedb, AssetDatabase assetdb)
		{
			Gardeners gardeners = new Gardeners();
			gardeners.PlayerId = gamedb.InsertPlayer("Protean", "protean", new int?(), assetdb.RandomEncounterPrimaryColor, new Vector3(0.0f), "", "\\base\\factions\\protean\\avatars\\Protean_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal);
			DesignInfo design1 = new DesignInfo(gardeners.PlayerId, "Protean", new string[1]
			{
		string.Format("factions\\{0}\\sections\\{1}", (object) "protean", (object) "protean.section")
			});
			gardeners._proteanDesignId = gamedb.InsertDesignByDesignInfo(design1);
			if (gamedb.HasEndOfFleshExpansion())
			{
				DesignInfo design2 = new DesignInfo(gardeners.PlayerId, "Gardener", new string[1]
				{
		  string.Format("factions\\{0}\\sections\\{1}", (object) "protean", (object) "Rama.section")
				});
				gardeners._gardenerDesignId = gamedb.InsertDesignByDesignInfo(design2);
			}
			return gardeners;
		}

		public static Gardeners ResumeEncounter(GameDatabase gamedb)
		{
			Gardeners gardeners = new Gardeners();
			PlayerInfo playerInfo = gamedb.GetPlayerInfos().ToList<PlayerInfo>().FirstOrDefault<PlayerInfo>((Func<PlayerInfo, bool>)(x =>
		   {
			   if (!x.isStandardPlayer)
				   return x.Name.Contains("Protean");
			   return false;
		   }));
			gardeners.PlayerId = playerInfo.ID;
			List<DesignInfo> list = gamedb.GetDesignInfosForPlayer(gardeners.PlayerId).ToList<DesignInfo>();
			gardeners._proteanDesignId = list.First<DesignInfo>((Func<DesignInfo, bool>)(x => x.DesignSections[0].FilePath.EndsWith("protean.section"))).ID;
			DesignInfo designInfo = list.First<DesignInfo>((Func<DesignInfo, bool>)(x => x.DesignSections[0].FilePath.EndsWith("Rama.section")));
			if (designInfo != null && gamedb.HasEndOfFleshExpansion())
				gardeners._gardenerDesignId = designInfo.ID;
			return gardeners;
		}

		public static int GetPlayerID(GameDatabase gamedb)
		{
			PlayerInfo playerInfo = gamedb.GetPlayerInfos().ToList<PlayerInfo>().FirstOrDefault<PlayerInfo>((Func<PlayerInfo, bool>)(x =>
		   {
			   if (!x.isStandardPlayer)
				   return x.Name.Contains("Protean");
			   return false;
		   }));
			if (playerInfo == null)
				return 0;
			return playerInfo.ID;
		}

		public static Matrix GetBaseEnemyFleetTrans(
		  App app,
		  int systemId,
		  OrbitalObjectInfo[] objects)
		{
			if (((IEnumerable<OrbitalObjectInfo>)objects).Count<OrbitalObjectInfo>() == 0)
				return Matrix.Identity;
			GardenerInfo gardenerInfo = app.GameDatabase.GetGardenerInfos().ToList<GardenerInfo>().FirstOrDefault<GardenerInfo>((Func<GardenerInfo, bool>)(x => x.SystemId == systemId));
			float num1 = 0.0f;
			Matrix matrix = Matrix.Identity;
			foreach (OrbitalObjectInfo orbitalObjectInfo in objects)
			{
				OrbitalObjectInfo oo = orbitalObjectInfo;
				PlanetInfo planetInfo = app.GameDatabase.GetPlanetInfo(oo.ID);
				if (planetInfo != null && !(planetInfo.Type == "barren") && !(planetInfo.Type == "gaseous"))
				{
					if (gardenerInfo == null || (gardenerInfo.ShipOrbitMap.Where<KeyValuePair<int, int>>((Func<KeyValuePair<int, int>, bool>)(x => x.Value == oo.ID)).Count<KeyValuePair<int, int>>() != 0 || (double)num1 <= 0.0))
					{
						Vector3 position = app.GameDatabase.GetOrbitalTransform(planetInfo.ID).Position;
						float lengthSquared = position.LengthSquared;
						if ((double)lengthSquared > (double)num1)
						{
							num1 = lengthSquared;
							Vector3 forward = position;
							double num2 = (double)forward.Normalize();
							matrix = Matrix.CreateWorld(Vector3.Zero, forward, Vector3.UnitY);
							float num3 = StarSystemVars.Instance.SizeToRadius(planetInfo.Size) + 3000f;
							matrix.Position = position - matrix.Forward * num3;
						}
					}
				}
			}
			return matrix;
		}

		public static Matrix GetSpawnTransform(
		  App app,
		  int databaseId,
		  int fleetId,
		  int shipIndex,
		  int systemId,
		  OrbitalObjectInfo[] objects)
		{
			GardenerInfo gardenerInfo = app.GameDatabase.GetGardenerInfos().FirstOrDefault<GardenerInfo>((Func<GardenerInfo, bool>)(x => x.FleetId == fleetId));
			if (gardenerInfo == null)
				return Matrix.Identity;
			if (gardenerInfo.IsGardener)
				return Gardeners.GetSpawnMatrixForGardener(app, systemId, objects);
			if (gardenerInfo.GardenerFleetId != 0)
				return Gardeners.GetSpawnMatrixForProteansWithGardener(app, databaseId, systemId, objects);
			return Gardeners.GetSpawnMatrixForProteansAtHome(app, databaseId, shipIndex, systemId, objects);
		}

		private static Matrix GetSpawnMatrixForGardener(
		  App app,
		  int systemId,
		  OrbitalObjectInfo[] objects)
		{
			StarSystemInfo starSystemInfo = app.GameDatabase.GetStarSystemInfo(systemId);
			if (starSystemInfo == (StarSystemInfo)null || starSystemInfo.IsDeepSpace)
				return Matrix.Identity;
			float num1 = 0.0f;
			float num2 = 5000f;
			Vector3 vector3_1 = Vector3.Zero;
			bool flag = false;
			foreach (OrbitalObjectInfo orbitalObjectInfo in objects)
			{
				PlanetInfo planetInfo = app.GameDatabase.GetPlanetInfo(orbitalObjectInfo.ID);
				if (planetInfo != null)
				{
					ColonyInfo colonyInfoForPlanet = app.GameDatabase.GetColonyInfoForPlanet(orbitalObjectInfo.ID);
					Matrix orbitalTransform = app.GameDatabase.GetOrbitalTransform(orbitalObjectInfo.ID);
					float lengthSquared = orbitalTransform.Position.LengthSquared;
					if ((double)lengthSquared > (double)num1 || !flag && colonyInfoForPlanet != null)
					{
						num1 = lengthSquared;
						vector3_1 = orbitalTransform.Position;
						num2 = StarSystemVars.Instance.SizeToRadius(planetInfo.Size) + 1500f;
						flag = colonyInfoForPlanet != null;
					}
				}
			}
			if ((double)num1 <= 0.0)
				return Matrix.Identity;
			Vector3 vector3_2 = Vector3.Normalize(vector3_1);
			return Matrix.CreateWorld(vector3_1 + vector3_2 * num2, -vector3_2, Vector3.UnitY);
		}

		private static Matrix GetSpawnMatrixForProteansWithGardener(
		  App app,
		  int databaseId,
		  int systemId,
		  OrbitalObjectInfo[] objects)
		{
			Matrix matrixForGardener = Gardeners.GetSpawnMatrixForGardener(app, systemId, objects);
			StarSystemInfo starSystemInfo = app.GameDatabase.GetStarSystemInfo(systemId);
			float num = starSystemInfo == (StarSystemInfo)null || starSystemInfo.IsDeepSpace ? 1000f : 10000f;
			Vector3 vector3 = -matrixForGardener.Forward;
			Matrix world = Matrix.CreateWorld(matrixForGardener.Position + vector3 * num, -vector3, Vector3.UnitY);
			Vector3 position = world.Position;
			Vector3? shipFleetPosition = app.GameDatabase.GetShipFleetPosition(databaseId);
			if (shipFleetPosition.HasValue)
				position = Vector3.Transform(shipFleetPosition.Value, world);
			return Matrix.CreateWorld(position, world.Forward, Vector3.UnitY);
		}

		private static Matrix GetSpawnMatrixForProteansAtHome(
		  App app,
		  int databaseId,
		  int shipIndex,
		  int systemId,
		  OrbitalObjectInfo[] objects)
		{
			if (((IEnumerable<OrbitalObjectInfo>)objects).Count<OrbitalObjectInfo>() == 0)
				return Matrix.Identity;
			GardenerInfo gardenerInfo = app.GameDatabase.GetGardenerInfos().FirstOrDefault<GardenerInfo>((Func<GardenerInfo, bool>)(x => x.SystemId == systemId));
			int orbitalId = 0;
			if (gardenerInfo != null && gardenerInfo.ShipOrbitMap.ContainsKey(databaseId))
				orbitalId = gardenerInfo.ShipOrbitMap[databaseId];
			Matrix matrix = Matrix.Identity;
			if (orbitalId == 0)
			{
				int num1 = 0;
				foreach (OrbitalObjectInfo orbitalObjectInfo in objects)
				{
					PlanetInfo planetInfo = app.GameDatabase.GetPlanetInfo(orbitalObjectInfo.ID);
					if (planetInfo != null && !(planetInfo.Type == "barren") && !(planetInfo.Type == "gaseous"))
					{
						num1 += (int)planetInfo.Size;
						if (shipIndex < num1)
						{
							int num2 = num1 - shipIndex;
							matrix = Matrix.CreateRotationYPR(MathHelper.DegreesToRadians(360f / planetInfo.Size * (float)num2), 0.0f, 0.0f);
							float num3 = StarSystemVars.Instance.SizeToRadius(planetInfo.Size) + 3000f;
							matrix.Position = app.GameDatabase.GetOrbitalTransform(planetInfo.ID).Position + matrix.Forward * num3;
							if (gardenerInfo != null)
							{
								app.Game.GameDatabase.InsertProteanShipOrbitMap(gardenerInfo.Id, databaseId, planetInfo.ID);
								break;
							}
							break;
						}
					}
				}
			}
			else
			{
				PlanetInfo planetInfo = app.GameDatabase.GetPlanetInfo(orbitalId);
				gardenerInfo.ShipOrbitMap.Where<KeyValuePair<int, int>>((Func<KeyValuePair<int, int>, bool>)(x => x.Value == orbitalId)).Count<KeyValuePair<int, int>>();
				int num1 = 0;
				foreach (KeyValuePair<int, int> keyValuePair in gardenerInfo.ShipOrbitMap.Where<KeyValuePair<int, int>>((Func<KeyValuePair<int, int>, bool>)(x => x.Value == orbitalId)))
				{
					if (keyValuePair.Key != databaseId)
						++num1;
					else
						break;
				}
				matrix = Matrix.CreateRotationYPR(MathHelper.DegreesToRadians(360f / planetInfo.Size * (float)num1), 0.0f, 0.0f);
				float num2 = StarSystemVars.Instance.SizeToRadius(planetInfo.Size) + 3000f;
				matrix.Position = app.GameDatabase.GetOrbitalTransform(planetInfo.ID).Position + matrix.Forward * num2;
			}
			return matrix;
		}

		public static List<PlanetInfo> GetGardenerPlanetsFromList(App app, int systemId)
		{
			GardenerInfo gardenerInfo = app.GameDatabase.GetGardenerInfos().FirstOrDefault<GardenerInfo>((Func<GardenerInfo, bool>)(x => x.SystemId == systemId));
			List<PlanetInfo> planetInfoList = new List<PlanetInfo>();
			if (gardenerInfo != null)
			{
				foreach (KeyValuePair<int, int> shipOrbit in gardenerInfo.ShipOrbitMap)
				{
					KeyValuePair<int, int> ship = shipOrbit;
					if (gardenerInfo.ShipOrbitMap.Where<KeyValuePair<int, int>>((Func<KeyValuePair<int, int>, bool>)(x => x.Value == ship.Value)).Count<KeyValuePair<int, int>>() != 0)
					{
						PlanetInfo planetInfo = app.GameDatabase.GetPlanetInfo(ship.Value);
						if (planetInfo != null && !planetInfoList.Contains(planetInfo))
							planetInfoList.Add(planetInfo);
					}
				}
			}
			return planetInfoList;
		}

		public void AddInstance(GameDatabase gamedb, AssetDatabase assetdb, int SystemId)
		{
			if (gamedb.HasEndOfFleshExpansion() && SystemId == 0)
				this.AddGardenerInstance(gamedb, assetdb);
			else
				this.AddProteanSystemInstance(gamedb, assetdb, SystemId);
		}

		private void AddProteanSystemInstance(GameDatabase gamedb, AssetDatabase assetdb, int SystemId)
		{
			GardenerInfo gi = new GardenerInfo();
			gi.SystemId = SystemId;
			List<PlanetInfo> list = ((IEnumerable<PlanetInfo>)gamedb.GetStarSystemPlanetInfos(SystemId)).ToList<PlanetInfo>();
			int num1 = 0;
			int num2 = 0;
			float num3 = gamedb.GetFactions().Average<FactionInfo>((Func<FactionInfo, float>)(x => x.IdealSuitability));
			Random safeRandom = App.GetSafeRandom();
			GardenerGlobalData globalGardenerData = assetdb.GlobalGardenerData;
			foreach (PlanetInfo planet in list)
			{
				if (!gamedb.GetOrbitalObjectInfo(planet.ID).ParentID.HasValue && planet.Type != "barren" && planet.Type != "gaseous")
				{
					planet.Biosphere = safeRandom.Next(globalGardenerData.MinBiosphere, globalGardenerData.MaxBiosphere);
					planet.Suitability = num3;
					++num1;
					gamedb.UpdatePlanet(planet);
				}
			}
			if (num1 < globalGardenerData.MinPlanets)
			{
				int num4 = globalGardenerData.MinPlanets - num1;
				int num5 = gamedb.GetStarSystemOrbitalObjectInfos(SystemId).Where<OrbitalObjectInfo>((Func<OrbitalObjectInfo, bool>)(x => !x.ParentID.HasValue)).Count<OrbitalObjectInfo>() + 1;
				for (int index = 0; index < num4; ++index)
				{
					PlanetOrbit planetOrbit = new PlanetOrbit();
					planetOrbit.OrbitNumber = num5 + index;
					planetOrbit.Biosphere = new int?(safeRandom.Next(globalGardenerData.MinBiosphere, globalGardenerData.MaxBiosphere));
					planetOrbit.Suitability = new float?(num3);
					PlanetInfo pi = StarSystemHelper.InferPlanetInfo((Kerberos.Sots.Data.StarMapFramework.Orbit)planetOrbit);
					gamedb.AddPlanetToSystem(SystemId, new int?(), null, pi, new int?(num5 + index));
				}
			}
			List<PlanetInfo> planetInfoList = new List<PlanetInfo>();
			foreach (PlanetInfo planetInfo in list)
			{
				if (planetInfo.Type != "barren" && planetInfo.Type != "gaseous")
				{
					num2 += (int)planetInfo.Size;
					planetInfoList.Add(planetInfo);
				}
			}
			int fleetID = gamedb.InsertFleet(this.PlayerId, 0, SystemId, SystemId, "Protean Pod", FleetType.FL_NORMAL);
			gi.FleetId = fleetID;
			gamedb.InsertGardenerInfo(gi);
			int encounterIdAtSystem = gamedb.GetEncounterIDAtSystem(EasterEgg.EE_GARDENERS, gi.SystemId);
			for (int index = 0; index < num2; ++index)
			{
				int shipId = gamedb.InsertShip(fleetID, this._proteanDesignId, null, (ShipParams)0, new int?(), 0);
				int num4 = 0;
				foreach (PlanetInfo planetInfo in planetInfoList)
				{
					num4 += (int)planetInfo.Size;
					if (index < num4)
					{
						gamedb.InsertProteanShipOrbitMap(encounterIdAtSystem, shipId, planetInfo.ID);
						break;
					}
				}
			}
		}

		private void AddGardenerInstance(GameDatabase gamedb, AssetDatabase assetdb)
		{
			GardenerInfo gi = new GardenerInfo();
			gi.FleetId = gamedb.InsertFleet(this.PlayerId, 0, 0, 0, "Gardener", FleetType.FL_NORMAL);
			gamedb.InsertShip(gi.FleetId, this.GardenerDesignId, null, (ShipParams)0, new int?(), 0);
			gi.IsGardener = true;
			Vector3 vector3_1 = new Vector3();
			vector3_1.X = (App.GetSafeRandom().CoinToss(50) ? -1f : 1f) * App.GetSafeRandom().NextInclusive(1E-06f, 1f);
			vector3_1.Y = (App.GetSafeRandom().CoinToss(50) ? -1f : 1f) * App.GetSafeRandom().NextInclusive(1E-06f, 1f);
			vector3_1.Z = (App.GetSafeRandom().CoinToss(50) ? -1f : 1f) * App.GetSafeRandom().NextInclusive(1E-06f, 1f);
			double num = (double)vector3_1.Normalize();
			Vector3 vector3_2 = vector3_1 * -10f;
			Vector3 toCoords = vector3_1 * 10f;
			int missionID = gamedb.InsertMission(gi.FleetId, MissionType.RELOCATION, 0, 0, 0, 0, true, new int?());
			gamedb.InsertWaypoint(missionID, WaypointType.TravelTo, new int?());
			gamedb.InsertMoveOrder(gi.FleetId, 0, vector3_2, 0, toCoords);
			gi.DeepSpaceSystemId = new int?(gamedb.InsertStarSystem(new int?(), App.Localize("@UI_STARMAP_ENCOUNTER_DEEPSPACE"), new int?(), "Deepspace", vector3_2, false, false, new int?()));
			gi.DeepSpaceOrbitalId = new int?(gamedb.InsertOrbitalObject(new int?(), gi.DeepSpaceSystemId.Value, new OrbitalPath()
			{
				Scale = new Vector2(20000f, Ellipse.CalcSemiMinorAxis(20000f, 0.0f)),
				InitialAngle = 0.0f
			}, "space"));
			gamedb.GetOrbitalTransform(gi.DeepSpaceOrbitalId.Value);
			gamedb.InsertGardenerInfo(gi);
			foreach (PlayerInfo playerInfo in gamedb.GetStandardPlayerInfos().ToList<PlayerInfo>())
			{
				if (gamedb.GetStratModifier<int>(StratModifiers.GrandMenaceWarningTime, playerInfo.ID) > 0)
					gamedb.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_INCOMING_GARDENER,
						EventMessage = TurnEventMessage.EM_INCOMING_GARDENER,
						PlayerID = playerInfo.ID,
						TurnNumber = gamedb.GetTurnCount()
					});
			}
		}

		public void UpdateTurn(GameSession game, int id)
		{
			GardenerInfo gi = game.GameDatabase.GetGardenerInfo(id);
			if (gi == null)
				game.GameDatabase.RemoveEncounter(id);
			else if (gi.IsGardener)
			{
				FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(gi.FleetId);
				if (fleetInfo != null && fleetInfo.PlayerID != this.PlayerID)
					return;
				if (game.GameDatabase.GetMoveOrderInfoByFleetID(gi.FleetId) == null)
				{
					if (gi.DeepSpaceSystemId.HasValue)
						game.GameDatabase.DestroyStarSystem(game, gi.DeepSpaceSystemId.Value);
					if (gi.DeepSpaceOrbitalId.HasValue)
						game.GameDatabase.RemoveOrbitalObject(gi.DeepSpaceOrbitalId.Value);
					game.GameDatabase.RemoveEncounter(id);
				}
				else
				{
					FleetLocation fl = game.GameDatabase.GetFleetLocation(fleetInfo.ID, false);
					if (!gi.DeepSpaceSystemId.HasValue || game.GameDatabase.GetStarSystemInfos().Any<StarSystemInfo>((Func<StarSystemInfo, bool>)(x =>
				   {
					   if (x.ID != gi.DeepSpaceSystemId.Value)
						   return (double)(game.GameDatabase.GetStarSystemOrigin(x.ID) - fl.Coords).LengthSquared < 9.99999974737875E-05;
					   return false;
				   })))
						return;
					game.GameDatabase.UpdateStarSystemOrigin(gi.DeepSpaceSystemId.Value, fl.Coords);
				}
			}
			else
			{
				if (gi.SystemId != 0 || gi.GardenerFleetId == 0)
					return;
				FleetInfo gardenerFleet = game.GameDatabase.GetFleetInfo(gi.GardenerFleetId);
				bool flag = gardenerFleet == null;
				if (!flag)
				{
					if (gi.TurnsToWait <= 0)
					{
						FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(gi.FleetId);
						if (fleetInfo != null)
						{
							if (game.GameDatabase.GetMoveOrderInfoByFleetID(gardenerFleet.ID) == null)
							{
								if (gardenerFleet.SystemID != fleetInfo.SystemID)
									game.GameDatabase.UpdateFleetLocation(fleetInfo.ID, gardenerFleet.SystemID, new int?());
							}
							else
							{
								GardenerInfo gardenerInfo = game.GameDatabase.GetGardenerInfos().FirstOrDefault<GardenerInfo>((Func<GardenerInfo, bool>)(x => x.FleetId == gardenerFleet.ID));
								if (gardenerInfo != null && gardenerInfo.DeepSpaceSystemId.HasValue)
									game.GameDatabase.UpdateFleetLocation(fleetInfo.ID, gardenerInfo.DeepSpaceSystemId.Value, new int?());
								else
									game.GameDatabase.UpdateFleetLocation(fleetInfo.ID, 0, new int?());
							}
						}
					}
					else if (game.GameDatabase.GetMoveOrderInfoByFleetID(gardenerFleet.ID) == null)
					{
						--gi.TurnsToWait;
						game.GameDatabase.UpdateGardenerInfo(gi);
					}
				}
				if (!flag)
					return;
				game.GameDatabase.RemoveEncounter(id);
			}
		}

		public void SpawnProteanChaser(GameSession game, GardenerInfo gardener)
		{
			GardenerInfo gi = new GardenerInfo();
			gi.TurnsToWait = game.AssetDatabase.GlobalGardenerData.CatchUpDelay;
			gi.GardenerFleetId = gardener.FleetId;
			gi.FleetId = game.GameDatabase.InsertFleet(this.PlayerID, 0, 0, 0, "Protean Follow", FleetType.FL_NORMAL);
			game.GameDatabase.InsertGardenerInfo(gi);
			int num1 = App.GetSafeRandom().NextInclusive(game.AssetDatabase.GlobalGardenerData.ProteanMobMin, game.AssetDatabase.GlobalGardenerData.ProteanMobMax);
			float num2 = 250000f;
			float maxValue = 2000f;
			List<Vector3> vector3List = new List<Vector3>();
			for (int index = 0; index < num1; ++index)
			{
				int shipID = game.GameDatabase.InsertShip(gi.FleetId, this.ProteanDesignId, null, (ShipParams)0, new int?(), 0);
				Vector3 vector3_1 = new Vector3();
				bool flag = false;
				while (!flag)
				{
					flag = true;
					vector3_1.Y = 0.0f;
					vector3_1.X = (App.GetSafeRandom().CoinToss(50) ? -1f : 1f) * App.GetSafeRandom().NextInclusive(0.0f, maxValue);
					vector3_1.Z = (App.GetSafeRandom().CoinToss(50) ? -1f : 1f) * App.GetSafeRandom().NextInclusive(0.0f, maxValue);
					foreach (Vector3 vector3_2 in vector3List)
					{
						if ((double)(vector3_2 - vector3_1).LengthSquared < (double)num2)
						{
							flag = false;
							break;
						}
					}
				}
				vector3List.Add(vector3_1);
				game.GameDatabase.UpdateShipFleetPosition(shipID, new Vector3?(vector3_1));
			}
		}

		public void HandleGardenerCaptured(
		  GameSession game,
		  GameDatabase gamedb,
		  int playerId,
		  int gardenerId)
		{
			GardenerInfo gardenerInfo = gamedb.GetGardenerInfo(gardenerId);
			if (gardenerInfo == null)
				return;
			FleetInfo fleetInfo = gamedb.GetFleetInfo(gardenerInfo.FleetId);
			if (fleetInfo == null)
				return;
			if (gardenerInfo.DeepSpaceSystemId.HasValue)
			{
				foreach (StationInfo stationInfo in game.GameDatabase.GetStationForSystem(gardenerInfo.DeepSpaceSystemId.Value).ToList<StationInfo>())
					game.GameDatabase.DestroyStation(game, stationInfo.ID, 0);
			}
			fleetInfo.PlayerID = playerId;
			gamedb.UpdateFleetInfo(fleetInfo);
			FleetLocation fleetLocation = gamedb.GetFleetLocation(fleetInfo.ID, false);
			List<StarSystemInfo> closestStars = EncounterTools.GetClosestStars(gamedb, fleetLocation.Coords);
			StarSystemInfo starSystemInfo1 = (StarSystemInfo)null;
			foreach (StarSystemInfo starSystemInfo2 in closestStars)
			{
				int? systemOwningPlayer = gamedb.GetSystemOwningPlayer(starSystemInfo2.ID);
				if (systemOwningPlayer.HasValue && systemOwningPlayer.Value == playerId)
				{
					starSystemInfo1 = starSystemInfo2;
					break;
				}
			}
			if (!(starSystemInfo1 != (StarSystemInfo)null))
				return;
			MissionInfo missionByFleetId = gamedb.GetMissionByFleetID(fleetInfo.ID);
			if (missionByFleetId != null)
				gamedb.RemoveMission(missionByFleetId.ID);
			MoveOrderInfo orderInfoByFleetId = gamedb.GetMoveOrderInfoByFleetID(fleetInfo.ID);
			if (orderInfoByFleetId != null)
				gamedb.RemoveMoveOrder(orderInfoByFleetId.ID);
			int missionID = gamedb.InsertMission(fleetInfo.ID, MissionType.RETURN, starSystemInfo1.ID, 0, 0, 0, false, new int?());
			gamedb.InsertWaypoint(missionID, WaypointType.TravelTo, new int?(starSystemInfo1.ID));
			gamedb.InsertMoveOrder(fleetInfo.ID, 0, fleetLocation.Coords, starSystemInfo1.ID, Vector3.Zero);
			this.SpawnProteanChaser(game, gardenerInfo);
		}
	}
}
