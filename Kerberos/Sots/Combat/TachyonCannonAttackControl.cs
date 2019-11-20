// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.TachyonCannonAttackControl
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
	internal class TachyonCannonAttackControl : PositionalAttackControl
	{
		private int m_EnemyGroupStrngth = 27;
		private float m_MaxRange;

		public TachyonCannonAttackControl(
		  App game,
		  CombatAI commanderAI,
		  Ship ship,
		  int weaponID,
		  WeaponEnums.TurretClasses weaponType)
		  : base(game, commanderAI, ship, weaponID, weaponType)
		{
			WeaponBank weaponBank = ship.WeaponBanks.FirstOrDefault<WeaponBank>((Func<WeaponBank, bool>)(x => x.Weapon.UniqueWeaponID == weaponID));
			if (weaponBank == null)
				return;
			this.m_MaxRange = weaponBank.Weapon.Range;
		}

		protected override bool IsReadyToFire()
		{
			if (this.m_Ship.TaskGroup == null || this.m_Ship.TaskGroup.Objective.m_TargetEnemyGroup == null || (double)this.m_Ship.Maneuvering.Velocity.LengthSquared > 100.0)
				return false;
			Vector3 desiredTargetPosition = this.GetDesiredTargetPosition(this.m_Ship.TaskGroup.Objective.m_TargetEnemyGroup);
			float num = this.m_MaxRange * 0.75f;
			Vector3 v1 = desiredTargetPosition - this.m_Ship.Position;
			v1.Y = 0.0f;
			if ((double)v1.Normalize() > (double)num)
				return false;
			bool flag = false;
			foreach (Ship friendlyShip in this.m_CommanderAI.GetFriendlyShips())
			{
				if (Ship.IsActiveShip(friendlyShip))
				{
					Vector3 v0 = friendlyShip.Position - this.m_Ship.Position;
					v0.Y = 0.0f;
					if ((double)v0.Normalize() <= (double)this.m_MaxRange && (double)Vector3.Dot(v0, v1) > 0.899999976158142)
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
				return false;
			if (CombatAI.AssessGroupStrength(this.m_Ship.TaskGroup.Objective.m_TargetEnemyGroup.m_Ships) < this.m_EnemyGroupStrngth)
				return true;
			List<Ship> shipList = new List<Ship>();
			foreach (Ship ship in this.m_Ship.TaskGroup.Objective.m_TargetEnemyGroup.m_Ships)
			{
				Vector3 v0 = ship.Position - this.m_Ship.Position;
				v0.Y = 0.0f;
				if ((double)v0.Normalize() <= (double)this.m_MaxRange && (double)Vector3.Dot(v0, v1) > 0.899999976158142)
					shipList.Add(ship);
			}
			return CombatAI.AssessGroupStrength(this.m_Ship.TaskGroup.Objective.m_TargetEnemyGroup.m_Ships) >= this.m_EnemyGroupStrngth;
		}

		protected override Vector3 GetDesiredTargetPosition(EnemyGroup enemy)
		{
			Vector3 vector3 = enemy.m_LastKnownPosition;
			if ((double)(enemy.m_LastKnownDestination - enemy.m_LastKnownPosition).LengthSquared < (double)(enemy.m_LastKnownPosition - vector3).LengthSquared)
				vector3 = enemy.m_LastKnownDestination;
			return this.m_Ship.Maneuvering.Position + Vector3.Normalize(this.m_Ship.Maneuvering.Position - vector3) * this.m_MaxRange;
		}
	}
}
