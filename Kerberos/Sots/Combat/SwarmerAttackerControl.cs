// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.SwarmerAttackerControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using System.Collections.Generic;

namespace Kerberos.Sots.Combat
{
	internal class SwarmerAttackerControl : CombatAIController
	{
		private App m_Game;
		private Ship m_Swarmer;
		private SwarmerSpawnerControl m_SwarmerParent;
		private IGameObject m_Target;
		private SwarmerAttackerStates m_State;
		private int m_UpdateRate;
		private SwarmerAttackerType m_Type;

		public static int NumSwarmersPerShip(ShipClass sc)
		{
			switch (sc)
			{
				case ShipClass.Cruiser:
					return 5;
				case ShipClass.Dreadnought:
					return 7;
				case ShipClass.Leviathan:
				case ShipClass.Station:
					return 10;
				case ShipClass.BattleRider:
					return 2;
				default:
					return 5;
			}
		}

		public static int NumGuardiansPerShip(ShipClass sc)
		{
			switch (sc)
			{
				case ShipClass.Cruiser:
					return 3;
				case ShipClass.Dreadnought:
					return 5;
				case ShipClass.Leviathan:
				case ShipClass.Station:
					return 7;
				case ShipClass.BattleRider:
					return 1;
				default:
					return 3;
			}
		}

		public SwarmerAttackerType Type
		{
			get
			{
				return this.m_Type;
			}
		}

		public override Ship GetShip()
		{
			return this.m_Swarmer;
		}

		public override void SetTarget(IGameObject target)
		{
			if (target == this.m_Target)
				return;
			this.m_Target = target;
		}

		public override IGameObject GetTarget()
		{
			return this.m_Target;
		}

		public SwarmerAttackerStates State
		{
			get
			{
				return this.m_State;
			}
			set
			{
				this.m_State = value;
			}
		}

		public SwarmerAttackerControl(App game, Ship ship, SwarmerAttackerType type)
		{
			this.m_Game = game;
			this.m_Swarmer = ship;
			this.m_Type = type;
		}

		public override void Initialize()
		{
			this.m_Target = (IGameObject)null;
			this.m_SwarmerParent = (SwarmerSpawnerControl)null;
			this.m_State = SwarmerAttackerStates.SEEK;
			this.m_UpdateRate = 0;
			if (this.m_Swarmer == null)
				return;
			foreach (IGameObject weaponBank in this.m_Swarmer.WeaponBanks)
				weaponBank.PostSetProp("IgnoreLineOfSight", true);
		}

		public override void Terminate()
		{
		}

		public override void ObjectRemoved(IGameObject obj)
		{
			if (this.m_SwarmerParent != null && this.m_SwarmerParent.GetShip() == obj)
				this.m_SwarmerParent = (SwarmerSpawnerControl)null;
			if (this.m_Target != obj)
				return;
			this.m_Target = (IGameObject)null;
		}

		public override void OnThink()
		{
			if (this.m_Swarmer == null)
				return;
			switch (this.m_State)
			{
				case SwarmerAttackerStates.SEEK:
					this.ThinkSeek();
					break;
				case SwarmerAttackerStates.TRACK:
					this.ThinkTrack();
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
			if (this.m_Swarmer == null || this.m_Swarmer.DockedWithParent || this.m_Target != null)
				return false;
			return this.m_State == SwarmerAttackerStates.SEEK;
		}

		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			if (this.m_SwarmerParent == null)
			{
				float num = float.MaxValue;
				this.m_Target = (IGameObject)null;
				foreach (IGameObject gameObject in objs)
				{
					if (gameObject is Ship)
					{
						Ship ship = gameObject as Ship;
						if (ship.Player != this.m_Swarmer.Player && ship.Active && (Ship.IsActiveShip(ship) && ship.IsDetected(this.m_Swarmer.Player)))
						{
							float lengthSquared = (ship.Position - this.m_Swarmer.Position).LengthSquared;
							if ((double)lengthSquared < (double)num)
							{
								this.m_Target = (IGameObject)ship;
								num = lengthSquared;
							}
						}
					}
				}
			}
			else
				this.m_SwarmerParent.RequestTargetFromParent(this);
		}

		public override bool NeedsAParent()
		{
			return this.m_SwarmerParent == null;
		}

		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
			foreach (CombatAIController controller in controllers)
			{
				if (controller is SwarmerSpawnerControl)
				{
					SwarmerSpawnerControl swarmerSpawnerControl = controller as SwarmerSpawnerControl;
					if (swarmerSpawnerControl.IsThisMyParent(this.m_Swarmer))
					{
						swarmerSpawnerControl.AddChild((CombatAIController)this);
						this.m_SwarmerParent = swarmerSpawnerControl;
						break;
					}
				}
			}
		}

		private void ThinkSeek()
		{
			if (this.m_Target != null)
			{
				if (this.m_Target is Ship)
					this.m_Swarmer.SetShipTarget(this.m_Target.ObjectID, (this.m_Target as Ship).ShipSphere.center, true, 0);
				else
					this.m_Swarmer.SetShipTarget(this.m_Target.ObjectID, Vector3.Zero, true, 0);
				this.m_State = SwarmerAttackerStates.TRACK;
			}
			else
			{
				if (this.m_SwarmerParent == null)
					return;
				--this.m_UpdateRate;
				if (this.m_UpdateRate > 0)
					return;
				this.m_UpdateRate = 3;
				Vector3 look = this.m_Swarmer.Position - this.m_SwarmerParent.GetShip().Position;
				double num = (double)look.Normalize();
				this.m_Swarmer.Maneuvering.PostAddGoal(this.m_SwarmerParent.GetShip().Position + look * 1500f, look);
			}
		}

		private void ThinkTrack()
		{
			if (this.m_Target != null)
				return;
			this.m_Swarmer.SetShipTarget(0, Vector3.Zero, true, 0);
			this.m_State = SwarmerAttackerStates.SEEK;
		}
	}
}
