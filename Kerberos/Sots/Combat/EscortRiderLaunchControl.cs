﻿// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.EscortRiderLaunchControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.GameObjects;

namespace Kerberos.Sots.Combat
{
	internal class EscortRiderLaunchControl : BattleRiderLaunchControl
	{
		public EscortRiderLaunchControl(
		  App game,
		  CombatAI commanderAI,
		  Ship ship,
		  WeaponEnums.TurretClasses weaponType)
		  : base(game, commanderAI, ship, weaponType)
		{
			this.m_CurrMaxLaunchDelay = BattleRiderLaunchControl.kMinRiderHoldDuration * 5;
			this.m_LaunchDelay = this.m_CurrMaxLaunchDelay;
		}

		public override void UpdateBattleRiderWeaponControl(int framesElapsed)
		{
			if (this.m_Ship.Target != null && this.m_Ship.Target is Ship && this.CarrierCanLaunch())
			{
				if ((double)(this.m_Ship.Maneuvering.Position - (this.m_Ship.Target as Ship).Maneuvering.Position).LengthSquared >= (double)this.m_MinAttackDist * (double)this.m_MinAttackDist)
					return;
				this.m_LaunchDelay -= framesElapsed;
				if (this.m_LaunchDelay > 0)
					return;
				this.LaunchRiders(this.m_Ship.Target);
			}
			else
				this.m_LaunchDelay = this.m_CurrMaxLaunchDelay;
		}
	}
}
