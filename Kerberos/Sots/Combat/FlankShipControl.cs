// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.FlankShipControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using System;

namespace Kerberos.Sots.Combat
{
	internal class FlankShipControl : SurroundShipControl
	{
		private Vector3 m_CurrentFlankPosition;
		private Vector3 m_FlankDriveDir;
		private bool m_InFlankPosition;

		public FlankShipControl(
		  App game,
		  TacticalObjective to,
		  CombatAI commanderAI,
		  Vector3 attackVector,
		  float minDist,
		  float desiredDist)
		  : base(game, to, commanderAI, attackVector, minDist, desiredDist)
		{
			this.m_Type = ShipControlType.Flank;
			this.m_CurrentFlankPosition = Vector3.Zero;
			this.m_FlankDriveDir = Vector3.UnitZ;
			this.m_InFlankPosition = false;
		}

		protected override void OnAttackUpdate(int framesElapsed)
		{
			if (this.m_TaskGroupObjective == null || this.m_TaskGroupObjective.m_TargetEnemyGroup == null)
				return;
			if (this.m_InFlankPosition)
			{
				base.OnAttackUpdate(framesElapsed);
			}
			else
			{
				Vector3 currentPosition = this.GetCurrentPosition();
				float num1 = (float)((double)this.m_Game.AssetDatabase.DefaultTacSensorRange + (double)this.m_TaskGroupObjective.m_TargetEnemyGroup.GetGroupRadius() + 500.0);
				Vector3 forward = this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownDestination - this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownPosition;
				if ((double)forward.LengthSquared <= 0.0)
					forward = currentPosition - this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownPosition;
				double num2 = (double)forward.Normalize();
				Matrix world = Matrix.CreateWorld(this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownPosition, forward, Vector3.UnitY);
				Matrix mat = Matrix.Inverse(world);
				Vector3 attackVector = this.GetAttackVector(currentPosition, world.Position);
				this.m_CurrentFlankPosition = world.Position + attackVector * this.m_DesiredStandOffDist;
				Vector3 vector3 = Vector3.Transform(this.m_CurrentFlankPosition, mat);
				this.m_CurrentFlankPosition = (double)Math.Abs(Vector3.Transform(currentPosition, mat).X) <= (double)num1 ? ((double)vector3.X >= 0.0 ? currentPosition - world.Right * num1 : currentPosition + world.Right * num1) : ((double)vector3.Z >= 0.0 ? currentPosition - world.Forward * num1 : currentPosition + world.Forward * num1);
				Vector3 v0 = vector3;
				double num3 = (double)v0.Normalize();
				this.m_InFlankPosition = (double)Vector3.Dot(v0, -Vector3.UnitZ) > 0.800000011920929;
				foreach (Ship ship in this.m_Ships)
					this.SetShipSpeed(ship, this.m_InFlankPosition ? ShipSpeedState.Normal : ShipSpeedState.Overthrust);
				this.SetFUP(this.m_CurrentFlankPosition, -attackVector);
			}
		}
	}
}
