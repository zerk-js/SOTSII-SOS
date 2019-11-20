// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.DroneLaunchControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using System.Linq;

namespace Kerberos.Sots.Combat
{
	internal class DroneLaunchControl : BattleRiderLaunchControl
	{
		public DroneLaunchControl(App game, CombatAI commanderAI, Ship ship)
		  : base(game, commanderAI, ship, WeaponEnums.TurretClasses.Drone)
		{
			this.m_CurrMaxLaunchDelay = commanderAI.AIRandom.NextInclusive(BattleRiderLaunchControl.kMinRiderHoldDuration, BattleRiderLaunchControl.kMinRiderHoldDuration * 3);
			this.m_LaunchDelay = this.m_CurrMaxLaunchDelay;
		}

		public override void UpdateBattleRiderWeaponControl(int framesElapsed)
		{
			if ((this.m_Ship.Target == null || !(this.m_Ship.Target is Ship)) && this.m_CommanderAI.GetEnemyGroups().Count<EnemyGroup>() <= 0)
			{
				if (this.CarrierCanLaunch())
					return;
				this.RecoverRiders();
			}
			else if (this.CarrierCanLaunch())
			{
				IGameObject target = this.m_Ship.Target != null ? this.m_Ship.Target : (IGameObject)this.ShipTargetInRange();
				if (target == null)
					return;
				this.m_LaunchDelay -= framesElapsed;
				if (this.m_LaunchDelay > 0)
					return;
				this.LaunchRiders(target);
			}
			else
				this.m_LaunchDelay = this.m_CurrMaxLaunchDelay;
		}
	}
}
