// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.MorrigiCrowControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using System.Collections.Generic;

namespace Kerberos.Sots.Combat
{
	internal class MorrigiCrowControl : CombatAIController
	{
		private App m_Game;
		private Ship m_MorrigiCrow;
		private MorrigiRelicControl m_MorrigiRelic;
		private IGameObject m_Target;
		private MorrigiCrowstates m_State;
		private int m_RefreshTargetDelay;

		public override Ship GetShip()
		{
			return this.m_MorrigiCrow;
		}

		public override void SetTarget(IGameObject target)
		{
			if (target == this.m_Target)
				return;
			this.m_Target = target;
			this.m_MorrigiCrow.SetShipTarget(target != null ? target.ObjectID : 0, Vector3.Zero, true, 0);
		}

		public override IGameObject GetTarget()
		{
			return this.m_Target;
		}

		public MorrigiCrowControl(App game, Ship ship)
		{
			this.m_Game = game;
			this.m_MorrigiCrow = ship;
		}

		public override void Initialize()
		{
			this.m_Target = (IGameObject)null;
			this.m_MorrigiRelic = (MorrigiRelicControl)null;
			this.m_State = MorrigiCrowstates.SEEK;
			this.m_RefreshTargetDelay = 0;
		}

		public override void Terminate()
		{
		}

		public override void ObjectRemoved(IGameObject obj)
		{
			if (this.m_MorrigiRelic != null && this.m_MorrigiRelic.GetShip() == obj)
				this.m_MorrigiRelic = (MorrigiRelicControl)null;
			if (this.m_Target != obj)
				return;
			this.m_Target = (IGameObject)null;
		}

		public override void OnThink()
		{
			if (this.m_MorrigiCrow == null)
				return;
			this.UpdateTarget();
			switch (this.m_State)
			{
				case MorrigiCrowstates.IDLE:
					this.ThinkIdle();
					break;
				case MorrigiCrowstates.SEEK:
					this.ThinkSeek();
					break;
				case MorrigiCrowstates.TRACK:
					this.ThinkTrack();
					break;
			}
		}

		public void UpdateTarget()
		{
			if (this.m_Target != null)
			{
				--this.m_RefreshTargetDelay;
				if (this.m_RefreshTargetDelay > 0)
					return;
				this.SetTarget((IGameObject)null);
				this.m_RefreshTargetDelay = 300;
			}
			else
				this.m_RefreshTargetDelay = 300;
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
			return this.m_Target == null;
		}

		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			if (this.m_MorrigiRelic != null)
				return;
			float num = float.MaxValue;
			IGameObject target = (IGameObject)null;
			foreach (IGameObject gameObject in objs)
			{
				if (gameObject is Ship)
				{
					Ship ship = gameObject as Ship;
					if (ship.Player != this.m_MorrigiCrow.Player && ship.Active && (Ship.IsActiveShip(ship) && ship.IsDetected(this.m_MorrigiCrow.Player)))
					{
						float lengthSquared = (ship.Position - this.m_MorrigiCrow.Position).LengthSquared;
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

		public override bool NeedsAParent()
		{
			return this.m_MorrigiRelic == null;
		}

		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
			foreach (CombatAIController controller in controllers)
			{
				if (controller is MorrigiRelicControl)
				{
					MorrigiRelicControl morrigiRelicControl = controller as MorrigiRelicControl;
					if (morrigiRelicControl.IsThisMyRelic(this.m_MorrigiCrow))
					{
						morrigiRelicControl.AddCrow((CombatAIController)this);
						this.m_MorrigiRelic = morrigiRelicControl;
						break;
					}
				}
			}
		}

		private void ThinkIdle()
		{
		}

		private void ThinkSeek()
		{
			if (this.m_Target == null)
				return;
			this.m_State = MorrigiCrowstates.TRACK;
		}

		private void ThinkTrack()
		{
			if (this.m_Target != null)
				return;
			this.m_State = MorrigiCrowstates.SEEK;
		}
	}
}
