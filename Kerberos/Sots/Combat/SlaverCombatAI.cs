// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.SlaverCombatAI
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Combat
{
	internal class SlaverCombatAI : CombatAI
	{
		private List<Ship> m_SlaverShips;

		public SlaverCombatAI(
		  App game,
		  Player player,
		  bool playerControlled,
		  Kerberos.Sots.GameStates.StarSystem starSystem,
		  Dictionary<int, DiplomacyState> diploStates)
		  : base(game, player, playerControlled, starSystem, diploStates, false)
		{
			this.SetAIType(OverallAIType.SLAVER);
			this.m_SlaverShips = new List<Ship>();
		}

		public override void Update(List<IGameObject> objs)
		{
			base.Update(objs);
			int num = 0;
			foreach (IGameObject gameObject in objs)
			{
				if (gameObject is Ship)
				{
					Ship ship = gameObject as Ship;
					if (ship.Player == this.m_Player && ship.Active && !ship.IsDestroyed)
					{
						if (ship.IsBattleRider && ship.BattleRiderType == BattleRiderTypes.assaultshuttle)
						{
							Ship ship1 = this.m_Friendly.FirstOrDefault<Ship>((Func<Ship, bool>)(x => x.DatabaseID == ship.ParentDatabaseID));
							if (ship1 != null && ship1.CombatStance != CombatStance.RETREAT)
								++num;
						}
						if (ship.IsWraithAbductor && ship.CombatStance != CombatStance.RETREAT)
							++num;
						if (!this.m_SlaverShips.Contains(ship))
						{
							this.m_SlaverShips.Add(ship);
							if (ship.WeaponControls != null)
							{
								foreach (AssaultShuttleLaunchControl shuttleLaunchControl in ship.WeaponControls.OfType<AssaultShuttleLaunchControl>().ToList<AssaultShuttleLaunchControl>())
									shuttleLaunchControl.SetMinAttackRange(Math.Min(5000f, shuttleLaunchControl.GetMinAttackRange()));
							}
						}
					}
				}
			}
			if ((this.m_Friendly.Count <= 0 || num != 0) && this.m_Planets.Sum<StellarBody>((Func<StellarBody, double>)(x => x.Population)) != 0.0)
				return;
			foreach (TaskGroup taskGroup in this.m_TaskGroups)
			{
				if (!(taskGroup.Objective is RetreatObjective))
					taskGroup.Objective = this.GetRetreatObjective(taskGroup);
			}
		}

		public override DiplomacyState GetDiplomacyState(int playerID)
		{
			return playerID != this.m_Player.ID ? DiplomacyState.WAR : DiplomacyState.PEACE;
		}
	}
}
