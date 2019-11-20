// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.ColonyTrapDroneControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Combat
{
	internal class ColonyTrapDroneControl : CombatAIController
	{
		private const int kHelpDelay = 200;
		private App m_Game;
		private Ship m_Drone;
		private NPCFactionCombatAI m_Controller;
		private Ship m_Target;
		private DroneTrapState m_State;
		private StellarBody m_TrapPlanet;
		private Vector3 m_DefaultTestPlanetPos;
		private float m_PrevDistFromPlanet;
		private float m_AttackRange;
		private bool m_RequiresHelp;
		private bool m_WeaponsDisabled;
		private int m_RequestHelpDelay;
		private int m_PlanetID;

		public static int MinNumDronesPerShip(ShipClass sc)
		{
			switch (sc)
			{
				case ShipClass.Cruiser:
					return 1;
				case ShipClass.Dreadnought:
					return 3;
				case ShipClass.Leviathan:
					return 9;
				case ShipClass.BattleRider:
				case ShipClass.Station:
					return 0;
				default:
					return 0;
			}
		}

		public static int TrapPriorityValue(Ship ship)
		{
			if (ship == null)
				return 0;
			if (ship.ShipRole == ShipRole.COLONIZER)
				return 10;
			if (ship.ShipRole == ShipRole.COMMAND)
				return 5;
			switch (ship.ShipClass)
			{
				case ShipClass.Cruiser:
					return 5;
				case ShipClass.Dreadnought:
					return 4;
				case ShipClass.Leviathan:
					return 3;
				default:
					return 0;
			}
		}

		public override Ship GetShip()
		{
			return this.m_Drone;
		}

		public override void SetTarget(IGameObject target)
		{
			if (target != null && !(target is Ship))
				return;
			this.m_Drone.SetShipTarget(target != null ? target.ObjectID : 0, Vector3.Zero, true, 0);
			this.m_Target = target != null ? target as Ship : (Ship)null;
		}

		public override IGameObject GetTarget()
		{
			return (IGameObject)this.m_Target;
		}

		public DroneTrapState State
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

		public ColonyTrapDroneControl(App game, Ship ship, NPCFactionCombatAI controller, int fleetID)
		{
			this.m_Game = game;
			this.m_Drone = ship;
			this.m_Controller = controller;
			this.m_DefaultTestPlanetPos = ship.Position;
			ColonyTrapInfo trapInfoByFleetId = game.GameDatabase.GetColonyTrapInfoByFleetID(fleetID);
			this.m_PlanetID = trapInfoByFleetId != null ? trapInfoByFleetId.PlanetID : 0;
		}

		public override void Initialize()
		{
			this.m_Target = (Ship)null;
			this.m_TrapPlanet = (StellarBody)null;
			this.m_RequiresHelp = false;
			this.m_State = DroneTrapState.SEEK;
			this.m_RequestHelpDelay = 200;
			this.m_PrevDistFromPlanet = 0.0f;
			this.m_AttackRange = 0.0f;
			foreach (WeaponBank state in this.m_Drone.WeaponBanks.ToList<WeaponBank>())
			{
				state.PostSetProp("RequestFireStateChange", true);
				state.PostSetProp("DisableAllTurrets", true);
				this.m_AttackRange = Math.Max(state.Weapon.RangeTable.Effective.Range, this.m_AttackRange);
				this.m_WeaponsDisabled = true;
			}
			this.m_AttackRange = Math.Min(this.m_AttackRange, 1000f);
		}

		public override void Terminate()
		{
		}

		public override void ObjectRemoved(IGameObject obj)
		{
			if (this.m_Drone == obj)
				this.m_Drone = (Ship)null;
			if (this.m_Target != obj)
				return;
			this.SetTarget((IGameObject)null);
		}

		public override void OnThink()
		{
			if (this.m_Drone == null)
				return;
			switch (this.m_State)
			{
				case DroneTrapState.SEEK:
					this.ThinkSeek();
					break;
				case DroneTrapState.TRACK:
					this.ThinkTrack();
					break;
				case DroneTrapState.DRAG:
					this.ThinkDrag();
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
			if (this.m_Target != null || this.m_State != DroneTrapState.SEEK)
				return this.m_TrapPlanet == null;
			return true;
		}

		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			float num1 = float.MaxValue;
			float num2 = float.MaxValue;
			Ship ship1 = (Ship)null;
			int num3 = -1;
			foreach (IGameObject gameObject in objs)
			{
				if (gameObject is Ship)
				{
					Ship ship2 = gameObject as Ship;
					if (ship2.Player != this.m_Drone.Player && Ship.IsActiveShip(ship2) && ship2.IsDetected(this.m_Drone.Player))
					{
						int num4 = ColonyTrapDroneControl.TrapPriorityValue(ship2);
						float lengthSquared = (ship2.Position - this.m_Drone.Position).LengthSquared;
						if (num4 == num3 && (double)lengthSquared < (double)num1 || num4 > num3)
						{
							ship1 = ship2;
							num1 = lengthSquared;
							num3 = num4;
						}
					}
				}
				else if (gameObject is StellarBody && (this.m_TrapPlanet == null || this.m_TrapPlanet.PlanetInfo.ID != this.m_PlanetID))
				{
					StellarBody stellarBody = gameObject as StellarBody;
					float lengthSquared = (stellarBody.Parameters.Position - this.m_Drone.Position).LengthSquared;
					if ((double)lengthSquared < (double)num2 || stellarBody.PlanetInfo.ID == this.m_PlanetID)
					{
						num2 = lengthSquared;
						this.m_TrapPlanet = stellarBody;
					}
				}
			}
			if (ship1 == null)
				return;
			this.SetTarget((IGameObject)ship1);
		}

		public override bool NeedsAParent()
		{
			return false;
		}

		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
		}

		public bool RequestingHelpWithTarget()
		{
			if (this.m_Target != null && this.m_State == DroneTrapState.DRAG)
				return this.m_RequiresHelp;
			return false;
		}

		public void CheckIfHelpIsRequired()
		{
			if (this.m_Controller == null || this.RequestingHelpWithTarget())
				return;
			using (List<ColonyTrapDroneControl>.Enumerator enumerator = this.m_Controller.CombatAIControllers.OfType<ColonyTrapDroneControl>().ToList<ColonyTrapDroneControl>().GetEnumerator())
			{
				do
					;
				while (enumerator.MoveNext() && !enumerator.Current.RequestingHelpWithTarget());
			}
		}

		private void ThinkSeek()
		{
			if (this.m_Target != null)
			{
				this.m_State = DroneTrapState.TRACK;
			}
			else
			{
				Vector3 targetPos = this.m_TrapPlanet != null ? this.m_TrapPlanet.Parameters.Position : this.m_DefaultTestPlanetPos;
				if ((double)(this.m_Drone.Maneuvering.Destination - targetPos).LengthSquared <= 100.0)
					return;
				this.m_Drone.Maneuvering.PostAddGoal(targetPos, -Vector3.UnitZ);
			}
		}

		private void ThinkTrack()
		{
			if (this.m_Target == null)
			{
				this.m_Drone.SetShipTarget(0, Vector3.Zero, true, 0);
				this.m_State = DroneTrapState.SEEK;
			}
			else if (this.m_Drone.ListenTurretFiring == Turret.FiringEnum.Firing)
			{
				this.m_State = DroneTrapState.DRAG;
			}
			else
			{
				bool flag = (double)(this.m_Drone.Position - this.m_Target.Position).LengthSquared > (double)this.m_AttackRange * (double)this.m_AttackRange;
				if (this.m_WeaponsDisabled != flag)
				{
					foreach (IGameObject state in this.m_Drone.WeaponBanks.ToList<WeaponBank>())
						state.PostSetProp("DisableAllTurrets", flag);
					this.m_WeaponsDisabled = flag;
				}
				Vector3 targetPos = this.m_Target.Position + Vector3.Normalize(this.m_DefaultTestPlanetPos - this.m_Target.Position) * (this.m_AttackRange * 0.75f);
				Vector3 look = Vector3.Normalize(this.m_Target.Position - this.m_Drone.Position);
				if ((double)(this.m_Drone.Maneuvering.Destination - targetPos).LengthSquared <= 2500.0)
					return;
				this.m_Drone.Maneuvering.PostAddGoal(targetPos, look);
			}
		}

		private void ThinkDrag()
		{
			if (this.m_Target == null || this.m_Drone.ListenTurretFiring != Turret.FiringEnum.Firing)
			{
				this.SetTarget((IGameObject)null);
				this.m_State = DroneTrapState.SEEK;
				foreach (IGameObject state in this.m_Drone.WeaponBanks.ToList<WeaponBank>())
					state.PostSetProp("DisableAllTurrets", true);
				this.m_WeaponsDisabled = true;
			}
			else
			{
				Vector3 defaultTestPlanetPos = this.m_DefaultTestPlanetPos;
				Vector3 look = defaultTestPlanetPos - this.m_Target.Position;
				double num1 = (double)look.Normalize();
				Vector3 targetPos = defaultTestPlanetPos;
				if ((double)(this.m_Drone.Maneuvering.Destination - targetPos).LengthSquared > 2500.0)
					this.m_Drone.Maneuvering.PostAddGoal(targetPos, look);
				if ((double)(this.m_Target.Position - targetPos).LengthSquared > (double)this.m_PrevDistFromPlanet)
					--this.m_RequestHelpDelay;
				else
					this.m_RequestHelpDelay = 200;
				this.m_RequiresHelp = this.m_RequestHelpDelay <= 0;
				float num2 = (float)((this.m_TrapPlanet != null ? (double)this.m_TrapPlanet.Parameters.Radius : 500.0) + (double)this.m_Target.ShipSphere.radius * 0.5);
				if ((double)(this.m_Target.Position - defaultTestPlanetPos).LengthSquared >= (double)num2 * (double)num2)
					return;
				this.m_Target.KillShip(true);
			}
		}
	}
}
