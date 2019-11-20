// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.PositionalAttackControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Combat
{
	internal class PositionalAttackControl : SpecWeaponControl
	{
		private static int kMaxHoldShipDelay = 720;
		private float m_WeaponSpeed;
		private int m_WeaponID;
		private int m_HoldShipDelay;
		private bool m_TargetSet;
		private bool m_DetonatingWeapon;
		private Vector3 m_TargetPosition;

		public int WeaponID
		{
			get
			{
				return this.m_WeaponID;
			}
		}

		public PositionalAttackControl(
		  App game,
		  CombatAI commanderAI,
		  Ship ship,
		  int weaponID,
		  WeaponEnums.TurretClasses weaponType)
		  : base(game, commanderAI, ship, weaponType)
		{
			this.m_DetonatingWeapon = false;
			WeaponBank weaponBank = ship.WeaponBanks.FirstOrDefault<WeaponBank>((Func<WeaponBank, bool>)(x => x.Weapon.UniqueWeaponID == weaponID));
			if (weaponBank != null)
			{
				this.m_WeaponSpeed = weaponBank.Weapon.Speed;
				this.m_WeaponID = weaponBank.Weapon.UniqueWeaponID;
				this.m_DetonatingWeapon = ((IEnumerable<WeaponEnums.WeaponTraits>)weaponBank.Weapon.Traits).Contains<WeaponEnums.WeaponTraits>(WeaponEnums.WeaponTraits.Detonating);
			}
			this.m_TargetSet = false;
			this.m_TargetPosition = Vector3.Zero;
			this.m_HoldShipDelay = 0;
		}

		public override void Update(int framesElapsed)
		{
			if (this.m_DisableWeaponFire)
				return;
			this.m_HoldShipDelay -= framesElapsed;
			this.m_RequestHoldShip = this.m_HoldShipDelay > 0;
			if (this.m_Ship.TaskGroup == null || !(this.m_Ship.TaskGroup.Objective is AttackGroupObjective) || this.m_Ship.TaskGroup.Objective.m_TargetEnemyGroup == null)
				return;
			if (!this.IsReadyToFire())
			{
				this.m_TargetSet = false;
				this.m_Ship.SetShipWeaponToggleOn(this.m_WeaponID, false);
				this.m_HoldShipDelay = PositionalAttackControl.kMaxHoldShipDelay;
			}
			else
			{
				Vector3 desiredTargetPosition = this.GetDesiredTargetPosition(this.m_Ship.TaskGroup.Objective.m_TargetEnemyGroup);
				if (this.m_TargetSet && (double)(this.m_TargetPosition - desiredTargetPosition).LengthSquared <= 250000.0)
					return;
				this.m_TargetPosition = desiredTargetPosition;
				this.m_Ship.SetShipWeaponToggleOn(this.m_WeaponID, true);
				this.m_Ship.SetShipPositionalTarget(this.m_WeaponID, desiredTargetPosition, true);
				this.m_TargetSet = true;
				this.m_HoldShipDelay = PositionalAttackControl.kMaxHoldShipDelay;
			}
		}

		protected virtual bool IsReadyToFire()
		{
			if (!this.m_DetonatingWeapon)
				return (double)this.m_Ship.Maneuvering.Velocity.LengthSquared < 100.0;
			return true;
		}

		protected virtual Vector3 GetDesiredTargetPosition(EnemyGroup enemy)
		{
			Vector3 vector3 = enemy.m_LastKnownPosition;
			if ((double)this.m_WeaponSpeed > 0.0)
			{
				float num = (this.m_Ship.Maneuvering.Position - vector3).Length / this.m_WeaponSpeed;
				vector3 += enemy.m_LastKnownHeading * num;
				if ((double)(enemy.m_LastKnownDestination - enemy.m_LastKnownPosition).LengthSquared < (double)(enemy.m_LastKnownPosition - vector3).LengthSquared)
					vector3 = enemy.m_LastKnownDestination;
			}
			return vector3;
		}
	}
}
