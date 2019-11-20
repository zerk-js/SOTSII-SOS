// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.FollowTaskGroupObjective
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using System.Collections.Generic;

namespace Kerberos.Sots.Combat
{
	internal class FollowTaskGroupObjective : TacticalObjective
	{
		public FollowTaskGroupObjective(TaskGroup tg)
		{
			this.m_ObjectiveType = ObjectiveType.DEFEND_TARGET;
			this.m_TargetTaskGroup = tg;
			this.m_RequestTaskGroup = false;
			this.m_TaskGroups = new List<TaskGroup>();
		}

		public override bool IsComplete()
		{
			return this.m_TargetTaskGroup == null || this.m_TargetTaskGroup.GetShipCount() == 0;
		}

		public override Vector3 GetObjectiveLocation()
		{
			if (this.m_TargetTaskGroup != null)
				return this.m_TargetTaskGroup.GetBaseGroupPosition();
			return Vector3.Zero;
		}

		public override int ResourceNeeds()
		{
			return 1;
		}
	}
}
