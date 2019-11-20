// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.NodeCannonAttackControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using System;
using System.Linq;

namespace Kerberos.Sots.Combat
{
	internal class NodeCannonAttackControl : PositionalAttackControl
	{
		private float m_EffectRange;
		private float m_MaxRange;

		public NodeCannonAttackControl(
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
			this.m_EffectRange = weaponBank.Weapon.GravityAffectRange;
			this.m_MaxRange = weaponBank.Weapon.Range;
		}

		protected override bool IsReadyToFire()
		{
			if (this.m_Ship.TaskGroup == null || this.m_Ship.TaskGroup.Objective.m_TargetEnemyGroup == null || (double)this.m_Ship.Maneuvering.Velocity.LengthSquared > 100.0)
				return false;
			Vector3 desiredTargetPosition = this.GetDesiredTargetPosition(this.m_Ship.TaskGroup.Objective.m_TargetEnemyGroup);
			if ((double)(this.m_Ship.Position - desiredTargetPosition).LengthSquared > (double)this.m_MaxRange * (double)this.m_MaxRange)
				return false;
			bool flag = false;
			foreach (Ship friendlyShip in this.m_CommanderAI.GetFriendlyShips())
			{
				float num = (float)((double)this.m_EffectRange + (double)friendlyShip.ShipSphere.radius + 200.0);
				if (Ship.IsActiveShip(friendlyShip) && (double)(friendlyShip.Position - desiredTargetPosition).LengthSquared < (double)num * (double)num)
				{
					flag = true;
					break;
				}
			}
			if (flag)
				return false;
			if (this.m_Ship.TaskGroup.Objective.m_TargetEnemyGroup.m_Ships.Count > 3)
				return true;
			int num1 = 0;
			foreach (Ship ship in this.m_Ship.TaskGroup.Objective.m_TargetEnemyGroup.m_Ships)
			{
				if ((double)(ship.Position - desiredTargetPosition).LengthSquared < (double)this.m_EffectRange * (double)this.m_EffectRange)
				{
					if (Ship.IsShipClassBigger(ship.ShipClass, ShipClass.Cruiser, false))
						num1 += 3;
					else
						++num1;
				}
			}
			return num1 > 3;
		}

		protected override Vector3 GetDesiredTargetPosition(EnemyGroup enemy)
		{
			Vector3 vector3_1 = enemy.m_LastKnownPosition;
			if ((double)(enemy.m_LastKnownDestination - enemy.m_LastKnownPosition).LengthSquared < (double)(enemy.m_LastKnownPosition - vector3_1).LengthSquared)
				vector3_1 = enemy.m_LastKnownDestination;
			Vector3 vector3_2 = this.m_Ship.Maneuvering.Position - vector3_1;
			float num1 = vector3_2.Normalize();
			float num2 = (float)((double)this.m_EffectRange + (double)this.m_Ship.ShipSphere.radius + 500.0);
			if ((double)num1 < (double)num2)
				vector3_1 = this.m_Ship.Maneuvering.Position + vector3_2 * num2;
			return vector3_1;
		}
	}
}
