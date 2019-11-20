// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.EnemyGroup
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Combat
{
	internal class EnemyGroup
	{
		public List<Ship> m_Ships;
		public Vector3 m_LastKnownPosition;
		public Vector3 m_LastKnownHeading;
		public Vector3 m_LastKnownDestination;

		public EnemyGroup()
		{
			this.m_Ships = new List<Ship>();
			this.m_LastKnownPosition = Vector3.Zero;
			this.m_LastKnownHeading = Vector3.UnitX;
			this.m_LastKnownDestination = Vector3.Zero;
		}

		public void PurgeDestroyed()
		{
			List<Ship> shipList = new List<Ship>();
			foreach (Ship ship in this.m_Ships)
			{
				if (!Ship.IsActiveShip(ship))
					shipList.Add(ship);
			}
			foreach (Ship ship in shipList)
				this.m_Ships.Remove(ship);
		}

		public Vector3 GetAverageVelocity(CombatAI detectingAi)
		{
			Vector3 zero = Vector3.Zero;
			int num = 0;
			foreach (Ship ship in this.m_Ships)
			{
				if (detectingAi.IsShipDetected(ship))
				{
					zero += ship.Maneuvering.Velocity;
					++num;
				}
			}
			if (num > 0)
			{
				zero /= (float)num;
				this.m_LastKnownHeading = zero;
			}
			return zero;
		}

		public bool IsDetected(CombatAI detectingAi)
		{
			foreach (Ship ship in this.m_Ships)
			{
				if (detectingAi.IsShipDetected(ship))
					return true;
			}
			return false;
		}

		public Ship GetClosestShip(Vector3 fromCoords, CombatAI detectingAi)
		{
			Ship ship1 = (Ship)null;
			float num = 0.0f;
			foreach (Ship ship2 in this.m_Ships)
			{
				if (detectingAi.IsShipDetected(ship2))
				{
					float lengthSquared = (fromCoords - ship2.Maneuvering.Position).LengthSquared;
					if (ship1 == null || (double)lengthSquared < (double)num)
					{
						ship1 = ship2;
						num = lengthSquared;
					}
				}
			}
			return ship1;
		}

		public Ship GetClosestShip(Vector3 fromCoords, float range)
		{
			Ship ship1 = (Ship)null;
			float num = range * range;
			foreach (Ship ship2 in this.m_Ships)
			{
				float lengthSquared = (fromCoords - ship2.Maneuvering.Position).LengthSquared;
				if ((double)lengthSquared < (double)num)
				{
					ship1 = ship2;
					num = lengthSquared;
				}
			}
			return ship1;
		}

		public float GetGroupRadius()
		{
			if (this.m_Ships.Count == 0)
				return 0.0f;
			Vector3 zero = Vector3.Zero;
			foreach (Ship ship in this.m_Ships)
				zero += ship.Maneuvering.Position;
			Vector3 vector3 = zero / (float)this.m_Ships.Count;
			float val2 = 0.0f;
			foreach (Ship ship in this.m_Ships)
				val2 = Math.Max((vector3 - ship.Maneuvering.Position).LengthSquared, val2);
			return (float)Math.Sqrt((double)val2);
		}

		public bool IsFreighterEnemyGroup()
		{
			return this.m_Ships.Any<Ship>((Func<Ship, bool>)(x => x.IsNPCFreighter));
		}

		public bool IsEncounterEnemyGroup(CombatAI ai)
		{
			return this.m_Ships.Any<Ship>((Func<Ship, bool>)(x => ai.IsEncounterPlayer(x.Player.ID)));
		}

		public bool IsHigherPriorityThan(EnemyGroup eg, CombatAI ai, bool defendObjAsking = false)
		{
			if (eg == null)
				return true;
			EnemyGroupData enemyGroupData1 = EnemyGroup.GetEnemyGroupData(ai, this, ai.PlanetsInSystem);
			EnemyGroupData enemyGroupData2 = EnemyGroup.GetEnemyGroupData(ai, eg, ai.PlanetsInSystem);
			if (ai.GetAIType() == OverallAIType.PIRATE)
			{
				if (enemyGroupData1.IsFreighter || enemyGroupData2.IsFreighter)
				{
					if (enemyGroupData1.IsFreighter && !enemyGroupData2.IsFreighter)
						return true;
					if (!enemyGroupData1.IsFreighter && enemyGroupData2.IsFreighter)
						return false;
					float val2_1 = float.MaxValue;
					float val2_2 = float.MaxValue;
					bool flag = false;
					foreach (TaskGroup taskGroup in ai.GetTaskGroups())
					{
						Vector3 baseGroupPosition = taskGroup.GetBaseGroupPosition();
						Ship closestShip1 = this.GetClosestShip(baseGroupPosition, 100000f);
						Ship closestShip2 = eg.GetClosestShip(baseGroupPosition, 100000f);
						if (closestShip1 != null && closestShip2 != null)
						{
							val2_1 = Math.Min((closestShip1.Position - baseGroupPosition).LengthSquared, val2_1);
							val2_2 = Math.Min((closestShip2.Position - baseGroupPosition).LengthSquared, val2_2);
							flag = true;
						}
					}
					if (flag)
						return (double)val2_1 < (double)val2_2;
				}
				return CombatAI.AssessGroupStrength(this.m_Ships) > CombatAI.AssessGroupStrength(eg.m_Ships);
			}
			if (enemyGroupData1.IsEncounter || enemyGroupData2.IsEncounter)
			{
				if (enemyGroupData1.IsEncounter && !enemyGroupData2.IsEncounter)
					return true;
				if (enemyGroupData1.IsEncounter)
					return !enemyGroupData1.IsStation;
				return false;
			}
			if (ai.OwnsSystem)
			{
				if (enemyGroupData1.IsAttackingPlanetOrStation || enemyGroupData2.IsAttackingPlanetOrStation || defendObjAsking)
				{
					if (enemyGroupData1.IsAttackingPlanetOrStation && !enemyGroupData2.IsAttackingPlanetOrStation)
						return true;
					if (!enemyGroupData1.IsAttackingPlanetOrStation && enemyGroupData2.IsAttackingPlanetOrStation)
						return false;
					if ((double)enemyGroupData1.DistanceFromColony > 0.0 && (double)enemyGroupData2.DistanceFromColony > 0.0)
						return (double)enemyGroupData1.DistanceFromColony < (double)enemyGroupData2.DistanceFromColony;
				}
				if (enemyGroupData1.NumAggressive > 0 || enemyGroupData2.NumAggressive > 0)
					return enemyGroupData1.NumAggressive > enemyGroupData2.NumAggressive;
				if (enemyGroupData1.NumPassive > 0 || enemyGroupData2.NumPassive > 0)
					return enemyGroupData1.NumPassive > enemyGroupData2.NumPassive;
				if (enemyGroupData1.NumCivilian > 0 || enemyGroupData2.NumCivilian > 0)
					return enemyGroupData1.NumCivilian > enemyGroupData2.NumCivilian;
				if (enemyGroupData1.NumUnarmed > 0 || enemyGroupData2.NumUnarmed > 0)
					return enemyGroupData1.NumUnarmed > enemyGroupData2.NumUnarmed;
			}
			else
			{
				if (enemyGroupData1.IsStation || enemyGroupData2.IsStation)
				{
					if (enemyGroupData1.IsAttackingPlanetOrStation && !enemyGroupData2.IsAttackingPlanetOrStation)
						return true;
					if (!enemyGroupData1.IsAttackingPlanetOrStation && enemyGroupData2.IsAttackingPlanetOrStation)
						return false;
					if ((double)enemyGroupData1.DistanceFromColony > 0.0 && (double)enemyGroupData2.DistanceFromColony > 0.0)
						return (double)enemyGroupData1.DistanceFromColony < (double)enemyGroupData2.DistanceFromColony;
				}
				if (enemyGroupData1.NumAggressive > 0 || enemyGroupData2.NumAggressive > 0)
					return enemyGroupData1.NumAggressive > enemyGroupData2.NumAggressive;
				if (enemyGroupData1.NumPassive > 0 || enemyGroupData2.NumPassive > 0)
					return enemyGroupData1.NumPassive > enemyGroupData2.NumPassive;
				if (enemyGroupData1.NumCivilian > 0 || enemyGroupData2.NumCivilian > 0)
					return enemyGroupData1.NumCivilian > enemyGroupData2.NumCivilian;
				if (enemyGroupData1.NumUnarmed > 0 || enemyGroupData2.NumUnarmed > 0)
					return enemyGroupData1.NumUnarmed > enemyGroupData2.NumUnarmed;
			}
			return CombatAI.AssessGroupStrength(this.m_Ships) > CombatAI.AssessGroupStrength(eg.m_Ships);
		}

		public static EnemyGroupData GetEnemyGroupData(
		  CombatAI ai,
		  EnemyGroup eg,
		  List<StellarBody> planets)
		{
			EnemyGroupData enemyGroupData = new EnemyGroupData();
			enemyGroupData.DistanceFromColony = 0.0f;
			enemyGroupData.NumAggressive = 0;
			enemyGroupData.NumCivilian = 0;
			enemyGroupData.NumPassive = 0;
			enemyGroupData.NumUnarmed = 0;
			enemyGroupData.IsAttackingPlanetOrStation = false;
			enemyGroupData.IsEncounter = false;
			enemyGroupData.IsFreighter = false;
			enemyGroupData.IsStation = false;
			foreach (Ship ship in eg.m_Ships)
			{
				if (ship.ShipRole == ShipRole.FREIGHTER)
					enemyGroupData.IsFreighter = true;
				if (TaskGroup.IsValidTaskGroupShip(ship))
				{
					switch (TaskGroup.GetTaskTypeFromShip(ship))
					{
						case TaskGroupType.Aggressive:
						case TaskGroupType.Police:
						case TaskGroupType.BoardingGroup:
						case TaskGroupType.FollowGroup:
						case TaskGroupType.PlanetAssault:
							++enemyGroupData.NumAggressive;
							break;
						case TaskGroupType.Passive:
							++enemyGroupData.NumPassive;
							break;
						case TaskGroupType.Civilian:
							++enemyGroupData.NumCivilian;
							break;
						case TaskGroupType.UnArmed:
							++enemyGroupData.NumUnarmed;
							break;
					}
				}
				else if (ship.ShipClass == ShipClass.Station)
					enemyGroupData.IsStation = true;
				if (ship.Target != null && !enemyGroupData.IsAttackingPlanetOrStation)
				{
					if (ship.Target is Ship)
					{
						Ship target = ship.Target as Ship;
						enemyGroupData.IsAttackingPlanetOrStation = target.Player == ai.m_Player && target.ShipClass == ShipClass.Station;
					}
					else if (ship.Target is StellarBody && (ship.Target as StellarBody).Parameters.ColonyPlayerID == ai.m_Player.ID)
						enemyGroupData.IsAttackingPlanetOrStation = true;
				}
				enemyGroupData.IsEncounter = enemyGroupData.IsEncounter || ai.IsEncounterPlayer(ship.Player.ID);
				foreach (StellarBody planet in planets)
				{
					if (planet.Parameters.ColonyPlayerID == ai.m_Player.ID)
					{
						float lengthSquared = (planet.Parameters.Position - ship.Position).LengthSquared;
						if ((double)enemyGroupData.DistanceFromColony == 0.0 || (double)lengthSquared < (double)enemyGroupData.DistanceFromColony)
							enemyGroupData.DistanceFromColony = lengthSquared;
					}
				}
			}
			if ((double)enemyGroupData.DistanceFromColony > 0.0)
				enemyGroupData.DistanceFromColony = (float)Math.Sqrt((double)enemyGroupData.DistanceFromColony);
			return enemyGroupData;
		}
	}
}
