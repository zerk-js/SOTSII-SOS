// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.AttackPlanetShipControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;

namespace Kerberos.Sots.Combat
{
	internal class AttackPlanetShipControl : BaseAttackShipControl
	{
		public AttackPlanetShipControl(App game, TacticalObjective to, CombatAI commanderAI)
		  : base(game, to, commanderAI)
		{
		}

		protected override void OnAttackUpdate(int framesElapsed)
		{
			if (this.m_TaskGroupObjective == null || this.m_TaskGroupObjective.m_Planet == null)
				return;
			foreach (Ship ship in this.m_Ships)
			{
				if (ship.Target != this.m_TaskGroupObjective.m_Planet)
					this.SetNewTarget(ship, (IGameObject)this.m_TaskGroupObjective.m_Planet);
			}
			Vector3 currentPosition = this.GetCurrentPosition();
			Vector3 safeDestination = this.m_CommanderAI.GetSafeDestination(currentPosition, this.m_TaskGroupObjective.m_Planet.Parameters.Position);
			Vector3 destFacing = safeDestination - currentPosition;
			destFacing.Y = 0.0f;
			double num1 = (double)destFacing.Normalize();
			this.SetFUP(safeDestination, destFacing);
			float num2 = 15000f;
			float num3 = !this.m_CommanderAI.EnemiesPresentInSystem ? 15000f : 100000f;
			float num4 = num2 * num2;
			float num5 = num3 * num3;
			foreach (Ship ship in this.m_Ships)
			{
				ShipSpeedState sss = (double)(ship.Position - safeDestination).LengthSquared > (double)num5 ? ShipSpeedState.Overthrust : ShipSpeedState.Normal;
				if (sss == ShipSpeedState.Overthrust && ship.TaskGroup != null && ship.TaskGroup.EnemyGroupInContact != null && (double)(ship.Position - ship.TaskGroup.EnemyGroupInContact.m_LastKnownPosition).LengthSquared < (double)num4)
					sss = ShipSpeedState.Normal;
				this.SetShipSpeed(ship, sss);
			}
		}
	}
}
