// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.RetreatShipControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using System;
using System.Linq;

namespace Kerberos.Sots.Combat
{
	internal class RetreatShipControl : TaskGroupShipControl
	{
		private bool m_UseRetreatPos;

		public RetreatShipControl(
		  App game,
		  TacticalObjective to,
		  CombatAI commanderAI,
		  bool useRetreatPos)
		  : base(game, to, commanderAI)
		{
			this.m_Type = ShipControlType.Retreat;
			this.m_UseRetreatPos = useRetreatPos;
		}

		public override void Update(int framesElapsed)
		{
			if (this.m_TaskGroupObjective == null)
				return;
			Vector3 currentPosition = this.GetCurrentPosition();
			currentPosition.Y = 0.0f;
			currentPosition *= 2f;
			if (!this.m_Ships.Any<Ship>((Func<Ship, bool>)(x => x.IsNPCFreighter)))
			{
				foreach (Ship ship in this.m_Ships.Where<Ship>((Func<Ship, bool>)(x => !x.IsNPCFreighter)))
					this.SetShipSpeed(ship, ShipSpeedState.Overthrust);
			}
			foreach (Ship ship in this.m_Ships)
			{
				if (ship.CombatStance != CombatStance.RETREAT)
				{
					if (this.m_UseRetreatPos)
						ship.Maneuvering.RetreatDestination = this.m_TaskGroupObjective.GetObjectiveLocation();
					ship.SetCombatStance(CombatStance.RETREAT);
				}
			}
			Vector3 destFacing = currentPosition;
			double num = (double)destFacing.Normalize();
			this.SetFUP(currentPosition, destFacing);
		}
	}
}
