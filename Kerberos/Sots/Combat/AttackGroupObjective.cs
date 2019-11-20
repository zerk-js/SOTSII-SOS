// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.AttackGroupObjective
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Combat
{
	internal class AttackGroupObjective : TacticalObjective
	{
		public AttackGroupObjective(EnemyGroup eGroup)
		{
			this.m_ObjectiveType = ObjectiveType.ATTACK_TARGET;
			this.m_TargetEnemyGroup = eGroup;
			this.m_TaskGroups = new List<TaskGroup>();
			this.m_RequestTaskGroup = false;
		}

		public override bool IsComplete()
		{
			return this.m_TargetEnemyGroup == null || this.m_TargetEnemyGroup.m_Ships.Count<Ship>() < 1;
		}

		public override int ResourceNeeds()
		{
			return CombatAI.AssessGroupStrength(this.m_TargetEnemyGroup.m_Ships);
		}

		public override void AssignResources(TaskGroup group)
		{
			base.AssignResources(group);
		}

		public override Vector3 GetObjectiveLocation()
		{
			return this.m_TargetEnemyGroup.m_LastKnownPosition;
		}

		public enum AttackStyle
		{
			StandOff,
			Pursue,
			BroadsidePursue,
			Encircle,
			FlyBy,
			Flank,
		}
	}
}
