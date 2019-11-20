// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Encounters.Swarmers
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Encounters
{
	internal class Swarmers
	{
		private int PlayerId = -1;
		private const string FactionName = "swarm";
		private const string PlayerName = "Swarm";
		private const string PlayerAvatar = "\\base\\factions\\swarm\\avatars\\Swarm_Avatar.tga";
		private const int SpawnHiveDelay = 1;
		public const string SwarmHiveFleetName = "Swarm";
		public const string SwarmQueenFleetName = "Swarm Queen";
		private const string GuardianSectionName = "guardian.section";
		private const string SwarmerSectionName = "swarmer.section";
		private const string HiveStage1SectionName = "hive_stage1.section";
		private const string LarvalQueenSectionName = "larval_queen.section";
		private const string SwarmQueenSectionName = "swarm_queen.section";
		private int _guardianDesignId;
		private int _swarmerDesignId;
		private int _hiveStage1DesignId;
		private int _larvalQueenDesignId;
		private int _swarmQueenDesignId;

		public int PlayerID
		{
			get
			{
				return this.PlayerId;
			}
		}

		public int HiveDesignID
		{
			get
			{
				return this._hiveStage1DesignId;
			}
		}

		public int GuardianDesignID
		{
			get
			{
				return this._guardianDesignId;
			}
		}

		public int SwarmerDesignID
		{
			get
			{
				return this._swarmerDesignId;
			}
		}

		public int LarvalQueenDesignID
		{
			get
			{
				return this._larvalQueenDesignId;
			}
		}

		public int SwarmQueenDesignID
		{
			get
			{
				return this._swarmQueenDesignId;
			}
		}

		private void InitDesigns(GameDatabase db)
		{
			List<DesignInfo> list = db.GetDesignInfosForPlayer(this.PlayerId).ToList<DesignInfo>();
			this._guardianDesignId = db.AddOrGetEncounterDesignInfo((IEnumerable<DesignInfo>)list, this.PlayerId, "Swarm Guardian", "swarm", "guardian.section");
			this._swarmerDesignId = db.AddOrGetEncounterDesignInfo((IEnumerable<DesignInfo>)list, this.PlayerId, "Swarm Swarmer", "swarm", "swarmer.section");
			this._hiveStage1DesignId = db.AddOrGetEncounterDesignInfo((IEnumerable<DesignInfo>)list, this.PlayerId, "Swarm Hive", "swarm", "hive_stage1.section");
			this._larvalQueenDesignId = db.AddOrGetEncounterDesignInfo((IEnumerable<DesignInfo>)list, this.PlayerId, "Swarm Larval Queen", "swarm", "larval_queen.section");
			this._swarmQueenDesignId = db.AddOrGetEncounterDesignInfo((IEnumerable<DesignInfo>)list, this.PlayerId, "Swarm Queen", "swarm", "swarm_queen.section");
		}

		public static Swarmers InitializeEncounter(GameDatabase gamedb, AssetDatabase assetdb)
		{
			Swarmers swarmers = new Swarmers();
			swarmers.PlayerId = gamedb.InsertPlayer("Swarm", "swarm", new int?(), assetdb.RandomEncounterPrimaryColor, new Vector3(0.0f), "", "\\base\\factions\\swarm\\avatars\\Swarm_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal);
			foreach (PlayerInfo playerInfo in gamedb.GetStandardPlayerInfos().ToList<PlayerInfo>())
				gamedb.ChangeDiplomacyState(playerInfo.ID, swarmers.PlayerId, DiplomacyState.WAR);
			swarmers.InitDesigns(gamedb);
			return swarmers;
		}

		public static Swarmers ResumeEncounter(GameDatabase gamedb)
		{
			Swarmers swarmers = new Swarmers();
			PlayerInfo playerInfo = gamedb.GetPlayerInfos().ToList<PlayerInfo>().FirstOrDefault<PlayerInfo>((Func<PlayerInfo, bool>)(x =>
		   {
			   if (!x.isStandardPlayer)
				   return x.Name.Contains("Swarm");
			   return false;
		   }));
			swarmers.PlayerId = playerInfo.ID;
			swarmers.InitDesigns(gamedb);
			return swarmers;
		}

		public void AddInstance(GameDatabase gamedb, AssetDatabase assetdb, int SystemId, int OrbitId)
		{
			int num1 = OrbitId;
			if (gamedb.GetLargeAsteroidInfo(OrbitId) == null && gamedb.GetAsteroidBeltInfo(OrbitId) == null)
			{
				OrbitalObjectInfo orbitalObjectInfo = gamedb.GetOrbitalObjectInfo(OrbitId);
				StarSystemInfo starSystemInfo = gamedb.GetStarSystemInfo(SystemId);
				gamedb.RemoveOrbitalObject(orbitalObjectInfo.ID);
				if (!orbitalObjectInfo.ParentID.HasValue)
				{
					num1 = gamedb.InsertAsteroidBelt(new int?(), orbitalObjectInfo.StarSystemID, orbitalObjectInfo.OrbitalPath, "Swarmed Asteroid Belt", App.GetSafeRandom().Next());
				}
				else
				{
					int num2 = gamedb.GetStarSystemOrbitalObjectInfos(SystemId).Where<OrbitalObjectInfo>((Func<OrbitalObjectInfo, bool>)(x => !x.ParentID.HasValue)).Count<OrbitalObjectInfo>();
					OrbitalPath orbitalPath = gamedb.OrbitNumberToOrbitalPath(num2 + 1, StellarClass.Parse(starSystemInfo.StellarClass).Size, new float?());
					num1 = gamedb.InsertAsteroidBelt(new int?(), orbitalObjectInfo.StarSystemID, orbitalPath, "Swarmed Asteroid Belt", App.GetSafeRandom().Next());
				}
			}
			SwarmerInfo si = new SwarmerInfo();
			si.GrowthStage = 0;
			si.SystemId = SystemId;
			si.OrbitalId = num1;
			si.HiveFleetId = new int?(gamedb.InsertFleet(this.PlayerId, 0, SystemId, SystemId, "Swarm", FleetType.FL_NORMAL));
			si.QueenFleetId = new int?();
			si.SpawnHiveDelay = 1;
			gamedb.InsertSwarmerInfo(si);
			gamedb.InsertShip(si.HiveFleetId.Value, this._hiveStage1DesignId, null, (ShipParams)0, new int?(), 0);
		}

		public void UpdateTurn(GameSession game, int id)
		{
			Random safeRandom = App.GetSafeRandom();
			ShipInfo shipInfo = (ShipInfo)null;
			SwarmerInfo si = game.GameDatabase.GetSwarmerInfo(id);
			if (si == null)
			{
				game.GameDatabase.RemoveEncounter(id);
			}
			else
			{
				if (si.QueenFleetId.HasValue)
				{
					FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(si.QueenFleetId.Value);
					if (fleetInfo == null)
					{
						si.QueenFleetId = new int?();
						si.SpawnHiveDelay = 1;
					}
					else if (fleetInfo.SystemID != 0 && fleetInfo.SystemID != si.SystemId)
					{
						if (si.SpawnHiveDelay <= 0)
						{
							int id1 = game.GameDatabase.GetStarSystemOrbitalObjectInfos(fleetInfo.SystemID).ToList<OrbitalObjectInfo>().First<OrbitalObjectInfo>((Func<OrbitalObjectInfo, bool>)(x => game.GameDatabase.GetAsteroidBeltInfo(x.ID) != null)).ID;
							this.AddInstance(game.GameDatabase, game.AssetDatabase, fleetInfo.SystemID, id1);
							game.GameDatabase.RemoveFleet(fleetInfo.ID);
							si.QueenFleetId = new int?();
							si.SpawnHiveDelay = 1;
							foreach (int playerid in game.GameDatabase.GetStandardPlayerIDs().ToList<int>())
							{
								if (StarMap.IsInRange(game.GameDatabase, playerid, fleetInfo.SystemID))
									game.GameDatabase.InsertTurnEvent(new TurnEvent()
									{
										EventType = TurnEventType.EV_SWARM_INFESTATION,
										EventMessage = TurnEventMessage.EM_SWARM_INFESTATION,
										PlayerID = playerid,
										SystemID = fleetInfo.SystemID,
										TurnNumber = game.GameDatabase.GetTurnCount()
									});
							}
						}
						else
							--si.SpawnHiveDelay;
					}
				}
				if (si.HiveFleetId.HasValue)
				{
					if (game.GameDatabase.GetFleetInfo(si.HiveFleetId.Value) == null)
					{
						si.HiveFleetId = new int?();
					}
					else
					{
						shipInfo = game.GameDatabase.GetShipInfoByFleetID(si.HiveFleetId.Value, true).ToList<ShipInfo>().FirstOrDefault<ShipInfo>((Func<ShipInfo, bool>)(x => x.DesignInfo.ID == this._larvalQueenDesignId));
						int num = si.GrowthStage - game.AssetDatabase.GlobalSwarmerData.GrowthRateLarvaSpawn;
						if (num > 0 && !si.QueenFleetId.HasValue && (shipInfo == null && safeRandom.CoinToss(Math.Max(0, num * 10))))
						{
							game.GameDatabase.InsertShip(si.HiveFleetId.Value, this._larvalQueenDesignId, "Swarm Larval Queen", (ShipParams)0, new int?(), 0);
							si.GrowthStage = 0;
						}
					}
				}
				if (si.GrowthStage > game.AssetDatabase.GlobalSwarmerData.GrowthRateQueenSpawn && shipInfo != null)
				{
					List<StarSystemInfo> list1 = game.GameDatabase.GetStarSystemInfos().ToList<StarSystemInfo>();
					list1.RemoveAll((Predicate<StarSystemInfo>)(x => !game.GameDatabase.GetStarSystemOrbitalObjectInfos(x.ID).Any<OrbitalObjectInfo>((Func<OrbitalObjectInfo, bool>)(y => game.GameDatabase.GetAsteroidBeltInfo(y.ID) != null))));
					foreach (SwarmerInfo swarmerInfo in game.GameDatabase.GetSwarmerInfos().ToList<SwarmerInfo>())
					{
						SwarmerInfo swarmer = swarmerInfo;
						int targetID = 0;
						int fleetSystem = 0;
						if (swarmer.QueenFleetId.HasValue)
						{
							MissionInfo missionByFleetId = game.GameDatabase.GetMissionByFleetID(swarmer.QueenFleetId.Value);
							if (missionByFleetId != null)
								targetID = missionByFleetId.TargetSystemID;
							FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(swarmer.QueenFleetId.Value);
							if (fleetInfo != null)
								fleetSystem = fleetInfo.SystemID;
						}
						list1.RemoveAll((Predicate<StarSystemInfo>)(x =>
					   {
						   if (x.ID != swarmer.SystemId && x.ID != targetID)
							   return x.ID == fleetSystem;
						   return true;
					   }));
					}
					List<StarSystemInfo> list2 = list1.OrderBy<StarSystemInfo, float>((Func<StarSystemInfo, float>)(x => (game.GameDatabase.GetStarSystemOrigin(si.SystemId) - x.Origin).Length)).ToList<StarSystemInfo>();
					if (list2.Count > 0)
					{
						StarSystemInfo starSystemInfo = list2[safeRandom.Next(0, Math.Min(Math.Max(0, list2.Count - 1), 3))];
						si.QueenFleetId = new int?(game.GameDatabase.InsertFleet(this.PlayerId, 0, si.SystemId, si.SystemId, "Swarm Queen", FleetType.FL_NORMAL));
						game.GameDatabase.RemoveShip(shipInfo.ID);
						game.GameDatabase.InsertShip(si.QueenFleetId.Value, this._swarmQueenDesignId, "Swarm Queen", (ShipParams)0, new int?(), 0);
						int missionID = game.GameDatabase.InsertMission(si.QueenFleetId.Value, MissionType.STRIKE, starSystemInfo.ID, 0, 0, 0, true, new int?());
						game.GameDatabase.InsertWaypoint(missionID, WaypointType.TravelTo, new int?(starSystemInfo.ID));
					}
					si.GrowthStage = 0;
				}
				if (si.HiveFleetId.HasValue && !si.QueenFleetId.HasValue)
					++si.GrowthStage;
				if (!si.HiveFleetId.HasValue && !si.QueenFleetId.HasValue)
					game.GameDatabase.RemoveEncounter(si.Id);
				else
					game.GameDatabase.UpdateSwarmerInfo(si);
			}
		}

		public static void SetInitialSwarmerPosition(GameSession sim, SwarmerInfo si, int systemId)
		{
			List<CombatZonePositionInfo> combatZonesForSystem = Kerberos.Sots.GameStates.StarSystem.GetCombatZonesForSystem(sim, systemId, 1f);
			if (combatZonesForSystem.Count == 0)
				return;
			FleetInfo fleetInfo = si.QueenFleetId.HasValue ? sim.GameDatabase.GetFleetInfo(si.QueenFleetId.Value) : (FleetInfo)null;
			if (fleetInfo == null || combatZonesForSystem.Count <= 0)
				return;
			Vector3 center = combatZonesForSystem.Last<CombatZonePositionInfo>().Center;
			float num1 = 0.0f;
			int num2 = si != null ? si.OrbitalId : 0;
			if (num2 == 0)
			{
				float val1 = 0.0f;
				foreach (OrbitalObjectInfo orbitalObjectInfo in sim.GameDatabase.GetStarSystemOrbitalObjectInfos(systemId).ToList<OrbitalObjectInfo>())
				{
					if (sim.GameDatabase.GetAsteroidBeltInfo(orbitalObjectInfo.ID) != null)
					{
						Matrix orbitalTransform = sim.GameDatabase.GetOrbitalTransform(orbitalObjectInfo.ID);
						double num3 = (double)Math.Max(val1, orbitalTransform.Position.LengthSquared);
					}
				}
				if ((double)val1 > 0.0)
					num1 = (float)Math.Sqrt((double)val1);
			}
			if (num2 != 0 && (double)num1 == 0.0)
			{
				Vector3 zero = Vector3.Zero;
				Matrix orbitalTransform = sim.GameDatabase.GetOrbitalTransform(num2);
				List<LargeAsteroidInfo> list = sim.GameDatabase.GetLargeAsteroidsInAsteroidBelt(num2).ToList<LargeAsteroidInfo>();
				if (list.Count > 0)
				{
					int id = list.First<LargeAsteroidInfo>().ID;
					orbitalTransform = sim.GameDatabase.GetOrbitalTransform(id);
				}
				num1 = orbitalTransform.Position.Length;
			}
			Vector3 vector3 = Vector3.UnitZ;
			if (fleetInfo.PreviousSystemID.HasValue)
			{
				int? previousSystemId = fleetInfo.PreviousSystemID;
				int num3 = systemId;
				if ((previousSystemId.GetValueOrDefault() != num3 ? 1 : (!previousSystemId.HasValue ? 1 : 0)) != 0)
				{
					Vector3 starSystemOrigin = sim.GameDatabase.GetStarSystemOrigin(systemId);
					vector3 = sim.GameDatabase.GetStarSystemOrigin(fleetInfo.PreviousSystemID.Value) - starSystemOrigin;
					vector3.Y = 0.0f;
					double num4 = (double)vector3.Normalize();
				}
			}
			Vector3 position = vector3 * num1;
			Vector3 forward = -position;
			double num5 = (double)forward.Normalize();
			List<int> list1 = sim.GameDatabase.GetShipsByFleetID(fleetInfo.ID).ToList<int>();
			if (list1.Count <= 0)
				return;
			sim.GameDatabase.UpdateShipSystemPosition(list1.First<int>(), new Matrix?(Matrix.CreateWorld(position, forward, Vector3.UnitY)));
		}

		public static void ClearTransform(GameDatabase db, SwarmerInfo si)
		{
			if (si == null || !si.QueenFleetId.HasValue)
				return;
			ShipInfo shipInfo = db.GetShipInfoByFleetID(si.QueenFleetId.Value, false).FirstOrDefault<ShipInfo>();
			if (shipInfo == null || !db.GetShipSystemPosition(shipInfo.ID).HasValue)
				return;
			db.UpdateShipSystemPosition(shipInfo.ID, new Matrix?());
		}

		public static Matrix GetBaseEnemyFleetTrans(App app, int systemID)
		{
			return Swarmers.GetSpawnTransform(app, systemID);
		}

		public static Matrix GetSpawnTransform(App app, int systemID)
		{
			StarSystemInfo starSystemInfo = app.GameDatabase.GetStarSystemInfo(systemID);
			if (starSystemInfo == (StarSystemInfo)null || starSystemInfo.IsDeepSpace)
				return Matrix.Identity;
			SwarmerInfo swarmerInfo = app.GameDatabase.GetSwarmerInfos().FirstOrDefault<SwarmerInfo>((Func<SwarmerInfo, bool>)(x => x.SystemId == systemID));
			if (swarmerInfo != null && swarmerInfo.QueenFleetId.HasValue)
			{
				ShipInfo shipInfo = app.GameDatabase.GetShipInfoByFleetID(swarmerInfo.QueenFleetId.Value, false).FirstOrDefault<ShipInfo>();
				if (shipInfo != null)
				{
					Matrix? shipSystemPosition = app.GameDatabase.GetShipSystemPosition(shipInfo.ID);
					if (shipSystemPosition.HasValue)
						return shipSystemPosition.Value;
				}
			}
			int num1 = swarmerInfo != null ? swarmerInfo.OrbitalId : 0;
			if (num1 == 0)
			{
				Matrix matrix = Matrix.Identity;
				float num2 = 0.0f;
				foreach (OrbitalObjectInfo orbitalObjectInfo in app.GameDatabase.GetStarSystemOrbitalObjectInfos(systemID).ToList<OrbitalObjectInfo>())
				{
					if (app.GameDatabase.GetAsteroidBeltInfo(orbitalObjectInfo.ID) != null)
					{
						Matrix orbitalTransform = app.GameDatabase.GetOrbitalTransform(orbitalObjectInfo.ID);
						float lengthSquared = orbitalTransform.Position.LengthSquared;
						if ((double)lengthSquared > (double)num2)
						{
							num2 = lengthSquared;
							matrix = orbitalTransform;
							num1 = orbitalObjectInfo.ID;
						}
					}
				}
				if ((double)num2 > 0.0)
					return matrix;
			}
			if (num1 != 0)
			{
				Vector3 zero = Vector3.Zero;
				Matrix orbitalTransform = app.GameDatabase.GetOrbitalTransform(num1);
				List<LargeAsteroidInfo> list = app.GameDatabase.GetLargeAsteroidsInAsteroidBelt(num1).ToList<LargeAsteroidInfo>();
				if (list.Count > 0)
				{
					int id = list.First<LargeAsteroidInfo>().ID;
					orbitalTransform = app.GameDatabase.GetOrbitalTransform(id);
					Vector3 position = orbitalTransform.Position;
					float num2 = position.Normalize();
					Vector3 vector3 = Vector3.Cross(position, Vector3.UnitY) * 1000f;
					orbitalTransform.Position = Vector3.Normalize(vector3) * num2;
				}
				return orbitalTransform;
			}
			Matrix rotationYpr = Matrix.CreateRotationYPR(MathHelper.DegreesToRadians(new Random().NextInclusive(0.0f, 360f)), 0.0f, 0.0f);
			return Matrix.CreateWorld(rotationYpr.Forward * 50000f, -rotationYpr.Forward, Vector3.UnitY);
		}
	}
}
