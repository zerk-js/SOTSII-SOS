// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.DefendPlanetObjective
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.Strategy;
using System.Collections.Generic;

namespace Kerberos.Sots.Combat
{
	internal class DefendPlanetObjective : TacticalObjective
	{
		private const float THREAT_RANGE = 10000f;
		private const float MAX_ATTACKING_THREAT_RANGE = 50000f;
		public List<EnemyGroup> m_ThreatsInRange;
		private CombatAI m_CommandAI;
		private PatrolObjective m_PatrolObjective;

		public PatrolObjective DefendPatrolObjective
		{
			get
			{
				return this.m_PatrolObjective;
			}
		}

		public DefendPlanetObjective(
		  StellarBody planet,
		  CombatAI command,
		  PatrolObjective patrolObjective)
		{
			this.m_ObjectiveType = ObjectiveType.DEFEND_TARGET;
			this.m_Planet = planet;
			this.m_CommandAI = command;
			this.m_TaskGroups = new List<TaskGroup>();
			this.m_ThreatsInRange = new List<EnemyGroup>();
			this.m_RequestTaskGroup = false;
			this.m_PatrolObjective = patrolObjective;
		}

		public override void Shutdown()
		{
			this.m_CommandAI = (CombatAI)null;
			this.m_PatrolObjective = (PatrolObjective)null;
			this.m_ThreatsInRange.Clear();
			base.Shutdown();
		}

		public override Vector3 GetObjectiveLocation()
		{
			return this.m_Planet.Parameters.Position;
		}

		public override bool IsComplete()
		{
			return false;
		}

		public override void Update()
		{
			this.m_RequestTaskGroup = false;
			this.m_ThreatsInRange.Clear();
			Ship ship = (Ship)null;
			if (this.m_Planet.LastAttackingObject is Ship)
			{
				ship = this.m_Planet.LastAttackingObject as Ship;
				if ((double)(this.m_Planet.Parameters.Position - ship.Position).LengthSquared > 2500000000.0)
					ship = (Ship)null;
			}
			foreach (EnemyGroup enemyGroup in this.m_CommandAI.GetEnemyGroups())
			{
				if (enemyGroup.GetClosestShip(this.m_Planet.Parameters.Position, this.m_Planet.Parameters.Radius + 10000f) != null || ship != null && enemyGroup.m_Ships.Contains(ship))
				{
					this.m_ThreatsInRange.Add(enemyGroup);
					this.m_RequestTaskGroup = true;
				}
			}
			if (!(this.m_Planet.LastAttackingObject is Ship))
				return;
			Ship lastAttackingObject = this.m_Planet.LastAttackingObject as Ship;
			if (this.m_CommandAI == null || this.m_CommandAI.GetDiplomacyState(lastAttackingObject.Player.ID) != DiplomacyState.WAR)
				return;
			this.m_CommandAI.FlagAttackingShip(lastAttackingObject);
		}

		public Vector3 GetPatrolDirection(TaskGroup tg)
		{
			int index = 0;
			foreach (TaskGroup taskGroup in this.m_TaskGroups)
			{
				if (taskGroup != tg)
					++index;
				else
					break;
			}
			return this.GetPatrolDirection(index);
		}

		public Vector3 GetPatrolDirection(int index)
		{
			Vector3 unitZ = Vector3.UnitZ;
			float radians = MathHelper.DegreesToRadians(60f);
			int num1 = (index % 6 + 1) / 2;
			float x = (float)((index % 2 == 0 ? 1 : -1) * num1) * radians;
			Vector3 forward = -this.GetObjectiveLocation();
			double num2 = (double)forward.Normalize();
			return (Matrix.CreateRotationYPR(new Vector3(x, 0.0f, 0.0f)) * Matrix.CreateWorld(Vector3.Zero, forward, Vector3.UnitY)).Forward;
		}

		public EnemyGroup GetClosestThreat()
		{
			EnemyGroup eg = (EnemyGroup)null;
			foreach (EnemyGroup enemyGroup in this.m_ThreatsInRange)
			{
				if (eg == null || enemyGroup.IsHigherPriorityThan(eg, this.m_CommandAI, true))
					eg = enemyGroup;
			}
			return eg;
		}

		public override int ResourceNeeds()
		{
			int num = 0;
			foreach (EnemyGroup enemyGroup in this.m_ThreatsInRange)
				num += CombatAI.AssessGroupStrength(enemyGroup.m_Ships);
			return num;
		}
	}
}
