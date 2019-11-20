// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.RetreatObjective
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using System.Collections.Generic;

namespace Kerberos.Sots.Combat
{
	internal class RetreatObjective : TacticalObjective
	{
		public Vector3 m_Destination;

		public RetreatObjective(Vector3 pos)
		{
			this.m_ObjectiveType = ObjectiveType.RETREAT;
			this.m_TaskGroups = new List<TaskGroup>();
			this.m_Destination = pos;
			this.m_RequestTaskGroup = false;
		}

		public RetreatObjective(StellarBody homeColony)
		{
			this.m_ObjectiveType = ObjectiveType.RETREAT;
			this.m_Planet = homeColony;
			this.m_Destination = homeColony.Parameters.Position;
			this.m_TaskGroups = new List<TaskGroup>();
			this.m_RequestTaskGroup = false;
		}

		public void ResetRetreatPosition(TaskGroup tg)
		{
			if (tg == null)
				return;
			float length = this.m_Destination.Length;
			this.m_Destination = Vector3.Normalize(tg.GetBaseGroupPosition()) * length;
		}

		public override Vector3 GetObjectiveLocation()
		{
			return this.m_Destination;
		}

		public void SetPatrolDestination(Vector3 dest)
		{
			this.m_Destination = dest;
		}

		public override bool IsComplete()
		{
			return false;
		}

		public override int ResourceNeeds()
		{
			return 1;
		}
	}
}
