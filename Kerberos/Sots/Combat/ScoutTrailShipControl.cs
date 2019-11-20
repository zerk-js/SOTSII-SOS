// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.ScoutTrailShipControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using System;
using System.Linq;

namespace Kerberos.Sots.Combat
{
	internal class ScoutTrailShipControl : TaskGroupShipControl
	{
		private ScoutShipControl m_ScoutShip;

		public ScoutTrailShipControl(
		  App game,
		  TacticalObjective to,
		  CombatAI commanderAI,
		  ScoutShipControl scoutShip)
		  : base(game, to, commanderAI)
		{
			this.m_Type = ShipControlType.ScoutTrail;
			this.m_ScoutShip = scoutShip;
		}

		public override void Update(int framesElapsed)
		{
			if (this.m_TaskGroupObjective == null || this.m_ScoutShip == null)
				return;
			Vector3 zero = Vector3.Zero;
			Vector3 vector3 = -Vector3.UnitZ;
			float num1 = this.m_CommanderAI.PlanetsInSystem.Any<StellarBody>((Func<StellarBody, bool>)(x => x.Parameters.ColonyPlayerID == this.m_CommanderAI.m_Player.ID)) ? 10000f : 3000f;
			Vector3 dest;
			Vector3 destFacing;
			if (this.m_ScoutShip != null && this.m_ScoutShip.EncounteredEnemy)
			{
				dest = this.m_ScoutShip.GetCurrentPosition();
				destFacing = dest - this.GetCurrentPosition();
				destFacing.Y = 0.0f;
				double num2 = (double)destFacing.Normalize();
			}
			else
			{
				destFacing = this.m_ScoutShip.GetCurrentPosition() - this.GetCurrentPosition();
				destFacing.Y = 0.0f;
				double num2 = (double)destFacing.Normalize();
				dest = this.m_ScoutShip.GetCurrentPosition() - destFacing * num1;
			}
			if ((double)(this.m_ScoutShip.GetCurrentPosition() - this.GetCurrentPosition()).LengthSquared <= (double)num1 * (double)num1 && this.m_Formation.DestinationSet)
				return;
			this.SetFUP(dest, destFacing);
		}
	}
}
