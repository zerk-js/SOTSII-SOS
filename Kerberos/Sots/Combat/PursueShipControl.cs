// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.PursueShipControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using System;

namespace Kerberos.Sots.Combat
{
	internal class PursueShipControl : BaseAttackShipControl
	{
		private static int kMinFlyByDelay = 600;
		private Vector3 m_PrevEnemyPosition;
		private Vector3 m_FlyByPosition;
		private int m_MinFlyByDelay;
		private float m_PursueRange;
		private float m_PrevFlyByDistSq;
		private bool m_DoFlyBy;

		public PursueShipControl(
		  App game,
		  TacticalObjective to,
		  CombatAI commanderAI,
		  float pursueRange)
		  : base(game, to, commanderAI)
		{
			this.m_Type = ShipControlType.Pursue;
			this.m_PursueRange = pursueRange;
			this.m_DoFlyBy = false;
			this.m_PrevEnemyPosition = Vector3.Zero;
			this.m_FlyByPosition = Vector3.Zero;
			this.m_MinFlyByDelay = PursueShipControl.kMinFlyByDelay;
			this.m_PrevFlyByDistSq = 0.0f;
		}

		protected override void OnAttackUpdate(int framesElapsed)
		{
			if (this.m_TaskGroupObjective == null || this.m_TaskGroupObjective.m_TargetEnemyGroup == null)
				return;
			Vector3 lastKnownPosition = this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownPosition;
			Vector3 currentPosition = this.GetCurrentPosition();
			Vector3 zero1 = Vector3.Zero;
			Vector3 vector3_1;
			if (this.m_DoFlyBy)
			{
				vector3_1 = this.m_FlyByPosition;
				if ((double)(currentPosition - vector3_1).LengthSquared < 10000.0)
				{
					this.m_DoFlyBy = false;
					this.m_MinFlyByDelay = PursueShipControl.kMinFlyByDelay;
				}
			}
			else
			{
				Ship ship = this.m_GroupPriorityTarget != null ? this.m_GroupPriorityTarget : this.m_CommanderAI.GetBestShipTarget(currentPosition, this.m_TaskGroupObjective.m_TargetEnemyGroup.m_Ships);
				vector3_1 = ship == null ? lastKnownPosition : ship.Maneuvering.Position;
			}
			vector3_1.Y = 0.0f;
			Vector3 vector3_2 = currentPosition - vector3_1;
			vector3_2.Y = 0.0f;
			bool flag = false;
			if ((double)(this.m_PrevEnemyPosition - lastKnownPosition).LengthSquared < 10.0)
			{
				if (!this.m_DoFlyBy)
				{
					float num = this.m_PursueRange + 200f;
					if ((double)vector3_2.LengthSquared < (double)num * (double)num)
						flag = true;
				}
				else
				{
					float lengthSquared = vector3_2.LengthSquared;
					if ((double)Math.Abs(lengthSquared - this.m_PrevFlyByDistSq) < 10.0)
						flag = true;
					this.m_PrevFlyByDistSq = lengthSquared;
				}
			}
			else
				this.m_DoFlyBy = false;
			if (flag)
			{
				this.m_MinFlyByDelay -= framesElapsed;
				if (this.m_MinFlyByDelay <= 0)
				{
					this.m_DoFlyBy = true;
					this.m_MinFlyByDelay = PursueShipControl.kMinFlyByDelay;
					Vector3 vector3_3 = lastKnownPosition - currentPosition;
					vector3_3.Y = 0.0f;
					double num = (double)vector3_3.Normalize();
					this.m_FlyByPosition = this.GetFlyByPosition(currentPosition, lastKnownPosition, 2f * this.m_PursueRange + this.m_TaskGroupObjective.m_TargetEnemyGroup.GetGroupRadius(), 0.0f);
				}
			}
			else
				this.m_MinFlyByDelay = PursueShipControl.kMinFlyByDelay;
			this.m_PrevEnemyPosition = lastKnownPosition;
			Vector3 zero2 = Vector3.Zero;
			Vector3 vector3_4 = -Vector3.UnitZ;
			if (!this.m_DoFlyBy && (double)vector3_2.LengthSquared < (double)this.m_PursueRange * (double)this.m_PursueRange && this.m_Formation.DestinationSet)
			{
				this.FaceShipsToTarget();
			}
			else
			{
				Vector3 vector3_3;
				Vector3 destFacing;
				if (this.m_DoFlyBy)
				{
					vector3_3 = this.m_FlyByPosition;
					double num = (double)vector3_2.Normalize();
					destFacing = vector3_2 * -1f;
				}
				else
				{
					float length = vector3_2.Length;
					if ((double)length > 0.0)
					{
						vector3_2 /= length;
						float num1 = Math.Min(length, this.m_PursueRange);
						vector3_3 = vector3_1 + vector3_2 * num1;
						destFacing = vector3_2 * -1f;
						StellarBody stellarBody = this.m_CommanderAI != null ? this.m_CommanderAI.GetPlanetContainingPosition(vector3_3) : (StellarBody)null;
						if (stellarBody != null)
						{
							vector3_3 = vector3_1 - vector3_2 * num1;
							destFacing = vector3_2;
						}
						if (this.IsPlanetSeparatingTarget(vector3_3, currentPosition, 1000f))
						{
							float num2 = stellarBody != null ? -1f : 1f;
							vector3_3 = vector3_1 + vector3_2 * (5000f * num2);
							destFacing = vector3_2 * -num2;
						}
					}
					else
					{
						vector3_3 = currentPosition;
						destFacing = this.m_Formation.Facing;
					}
				}
				this.SetFUP(vector3_3, destFacing);
			}
		}
	}
}
