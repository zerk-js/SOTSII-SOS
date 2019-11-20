// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.BioMissileLaunchControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;

namespace Kerberos.Sots.Combat
{
	internal class BioMissileLaunchControl : BattleRiderLaunchControl
	{
		public BioMissileLaunchControl(App game, CombatAI commanderAI, Ship ship)
		  : base(game, commanderAI, ship, WeaponEnums.TurretClasses.Biomissile)
		{
		}

		public override void UpdateBattleRiderWeaponControl(int framesElapsed)
		{
			if (this.m_Ship.Target == null || !(this.m_Ship.Target is StellarBody) || !this.CarrierCanLaunch() || (double)(this.m_Ship.Maneuvering.Position - (this.m_Ship.Target as StellarBody).Parameters.Position).LengthSquared >= (double)this.m_MinAttackDist * (double)this.m_MinAttackDist)
				return;
			this.LaunchRiders(this.m_Ship.Target);
		}
	}
}
