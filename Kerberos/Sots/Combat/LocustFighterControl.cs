// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.LocustFighterControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using System.Collections.Generic;

namespace Kerberos.Sots.Combat
{
	internal class LocustFighterControl : CombatAIController
	{
		private App m_Game;
		private Ship m_LocustFighter;
		private LocustNestControl m_LocustNest;
		private IGameObject m_Target;
		private LocustFighterStates m_State;

		public static int NumFightersPerShip(ShipClass sc)
		{
			switch (sc)
			{
				case ShipClass.Cruiser:
					return 6;
				case ShipClass.Dreadnought:
					return 10;
				case ShipClass.Leviathan:
				case ShipClass.Station:
					return 15;
				case ShipClass.BattleRider:
					return 2;
				default:
					return 5;
			}
		}

		public override Ship GetShip()
		{
			return this.m_LocustFighter;
		}

		public override void SetTarget(IGameObject target)
		{
			if (target == this.m_Target)
				return;
			this.m_Target = target;
			this.m_LocustFighter.SetShipTarget(target != null ? target.ObjectID : 0, Vector3.Zero, true, 0);
		}

		public override IGameObject GetTarget()
		{
			return this.m_Target;
		}

		public LocustFighterControl(App game, Ship ship)
		{
			this.m_Game = game;
			this.m_LocustFighter = ship;
		}

		public override void Initialize()
		{
			this.m_Target = (IGameObject)null;
			this.m_LocustNest = (LocustNestControl)null;
			this.m_State = LocustFighterStates.IDLE;
		}

		public override void Terminate()
		{
		}

		public override void ObjectRemoved(IGameObject obj)
		{
			if (this.m_LocustNest != null && this.m_LocustNest.GetShip() == obj)
				this.m_LocustNest = (LocustNestControl)null;
			if (this.m_Target != obj)
				return;
			this.m_Target = (IGameObject)null;
		}

		public override void OnThink()
		{
			if (this.m_LocustFighter == null)
				return;
			switch (this.m_State)
			{
				case LocustFighterStates.IDLE:
					this.ThinkIdle();
					break;
				case LocustFighterStates.SEEK:
					this.ThinkSeek();
					break;
				case LocustFighterStates.TRACK:
					this.ThinkTrack();
					break;
				case LocustFighterStates.ASSAULT:
					this.ThinkAssault();
					break;
			}
		}

		public override void ForceFlee()
		{
		}

		public override bool VictoryConditionIsMet()
		{
			return false;
		}

		public override bool RequestingNewTarget()
		{
			if (this.m_LocustFighter != null && this.m_LocustFighter.DockedWithParent)
				return false;
			if (this.m_Target == null)
				return true;
			if (this.m_LocustNest != null)
				return this.m_Target == this.m_LocustNest.GetShip();
			return false;
		}

		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			if (this.m_LocustNest == null)
			{
				float num = 1E+17f;
				IGameObject target = (IGameObject)null;
				foreach (IGameObject gameObject in objs)
				{
					if (gameObject is Ship)
					{
						Ship ship = gameObject as Ship;
						if (ship.Player != this.m_LocustFighter.Player && Ship.IsActiveShip(ship) && ship.IsDetected(this.m_LocustFighter.Player))
						{
							float lengthSquared = (ship.Position - this.m_LocustFighter.Position).LengthSquared;
							if ((double)lengthSquared < (double)num)
							{
								target = (IGameObject)ship;
								num = lengthSquared;
							}
						}
					}
				}
				this.SetTarget(target);
			}
			else
				this.m_LocustNest.RequestTargetFromParent(this);
		}

		public override bool NeedsAParent()
		{
			return this.m_LocustNest == null;
		}

		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
			foreach (CombatAIController controller in controllers)
			{
				if (controller is LocustNestControl)
				{
					LocustNestControl locustNestControl = controller as LocustNestControl;
					if (locustNestControl.IsThisMyNest(this.m_LocustFighter))
					{
						locustNestControl.AddFighter((CombatAIController)this);
						this.m_LocustNest = locustNestControl;
						break;
					}
				}
			}
		}

		public void ClearPlanetTarget()
		{
			if (!(this.m_Target is StellarBody) || this.m_State == LocustFighterStates.ASSAULT)
				return;
			this.SetTarget((IGameObject)null);
		}

		private void ThinkIdle()
		{
			if (this.m_Target == null || this.m_Target == this.m_LocustNest)
				return;
			this.m_State = LocustFighterStates.TRACK;
		}

		private void ThinkSeek()
		{
			if (this.m_Target == null)
				return;
			if (this.m_Target != this.m_LocustNest)
				this.m_State = LocustFighterStates.TRACK;
			else
				this.m_State = LocustFighterStates.IDLE;
		}

		private void ThinkTrack()
		{
			if (this.m_Target == null)
			{
				if (this.m_Target != this.m_LocustNest)
					this.m_State = LocustFighterStates.SEEK;
				else
					this.m_State = LocustFighterStates.IDLE;
			}
			else
			{
				if (!(this.m_Target is StellarBody))
					return;
				StellarBody target = this.m_Target as StellarBody;
				float num = target.Parameters.Radius + this.m_LocustFighter.ShipSphere.radius;
				if ((double)(target.Parameters.Position - this.m_LocustFighter.Position).LengthSquared >= (double)num * (double)num)
					return;
				this.m_State = LocustFighterStates.ASSAULT;
				if (this.m_LocustNest == null)
					return;
				this.m_LocustNest.NotifyFighterHasLanded();
			}
		}

		private void ThinkAssault()
		{
			bool flag = this.m_Target == null;
			if (this.m_Target is StellarBody)
			{
				StellarBody target = this.m_Target as StellarBody;
				float num = target.Parameters.Radius + this.m_LocustFighter.ShipSphere.radius;
				if ((double)(target.Parameters.Position - this.m_LocustFighter.Position).LengthSquared > (double)num * (double)num)
					flag = true;
			}
			if (!flag)
				return;
			this.m_State = LocustFighterStates.SEEK;
			this.SetTarget((IGameObject)null);
		}
	}
}
