// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.CombatAI
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Combat
{
	internal class CombatAI
	{
		public const float kEnemyGroupThreshold = 1.25f;
		public const float kEnemyGroupDistance = 2000f;
		public const float kPlanetPatrolOffset = 1500f;
		public const float kStarPatrolOffset = 2500f;
		public const float kPlanetOffset = 750f;
		public const float kStarOffset = 7500f;
		public const float kBufferDist = 500f;
		public const int kTaskGroupUpdateRate = 5;
		public const int kTaskGroupUpdateRateSimMode = 2;
		protected App m_Game;
		private OverallAIType m_AIType;
		private bool m_UseCTAMainMenu;
		protected int m_UpdateCounter;
		protected int m_NodeMawUpdateRate;
		protected int m_FramesElapsed;
		public int m_FleetID;
		public Player m_Player;
		public bool m_bIsHumanPlayerControlled;
		public bool m_bHasListener;
		protected List<Ship> m_Friendly;
		protected List<Ship> m_Enemy;
		protected List<StellarBody> m_Planets;
		protected List<StarModel> m_Stars;
		protected List<TaskGroup> m_TaskGroups;
		protected List<EnemyGroup> m_EnemyGroups;
		protected List<SpecWeaponControl> m_ShipWeaponControls;
		protected List<ShipPsionicControl> m_ShipPsionicControls;
		protected List<NodeMawInfo> m_NodeMaws;
		private List<int> m_EncounterPlayerIDs;
		private Dictionary<int, DiplomacyState> _currentDiploStates;
		protected int m_EGUpdatePhase;
		protected List<TacticalObjective> m_Objectives;
		protected CombatZoneEnemySpotted m_SpottedEnemies;
		protected CloakedShipDetection m_CloakedEnemyDetection;
		protected bool m_bEnemyShipsInSystem;
		public bool m_bPlanetsInitialized;
		public bool m_bObjectivesInitialized;
		private bool m_OwnsSystem;
		public SpawnProfile m_SpawnProfile;
		private Random m_Random;
		private int m_SystemID;
		private bool m_IsEncounterCombat;
		private float m_SystemRadius;
		private float m_MinSystemRadius;
		private bool _simMode;
		private bool _inTestMode;

		public void SetAIType(OverallAIType type)
		{
			this.m_AIType = type;
		}

		public OverallAIType GetAIType()
		{
			return this.m_AIType;
		}

		public bool OwnsSystem
		{
			get
			{
				return this.m_OwnsSystem;
			}
		}

		public Random AIRandom
		{
			get
			{
				return this.m_Random;
			}
		}

		public bool IsEncounterCombat
		{
			get
			{
				return this.m_IsEncounterCombat;
			}
		}

		public float SystemRadius
		{
			get
			{
				return this.m_SystemRadius;
			}
		}

		public float MinSystemRadius
		{
			get
			{
				return this.m_MinSystemRadius;
			}
		}

		public bool EnemiesPresentInSystem
		{
			get
			{
				return this.m_bEnemyShipsInSystem;
			}
		}

		public List<StellarBody> PlanetsInSystem
		{
			get
			{
				return this.m_Planets;
			}
		}

		public List<StarModel> StarsInSystem
		{
			get
			{
				return this.m_Stars;
			}
		}

		public CombatAI(
		  App game,
		  Player player,
		  bool playerControlled,
		  Kerberos.Sots.GameStates.StarSystem starSystem,
		  Dictionary<int, DiplomacyState> diploStates,
		  bool useCTAMainMenu = false)
		{
			this.m_Game = game;
			this.m_Player = player;
			this.m_bIsHumanPlayerControlled = playerControlled;
			this.m_TaskGroups = new List<TaskGroup>();
			this.m_EnemyGroups = new List<EnemyGroup>();
			this.m_ShipWeaponControls = new List<SpecWeaponControl>();
			this.m_ShipPsionicControls = new List<ShipPsionicControl>();
			this.m_Friendly = new List<Ship>();
			this.m_Enemy = new List<Ship>();
			this.m_Planets = new List<StellarBody>();
			this.m_Stars = new List<StarModel>();
			this.m_EGUpdatePhase = 0;
			this.m_Objectives = new List<TacticalObjective>();
			this.m_bPlanetsInitialized = false;
			this.m_bObjectivesInitialized = false;
			this.m_bEnemyShipsInSystem = false;
			this.m_bHasListener = false;
			this.m_IsEncounterCombat = false;
			this.m_UpdateCounter = 0;
			this.m_NodeMawUpdateRate = 0;
			this.m_FramesElapsed = 0;
			this.m_FleetID = 0;
			this.m_AIType = OverallAIType.AGGRESSIVE;
			this.m_Random = new Random();
			this.m_SpottedEnemies = new CombatZoneEnemySpotted(starSystem);
			this.m_CloakedEnemyDetection = new CloakedShipDetection();
			this._currentDiploStates = diploStates;
			this.m_SpawnProfile = (SpawnProfile)null;
			this.m_OwnsSystem = starSystem.GetPlanetsInSystem().Any<StellarBody>((Func<StellarBody, bool>)(x => x.Parameters.ColonyPlayerID == player.ID));
			this.m_UseCTAMainMenu = useCTAMainMenu;
			this.m_SystemID = starSystem.SystemID;
			this.m_EncounterPlayerIDs = CombatAI.GetAllEncounterPlayerIDs(game);
			this.InitializeNodeMaws(game, starSystem);
			this.m_SystemRadius = starSystem.GetSystemRadius();
			this.m_MinSystemRadius = starSystem.GetBaseOffset() * 5700f;
		}

		public virtual void Shutdown()
		{
			foreach (TaskGroup taskGroup in this.m_TaskGroups)
				taskGroup.ShutDown();
			foreach (SpecWeaponControl shipWeaponControl in this.m_ShipWeaponControls)
				shipWeaponControl.Shutdown();
			foreach (ShipPsionicControl shipPsionicControl in this.m_ShipPsionicControls)
				shipPsionicControl.Shutdown();
			foreach (TacticalObjective objective in this.m_Objectives)
				objective.Shutdown();
			this.m_Friendly.Clear();
			this.m_Enemy.Clear();
			this.m_TaskGroups.Clear();
			this.m_EnemyGroups.Clear();
			this.m_ShipWeaponControls.Clear();
			this.m_ShipPsionicControls.Clear();
			this.m_Planets.Clear();
			this.m_Stars.Clear();
			this.m_Objectives.Clear();
			this.m_Game = (App)null;
			this.m_Player = (Player)null;
			this.m_SpottedEnemies = (CombatZoneEnemySpotted)null;
			this.m_CloakedEnemyDetection = (CloakedShipDetection)null;
		}

		public virtual void ObjectRemoved(IGameObject obj)
		{
			if (obj is Ship)
			{
				Ship ship = obj as Ship;
				this.m_Friendly.Remove(ship);
				this.m_Enemy.Remove(ship);
				foreach (TaskGroup taskGroup in this.m_TaskGroups)
					taskGroup.ObjectRemoved(ship);
				foreach (EnemyGroup enemyGroup in this.m_EnemyGroups)
					enemyGroup.m_Ships.Remove(ship);
				List<TacticalObjective> tacticalObjectiveList = new List<TacticalObjective>();
				foreach (TacticalObjective objective in this.m_Objectives)
				{
					if (objective.m_PoliceOwner == obj)
					{
						objective.m_PoliceOwner = (Ship)null;
						tacticalObjectiveList.Add(objective);
					}
				}
				foreach (TacticalObjective tacticalObjective in tacticalObjectiveList)
					this.m_Objectives.Remove(tacticalObjective);
				List<SpecWeaponControl> specWeaponControlList = new List<SpecWeaponControl>();
				foreach (SpecWeaponControl shipWeaponControl in this.m_ShipWeaponControls)
				{
					if (shipWeaponControl.ControlledShip == ship)
						specWeaponControlList.Add(shipWeaponControl);
					else
						shipWeaponControl.ObjectRemoved(obj);
				}
				foreach (SpecWeaponControl specWeaponControl in specWeaponControlList)
				{
					specWeaponControl.Shutdown();
					this.m_ShipWeaponControls.Remove(specWeaponControl);
				}
				List<ShipPsionicControl> shipPsionicControlList = new List<ShipPsionicControl>();
				foreach (ShipPsionicControl shipPsionicControl in this.m_ShipPsionicControls)
				{
					if (shipPsionicControl.ControlledShip == ship)
						shipPsionicControlList.Add(shipPsionicControl);
				}
				foreach (ShipPsionicControl shipPsionicControl in shipPsionicControlList)
				{
					shipPsionicControl.Shutdown();
					this.m_ShipPsionicControls.Remove(shipPsionicControl);
				}
			}
			if (obj is StellarBody)
				this.m_Planets.Remove(obj as StellarBody);
			if (!(obj is StarModel))
				return;
			this.m_Stars.Remove(obj as StarModel);
		}

		private static List<int> GetAllEncounterPlayerIDs(App game)
		{
			List<int> intList = new List<int>();
			if (game.Game.ScriptModules.AsteroidMonitor != null)
				intList.Add(game.Game.ScriptModules.AsteroidMonitor.PlayerID);
			if (game.Game.ScriptModules.MorrigiRelic != null)
				intList.Add(game.Game.ScriptModules.MorrigiRelic.PlayerID);
			if (game.Game.ScriptModules.Gardeners != null)
				intList.Add(game.Game.ScriptModules.Gardeners.PlayerID);
			if (game.Game.ScriptModules.Swarmers != null)
				intList.Add(game.Game.ScriptModules.Swarmers.PlayerID);
			if (game.Game.ScriptModules.VonNeumann != null)
				intList.Add(game.Game.ScriptModules.VonNeumann.PlayerID);
			if (game.Game.ScriptModules.Locust != null)
				intList.Add(game.Game.ScriptModules.Locust.PlayerID);
			if (game.Game.ScriptModules.Comet != null)
				intList.Add(game.Game.ScriptModules.Comet.PlayerID);
			if (game.Game.ScriptModules.SystemKiller != null)
				intList.Add(game.Game.ScriptModules.SystemKiller.PlayerID);
			if (game.Game.ScriptModules.MeteorShower != null)
				intList.Add(game.Game.ScriptModules.MeteorShower.PlayerID);
			if (game.Game.ScriptModules.Spectre != null)
				intList.Add(game.Game.ScriptModules.Spectre.PlayerID);
			if (game.Game.ScriptModules.GhostShip != null)
				intList.Add(game.Game.ScriptModules.GhostShip.PlayerID);
			return intList;
		}

		public bool IsEncounterPlayer(int playerId)
		{
			return this.m_EncounterPlayerIDs.Contains(playerId);
		}

		public float GetCloakedDetectionPercent(Ship ship)
		{
			return this.m_CloakedEnemyDetection.GetVisibilityPercent(ship);
		}

		public bool IsShipDetected(Ship ship)
		{
			if (ship == null || (double)this.GetCloakedDetectionPercent(ship) <= 0.0)
				return false;
			if (ship.IsDetected(this.m_Player))
				return true;
			return this.m_SpottedEnemies.IsShipSpotted(ship);
		}

		public bool ShipCanChangeTarget(Ship ship)
		{
			if (ship == null)
				return false;
			ShipPsionicControl shipPsionicControl = this.m_ShipPsionicControls.FirstOrDefault<ShipPsionicControl>((Func<ShipPsionicControl, bool>)(x => x.ControlledShip == ship));
			if (shipPsionicControl != null)
				return shipPsionicControl.CanChangeTarget();
			return true;
		}

		public virtual bool VictoryConditionsAreMet()
		{
			return false;
		}

		public bool SimMode
		{
			get
			{
				return this._simMode;
			}
			set
			{
				this._simMode = value;
			}
		}

		public bool InTestMode
		{
			get
			{
				return this._inTestMode;
			}
			set
			{
				this._inTestMode = value;
			}
		}

		public virtual void Update(List<IGameObject> objs)
		{
			this.PurgeDestroyed();
			if (!App.m_bAI_Enabled)
				return;
			List<TaskGroup> taskGroupList = new List<TaskGroup>();
			foreach (TaskGroup taskGroup in this.m_TaskGroups)
			{
				if (taskGroup.GetShipCount() == 0)
					taskGroupList.Add(taskGroup);
			}
			foreach (TaskGroup taskGroup in taskGroupList)
			{
				foreach (TacticalObjective objective in this.m_Objectives)
				{
					if (objective.m_TargetTaskGroup == taskGroup)
						objective.m_TargetTaskGroup = (TaskGroup)null;
				}
				this.m_TaskGroups.Remove(taskGroup);
				taskGroup.ShutDown();
			}
			this.m_FramesElapsed += this._simMode ? 20 : 1;
			--this.m_UpdateCounter;
			if (this.m_UpdateCounter > 0)
				return;
			this.m_UpdateCounter = this._simMode ? 2 : 5;
			List<Ship> shipList1 = new List<Ship>();
			List<Ship> shipList2 = new List<Ship>();
			List<StellarBody> stellarBodyList = new List<StellarBody>();
			List<StarModel> starModelList = new List<StarModel>();
			this.m_bEnemyShipsInSystem = false;
			bool flag = false;
			foreach (IGameObject gameObject in objs)
			{
				if (gameObject is Ship)
				{
					Ship ship = gameObject as Ship;
					if (!ship.IsDestroyed && !ship.HasRetreated && !ship.HitByNodeCannon)
					{
						if (ship.Player == this.m_Player)
						{
							flag = flag || ship.IsListener;
							if (!this.m_bIsHumanPlayerControlled || ship.IsNPCFreighter || ship.IsPolice && ship.IsPolicePatrolling)
							{
								shipList1.Add(ship);
								if (TaskGroup.IsValidTaskGroupShip(ship) && this.m_TaskGroups.Count > 0 && ship.TaskGroup == null)
									this.AddToBestTaskGroup(ship);
							}
							else
								continue;
						}
						else if (this.GetDiplomacyState(ship.Player.ID) == DiplomacyState.WAR || this._inTestMode && this.m_Player != ship.Player)
						{
							bool seenByDefault = ship.ShipClass == ShipClass.Station || ship.IsNPCFreighter || (ship.Deployed || ship.IsAcceleratorHoop) || (ship.IsDriveless || this.IsEncounterPlayer(ship.Player.ID)) || this.m_bHasListener;
							this.m_CloakedEnemyDetection.AddShip(ship);
							if ((double)this.m_CloakedEnemyDetection.GetVisibilityPercent(ship) > 0.0)
								this.m_SpottedEnemies.AddShip(ship, seenByDefault);
							else
								this.m_SpottedEnemies.RemoveShip(ship);
							if (this.IsShipDetected(ship) || seenByDefault)
							{
								shipList2.Add(ship);
								this.m_SpottedEnemies.SetEnemySpotted(ship);
							}
							this.m_bEnemyShipsInSystem = true;
							this.m_IsEncounterCombat = this.m_IsEncounterCombat || this.IsEncounterPlayer(ship.Player.ID);
						}
						if (this.m_UseCTAMainMenu && TaskGroup.IsValidTaskGroupShip(ship) && (ship.CombatStance != CombatStance.RETREAT && ship.CombatStance != CombatStance.CLOSE_TO_ATTACK))
							ship.SetCombatStance(CombatStance.CLOSE_TO_ATTACK);
					}
				}
				if (!this.m_bPlanetsInitialized)
				{
					if (gameObject is StellarBody)
					{
						StellarBody stellarBody = gameObject as StellarBody;
						stellarBodyList.Add(stellarBody);
					}
					if (gameObject is StarModel)
					{
						StarModel starModel = gameObject as StarModel;
						starModelList.Add(starModel);
					}
				}
			}
			this.m_Friendly = shipList1;
			this.m_Enemy = shipList2;
			if (!this.m_bPlanetsInitialized)
			{
				this.m_Planets = stellarBodyList;
				this.m_Stars = starModelList;
				this.m_bPlanetsInitialized = true;
			}
			if (this.m_Friendly.Count > 0 || flag || this.m_bHasListener)
			{
				this.m_CloakedEnemyDetection.UpdateCloakedDetection(this.m_FramesElapsed);
				if (this.m_EnemyGroups.Count<EnemyGroup>() < 1)
					this.IdentifyEnemyGroups(this.m_Enemy);
				this.UpdateEnemyGroups();
				if ((flag || this.m_bHasListener) && this.m_Player == this.m_Game.LocalPlayer)
					this.SyncListenerTargets();
				this.m_bHasListener = flag;
			}
			if (this.m_Friendly.Count == 0)
			{
				this.m_FramesElapsed = 0;
			}
			else
			{
				foreach (Ship ship in this.m_Friendly)
					this.TryAddSpecialShipControl(ship);
				if (this.m_TaskGroups.Count<TaskGroup>() < 1)
					this.SetInitialTaskGroups(this.m_Friendly);
				this.UpdateObjectives();
				this.UpdateMergeTaskGroups();
				foreach (TaskGroup taskGroup in this.m_TaskGroups)
					this.UpdateTaskGroup(taskGroup);
				this.UpdateSpecialWeaponUpdates(this.m_FramesElapsed);
				this.UpdateShipPsionics(this.m_FramesElapsed);
				this.UpdateNodeMaws();
				this.m_FramesElapsed = 0;
			}
		}

		private void SyncListenerTargets()
		{
			List<object> objectList = new List<object>();
			int count1 = this.m_EnemyGroups.Count;
			objectList.Add((object)InteropMessageID.IMID_ENGINE_COMBAT_SYNC_LISTENERS);
			if (this.m_bHasListener)
			{
				objectList.Add((object)count1);
				foreach (EnemyGroup enemyGroup in this.m_EnemyGroups)
				{
					int count2 = enemyGroup.m_Ships.Count;
					objectList.Add((object)count2);
					foreach (Ship ship in enemyGroup.m_Ships)
						objectList.Add((object)ship.ObjectID);
				}
			}
			else
				objectList.Add((object)0);
			this.m_Game.PostEngineMessage(objectList.ToArray());
		}

		public void UpdateDiploStates(int playerID, DiplomacyState diploState)
		{
			if (this._currentDiploStates == null || !this._currentDiploStates.ContainsKey(playerID))
				return;
			this._currentDiploStates[playerID] = diploState;
		}

		protected virtual void PurgeDestroyed()
		{
			if (this.m_Enemy == null || this.m_Friendly == null)
				return;
			List<Ship> shipList = new List<Ship>();
			foreach (Ship ship in this.m_Enemy)
			{
				if (!Ship.IsActiveShip(ship) || ship.Player == this.m_Player)
					shipList.Add(ship);
			}
			foreach (Ship ship in shipList)
			{
				this.m_Enemy.Remove(ship);
				this.m_SpottedEnemies.RemoveShip(ship);
			}
			shipList.Clear();
			foreach (Ship ship in this.m_Friendly)
			{
				if (!Ship.IsActiveShip(ship) || ship.Player != this.m_Player)
					shipList.Add(ship);
			}
			foreach (Ship ship in shipList)
			{
				if (ship.TaskGroup != null)
					ship.TaskGroup.RemoveShip(ship);
				this.m_Friendly.Remove(ship);
			}
			foreach (EnemyGroup enemyGroup in this.m_EnemyGroups)
			{
				shipList.Clear();
				foreach (Ship ship in enemyGroup.m_Ships)
				{
					if (ship.Player == this.m_Player || !this.IsShipDetected(ship))
						shipList.Add(ship);
				}
				foreach (Ship ship in shipList)
					enemyGroup.m_Ships.Remove(ship);
				enemyGroup.PurgeDestroyed();
			}
		}

		protected void UpdateSpecialWeaponUpdates(int elapsedFrames)
		{
			List<SpecWeaponControl> specWeaponControlList = new List<SpecWeaponControl>();
			foreach (SpecWeaponControl shipWeaponControl in this.m_ShipWeaponControls)
			{
				if (!shipWeaponControl.RemoveWeaponControl())
					shipWeaponControl.Update(elapsedFrames);
				else
					specWeaponControlList.Add(shipWeaponControl);
			}
			foreach (SpecWeaponControl specWeaponControl in specWeaponControlList)
			{
				specWeaponControl.Shutdown();
				this.m_ShipWeaponControls.Remove(specWeaponControl);
			}
		}

		protected void UpdateShipPsionics(int elapsedFrames)
		{
			List<ShipPsionicControl> shipPsionicControlList = new List<ShipPsionicControl>();
			foreach (ShipPsionicControl shipPsionicControl in this.m_ShipPsionicControls)
			{
				if (shipPsionicControl.ControlledShip != null && shipPsionicControl.ControlledShip.CurrentPsiPower > 0)
					shipPsionicControl.Update(elapsedFrames);
				else
					shipPsionicControlList.Add(shipPsionicControl);
			}
			foreach (ShipPsionicControl shipPsionicControl in shipPsionicControlList)
			{
				shipPsionicControl.Shutdown();
				this.m_ShipPsionicControls.Remove(shipPsionicControl);
			}
		}

		protected virtual void UpdateObjectives()
		{
			if (!this.m_bObjectivesInitialized)
				this.InitializeObjectives();
			List<TacticalObjective> tacticalObjectiveList = new List<TacticalObjective>();
			using (List<TacticalObjective>.Enumerator enumerator = this.m_Objectives.GetEnumerator())
			{
			label_23:
				while (enumerator.MoveNext())
				{
					TacticalObjective current = enumerator.Current;
					if (current.IsComplete())
					{
						tacticalObjectiveList.Add(current);
					}
					else
					{
						current.Update();
						if (current.m_RequestTaskGroup)
						{
							List<TaskGroup> list = this.m_TaskGroups.Where<TaskGroup>((Func<TaskGroup, bool>)(x => !(x.Objective is RetreatObjective))).ToList<TaskGroup>();
							EnemyGroup eg = current.m_TargetEnemyGroup;
							if (eg == null && current is DefendPlanetObjective)
								eg = (current as DefendPlanetObjective).GetClosestThreat();
							int num1 = current.ResourceNeeds();
							while (true)
							{
								if (list.Count > 0 && current.m_CurrentResources < num1)
								{
									TaskGroup taskGroup1 = (TaskGroup)null;
									float num2 = float.MaxValue;
									foreach (TaskGroup taskGroup2 in list)
									{
										if (!(taskGroup2.Objective is EvadeEnemyObjective) && taskGroup2.Type != TaskGroupType.Freighter && taskGroup2.Type != TaskGroupType.UnArmed)
										{
											if (taskGroup2.Objective is AttackGroupObjective && taskGroup2.Objective.m_TargetEnemyGroup != null)
											{
												CombatAI.AssessGroupStrength(taskGroup2.Objective.m_TargetEnemyGroup.m_Ships);
												if (taskGroup2.Objective.m_TargetEnemyGroup.IsHigherPriorityThan(eg, this, true))
													continue;
											}
											float lengthSquared = (taskGroup2.GetBaseGroupPosition() - current.GetObjectiveLocation()).LengthSquared;
											if ((double)lengthSquared < (double)num2)
											{
												num2 = lengthSquared;
												taskGroup1 = taskGroup2;
											}
										}
									}
									if (taskGroup1 != null)
									{
										taskGroup1.Objective = current;
										list.Remove(taskGroup1);
									}
									else
										break;
								}
								else
									goto label_23;
							}
							list.FirstOrDefault<TaskGroup>((Func<TaskGroup, bool>)(x =>
						   {
							   if (x.Type != TaskGroupType.Freighter)
								   return x.Type != TaskGroupType.UnArmed;
							   return false;
						   }));
						}
					}
				}
			}
			foreach (TacticalObjective tacticalObjective in tacticalObjectiveList)
			{
				foreach (TaskGroup taskGroup in this.m_TaskGroups)
				{
					if (taskGroup.Objective == tacticalObjective)
						taskGroup.Objective = (TacticalObjective)null;
				}
				this.m_Objectives.Remove(tacticalObjective);
			}
		}

		private void UpdateMergeTaskGroups()
		{
			if (this.m_TaskGroups.Count < 2)
				return;
			bool flag = false;
			while (!flag)
			{
				flag = true;
				List<Ship> ships = new List<Ship>();
				foreach (TaskGroup taskGroup1 in this.m_TaskGroups)
				{
					if (taskGroup1.GetShipCount() != 0 && (taskGroup1.Objective is AttackGroupObjective || taskGroup1.Objective is AttackPlanetObjective) && taskGroup1.Type != TaskGroupType.Police)
					{
						Vector3 baseGroupPosition = taskGroup1.GetBaseGroupPosition();
						foreach (TaskGroup taskGroup2 in this.m_TaskGroups)
						{
							if (taskGroup2 != taskGroup1 && taskGroup2.Objective == taskGroup1.Objective && taskGroup2.GetShipCount() != 0 && ((taskGroup1.Objective is AttackGroupObjective || taskGroup1.Objective is AttackPlanetObjective) && taskGroup2.Type != TaskGroupType.Police) && (taskGroup1.Type == taskGroup2.Type || taskGroup1.Type != TaskGroupType.PlanetAssault && taskGroup2.Type != TaskGroupType.PlanetAssault) && (double)(baseGroupPosition - taskGroup2.GetBaseGroupPosition()).LengthSquared < 9000000.0)
							{
								ships.AddRange((IEnumerable<Ship>)taskGroup2.GetShips());
								flag = false;
							}
						}
						if (!flag)
						{
							taskGroup1.AddShips(ships);
							break;
						}
					}
				}
			}
		}

		private void AddToBestTaskGroup(Ship ship)
		{
			if (this.m_TaskGroups.Count < 1 || ship.TaskGroup != null || !TaskGroup.IsValidTaskGroupShip(ship))
				return;
			TaskGroup taskGroup1 = (TaskGroup)null;
			TaskGroupType taskGroupType = TaskGroup.GetTaskTypeFromShip(ship);
			if (this.m_IsEncounterCombat && (taskGroupType == TaskGroupType.Passive || taskGroupType == TaskGroupType.Civilian || taskGroupType == TaskGroupType.PlanetAssault))
				taskGroupType = TaskGroupType.Aggressive;
			float num = float.MaxValue;
			foreach (TaskGroup taskGroup2 in this.m_TaskGroups)
			{
				if (taskGroup2.Type != TaskGroupType.Police)
				{
					switch (taskGroupType)
					{
						case TaskGroupType.Civilian:
							if (taskGroup2.Type == TaskGroupType.Civilian)
							{
								taskGroup1 = taskGroup2;
								goto label_18;
							}
							else
								continue;
						case TaskGroupType.UnArmed:
							if (taskGroup2.Type == TaskGroupType.UnArmed)
							{
								taskGroup1 = taskGroup2;
								goto label_18;
							}
							else
								continue;
						case TaskGroupType.PlanetAssault:
							if (taskGroup2.Type == TaskGroupType.PlanetAssault)
							{
								taskGroup1 = taskGroup2;
								goto label_18;
							}
							else
								continue;
						default:
							float lengthSquared = (taskGroup2.GetBaseGroupPosition() - ship.Position).LengthSquared;
							if ((double)lengthSquared < (double)num && (double)lengthSquared < (double)TaskGroup.ATTACK_GROUP_RANGE * (double)TaskGroup.ATTACK_GROUP_RANGE)
							{
								num = lengthSquared;
								taskGroup1 = taskGroup2;
								continue;
							}
							continue;
					}
				}
			}
		label_18:
			if (taskGroup1 != null)
			{
				taskGroup1.AddShip(ship);
			}
			else
			{
				TaskGroup taskGroup2 = new TaskGroup(this.m_Game, this);
				taskGroup2.AddShips(new List<Ship>() { ship });
				taskGroup2.UpdateTaskGroupType();
				this.m_TaskGroups.Add(taskGroup2);
			}
		}

		private void UpdateTaskGroup(TaskGroup group)
		{
			group.UpdateEnemyContact();
			this.UpdateTaskGroupObjective(group);
			group.Update(this.m_FramesElapsed);
		}

		private void UpdateTaskGroupObjective(TaskGroup group)
		{
			if (group.GetShipCount() == 0)
				return;
			ObjectiveType objectiveType = group.GetRequestedObjectiveType();
			if (this.m_AIType == OverallAIType.SLAVER && (group.Objective == null || !(group.Objective is RetreatObjective) && !(group.Objective is AttackPlanetObjective)))
				objectiveType = ObjectiveType.ATTACK_TARGET;
			if (group.Objective != null && (objectiveType == ObjectiveType.NO_OBJECTIVE || objectiveType == group.Objective.m_ObjectiveType))
				return;
			TacticalObjective tacticalObjective1 = (TacticalObjective)null;
			if (group.Objective != null)
			{
				switch (objectiveType)
				{
					case ObjectiveType.PATROL:
						tacticalObjective1 = this.GetPatrolObjective(group);
						break;
					case ObjectiveType.SCOUT:
						tacticalObjective1 = this.GetScoutObjective(group);
						break;
					case ObjectiveType.DEFEND_TARGET:
						tacticalObjective1 = this.GetDefendObjective(group);
						break;
					case ObjectiveType.ATTACK_TARGET:
						if (this.m_AIType == OverallAIType.PIRATE)
							tacticalObjective1 = this.GetBoardTargetObjective(group);
						if (tacticalObjective1 == null)
						{
							tacticalObjective1 = this.GetAttackObjective(group);
							break;
						}
						break;
					case ObjectiveType.EVADE_ENEMY:
						tacticalObjective1 = this.GetEvadeObjective(group);
						break;
					case ObjectiveType.RETREAT:
						tacticalObjective1 = this.GetRetreatObjective(group);
						break;
				}
			}
			if (tacticalObjective1 == null && this.m_AIType == OverallAIType.PIRATE)
			{
				if (group.Type == TaskGroupType.BoardingGroup)
				{
					tacticalObjective1 = this.GetBoardTargetObjective(group);
					if (tacticalObjective1 != null)
					{
						bool flag = false;
						foreach (TacticalObjective tacticalObjective2 in this.m_Objectives.Where<TacticalObjective>((Func<TacticalObjective, bool>)(x => x is FollowTaskGroupObjective)))
						{
							if (tacticalObjective2.m_TargetTaskGroup == group)
							{
								flag = true;
								break;
							}
						}
						if (!flag)
							this.m_Objectives.Add((TacticalObjective)new FollowTaskGroupObjective(group));
					}
					else
						tacticalObjective1 = this.GetAttackFreighterObjective(group);
					if (tacticalObjective1 == null)
						tacticalObjective1 = this.GetAttackObjective(group);
					if (tacticalObjective1 == null)
						tacticalObjective1 = this.GetDefendObjective(group);
					if (tacticalObjective1 == null)
						tacticalObjective1 = this.GetRetreatObjective(group);
				}
				else if (group.Type == TaskGroupType.FollowGroup)
					tacticalObjective1 = (((this.GetFollowBoardingGroupObjective(group) ?? this.GetAttackObjective(group)) ?? this.GetAttackFreighterObjective(group)) ?? this.GetDefendObjective(group)) ?? this.GetRetreatObjective(group);
			}
			if (tacticalObjective1 == null)
			{
				switch (group.Type)
				{
					case TaskGroupType.Aggressive:
					case TaskGroupType.Passive:
						if (this.m_AIType == OverallAIType.PIRATE)
							tacticalObjective1 = this.GetAttackFreighterObjective(group);
						if (tacticalObjective1 == null)
							tacticalObjective1 = this.GetAttackObjective(group);
						if (tacticalObjective1 == null)
							tacticalObjective1 = this.GetScoutObjective(group);
						if (tacticalObjective1 == null)
							tacticalObjective1 = this.GetDefendObjective(group);
						if (tacticalObjective1 == null)
						{
							tacticalObjective1 = this.GetPatrolObjective(group);
							break;
						}
						break;
					case TaskGroupType.Civilian:
						tacticalObjective1 = this.m_TaskGroups.Where<TaskGroup>((Func<TaskGroup, bool>)(x =>
					   {
						   if (x.Type != TaskGroupType.Aggressive)
							   return x.Type == TaskGroupType.Passive;
						   return true;
					   })).Count<TaskGroup>() != 0 ? (this.GetEvadeObjective(group) ?? this.GetDefendObjective(group)) ?? this.GetPatrolObjective(group) : ((this.GetAttackObjective(group) ?? this.GetScoutObjective(group)) ?? this.GetDefendObjective(group)) ?? this.GetPatrolObjective(group);
						break;
					case TaskGroupType.Police:
						tacticalObjective1 = this.GetAttackObjective(group) ?? this.GetPatrolObjective(group);
						break;
					case TaskGroupType.Freighter:
						TacticalObjective patrolObjective = this.GetPatrolObjective(group);
						TacticalObjective retreatObjective = this.GetRetreatObjective(group);
						Vector3 baseGroupPosition = group.GetBaseGroupPosition();
						tacticalObjective1 = patrolObjective == null || (double)(retreatObjective.GetObjectiveLocation() - baseGroupPosition).LengthSquared < (double)(patrolObjective.GetObjectiveLocation() - baseGroupPosition).LengthSquared ? retreatObjective : patrolObjective;
						break;
					case TaskGroupType.UnArmed:
						tacticalObjective1 = this.m_TaskGroups.Where<TaskGroup>((Func<TaskGroup, bool>)(x =>
					   {
						   if (x.Type != TaskGroupType.Aggressive && x.Type != TaskGroupType.Passive)
							   return x.Type == TaskGroupType.Civilian;
						   return true;
					   })).Count<TaskGroup>() != 0 ? this.GetEvadeObjective(group) : this.GetRetreatObjective(group);
						break;
					case TaskGroupType.PlanetAssault:
						if (tacticalObjective1 == null)
							tacticalObjective1 = this.GetAttackObjective(group);
						if (tacticalObjective1 == null)
							tacticalObjective1 = this.GetDefendObjective(group);
						if (tacticalObjective1 == null)
						{
							tacticalObjective1 = this.GetPatrolObjective(group);
							break;
						}
						break;
				}
			}
			group.Objective = tacticalObjective1;
		}

		protected TacticalObjective GetPatrolObjective(TaskGroup tg)
		{
			TacticalObjective tacticalObjective1 = (TacticalObjective)null;
			Vector3 baseGroupPosition = tg.GetBaseGroupPosition();
			int num1 = 20;
			float num2 = float.MaxValue;
			foreach (TacticalObjective tacticalObjective2 in this.m_Objectives.Where<TacticalObjective>((Func<TacticalObjective, bool>)(x => x is PatrolObjective)))
			{
				if (tg.Type == TaskGroupType.Police)
				{
					foreach (Ship ship in tg.GetShips())
					{
						if (tacticalObjective2.m_PoliceOwner == ship)
						{
							tacticalObjective1 = tacticalObjective2;
							break;
						}
					}
					if (tacticalObjective1 != null)
						break;
				}
				else if (tg.Type == TaskGroupType.Freighter)
				{
					if (tacticalObjective2.m_Planet != null)
					{
						float lengthSquared = (baseGroupPosition - tacticalObjective2.GetObjectiveLocation()).LengthSquared;
						if ((double)lengthSquared < (double)num2)
						{
							tacticalObjective1 = tacticalObjective2;
							num2 = lengthSquared;
						}
					}
				}
				else
				{
					float lengthSquared = (baseGroupPosition - tacticalObjective2.GetObjectiveLocation()).LengthSquared;
					if (tacticalObjective2.m_TaskGroups.Count < num1 || (double)lengthSquared < (double)num2)
					{
						tacticalObjective1 = tacticalObjective2;
						num2 = lengthSquared;
						num1 = tacticalObjective2.m_TaskGroups.Count;
					}
				}
			}
			return tacticalObjective1;
		}

		protected TacticalObjective GetScoutObjective(TaskGroup tg)
		{
			TacticalObjective tacticalObjective1 = (TacticalObjective)null;
			Vector3 baseGroupPosition = tg.GetBaseGroupPosition();
			int num1 = 20;
			float num2 = float.MaxValue;
			foreach (TacticalObjective tacticalObjective2 in this.m_Objectives.Where<TacticalObjective>((Func<TacticalObjective, bool>)(x => x is ScoutObjective)))
			{
				float lengthSquared = (baseGroupPosition - tacticalObjective2.GetObjectiveLocation()).LengthSquared;
				if (tacticalObjective2.m_TaskGroups.Count < num1 || (double)lengthSquared < (double)num2)
				{
					tacticalObjective1 = tacticalObjective2;
					num2 = lengthSquared;
					num1 = tacticalObjective2.m_TaskGroups.Count;
				}
			}
			return tacticalObjective1;
		}

		protected TacticalObjective GetAttackObjective(TaskGroup tg)
		{
			TacticalObjective tacticalObjective;
			if (tg.Type == TaskGroupType.PlanetAssault)
			{
				tacticalObjective = this.GetAttackPlanetObjective(tg);
				TacticalObjective enemyGroupObjective = this.GetAttackEnemyGroupObjective(tg);
				if (tacticalObjective == null || enemyGroupObjective != null && (double)(tacticalObjective.GetObjectiveLocation() - tg.GetBaseGroupPosition()).Length > 100000.0)
					tacticalObjective = enemyGroupObjective;
			}
			else
			{
				TacticalObjective enemyGroupObjective = this.GetAttackEnemyGroupObjective(tg);
				TacticalObjective attackPlanetObjective = this.GetAttackPlanetObjective(tg);
				float num = 0.0f;
				tacticalObjective = enemyGroupObjective == null || this.m_AIType == OverallAIType.SLAVER || (double)num > (double)tg.GroupSpeed + 5.0 ? attackPlanetObjective : enemyGroupObjective;
			}
			return tacticalObjective;
		}

		protected TacticalObjective GetAttackEnemyGroupObjective(TaskGroup tg)
		{
			TacticalObjective tacticalObjective1 = (TacticalObjective)null;
			if (tg.IsInContactWithEnemy)
			{
				foreach (TacticalObjective tacticalObjective2 in this.m_Objectives.Where<TacticalObjective>((Func<TacticalObjective, bool>)(x => x is AttackGroupObjective)))
				{
					if (tacticalObjective2.m_TargetEnemyGroup == tg.EnemyGroupInContact)
					{
						tacticalObjective1 = tacticalObjective2;
						break;
					}
				}
			}
			else if (this.m_IsEncounterCombat)
			{
				float num = float.MaxValue;
				Vector3 baseGroupPosition = tg.GetBaseGroupPosition();
				foreach (TacticalObjective tacticalObjective2 in this.m_Objectives.Where<TacticalObjective>((Func<TacticalObjective, bool>)(x =>
			   {
				   if (x is AttackGroupObjective && x.m_TargetEnemyGroup != null)
					   return x.m_TargetEnemyGroup.IsEncounterEnemyGroup(this);
				   return false;
			   })).ToList<TacticalObjective>())
				{
					float lengthSquared = (baseGroupPosition - tacticalObjective2.GetObjectiveLocation()).LengthSquared;
					if ((double)lengthSquared < (double)num)
					{
						tacticalObjective1 = tacticalObjective2;
						num = lengthSquared;
					}
				}
			}
			return tacticalObjective1;
		}

		protected TacticalObjective GetAttackPlanetObjective(TaskGroup tg)
		{
			TacticalObjective tacticalObjective1 = (TacticalObjective)null;
			Vector3 baseGroupPosition = tg.GetBaseGroupPosition();
			float num = float.MaxValue;
			foreach (TacticalObjective tacticalObjective2 in this.m_Objectives.Where<TacticalObjective>((Func<TacticalObjective, bool>)(x => x is AttackPlanetObjective)))
			{
				float lengthSquared = (baseGroupPosition - tacticalObjective2.GetObjectiveLocation()).LengthSquared;
				if ((double)lengthSquared < (double)num)
				{
					tacticalObjective1 = tacticalObjective2;
					num = lengthSquared;
				}
			}
			return tacticalObjective1;
		}

		protected TacticalObjective GetAttackFreighterObjective(TaskGroup tg)
		{
			TacticalObjective tacticalObjective1 = (TacticalObjective)null;
			Vector3 baseGroupPosition = tg.GetBaseGroupPosition();
			float num = float.MaxValue;
			foreach (TacticalObjective tacticalObjective2 in this.m_Objectives.Where<TacticalObjective>((Func<TacticalObjective, bool>)(x => x is AttackGroupObjective)))
			{
				if (tacticalObjective2.m_TargetEnemyGroup != null && tacticalObjective2.m_TargetEnemyGroup.IsFreighterEnemyGroup())
				{
					float lengthSquared = (baseGroupPosition - tacticalObjective2.GetObjectiveLocation()).LengthSquared;
					if ((double)lengthSquared < (double)num)
					{
						tacticalObjective1 = tacticalObjective2;
						num = lengthSquared;
					}
				}
			}
			return tacticalObjective1;
		}

		protected TacticalObjective GetBoardTargetObjective(TaskGroup tg)
		{
			TacticalObjective tacticalObjective1 = (TacticalObjective)null;
			Vector3 baseGroupPosition = tg.GetBaseGroupPosition();
			float num = float.MaxValue;
			foreach (TacticalObjective tacticalObjective2 in this.m_Objectives.Where<TacticalObjective>((Func<TacticalObjective, bool>)(x => x is BoardTargetObjective)))
			{
				if (tacticalObjective2.m_TargetEnemyGroup != null && tacticalObjective2.m_TargetEnemyGroup.IsFreighterEnemyGroup())
				{
					float lengthSquared = (baseGroupPosition - tacticalObjective2.GetObjectiveLocation()).LengthSquared;
					if ((double)lengthSquared < (double)num)
					{
						tacticalObjective1 = tacticalObjective2;
						num = lengthSquared;
					}
				}
			}
			return tacticalObjective1;
		}

		protected TacticalObjective GetFollowBoardingGroupObjective(TaskGroup tg)
		{
			TacticalObjective tacticalObjective1 = (TacticalObjective)null;
			Vector3 baseGroupPosition = tg.GetBaseGroupPosition();
			float num = float.MaxValue;
			foreach (TacticalObjective tacticalObjective2 in this.m_Objectives.Where<TacticalObjective>((Func<TacticalObjective, bool>)(x => x is FollowTaskGroupObjective)))
			{
				float lengthSquared = (baseGroupPosition - tacticalObjective2.GetObjectiveLocation()).LengthSquared;
				if ((double)lengthSquared < (double)num)
				{
					tacticalObjective1 = tacticalObjective2;
					num = lengthSquared;
				}
			}
			return tacticalObjective1;
		}

		protected TacticalObjective GetDefendObjective(TaskGroup tg)
		{
			TacticalObjective tacticalObjective1 = (TacticalObjective)null;
			Vector3 baseGroupPosition = tg.GetBaseGroupPosition();
			float num = float.MaxValue;
			foreach (TacticalObjective tacticalObjective2 in this.m_Objectives.Where<TacticalObjective>((Func<TacticalObjective, bool>)(x => x is DefendPlanetObjective)))
			{
				float lengthSquared = (baseGroupPosition - tacticalObjective2.GetObjectiveLocation()).LengthSquared;
				if ((double)lengthSquared < (double)num)
				{
					tacticalObjective1 = tacticalObjective2;
					num = lengthSquared;
				}
			}
			return tacticalObjective1;
		}

		protected TacticalObjective GetEvadeObjective(TaskGroup tg)
		{
			TacticalObjective tacticalObjective1 = (TacticalObjective)null;
			Vector3 baseGroupPosition = tg.GetBaseGroupPosition();
			float num = float.MaxValue;
			foreach (TacticalObjective tacticalObjective2 in this.m_Objectives.Where<TacticalObjective>((Func<TacticalObjective, bool>)(x => x is EvadeEnemyObjective)))
			{
				if (!(tacticalObjective2 as EvadeEnemyObjective).IsUnsafe)
				{
					float lengthSquared = (baseGroupPosition - tacticalObjective2.GetObjectiveLocation()).LengthSquared;
					if ((double)lengthSquared < (double)num)
					{
						tacticalObjective1 = tacticalObjective2;
						num = lengthSquared;
					}
				}
			}
			return tacticalObjective1;
		}

		protected TacticalObjective GetRetreatObjective(TaskGroup tg)
		{
			TacticalObjective tacticalObjective1 = (TacticalObjective)null;
			Vector3 baseGroupPosition = tg.GetBaseGroupPosition();
			float num = float.MaxValue;
			foreach (TacticalObjective tacticalObjective2 in this.m_Objectives.Where<TacticalObjective>((Func<TacticalObjective, bool>)(x => x is RetreatObjective)))
			{
				float lengthSquared = (baseGroupPosition - tacticalObjective2.GetObjectiveLocation()).LengthSquared;
				if ((double)lengthSquared < (double)num)
				{
					tacticalObjective1 = tacticalObjective2;
					num = lengthSquared;
				}
			}
			return tacticalObjective1;
		}

		public static int AssessGroupStrength(List<Ship> ships)
		{
			int num = 0;
			foreach (Ship ship in ships)
				num += CombatAI.GetShipStrength(ship);
			return num;
		}

		public int GetTargetShipScore(Ship ship)
		{
			return CombatAI.GetShipStrength(ship) * 3 + this.GetTargetShipBonusScore(ship);
		}

		public Ship GetBestShipTarget(Vector3 pos, List<Ship> ships)
		{
			if (ships.Count == 0)
				return (Ship)null;
			ShipTargetComparision targetComparision = new ShipTargetComparision(this, pos);
			ships.Sort((IComparer<Ship>)targetComparision);
			return ships.First<Ship>();
		}

		public static int GetShipStrength(Ship ship)
		{
			int num = 0;
			if (!TaskGroup.IsValidTaskGroupShip(ship))
				return num;
			return num + CombatAI.GetShipStrength(ship.ShipClass);
		}

		public static int GetShipStrength(ShipClass shipClass)
		{
			int num1 = 0;
			int num2;
			switch (shipClass)
			{
				case ShipClass.Cruiser:
					num2 = num1 + 3;
					break;
				case ShipClass.Dreadnought:
					num2 = num1 + 9;
					break;
				case ShipClass.Leviathan:
					num2 = num1 + 42;
					break;
				default:
					num2 = num1 + 1;
					break;
			}
			return num2;
		}

		private int GetTargetShipBonusScore(Ship ship)
		{
			int num = 0;
			if (!TaskGroup.IsValidTaskGroupShip(ship))
				return num;
			TaskGroupType taskTypeFromShip = TaskGroup.GetTaskTypeFromShip(ship);
			if (this.m_AIType == OverallAIType.PIRATE)
			{
				if (taskTypeFromShip == TaskGroupType.Freighter)
					return 50;
				if (taskTypeFromShip == TaskGroupType.Police)
					return 25;
			}
			if (this.m_OwnsSystem && taskTypeFromShip == TaskGroupType.PlanetAssault)
				num += 6;
			return num;
		}

		private void SetInitialTaskGroups(List<Ship> ships)
		{
			List<Ship> ships1 = new List<Ship>();
			foreach (Ship ship in ships)
			{
				if (ship.TaskGroup == null && TaskGroup.IsValidTaskGroupShip(ship))
					ships1.Add(ship);
			}
			switch (this.m_AIType)
			{
				case OverallAIType.SLAVER:
					this.CreateSlaverTaskGroup(ships1);
					break;
				case OverallAIType.PIRATE:
					this.CreatePirateTaskGroup(ships1);
					break;
				default:
					this.CreateNormalTaskGroups(ships1);
					break;
			}
		}

		private void CreateNormalTaskGroups(List<Ship> ships)
		{
			if (ships.Count == 0)
				return;
			ShipInfo shipInfo = this.m_Game.GameDatabase.GetShipInfo(ships.First<Ship>().DatabaseID, false);
			int fleetID = shipInfo != null ? shipInfo.FleetID : 0;
			List<Ship> aggressive = new List<Ship>();
			List<Ship> passive = new List<Ship>();
			List<Ship> civilian = new List<Ship>();
			List<Ship> ships1 = new List<Ship>();
			List<Ship> police = new List<Ship>();
			List<Ship> freighters = new List<Ship>();
			List<Ship> shipList = new List<Ship>();
			foreach (Ship ship in ships)
			{
				if (ship.IsPolice)
				{
					police.Add(ship);
				}
				else
				{
					switch (TaskGroup.GetTaskTypeFromShip(ship))
					{
						case TaskGroupType.Passive:
							passive.Add(ship);
							continue;
						case TaskGroupType.Civilian:
							civilian.Add(ship);
							continue;
						case TaskGroupType.Freighter:
							freighters.Add(ship);
							continue;
						case TaskGroupType.UnArmed:
							ships1.Add(ship);
							continue;
						case TaskGroupType.PlanetAssault:
							shipList.Add(ship);
							continue;
						default:
							aggressive.Add(ship);
							continue;
					}
				}
			}
			MissionInfo missionByFleetId = this.m_Game.GameDatabase.GetMissionByFleetID(fleetID);
			if (this.m_IsEncounterCombat)
				this.AssignShipsToTaskGroupsForEncounterAttack(aggressive, passive, civilian, shipList);
			else if (missionByFleetId != null && (missionByFleetId.Type == MissionType.INVASION || missionByFleetId.Type == MissionType.STRIKE) || this.m_Planets.Count + this.m_Stars.Count == 0)
			{
				this.AssignShipsToTaskGroupsForAttack(aggressive, passive, civilian);
			}
			else
			{
				bool flag1 = false;
				bool flag2 = false;
				foreach (ColonyInfo colonyInfo in this.m_Game.GameDatabase.GetColonyInfosForSystem(0))
				{
					if (colonyInfo.PlayerID == this.m_Player.ID)
						flag1 = true;
					else if (this.GetDiplomacyState(colonyInfo.PlayerID) == DiplomacyState.WAR)
						flag2 = true;
				}
				if (flag1 && flag2)
					this.AssignShipsToTaskGroupsForMultiMission(aggressive, passive, civilian);
				else if (flag1)
					this.AssignShipsToTaskGroupsForDefending(aggressive, passive, civilian);
				else
					this.AssignShipsToTaskGroupsForAttack(aggressive, passive, civilian);
			}
			if (!this.m_IsEncounterCombat)
				this.CreatePlanetAssaultTaskGroup(shipList);
			this.CreateUnarmedTaskGroup(ships1);
			this.CreateFreighterTaskGroups(freighters);
			this.AssignShipsToPoliceTaskGroups(police);
		}

		private void AddNearbyShipsToTaskGroup(TaskGroup group, Ship ship, List<Ship> ungroupedList)
		{
			if (group.Type == TaskGroupType.Police)
				return;
			bool flag = true;
			while (flag)
			{
				flag = false;
				foreach (Ship ungrouped in ungroupedList)
				{
					if ((double)(ship.Maneuvering.Position - ungrouped.Maneuvering.Position).Length < 2000.0)
					{
						group.AddShip(ungrouped);
						ungroupedList.Remove(ungrouped);
						flag = true;
						break;
					}
				}
			}
		}

		private void AssignShipsToTaskGroupsForEncounterAttack(
		  List<Ship> aggressive,
		  List<Ship> passive,
		  List<Ship> civilian,
		  List<Ship> planetAssault)
		{
			if (aggressive.Count + passive.Count + civilian.Count <= 0)
				return;
			TaskGroup taskGroup = new TaskGroup(this.m_Game, this);
			taskGroup.AddShips(aggressive);
			taskGroup.AddShips(passive);
			taskGroup.AddShips(civilian);
			taskGroup.AddShips(planetAssault);
			taskGroup.UpdateTaskGroupType();
			this.m_TaskGroups.Add(taskGroup);
		}

		private void AssignShipsToTaskGroupsForAttack(
		  List<Ship> aggressive,
		  List<Ship> passive,
		  List<Ship> civilian)
		{
			bool flag = false;
			List<Ship> shipList1 = new List<Ship>();
			shipList1.AddRange((IEnumerable<Ship>)aggressive);
			if (shipList1.Count < TaskGroup.DESIRED_MIN_AGGRESSORS_PER_GROUP + TaskGroup.ABS_MIN_AGGRESSORS_PER_GROUP)
			{
				shipList1.AddRange((IEnumerable<Ship>)passive);
				flag = true;
			}
			if (shipList1.Count >= TaskGroup.DESIRED_MIN_AGGRESSORS_PER_GROUP + TaskGroup.ABS_MIN_AGGRESSORS_PER_GROUP)
			{
				int count = shipList1.Count;
				int num1 = count / TaskGroup.DESIRED_MIN_AGGRESSORS_PER_GROUP + 1;
				if (count % TaskGroup.DESIRED_MIN_AGGRESSORS_PER_GROUP < TaskGroup.ABS_MIN_AGGRESSORS_PER_GROUP)
					--num1;
				int num2 = count / num1;
				for (int index1 = 0; index1 < num1; ++index1)
				{
					Ship ship1 = shipList1[0];
					List<Ship> ships = new List<Ship>();
					ships.Add(ship1);
					aggressive.Remove(ship1);
					for (int index2 = 0; index2 < num2; ++index2)
					{
						float num3 = float.MaxValue;
						Ship ship2 = (Ship)null;
						foreach (Ship ship3 in shipList1)
						{
							float lengthSquared = (ship1.Position - ship3.Position).LengthSquared;
							if ((double)lengthSquared < (double)num3)
							{
								ship2 = ship3;
								num3 = lengthSquared;
							}
						}
						if (ship2 != null)
						{
							ships.Add(ship2);
							shipList1.Remove(ship2);
						}
						else
							break;
					}
					TaskGroup taskGroup = new TaskGroup(this.m_Game, this);
					taskGroup.AddShips(ships);
					this.m_TaskGroups.Add(taskGroup);
				}
			}
			if (this.m_TaskGroups.Count == 0)
				this.m_TaskGroups.Add(new TaskGroup(this.m_Game, this));
			while (shipList1.Count > 0)
			{
				this.m_TaskGroups[this.m_TaskGroups.Count - 1].AddShip(shipList1[0]);
				shipList1.RemoveAt(0);
			}
			int index = 0;
			int count1 = this.m_TaskGroups.Count;
			List<Ship> shipList2 = new List<Ship>();
			if (!flag)
				shipList2.AddRange((IEnumerable<Ship>)passive);
			foreach (Ship ship in shipList2)
			{
				this.m_TaskGroups[index].AddShip(ship);
				index = (index + 1) % count1;
			}
			if (civilian.Count > 0)
			{
				if (aggressive.Count + passive.Count > 0)
				{
					TaskGroup taskGroup = new TaskGroup(this.m_Game, this);
					taskGroup.AddShips(civilian);
					this.m_TaskGroups.Add(taskGroup);
				}
				else
					this.m_TaskGroups[0].AddShips(civilian);
			}
			foreach (TaskGroup taskGroup in this.m_TaskGroups)
			{
				taskGroup.UpdateTaskGroupType();
				taskGroup.m_Orders = taskGroup.Type != TaskGroupType.Aggressive ? TaskGroupOrders.Patrol : TaskGroupOrders.Scout;
			}
		}

		private void AssignShipsToTaskGroupsForDefending(
		  List<Ship> aggressive,
		  List<Ship> passive,
		  List<Ship> civilian)
		{
			List<Ship> ships1 = new List<Ship>();
			ships1.AddRange((IEnumerable<Ship>)aggressive);
			if (ships1.Count / TaskGroup.NUM_SHIPS_PER_SCOUT < 1)
				ships1.AddRange((IEnumerable<Ship>)passive);
			int num1 = ships1.Count / TaskGroup.NUM_SHIPS_PER_SCOUT;
			if (num1 < 1)
			{
				if (ships1.Count > 0)
				{
					TaskGroup taskGroup1 = new TaskGroup(this.m_Game, this);
					taskGroup1.AddShips(ships1);
					this.m_TaskGroups.Add(taskGroup1);
					TaskGroup taskGroup2 = new TaskGroup(this.m_Game, this);
					taskGroup2.AddShips(civilian);
					this.m_TaskGroups.Add(taskGroup2);
				}
				else if (civilian.Count > 0)
				{
					TaskGroup taskGroup = new TaskGroup(this.m_Game, this);
					taskGroup.AddShips(civilian);
					this.m_TaskGroups.Add(taskGroup);
				}
			}
			else
			{
				for (int index = 0; index < num1; ++index)
				{
					Ship bestScout = CombatAI.FindBestScout(ships1);
					if (bestScout != null)
					{
						TaskGroup taskGroup = new TaskGroup(this.m_Game, this);
						taskGroup.AddShip(bestScout);
						taskGroup.UpdateTaskGroupType();
						taskGroup.m_Orders = TaskGroupOrders.Scout;
						this.m_TaskGroups.Add(taskGroup);
						ships1.Remove(bestScout);
					}
					else
						break;
				}
				if (ships1.Count >= TaskGroup.DESIRED_MIN_AGGRESSORS_PER_GROUP + TaskGroup.ABS_MIN_AGGRESSORS_PER_GROUP)
				{
					int count = ships1.Count;
					int num2 = count / TaskGroup.DESIRED_MIN_AGGRESSORS_PER_GROUP + 1;
					if (count % TaskGroup.DESIRED_MIN_AGGRESSORS_PER_GROUP < TaskGroup.ABS_MIN_AGGRESSORS_PER_GROUP)
						--num2;
					int num3 = count / num2;
					for (int index1 = 0; index1 < num2; ++index1)
					{
						Ship ship1 = ships1[0];
						List<Ship> ships2 = new List<Ship>();
						ships2.Add(ship1);
						aggressive.Remove(ship1);
						for (int index2 = 0; index2 < num3; ++index2)
						{
							float num4 = float.MaxValue;
							Ship ship2 = (Ship)null;
							foreach (Ship ship3 in ships1)
							{
								float lengthSquared = (ship1.Position - ship3.Position).LengthSquared;
								if ((double)lengthSquared < (double)num4)
								{
									ship2 = ship3;
									num4 = lengthSquared;
								}
							}
							if (ship2 != null)
							{
								ships2.Add(ship2);
								ships1.Remove(ship2);
							}
							else
								break;
						}
						TaskGroup taskGroup = new TaskGroup(this.m_Game, this);
						taskGroup.AddShips(ships2);
						this.m_TaskGroups.Add(taskGroup);
					}
					while (ships1.Count > 0)
					{
						this.m_TaskGroups[this.m_TaskGroups.Count - 1].AddShip(ships1[0]);
						ships1.RemoveAt(0);
					}
					if (civilian.Count > 0)
					{
						TaskGroup taskGroup = new TaskGroup(this.m_Game, this);
						taskGroup.AddShips(civilian);
						this.m_TaskGroups.Add(taskGroup);
					}
				}
			}
			foreach (TaskGroup taskGroup in this.m_TaskGroups)
				taskGroup.UpdateTaskGroupType();
		}

		public static Ship FindBestScout(List<Ship> ships)
		{
			Ship ship1 = ships.FirstOrDefault<Ship>((Func<Ship, bool>)(x =>
		   {
			   if (x.ShipRole == ShipRole.SCOUT)
				   return !Ship.IsBattleRiderSize(x.RealShipClass);
			   return false;
		   }));
			if (ship1 == null)
			{
				List<Ship> list = ships.Where<Ship>((Func<Ship, bool>)(x => x.ShipRole == ShipRole.COMMAND)).ToList<Ship>();
				float num = 0.0f;
				foreach (Ship ship2 in ships)
				{
					if (!list.Contains(ship2) && !ship2.IsDriveless && (double)ship2.Maneuvering.MaxShipSpeed > (double)num)
						ship1 = ship2;
				}
				if (ship1 == null)
				{
					foreach (Ship ship2 in list)
					{
						if (!ship2.IsDriveless && (double)ship2.Maneuvering.MaxShipSpeed > (double)num)
							ship1 = ship2;
					}
				}
			}
			if (ship1 == null)
				ship1 = ships.FirstOrDefault<Ship>();
			return ship1;
		}

		private void AssignShipsToTaskGroupsForMultiMission(
		  List<Ship> aggressive,
		  List<Ship> passive,
		  List<Ship> civilian)
		{
		}

		private void AssignShipsToPoliceTaskGroups(List<Ship> police)
		{
			foreach (Ship ship in police)
			{
				bool flag = false;
				foreach (TacticalObjective tacticalObjective in this.m_Objectives.Where<TacticalObjective>((Func<TacticalObjective, bool>)(x => x.m_PoliceOwner != null)))
				{
					if (tacticalObjective.m_PoliceOwner == ship)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
					this.m_Objectives.Add((TacticalObjective)new PatrolObjective(ship, ship.Position, this.m_Game.AssetDatabase.PolicePatrolRadius, this.m_Game.AssetDatabase.PolicePatrolRadius));
				TaskGroup taskGroup = new TaskGroup(this.m_Game, this);
				taskGroup.AddShip(ship);
				taskGroup.UpdateTaskGroupType();
				this.m_TaskGroups.Add(taskGroup);
			}
		}

		private void CreateSlaverTaskGroup(List<Ship> ships)
		{
			if (ships.Count <= 0)
				return;
			TaskGroup taskGroup = new TaskGroup(this.m_Game, this);
			taskGroup.AddShips(ships);
			taskGroup.UpdateTaskGroupType();
			this.m_TaskGroups.Add(taskGroup);
		}

		private void CreatePirateTaskGroup(List<Ship> ships)
		{
			if (ships.Count <= 0)
				return;
			bool flag = false;
			if (!this.m_Game.GameDatabase.GetPirateBaseInfos().Any<PirateBaseInfo>((Func<PirateBaseInfo, bool>)(x => x.SystemId == this.m_SystemID)))
			{
				foreach (Ship ship in ships)
				{
					if (ship.WeaponBanks.Any<WeaponBank>((Func<WeaponBank, bool>)(x => x.TurretClass == WeaponEnums.TurretClasses.BoardingPod)))
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				TaskGroup taskGroup = new TaskGroup(this.m_Game, this);
				taskGroup.AddShips(ships);
				taskGroup.Type = TaskGroupType.BoardingGroup;
				this.m_TaskGroups.Add(taskGroup);
			}
			else
			{
				TaskGroup taskGroup = new TaskGroup(this.m_Game, this);
				taskGroup.AddShips(ships);
				taskGroup.UpdateTaskGroupType();
				this.m_TaskGroups.Add(taskGroup);
			}
		}

		private void CreateUnarmedTaskGroup(List<Ship> ships)
		{
			if (ships.Count <= 0)
				return;
			TaskGroup taskGroup = new TaskGroup(this.m_Game, this);
			taskGroup.AddShips(ships);
			taskGroup.UpdateTaskGroupType();
			this.m_TaskGroups.Add(taskGroup);
		}

		private void CreatePlanetAssaultTaskGroup(List<Ship> ships)
		{
			if (ships.Count <= 0)
				return;
			TaskGroup taskGroup = new TaskGroup(this.m_Game, this);
			taskGroup.AddShips(ships);
			taskGroup.UpdateTaskGroupType();
			this.m_TaskGroups.Add(taskGroup);
		}

		private void CreateFreighterTaskGroups(List<Ship> freighters)
		{
			if (freighters.Count <= 0)
				return;
			foreach (Ship freighter in freighters)
			{
				TaskGroup taskGroup = new TaskGroup(this.m_Game, this);
				taskGroup.AddShip(freighter);
				taskGroup.UpdateTaskGroupType();
				this.m_TaskGroups.Add(taskGroup);
			}
		}

		private void InitializeObjectives()
		{
			this.m_bObjectivesInitialized = true;
			this.CreateEnemyObjectives();
			this.CreatePatrolObjectives();
			this.CreateScoutObjectives();
			this.CreatePlanetObjectives();
			this.CreateEvadeObjectives();
			this.CreateRetreatObjectives();
		}

		private void CreateEnemyObjectives()
		{
			foreach (EnemyGroup enemyGroup in this.m_EnemyGroups)
			{
				if (enemyGroup.IsFreighterEnemyGroup())
					this.m_Objectives.Add((TacticalObjective)new BoardTargetObjective(enemyGroup));
				this.m_Objectives.Add((TacticalObjective)new AttackGroupObjective(enemyGroup));
			}
		}

		private void CreatePatrolObjectives()
		{
			if (this.m_Planets.Count == 0 && this.m_Stars.Count == 0)
			{
				Vector3 dest = Vector3.Zero;
				if (this.m_Game.GameDatabase.GetNeutronStarInfos().Any<NeutronStarInfo>((Func<NeutronStarInfo, bool>)(x =>
			   {
				   if (x.DeepSpaceSystemId.HasValue)
					   return x.DeepSpaceSystemId.Value == this.m_SystemID;
				   return false;
			   })) || this.m_Game.GameDatabase.GetGardenerInfos().Any<GardenerInfo>((Func<GardenerInfo, bool>)(x =>
	   {
				   if (x.DeepSpaceSystemId.HasValue)
					   return x.DeepSpaceSystemId.Value == this.m_SystemID;
				   return false;
			   })))
				{
					Vector3 vector3 = new Vector3();
					vector3.X = (this.m_Random.CoinToss(0.5) ? -1f : 1f) * this.m_Random.NextInclusive(1E-05f, 1f);
					vector3.Z = (this.m_Random.CoinToss(0.5) ? -1f : 1f) * this.m_Random.NextInclusive(1E-05f, 1f);
					double num = (double)vector3.Normalize();
					dest = vector3 * 20000f;
				}
				this.m_Objectives.Add((TacticalObjective)new PatrolObjective(dest, 0.0f, 50000f));
			}
			else
			{
				foreach (StellarBody planet in this.m_Planets)
				{
					ColonyInfo colonyInfoForPlanet = this.m_Game.GameDatabase.GetColonyInfoForPlanet(planet.Parameters.OrbitalID);
					if (colonyInfoForPlanet == null || colonyInfoForPlanet.PlayerID != this.m_Player.ID && this.GetDiplomacyState(colonyInfoForPlanet.PlayerID) != DiplomacyState.WAR)
					{
						float patrolDistFromTarget = this.ObtainMinPatrolDistFromTarget((IGameObject)planet);
						if ((double)patrolDistFromTarget > 0.0)
						{
							TacticalObjective tacticalObjective = (TacticalObjective)new PatrolObjective(planet.Parameters.Position, patrolDistFromTarget, this.ObtainMaxPatrolDistFromTarget((IGameObject)planet, patrolDistFromTarget));
							tacticalObjective.m_Planet = planet;
							this.m_Objectives.Add(tacticalObjective);
						}
					}
				}
				foreach (StarModel star in this.m_Stars)
				{
					float patrolDistFromTarget = this.ObtainMinPatrolDistFromTarget((IGameObject)star);
					if ((double)patrolDistFromTarget > 0.0)
						this.m_Objectives.Add((TacticalObjective)new PatrolObjective(star.Position, patrolDistFromTarget, this.ObtainMaxPatrolDistFromTarget((IGameObject)star, patrolDistFromTarget)));
				}
			}
		}

		private float ObtainMinPatrolDistFromTarget(IGameObject target)
		{
			float num1 = 1500f;
			Vector3 zero = Vector3.Zero;
			Vector3 position;
			float val1;
			if (target is StellarBody)
			{
				StellarBody stellarBody = target as StellarBody;
				position = stellarBody.Parameters.Position;
				val1 = (float)((double)stellarBody.Parameters.Radius + 750.0 + 1500.0);
			}
			else
			{
				if (!(target is StarModel))
					return num1;
				StarModel starModel = target as StarModel;
				position = starModel.Position;
				val1 = (float)((double)starModel.Radius + 7500.0 + 2500.0);
			}
			foreach (StellarBody planet in this.m_Planets)
			{
				if (planet != target)
				{
					float num2 = (float)((double)val1 + (double)planet.Parameters.Radius + 1500.0);
					float lengthSquared = (planet.Parameters.Position - position).LengthSquared;
					if ((double)lengthSquared < (double)num2 * (double)num2)
					{
						float val2 = (float)Math.Sqrt((double)lengthSquared) + 1500f + planet.Parameters.Radius;
						val1 = Math.Max(val1, val2);
					}
				}
			}
			foreach (StarModel star in this.m_Stars)
			{
				if (star != target)
				{
					float num2 = val1 + 2500f;
					if ((double)(star.Position - position).LengthSquared < (double)num2 * (double)num2)
						return 0.0f;
				}
			}
			return val1;
		}

		private float ObtainMaxPatrolDistFromTarget(IGameObject target, float minPatrolDist)
		{
			float num1 = minPatrolDist + 10000f;
			Vector3 zero = Vector3.Zero;
			Vector3 position;
			if (target is StellarBody)
			{
				position = (target as StellarBody).Parameters.Position;
			}
			else
			{
				if (!(target is StarModel))
					return num1;
				position = (target as StarModel).Position;
			}
			bool flag = false;
			float num2 = float.MaxValue;
			float num3 = 0.0f;
			Vector3 vector3_1 = Vector3.Zero;
			foreach (StellarBody planet in this.m_Planets)
			{
				if (planet != target)
				{
					float num4 = (float)((double)minPatrolDist + (double)planet.Parameters.Radius + 1500.0);
					float lengthSquared = (planet.Parameters.Position - position).LengthSquared;
					if ((double)lengthSquared >= (double)num4 * (double)num4 && (double)lengthSquared < (double)num2)
					{
						num2 = lengthSquared;
						vector3_1 = planet.Parameters.Position;
						num3 = planet.Parameters.Radius + 1500f;
						flag = true;
					}
				}
			}
			foreach (StarModel star in this.m_Stars)
			{
				if (star != target)
				{
					float num4 = minPatrolDist + 2500f;
					float lengthSquared = (star.Position - position).LengthSquared;
					if ((double)lengthSquared >= (double)num4 * (double)num4 && (double)lengthSquared < (double)num2)
					{
						num2 = lengthSquared;
						vector3_1 = star.Position;
						num3 = 2500f;
						flag = true;
					}
				}
			}
			if (!flag)
				return 1000000f;
			Vector3 vector3_2 = position - vector3_1;
			double num5 = (double)vector3_2.Normalize();
			Vector3 vector3_3 = vector3_1 + vector3_2 * num3;
			return (position - vector3_3).Length;
		}

		private void CreateScoutObjectives()
		{
			foreach (StellarBody planet in this.m_Planets)
			{
				ColonyInfo colonyInfoForPlanet = this.m_Game.GameDatabase.GetColonyInfoForPlanet(planet.Parameters.OrbitalID);
				if (colonyInfoForPlanet != null && this.GetDiplomacyState(colonyInfoForPlanet.PlayerID) == DiplomacyState.WAR)
					this.m_Objectives.Add((TacticalObjective)new ScoutObjective(planet, this.m_Player));
			}
			foreach (EnemyGroup enemyGroup in this.m_EnemyGroups)
			{
				bool flag = false;
				foreach (ScoutObjective scoutObjective in this.m_Objectives.Where<TacticalObjective>((Func<TacticalObjective, bool>)(x => x is ScoutObjective)))
				{
					if ((double)(scoutObjective.m_Destination - enemyGroup.m_LastKnownPosition).LengthSquared < 2500000000.0)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
					this.m_Objectives.Add((TacticalObjective)new ScoutObjective(enemyGroup, this.m_Player));
			}
		}

		private void CreatePlanetObjectives()
		{
			if (this.m_Game == null || this.m_Game.GameDatabase == null)
				return;
			switch (this.m_Game.CurrentState)
			{
				case CommonCombatState _:
					List<int> intList = new List<int>();
					foreach (Ship ship in this.m_Friendly)
					{
						ShipInfo shipInfo = this.m_Game.GameDatabase.GetShipInfo(ship.DatabaseID, false);
						if (shipInfo != null && !intList.Contains(shipInfo.FleetID))
							intList.Add(shipInfo.FleetID);
					}
					PirateBaseInfo pirateBaseInfo = this.m_Game.GameDatabase.GetPirateBaseInfos().FirstOrDefault<PirateBaseInfo>((Func<PirateBaseInfo, bool>)(x => x.SystemId == this.m_SystemID));
					bool flag = this.m_AIType == OverallAIType.PIRATE && pirateBaseInfo != null;
					OrbitalObjectInfo orbitalObjectInfo = (OrbitalObjectInfo)null;
					if (flag)
					{
						StationInfo stationInfo = this.m_Game.GameDatabase.GetStationInfo(pirateBaseInfo.BaseStationId);
						if (stationInfo != null)
							orbitalObjectInfo = this.m_Game.GameDatabase.GetOrbitalObjectInfo(stationInfo.OrbitalObjectID);
					}
					using (List<StellarBody>.Enumerator enumerator = this.m_Planets.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							StellarBody planet = enumerator.Current;
							if (flag && orbitalObjectInfo != null)
							{
								int? parentId = orbitalObjectInfo.ParentID;
								int orbitalId = planet.Parameters.OrbitalID;
								if ((parentId.GetValueOrDefault() != orbitalId ? 0 : (parentId.HasValue ? 1 : 0)) != 0)
								{
									float patrolDistFromTarget = this.ObtainMinPatrolDistFromTarget((IGameObject)planet);
									PatrolObjective patrolObjective = (PatrolObjective)null;
									if ((double)patrolDistFromTarget > 0.0)
									{
										patrolObjective = new PatrolObjective(planet.Parameters.Position, patrolDistFromTarget, this.ObtainMaxPatrolDistFromTarget((IGameObject)planet, patrolDistFromTarget));
										patrolObjective.m_Planet = planet;
										this.m_Objectives.Add((TacticalObjective)patrolObjective);
									}
									this.m_Objectives.Add((TacticalObjective)new DefendPlanetObjective(planet, this, patrolObjective));
									continue;
								}
							}
							ColonyInfo colonyInfoForPlanet = this.m_Game.GameDatabase.GetColonyInfoForPlanet(planet.Parameters.OrbitalID);
							if (colonyInfoForPlanet != null)
							{
								if (colonyInfoForPlanet.PlayerID == this.m_Player.ID)
								{
									float patrolDistFromTarget = this.ObtainMinPatrolDistFromTarget((IGameObject)planet);
									PatrolObjective patrolObjective = (PatrolObjective)null;
									if ((double)patrolDistFromTarget > 0.0)
									{
										patrolObjective = new PatrolObjective(planet.Parameters.Position, patrolDistFromTarget, this.ObtainMaxPatrolDistFromTarget((IGameObject)planet, patrolDistFromTarget));
										patrolObjective.m_Planet = planet;
										this.m_Objectives.Add((TacticalObjective)patrolObjective);
									}
									this.m_Objectives.Add((TacticalObjective)new DefendPlanetObjective(planet, this, patrolObjective));
								}
								else if (this.GetDiplomacyState(colonyInfoForPlanet.PlayerID) == DiplomacyState.WAR)
								{
									this.m_Objectives.Add((TacticalObjective)new AttackPlanetObjective(planet));
								}
								else
								{
									foreach (int fleetID in intList)
									{
										MissionInfo missionByFleetId = this.m_Game.GameDatabase.GetMissionByFleetID(fleetID);
										if (missionByFleetId != null && (missionByFleetId.Type == MissionType.INVASION || missionByFleetId.Type == MissionType.STRIKE) && missionByFleetId.TargetOrbitalObjectID == planet.Parameters.OrbitalID)
										{
											if (!this.m_Objectives.Any<TacticalObjective>((Func<TacticalObjective, bool>)(x =>
										   {
											   if (x.m_ObjectiveType == ObjectiveType.ATTACK_TARGET)
												   return x.m_Planet == planet;
											   return false;
										   })))
											{
												this.m_Objectives.Add((TacticalObjective)new AttackPlanetObjective(planet));
												break;
											}
											break;
										}
									}
								}
							}
						}
						break;
					}
			}
		}

		private void CreateEvadeObjectives()
		{
			foreach (TacticalObjective tacticalObjective in this.m_Objectives.Where<TacticalObjective>((Func<TacticalObjective, bool>)(x =>
		   {
			   if (!(x is PatrolObjective))
				   return x is DefendPlanetObjective;
			   return true;
		   })).ToList<TacticalObjective>())
			{
				if (tacticalObjective is PatrolObjective)
				{
					float sensorRange = this.m_Game.AssetDatabase.DefaultPlanetSensorRange * 0.5f;
					if (tacticalObjective.m_Planet != null)
						sensorRange = (float)((double)tacticalObjective.m_Planet.Parameters.Radius + 750.0 + 1500.0);
					this.m_Objectives.Add((TacticalObjective)new EvadeEnemyObjective(this, tacticalObjective as PatrolObjective, sensorRange));
				}
				if (tacticalObjective is DefendPlanetObjective && (tacticalObjective as DefendPlanetObjective).DefendPatrolObjective != null)
					this.m_Objectives.Add((TacticalObjective)new EvadeEnemyObjective(this, (tacticalObjective as DefendPlanetObjective).DefendPatrolObjective, (float)((double)tacticalObjective.m_Planet.Parameters.Radius + 750.0 + 1500.0)));
			}
		}

		private void CreateRetreatObjectives()
		{
			if (this.m_SpottedEnemies != null)
			{
				foreach (Vector3 allEntryPoint in this.m_SpottedEnemies.GetAllEntryPoints())
					this.m_Objectives.Add((TacticalObjective)new RetreatObjective(allEntryPoint));
			}
			if (this.m_Objectives.Any<TacticalObjective>((Func<TacticalObjective, bool>)(x => x is RetreatObjective)))
				return;
			if (this.m_SpawnProfile != null)
				this.m_Objectives.Add((TacticalObjective)new RetreatObjective(this.m_SpawnProfile._retreatPosition));
			else
				this.m_Objectives.Add((TacticalObjective)new RetreatObjective(Vector3.UnitZ * 100000f));
		}

		private void IdentifyEnemyGroups(List<Ship> enemyShips)
		{
			this.m_EnemyGroups.Clear();
			List<Ship> source = new List<Ship>();
			List<Ship> shipList = new List<Ship>();
			foreach (Ship enemyShip in enemyShips)
			{
				if (enemyShip.ShipClass != ShipClass.BattleRider)
				{
					if (enemyShip.IsNPCFreighter)
						shipList.Add(enemyShip);
					else
						source.Add(enemyShip);
				}
			}
			while (source.Count<Ship>() > 0)
			{
				EnemyGroup eg = new EnemyGroup();
				if (eg.m_Ships.Count<Ship>() < 1)
				{
					Ship ship = source[0];
					eg.m_Ships.Add(ship);
					source.Remove(ship);
				}
				if (!eg.IsFreighterEnemyGroup())
				{
					if (source.Count<Ship>() > 0)
					{
						bool flag = true;
						while (flag)
						{
							flag = false;
							foreach (Ship ship1 in eg.m_Ships)
							{
								foreach (Ship ship2 in source)
								{
									if (!ship2.IsNPCFreighter && (double)(ship1.Maneuvering.Position - ship2.Maneuvering.Position).Length < 2000.0)
									{
										eg.m_Ships.Add(ship2);
										source.Remove(ship2);
										flag = true;
										break;
									}
								}
								if (flag)
									break;
							}
						}
					}
					if (eg.m_Ships.Count<Ship>() > 0)
					{
						eg.m_LastKnownPosition = CombatAI.FindCentreOfMass(eg.m_Ships);
						eg.m_LastKnownDestination = eg.m_LastKnownPosition;
					}
					this.AddEnemyGroup(eg);
				}
			}
		}

		private void AddEnemyGroup(EnemyGroup eg)
		{
			this.m_EnemyGroups.Add(eg);
		}

		private void UpdateEnemyGroups()
		{
			List<EnemyGroup> enemyGroupList = new List<EnemyGroup>();
			foreach (EnemyGroup enemyGroup in this.m_EnemyGroups)
			{
				if (enemyGroup.m_Ships.Count<Ship>() < 1)
					enemyGroupList.Add(enemyGroup);
			}
			foreach (EnemyGroup enemyGroup in enemyGroupList)
			{
				foreach (TacticalObjective objective in this.m_Objectives)
				{
					if (objective.m_TargetEnemyGroup == enemyGroup)
						objective.m_TargetEnemyGroup = (EnemyGroup)null;
				}
				foreach (TaskGroup taskGroup in this.m_TaskGroups)
				{
					if (taskGroup.EnemyGroupInContact == enemyGroup)
						taskGroup.ClearEnemyGroupInContact();
				}
				this.m_EnemyGroups.Remove(enemyGroup);
			}
			if (this.m_EGUpdatePhase == 0)
			{
				List<Ship> shipList1 = new List<Ship>();
				List<Ship> shipList2 = new List<Ship>();
				foreach (Ship ship in this.m_Enemy)
				{
					if (this.IsShipDetected(ship) && ship.ShipClass != ShipClass.BattleRider)
					{
						if (ship.IsNPCFreighter)
							shipList2.Add(ship);
						else
							shipList1.Add(ship);
					}
				}
				foreach (EnemyGroup enemyGroup in this.m_EnemyGroups)
				{
					if (!enemyGroup.IsFreighterEnemyGroup())
					{
						foreach (Ship ship in enemyGroup.m_Ships)
						{
							if (shipList1.Contains(ship))
								shipList1.Remove(ship);
						}
					}
				}
				foreach (Ship ship in shipList1)
				{
					bool flag = false;
					foreach (EnemyGroup enemyGroup in this.m_EnemyGroups)
						flag = this.TryAddShipToEnemyGroup(ship, enemyGroup, 2000f);
					if (!flag)
					{
						EnemyGroup eg = new EnemyGroup();
						eg.m_Ships.Add(ship);
						if (eg.m_Ships.Count<Ship>() > 0)
						{
							eg.m_LastKnownPosition = CombatAI.FindCentreOfMass(eg.m_Ships);
							eg.m_LastKnownDestination = eg.m_LastKnownPosition;
						}
						this.AddEnemyGroup(eg);
					}
				}
				foreach (Ship ship in shipList2)
				{
					bool flag = false;
					foreach (EnemyGroup enemyGroup in this.m_EnemyGroups)
					{
						if (enemyGroup.m_Ships.Contains(ship))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						EnemyGroup eg = new EnemyGroup();
						eg.m_Ships.Add(ship);
						if (eg.m_Ships.Count<Ship>() > 0)
						{
							eg.m_LastKnownPosition = CombatAI.FindCentreOfMass(eg.m_Ships);
							eg.m_LastKnownDestination = eg.m_LastKnownPosition;
						}
						this.AddEnemyGroup(eg);
					}
				}
				foreach (EnemyGroup enemyGroup in this.m_EnemyGroups)
				{
					bool flag = false;
					foreach (TacticalObjective objective in this.m_Objectives)
					{
						if (objective.m_TargetEnemyGroup == enemyGroup)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						if (enemyGroup.IsFreighterEnemyGroup())
							this.m_Objectives.Add((TacticalObjective)new BoardTargetObjective(enemyGroup));
						this.m_Objectives.Add((TacticalObjective)new AttackGroupObjective(enemyGroup));
					}
				}
			}
			else if (this.m_EGUpdatePhase == 1)
			{
				for (int index1 = 0; index1 < this.m_EnemyGroups.Count<EnemyGroup>() - 1; ++index1)
				{
					bool flag = false;
					EnemyGroup enemyGroup1 = this.m_EnemyGroups[index1];
					for (int index2 = index1 + 1; index2 < this.m_EnemyGroups.Count<EnemyGroup>(); ++index2)
					{
						EnemyGroup enemyGroup2 = this.m_EnemyGroups[index2];
						List<Ship> shipList = new List<Ship>();
						foreach (Ship ship in enemyGroup2.m_Ships)
						{
							if (this.TryAddShipToEnemyGroup(ship, enemyGroup1, 2000f))
							{
								shipList.Add(ship);
								flag = true;
							}
						}
						if (flag)
						{
							Vector3 averageVelocity1 = enemyGroup1.GetAverageVelocity(this);
							Vector3 averageVelocity2 = enemyGroup2.GetAverageVelocity(this);
							if ((double)averageVelocity1.LengthSquared > 0.0 && (double)averageVelocity2.Length > 0.0)
							{
								double num1 = (double)averageVelocity1.Normalize();
								double num2 = (double)averageVelocity2.Normalize();
								if ((double)Vector3.Dot(averageVelocity1, averageVelocity2) < 0.699999988079071)
									flag = false;
							}
						}
						foreach (Ship ship in shipList)
							enemyGroup2.m_Ships.Remove(ship);
						if (flag)
						{
							shipList.Clear();
							using (List<Ship>.Enumerator enumerator = enemyGroup2.m_Ships.GetEnumerator())
							{
								if (enumerator.MoveNext())
								{
									Ship current = enumerator.Current;
									shipList.Add(current);
									enemyGroup1.m_Ships.Add(current);
								}
							}
							foreach (Ship ship in shipList)
								enemyGroup2.m_Ships.Remove(ship);
							if (enemyGroup2.m_Ships.Count<Ship>() < 1)
								this.m_EnemyGroups.Remove(enemyGroup2);
						}
					}
					if (flag)
						break;
				}
			}
			else if (this.m_EGUpdatePhase == 2)
			{
				foreach (EnemyGroup enemyGroup1 in this.m_EnemyGroups)
				{
					EnemyGroup enemyGroup2 = new EnemyGroup();
					bool flag = false;
					for (int index = 0; index < enemyGroup1.m_Ships.Count<Ship>(); ++index)
					{
						Ship ship = enemyGroup1.m_Ships[index];
						if (enemyGroup2.m_Ships.Count<Ship>() < 1)
							enemyGroup2.m_Ships.Add(ship);
						else if (!enemyGroup2.m_Ships.Contains(ship) && this.TryAddShipToEnemyGroup(ship, enemyGroup2, 2500f))
							index = 0;
					}
					if (enemyGroup2.m_Ships.Count<Ship>() > 0 && enemyGroup2.m_Ships.Count<Ship>() < enemyGroup1.m_Ships.Count<Ship>())
					{
						foreach (Ship ship in enemyGroup2.m_Ships)
							enemyGroup1.m_Ships.Remove(ship);
						this.AddEnemyGroup(enemyGroup2);
						flag = true;
					}
					if (flag)
						break;
				}
			}
			else if (this.m_EGUpdatePhase == 3)
			{
				for (int index = 0; index < this.m_EnemyGroups.Count<EnemyGroup>(); ++index)
				{
					Vector3 zero1 = Vector3.Zero;
					Vector3 zero2 = Vector3.Zero;
					int num = 0;
					foreach (Ship ship in this.m_EnemyGroups[index].m_Ships)
					{
						if (this._inTestMode || this.IsShipDetected(ship))
						{
							zero1 += ship.Maneuvering.Position;
							zero2 += ship.Maneuvering.Destination;
							++num;
						}
					}
					if (num < 1)
						this.m_EnemyGroups.Remove(this.m_EnemyGroups[index]);
					else if (num > 0)
					{
						this.m_EnemyGroups[index].m_LastKnownPosition = zero1 / (float)num;
						this.m_EnemyGroups[index].m_LastKnownPosition.Y = 0.0f;
						this.m_EnemyGroups[index].m_LastKnownDestination = zero2 / (float)num;
						this.m_EnemyGroups[index].m_LastKnownDestination.Y = 0.0f;
						this.m_EnemyGroups[index].GetAverageVelocity(this);
					}
					else if (this.m_EnemyGroups[index].m_Ships.Count > 0)
					{
						this.m_EnemyGroups[index].m_LastKnownPosition = CombatAI.FindCentreOfMass(this.m_EnemyGroups[index].m_Ships);
						this.m_EnemyGroups[index].m_LastKnownPosition.Y = 0.0f;
						this.m_EnemyGroups[index].m_LastKnownDestination = this.m_EnemyGroups[index].m_LastKnownPosition;
					}
				}
			}
			else
			{
				int egUpdatePhase = this.m_EGUpdatePhase;
			}
			this.m_EGUpdatePhase = (this.m_EGUpdatePhase + 1) % 5;
		}

		private void TryAddSpecialShipControl(Ship ship)
		{
			if (ship.WeaponControlsIsInitilized)
				return;
			ship.InitializeWeaponControls();
			if (ship.IsSystemDefenceBoat && !ship.DefenseBoatActive)
			{
				SDBInfo sdbInfo = this.m_Game.GameDatabase.GetSDBInfoFromShip(ship.DatabaseID);
				StellarBody planet = (StellarBody)null;
				if (sdbInfo != null)
					planet = this.m_Planets.FirstOrDefault<StellarBody>((Func<StellarBody, bool>)(x => x.Parameters.OrbitalID == sdbInfo.OrbitalId));
				this.m_ShipWeaponControls.Add((SpecWeaponControl)new SystemDefenseBoatControl(this.m_Game, this, ship, planet));
			}
			if (ship.IsWraithAbductor)
				this.m_ShipWeaponControls.Add((SpecWeaponControl)new WraithAbductorAssaultControl(this.m_Game, this, ship));
			else if (ship.IsCarrier)
			{
				if (ship.BattleRiderMounts.Any<BattleRiderMount>((Func<BattleRiderMount, bool>)(x => x.WeaponBank.TurretClass == WeaponEnums.TurretClasses.Biomissile)))
				{
					BioMissileLaunchControl missileLaunchControl = new BioMissileLaunchControl(this.m_Game, this, ship);
					missileLaunchControl.AddRiders(this.m_Friendly.Where<Ship>((Func<Ship, bool>)(x =>
				   {
					   if (!x.IsBattleRider || x.RealShipClass != RealShipClasses.Biomissile)
						   return false;
					   if (x.ParentDatabaseID != ship.DatabaseID)
						   return x.ParentID == ship.ObjectID;
					   return true;
				   })).ToList<Ship>());
					this.m_ShipWeaponControls.Add((SpecWeaponControl)missileLaunchControl);
				}
				if (ship.BattleRiderMounts.Any<BattleRiderMount>((Func<BattleRiderMount, bool>)(x => x.WeaponBank.TurretClass == WeaponEnums.TurretClasses.Drone)))
				{
					DroneLaunchControl droneLaunchControl = new DroneLaunchControl(this.m_Game, this, ship);
					droneLaunchControl.AddRiders(this.m_Friendly.Where<Ship>((Func<Ship, bool>)(x =>
				   {
					   if (!x.IsBattleRider || x.RealShipClass != RealShipClasses.Drone)
						   return false;
					   if (x.ParentDatabaseID != ship.DatabaseID)
						   return x.ParentID == ship.ObjectID;
					   return true;
				   })).ToList<Ship>());
					this.m_ShipWeaponControls.Add((SpecWeaponControl)droneLaunchControl);
				}
				if (ship.BattleRiderMounts.Any<BattleRiderMount>((Func<BattleRiderMount, bool>)(x => x.WeaponBank.TurretClass == WeaponEnums.TurretClasses.AssaultShuttle)))
				{
					AssaultShuttleLaunchControl shuttleLaunchControl = new AssaultShuttleLaunchControl(this.m_Game, this, ship);
					shuttleLaunchControl.AddRiders(this.m_Friendly.Where<Ship>((Func<Ship, bool>)(x =>
				   {
					   if (!x.IsBattleRider || x.RealShipClass != RealShipClasses.AssaultShuttle)
						   return false;
					   if (x.ParentDatabaseID != ship.DatabaseID)
						   return x.ParentID == ship.ObjectID;
					   return true;
				   })).ToList<Ship>());
					this.m_ShipWeaponControls.Add((SpecWeaponControl)shuttleLaunchControl);
				}
				if (ship.BattleRiderMounts.Any<BattleRiderMount>((Func<BattleRiderMount, bool>)(x => x.WeaponBank.TurretClass == WeaponEnums.TurretClasses.BoardingPod)))
				{
					BoardingPodLaunchControl podLaunchControl = new BoardingPodLaunchControl(this.m_Game, this, ship);
					podLaunchControl.AddRiders(this.m_Friendly.Where<Ship>((Func<Ship, bool>)(x =>
				   {
					   if (!x.IsBattleRider || x.RealShipClass != RealShipClasses.BoardingPod)
						   return false;
					   if (x.ParentDatabaseID != ship.DatabaseID)
						   return x.ParentID == ship.ObjectID;
					   return true;
				   })).ToList<Ship>());
					this.m_ShipWeaponControls.Add((SpecWeaponControl)podLaunchControl);
				}
				if (ship.BattleRiderMounts.Any<BattleRiderMount>((Func<BattleRiderMount, bool>)(x => x.WeaponBank.TurretClass == WeaponEnums.TurretClasses.DestroyerRider)))
				{
					AttackRiderLaunchControl riderLaunchControl = new AttackRiderLaunchControl(this.m_Game, this, ship, WeaponEnums.TurretClasses.DestroyerRider);
					riderLaunchControl.AddRiders(this.m_Friendly.Where<Ship>((Func<Ship, bool>)(x =>
				   {
					   if (!x.IsBattleRider || x.RealShipClass != RealShipClasses.BattleRider)
						   return false;
					   if (x.ParentDatabaseID != ship.DatabaseID)
						   return x.ParentID == ship.ObjectID;
					   return true;
				   })).ToList<Ship>());
					this.m_ShipWeaponControls.Add((SpecWeaponControl)riderLaunchControl);
				}
				if (ship.BattleRiderMounts.Any<BattleRiderMount>((Func<BattleRiderMount, bool>)(x => x.WeaponBank.TurretClass == WeaponEnums.TurretClasses.CruiserRider)))
				{
					LargeRiderLaunchControl riderLaunchControl = new LargeRiderLaunchControl(this.m_Game, this, ship, WeaponEnums.TurretClasses.CruiserRider);
					riderLaunchControl.AddRiders(this.m_Friendly.Where<Ship>((Func<Ship, bool>)(x =>
				   {
					   if (!x.IsBattleRider || x.ShipClass != ShipClass.Cruiser)
						   return false;
					   if (x.ParentDatabaseID != ship.DatabaseID)
						   return x.ParentID == ship.ObjectID;
					   return true;
				   })).ToList<Ship>());
					this.m_ShipWeaponControls.Add((SpecWeaponControl)riderLaunchControl);
				}
				if (ship.BattleRiderMounts.Any<BattleRiderMount>((Func<BattleRiderMount, bool>)(x => x.WeaponBank.TurretClass == WeaponEnums.TurretClasses.DreadnoughtRider)))
				{
					LargeRiderLaunchControl riderLaunchControl = new LargeRiderLaunchControl(this.m_Game, this, ship, WeaponEnums.TurretClasses.DreadnoughtRider);
					riderLaunchControl.AddRiders(this.m_Friendly.Where<Ship>((Func<Ship, bool>)(x =>
				   {
					   if (!x.IsBattleRider || x.ShipClass != ShipClass.Dreadnought)
						   return false;
					   if (x.ParentDatabaseID != ship.DatabaseID)
						   return x.ParentID == ship.ObjectID;
					   return true;
				   })).ToList<Ship>());
					this.m_ShipWeaponControls.Add((SpecWeaponControl)riderLaunchControl);
				}
			}
			if (ship.WeaponBanks.Any<WeaponBank>((Func<WeaponBank, bool>)(x => x.LogicalBank.TurretClass == WeaponEnums.TurretClasses.COL)))
			{
				WeaponBank weaponBank = ship.WeaponBanks.First<WeaponBank>((Func<WeaponBank, bool>)(x => x.TurretClass == WeaponEnums.TurretClasses.COL));
				this.m_ShipWeaponControls.Add((SpecWeaponControl)new PositionalAttackControl(this.m_Game, this, ship, weaponBank.Weapon.UniqueWeaponID, WeaponEnums.TurretClasses.COL));
			}
			if (ship.IsAcceleratorHoop && ship.WeaponBanks.Any<WeaponBank>((Func<WeaponBank, bool>)(x =>
		   {
			   if (x.Weapon != null)
				   return x.Weapon.PayloadType == WeaponEnums.PayloadTypes.MegaBeam;
			   return false;
		   })))
			{
				WeaponBank weaponBank = ship.WeaponBanks.First<WeaponBank>((Func<WeaponBank, bool>)(x => x.Weapon.PayloadType == WeaponEnums.PayloadTypes.MegaBeam));
				this.m_ShipWeaponControls.Add((SpecWeaponControl)new TachyonCannonAttackControl(this.m_Game, this, ship, weaponBank.Weapon.UniqueWeaponID, WeaponEnums.TurretClasses.FreeBeam));
			}
			foreach (WeaponBank weaponBank in ship.WeaponBanks)
			{
				WeaponBank wb = weaponBank;
				if (((IEnumerable<WeaponEnums.WeaponTraits>)wb.Weapon.Traits).Contains<WeaponEnums.WeaponTraits>(WeaponEnums.WeaponTraits.Detonating) && !this.m_ShipWeaponControls.OfType<PositionalAttackControl>().Any<PositionalAttackControl>((Func<PositionalAttackControl, bool>)(x =>
			  {
				  if (x.ControlledShip == ship)
					  return x.WeaponID == wb.Weapon.UniqueWeaponID;
				  return false;
			  })))
					this.m_ShipWeaponControls.Add((SpecWeaponControl)new PositionalAttackControl(this.m_Game, this, ship, wb.Weapon.UniqueWeaponID, wb.TurretClass));
			}
			if (ship.WeaponBanks.Any<WeaponBank>((Func<WeaponBank, bool>)(x => x.LogicalBank.TurretClass == WeaponEnums.TurretClasses.NodeCannon)))
			{
				WeaponBank weaponBank = ship.WeaponBanks.First<WeaponBank>((Func<WeaponBank, bool>)(x => x.LogicalBank.TurretClass == WeaponEnums.TurretClasses.NodeCannon));
				this.m_ShipWeaponControls.Add((SpecWeaponControl)new NodeCannonAttackControl(this.m_Game, this, ship, weaponBank.Weapon.UniqueWeaponID, WeaponEnums.TurretClasses.NodeCannon));
			}
			if (ship.WeaponBanks.Any<WeaponBank>((Func<WeaponBank, bool>)(x => x.LogicalBank.TurretClass == WeaponEnums.TurretClasses.Minelayer)))
				this.m_ShipWeaponControls.Add((SpecWeaponControl)new MineLayerControl(this.m_Game, this, ship, WeaponEnums.TurretClasses.Minelayer));
			if (ship.WeaponBanks.Any<WeaponBank>((Func<WeaponBank, bool>)(x => x.LogicalBank.TurretClass == WeaponEnums.TurretClasses.Siege)))
				this.m_ShipWeaponControls.Add((SpecWeaponControl)new AttackPlanetControl(this.m_Game, this, ship, WeaponEnums.TurretClasses.Siege));
			if (ship.CurrentPsiPower <= 0 || ship.Psionics.Count<Psionic>() <= 0)
				return;
			this.m_ShipPsionicControls.Add(new ShipPsionicControl(this.m_Game, this, ship));
		}

		private bool TryAddShipToEnemyGroup(Ship ship, EnemyGroup eGroup, float range = 2000f)
		{
			if (eGroup.IsFreighterEnemyGroup())
				return false;
			foreach (Ship ship1 in eGroup.m_Ships)
			{
				if ((double)(ship.Maneuvering.Position - ship1.Maneuvering.Position).LengthSquared < (double)range * (double)range)
				{
					eGroup.m_Ships.Add(ship);
					return true;
				}
			}
			return false;
		}

		private Ship GetShipByID(int id, bool friendly)
		{
			if (friendly)
			{
				foreach (Ship ship in this.m_Friendly)
				{
					if (ship.ObjectID == id)
						return ship;
				}
			}
			else
			{
				foreach (Ship ship in this.m_Friendly)
				{
					if (ship.ObjectID == id)
						return ship;
				}
			}
			return (Ship)null;
		}

		public static float GetMaxWeaponRangeFromShips(List<Ship> ships)
		{
			float val1 = 0.0f;
			foreach (Ship ship in ships)
			{
				List<WeaponBank> list = ship.WeaponBanks.ToList<WeaponBank>();
				if (list.Count > 0)
					val1 = Math.Max(val1, list.Max<WeaponBank>((Func<WeaponBank, float>)(x => x.Weapon.Range)));
			}
			return val1;
		}

		public static float GetMinWeaponRange(Ship ship, bool usePD = false)
		{
			float num = 0.0f;
			bool flag = false;
			foreach (WeaponBank weaponBank in ship.WeaponBanks)
			{
				if (usePD || !weaponBank.Weapon.IsPDWeapon())
				{
					float range = weaponBank.Weapon.Range;
					if ((double)range > 0.0)
					{
						if (!flag)
						{
							num = range;
							flag = true;
						}
						else if ((double)range < (double)num)
							num = range;
					}
				}
			}
			if (flag)
				return num;
			return 500f;
		}

		public static float GetMinEffectiveWeaponRange(Ship ship, bool usePD = false)
		{
			float num = 0.0f;
			bool flag = false;
			foreach (WeaponBank weaponBank in ship.WeaponBanks)
			{
				if (usePD || !weaponBank.Weapon.IsPDWeapon())
				{
					float range = weaponBank.Weapon.RangeTable.Effective.Range;
					if ((double)range > 0.0)
					{
						if (!flag)
						{
							num = range;
							flag = true;
						}
						else if ((double)range < (double)num)
							num = range;
					}
				}
			}
			if (flag)
				return num;
			return 500f;
		}

		public static float GetMaxEffectiveWeaponRange(Ship ship, bool usePD = false)
		{
			float val2 = 0.0f;
			foreach (WeaponBank weaponBank in ship.WeaponBanks)
			{
				if (usePD || !weaponBank.Weapon.IsPDWeapon())
					val2 = Math.Max(weaponBank.Weapon.RangeTable.Effective.Range, val2);
			}
			if ((double)val2 > 0.0)
				return val2;
			return 500f;
		}

		public static float GetMinPointBlankWeaponRange(Ship ship, bool usePD = false)
		{
			float num = 0.0f;
			bool flag = false;
			foreach (WeaponBank weaponBank in ship.WeaponBanks)
			{
				if (usePD || !weaponBank.Weapon.IsPDWeapon())
				{
					float range = weaponBank.Weapon.RangeTable.PointBlank.Range;
					if ((double)range > 0.0)
					{
						if (!flag)
						{
							num = range;
							flag = true;
						}
						else if ((double)range < (double)num)
							num = range;
					}
				}
			}
			if (flag)
				return num;
			return 500f;
		}

		public static float GetAveEffectiveWeaponRange(Ship ship, bool usePD = false)
		{
			float num1 = 0.0f;
			int num2 = 0;
			foreach (WeaponBank weaponBank in ship.WeaponBanks)
			{
				if (usePD || !weaponBank.Weapon.IsPDWeapon())
				{
					float range = weaponBank.Weapon.RangeTable.Effective.Range;
					if ((double)range > 0.0)
					{
						num1 += range;
						++num2;
					}
				}
			}
			if (num2 > 0)
				return num1 / (float)num2;
			return 500f;
		}

		public static float GetMaxWeaponRange(Ship ship, bool usePD = false)
		{
			float num = 0.0f;
			bool flag = false;
			foreach (WeaponBank weaponBank in ship.WeaponBanks)
			{
				if (usePD || !weaponBank.Weapon.IsPDWeapon())
				{
					float range = weaponBank.Weapon.RangeTable.Maximum.Range;
					if ((double)range > 0.0)
					{
						if (!flag)
						{
							num = range;
							flag = true;
						}
						else if ((double)range > (double)num)
							num = range;
					}
				}
			}
			if (flag)
				return num;
			return 500f;
		}

		public static bool IsTargetInRange(Ship ship, IGameObject target)
		{
			if (target == null)
				return false;
			float num1 = 0.0f;
			if (target is Ship)
			{
				Ship ship1 = target as Ship;
				num1 = (ship.Maneuvering.Position - ship1.Maneuvering.Position).Length + ship.ShipSphere.radius;
			}
			else if (target is StellarBody)
			{
				StellarBody stellarBody = target as StellarBody;
				num1 = (ship.Maneuvering.Position - stellarBody.Parameters.Position).Length - stellarBody.Parameters.Radius;
			}
			float num2 = CombatAI.GetMaxWeaponRange(ship, false) * 1.3f;
			return (double)num1 < (double)num2;
		}

		public static Vector3 FindCentreOfMass(List<Ship> ships)
		{
			Vector3 vector3 = new Vector3(0.0f, 0.0f, 0.0f);
			float num1 = 0.0f;
			float num2 = 0.0f;
			int num3 = 0;
			foreach (Ship ship in ships)
			{
				num1 += ship.Maneuvering.Position.X;
				num2 += ship.Maneuvering.Position.Z;
				++num3;
			}
			if (num3 < 1)
				return vector3;
			vector3.X = num1 / (float)num3;
			vector3.Z = num2 / (float)num3;
			return vector3;
		}

		private TaskGroup GetTaskGroupForShipList(List<int> shipIDs)
		{
			List<TaskGroup> source = new List<TaskGroup>();
			List<int> intList1 = new List<int>();
			foreach (int shipId in shipIDs)
			{
				Ship shipById = this.GetShipByID(shipId, true);
				if (shipById != null && shipById.TaskGroup != null && !source.Contains(shipById.TaskGroup))
				{
					source.Add(shipById.TaskGroup);
					intList1.Add(0);
				}
			}
			if (source.Count<TaskGroup>() == 1)
				return source[0];
			if (source.Count<TaskGroup>() <= 1)
				return new TaskGroup(this.m_Game, this);
			int num = 0;
			int index1 = -1;
			for (int index2 = 0; index2 < source.Count<TaskGroup>(); ++index2)
			{
				foreach (int shipId in shipIDs)
				{
					Ship shipById = this.GetShipByID(shipId, true);
					if (shipById != null && shipById.TaskGroup == source[index2])
					{
						List<int> intList2;
						int index3;
						(intList2 = intList1)[index3 = index2] = intList2[index3] + 1;
						if (intList1[index2] > num)
						{
							num = intList1[index2];
							index1 = index2;
						}
					}
				}
			}
			return source[index1];
		}

		public void AddTaskGroup(TaskGroup nuGroup)
		{
			if (this.m_TaskGroups.Contains(nuGroup))
				return;
			this.m_TaskGroups.Add(nuGroup);
		}

		public bool IsFriendOrAlly(int playerID)
		{
			if (this._currentDiploStates == null)
				return playerID == this.m_Player.ID;
			if (playerID == this.m_Player.ID)
				return true;
			DiplomacyState diplomacyState = this.GetDiplomacyState(playerID);
			if (diplomacyState != DiplomacyState.PEACE)
				return diplomacyState == DiplomacyState.ALLIED;
			return true;
		}

		public void SetDiplomacyState(int playerID, DiplomacyState state)
		{
			if (this._currentDiploStates == null || !this._currentDiploStates.ContainsKey(playerID))
				return;
			this._currentDiploStates[playerID] = state;
		}

		public virtual DiplomacyState GetDiplomacyState(int playerID)
		{
			if (this._currentDiploStates == null)
				return playerID != this.m_Player.ID ? DiplomacyState.WAR : DiplomacyState.PEACE;
			DiplomacyState diplomacyState;
			if (!this._currentDiploStates.TryGetValue(playerID, out diplomacyState))
				diplomacyState = DiplomacyState.NEUTRAL;
			return diplomacyState;
		}

		public StellarBody GetPlanetContainingPosition(Vector3 position)
		{
			StellarBody stellarBody = (StellarBody)null;
			foreach (StellarBody planet in this.m_Planets)
			{
				float lengthSquared = (planet.Parameters.Position - position).LengthSquared;
				float num = (float)((double)planet.Parameters.Radius + 750.0 + 500.0);
				if ((double)lengthSquared < (double)num * (double)num)
				{
					stellarBody = planet;
					break;
				}
			}
			return stellarBody;
		}

		public StellarBody GetClosestEnemyPlanet(Vector3 position, float range)
		{
			StellarBody stellarBody = (StellarBody)null;
			float num1 = float.MaxValue;
			foreach (StellarBody planet in this.m_Planets)
			{
				if (planet.Population != 0.0 && this.GetDiplomacyState(planet.Parameters.ColonyPlayerID) == DiplomacyState.WAR)
				{
					float lengthSquared = (planet.Parameters.Position - position).LengthSquared;
					float num2 = (float)((double)planet.Parameters.Radius + 750.0 + 500.0) + range;
					if ((double)lengthSquared < (double)num1 && (double)lengthSquared < (double)num2 * (double)num2)
					{
						num1 = lengthSquared;
						stellarBody = planet;
					}
				}
			}
			return stellarBody;
		}

		public Vector3 GetSafeDestination(Vector3 currentPos, Vector3 dest)
		{
			Vector3 vector3_1 = dest;
			bool flag = false;
			foreach (StellarBody planet in this.m_Planets)
			{
				float lengthSquared = (planet.Parameters.Position - vector3_1).LengthSquared;
				float num1 = (float)((double)planet.Parameters.Radius + 750.0 + 500.0);
				if ((double)lengthSquared < (double)num1 * (double)num1)
				{
					Vector3 vector3_2 = dest - planet.Parameters.Position;
					vector3_2.Y = 0.0f;
					if ((double)vector3_2.LengthSquared > 0.0)
					{
						double num2 = (double)vector3_2.Normalize();
					}
					else
					{
						vector3_2 = currentPos - dest;
						vector3_2.Y = 0.0f;
						double num3 = (double)vector3_2.Normalize();
					}
					vector3_1 = planet.Parameters.Position + vector3_2 * num1;
					flag = true;
				}
				if (flag)
					break;
			}
			if (!flag)
			{
				foreach (StarModel star in this.m_Stars)
				{
					float lengthSquared = (star.Position - vector3_1).LengthSquared;
					float num1 = (float)((double)star.Radius + 7500.0 + 500.0);
					if ((double)lengthSquared < (double)num1 * (double)num1)
					{
						Vector3 vector3_2 = dest - star.Position;
						vector3_2.Y = 0.0f;
						double num2 = (double)vector3_2.Normalize();
						vector3_1 = star.Position + vector3_2 * num1;
						flag = true;
					}
					if (flag)
						break;
				}
			}
			return vector3_1;
		}

		public Vector3 PickNewDest(
		  Vector3 desiredDest,
		  Vector3 from,
		  float radius,
		  float minRightAngle,
		  float minLeftAngle)
		{
			Vector3 zero = Vector3.Zero;
			Vector3 vector3_1 = from - desiredDest;
			vector3_1.Y = 0.0f;
			double num = (double)vector3_1.Normalize();
			Vector3 vector3_2;
			if ((double)minRightAngle < 0.00999999977648258 && (double)minLeftAngle < 0.00999999977648258)
				vector3_2 = desiredDest + vector3_1 * radius;
			else if ((double)Math.Abs(minRightAngle) < (double)Math.Abs(minLeftAngle))
			{
				Vector3 vector3_3 = vector3_1;
				vector3_3.X = (float)(Math.Cos((double)minRightAngle) * (double)vector3_1.X - Math.Sin((double)minRightAngle) * (double)vector3_1.Z);
				vector3_3.Z = (float)(Math.Sin((double)minRightAngle) * (double)vector3_1.X + Math.Cos((double)minRightAngle) * (double)vector3_1.Z);
				vector3_2 = desiredDest + vector3_3 * radius;
			}
			else
			{
				Vector3 vector3_3 = vector3_1;
				vector3_3.X = (float)(Math.Cos((double)minLeftAngle) * (double)vector3_1.X - Math.Sin((double)minLeftAngle) * (double)vector3_1.Z);
				vector3_3.Z = (float)(Math.Sin((double)minLeftAngle) * (double)vector3_1.X + Math.Cos((double)minLeftAngle) * (double)vector3_1.Z);
				vector3_2 = desiredDest + vector3_3 * radius;
			}
			return vector3_2;
		}

		private void FaceTarget(Ship ship, Ship target)
		{
			Vector3 look = target.Maneuvering.Position - ship.Maneuvering.Position;
			look.Y = 0.0f;
			double num = (double)look.Normalize();
			ship.Maneuvering.PostAddGoal(ship.Maneuvering.Position, look);
		}

		private void TurnBroadsideToTarget(Ship ship, Ship target)
		{
			Matrix rotationYpr = Matrix.CreateRotationYPR(ship.Maneuvering.Rotation);
			Vector3 vector3 = target.Maneuvering.Position - ship.Maneuvering.Position;
			vector3.Y = 0.0f;
			double num = (double)vector3.Normalize();
			Vector3 v0 = Vector3.Cross(vector3, Vector3.UnitY);
			Vector3 look = (double)Vector3.Dot(v0, rotationYpr.Forward) >= 0.0 ? v0 : Vector3.Cross(Vector3.UnitY, vector3);
			ship.Maneuvering.PostAddGoal(ship.Maneuvering.Position, look);
		}

		public List<Ship> GetFriendlyShips()
		{
			return this.m_Friendly;
		}

		public IEnumerable<EnemyGroup> GetEnemyGroups()
		{
			return (IEnumerable<EnemyGroup>)this.m_EnemyGroups;
		}

		public IEnumerable<TaskGroup> GetTaskGroups()
		{
			return (IEnumerable<TaskGroup>)this.m_TaskGroups;
		}

		public void FlagAttackingShip(Ship attackingShip)
		{
			this.m_SpottedEnemies.AddShip(attackingShip, true);
			this.m_SpottedEnemies.SetEnemySpotted(attackingShip);
		}

		public void NotifyCombatZoneChanged(CombatZonePositionInfo zone)
		{
			if (zone == null || zone.Player == 0 || this.GetDiplomacyState(zone.Player) != DiplomacyState.WAR)
				return;
			this.m_SpottedEnemies.UpdateSpottedShipsInZone(zone);
			List<Ship> detectedShips = this.m_SpottedEnemies.GetDetectedShips();
			bool flag = false;
			foreach (Ship ship in detectedShips)
			{
				if (!this.m_Enemy.Contains(ship))
				{
					flag = true;
					this.m_Enemy.Add(ship);
				}
				this.m_CloakedEnemyDetection.ShipSpotted(ship);
			}
			if (flag)
			{
				if (this.m_EnemyGroups.Count == 0)
				{
					this.IdentifyEnemyGroups(this.m_Enemy);
				}
				else
				{
					this.m_EGUpdatePhase = 0;
					this.UpdateEnemyGroups();
				}
			}
			List<EnemyGroup> enemyGroupList = new List<EnemyGroup>();
			foreach (EnemyGroup enemyGroup in this.m_EnemyGroups)
			{
				foreach (Ship ship in detectedShips)
				{
					if (!ship.IsDetected(this.m_Player) && enemyGroup.m_Ships.Contains(ship))
					{
						enemyGroupList.Add(enemyGroup);
						break;
					}
				}
			}
			foreach (ScoutObjective scoutObjective in this.m_Objectives.OfType<ScoutObjective>().ToList<ScoutObjective>())
			{
				if (scoutObjective.m_TargetEnemyGroup != null && enemyGroupList.Contains(scoutObjective.m_TargetEnemyGroup))
					enemyGroupList.Remove(scoutObjective.m_TargetEnemyGroup);
			}
			foreach (EnemyGroup eGroup in enemyGroupList)
				this.m_Objectives.Add((TacticalObjective)new ScoutObjective(eGroup, this.m_Player));
			if (enemyGroupList.Count <= 0)
				return;
			TaskGroup taskGroup1 = (TaskGroup)null;
			if (this.m_TaskGroups.Count == 1)
				this.m_TaskGroups.ElementAt<TaskGroup>(0).Objective = (TacticalObjective)null;
			if (taskGroup1 == null)
			{
				foreach (TaskGroup taskGroup2 in this.m_TaskGroups)
				{
					if (taskGroup2.Type != TaskGroupType.Civilian && (taskGroup2.Objective is PatrolObjective || taskGroup2.Objective is DefendPlanetObjective))
					{
						taskGroup1 = taskGroup2;
						break;
					}
				}
			}
			if (taskGroup1 == null)
				return;
			taskGroup1.Objective = (TacticalObjective)null;
		}

		private void InitializeNodeMaws(App game, Kerberos.Sots.GameStates.StarSystem starSystem)
		{
			this.m_NodeMaws = new List<NodeMawInfo>();
			if (starSystem == null)
				return;
			List<Vector3> locationsForPlayer = starSystem.GetNodeMawLocationsForPlayer(game, this.m_Player.ID);
			if (locationsForPlayer.Count == 0)
				return;
			LogicalWeapon logicalWeapon = game.AssetDatabase.Weapons.FirstOrDefault<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.WeaponName == "NodeMaw"));
			if (logicalWeapon == null)
				return;
			foreach (Vector3 vector3 in locationsForPlayer)
			{
				logicalWeapon.AddGameObjectReference();
				this.m_NodeMaws.Add(new NodeMawInfo()
				{
					Weapon = logicalWeapon,
					Range = logicalWeapon.RangeTable.Maximum.Range,
					Pos = vector3
				});
			}
			this.m_NodeMawUpdateRate = 120;
		}

		private void UpdateNodeMaws()
		{
			if (this.m_NodeMaws.Count == 0)
				return;
			this.m_NodeMawUpdateRate -= this.m_FramesElapsed;
			if (this.m_NodeMawUpdateRate > 0)
				return;
			this.m_NodeMawUpdateRate = 90;
			foreach (NodeMawInfo nodeMaw in this.m_NodeMaws)
			{
				bool flag = false;
				List<Ship> shipList1 = new List<Ship>();
				List<Ship> shipList2 = new List<Ship>();
				foreach (Ship ship in this.m_Friendly)
				{
					if (Ship.IsActiveShip(ship) && (double)(ship.Position - nodeMaw.Pos).LengthSquared < (double)nodeMaw.Range * (double)nodeMaw.Range)
					{
						shipList2.Add(ship);
						if (ship.IsSuulka || ship.ShipRole == ShipRole.COMMAND)
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					foreach (Ship ship in this.m_Enemy)
					{
						if (Ship.IsActiveShip(ship) && (double)(ship.Position - nodeMaw.Pos).LengthSquared < (double)nodeMaw.Range * (double)nodeMaw.Range)
							shipList1.Add(ship);
					}
					if (shipList1.Count > 3 && shipList2.Count < 2)
					{
						this.m_Player.PostSetProp("SpawnNodeMaw", (object)nodeMaw.Weapon.GameObject.ObjectID, (object)nodeMaw.Pos.X, (object)nodeMaw.Pos.Y, (object)nodeMaw.Pos.Z, (object)nodeMaw.Rot.X, (object)nodeMaw.Rot.Y, (object)nodeMaw.Rot.Z, (object)nodeMaw.Rot.W);
						nodeMaw.Weapon.ReleaseGameObjectReference();
						this.m_NodeMaws.Remove(nodeMaw);
						break;
					}
				}
			}
		}

		public static bool IsCombatGameObject(IGameObject obj)
		{
			return obj is Ship || obj is StellarBody || obj is StarModel;
		}

		public static List<IGameObject> GetCombatGameObjects(
		  IEnumerable<IGameObject> objects)
		{
			List<IGameObject> objs = objects.Where<IGameObject>((Func<IGameObject, bool>)(x => CombatAI.IsCombatGameObject(x))).ToList<IGameObject>();
			List<IGameObject> gameObjectList = new List<IGameObject>();
			gameObjectList.AddRange((IEnumerable<IGameObject>)objs);
			IGameObject gameObject = objects.FirstOrDefault<IGameObject>((Func<IGameObject, bool>)(x => x is Kerberos.Sots.GameStates.StarSystem));
			if (gameObject != null && gameObject is Kerberos.Sots.GameStates.StarSystem)
				gameObjectList.AddRange((IEnumerable<IGameObject>)(gameObject as Kerberos.Sots.GameStates.StarSystem).Crits.Objects.Where<IGameObject>((Func<IGameObject, bool>)(x => !objs.Contains(x))).ToList<IGameObject>());
			return gameObjectList;
		}
	}
}
