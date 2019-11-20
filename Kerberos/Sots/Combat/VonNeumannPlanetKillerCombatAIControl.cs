// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.VonNeumannPlanetKillerCombatAIControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using System.Collections.Generic;

namespace Kerberos.Sots.Combat
{
	internal class VonNeumannPlanetKillerCombatAIControl : SystemKillerCombatAIControl
	{
		private int m_SystemId;

		public VonNeumannPlanetKillerCombatAIControl(App game, Ship ship, int systemId)
		  : base(game, ship)
		{
			this.m_SystemId = systemId;
		}

		public override bool RequestingNewTarget()
		{
			if (base.RequestingNewTarget())
				return true;
			if (this.m_State == SystemKillerStates.SEEK)
				return this.m_CurrentTarget == null;
			return false;
		}

		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			if (this.m_SystemKiller == null)
				return;
			if (!this.m_SpaceBattle && this.m_Planets.Count + this.m_Stars.Count + this.m_Moons.Count == 0)
				base.FindNewTarget(objs);
			if (this.m_Game.Game.ScriptModules.VonNeumann != null && this.m_Game.Game.ScriptModules.VonNeumann.HomeWorldSystemID == this.m_SystemId)
			{
				float num = float.MaxValue;
				foreach (IGameObject gameObject in objs)
				{
					if (gameObject is Ship)
					{
						Ship ship = gameObject as Ship;
						if (ship.Player != this.m_SystemKiller.Player && Ship.IsActiveShip(ship) && ship.IsDetected(this.m_SystemKiller.Player))
						{
							float lengthSquared = (ship.Position - this.m_SystemKiller.Position).LengthSquared;
							if ((double)lengthSquared < (double)num)
							{
								this.m_CurrentTarget = (IGameObject)ship;
								this.m_PlanetOffsetDist = ship.ShipSphere.radius + 1000f;
								num = lengthSquared;
							}
						}
					}
				}
			}
			else
				this.FindCurrentTarget(true);
		}

		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
		}

		protected override void ThinkSeek()
		{
			if (this.m_CurrentTarget == null)
				return;
			this.m_State = SystemKillerStates.TRACK;
		}

		protected override void ThinkTrack()
		{
			if (this.m_CurrentTarget == null)
			{
				this.m_State = SystemKillerStates.SEEK;
			}
			else
			{
				--this.m_TrackUpdateRate;
				if (this.m_TrackUpdateRate > 0)
					return;
				this.m_TrackUpdateRate = 10;
				Vector3 vector3 = this.m_SystemKiller.Position - this.m_TargetCenter;
				double num1 = (double)vector3.Normalize();
				if (this.m_CurrentTarget is Ship)
				{
					this.m_TargetCenter = (this.m_CurrentTarget as Ship).Position;
					this.m_TargetLook = this.m_TargetCenter - this.m_SystemKiller.Position;
					this.m_TargetLook.Y = 0.0f;
					double num2 = (double)this.m_TargetLook.Normalize();
				}
				Vector3 targetPos = this.m_TargetCenter + vector3 * this.m_PlanetOffsetDist;
				this.m_TargetLook = -vector3;
				this.m_SystemKiller.Maneuvering.PostAddGoal(targetPos, this.m_TargetLook);
				if (!(this.m_CurrentTarget is StellarBody))
					return;
				Matrix rotationYpr = Matrix.CreateRotationYPR(this.m_SystemKiller.Rotation);
				if (this.m_SystemKiller.Target == null || ((double)(this.m_SystemKiller.Position - targetPos).LengthSquared > 9000.0 || (double)Vector3.Dot(rotationYpr.Forward, this.m_TargetLook) <= 0.800000011920929))
					return;
				if (this.m_BeamBank != null)
					this.m_BeamBank.PostSetProp("DisableAllTurrets", false);
				this.m_State = SystemKillerStates.FIREBEAM;
			}
		}
	}
}
