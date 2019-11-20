// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.SystemDefenseBoatControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;

namespace Kerberos.Sots.Combat
{
	internal class SystemDefenseBoatControl : SpecWeaponControl
	{
		private Vector3 m_Pos;
		private float m_MinRange;

		public SystemDefenseBoatControl(App game, CombatAI commanderAI, Ship ship, StellarBody planet)
		  : base(game, commanderAI, ship, WeaponEnums.TurretClasses.Standard)
		{
			this.m_MinRange = 10000f;
			if (planet != null)
			{
				this.m_MinRange += planet.Parameters.Radius;
				this.m_Pos = planet.Parameters.Position;
			}
			else
				this.m_Pos = ship.Position;
		}

		public override bool RemoveWeaponControl()
		{
			if (this.m_Ship != null)
				return this.m_Ship.DefenseBoatActive;
			return true;
		}

		public override void Update(int framesElapsed)
		{
			bool flag = false;
			foreach (EnemyGroup enemyGroup in this.m_CommanderAI.GetEnemyGroups())
			{
				if (enemyGroup.GetClosestShip(this.m_Pos, this.m_MinRange) != null)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
				return;
			this.m_Ship.DefenseBoatActive = true;
		}
	}
}
