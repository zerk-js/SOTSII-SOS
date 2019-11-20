// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.ScoutShipControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;

namespace Kerberos.Sots.Combat
{
	internal class ScoutShipControl : TaskGroupShipControl
	{
		private float m_SensorRange;
		private bool m_EncounteredEnemy;
		private EnemyGroup m_EnemyGroup;

		public bool EncounteredEnemy
		{
			get
			{
				return this.m_EncounteredEnemy;
			}
		}

		public ScoutShipControl(
		  App game,
		  TacticalObjective to,
		  CombatAI commanderAI,
		  float sensorRange)
		  : base(game, to, commanderAI)
		{
			this.m_Type = ShipControlType.Scout;
			this.m_EncounteredEnemy = false;
			this.m_SensorRange = sensorRange;
		}

		public void NotifyEnemyGroupDetected(EnemyGroup eGroup)
		{
			this.m_EnemyGroup = eGroup;
			this.m_EncounteredEnemy = this.m_EnemyGroup != null;
		}

		public override void Update(int framesElapsed)
		{
			if (this.m_TaskGroupObjective == null)
				return;
			Vector3 currentPosition = this.GetCurrentPosition();
			Vector3 zero = Vector3.Zero;
			Vector3 vector3 = -Vector3.UnitZ;
			Vector3 destFacing;
			Vector3 dest;
			if (this.m_EncounteredEnemy)
			{
				destFacing = currentPosition - this.m_EnemyGroup.m_LastKnownPosition;
				destFacing.Y = 0.0f;
				double num = (double)destFacing.Normalize();
				dest = this.m_EnemyGroup.m_LastKnownPosition + destFacing * this.m_SensorRange;
				destFacing *= -1f;
			}
			else
			{
				dest = (this.m_TaskGroupObjective as ScoutObjective).GetObjectiveLocation();
				destFacing = currentPosition - dest;
				destFacing.Y = 0.0f;
				double num = (double)destFacing.Normalize();
			}
			this.SetFUP(dest, destFacing);
		}
	}
}
