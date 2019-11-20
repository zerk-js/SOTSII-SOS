// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.ProteanCombatAIControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Combat
{
	internal class ProteanCombatAIControl : CombatAIController
	{
		private App m_Game;
		private Ship m_Protean;
		private Ship m_Gardener;
		private StellarBody m_Home;
		private IGameObject m_CurrentTarget;
		private int m_FindNewTargDelay;
		private ProteanCombatStates m_State;
		private Vector3 m_LastGardenerPos;
		private float m_GardenerRadius;
		private bool m_HasGardener;
		private bool m_Victory;

		public override Ship GetShip()
		{
			return this.m_Protean;
		}

		public override void SetTarget(IGameObject target)
		{
			if (target != this.m_CurrentTarget)
			{
				int targetId = 0;
				CombatStance stance = CombatStance.NO_STANCE;
				float num = 0.0f;
				if (target != null)
				{
					targetId = target.ObjectID;
					stance = CombatStance.PURSUE;
					num = Math.Max(CombatAI.GetMinEffectiveWeaponRange(this.m_Protean, false), 500f);
				}
				this.m_Protean.SetCombatStance(stance);
				this.m_Protean.SetShipTarget(targetId, Vector3.Zero, true, 0);
				this.m_Protean.Maneuvering.PostSetProp("SetStanceTarget", (object)targetId, (object)num);
			}
			this.m_CurrentTarget = target;
		}

		public override IGameObject GetTarget()
		{
			return this.m_CurrentTarget;
		}

		public ProteanCombatAIControl(App game, Ship ship)
		{
			this.m_Game = game;
			this.m_Protean = ship;
		}

		public override void Initialize()
		{
			this.m_CurrentTarget = (IGameObject)null;
			this.m_Gardener = (Ship)null;
			this.m_State = ProteanCombatStates.INIT;
			this.m_FindNewTargDelay = 300;
			this.m_Gardener = (Ship)null;
			this.m_HasGardener = false;
			this.m_LastGardenerPos = Vector3.Zero;
			this.m_GardenerRadius = 0.0f;
			this.m_Victory = false;
		}

		public override void Terminate()
		{
		}

		public override void ObjectRemoved(IGameObject obj)
		{
			if (obj == this.m_CurrentTarget)
				this.m_CurrentTarget = (IGameObject)null;
			if (obj == this.m_Home)
				this.m_Home = (StellarBody)null;
			if (obj != this.m_Gardener)
				return;
			this.m_Gardener = (Ship)null;
		}

		public override void OnThink()
		{
			if (this.m_Protean == null)
				return;
			switch (this.m_State)
			{
				case ProteanCombatStates.INIT:
					this.ThinkInit();
					break;
				case ProteanCombatStates.SEEK:
					this.ThinkSeek();
					break;
				case ProteanCombatStates.TRACK:
					this.ThinkTrack();
					break;
			}
		}

		public override void ForceFlee()
		{
		}

		public override bool VictoryConditionIsMet()
		{
			return this.m_Victory;
		}

		public override bool RequestingNewTarget()
		{
			if (this.m_State != ProteanCombatStates.INIT)
				return this.m_CurrentTarget == null;
			return true;
		}

		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			if (this.m_Protean == null)
				return;
			float num1 = this.m_Protean.SensorRange + 10000f;
			if (this.m_Home != null)
				num1 += this.m_Home.Parameters.Radius;
			IGameObject target = (IGameObject)null;
			List<StellarBody> stellarBodyList = new List<StellarBody>();
			List<Ship> shipList = new List<Ship>();
			List<Ship> source = new List<Ship>();
			bool flag = false;
			foreach (IGameObject gameObject1 in objs)
			{
				if (gameObject1 is StellarBody)
				{
					StellarBody stellarBody = gameObject1 as StellarBody;
					stellarBodyList.Add(stellarBody);
					if (stellarBody.Population > 0.0 && stellarBody.Parameters.ColonyPlayerID != this.m_Protean.Player.ID)
						flag = true;
				}
				else if (gameObject1 is Kerberos.Sots.GameStates.StarSystem)
				{
					foreach (IGameObject gameObject2 in (gameObject1 as Kerberos.Sots.GameStates.StarSystem).Crits.Objects)
					{
						if (gameObject2 is StellarBody)
							stellarBodyList.Add(gameObject2 as StellarBody);
					}
				}
				else if (gameObject1 is Ship)
				{
					Ship ship = gameObject1 as Ship;
					if (this.m_Gardener == null && ship.IsGardener)
					{
						this.m_Gardener = ship;
						this.m_GardenerRadius = ship.ShipSphere.radius;
						this.m_HasGardener = true;
					}
					if ((this.m_Home != null || this.m_Gardener != null || this.m_HasGardener) && (ship != this.m_Protean && Ship.IsActiveShip(ship)))
					{
						if (this.m_Gardener == ship)
							this.m_LastGardenerPos = ship.Position;
						else if (ship.Player == this.m_Protean.Player)
						{
							source.Add(ship);
						}
						else
						{
							flag = true;
							if (ship.IsDetected(this.m_Protean.Player) && (double)((this.m_HasGardener ? this.m_LastGardenerPos : this.m_Home.Parameters.Position) - ship.Position).LengthSquared <= (double)num1 * (double)num1)
								shipList.Add(ship);
						}
					}
				}
			}
			if (flag)
			{
				float num2 = float.MaxValue;
				foreach (Ship ship in shipList)
				{
					Ship enemy = ship;
					if (!source.Any<Ship>((Func<Ship, bool>)(x => x.Target == enemy)))
					{
						float lengthSquared = (this.m_Protean.Position - enemy.Position).LengthSquared;
						if ((double)lengthSquared < (double)num2)
						{
							num2 = lengthSquared;
							target = (IGameObject)enemy;
						}
					}
				}
				if (target == null)
				{
					int num3 = -1;
					float num4 = float.MaxValue;
					foreach (Ship ship in shipList)
					{
						Ship enemy = ship;
						List<Ship> list = source.Where<Ship>((Func<Ship, bool>)(x => x.Target == enemy)).ToList<Ship>();
						float lengthSquared = (this.m_Protean.Position - enemy.Position).LengthSquared;
						if ((double)lengthSquared < (double)num4 && list.Count == num3 || (list.Count < num3 || num3 < 0))
						{
							num4 = lengthSquared;
							target = (IGameObject)enemy;
							num3 = list.Count;
						}
					}
				}
				if (target == null && this.m_HasGardener)
				{
					float num3 = float.MaxValue;
					foreach (StellarBody stellarBody in stellarBodyList)
					{
						if (stellarBody.Population > 0.0 && stellarBody.Parameters.ColonyPlayerID != this.m_Protean.Player.ID)
						{
							float lengthSquared = (this.m_Protean.Position - stellarBody.Parameters.Position).LengthSquared;
							if ((double)lengthSquared < (double)num3)
							{
								num3 = lengthSquared;
								target = (IGameObject)stellarBody;
							}
						}
					}
				}
			}
			else if (this.m_HasGardener)
				this.m_Victory = true;
			if (this.m_Home == null && !this.m_HasGardener)
			{
				float num2 = float.MaxValue;
				foreach (StellarBody stellarBody in stellarBodyList)
				{
					float lengthSquared = (this.m_Protean.Position - stellarBody.Parameters.Position).LengthSquared;
					if ((double)lengthSquared < (double)num2)
					{
						num2 = lengthSquared;
						this.m_Home = stellarBody;
					}
				}
				if (this.m_Home != null)
					this.m_Protean.Maneuvering.PostAddGoal(this.m_Home.Parameters.Position, -Vector3.UnitZ);
			}
			this.SetTarget(target);
		}

		public override bool NeedsAParent()
		{
			return false;
		}

		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
		}

		private void ThinkInit()
		{
			if (this.m_Home == null)
				return;
			this.m_Protean.Maneuvering.PostAddGoal(this.m_Home.Parameters.Position, -Vector3.UnitZ);
			this.m_State = ProteanCombatStates.SEEK;
		}

		private void ThinkSeek()
		{
			if (this.m_CurrentTarget == null)
				return;
			this.m_FindNewTargDelay = 300;
			this.m_State = ProteanCombatStates.TRACK;
		}

		private void ThinkTrack()
		{
			--this.m_FindNewTargDelay;
			if (this.m_CurrentTarget == null || this.m_Home == null && (this.m_Gardener == null || !Ship.IsActiveShip(this.m_Gardener)) || this.m_FindNewTargDelay <= 0)
			{
				this.SetTarget((IGameObject)null);
				this.m_State = ProteanCombatStates.SEEK;
			}
			else
			{
				Ship currentTarget = this.m_CurrentTarget as Ship;
				if (currentTarget == null)
					return;
				Vector3 vector3 = this.m_HasGardener ? this.m_LastGardenerPos : this.m_Home.Parameters.Position;
				float num1 = this.m_HasGardener ? this.m_GardenerRadius : this.m_Home.Parameters.Radius;
				float lengthSquared = (vector3 - currentTarget.Position).LengthSquared;
				float num2 = (float)((double)num1 + (double)this.m_Protean.SensorRange + 20000.0);
				if (Ship.IsActiveShip(currentTarget) && (double)lengthSquared <= (double)num2 * (double)num2)
					return;
				this.SetTarget((IGameObject)null);
				this.m_State = ProteanCombatStates.SEEK;
				this.m_Protean.Maneuvering.PostAddGoal(this.m_Home.Parameters.Position, -Vector3.UnitZ);
			}
		}
	}
}
