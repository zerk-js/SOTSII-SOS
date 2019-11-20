// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.NPCFactionCombatAI
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Combat
{
	internal class NPCFactionCombatAI : CombatAI
	{
		internal List<int> PlanetsAttackedByNPC = new List<int>();
		protected int m_SystemID;
		protected List<CombatAIController> m_CombatAIControls;
		internal int NumAddedResources;
		internal int NumPlanetStruckAsteroids;

		public List<CombatAIController> CombatAIControllers
		{
			get
			{
				return this.m_CombatAIControls;
			}
		}

		public NPCFactionCombatAI(
		  App game,
		  Player player,
		  bool playerControlled,
		  int systemID,
		  Kerberos.Sots.GameStates.StarSystem starSystem,
		  Dictionary<int, DiplomacyState> diploStates)
		  : base(game, player, playerControlled, starSystem, diploStates, false)
		{
			this.m_SystemID = systemID;
			this.m_CombatAIControls = new List<CombatAIController>();
		}

		public override void Shutdown()
		{
			foreach (CombatAIController combatAiControl in this.m_CombatAIControls)
				combatAiControl.Terminate();
			this.m_CombatAIControls.Clear();
			base.Shutdown();
		}

		public override void ObjectRemoved(IGameObject obj)
		{
			base.ObjectRemoved(obj);
			foreach (CombatAIController combatAiControl in this.m_CombatAIControls)
			{
				if (combatAiControl.GetShip() == obj)
				{
					combatAiControl.ObjectRemoved(obj);
					this.m_CombatAIControls.Remove(combatAiControl);
					if (combatAiControl.GetType() == typeof(MeteorCombatAIControl))
					{
						this.NumAddedResources += ((MeteorCombatAIControl)combatAiControl).m_AddedResources;
						if (((MeteorCombatAIControl)combatAiControl).m_StruckPlanet)
						{
							++this.NumPlanetStruckAsteroids;
							break;
						}
						break;
					}
					break;
				}
			}
			foreach (CombatAIController combatAiControl in this.m_CombatAIControls)
				combatAiControl.ObjectRemoved(obj);
		}

		public override bool VictoryConditionsAreMet()
		{
			bool flag = true;
			if (this.m_CombatAIControls.Count == 0)
				return false;
			foreach (CombatAIController combatAiControl in this.m_CombatAIControls)
			{
				if (!combatAiControl.VictoryConditionIsMet())
				{
					flag = false;
					break;
				}
			}
			return flag;
		}

		public override void Update(List<IGameObject> objs)
		{
			if (!App.m_bAI_Enabled || this.m_bIsHumanPlayerControlled)
				return;
			List<IGameObject> gameObjectList = new List<IGameObject>();
			foreach (IGameObject gameObject in objs)
			{
				if (gameObject is Ship)
				{
					Ship ship = gameObject as Ship;
					gameObjectList.Add((IGameObject)ship);
					if (ship.Player == this.m_Player && !this.m_CombatAIControls.Any<CombatAIController>((Func<CombatAIController, bool>)(x => x.GetShip() == ship)))
					{
						CombatAIController combatAiController = this.CreateNewCombatAIController(ship);
						if (combatAiController != null)
						{
							combatAiController.Initialize();
							this.m_CombatAIControls.Add(combatAiController);
						}
					}
				}
				else if (gameObject is StellarBody)
					gameObjectList.Add(gameObject);
				else if (gameObject is StarModel)
					gameObjectList.Add(gameObject);
			}
			foreach (CombatAIController combatAiControl in this.m_CombatAIControls)
			{
				if (combatAiControl.NeedsAParent())
					combatAiControl.FindParent((IEnumerable<CombatAIController>)this.m_CombatAIControls);
				if (combatAiControl.GetShip().Active && Ship.IsActiveShip(combatAiControl.GetShip()) || combatAiControl is MeteorCombatAIControl)
				{
					if (combatAiControl.RequestingNewTarget())
						combatAiControl.FindNewTarget((IEnumerable<IGameObject>)gameObjectList);
					if (combatAiControl.GetTarget() != null && combatAiControl.GetTarget() is StellarBody && !this.PlanetsAttackedByNPC.Contains(combatAiControl.GetTarget().ObjectID))
						this.PlanetsAttackedByNPC.Add(combatAiControl.GetTarget().ObjectID);
					combatAiControl.OnThink();
				}
			}
		}

		private CombatAIController CreateNewCombatAIController(Ship ship)
		{
			CombatAIController combatAiController = (CombatAIController)null;
			switch (ship.CombatAI)
			{
				case SectionEnumerations.CombatAiType.TrapDrone:
					combatAiController = (CombatAIController)new ColonyTrapDroneControl(this.m_Game, ship, this, this.m_FleetID);
					break;
				case SectionEnumerations.CombatAiType.Swarmer:
					combatAiController = (CombatAIController)new SwarmerAttackerControl(this.m_Game, ship, SwarmerAttackerType.SWARMER);
					break;
				case SectionEnumerations.CombatAiType.SwarmerGuardian:
					combatAiController = (CombatAIController)new SwarmerAttackerControl(this.m_Game, ship, SwarmerAttackerType.GAURDIAN);
					break;
				case SectionEnumerations.CombatAiType.SwarmerHive:
					combatAiController = (CombatAIController)new SwarmerHiveControl(this.m_Game, ship, this.m_SystemID);
					break;
				case SectionEnumerations.CombatAiType.SwarmerQueenLarva:
					combatAiController = (CombatAIController)new SwarmerQueenLarvaControl(this.m_Game, ship);
					break;
				case SectionEnumerations.CombatAiType.SwarmerQueen:
					combatAiController = (CombatAIController)new SwarmerQueenControl(this.m_Game, ship, this.m_SystemID);
					break;
				case SectionEnumerations.CombatAiType.VonNeumannCollectorMotherShip:
					combatAiController = (CombatAIController)new VonNeumannMomControl(this.m_Game, ship, this.m_FleetID);
					break;
				case SectionEnumerations.CombatAiType.VonNeumannCollectorProbe:
					combatAiController = (CombatAIController)new VonNeumannChildControl(this.m_Game, ship);
					break;
				case SectionEnumerations.CombatAiType.VonNeumannSeekerMotherShip:
					combatAiController = (CombatAIController)new VonNeumannSeekerControl(this.m_Game, ship);
					break;
				case SectionEnumerations.CombatAiType.VonNeumannBerserkerMotherShip:
					combatAiController = (CombatAIController)new VonNeumannBerserkerControl(this.m_Game, ship);
					break;
				case SectionEnumerations.CombatAiType.VonNeumannNeoBerserker:
					combatAiController = (CombatAIController)new VonNeumannNeoBerserkerControl(this.m_Game, ship);
					break;
				case SectionEnumerations.CombatAiType.VonNeumannDisc:
					combatAiController = (CombatAIController)new VonNeumannDiscControl(this.m_Game, ship);
					break;
				case SectionEnumerations.CombatAiType.VonNeumannPyramid:
					combatAiController = (CombatAIController)new VonNeumannPyramidControl(this.m_Game, ship);
					break;
				case SectionEnumerations.CombatAiType.VonNeumannPlanetKiller:
					combatAiController = (CombatAIController)new VonNeumannPlanetKillerCombatAIControl(this.m_Game, ship, this.m_SystemID);
					break;
				case SectionEnumerations.CombatAiType.LocustMoon:
					combatAiController = (CombatAIController)new LocustMoonControl(this.m_Game, ship, this.m_FleetID);
					break;
				case SectionEnumerations.CombatAiType.LocustWorld:
					combatAiController = (CombatAIController)new LocustNestControl(this.m_Game, ship, this.m_FleetID);
					break;
				case SectionEnumerations.CombatAiType.LocustFighter:
					combatAiController = (CombatAIController)new LocustFighterControl(this.m_Game, ship);
					break;
				case SectionEnumerations.CombatAiType.SystemKiller:
					combatAiController = (CombatAIController)new SystemKillerCombatAIControl(this.m_Game, ship);
					break;
				case SectionEnumerations.CombatAiType.MorrigiRelic:
					combatAiController = (CombatAIController)new MorrigiRelicControl(this.m_Game, ship, this.m_FleetID);
					break;
				case SectionEnumerations.CombatAiType.MorrigiCrow:
					combatAiController = (CombatAIController)new MorrigiCrowControl(this.m_Game, ship);
					break;
				case SectionEnumerations.CombatAiType.Meteor:
					combatAiController = (CombatAIController)new MeteorCombatAIControl(this.m_Game, ship);
					break;
				case SectionEnumerations.CombatAiType.Comet:
					combatAiController = (CombatAIController)new CometCombatAIControl(this.m_Game, ship);
					break;
				case SectionEnumerations.CombatAiType.Specter:
					combatAiController = (CombatAIController)new SpecterCombatAIControl(this.m_Game, ship);
					break;
				case SectionEnumerations.CombatAiType.Gardener:
					combatAiController = (CombatAIController)new GardenerCombatAIControl(this.m_Game, ship);
					break;
				case SectionEnumerations.CombatAiType.Protean:
					combatAiController = (CombatAIController)new ProteanCombatAIControl(this.m_Game, ship);
					break;
				case SectionEnumerations.CombatAiType.CommandMonitor:
					combatAiController = (CombatAIController)new CommandMonitorCombatAIControl(this.m_Game, ship, this.m_Game.GameDatabase.GetEncounterIDAtSystem(EasterEgg.EE_ASTEROID_MONITOR, this.m_SystemID));
					break;
				case SectionEnumerations.CombatAiType.NormalMonitor:
					combatAiController = (CombatAIController)new NormalMonitorCombatAIControl(this.m_Game, ship);
					break;
				case SectionEnumerations.CombatAiType.GhostShip:
					combatAiController = (CombatAIController)new GhostShipCombatAIControl(this.m_Game, ship);
					break;
			}
			return combatAiController;
		}

		public void HandlePostCombat(
		  List<Player> playersInCombat,
		  List<int> fleetIdsInCombat,
		  int systemId)
		{
			if (this.m_Game.Game.ScriptModules.MorrigiRelic == null || this.m_Game.Game.ScriptModules.MorrigiRelic.PlayerID != this.m_Player.ID)
				return;
			FleetInfo relicFleet = (FleetInfo)null;
			List<FleetInfo> source = new List<FleetInfo>();
			foreach (int fleetID in fleetIdsInCombat)
			{
				FleetInfo fleetInfo = this.m_Game.GameDatabase.GetFleetInfo(fleetID);
				if (fleetInfo != null)
				{
					if (fleetInfo.PlayerID == this.m_Player.ID)
						relicFleet = fleetInfo;
					else
						source.Add(fleetInfo);
				}
			}
			if (relicFleet == null)
				return;
			MorrigiRelicInfo relicInfo = this.m_Game.GameDatabase.GetMorrigiRelicInfos().ToList<MorrigiRelicInfo>().FirstOrDefault<MorrigiRelicInfo>((Func<MorrigiRelicInfo, bool>)(x => x.FleetId == relicFleet.ID));
			if (relicInfo == null || !relicInfo.IsAggressive)
				return;
			List<ShipInfo> aliveRelicShips = new List<ShipInfo>();
			bool flag = true;
			foreach (CombatAIController combatAiControl in this.m_CombatAIControls)
			{
				if (!combatAiControl.VictoryConditionIsMet())
				{
					flag = false;
					break;
				}
				if (combatAiControl is MorrigiRelicControl && combatAiControl.GetShip() != null && !combatAiControl.GetShip().IsDestroyed)
					aliveRelicShips.Add(this.m_Game.GameDatabase.GetShipInfo(combatAiControl.GetShip().DatabaseID, true));
			}
			if (!flag)
				return;
			List<Player> rewardedPlayers = new List<Player>();
			foreach (Player player in playersInCombat)
			{
				Player p = player;
				if (p.ID != this.m_Player.ID && source.Any<FleetInfo>((Func<FleetInfo, bool>)(x => x.PlayerID == p.ID)))
					rewardedPlayers.Add(p);
			}
			this.m_Game.Game.ScriptModules.MorrigiRelic.ApplyRewardsToPlayers(this.m_Game, relicInfo, aliveRelicShips, rewardedPlayers);
		}
	}
}
