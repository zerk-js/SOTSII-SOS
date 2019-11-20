// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.EvadeEnemyObjective
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using System.Collections.Generic;

namespace Kerberos.Sots.Combat
{
	internal class EvadeEnemyObjective : TacticalObjective
	{
		public List<EnemyGroup> m_ThreatsInRange;
		private CombatAI m_CommandAI;
		private PatrolObjective m_PatrolObjective;
		private float m_ShipSensorRange;
		private bool m_IsUnsafe;

		public bool IsUnsafe
		{
			get
			{
				return this.m_IsUnsafe;
			}
		}

		public PatrolObjective EvadePatrolObjective
		{
			get
			{
				return this.m_PatrolObjective;
			}
		}

		public EvadeEnemyObjective(
		  CombatAI command,
		  PatrolObjective patrolObjective,
		  float sensorRange)
		{
			this.m_ObjectiveType = ObjectiveType.EVADE_ENEMY;
			this.m_CommandAI = command;
			this.m_TaskGroups = new List<TaskGroup>();
			this.m_ThreatsInRange = new List<EnemyGroup>();
			this.m_RequestTaskGroup = false;
			this.m_IsUnsafe = false;
			this.m_PatrolObjective = patrolObjective;
			this.m_ShipSensorRange = sensorRange;
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
			return this.m_PatrolObjective.GetObjectiveLocation();
		}

		public override bool IsComplete()
		{
			return false;
		}

		public override void Update()
		{
			int num = 0;
			this.m_ThreatsInRange.Clear();
			foreach (EnemyGroup enemyGroup in this.m_CommandAI.GetEnemyGroups())
			{
				if (enemyGroup.GetClosestShip(this.GetObjectiveLocation(), this.m_ShipSensorRange) != null)
				{
					this.m_ThreatsInRange.Add(enemyGroup);
					num += CombatAI.AssessGroupStrength(enemyGroup.m_Ships);
				}
			}
			this.m_IsUnsafe = num > 50;
		}

		public Vector3 GetSafePatrolDirection(Vector3 currentLocation)
		{
			Vector3 objectiveLocation = this.GetObjectiveLocation();
			Vector3 unitZ = Vector3.UnitZ;
			Vector3 vector3_1;
			if (this.m_ThreatsInRange.Count > 0)
			{
				foreach (EnemyGroup enemyGroup in this.m_ThreatsInRange)
				{
					Vector3 vector3_2 = objectiveLocation - enemyGroup.m_LastKnownPosition;
					unitZ += vector3_2;
				}
				vector3_1 = unitZ / (float)this.m_ThreatsInRange.Count;
			}
			else
				vector3_1 = (double)objectiveLocation.LengthSquared <= 0.0 ? currentLocation : objectiveLocation;
			double num = (double)vector3_1.Normalize();
			return vector3_1;
		}

		public EnemyGroup GetClosestThreat()
		{
			EnemyGroup enemyGroup1 = (EnemyGroup)null;
			float num = float.MaxValue;
			foreach (EnemyGroup enemyGroup2 in this.m_ThreatsInRange)
			{
				float lengthSquared = (enemyGroup2.m_LastKnownPosition - this.m_Planet.Parameters.Position).LengthSquared;
				if ((double)lengthSquared < (double)num)
				{
					num = lengthSquared;
					enemyGroup1 = enemyGroup2;
				}
			}
			return enemyGroup1;
		}

		public override int ResourceNeeds()
		{
			return 1;
		}
	}
}
