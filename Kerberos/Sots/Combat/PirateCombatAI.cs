// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.PirateCombatAI
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Combat
{
	internal class PirateCombatAI : CombatAI
	{
		private List<BoardingPodLaunchControl> m_BoardingPodControls;
		private List<DroneLaunchControl> m_DroneControls;
		private bool m_IsPirateBase;

		public PirateCombatAI(
		  App game,
		  Player player,
		  bool playerControlled,
		  Kerberos.Sots.GameStates.StarSystem starSystem,
		  Dictionary<int, DiplomacyState> diploStates)
		  : base(game, player, playerControlled, starSystem, diploStates, false)
		{
			this.SetAIType(OverallAIType.PIRATE);
			this.m_BoardingPodControls = new List<BoardingPodLaunchControl>();
			this.m_DroneControls = new List<DroneLaunchControl>();
			this.m_IsPirateBase = game.GameDatabase.GetPirateBaseInfos().FirstOrDefault<PirateBaseInfo>((Func<PirateBaseInfo, bool>)(x => x.SystemId == starSystem.SystemID)) != null;
		}

		public override void Update(List<IGameObject> objs)
		{
			base.Update(objs);
			if (this.m_IsPirateBase)
				return;
			this.m_BoardingPodControls = this.m_ShipWeaponControls.OfType<BoardingPodLaunchControl>().ToList<BoardingPodLaunchControl>();
			this.m_DroneControls = this.m_ShipWeaponControls.OfType<DroneLaunchControl>().ToList<DroneLaunchControl>();
			foreach (BoardingPodLaunchControl boardingPodControl in this.m_BoardingPodControls)
			{
				if (boardingPodControl.ControlledShip != null)
				{
					if (boardingPodControl.ControlledShip.Target is Ship)
					{
						Ship target = boardingPodControl.ControlledShip.Target as Ship;
						boardingPodControl.DisableWeaponFire = target.ShipRole != ShipRole.FREIGHTER;
					}
					else
						boardingPodControl.DisableWeaponFire = true;
				}
			}
			foreach (DroneLaunchControl droneControl in this.m_DroneControls)
			{
				if (droneControl.ControlledShip != null)
				{
					if (droneControl.ControlledShip.Target is Ship)
					{
						Ship target = droneControl.ControlledShip.Target as Ship;
						droneControl.DisableWeaponFire = target.ShipRole == ShipRole.FREIGHTER && this.m_BoardingPodControls.Count > 0;
					}
					else
						droneControl.DisableWeaponFire = true;
				}
			}
			bool flag = this.m_Enemy.Any<Ship>((Func<Ship, bool>)(x => x.ShipRole == ShipRole.FREIGHTER));
			if (this.m_bEnemyShipsInSystem && flag)
				return;
			foreach (TaskGroup taskGroup in this.m_TaskGroups)
			{
				if (!(taskGroup.Objective is RetreatObjective))
				{
					TacticalObjective retreatObjective1 = this.GetRetreatObjective(taskGroup);
					if (retreatObjective1 is RetreatObjective)
					{
						RetreatObjective retreatObjective2 = retreatObjective1 as RetreatObjective;
						retreatObjective2.ResetRetreatPosition(taskGroup);
						taskGroup.Objective = (TacticalObjective)retreatObjective2;
					}
				}
			}
		}
	}
}
