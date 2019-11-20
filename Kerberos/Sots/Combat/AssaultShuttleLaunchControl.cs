// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.AssaultShuttleLaunchControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;

namespace Kerberos.Sots.Combat
{
	internal class AssaultShuttleLaunchControl : BattleRiderLaunchControl
	{
		public bool m_AutoDisabled = true;

		public AssaultShuttleLaunchControl(App game, CombatAI commanderAI, Ship ship)
		  : base(game, commanderAI, ship, WeaponEnums.TurretClasses.AssaultShuttle)
		{
			this.m_CurrMaxLaunchDelay = BattleRiderLaunchControl.kMinRiderHoldDuration;
			this.m_LaunchDelay = this.m_CurrMaxLaunchDelay;
			this.m_AutoDisabled = true;
			if (this.m_Ship == null)
				return;
			this.m_Ship.PostSetProp("SetDisableAutoLaunching", true);
		}

		public override void UpdateBattleRiderWeaponControl(int framesElapsed)
		{
			if (this.m_Ship == null)
				return;
			StellarBody body = this.m_Ship.Target is StellarBody ? this.m_Ship.Target as StellarBody : this.PlanetTargetInRange();
			if (body == null)
			{
				if (this.CarrierCanLaunch())
					return;
				if (!this.m_AutoDisabled)
				{
					this.m_Ship.PostSetProp("SetDisableAutoLaunching", true);
					this.m_AutoDisabled = true;
				}
				this.RecoverRiders();
			}
			else if (this.CarrierCanLaunch() && AssaultShuttleLaunchControl.TargetInRange(body, this.m_Ship.Position, this.m_MinAttackDist))
			{
				if (this.m_AutoDisabled)
				{
					this.m_Ship.PostSetProp("SetDisableAutoLaunching", false);
					this.m_AutoDisabled = false;
				}
				this.m_LaunchDelay -= framesElapsed;
				if (this.m_LaunchDelay > 0)
					return;
				this.LaunchRiders((IGameObject)body);
			}
			else
				this.m_LaunchDelay = this.m_CurrMaxLaunchDelay;
		}

		private static bool TargetInRange(StellarBody body, Vector3 pos, float range)
		{
			if (body == null)
				return false;
			float num = body.Parameters.Radius + 750f + range;
			return (double)(body.Parameters.Position - pos).LengthSquared < (double)num * (double)num;
		}
	}
}
