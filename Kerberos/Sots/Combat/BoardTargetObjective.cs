// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.BoardTargetObjective
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using System.Collections.Generic;

namespace Kerberos.Sots.Combat
{
	internal class BoardTargetObjective : TacticalObjective
	{
		public BoardTargetObjective(EnemyGroup eGroup)
		{
			this.m_ObjectiveType = ObjectiveType.ATTACK_TARGET;
			this.m_TargetEnemyGroup = eGroup;
			this.m_TaskGroups = new List<TaskGroup>();
			this.m_RequestTaskGroup = false;
		}

		public override bool IsComplete()
		{
			return this.m_TargetEnemyGroup == null || this.m_TargetEnemyGroup.m_Ships.Count == 0;
		}

		public override Vector3 GetObjectiveLocation()
		{
			if (this.m_TargetEnemyGroup != null)
				return this.m_TargetEnemyGroup.m_LastKnownPosition;
			return Vector3.Zero;
		}

		public override int ResourceNeeds()
		{
			return 1;
		}
	}
}
