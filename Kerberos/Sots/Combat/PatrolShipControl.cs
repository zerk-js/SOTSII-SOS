// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.PatrolShipControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using System.Collections.Generic;

namespace Kerberos.Sots.Combat
{
	internal class PatrolShipControl : TaskGroupShipControl
	{
		private static int kStuckDelay = 300;
		private PatrolType m_PatrolType;
		private bool m_ClockWise;
		private List<Vector3> m_PatrolWaypoints;
		private Vector3 m_PrevPosition;
		private Vector3 m_PrevDir;
		private int m_CurrWaypointIndex;
		private int m_StuckDelay;
		private float m_CloseToDist;

		public Vector3 PreviousDir
		{
			get
			{
				return this.m_PrevDir;
			}
		}

		public PatrolShipControl(
		  App game,
		  TacticalObjective to,
		  CombatAI commanderAI,
		  PatrolType pt,
		  Vector3 dir,
		  float maxPatrolDist,
		  bool clockwise)
		  : base(game, to, commanderAI)
		{
			this.m_Type = ShipControlType.Patrol;
			this.m_PatrolType = pt;
			this.m_ClockWise = clockwise;
			this.m_PrevPosition = new Vector3();
			this.m_PrevDir = new Vector3();
			this.m_StuckDelay = PatrolShipControl.kStuckDelay;
			this.ResetPatrolWaypoints(pt, dir, maxPatrolDist);
			this.m_CloseToDist = 1000f;
		}

		public void ResetPatrolWaypoints(PatrolType pt, Vector3 dir, float maxPatrolDist)
		{
			if (!(this.m_TaskGroupObjective is PatrolObjective))
				return;
			this.m_PatrolWaypoints = (this.m_TaskGroupObjective as PatrolObjective).GetPatrolWaypoints(pt, dir, maxPatrolDist);
			this.m_PrevDir = dir;
			Vector3 currentPosition = this.GetCurrentPosition();
			this.m_CurrWaypointIndex = 0;
			float num = float.MaxValue;
			for (int index = 0; index < this.m_PatrolWaypoints.Count; ++index)
			{
				float lengthSquared = (this.m_PatrolWaypoints[index] - currentPosition).LengthSquared;
				if ((double)lengthSquared < (double)num)
				{
					num = lengthSquared;
					this.m_CurrWaypointIndex = index;
				}
			}
		}

		public override void Update(int framesElapsed)
		{
			if (this.m_TaskGroupObjective == null)
				return;
			Vector3 currentPosition = this.GetCurrentPosition();
			Vector3 destFacing = this.m_PatrolWaypoints[this.m_CurrWaypointIndex] - currentPosition;
			bool flag = false;
			if ((double)(this.m_PrevPosition - currentPosition).LengthSquared < 10.0)
			{
				this.m_StuckDelay -= framesElapsed;
				if (this.m_StuckDelay <= 0)
				{
					flag = true;
					this.m_StuckDelay = PatrolShipControl.kStuckDelay;
				}
			}
			else
			{
				this.m_PrevPosition = currentPosition;
				this.m_StuckDelay = PatrolShipControl.kStuckDelay;
			}
			if ((double)destFacing.LengthSquared < (double)this.m_CloseToDist * (double)this.m_CloseToDist || flag)
			{
				if (!this.m_ClockWise)
				{
					this.m_CurrWaypointIndex = (this.m_CurrWaypointIndex + 1) % this.m_PatrolWaypoints.Count;
				}
				else
				{
					--this.m_CurrWaypointIndex;
					if (this.m_CurrWaypointIndex < 0)
						this.m_CurrWaypointIndex = this.m_PatrolWaypoints.Count - 1;
				}
				destFacing = this.m_PatrolWaypoints[this.m_CurrWaypointIndex] - this.GetCurrentPosition();
			}
			destFacing.Y = 0.0f;
			double num = (double)destFacing.Normalize();
			this.SetFUP(this.m_PatrolWaypoints[this.m_CurrWaypointIndex], destFacing);
		}
	}
}
