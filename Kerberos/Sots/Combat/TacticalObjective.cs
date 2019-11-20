// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.TacticalObjective
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using System.Collections.Generic;

namespace Kerberos.Sots.Combat
{
	internal abstract class TacticalObjective
	{
		public ObjectiveType m_ObjectiveType;
		public EnemyGroup m_TargetEnemyGroup;
		public Ship m_PoliceOwner;
		public StellarBody m_Planet;
		public List<TaskGroup> m_TaskGroups;
		public TaskGroup m_TargetTaskGroup;
		public int m_CurrentResources;
		public bool m_RequestTaskGroup;

		public abstract bool IsComplete();

		public abstract int ResourceNeeds();

		public abstract Vector3 GetObjectiveLocation();

		public virtual void Update()
		{
		}

		public virtual void Shutdown()
		{
			this.m_TargetTaskGroup = (TaskGroup)null;
			this.m_TargetEnemyGroup = (EnemyGroup)null;
			this.m_PoliceOwner = (Ship)null;
			this.m_Planet = (StellarBody)null;
			this.m_TaskGroups.Clear();
		}

		public virtual int CurrentResources()
		{
			int num = 0;
			foreach (TaskGroup taskGroup in this.m_TaskGroups)
				num += CombatAI.AssessGroupStrength(taskGroup.GetShips());
			return num;
		}

		public virtual void AssignResources(TaskGroup group)
		{
			this.m_TaskGroups.Add(group);
			group.Objective = this;
			this.m_CurrentResources = this.CurrentResources();
		}

		public virtual void RemoveTaskGroup(TaskGroup group)
		{
			if (!this.m_TaskGroups.Contains(group))
				return;
			this.m_TaskGroups.Remove(group);
			group.Objective = (TacticalObjective)null;
			this.m_CurrentResources = this.CurrentResources();
		}
	}
}
