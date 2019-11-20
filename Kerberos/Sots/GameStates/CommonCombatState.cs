// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.CommonCombatState
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Combat;
using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Encounters;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameTriggers;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Ships;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Steam;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Kerberos.Sots.GameStates
{
	internal abstract class CommonCombatState : GameState
	{
		public Dictionary<int, Dictionary<int, bool>> _combatStanceMap = new Dictionary<int, Dictionary<int, bool>>();
		protected Vector3? _simSpawnPosition = new Vector3?();
		private Vector3 _origin = Vector3.Zero;
		public const string ScratchFile = "data/scratch_combat.xml";
		private const int DEFAULT_COMMAND_POINTS = 18;
		protected GameObjectSet _crits;
		public List<IGameObject> _postLoadedObjects;
		private Sky _sky;
		private OrbitCameraController _camera;
		private Kerberos.Sots.GameStates.Combat _combat;
		private CombatInput _input;
		private CombatGrid _grid;
		private CombatSensor _sensor;
		public StarSystem _starSystemObjects;
		protected int _systemId;
		protected int _combatId;
		private int _detectionUpdateRate;
		private bool _testingState;
		private bool _authority;
		private bool _ignoreEncounterSpawnPos;
		private CommonCombatState.PirateEncounterData _pirateEncounterData;
		private CommonCombatState.ColonyTrapCombatData _trapCombatData;
		private List<SpawnProfile> _spawnPositions;
		private List<EntrySpawnLocation> _entryLocations;
		private List<Ship> _hitByNodeCannon;
		private List<FleetInfo> _mediaHeroFleets;
		private Random _random;
		private NeutralCombatStanceInfo _neutralCombatStanceInfo;
		private BidirMap<int, Ship> _ships;
		public List<CombatAI> AI_Commanders;
		public List<PointOfInterest> _pointsOfInterest;
		private CommonCombatState.CombatSubState _subState;
		private bool _combatEndingStatsGathered;
		private bool _combatEndDelayComplete;
		private bool _isPaused;
		private bool _engCombatActive;
		private PendingCombat _lastPendingCombat;
		protected List<Player> _playersInCombat;
		private List<Player> _ignoreCombatZonePlayers;
		private Dictionary<int, Dictionary<int, DiplomacyState>> _initialDiploStates;
		protected List<Player> _playersWithAssets;
		private Dictionary<int, int> _fleetsPerPlayer;
		private CombatData _combatData;
		private bool _sim;
		private XmlDocument _combatConfig;
		private Dictionary<IGameObject, XmlElement> _gameObjectConfigs;
		public static bool RetainCombatConfig;
		private List<DetectionSpheres> m_DetectionSpheres;
		private List<DetectionSpheres> m_SlewPlanetDetectionSpheres;
		private bool m_SlewMode;

		public int GetCombatID()
		{
			return this._combatId;
		}

		public IEnumerable<IGameObject> Objects
		{
			get
			{
				return this._crits.Objects;
			}
		}

		protected bool SimMode
		{
			get
			{
				return this._sim;
			}
			set
			{
				this._sim = value;
				foreach (CombatAI aiCommander in this.AI_Commanders)
					aiCommander.SimMode = this._sim;
			}
		}

		public Vector3 Origin
		{
			get
			{
				return this._origin;
			}
		}

		public CombatInput Input
		{
			get
			{
				return this._input;
			}
		}

		public Kerberos.Sots.GameStates.Combat Combat
		{
			get
			{
				return this._combat;
			}
		}

		public List<Player> PlayersInCombat
		{
			get
			{
				return this._playersInCombat;
			}
		}

		public void SaveCombatConfig(string filename)
		{
			if (!CommonCombatState.RetainCombatConfig)
				throw new InvalidOperationException("There is no retained combat configuration to recover evidence from. (Did you forget to tell the game to retain combat configuration evidence before entering combat?)");
			foreach (IGameObject key in this.Objects)
			{
				if (this._gameObjectConfigs.ContainsKey(key))
				{
					Ship ship = key as Ship;
					if (ship != null && ship.Active)
					{
						Vector3 position = ship.Maneuvering.Position;
						Vector3 rotation = ship.Maneuvering.Rotation;
						Vector3 degrees = Vector3.RadiansToDegrees(Matrix.CreateWorld(Vector3.Zero, rotation, Vector3.UnitY).EulerAngles);
						CombatConfig.ChangeXmlElementPositionAndRotation(this._gameObjectConfigs[key], position, degrees);
					}
				}
			}
			this._combatConfig.Save(filename);
		}

		public CommonCombatState(App game)
		  : base(game)
		{
			this.AI_Commanders = new List<CombatAI>();
			this._postLoadedObjects = new List<IGameObject>();
			this.m_DetectionSpheres = new List<DetectionSpheres>();
			this.m_SlewPlanetDetectionSpheres = new List<DetectionSpheres>();
			this._spawnPositions = new List<SpawnProfile>();
			this._pointsOfInterest = new List<PointOfInterest>();
			this._hitByNodeCannon = new List<Ship>();
			this._mediaHeroFleets = new List<FleetInfo>();
			this._isPaused = false;
			this._engCombatActive = false;
			this._pirateEncounterData = new CommonCombatState.PirateEncounterData();
			this._trapCombatData = new CommonCombatState.ColonyTrapCombatData();
			this.m_SlewMode = false;
			this._playersInCombat = new List<Player>();
			this._ignoreCombatZonePlayers = new List<Player>();
			this._initialDiploStates = (Dictionary<int, Dictionary<int, DiplomacyState>>)null;
			this._playersWithAssets = new List<Player>();
			this._neutralCombatStanceInfo = new NeutralCombatStanceInfo();
			this._detectionUpdateRate = 0;
			this._random = new Random();
		}

		public static Vector3 ApplyOriginShift(Vector3 origin, Vector3 position)
		{
			return position - origin;
		}

		protected override void OnPrepare(GameState prev, object[] stateParams)
		{
			this._simSpawnPosition = new Vector3?();
			this._ships = new BidirMap<int, Ship>();
			this.App.ObjectReleased += new Action<IGameObject>(this.Game_ObjectReleased);
			this._pirateEncounterData.Clear();
			this._trapCombatData.Clear();
			this._detectionUpdateRate = 0;
			if (stateParams == null || stateParams.Length == 0)
			{
				this.SimMode = false;
				this._authority = true;
				if (this.App.GameDatabase == null)
					this.App.NewGame();
				stateParams = new object[2]
				{
		  (object) new PendingCombat()
		  {
			SystemID = this.App.GameDatabase.GetHomeworlds().First<HomeworldInfo>((Func<HomeworldInfo, bool>) (x => x.PlayerID == this.App.LocalPlayer.ID)).SystemID
		  },
		  null
				};
			}
			this._spawnPositions.Clear();
			this._lastPendingCombat = stateParams[0] is PendingCombat ? (PendingCombat)stateParams[0] : new PendingCombat();
			this._systemId = this._lastPendingCombat.SystemID;
			this._combatId = this._lastPendingCombat.ConflictID;
			this._trapCombatData.IsTrapCombat = this._lastPendingCombat.Type == CombatType.CT_Colony_Trap;
			this._pirateEncounterData.IsPirateEncounter = this._lastPendingCombat.Type == CombatType.CT_Piracy;
			this._pirateEncounterData.PirateBase = this.App.GameDatabase.GetPirateBaseInfos().FirstOrDefault<PirateBaseInfo>((Func<PirateBaseInfo, bool>)(x => x.SystemId == this._systemId));
			XmlDocument stateParam = (XmlDocument)stateParams[1];
			if (stateParams.Length >= 4)
			{
				this._testingState = (bool)stateParams[2];
				this._authority = (bool)stateParams[3];
			}
			else
			{
				this._testingState = true;
				this._authority = true;
			}
			int shipOrbitParentId = this.SelectOriginOrbital(this._systemId);
			this._origin = Vector3.Zero;
			float radius = 1E+09f;
			this._crits = new GameObjectSet(this.App);
			this._sensor = this._crits.Add<CombatSensor>();
			this._input = this._crits.Add<CombatInput>();
			this._starSystemObjects = new StarSystem(this.App, 1f, this._systemId, Vector3.Zero, true, this._sensor, true, this._input.ObjectID, false, true);
			this._starSystemObjects.PostSetProp("InputEnabled", false);
			this._crits.Add((IGameObject)this._starSystemObjects);
			foreach (IGameObject gameObject in this._starSystemObjects.Crits.OfType<Ship>())
				this._crits.Add(gameObject);
			this._sky = new Sky(this.App, SkyUsage.InSystem, this._systemId);
			this._crits.Add((IGameObject)this._sky);
			this._camera = this._crits.Add<OrbitCameraController>();
			this._grid = this._crits.Add<CombatGrid>();
			this._fleetsPerPlayer = new Dictionary<int, int>();
			foreach (int fleetId in this._lastPendingCombat.FleetIDs)
			{
				FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(fleetId);
				if (fleetInfo != null)
				{
					if (this._fleetsPerPlayer.ContainsKey(fleetInfo.PlayerID))
					{
						Dictionary<int, int> fleetsPerPlayer;
						int playerId;
						(fleetsPerPlayer = this._fleetsPerPlayer)[playerId = fleetInfo.PlayerID] = fleetsPerPlayer[playerId] + 1;
					}
					else
						this._fleetsPerPlayer[fleetInfo.PlayerID] = 1;
				}
			}
			Dictionary<IGameObject, XmlElement> dictionary1 = new Dictionary<IGameObject, XmlElement>();
			if (stateParam != null)
				dictionary1 = CombatConfig.CreateGameObjects(this.App, this.Origin, stateParam, this._input.ObjectID);
			if (this._lastPendingCombat.PlayersInCombat != null && this._lastPendingCombat.PlayersInCombat.Count > 0)
			{
				this._playersInCombat = new List<Player>();
				foreach (int playerId in this._lastPendingCombat.PlayersInCombat)
					this._playersInCombat.Add(this.App.Game.GetPlayerObject(playerId));
			}
			else
			{
				this._playersInCombat = GameSession.GetPlayersWithCombatAssets(this.App, this._systemId).ToList<Player>();
				this.App.GameDatabase.GetFreighterInfosForSystem(this._systemId);
				foreach (IGameObject key in dictionary1.Keys)
				{
					if (key is Ship)
					{
						Player player = (key as Ship).Player;
						if (!this._playersInCombat.Contains(player))
							this._playersInCombat.Add(player);
					}
				}
			}
			foreach (int num in this._lastPendingCombat.NPCPlayersInCombat)
			{
				int npcId = num;
				if (!this._playersInCombat.Any<Player>((Func<Player, bool>)(x => x.ID == npcId)))
					this._playersInCombat.Add(this.App.Game.GetPlayerObject(npcId));
			}
			List<FleetInfo> source1 = new List<FleetInfo>();
			if (this._pirateEncounterData.IsPirateEncounter)
			{
				foreach (FleetInfo fleetInfo in this.App.GameDatabase.GetFleetInfoBySystemID(this._systemId, FleetType.FL_NORMAL).Where<FleetInfo>((Func<FleetInfo, bool>)(x => !Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(this.App.Game, x))).ToList<FleetInfo>())
				{
					MissionInfo missionByFleetId = this.App.GameDatabase.GetMissionByFleetID(fleetInfo.ID);
					if (missionByFleetId != null && missionByFleetId.Type == MissionType.PIRACY)
					{
						source1.Add(fleetInfo);
						Player playerObject = this.App.Game.GetPlayerObject(fleetInfo.PlayerID);
						if (!this._playersInCombat.Contains(playerObject))
							this._playersInCombat.Add(playerObject);
					}
				}
			}
			foreach (Player player in this._playersInCombat)
				this._playersWithAssets.Add(player);
			Dictionary<int, List<FleetInfo>> dictionary2 = new Dictionary<int, List<FleetInfo>>();
			if (this._lastPendingCombat != null)
			{
				foreach (FleetInfo fleetInfo in source1)
				{
					if (!dictionary2.ContainsKey(fleetInfo.PlayerID))
					{
						dictionary2[fleetInfo.PlayerID] = new List<FleetInfo>((IEnumerable<FleetInfo>)new FleetInfo[1]
						{
			  fleetInfo
						});
						this._pirateEncounterData.PiratePlayerIDs.Add(fleetInfo.PlayerID);
					}
				}
				foreach (KeyValuePair<int, int> selectedPlayerFleet1 in this._lastPendingCombat.SelectedPlayerFleets)
				{
					KeyValuePair<int, int> selectedPlayerFleet = selectedPlayerFleet1;
					if (!this._pirateEncounterData.IsPirateEncounter || !source1.Any<FleetInfo>((Func<FleetInfo, bool>)(x => x.PlayerID == selectedPlayerFleet.Key)))
					{
						FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(selectedPlayerFleet.Value);
						if (fleetInfo == null)
						{
							List<FleetInfo> list = this.App.GameDatabase.GetFleetsByPlayerAndSystem(selectedPlayerFleet.Key, this._systemId, FleetType.FL_NORMAL).ToList<FleetInfo>();
							if (list != null && list.Count > 0)
								fleetInfo = list.First<FleetInfo>();
						}
						if (this._trapCombatData.IsTrapCombat)
						{
							PlayerInfo pi = fleetInfo != null ? this.App.GameDatabase.GetPlayerInfo(fleetInfo.PlayerID) : (PlayerInfo)null;
							Faction faction = pi != null ? this.App.AssetDatabase.Factions.FirstOrDefault<Faction>((Func<Faction, bool>)(x => x.ID == pi.FactionID)) : (Faction)null;
							if (faction == null || faction.Name == "morrigi")
								fleetInfo = (FleetInfo)null;
						}
						dictionary2[selectedPlayerFleet.Key] = new List<FleetInfo>((IEnumerable<FleetInfo>)new FleetInfo[1]
						{
			  fleetInfo
						});
					}
				}
			}
			if (this._systemId != 0)
			{
				List<FleetInfo> list = this.App.GameDatabase.GetFleetInfoBySystemID(this._systemId, FleetType.FL_NORMAL).ToList<FleetInfo>();
				list.AddRange((IEnumerable<FleetInfo>)this.App.GameDatabase.GetFleetInfoBySystemID(this._systemId, FleetType.FL_ALL_COMBAT).ToList<FleetInfo>());
				list.AddRange((IEnumerable<FleetInfo>)this.App.GameDatabase.GetFleetInfoBySystemID(this._systemId, FleetType.FL_NORMAL).Where<FleetInfo>((Func<FleetInfo, bool>)(x => Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(this.App.Game, x))).ToList<FleetInfo>());
				foreach (FleetInfo fleet in list)
				{
					if (fleet != null)
					{
						if (!dictionary2.ContainsKey(fleet.PlayerID))
							dictionary2[fleet.PlayerID] = new List<FleetInfo>((IEnumerable<FleetInfo>)new FleetInfo[1]
							{
				fleet
							});
						else if (!dictionary2[fleet.PlayerID].Contains(fleet) && ((FleetType.FL_ALL_COMBAT & fleet.Type) != (FleetType)0 || Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(this.App.Game, fleet)))
							dictionary2[fleet.PlayerID].Add(fleet);
					}
				}
				VonNeumann vonNeumann = this.App.Game.ScriptModules.VonNeumann;
				if (vonNeumann != null && vonNeumann.HomeWorldSystemID == this._systemId)
				{
					foreach (FleetInfo fi in list)
					{
						if (fi != null && fi.PlayerID == vonNeumann.PlayerID && vonNeumann.CanSpawnFleetAtHomeWorld(fi))
						{
							if (!dictionary2.ContainsKey(fi.PlayerID))
								dictionary2[fi.PlayerID] = new List<FleetInfo>((IEnumerable<FleetInfo>)new FleetInfo[1]
								{
				  fi
								});
							else if (!dictionary2[fi.PlayerID].Contains(fi))
								dictionary2[fi.PlayerID].Add(fi);
						}
					}
				}
			}
			if (this._trapCombatData.IsTrapCombat)
			{
				List<FleetInfo> fleetInfoList = new List<FleetInfo>();
				foreach (KeyValuePair<int, List<FleetInfo>> keyValuePair in dictionary2)
					fleetInfoList.AddRange((IEnumerable<FleetInfo>)keyValuePair.Value);
				foreach (FleetInfo fleetInfo1 in fleetInfoList)
				{
					if (fleetInfo1 != null && fleetInfo1.IsNormalFleet)
					{
						PlayerInfo pi = this.App.GameDatabase.GetPlayerInfo(fleetInfo1.PlayerID);
						if (pi != null && pi.isStandardPlayer)
						{
							Faction faction = this.App.AssetDatabase.Factions.FirstOrDefault<Faction>((Func<Faction, bool>)(x => x.ID == pi.FactionID));
							if (faction != null && !(faction.Name == "morrigi"))
							{
								MissionInfo missionByFleetId = this.App.GameDatabase.GetMissionByFleetID(fleetInfo1.ID);
								if (missionByFleetId != null && missionByFleetId.Type == MissionType.COLONIZATION)
								{
									ColonyTrapInfo trapInfoByPlanetId = this.App.GameDatabase.GetColonyTrapInfoByPlanetID(missionByFleetId.TargetOrbitalObjectID);
									if (trapInfoByPlanetId != null)
									{
										FleetInfo fleetInfo2 = this.App.GameDatabase.GetFleetInfo(trapInfoByPlanetId.FleetID);
										if (fleetInfo2 != null && this.App.GameDatabase.GetDiplomacyStateBetweenPlayers(fleetInfo2.PlayerID, fleetInfo1.PlayerID) != DiplomacyState.ALLIED)
										{
											if (!dictionary2.ContainsKey(fleetInfo2.PlayerID))
												dictionary2[fleetInfo2.PlayerID] = new List<FleetInfo>((IEnumerable<FleetInfo>)new FleetInfo[1]
												{
						  fleetInfo2
												});
											else if (!dictionary2[fleetInfo2.PlayerID].Contains(fleetInfo2))
												dictionary2[fleetInfo2.PlayerID].Add(fleetInfo2);
											if (!this._trapCombatData.TrapFleets.Contains(fleetInfo2))
											{
												this._trapCombatData.TrapFleets.Add(fleetInfo2);
												this._trapCombatData.TrapToPlanet.Add(fleetInfo2.ID, trapInfoByPlanetId.PlanetID);
												if (!this._trapCombatData.TrapPlayers.Contains(fleetInfo2.PlayerID))
													this._trapCombatData.TrapPlayers.Add(fleetInfo2.PlayerID);
											}
											if (!this._trapCombatData.ColonyTrappedFleets.Contains(fleetInfo1))
											{
												this._trapCombatData.ColonyTrappedFleets.Add(fleetInfo1);
												this._trapCombatData.ColonyFleetToTrap.Add(fleetInfo1.ID, fleetInfo2.ID);
											}
										}
									}
								}
							}
						}
					}
				}
			}
			int encounterIdAtSystem = this.App.GameDatabase.GetEncounterIDAtSystem(EasterEgg.EE_ASTEROID_MONITOR, this._systemId);
			List<MorrigiRelicInfo> list1 = this.App.GameDatabase.GetMorrigiRelicInfos().ToList<MorrigiRelicInfo>();
			AsteroidMonitorInfo asteroidMonitorInfo = this.App.GameDatabase.GetAsteroidMonitorInfo(encounterIdAtSystem);
			MorrigiRelicInfo mri = list1.FirstOrDefault<MorrigiRelicInfo>((Func<MorrigiRelicInfo, bool>)(x => x.SystemId == this._systemId));
			this.RebuildInitialDiploStates(asteroidMonitorInfo, mri);
			Dictionary<Player, Dictionary<Player, PlayerCombatDiplomacy>> diplomacyStates = new Dictionary<Player, Dictionary<Player, PlayerCombatDiplomacy>>();
			foreach (Player key1 in this._playersInCombat)
			{
				diplomacyStates.Add(key1, new Dictionary<Player, PlayerCombatDiplomacy>());
				foreach (Player key2 in this._playersInCombat)
				{
					if (key2 != key1)
					{
						if (!this.EncounterIsNeutral(key1.ID, key2.ID, asteroidMonitorInfo, mri))
						{
							switch (this.GetDiplomacyState(key1.ID, key2.ID))
							{
								case DiplomacyState.WAR:
									diplomacyStates[key1].Add(key2, PlayerCombatDiplomacy.War);
									continue;
								case DiplomacyState.ALLIED:
									diplomacyStates[key1].Add(key2, PlayerCombatDiplomacy.Allied);
									continue;
								default:
									diplomacyStates[key1].Add(key2, PlayerCombatDiplomacy.Neutral);
									continue;
							}
						}
						else
							diplomacyStates[key1].Add(key2, PlayerCombatDiplomacy.Neutral);
					}
				}
			}
			this.IdentifyEntryPointLocations((IDictionary<int, List<FleetInfo>>)dictionary2);
			if (this.App.Game.ScriptModules.Pirates != null && !this._pirateEncounterData.IsPirateEncounter)
			{
				this._pirateEncounterData.IsPirateEncounter = dictionary2.Keys.Any<int>((Func<int, bool>)(x => x == this.App.Game.ScriptModules.Pirates.PlayerID));
				if (this._pirateEncounterData.IsPirateEncounter)
					this._pirateEncounterData.PiratePlayerIDs.Add(this.App.Game.ScriptModules.Pirates.PlayerID);
			}
			this._ignoreEncounterSpawnPos = false;
			using (List<Player>.Enumerator enumerator = this._playersInCombat.Where<Player>((Func<Player, bool>)(x => x.IsStandardPlayer)).ToList<Player>().GetEnumerator())
			{
			label_147:
				while (enumerator.MoveNext())
				{
					Player current = enumerator.Current;
					foreach (Player player in this._playersInCombat.Where<Player>((Func<Player, bool>)(x => x.IsStandardPlayer)).ToList<Player>())
					{
						if (current != player)
						{
							switch (this.GetDiplomacyState(current.ID, player.ID))
							{
								case DiplomacyState.ALLIED:
									continue;
								default:
									this._ignoreEncounterSpawnPos = true;
									goto label_147;
							}
						}
					}
				}
			}
			OrbitalObjectInfo[] array1 = this.App.GameDatabase.GetStarSystemOrbitalObjectInfos(this._systemId).ToArray<OrbitalObjectInfo>();
			int[] array2 = this.CreateShips(this._crits, this._systemId, shipOrbitParentId, (IDictionary<int, List<FleetInfo>>)dictionary2, array1).ToArray<int>();
			if (this._pirateEncounterData.IsPirateEncounter)
				((IEnumerable<int>)array2).Concat<int>((IEnumerable<int>)this.SpawnPiracyEncounterShips(this._crits, array1).ToArray<int>());
			if (CommonCombatState.RetainCombatConfig)
				this._combatConfig = stateParam;
			if (stateParam != null)
			{
				if (CommonCombatState.RetainCombatConfig)
					this._gameObjectConfigs = dictionary1;
				this._crits.Add((IEnumerable<IGameObject>)dictionary1.Keys);
			}
			List<BattleRiderSquad> battleRiderSquadList = new List<BattleRiderSquad>();
			List<Ship> source2 = new List<Ship>();
			List<IGameObject> gameObjectList = new List<IGameObject>();
			Dictionary<int, List<Ship>> dictionary3 = new Dictionary<int, List<Ship>>();
			foreach (IGameObject gameObject in this.Objects.Where<IGameObject>((Func<IGameObject, bool>)(x => x is Ship)))
			{
				Ship ship = gameObject as Ship;
				if (ship != null)
				{
					if (dictionary3.ContainsKey(ship.Player.ID))
						dictionary3[ship.Player.ID].Add(ship);
					else
						dictionary3.Add(ship.Player.ID, new List<Ship>()
			{
			  ship
			});
					if (ship.IsCarrier)
					{
						foreach (BattleRiderSquad battleRiderSquad in ship.BattleRiderSquads)
							battleRiderSquadList.Add(battleRiderSquad);
						source2.Add(ship);
					}
					if (ship.IsBattleRider)
						gameObjectList.Add(gameObject);
				}
			}
			foreach (KeyValuePair<int, List<Ship>> keyValuePair1 in dictionary3)
			{
				Dictionary<ShipFleetAbilityType, List<Ship>> dictionary4 = new Dictionary<ShipFleetAbilityType, List<Ship>>();
				foreach (Ship ship in keyValuePair1.Value)
				{
					if (ship.AbilityType != ShipFleetAbilityType.None)
					{
						if (dictionary4.ContainsKey(ship.AbilityType))
							dictionary4[ship.AbilityType].Add(ship);
						else
							dictionary4.Add(ship.AbilityType, new List<Ship>()
			  {
				ship
			  });
					}
				}
				foreach (KeyValuePair<ShipFleetAbilityType, List<Ship>> keyValuePair2 in dictionary4)
					this.CreateShipAbility(keyValuePair2.Key, keyValuePair2.Value, keyValuePair1.Value);
			}
			int num1 = 0;
			int num2 = 0;
			foreach (Ship ship1 in gameObjectList)
			{
				Ship battleRider = ship1;
				if (battleRider.ParentDatabaseID != 0)
				{
					Ship ship2 = source2.FirstOrDefault<Ship>((Func<Ship, bool>)(x => x.DatabaseID == battleRider.ParentDatabaseID));
					if (ship2 != null)
					{
						BattleRiderSquad squad = ship2?.AssignRiderToSquad(battleRider as BattleRiderShip, battleRider.RiderIndex);
						if (squad != null)
						{
							battleRider.ParentID = ship2.ObjectID;
							battleRider.PostSetBattleRiderParent(squad.ObjectID);
						}
					}
				}
				else
				{
					int num3 = 0;
					foreach (BattleRiderSquad battleRiderSquad in battleRiderSquadList)
					{
						if (num3 == num2 && num1 < battleRiderSquad.NumRiders)
						{
							battleRider.ParentID = battleRiderSquad.ParentID;
							battleRider.PostSetBattleRiderParent(battleRiderSquad.ObjectID);
							++num1;
							if (num1 >= battleRiderSquad.NumRiders)
							{
								num1 = 0;
								++num2;
								break;
							}
							break;
						}
						++num3;
					}
				}
			}
			foreach (IGameObject gameObject in this.Objects)
			{
				if (gameObject is Ship)
					this.AddAItoCombat(gameObject as Ship);
			}
			float val1 = this.App.GameSetup.CombatTurnLength;
			if (this._pirateEncounterData.IsPirateEncounter && this._pirateEncounterData.PirateBase == null)
				val1 = 2.5f;
			else if (this.App.Game != null && this.App.Game.ScriptModules != null && !this._playersInCombat.Any<Player>((Func<Player, bool>)(x => x.ID == this.App.LocalPlayer.ID)))
			{
				int num3 = this._playersInCombat.Where<Player>((Func<Player, bool>)(x =>
			   {
				   if (x.IsStandardPlayer)
					   return x.ID != this.App.LocalPlayer.ID;
				   return false;
			   })).Count<Player>();
				int num4 = this._playersInCombat.Where<Player>((Func<Player, bool>)(x => this.App.Game.ScriptModules.IsEncounterPlayer(x.ID))).Count<Player>();
				if (num3 == 1 && num4 > 0)
					val1 = Math.Min(val1, 5f);
			}
			this._mediaHeroFleets.Clear();
			foreach (KeyValuePair<int, List<FleetInfo>> keyValuePair in dictionary2)
			{
				foreach (FleetInfo fleetInfo in keyValuePair.Value)
				{
					FleetInfo fleet = fleetInfo;
					if (fleet != null)
					{
						List<AdmiralInfo.TraitType> list2 = this.App.GameDatabase.GetAdmiralTraits(fleet.AdmiralID).ToList<AdmiralInfo.TraitType>();
						if (list2.Contains(AdmiralInfo.TraitType.GloryHound))
						{
							foreach (IGameObject state in this._starSystemObjects.GetPlanetsInSystem().Where<StellarBody>((Func<StellarBody, bool>)(x => x.Parameters.ColonyPlayerID == fleet.PlayerID)))
								state.PostSetProp("SetPlanetDamageModifier", 1.5f);
						}
						if (list2.Contains(AdmiralInfo.TraitType.MediaHero))
							this._mediaHeroFleets.Add(fleet);
					}
				}
			}
			this._combat = Kerberos.Sots.GameStates.Combat.Create(this.App, this._camera, this._input, this._sensor, this._starSystemObjects, this._grid, this._origin, radius, (int)((double)val1 * 60000.0), this._playersInCombat.ToArray(), diplomacyStates, this.SimMode);
			this._combat.PostObjectAddObjects(this._crits.ToArray<IGameObject>());
			this._combat.PostSetProp("SetSlewModeMultipliers", (object)this.App.AssetDatabase.SlewModeMultiplier, (object)this.App.AssetDatabase.SlewModeDecelMultiplier);
			this._crits.Add((IGameObject)this._combat);
			this.PopulateStarSystemAcceleratorHoops();
			this.PopulateCombatAsteroidBeltJammers();
			this.InitializePlayerCombatZonesToIgnore();
			if (!this._sim)
				return;
			List<object> objectList = new List<object>();
			objectList.Add((object)true);
			objectList.Add((object)false);
			foreach (IGameObject state in this._starSystemObjects.Crits.OfType<StellarBody>().ToList<StellarBody>())
				state.PostSetProp("SetSensorMode", objectList.ToArray());
		}

		private void PopulateCombatAsteroidBeltJammers()
		{
			if (this._starSystemObjects == null || this._starSystemObjects.AsteroidBelts.Count == 0)
				return;
			List<object> objectList = new List<object>();
			objectList.Add((object)this._starSystemObjects.AsteroidBelts.Count);
			foreach (AsteroidBelt asteroidBelt in this._starSystemObjects.AsteroidBelts)
				objectList.Add((object)asteroidBelt.ObjectID);
			this._combat.PostSetProp("PopulateAsteroidBeltJammers", objectList.ToArray());
		}

		private void InitializePlayerCombatZonesToIgnore()
		{
			if (this._starSystemObjects == null)
				return;
			foreach (Player player in this._playersInCombat)
			{
				if (!player.IsStandardPlayer || this._pirateEncounterData.IsPirateEncounter && this._pirateEncounterData.PiratePlayerIDs.Contains(player.ID))
					this._ignoreCombatZonePlayers.Add(player);
			}
			if (this._ignoreCombatZonePlayers.Count <= 0)
				return;
			List<object> objectList = new List<object>();
			objectList.Add((object)this._ignoreCombatZonePlayers.Count);
			foreach (Player combatZonePlayer in this._ignoreCombatZonePlayers)
				objectList.Add((object)combatZonePlayer.ObjectID);
			this._starSystemObjects.PostSetProp("SetPlayerZoneColorsToIgnore", objectList.ToArray());
		}

		private void PopulateStarSystemAcceleratorHoops()
		{
			if (this._starSystemObjects == null)
				return;
			List<Ship> list = this._crits.OfType<Ship>().Where<Ship>((Func<Ship, bool>)(x =>
		   {
			   if (x.IsAcceleratorHoop && x.WeaponBanks != null)
				   return x.WeaponBanks.Any<WeaponBank>((Func<WeaponBank, bool>)(y => y.Weapon.PayloadType == WeaponEnums.PayloadTypes.MegaBeam));
			   return false;
		   })).ToList<Ship>();
			if (list.Count <= 0)
				return;
			List<object> objectList = new List<object>();
			objectList.Add((object)list.Count);
			foreach (Ship ship in list)
			{
				objectList.Add((object)ship.ObjectID);
				objectList.Add((object)ship.WeaponBanks.Select<WeaponBank, LogicalWeapon>((Func<WeaponBank, LogicalWeapon>)(x => x.Weapon)).First<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.PayloadType == WeaponEnums.PayloadTypes.MegaBeam)).UniqueWeaponID);
				objectList.Add((object)ship.Position);
			}
			this._starSystemObjects.PostSetProp("SetAcceleratorHoopLocationsCombat", objectList.ToArray());
		}

		private void Game_ObjectReleased(IGameObject obj)
		{
			Ship index = obj as Ship;
			if (index == null || !this._ships.Reverse.ContainsKey(index))
				return;
			this._ships.Remove(this._ships.Reverse[index], index);
		}

		protected abstract void OnCombatEnding();

		protected abstract void SyncPlayerList();

		public Ship GetShipCompoundByObjectID(int objectID)
		{
			return (Ship)this.App.GetGameObject(objectID);
		}

		private void IdentifyEntryPointLocations(
		  IDictionary<int, List<FleetInfo>> selectedPlayerFleets)
		{
			this._entryLocations = new List<EntrySpawnLocation>();
			foreach (List<FleetInfo> fleetInfoList in (IEnumerable<List<FleetInfo>>)selectedPlayerFleets.Values)
			{
				foreach (FleetInfo fleetInfo in fleetInfoList)
				{
					FleetInfo fleet = fleetInfo;
					if (fleet != null && fleet.Type != FleetType.FL_DEFENSE && fleet.Type == FleetType.FL_NORMAL)
					{
						int? previousSystemId = fleet.PreviousSystemID;
						int systemId = this._systemId;
						if ((previousSystemId.GetValueOrDefault() != systemId ? 0 : (previousSystemId.HasValue ? 1 : 0)) == 0 && fleet.PreviousSystemID.HasValue)
						{
							CombatZonePositionInfo zoneForOuterSystem = this._starSystemObjects.GetEnteryZoneForOuterSystem(fleet.PreviousSystemID.Value);
							if (zoneForOuterSystem != null)
							{
								Faction faction = this.App.GetPlayer(fleet.PlayerID).Faction;
								if (!this._entryLocations.Any<EntrySpawnLocation>((Func<EntrySpawnLocation, bool>)(x =>
							   {
								   if (x.PreviousSystemID == fleet.PreviousSystemID.Value)
									   return x.FactionID == faction.ID;
								   return false;
							   })))
								{
									Vector3 starSystemOrigin = this.App.GameDatabase.GetStarSystemOrigin(this._systemId);
									Vector3 vector3 = this.App.GameDatabase.GetStarSystemOrigin(fleet.PreviousSystemID.Value) - starSystemOrigin;
									vector3.Y = 0.0f;
									this._entryLocations.Add(new EntrySpawnLocation()
									{
										FactionID = faction.ID,
										PreviousSystemID = fleet.PreviousSystemID.Value,
										Position = Vector3.Normalize(vector3) * zoneForOuterSystem.RadiusLower
									});
								}
							}
						}
					}
				}
			}
		}

		private IEnumerable<int> CreateShips(
		  GameObjectSet crits,
		  int systemId,
		  int shipOrbitParentId,
		  IDictionary<int, List<FleetInfo>> selectedPlayerFleets,
		  OrbitalObjectInfo[] objects)
		{
			if (systemId != 0 || shipOrbitParentId != 0)
			{
				List<CombatEasterEggData> eeData = this.GetCombatEasterEggData(objects, selectedPlayerFleets);
				List<CombatRandomData> randData = new List<CombatRandomData>();
				if (eeData.Count == 0)
					randData = this.GetCombatRandomData(objects, selectedPlayerFleets);
				this.InitializeNeutralCombatStanceInfo(selectedPlayerFleets);
				foreach (List<FleetInfo> fleetInfoList in (IEnumerable<List<FleetInfo>>)selectedPlayerFleets.Values)
				{
					int spawnedFleetCount = 0;
					foreach (FleetInfo fleetInfo in fleetInfoList)
					{
						if (fleetInfo != null && fleetInfo.Type != FleetType.FL_RESERVE && (this.App.Game.ScriptModules.VonNeumann == null || this.App.Game.ScriptModules.VonNeumann.PlayerID != fleetInfo.PlayerID || (this.App.Game.ScriptModules.VonNeumann.HomeWorldSystemID != this._systemId || this.App.Game.ScriptModules.VonNeumann.CanSpawnFleetAtHomeWorld(fleetInfo))))
						{
							switch (fleetInfo.Type)
							{
								case FleetType.FL_DEFENSE:
									using (IEnumerator<int> enumerator = this.SpawnDefenseFleet(crits, fleetInfo).GetEnumerator())
									{
										while (enumerator.MoveNext())
										{
											int defense = enumerator.Current;
											yield return defense;
										}
										break;
									}
								case FleetType.FL_GATE:
									using (IEnumerator<int> enumerator = this.SpawnGateFleet(crits, fleetInfo).GetEnumerator())
									{
										while (enumerator.MoveNext())
										{
											int gate = enumerator.Current;
											yield return gate;
										}
										break;
									}
								case FleetType.FL_ACCELERATOR:
									using (IEnumerator<int> enumerator = this.SpawnAcceleratorFleet(crits, fleetInfo).GetEnumerator())
									{
										while (enumerator.MoveNext())
										{
											int accelerator = enumerator.Current;
											yield return accelerator;
										}
										break;
									}
								default:
									using (IEnumerator<int> enumerator = this.SpawnFleet(crits, fleetInfo, eeData, randData, selectedPlayerFleets, objects, spawnedFleetCount).GetEnumerator())
									{
										while (enumerator.MoveNext())
										{
											int ship = enumerator.Current;
											yield return ship;
										}
										break;
									}
							}
							++spawnedFleetCount;
						}
					}
				}
				this.CreateAllPointsOfInterest(crits, objects, eeData);
			}
		}

		private int GetNumFreighters(int max)
		{
			int num;
			for (num = 1; num < max; ++num)
			{
				switch (num)
				{
					case 1:
						if (this._random.CoinToss(50))
							break;
						goto label_7;
					case 2:
						if (this._random.CoinToss(20))
							break;
						goto label_7;
					default:
						if (!this._random.CoinToss(10))
							goto label_7;
						else
							break;
				}
			}
		label_7:
			return num;
		}

		private IEnumerable<int> SpawnPiracyEncounterShips(
		  GameObjectSet crits,
		  OrbitalObjectInfo[] objects)
		{
			List<StellarBody> planets = this._starSystemObjects.Crits.Objects.OfType<StellarBody>().ToList<StellarBody>();
			List<StarModel> stars = this._starSystemObjects.Crits.Objects.OfType<StarModel>().ToList<StarModel>();
			List<StellarBody> populatedWorlds = new List<StellarBody>();
			float furthestOffset = 0.0f;
			foreach (StellarBody stellarBody in planets)
			{
				if (stellarBody.Population > 0.0)
					populatedWorlds.Add(stellarBody);
				float lengthSquared = stellarBody.Parameters.Position.LengthSquared;
				if ((double)lengthSquared > (double)furthestOffset)
					furthestOffset = lengthSquared;
			}
			if (this._starSystemObjects.CombatZones.Count > 0)
			{
				CombatZonePositionInfo zonePositionInfo = this._starSystemObjects.CombatZones.Last<CombatZonePositionInfo>();
				furthestOffset = (float)(((double)zonePositionInfo.RadiusLower + (double)zonePositionInfo.RadiusUpper) * 0.5);
			}
			else
				furthestOffset = (double)furthestOffset <= 0.0 ? (stars.Count <= 0 ? 50000f : (float)((double)stars.First<StarModel>().Radius + 7500.0 + 10000.0)) : (float)Math.Sqrt((double)furthestOffset);
			Dictionary<int, List<PlayerTechInfo>> playerTechs = new Dictionary<int, List<PlayerTechInfo>>();
			Dictionary<int, List<FreighterInfo>> playerFreighters = new Dictionary<int, List<FreighterInfo>>();
			Dictionary<int, StationInfo> playerCivStation = new Dictionary<int, StationInfo>();
			Dictionary<int, int> numPlayerFreighters = new Dictionary<int, int>();
			TradeResultsTable trt = this.App.GameDatabase.GetTradeResultsTable();
			List<FreighterInfo> freighters = this.App.GameDatabase.GetFreighterInfosForSystem(this._systemId).ToList<FreighterInfo>();
			foreach (Player player in this._playersInCombat)
			{
				Player p = player;
				playerTechs.Add(p.ID, this.App.GameDatabase.GetPlayerTechInfos(p.ID).Where<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.State == TechStates.Researched)).ToList<PlayerTechInfo>());
				playerFreighters.Add(p.ID, freighters.Where<FreighterInfo>((Func<FreighterInfo, bool>)(x => x.PlayerId == p.ID)).ToList<FreighterInfo>());
				playerCivStation.Add(p.ID, this.App.GameDatabase.GetCivilianStationForSystemAndPlayer(this._systemId, p.ID));
				int num = 0;
				if (trt != null && trt.TradeNodes.ContainsKey(this._systemId))
					num = num + trt.TradeNodes[this._systemId].ImportInt + trt.TradeNodes[this._systemId].ImportProv + trt.TradeNodes[this._systemId].ImportLoc;
				int numFreighters = this.GetNumFreighters(Math.Max((playerFreighters[p.ID].Count + num) * 60 / 100, 1));
				numPlayerFreighters.Add(p.ID, numFreighters);
			}
			List<Ship> freighterShips = new List<Ship>();
			List<Ship> qShips = new List<Ship>();
			foreach (Player player in this._playersInCombat)
			{
				if (playerFreighters[player.ID].Count != 0)
				{
					Vector3 baseOrigin = Vector3.Zero;
					float centerOffset = 0.0f;
					if (playerCivStation[player.ID] != null)
					{
						Vector3 position = this.App.GameDatabase.GetOrbitalTransform(playerCivStation[player.ID].OrbitalObjectID).Position;
						float num = float.MaxValue;
						foreach (OrbitalObjectInfo orbitalObjectInfo in objects)
						{
							if (orbitalObjectInfo.ID != playerCivStation[player.ID].OrbitalObjectID)
							{
								float lengthSquared = (this.App.GameDatabase.GetOrbitalTransform(orbitalObjectInfo.ID).Position - position).LengthSquared;
								if ((double)lengthSquared < (double)num)
								{
									num = lengthSquared;
									centerOffset = lengthSquared;
									baseOrigin = position;
								}
							}
						}
						centerOffset = (float)Math.Sqrt((double)centerOffset) + 500f;
					}
					else
					{
						double num = 0.0;
						foreach (OrbitalObjectInfo orbitalObjectInfo in objects)
						{
							PlanetInfo planetInfo = this.App.GameDatabase.GetPlanetInfo(orbitalObjectInfo.ID);
							if (planetInfo != null)
							{
								ColonyInfo colonyInfoForPlanet = this.App.GameDatabase.GetColonyInfoForPlanet(orbitalObjectInfo.ID);
								if (colonyInfoForPlanet != null)
								{
									double totalPopulation = this.App.GameDatabase.GetTotalPopulation(colonyInfoForPlanet);
									if (totalPopulation > num)
									{
										num = totalPopulation;
										baseOrigin = this.App.GameDatabase.GetOrbitalTransform(orbitalObjectInfo.ID).Position;
										centerOffset = StarSystemVars.Instance.SizeToRadius(planetInfo.Size) + 750f;
									}
								}
							}
						}
					}
					bool hasConvoySystems = playerTechs[player.ID].Any<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == "CCC_Convoy_Systems"));
					int maxNumNodes = hasConvoySystems ? 2 : 4;
					maxNumNodes = Math.Min(maxNumNodes, this._starSystemObjects.NeighboringSystems.Count);
					List<NeighboringSystemInfo> closestSystems = new List<NeighboringSystemInfo>();
					for (int index = 0; index < maxNumNodes; ++index)
					{
						NeighboringSystemInfo neighboringSystemInfo = (NeighboringSystemInfo)null;
						float num = float.MaxValue;
						foreach (NeighboringSystemInfo neighboringSystem in this._starSystemObjects.NeighboringSystems)
						{
							if (!closestSystems.Contains(neighboringSystem))
							{
								float lengthSquared = (neighboringSystem.BaseOffsetLocation - baseOrigin).LengthSquared;
								if ((double)lengthSquared < (double)num)
								{
									neighboringSystemInfo = neighboringSystem;
									num = lengthSquared;
								}
							}
						}
						if (neighboringSystemInfo != null)
							closestSystems.Add(neighboringSystemInfo);
					}
					int remainingFreighters = numPlayerFreighters[player.ID];
					foreach (FreighterInfo freighterInfo in playerFreighters[player.ID])
					{
						ShipInfo si = this.App.GameDatabase.GetShipInfo(freighterInfo.ShipId, false);
						if (si != null)
						{
							Vector3 spawnPos = this.GetValidFreighterSpawnPosition(populatedWorlds, planets, stars, closestSystems, baseOrigin, centerOffset, furthestOffset, hasConvoySystems);
							Vector3 spawnFacing = Vector3.Normalize(-spawnPos);
							Matrix spawnTrans = Matrix.CreateWorld(spawnPos, spawnFacing, Vector3.UnitY);
							Ship ship = Ship.CreateShip(this.App.Game, spawnTrans, si, 0, this._input.ObjectID, player.ObjectID, this._starSystemObjects.IsDeepSpace, (IEnumerable<Player>)null);
							NeighboringSystemInfo retreatSystem = (NeighboringSystemInfo)null;
							if (closestSystems.Count > 0)
							{
								float num = float.MaxValue;
								foreach (NeighboringSystemInfo neighboringSystemInfo in closestSystems)
								{
									float lengthSquared = (spawnPos - neighboringSystemInfo.BaseOffsetLocation).LengthSquared;
									if ((double)lengthSquared < (double)num)
									{
										num = lengthSquared;
										retreatSystem = neighboringSystemInfo;
									}
								}
							}
							ship.Maneuvering.RetreatData = this.GetRetreatData(freighterInfo.PlayerId, retreatSystem != null ? retreatSystem.SystemID : 0, spawnPos);
							crits.Add((IGameObject)ship);
							yield return ship.ObjectID;
							if (this._pirateEncounterData.PlayerFreightersInSystem.ContainsKey(player.ID))
								this._pirateEncounterData.PlayerFreightersInSystem[player.ID].Add(new CommonCombatState.PiracyFreighterInfo()
								{
									ShipID = si.ID,
									FreighterID = freighterInfo.ID
								});
							else
								this._pirateEncounterData.PlayerFreightersInSystem.Add(player.ID, new List<CommonCombatState.PiracyFreighterInfo>()
				{
				  new CommonCombatState.PiracyFreighterInfo()
				  {
					ShipID = si.ID,
					FreighterID = freighterInfo.ID
				  }
				});
							if (ship.IsQShip)
								qShips.Add(ship);
							else
								freighterShips.Add(ship);
							--remainingFreighters;
							if (remainingFreighters <= 0)
								break;
						}
					}
				}
			}
			if (freighterShips.Count<Ship>() != 0 || qShips.Count<Ship>() != 0)
			{
				if (qShips.Count > 0 && freighterShips.Count == 0)
				{
					freighterShips.AddRange((IEnumerable<Ship>)qShips);
					qShips.Clear();
				}
				int freighterIdx = 0;
				if (freighterShips.Count > 0)
				{
					foreach (Ship ship in this._pirateEncounterData.PoliceShipsInSystem)
					{
						Vector3 position1 = freighterShips[freighterIdx].Position;
						float num = (float)this._random.NextInclusive(0.0, 2.0 * Math.PI);
						Vector3 position2 = new Vector3()
						{
							X = (float)Math.Sin((double)num) * 1000f,
							Z = (float)-Math.Cos((double)num) * 1000f,
							Y = 0.0f
						} + position1;
						Matrix world = Matrix.CreateWorld(position2, Vector3.Normalize(position1 - position2), Vector3.UnitY);
						ship.InitialSetPos(world.Position, world.EulerAngles);
						freighterIdx = (freighterIdx + 1) % freighterShips.Count;
					}
				}
				Dictionary<Ship, List<Ship>> nearByFreighters = new Dictionary<Ship, List<Ship>>();
				Dictionary<Ship, List<Ship>> policeNearGroup = new Dictionary<Ship, List<Ship>>();
				foreach (Ship key in freighterShips)
				{
					if (!nearByFreighters.ContainsKey(key))
					{
						bool flag = false;
						foreach (KeyValuePair<Ship, List<Ship>> keyValuePair in nearByFreighters)
						{
							if (keyValuePair.Value.Contains(key))
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							nearByFreighters.Add(key, new List<Ship>());
							foreach (Ship ship in freighterShips)
							{
								if (ship != key && (double)(ship.Position - key.Position).LengthSquared < 9000000.0)
									nearByFreighters[key].Add(ship);
							}
						}
					}
				}
				int freighterPlayer = 0;
				int bestWeight = int.MinValue;
				Vector3 bestCenter = new Vector3();
				Vector3? centerRetreatPos = new Vector3?();
				foreach (KeyValuePair<Ship, List<Ship>> keyValuePair in nearByFreighters)
				{
					int num1 = 1 + keyValuePair.Value.Count;
					Vector3 position = keyValuePair.Key.Position;
					foreach (Ship ship in keyValuePair.Value)
						position += ship.Position;
					Vector3 vector3 = position / (float)num1;
					policeNearGroup.Add(keyValuePair.Key, new List<Ship>());
					int num2 = 0;
					foreach (Ship ship in this._pirateEncounterData.PoliceShipsInSystem)
					{
						if ((double)(vector3 - ship.Position).LengthSquared < 9000000.0)
						{
							policeNearGroup[keyValuePair.Key].Add(ship);
							++num2;
						}
					}
					int num3 = num1 - num2;
					if (num3 > bestWeight)
					{
						bestWeight = num3;
						bestCenter = vector3;
						freighterPlayer = keyValuePair.Key.Player.ID;
						centerRetreatPos = new Vector3?(keyValuePair.Key.Maneuvering.RetreatDestination);
					}
				}
				Vector3 retreatPos = centerRetreatPos.HasValue ? centerRetreatPos.Value : Vector3.Normalize(bestCenter) * furthestOffset;
				foreach (Ship ship in qShips)
				{
					ship.InitialSetPos(this.GetValidNearFreighterSpawnPosition(planets, stars, bestCenter, 750f, 2000f, furthestOffset), Vector3.Zero);
					ship.Maneuvering.RetreatDestination = retreatPos;
				}
				Vector3 originalPirateSpawnPos = new Vector3();
				foreach (SpawnProfile spawnPosition in this._spawnPositions)
				{
					if (this._pirateEncounterData.PiratePlayerIDs.Contains(spawnPosition._playerID))
					{
						originalPirateSpawnPos = spawnPosition._spawnPosition;
						spawnPosition._spawnPosition = this.GetValidNearFreighterSpawnPosition(planets, stars, bestCenter, 5000f, 5000f, furthestOffset);
						spawnPosition._spawnFacing = Vector3.Normalize(bestCenter - spawnPosition._spawnPosition);
						spawnPosition._startingPosition = spawnPosition._spawnPosition;
						int systemID = 0;
						FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(spawnPosition._fleetID);
						if (fleetInfo != null && fleetInfo.PreviousSystemID.HasValue)
							systemID = fleetInfo.PreviousSystemID.Value;
						RetreatData retreatData = this.GetRetreatData(spawnPosition._playerID, systemID, spawnPosition._spawnPosition);
						spawnPosition._retreatPosition = retreatData.DefaultDestination;
						foreach (KeyValuePair<int, List<Ship>> keyValuePair in this._pirateEncounterData.PirateShipsInSystem)
						{
							if (keyValuePair.Key == spawnPosition._playerID)
							{
								foreach (Ship ship in keyValuePair.Value)
								{
									Matrix world = Matrix.CreateWorld(ship.Position - originalPirateSpawnPos + spawnPosition._spawnPosition, spawnPosition._spawnFacing, Vector3.UnitY);
									ship.InitialSetPos(world.Position, world.EulerAngles);
									ship.Maneuvering.RetreatData = retreatData;
								}
							}
						}
					}
				}
				SpawnProfile freighterFleet = this._spawnPositions.FirstOrDefault<SpawnProfile>((Func<SpawnProfile, bool>)(x => x._playerID == freighterPlayer));
				if (freighterFleet != null)
				{
					FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(freighterFleet._fleetID);
					if (fleetInfo != null)
					{
						AdmiralInfo admiralInfo = this.App.GameDatabase.GetAdmiralInfo(fleetInfo.AdmiralID);
						List<AdmiralInfo.TraitType> list = this.App.GameDatabase.GetAdmiralTraits(fleetInfo.AdmiralID).ToList<AdmiralInfo.TraitType>();
						if (admiralInfo != null && this._random.CoinToss((list.Contains(AdmiralInfo.TraitType.Vigilant) ? 2 : 1) * admiralInfo.ReactionBonus))
						{
							freighterFleet._startingPosition = this.GetValidNearFreighterSpawnPosition(planets, stars, bestCenter, 3000f, 3000f, furthestOffset);
							freighterFleet._spawnPosition = freighterFleet._startingPosition;
							freighterFleet._spawnFacing = Vector3.Normalize(bestCenter - freighterFleet._startingPosition);
							freighterFleet._retreatPosition = this.GetRetreatData(fleetInfo.PlayerID, fleetInfo.PreviousSystemID.HasValue ? fleetInfo.PreviousSystemID.Value : 0, freighterFleet._startingPosition).DefaultDestination;
							Matrix world1 = Matrix.CreateWorld(freighterFleet._spawnPosition, freighterFleet._spawnFacing, Vector3.UnitY);
							int num = 0;
							Vector3 position = freighterFleet._startingPosition;
							Vector3 vector3 = new Vector3(freighterFleet._spawnFacing.Z, 0.0f, freighterFleet._spawnFacing.X * -1f) * 600f;
							foreach (int activeShip in freighterFleet._activeShips)
							{
								if (this._ships.Forward.ContainsKey(activeShip))
								{
									Ship ship = this._ships.Forward[activeShip];
									Vector3? shipFleetPosition = this.App.GameDatabase.GetShipFleetPosition(ship.DatabaseID);
									if (!ShipSectionAsset.IsBattleRiderClass(ship.RealShipClass) || ship.RiderIndex < 0)
									{
										if (shipFleetPosition.HasValue)
											position = Vector3.Transform(shipFleetPosition.Value, world1);
										else if (num > 0)
											position += vector3;
										Matrix world2 = Matrix.CreateWorld(position, freighterFleet._spawnFacing, Vector3.UnitY);
										ship.InitialSetPos(world2.Position, world2.EulerAngles);
									}
								}
								++num;
							}
						}
					}
				}
			}
		}

		private Vector3 GetValidFreighterSpawnPosition(
		  List<StellarBody> populatedPlanets,
		  List<StellarBody> planets,
		  List<StarModel> stars,
		  List<NeighboringSystemInfo> closestSystems,
		  Vector3 baseCenter,
		  float baseOffset,
		  float furthestOffset,
		  bool hasConvoySystems)
		{
			float num1 = hasConvoySystems ? 20000f : 15000f;
			Vector3 vector3_1 = Vector3.Zero;
			bool flag = false;
			while (!flag)
			{
				flag = true;
				if (closestSystems.Count > 0)
				{
					int index = this._random.NextInclusive(0, closestSystems.Count - 1);
					Vector3 vector3_2 = closestSystems[index].BaseOffsetLocation - baseCenter;
					float num2 = vector3_2.Normalize();
					float num3 = !hasConvoySystems ? this._random.NextInclusive(num1, num2) : this._random.NextInclusive(baseOffset, Math.Min(num1, num2));
					vector3_1 = baseCenter + vector3_2 * num3;
				}
				else if (hasConvoySystems)
				{
					float num2 = (float)this._random.NextInclusive(0.0, 2.0 * Math.PI);
					float num3 = this._random.NextInclusive(0.0f, num1);
					int index = this._random.NextInclusive(0, populatedPlanets.Count - 1);
					vector3_1.X = (float)Math.Sin((double)num2) * num3;
					vector3_1.Z = (float)-Math.Cos((double)num2) * num3;
					vector3_1.Y = 0.0f;
					vector3_1 += populatedPlanets[index].Parameters.Position;
				}
				else
				{
					float num2 = (float)this._random.NextInclusive(0.0, 2.0 * Math.PI);
					float num3 = this._random.NextInclusive(0.0f, furthestOffset);
					vector3_1.X = (float)Math.Sin((double)num2) * num3;
					vector3_1.Z = (float)-Math.Cos((double)num2) * num3;
					vector3_1.Y = 0.0f;
				}
				if ((double)vector3_1.LengthSquared > (double)furthestOffset * (double)furthestOffset)
					flag = false;
				if (flag)
				{
					foreach (StarModel star in stars)
					{
						float num2 = star.Radius + 7500f;
						if ((double)(vector3_1 - star.Position).LengthSquared < (double)num2 * (double)num2)
						{
							flag = false;
							break;
						}
					}
				}
				if (flag)
				{
					foreach (StellarBody planet in planets)
					{
						float num2 = planet.Parameters.Radius + 750f;
						if (planet.Population > 0.0 && !hasConvoySystems)
							num2 += num1;
						if ((double)(vector3_1 - planet.Parameters.Position).LengthSquared < (double)num2 * (double)num2)
						{
							flag = false;
							break;
						}
					}
				}
			}
			return vector3_1;
		}

		private Vector3 GetValidNearFreighterSpawnPosition(
		  List<StellarBody> planets,
		  List<StarModel> stars,
		  Vector3 pirateAttackCenter,
		  float minOffset,
		  float maxOffset,
		  float systemBoundary)
		{
			Vector3 zero = Vector3.Zero;
			bool flag = false;
			while (!flag)
			{
				flag = true;
				float num1 = (float)this._random.NextInclusive(0.0, 2.0 * Math.PI);
				float num2 = this._random.NextInclusive(minOffset, maxOffset);
				zero.X = (float)Math.Sin((double)num1) * num2;
				zero.Z = (float)-Math.Cos((double)num1) * num2;
				zero.Y = 0.0f;
				zero += pirateAttackCenter;
				if ((double)zero.LengthSquared > (double)systemBoundary * (double)systemBoundary)
					flag = false;
				if (flag)
				{
					foreach (StarModel star in stars)
					{
						float num3 = star.Radius + 7500f;
						if ((double)(zero - star.Position).LengthSquared < (double)num3 * (double)num3)
						{
							flag = false;
							break;
						}
					}
				}
				if (flag)
				{
					foreach (StellarBody planet in planets)
					{
						float num3 = planet.Parameters.Radius + 750f;
						if ((double)(zero - planet.Parameters.Position).LengthSquared < (double)num3 * (double)num3)
						{
							flag = false;
							break;
						}
					}
				}
			}
			return zero;
		}

		private IEnumerable<int> SpawnFleet(
		  GameObjectSet crits,
		  FleetInfo fleet,
		  List<CombatEasterEggData> eeData,
		  List<CombatRandomData> randData,
		  IDictionary<int, List<FleetInfo>> selectedPlayerFleets,
		  OrbitalObjectInfo[] objects,
		  int spawnedFleetCount)
		{
			List<Ship> userShips = new List<Ship>();
			List<FleetFormationCreationData> lpFleetFormation = new List<FleetFormationCreationData>();
			bool allowPoliceInCombat = this.App.GetStratModifier<bool>(StratModifiers.AllowPoliceInCombat, fleet.PlayerID);
			SpawnProfile spawnPos = this.GetSpawnProfileForFleet(fleet.ID, selectedPlayerFleets, eeData, randData, ref spawnedFleetCount);
			Matrix spawnMatrix = Matrix.CreateWorld(spawnPos._startingPosition, spawnPos._spawnFacing, Vector3.UnitY);
			RetreatData rd = this.GetRetreatData(fleet.PlayerID, fleet.PreviousSystemID.HasValue ? fleet.PreviousSystemID.Value : 0, spawnPos._startingPosition);
			spawnPos._retreatPosition = rd.DefaultDestination;
			Vector3 spawn = spawnPos._startingPosition;
			Vector3 shipOffset = new Vector3(spawnPos._spawnFacing.Z, 0.0f, spawnPos._spawnFacing.X * -1f) * 600f;
			FleetFormationCreationData newFleetFormation = new FleetFormationCreationData();
			newFleetFormation.FormationData = new List<FormationCreationData>();
			List<ShipInfo> shipInfoList = new List<ShipInfo>();
			List<ShipInfo> shiprows = this.App.GameDatabase.GetShipInfoByFleetID(fleet.ID, false).ToList<ShipInfo>();
			Dictionary<int, bool> hasSystemPos = new Dictionary<int, bool>();
			foreach (ShipInfo shipInfo1 in shiprows.ToList<ShipInfo>())
			{
				foreach (ShipInfo shipInfo2 in this.App.GameDatabase.GetBattleRidersByParentID(shipInfo1.ID))
				{
					ShipInfo rider = shipInfo2;
					if (!shiprows.Any<ShipInfo>((Func<ShipInfo, bool>)(x => x.ID == rider.ID)))
						shiprows.Add(rider);
				}
			}
			Player player = this.App.GetPlayer(fleet.PlayerID);
			int playerGOID = player != null ? player.ObjectID : 0;
			bool isLocalPlayer = false;
			int shipIndex = 0;
			bool hasFleetPosition = false;
			foreach (ShipInfo shipInfo in shiprows)
			{
				bool isPoliceShip = false;
				IEnumerable<string> sections = this.App.GameDatabase.GetDesignSectionNames(shipInfo.DesignID);
				RealShipClasses realShipClass = RealShipClasses.Cruiser;
				SectionEnumerations.CombatAiType combatAI = SectionEnumerations.CombatAiType.Normal;
				foreach (string str in sections)
				{
					string s = str;
					ShipSectionAsset shipSectionAsset = this.App.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == s));
					if (shipSectionAsset != null)
					{
						if (shipSectionAsset.isPolice)
							isPoliceShip = true;
						if (shipSectionAsset.Type == ShipSectionType.Mission)
						{
							combatAI = shipSectionAsset.CombatAIType;
							int num1 = (int)shipSectionAsset.Class;
							realShipClass = shipSectionAsset.RealClass;
							int num2 = shipSectionAsset.isMineLayer ? 1 : 0;
						}
					}
				}
				if ((!ShipSectionAsset.IsBattleRiderClass(realShipClass) || shipInfo.RiderIndex >= 0 || fleet.IsTrapFleet) && (fleet.Type == FleetType.FL_DEFENSE || !shipInfo.DesignInfo.IsSDB() && !shipInfo.DesignInfo.IsPlatform()) && !shipInfo.DesignInfo.IsLoaCube())
				{
					Matrix fullTrans = Matrix.Identity;
					Vector3? fleetPos = this.App.GameDatabase.GetShipFleetPosition(shipInfo.ID);
					hasFleetPosition = hasFleetPosition || fleetPos.HasValue;
					if (!hasSystemPos.ContainsKey(shipInfo.ID))
						hasSystemPos.Add(shipInfo.ID, false);
					switch (combatAI)
					{
						case SectionEnumerations.CombatAiType.SystemKiller:
							Matrix? previousShipTransform = this.App.Game.MCCarryOverData.GetPreviousShipTransform(this._systemId, fleet.ID, shipInfo.ID);
							fullTrans = !previousShipTransform.HasValue ? SystemKiller.GetSpawnTransform(this.App, fleet.ID, this._starSystemObjects, objects) : previousShipTransform.Value;
							break;
						case SectionEnumerations.CombatAiType.MorrigiRelic:
							Matrix? shipSystemPosition1 = this.App.GameDatabase.GetShipSystemPosition(shipInfo.ID);
							if (!shipSystemPosition1.HasValue)
							{
								fullTrans = MorrigiRelic.GetSpawnTransform(this.App, this._random, shipIndex, objects);
								this.App.GameDatabase.UpdateShipSystemPosition(shipInfo.ID, new Matrix?(fullTrans));
								break;
							}
							fullTrans = shipSystemPosition1.Value;
							break;
						case SectionEnumerations.CombatAiType.Meteor:
							Matrix? shipSystemPosition2 = this.App.GameDatabase.GetShipSystemPosition(shipInfo.ID);
							fullTrans = shipSystemPosition2.HasValue ? shipSystemPosition2.Value : MeteorShower.GetSpawnTransform(this.App, this._systemId, 0);
							break;
						case SectionEnumerations.CombatAiType.Comet:
							fullTrans = Comet.GetSpawnTransform(this.App, this._starSystemObjects, objects);
							break;
						case SectionEnumerations.CombatAiType.Gardener:
						case SectionEnumerations.CombatAiType.Protean:
							fullTrans = Gardeners.GetSpawnTransform(this.App, shipInfo.ID, fleet.ID, shipIndex, this._systemId, objects);
							break;
						case SectionEnumerations.CombatAiType.CommandMonitor:
						case SectionEnumerations.CombatAiType.NormalMonitor:
							Matrix? shipSystemPosition3 = this.App.GameDatabase.GetShipSystemPosition(shipInfo.ID);
							if (!shipSystemPosition3.HasValue)
							{
								fullTrans = AsteroidMonitor.GetSpawnTransform(this.App, shipInfo.DesignID, shipIndex, shiprows.Count, this._systemId);
								this.App.GameDatabase.UpdateShipSystemPosition(shipInfo.ID, new Matrix?(fullTrans));
								break;
							}
							fullTrans = shipSystemPosition3.Value;
							break;
						default:
							if (!ShipSectionAsset.IsBattleRiderClass(realShipClass) || shipInfo.RiderIndex < 0)
							{
								if (fleetPos.HasValue)
									spawn = Vector3.Transform(fleetPos.Value, spawnMatrix);
								else if (shipIndex > 0)
									spawn += shipOffset;
							}
							spawn = CommonCombatState.ApplyOriginShift(this.Origin, spawn);
							fullTrans = Matrix.CreateWorld(spawn, spawnPos._spawnFacing, Vector3.UnitY);
							Matrix? nullable = this.App.GameDatabase.GetShipSystemPosition(shipInfo.ID);
							if (nullable.HasValue && !this._trapCombatData.ColonyTrappedFleets.Contains(fleet))
								fullTrans = nullable.Value;
							hasSystemPos[shipInfo.ID] = hasSystemPos[shipInfo.ID] || nullable.HasValue;
							nullable = this.App.Game.MCCarryOverData.GetPreviousShipTransform(this._systemId, fleet.ID, shipInfo.ID);
							if (nullable.HasValue)
								fullTrans = nullable.Value;
							hasSystemPos[shipInfo.ID] = hasSystemPos[shipInfo.ID] || nullable.HasValue;
							break;
					}
					Ship ship = Ship.CreateShip(this.App.Game, fullTrans, shipInfo, 0, this._input.ObjectID, playerGOID, this._starSystemObjects.IsDeepSpace, (IEnumerable<Player>)this._playersInCombat);
					ship.ParentDatabaseID = shipInfo.ParentID;
					ship.Maneuvering.RetreatData = rd;
					crits.Add((IGameObject)ship);
					if (this._pirateEncounterData.IsPirateEncounter)
					{
						if (isPoliceShip)
							this._pirateEncounterData.PoliceShipsInSystem.Add(ship);
						if (fleet.Type == FleetType.FL_NORMAL && this._pirateEncounterData.PiratePlayerIDs.Contains(fleet.PlayerID))
						{
							if (this._pirateEncounterData.PirateShipsInSystem.ContainsKey(fleet.PlayerID))
								this._pirateEncounterData.PirateShipsInSystem[fleet.PlayerID].Add(ship);
							else
								this._pirateEncounterData.PirateShipsInSystem.Add(fleet.PlayerID, new List<Ship>()
				{
				  ship
				});
							if (fleet.PlayerID != this.App.Game.ScriptModules.Pirates.PlayerID)
							{
								Player playerObject = this.App.Game.GetPlayerObject(this.App.Game.ScriptModules.Pirates.PlayerID);
								if (playerObject != null)
									ship.PostSetProp("SetPiracyPlayer", playerObject.ObjectID);
							}
						}
					}
					yield return ship.ObjectID;
					if (!isPoliceShip || allowPoliceInCombat)
						this._ships.Insert(shipInfo.ID, ship);
					if (ship.Player == this.App.LocalPlayer)
					{
						if (fleetPos.HasValue)
						{
							newFleetFormation.FormationData.Add(new FormationCreationData()
							{
								ShipID = ship.ObjectID,
								DesignID = shipInfo.DesignID,
								ShipRole = ship.ShipRole,
								ShipClass = ship.ShipClass,
								FormationPosition = fleetPos.Value
							});
							isLocalPlayer = true;
						}
						if (ship.ShipClass != ShipClass.BattleRider && ship.ShipClass != ShipClass.Station)
							userShips.Add(ship);
					}
					++shipIndex;
				}
			}
			if (!hasFleetPosition && player.Faction.Name == "loa")
			{
				foreach (FormationPatternData formationPatternData in FormationPatternCreator.CreateCubeFormationPattern(userShips, spawnMatrix))
				{
					if (formationPatternData.Ship != null)
					{
						newFleetFormation.FormationData.Add(new FormationCreationData()
						{
							ShipID = formationPatternData.Ship.ObjectID,
							DesignID = formationPatternData.Ship.DesignID,
							ShipRole = formationPatternData.Ship.ShipRole,
							ShipClass = formationPatternData.Ship.ShipClass,
							FormationPosition = formationPatternData.Position
						});
						if (!hasSystemPos[formationPatternData.Ship.DatabaseID])
						{
							Matrix world = Matrix.CreateWorld(Vector3.Transform(formationPatternData.Position, spawnMatrix), spawnPos._spawnFacing, Vector3.UnitY);
							formationPatternData.Ship.InitialSetPos(world.Position, world.EulerAngles);
						}
					}
				}
				if (player == this.App.LocalPlayer)
					isLocalPlayer = true;
			}
			if (isLocalPlayer)
				lpFleetFormation.Add(newFleetFormation);
			if (isLocalPlayer)
			{
				this.SetSelectionToGroup(userShips);
				this.PostNewPlayerFormation(lpFleetFormation);
			}
		}

		protected static void Trace(string message)
		{
			App.Log.Trace(message, "combat");
		}

		protected static void Warn(string message)
		{
			App.Log.Warn(message, "combat");
		}

		private IEnumerable<int> SpawnDefenseFleet(GameObjectSet crits, FleetInfo fleet)
		{
			if (fleet != null && fleet.Type == FleetType.FL_DEFENSE)
			{
				bool allowPoliceInCombat = this.App.GetStratModifier<bool>(StratModifiers.AllowPoliceInCombat, fleet.PlayerID);
				List<ShipInfo> shiprows = this.App.GameDatabase.GetShipInfoByFleetID(fleet.ID, false).ToList<ShipInfo>();
				foreach (ShipInfo shipInfo1 in shiprows.ToList<ShipInfo>())
				{
					foreach (ShipInfo shipInfo2 in this.App.GameDatabase.GetBattleRidersByParentID(shipInfo1.ID))
					{
						ShipInfo rider = shipInfo2;
						if (!shiprows.Any<ShipInfo>((Func<ShipInfo, bool>)(x => x.ID == rider.ID)))
							shiprows.Add(rider);
					}
				}
				List<object> parms = new List<object>();
				Player player = this.App.GetPlayer(fleet.PlayerID);
				int playerGOID = player != null ? player.ObjectID : 0;
				foreach (ShipInfo shipInfo in shiprows)
				{
					Matrix? trans = shipInfo.RiderIndex >= 0 ? this.App.GameDatabase.GetShipSystemPosition(shipInfo.ParentID) : this.App.GameDatabase.GetShipSystemPosition(shipInfo.ID);
					if (!trans.HasValue)
					{
						CommonCombatState.Warn("Ship [" + shipInfo.ShipName + "] is in a [" + fleet.Type.ToString() + "] fleet failed to create valid transform, therefore it will not be spawned");
					}
					else
					{
						bool isPoliceShip = false;
						bool isMineLayer = false;
						IEnumerable<string> sections = this.App.GameDatabase.GetDesignSectionNames(shipInfo.DesignID);
						foreach (string str in sections)
						{
							string s = str;
							ShipSectionAsset shipSectionAsset = this.App.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == s));
							if (shipSectionAsset != null)
							{
								if (shipSectionAsset.isPolice)
									isPoliceShip = true;
								if (shipSectionAsset.Type == ShipSectionType.Mission)
								{
									int combatAiType = (int)shipSectionAsset.CombatAIType;
									int num = (int)shipSectionAsset.Class;
									int realClass = (int)shipSectionAsset.RealClass;
									isMineLayer = shipSectionAsset.isMineLayer;
								}
							}
						}
						Matrix fullTrans = trans.Value;
						if (fleet.Type == FleetType.FL_DEFENSE && isMineLayer)
						{
							DesignInfo design = this.App.GameDatabase.GetDesignInfo(shipInfo.DesignID);
							if (design != null)
							{
								DesignSectionInfo mission = ((IEnumerable<DesignSectionInfo>)design.DesignSections).FirstOrDefault<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(x => x.ShipSectionAsset.Type == ShipSectionType.Mission));
								if (mission != null)
								{
									ShipSectionAsset section = this.App.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == mission.FilePath));
									foreach (LogicalBank bank in section.Banks)
									{
										LogicalBank lb = bank;
										if (lb.TurretClass == WeaponEnums.TurretClasses.Minelayer)
										{
											WeaponBankInfo wbi = mission.WeaponBanks.FirstOrDefault<WeaponBankInfo>((Func<WeaponBankInfo, bool>)(x => x.BankGUID == lb.GUID));
											if (wbi != null && wbi.WeaponID.HasValue)
											{
												string weaponName = Path.GetFileNameWithoutExtension(this.App.GameDatabase.GetWeaponAsset(wbi.WeaponID.Value));
												LogicalWeapon weapon = this.App.AssetDatabase.Weapons.First<LogicalWeapon>((Func<LogicalWeapon, bool>)(w => string.Equals(w.WeaponName, weaponName, StringComparison.InvariantCultureIgnoreCase)));
												LogicalWeapon subWeapon = !string.IsNullOrEmpty(weapon.SubWeapon) ? this.App.AssetDatabase.Weapons.First<LogicalWeapon>((Func<LogicalWeapon, bool>)(w => string.Equals(w.WeaponName, weapon.SubWeapon, StringComparison.InvariantCultureIgnoreCase))) : (LogicalWeapon)null;
												weapon.AddGameObjectReference();
												subWeapon?.AddGameObjectReference();
												Faction faction = this.App.AssetDatabase.Factions.First<Faction>((Func<Faction, bool>)(x => mission.ShipSectionAsset.Faction == x.Name));
												Player desPlayer = this.App.Game.GetPlayerObject(design.PlayerID);
												Subfaction subfaction = (Subfaction)null;
												string preferredMount = "";
												if (desPlayer != null)
												{
													subfaction = faction.Subfactions[Math.Min(desPlayer.SubfactionIndex, faction.Subfactions.Length - 1)];
													preferredMount = this.App.LocalPlayer != desPlayer || !subfaction.DlcID.HasValue || this.App.Steam.HasDLC((int)subfaction.DlcID.Value) ? subfaction.MountName : faction.Subfactions[0].MountName;
												}
												MountObject.WeaponModels weaponModels = new MountObject.WeaponModels();
												weaponModels.FillOutModelFilesWithWeapon(weapon, faction, preferredMount, this.App.AssetDatabase.Weapons);
												IGameObject mineField = this.App.AddObject(InteropGameObjectType.IGOT_MINEFIELD, new List<object>()
						{
						  (object) shipInfo.ID,
						  (object) fullTrans.Position,
						  (object) Vector3.Normalize(fullTrans.Forward),
						  (object) this.App.AssetDatabase.MineFieldParams.Width,
						  (object) this.App.AssetDatabase.MineFieldParams.Length,
						  (object) this.App.AssetDatabase.MineFieldParams.Height,
						  (object) this.App.AssetDatabase.MineFieldParams.SpacingOffset,
						  (object) weapon.GameObject.ObjectID,
						  (object) (subWeapon != null ? subWeapon.GameObject.ObjectID : 0),
						  (object) (player != null ? player.ObjectID : 0),
						  (object) weaponModels.WeaponModelPath.ModelPath,
						  (object) weaponModels.SubWeaponModelPath.ModelPath
						}.ToArray());
												crits.Add(mineField);
												yield return mineField.ObjectID;
												break;
											}
											break;
										}
									}
								}
							}
						}
						else
						{
							Ship ship = Ship.CreateShip(this.App.Game, fullTrans, shipInfo, 0, this._input.ObjectID, playerGOID, this._starSystemObjects.IsDeepSpace, (IEnumerable<Player>)this._playersInCombat);
							ship.ParentDatabaseID = shipInfo.ParentID;
							CombatZonePositionInfo endRadius = this._starSystemObjects.CombatZones.Last<CombatZonePositionInfo>();
							ship.Maneuvering.RetreatDestination = Vector3.Normalize(fullTrans.Position) * endRadius.RadiusUpper;
							crits.Add((IGameObject)ship);
							if (this._pirateEncounterData.IsPirateEncounter && isPoliceShip)
								this._pirateEncounterData.PoliceShipsInSystem.Add(ship);
							SDBInfo info = this.App.GameDatabase.GetSDBInfoFromShip(shipInfo.ID);
							if (info != null)
							{
								parms.Add((object)ship.ObjectID);
								parms.Add((object)info.OrbitalId);
							}
							yield return ship.ObjectID;
							if (!isPoliceShip || allowPoliceInCombat)
								this._ships.Insert(shipInfo.ID, ship);
						}
					}
				}
				parms.Insert(0, (object)(parms.Count / 2));
				if (player == this.App.LocalPlayer)
					this._starSystemObjects.PostSetProp("SetSDBSlotValuesCombat", parms.ToArray());
			}
		}

		private IEnumerable<int> SpawnAcceleratorFleet(GameObjectSet crits, FleetInfo fleet)
		{
			if (fleet != null && fleet.Type == FleetType.FL_ACCELERATOR)
			{
				Player player = this.App.GetPlayer(fleet.PlayerID);
				int playerGOID = player != null ? player.ObjectID : 0;
				List<ShipInfo> shiprows = this.App.GameDatabase.GetShipInfoByFleetID(fleet.ID, false).ToList<ShipInfo>();
				foreach (ShipInfo shipInfo in shiprows)
				{
					Matrix fullTrans = Matrix.Identity;
					Matrix? trans = this.App.GameDatabase.GetShipSystemPosition(shipInfo.ID);
					if (!trans.HasValue)
					{
						StarSystemInfo starSystemInfo = this.App.GameDatabase.GetStarSystemInfo(this._systemId);
						if (starSystemInfo != (StarSystemInfo)null && !starSystemInfo.IsDeepSpace)
						{
							trans = GameSession.GetValidGateShipTransform(this.App.Game, this._systemId, fleet.ID);
							if (trans.HasValue)
								this.App.GameDatabase.UpdateShipSystemPosition(shipInfo.ID, new Matrix?(trans.Value));
							CommonCombatState.Warn("Loa Gate [" + shipInfo.ShipName + "] is in a [" + fleet.Type.ToString() + "] fleet and doesnt not have a valid transform, attempt to create new transform");
						}
					}
					if (trans.HasValue)
						fullTrans = trans.Value;
					Ship ship = Ship.CreateShip(this.App.Game, fullTrans, shipInfo, 0, this._input.ObjectID, playerGOID, this._starSystemObjects.IsDeepSpace, (IEnumerable<Player>)this._playersInCombat);
					ship.ParentDatabaseID = shipInfo.ParentID;
					crits.Add((IGameObject)ship);
					yield return ship.ObjectID;
					this._ships.Insert(shipInfo.ID, ship);
				}
			}
		}

		private IEnumerable<int> SpawnGateFleet(GameObjectSet crits, FleetInfo fleet)
		{
			if (fleet != null && fleet.Type == FleetType.FL_GATE)
			{
				Player player = this.App.GetPlayer(fleet.PlayerID);
				int playerGOID = player != null ? player.ObjectID : 0;
				List<ShipInfo> shiprows = this.App.GameDatabase.GetShipInfoByFleetID(fleet.ID, false).ToList<ShipInfo>();
				foreach (ShipInfo shipInfo in shiprows)
				{
					Matrix? trans = this.App.GameDatabase.GetShipSystemPosition(shipInfo.ID);
					if (!trans.HasValue && (fleet.Type == FleetType.FL_DEFENSE || fleet.Type == FleetType.FL_GATE))
					{
						if (fleet.Type == FleetType.FL_GATE)
						{
							trans = GameSession.GetValidGateShipTransform(this.App.Game, this._systemId, fleet.ID);
							if (trans.HasValue)
								this.App.GameDatabase.UpdateShipSystemPosition(shipInfo.ID, new Matrix?(trans.Value));
							CommonCombatState.Warn("Ship [" + shipInfo.ShipName + "] is in a [" + fleet.Type.ToString() + "] fleet and doesnt not have a valid transform, attempt to create new transform");
						}
						if (!trans.HasValue)
						{
							CommonCombatState.Warn("Ship [" + shipInfo.ShipName + "] is in a [" + fleet.Type.ToString() + "] fleet failed to create valid transform, therefore it will not be spawned");
							continue;
						}
					}
					Matrix fullTrans = trans.Value;
					Ship ship = Ship.CreateShip(this.App.Game, fullTrans, shipInfo, 0, this._input.ObjectID, playerGOID, this._starSystemObjects.IsDeepSpace, (IEnumerable<Player>)this._playersInCombat);
					ship.ParentDatabaseID = shipInfo.ParentID;
					crits.Add((IGameObject)ship);
					ship.Deployed = (shipInfo.Params & ShipParams.HS_GATE_DEPLOYED) != (ShipParams)0;
					yield return ship.ObjectID;
					this._ships.Insert(shipInfo.ID, ship);
				}
			}
		}

		private RetreatData GetRetreatData(int playerID, int systemID, Vector3 spawnPos)
		{
			RetreatData retreatData = new RetreatData();
			retreatData.DefaultDestination = spawnPos;
			retreatData.SystemRadius = this._starSystemObjects.GetSystemRadius();
			if (this._starSystemObjects.CombatZones.Count > 0)
				retreatData.SystemRadius = this._starSystemObjects.CombatZones.Last<CombatZonePositionInfo>().RadiusUpper;
			PlayerInfo pi = this.App.GameDatabase.GetPlayerInfo(playerID);
			Faction faction = (Faction)null;
			if (pi != null)
				faction = this.App.AssetDatabase.Factions.FirstOrDefault<Faction>((Func<Faction, bool>)(x => x.ID == pi.FactionID));
			bool flag = false;
			if (faction != null)
			{
				if (faction.CanUseNodeLine(new bool?()))
				{
					if (this.App.GameDatabase.GetNodeLineBetweenSystems(playerID, this._systemId, systemID, faction.CanUseNodeLine(new bool?(true)), false) != null)
					{
						Vector3 starSystemOrigin = this.App.GameDatabase.GetStarSystemOrigin(this._systemId);
						Vector3 vector3 = this.App.GameDatabase.GetStarSystemOrigin(systemID) - starSystemOrigin;
						vector3.Y = 0.0f;
						if (this._starSystemObjects.CombatZones.Count > 0)
						{
							CombatZonePositionInfo zonePositionInfo = this._starSystemObjects.CombatZones.Last<CombatZonePositionInfo>();
							retreatData.DefaultDestination = Vector3.Normalize(vector3) * (zonePositionInfo.RadiusLower + faction.EntryPointOffset);
						}
						else
							retreatData.DefaultDestination = Vector3.Normalize(vector3) * (((IEnumerable<float>)StarSystem.CombatZoneMapRadii).ElementAt<float>(((IEnumerable<float>)StarSystem.CombatZoneMapRadii).Count<float>() - 1) * 5700f + faction.EntryPointOffset);
						flag = true;
					}
					if (faction.CanUseNodeLine(new bool?(true)))
					{
						retreatData.DefaultDestination = this._starSystemObjects.GetClosestPermanentNodeToPosition(this.App, spawnPos);
						flag = true;
					}
				}
				else if (faction.CanUseGate() || faction.CanUseAccelerators())
				{
					Matrix? gateSpawnTransform = this.GetGateSpawnTransform(playerID);
					if (gateSpawnTransform.HasValue)
					{
						retreatData.DefaultDestination = gateSpawnTransform.Value.Position;
						flag = true;
					}
				}
			}
			if (!flag)
			{
				if (this.App.Game.ScriptModules.GhostShip != null && this.App.Game.ScriptModules.GhostShip.PlayerID == playerID)
				{
					retreatData.DefaultDestination = this._starSystemObjects.GetClosestPermanentNodeToPosition(this.App, spawnPos);
					flag = true;
				}
				else if (this._starSystemObjects.CombatZones.Count > 0)
				{
					CombatZonePositionInfo zonePositionInfo = this._starSystemObjects.CombatZones.Last<CombatZonePositionInfo>();
					retreatData.DefaultDestination = Vector3.Normalize(spawnPos) * zonePositionInfo.RadiusUpper;
				}
				else
					retreatData.DefaultDestination = Vector3.Normalize(spawnPos) * 125000f;
			}
			retreatData.SetDestination = flag;
			return retreatData;
		}

		private void AddRidersToShip(GameObjectSet crits, Ship ship)
		{
			if (ship == null)
				return;
			foreach (Kerberos.Sots.GameObjects.BattleRiderMount battleRiderMount in ship.BattleRiderMounts)
			{
				DesignInfo designInfo = this.App.GameDatabase.GetDesignInfo(battleRiderMount.DesignID);
				if (designInfo != null)
				{
					ShipInfo shipInfo = new ShipInfo()
					{
						DesignID = battleRiderMount.DesignID,
						DesignInfo = designInfo,
						FleetID = 0,
						ParentID = ship.DatabaseID,
						SerialNumber = 0,
						ShipName = string.Empty
					};
					Ship ship1 = Ship.CreateShip(this.App.Game, Matrix.Identity, shipInfo, 0, this._input.ObjectID, ship.Player.ObjectID, this._starSystemObjects.IsDeepSpace, (IEnumerable<Player>)this._playersInCombat);
					ship1.ParentDatabaseID = ship.DatabaseID;
					crits.Add((IGameObject)ship1);
				}
			}
		}

		private void CreateAllPointsOfInterest(
		  GameObjectSet crits,
		  OrbitalObjectInfo[] oribitalObjects,
		  List<CombatEasterEggData> easterEgg)
		{
			StationInfo stationInfo = this._pirateEncounterData.PirateBase != null ? this.App.GameDatabase.GetStationInfo(this._pirateEncounterData.PirateBase.BaseStationId) : (StationInfo)null;
			foreach (IGameObject gameObject in crits.Objects.ToList<IGameObject>())
			{
				if (gameObject is Ship)
				{
					Ship ship = gameObject as Ship;
					switch (ship.CombatAI)
					{
						case SectionEnumerations.CombatAiType.SwarmerHive:
						case SectionEnumerations.CombatAiType.SwarmerQueen:
						case SectionEnumerations.CombatAiType.VonNeumannCollectorMotherShip:
						case SectionEnumerations.CombatAiType.VonNeumannSeekerMotherShip:
						case SectionEnumerations.CombatAiType.VonNeumannBerserkerMotherShip:
						case SectionEnumerations.CombatAiType.VonNeumannNeoBerserker:
						case SectionEnumerations.CombatAiType.VonNeumannPlanetKiller:
						case SectionEnumerations.CombatAiType.LocustMoon:
						case SectionEnumerations.CombatAiType.LocustWorld:
						case SectionEnumerations.CombatAiType.SystemKiller:
							this.AddPointOfInterest(ship.ObjectID, ship.Maneuvering.Position, false);
							break;
						case SectionEnumerations.CombatAiType.CommandMonitor:
							FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(this.App.GameDatabase.GetShipInfo(ship.DatabaseID, false).FleetID);
							if (fleetInfo != null && fleetInfo.PlayerID == this.App.Game.ScriptModules.AsteroidMonitor.PlayerID)
							{
								this.AddPointOfInterest(ship.ObjectID, ship.Maneuvering.Position, false);
								break;
							}
							break;
					}
					if (this._pirateEncounterData.IsPirateEncounter && stationInfo != null && ship.DatabaseID == stationInfo.ShipID)
						this.AddPointOfInterest(ship.ObjectID, ship.Maneuvering.Position, false);
				}
			}
			if (easterEgg.Count <= 0)
				return;
			foreach (CombatEasterEggData combatEasterEggData in easterEgg)
			{
				if (combatEasterEggData.Type == EasterEgg.EE_GARDENERS)
				{
					foreach (PlanetInfo gardenerPlanetsFrom in Gardeners.GetGardenerPlanetsFromList(this.App, this._systemId))
						this.AddPointOfInterest(0, this.App.GameDatabase.GetOrbitalTransform(gardenerPlanetsFrom.ID).Position, false);
				}
			}
		}

		private void AddPointOfInterest(int targetID, Vector3 startPosition, bool hasBeenSeen)
		{
			PointOfInterest pointOfInterest = this._crits.Add<PointOfInterest>((object)"effects\\ui\\Point_of_Interest.effect", (object)targetID);
			pointOfInterest.TargetID = targetID;
			pointOfInterest.HasBeenSeen = hasBeenSeen;
			pointOfInterest.Position = startPosition;
			this._postLoadedObjects.Add((IGameObject)pointOfInterest);
		}

		private List<CombatEasterEggData> GetCombatEasterEggData(
		  OrbitalObjectInfo[] oribitalObjects,
		  IDictionary<int, List<FleetInfo>> selectedPlayerFleets)
		{
			List<CombatEasterEggData> combatEasterEggDataList = new List<CombatEasterEggData>();
			foreach (List<FleetInfo> fleetInfoList in (IEnumerable<List<FleetInfo>>)selectedPlayerFleets.Values)
			{
				foreach (FleetInfo fleet in fleetInfoList)
				{
					if (fleet != null)
					{
						if (this.App.Game.ScriptModules.AsteroidMonitor != null && this.App.Game.ScriptModules.AsteroidMonitor.PlayerID == fleet.PlayerID)
							combatEasterEggDataList.Add(new CombatEasterEggData()
							{
								Type = EasterEgg.EE_ASTEROID_MONITOR,
								FleetID = fleet.ID
							});
						else if (this.App.Game.ScriptModules.MorrigiRelic != null && this.App.Game.ScriptModules.MorrigiRelic.PlayerID == fleet.PlayerID)
							combatEasterEggDataList.Add(new CombatEasterEggData()
							{
								Type = EasterEgg.EE_MORRIGI_RELIC,
								FleetID = fleet.ID
							});
						else if (!Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(this.App.Game, fleet) && this.App.Game.ScriptModules.Gardeners != null && this.App.Game.ScriptModules.Gardeners.PlayerID == fleet.PlayerID)
							combatEasterEggDataList.Add(new CombatEasterEggData()
							{
								Type = EasterEgg.EE_GARDENERS,
								FleetID = fleet.ID
							});
						else if (this.App.Game.ScriptModules.Swarmers != null && this.App.Game.ScriptModules.Swarmers.PlayerID == fleet.PlayerID)
							combatEasterEggDataList.Add(new CombatEasterEggData()
							{
								Type = EasterEgg.EE_SWARM,
								FleetID = fleet.ID
							});
						else if (this.App.Game.ScriptModules.VonNeumann != null && this.App.Game.ScriptModules.VonNeumann.PlayerID == fleet.PlayerID)
							combatEasterEggDataList.Add(new CombatEasterEggData()
							{
								Type = EasterEgg.EE_VON_NEUMANN,
								FleetID = fleet.ID
							});
						else if (Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(this.App.Game, fleet))
							combatEasterEggDataList.Add(new CombatEasterEggData()
							{
								Type = EasterEgg.GM_GARDENER,
								FleetID = fleet.ID
							});
						else if (this.App.Game.ScriptModules.Locust != null && this.App.Game.ScriptModules.Locust.PlayerID == fleet.PlayerID)
							combatEasterEggDataList.Add(new CombatEasterEggData()
							{
								Type = EasterEgg.GM_LOCUST_SWARM,
								FleetID = fleet.ID
							});
						else if (this.App.Game.ScriptModules.Comet != null && this.App.Game.ScriptModules.Comet.PlayerID == fleet.PlayerID)
							combatEasterEggDataList.Add(new CombatEasterEggData()
							{
								Type = EasterEgg.GM_COMET,
								FleetID = fleet.ID
							});
						else if (this.App.Game.ScriptModules.SystemKiller != null && this.App.Game.ScriptModules.SystemKiller.PlayerID == fleet.PlayerID)
							combatEasterEggDataList.Add(new CombatEasterEggData()
							{
								Type = EasterEgg.GM_SYSTEM_KILLER,
								FleetID = fleet.ID
							});
						else if (this.App.Game.ScriptModules.Pirates != null && this.App.Game.ScriptModules.Pirates.PlayerID == fleet.PlayerID && this._pirateEncounterData.PirateBase != null)
							combatEasterEggDataList.Add(new CombatEasterEggData()
							{
								Type = EasterEgg.EE_PIRATE_BASE,
								FleetID = fleet.ID
							});
					}
				}
			}
			foreach (CombatEasterEggData combatEasterEggData in combatEasterEggDataList)
			{
				switch (combatEasterEggData.Type)
				{
					case EasterEgg.EE_SWARM:
						combatEasterEggData.BaseFleetSpawnMatrix = Swarmers.GetBaseEnemyFleetTrans(this.App, this._systemId);
						continue;
					case EasterEgg.EE_ASTEROID_MONITOR:
						combatEasterEggData.BaseFleetSpawnMatrix = AsteroidMonitor.GetBaseEnemyFleetTrans(this.App, this.App.GameDatabase.GetShipInfoByFleetID(combatEasterEggData.FleetID, false).ToList<ShipInfo>(), this._systemId);
						continue;
					case EasterEgg.EE_PIRATE_BASE:
						combatEasterEggData.BaseFleetSpawnMatrix = Pirates.GetBaseEnemyFleetTrans(this.App, this._pirateEncounterData.PirateBase);
						continue;
					case EasterEgg.EE_VON_NEUMANN:
						combatEasterEggData.BaseFleetSpawnMatrix = VonNeumann.GetBaseEnemyFleetTrans(this.App, this._systemId);
						continue;
					case EasterEgg.EE_GARDENERS:
					case EasterEgg.GM_GARDENER:
						combatEasterEggData.BaseFleetSpawnMatrix = Gardeners.GetBaseEnemyFleetTrans(this.App, this._systemId, oribitalObjects);
						continue;
					case EasterEgg.EE_MORRIGI_RELIC:
						combatEasterEggData.BaseFleetSpawnMatrix = MorrigiRelic.GetBaseEnemyFleetTrans(this.App, this.App.GameDatabase.GetShipInfoByFleetID(combatEasterEggData.FleetID, false).Where<ShipInfo>((Func<ShipInfo, bool>)(x => x.ParentID == 0)).ToList<ShipInfo>(), oribitalObjects);
						continue;
					case EasterEgg.GM_SYSTEM_KILLER:
						combatEasterEggData.BaseFleetSpawnMatrix = SystemKiller.GetBaseEnemyFleetTrans(this.App, combatEasterEggData.FleetID, this._starSystemObjects, oribitalObjects);
						continue;
					case EasterEgg.GM_LOCUST_SWARM:
						combatEasterEggData.BaseFleetSpawnMatrix = Locust.GetBaseEnemyFleetTrans(this.App, this._systemId);
						continue;
					case EasterEgg.GM_COMET:
						combatEasterEggData.BaseFleetSpawnMatrix = Comet.GetBaseEnemyFleetTrans(this.App, this._starSystemObjects, oribitalObjects);
						continue;
					default:
						continue;
				}
			}
			return combatEasterEggDataList;
		}

		private List<CombatRandomData> GetCombatRandomData(
		  OrbitalObjectInfo[] oribitalObjects,
		  IDictionary<int, List<FleetInfo>> selectedPlayerFleets)
		{
			List<CombatRandomData> combatRandomDataList = new List<CombatRandomData>();
			foreach (List<FleetInfo> fleetInfoList in (IEnumerable<List<FleetInfo>>)selectedPlayerFleets.Values)
			{
				foreach (FleetInfo fleetInfo in fleetInfoList)
				{
					if (fleetInfo != null)
					{
						if (this.App.Game.ScriptModules.MeteorShower != null && this.App.Game.ScriptModules.MeteorShower.PlayerID == fleetInfo.PlayerID)
							combatRandomDataList.Add(new CombatRandomData()
							{
								Type = RandomEncounter.RE_ASTEROID_SHOWER,
								FleetID = fleetInfo.ID
							});
						else if (this.App.Game.ScriptModules.Spectre != null && this.App.Game.ScriptModules.Spectre.PlayerID == fleetInfo.PlayerID)
							combatRandomDataList.Add(new CombatRandomData()
							{
								Type = RandomEncounter.RE_SPECTORS,
								FleetID = fleetInfo.ID
							});
						else if (this.App.Game.ScriptModules.GhostShip != null && this.App.Game.ScriptModules.GhostShip.PlayerID == fleetInfo.PlayerID)
							combatRandomDataList.Add(new CombatRandomData()
							{
								Type = RandomEncounter.RE_GHOST_SHIP,
								FleetID = fleetInfo.ID
							});
						else if (this.App.Game.ScriptModules.Slaver != null && this.App.Game.ScriptModules.Slaver.PlayerID == fleetInfo.PlayerID)
							combatRandomDataList.Add(new CombatRandomData()
							{
								Type = RandomEncounter.RE_SLAVERS,
								FleetID = fleetInfo.ID
							});
					}
				}
			}
			foreach (CombatRandomData combatRandomData in combatRandomDataList)
			{
				switch (combatRandomData.Type)
				{
					case RandomEncounter.RE_ASTEROID_SHOWER:
						combatRandomData.BaseFleetSpawnMatrix = MeteorShower.GetBaseEnemyFleetTrans(this.App, this._systemId);
						continue;
					case RandomEncounter.RE_SPECTORS:
						combatRandomData.BaseFleetSpawnMatrix = Spectre.GetBaseEnemyFleetTrans(this.App, this._systemId);
						continue;
					case RandomEncounter.RE_SLAVERS:
						combatRandomData.BaseFleetSpawnMatrix = Slaver.GetBaseEnemyFleetTrans(this.App, this._starSystemObjects);
						continue;
					case RandomEncounter.RE_GHOST_SHIP:
						combatRandomData.BaseFleetSpawnMatrix = GhostShip.GetBaseEnemyFleetTrans(this.App, this._starSystemObjects);
						continue;
					default:
						combatRandomData.BaseFleetSpawnMatrix = Matrix.Identity;
						continue;
				}
			}
			return combatRandomDataList;
		}

		private CombatEasterEggData GetMostThreateningEasterEgg(
		  List<CombatEasterEggData> ceed)
		{
			if (ceed.Count == 0)
				return (CombatEasterEggData)null;
			return ceed.FirstOrDefault<CombatEasterEggData>((Func<CombatEasterEggData, bool>)(x => x.Type == EasterEgg.GM_SYSTEM_KILLER)) ?? ceed.FirstOrDefault<CombatEasterEggData>((Func<CombatEasterEggData, bool>)(x => x.Type == EasterEgg.GM_LOCUST_SWARM)) ?? ceed.FirstOrDefault<CombatEasterEggData>((Func<CombatEasterEggData, bool>)(x => x.Type == EasterEgg.GM_COMET)) ?? ceed.FirstOrDefault<CombatEasterEggData>((Func<CombatEasterEggData, bool>)(x => x.Type == EasterEgg.GM_GARDENER)) ?? ceed.FirstOrDefault<CombatEasterEggData>((Func<CombatEasterEggData, bool>)(x => x.Type == EasterEgg.EE_VON_NEUMANN)) ?? ceed.FirstOrDefault<CombatEasterEggData>((Func<CombatEasterEggData, bool>)(x => x.Type == EasterEgg.EE_SWARM)) ?? ceed.FirstOrDefault<CombatEasterEggData>((Func<CombatEasterEggData, bool>)(x => x.Type == EasterEgg.EE_GARDENERS)) ?? ceed.First<CombatEasterEggData>();
		}

		private CombatRandomData GetMostThreateningRandom(List<CombatRandomData> crd)
		{
			if (crd.Count == 0)
				return (CombatRandomData)null;
			return crd.FirstOrDefault<CombatRandomData>((Func<CombatRandomData, bool>)(x => x.Type == RandomEncounter.RE_GHOST_SHIP)) ?? crd.FirstOrDefault<CombatRandomData>((Func<CombatRandomData, bool>)(x => x.Type == RandomEncounter.RE_SPECTORS)) ?? crd.First<CombatRandomData>();
		}

		private Vector3 GetBaseFleetSpawnDirectionEasterEgg(CombatEasterEggData ceed)
		{
			if (ceed == null || (double)ceed.BaseFleetSpawnMatrix.Position.LengthSquared < 1.40129846432482E-45)
				return Vector3.UnitX;
			switch (ceed.Type)
			{
				case EasterEgg.EE_VON_NEUMANN:
				case EasterEgg.GM_LOCUST_SWARM:
					return Vector3.Cross(ceed.BaseFleetSpawnMatrix.Forward, Vector3.UnitY);
				case EasterEgg.GM_SYSTEM_KILLER:
				case EasterEgg.GM_COMET:
				case EasterEgg.GM_GARDENER:
					return ceed.BaseFleetSpawnMatrix.Forward;
				default:
					return Vector3.Normalize(ceed.BaseFleetSpawnMatrix.Position);
			}
		}

		private Vector3 GetBaseFleetSpawnDirectionFromRandom(CombatRandomData crd)
		{
			if (crd == null || (double)crd.BaseFleetSpawnMatrix.Position.LengthSquared < 1.40129846432482E-45)
				return Vector3.UnitX;
			switch (crd.Type)
			{
				case RandomEncounter.RE_SPECTORS:
				case RandomEncounter.RE_PIRATES:
				case RandomEncounter.RE_SLAVERS:
				case RandomEncounter.RE_REFUGEES:
				case RandomEncounter.RE_GHOST_SHIP:
					return crd.BaseFleetSpawnMatrix.Forward;
				case RandomEncounter.RE_FLYING_DUTCHMAN:
					return Vector3.Cross(crd.BaseFleetSpawnMatrix.Forward, Vector3.UnitY);
				default:
					return -crd.BaseFleetSpawnMatrix.Forward;
			}
		}

		private void SetSelectionToGroup(List<Ship> ships)
		{
			List<object> objectList = new List<object>();
			objectList.Add((object)1);
			objectList.Add((object)ships.Count);
			foreach (Ship ship in ships)
				objectList.Add((object)ship.ObjectID);
			this._input.PostSetProp(nameof(SetSelectionToGroup), objectList.ToArray());
		}

		private void CreateShipAbility(
		  ShipFleetAbilityType type,
		  List<Ship> owners,
		  List<Ship> affected)
		{
			if (owners.Count == 0)
				return;
			List<object> objectList = new List<object>();
			objectList.Add((object)owners.Count);
			foreach (Ship owner in owners)
			{
				objectList.Add((object)owner.ObjectID);
				objectList.Add((object)owner.MissionSection.ObjectID);
			}
			objectList.Add((object)affected.Count);
			foreach (Ship ship in affected)
				objectList.Add((object)ship.ObjectID);
			ShipFleetAbility shipFleetAbility = (ShipFleetAbility)null;
			switch (type)
			{
				case ShipFleetAbilityType.Protectorate:
					shipFleetAbility = (ShipFleetAbility)this.App.AddObject<ProtectorateAbility>(objectList.ToArray());
					LogicalShield logShield = this.App.AssetDatabase.Shields.FirstOrDefault<LogicalShield>((Func<LogicalShield, bool>)(x => x.TechID == "SLD_Shield_Mk._II"));
					if (logShield != null)
					{
						using (List<Ship>.Enumerator enumerator = affected.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								Ship current = enumerator.Current;
								if (current.Shield == null && !ShipSectionAsset.IsWeaponBattleRiderClass(current.RealShipClass) && Ship.IsShipClassBigger(ShipClass.Cruiser, current.ShipClass, true))
									current.ExternallyAssignShieldToShip(this.App, logShield);
							}
							break;
						}
					}
					else
						break;
				case ShipFleetAbilityType.Hidden:
					shipFleetAbility = (ShipFleetAbility)this.App.AddObject<TheHiddenAbility>(objectList.ToArray());
					break;
				case ShipFleetAbilityType.Deaf:
					shipFleetAbility = (ShipFleetAbility)this.App.AddObject<TheDeafAbility>(objectList.ToArray());
					break;
			}
			if (shipFleetAbility == null)
				return;
			this._crits.Add((IGameObject)shipFleetAbility);
		}

		private void PostNewPlayerFormation(List<FleetFormationCreationData> ffcd)
		{
			List<object> objectList = new List<object>();
			objectList.Add((object)InteropMessageID.IMID_ENGINE_ADD_FORMATION_PATTERN);
			objectList.Add((object)ffcd.Count);
			foreach (FleetFormationCreationData formationCreationData1 in ffcd)
			{
				objectList.Add((object)formationCreationData1.FormationData.Count);
				foreach (FormationCreationData formationCreationData2 in formationCreationData1.FormationData)
				{
					objectList.Add((object)formationCreationData2.ShipID);
					objectList.Add((object)formationCreationData2.DesignID);
					objectList.Add((object)(int)formationCreationData2.ShipRole);
					objectList.Add((object)(int)formationCreationData2.ShipClass);
					objectList.Add((object)formationCreationData2.FormationPosition);
				}
			}
			this.App.PostEngineMessage((IEnumerable)objectList);
			if (ffcd.Count <= 0 || this._input == null)
				return;
			this._input.PostSetProp("SyncFormationGroups");
		}

		private SpawnProfile GetSpawnProfileForFleet(
		  int fleetID,
		  IDictionary<int, List<FleetInfo>> selectedPlayerFleets,
		  List<CombatEasterEggData> eeData,
		  List<CombatRandomData> randData,
		  ref int spawnedFleetCount)
		{
			foreach (SpawnProfile spawnPosition in this._spawnPositions)
			{
				if (spawnPosition._fleetID == fleetID)
					return spawnPosition;
			}
			SpawnProfile spawnPos = new SpawnProfile();
			spawnPos._fleetID = fleetID;
			spawnPos._spawnFacing = Vector3.Zero;
			int num1 = 0;
			FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(fleetID);
			spawnPos._playerID = fleetInfo.PlayerID;
			foreach (ShipInfo shipInfo in this.App.GameDatabase.GetShipInfoByFleetID(fleetID, false))
			{
				spawnPos._reserveShips.Add(shipInfo.ID);
				int commandPointQuota = this.App.GameDatabase.GetShipCommandPointQuota(shipInfo.ID);
				if (commandPointQuota > num1)
				{
					num1 = commandPointQuota;
					spawnPos._activeCommandShipID = shipInfo.ID;
				}
			}
			int num2;
			if (spawnPos._activeCommandShipID > 0)
			{
				spawnPos._reserveShips.Remove(spawnPos._activeCommandShipID);
				spawnPos._activeShips.Add(spawnPos._activeCommandShipID);
				num2 = num1 - this.App.GameDatabase.GetCommandPointCost(this.App.GameDatabase.GetShipInfo(spawnPos._activeCommandShipID, false).DesignID);
			}
			else
				num2 = 18;
			while (num2 > 0 && spawnPos._reserveShips.Count<int>() > 0)
			{
				int parentID = 0;
				int num3 = 0;
				foreach (ShipInfo shipInfo in this.App.GameDatabase.GetShipInfoByFleetID(fleetID, false))
				{
					if (shipInfo.ID != spawnPos._activeCommandShipID && spawnPos._reserveShips.Contains(shipInfo.ID))
					{
						int commandPointCost = this.App.GameDatabase.GetCommandPointCost(shipInfo.DesignID);
						if (commandPointCost <= num2 && commandPointCost > num3)
						{
							num3 = commandPointCost;
							parentID = shipInfo.ID;
						}
					}
				}
				if (parentID != 0)
				{
					spawnPos._reserveShips.Remove(parentID);
					spawnPos._activeShips.Add(parentID);
					num2 -= num3;
					foreach (ShipInfo shipInfo in this.App.GameDatabase.GetBattleRidersByParentID(parentID))
					{
						spawnPos._reserveShips.Remove(shipInfo.ID);
						if (shipInfo.RiderIndex >= 0)
							spawnPos._activeShips.Add(shipInfo.ID);
					}
				}
				else
					break;
			}
			if (this.App.Game.ScriptModules.VonNeumann != null && this.App.Game.ScriptModules.VonNeumann.PlayerID == fleetInfo.PlayerID && this.App.Game.ScriptModules.VonNeumann.HomeWorldSystemID == this._systemId)
			{
				Matrix? matrixAtHomeWorld = this.App.Game.ScriptModules.VonNeumann.GetVNFleetSpawnMatrixAtHomeWorld(this.App.GameDatabase, fleetInfo, spawnedFleetCount);
				if (matrixAtHomeWorld.HasValue)
				{
					spawnPos._spawnFacing = matrixAtHomeWorld.Value.Forward;
					spawnPos._startingPosition = matrixAtHomeWorld.Value.Position;
					spawnPos._spawnPosition = spawnPos._startingPosition;
				}
				return spawnPos;
			}
			if (this._trapCombatData.IsTrapCombat)
				return this.GetTrapSpawnLocation(fleetInfo, spawnPos, ref spawnedFleetCount);
			if (this._pirateEncounterData.PirateBase != null && this.App.Game.ScriptModules.Pirates != null && this.App.Game.ScriptModules.Pirates.PlayerID == fleetInfo.PlayerID)
			{
				StationInfo stationInfo = this.App.Game.GameDatabase.GetStationInfo(this._pirateEncounterData.PirateBase.BaseStationId);
				Matrix orbitalTransform1 = this.App.Game.GameDatabase.GetOrbitalTransform(stationInfo.OrbitalObjectID);
				Vector3 vector3 = orbitalTransform1.Position;
				OrbitalObjectInfo orbitalObjectInfo = this.App.Game.GameDatabase.GetOrbitalObjectInfo(stationInfo.OrbitalObjectID);
				if (orbitalObjectInfo.ParentID.HasValue)
				{
					Matrix orbitalTransform2 = this.App.Game.GameDatabase.GetOrbitalTransform(orbitalObjectInfo.ParentID.Value);
					vector3 = orbitalTransform1.Position - orbitalTransform2.Position;
				}
				double num3 = (double)vector3.Normalize();
				spawnPos._spawnFacing = -vector3;
				spawnPos._startingPosition = orbitalTransform1.Position + vector3 * 2500f;
				spawnPos._spawnPosition = spawnPos._startingPosition;
				return spawnPos;
			}
			SpawnProfile inforAtHomeColony = this.GetSpawnInforAtHomeColony(fleetInfo, spawnPos, ref spawnedFleetCount);
			if (inforAtHomeColony != null)
			{
				this._spawnPositions.Add(inforAtHomeColony);
				++spawnedFleetCount;
				return inforAtHomeColony;
			}
			SpawnProfile spawnProfile = this.GetSpawnInforGate(fleetInfo, spawnPos, selectedPlayerFleets, eeData, randData, ref spawnedFleetCount);
			if (spawnProfile != null)
			{
				bool flag = false;
				foreach (KeyValuePair<int, List<FleetInfo>> selectedPlayerFleet in (IEnumerable<KeyValuePair<int, List<FleetInfo>>>)selectedPlayerFleets)
				{
					if (this.GetDiplomacyState(selectedPlayerFleet.Key, fleetInfo.PlayerID) == DiplomacyState.WAR && selectedPlayerFleet.Value.Where<FleetInfo>((Func<FleetInfo, bool>)(x => x != null)).Count<FleetInfo>() > 0)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					SpawnProfile infoFromCombatZone = this.GetSpawnInfoFromCombatZone(fleetInfo, spawnPos, selectedPlayerFleets, ref spawnedFleetCount);
					if (infoFromCombatZone != null)
						spawnProfile = infoFromCombatZone;
				}
				this._spawnPositions.Add(spawnPos);
				++spawnedFleetCount;
				return spawnProfile;
			}
			SpawnProfile inforNotHomeColony = this.GetSpawnInforNotHomeColony(fleetInfo, spawnPos, selectedPlayerFleets, eeData, randData, ref spawnedFleetCount);
			if (inforNotHomeColony != null)
			{
				this._spawnPositions.Add(spawnPos);
				++spawnedFleetCount;
				return inforNotHomeColony;
			}
			SpawnProfile defaultSpawnInfo = this.GetDefaultSpawnInfo(fleetInfo, spawnPos, selectedPlayerFleets, ref spawnedFleetCount);
			if (defaultSpawnInfo != null)
			{
				this._spawnPositions.Add(spawnPos);
				++spawnedFleetCount;
				return defaultSpawnInfo;
			}
			++spawnedFleetCount;
			return spawnPos;
		}

		private SpawnProfile GetTrapSpawnLocation(
		  FleetInfo fleet,
		  SpawnProfile spawnPos,
		  ref int spawnedFleetCount)
		{
			List<StellarBody> planetsInSystem = this._starSystemObjects.GetPlanetsInSystem();
			if (this._trapCombatData.TrapFleets.Contains(fleet))
			{
				int planetID = this._trapCombatData.TrapToPlanet[fleet.ID];
				StellarBody stellarBody = planetsInSystem.FirstOrDefault<StellarBody>((Func<StellarBody, bool>)(x => x.PlanetInfo.ID == planetID));
				if (stellarBody != null)
				{
					spawnPos._startingPosition = stellarBody.Parameters.Position;
					spawnPos._spawnFacing = Vector3.Normalize(stellarBody.Parameters.Position);
				}
				spawnPos._spawnPosition = spawnPos._startingPosition;
				return spawnPos;
			}
			if (this._trapCombatData.ColonyTrappedFleets.Contains(fleet))
			{
				int planetID = this._trapCombatData.TrapToPlanet[this._trapCombatData.ColonyFleetToTrap[fleet.ID]];
				StellarBody stellarBody = planetsInSystem.FirstOrDefault<StellarBody>((Func<StellarBody, bool>)(x => x.PlanetInfo.ID == planetID));
				if (stellarBody != null)
				{
					Vector3 zero = Vector3.Zero;
					zero.X = (this._random.CoinToss(0.5) ? -1f : 1f) * this._random.NextInclusive(1E-05f, 1f);
					zero.Z = (this._random.CoinToss(0.5) ? -1f : 1f) * this._random.NextInclusive(1E-05f, 1f);
					double num = (double)zero.Normalize();
					spawnPos._startingPosition = stellarBody.Parameters.Position + zero * (float)((double)stellarBody.Parameters.Radius + 750.0 + 2000.0);
					spawnPos._spawnFacing = Vector3.Normalize(stellarBody.Parameters.Position - spawnPos._startingPosition);
				}
			}
			else
			{
				int planetID = this._trapCombatData.TrapToPlanet[this._random.Choose<FleetInfo>((IList<FleetInfo>)this._trapCombatData.TrapFleets).ID];
				StellarBody stellarBody = planetsInSystem.FirstOrDefault<StellarBody>((Func<StellarBody, bool>)(x => x.PlanetInfo.ID == planetID));
				if (stellarBody != null)
				{
					Vector3 zero = Vector3.Zero;
					zero.X = (this._random.CoinToss(0.5) ? -1f : 1f) * this._random.NextInclusive(1E-05f, 1f);
					zero.Z = (this._random.CoinToss(0.5) ? -1f : 1f) * this._random.NextInclusive(1E-05f, 1f);
					double num = (double)zero.Normalize();
					spawnPos._startingPosition = stellarBody.Parameters.Position + zero * (float)((double)stellarBody.Parameters.Radius + 750.0 + 5000.0);
					spawnPos._spawnFacing = Vector3.Normalize(stellarBody.Parameters.Position - spawnPos._startingPosition);
				}
			}
			spawnPos._spawnPosition = spawnPos._startingPosition;
			return spawnPos;
		}

		private SpawnProfile GetSpawnInforAtHomeColony(
		  FleetInfo fleet,
		  SpawnProfile spawnPos,
		  ref int spawnedFleetCount)
		{
			if (this._systemId != 0)
			{
				bool flag = false;
				foreach (PlanetInfo systemPlanetInfo in this.App.GameDatabase.GetStarSystemPlanetInfos(this._systemId))
				{
					ColonyInfo colonyInfoForPlanet = this.App.GameDatabase.GetColonyInfoForPlanet(systemPlanetInfo.ID);
					if (colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == fleet.PlayerID)
					{
						Vector3 position = this.App.GameDatabase.GetOrbitalTransform(systemPlanetInfo.ID).Position;
						float radius = StarSystemVars.Instance.SizeToRadius(systemPlanetInfo.Size);
						spawnPos._startingPosition = position;
						Vector3 vector3_1 = Vector3.Normalize(spawnPos._startingPosition) * (float)((double)radius + 5000.0 + 1000.0 * (double)spawnedFleetCount);
						spawnPos._startingPosition += vector3_1;
						spawnPos._spawnFacing = Vector3.Normalize(spawnPos._startingPosition);
						List<Ship> stationsAroundPlanet = this._starSystemObjects.GetStationsAroundPlanet(systemPlanetInfo.ID);
						if (stationsAroundPlanet.Count > 0)
						{
							foreach (Ship station in stationsAroundPlanet)
							{
								StationInfo stationInfo = this._starSystemObjects.GetStationInfo(station);
								if (stationInfo != null && stationInfo.DesignInfo.StationType == StationType.NAVAL && stationInfo.DesignInfo.StationLevel > 0)
								{
									Vector3 vector3_2 = station.Position - position;
									double num = (double)vector3_2.Normalize();
									spawnPos._startingPosition = station.Position + vector3_2 * 3500f;
									Vector3 vector3_3 = Vector3.Normalize(spawnPos._startingPosition) * (1000f * (float)spawnedFleetCount);
									spawnPos._startingPosition += vector3_3;
								}
							}
						}
						spawnPos._spawnPosition = spawnPos._startingPosition;
						flag = true;
						break;
					}
				}
				if (flag)
				{
					Player playerObject = this.App.Game.GetPlayerObject(fleet.PlayerID);
					if (playerObject == null || !playerObject.IsStandardPlayer || !playerObject.IsAI())
						return spawnPos;
					Vector3? nullable = new Vector3?();
					float num1 = float.MaxValue;
					foreach (FleetInfo fleetInfo in this.App.GameDatabase.GetFleetInfoBySystemID(this._systemId, FleetType.FL_ALL_COMBAT).ToList<FleetInfo>())
					{
						if (this.GetDiplomacyState(fleet.PlayerID, fleetInfo.PlayerID) == DiplomacyState.WAR)
						{
							if (fleetInfo.Type == FleetType.FL_NORMAL)
							{
								nullable = new Vector3?();
								break;
							}
							if (fleetInfo.Type == FleetType.FL_GATE || fleetInfo.Type == FleetType.FL_ACCELERATOR)
							{
								List<ShipInfo> list = this.App.GameDatabase.GetShipInfoByFleetID(fleetInfo.ID, false).ToList<ShipInfo>();
								if (list.Count != 0)
								{
									Vector3 zero = Vector3.Zero;
									int num2 = 0;
									foreach (ShipInfo shipInfo in list)
									{
										Matrix? shipSystemPosition = this.App.GameDatabase.GetShipSystemPosition(shipInfo.ID);
										if (shipSystemPosition.HasValue)
										{
											++num2;
											zero += shipSystemPosition.Value.Position;
										}
									}
									if (num2 != 0)
									{
										Vector3 targetPosition = zero / (float)num2;
										CombatZonePositionInfo closestZoneToPosition = this._starSystemObjects.GetClosestZoneToPosition(this.App, fleet.PlayerID, targetPosition);
										if (closestZoneToPosition != null)
										{
											float lengthSquared = (closestZoneToPosition.Center - spawnPos._spawnPosition).LengthSquared;
											if ((double)lengthSquared < (double)num1)
											{
												nullable = new Vector3?(closestZoneToPosition.Center);
												num1 = lengthSquared;
											}
										}
									}
								}
							}
						}
					}
					if (nullable.HasValue)
					{
						spawnPos._spawnFacing = Vector3.Normalize(nullable.Value);
						spawnPos._startingPosition = nullable.Value - spawnPos._spawnFacing * 5000f;
						spawnPos._spawnPosition = spawnPos._startingPosition;
					}
					return spawnPos;
				}
			}
			return (SpawnProfile)null;
		}

		private Matrix? GetGateSpawnTransform(int playerID)
		{
			int? systemOwningPlayer = this.App.GameDatabase.GetSystemOwningPlayer(this._systemId);
			if (systemOwningPlayer.HasValue && systemOwningPlayer.Value == playerID)
				return new Matrix?();
			List<FleetInfo> list1 = this.App.GameDatabase.GetFleetsByPlayerAndSystem(playerID, this._systemId, FleetType.FL_GATE | FleetType.FL_ACCELERATOR).ToList<FleetInfo>();
			if (list1.Count == 0)
				return new Matrix?();
			List<ShipInfo> list2 = this.App.GameDatabase.GetShipInfoByFleetID(list1.First<FleetInfo>().ID, false).ToList<ShipInfo>();
			if (list2.Count == 0)
				return new Matrix?();
			return this.App.GameDatabase.GetShipSystemPosition(list2.First<ShipInfo>().ID);
		}

		private SpawnProfile GetSpawnInforGate(
		  FleetInfo fleet,
		  SpawnProfile spawnPos,
		  IDictionary<int, List<FleetInfo>> selectedPlayerFleets,
		  List<CombatEasterEggData> eeData,
		  List<CombatRandomData> randData,
		  ref int spawnedFleetCount)
		{
			if (eeData.Count > 0 || randData.Count > 0 || fleet.IsGateFleet)
				return (SpawnProfile)null;
			Matrix? gateSpawnTransform = this.GetGateSpawnTransform(fleet.PlayerID);
			if (!gateSpawnTransform.HasValue)
				return (SpawnProfile)null;
			spawnPos._spawnFacing = gateSpawnTransform.Value.Forward;
			spawnPos._startingPosition = gateSpawnTransform.Value.Position + gateSpawnTransform.Value.Forward * 1500f;
			spawnPos._spawnPosition = spawnPos._startingPosition;
			return spawnPos;
		}

		private SpawnProfile GetSpawnInforNotHomeColony(
		  FleetInfo fleet,
		  SpawnProfile spawnPos,
		  IDictionary<int, List<FleetInfo>> selectedPlayerFleets,
		  List<CombatEasterEggData> eeData,
		  List<CombatRandomData> randData,
		  ref int spawnedFleetCount)
		{
			CombatEasterEggData combatEasterEggData = eeData.FirstOrDefault<CombatEasterEggData>((Func<CombatEasterEggData, bool>)(x => x.FleetID == fleet.ID));
			CombatRandomData combatRandomData = randData.FirstOrDefault<CombatRandomData>((Func<CombatRandomData, bool>)(x => x.FleetID == fleet.ID));
			bool flag1 = eeData.Count > 0 || randData.Count > 0;
			if (this._systemId != 0)
			{
				OrbitalObjectInfo orbital = (OrbitalObjectInfo)null;
				float num1 = 0.0f;
				float val1 = 0.0f;
				int orbitalObjectID = 0;
				if (combatEasterEggData != null)
					orbitalObjectID = this.App.GameDatabase.GetEncounterOrbitalId(combatEasterEggData.Type, this._systemId);
				if (orbitalObjectID != 0)
				{
					this.App.GameDatabase.GetEncounterPlayerId(this.App.Game, this._systemId);
					orbital = this.App.GameDatabase.GetOrbitalObjectInfo(orbitalObjectID);
				}
				if (combatEasterEggData != null || combatRandomData != null)
				{
					Matrix matrix = Matrix.Identity;
					if (orbital != null)
						matrix = this.App.GameDatabase.GetOrbitalTransform(orbital.ID);
					if (combatEasterEggData != null)
					{
						switch (combatEasterEggData.Type)
						{
							case EasterEgg.EE_SWARM:
								matrix = Swarmers.GetSpawnTransform(this.App, this._systemId);
								break;
							case EasterEgg.EE_PIRATE_BASE:
								matrix = Pirates.GetSpawnTransform(this.App, this._pirateEncounterData.PirateBase);
								break;
							case EasterEgg.EE_VON_NEUMANN:
								matrix = VonNeumann.GetSpawnTransform(this.App, this._systemId);
								break;
							case EasterEgg.GM_LOCUST_SWARM:
								matrix = Locust.GetSpawnTransform(this.App, this._systemId);
								break;
						}
					}
					else if (combatRandomData != null)
					{
						switch (combatRandomData.Type)
						{
							case RandomEncounter.RE_SPECTORS:
								matrix = Spectre.GetSpawnTransform(this.App, this._systemId);
								break;
							case RandomEncounter.RE_SLAVERS:
								matrix = Slaver.GetSpawnTransform(this.App, this._starSystemObjects);
								break;
							case RandomEncounter.RE_GHOST_SHIP:
								matrix = GhostShip.GetSpawnTransform(this.App, this._starSystemObjects);
								break;
						}
					}
					spawnPos._spawnFacing = matrix.Forward;
					spawnPos._spawnPosition = matrix.Position;
					spawnPos._startingPosition = matrix.Position;
					return spawnPos;
				}
				bool flag2 = false;
				bool flag3 = false;
				if (orbital == null)
				{
					float num2 = 0.0f;
					foreach (OrbitalObjectInfo orbitalObjectInfo in this.App.GameDatabase.GetStarSystemOrbitalObjectInfos(this._systemId))
					{
						PlanetInfo planetInfo = this.App.GameDatabase.GetPlanetInfo(orbitalObjectInfo.ID);
						List<Ship> shipList = new List<Ship>();
						ColonyInfo colonyInfoForPlanet = this.App.GameDatabase.GetColonyInfoForPlanet(orbitalObjectInfo.ID);
						if (colonyInfoForPlanet != null)
						{
							DiplomacyState diplomacyState = this.GetDiplomacyState(colonyInfoForPlanet.PlayerID, fleet.PlayerID);
							flag2 = flag2 || diplomacyState == DiplomacyState.WAR;
							flag3 = flag3 || diplomacyState == DiplomacyState.ALLIED;
							if (diplomacyState == DiplomacyState.WAR)
							{
								float lengthSquared = this.App.GameDatabase.GetOrbitalTransform(orbitalObjectInfo.ID).Position.LengthSquared;
								if ((double)lengthSquared > (double)num2)
									num2 = lengthSquared;
							}
						}
						float length = this.App.GameDatabase.GetOrbitalTransform(orbitalObjectInfo.ID).Position.Length;
						if (!orbitalObjectInfo.ParentID.HasValue)
							val1 = Math.Max(val1, length);
						if (planetInfo != null)
						{
							float num3 = length + StarSystemVars.Instance.SizeToRadius(planetInfo.Size);
							if ((double)num3 > (double)num1)
							{
								orbital = orbitalObjectInfo;
								num1 = num3;
							}
						}
					}
				}
				if (!flag1 || this._ignoreEncounterSpawnPos)
				{
					SpawnProfile infoFromCombatZone = this.GetSpawnInfoFromCombatZone(fleet, spawnPos, selectedPlayerFleets, ref spawnedFleetCount);
					if (infoFromCombatZone != null)
						return infoFromCombatZone;
					if (flag3)
					{
						SpawnProfile forFriendlySystem = this.GetSpawnInfoForFriendlySystem(fleet, spawnPos, selectedPlayerFleets, ref spawnedFleetCount);
						if (forFriendlySystem != null)
							return forFriendlySystem;
					}
					if (!flag2)
					{
						int? systemOwningPlayer = this.App.GameDatabase.GetSystemOwningPlayer(this._systemId);
						if (!systemOwningPlayer.HasValue || this.GetDiplomacyState(systemOwningPlayer.Value, fleet.PlayerID) != DiplomacyState.WAR)
						{
							SpawnProfile defaultControlZone = this.GetSpawnInfoForDefaultControlZone(fleet, spawnPos, selectedPlayerFleets, ref spawnedFleetCount);
							if (defaultControlZone != null)
								return defaultControlZone;
							SpawnProfile infoAtNeutralSystem = this.GetSpawnInfoAtNeutralSystem(fleet, spawnPos, selectedPlayerFleets, ref spawnedFleetCount);
							if (infoAtNeutralSystem != null)
								return infoAtNeutralSystem;
						}
					}
				}
				if (orbital != null || flag1)
				{
					int factionID = this.App.GameDatabase.GetPlayerInfo(fleet.PlayerID).FactionID;
					Faction faction = this.App.AssetDatabase.Factions.FirstOrDefault<Faction>((Func<Faction, bool>)(x => x.ID == factionID));
					if (faction == null)
						return (SpawnProfile)null;
					Vector3 vector3_1 = new Vector3();
					List<ColonyInfo> list = this.App.GameDatabase.GetColonyInfosForSystem(this._systemId).ToList<ColonyInfo>();
					Vector3 vector3_2;
					if (list.Count == 0)
					{
						vector3_2 = Vector3.UnitX;
					}
					else
					{
						float num2 = 0.0f;
						foreach (ColonyInfo colonyInfo in list)
						{
							if (colonyInfo.PlayerID != fleet.PlayerID)
							{
								vector3_1 += this.App.GameDatabase.GetOrbitalTransform(colonyInfo.OrbitalObjectID).Position;
								++num2;
							}
						}
						vector3_2 = vector3_1 / num2;
					}
					float num3 = 0.0f;
					if (orbital != null)
					{
						PlanetInfo planetInfo = this.App.GameDatabase.GetPlanetInfo(orbital.ID);
						num3 = planetInfo == null ? StarHelper.CalcRadius(StellarClass.Parse(this.App.GameDatabase.GetStarSystemInfo(this._systemId).StellarClass).Size) * 3f : StarSystemVars.Instance.SizeToRadius(planetInfo.Size);
					}
					Vector3 vector1 = Vector3.UnitX;
					Vector3 vector3_3;
					if (flag1 && !this._ignoreEncounterSpawnPos)
					{
						float num2 = 0.0f;
						if (fleet.AdmiralID != 0)
						{
							AdmiralInfo admiralInfo = this.App.GameDatabase.GetAdmiralInfo(fleet.AdmiralID);
							if (admiralInfo != null)
								num2 = Math.Min((float)((double)admiralInfo.ReactionBonus * (double)this.App.GetStratModifier<float>(StratModifiers.AdmiralReactionModifier, admiralInfo.PlayerID) / 100.0), 1f);
						}
						float encounterStartPos = this.App.AssetDatabase.MinEncounterStartPos;
						float num4 = (Math.Max(this.App.AssetDatabase.MaxEncounterStartPos, encounterStartPos) - encounterStartPos) * num2;
						if (eeData.Count > 0)
						{
							CombatEasterEggData threateningEasterEgg = this.GetMostThreateningEasterEgg(eeData);
							spawnPos._startingPosition = threateningEasterEgg.BaseFleetSpawnMatrix.Position;
							vector1 = this.GetBaseFleetSpawnDirectionEasterEgg(threateningEasterEgg);
						}
						else if (randData.Count > 0)
						{
							CombatRandomData threateningRandom = this.GetMostThreateningRandom(randData);
							spawnPos._startingPosition = threateningRandom.BaseFleetSpawnMatrix.Position;
							vector1 = this.GetBaseFleetSpawnDirectionFromRandom(threateningRandom);
						}
						else if (orbital != null)
						{
							Matrix orbitalTransform = this.App.GameDatabase.GetOrbitalTransform(orbital.ID);
							spawnPos._startingPosition = orbitalTransform.Position;
							vector1 = (double)spawnPos._startingPosition.LengthSquared >= 0.0001 ? Vector3.Normalize(spawnPos._startingPosition) : Vector3.UnitX;
						}
						vector3_3 = vector1 * (float)((double)encounterStartPos + (double)num4 + 5000.0);
					}
					else
					{
						if (!flag1 || this._ignoreEncounterSpawnPos)
						{
							SpawnProfile profileForEnterSystem = this.GetSpawnProfileForEnterSystem(fleet, faction, spawnPos);
							if (profileForEnterSystem != null)
								return profileForEnterSystem;
						}
						float num2 = 0.0f;
						if (faction.Name == "liir_zuul")
							num2 = 6000f;
						else if (faction.Name == "morrigi")
							num2 = 5500f;
						else if (faction.Name == "hiver")
							num2 = 7000f;
						else if (faction.Name == "tarkas")
							num2 = 4000f;
						else if (faction.Name == "human" || faction.Name == "zuul")
							num2 = 5000f;
						else if (faction.Name == "loa")
							num2 = 4000f;
						Vector3 vector3_4 = vector3_2 * -1f;
						double num4 = (double)vector3_4.Normalize();
						if (!(faction.Name == "zuul") && !(faction.Name == "human"))
							spawnPos._startingPosition = vector3_4 * num1;
						else if (orbital != null)
							spawnPos._startingPosition = this.App.GameDatabase.GetOrbitalTransform(orbital).Position;
						if (faction.Name == "zuul" && this.App.GameDatabase.PlayerHasTech(fleet.PlayerID, "DRV_Star_Tear"))
							spawnPos._startingPosition = vector3_4 * 40000f;
						vector3_3 = vector1 * (num2 + num3);
					}
					Vector3 vector3_5 = Vector3.Cross(vector1, new Vector3(0.0f, 1f, 0.0f));
					double num5 = (double)vector3_5.Normalize();
					spawnPos._startingPosition += vector3_3 + vector3_5 * (float)spawnedFleetCount * 10000f;
					spawnPos._spawnPosition = spawnPos._startingPosition;
					spawnPos._spawnFacing = vector1 * -1f;
					return spawnPos;
				}
			}
			return (SpawnProfile)null;
		}

		private SpawnProfile GetDefaultSpawnInfo(
		  FleetInfo fleet,
		  SpawnProfile spawnPos,
		  IDictionary<int, List<FleetInfo>> selectedPlayerFleets,
		  ref int spawnedFleetCount)
		{
			float num = this._starSystemObjects.GetStarRadius();
			if (this.App.GameDatabase.GetNeutronStarInfos().Any<NeutronStarInfo>((Func<NeutronStarInfo, bool>)(x =>
		   {
			   if (x.DeepSpaceSystemId.HasValue)
				   return x.DeepSpaceSystemId.Value == this._systemId;
			   return false;
		   })) || this.App.GameDatabase.GetGardenerInfos().Any<GardenerInfo>((Func<GardenerInfo, bool>)(x =>
	 {
		 if (x.DeepSpaceSystemId.HasValue)
			 return x.DeepSpaceSystemId.Value == this._systemId;
		 return false;
	 })))
				num = 5000f;
			Matrix rotationYpr = Matrix.CreateRotationYPR(MathHelper.DegreesToRadians(360f / (float)selectedPlayerFleets.Count) * (float)this._spawnPositions.Count, 0.0f, 0.0f);
			spawnPos._startingPosition += rotationYpr.Forward * (float)((double)num + 15000.0 + 5000.0 * (double)spawnedFleetCount);
			spawnPos._spawnPosition = spawnPos._startingPosition;
			spawnPos._spawnFacing = Vector3.Normalize(spawnPos._startingPosition) * -1f;
			return spawnPos;
		}

		private void InitializeNeutralCombatStanceInfo(
		  IDictionary<int, List<FleetInfo>> selectedPlayerFleets)
		{
			this._neutralCombatStanceInfo.InitData();
			FleetInfo fleetInfo1 = (FleetInfo)null;
			FleetInfo fleetInfo2 = (FleetInfo)null;
			foreach (IEnumerable<FleetInfo> source in (IEnumerable<List<FleetInfo>>)selectedPlayerFleets.Values)
			{
				fleetInfo1 = source.FirstOrDefault<FleetInfo>((Func<FleetInfo, bool>)(x =>
			   {
				   if (x != null)
					   return x.AdmiralID != 0;
				   return false;
			   }));
				fleetInfo2 = (FleetInfo)null;
				if (fleetInfo1 != null && fleetInfo1.IsNormalFleet)
				{
					foreach (List<FleetInfo> fleetInfoList in (IEnumerable<List<FleetInfo>>)selectedPlayerFleets.Values)
					{
						foreach (FleetInfo fleetInfo3 in fleetInfoList)
						{
							if (fleetInfo3 != null && fleetInfo3.AdmiralID != 0 && fleetInfo3.Type == FleetType.FL_NORMAL)
							{
								if (fleetInfo1.PlayerID != fleetInfo3.PlayerID)
								{
									if (this.App.GameDatabase.GetDiplomacyStateBetweenPlayers(fleetInfo1.PlayerID, fleetInfo3.PlayerID) == DiplomacyState.WAR)
									{
										fleetInfo2 = fleetInfo3;
										break;
									}
								}
								else
									break;
							}
						}
					}
					if (fleetInfo1 != null)
					{
						if (fleetInfo2 != null)
							break;
					}
				}
			}
			if (fleetInfo1 == null || fleetInfo2 == null)
			{
				this._neutralCombatStanceInfo.Stance = NeutralCombatStance.Invalid;
			}
			else
			{
				AdmiralInfo admiralInfo1 = this.App.GameDatabase.GetAdmiralInfo(fleetInfo1.AdmiralID);
				AdmiralInfo admiralInfo2 = this.App.GameDatabase.GetAdmiralInfo(fleetInfo2.AdmiralID);
				int num1 = admiralInfo1.ReactionBonus + this._random.NextInclusive(1, 30);
				int num2 = admiralInfo2.ReactionBonus + this._random.NextInclusive(1, 30);
				int num3 = Math.Abs(num1 - num2);
				FleetInfo fleetInfo3 = fleetInfo1;
				FleetInfo fleetInfo4 = fleetInfo2;
				Vector3 vector3_1 = Vector3.UnitX;
				float num4 = 5000f;
				if (num3 > 20)
				{
					this._neutralCombatStanceInfo.Stance = NeutralCombatStance.Chasing;
					fleetInfo3 = num1 > num2 ? fleetInfo2 : fleetInfo1;
					fleetInfo4 = num1 > num2 ? fleetInfo1 : fleetInfo2;
				}
				else if (num3 >= 10)
				{
					this._neutralCombatStanceInfo.Stance = NeutralCombatStance.Facing;
				}
				else
				{
					this._neutralCombatStanceInfo.Stance = NeutralCombatStance.Side;
					vector3_1 = -Vector3.UnitZ;
				}
				this._neutralCombatStanceInfo.FleetAPos.FleetID = fleetInfo3.ID;
				this._neutralCombatStanceInfo.FleetBPos.FleetID = fleetInfo4.ID;
				switch (this._neutralCombatStanceInfo.Stance)
				{
					case NeutralCombatStance.Facing:
						this._neutralCombatStanceInfo.FleetAPos.Position = new Vector3(0.0f, 0.0f, -3000f);
						this._neutralCombatStanceInfo.FleetAPos.Facing = Vector3.UnitZ;
						this._neutralCombatStanceInfo.FleetBPos.Position = new Vector3(0.0f, 0.0f, 3000f);
						this._neutralCombatStanceInfo.FleetBPos.Facing = -Vector3.UnitZ;
						break;
					case NeutralCombatStance.Chasing:
						this._neutralCombatStanceInfo.FleetAPos.Position = new Vector3(0.0f, 0.0f, -2000f);
						this._neutralCombatStanceInfo.FleetAPos.Facing = -Vector3.UnitZ;
						this._neutralCombatStanceInfo.FleetBPos.Position = new Vector3(0.0f, 0.0f, 2000f);
						this._neutralCombatStanceInfo.FleetBPos.Facing = -Vector3.UnitZ;
						break;
					case NeutralCombatStance.Side:
						this._neutralCombatStanceInfo.FleetAPos.Position = new Vector3(0.0f, 0.0f, -2500f);
						this._neutralCombatStanceInfo.FleetAPos.Facing = -Vector3.UnitZ;
						this._neutralCombatStanceInfo.FleetBPos.Position = new Vector3(0.0f, 0.0f, 2500f);
						this._neutralCombatStanceInfo.FleetBPos.Facing = -Vector3.UnitZ;
						break;
				}
				foreach (List<FleetInfo> fleetInfoList in (IEnumerable<List<FleetInfo>>)selectedPlayerFleets.Values)
				{
					foreach (FleetInfo fleetInfo5 in fleetInfoList)
					{
						if (fleetInfo5 != null && fleetInfo5.IsNormalFleet)
						{
							if (fleetInfo5.PlayerID != fleetInfo3.PlayerID)
							{
								if (fleetInfo5.PlayerID != fleetInfo4.PlayerID)
								{
									DiplomacyState stateBetweenPlayers1 = this.App.GameDatabase.GetDiplomacyStateBetweenPlayers(fleetInfo3.PlayerID, fleetInfo5.PlayerID);
									DiplomacyState stateBetweenPlayers2 = this.App.GameDatabase.GetDiplomacyStateBetweenPlayers(fleetInfo4.PlayerID, fleetInfo5.PlayerID);
									if (stateBetweenPlayers1 != DiplomacyState.ALLIED)
									{
										if (stateBetweenPlayers2 != DiplomacyState.ALLIED)
											break;
									}
									if (stateBetweenPlayers1 == DiplomacyState.ALLIED)
										this._neutralCombatStanceInfo.FleetAAllies.Add(new NeutralCombatStanceSpawnPosition()
										{
											FleetID = fleetInfo5.ID,
											Position = this._neutralCombatStanceInfo.FleetAPos.Position + vector3_1 * num4 * (float)(this._neutralCombatStanceInfo.FleetAAllies.Count + 1),
											Facing = this._neutralCombatStanceInfo.FleetAPos.Facing
										});
									else if (stateBetweenPlayers2 == DiplomacyState.ALLIED)
										this._neutralCombatStanceInfo.FleetBAllies.Add(new NeutralCombatStanceSpawnPosition()
										{
											FleetID = fleetInfo5.ID,
											Position = this._neutralCombatStanceInfo.FleetBPos.Position + vector3_1 * num4 * (float)(this._neutralCombatStanceInfo.FleetBAllies.Count + 1),
											Facing = this._neutralCombatStanceInfo.FleetBPos.Facing
										});
								}
								else
									break;
							}
							else
								break;
						}
					}
				}
				float num5 = 2000f;
				Vector3 zero = Vector3.Zero;
				int num6 = 2 + this._neutralCombatStanceInfo.FleetAAllies.Count + this._neutralCombatStanceInfo.FleetBAllies.Count;
				foreach (NeutralCombatStanceSpawnPosition fleetAally in this._neutralCombatStanceInfo.FleetAAllies)
					zero += fleetAally.Position;
				foreach (NeutralCombatStanceSpawnPosition fleetBally in this._neutralCombatStanceInfo.FleetBAllies)
					zero += fleetBally.Position;
				Vector3 vector3_2 = zero / (float)num6;
				float lengthSquared = (this._neutralCombatStanceInfo.FleetAPos.Position - vector3_2).LengthSquared;
				float val2 = Math.Max((this._neutralCombatStanceInfo.FleetBPos.Position - vector3_2).LengthSquared, lengthSquared);
				foreach (NeutralCombatStanceSpawnPosition fleetAally in this._neutralCombatStanceInfo.FleetAAllies)
					val2 = Math.Max((fleetAally.Position - vector3_2).LengthSquared, val2);
				foreach (NeutralCombatStanceSpawnPosition fleetBally in this._neutralCombatStanceInfo.FleetBAllies)
					val2 = Math.Max((fleetBally.Position - vector3_2).LengthSquared, val2);
				float num7 = (float)Math.Sqrt((double)val2);
				float num8 = num5 + num7;
				List<StellarBody> planetsInSystem = this._starSystemObjects.GetPlanetsInSystem();
				float starRadius = this._starSystemObjects.GetStarRadius();
				float num9 = this._starSystemObjects.CombatZones.Count <= 0 ? Math.Max(125000f - num8, 10000f) : Math.Max(this._starSystemObjects.CombatZones.Last<CombatZonePositionInfo>().RadiusUpper - num8, 10000f);
				if ((double)starRadius > 0.0)
					num9 = Math.Max(num8 + 7500f + starRadius, num9);
				Vector3 safeRandSpawnPos = Vector3.Zero;
				bool flag = false;
				while (!flag)
				{
					flag = true;
					safeRandSpawnPos.X = this._random.NextInclusive(-num9, num9);
					safeRandSpawnPos.Z = this._random.NextInclusive(-num9, num9);
					if ((double)starRadius > 0.0)
					{
						float num10 = (float)((double)starRadius + (double)num8 + 7500.0);
						if ((double)safeRandSpawnPos.LengthSquared < (double)num10 * (double)num10)
							flag = false;
					}
					if (flag)
					{
						foreach (StellarBody stellarBody in planetsInSystem)
						{
							float num10 = (float)((double)stellarBody.Parameters.Radius + (double)num8 + 750.0);
							if ((double)(stellarBody.Parameters.Position - safeRandSpawnPos).LengthSquared < (double)num10 * (double)num10)
							{
								flag = false;
								break;
							}
							if (!flag)
								break;
						}
					}
				}
				safeRandSpawnPos -= vector3_2;
				this._neutralCombatStanceInfo.FleetAPos.Position += safeRandSpawnPos;
				this._neutralCombatStanceInfo.FleetAAllies.ForEach((Action<NeutralCombatStanceSpawnPosition>)(x => x.Position += safeRandSpawnPos));
				this._neutralCombatStanceInfo.FleetBPos.Position += safeRandSpawnPos;
				this._neutralCombatStanceInfo.FleetBAllies.ForEach((Action<NeutralCombatStanceSpawnPosition>)(x => x.Position += safeRandSpawnPos));
			}
		}

		private SpawnProfile GetSpawnInfoAtNeutralSystem(
		  FleetInfo fleet,
		  SpawnProfile spawnPos,
		  IDictionary<int, List<FleetInfo>> selectedPlayerFleets,
		  ref int spawnedFleetCount)
		{
			if (this._neutralCombatStanceInfo.Stance == NeutralCombatStance.Invalid || this._neutralCombatStanceInfo.Stance == NeutralCombatStance.None)
				return (SpawnProfile)null;
			if (this._neutralCombatStanceInfo.FleetAPos.FleetID == fleet.ID)
			{
				spawnPos._spawnPosition = this._neutralCombatStanceInfo.FleetAPos.Position;
				spawnPos._spawnFacing = this._neutralCombatStanceInfo.FleetAPos.Facing;
				spawnPos._startingPosition = spawnPos._spawnPosition;
				return spawnPos;
			}
			if (this._neutralCombatStanceInfo.FleetBPos.FleetID == fleet.ID)
			{
				spawnPos._spawnPosition = this._neutralCombatStanceInfo.FleetBPos.Position;
				spawnPos._spawnFacing = this._neutralCombatStanceInfo.FleetBPos.Facing;
				spawnPos._startingPosition = spawnPos._spawnPosition;
				return spawnPos;
			}
			NeutralCombatStanceSpawnPosition stanceSpawnPosition = this._neutralCombatStanceInfo.FleetAAllies.FirstOrDefault<NeutralCombatStanceSpawnPosition>((Func<NeutralCombatStanceSpawnPosition, bool>)(x => x.FleetID == fleet.ID)) ?? this._neutralCombatStanceInfo.FleetBAllies.FirstOrDefault<NeutralCombatStanceSpawnPosition>((Func<NeutralCombatStanceSpawnPosition, bool>)(x => x.FleetID == fleet.ID));
			if (stanceSpawnPosition == null)
				return (SpawnProfile)null;
			spawnPos._spawnPosition = stanceSpawnPosition.Position;
			spawnPos._spawnFacing = stanceSpawnPosition.Facing;
			spawnPos._startingPosition = spawnPos._spawnPosition;
			return spawnPos;
		}

		private SpawnProfile GetSpawnInfoFromCombatZone(
		  FleetInfo fleet,
		  SpawnProfile spawnPos,
		  IDictionary<int, List<FleetInfo>> selectedPlayerFleets,
		  ref int spawnedFleetCount)
		{
			if (this.App.GameDatabase.GetStarSystemInfo(this._systemId).ControlZones == null)
				return (SpawnProfile)null;
			float num = float.MaxValue;
			CombatZonePositionInfo czpi = (CombatZonePositionInfo)null;
			Vector3 bestPlanetPos = Vector3.Zero;
			List<StarModel> stars = new List<StarModel>();
			List<StellarBody> planets = new List<StellarBody>();
			foreach (IGameObject gameObject in this._starSystemObjects.Crits.Objects.Where<IGameObject>((Func<IGameObject, bool>)(x =>
		   {
			   if (!(x is StellarBody))
				   return x is StarModel;
			   return true;
		   })).ToList<IGameObject>())
			{
				if (gameObject is StarModel)
					stars.Add(gameObject as StarModel);
				else if (gameObject is StellarBody)
				{
					StellarBody stellarBody = gameObject as StellarBody;
					if (stellarBody != null)
						planets.Add(stellarBody);
					ColonyInfo colonyInfoForPlanet = this.App.GameDatabase.GetColonyInfoForPlanet(stellarBody.Parameters.OrbitalID);
					if (stellarBody != null && colonyInfoForPlanet != null && this.GetDiplomacyState(fleet.PlayerID, colonyInfoForPlanet.PlayerID) == DiplomacyState.WAR)
					{
						CombatZonePositionInfo closestZoneToPosition = this._starSystemObjects.GetClosestZoneToPosition(this.App, fleet.PlayerID, stellarBody.Parameters.Position);
						if (closestZoneToPosition != null)
						{
							float lengthSquared = (closestZoneToPosition.Center - stellarBody.Parameters.Position).LengthSquared;
							if ((double)lengthSquared < (double)num)
							{
								num = lengthSquared;
								czpi = closestZoneToPosition;
								bestPlanetPos = stellarBody.Parameters.Position;
							}
						}
						else
							break;
					}
				}
			}
			return this.GetSpawnProfileFromCombatZone(spawnPos, czpi, bestPlanetPos, planets, stars);
		}

		private SpawnProfile GetSpawnInfoForDefaultControlZone(
		  FleetInfo fleet,
		  SpawnProfile spawnPos,
		  IDictionary<int, List<FleetInfo>> selectedPlayerFleets,
		  ref int spawnedFleetCount)
		{
			List<CombatZonePositionInfo> list = this._starSystemObjects.CombatZones.Where<CombatZonePositionInfo>((Func<CombatZonePositionInfo, bool>)(x => x.Player == fleet.PlayerID)).ToList<CombatZonePositionInfo>();
			if (list.Count > 0)
			{
				list.Sort((Comparison<CombatZonePositionInfo>)((x, y) => x.Center.LengthSquared.CompareTo(y.Center.LengthSquared)));
				float num = -1f;
				int maxValue = 0;
				foreach (CombatZonePositionInfo zonePositionInfo in list)
				{
					if ((double)num < 0.0)
						num = zonePositionInfo.Center.LengthSquared;
					else if ((double)zonePositionInfo.Center.LengthSquared <= (double)num)
						++maxValue;
					else
						break;
				}
				if ((double)num > 0.0)
				{
					List<StarModel> stars = new List<StarModel>();
					List<StellarBody> planets = new List<StellarBody>();
					foreach (IGameObject gameObject in this._starSystemObjects.Crits.Objects.Where<IGameObject>((Func<IGameObject, bool>)(x =>
				   {
					   if (!(x is StellarBody))
						   return x is StarModel;
					   return true;
				   })).ToList<IGameObject>())
					{
						if (gameObject is StarModel)
							stars.Add(gameObject as StarModel);
						else if (gameObject is StellarBody)
							planets.Add(gameObject as StellarBody);
					}
					int index = this._random.NextInclusive(0, maxValue);
					return this.GetSpawnProfileFromCombatZone(spawnPos, list[index], Vector3.Zero, planets, stars);
				}
			}
			return (SpawnProfile)null;
		}

		private SpawnProfile GetSpawnInfoForFriendlySystem(
		  FleetInfo fleet,
		  SpawnProfile spawnPos,
		  IDictionary<int, List<FleetInfo>> selectedPlayerFleets,
		  ref int spawnedFleetCount)
		{
			if (this._systemId != 0)
			{
				foreach (PlanetInfo systemPlanetInfo in this.App.GameDatabase.GetStarSystemPlanetInfos(this._systemId))
				{
					ColonyInfo colonyInfoForPlanet = this.App.GameDatabase.GetColonyInfoForPlanet(systemPlanetInfo.ID);
					if (colonyInfoForPlanet != null && this.App.GameDatabase.GetDiplomacyStateBetweenPlayers(fleet.PlayerID, colonyInfoForPlanet.PlayerID) == DiplomacyState.ALLIED)
					{
						Vector3 position = this.App.GameDatabase.GetOrbitalTransform(systemPlanetInfo.ID).Position;
						float radius = StarSystemVars.Instance.SizeToRadius(systemPlanetInfo.Size);
						spawnPos._startingPosition = position;
						Vector3 vector3_1 = Vector3.Normalize(spawnPos._startingPosition) * (float)((double)radius + 7000.0 + 1000.0 * (double)spawnedFleetCount);
						spawnPos._startingPosition += vector3_1;
						spawnPos._spawnFacing = Vector3.Normalize(spawnPos._startingPosition);
						List<Ship> stationsAroundPlanet = this._starSystemObjects.GetStationsAroundPlanet(systemPlanetInfo.ID);
						if (stationsAroundPlanet.Count > 0)
						{
							foreach (Ship station in stationsAroundPlanet)
							{
								StationInfo stationInfo = this._starSystemObjects.GetStationInfo(station);
								if (stationInfo != null && stationInfo.DesignInfo.StationType == StationType.NAVAL && stationInfo.DesignInfo.StationLevel > 0)
								{
									Vector3 vector3_2 = station.Position - position;
									double num = (double)vector3_2.Normalize();
									spawnPos._startingPosition = station.Position + vector3_2 * 3500f;
									Vector3 vector3_3 = Vector3.Normalize(spawnPos._startingPosition) * (float)(2000.0 + 1000.0 * (double)spawnedFleetCount);
									spawnPos._startingPosition += vector3_3;
								}
							}
						}
						spawnPos._spawnPosition = spawnPos._startingPosition;
						return spawnPos;
					}
				}
			}
			return (SpawnProfile)null;
		}

		private SpawnProfile GetSpawnProfileFromCombatZone(
		  SpawnProfile spawnPos,
		  CombatZonePositionInfo czpi,
		  Vector3 bestPlanetPos,
		  List<StellarBody> planets,
		  List<StarModel> stars)
		{
			if (czpi != null)
			{
				float radians = MathHelper.DegreesToRadians(5f);
				Vector3 vector3 = czpi.Center;
				bool flag = true;
				for (int index = 30; !flag && index > 0; --index)
				{
					flag = true;
					vector3 = Matrix.CreateRotationYPR(this._random.NextInclusive(czpi.AngleLeft + radians, czpi.AngleRight - radians), 0.0f, 0.0f).Forward * this._random.NextInclusive(czpi.RadiusLower + 2000f, czpi.RadiusUpper - 2000f);
					foreach (StarModel star in stars)
					{
						float num = (float)((double)star.Radius + 2000.0 + 7500.0);
						if ((double)(star.Position - vector3).LengthSquared < (double)num * (double)num)
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						foreach (StellarBody planet in planets)
						{
							float num = (float)((double)planet.Parameters.Radius + 2000.0 + 750.0);
							if ((double)(planet.Parameters.Position - vector3).LengthSquared < (double)num * (double)num)
							{
								flag = false;
								break;
							}
						}
					}
				}
				if (flag)
				{
					spawnPos._spawnPosition = vector3;
					spawnPos._spawnFacing = bestPlanetPos - vector3;
					double num = (double)spawnPos._spawnFacing.Normalize();
					spawnPos._startingPosition = vector3;
					return spawnPos;
				}
			}
			return (SpawnProfile)null;
		}

		private SpawnProfile GetSpawnProfileForEnterSystem(
		  FleetInfo fleet,
		  Faction faction,
		  SpawnProfile spawnPos)
		{
			if (fleet.PreviousSystemID.HasValue)
			{
				int? previousSystemId = fleet.PreviousSystemID;
				int systemId = this._systemId;
				if ((previousSystemId.GetValueOrDefault() != systemId ? 0 : (previousSystemId.HasValue ? 1 : 0)) == 0)
				{
					EntrySpawnLocation entrySpawnLocation = this._entryLocations.FirstOrDefault<EntrySpawnLocation>((Func<EntrySpawnLocation, bool>)(x =>
				   {
					   if (x.PreviousSystemID == fleet.PreviousSystemID.Value)
						   return x.FactionID == faction.ID;
					   return false;
				   }));
					if (entrySpawnLocation == null)
						return (SpawnProfile)null;
					spawnPos._spawnFacing = -Vector3.Normalize(entrySpawnLocation.Position);
					if (faction.Name == "zuul" && this.App.GameDatabase.PlayerHasTech(fleet.PlayerID, "DRV_Star_Tear"))
						spawnPos._startingPosition = -(spawnPos._spawnFacing * (this._starSystemObjects.GetStarRadius() + faction.StarTearTechEnteryPointOffset));
					else
						spawnPos._spawnPosition = entrySpawnLocation.Position - spawnPos._spawnFacing * (faction.EntryPointOffset + 1500f);
					return this.GetSafeEntryPosition(spawnPos);
				}
			}
			return (SpawnProfile)null;
		}

		private SpawnProfile GetSafeEntryPosition(SpawnProfile spawnPos)
		{
			bool flag = true;
			foreach (SpawnProfile spawnPosition in this._spawnPositions)
			{
				if (spawnPosition.SpawnProfileOverlaps(spawnPos))
				{
					flag = false;
					break;
				}
			}
			if (!flag)
			{
				flag = true;
				spawnPos._spawnPosition.Y -= 400f;
				foreach (SpawnProfile spawnPosition in this._spawnPositions)
				{
					if (spawnPosition.SpawnProfileOverlaps(spawnPos))
					{
						flag = false;
						break;
					}
				}
			}
			if (!flag)
			{
				flag = true;
				spawnPos._spawnPosition.Y += 800f;
				foreach (SpawnProfile spawnPosition in this._spawnPositions)
				{
					if (spawnPosition.SpawnProfileOverlaps(spawnPos))
					{
						flag = false;
						break;
					}
				}
			}
			if (!flag)
			{
				spawnPos._spawnPosition.Y -= 400f;
				while (!flag)
				{
					flag = true;
					spawnPos._spawnPosition.X += 3000f;
					foreach (SpawnProfile spawnPosition in this._spawnPositions)
					{
						if (spawnPosition.SpawnProfileOverlaps(spawnPos))
						{
							flag = false;
							break;
						}
					}
				}
			}
			spawnPos._startingPosition = spawnPos._spawnPosition;
			return spawnPos;
		}

		private SpawnProfile GetSpawnProfileForInitialAttack(
		  FleetInfo fleet,
		  SpawnProfile spawnPos,
		  PlanetInfo furthestTarget,
		  float furthestOrbitDist,
		  float edgeDistance,
		  IDictionary<int, List<FleetInfo>> selectedPlayerFleets,
		  ref int spawnedFleetCount)
		{
			if (furthestTarget == null)
				return (SpawnProfile)null;
			Matrix orbitalTransform = this.App.GameDatabase.GetOrbitalTransform(furthestTarget.ID);
			int index1 = this._starSystemObjects.GetCombatZoneRingAtRange(furthestOrbitDist) + 1;
			int combatZoneInRing = this._starSystemObjects.GetCombatZoneInRing(index1, orbitalTransform.Position);
			int num1 = Math.Min(Math.Max(this._playersInCombat.Count / 2, 1), 3);
			int num2 = combatZoneInRing - num1;
			int num3 = num1 * 2 + 1;
			List<int> intList = new List<int>();
			for (int index2 = 0; index2 < num3; ++index2)
			{
				int num4 = num2 + index2;
				if (num4 < 0)
					num4 = StarSystem.CombatZoneMapAngleDivs[index1] + num4;
				intList.Add(num4);
			}
			foreach (SpawnProfile spawnPosition in this._spawnPositions)
			{
				CombatZonePositionInfo closestZoneToPosition = this._starSystemObjects.GetClosestZoneToPosition(this.App, 0, spawnPosition._spawnPosition);
				if (closestZoneToPosition != null && closestZoneToPosition.RingIndex == index1)
				{
					foreach (int num4 in intList)
					{
						if (num4 == closestZoneToPosition.ZoneIndex)
						{
							intList.Remove(num4);
							break;
						}
					}
				}
			}
			if (intList.Count == 0)
				return (SpawnProfile)null;
			int index3 = this._random.NextInclusive(0, intList.Count - 1);
			CombatZonePositionInfo zonePositionInfo = this._starSystemObjects.GetCombatZonePositionInfo(index1, intList[index3]);
			if (zonePositionInfo == null)
				return (SpawnProfile)null;
			spawnPos._spawnPosition = Vector3.Normalize(zonePositionInfo.Center) * (float)((double)zonePositionInfo.RadiusLower + (double)edgeDistance + (double)spawnedFleetCount * 1000.0);
			spawnPos._spawnFacing = Vector3.Normalize(orbitalTransform.Position - spawnPos._spawnPosition);
			spawnPos._startingPosition = spawnPos._spawnPosition;
			return spawnPos;
		}

		private int SelectOriginOrbital(int systemId)
		{
			if (systemId == 0)
				return 0;
			PlanetInfo[] systemPlanetInfos = this.App.GameDatabase.GetStarSystemPlanetInfos(systemId);
			PlanetInfo planetInfo1 = ((IEnumerable<PlanetInfo>)systemPlanetInfos).FirstOrDefault<PlanetInfo>((Func<PlanetInfo, bool>)(x => this.App.GameDatabase.GetColonyInfoForPlanet(x.ID) != null));
			if (planetInfo1 != null)
				return planetInfo1.ID;
			PlanetInfo planetInfo2 = ((IEnumerable<PlanetInfo>)systemPlanetInfos).FirstOrDefault<PlanetInfo>((Func<PlanetInfo, bool>)(x => StellarBodyTypes.IsTerrestrial(x.Type.ToLowerInvariant())));
			if (planetInfo2 != null)
				return planetInfo2.ID;
			PlanetInfo planetInfo3 = ((IEnumerable<PlanetInfo>)systemPlanetInfos).FirstOrDefault<PlanetInfo>((Func<PlanetInfo, bool>)(x => x.Type.ToLowerInvariant() == StellarBodyTypes.Gaseous));
			if (planetInfo3 != null)
				return planetInfo3.ID;
			return 0;
		}

		protected override void OnEnter()
		{
			this._subState = CommonCombatState.CombatSubState.Running;
			this._combatEndingStatsGathered = false;
			this._combatEndDelayComplete = false;
			this._camera.MinDistance = 6f;
			this._camera.DesiredDistance = 1000f;
			this._camera.DesiredYaw = MathHelper.DegreesToRadians(-215f);
			this._camera.DesiredPitch = MathHelper.DegreesToRadians(-10f);
			this._camera.MaxDistance = 2000000f;
			this._grid.GridSize = 5000f;
			this._grid.CellSize = 500f;
			int num1 = 0;
			List<Player> source1 = new List<Player>();
			foreach (CombatAI aiCommander in this.AI_Commanders)
			{
				if (aiCommander.m_Player.IsStandardPlayer && aiCommander.m_bIsHumanPlayerControlled)
				{
					++num1;
					source1.Add(aiCommander.m_Player);
				}
			}
			foreach (StellarBody stellarBody in this._starSystemObjects.Crits.Objects.OfType<StellarBody>().ToList<StellarBody>())
			{
				StellarBody p = stellarBody;
				if (!source1.Any<Player>((Func<Player, bool>)(x => x.ID == p.Parameters.ColonyPlayerID)))
				{
					Player player = this.App.GetPlayer(p.Parameters.ColonyPlayerID);
					if (this.isHumanControlledAI(player))
					{
						++num1;
						source1.Add(player);
					}
				}
			}
			this._input.PlayerId = this.App.LocalPlayer.ObjectID;
			this._input.CameraID = this._camera.ObjectID;
			this._input.CombatGridID = this._grid.ObjectID;
			this._input.CombatSensorID = this._sensor.ObjectID;
			this._input.CombatID = this._combat.ObjectID;
			this._input.EnableTimeScale = num1 == 1;
			foreach (IActive active in this._crits.OfType<IActive>())
				active.Active = true;
			foreach (SpawnProfile spawnPosition in this._spawnPositions)
			{
				foreach (int activeShip in spawnPosition._activeShips)
				{
					if (this._ships.Forward.ContainsKey(activeShip))
						this._ships.Forward[activeShip].Active = true;
				}
			}
			IEnumerable<IGameObject> source2 = this._crits.Objects.Where<IGameObject>((Func<IGameObject, bool>)(x => x is Ship));
			GameObject gameObject1 = (GameObject)null;
			foreach (Ship ship in source2)
			{
				if (ship.Player.ID == this.App.LocalPlayer.ID && (gameObject1 == null || (gameObject1 as Ship).ShipClass == ShipClass.Station && ship.ShipClass != ShipClass.Station))
					gameObject1 = (GameObject)ship;
				ship.SyncAltitude();
			}
			if (gameObject1 == null)
			{
				foreach (IGameObject gameObject2 in this._starSystemObjects.Crits.Objects.Where<IGameObject>((Func<IGameObject, bool>)(x => x is StellarBody)))
				{
					StellarBody stellarBody = gameObject2 as StellarBody;
					if (stellarBody != null && stellarBody.Parameters.ColonyPlayerID == this.App.LocalPlayer.ID)
					{
						gameObject1 = (GameObject)stellarBody;
						break;
					}
				}
			}
			if (gameObject1 != null)
				this._camera.TargetID = gameObject1.ObjectID;
			this.App.PostPlayMusic(string.Format("Combat_{0}", (object)this.App.GameDatabase.GetFactionName(this.App.GameDatabase.GetPlayerFactionID(this.App.LocalPlayer.ID))) + (this.isSmallScaleCombat() ? "_low" : ""));
			this.m_SlewMode = false;
			this._combat.PostSetProp("SetSlewMode", this.m_SlewMode);
			bool flag = false;
			foreach (Player player1 in this._playersInCombat)
			{
				foreach (Player player2 in this._playersInCombat)
				{
					if (this.GetDiplomacyState(player1.ID, player2.ID) == DiplomacyState.WAR)
						flag = true;
				}
			}
			this._combatStanceMap.Clear();
			foreach (Player player1 in this._playersInCombat)
			{
				this._combatStanceMap.Add(player1.ID, new Dictionary<int, bool>());
				foreach (Player player2 in this._playersInCombat)
				{
					if (player1 != player2)
					{
						DiplomacyState diplomacyState = this.GetDiplomacyState(player1.ID, player2.ID);
						this._combatStanceMap[player1.ID].Add(player2.ID, diplomacyState != DiplomacyState.WAR && (flag || diplomacyState != DiplomacyState.NEUTRAL));
					}
				}
			}
			if (!this.SimMode)
			{
				this._camera.DesiredDistance = 250f;
				this._camera.PostSetProp("SinglePassAttractMode", (object)true, (object)this._camera.TargetID, (object)5, (object)Vector3.DegreesToRadians(new Vector3(0.0f, -30f, 0.0f)), (object)Vector3.DegreesToRadians(new Vector3(0.0f, -30f, 0.0f)), (object)15f, (object)15f);
				this._input.PostSetProp("HookHotKeys", this.App.HotKeyManager.GetObjectID());
			}
			this._combat.PostSetProp("CombatStart", this.SimMode || !this.App.GameSetup.IsMultiplayer);
			if (this._lastPendingCombat.SelectedPlayerFleets.ContainsKey(this.App.Game.ScriptModules.VonNeumann.PlayerID))
			{
				List<ShipInfo> list = this.App.GameDatabase.GetShipInfoByFleetID(this._lastPendingCombat.SelectedPlayerFleets[this.App.Game.ScriptModules.VonNeumann.PlayerID], true).ToList<ShipInfo>();
				if (list.Any<ShipInfo>((Func<ShipInfo, bool>)(x => x.DesignID == VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.CollectorMothership].DesignId)))
				{
					foreach (int num2 in this._lastPendingCombat.PlayersInCombat)
						this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_VN_COLLECTOR_ATTACK,
							EventMessage = TurnEventMessage.EM_VN_COLLECTOR_ATTACK,
							PlayerID = num2,
							SystemID = this._lastPendingCombat.SystemID,
							TurnNumber = this.App.GameDatabase.GetTurnCount()
						});
				}
				else if (list.Any<ShipInfo>((Func<ShipInfo, bool>)(x => x.DesignID == VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.SeekerMothership].DesignId)))
				{
					foreach (int num2 in this._lastPendingCombat.PlayersInCombat)
						this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_VN_SEEKER_ATTACK,
							EventMessage = TurnEventMessage.EM_VN_SEEKER_ATTACK,
							PlayerID = num2,
							SystemID = this._lastPendingCombat.SystemID,
							TurnNumber = this.App.GameDatabase.GetTurnCount()
						});
				}
				else if (list.Any<ShipInfo>((Func<ShipInfo, bool>)(x => x.DesignID == VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.BerserkerMothership].DesignId)))
				{
					foreach (int num2 in this._lastPendingCombat.PlayersInCombat)
						this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_VN_BERSERKER_ATTACK,
							EventMessage = TurnEventMessage.EM_VN_BERSERKER_ATTACK,
							PlayerID = num2,
							SystemID = this._lastPendingCombat.SystemID,
							TurnNumber = this.App.GameDatabase.GetTurnCount()
						});
				}
				else if (list.Any<ShipInfo>((Func<ShipInfo, bool>)(x => x.DesignID == VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.PlanetKiller].DesignId)))
				{
					foreach (int num2 in this._lastPendingCombat.PlayersInCombat)
						this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_VN_SYS_KILLER_ATTACK,
							EventMessage = TurnEventMessage.EM_VN_SYS_KILLER_ATTACK,
							PlayerID = num2,
							SystemID = this._lastPendingCombat.SystemID,
							TurnNumber = this.App.GameDatabase.GetTurnCount()
						});
				}
			}
			if (this._pirateEncounterData.IsPirateEncounter || this.App.Game == null || (this.App.Game.ScriptModules == null || this._playersInCombat.Any<Player>((Func<Player, bool>)(x => this.App.Game.ScriptModules.IsEncounterPlayer(x.ID)))))
				return;
			Player player3 = this._playersInCombat.FirstOrDefault<Player>((Func<Player, bool>)(x => x == this.App.Game.LocalPlayer));
			if (player3 == null)
				return;
			foreach (Ship ship in source2.OfType<Ship>().Where<Ship>((Func<Ship, bool>)(x => x.IsSuulka)).ToList<Ship>())
			{
				if (ship.Player != player3 && this.GetDiplomacyState(player3.ID, ship.Player.ID) == DiplomacyState.WAR)
				{
					this.App.PostRequestSpeech(string.Format("COMBAT_126-01_{0}_EnterBattleWithSuulka", (object)player3.Faction.Name), 50, 20, 0.0f);
					break;
				}
			}
		}

		protected bool isSmallScaleCombat()
		{
			return this._crits.Objects.Where<IGameObject>((Func<IGameObject, bool>)(x => x is Ship)).Count<IGameObject>() < this.App.AssetDatabase.LargeCombatThreshold;
		}

		protected override void OnExit(GameState prev, ExitReason reason)
		{
			this._starSystemObjects = (StarSystem)null;
			this.m_SlewMode = false;
			if (!this._sim)
				this._input.PostSetProp("UnHookHotKeys", this.App.HotKeyManager.GetObjectID());
			if (this._crits != null)
			{
				Player[] array = GameSession.GetPlayersWithCombatAssets(this.App, this._systemId).ToArray<Player>();
				foreach (IGameObject crit in this._crits)
				{
					if (crit is Ship)
					{
						Ship ship = crit as Ship;
						if (ship.IsDestroyed)
						{
							foreach (Player player in array)
							{
								if (this.App.GameDatabase.GetPlayerInfo(player.ID).isStandardPlayer && player.ID != ship.Player.ID)
								{
									int id = player.ID;
									if (this.App.GameDatabase.GetPlayerInfo(ship.Player.ID).isStandardPlayer)
										this.App.GameDatabase.ApplyDiplomacyReaction(ship.Player.ID, id, ship.GetCruiserEquivalent(), new StratModifiers?(StratModifiers.DiplomacyReactionKillShips), 1);
									foreach (int standardPlayerId in this.App.GameDatabase.GetStandardPlayerIDs())
									{
										if (standardPlayerId != ship.Player.ID && standardPlayerId != player.ID && this.App.GameDatabase.GetDiplomacyStateBetweenPlayers(standardPlayerId, ship.Player.ID) == DiplomacyState.WAR)
											this.App.GameDatabase.ApplyDiplomacyReaction(standardPlayerId, id, ship.GetCruiserEquivalent(), new StratModifiers?(StratModifiers.DiplomacyReactionKillEnemy), 1);
									}
								}
							}
							if (ship.Faction.Name == "zuul" && ship.ShipClass == ShipClass.Leviathan)
							{
								foreach (int playerId in this.App.GameDatabase.GetPlayerIDs())
								{
									if (playerId != ship.Player.ID)
									{
										foreach (Player player in array)
										{
											if (playerId != player.ID)
												this.App.GameDatabase.ApplyDiplomacyReaction(playerId, player.ID, StratModifiers.DiplomacyReactionKillSuulka, 1);
										}
									}
								}
							}
						}
					}
				}
				this._crits.Dispose();
				this._crits = (GameObjectSet)null;
			}
			this.App.ObjectReleased -= new Action<IGameObject>(this.Game_ObjectReleased);
			this._ships = (BidirMap<int, Ship>)null;
			foreach (CombatAI aiCommander in this.AI_Commanders)
				aiCommander.Shutdown();
			this.AI_Commanders.Clear();
			this._pirateEncounterData.Clear();
			this._trapCombatData.Clear();
			this._spawnPositions.Clear();
			this._hitByNodeCannon.Clear();
			this._mediaHeroFleets.Clear();
			this._pointsOfInterest.Clear();
			this._playersInCombat.Clear();
			this._ignoreCombatZonePlayers.Clear();
			this._playersWithAssets.Clear();
			this._combatStanceMap.Clear();
			if (this._entryLocations != null)
				this._entryLocations.Clear();
			if (this._fleetsPerPlayer != null)
				this._fleetsPerPlayer.Clear();
			if (this._ships != null)
				this._ships.Clear();
			if (this._initialDiploStates != null)
				this._initialDiploStates.Clear();
			this._combatConfig = (XmlDocument)null;
			this._gameObjectConfigs = (Dictionary<IGameObject, XmlElement>)null;
		}

		public bool EndCombat()
		{
			if (this._subState == CommonCombatState.CombatSubState.Ending || this._subState == CommonCombatState.CombatSubState.Ended)
				return false;
			foreach (SwarmerInfo si in this.App.GameDatabase.GetSwarmerInfos().ToList<SwarmerInfo>())
			{
				if (si.QueenFleetId.HasValue)
				{
					FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(si.QueenFleetId.Value);
					if (fleetInfo != null && fleetInfo.SystemID == this._systemId)
						Swarmers.ClearTransform(this.App.GameDatabase, si);
				}
			}
			this._subState = CommonCombatState.CombatSubState.Ending;
			this.App.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_COMBAT_ENDED);
			this.OnCombatEnding();
			return true;
		}

		private void UpdateRunning()
		{
			List<IGameObject> gameObjectList = new List<IGameObject>();
			foreach (IGameObject postLoadedObject in this._postLoadedObjects)
			{
				if (postLoadedObject.ObjectStatus == GameObjectStatus.Ready && !this._isPaused)
				{
					if (postLoadedObject is IActive)
						(postLoadedObject as IActive).Active = true;
					this._crits.Add(postLoadedObject);
					gameObjectList.Add(postLoadedObject);
					if (postLoadedObject is PointOfInterest)
						this._pointsOfInterest.Add(postLoadedObject as PointOfInterest);
				}
			}
			if (gameObjectList.Count > 0)
				this._combat.PostObjectAddObjects(gameObjectList.ToArray());
			foreach (IGameObject gameObject in gameObjectList)
				this._postLoadedObjects.Remove(gameObject);
			if (this._isPaused || !this._engCombatActive)
				return;
			List<IGameObject> combatGameObjects = CombatAI.GetCombatGameObjects(this.Objects);
			--this._detectionUpdateRate;
			if (this._detectionUpdateRate <= 0)
			{
				this.DetectionUpdate(combatGameObjects);
				this._detectionUpdateRate = 3;
			}
			if (!this._testingState && this.CheckVictory() && this._authority)
				this.EndCombat();
			foreach (CombatAI aiCommander in this.AI_Commanders)
				aiCommander.Update(combatGameObjects);
		}

		protected virtual GameState GetExitState()
		{
			return this.App.PreviousState;
		}

		private bool UpdateEnding()
		{
			if (!this._combatEndingStatsGathered || !this._combatEndDelayComplete)
				return false;
			GameState exitState = this.GetExitState();
			this.App.SwitchGameState(exitState == this ? (GameState)this.App.GetGameState<StarMapState>() : exitState ?? (GameState)this.App.GetGameState<MainMenuState>());
			return true;
		}

		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_START_SENDINGDATA(
		  ScriptMessageReader mr)
		{
			this._combatData = this.App.Game.CombatData.AddCombat(this._combatId, this._systemId, this.App.GameDatabase.GetTurnCount());
			this._combatEndingStatsGathered = false;
			this._lastPendingCombat.CombatResults = new PostCombatData();
			foreach (Player playersWithCombatAsset in GameSession.GetPlayersWithCombatAssets(this.App, this._lastPendingCombat.SystemID))
				this._lastPendingCombat.CombatResults.PlayersInCombat.Add(playersWithCombatAsset.ID);
		}

		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_COMBAT_DATA_SHIP(
		  ScriptMessageReader mr)
		{
			int id1 = mr.ReadInteger();
			ShipClass shipClass = (ShipClass)mr.ReadInteger();
			Vector3 position = new Vector3();
			Vector3 forward = new Vector3();
			position.X = mr.ReadSingle();
			position.Y = mr.ReadSingle();
			position.Z = mr.ReadSingle();
			forward.X = mr.ReadSingle();
			forward.Y = mr.ReadSingle();
			forward.Z = mr.ReadSingle();
			int kills = mr.ReadInteger();
			float damageDealt = mr.ReadSingle();
			float damageReceived = 0.0f;
			int power = mr.ReadInteger();
			double slaves = mr.ReadDouble();
			Ship gameObject = this.App.GetGameObject<Ship>(id1);
			ShipInfo si = this.App.GameDatabase.GetShipInfo(gameObject.DatabaseID, true);
			List<SectionInstanceInfo> source1 = new List<SectionInstanceInfo>();
			if (si != null)
			{
				if (shipClass != ShipClass.Station && shipClass != ShipClass.BattleRider)
				{
					FleetInfo fi = this.App.GameDatabase.GetFleetInfo(si.FleetID);
					if (fi != null && !this._lastPendingCombat.CombatResults.FleetsInCombat.Any<FleetInfo>((Func<FleetInfo, bool>)(x => x.ID == fi.ID)))
						this._lastPendingCombat.CombatResults.FleetsInCombat.Add(fi);
				}
				CommonCombatState.Trace(string.Format("- Ship Combat Data Id: {0} Class: {1} kill Count: {2}, damageApplied: {3} psionicPowerRemaining:{4} -", (object)gameObject.DatabaseID, (object)shipClass, (object)kills, (object)damageDealt, (object)damageReceived));
				if (slaves > 0.0)
					this.App.GameDatabase.UpdateShipObtainedSlaves(si.ID, slaves);
				if (!si.DesignInfo.isAttributesDiscovered)
				{
					si.DesignInfo.isAttributesDiscovered = true;
					foreach (SectionEnumerations.DesignAttribute designAttribute in this.App.GameDatabase.GetDesignAttributesForDesign(si.DesignID).ToList<SectionEnumerations.DesignAttribute>())
						this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_ATTRIBUTES_DISCOVERED,
							EventMessage = TurnEventMessage.EM_ATTRIBUTES_DISCOVERED,
							PlayerID = si.DesignInfo.PlayerID,
							DesignID = si.DesignID,
							DesignAttribute = designAttribute,
							TurnNumber = this.App.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
					this.App.GameDatabase.UpdateDesignAttributeDiscovered(si.DesignInfo.ID, true);
				}
				source1 = this.App.GameDatabase.GetShipSectionInstances(si.ID).ToList<SectionInstanceInfo>();
				this.App.GameDatabase.UpdateShipPsionicPower(si.ID, power);
				if (gameObject.HasRetreated)
				{
					FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(si.FleetID);
					PlayerInfo playerInfo = fleetInfo != null ? this.App.GameDatabase.GetPlayerInfo(fleetInfo.PlayerID) : (PlayerInfo)null;
					if (playerInfo != null && playerInfo.isStandardPlayer && (!fleetInfo.IsDefenseFleet && !fleetInfo.IsGateFleet) && this.App.GameDatabase.GetStandardPlayerIDs().ToList<int>().Contains(fleetInfo.PlayerID))
					{
						int num = this.App.Game.MCCarryOverData.GetRetreatFleetID(fleetInfo.SystemID, fleetInfo.ID);
						if (num == 0 || num != fleetInfo.ID)
						{
							if (num == 0)
							{
								num = this.App.GameDatabase.InsertFleet(fleetInfo.PlayerID, 0, fleetInfo.SystemID, fleetInfo.SupportingSystemID, App.Localize("@FLEET_RETREAT"), FleetType.FL_NORMAL);
								int missionID = this.App.GameDatabase.InsertMission(num, MissionType.RETREAT, 0, 0, 0, 0, false, new int?());
								this.App.GameDatabase.InsertWaypoint(missionID, WaypointType.ReturnHome, new int?());
								this.App.GameDatabase.InsertWaypoint(missionID, WaypointType.DisbandFleet, new int?());
								this.App.Game.MCCarryOverData.SetRetreatFleetID(fleetInfo.SystemID, fleetInfo.ID, num);
							}
							List<ShipInfo> list = this.App.GameDatabase.GetShipInfoByFleetID(si.FleetID, false).ToList<ShipInfo>();
							list.RemoveAll((Predicate<ShipInfo>)(x => x.ID == si.ID));
							if (this.App.GameDatabase.GetShipsCommandPointQuota((IEnumerable<ShipInfo>)list) < this.App.GameDatabase.GetShipsCommandPointCost((IEnumerable<ShipInfo>)list))
							{
								if (num != 0)
								{
									foreach (int shipID in this.App.GameDatabase.GetShipsByFleetID(num).ToList<int>())
										this.App.GameDatabase.TransferShip(shipID, fleetInfo.ID);
									MissionInfo missionByFleetId = this.App.GameDatabase.GetMissionByFleetID(num);
									if (missionByFleetId != null)
										this.App.GameDatabase.RemoveMission(missionByFleetId.ID);
									this.App.GameDatabase.RemoveFleet(num);
									this.App.Game.MCCarryOverData.SetRetreatFleetID(fleetInfo.SystemID, fleetInfo.ID, fleetInfo.ID);
								}
								MissionInfo missionByFleetId1 = this.App.GameDatabase.GetMissionByFleetID(fleetInfo.ID);
								if (missionByFleetId1 != null)
									this.App.GameDatabase.RemoveMission(missionByFleetId1.ID);
								this.App.GameDatabase.InsertWaypoint(this.App.GameDatabase.InsertMission(fleetInfo.ID, MissionType.RETREAT, 0, 0, 0, 0, false, new int?()), WaypointType.ReturnHome, new int?());
								this.CheckToReturnControlZonesToOwner(si.DesignInfo.PlayerID);
							}
							else
								this.App.GameDatabase.TransferShip(si.ID, num);
						}
					}
				}
				else if (gameObject.HitByNodeCannon)
					this._hitByNodeCannon.Add(gameObject);
			}
			int num1 = mr.ReadInteger();
			for (int index1 = 0; index1 < num1; ++index1)
			{
				int num2 = mr.ReadInteger();
				SectionInstanceInfo sectionInstance = (SectionInstanceInfo)null;
				if (si != null)
				{
					foreach (Kerberos.Sots.GameObjects.Section section in gameObject.Sections)
					{
						Kerberos.Sots.GameObjects.Section s = section;
						if (s.ObjectID == num2)
						{
							DesignSectionInfo dsi = ((IEnumerable<DesignSectionInfo>)si.DesignInfo.DesignSections).First<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(x => x.ShipSectionAsset.Type == s.ShipSectionAsset.Type));
							sectionInstance = source1.FirstOrDefault<SectionInstanceInfo>((Func<SectionInstanceInfo, bool>)(x => x.SectionID == dsi.ID));
							break;
						}
					}
				}
				float num3 = mr.ReadSingle();
				damageReceived += num3;
				float num4 = mr.ReadSingle();
				int num5 = mr.ReadInteger();
				int num6 = mr.ReadInteger();
				int num7 = mr.ReadInteger();
				for (int index2 = 0; index2 < num7; ++index2)
				{
					if (mr.ReadInteger() == 1)
					{
						DamagePattern damagePattern = DamagePattern.Read(mr);
						if (si != null)
							sectionInstance.Armor[(ArmorSide)index2] = damagePattern;
					}
				}
				List<WeaponInstanceInfo> source2 = new List<WeaponInstanceInfo>();
				List<ModuleInstanceInfo> source3 = new List<ModuleInstanceInfo>();
				if (si != null)
				{
					if (!ShipSectionAsset.IsWeaponBattleRiderClass(gameObject.RealShipClass))
					{
						int minStructure = ((IEnumerable<DesignSectionInfo>)si.DesignInfo.DesignSections).First<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(x => x.ID == sectionInstance.SectionID)).GetMinStructure(this.App.GameDatabase, this.App.AssetDatabase);
						sectionInstance.Structure = Math.Max((int)Math.Ceiling((double)num4), minStructure);
						sectionInstance.Crew = num5;
						sectionInstance.Supply = num6;
					}
					this.App.GameDatabase.UpdateArmorInstances(sectionInstance.ID, sectionInstance.Armor);
					this.App.GameDatabase.UpdateSectionInstance(sectionInstance);
					source3 = this.App.GameDatabase.GetModuleInstances(sectionInstance.ID).ToList<ModuleInstanceInfo>();
					source2 = this.App.GameDatabase.GetWeaponInstances(sectionInstance.ID).ToList<WeaponInstanceInfo>();
				}
				int num8 = mr.ReadInteger();
				List<int> list1 = source3.Select<ModuleInstanceInfo, int>((Func<ModuleInstanceInfo, int>)(x => x.ID)).ToList<int>();
				for (int index2 = 0; index2 < num8; ++index2)
				{
					int id2 = mr.ReadInteger();
					float num9 = mr.ReadSingle();
					if (source3.Count != 0)
					{
						Module m = this.App.GetGameObject<Module>(id2);
						if (m != null)
						{
							ModuleInstanceInfo module = source3.FirstOrDefault<ModuleInstanceInfo>((Func<ModuleInstanceInfo, bool>)(x => x.ModuleNodeID == m.Attachment.NodeName));
							if (module != null)
							{
								module.Structure = Math.Max((int)Math.Ceiling((double)num9), 0);
								this.App.GameDatabase.UpdateModuleInstance(module);
								list1.Remove(module.ID);
							}
						}
					}
				}
				foreach (int num9 in list1)
				{
					int deadModID = num9;
					ModuleInstanceInfo module = source3.FirstOrDefault<ModuleInstanceInfo>((Func<ModuleInstanceInfo, bool>)(x => x.ID == deadModID));
					if (module != null)
					{
						module.Structure = 0;
						this.App.GameDatabase.UpdateModuleInstance(module);
					}
				}
				int num10 = mr.ReadInteger();
				List<int> list2 = source2.Select<WeaponInstanceInfo, int>((Func<WeaponInstanceInfo, int>)(x => x.ID)).ToList<int>();
				for (int index2 = 0; index2 < num10; ++index2)
				{
					int num9 = mr.ReadInteger();
					for (int index3 = 0; index3 < num9; ++index3)
					{
						int id2 = mr.ReadInteger();
						float num11 = mr.ReadSingle();
						if (source2.Count != 0)
						{
							Turret t = this.App.GetGameObject<Turret>(id2);
							if (t != null)
							{
								WeaponInstanceInfo weapon = source2.FirstOrDefault<WeaponInstanceInfo>((Func<WeaponInstanceInfo, bool>)(x => x.NodeName == t.NodeName));
								if (weapon != null)
								{
									weapon.Structure = Math.Max((float)Math.Ceiling((double)num11), 0.0f);
									this.App.GameDatabase.UpdateWeaponInstance(weapon);
									list2.Remove(weapon.ID);
								}
							}
						}
					}
				}
				foreach (int num9 in list2)
				{
					int deadWepID = num9;
					WeaponInstanceInfo weapon = source2.FirstOrDefault<WeaponInstanceInfo>((Func<WeaponInstanceInfo, bool>)(x => x.ID == deadWepID));
					if (weapon != null)
					{
						weapon.Structure = 0.0f;
						this.App.GameDatabase.UpdateWeaponInstance(weapon);
					}
				}
			}
			if (si != null && gameObject != null)
			{
				this._combatData.GetOrAddPlayer(gameObject.Player.ID).AddShipData(si.DesignID, damageDealt, damageReceived, kills, false);
				this.App.Game.MCCarryOverData.AddCarryOverInfo(this._systemId, si.FleetID, si.ID, Matrix.CreateWorld(position, forward, Vector3.UnitY));
			}
			CommonCombatState.Trace(string.Format("- TotalDamage:{0} -", (object)damageReceived));
		}

		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_COMBAT_DATA_PLANET(ScriptMessageReader mr)
		{
			List<int> AlliedPlayers = (from x in this._lastPendingCombat.CombatResults.PlayersInCombat
									   where x == base.App.Game.LocalPlayer.ID || base.App.GameDatabase.GetDiplomacyStateBetweenPlayers(base.App.Game.LocalPlayer.ID, x) == DiplomacyState.ALLIED
									   select x).ToList<int>();
			List<int> list = (from x in this._lastPendingCombat.CombatResults.PlayersInCombat
							  where !AlliedPlayers.Contains(x)
							  select x).ToList<int>();
			int num = mr.ReadInteger();
			mr.ReadInteger();
			bool flag = mr.ReadBool();
			CommonCombatState.Trace("Getting InteropMessageID.IMID_SCRIPT_COMBAT_DATA_PLANET for planet object ID: " + num);
			int num2 = 0;
			OrbitalObjectInfo orbitalObjectInfo = null;
			PlanetInfo planetInfo = null;
			ColonyInfo colonyInfo = null;
			StarSystemInfo system = null;
			if (this._starSystemObjects.PlanetMap.Forward.ContainsKey(base.App.GetGameObject(num)))
			{
				num2 = this._starSystemObjects.PlanetMap.Forward[base.App.GetGameObject(num)];
				orbitalObjectInfo = base.App.GameDatabase.GetOrbitalObjectInfo(num2);
				colonyInfo = base.App.GameDatabase.GetColonyInfoForPlanet(num2);
				planetInfo = base.App.GameDatabase.GetPlanetInfo(num2);
				system = ((planetInfo == null) ? null : base.App.GameDatabase.GetStarSystemInfo(orbitalObjectInfo.StarSystemID));
			}
			float num3 = mr.ReadSingle();
			if (num3 != 0f && orbitalObjectInfo != null)
			{
				this._lastPendingCombat.CombatResults.AdditionalInfo.Add(string.Format(App.Localize("@UI_POST_COMBAT_STAT_PLANET_DAMAGE"), num3, orbitalObjectInfo.Name));
			}
			int num4 = mr.ReadInteger();
			for (int i = 0; i < num4; i++)
			{
				int playerGameObjectID = mr.ReadInteger();
				Player player = this._playersInCombat.FirstOrDefault((Player x) => x.ObjectID == playerGameObjectID);
				int num5 = mr.ReadInteger();
				for (int j = 0; j < num5; j++)
				{
					WeaponEnums.PlagueType plagueType = (WeaponEnums.PlagueType)mr.ReadInteger();
					double num6 = mr.ReadDouble();
					if (plagueType != WeaponEnums.PlagueType.ZUUL)
					{
						if (orbitalObjectInfo != null)
						{
							string item = string.Format(App.Localize("@UI_POST_COMBAT_STAT_PLAGUED"), orbitalObjectInfo.Name, App.Localize("@UI_PLAGUE_" + plagueType.ToString().ToUpper()));
							this._lastPendingCombat.CombatResults.AdditionalInfo.Add(item);
						}
						if (colonyInfo != null)
						{
							GameSession.ApplyMoralEvent(base.App, MoralEvent.ME_PLAGUE_OUTBREAK, colonyInfo.PlayerID, new int?(colonyInfo.ID), null, null);
							PlagueInfo pi = new PlagueInfo
							{
								PlagueType = plagueType,
								ColonyId = colonyInfo.ID,
								LaunchingPlayer = player.ID,
								InfectedPopulationCivilian = Math.Floor(num6 * 0.75),
								InfectedPopulationImperial = Math.Floor(num6 * 0.25),
								InfectionRate = base.App.AssetDatabase.GetPlagueInfectionRate(plagueType)
							};
							base.App.GameDatabase.InsertPlagueInfo(pi);
							base.App.GameDatabase.InsertTurnEvent(new TurnEvent
							{
								EventType = TurnEventType.EV_PLAGUE_STARTED,
								EventMessage = TurnEventMessage.EM_PLAGUE_STARTED,
								PlagueType = plagueType,
								ColonyID = colonyInfo.ID,
								PlayerID = colonyInfo.PlayerID,
								TurnNumber = base.App.GameDatabase.GetTurnCount(),
								ShowsDialog = false
							});
						}
					}
				}
			}
			double num7 = Math.Floor(mr.ReadDouble());
			double num8 = Math.Floor(mr.ReadDouble());
			double num9 = 0.0;
			int num10 = mr.ReadInteger();
			List<PopulationData> list2 = new List<PopulationData>();
			if (num10 > 0)
			{
				for (int k = 0; k < num10; k++)
				{
					string civilianFactionType = mr.ReadString();
					double num11 = Math.Floor(mr.ReadDouble());
					double num12 = Math.Floor(mr.ReadDouble());
					num9 += num12;
					if (num12 != 0.0 && orbitalObjectInfo != null)
					{
						this._lastPendingCombat.CombatResults.AdditionalInfo.Add(string.Format(App.Localize("@UI_POST_COMBAT_STAT_CIVILIANS_DEAD"), civilianFactionType, num12, orbitalObjectInfo.Name));
						if (colonyInfo != null)
						{
							ColonyFactionInfo cfi = colonyInfo.Factions.First((ColonyFactionInfo x) => this.App.GameDatabase.GetFactionName(x.FactionID) == civilianFactionType);
							cfi.CivilianPop = num11;
							if (cfi.CivilianPop <= 0.0)
							{
								base.App.GameDatabase.RemoveCivilianPopulation(colonyInfo.OrbitalObjectID, cfi.FactionID);
								List<ColonyFactionInfo> list3 = colonyInfo.Factions.ToList<ColonyFactionInfo>();
								list3.RemoveAll((ColonyFactionInfo x) => x.FactionID == cfi.FactionID);
								colonyInfo.Factions = list3.ToArray();
							}
							else
							{
								base.App.GameDatabase.UpdateCivilianPopulation(cfi);
							}
						}
					}
					else if (num11 <= 0.0 && colonyInfo != null)
					{
						ColonyFactionInfo cfi = colonyInfo.Factions.FirstOrDefault((ColonyFactionInfo x) => this.App.GameDatabase.GetFactionName(x.FactionID) == civilianFactionType);
						if (cfi != null)
						{
							base.App.GameDatabase.RemoveCivilianPopulation(colonyInfo.OrbitalObjectID, cfi.FactionID);
							List<ColonyFactionInfo> list4 = colonyInfo.Factions.ToList<ColonyFactionInfo>();
							list4.RemoveAll((ColonyFactionInfo x) => x.FactionID == cfi.FactionID);
							colonyInfo.Factions = list4.ToArray();
						}
					}
					PopulationData item2;
					item2.faction = civilianFactionType;
					item2.damage = num12;
					list2.Add(item2);
				}
			}
			int num13 = (int)Math.Floor(num9 / 200000000.0);
			for (int l = 0; l < num13; l++)
			{
				if (colonyInfo != null)
				{
					GameSession.ApplyMoralEvent(base.App, MoralEvent.ME_200MILLION_CIV_DEATHS, colonyInfo.PlayerID, new int?(colonyInfo.ID), null, null);
				}
			}
			if (num8 != 0.0 && orbitalObjectInfo != null)
			{
				this._lastPendingCombat.CombatResults.AdditionalInfo.Add(string.Format(App.Localize("@UI_POST_COMBAT_STAT_IMPERIALS_DEAD"), num8, orbitalObjectInfo.Name));
			}
			float num14 = mr.ReadSingle();
			float num15 = mr.ReadSingle();
			if (num15 != 0f && orbitalObjectInfo != null)
			{
				this._lastPendingCombat.CombatResults.AdditionalInfo.Add(string.Format(App.Localize("@UI_POST_COMBAT_STAT_INFRASTRUCTURE_DESTROYED"), num15 * 100f, orbitalObjectInfo.Name));
			}
			float num16 = mr.ReadSingle();
			float num17 = mr.ReadSingle();
			if (num17 != 0f && orbitalObjectInfo != null)
			{
				this._lastPendingCombat.CombatResults.AdditionalInfo.Add(string.Format(App.Localize("@UI_POST_COMBAT_STAT_SUITABILITY_CHANGE"), num17, orbitalObjectInfo.Name));
			}
			if (planetInfo != null && colonyInfo != null)
			{
				colonyInfo.ModifyEconomyRating(base.App.GameDatabase, ColonyInfo.EconomicChangeReason.CombatInfrastructureLoss2Points, (int)Math.Floor((double)(num15 / 0.02f)));
				base.App.GameDatabase.UpdateColony(colonyInfo);
				planetInfo.Infrastructure = num14;
				planetInfo.Suitability = num16;
				planetInfo.Biosphere = (int)Math.Max((float)planetInfo.Biosphere - Math.Abs(num17) * 10f, 0f);
				base.App.GameDatabase.UpdatePlanet(planetInfo);
				List<FleetInfo> source = base.App.GameDatabase.GetFleetInfoBySystemID(system.ID, FleetType.FL_NORMAL).ToList<FleetInfo>();
				if (source.Any((FleetInfo x) => x.PlayerID == base.App.Game.ScriptModules.MeteorShower.PlayerID))
				{
					NPCFactionCombatAI npcfactionCombatAI = this.AI_Commanders.FirstOrDefault((CombatAI x) => x.m_Player.ID == base.App.Game.ScriptModules.MeteorShower.PlayerID) as NPCFactionCombatAI;
					if (npcfactionCombatAI != null && npcfactionCombatAI.PlanetsAttackedByNPC.Contains(num))
					{
						base.App.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_ASTEROID_STORM,
							EventMessage = TurnEventMessage.EM_ASTEROID_STORM,
							PlayerID = colonyInfo.PlayerID,
							ColonyID = colonyInfo.ID,
							SystemID = system.ID,
							CivilianPop = (float)(num9 + num8),
							Infrastructure = num15 * 100f,
							TurnNumber = base.App.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
					}
				}
				else if (source.Any((FleetInfo x) => x.PlayerID == base.App.Game.ScriptModules.Slaver.PlayerID))
				{
					NPCFactionCombatAI npcfactionCombatAI2 = this.AI_Commanders.FirstOrDefault((CombatAI x) => x.m_Player.ID == base.App.Game.ScriptModules.Slaver.PlayerID) as NPCFactionCombatAI;
					if (npcfactionCombatAI2 != null && npcfactionCombatAI2.PlanetsAttackedByNPC.Contains(num))
					{
						base.App.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_SLAVER_ATTACK,
							EventMessage = TurnEventMessage.EM_SLAVER_ATTACK,
							PlayerID = colonyInfo.PlayerID,
							ColonyID = colonyInfo.ID,
							OrbitalID = colonyInfo.OrbitalObjectID,
							SystemID = system.ID,
							CivilianPop = (float)num9,
							ImperialPop = (float)num8,
							TurnNumber = base.App.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
					}
				}
			}
			if (colonyInfo != null && system != null && orbitalObjectInfo != null)
			{
				if (num7 <= 0.0)
				{
					List<PlayerInfo> standardPlayers = base.App.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>();
					bool flag2 = colonyInfo.IsIndependentColony(base.App);
					if (flag2)
					{
						List<FleetInfo> list5 = base.App.GameDatabase.GetFleetInfoBySystemID(system.ID, FleetType.FL_NORMAL).ToList<FleetInfo>();
						foreach (FleetInfo fleetInfo in list5)
						{
							MissionInfo missionByFleetID = base.App.GameDatabase.GetMissionByFleetID(fleetInfo.ID);
							if (missionByFleetID != null && missionByFleetID.Type == MissionType.INVASION && missionByFleetID.TargetOrbitalObjectID == orbitalObjectInfo.ID)
							{
								base.App.GameDatabase.InsertGovernmentAction(fleetInfo.PlayerID, App.Localize("@GA_INDEPENDANTCONQUERED"), "IndependantConquered", 0, 0);
								foreach (PlayerInfo playerInfo in standardPlayers)
								{
									if (base.App.GameDatabase.GetDiplomacyInfo(playerInfo.ID, fleetInfo.PlayerID).isEncountered)
									{
										base.App.GameDatabase.ApplyDiplomacyReaction(playerInfo.ID, fleetInfo.PlayerID, StratModifiers.DiplomacyReactionInvadeIndependentWorld, 1);
									}
								}
							}
						}
					}
					base.App.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_COLONY_DESTROYED,
						EventMessage = TurnEventMessage.EM_COLONY_DESTROYED,
						PlayerID = colonyInfo.PlayerID,
						ColonyID = colonyInfo.ID,
						SystemID = system.ID,
						TurnNumber = base.App.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
					StationTypeFlags stationTypeFlags = StationTypeFlags.CIVILIAN | StationTypeFlags.DIPLOMATIC | StationTypeFlags.DEFENCE;
					List<StationInfo> list6 = base.App.GameDatabase.GetStationForSystemAndPlayer(this._systemId, colonyInfo.PlayerID).ToList<StationInfo>();
					foreach (StationInfo stationInfo in list6)
					{
						if (stationInfo.OrbitalObjectInfo.ParentID == orbitalObjectInfo.ID && (1 << (int)stationInfo.DesignInfo.StationType & (int)stationTypeFlags) != 0)
						{
							base.App.GameDatabase.DestroyStation(base.App.Game, stationInfo.ID, 0);
						}
					}
					base.App.GameDatabase.RemoveColonyOnPlanet(num2);
					bool flag3 = base.App.GameDatabase.GetColonyInfosForSystem(system.ID).Count<ColonyInfo>() == 0;
					this._lastPendingCombat.CombatResults.AdditionalInfo.Add(string.Format(App.Localize("@UI_POST_COMBAT_STAT_COLONY_DESTROYED"), orbitalObjectInfo.Name));
					if (list.Any((int x) => standardPlayers.Any((PlayerInfo y) => y.ID == x)))
					{
						GameSession.ApplyMoralEvent(base.App, MoralEvent.ME_LOSE_WORLD_ENEMY, colonyInfo.PlayerID, null, system.ProvinceID, null);
					}
					using (List<int>.Enumerator enumerator4 = list.GetEnumerator())
					{
						while (enumerator4.MoveNext())
						{
							//CommonCombatState.<>c__DisplayClass1c8 CS$<>8__locals7 = new CommonCombatState.<>c__DisplayClass1c8();
							int pID = enumerator4.Current;
							base.App.GameDatabase.ApplyDiplomacyReaction(colonyInfo.PlayerID, pID, StratModifiers.DiplomacyReactionKillColony, 1);
							int factionId = base.App.GameDatabase.GetPlayerFactionID(colonyInfo.PlayerID);
							List<PlayerInfo> list7 = (from x in standardPlayers
													  where x.FactionID == factionId && x.ID != pID
													  select x).ToList<PlayerInfo>();
							foreach (PlayerInfo playerInfo2 in list7)
							{
								if (base.App.GameDatabase.GetDiplomacyInfo(playerInfo2.ID, pID).isEncountered)
								{
									base.App.GameDatabase.ApplyDiplomacyReaction(playerInfo2.ID, pID, StratModifiers.DiplomacyReactionKillRaceWorld, 1);
								}
							}
						}
					}
					if (colonyInfo.CurrentStage == Kerberos.Sots.Data.ColonyStage.GemWorld)
					{
						GameSession.ApplyMoralEvent(base.App, MoralEvent.ME_LOSE_GEM, colonyInfo.PlayerID, null, system.ProvinceID, null);
					}
					else if (colonyInfo.CurrentStage == Kerberos.Sots.Data.ColonyStage.ForgeWorld)
					{
						GameSession.ApplyMoralEvent(base.App, MoralEvent.ME_LOSE_FORGE, colonyInfo.PlayerID, null, system.ProvinceID, null);
					}
					if (!flag3 || flag2)
					{
						goto IL_125C;
					}
					foreach (int player2 in list)
					{
						GameSession.ApplyMoralEvent(base.App, MoralEvent.ME_ENEMY_SYSTEM_CONQUERED, player2, null, null, null);
					}
					if (system.ProvinceID != null && system.ID == base.App.GameDatabase.GetProvinceCapitalSystemID(system.ProvinceID.Value))
					{
						GameSession.ApplyMoralEvent(base.App, MoralEvent.ME_LOSE_PROVINCE_CAPITAL, colonyInfo.PlayerID, null, null, null);
					}
					List<StarSystemInfo> source2 = base.App.GameDatabase.GetStarSystemInfos().ToList<StarSystemInfo>();
					if (system.ProvinceID != null)
					{
						if (!source2.Any((StarSystemInfo x) => x.ProvinceID == system.ProvinceID))
						{
							GameSession.ApplyMoralEvent(base.App, MoralEvent.ME_LOSE_PROVINCE, colonyInfo.PlayerID, null, null, null);
							foreach (int player3 in list)
							{
								GameSession.ApplyMoralEvent(base.App, MoralEvent.ME_ENEMY_PROVINCE_CAPTURED, player3, null, null, null);
							}
						}
					}
					if (base.App.GameDatabase.GetPlayerInfo(colonyInfo.PlayerID).Homeworld == colonyInfo.OrbitalObjectID)
					{
						GameSession.ApplyMoralEvent(base.App, MoralEvent.ME_LOSE_EMPIRE_CAPITAL, colonyInfo.PlayerID, null, null, null);
					}
					if (base.App.GameDatabase.GetPlayerColoniesByPlayerId(colonyInfo.PlayerID).Count<ColonyInfo>() != 0)
					{
						goto IL_125C;
					}
					using (List<int>.Enumerator enumerator4 = list.GetEnumerator())
					{
						while (enumerator4.MoveNext())
						{
							int player4 = enumerator4.Current;
							GameSession.ApplyMoralEvent(base.App, MoralEvent.ME_ENEMY_EMPIRE_DESTROYED, player4, null, null, null);
						}
						goto IL_125C;
					}
				}
				colonyInfo.DamagedLastTurn = (colonyInfo.ImperialPop != num7);
				colonyInfo.ImperialPop = num7;
				base.App.GameDatabase.UpdateColony(colonyInfo);
			}
		IL_125C:
			if (colonyInfo != null && planetInfo != null)
			{
				this._combatData.GetOrAddPlayer(colonyInfo.PlayerID).AddPlanetData(orbitalObjectInfo.ID, num17, num15, num8, list2);
			}
			if (flag)
			{
				base.App.GameDatabase.DestroyOrbitalObject(base.App.Game, num2);
				this._lastPendingCombat.CombatResults.AdditionalInfo.Add(string.Format(App.Localize("@UI_POST_COMBAT_STAT_ORBITAL_DESTROYED"), orbitalObjectInfo.Name));
			}
			CommonCombatState.Trace(string.Format("Planet Data - Imperial Pop: {0} Diff: {1} \n Infrastructure: {2} diff:{3} \n Suitability:{4} diff:{5} \n total Damage: {6}", new object[]
			{
				num7,
				num8,
				num14,
				num15,
				num16,
				num17,
				num3
			}));
		}

		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_COMBAT_DATA_STAR(
		  ScriptMessageReader mr)
		{
			mr.ReadInteger();
			mr.ReadBool();
		}

		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_COMBAT_DESTROYED_SHIPS(
		  ScriptMessageReader mr)
		{
			int num1 = mr.ReadInteger();
			int num2 = num1;
			List<int> intList = new List<int>();
			for (int index = 0; index < num1; ++index)
			{
				int shipID1 = mr.ReadInteger();
				int designID = mr.ReadInteger();
				string str = mr.ReadString();
				float damageReceived = mr.ReadSingle();
				float damageDealt = mr.ReadSingle();
				int kills = mr.ReadInteger();
				int num3 = mr.ReadInteger();
				DestroyedShip destroyedShip = new DestroyedShip()
				{
					DatabaseId = shipID1,
					Name = str,
					DamageReceived = damageReceived,
					DamageApplied = damageDealt
				};
				this._lastPendingCombat.CombatResults.DestroyedShips.Add(destroyedShip);
				CommonCombatState.Trace(string.Format("- Ship Destroyed Message Id: {0} name: {1} damageRecieved: {2}, damageApplied: {3} -", (object)shipID1, (object)str, (object)damageReceived, (object)damageDealt));
				List<int> AlliedPlayers = this._lastPendingCombat.CombatResults.PlayersInCombat.Where<int>((Func<int, bool>)(x =>
			   {
				   if (x != this.App.Game.LocalPlayer.ID)
					   return this.App.GameDatabase.GetDiplomacyStateBetweenPlayers(this.App.Game.LocalPlayer.ID, x) == DiplomacyState.ALLIED;
				   return true;
			   })).ToList<int>();
				List<int> list1 = this._lastPendingCombat.CombatResults.PlayersInCombat.Where<int>((Func<int, bool>)(x => !AlliedPlayers.Contains(x))).ToList<int>();
				DesignInfo designInfo = this.App.GameDatabase.GetDesignInfo(designID);
				if (designInfo != null)
				{
					if (designInfo.Class == ShipClass.Station)
					{
						bool flag = false;
						foreach (StationInfo stationInfo in this.App.GameDatabase.GetStationForSystemAndPlayer(this._systemId, designInfo.PlayerID).ToList<StationInfo>())
						{
							if (stationInfo.DesignInfo.ID == designID)
							{
								this.ApplyRewardsForShipDeath(this.App.GameDatabase.GetShipInfo(stationInfo.ShipID, false), designInfo.PlayerID, num3, (FleetInfo)null);
								if (this.App.Game.ScriptModules.Pirates == null || this.App.Game.ScriptModules.Pirates.PlayerID != stationInfo.PlayerID)
								{
									SuulkaInfo suulkaByStationId = this.App.GameDatabase.GetSuulkaByStationID(stationInfo.ID);
									if (suulkaByStationId != null)
									{
										foreach (int num4 in this._playersInCombat.Where<Player>((Func<Player, bool>)(x => x.IsStandardPlayer)).Select<Player, int>((Func<Player, int>)(x => x.ID)).ToList<int>())
											this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
											{
												EventType = TurnEventType.EV_SUULKA_LEAVES,
												EventMessage = TurnEventMessage.EM_SUULKA_LEAVES,
												PlayerID = num4,
												ShipID = suulkaByStationId.ShipID,
												SystemID = this._systemId,
												Param1 = suulkaByStationId.ID.ToString(),
												TurnNumber = this.App.GameDatabase.GetTurnCount(),
												ShowsDialog = false
											});
									}
									this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
									{
										EventType = TurnEventType.EV_STATION_DESTROYED,
										EventMessage = TurnEventMessage.EM_STATION_DESTROYED,
										PlayerID = designInfo.PlayerID,
										OrbitalID = stationInfo.OrbitalObjectID,
										SystemID = this.App.GameDatabase.GetOrbitalObjectInfo(stationInfo.OrbitalObjectID).StarSystemID,
										TurnNumber = this.App.GameDatabase.GetTurnCount(),
										ShowsDialog = false
									});
									GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_LOSE_STATION, designInfo.PlayerID, new int?(), new int?(), new int?(this._lastPendingCombat.SystemID));
									if (designInfo.StationLevel == 5)
										GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_LOSE_LVL5_STATION, designInfo.PlayerID, new int?(), new int?(), new int?(this._lastPendingCombat.SystemID));
								}
								flag = true;
								this.App.GameDatabase.DestroyStation(this.App.Game, stationInfo.OrbitalObjectID, 0);
								break;
							}
						}
						if (flag)
							continue;
					}
					DesignSectionInfo designSectionInfo = ((IEnumerable<DesignSectionInfo>)designInfo.DesignSections).FirstOrDefault<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(x => x.ShipSectionAsset.Type == ShipSectionType.Mission));
					if (designSectionInfo == null || ShipSectionAsset.IsWeaponBattleRiderClass(designSectionInfo.ShipSectionAsset.RealClass))
					{
						--num2;
						this._lastPendingCombat.CombatResults.DestroyedShips.Remove(destroyedShip);
					}
					ShipInfo si = this.App.GameDatabase.GetShipInfo(shipID1, true);
					if (si != null)
					{
						if (si.DesignInfo.IsSuperTransport())
						{
							foreach (ShipInfo shipInfo in this.App.GameDatabase.GetShipInfoByFleetID(si.FleetID, true).ToList<ShipInfo>())
							{
								if (shipInfo.DesignInfo.IsSDB() || shipInfo.DesignInfo.IsPlatform())
									this.App.GameDatabase.RemoveShip(shipInfo.ID);
							}
						}
						if (this.App.AssetDatabase.GetFaction(this.App.GameDatabase.GetPlayerInfo(designInfo.PlayerID).FactionID).Name == "loa" && !intList.Contains(si.FleetID))
							intList.Add(si.FleetID);
						if (this._pirateEncounterData.IsPirateEncounter && designInfo.Role == ShipRole.FREIGHTER && this._pirateEncounterData.PlayerFreightersInSystem.ContainsKey(designInfo.PlayerID))
						{
							this.App.GameDatabase.RemoveFreighterInfo(this._pirateEncounterData.PlayerFreightersInSystem[designInfo.PlayerID].First<CommonCombatState.PiracyFreighterInfo>((Func<CommonCombatState.PiracyFreighterInfo, bool>)(x => x.ShipID == si.ID)).FreighterID);
							if (this._pirateEncounterData.DestroyedFreighters.ContainsKey(designInfo.PlayerID))
							{
								Dictionary<int, int> destroyedFreighters;
								int playerId;
								(destroyedFreighters = this._pirateEncounterData.DestroyedFreighters)[playerId = designInfo.PlayerID] = destroyedFreighters[playerId] + 1;
							}
							else
								this._pirateEncounterData.DestroyedFreighters.Add(designInfo.PlayerID, 1);
							foreach (ColonyInfo colony in this.App.GameDatabase.GetColonyInfosForSystem(this._systemId).ToList<ColonyInfo>())
							{
								colony.ModifyEconomyRating(this.App.GameDatabase, ColonyInfo.EconomicChangeReason.FreighterKilled, 1);
								this.App.GameDatabase.UpdateColony(colony);
							}
						}
						si.DesignInfo = designInfo;
						FleetInfo fi = this.App.GameDatabase.GetFleetInfo(si.FleetID);
						if (fi == null)
						{
							CommonCombatState.Warn("Attemping to access a fleet, for a destroyed ship, which has already been removed.");
						}
						else
						{
							this.ApplyRewardsForShipDeath(si, fi.PlayerID, num3, fi);
							Player playerByObjectId = this.App.GetPlayerByObjectID(num3);
							int num4 = playerByObjectId != null ? playerByObjectId.ID : 0;
							this._combatData.GetOrAddPlayer(fi.PlayerID).AddShipData(si.DesignID, damageDealt, damageReceived, kills, true);
							if (fi.PlayerID == this.App.Game.LocalPlayer.ID && si.DesignInfo.Class == ShipClass.Leviathan)
								this.App.SteamHelper.DoAchievement(AchievementType.SOTS2_INCONVIENIENCE);
							if (this.App.Game.ScriptModules.AsteroidMonitor != null && si.DesignID == this.App.Game.ScriptModules.AsteroidMonitor.MonitorCommandDesignId)
							{
								foreach (int playerId in this.App.GameDatabase.GetPlayerIDs())
								{
									if (playerId != fi.ID)
										this.App.GameDatabase.ChangeDiplomacyState(fi.PlayerID, playerId, DiplomacyState.NEUTRAL);
								}
							}
							if (si.DesignInfo.IsSuulka())
							{
								TurnEvent turnEvent = this.App.GameDatabase.GetTurnEventsByTurnNumber(this.App.GameDatabase.GetTurnCount(), fi.PlayerID).FirstOrDefault<TurnEvent>((Func<TurnEvent, bool>)(x => x.ShipID == si.ID));
								if (turnEvent != null)
									this.App.GameDatabase.RemoveTurnEvent(turnEvent.ID);
								this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
								{
									EventType = TurnEventType.EV_SUULKA_DIES,
									EventMessage = TurnEventMessage.EM_SUULKA_DIES,
									PlayerID = num4,
									SystemID = fi.SystemID,
									ShipID = si.ID,
									DesignID = si.DesignID,
									TurnNumber = this.App.GameDatabase.GetTurnCount(),
									ShowsDialog = false
								});
								SuulkaInfo suulkaByShipId = this.App.GameDatabase.GetSuulkaByShipID(si.ID);
								if (suulkaByShipId != null)
									this.App.GameDatabase.RemoveSuulka(suulkaByShipId.ID);
							}
							GameTrigger.PushEvent(EventType.EVNT_SHIPDIED, (object)si.DesignInfo.Class, this.App.Game);
							this.App.GameDatabase.RemoveShip(shipID1);
							bool flag1 = this.App.GameDatabase.GetFactionName(this.App.GameDatabase.GetPlayerFactionID(si.DesignInfo.PlayerID)) == "loa";
							bool flag2 = false;
							if (si.DesignInfo.Class == ShipClass.Leviathan)
							{
								GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_LOSE_FLAGSHIP, si.DesignInfo.PlayerID, new int?(), new int?(), new int?(this._lastPendingCombat.SystemID));
								flag2 = !flag1;
								if (AlliedPlayers.Contains(si.DesignInfo.PlayerID))
								{
									foreach (int player in list1)
										GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_ENEMY_LEVIATHAN_DESTROYED, player, new int?(), new int?(), new int?());
									GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_LOSE_LEVIATHAN, si.DesignInfo.PlayerID, new int?(), new int?(), new int?(this._lastPendingCombat.SystemID));
								}
								else
								{
									foreach (int player in AlliedPlayers)
										GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_ENEMY_LEVIATHAN_DESTROYED, player, new int?(), new int?(), new int?());
									GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_LOSE_LEVIATHAN, si.DesignInfo.PlayerID, new int?(), new int?(), new int?(this._lastPendingCombat.SystemID));
								}
							}
							else if (((IEnumerable<DesignSectionInfo>)si.DesignInfo.DesignSections).Any<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(x => x.FilePath.Contains("cnc"))) && !flag1)
							{
								if (si.DesignInfo.Class == ShipClass.Dreadnought)
									GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_LOSE_FLAGSHIP, si.DesignInfo.PlayerID, new int?(), new int?(), new int?(this._lastPendingCombat.SystemID));
								flag2 = true;
							}
							List<int> list2 = this.App.GameDatabase.GetShipsByFleetID(si.FleetID).ToList<int>();
							if (list2.Count == 0)
							{
								if (fi.AdmiralID != 0 && playerByObjectId != null && playerByObjectId.IsStandardPlayer)
									this.CheckAdmiralCaptured(fi, playerByObjectId.ID);
								if (fi.PlayerID == this.App.Game.ScriptModules.Gardeners.PlayerID)
									this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
									{
										EventType = TurnEventType.EV_PROTEANS_REMOVED,
										EventMessage = TurnEventMessage.EM_PROTEANS_REMOVED,
										PlayerID = num4,
										SystemID = fi.SystemID,
										TurnNumber = this.App.GameDatabase.GetTurnCount(),
										ShowsDialog = false
									});
								else if (fi.PlayerID == this.App.Game.ScriptModules.Swarmers.PlayerID)
								{
									if (si.DesignID == this.App.Game.ScriptModules.Swarmers.SwarmQueenDesignID)
										this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
										{
											EventType = TurnEventType.EV_SWARM_QUEEN_DESTROYED,
											EventMessage = TurnEventMessage.EM_SWARM_QUEEN_DESTROYED,
											PlayerID = num4,
											SystemID = fi.SystemID,
											TurnNumber = this.App.GameDatabase.GetTurnCount(),
											ShowsDialog = false
										});
									else
										this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
										{
											EventType = TurnEventType.EV_SWARM_DESTROYED,
											EventMessage = TurnEventMessage.EM_SWARM_DESTROYED,
											PlayerID = num4,
											SystemID = fi.SystemID,
											TurnNumber = this.App.GameDatabase.GetTurnCount(),
											ShowsDialog = false
										});
								}
								else if (fi.PlayerID == this.App.Game.ScriptModules.SystemKiller.PlayerID)
								{
									FleetLocation fleetLocation = this.App.GameDatabase.GetFleetLocation(fi.ID, false);
									if (fleetLocation != null)
									{
										foreach (int standardPlayerId in this.App.GameDatabase.GetStandardPlayerIDs())
										{
											if (StarMap.IsInRange(this.App.GameDatabase, standardPlayerId, fleetLocation.Coords, 1f, (Dictionary<int, List<ShipInfo>>)null))
												this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
												{
													EventType = TurnEventType.EV_SYS_KILLER_DESTROYED,
													EventMessage = TurnEventMessage.EM_SYS_KILLER_DESTROYED,
													PlayerID = standardPlayerId,
													TurnNumber = this.App.GameDatabase.GetTurnCount()
												});
										}
									}
								}
								else if (fi.PlayerID == this.App.Game.ScriptModules.MorrigiRelic.PlayerID)
									this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
									{
										EventType = TurnEventType.EV_TOMB_DESTROYED,
										EventMessage = TurnEventMessage.EM_TOMB_DESTROYED,
										PlayerID = num4,
										SystemID = fi.SystemID,
										TurnNumber = this.App.GameDatabase.GetTurnCount(),
										ShowsDialog = false
									});
								else if (fi.PlayerID == this.App.Game.ScriptModules.VonNeumann.PlayerID && this.App.Game.ScriptModules.VonNeumann.IsHomeWorldFleet(fi))
								{
									this.App.Game.ScriptModules.VonNeumann.HandleHomeSystemDefeated(this.App, fi, this._playersInCombat.Where<Player>((Func<Player, bool>)(x =>
								   {
									   if (x.IsStandardPlayer)
										   return x.ID != fi.PlayerID;
									   return false;
								   })).Select<Player, int>((Func<Player, int>)(x => x.ID)).ToList<int>());
									this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
									{
										EventType = TurnEventType.EV_FLEET_DESTROYED,
										EventMessage = TurnEventMessage.EM_FLEET_DESTROYED,
										PlayerID = num4,
										SystemID = fi.SystemID,
										FleetID = fi.ID,
										TurnNumber = this.App.GameDatabase.GetTurnCount(),
										ShowsDialog = false
									});
								}
								else
									this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
									{
										EventType = TurnEventType.EV_FLEET_DESTROYED,
										EventMessage = TurnEventMessage.EM_FLEET_DESTROYED,
										PlayerID = num4,
										SystemID = fi.SystemID,
										FleetID = fi.ID,
										TurnNumber = this.App.GameDatabase.GetTurnCount(),
										ShowsDialog = false
									});
								GameTrigger.PushEvent(EventType.EVNT_FLEETDIED, (object)fi.Name, this.App.Game);
								this.App.GameDatabase.RemoveFleet(si.FleetID);
								this.CheckToReturnControlZonesToOwner(si.DesignInfo.PlayerID);
								if (AlliedPlayers.Contains(si.DesignInfo.PlayerID))
								{
									foreach (int player in list1)
										GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_ENEMY_FLEET_DESTROYED, player, new int?(), new int?(), new int?());
								}
								else
								{
									foreach (int player in AlliedPlayers)
										GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_ENEMY_FLEET_DESTROYED, player, new int?(), new int?(), new int?());
								}
							}
							else if (flag2)
							{
								PlayerInfo playerInfo = this.App.GameDatabase.GetPlayerInfo(fi.PlayerID);
								if (playerInfo != null && playerInfo.isStandardPlayer && list2.Max<int>((Func<int, int>)(x => this.App.GameDatabase.GetShipCommandPointQuota(x))) == 0)
								{
									if (fi.AdmiralID != 0)
										this.CheckAdmiralSurvival(fi, si);
									int num5 = this.App.Game.MCCarryOverData.GetRetreatFleetID(fi.SystemID, fi.ID);
									if (num5 != fi.ID)
									{
										if (num5 == 0)
										{
											num5 = this.App.GameDatabase.InsertFleet(fi.PlayerID, 0, fi.SystemID, fi.SupportingSystemID, App.Localize("@FLEET_RETREAT"), FleetType.FL_NORMAL);
											int missionID = this.App.GameDatabase.InsertMission(num5, MissionType.RETREAT, 0, 0, 0, 0, false, new int?());
											this.App.GameDatabase.InsertWaypoint(missionID, WaypointType.ReturnHome, new int?());
											this.App.GameDatabase.InsertWaypoint(missionID, WaypointType.DisbandFleet, new int?());
											this.App.Game.MCCarryOverData.SetRetreatFleetID(fi.SystemID, fi.ID, num5);
										}
										foreach (int shipID2 in list2)
											this.App.GameDatabase.TransferShip(shipID2, num5);
										this.App.GameDatabase.RemoveFleet(fi.ID);
									}
									this.CheckToReturnControlZonesToOwner(si.DesignInfo.PlayerID);
								}
							}
						}
					}
				}
			}
			this._lastPendingCombat.CombatResults.AdditionalInfo.Add(string.Format(App.Localize("@UI_POST_COMBAT_STAT_DESTROYED_SHIPS"), (object)num2));
			foreach (int num3 in intList)
			{
				FleetInfo fi = this.App.GameDatabase.GetFleetInfo(num3);
				if (fi != null)
				{
					MissionInfo missionByFleetId = this.App.GameDatabase.GetMissionByFleetID(fi.ID);
					int fleetLoaCubeValue = Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(this.App.Game, num3);
					if (fi.FleetConfigID.HasValue)
					{
						LoaFleetComposition composition = this.App.GameDatabase.GetLoaFleetCompositions().FirstOrDefault<LoaFleetComposition>((Func<LoaFleetComposition, bool>)(x => x.ID == fi.FleetConfigID.Value));
						if (composition != null)
						{
							List<DesignInfo> list = Kerberos.Sots.StarFleet.StarFleet.GetDesignBuildOrderForComposition(this.App.Game, fi.ID, composition, missionByFleetId != null ? missionByFleetId.Type : MissionType.NO_MISSION).ToList<DesignInfo>();
							if (!list.Any<DesignInfo>())
							{
								this.App.GameDatabase.RemoveFleet(fi.ID);
							}
							else
							{
								DesignInfo designInfo = list.First<DesignInfo>();
								if (designInfo.GetCommandPoints() == 0 || designInfo.ProductionCost > fleetLoaCubeValue)
									this.App.GameDatabase.RemoveFleet(fi.ID);
							}
						}
					}
				}
			}
		}

		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_COMBAT_CAPTURED_SHIPS(
		  ScriptMessageReader mr)
		{
			int num1 = mr.ReadInteger();
			for (int index = 0; index < num1; ++index)
			{
				int id1 = mr.ReadInteger();
				mr.ReadInteger();
				int objectId1 = mr.ReadInteger();
				int objectId2 = mr.ReadInteger();
				mr.ReadBool();
				Ship gameObject = this.App.GetGameObject<Ship>(id1);
				this.App.GetPlayerByObjectID(objectId1);
				Player playerByObjectId = this.App.GetPlayerByObjectID(objectId2);
				if (this._pirateEncounterData.IsPirateEncounter && gameObject != null && (playerByObjectId != null && this._pirateEncounterData.PiratePlayerIDs.Contains(playerByObjectId.ID)))
				{
					int num2 = 0;
					if (gameObject.ShipRole == ShipRole.FREIGHTER)
						num2 = this.App.AssetDatabase.GlobalPiracyData.Bounties[3];
					if (this._pirateEncounterData.PlayerBounties.ContainsKey(playerByObjectId.ID))
					{
						Dictionary<int, int> playerBounties;
						int id2;
						(playerBounties = this._pirateEncounterData.PlayerBounties)[id2 = playerByObjectId.ID] = playerBounties[id2] + num2;
					}
					else
						this._pirateEncounterData.PlayerBounties.Add(playerByObjectId.ID, num2);
				}
			}
		}

		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_COMBAT_WEAPON_DAMAGE_STATS(
		  ScriptMessageReader mr)
		{
			int num1 = mr.ReadInteger();
			for (int index1 = 0; index1 < num1; ++index1)
			{
				int id = mr.ReadInteger();
				int num2 = mr.ReadInteger();
				Player gameObject = this.App.GetGameObject<Player>(id);
				CommonCombatState.Trace(string.Format("- Weapon Damage Stats for Player {0} -", (object)gameObject.ID));
				this._lastPendingCombat.CombatResults.WeaponDamageTable.Add(gameObject.ID, new Dictionary<int, float>());
				for (int index2 = 0; index2 < num2; ++index2)
				{
					int weaponID = mr.ReadInteger();
					float damage = mr.ReadSingle();
					LogicalWeapon logicalWeapon = this.App.AssetDatabase.Weapons.First<LogicalWeapon>((Func<LogicalWeapon, bool>)(x =>
				   {
					   if (x.GameObject == null)
						   return false;
					   return x.GameObject.ObjectID == weaponID;
				   }));
					this._combatData.GetOrAddPlayer(gameObject.ID).AddWeaponData(logicalWeapon.UniqueWeaponID, damage);
					this._lastPendingCombat.CombatResults.WeaponDamageTable[gameObject.ID].Add(logicalWeapon.UniqueWeaponID, damage);
					CommonCombatState.Trace(string.Format("   Weapon: {0} Damage: {1}", (object)logicalWeapon.UniqueWeaponID, (object)damage));
				}
			}
		}

		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_COMBAT_ZONE_STATES(
		  ScriptMessageReader mr)
		{
			int num = mr.ReadInteger();
			List<int> zones = new List<int>();
			for (int index = 0; index < num; ++index)
			{
				Player playerByObjectId = this.App.GetPlayerByObjectID(mr.ReadInteger());
				if (playerByObjectId != null)
					zones.Add(playerByObjectId.ID);
				else
					zones.Add(0);
			}
			this.App.GameDatabase.UpdateSystemCombatZones(this._systemId, zones);
		}

		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_ZONE_OWNER_CHANGED(
		  ScriptMessageReader mr)
		{
			int ring = mr.ReadInteger();
			int zone1 = mr.ReadInteger();
			Player gameObject = this.App.GetGameObject<Player>(mr.ReadInteger());
			if (this._starSystemObjects == null)
				return;
			CombatZonePositionInfo zone2 = this._starSystemObjects.ChangeCombatZoneOwner(ring, zone1, gameObject);
			foreach (CombatAI aiCommander in this.AI_Commanders)
				aiCommander.NotifyCombatZoneChanged(zone2);
		}

		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_PLAYER_DIPLO_CHANGED(
		  ScriptMessageReader mr)
		{
			int objectId1 = mr.ReadInteger();
			int objectId2 = mr.ReadInteger();
			int num = mr.ReadInteger();
			if (this._initialDiploStates == null)
				return;
			PlayerCombatDiplomacy playerCombatDiplomacy = (PlayerCombatDiplomacy)num;
			Player playerByObjectId1 = this.App.GetPlayerByObjectID(objectId1);
			Player playerByObjectId2 = this.App.GetPlayerByObjectID(objectId2);
			if (playerByObjectId1 == null || playerByObjectId2 == null || playerCombatDiplomacy != PlayerCombatDiplomacy.War)
				return;
			this.SetDiplomacyState(playerByObjectId1.ID, playerByObjectId2.ID, DiplomacyState.WAR);
			this.SyncPlayerList();
		}

		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_COMBAT_ENDED(ScriptMessageReader mr)
		{
			mr.ReadInteger();
			this.EndCombat();
		}

		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_SET_PAUSE_STATE(ScriptMessageReader mr)
		{
			this._isPaused = mr.ReadInteger() == 1;
		}

		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_SET_COMBAT_ACTIVE(
		  ScriptMessageReader mr)
		{
			this._engCombatActive = mr.ReadBool();
		}

		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_END_SENDINGDATA(ScriptMessageReader mr)
		{
			if (this._authority)
			{
				if (this._lastPendingCombat.CombatResults == null)
					this._lastPendingCombat.CombatResults = new PostCombatData();
				if ((this.App.Network.IsHosting || !this.App.GameSetup.IsMultiplayer) && this._combatData.SystemID != 0)
					this.App.GameDatabase.InsertCombatData(this._combatData.SystemID, this._combatData.CombatID, this._combatData.Turn, this._combatData.ToByteArray());
				this.ProcessShipsHitByNodeCannon();
				foreach (Player player1 in this._playersInCombat)
				{
					Player player = player1;
					PlayerCombatData pcd = this._combatData.GetPlayer(player.ID);
					if (pcd != null)
					{
						pcd.VictoryStatus = this.App.Game.GetPlayerVictoryStatus(player.ID, this._systemId);
						pcd.FleetCount = this._fleetsPerPlayer.ContainsKey(player.ID) ? this._fleetsPerPlayer[player.ID] : 0;
						if (pcd.VictoryStatus == GameSession.VictoryStatus.Win && pcd.FleetCount > 0 && this._mediaHeroFleets.Any<FleetInfo>((Func<FleetInfo, bool>)(x => x.PlayerID == player.ID)))
							GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_ADMIRAL_MEDIA_HERO_WIN, player.ID, new int?(), new int?(), new int?());
						if (pcd.PlayerID == this.App.Game.ScriptModules.Spectre.PlayerID)
						{
							foreach (Player player2 in this._playersInCombat)
							{
								Player player3 = this.App.GetPlayer(player2.ID);
								if (player3.IsStandardPlayer && player3.ID != pcd.PlayerID)
									this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
									{
										EventType = TurnEventType.EV_SPECTRE_ATTACK,
										EventMessage = TurnEventMessage.EM_SPECTRE_ATTACK,
										PlayerID = player3.ID,
										SystemID = this._combatData.SystemID,
										TurnNumber = this.App.GameDatabase.GetTurnCount()
									});
							}
						}
						if (pcd.PlayerID == this.App.Game.ScriptModules.MeteorShower.PlayerID)
						{
							CombatAI combatAi = this.AI_Commanders.FirstOrDefault<CombatAI>((Func<CombatAI, bool>)(x => x.m_Player.ID == pcd.PlayerID));
							List<int> intList = new List<int>();
							foreach (ColonyInfo colonyInfo in this.App.GameDatabase.GetColonyInfosForSystem(this._systemId))
							{
								if (!intList.Contains(colonyInfo.PlayerID))
									intList.Add(colonyInfo.PlayerID);
							}
							foreach (int player2 in intList)
								GameSession.ApplyMoralEvent(this.App, ((NPCFactionCombatAI)combatAi).NumPlanetStruckAsteroids, MoralEvent.ME_ASTEROID_STRIKE, player2, new int?(), new int?(), new int?(this._systemId));
						}
					}
				}
				foreach (NPCFactionCombatAI npcFactionCombatAi in this.AI_Commanders.OfType<NPCFactionCombatAI>())
					npcFactionCombatAi.HandlePostCombat(this._playersInCombat, this._spawnPositions.Select<SpawnProfile, int>((Func<SpawnProfile, int>)(x => x._fleetID)).ToList<int>(), this._systemId);
				this.UpdateDiploStatesInScript();
				if (this._pirateEncounterData.IsPirateEncounter)
				{
					foreach (KeyValuePair<int, List<CommonCombatState.PiracyFreighterInfo>> keyValuePair in this._pirateEncounterData.PlayerFreightersInSystem)
					{
						int num = 0;
						if (!this._pirateEncounterData.DestroyedFreighters.TryGetValue(keyValuePair.Key, out num))
							num = 0;
						this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_PIRATE_RAID,
							EventMessage = TurnEventMessage.EM_PIRATE_RAID,
							SystemID = this._systemId,
							PlayerID = keyValuePair.Key,
							NumShips = num,
							TurnNumber = this.App.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
					}
					foreach (KeyValuePair<int, int> playerBounty in this._pirateEncounterData.PlayerBounties)
					{
						PlayerInfo playerInfo = this.App.GameDatabase.GetPlayerInfo(playerBounty.Key);
						if (playerInfo != null && (this.App.Game.ScriptModules.Pirates == null || this.App.Game.ScriptModules.Pirates.PlayerID != playerInfo.ID))
							this.App.GameDatabase.UpdatePlayerSavings(playerBounty.Key, playerInfo.Savings + (double)playerBounty.Value);
					}
				}
				foreach (Player player in this._playersInCombat)
				{
					if (this._ignoreCombatZonePlayers.Contains(player))
						StarSystem.RemoveSystemPlayerColor(this.App.GameDatabase, this._systemId, player.ID);
				}
				StarSystem.RestoreNeutralSystemColor(this.App, this._systemId, true);
				if (this.App.GameSetup.IsMultiplayer)
				{
					this.App.Network.SendCarryOverData(this.App.Game.MCCarryOverData.GetCarryOverDataList(this._systemId));
					this.App.Network.SendCombatData(this._combatData);
				}
			}
			this._combatData = (CombatData)null;
			this._combatEndingStatsGathered = true;
		}

		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_END_DELAYCOMPLETE(
		  ScriptMessageReader mr)
		{
			this._combatEndDelayComplete = true;
		}

		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_OBJECT_ADD(ScriptMessageReader mr)
		{
			this.AddObject(mr);
		}

		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_OBJECTS_ADD(ScriptMessageReader mr)
		{
			this.AddObjects(mr);
		}

		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_OBJECT_RELEASE(ScriptMessageReader mr)
		{
			this.RemoveObject(mr);
		}

		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_OBJECTS_RELEASE(ScriptMessageReader mr)
		{
			this.RemoveObjects(mr);
		}

		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
			switch (messageID)
			{
				case InteropMessageID.IMID_SCRIPT_SET_PAUSE_STATE:
					this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_SET_PAUSE_STATE(mr);
					break;
				case InteropMessageID.IMID_SCRIPT_SET_COMBAT_ACTIVE:
					this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_SET_COMBAT_ACTIVE(mr);
					break;
				case InteropMessageID.IMID_SCRIPT_PLAYER_DIPLO_CHANGED:
					this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_PLAYER_DIPLO_CHANGED(mr);
					break;
				case InteropMessageID.IMID_SCRIPT_ZONE_OWNER_CHANGED:
					this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_ZONE_OWNER_CHANGED(mr);
					break;
				case InteropMessageID.IMID_SCRIPT_OBJECT_ADD:
					this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_OBJECT_ADD(mr);
					break;
				case InteropMessageID.IMID_SCRIPT_OBJECTS_ADD:
					this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_OBJECTS_ADD(mr);
					break;
				case InteropMessageID.IMID_SCRIPT_OBJECT_RELEASE:
					this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_OBJECT_RELEASE(mr);
					break;
				case InteropMessageID.IMID_SCRIPT_OBJECTS_RELEASE:
					this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_OBJECTS_RELEASE(mr);
					break;
				case InteropMessageID.IMID_SCRIPT_COMBAT_ENDED:
					this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_COMBAT_ENDED(mr);
					break;
				case InteropMessageID.IMID_SCRIPT_START_SENDINGDATA:
					this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_START_SENDINGDATA(mr);
					break;
				case InteropMessageID.IMID_SCRIPT_COMBAT_DATA_SHIP:
					this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_COMBAT_DATA_SHIP(mr);
					break;
				case InteropMessageID.IMID_SCRIPT_COMBAT_DATA_PLANET:
					this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_COMBAT_DATA_PLANET(mr);
					break;
				case InteropMessageID.IMID_SCRIPT_COMBAT_DATA_STAR:
					this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_COMBAT_DATA_STAR(mr);
					break;
				case InteropMessageID.IMID_SCRIPT_COMBAT_DESTROYED_SHIPS:
					this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_COMBAT_DESTROYED_SHIPS(mr);
					break;
				case InteropMessageID.IMID_SCRIPT_COMBAT_CAPTURED_SHIPS:
					this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_COMBAT_CAPTURED_SHIPS(mr);
					break;
				case InteropMessageID.IMID_SCRIPT_COMBAT_WEAPON_DAMAGE_STATS:
					this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_COMBAT_WEAPON_DAMAGE_STATS(mr);
					break;
				case InteropMessageID.IMID_SCRIPT_COMBAT_ZONE_STATES:
					this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_COMBAT_ZONE_STATES(mr);
					break;
				case InteropMessageID.IMID_SCRIPT_END_SENDINGDATA:
					this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_END_SENDINGDATA(mr);
					break;
				case InteropMessageID.IMID_SCRIPT_END_DELAYCOMPLETE:
					this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_END_DELAYCOMPLETE(mr);
					break;
				default:
					CommonCombatState.Warn("Unhandled message (id=" + (object)messageID + ").");
					break;
			}
		}

		protected void CheckAdmiralSurvival(FleetInfo fi, ShipInfo si)
		{
			float num = 0.35f;
			AdmiralInfo admiralInfo = this.App.GameDatabase.GetAdmiralInfo(fi.AdmiralID);
			if (admiralInfo != null)
				num = (float)admiralInfo.EvasionBonus;
			List<AdmiralInfo.TraitType> list1 = this.App.GameDatabase.GetAdmiralTraits(fi.AdmiralID).ToList<AdmiralInfo.TraitType>();
			if (list1.Contains(AdmiralInfo.TraitType.Slippery))
				num += 0.2f;
			if (admiralInfo.Engram)
				num += 0.1f;
			string factionName = this.App.GameDatabase.GetFactionName(this.App.GameDatabase.GetPlayerFactionID(fi.PlayerID));
			List<LogicalModule> list2 = this.App.AssetDatabase.Modules.Where<LogicalModule>((Func<LogicalModule, bool>)(x =>
		   {
			   if (x.Faction == factionName)
				   return x.ModulePath.Contains("hannibal");
			   return false;
		   })).ToList<LogicalModule>();
			bool flag = false;
			int moduleId = 0;
			foreach (LogicalModule logicalModule in list2)
			{
				moduleId = this.App.GameDatabase.GetModuleID(logicalModule.ModulePath, fi.PlayerID);
				flag = flag && ((IEnumerable<DesignSectionInfo>)si.DesignInfo.DesignSections).Any<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(x => x.Modules.Any<DesignModuleInfo>((Func<DesignModuleInfo, bool>)(y => y.ModuleID == moduleId))));
			}
			if (flag)
				num += 0.25f;
			if (this._random.CoinToss((double)num))
				return;
			if (admiralInfo.HomeworldID.HasValue)
			{
				ColonyInfo colonyInfo = this.App.GameDatabase.GetColonyInfo(admiralInfo.HomeworldID.Value);
				if (colonyInfo != null && colonyInfo.PlayerID == admiralInfo.PlayerID)
					GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_ADMIRAL_KILLED, fi.PlayerID, new int?(), new int?(), new int?(colonyInfo.CachedStarSystemID));
			}
			if (list1.Contains(AdmiralInfo.TraitType.MediaHero))
				GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_ADMIRAL_MEDIA_HERO_KILLED, fi.PlayerID, new int?(), new int?(), new int?());
			this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
			{
				EventType = TurnEventType.EV_ADMIRAL_DEAD,
				EventMessage = TurnEventMessage.EM_ADMIRAL_DEAD,
				PlayerID = fi.PlayerID,
				AdmiralID = fi.AdmiralID,
				TurnNumber = this.App.GameDatabase.GetTurnCount()
			});
			this.App.GameDatabase.RemoveAdmiral(fi.AdmiralID);
		}

		protected void CheckAdmiralCaptured(FleetInfo fi, int destroyingPlayer)
		{
			float num = 0.75f;
			AdmiralInfo admiralInfo1 = this.App.GameDatabase.GetAdmiralInfo(fi.AdmiralID);
			if (admiralInfo1 != null)
				num -= (float)admiralInfo1.EvasionBonus;
			List<AdmiralInfo.TraitType> list1 = this.App.GameDatabase.GetAdmiralTraits(fi.AdmiralID).ToList<AdmiralInfo.TraitType>();
			if (list1.Contains(AdmiralInfo.TraitType.Slippery))
				num -= 0.2f;
			if (this._random.CoinToss((double)num))
			{
				foreach (FleetInfo fleetInfo in this.App.GameDatabase.GetFleetsByPlayerAndSystem(destroyingPlayer, fi.SystemID, FleetType.FL_NORMAL).ToList<FleetInfo>())
				{
					List<AdmiralInfo.TraitType> list2 = this.App.GameDatabase.GetAdmiralTraits(fleetInfo.AdmiralID).ToList<AdmiralInfo.TraitType>();
					if (list2.Contains(AdmiralInfo.TraitType.Inquisitor) && this._random.CoinToss(0.25))
					{
						foreach (AdmiralInfo.TraitType traitType in list1)
						{
							AdmiralInfo.TraitType tt = traitType;
							if (!list2.Contains(tt) && !list2.Any<AdmiralInfo.TraitType>((Func<AdmiralInfo.TraitType, bool>)(x => AdmiralInfo.AreTraitsMutuallyExclusive(x, tt))))
								this.App.GameDatabase.AddAdmiralTrait(fleetInfo.AdmiralID, tt, 1);
						}
					}
					if (list2.Contains(AdmiralInfo.TraitType.Evangelist) && this._random.CoinToss(0.25))
					{
						this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_ADMIRAL_INTEL_LEAK,
							EventMessage = TurnEventMessage.EM_ADMIRAL_INTEL_LEAK_TAKE,
							PlayerID = fleetInfo.PlayerID,
							AdmiralID = fleetInfo.AdmiralID,
							TargetPlayerID = destroyingPlayer,
							TurnNumber = this.App.GameDatabase.GetTurnCount()
						});
						this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_ADMIRAL_INTEL_LEAK,
							EventMessage = TurnEventMessage.EM_ADMIRAL_INTEL_LEAK_GIVE,
							PlayerID = fi.PlayerID,
							AdmiralID = fi.AdmiralID,
							TargetPlayerID = destroyingPlayer,
							TurnNumber = this.App.GameDatabase.GetTurnCount()
						});
						this.App.AssetDatabase.IntelMissions.Choose(this._random).OnCommit(this.App.Game, destroyingPlayer, fi.PlayerID, new int?());
					}
					if (list2.Contains(AdmiralInfo.TraitType.HeadHunter) && list1.Contains(AdmiralInfo.TraitType.Conscript) && this._random.CoinToss(0.5))
					{
						AdmiralInfo admiralInfo2 = this.App.GameDatabase.GetAdmiralInfo(fi.AdmiralID);
						admiralInfo2.PlayerID = fleetInfo.PlayerID;
						this.App.GameDatabase.UpdateAdmiralInfo(admiralInfo2);
						this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_ADMIRAL_DEFECTS,
							EventMessage = TurnEventMessage.EM_ADMIRAL_DEFECTS,
							PlayerID = fi.PlayerID,
							AdmiralID = fi.AdmiralID,
							TargetPlayerID = destroyingPlayer,
							TurnNumber = this.App.GameDatabase.GetTurnCount()
						});
						return;
					}
					this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_ADMIRAL_CAPTURED,
						EventMessage = TurnEventMessage.EM_ADMIRAL_CAPTURED,
						PlayerID = fi.PlayerID,
						AdmiralID = fi.AdmiralID,
						TargetPlayerID = destroyingPlayer,
						TurnNumber = this.App.GameDatabase.GetTurnCount()
					});
				}
				this.App.GameDatabase.RemoveAdmiral(fi.AdmiralID);
			}
			else
				this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
				{
					EventType = TurnEventType.EV_ADMIRAL_ESCAPES,
					EventMessage = TurnEventMessage.EM_ADMIRAL_ESCAPES,
					PlayerID = fi.PlayerID,
					AdmiralID = fi.AdmiralID,
					TargetPlayerID = destroyingPlayer,
					TurnNumber = this.App.GameDatabase.GetTurnCount()
				});
		}

		protected override void OnUpdate()
		{
			if (this._subState == CommonCombatState.CombatSubState.Ending)
			{
				if (!this.UpdateEnding())
					return;
				this._subState = CommonCombatState.CombatSubState.Ended;
			}
			else
				this.UpdateRunning();
		}

		public override bool IsReady()
		{
			bool flag = true;
			if (this._crits != null && !this._crits.IsReady())
				flag = false;
			return flag;
		}

		public override void AddGameObject(IGameObject gameObject, bool autoSetActive = false)
		{
			if (gameObject == null)
				return;
			if (autoSetActive)
			{
				this._postLoadedObjects.Add(gameObject);
			}
			else
			{
				this._combat.PostObjectAddObjects(gameObject);
				this._crits.Add(gameObject);
			}
		}

		public override void RemoveGameObject(IGameObject gameObject)
		{
			if (gameObject == null)
				return;
			IGameObject gameObject1 = this._crits.Objects.FirstOrDefault<IGameObject>((Func<IGameObject, bool>)(x =>
		   {
			   if (x.ObjectID == gameObject.ObjectID)
				   return x is IDisposable;
			   return false;
		   }));
			if (gameObject1 != null)
			{
				if (this._starSystemObjects != null && this._starSystemObjects.Crits.Objects.Contains<IGameObject>(gameObject1))
					this._starSystemObjects.Crits.Remove(gameObject1);
				(gameObject1 as IDisposable).Dispose();
			}
			else
			{
				if (this._starSystemObjects != null)
				{
					gameObject1 = this._starSystemObjects.Crits.Objects.FirstOrDefault<IGameObject>((Func<IGameObject, bool>)(x =>
				   {
					   if (x.ObjectID == gameObject.ObjectID)
						   return x is IDisposable;
					   return false;
				   }));
					if (gameObject1 != null)
					{
						(gameObject1 as IDisposable).Dispose();
						this._starSystemObjects.Crits.Remove(gameObject);
					}
				}
				if (gameObject1 == null)
				{
					IGameObject gameObject2 = this.App.GetGameObject(gameObject.ObjectID);
					if (gameObject2 != null)
						this.App.ReleaseObject(gameObject2);
				}
			}
			this._crits.Remove(gameObject);
			foreach (CombatAI aiCommander in this.AI_Commanders)
				aiCommander.ObjectRemoved(gameObject);
			this._postLoadedObjects.Remove(gameObject);
		}

		private bool isHumanControlledAI(Player player)
		{
			if (player == null || !player.IsStandardPlayer)
				return false;
			bool flag = false;
			if (!this.SimMode && !player.IsAI())
			{
				PendingCombat currentCombat = this.App.Game.GetCurrentCombat();
				if (currentCombat != null)
				{
					if (currentCombat.CombatStanceSelections.ContainsKey(player.ID))
					{
						switch (currentCombat.CombatResolutionSelections[player.ID])
						{
							case ResolutionType.FIGHT:
							case ResolutionType.FIGHT_ON_FIGHT:
								CommonCombatState.Trace("Player " + (object)player.ID + " chose not to simulate.");
								flag = true;
								break;
						}
					}
				}
				else
					flag = !player.IsAI();
			}
			if (!this.SimMode && player == this.App.Game.LocalPlayer)
			{
				CommonCombatState.Trace("Local player, not sim, setting AI to false.");
				flag = true;
			}
			if (!this._authority)
				flag = true;
			return flag;
		}

		public void AddAItoCombat(Ship ship)
		{
			if (this.AI_Commanders.Count<CombatAI>() > 0)
			{
				foreach (CombatAI aiCommander in this.AI_Commanders)
				{
					if (aiCommander.m_Player == ship.Player)
						return;
				}
			}
			Dictionary<int, DiplomacyState> diploStates = (Dictionary<int, DiplomacyState>)null;
			if (this._initialDiploStates == null || !this._initialDiploStates.TryGetValue(ship.Player.ID, out diploStates))
				diploStates = (Dictionary<int, DiplomacyState>)null;
			int fleetID = 0;
			SpawnProfile spawnProfile = this._spawnPositions.FirstOrDefault<SpawnProfile>((Func<SpawnProfile, bool>)(x => x._playerID == ship.Player.ID));
			if (spawnProfile != null)
				fleetID = spawnProfile._fleetID;
			FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(fleetID);
			CombatAI combatAi;
			if ((this._testingState || !ship.Player.IsStandardPlayer) && ship.Faction.UsesNPCCombatAI || (fleetInfo != null && fleetInfo.IsTrapFleet || ship.CombatAI == SectionEnumerations.CombatAiType.TrapDrone))
			{
				combatAi = (CombatAI)new NPCFactionCombatAI(this.App, ship.Player, false, this._systemId, this._starSystemObjects, diploStates);
			}
			else
			{
				bool playerControlled = this.isHumanControlledAI(ship.Player);
				CommonCombatState.Trace("Player controlled: " + (object)playerControlled);
				combatAi = !this._pirateEncounterData.IsPirateEncounter || !this._pirateEncounterData.PiratePlayerIDs.Contains(ship.Player.ID) ? (this.App.Game.ScriptModules.Slaver == null || this.App.Game.ScriptModules.Slaver.PlayerID != ship.Player.ID ? new CombatAI(this.App, ship.Player, playerControlled, this._starSystemObjects, diploStates, false) : (CombatAI)new SlaverCombatAI(this.App, ship.Player, playerControlled, this._starSystemObjects, diploStates)) : (CombatAI)new PirateCombatAI(this.App, ship.Player, playerControlled, this._starSystemObjects, diploStates);
			}
			combatAi.SimMode = this.SimMode;
			combatAi.m_FleetID = fleetID;
			combatAi.InTestMode = this._testingState;
			ShipInfo si = this.App.GameDatabase.GetShipInfo(ship.DatabaseID, false);
			if (si != null)
				combatAi.m_SpawnProfile = this._spawnPositions.FirstOrDefault<SpawnProfile>((Func<SpawnProfile, bool>)(x => x._fleetID == si.FleetID));
			this.AI_Commanders.Add(combatAi);
		}

		public CombatAI GetCommanderForPlayerID(int playerID)
		{
			foreach (CombatAI aiCommander in this.AI_Commanders)
			{
				if (aiCommander.m_Player.ID == playerID)
					return aiCommander;
			}
			return (CombatAI)null;
		}

		protected void AddObject(ScriptMessageReader data)
		{
			InteropGameObjectType interopGameObjectType = (InteropGameObjectType)data.ReadInteger();
			IGameObject state1 = (IGameObject)null;
			IGameObject state2 = (IGameObject)null;
			if (interopGameObjectType == InteropGameObjectType.IGOT_SHIP)
			{
				state2 = this.App.GetGameObject(data.ReadInteger());
				Vector3 position = new Vector3();
				Vector3 forward = new Vector3();
				position.X = data.ReadSingle();
				position.Y = data.ReadSingle();
				position.Z = data.ReadSingle();
				forward.X = data.ReadSingle();
				forward.Y = data.ReadSingle();
				forward.Z = data.ReadSingle();
				Matrix world = Matrix.CreateWorld(position, forward, Vector3.UnitY);
				int designID = data.ReadInteger();
				int parentId = data.ReadInteger();
				int playerId = data.ReadInteger();
				bool flag = data.ReadBool();
				if (designID != 0)
				{
					DesignInfo designInfo = this.App.GameDatabase.GetDesignInfo(designID);
					ShipInfo shipInfo = new ShipInfo()
					{
						DesignID = 0,
						DesignInfo = designInfo,
						FleetID = 0,
						ParentID = 0,
						SerialNumber = 0,
						ShipName = string.Empty
					};
					state1 = (IGameObject)Ship.CreateShip(this.App.Game, world, shipInfo, parentId, this._input.ObjectID, playerId, this._starSystemObjects.IsDeepSpace, (IEnumerable<Player>)this._playersInCombat);
					state1.PostSetProp("IsMirage", flag);
				}
			}
			if (state1 != null)
				this._postLoadedObjects.Add(state1);
			if (state2 == null)
				return;
			state2.PostNotifyObjectHasBeenAdded(state1 != null ? state1.ObjectID : 0);
		}

		protected void AddObjects(ScriptMessageReader data)
		{
			InteropGameObjectType interopGameObjectType = (InteropGameObjectType)data.ReadInteger();
			List<IGameObject> source = new List<IGameObject>();
			IGameObject state = (IGameObject)null;
			if (interopGameObjectType == InteropGameObjectType.IGOT_SHIP)
			{
				state = this.App.GetGameObject(data.ReadInteger());
				int num = data.ReadInteger();
				int designID = data.ReadInteger();
				int parentId = data.ReadInteger();
				int playerId = data.ReadInteger();
				DesignInfo designInfo = this.App.GameDatabase.GetDesignInfo(designID);
				ShipInfo shipInfo = new ShipInfo()
				{
					DesignID = 0,
					DesignInfo = designInfo,
					FleetID = 0,
					ParentID = 0,
					SerialNumber = 0,
					ShipName = string.Empty
				};
				for (int index = 0; index < num; ++index)
					source.Add((IGameObject)Ship.CreateShip(this.App.Game, Matrix.Identity, shipInfo, parentId, this._input.ObjectID, playerId, this._starSystemObjects.IsDeepSpace, (IEnumerable<Player>)this._playersInCombat));
			}
			if (source.Count > 0)
				this._postLoadedObjects.AddRange((IEnumerable<IGameObject>)source.ToArray());
			if (state == null)
				return;
			state.PostNotifyObjectsHaveBeenAdded(source.Select<IGameObject, int>((Func<IGameObject, int>)(x => x.ObjectID)).ToArray<int>());
		}

		protected void RemoveObject(ScriptMessageReader data)
		{
			this.RemoveGameObject(this.App.GetGameObject(data.ReadInteger()));
		}

		protected void RemoveObjects(ScriptMessageReader data)
		{
			for (int id = data.ReadInteger(); id != 0; id = data.ReadInteger())
				this.RemoveGameObject(this.App.GetGameObject(id));
		}

		private bool CheckVictory()
		{
			foreach (CombatAI aiCommander in this.AI_Commanders)
			{
				if (aiCommander.VictoryConditionsAreMet())
					return true;
			}
			List<Player> playersWithAssets = this._playersWithAssets;
			if (this.App.Game.ScriptModules.NeutronStar != null)
				playersWithAssets.RemoveAll((Predicate<Player>)(x => x.ID == this.App.Game.ScriptModules.NeutronStar.PlayerID));
			foreach (int num1 in this._combatStanceMap.Keys.ToList<int>())
			{
				int x = num1;
				if (playersWithAssets.Any<Player>((Func<Player, bool>)(p => p.ID == x)))
				{
					foreach (int num2 in this._combatStanceMap[x].Keys.ToList<int>())
					{
						int y = num2;
						if (playersWithAssets.Any<Player>((Func<Player, bool>)(p => p.ID == y)) && !this._combatStanceMap[x][y])
							return false;
					}
				}
			}
			return true;
		}

		private bool EncounterIsNeutral(
		  int playerAID,
		  int playerBID,
		  AsteroidMonitorInfo ami,
		  MorrigiRelicInfo mri)
		{
			if (ami == null && mri == null)
				return false;
			if (ami != null && this.App.Game.ScriptModules.AsteroidMonitor != null && (this.App.Game.ScriptModules.AsteroidMonitor.PlayerID == playerAID || this.App.Game.ScriptModules.AsteroidMonitor.PlayerID == playerBID))
				return !ami.IsAggressive;
			if (mri != null && this.App.Game.ScriptModules.MorrigiRelic != null && (this.App.Game.ScriptModules.MorrigiRelic.PlayerID == playerAID || this.App.Game.ScriptModules.MorrigiRelic.PlayerID == playerBID))
				return !mri.IsAggressive;
			return false;
		}

		private void RebuildInitialDiploStates(AsteroidMonitorInfo ami, MorrigiRelicInfo mri)
		{
			this._initialDiploStates = new Dictionary<int, Dictionary<int, DiplomacyState>>();
			foreach (Player player3 in this._playersInCombat)
			{
				Player player1 = player3;
				Dictionary<int, DiplomacyState> dictionary;
				if (!this._initialDiploStates.TryGetValue(player1.ID, out dictionary))
				{
					dictionary = new Dictionary<int, DiplomacyState>();
					this._initialDiploStates[player1.ID] = dictionary;
				}
				foreach (Player player4 in this._playersInCombat)
				{
					Player player2 = player4;
					dictionary[player2.ID] = !this._testingState ? (!this._trapCombatData.IsTrapCombat || !this._trapCombatData.TrapPlayers.Any<int>((Func<int, bool>)(x =>
				   {
					   if (x != player1.ID)
						   return x == player2.ID;
					   return true;
				   })) ? (!this._pirateEncounterData.IsPirateEncounter || !this._pirateEncounterData.PiratePlayerIDs.Any<int>((Func<int, bool>)(x =>
		 {
			 if (x != player1.ID)
				 return x == player2.ID;
			 return true;
		 })) ? (this.EncounterIsNeutral(player1.ID, player2.ID, ami, mri) ? DiplomacyState.NEUTRAL : this.App.GameDatabase.GetDiplomacyStateBetweenPlayers(player1.ID, player2.ID)) : DiplomacyState.WAR) : DiplomacyState.WAR) : (player1.ID == player2.ID ? DiplomacyState.NEUTRAL : DiplomacyState.WAR);
				}
			}
		}

		private void UpdateDiploStatesInScript()
		{
			if (this._initialDiploStates == null || this._trapCombatData.IsTrapCombat || this._pirateEncounterData.IsPirateEncounter)
				return;
			List<int> intList = new List<int>();
			foreach (KeyValuePair<int, Dictionary<int, DiplomacyState>> initialDiploState in this._initialDiploStates)
			{
				PlayerInfo playerInfo1 = this.App.GameDatabase.GetPlayerInfo(initialDiploState.Key);
				if (playerInfo1 != null && playerInfo1.isStandardPlayer)
				{
					foreach (KeyValuePair<int, DiplomacyState> keyValuePair in initialDiploState.Value)
					{
						if (!intList.Contains(keyValuePair.Key))
						{
							PlayerInfo playerInfo2 = this.App.GameDatabase.GetPlayerInfo(keyValuePair.Key);
							if (playerInfo2 != null && playerInfo2.isStandardPlayer)
							{
								DiplomacyInfo diplomacyInfo = this.App.GameDatabase.GetDiplomacyInfo(initialDiploState.Key, keyValuePair.Key);
								if (diplomacyInfo != null)
									this.App.Game.GameDatabase.UpdateDiplomacyState(initialDiploState.Key, keyValuePair.Key, keyValuePair.Value, diplomacyInfo.Relations, true);
							}
						}
					}
					intList.Add(initialDiploState.Key);
				}
			}
		}

		public DiplomacyState GetDiplomacyState(int playerA, int playerB)
		{
			DiplomacyState diplomacyState = DiplomacyState.NEUTRAL;
			if (this._initialDiploStates != null)
			{
				Dictionary<int, DiplomacyState> dictionary;
				if (!this._initialDiploStates.TryGetValue(playerA, out dictionary))
					diplomacyState = DiplomacyState.NEUTRAL;
				else if (!dictionary.TryGetValue(playerB, out diplomacyState))
					diplomacyState = DiplomacyState.NEUTRAL;
			}
			return diplomacyState;
		}

		public void SetDiplomacyState(int playerA, int playerB, DiplomacyState state)
		{
			if (this._initialDiploStates == null)
				return;
			if (this._initialDiploStates.ContainsKey(playerA) && this._initialDiploStates[playerA].ContainsKey(playerB))
				this._initialDiploStates[playerA][playerB] = state;
			if (!this._initialDiploStates.ContainsKey(playerB) || !this._initialDiploStates[playerB].ContainsKey(playerA))
				return;
			this._initialDiploStates[playerB][playerA] = state;
		}

		private void DetectionUpdate(List<IGameObject> objects)
		{
			List<int> intList = new List<int>();
			List<Ship> source = new List<Ship>();
			List<Ship> shipList = new List<Ship>();
			List<StellarBody> stellarBodyList = new List<StellarBody>();
			this.m_DetectionSpheres.Clear();
			this.m_SlewPlanetDetectionSpheres.Clear();
			this._playersWithAssets.Clear();
			float val2 = CombatAI.GetMaxWeaponRangeFromShips(objects.OfType<Ship>().ToList<Ship>()) + 2000f;
			foreach (IGameObject gameObject in objects)
			{
				if (gameObject is Ship)
				{
					Ship ship = gameObject as Ship;
					if (!Ship.IsActiveShip(ship))
					{
						if (ship.DockedWithParent)
							shipList.Add(ship);
					}
					else
					{
						DetectionSpheres detectionSpheres1 = new DetectionSpheres(ship.Player.ID, ship.Maneuvering.Position);
						detectionSpheres1.minRadius = ship.ShipSphere.radius + 2000f;
						detectionSpheres1.sensorRange = ship.SensorRange;
						detectionSpheres1.slewModeRange = this.App.AssetDatabase.SlewModeExitRange;
						detectionSpheres1.ignoreNeutralPlanets = ship.ShipClass == ShipClass.Station;
						DetectionSpheres detectionSpheres2 = new DetectionSpheres(ship.Player.ID, ship.Maneuvering.Position);
						detectionSpheres2.slewModeRange = Math.Max(detectionSpheres1.slewModeRange, CombatAI.GetMaxWeaponRange(ship, false));
						detectionSpheres2.ignoreNeutralPlanets = ship.ShipClass == ShipClass.Station;
						if (!this.m_SlewMode)
						{
							detectionSpheres1.slewModeRange = Math.Max(detectionSpheres1.slewModeRange + this.App.AssetDatabase.SlewModeEnterOffset, this.App.AssetDatabase.SlewModeExitRange + 2000f);
							detectionSpheres2.slewModeRange += this.App.AssetDatabase.SlewModeEnterOffset;
						}
						if (ship.IsUnderAttack)
							detectionSpheres1.sensorRange = Math.Max(detectionSpheres1.sensorRange, val2);
						this.m_DetectionSpheres.Add(detectionSpheres1);
						this.m_SlewPlanetDetectionSpheres.Add(detectionSpheres2);
						float length = ship.Maneuvering.Velocity.Length;
						ship.Signature = Math.Min(ship.Signature - 0.05f, length);
						if (!intList.Contains(ship.Player.ID))
							intList.Add(ship.Player.ID);
						if (!this._playersWithAssets.Contains(ship.Player) && this._playersInCombat.Any<Player>((Func<Player, bool>)(x => x == ship.Player)))
							this._playersWithAssets.Add(ship.Player);
						source.Add(gameObject as Ship);
					}
				}
				else if (gameObject is StellarBody)
				{
					StellarBody stellarBody = gameObject as StellarBody;
					if (stellarBody.Parameters.ColonyPlayerID != 0 && stellarBody.Population > 0.0)
					{
						if (!intList.Contains(stellarBody.Parameters.ColonyPlayerID))
							intList.Add(stellarBody.Parameters.ColonyPlayerID);
						Player p = this.App.Game.GetPlayerObject(stellarBody.Parameters.ColonyPlayerID);
						if (p != null && !this._playersWithAssets.Contains(p) && this._playersInCombat.Any<Player>((Func<Player, bool>)(x => x == p)))
							this._playersWithAssets.Add(p);
						DetectionSpheres detectionSpheres = new DetectionSpheres(stellarBody.Parameters.ColonyPlayerID, stellarBody.Parameters.Position)
						{
							minRadius = stellarBody.Parameters.Radius + Math.Min(this.App.AssetDatabase.DefaultPlanetSensorRange * 0.25f, 2000f),
							sensorRange = stellarBody.Parameters.Radius + this.App.AssetDatabase.DefaultPlanetSensorRange
						};
						detectionSpheres.slewModeRange = detectionSpheres.sensorRange;
						detectionSpheres.isPlanet = true;
						detectionSpheres.ignoreNeutralPlanets = true;
						this.m_DetectionSpheres.Add(detectionSpheres);
						stellarBodyList.Add(gameObject as StellarBody);
					}
				}
			}
			bool flag = intList.Count > 1;
			foreach (Ship ship in source)
			{
				foreach (int num1 in intList)
				{
					if (num1 != ship.Player.ID)
					{
						Ship.DetectionState detectionStateForPlayer = ship.GetDetectionStateForPlayer(num1);
						bool spotted = detectionStateForPlayer.spotted;
						detectionStateForPlayer.scanned = false;
						float num2 = ship.ShipSphere.radius + ship.BonusSpottedRange;
						detectionStateForPlayer.spotted = (double)ship.BonusSpottedRange < 0.0;
						foreach (DetectionSpheres detectionSphere in this.m_DetectionSpheres)
						{
							if (detectionSphere.playerID == num1)
							{
								float lengthSquared = (detectionSphere.center - ship.Maneuvering.Position).LengthSquared;
								if (!detectionStateForPlayer.spotted)
								{
									float num3 = detectionSphere.minRadius + num2;
									if ((double)lengthSquared <= (double)num3 * (double)num3)
										detectionStateForPlayer.spotted = true;
								}
								if (!detectionStateForPlayer.scanned)
								{
									float num3 = detectionSphere.sensorRange + ship.ShipSphere.radius;
									if ((double)num3 * (double)num3 > (double)lengthSquared)
										detectionStateForPlayer.scanned = true;
								}
								if (flag)
								{
									switch (this.GetDiplomacyState(ship.Player.ID, num1))
									{
										case DiplomacyState.WAR:
											float num4 = detectionSphere.slewModeRange + ship.ShipSphere.radius;
											if ((double)num4 * (double)num4 > (double)lengthSquared)
											{
												flag = false;
												break;
											}
											break;
										case DiplomacyState.ALLIED:
											break;
										default:
											if (!detectionSphere.isPlanet || ship.ShipClass != ShipClass.Station)
												goto case DiplomacyState.WAR;
											else
												break;
									}
								}
								if (detectionStateForPlayer.spotted && detectionStateForPlayer.scanned)
								{
									if (!flag)
										break;
								}
							}
						}
						if (num1 == this.App.Game.LocalPlayer.ID && (detectionStateForPlayer.spotted != spotted || ship.Visible != detectionStateForPlayer.spotted))
							ship.Visible = detectionStateForPlayer.spotted;
					}
					else if (num1 == this.App.Game.LocalPlayer.ID && !ship.Visible)
					{
						ship.Visible = true;
						Ship.DetectionState detectionStateForPlayer = ship.GetDetectionStateForPlayer(num1);
						detectionStateForPlayer.spotted = true;
						detectionStateForPlayer.scanned = true;
					}
				}
			}
			foreach (Ship ship1 in shipList)
			{
				Ship rider = ship1;
				Ship ship2 = source.FirstOrDefault<Ship>((Func<Ship, bool>)(x => x.DatabaseID == rider.ParentDatabaseID));
				if (ship2 != null)
				{
					foreach (int playerID in intList)
					{
						Ship.DetectionState detectionStateForPlayer = rider.GetDetectionStateForPlayer(playerID);
						if (playerID == this.App.Game.LocalPlayer.ID)
						{
							detectionStateForPlayer.spotted = true;
							detectionStateForPlayer.scanned = true;
						}
						else
						{
							detectionStateForPlayer.spotted = false;
							detectionStateForPlayer.scanned = false;
						}
					}
					if (rider.Visible != ship2.Visible)
						rider.Visible = ship2.Visible;
				}
			}
			if (flag)
			{
				foreach (StellarBody stellarBody in stellarBodyList)
				{
					foreach (int playerB in intList)
					{
						if (stellarBody.Parameters.ColonyPlayerID != 0 && playerB != stellarBody.Parameters.ColonyPlayerID)
						{
							foreach (DetectionSpheres planetDetectionSphere in this.m_SlewPlanetDetectionSpheres)
							{
								if (planetDetectionSphere.playerID == playerB)
								{
									switch (this.GetDiplomacyState(stellarBody.Parameters.ColonyPlayerID, playerB))
									{
										case DiplomacyState.WAR:
											Vector3 vector3 = planetDetectionSphere.center - stellarBody.Parameters.Position;
											float num = planetDetectionSphere.slewModeRange + stellarBody.Parameters.Radius;
											if ((double)num * (double)num > (double)vector3.LengthSquared)
											{
												flag = false;
												goto label_84;
											}
											else
												continue;
										case DiplomacyState.ALLIED:
											continue;
										default:
											if (planetDetectionSphere.ignoreNeutralPlanets)
												continue;
											goto case DiplomacyState.WAR;
									}
								}
							}
						}
					label_84:
						if (!flag)
							break;
					}
					if (!flag)
						break;
				}
			}
			if (this._pointsOfInterest.Count > 0)
			{
				foreach (PointOfInterest pointOfInterest in this._pointsOfInterest)
				{
					PointOfInterest poi = pointOfInterest;
					if (!poi.HasBeenSeen)
					{
						if (poi.TargetID != 0)
						{
							IGameObject gameObject = this._crits.Objects.FirstOrDefault<IGameObject>((Func<IGameObject, bool>)(x => x.ObjectID == poi.TargetID));
							if (gameObject != null && gameObject is Ship)
							{
								Ship.DetectionState detectionStateForPlayer = (gameObject as Ship).GetDetectionStateForPlayer(this.App.LocalPlayer.ID);
								poi.HasBeenSeen = detectionStateForPlayer.scanned;
								continue;
							}
						}
						foreach (DetectionSpheres detectionSphere in this.m_DetectionSpheres)
						{
							if (detectionSphere.playerID == this.App.LocalPlayer.ID)
							{
								Vector3 vector3 = detectionSphere.center - poi.Position;
								float num = (float)((double)detectionSphere.minRadius + (double)this.App.AssetDatabase.GlobalSpotterRangeData.SpotterValues[1] + 2000.0);
								if ((double)num * (double)num > (double)vector3.LengthSquared)
								{
									poi.HasBeenSeen = true;
									break;
								}
							}
						}
					}
				}
			}
			if (this.m_SlewMode == flag)
				return;
			this.m_SlewMode = flag;
			this._combat.PostSetProp("SetSlewMode", this.m_SlewMode);
		}

		private void CheckToReturnControlZonesToOwner(int playerID)
		{
			List<FleetInfo> list = this.App.GameDatabase.GetFleetsByPlayerAndSystem(playerID, this._systemId, FleetType.FL_NORMAL).ToList<FleetInfo>();
			int num = 0;
			foreach (FleetInfo fleetInfo in list)
			{
				MissionInfo missionByFleetId = this.App.GameDatabase.GetMissionByFleetID(fleetInfo.ID);
				if (missionByFleetId == null || missionByFleetId.Type != MissionType.RETREAT && missionByFleetId.Type != MissionType.RETURN)
					++num;
			}
			if (num != 0)
				return;
			StarSystem.RemoveSystemPlayerColor(this.App.GameDatabase, this._systemId, playerID);
		}

		private void ApplyRewardsForShipDeath(
		  ShipInfo si,
		  int playerID,
		  int killedByPlayerGOID,
		  FleetInfo fi = null)
		{
			if (this._pirateEncounterData.IsPirateEncounter && this._pirateEncounterData.PiratePlayerIDs.Contains(playerID))
			{
				Player playerByObjectId = this.App.GetPlayerByObjectID(killedByPlayerGOID);
				if (playerByObjectId == null)
					return;
				PiracyGlobalData.PiracyBountyType piracyBountyType = this.App.Game.ScriptModules.Pirates.PirateBaseDesignId == si.DesignID ? PiracyGlobalData.PiracyBountyType.PirateBaseDestroyed : PiracyGlobalData.PiracyBountyType.PirateShipDestroyed;
				int bounty = this.App.AssetDatabase.GlobalPiracyData.Bounties[(int)piracyBountyType];
				if (this._pirateEncounterData.PlayerBounties.ContainsKey(playerByObjectId.ID))
				{
					Dictionary<int, int> playerBounties;
					int id;
					(playerBounties = this._pirateEncounterData.PlayerBounties)[id = playerByObjectId.ID] = playerBounties[id] + bounty;
				}
				else
					this._pirateEncounterData.PlayerBounties.Add(playerByObjectId.ID, bounty);
				if (piracyBountyType == PiracyGlobalData.PiracyBountyType.PirateBaseDestroyed)
				{
					foreach (int num in this.App.GameDatabase.GetStandardPlayerIDs().ToList<int>())
					{
						if (num != playerByObjectId.ID)
						{
							string factionName = this.App.GameDatabase.GetFactionName(this.App.GameDatabase.GetPlayerFactionID(num));
							int reactionAmount = 0;
							this.App.AssetDatabase.GlobalPiracyData.ReactionBonuses.TryGetValue(factionName, out reactionAmount);
							this.App.GameDatabase.ApplyDiplomacyReaction(num, playerByObjectId.ID, reactionAmount, new StratModifiers?(StratModifiers.DiplomacyReactionElimPirates), 1);
						}
					}
					this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_PIRATE_BASE_DESTROYED,
						EventMessage = TurnEventMessage.EM_PIRATE_BASE_DESTROYED,
						PlayerID = playerByObjectId.ID,
						SystemID = this._systemId,
						TurnNumber = this.App.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
				}
				else
				{
					if (playerID != this.App.Game.ScriptModules.Pirates.PlayerID)
						return;
					PirateBaseInfo pbi = this.App.GameDatabase.GetPirateBaseInfos().FirstOrDefault<PirateBaseInfo>((Func<PirateBaseInfo, bool>)(x => x.SystemId == this._systemId));
					if (pbi == null)
						return;
					pbi.NumShips = Math.Max(pbi.NumShips - 1, 0);
					this.App.GameDatabase.UpdatePirateBaseInfo(pbi);
				}
			}
			else if (this._pirateEncounterData.IsPirateEncounter && si.DesignInfo.Role == ShipRole.FREIGHTER)
			{
				Player playerByObjectId = this.App.GetPlayerByObjectID(killedByPlayerGOID);
				if (playerByObjectId == null)
					return;
				int bounty = this.App.AssetDatabase.GlobalPiracyData.Bounties[2];
				if (this._pirateEncounterData.PlayerBounties.ContainsKey(playerByObjectId.ID))
				{
					Dictionary<int, int> playerBounties;
					int id;
					(playerBounties = this._pirateEncounterData.PlayerBounties)[id = playerByObjectId.ID] = playerBounties[id] + bounty;
				}
				else
					this._pirateEncounterData.PlayerBounties.Add(playerByObjectId.ID, bounty);
			}
			else if (this.App.Game.ScriptModules.GhostShip != null && this.App.Game.ScriptModules.GhostShip.PlayerID == playerID)
			{
				foreach (PlayerInfo playerInfo in this.App.GameDatabase.GetPlayerInfos().ToList<PlayerInfo>())
				{
					if (playerInfo.isStandardPlayer && playerInfo.ID != playerID)
					{
						DiplomacyInfo diplomacyInfo = this.App.GameDatabase.GetDiplomacyInfo(playerID, playerInfo.ID);
						if (diplomacyInfo != null)
							this.App.Game.GameDatabase.UpdateDiplomacyState(playerID, playerInfo.ID, diplomacyInfo.State, diplomacyInfo.Relations + 100, true);
					}
				}
				Player playerByObjectId = this.App.GetPlayerByObjectID(killedByPlayerGOID);
				if (playerByObjectId == null)
					return;
				float stratModifier = this.App.GameDatabase.GetStratModifier<float>(StratModifiers.LeviathanResearchModifier, playerByObjectId.ID);
				this.App.GameDatabase.SetStratModifier(StratModifiers.LeviathanResearchModifier, playerByObjectId.ID, (object)(float)((double)stratModifier + 0.200000002980232));
				GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_GHOSTSHIP_KILLED, playerByObjectId.ID, new int?(), new int?(), new int?());
			}
			else if (this.App.Game.ScriptModules.VonNeumann != null && this.App.Game.ScriptModules.VonNeumann.PlayerID == playerID)
			{
				if (si.DesignID != VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.CollectorMothership].DesignId)
					return;
				Player playerByObjectId = this.App.GetPlayerByObjectID(killedByPlayerGOID);
				if (playerByObjectId == null || playerID == playerByObjectId.ID)
					return;
				PlayerInfo playerInfo = this.App.GameDatabase.GetPlayerInfo(playerByObjectId.ID);
				double netRevenue = this.App.Game.CalculateNetRevenue(playerInfo);
				int num = (int)GameSession.SplitResearchRevenue(playerInfo, netRevenue);
				int researchPoints = this.App.Game.ConvertToResearchPoints(playerInfo.ID, (double)num);
				this.App.GameDatabase.UpdatePlayerAdditionalResearchPoints(playerByObjectId.ID, playerInfo.AdditionalResearchPoints + (int)((double)researchPoints * 0.0500000007450581));
			}
			else if (this.App.Game.ScriptModules.Locust != null && this.App.Game.ScriptModules.Locust.PlayerID == playerID)
			{
				if (si.DesignID != this.App.Game.ScriptModules.Locust.HeraldMoonDesignId && si.DesignID != this.App.Game.ScriptModules.Locust.WorldShipDesignId)
					return;
				Player playerByObjectId = this.App.GetPlayerByObjectID(killedByPlayerGOID);
				if (playerByObjectId == null)
					return;
				this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
				{
					EventType = TurnEventType.EV_LOCUST_SHIP_DESTROYED,
					EventMessage = TurnEventMessage.EM_LOCUST_SHIP_DESTROYED,
					PlayerID = playerByObjectId.ID,
					SystemID = this._systemId,
					TurnNumber = this.App.GameDatabase.GetTurnCount(),
					ShowsDialog = false
				});
				bool flag = false;
				foreach (LocustSwarmInfo locustSwarmInfo in this.App.GameDatabase.GetLocustSwarmInfos().ToList<LocustSwarmInfo>())
				{
					if (locustSwarmInfo.FleetId.HasValue)
					{
						flag = this.App.GameDatabase.GetShipInfoByFleetID(locustSwarmInfo.FleetId.Value, true).ToList<ShipInfo>().Any<ShipInfo>((Func<ShipInfo, bool>)(x =>
					   {
						   if (x.DesignID == this.App.Game.ScriptModules.Locust.HeraldMoonDesignId || x.DesignID == this.App.Game.ScriptModules.Locust.WorldShipDesignId)
							   return x.ID != si.ID;
						   return false;
					   }));
						if (flag)
							break;
					}
				}
				if (flag)
					return;
				this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
				{
					EventType = TurnEventType.EV_LOCUST_INFESTATION_DEFEATED,
					EventMessage = TurnEventMessage.EM_LOCUST_INFESTATION_DEFEATED,
					PlayerID = playerByObjectId.ID,
					TurnNumber = this.App.GameDatabase.GetTurnCount(),
					ShowsDialog = false
				});
			}
			else if (si.DesignInfo.StationType == StationType.GATE || fi != null && fi.IsGateFleet)
			{
				this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
				{
					EventType = TurnEventType.EV_GATE_DESTROYED,
					EventMessage = TurnEventMessage.EM_GATE_DESTROYED,
					PlayerID = si.DesignInfo.PlayerID,
					SystemID = this._systemId,
					TurnNumber = this.App.GameDatabase.GetTurnCount(),
					ShowsDialog = false
				});
			}
			else
			{
				if (this.App.Game.ScriptModules.Swarmers == null || this.App.Game.ScriptModules.Swarmers.PlayerID != playerID)
					return;
				double num = 0.0;
				if (si.DesignID == this.App.Game.ScriptModules.Swarmers.HiveDesignID)
					num = 50000.0;
				else if (si.DesignID == this.App.Game.ScriptModules.Swarmers.SwarmQueenDesignID)
					num = 40000.0;
				if (num <= 0.0)
					return;
				Player playerByObjectId = this.App.GetPlayerByObjectID(killedByPlayerGOID);
				if (playerByObjectId == null || playerByObjectId.ID == playerID)
					return;
				PlayerInfo playerInfo = this.App.GameDatabase.GetPlayerInfo(playerID);
				if (playerInfo == null || !playerInfo.isStandardPlayer)
					return;
				this.App.GameDatabase.UpdatePlayerSavings(playerInfo.ID, playerInfo.Savings + num);
			}
		}

		private void ProcessShipsHitByNodeCannon()
		{
			if (this._systemId == 0 || this._hitByNodeCannon.Count == 0)
				return;
			Vector3 systemOrigin = this.App.GameDatabase.GetStarSystemOrigin(this._systemId);
			List<StarSystemInfo> list1 = this.App.GameDatabase.GetSystemsInRange(systemOrigin, 10f).ToList<StarSystemInfo>();
			foreach (StarSystemInfo starSystemInfo in list1)
			{
				if (starSystemInfo.ID == this._systemId)
				{
					list1.Remove(starSystemInfo);
					break;
				}
			}
			if (list1.Count == 0)
				return;
			list1.Sort((Comparison<StarSystemInfo>)((a, b) => (a.Origin - systemOrigin).LengthSquared.CompareTo((b.Origin - systemOrigin).LengthSquared)));
			int index = this._random.NextInclusive(0, Math.Max(Math.Min(list1.Count - 1, 3), 0));
			StarSystemInfo starSystemInfo1 = list1[index];
			Dictionary<int, List<Ship>> dictionary = new Dictionary<int, List<Ship>>();
			foreach (Ship ship in this._hitByNodeCannon)
			{
				if (dictionary.ContainsKey(ship.Player.ID))
					dictionary[ship.Player.ID].Add(ship);
				else
					dictionary.Add(ship.Player.ID, new List<Ship>()
		  {
			ship
		  });
			}
			foreach (KeyValuePair<int, List<Ship>> keyValuePair in dictionary)
			{
				KeyValuePair<int, List<Ship>> fleets = keyValuePair;
				if (fleets.Value.Count != 0)
				{
					FleetInfo fleetInfo1 = this._lastPendingCombat.CombatResults.FleetsInCombat.FirstOrDefault<FleetInfo>((Func<FleetInfo, bool>)(x => x.PlayerID == fleets.Key));
					int num = this.App.GameDatabase.InsertFleet(fleets.Key, 0, starSystemInfo1.ID, fleetInfo1 != null ? fleetInfo1.SupportingSystemID : 0, App.Localize("@FLEET_NODE_CANNONED_FLEET"), FleetType.FL_NORMAL);
					this.App.GameDatabase.UpdateFleetLocation(num, starSystemInfo1.ID, new int?());
					int missionID = this.App.GameDatabase.InsertMission(num, MissionType.RETREAT, 0, 0, 0, 0, false, new int?());
					this.App.GameDatabase.InsertWaypoint(missionID, WaypointType.ReturnHome, new int?());
					this.App.GameDatabase.InsertWaypoint(missionID, WaypointType.DisbandFleet, new int?());
					foreach (Ship ship in fleets.Value)
					{
						if (ship != null)
						{
							ShipInfo shipInfo = this.App.GameDatabase.GetShipInfo(ship.DatabaseID, false);
							if (shipInfo != null)
							{
								FleetInfo fleetInfo2 = this.App.GameDatabase.GetFleetInfo(shipInfo.FleetID);
								this.App.GameDatabase.TransferShip(ship.DatabaseID, num);
								if (fleetInfo2 != null && fleetInfo2.ID != num && this.App.GameDatabase.GetShipsByFleetID(fleetInfo2.ID).Count<int>() == 0)
									this.App.GameDatabase.RemoveFleet(fleetInfo2.ID);
								this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
								{
									EventType = TurnEventType.EV_SHIPS_SCATTERED_NODE_CANNON,
									EventMessage = TurnEventMessage.EM_SHIPS_SCATTERED_NODE_CANNON,
									PlayerID = ship.Player.ID,
									ShipID = ship.DatabaseID,
									TurnNumber = this.App.GameDatabase.GetTurnCount(),
									ShowsDialog = false
								});
							}
						}
					}
				}
			}
			foreach (FleetInfo fleetInfo in this._lastPendingCombat.CombatResults.FleetsInCombat)
			{
				PlayerInfo playerInfo = fleetInfo != null ? this.App.GameDatabase.GetPlayerInfo(fleetInfo.PlayerID) : (PlayerInfo)null;
				if (playerInfo != null && playerInfo.isStandardPlayer && (!fleetInfo.IsDefenseFleet && !fleetInfo.IsGateFleet))
				{
					int retreatFleetId = this.App.Game.MCCarryOverData.GetRetreatFleetID(fleetInfo.SystemID, fleetInfo.ID);
					if (retreatFleetId == 0 || retreatFleetId != fleetInfo.ID)
					{
						List<ShipInfo> list2 = this.App.GameDatabase.GetShipInfoByFleetID(fleetInfo.ID, false).ToList<ShipInfo>();
						if (this.App.GameDatabase.GetShipsCommandPointQuota((IEnumerable<ShipInfo>)list2) < this.App.GameDatabase.GetShipsCommandPointCost((IEnumerable<ShipInfo>)list2))
						{
							if (retreatFleetId != 0)
							{
								foreach (int shipID in this.App.GameDatabase.GetShipsByFleetID(retreatFleetId).ToList<int>())
									this.App.GameDatabase.TransferShip(shipID, fleetInfo.ID);
								MissionInfo missionByFleetId = this.App.GameDatabase.GetMissionByFleetID(retreatFleetId);
								if (missionByFleetId != null)
									this.App.GameDatabase.RemoveMission(missionByFleetId.ID);
								this.App.GameDatabase.RemoveFleet(retreatFleetId);
								this.App.Game.MCCarryOverData.SetRetreatFleetID(fleetInfo.SystemID, fleetInfo.ID, fleetInfo.ID);
							}
							MissionInfo missionByFleetId1 = this.App.GameDatabase.GetMissionByFleetID(fleetInfo.ID);
							if (missionByFleetId1 != null)
								this.App.GameDatabase.RemoveMission(missionByFleetId1.ID);
							this.App.GameDatabase.InsertWaypoint(this.App.GameDatabase.InsertMission(fleetInfo.ID, MissionType.RETREAT, 0, 0, 0, 0, false, new int?()), WaypointType.ReturnHome, new int?());
							this.CheckToReturnControlZonesToOwner(playerInfo.ID);
						}
					}
				}
			}
		}

		private enum CombatSubState
		{
			Running,
			Ending,
			Ended,
		}

		public class PiracyFreighterInfo
		{
			public int FreighterID;
			public int ShipID;
		}

		public class PirateEncounterData
		{
			public bool IsPirateEncounter;
			public PirateBaseInfo PirateBase;
			public List<int> PiratePlayerIDs;
			public List<Ship> PoliceShipsInSystem;
			public Dictionary<int, List<Ship>> PirateShipsInSystem;
			public Dictionary<int, int> PlayerBounties;
			public Dictionary<int, List<CommonCombatState.PiracyFreighterInfo>> PlayerFreightersInSystem;
			public Dictionary<int, int> DestroyedFreighters;

			public PirateEncounterData()
			{
				this.IsPirateEncounter = false;
				this.PirateBase = (PirateBaseInfo)null;
				this.PiratePlayerIDs = new List<int>();
				this.PoliceShipsInSystem = new List<Ship>();
				this.PirateShipsInSystem = new Dictionary<int, List<Ship>>();
				this.PlayerBounties = new Dictionary<int, int>();
				this.PlayerFreightersInSystem = new Dictionary<int, List<CommonCombatState.PiracyFreighterInfo>>();
				this.DestroyedFreighters = new Dictionary<int, int>();
			}

			public void Clear()
			{
				this.IsPirateEncounter = false;
				this.PirateBase = (PirateBaseInfo)null;
				this.PiratePlayerIDs.Clear();
				this.PoliceShipsInSystem.Clear();
				this.PirateShipsInSystem.Clear();
				this.PlayerBounties.Clear();
				this.PlayerFreightersInSystem.Clear();
				this.DestroyedFreighters.Clear();
			}
		}

		public class ColonyTrapCombatData
		{
			public bool IsTrapCombat;
			public List<int> TrapPlayers;
			public Dictionary<int, int> ColonyFleetToTrap;
			public Dictionary<int, int> TrapToPlanet;
			public List<FleetInfo> TrapFleets;
			public List<FleetInfo> ColonyTrappedFleets;

			public ColonyTrapCombatData()
			{
				this.IsTrapCombat = false;
				this.ColonyFleetToTrap = new Dictionary<int, int>();
				this.TrapToPlanet = new Dictionary<int, int>();
				this.TrapPlayers = new List<int>();
				this.TrapFleets = new List<FleetInfo>();
				this.ColonyTrappedFleets = new List<FleetInfo>();
			}

			public void Clear()
			{
				this.IsTrapCombat = false;
				this.ColonyFleetToTrap.Clear();
				this.TrapToPlanet.Clear();
				this.TrapPlayers.Clear();
				this.TrapFleets.Clear();
				this.ColonyTrappedFleets.Clear();
			}
		}
	}
}
