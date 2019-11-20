// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.VonNeumannNeoBerserkerControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Encounters;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Combat
{
	internal class VonNeumannNeoBerserkerControl : CombatAIController
	{
		protected const int MAX_DISCS = 10;
		protected App m_Game;
		protected Ship m_VonNeumannBerserker;
		protected List<Ship> m_LoadingDiscs;
		protected List<VonNeumannDiscControl> m_Discs;
		protected StellarBody m_Target;
		protected VonNeumannBerserkerStates m_State;
		protected bool m_DiscsActivated;

		public override Ship GetShip()
		{
			return this.m_VonNeumannBerserker;
		}

		public override void SetTarget(IGameObject target)
		{
			if (!(target is StellarBody) || this.m_VonNeumannBerserker == null)
				return;
			this.m_Target = target as StellarBody;
			this.m_VonNeumannBerserker.SetShipTarget(this.m_Target != null ? this.m_Target.ObjectID : 0, Vector3.Zero, true, 0);
		}

		public override IGameObject GetTarget()
		{
			return (IGameObject)this.m_Target;
		}

		public VonNeumannNeoBerserkerControl(App game, Ship ship)
		{
			this.m_Game = game;
			this.m_VonNeumannBerserker = ship;
		}

		public override void Initialize()
		{
			this.m_LoadingDiscs = new List<Ship>();
			this.m_Discs = new List<VonNeumannDiscControl>();
			this.m_DiscsActivated = false;
			Matrix rotationYpr = Matrix.CreateRotationYPR(this.m_VonNeumannBerserker.Maneuvering.Rotation);
			rotationYpr.Position = this.m_VonNeumannBerserker.Maneuvering.Position;
			float radians = MathHelper.DegreesToRadians(36f);
			for (int index = 0; index < 10; ++index)
			{
				Matrix worldMat = Matrix.CreateRotationY((float)index * radians) * rotationYpr;
				worldMat.Position += worldMat.Forward * 500f;
				int num = 5 + index;
				Ship newShip = CombatAIController.CreateNewShip(this.m_Game.Game, worldMat, VonNeumann.StaticShipDesigns[(VonNeumann.VonNeumannShipDesigns)num].DesignId, this.m_VonNeumannBerserker.ObjectID, this.m_VonNeumannBerserker.InputID, this.m_VonNeumannBerserker.Player.ObjectID);
				if (newShip != null)
					this.m_LoadingDiscs.Add(newShip);
			}
			this.m_State = VonNeumannBerserkerStates.INTEGRATECHILDS;
		}

		public override void Terminate()
		{
		}

		public override void ObjectRemoved(IGameObject obj)
		{
			foreach (VonNeumannDiscControl disc in this.m_Discs)
			{
				if (disc.GetShip() == obj)
				{
					this.m_Discs.Remove(disc);
					break;
				}
			}
			foreach (Ship loadingDisc in this.m_LoadingDiscs)
			{
				if (loadingDisc == obj)
				{
					this.m_LoadingDiscs.Remove(loadingDisc);
					break;
				}
			}
			if (obj != this.m_VonNeumannBerserker)
				return;
			if (this.m_VonNeumannBerserker != null && this.m_VonNeumannBerserker.IsDestroyed && !this.m_DiscsActivated)
			{
				foreach (VonNeumannDiscControl disc in this.m_Discs)
				{
					if (disc.GetShip() != null)
						disc.GetShip().KillShip(false);
				}
			}
			this.m_VonNeumannBerserker = (Ship)null;
		}

		public virtual bool IsThisMyMom(Ship ship)
		{
			return this.m_LoadingDiscs.Any<Ship>((Func<Ship, bool>)(x => x == ship));
		}

		public virtual void AddChild(CombatAIController child)
		{
			if (!(child is VonNeumannDiscControl))
				return;
			foreach (Ship loadingDisc in this.m_LoadingDiscs)
			{
				if (loadingDisc == child.GetShip())
				{
					loadingDisc.Active = this.m_DiscsActivated;
					this.m_LoadingDiscs.Remove(loadingDisc);
					break;
				}
			}
			this.m_Discs.Add(child as VonNeumannDiscControl);
		}

		public override void OnThink()
		{
			if (this.m_VonNeumannBerserker == null)
				return;
			if (this.m_VonNeumannBerserker.IsDestroyed && !this.m_DiscsActivated)
			{
				foreach (VonNeumannDiscControl disc in this.m_Discs)
				{
					if (disc.GetShip() != null)
						disc.GetShip().KillShip(false);
				}
			}
			else
			{
				switch (this.m_State)
				{
					case VonNeumannBerserkerStates.INTEGRATECHILDS:
						this.ThinkIntegrateChilds();
						break;
					case VonNeumannBerserkerStates.SEEK:
						this.ThinkSeek();
						break;
					case VonNeumannBerserkerStates.TRACK:
						this.ThinkTrack();
						break;
					case VonNeumannBerserkerStates.ACTIVATINGDISCS:
						this.ThinkActivatingDiscs();
						break;
				}
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
			if (this.m_LoadingDiscs.Count > 0)
				return false;
			bool flag = this.m_State == VonNeumannBerserkerStates.SEEK;
			foreach (VonNeumannDiscControl disc in this.m_Discs)
			{
				flag = flag || disc.RequestingNewTarget();
				if (flag)
					break;
			}
			if (!flag)
				return !this.m_DiscsActivated;
			return true;
		}

		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			List<Ship> shipList1 = new List<Ship>();
			List<Ship> shipList2 = new List<Ship>();
			foreach (IGameObject gameObject in objs)
			{
				if (gameObject is Ship)
				{
					Ship ship = gameObject as Ship;
					if (ship != this.m_VonNeumannBerserker && Ship.IsActiveShip(ship) && (ship.IsDetected(this.m_VonNeumannBerserker.Player) && ship.Player != this.m_VonNeumannBerserker.Player))
						shipList1.Add(ship);
				}
			}
			foreach (VonNeumannDiscControl disc in this.m_Discs)
			{
				if (VonNeumannDiscControl.DiscTypeIsAttacker(disc.DiscType))
					shipList2.Add(disc.GetShip());
			}
			if (shipList1.Count == 0)
				return;
			if (!this.m_DiscsActivated)
				this.m_State = VonNeumannBerserkerStates.ACTIVATINGDISCS;
			foreach (VonNeumannDiscControl disc in this.m_Discs)
			{
				if (Ship.IsActiveShip(disc.GetShip()) && disc.RequestingNewTarget())
				{
					Vector3 position = disc.GetShip().Position;
					float num = float.MaxValue;
					Ship ship1 = (Ship)null;
					foreach (Ship ship2 in VonNeumannDiscControl.DiscTypeIsAttacker(disc.DiscType) ? shipList1 : shipList2)
					{
						if (ship2 != disc.GetShip())
						{
							float lengthSquared = (ship2.Position - position).LengthSquared;
							if ((double)lengthSquared < (double)num)
							{
								ship1 = ship2;
								num = lengthSquared;
							}
						}
					}
					if (ship1 != null)
						disc.SetTarget((IGameObject)ship1);
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

		private void ThinkIntegrateChilds()
		{
			if (this.m_LoadingDiscs.Count == 0)
			{
				this.m_State = VonNeumannBerserkerStates.SEEK;
			}
			else
			{
				bool flag = true;
				foreach (GameObject loadingDisc in this.m_LoadingDiscs)
				{
					if (loadingDisc.ObjectStatus != GameObjectStatus.Ready)
						flag = false;
				}
				if (!flag)
					return;
				foreach (Ship loadingDisc in this.m_LoadingDiscs)
				{
					loadingDisc.Player = this.m_VonNeumannBerserker.Player;
					loadingDisc.Active = false;
					this.m_Game.CurrentState.AddGameObject((IGameObject)loadingDisc, false);
				}
				this.m_State = VonNeumannBerserkerStates.SEEK;
			}
		}

		protected virtual void ThinkSeek()
		{
			if (this.m_Target != null)
				this.m_State = VonNeumannBerserkerStates.TRACK;
			if (this.m_DiscsActivated || this is VonNeumannBerserkerControl)
				return;
			this.m_State = VonNeumannBerserkerStates.ACTIVATINGDISCS;
		}

		protected virtual void ThinkTrack()
		{
			if (this.m_Target != null)
				return;
			this.m_State = VonNeumannBerserkerStates.SEEK;
		}

		protected virtual void ThinkActivatingDiscs()
		{
			Matrix rotationYpr = Matrix.CreateRotationYPR(this.m_VonNeumannBerserker.Maneuvering.Rotation);
			rotationYpr.Position = this.m_VonNeumannBerserker.Maneuvering.Position;
			float radians = MathHelper.DegreesToRadians(36f);
			int num = 0;
			foreach (VonNeumannDiscControl disc in this.m_Discs)
			{
				Ship ship = disc.GetShip();
				Matrix matrix = Matrix.CreateRotationY((float)num * radians) * rotationYpr;
				ship.Position = matrix.Position + matrix.Forward * 500f;
				ship.Active = true;
				if (ship.Shield != null)
					ship.Shield.Active = false;
				disc.SetListOfDiscs(this.m_Discs);
				++num;
			}
			this.m_DiscsActivated = true;
			this.m_State = VonNeumannBerserkerStates.SEEK;
		}
	}
}
