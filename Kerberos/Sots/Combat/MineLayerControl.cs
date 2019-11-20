// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.MineLayerControl
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
	internal class MineLayerControl : SpecWeaponControl
	{
		private Vector3 m_MaxPos;
		private Vector3 m_MinPos;
		private int m_WeaponID;
		private bool m_ForceOn;

		public bool ForceOn
		{
			get
			{
				return this.m_ForceOn;
			}
			set
			{
				this.m_ForceOn = value;
			}
		}

		public MineLayerControl(
		  App game,
		  CombatAI commanderAI,
		  Ship ship,
		  WeaponEnums.TurretClasses weaponType)
		  : base(game, commanderAI, ship, weaponType)
		{
			this.m_ForceOn = false;
			WeaponBank weaponBank = ship.WeaponBanks.FirstOrDefault<WeaponBank>((Func<WeaponBank, bool>)(x => x.TurretClass == weaponType));
			if (weaponBank != null)
				this.m_WeaponID = weaponBank.Weapon.UniqueWeaponID;
			this.m_MaxPos = Vector3.Zero;
			this.m_MinPos = Vector3.Zero;
		}

		public void SetMineLayingArea(Vector3 maxPos, Vector3 minPos)
		{
			this.m_MaxPos = maxPos;
			this.m_MinPos = minPos;
		}

		public override void Update(int framesElapsed)
		{
			Vector3 position = this.m_Ship.Maneuvering.Position;
			this.m_Ship.SetShipWeaponToggleOn(this.m_WeaponID, !this.m_DisableWeaponFire && (this.m_ForceOn || (double)position.X < (double)this.m_MaxPos.X && (double)position.Y < (double)this.m_MaxPos.Y && ((double)position.Z < (double)this.m_MaxPos.Z && (double)position.X > (double)this.m_MinPos.X) && (double)position.Y > (double)this.m_MinPos.Y && (double)position.Z > (double)this.m_MinPos.Z));
		}
	}
}
