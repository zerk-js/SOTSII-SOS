// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.GhostShipCombatAIControl
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
	internal class GhostShipCombatAIControl : CombatAIController
	{
		private App m_Game;
		private Ship m_GhostShip;
		private IGameObject m_CurrentTarget;
		private GhostShipAIStates m_State;

		public override Ship GetShip()
		{
			return this.m_GhostShip;
		}

		public override void SetTarget(IGameObject target)
		{
			this.m_CurrentTarget = target;
		}

		public override IGameObject GetTarget()
		{
			return this.m_CurrentTarget;
		}

		public GhostShipCombatAIControl(App game, Ship ship)
		{
			this.m_Game = game;
			this.m_GhostShip = ship;
		}

		public override void Initialize()
		{
			this.m_CurrentTarget = (IGameObject)null;
			this.m_State = GhostShipAIStates.SEEK;
		}

		public override void Terminate()
		{
		}

		public override void ObjectRemoved(IGameObject obj)
		{
			if (obj != this.m_CurrentTarget)
				return;
			this.m_CurrentTarget = (IGameObject)null;
		}

		public override void OnThink()
		{
			if (this.m_GhostShip == null)
				return;
			switch (this.m_State)
			{
				case GhostShipAIStates.SEEK:
					this.ThinkSeek();
					break;
				case GhostShipAIStates.TRACK:
					this.ThinkTrack();
					break;
				case GhostShipAIStates.FLEE:
					this.ThinkFlee();
					break;
			}
		}

		public override void ForceFlee()
		{
		}

		public override bool VictoryConditionIsMet()
		{
			if (this.m_GhostShip != null)
				return this.m_GhostShip.HasRetreated;
			return false;
		}

		public override bool RequestingNewTarget()
		{
			if (this.m_State == GhostShipAIStates.SEEK)
				return this.m_CurrentTarget == null;
			return false;
		}

		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			this.m_CurrentTarget = (IGameObject)null;
			float num = float.MaxValue;
			List<StellarBody> stellarBodyList = new List<StellarBody>();
			List<Ship> shipList = new List<Ship>();
			foreach (IGameObject gameObject in objs)
			{
				if (gameObject != this.m_GhostShip)
				{
					if (gameObject is Ship)
					{
						Ship ship = gameObject as Ship;
						if (ship.Player.ID != this.m_GhostShip.Player.ID && ship.RealShipClass == RealShipClasses.Station)
							shipList.Add(ship);
					}
					else if (gameObject is StellarBody)
					{
						StellarBody stellarBody = gameObject as StellarBody;
						if (stellarBody.Population > 0.0 && stellarBody.Parameters.ColonyPlayerID != this.m_GhostShip.Player.ID)
							stellarBodyList.Add(gameObject as StellarBody);
					}
				}
			}
			foreach (Ship ship in shipList)
			{
				float lengthSquared = (this.m_GhostShip.Position - ship.Position).LengthSquared;
				if ((double)lengthSquared < (double)num)
				{
					num = lengthSquared;
					this.m_CurrentTarget = (IGameObject)ship;
				}
			}
			if (this.m_CurrentTarget != null)
				return;
			foreach (StellarBody stellarBody in stellarBodyList)
			{
				float lengthSquared = (this.m_GhostShip.Position - stellarBody.Parameters.Position).LengthSquared;
				if ((double)lengthSquared < (double)num)
				{
					num = lengthSquared;
					this.m_CurrentTarget = (IGameObject)stellarBody;
				}
			}
		}

		public override bool NeedsAParent()
		{
			return false;
		}

		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
		}

		private void ThinkSeek()
		{
			if (this.m_CurrentTarget == null)
				return;
			if (this.m_CurrentTarget is Ship)
				this.m_GhostShip.SetShipTarget(this.m_CurrentTarget.ObjectID, (this.m_CurrentTarget as Ship).ShipSphere.center, true, 0);
			else if (this.m_CurrentTarget is StellarBody)
				this.m_GhostShip.SetShipTarget(this.m_CurrentTarget.ObjectID, Vector3.Zero, true, 0);
			this.m_State = GhostShipAIStates.TRACK;
		}

		private void ThinkTrack()
		{
			bool flag = false;
			if (this.m_CurrentTarget == null)
				this.m_State = GhostShipAIStates.SEEK;
			else if (this.m_CurrentTarget is Ship)
			{
				Ship currentTarget = this.m_CurrentTarget as Ship;
				float num1 = (float)((double)currentTarget.ShipSphere.radius + (double)this.m_GhostShip.ShipSphere.radius + 500.0);
				Vector3 look = currentTarget.Position - this.m_GhostShip.Position;
				double num2 = (double)look.Normalize();
				this.m_GhostShip.Maneuvering.PostAddGoal(currentTarget.Position - look * num1, look);
				if (currentTarget.IsDestroyed)
					flag = true;
			}
			else if (this.m_CurrentTarget is StellarBody)
			{
				StellarBody currentTarget = this.m_CurrentTarget as StellarBody;
				float num1 = (float)((double)currentTarget.Parameters.Radius + (double)this.m_GhostShip.ShipSphere.radius + 750.0);
				Vector3 look = currentTarget.Parameters.Position - this.m_GhostShip.Position;
				double num2 = (double)look.Normalize();
				this.m_GhostShip.Maneuvering.PostAddGoal(currentTarget.Parameters.Position - look * num1, look);
				if ((this.m_CurrentTarget as StellarBody).Population <= 0.0)
					flag = true;
			}
			if (!flag)
				return;
			this.m_CurrentTarget = (IGameObject)null;
			this.m_State = GhostShipAIStates.FLEE;
		}

		private void ThinkFlee()
		{
			if (this.m_GhostShip == null || this.m_GhostShip.CombatStance == CombatStance.RETREAT)
				return;
			this.m_GhostShip.SetCombatStance(CombatStance.RETREAT);
		}
	}
}
