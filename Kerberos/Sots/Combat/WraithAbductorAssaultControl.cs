// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.WraithAbductorAssaultControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;

namespace Kerberos.Sots.Combat
{
	internal class WraithAbductorAssaultControl : SpecWeaponControl
	{
		private StellarBody m_TargetPlanet;
		private int m_LaunchDelay;
		private int m_CurrMaxLaunchDelay;
		private float m_MinAttackDist;
		private bool m_Assaulting;

		public WraithAbductorAssaultControl(App game, CombatAI commanderAI, Ship ship)
		  : base(game, commanderAI, ship, WeaponEnums.TurretClasses.AssaultShuttle)
		{
			this.m_Ship = ship;
			this.m_CurrMaxLaunchDelay = 300;
			this.m_LaunchDelay = this.m_CurrMaxLaunchDelay;
			this.m_MinAttackDist = (float)((double)ship.MissionSection.ShipSectionAsset.MissionTime * (double)ship.Maneuvering.MaxShipSpeed * 0.5);
			this.m_Assaulting = false;
		}

		public override void ObjectRemoved(IGameObject obj)
		{
			if (obj == this.m_TargetPlanet)
				this.m_TargetPlanet = (StellarBody)null;
			base.ObjectRemoved(obj);
		}

		public override bool RemoveWeaponControl()
		{
			return this.m_Ship == null;
		}

		public override void Update(int framesElapsed)
		{
			if (this.m_CommanderAI.GetClosestEnemyPlanet(this.m_Ship.Maneuvering.Position, this.m_MinAttackDist) != null)
			{
				if (!this.m_Ship.AssaultingPlanet)
				{
					this.m_LaunchDelay -= framesElapsed;
					if (this.m_LaunchDelay <= 0)
						this.m_Ship.PostSetProp("LaunchBattleriders");
				}
				else
					this.m_LaunchDelay = this.m_CurrMaxLaunchDelay;
			}
			else
			{
				this.m_LaunchDelay = this.m_CurrMaxLaunchDelay;
				if (this.m_Ship.AssaultingPlanet)
					this.m_Ship.PostSetProp("RecoverBattleriders");
			}
			if (this.m_CommanderAI.GetAIType() == OverallAIType.SLAVER && this.m_Assaulting != this.m_Ship.AssaultingPlanet && this.m_Assaulting)
			{
				this.m_Ship.SetCombatStance(CombatStance.RETREAT);
				this.m_Ship.TaskGroup = (TaskGroup)null;
			}
			this.m_Assaulting = this.m_Ship.AssaultingPlanet;
		}

		public bool CanAssaultPlanet()
		{
			if (this.m_Ship.Maneuvering.SpeedState != ShipSpeedState.Overthrust && this.m_Ship.CombatStance != CombatStance.RETREAT)
				return this.m_Ship.Target != null;
			return false;
		}
	}
}
