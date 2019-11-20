// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.FlyByShipControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;

namespace Kerberos.Sots.Combat
{
	internal class FlyByShipControl : BaseAttackShipControl
	{
		private static int kHoldDuration = 900;
		private static int kStuckDuration = 600;
		private float m_DistancePast;
		private float m_CurrentDistPast;
		private int m_HoldDuration;
		private int m_CurrentHoldDelay;
		private bool m_InitialPass;
		private Matrix m_AttackMatrix;
		private int m_StuckDelay;
		private Vector3 m_PrevPos;

		public FlyByShipControl(App game, TacticalObjective to, CombatAI commanderAI, float distPast)
		  : base(game, to, commanderAI)
		{
			this.m_Type = ShipControlType.Flyby;
			this.m_DistancePast = distPast;
			this.m_CurrentDistPast = 0.0f;
			this.m_HoldDuration = FlyByShipControl.kHoldDuration;
			this.m_AttackMatrix = Matrix.Identity;
			this.m_InitialPass = true;
			this.m_CurrentHoldDelay = this.m_CommanderAI.AIRandom.NextInclusive(FlyByShipControl.kHoldDuration - 20, FlyByShipControl.kHoldDuration + 20);
			this.m_StuckDelay = FlyByShipControl.kStuckDuration;
			this.m_PrevPos = Vector3.Zero;
		}

		protected override void OnAttackUpdate(int framesElapsed)
		{
			if (this.m_TaskGroupObjective == null || !(this.m_TaskGroupObjective is AttackGroupObjective))
				return;
			if (this.m_InitialPass)
			{
				this.m_InitialPass = false;
				Vector3 forward = this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownPosition - (this.m_Formation.Ships.Count > 0 ? ShipFormation.GetCenterOfMass(this.m_Formation.Ships) : ShipFormation.GetCenterOfMass(this.m_Formation.ShipsOnBackLine));
				forward.Y = 0.0f;
				double num = (double)forward.Normalize();
				this.m_AttackMatrix = Matrix.CreateWorld(Vector3.Zero, forward, Vector3.UnitY);
				this.m_CurrentDistPast = this.m_CommanderAI.AIRandom.NextInclusive(this.m_DistancePast - 1000f, this.m_DistancePast + 1000f);
			}
			Vector3 currentPosition = this.GetCurrentPosition();
			Vector3 vector3_1 = this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownPosition + this.m_AttackMatrix.Forward * this.m_CurrentDistPast;
			if ((this.m_CommanderAI != null ? this.m_CommanderAI.GetPlanetContainingPosition(vector3_1) : (StellarBody)null) != null)
				vector3_1 = this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownPosition - this.m_AttackMatrix.Forward * this.m_CurrentDistPast;
			if (this.IsPlanetSeparatingTarget(vector3_1, currentPosition, 1000f))
			{
				Vector3 vector3_2 = this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownPosition - currentPosition;
				vector3_2.Y = 0.0f;
				vector3_1 = this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownPosition + Vector3.Normalize(vector3_2) * 5000f;
			}
			Vector3 vector3_3 = currentPosition - vector3_1;
			vector3_3.Y = 0.0f;
			double lengthSquared = (double)vector3_3.LengthSquared;
			if ((double)(this.m_PrevPos - currentPosition).LengthSquared < 1.0)
				this.m_StuckDelay -= framesElapsed;
			else
				this.m_StuckDelay = FlyByShipControl.kStuckDuration;
			this.m_PrevPos = currentPosition;
			if (lengthSquared < 40000.0 && this.m_Formation.DestinationSet && this.m_HoldDuration > 0 || this.m_StuckDelay <= 0)
			{
				this.FaceShipsToTarget();
				this.m_HoldDuration -= framesElapsed;
				if (this.m_HoldDuration > 0)
					return;
				this.m_AttackMatrix = Matrix.CreateRotationYPR(this.m_AttackMatrix.EulerAngles.X + MathHelper.DegreesToRadians(this.m_CommanderAI.AIRandom.NextInclusive(145f, 225f)), 0.0f, 0.0f);
				this.m_CurrentDistPast = this.m_CommanderAI.AIRandom.NextInclusive(this.m_DistancePast - 1000f, this.m_DistancePast + 1000f);
				this.m_CurrentHoldDelay = this.m_CommanderAI.AIRandom.NextInclusive(FlyByShipControl.kHoldDuration - 20, FlyByShipControl.kHoldDuration + 20);
				this.m_AttackMatrix = Matrix.CreateRotationYPR(this.m_AttackMatrix.EulerAngles.X + this.GetAddedFlyByAngle(this.m_AttackMatrix, this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownPosition, this.m_CurrentDistPast), 0.0f, 0.0f);
				this.m_StuckDelay = FlyByShipControl.kStuckDuration;
			}
			else
			{
				this.m_HoldDuration = this.m_CurrentHoldDelay;
				this.SetFUP(vector3_1, -this.m_AttackMatrix.Forward);
			}
		}
	}
}
