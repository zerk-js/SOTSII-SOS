// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.SupportGroupShipControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;

namespace Kerberos.Sots.Combat
{
	internal class SupportGroupShipControl : TaskGroupShipControl
	{
		private TaskGroupShipControl m_SupportGroup;

		public SupportGroupShipControl(
		  App game,
		  TacticalObjective to,
		  CombatAI commanderAI,
		  TaskGroupShipControl supportTaskGroup)
		  : base(game, to, commanderAI)
		{
			this.m_Type = ShipControlType.SupportGroup;
			this.m_SupportGroup = supportTaskGroup;
		}

		public override void Update(int framesElapsed)
		{
			if (this.m_TaskGroupObjective == null)
				return;
			Vector3 currentPosition = this.GetCurrentPosition();
			Vector3 dest = this.m_SupportGroup == null || this.m_SupportGroup.m_Formation == null || !this.m_SupportGroup.m_Formation.DestinationSet ? this.m_TaskGroupObjective.GetObjectiveLocation() : this.m_SupportGroup.m_Formation.Destination;
			Vector3 destFacing = dest - currentPosition;
			destFacing.Y = 0.0f;
			double num = (double)destFacing.Normalize();
			foreach (Ship ship in this.m_Ships)
			{
				float lengthSquared1 = (ship.Position - dest).LengthSquared;
				float lengthSquared2 = (ship.Position - this.m_TaskGroupObjective.GetObjectiveLocation()).LengthSquared;
				this.SetShipSpeed(ship, (double)lengthSquared1 >= (double)TaskGroup.ATTACK_GROUP_RANGE * (double)TaskGroup.ATTACK_GROUP_RANGE || (double)lengthSquared2 >= (double)lengthSquared1 ? ShipSpeedState.Overthrust : ShipSpeedState.Normal);
			}
			this.SetFUP(dest, destFacing);
		}
	}
}
