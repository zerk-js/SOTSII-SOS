// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.TaskGroup
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Combat
{
	internal class TaskGroup
	{
		public static int ABS_MIN_AGGRESSORS_PER_GROUP = 3;
		public static int DESIRED_MIN_AGGRESSORS_PER_GROUP = 5;
		public static int NUM_SHIPS_PER_SCOUT = 5;
		public static float ATTACK_GROUP_RANGE = 3000f;
		private const float MIN_FOR_ALT_ATTACK_MODES = 5f;
		private const float THREAT_RANGE = 6000f;
		private const int AVE_ATTACK_CHANGE_RATE = 4800;
		private const int AVE_ATTACK_CHANGE_RATE_DEV = 1200;
		private App m_Game;
		private CombatAI m_CommanderAI;
		private List<Ship> m_Ships;
		private List<TaskGroupShipControl> m_ShipControls;
		private int[] m_NumShipTypes;
		private int m_CurrentChangeAttackDelay;
		private int m_ChangeAttackTime;
		private bool m_RequestRefreshShipControls;
		private float m_GroupSpeed;
		private TacticalObjective m_Objective;
		public TaskGroupOrders m_Orders;
		public bool m_bIsPlayerOrder;
		private EnemyGroup m_EnemyGroupInContact;
		private bool m_bIsInContactWithEnemy;
		private ObjectiveType m_RequestedObjectiveType;
		private TaskGroupType m_Type;
		public PatrolType m_PatrolType;
		public List<Ship> m_Targets;
		public EnemyGroup m_TargetGroup;
		public TaskGroup m_TargetTaskGroup;

		public CombatAI Commander
		{
			get
			{
				return this.m_CommanderAI;
			}
		}

		public float GroupSpeed
		{
			get
			{
				return this.m_GroupSpeed;
			}
		}

		public TacticalObjective Objective
		{
			get
			{
				return this.m_Objective;
			}
			set
			{
				if (value == this.m_Objective)
					return;
				if (this.m_Objective != null)
					this.m_Objective.RemoveTaskGroup(this);
				this.m_Objective = value;
				if (this.m_Objective != null)
					this.m_Objective.AssignResources(this);
				this.m_RequestedObjectiveType = ObjectiveType.NO_OBJECTIVE;
				this.m_RequestRefreshShipControls = true;
			}
		}

		public EnemyGroup EnemyGroupInContact
		{
			get
			{
				return this.m_EnemyGroupInContact;
			}
		}

		public void ClearEnemyGroupInContact()
		{
			this.m_EnemyGroupInContact = (EnemyGroup)null;
			this.m_bIsInContactWithEnemy = false;
		}

		public bool IsInContactWithEnemy
		{
			get
			{
				return this.m_bIsInContactWithEnemy;
			}
		}

		public TaskGroupType Type
		{
			get
			{
				return this.m_Type;
			}
			set
			{
				this.m_Type = value;
			}
		}

		public TaskGroup(App game, CombatAI commandAI)
		{
			this.m_Game = game;
			this.m_CommanderAI = commandAI;
			this.m_Objective = (TacticalObjective)null;
			this.m_Orders = TaskGroupOrders.None;
			this.m_Type = TaskGroupType.Aggressive;
			this.m_bIsPlayerOrder = false;
			this.m_bIsInContactWithEnemy = false;
			this.m_EnemyGroupInContact = (EnemyGroup)null;
			this.m_Ships = new List<Ship>();
			this.m_Targets = new List<Ship>();
			this.m_ShipControls = new List<TaskGroupShipControl>();
			this.m_TargetGroup = (EnemyGroup)null;
			this.m_TargetTaskGroup = (TaskGroup)null;
			this.m_PatrolType = PatrolType.Circular;
			this.m_RequestedObjectiveType = ObjectiveType.NO_OBJECTIVE;
			this.m_NumShipTypes = new int[14];
			for (int index = 0; index < 14; ++index)
				this.m_NumShipTypes[index] = 0;
			this.m_GroupSpeed = 0.0f;
			this.m_RequestRefreshShipControls = false;
			this.m_ChangeAttackTime = 4800;
			this.m_CurrentChangeAttackDelay = this.m_ChangeAttackTime;
		}

		public void ShutDown()
		{
			if (this.m_Objective != null)
				this.m_Objective.RemoveTaskGroup(this);
			foreach (TaskGroupShipControl shipControl in this.m_ShipControls)
				shipControl.ShutDown();
		}

		public void AddShips(List<Ship> ships)
		{
			foreach (Ship ship in ships)
				this.AddShip(ship);
		}

		public ObjectiveType GetRequestedObjectiveType()
		{
			return this.m_RequestedObjectiveType;
		}

		public Vector3 GetBaseGroupPosition()
		{
			Vector3 zero = Vector3.Zero;
			if (this.m_Ships.Count == 0)
				return zero;
			if (this.m_ShipControls.Count == 0)
			{
				foreach (Ship ship in this.m_Ships)
					zero += ship.Maneuvering.Position;
				return zero / (float)this.m_Ships.Count;
			}
			foreach (TaskGroupShipControl shipControl in this.m_ShipControls)
				zero += shipControl.GetCurrentPosition();
			return zero / (float)this.m_ShipControls.Count;
		}

		public static TaskGroupType GetTaskTypeFromShip(Ship ship)
		{
			if (ship.IsSuulka)
				return TaskGroupType.Aggressive;
			if (!ship.IsCarrier && ship.WeaponBanks.ToList<WeaponBank>().Count == 0)
				return TaskGroupType.UnArmed;
			if (ship.IsPlanetAssaultShip)
				return TaskGroupType.PlanetAssault;
			switch (ship.ShipRole)
			{
				case ShipRole.COMMAND:
					return !Ship.IsShipClassBigger(ship.ShipClass, ShipClass.Dreadnought, true) ? TaskGroupType.Civilian : TaskGroupType.Aggressive;
				case ShipRole.COLONIZER:
				case ShipRole.CONSTRUCTOR:
				case ShipRole.SUPPLY:
				case ShipRole.GATE:
					return TaskGroupType.Civilian;
				case ShipRole.SCOUT:
				case ShipRole.BORE:
					return TaskGroupType.Passive;
				case ShipRole.FREIGHTER:
					return !ship.IsNPCFreighter ? TaskGroupType.Passive : TaskGroupType.Freighter;
				default:
					return TaskGroupType.Aggressive;
			}
		}

		public void UpdateTaskGroupType()
		{
			int num1 = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			int num7 = 0;
			foreach (Ship ship in this.m_Ships)
			{
				if (ship.IsPolice)
				{
					++num1;
				}
				else
				{
					switch (TaskGroup.GetTaskTypeFromShip(ship))
					{
						case TaskGroupType.Passive:
							++num4;
							continue;
						case TaskGroupType.Civilian:
							++num5;
							continue;
						case TaskGroupType.Freighter:
							++num2;
							continue;
						case TaskGroupType.UnArmed:
							++num6;
							continue;
						case TaskGroupType.PlanetAssault:
							++num7;
							continue;
						default:
							++num3;
							continue;
					}
				}
			}
			this.m_Type = num1 <= 0 ? (num2 <= 0 ? (num6 <= 0 ? (!this.m_CommanderAI.IsEncounterCombat ? (num7 <= 0 ? (this.m_Ships.Count != 2 || num3 == this.m_Ships.Count || (num5 == this.m_Ships.Count || num4 == this.m_Ships.Count) ? (num5 <= num4 || num5 <= num3 ? (num3 <= num4 || num3 <= num5 ? TaskGroupType.Passive : (num5 <= num4 || num5 <= num3 / 5 ? TaskGroupType.Aggressive : TaskGroupType.Passive)) : (num3 > num4 ? TaskGroupType.Passive : TaskGroupType.Civilian)) : (num5 > 0 ? TaskGroupType.Passive : TaskGroupType.Aggressive)) : TaskGroupType.PlanetAssault) : TaskGroupType.Aggressive) : TaskGroupType.UnArmed) : TaskGroupType.Freighter) : TaskGroupType.Police;
			if (this.m_Type == TaskGroupType.Passive && (this.m_CommanderAI.GetAIType() == OverallAIType.PIRATE || this.m_CommanderAI.GetAIType() == OverallAIType.SLAVER))
				this.m_Type = TaskGroupType.Aggressive;
			if (this.m_Type == TaskGroupType.Civilian || this.m_Type == TaskGroupType.Freighter || this.m_Type == TaskGroupType.Police)
				this.m_PatrolType = PatrolType.Orbit;
			else
				this.m_PatrolType = PatrolType.Circular;
		}

		public void UpdateGroupSpeed()
		{
			this.m_GroupSpeed = 0.0f;
			if (this.m_Ships.Count <= 0)
				return;
			this.m_GroupSpeed = this.m_Ships.Average<Ship>((Func<Ship, float>)(x => x.Maneuvering.MaxShipSpeed));
		}

		public static bool IsValidTaskGroupShip(Ship ship)
		{
			if (!Ship.IsActiveShip(ship) || ship.IsDriveless || (Ship.IsStationSize(ship.RealShipClass) || Ship.IsBattleRiderSize(ship.RealShipClass)) || (ship.IsAcceleratorHoop || ship.CombatStance == CombatStance.RETREAT || ship.AssaultingPlanet))
				return false;
			if (ship.IsSystemDefenceBoat)
				return ship.DefenseBoatActive;
			return true;
		}

		public bool IsDesiredGroupTargetTaken(TaskGroupShipControl group, Ship desiredTarget)
		{
			int num = CombatAI.AssessGroupStrength(group.GetShips());
			foreach (TaskGroupShipControl shipControl in this.m_ShipControls)
			{
				if (shipControl != group && shipControl.GroupPriorityTarget == desiredTarget)
				{
					if (CombatAI.AssessGroupStrength(shipControl.GetShips()) >= num)
						return true;
					break;
				}
			}
			return false;
		}

		public void AddShip(Ship ship)
		{
			if (ship.TaskGroup != null)
				ship.TaskGroup.RemoveShip(ship);
			this.m_Ships.Add(ship);
			ship.TaskGroup = this;
			++this.m_NumShipTypes[(int)ship.RealShipClass];
			this.UpdateTaskGroupType();
			this.UpdateGroupSpeed();
			if (ship.IsSuulka)
				ship.Maneuvering.TargetFacingAngle = TargetFacingAngle.BroadSide;
			this.m_RequestRefreshShipControls = true;
		}

		public void RemoveShip(Ship ship)
		{
			if (!this.m_Ships.Contains(ship))
				return;
			ship.TaskGroup = (TaskGroup)null;
			if (this.m_Ships.Contains(ship))
			{
				--this.m_NumShipTypes[(int)ship.RealShipClass];
				this.m_Ships.Remove(ship);
			}
			foreach (TaskGroupShipControl shipControl in this.m_ShipControls)
				shipControl.RemoveShip(ship);
			this.UpdateTaskGroupType();
			this.UpdateGroupSpeed();
			this.m_RequestRefreshShipControls = true;
		}

		public void ObjectRemoved(Ship ship)
		{
			this.m_Targets.Remove(ship);
			this.RemoveShip(ship);
			foreach (TaskGroupShipControl shipControl in this.m_ShipControls)
			{
				if (shipControl.GroupPriorityTarget == ship)
					shipControl.ClearPriorityTarget();
			}
		}

		public bool ContainsShip(Ship ship)
		{
			return this.m_Ships.Contains(ship);
		}

		public int GetShipCount()
		{
			return this.m_Ships.Count;
		}

		public List<Ship> GetShips()
		{
			return this.m_Ships;
		}

		public void NotifyAllRidersDeployed(Ship carrier)
		{
		}

		public void NotifyAllRidersDocked(Ship carrier)
		{
			if (this.m_CommanderAI.GetAIType() != OverallAIType.SLAVER)
				return;
			if (carrier.WeaponBanks.Any<WeaponBank>((Func<WeaponBank, bool>)(x => x.TurretClass == WeaponEnums.TurretClasses.AssaultShuttle)))
			{
				carrier.SetCombatStance(CombatStance.RETREAT);
				foreach (SpecWeaponControl specWeaponControl in carrier.WeaponControls.OfType<AssaultShuttleLaunchControl>().ToList<AssaultShuttleLaunchControl>())
					specWeaponControl.DisableWeaponFire = true;
				carrier.TaskGroup = (TaskGroup)null;
			}
			if (this.m_Ships.Any<Ship>((Func<Ship, bool>)(x => x.WeaponBanks.Any<WeaponBank>((Func<WeaponBank, bool>)(y => y.TurretClass == WeaponEnums.TurretClasses.AssaultShuttle)))))
				return;
			this.m_Objective = (TacticalObjective)null;
			this.m_RequestedObjectiveType = ObjectiveType.RETREAT;
		}

		public void Update(int framesElapsed)
		{
			if (this.m_Objective == null)
				return;
			if (!this.m_RequestRefreshShipControls && this.TaskGroupCanMerge())
			{
				Vector3 currentPosition = this.m_ShipControls[0].GetCurrentPosition();
				foreach (TaskGroupShipControl shipControl in this.m_ShipControls)
				{
					if (shipControl != this.m_ShipControls[0] && shipControl.CanMerge && (double)(currentPosition - shipControl.GetCurrentPosition()).LengthSquared < (double)TaskGroup.ATTACK_GROUP_RANGE * (double)TaskGroup.ATTACK_GROUP_RANGE)
					{
						this.m_RequestRefreshShipControls = true;
						break;
					}
				}
			}
			if (this.m_Objective is AttackGroupObjective && (double)this.m_Ships.Count > 5.0 && (this.m_ShipControls.Count > 0 && this.m_Objective.m_TargetEnemyGroup != null))
			{
				Vector3 currentPosition = this.m_ShipControls[0].GetCurrentPosition();
				Vector3 lastKnownHeading = this.m_Objective.m_TargetEnemyGroup.m_LastKnownHeading;
				if (this.m_Objective.m_TargetEnemyGroup.GetClosestShip(currentPosition, TaskGroup.ATTACK_GROUP_RANGE) != null && (double)lastKnownHeading.LengthSquared < 100.0)
					this.m_CurrentChangeAttackDelay -= framesElapsed;
				else
					this.m_CurrentChangeAttackDelay = this.m_ChangeAttackTime;
				if ((double)this.m_CurrentChangeAttackDelay <= 0.0)
				{
					this.m_RequestRefreshShipControls = true;
					this.m_ChangeAttackTime = this.m_CommanderAI.AIRandom.NextInclusive(3600, 6000);
					this.m_CurrentChangeAttackDelay = this.m_ChangeAttackTime;
				}
			}
			if (this.m_RequestRefreshShipControls)
				this.RefreshShipControls();
			if (this.m_ShipControls.Count == 0)
				this.AssignShipsToObjective();
			List<TaskGroupShipControl> groupShipControlList = new List<TaskGroupShipControl>();
			foreach (TaskGroupShipControl shipControl in this.m_ShipControls)
			{
				if (shipControl.GetShipCount() > 0)
					shipControl.Update(framesElapsed);
				else
					groupShipControlList.Add(shipControl);
			}
			foreach (TaskGroupShipControl groupShipControl in groupShipControlList)
			{
				groupShipControl.ShutDown();
				this.m_ShipControls.Remove(groupShipControl);
			}
			if (this.m_Type == TaskGroupType.BoardingGroup && !this.m_Ships.Any<Ship>((Func<Ship, bool>)(x => x.WeaponControls.Any<SpecWeaponControl>((Func<SpecWeaponControl, bool>)(y => y is BoardingPodLaunchControl)))))
			{
				this.Objective = (TacticalObjective)null;
				this.UpdateTaskGroupType();
			}
			if (this.m_Type != TaskGroupType.Civilian && this.m_Type != TaskGroupType.UnArmed || !(this.m_Objective is EvadeEnemyObjective))
				return;
			if ((this.m_Type != TaskGroupType.Civilian ? this.m_CommanderAI.GetTaskGroups().Where<TaskGroup>((Func<TaskGroup, bool>)(x =>
		   {
			   if (x.Type != TaskGroupType.Aggressive && x.Type != TaskGroupType.Passive)
				   return x.Type == TaskGroupType.Civilian;
			   return true;
		   })).Count<TaskGroup>() : this.m_CommanderAI.GetTaskGroups().Where<TaskGroup>((Func<TaskGroup, bool>)(x =>
	 {
			   if (x.Type != TaskGroupType.Aggressive)
				   return x.Type == TaskGroupType.Passive;
			   return true;
		   })).Count<TaskGroup>()) != 0)
				return;
			this.Objective = (TacticalObjective)null;
		}

		private bool TaskGroupCanMerge()
		{
			if (this.m_ShipControls.Count <= 1 || this.m_Type == TaskGroupType.Freighter)
				return false;
			if (!(this.m_Objective is AttackGroupObjective))
				return this.m_Objective is AttackPlanetObjective;
			return true;
		}

		private void RefreshShipControls()
		{
			foreach (TaskGroupShipControl shipControl in this.m_ShipControls)
				shipControl.ShutDown();
			this.m_ShipControls.Clear();
			this.m_RequestRefreshShipControls = false;
		}

		private void AssignShipsToObjective()
		{
			if (this.m_Ships.Count == 0)
				return;
			if (this.m_Objective is ScoutObjective)
			{
				Ship bestScout = CombatAI.FindBestScout(this.m_Ships);
				if (bestScout == null)
				{
					this.m_RequestedObjectiveType = ObjectiveType.PATROL;
				}
				else
				{
					ScoutShipControl scoutShip = new ScoutShipControl(this.m_Game, this.m_Objective, this.m_CommanderAI, bestScout.SensorRange * 0.75f);
					scoutShip.AddShip(bestScout, false);
					if (this.m_Type == TaskGroupType.Civilian)
					{
						foreach (Ship ship in this.m_Ships)
						{
							if (ship != bestScout)
								scoutShip.AddShip(ship, false);
						}
						this.m_ShipControls.Add((TaskGroupShipControl)scoutShip);
					}
					else
					{
						ScoutTrailShipControl trailShipControl = new ScoutTrailShipControl(this.m_Game, this.m_Objective, this.m_CommanderAI, scoutShip);
						foreach (Ship ship in this.m_Ships)
						{
							if (ship != bestScout)
								trailShipControl.AddShip(ship, false);
						}
						this.m_ShipControls.Add((TaskGroupShipControl)scoutShip);
						this.m_ShipControls.Add((TaskGroupShipControl)trailShipControl);
					}
				}
			}
			else if (this.m_Objective is PatrolObjective)
			{
				PatrolShipControl patrolShipControl;
				if (this.m_Type == TaskGroupType.Police)
				{
					patrolShipControl = new PatrolShipControl(this.m_Game, this.m_Objective, this.m_CommanderAI, this.m_PatrolType, Vector3.UnitZ, this.m_Game.AssetDatabase.PolicePatrolRadius, this.m_CommanderAI.AIRandom.CoinToss(0.5));
				}
				else
				{
					Vector3 dir = -this.m_Objective.GetObjectiveLocation();
					if ((double)dir.LengthSquared > 0.0)
					{
						double num = (double)dir.Normalize();
					}
					else
						dir = -Vector3.UnitZ;
					bool clockwise = this.m_Type != TaskGroupType.Freighter && this.m_CommanderAI.AIRandom.CoinToss(0.5);
					patrolShipControl = new PatrolShipControl(this.m_Game, this.m_Objective, this.m_CommanderAI, this.m_PatrolType, dir, 10000f, clockwise);
				}
				foreach (Ship ship in this.m_Ships)
					patrolShipControl.AddShip(ship, false);
				this.m_ShipControls.Add((TaskGroupShipControl)patrolShipControl);
			}
			else if (this.m_Objective is AttackGroupObjective)
				this.AssignShipsToAttackObjective();
			else if (this.m_Objective is AttackPlanetObjective)
			{
				AttackPlanetShipControl planetShipControl = new AttackPlanetShipControl(this.m_Game, this.m_Objective, this.m_CommanderAI);
				foreach (Ship ship in this.m_Ships)
					planetShipControl.AddShip(ship, false);
				this.m_ShipControls.Add((TaskGroupShipControl)planetShipControl);
			}
			else if (this.m_Objective is DefendPlanetObjective)
				this.AssignShipsToDefendObjective();
			else if (this.m_Objective is EvadeEnemyObjective)
			{
				EvadeEnemyObjective objective = this.m_Objective as EvadeEnemyObjective;
				if (objective.EvadePatrolObjective != null)
				{
					Vector3 safePatrolDirection = objective.GetSafePatrolDirection(this.GetBaseGroupPosition());
					PatrolShipControl patrolShipControl = new PatrolShipControl(this.m_Game, (TacticalObjective)objective.EvadePatrolObjective, this.m_CommanderAI, PatrolType.Circular, safePatrolDirection, 10000f, this.m_CommanderAI.AIRandom.CoinToss(0.5));
					foreach (Ship ship in this.m_Ships)
						patrolShipControl.AddShip(ship, false);
					this.m_ShipControls.Add((TaskGroupShipControl)patrolShipControl);
				}
				else
					this.m_RequestedObjectiveType = ObjectiveType.PATROL;
			}
			else if (this.m_Objective is RetreatObjective)
			{
				RetreatShipControl retreatShipControl = new RetreatShipControl(this.m_Game, this.m_Objective, this.m_CommanderAI, this.m_Type != TaskGroupType.Freighter);
				foreach (Ship ship in this.m_Ships)
					retreatShipControl.AddShip(ship, false);
				this.m_ShipControls.Add((TaskGroupShipControl)retreatShipControl);
			}
			else if (this.m_Objective is BoardTargetObjective)
			{
				float val1 = float.MaxValue;
				foreach (Ship ship in this.m_Ships)
					val1 = Math.Min(val1, CombatAI.GetAveEffectiveWeaponRange(ship, false));
				PursueShipControl pursueShipControl = new PursueShipControl(this.m_Game, this.m_Objective, this.m_CommanderAI, val1 * 0.75f);
				foreach (Ship ship in this.m_Ships)
					pursueShipControl.AddShip(ship, false);
				this.m_ShipControls.Add((TaskGroupShipControl)pursueShipControl);
			}
			else
			{
				if (!(this.m_Objective is FollowTaskGroupObjective))
					return;
				SupportGroupShipControl groupShipControl = new SupportGroupShipControl(this.m_Game, this.m_Objective, this.m_CommanderAI, (TaskGroupShipControl)null);
				foreach (Ship ship in this.m_Ships)
					groupShipControl.AddShip(ship, false);
				this.m_ShipControls.Add((TaskGroupShipControl)groupShipControl);
			}
		}

		private void AssignShipsToDefendObjective()
		{
			DefendPlanetObjective objective = this.m_Objective as DefendPlanetObjective;
			if (objective.DefendPatrolObjective != null)
			{
				int index = 0;
				foreach (Ship ship in this.m_Ships)
				{
					if (index < this.m_ShipControls.Count)
					{
						this.m_ShipControls[index].AddShip(ship, false);
					}
					else
					{
						Vector3 patrolDirection = objective.GetPatrolDirection(index);
						PatrolShipControl patrolShipControl = new PatrolShipControl(this.m_Game, (TacticalObjective)objective.DefendPatrolObjective, this.m_CommanderAI, this.m_PatrolType, patrolDirection, 10000f, this.m_CommanderAI.AIRandom.CoinToss(0.5));
						patrolShipControl.AddShip(ship, false);
						this.m_ShipControls.Add((TaskGroupShipControl)patrolShipControl);
					}
					index = (index + 1) % 6;
				}
			}
			else
				this.m_RequestedObjectiveType = ObjectiveType.PATROL;
		}

		private void AssignShipsToAttackObjective()
		{
			if (this.m_Objective != null && this.m_Objective.m_TargetEnemyGroup == null)
				this.m_RequestedObjectiveType = ObjectiveType.PATROL;
			else if (this.m_Type == TaskGroupType.Police || this.m_CommanderAI.GetAIType() == OverallAIType.PIRATE)
			{
				this.m_ShipControls.Add(this.CreatePursueAttackGroup(this.m_Ships));
			}
			else
			{
				Vector3 objectiveLocation = this.m_Objective.GetObjectiveLocation();
				float groupRadius = this.m_Objective.m_TargetEnemyGroup.GetGroupRadius();
				List<Ship> shipList1 = new List<Ship>();
				List<Ship> shipList2 = new List<Ship>();
				int[] numArray = new int[14];
				foreach (Ship ship in this.m_Ships)
				{
					float num = groupRadius + TaskGroup.ATTACK_GROUP_RANGE + ship.SensorRange;
					if ((double)(objectiveLocation - ship.Maneuvering.Position).LengthSquared < (double)num * (double)num)
					{
						shipList1.Add(ship);
						++numArray[(int)ship.RealShipClass];
					}
					else
						shipList2.Add(ship);
				}
				int num1 = CombatAI.AssessGroupStrength(this.m_Objective.m_TargetEnemyGroup.m_Ships);
				int num2 = CombatAI.AssessGroupStrength(shipList1);
				if (num2 > num1 && this.m_Type != TaskGroupType.Civilian)
				{
					List<Ship> list = shipList1.Where<Ship>((Func<Ship, bool>)(x =>
				   {
					   if (x.RealShipClass != RealShipClasses.Leviathan)
						   return x.RealShipClass == RealShipClasses.Dreadnought;
					   return true;
				   })).ToList<Ship>();
					foreach (Ship ship in list)
						shipList1.Remove(ship);
					if (list.Count > 0)
						this.m_ShipControls.Add(this.CreateStandOffAttackGroup(list));
					if (shipList1.Count > 0)
					{
						float num3 = 5f;
						float num4 = Math.Min(Math.Max((float)((double)shipList1.Count / 5.0 - 1.0), 0.0f), 1f) * 5f;
						float num5 = 0.0f;
						if ((double)this.m_CommanderAI.AIRandom.NextInclusive(0.0f, num3 + num4 + num5) <= (double)num3)
						{
							List<Ship> ships = new List<Ship>();
							int maxValue = Math.Max(shipList1.Count - 5, 0);
							if (maxValue > 0 && this.m_Type != TaskGroupType.Civilian)
							{
								for (int index1 = 1; index1 > 0 && maxValue > 0; maxValue = Math.Max(shipList1.Count - 5, 0))
								{
									index1 = this.m_CommanderAI.AIRandom.NextInclusive(0, maxValue);
									for (int index2 = 0; index2 < index1; ++index2)
										ships.Add(shipList1[index2]);
									if (ships.Count != 0)
									{
										foreach (Ship ship in ships)
											shipList1.Remove(ship);
										this.m_ShipControls.Add(this.CreateStandOffAttackGroup(ships));
									}
									else
										break;
								}
							}
							this.m_ShipControls.Add(this.CreateFlyByAttackGroup(shipList1));
						}
						else if (shipList1.Count > 0)
						{
							bool flag = false;
							int num6 = Math.Min(shipList1.Count, 6);
							int val1 = (int)Math.Ceiling((double)shipList1.Count / (double)num6);
							float radians = MathHelper.DegreesToRadians(360f / (float)num6);
							float num7 = (float)-((double)radians * 0.5);
							Vector3 baseGroupPosition = this.GetBaseGroupPosition();
							Vector3 forward = objectiveLocation - baseGroupPosition;
							forward.Y = 0.0f;
							double num8 = (double)forward.Normalize();
							Matrix world = Matrix.CreateWorld(baseGroupPosition, forward, Vector3.UnitY);
							for (int index1 = 0; index1 < num6; ++index1)
							{
								float num9 = index1 % 2 == 0 ? -1f : 1f;
								float num10 = (float)Math.Floor((double)(index1 % num6 + 1) * 0.5);
								Matrix matrix = Matrix.CreateRotationYPR(num9 * radians * num10 + num7, 0.0f, 0.0f) * world;
								List<Ship> ships = new List<Ship>();
								int num11 = shipList1.Count <= num6 - index1 ? 1 : Math.Min(val1, shipList1.Count - 1);
								for (int index2 = 0; index2 < num11; ++index2)
									ships.Add(shipList1[index2]);
								foreach (Ship ship in ships)
									shipList1.Remove(ship);
								if (flag)
									this.m_ShipControls.Add(this.CreateFlankAttackGroups(ships, matrix.Forward));
								else
									this.m_ShipControls.Add(this.CreateSurroundAttackGroups(ships, matrix.Forward));
								if (shipList1.Count == 0)
									break;
							}
						}
					}
				}
				else if (num2 * 2 < num1)
					this.m_ShipControls.Add(this.CreateStandOffAttackGroup(shipList1));
				else
					this.m_ShipControls.Add(this.CreatePursueAttackGroup(shipList1));
				TaskGroupShipControl shipControl1 = this.m_ShipControls[0];
				foreach (Ship ship1 in shipList2)
				{
					bool flag = false;
					foreach (TaskGroupShipControl shipControl2 in this.m_ShipControls)
					{
						if (shipControl2 != shipControl1)
						{
							foreach (Ship ship2 in shipControl2.GetShips())
							{
								if ((double)(ship2.Maneuvering.Position - ship1.Maneuvering.Position).LengthSquared < (double)TaskGroup.ATTACK_GROUP_RANGE * (double)TaskGroup.ATTACK_GROUP_RANGE)
								{
									shipControl2.AddShip(ship1, false);
									flag = true;
									break;
								}
							}
							if (flag)
								break;
						}
					}
					if (!flag)
						this.m_ShipControls.Add(this.CreateSupportGroup(new List<Ship>()
			{
			  ship1
			}, this.m_ShipControls[0]));
				}
			}
		}

		private TaskGroupShipControl CreateStandOffAttackGroup(List<Ship> ships)
		{
			float num1 = 0.0f;
			float val1 = float.MaxValue;
			foreach (Ship ship in ships)
			{
				num1 += CombatAI.GetMinEffectiveWeaponRange(ship, false);
				val1 = Math.Min(val1, ship.SensorRange);
			}
			float num2 = num1 / (float)ships.Count;
			float num3 = val1 * 0.75f;
			float minDist = num2 * 0.75f;
			float desiredDist = minDist + 500f;
			if ((double)desiredDist > (double)num3)
			{
				desiredDist = num3;
				minDist = 0.75f * desiredDist;
			}
			else if ((double)minDist >= (double)desiredDist)
				minDist = desiredDist * 0.75f;
			StandOffShipControl standOffShipControl = new StandOffShipControl(this.m_Game, this.m_Objective, this.m_CommanderAI, minDist, desiredDist);
			foreach (Ship ship in ships)
				standOffShipControl.AddShip(ship, (this.m_Type == TaskGroupType.Aggressive || this.m_Type == TaskGroupType.Passive) && TaskGroup.GetTaskTypeFromShip(ship) == TaskGroupType.Civilian);
			return (TaskGroupShipControl)standOffShipControl;
		}

		private TaskGroupShipControl CreatePursueAttackGroup(List<Ship> ships)
		{
			float val1_1 = float.MaxValue;
			float val1_2 = float.MaxValue;
			foreach (Ship ship in ships)
			{
				val1_1 = Math.Min(val1_1, CombatAI.GetAveEffectiveWeaponRange(ship, false));
				val1_2 = Math.Min(val1_2, ship.SensorRange);
			}
			PursueShipControl pursueShipControl = new PursueShipControl(this.m_Game, this.m_Objective, this.m_CommanderAI, Math.Min(val1_1 * 0.75f, val1_2 * 0.75f));
			foreach (Ship ship in ships)
				pursueShipControl.AddShip(ship, (this.m_Type == TaskGroupType.Aggressive || this.m_Type == TaskGroupType.Passive) && TaskGroup.GetTaskTypeFromShip(ship) == TaskGroupType.Civilian);
			return (TaskGroupShipControl)pursueShipControl;
		}

		private TaskGroupShipControl CreateSupportGroup(
		  List<Ship> ships,
		  TaskGroupShipControl supportGroup)
		{
			SupportGroupShipControl groupShipControl = new SupportGroupShipControl(this.m_Game, this.m_Objective, this.m_CommanderAI, supportGroup);
			foreach (Ship ship in ships)
				groupShipControl.AddShip(ship, (this.m_Type == TaskGroupType.Aggressive || this.m_Type == TaskGroupType.Passive) && TaskGroup.GetTaskTypeFromShip(ship) == TaskGroupType.Civilian);
			return (TaskGroupShipControl)groupShipControl;
		}

		private TaskGroupShipControl CreateFlyByAttackGroup(List<Ship> ships)
		{
			float num = 0.0f;
			foreach (Ship ship in ships)
				num += CombatAI.GetMinEffectiveWeaponRange(ship, false);
			FlyByShipControl flyByShipControl = new FlyByShipControl(this.m_Game, this.m_Objective, this.m_CommanderAI, Math.Max(num / (float)ships.Count * 0.75f, 1000f));
			foreach (Ship ship in ships)
				flyByShipControl.AddShip(ship, (this.m_Type == TaskGroupType.Aggressive || this.m_Type == TaskGroupType.Passive) && TaskGroup.GetTaskTypeFromShip(ship) == TaskGroupType.Civilian);
			return (TaskGroupShipControl)flyByShipControl;
		}

		private TaskGroupShipControl CreateSurroundAttackGroups(
		  List<Ship> ships,
		  Vector3 attackVec)
		{
			float num = 0.0f;
			foreach (Ship ship in ships)
				num += CombatAI.GetMinEffectiveWeaponRange(ship, false);
			float minDist = num / (float)ships.Count * 0.75f;
			float desiredDist = minDist + 500f;
			SurroundShipControl surroundShipControl = new SurroundShipControl(this.m_Game, this.m_Objective, this.m_CommanderAI, attackVec, minDist, desiredDist);
			foreach (Ship ship in ships)
				surroundShipControl.AddShip(ship, (this.m_Type == TaskGroupType.Aggressive || this.m_Type == TaskGroupType.Passive) && TaskGroup.GetTaskTypeFromShip(ship) == TaskGroupType.Civilian);
			return (TaskGroupShipControl)surroundShipControl;
		}

		private TaskGroupShipControl CreateFlankAttackGroups(
		  List<Ship> ships,
		  Vector3 attackVec)
		{
			float num = 0.0f;
			foreach (Ship ship in ships)
				num += CombatAI.GetMinEffectiveWeaponRange(ship, false);
			float minDist = num / (float)ships.Count * 0.75f;
			float desiredDist = minDist + 500f;
			FlankShipControl flankShipControl = new FlankShipControl(this.m_Game, this.m_Objective, this.m_CommanderAI, attackVec, minDist, desiredDist);
			foreach (Ship ship in ships)
				flankShipControl.AddShip(ship, (this.m_Type == TaskGroupType.Aggressive || this.m_Type == TaskGroupType.Passive) && TaskGroup.GetTaskTypeFromShip(ship) == TaskGroupType.Civilian);
			return (TaskGroupShipControl)flankShipControl;
		}

		public bool UpdateEnemyContact()
		{
			this.m_bIsInContactWithEnemy = false;
			if (this.m_Objective is DefendPlanetObjective)
			{
				this.m_EnemyGroupInContact = (this.m_Objective as DefendPlanetObjective).GetClosestThreat();
				this.m_bIsInContactWithEnemy = this.m_EnemyGroupInContact != null;
			}
			if (!this.m_bIsInContactWithEnemy)
			{
				foreach (EnemyGroup enemyGroup in this.m_CommanderAI.GetEnemyGroups())
				{
					foreach (Ship ship in enemyGroup.m_Ships)
					{
						foreach (TaskGroupShipControl shipControl in this.m_ShipControls)
						{
							if ((double)(ship.Maneuvering.Position - shipControl.GetCurrentPosition()).LengthSquared < (double)shipControl.SensorRange * (double)shipControl.SensorRange && (this.m_EnemyGroupInContact == null || this.m_EnemyGroupInContact == enemyGroup || enemyGroup.IsHigherPriorityThan(this.m_EnemyGroupInContact, this.m_CommanderAI, false)))
							{
								if (shipControl is ScoutShipControl)
								{
									(shipControl as ScoutShipControl).NotifyEnemyGroupDetected(enemyGroup);
									if (this.m_ShipControls.Any<TaskGroupShipControl>((Func<TaskGroupShipControl, bool>)(x => x is ScoutTrailShipControl)))
										continue;
								}
								this.m_bIsInContactWithEnemy = true;
								this.m_EnemyGroupInContact = enemyGroup;
							}
						}
					}
				}
			}
			if (this.m_bIsInContactWithEnemy && !(this.m_Objective is RetreatObjective) && (!(this.m_Objective is BoardTargetObjective) && this.m_Type != TaskGroupType.Freighter) && this.m_Type != TaskGroupType.BoardingGroup)
			{
				if (this.m_Objective is AttackGroupObjective || this.m_Objective is AttackPlanetObjective)
				{
					float num = this.m_EnemyGroupInContact.m_Ships.Count > 0 ? this.m_EnemyGroupInContact.m_Ships.Average<Ship>((Func<Ship, float>)(x => x.Maneuvering.MaxShipSpeed)) : 0.0f;
					if ((!(this.m_Objective is AttackPlanetObjective) || this.m_Type != TaskGroupType.PlanetAssault && (double)num <= (double)this.m_GroupSpeed + 5.0) && (this.m_Objective.m_TargetEnemyGroup == null || this.m_Objective.m_TargetEnemyGroup != this.m_EnemyGroupInContact && !this.m_Objective.m_TargetEnemyGroup.IsFreighterEnemyGroup()) && (this.m_Objective.m_TargetEnemyGroup == null || this.m_EnemyGroupInContact.IsHigherPriorityThan(this.m_Objective.m_TargetEnemyGroup, this.m_CommanderAI, false)))
						this.Objective = (TacticalObjective)null;
				}
				else if (this.m_Objective is EvadeEnemyObjective && this.m_CommanderAI.GetTaskGroups().Count<TaskGroup>() > 1)
				{
					Vector3 safePatrolDirection = (this.m_Objective as EvadeEnemyObjective).GetSafePatrolDirection(this.GetBaseGroupPosition());
					foreach (TaskGroupShipControl shipControl in this.m_ShipControls)
					{
						if (shipControl is PatrolShipControl)
						{
							PatrolShipControl patrolShipControl = shipControl as PatrolShipControl;
							if ((double)Vector3.Dot(safePatrolDirection, patrolShipControl.PreviousDir) < 0.800000011920929)
								patrolShipControl.ResetPatrolWaypoints(PatrolType.Circular, safePatrolDirection, 10000f);
						}
					}
				}
				else
					this.m_RequestedObjectiveType = ObjectiveType.ATTACK_TARGET;
			}
			return this.m_bIsInContactWithEnemy;
		}
	}
}
