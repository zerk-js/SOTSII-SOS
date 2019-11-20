// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.ScoutObjective
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Combat
{
	internal class ScoutObjective : TacticalObjective
	{
		public Vector3 m_Destination;
		private Player m_Player;

		public ScoutObjective(EnemyGroup eGroup, Player player)
		{
			this.m_ObjectiveType = ObjectiveType.SCOUT;
			this.m_TargetEnemyGroup = eGroup;
			this.m_Destination = eGroup.m_LastKnownPosition;
			this.m_TaskGroups = new List<TaskGroup>();
			this.m_Player = player;
			this.m_RequestTaskGroup = false;
			this.m_Planet = (StellarBody)null;
		}

		public override void Shutdown()
		{
			this.m_Player = (Player)null;
			base.Shutdown();
		}

		public ScoutObjective(StellarBody ePlanet, Player player)
		{
			this.m_ObjectiveType = ObjectiveType.SCOUT;
			this.m_Planet = ePlanet;
			this.m_Destination = ePlanet.Parameters.Position;
			this.m_TaskGroups = new List<TaskGroup>();
			this.m_Player = player;
			this.m_RequestTaskGroup = false;
			this.m_TargetEnemyGroup = (EnemyGroup)null;
		}

		public override Vector3 GetObjectiveLocation()
		{
			if (this.m_TargetEnemyGroup != null)
				return this.m_TargetEnemyGroup.m_LastKnownPosition;
			return this.m_Destination;
		}

		public void SetPatrolDestination(Vector3 dest)
		{
			this.m_Destination = dest;
		}

		public override bool IsComplete()
		{
			if (this.m_TargetEnemyGroup != null && (this.m_TargetEnemyGroup.m_Ships.Count == 0 || this.m_Player != null && this.m_TaskGroups.Any<TaskGroup>((Func<TaskGroup, bool>)(x => x.EnemyGroupInContact == this.m_TargetEnemyGroup))))
				return true;
			if (this.m_Planet != null)
			{
				float offset = (float)((double)this.m_Planet.Parameters.Radius + 750.0 + 500.0 + 2000.0);
				offset *= offset;
				if (this.m_TaskGroups.Any<TaskGroup>((Func<TaskGroup, bool>)(x =>
			   {
				   if (x.GetShipCount() > 0)
					   return (double)(x.GetBaseGroupPosition() - this.m_Planet.Parameters.Position).LengthSquared < (double)offset;
				   return false;
			   })))
					return true;
			}
			if (this.m_TargetEnemyGroup == null)
				return this.m_Planet == null;
			return false;
		}

		public override int ResourceNeeds()
		{
			return 1;
		}
	}
}
