// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Encounters.NeutronStar
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Encounters
{
	internal class NeutronStar
	{
		private int PlayerId = -1;
		private const string FactionName = "grandmenaces";
		private const string PlayerName = "NetronStar";
		private const string FleetName = "NetronStar";
		private const string PlayerAvatar = "\\base\\factions\\grandmenaces\\avatars\\Neutron_Star_Avatar.tga";
		private const string NeutronDesignFile = "neutron_star.section";
		public static bool ForceEncounter;
		private int _neutronDesignId;

		public int PlayerID
		{
			get
			{
				return this.PlayerId;
			}
		}

		public int NeutronDesignId
		{
			get
			{
				return this._neutronDesignId;
			}
		}

		public static NeutronStar InitializeEncounter(
		  GameDatabase gamedb,
		  AssetDatabase assetdb)
		{
			if (assetdb.GetFaction("grandmenaces") == null)
				return (NeutronStar)null;
			NeutronStar neutronStar = new NeutronStar();
			neutronStar.PlayerId = gamedb.InsertPlayer("NetronStar", "grandmenaces", new int?(), assetdb.RandomEncounterPrimaryColor, new Vector3(0.0f), "", "\\base\\factions\\grandmenaces\\avatars\\Neutron_Star_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal);
			DesignInfo design = new DesignInfo(neutronStar.PlayerId, nameof(NeutronStar), new string[1]
			{
		string.Format("factions\\{0}\\sections\\{1}", (object) "grandmenaces", (object) "neutron_star.section")
			});
			neutronStar._neutronDesignId = gamedb.InsertDesignByDesignInfo(design);
			foreach (int towardsPlayerID in gamedb.GetPlayerIDs().ToList<int>())
				gamedb.UpdateDiplomacyState(neutronStar.PlayerId, towardsPlayerID, DiplomacyState.NEUTRAL, 1000, true);
			return neutronStar;
		}

		public static NeutronStar ResumeEncounter(GameDatabase gamedb)
		{
			if (gamedb.AssetDatabase.GetFaction("grandmenaces") == null)
				return (NeutronStar)null;
			NeutronStar neutronStar = new NeutronStar();
			PlayerInfo playerInfo = gamedb.GetPlayerInfos().ToList<PlayerInfo>().FirstOrDefault<PlayerInfo>((Func<PlayerInfo, bool>)(x =>
		   {
			   if (!x.isStandardPlayer)
				   return x.Name.Contains("NetronStar");
			   return false;
		   }));
			neutronStar.PlayerId = playerInfo == null ? gamedb.InsertPlayer("NetronStar", "grandmenaces", new int?(), new Vector3(0.0f), new Vector3(0.0f), "", "\\base\\factions\\grandmenaces\\avatars\\Neutron_Star_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal) : playerInfo.ID;
			DesignInfo designInfo = gamedb.GetDesignInfosForPlayer(neutronStar.PlayerId).ToList<DesignInfo>().FirstOrDefault<DesignInfo>((Func<DesignInfo, bool>)(x => x.DesignSections[0].FilePath.EndsWith("neutron_star.section")));
			if (designInfo == null)
			{
				DesignInfo design = new DesignInfo(neutronStar.PlayerId, nameof(NeutronStar), new string[1]
				{
		  string.Format("factions\\{0}\\sections\\{1}", (object) "grandmenaces", (object) "neutron_star.section")
				});
				neutronStar._neutronDesignId = gamedb.InsertDesignByDesignInfo(design);
			}
			else
				neutronStar._neutronDesignId = designInfo.ID;
			return neutronStar;
		}

		public void UpdateTurn(GameSession game, int id)
		{
			NeutronStarInfo nsi = game.GameDatabase.GetNeutronStarInfo(id);
			FleetInfo fleetInfo = nsi != null ? game.GameDatabase.GetFleetInfo(nsi.FleetId) : (FleetInfo)null;
			if (fleetInfo == null)
			{
				game.GameDatabase.RemoveEncounter(id);
			}
			else
			{
				MissionInfo missionByFleetId = game.GameDatabase.GetMissionByFleetID(fleetInfo.ID);
				if (game.GameDatabase.GetMoveOrderInfoByFleetID(fleetInfo.ID) != null)
				{
					FleetLocation fl = game.GameDatabase.GetFleetLocation(fleetInfo.ID, false);
					if (!nsi.DeepSpaceSystemId.HasValue || game.GameDatabase.GetStarSystemInfos().Any<StarSystemInfo>((Func<StarSystemInfo, bool>)(x =>
				   {
					   if (x.ID != nsi.DeepSpaceSystemId.Value)
						   return (double)(game.GameDatabase.GetStarSystemOrigin(x.ID) - fl.Coords).LengthSquared < 9.99999974737875E-05;
					   return false;
				   })))
						return;
					game.GameDatabase.UpdateStarSystemOrigin(nsi.DeepSpaceSystemId.Value, fl.Coords);
				}
				else
				{
					if (missionByFleetId != null)
						game.GameDatabase.RemoveMission(missionByFleetId.ID);
					StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(fleetInfo.SystemID);
					foreach (int standardPlayerId in game.GameDatabase.GetStandardPlayerIDs())
					{
						if (StarMap.IsInRange(game.GameDatabase, standardPlayerId, game.GameDatabase.GetFleetLocation(fleetInfo.ID, false).Coords, 1f, (Dictionary<int, List<ShipInfo>>)null))
							game.GameDatabase.InsertTurnEvent(new TurnEvent()
							{
								EventType = TurnEventType.EV_NEUTRON_STAR_DESTROYED_SYSTEM,
								EventMessage = TurnEventMessage.EM_NEUTRON_STAR_DESTROYED_SYSTEM,
								SystemID = starSystemInfo.ID,
								PlayerID = this.PlayerID,
								TurnNumber = game.GameDatabase.GetTurnCount()
							});
					}
					if (nsi.DeepSpaceSystemId.HasValue)
					{
						game.GameDatabase.RemoveFleet(fleetInfo.ID);
						game.GameDatabase.RemoveEncounter(id);
						game.GameDatabase.DestroyStarSystem(game, nsi.DeepSpaceSystemId.Value);
					}
					if (fleetInfo.SystemID != 0)
						game.GameDatabase.DestroyStarSystem(game, starSystemInfo.ID);
					if (!(game.App.CurrentState is StarMapState))
						return;
					((StarMapState)game.App.CurrentState).ClearSelectedObject();
					((StarMapState)game.App.CurrentState).RefreshStarmap(StarMapState.StarMapRefreshType.REFRESH_ALL);
				}
			}
		}

		public void ExecuteInstance(GameDatabase gamedb, AssetDatabase assetdb, int systemid)
		{
			NeutronStarInfo nsi = new NeutronStarInfo();
			nsi.FleetId = gamedb.InsertFleet(this.PlayerId, 0, 0, 0, "NetronStar", FleetType.FL_NORMAL);
			nsi.TargetSystemId = systemid;
			Vector3 starSystemOrigin = gamedb.GetStarSystemOrigin(nsi.TargetSystemId);
			Vector3 travelDirection = this.GetTravelDirection(gamedb, starSystemOrigin, nsi.TargetSystemId);
			int shipID = gamedb.InsertShip(nsi.FleetId, this._neutronDesignId, null, (ShipParams)0, new int?(), 0);
			gamedb.UpdateShipSystemPosition(shipID, new Matrix?(Matrix.Identity));
			Vector3 vector3 = starSystemOrigin - travelDirection * 20f;
			int missionID = gamedb.InsertMission(nsi.FleetId, MissionType.STRIKE, nsi.TargetSystemId, 0, 0, 0, true, new int?());
			gamedb.InsertWaypoint(missionID, WaypointType.TravelTo, new int?(nsi.TargetSystemId));
			gamedb.InsertMoveOrder(nsi.FleetId, 0, vector3, nsi.TargetSystemId, Vector3.Zero);
			nsi.DeepSpaceSystemId = new int?(gamedb.InsertStarSystem(new int?(), App.Localize("@UI_STARMAP_ENCOUNTER_DEEPSPACE"), new int?(), "Deepspace", vector3, false, false, new int?()));
			nsi.DeepSpaceOrbitalId = new int?(gamedb.InsertOrbitalObject(new int?(), nsi.DeepSpaceSystemId.Value, new OrbitalPath()
			{
				Scale = new Vector2(20000f, Ellipse.CalcSemiMinorAxis(20000f, 0.0f)),
				InitialAngle = 0.0f
			}, "space"));
			gamedb.GetOrbitalTransform(nsi.DeepSpaceOrbitalId.Value);
			gamedb.InsertNeutronStarInfo(nsi);
		}

		public void AddInstance(GameDatabase gamedb, AssetDatabase assetdb, int? targetSystem = null)
		{
			Random safeRandom = App.GetSafeRandom();
			if (targetSystem == null)
			{
				List<ColonyInfo> source = gamedb.GetColonyInfos().ToList<ColonyInfo>();
				List<int> list = (from x in source
								  select gamedb.GetOrbitalObjectInfo(x.OrbitalObjectID).StarSystemID).ToList<int>();
				List<NeutronStarInfo> neutronStars = new List<NeutronStarInfo>();
				list.RemoveAll((int x) => neutronStars.Any((NeutronStarInfo y) => y.TargetSystemId == x));
				if (list.Count == 0)
				{
					return;
				}
				targetSystem = new int?(safeRandom.Choose(list));
			}
			gamedb.InsertIncomingGM(targetSystem.Value, EasterEgg.GM_NEUTRON_STAR, gamedb.GetTurnCount() + 1);
			List<PlayerInfo> list2 = gamedb.GetStandardPlayerInfos().ToList<PlayerInfo>();
			foreach (PlayerInfo playerInfo in list2)
			{
				if (gamedb.GetStratModifier<int>(StratModifiers.GrandMenaceWarningTime, playerInfo.ID) > 0)
				{
					gamedb.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_INCOMING_NEUTRON_STAR,
						EventMessage = TurnEventMessage.EM_INCOMING_NEUTRON_STAR,
						PlayerID = playerInfo.ID,
						SystemID = targetSystem.Value,
						TurnNumber = gamedb.GetTurnCount()
					});
				}
			}
		}

		private Vector3 GetTravelDirection(
		  GameDatabase gamedb,
		  Vector3 systemOrigin,
		  int targetSystemId)
		{
			return Vector3.Normalize(systemOrigin);
		}

		public static void GenerateMeteorAndCometEncounters(App game)
		{
			List<NeutronStarInfo> list1 = game.GameDatabase.GetNeutronStarInfos().ToList<NeutronStarInfo>();
			if (list1.Count == 0)
				return;
			float fleetTravelSpeed = Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(game.Game, list1.First<NeutronStarInfo>().FleetId, false);
			List<StarSystemInfo> list2 = game.GameDatabase.GetStarSystemInfos().ToList<StarSystemInfo>();
			float num1 = game.AssetDatabase.GlobalNeutronStarData.AffectRange * game.AssetDatabase.GlobalNeutronStarData.AffectRange;
			Dictionary<int, List<int>> dictionary1 = new Dictionary<int, List<int>>();
			Dictionary<int, float> dictionary2 = new Dictionary<int, float>();
			List<int> intList = new List<int>();
			foreach (NeutronStarInfo neutronStarInfo in list1)
			{
				FleetLocation fleetLocation = game.GameDatabase.GetFleetLocation(neutronStarInfo.FleetId, true);
				if (fleetLocation != null)
				{
					Vector3 coords = fleetLocation.Coords;
					Vector3 vector3 = fleetLocation.Direction.HasValue ? fleetLocation.Direction.Value * fleetTravelSpeed + fleetLocation.Coords : fleetLocation.Coords;
					foreach (StarSystemInfo starSystemInfo in list2)
					{
						float lengthSquared = (coords - starSystemInfo.Origin).LengthSquared;
						if ((double)lengthSquared <= (double)num1)
						{
							if (dictionary2.ContainsKey(starSystemInfo.ID))
								dictionary2[starSystemInfo.ID] = Math.Min(dictionary2[starSystemInfo.ID], lengthSquared);
							else
								dictionary2.Add(starSystemInfo.ID, lengthSquared);
						}
						if ((double)(vector3 - starSystemInfo.Origin).LengthSquared <= (double)num1)
						{
							if (!intList.Contains(starSystemInfo.ID))
								intList.Add(starSystemInfo.ID);
							if (dictionary2.ContainsKey(starSystemInfo.ID))
								dictionary2[starSystemInfo.ID] = Math.Min(dictionary2[starSystemInfo.ID], lengthSquared);
							else
								dictionary2.Add(starSystemInfo.ID, lengthSquared);
						}
					}
				}
			}
			Random safeRandom = App.GetSafeRandom();
			foreach (KeyValuePair<int, float> keyValuePair in dictionary2)
			{
				KeyValuePair<int, float> inRangeSys = keyValuePair;
				List<ColonyInfo> list3 = game.GameDatabase.GetColonyInfosForSystem(inRangeSys.Key).ToList<ColonyInfo>();
				List<int> source = new List<int>();
				foreach (ColonyInfo colonyInfo in list3)
				{
					Player player = game.GetPlayer(colonyInfo.PlayerID);
					if (player != null && player.IsStandardPlayer)
					{
						if (intList.Contains(colonyInfo.CachedStarSystemID))
						{
							if (!dictionary1.ContainsKey(player.ID))
								dictionary1.Add(colonyInfo.PlayerID, new List<int>());
							if (!dictionary1[colonyInfo.PlayerID].Contains(colonyInfo.CachedStarSystemID))
								dictionary1[colonyInfo.PlayerID].Add(colonyInfo.CachedStarSystemID);
						}
						if (!player.IsAI() && !source.Contains(player.ID))
							source.Add(player.ID);
					}
				}
				if ((double)dictionary2[inRangeSys.Key] <= (double)num1 && source.Count != 0 && !source.Any<int>((Func<int, bool>)(x => game.GameDatabase.GetIncomingRandomsForPlayerThisTurn(x).Any<IncomingRandomInfo>((Func<IncomingRandomInfo, bool>)(y => y.SystemId == inRangeSys.Key)))))
				{
					float num2 = Math.Min((float)Math.Sqrt((double)inRangeSys.Value) / game.AssetDatabase.GlobalNeutronStarData.AffectRange, 1f);
					int odds = (int)(90.0 * (double)num2) + 10;
					if (safeRandom.CoinToss(odds))
					{
						if (safeRandom.CoinToss(game.AssetDatabase.GlobalNeutronStarData.MeteorRatio))
						{
							float intensity = (float)(((double)game.AssetDatabase.GlobalNeutronStarData.MaxMeteorIntensity - 1.0) * (double)num2 + 1.0);
							if (game.Game.ScriptModules.MeteorShower != null)
								game.Game.ScriptModules.MeteorShower.ExecuteEncounter(game.Game, inRangeSys.Key, intensity, true);
						}
						else if (game.Game.ScriptModules.Comet != null)
							game.Game.ScriptModules.Comet.ExecuteInstance(game.GameDatabase, game.AssetDatabase, inRangeSys.Key);
					}
				}
			}
			foreach (KeyValuePair<int, List<int>> keyValuePair in dictionary1)
			{
				if (keyValuePair.Value.Count != 0)
					game.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_NEUTRON_STAR_NEARBY,
						EventMessage = TurnEventMessage.EM_NEUTRON_STAR_NEARBY,
						PlayerID = keyValuePair.Key,
						NumShips = keyValuePair.Value.Count,
						TurnNumber = game.GameDatabase.GetTurnCount(),
						ShowsDialog = true
					});
			}
		}

		public static Matrix GetBaseEnemyFleetTrans()
		{
			return Matrix.Identity;
		}

		public static Matrix GetSpawnTransform()
		{
			return Matrix.Identity;
		}

		public static bool IsPlayerSystemsInNeutronStarEffectRanges(
		  GameSession game,
		  int playerId,
		  int systemId)
		{
			StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(systemId);
			if (starSystemInfo == (StarSystemInfo)null)
				return false;
			List<NeutronStarInfo> list = game.GameDatabase.GetNeutronStarInfos().ToList<NeutronStarInfo>();
			if (list.Count == 0)
				return false;
			bool flag = false;
			foreach (ColonyInfo colonyInfo in game.GameDatabase.GetColonyInfosForSystem(systemId).ToList<ColonyInfo>())
			{
				if (colonyInfo.PlayerID == playerId)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
				return false;
			float fleetTravelSpeed = Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(game, list.First<NeutronStarInfo>().FleetId, false);
			float num = game.GameDatabase.AssetDatabase.GlobalNeutronStarData.AffectRange * game.GameDatabase.AssetDatabase.GlobalNeutronStarData.AffectRange;
			foreach (NeutronStarInfo neutronStarInfo in list)
			{
				FleetLocation fleetLocation = game.GameDatabase.GetFleetLocation(neutronStarInfo.FleetId, true);
				if (fleetLocation != null && (double)((fleetLocation.Direction.HasValue ? fleetLocation.Direction.Value * fleetTravelSpeed + fleetLocation.Coords : fleetLocation.Coords) - starSystemInfo.Origin).LengthSquared <= (double)num)
					return true;
			}
			return false;
		}
	}
}
