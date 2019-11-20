// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.StandOffShipControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using System;

namespace Kerberos.Sots.Combat
{
	internal class StandOffShipControl : BaseAttackShipControl
	{
		private static int kMinRunDuration = 1200;
		private static int kMinHoldGroundDuration = 2400;
		protected float m_MinStandOffDist;
		protected float m_DesiredStandOffDist;
		private float m_PrevDistFromEnemy;
		private int m_MinRunDuration;
		private int m_HoldGroundDuration;
		private bool m_UseStoredDir;
		private StandOffShipControl.StandoffState m_StandOffState;
		private Vector3 m_MoveFromInsideDir;

		public StandOffShipControl(
		  App game,
		  TacticalObjective to,
		  CombatAI commanderAI,
		  float minDist,
		  float desiredDist)
		  : base(game, to, commanderAI)
		{
			this.m_Type = ShipControlType.StandOff;
			this.m_StandOffState = StandOffShipControl.StandoffState.MoveToTargetPosition;
			this.m_MinStandOffDist = minDist;
			this.m_DesiredStandOffDist = desiredDist;
			this.m_MoveFromInsideDir = -Vector3.UnitZ;
			this.m_UseStoredDir = false;
		}

		protected override void OnAttackUpdate(int framesElapsed)
		{
			if (this.m_TaskGroupObjective == null || !(this.m_TaskGroupObjective is AttackGroupObjective))
				return;
			Vector3 currentPosition = this.GetCurrentPosition();
			Vector3 lastKnownPosition = this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownPosition;
			Vector3 zero1 = Vector3.Zero;
			Ship ship = this.m_GroupPriorityTarget != null ? this.m_GroupPriorityTarget : this.m_CommanderAI.GetBestShipTarget(currentPosition, this.m_TaskGroupObjective.m_TargetEnemyGroup.m_Ships);
			Vector3 enemyPos = ship == null ? lastKnownPosition : ship.Maneuvering.Position;
			enemyPos.Y = 0.0f;
			Vector3 vector3_1 = enemyPos - currentPosition;
			vector3_1.Y = 0.0f;
			float num1 = vector3_1.Normalize();
			Vector3 v1 = this.GetAttackVector(currentPosition, enemyPos);
			if ((double)v1.LengthSquared <= 0.0)
				v1 = -Vector3.UnitZ;
			Vector3 vector3_2 = enemyPos + v1 * this.m_DesiredStandOffDist;
			switch (this.m_StandOffState)
			{
				case StandOffShipControl.StandoffState.Hold:
					if ((double)num1 > (double)this.m_DesiredStandOffDist)
						this.m_StandOffState = StandOffShipControl.StandoffState.MoveFromOutside;
					else if ((double)num1 < (double)this.m_MinStandOffDist)
						this.m_StandOffState = StandOffShipControl.StandoffState.MoveFromInside;
					this.m_UseStoredDir = false;
					break;
				case StandOffShipControl.StandoffState.MoveFromOutside:
					this.m_HoldGroundDuration = 0;
					if ((double)num1 < (double)this.m_DesiredStandOffDist && (double)num1 > (double)this.m_MinStandOffDist)
						this.m_StandOffState = (double)(currentPosition - vector3_2).LengthSquared <= (double)this.m_MinStandOffDist ? StandOffShipControl.StandoffState.Hold : StandOffShipControl.StandoffState.MoveToTargetPosition;
					this.m_UseStoredDir = false;
					break;
				case StandOffShipControl.StandoffState.MoveFromInside:
					if ((double)num1 > (double)this.m_DesiredStandOffDist)
					{
						this.m_StandOffState = StandOffShipControl.StandoffState.Hold;
						break;
					}
					break;
				case StandOffShipControl.StandoffState.MoveToTargetPosition:
					if ((double)(currentPosition - vector3_2).LengthSquared < 250000.0)
					{
						this.m_StandOffState = StandOffShipControl.StandoffState.MoveFromInside;
						break;
					}
					break;
			}
			Vector3 zero2 = Vector3.Zero;
			Vector3 destFacing = -v1;
			Vector3 vector3_3;
			if (this.m_StandOffState == StandOffShipControl.StandoffState.Hold || this.m_HoldGroundDuration > 0)
			{
				this.FaceShipsToTarget();
				if ((double)num1 < (double)this.m_MinStandOffDist)
					this.m_HoldGroundDuration -= 2 * framesElapsed;
				else if ((double)num1 < (double)this.m_DesiredStandOffDist)
					this.m_HoldGroundDuration -= framesElapsed;
				vector3_3 = currentPosition;
				vector3_3.Y = enemyPos.Y;
			}
			else
			{
				if (this.m_StandOffState == StandOffShipControl.StandoffState.MoveFromInside && !(this is SurroundShipControl))
				{
					if (this.m_HoldGroundDuration <= 0 && (double)num1 - (double)this.m_PrevDistFromEnemy < 20.0)
					{
						this.m_MinRunDuration -= framesElapsed;
						if (this.m_MinRunDuration <= 0)
						{
							this.m_HoldGroundDuration = StandOffShipControl.kMinHoldGroundDuration;
							this.m_MinRunDuration = StandOffShipControl.kMinRunDuration;
							Vector3 averageVelocity = this.m_TaskGroupObjective.m_TargetEnemyGroup.GetAverageVelocity(this.m_CommanderAI);
							double num2 = (double)averageVelocity.Normalize();
							if ((double)v1.LengthSquared > 0.0)
								this.m_MoveFromInsideDir = v1;
							if ((double)Vector3.Dot(averageVelocity, v1) > 0.25)
								this.m_MoveFromInsideDir *= -1f;
							this.m_UseStoredDir = true;
						}
					}
					else
						this.m_MinRunDuration = StandOffShipControl.kMinRunDuration;
				}
				if (this.m_UseStoredDir)
					destFacing = -this.m_MoveFromInsideDir;
				vector3_3 = enemyPos + -destFacing * this.m_DesiredStandOffDist;
				if ((this.m_CommanderAI != null ? this.m_CommanderAI.GetPlanetContainingPosition(vector3_3) : (StellarBody)null) != null)
					vector3_3 = enemyPos + destFacing * this.m_DesiredStandOffDist;
				if (this.IsPlanetSeparatingTarget(vector3_3, currentPosition, 1000f))
					vector3_3 = enemyPos + vector3_1 * 5000f;
			}
			this.m_PrevDistFromEnemy = num1;
			this.SetFUP(vector3_3, destFacing);
		}

		protected virtual Vector3 GetAttackVector(Vector3 currentPos, Vector3 enemyPos)
		{
			Vector3 v0 = currentPos - enemyPos;
			v0.Y = 0.0f;
			double num1 = (double)v0.Normalize();
			Vector3 forward = Vector3.Normalize(new Vector3(enemyPos.X, 0.0f, enemyPos.Z));
			Matrix world = Matrix.CreateWorld(Vector3.Zero, forward, Vector3.UnitY);
			Vector3 vector3_1 = (double)Vector3.Dot(v0, world.Right) > 0.0 ? world.Right : -world.Right;
			float length = currentPos.Length;
			float num2 = this.m_CommanderAI.SystemRadius - this.m_CommanderAI.MinSystemRadius;
			float num3 = num2 * 0.8f;
			float t1 = Math.Max(Math.Min((float)(((double)length - (double)num3) / ((double)num2 - (double)num3)), 1f), 0.0f);
			float t2 = Math.Max(Math.Min((length - this.m_CommanderAI.MinSystemRadius) / num2, 1f), 0.0f);
			if ((double)t1 > 0.0)
			{
				vector3_1 = Vector3.Lerp(vector3_1, -Vector3.Normalize(currentPos), t1);
				double num4 = (double)vector3_1.Normalize();
			}
			Vector3 vector3_2 = Vector3.Lerp(v0, vector3_1, t2);
			double num5 = (double)vector3_2.Normalize();
			return vector3_2;
		}

		private enum StandoffState
		{
			Hold,
			MoveFromOutside,
			MoveFromInside,
			MoveToTargetPosition,
		}
	}
}
