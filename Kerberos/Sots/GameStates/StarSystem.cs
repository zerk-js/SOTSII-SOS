// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.StarSystem
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.GameStates
{
	[GameObjectType(InteropGameObjectType.IGOT_STARSYSTEM)]
	internal class StarSystem : GameObject, IDisposable, IActive
	{
		public static readonly int CombatZoneIndices = 144;
		private static float RadiiMult = 0.7f;
		private static float BaseOffset = 0.0f;
		public static readonly float[] CombatZoneMapRadii = new float[10]
		{
	  2f * StarSystem.RadiiMult,
	  6f * StarSystem.RadiiMult,
	  10f * StarSystem.RadiiMult,
	  14f * StarSystem.RadiiMult,
	  18f * StarSystem.RadiiMult,
	  22f * StarSystem.RadiiMult,
	  26f * StarSystem.RadiiMult,
	  30f * StarSystem.RadiiMult,
	  34f * StarSystem.RadiiMult,
	  38f * StarSystem.RadiiMult
		};
		public static readonly int[] CombatZoneMapAngleDivs = new int[10]
		{
	  8,
	  8,
	  8,
	  16,
	  16,
	  16,
	  24,
	  24,
	  24,
	  24
		};
		private float _scale = 1f;
		public BidirMap<IGameObject, int> ObjectMap = new BidirMap<IGameObject, int>();
		public BidirMap<IGameObject, StationInfo> StationInfoMap = new BidirMap<IGameObject, StationInfo>();
		public BidirMap<IGameObject, int> PlanetMap = new BidirMap<IGameObject, int>();
		public List<CombatZonePositionInfo> CombatZones = new List<CombatZonePositionInfo>();
		public List<NeighboringSystemInfo> NeighboringSystems = new List<NeighboringSystemInfo>();
		public List<NodePoints> VisibleNodePoints = new List<NodePoints>();
		public List<ApproachingFleet> ApproachingFleets = new List<ApproachingFleet>();
		public List<AsteroidBelt> AsteroidBelts = new List<AsteroidBelt>();
		public Vector3 SystemOrigin = new Vector3();
		public const float MasterScale = 5700f;
		public const float DefaultSystemViewScale = 1f;
		private OrbitPainter _orbitPainter;
		private bool _isDeepSpace;
		private readonly GameObjectSet _crits;
		private readonly GameObjectSet _slots;
		private Vector3 _origin;
		private bool _active;
		private int _system;
		private float _starRadius;
		public int _furthestRing;

		public OrbitPainter OrbitPainter
		{
			get
			{
				return this._orbitPainter;
			}
		}

		public bool IsDeepSpace
		{
			get
			{
				return this._isDeepSpace;
			}
		}

		public int SystemID
		{
			get
			{
				return this._system;
			}
		}

		public GameObjectSet Crits
		{
			get
			{
				return this._crits;
			}
		}

		private static IEnumerable<object> GetCombatZoneMapInitParams()
		{
			yield return (object)StarSystem.CombatZoneMapRadii.Length;
			foreach (float combatZoneMapRadius in StarSystem.CombatZoneMapRadii)
				yield return (object)combatZoneMapRadius;
			foreach (int combatZoneMapAngleDiv in StarSystem.CombatZoneMapAngleDivs)
				yield return (object)combatZoneMapAngleDiv;
		}

		public int GetCombatZoneIndexAtPosition(Vector3 position)
		{
			return this.GetCombatZoneIndexAtPosition(new Vector2(position.X, position.Z));
		}

		public int GetCombatZoneIndexAtPosition(Vector2 position)
		{
			position.X /= 5700f;
			position.Y /= 5700f;
			float lengthSq = position.LengthSq;
			int index1;
			for (index1 = 0; index1 < ((IEnumerable<float>)StarSystem.CombatZoneMapRadii).Count<float>() - 1; ++index1)
			{
				float num = (float)(((double)this.GetBaseOffset() + (double)StarSystem.CombatZoneMapRadii[index1]) * ((double)this.GetBaseOffset() + (double)StarSystem.CombatZoneMapRadii[index1]));
				if ((double)lengthSq < (double)num)
					break;
			}
			int num1 = 0;
			if (index1 > 0)
			{
				float num2 = 6.283185f / (float)StarSystem.CombatZoneMapAngleDivs[index1 - 1];
				num1 = (int)(Math.Abs((Math.Atan2((double)position.Y, (double)position.X) + 4.0 * Math.PI) % (2.0 * Math.PI)) / (double)num2);
			}
			int num3 = 0;
			for (int index2 = 0; index2 < index1 - 1; ++index2)
				num3 += StarSystem.CombatZoneMapAngleDivs[index2];
			return num3 + num1;
		}

		public static int GetCombatZoneIndex(int ring, int zone)
		{
			int num = 0;
			for (int index = 0; index < ring; ++index)
				num += StarSystem.CombatZoneMapAngleDivs[index];
			return num + zone;
		}

		public CombatZonePositionInfo GetClosestZoneToPosition(
		  App game,
		  int playerID,
		  Vector3 targetPosition)
		{
			CombatZonePositionInfo zonePositionInfo = (CombatZonePositionInfo)null;
			StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(this._system);
			if (starSystemInfo == (StarSystemInfo)null || starSystemInfo.ControlZones == null)
				return zonePositionInfo;
			float num = float.MaxValue;
			foreach (CombatZonePositionInfo combatZone in this.CombatZones)
			{
				if (playerID == 0 || combatZone.Player == playerID)
				{
					float lengthSquared = (combatZone.Center - targetPosition).LengthSquared;
					if ((double)lengthSquared < (double)num)
					{
						num = lengthSquared;
						zonePositionInfo = combatZone;
					}
				}
			}
			return zonePositionInfo;
		}

		public CombatZonePositionInfo GetCombatZonePositionInfo(
		  int ringIndex,
		  int zoneIndex)
		{
			return this.CombatZones.FirstOrDefault<CombatZonePositionInfo>((Func<CombatZonePositionInfo, bool>)(x =>
		   {
			   if (x.RingIndex == ringIndex)
				   return x.ZoneIndex == zoneIndex;
			   return false;
		   }));
		}

		public CombatZonePositionInfo GetEnteryZoneForOuterSystem(
		  int outerSystemID)
		{
			if (outerSystemID == 0 || outerSystemID == this._system)
				return (CombatZonePositionInfo)null;
			return this.GetCombatZonePositionInfo(this._furthestRing - 1, this.GetCombatZoneInRing(this._furthestRing - 1, this.App.GameDatabase.GetStarSystemOrigin(outerSystemID) - this.SystemOrigin));
		}

		public float GetBaseOffset()
		{
			return this._starRadius + StarSystem.BaseOffset;
		}

		public float GetStarRadius()
		{
			return this._starRadius * 5700f;
		}

		public int GetFurthestRing()
		{
			return this._furthestRing;
		}

		public float GetSystemRadius()
		{
			return (float)(((double)this.GetBaseOffset() + (double)StarSystem.CombatZoneMapRadii[this._furthestRing]) * 5700.0);
		}

		public int GetCombatZoneRingAtRange(float range)
		{
			range /= 5700f;
			int index = 0;
			while (index < ((IEnumerable<float>)StarSystem.CombatZoneMapRadii).Count<float>() - 1 && (double)range >= (double)(this.GetBaseOffset() + StarSystem.CombatZoneMapRadii[index]))
				++index;
			return index - 1;
		}

		public int GetCombatZoneInRing(int ring, Vector3 position)
		{
			int num1 = 0;
			if (ring >= 0)
			{
				float num2 = 6.283185f / (float)StarSystem.CombatZoneMapAngleDivs[ring];
				num1 = (int)(Math.Abs((Math.Atan2((double)position.Z, (double)position.X) + 4.0 * Math.PI) % (2.0 * Math.PI)) / (double)num2);
			}
			return num1;
		}

		public static void PaintSystemPlayerColor(GameDatabase gamedb, int systemID, int playerID)
		{
			List<int> zones = new List<int>();
			for (int index = 0; index < StarSystem.CombatZoneIndices; ++index)
				zones.Add(playerID);
			gamedb.UpdateSystemCombatZones(systemID, zones);
		}

		public static void RemoveSystemPlayerColor(GameDatabase gamedb, int systemID, int playerID)
		{
			int num = gamedb.GetSystemOwningPlayer(systemID) ?? 0;
			StarSystemInfo starSystemInfo = gamedb.GetStarSystemInfo(systemID);
			if (!(starSystemInfo != (StarSystemInfo)null) || starSystemInfo.ControlZones == null)
				return;
			for (int index = 0; index < starSystemInfo.ControlZones.Count; ++index)
			{
				if (starSystemInfo.ControlZones[index] == 0 || starSystemInfo.ControlZones[index] == playerID)
					starSystemInfo.ControlZones[index] = num;
			}
			gamedb.UpdateSystemCombatZones(systemID, starSystemInfo.ControlZones);
		}

		public static void RestoreNeutralSystemColor(App game, int systemID, bool saveMultiCombat = false)
		{
			StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(systemID);
			if (starSystemInfo == (StarSystemInfo)null)
				return;
			int num = 0;
			List<ColonyInfo> list = game.GameDatabase.GetColonyInfosForSystem(systemID).ToList<ColonyInfo>();
			if (list.Count > 0)
				num = list.First<ColonyInfo>().PlayerID;
			if (starSystemInfo.ControlZones == null)
				return;
			if (saveMultiCombat)
				game.Game.MCCarryOverData.AddCarryOverCombatZoneInfo(systemID, starSystemInfo.ControlZones);
			for (int index = 0; index < starSystemInfo.ControlZones.Count; ++index)
			{
				if (starSystemInfo.ControlZones[index] == 0)
					starSystemInfo.ControlZones[index] = num;
			}
			game.GameDatabase.UpdateSystemCombatZones(systemID, starSystemInfo.ControlZones);
		}

		public static void SaveCombatZonePlayerColor(
		  GameDatabase gamedb,
		  int systemID,
		  int playerID,
		  int index)
		{
			StarSystemInfo starSystemInfo = gamedb.GetStarSystemInfo(systemID);
			if (starSystemInfo.ControlZones == null)
				return;
			starSystemInfo.ControlZones[index] = playerID;
			gamedb.UpdateSystemCombatZones(systemID, starSystemInfo.ControlZones);
		}

		private void ObtainFurthestRing(App game)
		{
			this._furthestRing = 0;
			StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(this._system);
			List<OrbitalObjectInfo> list1 = game.GameDatabase.GetStarSystemOrbitalObjectInfos(this._system).ToList<OrbitalObjectInfo>();
			if (starSystemInfo != (StarSystemInfo)null && list1.Count != 0)
			{
				List<OrbitalObjectInfo> list2 = list1.ToList<OrbitalObjectInfo>();
				float num1 = 0.0f;
				foreach (OrbitalObjectInfo orbitalObjectInfo in list2)
				{
					Vector3 position = game.GameDatabase.GetOrbitalTransform(orbitalObjectInfo.ID).Position;
					PlanetInfo planetInfo = game.GameDatabase.GetPlanetInfo(orbitalObjectInfo.ID);
					float length = position.Length;
					if (planetInfo != null)
						length += StarSystemVars.Instance.SizeToRadius(planetInfo.Size);
					if ((double)length > (double)num1)
						num1 = length;
				}
				float num2 = num1 / 5700f - ((double)this.GetBaseOffset() > 0.0 ? this.GetBaseOffset() : 2f);
				this._furthestRing = 0;
				this._furthestRing = 1;
				while (this._furthestRing < ((IEnumerable<float>)StarSystem.CombatZoneMapRadii).Count<float>() && (double)num2 >= (double)StarSystem.CombatZoneMapRadii[this._furthestRing - 1])
					++this._furthestRing;
				this._furthestRing = Math.Min(this._furthestRing, StarSystem.CombatZoneMapRadii.Length - 1);
			}
			this._furthestRing = Math.Max(this._furthestRing, 3);
		}

		private IEnumerable<object> CombatZoneMapInitParams()
		{
			yield return (object)5700f;
			yield return (object)(float)((double)this.GetBaseOffset() > 0.0 ? (double)this.GetBaseOffset() : 2.0);
			if (this._furthestRing > 0)
			{
				yield return (object)(this._furthestRing + 1);
				for (int rad = 0; rad <= this._furthestRing; ++rad)
					yield return (object)StarSystem.CombatZoneMapRadii[rad];
				for (int div = 0; div <= this._furthestRing; ++div)
					yield return (object)StarSystem.CombatZoneMapAngleDivs[div];
			}
			else
				yield return (object)0;
		}

		public CombatZonePositionInfo ChangeCombatZoneOwner(
		  int ring,
		  int zone,
		  Player player)
		{
			CombatZonePositionInfo zonePositionInfo = this.CombatZones.FirstOrDefault<CombatZonePositionInfo>((Func<CombatZonePositionInfo, bool>)(x =>
		   {
			   if (x.RingIndex == ring)
				   return x.ZoneIndex == zone;
			   return false;
		   }));
			if (zonePositionInfo == null)
				return (CombatZonePositionInfo)null;
			zonePositionInfo.Player = player != null ? player.ID : 0;
			return zonePositionInfo;
		}

		public void SetAutoDrawEnabled(bool value)
		{
			this.PostSetProp("AutoDrawEnabled", value);
		}

		public void SetCamera(OrbitCameraController value)
		{
			this.PostSetProp("CameraController", value.GetObjectID());
		}

		public void SetInputEnabled(bool value)
		{
			this.PostSetProp("InputEnabled", value);
		}

		public StarSystem(
		  App game,
		  float scale,
		  int systemId,
		  Vector3 origin,
		  bool showOrbits,
		  CombatSensor forCombatSensor,
		  bool isInCombat,
		  int inputID,
		  bool showStationLabels = false,
		  bool autodrawenabled = true)
		{
			this._crits = new GameObjectSet(game);
			this._slots = new GameObjectSet(game);
			this._system = systemId;
			this._scale = scale;
			this._origin = origin;
			this._furthestRing = 0;
			List<IGameObject> gameObjectList = new List<IGameObject>();
			if (systemId != 0 && showOrbits)
			{
				this._orbitPainter = this._crits.Add<OrbitPainter>();
				gameObjectList.Add((IGameObject)this._orbitPainter);
				if (forCombatSensor != null)
					forCombatSensor.PostSetProp(nameof(OrbitPainter), this._orbitPainter.ObjectID);
			}
			StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(systemId);
			this._starRadius = 0.0f;
			if (starSystemInfo != (StarSystemInfo)null && systemId != 0)
				this._starRadius = StarHelper.CalcRadius(StellarClass.Parse(starSystemInfo.StellarClass).Size) / 5700f;
			if (game.GameDatabase.GetStarSystemInfo(this._system) != (StarSystemInfo)null)
				this.SystemOrigin = game.GameDatabase.GetStarSystemOrigin(this._system);
			this.ObtainFurthestRing(game);
			this.InitializeNeighboringSystems(game);
			this.InitializeSystemNodeLocations(game);
			this.InitializeApproachingFleets(game, isInCombat);
			List<object> objectList1 = new List<object>();
			objectList1.Add((object)this._orbitPainter.GetObjectID());
			objectList1.Add((object)this.NeighboringSystems.Count);
			foreach (NeighboringSystemInfo neighboringSystem in this.NeighboringSystems)
			{
				objectList1.Add((object)neighboringSystem.Name);
				objectList1.Add((object)neighboringSystem.Location);
			}
			objectList1.Add((object)this.VisibleNodePoints.Count);
			foreach (NodePoints visibleNodePoint in this.VisibleNodePoints)
			{
				objectList1.Add((object)visibleNodePoint.Location);
				objectList1.Add((object)visibleNodePoint.Effect);
			}
			objectList1.Add((object)this.ApproachingFleets.Count);
			foreach (ApproachingFleet approachingFleet in this.ApproachingFleets)
			{
				Player player = game.GetPlayer(approachingFleet.PlayerID);
				objectList1.Add((object)approachingFleet.Name);
				objectList1.Add((object)player.ObjectID);
				objectList1.Add((object)approachingFleet.Location);
			}
			objectList1.AddRange(this.CombatZoneMapInitParams());
			game.AddExistingObject((IGameObject)this, objectList1.ToArray());
			if (isInCombat)
				this.AddNodeMaws(game);
			this._isDeepSpace = starSystemInfo != (StarSystemInfo)null && starSystemInfo.IsDeepSpace;
			IEnumerable<OrbitalObjectInfo> orbitalObjectInfos = game.GameDatabase.GetStarSystemOrbitalObjectInfos(systemId);
			if (starSystemInfo == (StarSystemInfo)null || starSystemInfo.IsDeepSpace)
			{
				foreach (OrbitalObjectInfo orbitalObjectInfo in orbitalObjectInfos)
				{
					StationInfo stationInfo = game.GameDatabase.GetStationInfo(orbitalObjectInfo.ID);
					if (stationInfo != null)
					{
						foreach (Ship ship in this.AddStationToSystem(game, stationInfo, stationInfo.OrbitalObjectID, inputID))
						{
							gameObjectList.Add((IGameObject)ship);
							this._crits.Add((IGameObject)ship);
						}
					}
				}
				this.PostObjectAddObjects(gameObjectList.ToArray());
			}
			else
			{
				StarModel star = this.CreateStar(this._crits, this._origin, starSystemInfo, true);
				this.ObjectMap.Insert((IGameObject)star, StarSystemDetailsUI.StarItemID);
				gameObjectList.Add((IGameObject)star);
				this.CombatZones.Clear();
				if (this.App.GameDatabase.GetStarSystemInfo(systemId).ControlZones != null)
				{
					List<object> objectList2 = new List<object>();
					List<int> intList = this.App.Game.MCCarryOverData.GetPreviousControlZones(systemId);
					if (intList.Count != starSystemInfo.ControlZones.Count)
						intList = starSystemInfo.ControlZones;
					objectList2.Add((object)intList.Count);
					for (int index = 0; index < intList.Count; ++index)
					{
						Player player = this.App.GetPlayer(intList[index]);
						if (player != null)
							objectList2.Add((object)player.ObjectID);
						else
							objectList2.Add((object)0);
					}
					this.PostSetProp("SyncZoneMapInfo", objectList2.ToArray());
				}
				this.CombatZones = StarSystem.GetCombatZonesForSystem(game.Game, systemId, scale);
				foreach (PlanetInfo systemPlanetInfo in game.GameDatabase.GetStarSystemPlanetInfos(systemId))
				{
					PlanetInfo planetInfo = systemPlanetInfo;
					OrbitalObjectInfo orbitalObjectInfo = orbitalObjectInfos.First<OrbitalObjectInfo>((Func<OrbitalObjectInfo, bool>)(x => x.ID == planetInfo.ID));
					Matrix orbitalTransform = game.GameDatabase.GetOrbitalTransform(planetInfo.ID);
					StellarBody planet = this.CreatePlanet(this.App.Game, this._crits, this._origin, planetInfo, orbitalTransform, true);
					if (isInCombat)
					{
						ColonyInfo colonyInfoForPlanet = game.GameDatabase.GetColonyInfoForPlanet(planetInfo.ID);
						if (colonyInfoForPlanet != null)
						{
							planet.WeaponBanks = this.AddWeaponsToPlanet(game, colonyInfoForPlanet, planet);
							gameObjectList.AddRange((IEnumerable<IGameObject>)planet.WeaponBanks.ToArray());
							planet.PostSetProp("SetHardenedStructures", colonyInfoForPlanet.isHardenedStructures && !game.GameDatabase.GetStarSystemInfo(colonyInfoForPlanet.CachedStarSystemID).IsOpen);
						}
					}
					this.ObjectMap.Insert((IGameObject)planet, planetInfo.ID);
					this.PlanetMap.Insert((IGameObject)planet, planetInfo.ID);
					gameObjectList.Add((IGameObject)planet);
					Vector3 vector3_1 = orbitalTransform.Position * this._scale;
					Vector3 vector3_2 = (orbitalObjectInfo.ParentID.HasValue ? game.GameDatabase.GetOrbitalTransform(orbitalObjectInfo.ParentID.Value).Position : this._origin) * this._scale;
					float length = (orbitalTransform.Position * this._scale - vector3_2).Length;
					Matrix scale1 = Matrix.CreateScale(length, length, length);
					scale1.Position = vector3_2;
					if (showOrbits)
						this._orbitPainter.Add(scale1);
				}
				if (!isInCombat)
				{
					IEnumerable<FleetInfo> fleetInfoBySystemId = this.App.GameDatabase.GetFleetInfoBySystemID(systemId, FleetType.FL_GATE | FleetType.FL_ACCELERATOR);
					if (fleetInfoBySystemId != null)
					{
						foreach (FleetInfo fleetInfo in fleetInfoBySystemId)
						{
							foreach (ShipInfo shipInfo in this.App.GameDatabase.GetShipInfoByFleetID(fleetInfo.ID, false))
							{
								if (shipInfo.ShipSystemPosition.HasValue)
								{
									Matrix world = shipInfo.ShipSystemPosition.Value;
									Ship ship = Ship.CreateShip(game.Game, world, new ShipInfo()
									{
										SerialNumber = 1,
										ShipName = string.Empty,
										DesignID = shipInfo.DesignInfo.ID,
										DesignInfo = shipInfo.DesignInfo,
										ID = shipInfo.ID
									}, 0, inputID, 0, false, (IEnumerable<Player>)null);
									gameObjectList.Add((IGameObject)ship);
									this._crits.Add((IGameObject)ship);
								}
							}
						}
					}
				}
				foreach (OrbitalObjectInfo orbitalObjectInfo in orbitalObjectInfos)
				{
					StationInfo stationInfo = game.GameDatabase.GetStationInfo(orbitalObjectInfo.ID);
					if (stationInfo != null)
					{
						List<Ship> system = this.AddStationToSystem(game, stationInfo, stationInfo.OrbitalObjectID, inputID);
						foreach (Ship ship in system)
						{
							gameObjectList.Add((IGameObject)ship);
							this._crits.Add((IGameObject)ship);
						}
						Ship ship1 = system.FirstOrDefault<Ship>();
						if (ship1 != null)
						{
							this.StationInfoMap.Insert((IGameObject)ship1, stationInfo);
							this.ObjectMap.Insert((IGameObject)ship1, stationInfo.OrbitalObjectID);
							if (orbitalObjectInfo.ParentID.HasValue)
							{
								IGameObject state;
								if (this.ObjectMap.Reverse.TryGetValue(orbitalObjectInfo.ParentID.Value, out state))
									state.PostSetProp("AddStation", (IGameObject)ship1);
								else
									continue;
							}
						}
					}
					this.SetAutoDrawEnabled(autodrawenabled);
				}
				foreach (AsteroidBeltInfo asteroidBeltInfo in game.GameDatabase.GetStarSystemAsteroidBeltInfos(systemId).ToList<AsteroidBeltInfo>())
				{
					AsteroidBeltInfo belt = asteroidBeltInfo;
					OrbitalObjectInfo orbitalObjectInfo = orbitalObjectInfos.First<OrbitalObjectInfo>((Func<OrbitalObjectInfo, bool>)(x => x.ID == belt.ID));
					Matrix orbitalTransform = game.GameDatabase.GetOrbitalTransform(orbitalObjectInfo.ID);
					List<LargeAsteroidInfo> list = game.GameDatabase.GetLargeAsteroidsInAsteroidBelt(orbitalObjectInfo.ID).ToList<LargeAsteroidInfo>();
					int num1 = 0;
					foreach (LargeAsteroidInfo largeAsteroid1 in list)
					{
						float num2 = 6.283185f / (float)list.Count<LargeAsteroidInfo>() * (float)num1;
						++num1;
						float num3 = 6.283185f / (float)list.Count<LargeAsteroidInfo>() * (float)num1;
						Matrix world = orbitalTransform * Matrix.CreateRotationY(num2 + App.GetSafeRandom().NextSingle() % num3);
						LargeAsteroid largeAsteroid2 = this.CreateLargeAsteroid(game, origin, largeAsteroid1, world);
						this.ObjectMap.Insert((IGameObject)largeAsteroid2, largeAsteroid1.ID);
						gameObjectList.Add((IGameObject)largeAsteroid2);
						this._crits.Add((IGameObject)largeAsteroid2);
					}
					AsteroidBelt asteroidBelt = this.CreateAsteroidBelt(game, origin, belt, orbitalTransform);
					this.ObjectMap.Insert((IGameObject)asteroidBelt, belt.ID);
					gameObjectList.Add((IGameObject)asteroidBelt);
					this.AsteroidBelts.Add(asteroidBelt);
					Vector3 vector3_1 = orbitalTransform.Position * this._scale;
					Vector3 vector3_2 = (orbitalObjectInfo.ParentID.HasValue ? game.GameDatabase.GetOrbitalTransform(orbitalObjectInfo.ParentID.Value).Position : origin) * this._scale;
					float length = (orbitalTransform.Position * this._scale - vector3_2).Length;
					Matrix scale1 = Matrix.CreateScale(length, length, length);
					scale1.Position = vector3_2;
					if (showOrbits)
						this._orbitPainter.Add(scale1);
				}
				this.PostObjectAddObjects(gameObjectList.ToArray());
				this.InitializeSlots(systemId, isInCombat);
				if (!showStationLabels)
					return;
				List<Ship> list1 = this._crits.Objects.OfType<Ship>().ToList<Ship>();
				List<object> objectList3 = new List<object>();
				objectList3.Add((object)list1.Count);
				foreach (Ship ship in list1)
				{
					DesignInfo designInfo = game.GameDatabase.GetDesignInfo(ship.DesignID);
					objectList3.Add((object)designInfo.Name);
					objectList3.Add((object)ship.Position);
				}
				this.PostSetProp("SyncStationLabels", objectList3.ToArray());
			}
		}

		private List<Ship> AddStationToSystem(
		  App game,
		  StationInfo stationInfo,
		  int orbitalId,
		  int inputId)
		{
			List<Ship> shipList = new List<Ship>();
			if (stationInfo != null)
			{
				Matrix orbitalTransform = game.GameDatabase.GetOrbitalTransform(orbitalId);
				Ship ship1 = Ship.CreateShip(game.Game, orbitalTransform, new ShipInfo()
				{
					SerialNumber = 1,
					ShipName = string.Empty,
					DesignID = stationInfo.DesignInfo.ID,
					DesignInfo = stationInfo.DesignInfo,
					ID = stationInfo.ShipID
				}, 0, inputId, 0, false, (IEnumerable<Player>)null);
				shipList.Add(ship1);
				foreach (DesignSectionInfo designSection in stationInfo.DesignInfo.DesignSections)
				{
					foreach (WeaponBankInfo weaponBank in designSection.WeaponBanks)
					{
						if (weaponBank.DesignID.HasValue)
						{
							int? designId = weaponBank.DesignID;
							if ((designId.GetValueOrDefault() != 0 ? 1 : (!designId.HasValue ? 1 : 0)) != 0)
							{
								ShipSectionAsset shipSectionAsset = designSection.ShipSectionAsset;
								if (shipSectionAsset != null)
								{
									int num = 0;
									foreach (LogicalBank bank in shipSectionAsset.Banks)
									{
										if (weaponBank.BankGUID == bank.GUID)
										{
											foreach (LogicalMount mount in shipSectionAsset.Mounts)
											{
												if (mount.Bank == bank)
													++num;
											}
										}
									}
									for (int index = 0; index < num; ++index)
									{
										ShipInfo shipInfo = new ShipInfo()
										{
											SerialNumber = index,
											ShipName = string.Empty,
											DesignID = weaponBank.DesignID.Value
										};
										Ship ship2 = Ship.CreateShip(game.Game, orbitalTransform, shipInfo, ship1.ObjectID, ship1.InputID, ship1.Player.ObjectID, false, (IEnumerable<Player>)null);
										shipList.Add(ship2);
									}
								}
							}
						}
					}
				}
			}
			return shipList;
		}

		public void InitializeNeighboringSystems(App game)
		{
			this.NeighboringSystems.Clear();
			if (this._system == 0)
				return;
			int furthestRing = this._furthestRing > 0 ? Math.Min(this._furthestRing + 1, StarSystem.CombatZoneMapRadii.Length - 1) : StarSystem.CombatZoneMapRadii.Length - 1;
			float offsetDist = 2000f;
			foreach (StarSystemInfo ssi in game.GameDatabase.GetSystemsInRange(this.SystemOrigin, game.AssetDatabase.StarSystemEntryPointRange).ToList<StarSystemInfo>())
			{
				if (ssi.ID != this._system)
					this.AddNeighboringSystem(ssi, this.SystemOrigin, furthestRing, offsetDist);
			}
		}

		public void InitializeSystemNodeLocations(App game)
		{
			this.VisibleNodePoints.Clear();
			Player localPlayer = game.LocalPlayer;
			if (localPlayer == null || this._system == 0)
				return;
			bool flag1 = localPlayer.Faction.CanUseNodeLine(new bool?());
			bool flag2 = localPlayer.Faction.CanUseNodeLine(new bool?(true));
			bool flag3 = localPlayer.Faction.CanUseNodeLine(new bool?(false));
			if (!flag1)
			{
				List<PlayerTechInfo> list = game.GameDatabase.GetPlayerTechInfos(localPlayer.ID).ToList<PlayerTechInfo>().Where<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x =>
			   {
				   if (!(x.TechFileID == "CCC_Node_Tracking:_Human"))
					   return x.TechFileID == "CCC_Node_Tracking:_Zuul";
				   return true;
			   })).ToList<PlayerTechInfo>();
				if (list.Count > 0)
				{
					flag1 = list.Any<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.State == TechStates.Researched));
					flag2 = list.Any<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x =>
				   {
					   if (x.State == TechStates.Researched)
						   return x.TechFileID == "CCC_Node_Tracking:_Human";
					   return false;
				   }));
					flag3 = list.Any<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x =>
				   {
					   if (x.State == TechStates.Researched)
						   return x.TechFileID == "CCC_Node_Tracking:_Zuul";
					   return false;
				   }));
				}
			}
			Faction faction1 = game.AssetDatabase.Factions.FirstOrDefault<Faction>((Func<Faction, bool>)(x => x.Name == "human"));
			Faction faction2 = game.AssetDatabase.Factions.FirstOrDefault<Faction>((Func<Faction, bool>)(x => x.Name == "zuul"));
			float num1 = faction1 != null ? faction1.EntryPointOffset : 1000f;
			float num2 = faction2 != null ? faction2.EntryPointOffset : 1000f;
			int furthestRing = Math.Max(this._furthestRing - 1, 1);
			if (!flag1 || !flag2 && !flag3)
				return;
			List<NodeLineInfo> list1 = game.GameDatabase.GetNodeLines().Where<NodeLineInfo>((Func<NodeLineInfo, bool>)(x => !x.IsLoaLine)).ToList<NodeLineInfo>();
			list1.AddRange((IEnumerable<NodeLineInfo>)game.GameDatabase.GetNonPermenantNodeLines().Where<NodeLineInfo>((Func<NodeLineInfo, bool>)(x => !x.IsLoaLine)).ToList<NodeLineInfo>());
			foreach (NodeLineInfo nodeLineInfo in list1)
			{
				if (!nodeLineInfo.IsLoaLine && (!nodeLineInfo.IsPermenant || flag2) && ((nodeLineInfo.IsPermenant || flag3) && (nodeLineInfo.System1ID == this._system || nodeLineInfo.System2ID == this._system)))
				{
					int systemId = nodeLineInfo.System1ID != this._system ? nodeLineInfo.System1ID : nodeLineInfo.System2ID;
					StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(systemId);
					float offsetDist = nodeLineInfo.IsPermenant ? num1 : num2;
					this.AddNeighboringSystem(starSystemInfo, this.SystemOrigin, furthestRing, offsetDist);
					this.AddNodePointLocation(starSystemInfo, this.SystemOrigin, nodeLineInfo.IsPermenant, nodeLineInfo.ID, furthestRing, offsetDist);
				}
			}
		}

		public void InitializeApproachingFleets(App game, bool isIncombat)
		{
			this.ApproachingFleets.Clear();
			if (this._system == 0)
				return;
			int furthestRing = this._furthestRing > 0 ? Math.Min(this._furthestRing + 1, StarSystem.CombatZoneMapRadii.Length - 1) : StarSystem.CombatZoneMapRadii.Length - 1;
			float offsetDist = 1000f;
			foreach (MoveOrderInfo moveOrderInfo in game.GameDatabase.GetMoveOrderInfos().ToList<MoveOrderInfo>())
			{
				MoveOrderInfo mo = moveOrderInfo;
				if (mo.ToSystemID == this._system && !this.ApproachingFleets.Any<ApproachingFleet>((Func<ApproachingFleet, bool>)(x => x.FleetID == mo.FleetID)))
				{
					FleetInfo fleetInfo1 = game.GameDatabase.GetFleetInfo(mo.FleetID);
					if (fleetInfo1 != null && fleetInfo1.Type == FleetType.FL_NORMAL)
					{
						FleetLocation fleetLocation = game.GameDatabase.GetFleetLocation(mo.FleetID, false);
						if (StarMap.IsInRange(game.GameDatabase, game.LocalPlayer.ID, fleetLocation.Coords, 1f, (Dictionary<int, List<ShipInfo>>)null))
						{
							FleetInfo fleetInfo2 = game.GameDatabase.GetFleetInfo(mo.FleetID);
							if (fleetInfo2 != null && game.GameDatabase.GetDiplomacyStateBetweenPlayers(fleetInfo2.PlayerID, game.LocalPlayer.ID) == DiplomacyState.WAR)
								this.AddApproachingFleet(fleetInfo2, fleetLocation.Coords, this.SystemOrigin, furthestRing, offsetDist);
						}
					}
				}
			}
			if (isIncombat)
				return;
			foreach (MoveOrderInfo tempMoveOrder in game.GameDatabase.GetTempMoveOrders())
			{
				MoveOrderInfo mo = tempMoveOrder;
				if (mo.ToSystemID == this._system && !this.ApproachingFleets.Any<ApproachingFleet>((Func<ApproachingFleet, bool>)(x => x.FleetID == mo.FleetID)))
				{
					FleetLocation fleetLocation = game.GameDatabase.GetFleetLocation(mo.FleetID, false);
					if (StarMap.IsInRange(game.GameDatabase, game.LocalPlayer.ID, fleetLocation.Coords, 1f, (Dictionary<int, List<ShipInfo>>)null))
					{
						FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(mo.FleetID);
						if (fleetInfo != null && fleetInfo.PreviousSystemID.HasValue)
						{
							StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(fleetInfo.PreviousSystemID.Value);
							if (!(starSystemInfo == (StarSystemInfo)null) && game.GameDatabase.GetDiplomacyStateBetweenPlayers(fleetInfo.PlayerID, game.LocalPlayer.ID) == DiplomacyState.WAR)
								this.AddApproachingFleet(fleetInfo, starSystemInfo.Origin, this.SystemOrigin, furthestRing, offsetDist);
						}
					}
				}
			}
		}

		private void CorrectAllOverlaps()
		{
			float radians = MathHelper.DegreesToRadians(5f);
			foreach (NeighboringSystemInfo neighboringSystem1 in this.NeighboringSystems)
			{
				bool flag1 = true;
				bool flag2 = false;
				Matrix world = Matrix.CreateWorld(Vector3.Zero, neighboringSystem1.DirFromSystem, Vector3.UnitY);
				Vector3 v0 = neighboringSystem1.DirFromSystem;
				float yawRadians = 0.0f;
				while (flag1)
				{
					flag1 = false;
					v0 = (Matrix.CreateRotationYPR(yawRadians, 0.0f, 0.0f) * world).Forward;
					foreach (NeighboringSystemInfo neighboringSystem2 in this.NeighboringSystems)
					{
						if (neighboringSystem1.SystemID != neighboringSystem2.SystemID && (double)Vector3.Dot(v0, neighboringSystem2.DirFromSystem) > 0.990000009536743)
						{
							flag1 = true;
							flag2 = true;
							break;
						}
					}
					yawRadians += radians;
				}
				if (flag2)
				{
					neighboringSystem1.DirFromSystem = v0;
					neighboringSystem1.BaseOffsetLocation = v0 * neighboringSystem1.BaseOffsetLocation.Length;
					neighboringSystem1.Location = v0 * neighboringSystem1.Location.Length;
					foreach (NodePoints visibleNodePoint in this.VisibleNodePoints)
					{
						if (visibleNodePoint.SystemID == neighboringSystem1.SystemID)
							visibleNodePoint.Location = v0 * visibleNodePoint.Location.Length;
					}
				}
			}
			foreach (ApproachingFleet approachingFleet1 in this.ApproachingFleets)
			{
				bool flag1 = true;
				bool flag2 = false;
				Matrix world = Matrix.CreateWorld(Vector3.Zero, approachingFleet1.DirFromSystem, Vector3.UnitY);
				Vector3 v0 = approachingFleet1.DirFromSystem;
				float yawRadians = 0.0f;
				while (flag1)
				{
					flag1 = false;
					v0 = (Matrix.CreateRotationYPR(yawRadians, 0.0f, 0.0f) * world).Forward;
					foreach (ApproachingFleet approachingFleet2 in this.ApproachingFleets)
					{
						if (approachingFleet1.FleetID != approachingFleet2.FleetID && (double)Vector3.Dot(v0, approachingFleet2.DirFromSystem) > 0.990000009536743)
						{
							flag1 = true;
							flag2 = true;
							break;
						}
					}
					yawRadians += radians;
				}
				if (flag2)
				{
					approachingFleet1.DirFromSystem = v0;
					approachingFleet1.Location = v0 * approachingFleet1.Location.Length;
				}
			}
		}

		private void AddNodeMaws(App game)
		{
			Player localPlayer = game.LocalPlayer;
			if (localPlayer == null || localPlayer.Faction == null)
				return;
			PlayerTechInfo playerTechInfo = game.GameDatabase.GetPlayerTechInfos(localPlayer.ID).ToList<PlayerTechInfo>().FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == "DRV_Node_Maw"));
			if (playerTechInfo == null || playerTechInfo.State != TechStates.Researched)
				return;
			LogicalWeapon logicalWeapon = game.AssetDatabase.Weapons.FirstOrDefault<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.WeaponName == "NodeMaw"));
			if (logicalWeapon == null)
				return;
			List<NodeLineInfo> nodeLineInfoList = new List<NodeLineInfo>();
			if (localPlayer.Faction.CanUseNodeLine(new bool?(true)))
				nodeLineInfoList = game.GameDatabase.GetNodeLines().Where<NodeLineInfo>((Func<NodeLineInfo, bool>)(x =>
			   {
				   if (!x.IsLoaLine && (x.System1ID == this._system || x.System2ID == this._system))
					   return this.VisibleNodePoints.Any<NodePoints>((Func<NodePoints, bool>)(y => y.NodeID == x.ID));
				   return false;
			   })).ToList<NodeLineInfo>();
			else if (localPlayer.Faction.CanUseNodeLine(new bool?(false)))
				nodeLineInfoList = game.GameDatabase.GetNonPermenantNodeLines().Where<NodeLineInfo>((Func<NodeLineInfo, bool>)(x =>
			   {
				   if (!x.IsLoaLine && (x.System1ID == this._system || x.System2ID == this._system))
					   return this.VisibleNodePoints.Any<NodePoints>((Func<NodePoints, bool>)(y => y.NodeID == x.ID));
				   return false;
			   })).ToList<NodeLineInfo>();
			List<object> objectList = new List<object>();
			objectList.Add((object)nodeLineInfoList.Count);
			foreach (NodeLineInfo nodeLineInfo in nodeLineInfoList)
			{
				NodeLineInfo node = nodeLineInfo;
				logicalWeapon.AddGameObjectReference();
				NodePoints nodePoints = this.VisibleNodePoints.First<NodePoints>((Func<NodePoints, bool>)(x => x.NodeID == node.ID));
				objectList.Add((object)logicalWeapon.GameObject.ObjectID);
				objectList.Add((object)nodePoints.Location);
			}
			this.PostSetProp("SetNodeMawLocationsCombat", objectList.ToArray());
		}

		public List<Vector3> GetNodeMawLocationsForPlayer(App game, int playerID)
		{
			List<Vector3> vector3List = new List<Vector3>();
			Player player = game.GetPlayer(playerID);
			if (player == null || player.Faction == null)
				return vector3List;
			PlayerTechInfo playerTechInfo = game.GameDatabase.GetPlayerTechInfos(playerID).ToList<PlayerTechInfo>().FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == "DRV_Node_Maw"));
			if (playerTechInfo == null || playerTechInfo.State != TechStates.Researched)
				return vector3List;
			float entryPointOffset = player.Faction.EntryPointOffset;
			int index = Math.Max(this._furthestRing - 1, 1);
			List<NodeLineInfo> nodeLineInfoList = new List<NodeLineInfo>();
			if (player.Faction.CanUseNodeLine(new bool?(true)))
				nodeLineInfoList = game.GameDatabase.GetNodeLines().Where<NodeLineInfo>((Func<NodeLineInfo, bool>)(x =>
			   {
				   if (x.IsLoaLine)
					   return false;
				   if (x.System1ID != this._system)
					   return x.System2ID == this._system;
				   return true;
			   })).ToList<NodeLineInfo>();
			else if (player.Faction.CanUseNodeLine(new bool?(false)))
				nodeLineInfoList = game.GameDatabase.GetNonPermenantNodeLines().Where<NodeLineInfo>((Func<NodeLineInfo, bool>)(x =>
			   {
				   if (x.IsLoaLine)
					   return false;
				   if (x.System1ID != this._system)
					   return x.System2ID == this._system;
				   return true;
			   })).ToList<NodeLineInfo>();
			foreach (NodeLineInfo nodeLineInfo in nodeLineInfoList)
			{
				Vector3 vector3 = game.GameDatabase.GetStarSystemInfo(nodeLineInfo.System1ID != this._system ? nodeLineInfo.System1ID : nodeLineInfo.System2ID).Origin - this.SystemOrigin;
				vector3.Y = 0.0f;
				double num = (double)vector3.Normalize();
				vector3List.Add(vector3 * ((float)(((double)this.GetBaseOffset() + (double)StarSystem.CombatZoneMapRadii[index]) * 5700.0) + entryPointOffset));
			}
			return vector3List;
		}

		private void AddNeighboringSystem(
		  StarSystemInfo ssi,
		  Vector3 systemOrigin,
		  int furthestRing,
		  float offsetDist)
		{
			if (ssi == (StarSystemInfo)null)
				return;
			Vector3 vector3 = ssi.Origin - systemOrigin;
			vector3.Y = 0.0f;
			double num = (double)vector3.Normalize();
			NeighboringSystemInfo neighboringSystemInfo = this.NeighboringSystems.FirstOrDefault<NeighboringSystemInfo>((Func<NeighboringSystemInfo, bool>)(x => x.SystemID == ssi.ID));
			if (neighboringSystemInfo != null)
				neighboringSystemInfo.Location = vector3 * ((float)(((double)this.GetBaseOffset() + (double)StarSystem.CombatZoneMapRadii[furthestRing]) * 5700.0) + offsetDist);
			else
				this.NeighboringSystems.Add(new NeighboringSystemInfo()
				{
					Name = ssi.Name,
					SystemID = ssi.ID,
					DirFromSystem = vector3,
					Location = vector3 * ((float)(((double)this.GetBaseOffset() + (double)StarSystem.CombatZoneMapRadii[furthestRing]) * 5700.0) + offsetDist),
					BaseOffsetLocation = vector3 * (float)(((double)this.GetBaseOffset() + (double)StarSystem.CombatZoneMapRadii[this.GetFurthestRing()]) * 5700.0)
				});
		}

		private void AddNodePointLocation(
		  StarSystemInfo ssi,
		  Vector3 systemOrigin,
		  bool isPermanent,
		  int nodeID,
		  int furthestRing,
		  float offsetDist)
		{
			if (ssi == (StarSystemInfo)null)
				return;
			NodePoints nodePoints = new NodePoints();
			nodePoints.NodeID = nodeID;
			nodePoints.SystemID = ssi.ID;
			nodePoints.Effect = !isPermanent ? "effects\\NodePoint_Zuul.effect" : "effects\\NodePoint_Human.effect";
			Vector3 vector3 = ssi.Origin - systemOrigin;
			vector3.Y = 0.0f;
			double num = (double)vector3.Normalize();
			nodePoints.Location = vector3 * ((float)(((double)this.GetBaseOffset() + (double)StarSystem.CombatZoneMapRadii[furthestRing]) * 5700.0) + offsetDist);
			this.VisibleNodePoints.Add(nodePoints);
		}

		private void AddApproachingFleet(
		  FleetInfo fi,
		  Vector3 fleetPos,
		  Vector3 systemOrigin,
		  int furthestRing,
		  float offsetDist)
		{
			if (fi == null)
				return;
			ApproachingFleet approachingFleet = new ApproachingFleet();
			Vector3 vector3 = fleetPos - systemOrigin;
			vector3.Y = 0.0f;
			double num = (double)vector3.Normalize();
			approachingFleet.Name = fi.Name;
			approachingFleet.FleetID = fi.ID;
			approachingFleet.PlayerID = fi.PlayerID;
			approachingFleet.DirFromSystem = vector3;
			approachingFleet.Location = vector3 * ((float)(((double)this.GetBaseOffset() + (double)StarSystem.CombatZoneMapRadii[furthestRing]) * 5700.0) + offsetDist);
			this.ApproachingFleets.Add(approachingFleet);
		}

		public Vector3 GetClosestPermanentNodeToPosition(App game, Vector3 fleetPos)
		{
			Vector3 vector3_1 = fleetPos;
			float num1 = float.MaxValue;
			Faction faction = game.AssetDatabase.Factions.FirstOrDefault<Faction>((Func<Faction, bool>)(x => x.Name == "human"));
			float num2 = faction != null ? faction.EntryPointOffset : 1000f;
			int index = Math.Max(this._furthestRing - 1, 1);
			foreach (NodeLineInfo nodeLineInfo in game.GameDatabase.GetNodeLines().Where<NodeLineInfo>((Func<NodeLineInfo, bool>)(x => !x.IsLoaLine)).ToList<NodeLineInfo>())
			{
				if (nodeLineInfo.System1ID == this._system || nodeLineInfo.System2ID == this._system)
				{
					int systemId = nodeLineInfo.System1ID != this._system ? nodeLineInfo.System1ID : nodeLineInfo.System2ID;
					Vector3 vector3_2 = game.GameDatabase.GetStarSystemInfo(systemId).Origin - this.SystemOrigin;
					vector3_2.Y = 0.0f;
					double num3 = (double)vector3_2.Normalize();
					Vector3 vector3_3 = vector3_2 * ((float)(((double)this.GetBaseOffset() + (double)StarSystem.CombatZoneMapRadii[index]) * 5700.0) + num2);
					float lengthSquared = (vector3_3 - fleetPos).LengthSquared;
					if ((double)lengthSquared < (double)num1)
					{
						num1 = lengthSquared;
						vector3_1 = vector3_3;
					}
				}
			}
			return vector3_1;
		}

		public Vector3 GetClosestTempNodeToPosition(App game, Vector3 fleetPos)
		{
			Vector3 vector3_1 = fleetPos;
			float num1 = float.MaxValue;
			Faction faction = game.AssetDatabase.Factions.FirstOrDefault<Faction>((Func<Faction, bool>)(x => x.Name == "zuul"));
			float num2 = faction != null ? faction.EntryPointOffset : 1000f;
			int index = Math.Max(this._furthestRing - 1, 1);
			foreach (NodeLineInfo nodeLineInfo in game.GameDatabase.GetNonPermenantNodeLines().Where<NodeLineInfo>((Func<NodeLineInfo, bool>)(x => !x.IsLoaLine)).ToList<NodeLineInfo>())
			{
				if (nodeLineInfo.System1ID == this._system || nodeLineInfo.System2ID == this._system)
				{
					int systemId = nodeLineInfo.System1ID != this._system ? nodeLineInfo.System1ID : nodeLineInfo.System2ID;
					Vector3 vector3_2 = game.GameDatabase.GetStarSystemInfo(systemId).Origin - this.SystemOrigin;
					vector3_2.Y = 0.0f;
					double num3 = (double)vector3_2.Normalize();
					Vector3 vector3_3 = vector3_2 * ((float)(((double)this.GetBaseOffset() + (double)StarSystem.CombatZoneMapRadii[index]) * 5700.0) + num2);
					float lengthSquared = (vector3_3 - fleetPos).LengthSquared;
					if ((double)lengthSquared < (double)num1)
					{
						num1 = lengthSquared;
						vector3_1 = vector3_3;
					}
				}
			}
			return vector3_1;
		}

		public static int GetFurthestRing(GameSession game, int systemID)
		{
			int val1_1 = 0;
			StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(systemID);
			List<OrbitalObjectInfo> list1 = game.GameDatabase.GetStarSystemOrbitalObjectInfos(systemID).ToList<OrbitalObjectInfo>();
			if (starSystemInfo != (StarSystemInfo)null && list1.Count != 0)
			{
				float num1 = StarHelper.CalcRadius(StellarClass.Parse(starSystemInfo.StellarClass).Size) / 5700f + StarSystem.BaseOffset;
				List<OrbitalObjectInfo> list2 = list1.ToList<OrbitalObjectInfo>();
				float num2 = 0.0f;
				foreach (OrbitalObjectInfo orbitalObjectInfo in list2)
				{
					Vector3 position = game.GameDatabase.GetOrbitalTransform(orbitalObjectInfo.ID).Position;
					PlanetInfo planetInfo = game.GameDatabase.GetPlanetInfo(orbitalObjectInfo.ID);
					float length = position.Length;
					if (planetInfo != null)
						length += StarSystemVars.Instance.SizeToRadius(planetInfo.Size);
					if ((double)length > (double)num2)
						num2 = length;
				}
				float num3 = num2 / 5700f - ((double)num1 > 0.0 ? num1 : 2f);
				int val1_2 = 1;
				while (val1_2 < ((IEnumerable<float>)StarSystem.CombatZoneMapRadii).Count<float>() && (double)num3 >= (double)StarSystem.CombatZoneMapRadii[val1_2 - 1])
					++val1_2;
				val1_1 = Math.Min(val1_2, StarSystem.CombatZoneMapRadii.Length - 1);
			}
			return Math.Max(val1_1, 3);
		}

		public static List<CombatZonePositionInfo> GetCombatZonesForSystem(
		  GameSession game,
		  int systemID,
		  float scale)
		{
			List<CombatZonePositionInfo> zonePositionInfoList = new List<CombatZonePositionInfo>();
			if (systemID == 0)
				return zonePositionInfoList;
			StarSystemInfo starSystemInfo1 = game.GameDatabase.GetStarSystemInfo(systemID);
			if (starSystemInfo1 == (StarSystemInfo)null)
				return zonePositionInfoList;
			float num1 = StarHelper.CalcRadius(StellarClass.Parse(starSystemInfo1.StellarClass).Size) / 5700f;
			float num2 = 0.0f;
			foreach (OrbitalObjectInfo orbitalObjectInfo in game.GameDatabase.GetStarSystemOrbitalObjectInfos(systemID).ToList<OrbitalObjectInfo>())
			{
				Vector3 position = game.GameDatabase.GetOrbitalTransform(orbitalObjectInfo.ID).Position;
				PlanetInfo planetInfo = game.GameDatabase.GetPlanetInfo(orbitalObjectInfo.ID);
				float length = position.Length;
				if (planetInfo != null)
					length += StarSystemVars.Instance.SizeToRadius(planetInfo.Size);
				if ((double)length > (double)num2)
					num2 = length;
			}
			StarSystemInfo starSystemInfo2 = game.GameDatabase.GetStarSystemInfo(systemID);
			int furthestRing = StarSystem.GetFurthestRing(game, systemID);
			for (int ring = 0; ring < ((IEnumerable<float>)StarSystem.CombatZoneMapRadii).Count<float>() - 1 && ring != furthestRing; ++ring)
			{
				for (int zone = 0; zone < StarSystem.CombatZoneMapAngleDivs[ring]; ++zone)
				{
					float num3 = 6.283185f / (float)StarSystem.CombatZoneMapAngleDivs[ring];
					float num4 = (float)zone * num3;
					float num5 = num4 + num3;
					float num6 = (float)(((double)num1 + (double)StarSystem.BaseOffset + (double)StarSystem.CombatZoneMapRadii[ring]) * 5700.0);
					float num7 = (float)(((double)num1 + (double)StarSystem.BaseOffset + (double)StarSystem.CombatZoneMapRadii[ring + 1]) * 5700.0);
					float num8 = (float)(((double)num4 + (double)num5) * 0.5);
					Vector3 vector3 = new Vector3((float)Math.Cos((double)num8), 0.0f, (float)Math.Sin((double)num8)) * (float)(((double)num7 + (double)num6) * 0.5);
					CombatZonePositionInfo zonePositionInfo = new CombatZonePositionInfo();
					int combatZoneIndex = StarSystem.GetCombatZoneIndex(ring, zone);
					zonePositionInfo.Player = !(starSystemInfo2 != (StarSystemInfo)null) || starSystemInfo2.ControlZones == null || combatZoneIndex >= starSystemInfo2.ControlZones.Count ? 0 : starSystemInfo2.ControlZones[StarSystem.GetCombatZoneIndex(ring, zone)];
					zonePositionInfo.RingIndex = ring;
					zonePositionInfo.ZoneIndex = zone;
					zonePositionInfo.Center = vector3;
					zonePositionInfo.AngleLeft = num4;
					zonePositionInfo.AngleRight = num5;
					zonePositionInfo.RadiusLower = num6;
					zonePositionInfo.RadiusUpper = num7;
					zonePositionInfoList.Add(zonePositionInfo);
				}
			}
			return zonePositionInfoList;
		}

		private LogicalWeapon GetBestPlanetBeamWeapon(App game, PlayerInfo planetOwner)
		{
			LogicalWeapon logicalWeapon1 = (LogicalWeapon)null;
			List<string> stringList = new List<string>()
	  {
		"Bem_Hvy_hclas",
		"Bem_Hvy_Lancer",
		"Bem_Hvy_Cutting"
	  };
			List<LogicalWeapon> list = game.GameDatabase.GetAvailableWeapons(game.AssetDatabase, planetOwner.ID).ToList<LogicalWeapon>();
			foreach (string str in stringList)
			{
				string w = str;
				LogicalWeapon logicalWeapon2 = list.FirstOrDefault<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => string.Equals(x.WeaponName, w, StringComparison.InvariantCultureIgnoreCase)));
				if (logicalWeapon2 != null)
					logicalWeapon1 = logicalWeapon2;
			}
			return logicalWeapon1;
		}

		private LogicalWeapon GetBestPlanetMissileWeapon(App game, PlayerInfo planetOwner)
		{
			return game.AssetDatabase.Weapons.First<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => string.Equals(x.WeaponName, "Mis_IOBM", StringComparison.InvariantCultureIgnoreCase)));
		}

		private LogicalWeapon GetBestPlanetHeavyMissileWeapon(
		  App game,
		  PlayerInfo planetOwner)
		{
			return game.AssetDatabase.Weapons.FirstOrDefault<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => string.Equals(x.WeaponName, "Mis_HeavyIOBM", StringComparison.InvariantCultureIgnoreCase)));
		}

		private LogicalWeapon GetBestMirvPlanetMissileWeapon(
		  App game,
		  PlayerInfo planetOwner)
		{
			return game.AssetDatabase.Weapons.First<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => string.Equals(x.WeaponName, "Mis_Mirv_IOBM", StringComparison.InvariantCultureIgnoreCase)));
		}

		private List<PlanetWeaponBank> AddWeaponsToPlanet(
		  App game,
		  ColonyInfo colony,
		  StellarBody planet)
		{
			PlayerInfo playerInfo = game.GameDatabase.GetPlayerInfo(colony.PlayerID);
			List<PlanetWeaponBank> planetWeaponBankList = new List<PlanetWeaponBank>();
			if (!playerInfo.isStandardPlayer)
				return planetWeaponBankList;
			Faction faction = game.AssetDatabase.GetFaction(game.GameDatabase.GetFactionName(playerInfo.FactionID));
			LogicalWeapon weapon = (LogicalWeapon)null;
			if (game.GetStratModifier<bool>(StratModifiers.AllowPlanetBeam, playerInfo.ID))
			{
				weapon = this.GetBestPlanetBeamWeapon(game, playerInfo);
				if (weapon != null)
				{
					weapon.AddGameObjectReference();
					LogicalWeapon logicalWeapon = game.AssetDatabase.Weapons.FirstOrDefault<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => string.Equals(x.WeaponName, weapon.SubWeapon, StringComparison.InvariantCultureIgnoreCase)));
					logicalWeapon?.AddGameObjectReference();
					int numLaunchers = (int)Math.Ceiling(game.GameDatabase.GetTotalPopulation(colony) / (double)game.AssetDatabase.PopulationPerPlanetBeam);
					if (numLaunchers > 0)
					{
						int weaponLevelFromTechs = Player.GetWeaponLevelFromTechs(weapon, this.App.GameDatabase.GetPlayerTechInfos(playerInfo.ID).ToList<PlayerTechInfo>());
						WeaponModelPaths weaponModelPaths1 = LogicalWeapon.GetWeaponModelPaths(weapon, faction);
						WeaponModelPaths weaponModelPaths2 = LogicalWeapon.GetWeaponModelPaths(logicalWeapon, faction);
						PlanetWeaponBank planetWeaponBank = new PlanetWeaponBank(game, (IGameObject)planet, (LogicalBank)null, (Module)null, weapon, weaponLevelFromTechs, logicalWeapon, weapon.DefaultWeaponClass, weaponModelPaths1.ModelPath, weaponModelPaths2.ModelPath, game.AssetDatabase.PlanetBeamDelay, numLaunchers);
						planetWeaponBank.AddExistingObject(game);
						planetWeaponBankList.Add(planetWeaponBank);
					}
				}
				else
					App.Log.Warn("[[NON CRASHING BUG]] - Planet is allowed to create beams, but no available beam weapon was found, planet recieves no beam weapon", nameof(game));
			}
			if (game.GetStratModifier<bool>(StratModifiers.AllowMirvPlanetaryMissiles, playerInfo.ID))
			{
				weapon = this.GetBestMirvPlanetMissileWeapon(game, playerInfo);
				if (weapon != null)
				{
					weapon.AddGameObjectReference();
					LogicalWeapon logicalWeapon = game.AssetDatabase.Weapons.FirstOrDefault<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => string.Equals(x.WeaponName, weapon.SubWeapon, StringComparison.InvariantCultureIgnoreCase)));
					string empty = string.Empty;
					logicalWeapon?.AddGameObjectReference();
					int numLaunchers = (int)Math.Ceiling(game.GameDatabase.GetTotalPopulation(colony) / (double)game.AssetDatabase.PopulationPerPlanetMirv);
					if (numLaunchers > 0)
					{
						int weaponLevelFromTechs = Player.GetWeaponLevelFromTechs(weapon, this.App.GameDatabase.GetPlayerTechInfos(playerInfo.ID).ToList<PlayerTechInfo>());
						WeaponModelPaths weaponModelPaths1 = LogicalWeapon.GetWeaponModelPaths(weapon, faction);
						WeaponModelPaths weaponModelPaths2 = LogicalWeapon.GetWeaponModelPaths(logicalWeapon, faction);
						PlanetWeaponBank planetWeaponBank = new PlanetWeaponBank(game, (IGameObject)planet, (LogicalBank)null, (Module)null, weapon, weaponLevelFromTechs, logicalWeapon, weapon.DefaultWeaponClass, weaponModelPaths1.ModelPath, weaponModelPaths2.ModelPath, game.AssetDatabase.PlanetMissileDelay, numLaunchers);
						planetWeaponBank.AddExistingObject(game);
						planetWeaponBankList.Add(planetWeaponBank);
					}
				}
				else
					App.Log.Warn("[[NON CRASHING BUG]] - Planet is allowed to create MIRVs, but no available MIRV weapon was found, planet recieves no MIRV weapon", nameof(game));
			}
			LogicalWeapon missileWeapon = this.GetBestPlanetMissileWeapon(game, playerInfo);
			if (missileWeapon != null)
				missileWeapon.AddGameObjectReference();
			LogicalWeapon logicalWeapon1 = (LogicalWeapon)null;
			if (!string.IsNullOrEmpty(missileWeapon.SubWeapon))
			{
				logicalWeapon1 = game.AssetDatabase.Weapons.First<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => string.Equals(x.WeaponName, missileWeapon.SubWeapon, StringComparison.InvariantCultureIgnoreCase)));
				logicalWeapon1?.AddGameObjectReference();
			}
			int numLaunchers1 = (int)Math.Ceiling(game.GameDatabase.GetTotalPopulation(colony) / (double)game.AssetDatabase.PopulationPerPlanetMissile);
			if (numLaunchers1 > 0)
			{
				int weaponLevelFromTechs = Player.GetWeaponLevelFromTechs(missileWeapon, this.App.GameDatabase.GetPlayerTechInfos(playerInfo.ID).ToList<PlayerTechInfo>());
				WeaponModelPaths weaponModelPaths1 = LogicalWeapon.GetWeaponModelPaths(missileWeapon, faction);
				WeaponModelPaths weaponModelPaths2 = LogicalWeapon.GetWeaponModelPaths(logicalWeapon1, faction);
				PlanetWeaponBank planetWeaponBank = new PlanetWeaponBank(game, (IGameObject)planet, (LogicalBank)null, (Module)null, missileWeapon, weaponLevelFromTechs, logicalWeapon1, missileWeapon.DefaultWeaponClass, weaponModelPaths1.ModelPath, weaponModelPaths2.ModelPath, game.AssetDatabase.PlanetMissileDelay, numLaunchers1);
				planetWeaponBank.AddExistingObject(game);
				planetWeaponBankList.Add(planetWeaponBank);
			}
			PlayerTechInfo playerTechInfo = game.GameDatabase.GetPlayerTechInfos(colony.PlayerID).FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == "WAR_Heavy_Planet_Missiles"));
			if (playerTechInfo != null && playerTechInfo.State == TechStates.Researched)
			{
				int numLaunchers2 = (int)Math.Ceiling(game.GameDatabase.GetTotalPopulation(colony) / (double)game.AssetDatabase.PopulationPerHeavyPlanetMissile);
				if (numLaunchers2 > 0)
				{
					missileWeapon = this.GetBestPlanetHeavyMissileWeapon(game, playerInfo);
					if (missileWeapon != null)
					{
						missileWeapon.AddGameObjectReference();
						LogicalWeapon logicalWeapon2 = (LogicalWeapon)null;
						if (!string.IsNullOrEmpty(missileWeapon.SubWeapon))
						{
							logicalWeapon2 = game.AssetDatabase.Weapons.First<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => string.Equals(x.WeaponName, missileWeapon.SubWeapon, StringComparison.InvariantCultureIgnoreCase)));
							logicalWeapon2?.AddGameObjectReference();
						}
						if (numLaunchers2 > 0)
						{
							int weaponLevelFromTechs = Player.GetWeaponLevelFromTechs(missileWeapon, this.App.GameDatabase.GetPlayerTechInfos(playerInfo.ID).ToList<PlayerTechInfo>());
							WeaponModelPaths weaponModelPaths1 = LogicalWeapon.GetWeaponModelPaths(missileWeapon, faction);
							WeaponModelPaths weaponModelPaths2 = LogicalWeapon.GetWeaponModelPaths(logicalWeapon2, faction);
							PlanetWeaponBank planetWeaponBank = new PlanetWeaponBank(game, (IGameObject)planet, (LogicalBank)null, (Module)null, missileWeapon, weaponLevelFromTechs, logicalWeapon2, missileWeapon.DefaultWeaponClass, weaponModelPaths1.ModelPath, weaponModelPaths2.ModelPath, game.AssetDatabase.PlanetMissileDelay, numLaunchers2);
							planetWeaponBank.AddExistingObject(game);
							planetWeaponBankList.Add(planetWeaponBank);
						}
					}
				}
			}
			return planetWeaponBankList;
		}

		private void InitializeSlots(int systemID, bool isInCombat)
		{
			this.App.GameDatabase.GetStarSystemOrbitalObjectInfos(systemID);
			foreach (IGameObject crit1 in this._crits)
			{
				StellarBody planet = crit1 as StellarBody;
				if (planet != null)
				{
					List<SlotData> slotsForPlanet1 = this.GetSlotsForPlanet(planet, false);
					float num = 360f / (float)slotsForPlanet1.Count;
					for (int index = 0; index < slotsForPlanet1.Count; ++index)
					{
						Matrix orbitalTransform = this.App.GameDatabase.GetOrbitalTransform(planet.PlanetInfo.ID);
						float radius = StarSystemVars.Instance.SizeToRadius(planet.PlanetInfo.Size);
						Matrix translation = Matrix.CreateTranslation(orbitalTransform.Position);
						Matrix matrix = Matrix.CreateTranslation(radius + (float)StarSystemVars.Instance.StationOrbitDistance, 0.0f, 0.0f) * Matrix.CreateRotationY((float)(((double)num * (double)index + 45.0) * 0.0174444448202848)) * translation;
						slotsForPlanet1[index].Position = matrix.Position;
						slotsForPlanet1[index].Rotation = matrix.EulerAngles.X;
						if (!isInCombat)
						{
							ColonyInfo colonyInfoForPlanet = this.App.GameDatabase.GetColonyInfoForPlanet(planet.Parameters.OrbitalID);
							if (planet.Parameters.ColonyPlayerID == this.App.LocalPlayer.ID || colonyInfoForPlanet != null && colonyInfoForPlanet.IsIndependentColony(this.App) || (planet.Parameters.BodyType == "gaseous" || planet.Parameters.BodyType == "barren" || this.App.GameDatabase.GetStratModifier<bool>(StratModifiers.AllowDeepSpaceConstruction, this.App.LocalPlayer.ID) && colonyInfoForPlanet == null))
							{
								StarSystemPlacementSlot systemPlacementSlot = new StarSystemPlacementSlot(this.App, slotsForPlanet1[index]);
								this._slots.Add((IGameObject)systemPlacementSlot);
								this.PostObjectAddObjects((IGameObject)systemPlacementSlot);
								systemPlacementSlot.SetTransform(matrix.Position, matrix.EulerAngles.X);
							}
						}
					}
					foreach (IGameObject crit2 in this._crits)
					{
						Ship ship = crit2 as Ship;
						if (ship != null && this.StationInfoMap.Forward.Keys.Contains<IGameObject>(crit2))
						{
							StationInfo stationInfo = this.StationInfoMap.Forward[crit2];
							OrbitalObjectInfo orbitalObjectInfo = this.App.GameDatabase.GetOrbitalObjectInfo(stationInfo.OrbitalObjectID);
							int? parentId = orbitalObjectInfo.ParentID;
							int id = planet.PlanetInfo.ID;
							if ((parentId.GetValueOrDefault() != id ? 0 : (parentId.HasValue ? 1 : 0)) != 0)
							{
								bool flag = false;
								StationInfo stinf = this.App.GameDatabase.GetStationInfo(orbitalObjectInfo.ID);
								foreach (SlotData slotData in slotsForPlanet1)
								{
									if ((slotData.SupportedTypes & (StationTypeFlags)(1 << (int)(stinf.DesignInfo.StationType & (StationType)31))) > (StationTypeFlags)0 && slotData.OccupantID == 0)
									{
										if (!isInCombat)
										{
											StarSystemPlacementSlot state = this._slots.OfType<StarSystemPlacementSlot>().FirstOrDefault<StarSystemPlacementSlot>((Func<StarSystemPlacementSlot, bool>)(x =>
										   {
											   if (x._slotData.Parent == planet.ObjectID && (x._slotData.SupportedTypes & (StationTypeFlags)(1 << (int)(stinf.DesignInfo.StationType & (StationType)31))) > (StationTypeFlags)0)
												   return x._slotData.OccupantID == 0;
											   return false;
										   }));
											if (state != null)
											{
												state.SetOccupant((IGameObject)ship);
												state.PostSetProp("StationType", (object)stationInfo.DesignInfo.StationType.ToFlags(), (object)(this.App.GameDatabase.GetFactionName(this.App.GameDatabase.GetPlayerFactionID(stationInfo.PlayerID)) == "zuul"));
											}
										}
										slotData.OccupantID = ship.ObjectID;
										ship.InitialSetPos(slotData.Position, Vector3.Zero);
										flag = true;
										break;
									}
								}
								if (!flag)
								{
									List<SlotData> slotsForPlanet2 = this.GetSlotsForPlanet(planet, true);
									for (int index = 0; index < slotsForPlanet2.Count; ++index)
									{
										if ((slotsForPlanet2[index].SupportedTypes & (StationTypeFlags)(1 << (int)(stinf.DesignInfo.StationType & (StationType)31))) > (StationTypeFlags)0 && slotsForPlanet1[index].OccupantID == 0)
										{
											slotsForPlanet1[index].OccupantID = ship.ObjectID;
											ship.InitialSetPos(slotsForPlanet1[index].Position, Vector3.Zero);
											break;
										}
									}
								}
							}
						}
					}
				}
				else
				{
					LargeAsteroid asteroid = crit1 as LargeAsteroid;
					if (asteroid != null)
					{
						this.App.GameDatabase.GetOrbitalObjectInfo(asteroid.ID);
						List<SlotData> slotsForAsteroid = this.GetSlotsForAsteroid(asteroid);
						List<StarSystemPlacementSlot> source = new List<StarSystemPlacementSlot>();
						float num1 = 1000f;
						float num2 = 360f / (float)slotsForAsteroid.Count;
						for (int index = 0; index < slotsForAsteroid.Count; ++index)
						{
							Matrix translation = Matrix.CreateTranslation(asteroid.WorldTransform.Position);
							Matrix matrix = Matrix.CreateTranslation(num1 + (float)StarSystemVars.Instance.StationOrbitDistance, 0.0f, 0.0f) * Matrix.CreateRotationY((float)(((double)num2 * (double)index + 45.0) * 0.0174444448202848)) * translation;
							slotsForAsteroid[index].Position = matrix.Position;
							slotsForAsteroid[index].Rotation = matrix.EulerAngles.X;
							if (!isInCombat)
							{
								StarSystemPlacementSlot state = new StarSystemPlacementSlot(this.App, slotsForAsteroid[index]);
								state.PostSetProp("SetPlacementEnabled", true);
								state.SetTransform(matrix.Position, matrix.EulerAngles.X);
								source.Add(state);
								this._slots.Add((IGameObject)state);
								this.PostObjectAddObjects((IGameObject)state);
							}
						}
						foreach (IGameObject crit2 in this._crits)
						{
							Ship ship = crit2 as Ship;
							if (ship != null && this.StationInfoMap.Forward.Keys.Contains<IGameObject>(crit2))
							{
								StationInfo stationInfo = this.StationInfoMap.Forward[crit2];
								OrbitalObjectInfo orbitalObjectInfo = this.App.GameDatabase.GetOrbitalObjectInfo(stationInfo.OrbitalObjectID);
								int? parentId = orbitalObjectInfo.ParentID;
								int id = asteroid.ID;
								if ((parentId.GetValueOrDefault() != id ? 0 : (parentId.HasValue ? 1 : 0)) != 0)
								{
									StationInfo stinf = this.App.GameDatabase.GetStationInfo(orbitalObjectInfo.ID);
									foreach (SlotData slotData in slotsForAsteroid)
									{
										if ((slotData.SupportedTypes & (StationTypeFlags)(1 << (int)(stinf.DesignInfo.StationType & (StationType)31))) > (StationTypeFlags)0 && slotData.OccupantID == 0)
										{
											if (!isInCombat)
											{
												StarSystemPlacementSlot state = source.FirstOrDefault<StarSystemPlacementSlot>((Func<StarSystemPlacementSlot, bool>)(x =>
											   {
												   if (x._slotData.Parent == asteroid.ObjectID && (x._slotData.SupportedTypes & (StationTypeFlags)(1 << (int)(stinf.DesignInfo.StationType & (StationType)31))) > (StationTypeFlags)0)
													   return x._slotData.OccupantID == 0;
												   return false;
											   }));
												if (state != null)
												{
													state.SetOccupant((IGameObject)ship);
													state.PostSetProp("StationType", (object)stationInfo.DesignInfo.StationType.ToFlags(), (object)(this.App.GameDatabase.GetFactionName(this.App.GameDatabase.GetPlayerFactionID(stationInfo.PlayerID)) == "zuul"));
												}
											}
											slotData.OccupantID = ship.ObjectID;
											ship.InitialSetPos(slotData.Position, Vector3.Zero);
											break;
										}
									}
								}
							}
						}
					}
				}
			}
		}

		public List<StellarBody> GetPlanetsInSystem()
		{
			List<StellarBody> stellarBodyList = new List<StellarBody>();
			foreach (IGameObject crit in this._crits)
			{
				if (crit is StellarBody)
					stellarBodyList.Add(crit as StellarBody);
			}
			return stellarBodyList;
		}

		public List<Ship> GetStationsAroundPlanet(int planetID)
		{
			List<Ship> shipList = new List<Ship>();
			foreach (IGameObject crit1 in this._crits)
			{
				StellarBody stellarBody = crit1 as StellarBody;
				if (stellarBody != null && stellarBody.PlanetInfo.ID == planetID)
				{
					foreach (IGameObject crit2 in this._crits)
					{
						Ship ship = crit2 as Ship;
						if (ship != null)
						{
							int? parentId = this.App.GameDatabase.GetOrbitalObjectInfo(this.StationInfoMap.Forward[crit2].OrbitalObjectID).ParentID;
							int id = stellarBody.PlanetInfo.ID;
							if ((parentId.GetValueOrDefault() != id ? 0 : (parentId.HasValue ? 1 : 0)) != 0)
								shipList.Add(ship);
						}
					}
				}
			}
			return shipList;
		}

		public StationInfo GetStationInfo(Ship station)
		{
			StationInfo stationInfo;
			this.StationInfoMap.Forward.TryGetValue((IGameObject)station, out stationInfo);
			return stationInfo;
		}

		protected int GetNumSupportedSlots(StellarBody body)
		{
			if (body.Parameters.BodyType == "gaseous")
				return 2;
			return body.Parameters.BodyType == "barren" ? 1 : 4;
		}

		public static bool IsColonizablePlanetType(string type)
		{
			if (!(type == "normal") && !(type == "pastoral") && (!(type == "volcanic") && !(type == "cavernous")) && (!(type == "tempestuous") && !(type == "magnar")))
				return type == "primordial";
			return true;
		}

		public static int? GetSuitablePlanetForStation(GameSession game, int playerID, int systemID, StationType stationtype)
		{
			int? result = null;
			if (stationtype != StationType.MINING)
			{
				if (stationtype == StationType.SCIENCE)
				{
					List<ColonyInfo> list = (from x in game.GameDatabase.GetColonyInfosForSystem(systemID)
											 where x.IsIndependentColony(game.App)
											 select x).ToList<ColonyInfo>();
					foreach (ColonyInfo colonyInfo in list)
					{
						if (StarSystem.GetAvailSlotTypesForPlanet(game, systemID, colonyInfo.OrbitalObjectID, playerID).Contains(stationtype))
						{
							result = new int?(colonyInfo.OrbitalObjectID);
							break;
						}
					}
				}
				List<ColonyInfo> Colonies = (from x in game.GameDatabase.GetColonyInfosForSystem(systemID)
											 where x.PlayerID == playerID
											 select x).ToList<ColonyInfo>();
				if (result == null)
				{
					foreach (ColonyInfo colonyInfo2 in Colonies)
					{
						if (StarSystem.GetAvailSlotTypesForPlanet(game, systemID, colonyInfo2.OrbitalObjectID, playerID).Contains(stationtype))
						{
							result = new int?(colonyInfo2.OrbitalObjectID);
							break;
						}
					}
				}
				if (result != null)
				{
					return result;
				}
				List<PlanetInfo> list2 = (from x in game.GameDatabase.GetPlanetInfosOrbitingStar(systemID)
										  where !Colonies.Any((ColonyInfo j) => j.OrbitalObjectID == x.ID)
										  select x).ToList<PlanetInfo>();
				using (List<PlanetInfo>.Enumerator enumerator3 = list2.GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						PlanetInfo planetInfo = enumerator3.Current;
						if (StarSystem.GetAvailSlotTypesForPlanet(game, systemID, planetInfo.ID, playerID).Contains(stationtype))
						{
							result = new int?(planetInfo.ID);
							break;
						}
					}
					return result;
				}
			}
			IList<AsteroidBeltInfo> list3 = game.GameDatabase.GetStarSystemAsteroidBeltInfos(systemID).ToList<AsteroidBeltInfo>();
			foreach (AsteroidBeltInfo asteroidBeltInfo in list3)
			{
				List<LargeAsteroidInfo> list4 = game.GameDatabase.GetLargeAsteroidsInAsteroidBelt(asteroidBeltInfo.ID).ToList<LargeAsteroidInfo>();
				foreach (LargeAsteroidInfo largeAsteroidInfo in list4)
				{
					List<StationType> availSlotTypesForAsteroid = StarSystem.GetAvailSlotTypesForAsteroid(game, systemID, largeAsteroidInfo.ID, playerID);
					if (availSlotTypesForAsteroid.Contains(StationType.MINING))
					{
						result = new int?(largeAsteroidInfo.ID);
						break;
					}
				}
			}
			if (result == null)
			{
				List<PlanetInfo> list5 = game.GameDatabase.GetStarSystemPlanetInfos(systemID).ToList<PlanetInfo>();
				foreach (PlanetInfo planetInfo2 in list5)
				{
					if (StarSystem.GetAvailSlotTypesForPlanet(game, systemID, planetInfo2.ID, playerID).Contains(stationtype))
					{
						result = new int?(planetInfo2.ID);
						break;
					}
				}
			}
			return result;
		}
		private static List<StationType> GetAvailSlotTypesForAsteroid(
	  GameSession game,
	  int systemID,
	  int asteroidid,
	  int playerId)
		{
			Dictionary<StationType, int> dictionary1 = new Dictionary<StationType, int>();
			List<StationType> stationTypeList = new List<StationType>();
			dictionary1.Add(StationType.CIVILIAN, 0);
			stationTypeList.Add(StationType.CIVILIAN);
			dictionary1.Add(StationType.DEFENCE, 0);
			stationTypeList.Add(StationType.DEFENCE);
			dictionary1.Add(StationType.DIPLOMATIC, 0);
			stationTypeList.Add(StationType.DIPLOMATIC);
			dictionary1.Add(StationType.GATE, 0);
			stationTypeList.Add(StationType.GATE);
			dictionary1.Add(StationType.MINING, 0);
			stationTypeList.Add(StationType.MINING);
			dictionary1.Add(StationType.NAVAL, 0);
			stationTypeList.Add(StationType.NAVAL);
			dictionary1.Add(StationType.SCIENCE, 0);
			stationTypeList.Add(StationType.SCIENCE);
			LargeAsteroidInfo asteroid = game.GameDatabase.GetLargeAsteroidInfo(asteroidid);
			if (asteroid != null)
			{
				Dictionary<StationType, int> dictionary2;
				(dictionary2 = dictionary1)[StationType.DEFENCE] = dictionary2[StationType.DEFENCE] + 2;
				Dictionary<StationType, int> dictionary3;
				(dictionary3 = dictionary1)[StationType.SCIENCE] = dictionary3[StationType.SCIENCE] + 1;
				Dictionary<StationType, int> dictionary4;
				(dictionary4 = dictionary1)[StationType.MINING] = dictionary4[StationType.MINING] + 1;
			}
			foreach (StationInfo stationInfo in game.GameDatabase.GetStationForSystem(systemID).Where<StationInfo>((Func<StationInfo, bool>)(x =>
		   {
			   int? parentId = game.GameDatabase.GetOrbitalObjectInfo(x.OrbitalObjectID).ParentID;
			   int id = asteroid.ID;
			   if (parentId.GetValueOrDefault() == id)
				   return parentId.HasValue;
			   return false;
		   })).ToList<StationInfo>())
			{
				int? parentId = game.GameDatabase.GetOrbitalObjectInfo(stationInfo.OrbitalObjectID).ParentID;
				if (parentId.HasValue)
				{
					int id = asteroid.ID;
					int? nullable = parentId;
					if ((id != nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) != 0)
					{
						if (stationInfo.DesignInfo.StationType == StationType.SCIENCE)
						{
							Dictionary<StationType, int> dictionary2;
							(dictionary2 = dictionary1)[StationType.SCIENCE] = dictionary2[StationType.SCIENCE] - 1;
							break;
						}
						if (stationInfo.DesignInfo.StationType == StationType.MINING)
						{
							Dictionary<StationType, int> dictionary2;
							(dictionary2 = dictionary1)[StationType.MINING] = dictionary2[StationType.MINING] - 1;
						}
					}
				}
			}
			foreach (KeyValuePair<StationType, int> keyValuePair in dictionary1)
			{
				if (keyValuePair.Value <= 0)
					stationTypeList.Remove(keyValuePair.Key);
			}
			if (!game.GetPlayerObject(playerId).Faction.CanUseGate() && !game.GameDatabase.PlayerHasTech(playerId, "DRV_Casting"))
				stationTypeList.Remove(StationType.GATE);
			if (!game.GameDatabase.PlayerHasTech(playerId, "POL_Xeno-Colloquy") && (game.GetPlayerObject(playerId).Faction.Name != "zuul" || !game.GameDatabase.PlayerHasTech(playerId, "POL_Tribute_Systems")))
				stationTypeList.Remove(StationType.DIPLOMATIC);
			if (!game.GameDatabase.PlayerHasTech(playerId, "POL_FTL_Economics") && game.GetPlayerObject(playerId).Faction.Name != "zuul" && !game.GameDatabase.GetStratModifier<bool>(StratModifiers.EnableTrade, playerId))
				stationTypeList.Remove(StationType.CIVILIAN);
			if (!game.GameDatabase.PlayerHasTech(playerId, "BRD_Stealthed_Structures"))
				stationTypeList.Remove(StationType.DEFENCE);
			if (!game.GameDatabase.PlayerHasTech(playerId, "IND_Mega-Strip_Mining"))
				stationTypeList.Remove(StationType.MINING);
			return stationTypeList;
		}

		public static List<StationType> GetAvailSlotTypesForPlanet(
		  GameSession game,
		  int systemID,
		  int planetID,
		  int playerId)
		{
			Dictionary<StationType, int> dictionary1 = new Dictionary<StationType, int>();
			List<StationType> stationTypeList = new List<StationType>();
			dictionary1.Add(StationType.CIVILIAN, 0);
			stationTypeList.Add(StationType.CIVILIAN);
			dictionary1.Add(StationType.DEFENCE, 0);
			stationTypeList.Add(StationType.DEFENCE);
			dictionary1.Add(StationType.DIPLOMATIC, 0);
			stationTypeList.Add(StationType.DIPLOMATIC);
			dictionary1.Add(StationType.GATE, 0);
			stationTypeList.Add(StationType.GATE);
			dictionary1.Add(StationType.MINING, 0);
			stationTypeList.Add(StationType.MINING);
			dictionary1.Add(StationType.NAVAL, 0);
			stationTypeList.Add(StationType.NAVAL);
			dictionary1.Add(StationType.SCIENCE, 0);
			stationTypeList.Add(StationType.SCIENCE);
			PlanetInfo planetInfo = game.GameDatabase.GetPlanetInfo(planetID);
			ColonyInfo colonyInfoForPlanet = game.GameDatabase.GetColonyInfoForPlanet(planetID);
			List<StationInfo> list = game.GameDatabase.GetStationForSystem(systemID).Where<StationInfo>((Func<StationInfo, bool>)(x =>
		   {
			   int? parentId = game.GameDatabase.GetOrbitalObjectInfo(x.OrbitalObjectID).ParentID;
			   int num = planetID;
			   if (parentId.GetValueOrDefault() == num)
				   return parentId.HasValue;
			   return false;
		   })).ToList<StationInfo>();
			if (planetInfo.Type == "barren")
			{
				Dictionary<StationType, int> dictionary2;
				(dictionary2 = dictionary1)[StationType.DEFENCE] = dictionary2[StationType.DEFENCE] + 1;
				Dictionary<StationType, int> dictionary3;
				(dictionary3 = dictionary1)[StationType.SCIENCE] = dictionary3[StationType.SCIENCE] + 1;
				Dictionary<StationType, int> dictionary4;
				(dictionary4 = dictionary1)[StationType.NAVAL] = dictionary4[StationType.NAVAL] + 1;
				Dictionary<StationType, int> dictionary5;
				(dictionary5 = dictionary1)[StationType.MINING] = dictionary5[StationType.MINING] + 2;
			}
			else if (StarSystem.IsColonizablePlanetType(planetInfo.Type))
			{
				if (colonyInfoForPlanet != null && (colonyInfoForPlanet.PlayerID == playerId || colonyInfoForPlanet.IsIndependentColony(game)) || game.GameDatabase.GetStratModifier<bool>(StratModifiers.AllowDeepSpaceConstruction, playerId) && colonyInfoForPlanet == null)
				{
					Dictionary<StationType, int> dictionary2;
					(dictionary2 = dictionary1)[StationType.SCIENCE] = dictionary2[StationType.SCIENCE] + 4;
					Dictionary<StationType, int> dictionary3;
					(dictionary3 = dictionary1)[StationType.NAVAL] = dictionary3[StationType.NAVAL] + 4;
					Dictionary<StationType, int> dictionary4;
					(dictionary4 = dictionary1)[StationType.GATE] = dictionary4[StationType.GATE] + 4;
				}
			}
			else if (planetInfo.Type == "gaseous")
			{
				Dictionary<StationType, int> dictionary2;
				(dictionary2 = dictionary1)[StationType.SCIENCE] = dictionary2[StationType.SCIENCE] + 2;
				Dictionary<StationType, int> dictionary3;
				(dictionary3 = dictionary1)[StationType.NAVAL] = dictionary3[StationType.NAVAL] + 2;
				Dictionary<StationType, int> dictionary4;
				(dictionary4 = dictionary1)[StationType.GATE] = dictionary4[StationType.GATE] + 2;
			}
			if (colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == playerId)
			{
				Dictionary<StationType, int> dictionary2;
				(dictionary2 = dictionary1)[StationType.DIPLOMATIC] = dictionary2[StationType.DIPLOMATIC] + 4;
				Dictionary<StationType, int> dictionary3;
				(dictionary3 = dictionary1)[StationType.CIVILIAN] = dictionary3[StationType.CIVILIAN] + 4;
				Dictionary<StationType, int> dictionary4;
				(dictionary4 = dictionary1)[StationType.DEFENCE] = dictionary4[StationType.DEFENCE] + 4;
			}
			foreach (StationInfo stationInfo in list)
			{
				int? parentId = game.GameDatabase.GetOrbitalObjectInfo(stationInfo.OrbitalObjectID).ParentID;
				if (parentId.HasValue)
				{
					int? nullable1 = parentId;
					int num = planetID;
					if ((nullable1.GetValueOrDefault() != num ? 1 : (!nullable1.HasValue ? 1 : 0)) == 0)
					{
						if (stationInfo.DesignInfo.StationType == StationType.DEFENCE)
						{
							Dictionary<StationType, int> dictionary2;
							(dictionary2 = dictionary1)[StationType.DEFENCE] = dictionary2[StationType.DEFENCE] - 1;
						}
						else if (stationInfo.DesignInfo.StationType == StationType.MINING)
						{
							Dictionary<StationType, int> dictionary2;
							(dictionary2 = dictionary1)[StationType.MINING] = dictionary2[StationType.MINING] - 1;
						}
						else if (stationInfo.DesignInfo.StationType == StationType.CIVILIAN || stationInfo.DesignInfo.StationType == StationType.DIPLOMATIC)
						{
							Dictionary<StationType, int> dictionary2;
							(dictionary2 = dictionary1)[StationType.CIVILIAN] = dictionary2[StationType.CIVILIAN] - 1;
							Dictionary<StationType, int> dictionary3;
							(dictionary3 = dictionary1)[StationType.DIPLOMATIC] = dictionary3[StationType.DIPLOMATIC] - 1;
							Dictionary<StationType, int> dictionary4;
							(dictionary4 = dictionary1)[StationType.NAVAL] = dictionary4[StationType.NAVAL] - 1;
							Dictionary<StationType, int> dictionary5;
							(dictionary5 = dictionary1)[StationType.SCIENCE] = dictionary5[StationType.SCIENCE] - 1;
							Dictionary<StationType, int> dictionary6;
							(dictionary6 = dictionary1)[StationType.GATE] = dictionary6[StationType.GATE] - 1;
						}
						else if (stationInfo.DesignInfo.StationType == StationType.SCIENCE || stationInfo.DesignInfo.StationType == StationType.NAVAL || stationInfo.DesignInfo.StationType == StationType.GATE)
						{
							bool flag = false;
							if (parentId.HasValue)
							{
								if (colonyInfoForPlanet != null)
								{
									int orbitalObjectId = colonyInfoForPlanet.OrbitalObjectID;
									int? nullable2 = parentId;
									if ((orbitalObjectId != nullable2.GetValueOrDefault() ? 0 : (nullable2.HasValue ? 1 : 0)) != 0)
									{
										Dictionary<StationType, int> dictionary2;
										(dictionary2 = dictionary1)[StationType.DIPLOMATIC] = dictionary2[StationType.DIPLOMATIC] - 1;
										Dictionary<StationType, int> dictionary3;
										(dictionary3 = dictionary1)[StationType.CIVILIAN] = dictionary3[StationType.CIVILIAN] - 1;
										Dictionary<StationType, int> dictionary4;
										(dictionary4 = dictionary1)[StationType.NAVAL] = dictionary4[StationType.NAVAL] - 1;
										Dictionary<StationType, int> dictionary5;
										(dictionary5 = dictionary1)[StationType.SCIENCE] = dictionary5[StationType.SCIENCE] - 1;
										Dictionary<StationType, int> dictionary6;
										(dictionary6 = dictionary1)[StationType.GATE] = dictionary6[StationType.GATE] - 1;
										break;
									}
								}
								if (!flag)
								{
									Dictionary<StationType, int> dictionary2;
									(dictionary2 = dictionary1)[StationType.SCIENCE] = dictionary2[StationType.SCIENCE] - 1;
									Dictionary<StationType, int> dictionary3;
									(dictionary3 = dictionary1)[StationType.NAVAL] = dictionary3[StationType.NAVAL] - 1;
									if (game.GameDatabase.GetPlanetInfo(parentId.Value).Type != "barren")
									{
										Dictionary<StationType, int> dictionary4;
										(dictionary4 = dictionary1)[StationType.GATE] = dictionary4[StationType.GATE] - 1;
									}
								}
							}
						}
					}
				}
			}
			foreach (StationType type in new List<StationType>((IEnumerable<StationType>)stationTypeList))
			{
				StationInfo systemPlayerAndType = game.GameDatabase.GetStationForSystemPlayerAndType(systemID, playerId, type);
				if (systemPlayerAndType != null && systemPlayerAndType.DesignInfo.StationType != StationType.DEFENCE && systemPlayerAndType.DesignInfo.StationType != StationType.MINING)
					dictionary1[type] = 0;
			}
			foreach (KeyValuePair<StationType, int> keyValuePair in dictionary1)
			{
				if (keyValuePair.Value <= 0)
					stationTypeList.Remove(keyValuePair.Key);
			}
			if (!game.GetPlayerObject(playerId).Faction.CanUseGate())
				stationTypeList.Remove(StationType.GATE);
			if (!game.GameDatabase.PlayerHasTech(playerId, "POL_Xeno-Colloquy") && (game.GetPlayerObject(playerId).Faction.Name != "zuul" || !game.GameDatabase.PlayerHasTech(playerId, "POL_Tribute_Systems")))
				stationTypeList.Remove(StationType.DIPLOMATIC);
			if (!game.GameDatabase.PlayerHasTech(playerId, "POL_FTL_Economics") && game.GetPlayerObject(playerId).Faction.Name != "zuul" && !game.GameDatabase.GetStratModifier<bool>(StratModifiers.EnableTrade, playerId))
				stationTypeList.Remove(StationType.CIVILIAN);
			if (!game.GameDatabase.PlayerHasTech(playerId, "BRD_Stealthed_Structures"))
				stationTypeList.Remove(StationType.DEFENCE);
			if (!game.GameDatabase.PlayerHasTech(playerId, "IND_Mega-Strip_Mining"))
				stationTypeList.Remove(StationType.MINING);
			return stationTypeList;
		}

		public static List<StationType> GetSystemCanSupportStations(
		  GameSession game,
		  int systemID,
		  int playerId)
		{
			Dictionary<StationType, int> dictionary1 = new Dictionary<StationType, int>();
			List<StationType> stationTypeList = new List<StationType>();
			dictionary1.Add(StationType.CIVILIAN, 0);
			stationTypeList.Add(StationType.CIVILIAN);
			dictionary1.Add(StationType.DEFENCE, 0);
			stationTypeList.Add(StationType.DEFENCE);
			dictionary1.Add(StationType.DIPLOMATIC, 0);
			stationTypeList.Add(StationType.DIPLOMATIC);
			dictionary1.Add(StationType.GATE, 0);
			stationTypeList.Add(StationType.GATE);
			dictionary1.Add(StationType.MINING, 0);
			stationTypeList.Add(StationType.MINING);
			dictionary1.Add(StationType.NAVAL, 0);
			stationTypeList.Add(StationType.NAVAL);
			dictionary1.Add(StationType.SCIENCE, 0);
			stationTypeList.Add(StationType.SCIENCE);
			IList<PlanetInfo> list1 = (IList<PlanetInfo>)((IEnumerable<PlanetInfo>)game.GameDatabase.GetStarSystemPlanetInfos(systemID)).ToList<PlanetInfo>();
			IList<AsteroidBeltInfo> list2 = (IList<AsteroidBeltInfo>)game.GameDatabase.GetStarSystemAsteroidBeltInfos(systemID).ToList<AsteroidBeltInfo>();
			List<StationInfo> list3 = game.GameDatabase.GetStationForSystem(systemID).ToList<StationInfo>();
			List<ColonyInfo> list4 = game.GameDatabase.GetColonyInfosForSystem(systemID).ToList<ColonyInfo>();
			List<MissionInfo> list5 = game.GameDatabase.GetMissionsBySystemDest(systemID).Where<MissionInfo>((Func<MissionInfo, bool>)(x =>
		   {
			   if (game.GameDatabase.GetFleetInfo(x.FleetID).PlayerID == playerId)
				   return x.Type == MissionType.CONSTRUCT_STN;
			   return false;
		   })).ToList<MissionInfo>();
			foreach (PlanetInfo planetInfo in (IEnumerable<PlanetInfo>)list1)
			{
				ColonyInfo colonyInfoForPlanet = game.GameDatabase.GetColonyInfoForPlanet(planetInfo.ID);
				if (planetInfo.Type == "barren")
				{
					Dictionary<StationType, int> dictionary2;
					(dictionary2 = dictionary1)[StationType.DEFENCE] = dictionary2[StationType.DEFENCE] + 1;
					Dictionary<StationType, int> dictionary3;
					(dictionary3 = dictionary1)[StationType.SCIENCE] = dictionary3[StationType.SCIENCE] + 1;
					Dictionary<StationType, int> dictionary4;
					(dictionary4 = dictionary1)[StationType.NAVAL] = dictionary4[StationType.NAVAL] + 1;
					Dictionary<StationType, int> dictionary5;
					(dictionary5 = dictionary1)[StationType.MINING] = dictionary5[StationType.MINING] + 2;
				}
				else if (StarSystem.IsColonizablePlanetType(planetInfo.Type))
				{
					if (colonyInfoForPlanet != null && (colonyInfoForPlanet.PlayerID == playerId || colonyInfoForPlanet.IsIndependentColony(game)) || game.GameDatabase.GetStratModifier<bool>(StratModifiers.AllowDeepSpaceConstruction, playerId) && colonyInfoForPlanet == null)
					{
						Dictionary<StationType, int> dictionary2;
						(dictionary2 = dictionary1)[StationType.SCIENCE] = dictionary2[StationType.SCIENCE] + 4;
						Dictionary<StationType, int> dictionary3;
						(dictionary3 = dictionary1)[StationType.NAVAL] = dictionary3[StationType.NAVAL] + 4;
						Dictionary<StationType, int> dictionary4;
						(dictionary4 = dictionary1)[StationType.GATE] = dictionary4[StationType.GATE] + 4;
					}
				}
				else if (planetInfo.Type == "gaseous")
				{
					Dictionary<StationType, int> dictionary2;
					(dictionary2 = dictionary1)[StationType.SCIENCE] = dictionary2[StationType.SCIENCE] + 2;
					Dictionary<StationType, int> dictionary3;
					(dictionary3 = dictionary1)[StationType.NAVAL] = dictionary3[StationType.NAVAL] + 2;
					Dictionary<StationType, int> dictionary4;
					(dictionary4 = dictionary1)[StationType.GATE] = dictionary4[StationType.GATE] + 2;
				}
				if (colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == playerId)
				{
					Dictionary<StationType, int> dictionary2;
					(dictionary2 = dictionary1)[StationType.DIPLOMATIC] = dictionary2[StationType.DIPLOMATIC] + 4;
					Dictionary<StationType, int> dictionary3;
					(dictionary3 = dictionary1)[StationType.CIVILIAN] = dictionary3[StationType.CIVILIAN] + 4;
					Dictionary<StationType, int> dictionary4;
					(dictionary4 = dictionary1)[StationType.DEFENCE] = dictionary4[StationType.DEFENCE] + 4;
				}
			}
			foreach (AsteroidBeltInfo asteroidBeltInfo in (IEnumerable<AsteroidBeltInfo>)list2)
			{
				Dictionary<StationType, int> dictionary2;
				(dictionary2 = dictionary1)[StationType.DEFENCE] = dictionary2[StationType.DEFENCE] + game.GameDatabase.GetLargeAsteroidsInAsteroidBelt(asteroidBeltInfo.ID).Count<LargeAsteroidInfo>() * 2;
				Dictionary<StationType, int> dictionary3;
				(dictionary3 = dictionary1)[StationType.SCIENCE] = dictionary3[StationType.SCIENCE] + game.GameDatabase.GetLargeAsteroidsInAsteroidBelt(asteroidBeltInfo.ID).Count<LargeAsteroidInfo>();
				Dictionary<StationType, int> dictionary4;
				(dictionary4 = dictionary1)[StationType.MINING] = dictionary4[StationType.MINING] + game.GameDatabase.GetLargeAsteroidsInAsteroidBelt(asteroidBeltInfo.ID).Count<LargeAsteroidInfo>();
			}
			foreach (StationInfo stationInfo in list3)
			{
				if (stationInfo.DesignInfo.StationType == StationType.DEFENCE)
				{
					Dictionary<StationType, int> dictionary2;
					(dictionary2 = dictionary1)[StationType.DEFENCE] = dictionary2[StationType.DEFENCE] - 1;
				}
				else if (stationInfo.DesignInfo.StationType == StationType.MINING)
				{
					Dictionary<StationType, int> dictionary2;
					(dictionary2 = dictionary1)[StationType.MINING] = dictionary2[StationType.MINING] - 1;
				}
				else if (stationInfo.DesignInfo.StationType == StationType.CIVILIAN || stationInfo.DesignInfo.StationType == StationType.DIPLOMATIC)
				{
					Dictionary<StationType, int> dictionary2;
					(dictionary2 = dictionary1)[StationType.CIVILIAN] = dictionary2[StationType.CIVILIAN] - 1;
					Dictionary<StationType, int> dictionary3;
					(dictionary3 = dictionary1)[StationType.DIPLOMATIC] = dictionary3[StationType.DIPLOMATIC] - 1;
					Dictionary<StationType, int> dictionary4;
					(dictionary4 = dictionary1)[StationType.NAVAL] = dictionary4[StationType.NAVAL] - 1;
					Dictionary<StationType, int> dictionary5;
					(dictionary5 = dictionary1)[StationType.SCIENCE] = dictionary5[StationType.SCIENCE] - 1;
					Dictionary<StationType, int> dictionary6;
					(dictionary6 = dictionary1)[StationType.GATE] = dictionary6[StationType.GATE] - 1;
				}
				else if (stationInfo.DesignInfo.StationType == StationType.GATE && stationInfo.PlayerID == playerId)
					dictionary1[StationType.GATE] = 0;
				else if (stationInfo.DesignInfo.StationType == StationType.SCIENCE || stationInfo.DesignInfo.StationType == StationType.NAVAL || stationInfo.DesignInfo.StationType == StationType.GATE)
				{
					bool flag = false;
					int? parentId = game.GameDatabase.GetOrbitalObjectInfo(stationInfo.OrbitalObjectID).ParentID;
					if (parentId.HasValue)
					{
						foreach (ColonyInfo colonyInfo in list4)
						{
							int orbitalObjectId = colonyInfo.OrbitalObjectID;
							int? nullable = parentId;
							if ((orbitalObjectId != nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) != 0)
							{
								Dictionary<StationType, int> dictionary2;
								(dictionary2 = dictionary1)[StationType.DIPLOMATIC] = dictionary2[StationType.DIPLOMATIC] - 1;
								Dictionary<StationType, int> dictionary3;
								(dictionary3 = dictionary1)[StationType.CIVILIAN] = dictionary3[StationType.CIVILIAN] - 1;
								Dictionary<StationType, int> dictionary4;
								(dictionary4 = dictionary1)[StationType.NAVAL] = dictionary4[StationType.NAVAL] - 1;
								Dictionary<StationType, int> dictionary5;
								(dictionary5 = dictionary1)[StationType.SCIENCE] = dictionary5[StationType.SCIENCE] - 1;
								Dictionary<StationType, int> dictionary6;
								(dictionary6 = dictionary1)[StationType.GATE] = dictionary6[StationType.GATE] - 1;
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							foreach (AsteroidBeltInfo asteroidBeltInfo in (IEnumerable<AsteroidBeltInfo>)list2)
							{
								foreach (LargeAsteroidInfo largeAsteroidInfo in game.GameDatabase.GetLargeAsteroidsInAsteroidBelt(asteroidBeltInfo.ID))
								{
									int id = largeAsteroidInfo.ID;
									int? nullable = parentId;
									if ((id != nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) != 0 && stationInfo.DesignInfo.StationType == StationType.SCIENCE)
									{
										Dictionary<StationType, int> dictionary2;
										(dictionary2 = dictionary1)[StationType.SCIENCE] = dictionary2[StationType.SCIENCE] - 1;
										flag = true;
										break;
									}
								}
							}
							if (!flag)
							{
								Dictionary<StationType, int> dictionary2;
								(dictionary2 = dictionary1)[StationType.SCIENCE] = dictionary2[StationType.SCIENCE] - 1;
								Dictionary<StationType, int> dictionary3;
								(dictionary3 = dictionary1)[StationType.NAVAL] = dictionary3[StationType.NAVAL] - 1;
								if (game.GameDatabase.GetPlanetInfo(parentId.Value) != null && game.GameDatabase.GetPlanetInfo(parentId.Value).Type != "barren")
								{
									Dictionary<StationType, int> dictionary4;
									(dictionary4 = dictionary1)[StationType.GATE] = dictionary4[StationType.GATE] - 1;
								}
							}
						}
					}
				}
			}
			foreach (MissionInfo missionInfo in list5)
			{
				if (missionInfo.StationType.HasValue)
				{
					if (missionInfo.StationType.Value == 7)
					{
						Dictionary<StationType, int> dictionary2;
						(dictionary2 = dictionary1)[StationType.DEFENCE] = dictionary2[StationType.DEFENCE] - 1;
					}
					else if (missionInfo.StationType.Value == 6)
					{
						Dictionary<StationType, int> dictionary2;
						(dictionary2 = dictionary1)[StationType.MINING] = dictionary2[StationType.MINING] - 1;
					}
					else if (missionInfo.StationType.Value == 3 || missionInfo.StationType.Value == 4)
					{
						Dictionary<StationType, int> dictionary2;
						(dictionary2 = dictionary1)[StationType.CIVILIAN] = dictionary2[StationType.CIVILIAN] - 1;
						Dictionary<StationType, int> dictionary3;
						(dictionary3 = dictionary1)[StationType.DIPLOMATIC] = dictionary3[StationType.DIPLOMATIC] - 1;
						Dictionary<StationType, int> dictionary4;
						(dictionary4 = dictionary1)[StationType.NAVAL] = dictionary4[StationType.NAVAL] - 1;
						Dictionary<StationType, int> dictionary5;
						(dictionary5 = dictionary1)[StationType.SCIENCE] = dictionary5[StationType.SCIENCE] - 1;
						Dictionary<StationType, int> dictionary6;
						(dictionary6 = dictionary1)[StationType.GATE] = dictionary6[StationType.GATE] - 1;
					}
					else if (missionInfo.StationType.Value == 5)
						dictionary1[StationType.GATE] = 0;
					else if (missionInfo.StationType.Value == 2 || missionInfo.StationType.Value == 1 || missionInfo.StationType.Value == 5)
					{
						bool flag = false;
						int targetOrbitalObjectId = missionInfo.TargetOrbitalObjectID;
						foreach (ColonyInfo colonyInfo in list4)
						{
							if (colonyInfo.OrbitalObjectID == targetOrbitalObjectId)
							{
								Dictionary<StationType, int> dictionary2;
								(dictionary2 = dictionary1)[StationType.DIPLOMATIC] = dictionary2[StationType.DIPLOMATIC] - 1;
								Dictionary<StationType, int> dictionary3;
								(dictionary3 = dictionary1)[StationType.CIVILIAN] = dictionary3[StationType.CIVILIAN] - 1;
								Dictionary<StationType, int> dictionary4;
								(dictionary4 = dictionary1)[StationType.NAVAL] = dictionary4[StationType.NAVAL] - 1;
								Dictionary<StationType, int> dictionary5;
								(dictionary5 = dictionary1)[StationType.SCIENCE] = dictionary5[StationType.SCIENCE] - 1;
								Dictionary<StationType, int> dictionary6;
								(dictionary6 = dictionary1)[StationType.GATE] = dictionary6[StationType.GATE] - 1;
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							foreach (AsteroidBeltInfo asteroidBeltInfo in (IEnumerable<AsteroidBeltInfo>)list2)
							{
								foreach (LargeAsteroidInfo largeAsteroidInfo in game.GameDatabase.GetLargeAsteroidsInAsteroidBelt(asteroidBeltInfo.ID))
								{
									if (largeAsteroidInfo.ID == targetOrbitalObjectId && missionInfo.StationType.Value == 2)
									{
										Dictionary<StationType, int> dictionary2;
										(dictionary2 = dictionary1)[StationType.SCIENCE] = dictionary2[StationType.SCIENCE] - 1;
										flag = true;
										break;
									}
								}
							}
							if (!flag)
							{
								Dictionary<StationType, int> dictionary2;
								(dictionary2 = dictionary1)[StationType.SCIENCE] = dictionary2[StationType.SCIENCE] - 1;
								Dictionary<StationType, int> dictionary3;
								(dictionary3 = dictionary1)[StationType.NAVAL] = dictionary3[StationType.NAVAL] - 1;
								if (game.GameDatabase.GetPlanetInfo(targetOrbitalObjectId) != null && game.GameDatabase.GetPlanetInfo(targetOrbitalObjectId).Type != "barren")
								{
									Dictionary<StationType, int> dictionary4;
									(dictionary4 = dictionary1)[StationType.GATE] = dictionary4[StationType.GATE] - 1;
								}
							}
						}
					}
				}
			}
			int num = game.GameDatabase.GetNumberMaxStationsSupportedBySystem(game, systemID, playerId) - game.GameDatabase.GetStationForSystemAndPlayer(systemID, playerId).Where<StationInfo>((Func<StationInfo, bool>)(x => game.GameDatabase.GetStationRequiresSupport(x.DesignInfo.StationType))).Count<StationInfo>();
			foreach (MissionInfo missionInfo in list5)
			{
				if (missionInfo.StationType.HasValue && (game.GameDatabase.GetStationRequiresSupport((StationType)missionInfo.StationType.Value) || game.GameDatabase.GetStationRequiresSupport((StationType)missionInfo.StationType.Value) && num <= 0))
				{
					dictionary1[(StationType)missionInfo.StationType.Value] = 0;
					--num;
				}
			}
			foreach (StationType type in new List<StationType>((IEnumerable<StationType>)stationTypeList))
			{
				if ((game.GameDatabase.GetStationForSystemPlayerAndType(systemID, playerId, type) != null || num <= 0) && game.GameDatabase.GetStationRequiresSupport(type))
					dictionary1[type] = 0;
			}
			foreach (KeyValuePair<StationType, int> keyValuePair in dictionary1)
			{
				if (keyValuePair.Value <= 0)
					stationTypeList.Remove(keyValuePair.Key);
			}
			Faction faction = game.AssetDatabase.GetFaction(game.GameDatabase.GetPlayerInfo(playerId).FactionID);
			if (!faction.CanUseGate() || !game.GameDatabase.PlayerHasTech(playerId, "DRV_Gate_Stations"))
				stationTypeList.Remove(StationType.GATE);
			if (!game.GameDatabase.PlayerHasTech(playerId, "POL_Xeno-Colloquy") && (faction.Name != "zuul" || !game.GameDatabase.PlayerHasTech(playerId, "POL_Tribute_Systems")))
				stationTypeList.Remove(StationType.DIPLOMATIC);
			if (!game.GameDatabase.PlayerHasTech(playerId, "POL_FTL_Economics") && faction.Name != "zuul" && !game.GameDatabase.GetStratModifier<bool>(StratModifiers.EnableTrade, playerId))
				stationTypeList.Remove(StationType.CIVILIAN);
			if (!game.GameDatabase.PlayerHasTech(playerId, "BRD_Stealthed_Structures"))
				stationTypeList.Remove(StationType.DEFENCE);
			if (!Player.CanBuildMiningStations(game.GameDatabase, playerId))
				stationTypeList.Remove(StationType.MINING);
			return stationTypeList;
		}

		public static StationTypeFlags GetSupportedStationTypesForPlanet(
		  GameDatabase db,
		  PlanetInfo planet)
		{
			if (planet.Type == "gaseous")
				return StationTypeFlags.NAVAL | StationTypeFlags.SCIENCE | StationTypeFlags.GATE;
			if (planet.Type == "barren")
				return StationTypeFlags.NAVAL | StationTypeFlags.SCIENCE | StationTypeFlags.MINING | StationTypeFlags.DEFENCE;
			return db.GetColonyInfoForPlanet(planet.ID) != null ? StationTypeFlags.NAVAL | StationTypeFlags.SCIENCE | StationTypeFlags.CIVILIAN | StationTypeFlags.DIPLOMATIC | StationTypeFlags.GATE | StationTypeFlags.DEFENCE : StationTypeFlags.NAVAL | StationTypeFlags.SCIENCE | StationTypeFlags.GATE;
		}

		protected List<SlotData> GetSlotsForPlanet(StellarBody body, bool forceWithColony = false)
		{
			List<SlotData> slotDataList = new List<SlotData>();
			if (body.Parameters.BodyType == "gaseous")
			{
				for (int index = 0; index < 2; ++index)
					slotDataList.Add(new SlotData()
					{
						OccupantID = 0,
						Parent = body.ObjectID,
						ParentDBID = body.PlanetInfo.ID,
						SupportedTypes = StationTypeFlags.NAVAL | StationTypeFlags.SCIENCE | StationTypeFlags.GATE
					});
			}
			else if (body.Parameters.BodyType == "barren")
			{
				for (int index = 0; index < 2; ++index)
					slotDataList.Add(new SlotData()
					{
						OccupantID = 0,
						Parent = body.ObjectID,
						ParentDBID = body.PlanetInfo.ID,
						SupportedTypes = StationTypeFlags.DEFENCE
					});
				for (int index = 0; index < 2; ++index)
					slotDataList.Add(new SlotData()
					{
						OccupantID = 0,
						Parent = body.ObjectID,
						ParentDBID = body.PlanetInfo.ID,
						SupportedTypes = StationTypeFlags.MINING
					});
				slotDataList.Add(new SlotData()
				{
					OccupantID = 0,
					Parent = body.ObjectID,
					ParentDBID = body.PlanetInfo.ID,
					SupportedTypes = StationTypeFlags.NAVAL | StationTypeFlags.SCIENCE
				});
			}
			else if (this.App.GameDatabase.GetColonyInfoForPlanet(body.PlanetInfo.ID) != null || forceWithColony)
			{
				for (int index = 0; index < 4; ++index)
					slotDataList.Add(new SlotData()
					{
						OccupantID = 0,
						Parent = body.ObjectID,
						ParentDBID = body.PlanetInfo.ID,
						SupportedTypes = StationTypeFlags.NAVAL | StationTypeFlags.SCIENCE | StationTypeFlags.CIVILIAN | StationTypeFlags.DIPLOMATIC | StationTypeFlags.GATE | StationTypeFlags.DEFENCE
					});
			}
			else
			{
				for (int index = 0; index < 4; ++index)
					slotDataList.Add(new SlotData()
					{
						OccupantID = 0,
						Parent = body.ObjectID,
						ParentDBID = body.PlanetInfo.ID,
						SupportedTypes = StationTypeFlags.NAVAL | StationTypeFlags.SCIENCE | StationTypeFlags.GATE
					});
			}
			return slotDataList;
		}

		protected List<SlotData> GetSlotsForAsteroid(LargeAsteroid body)
		{
			List<SlotData> slotDataList = new List<SlotData>();
			for (int index = 0; index < 2; ++index)
				slotDataList.Add(new SlotData()
				{
					OccupantID = 0,
					Parent = body.ObjectID,
					ParentDBID = body.ID,
					SupportedTypes = StationTypeFlags.DEFENCE
				});
			slotDataList.Add(new SlotData()
			{
				OccupantID = 0,
				Parent = body.ObjectID,
				ParentDBID = body.ID,
				SupportedTypes = StationTypeFlags.MINING
			});
			slotDataList.Add(new SlotData()
			{
				OccupantID = 0,
				Parent = body.ObjectID,
				ParentDBID = body.ID,
				SupportedTypes = StationTypeFlags.SCIENCE
			});
			return slotDataList;
		}

		protected override GameObjectStatus OnCheckStatus()
		{
			return this._crits.CheckStatus();
		}

		public static StarModel CreateStar(
		  App game,
		  Vector3 origin,
		  StellarClass sclass,
		  string name,
		  float scale,
		  bool isInCombat)
		{
			float num = StarHelper.CalcRadius(sclass.Size);
			bool impostorEnabled = true;
			return new StarModel(game, StarHelper.GetDisplayParams(sclass).AssetPath, CommonCombatState.ApplyOriginShift(origin, Vector3.Zero) * scale, num * scale, isInCombat, DefaultStarModelParameters.ImposterMaterial, DefaultStarModelParameters.ImposterSpriteScale * 0.2f, DefaultStarModelParameters.ImposterRange, StarHelper.CalcModelColor(sclass).Xyz, impostorEnabled, name);
		}

		public static StarModel CreateStar(
		  App game,
		  Vector3 origin,
		  StarSystemInfo starInfo,
		  float scale,
		  bool isInCombat)
		{
			return StarSystem.CreateStar(game, origin, StellarClass.Parse(starInfo.StellarClass), starInfo.Name, scale, isInCombat);
		}

		public StarModel CreateStar(
		  GameObjectSet gameObjects,
		  Vector3 origin,
		  StarSystemInfo starInfo,
		  bool isInCombat)
		{
			StarModel star = StarSystem.CreateStar(gameObjects.App, origin, StellarClass.Parse(starInfo.StellarClass), starInfo.Name, this._scale, isInCombat);
			gameObjects.Add((IGameObject)star);
			star.StarSystemDatabaseID = starInfo.ID;
			return star;
		}

		public static AsteroidBelt CreateAsteroidBelt(
		  App game,
		  Vector3 origin,
		  AsteroidBeltInfo planetInfo,
		  Matrix world,
		  float scale)
		{
			float length = (world.Position - origin).Length;
			return new AsteroidBelt(game, 0, origin, length - 2500f, length + 2500f, -500f, 500f, 2000);
		}

		public static LargeAsteroid CreateLargeAsteroid(
		  App game,
		  Vector3 origin,
		  LargeAsteroidInfo largeAsteroid,
		  Matrix world,
		  float scale)
		{
			return new LargeAsteroid(game, world.Position)
			{
				WorldTransform = world,
				ID = largeAsteroid.ID
			};
		}

		public static StellarBody CreatePlanet(
		  GameSession game,
		  Vector3 origin,
		  PlanetInfo planetInfo,
		  Matrix world,
		  float scale,
		  bool isInCombat,
		  StarSystem.TerrestrialPlanetQuality quality = StarSystem.TerrestrialPlanetQuality.High)
		{
			if (StellarBodyTypes.IsTerrestrial(planetInfo.Type.ToLowerInvariant()) || planetInfo.Type.ToLowerInvariant() == StellarBodyTypes.Gaseous || !(planetInfo.Type.ToLowerInvariant() == StellarBodyTypes.Barren))
				return StarSystem.CreateTerrestrialPlanet(game, origin, planetInfo, world, scale, isInCombat, quality);
			return StarSystem.CreateTerrestrialPlanet(game, origin, planetInfo, world, scale, isInCombat, quality);
		}

		public StellarBody CreatePlanet(
		  GameSession game,
		  GameObjectSet gameObjects,
		  Vector3 origin,
		  PlanetInfo planetInfo,
		  Matrix world,
		  bool isInCombat)
		{
			StellarBody planet = StarSystem.CreatePlanet(game, origin, planetInfo, world, this._scale, isInCombat, StarSystem.TerrestrialPlanetQuality.High);
			gameObjects.Add((IGameObject)planet);
			return planet;
		}

		public AsteroidBelt CreateAsteroidBelt(
		  App game,
		  Vector3 origin,
		  AsteroidBeltInfo asteroidBeltInfo,
		  Matrix world)
		{
			return StarSystem.CreateAsteroidBelt(game, origin, asteroidBeltInfo, world, this._scale);
		}

		public LargeAsteroid CreateLargeAsteroid(
		  App game,
		  Vector3 origin,
		  LargeAsteroidInfo largeAsteroid,
		  Matrix world)
		{
			return StarSystem.CreateLargeAsteroid(game, origin, largeAsteroid, world, this._scale);
		}

		private static StellarBody CreateTerrestrialPlanet(
		  GameSession game,
		  Vector3 origin,
		  PlanetInfo planetInfo,
		  Matrix world,
		  float scale,
		  bool isInCombat,
		  StarSystem.TerrestrialPlanetQuality quality = StarSystem.TerrestrialPlanetQuality.High)
		{
			Vector3 vector3 = CommonCombatState.ApplyOriginShift(origin, world.Position) * scale;
			StellarBody.Params stellarBodyParams = game.AssetDatabase.PlanetGenerationRules.GetStellarBodyParams(game, planetInfo.ID);
			stellarBodyParams.SurfaceMaterial = "planet_earth2";
			stellarBodyParams.Position = vector3;
			stellarBodyParams.Radius *= scale;
			stellarBodyParams.IsInCombat = isInCombat;
			stellarBodyParams.TextureSize = (int)quality;
			IEnumerable<ColonyFactionInfo> civilianPopulations = game.GameDatabase.GetCivilianPopulations(planetInfo.ID);
			stellarBodyParams.Civilians = civilianPopulations.Select<ColonyFactionInfo, StellarBody.PlanetCivilianData>((Func<ColonyFactionInfo, StellarBody.PlanetCivilianData>)(civilian => new StellarBody.PlanetCivilianData()
			{
				Faction = game.GameDatabase.GetFactionName(civilian.FactionID),
				Population = civilian.CivilianPop
			})).ToArray<StellarBody.PlanetCivilianData>();
			ColonyInfo colonyInfoForPlanet = game.GameDatabase.GetColonyInfoForPlanet(planetInfo.ID);
			stellarBodyParams.ImperialPopulation = colonyInfoForPlanet != null ? colonyInfoForPlanet.ImperialPop : 0.0;
			stellarBodyParams.Infrastructure = planetInfo.Infrastructure;
			stellarBodyParams.Suitability = planetInfo.Suitability;
			stellarBodyParams.OrbitalID = planetInfo.ID;
			stellarBodyParams.BodyName = game.GameDatabase.GetOrbitalObjectInfo(planetInfo.ID).Name;
			StellarBody stellarBody = StellarBody.Create(game.App, stellarBodyParams);
			stellarBody.Population = stellarBodyParams.ImperialPopulation;
			stellarBody.PlanetInfo = planetInfo;
			return stellarBody;
		}

		public void Dispose()
		{
			this._crits.Dispose();
			this._slots.Dispose();
			this.App.ReleaseObject((IGameObject)this);
		}

		public bool Active
		{
			get
			{
				return this._active;
			}
			set
			{
				if (this._active == value)
					return;
				this._active = true;
				this.PostSetActive(true);
			}
		}

		public enum TerrestrialPlanetQuality
		{
			Low = 128, // 0x00000080
			High = 512, // 0x00000200
		}
	}
}
