// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.AttackRiderLaunchControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using System.Linq;

namespace Kerberos.Sots.Combat
{
	internal class AttackRiderLaunchControl : BattleRiderLaunchControl
	{
		public AttackRiderLaunchControl(
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
			if ((this.m_Ship.Target != null && this.m_Ship.Target is Ship || this.m_CommanderAI.GetEnemyGroups().Count<EnemyGroup>() > 0) && this.CarrierCanLaunch())
			{
				IGameObject target = this.m_Ship.Target != null ? this.m_Ship.Target : (IGameObject)this.ShipTargetInRange();
				if (target == null)
					return;
				this.m_LaunchDelay -= framesElapsed;
				if (this.m_LaunchDelay > 0 && this.m_HasLaunchedBefore)
					return;
				this.LaunchRiders(target);
			}
			else
			{
				if (this.AllRidersAreLaunched())
					return;
				this.m_LaunchDelay = this.m_CurrMaxLaunchDelay;
			}
		}
	}
}
