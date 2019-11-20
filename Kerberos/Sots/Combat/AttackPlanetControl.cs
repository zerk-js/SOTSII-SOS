// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.AttackPlanetControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using System;
using System.Linq;

namespace Kerberos.Sots.Combat
{
	internal class AttackPlanetControl : SpecWeaponControl
	{
		private int m_WeaponID;
		private StellarBody m_TargetPlanet;

		public AttackPlanetControl(
		  App game,
		  CombatAI commanderAI,
		  Ship ship,
		  WeaponEnums.TurretClasses weaponType)
		  : base(game, commanderAI, ship, weaponType)
		{
			this.m_TargetPlanet = (StellarBody)null;
			WeaponBank weaponBank = ship.WeaponBanks.FirstOrDefault<WeaponBank>((Func<WeaponBank, bool>)(x => x.TurretClass == weaponType));
			if (weaponBank == null)
				return;
			this.m_WeaponID = weaponBank.Weapon.UniqueWeaponID;
		}

		public override void Update(int framesElapsed)
		{
			if (this.m_Ship.Target != null && this.m_Ship.Target is StellarBody && !this.m_DisableWeaponFire)
			{
				if (this.m_TargetPlanet == this.m_Ship.Target)
					return;
				this.m_TargetPlanet = this.m_Ship.Target as StellarBody;
				this.m_Ship.SetShipWeaponToggleOn(this.m_WeaponID, true);
				this.m_Ship.SetShipSpecWeaponTarget(this.m_WeaponID, this.m_TargetPlanet.ObjectID, Vector3.Zero);
			}
			else
			{
				this.m_Ship.SetShipWeaponToggleOn(this.m_WeaponID, false);
				this.m_TargetPlanet = (StellarBody)null;
			}
		}
	}
}
