// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameTriggers.GameTrigger
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ScenarioFramework;
using Kerberos.Sots.Framework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.GameTriggers
{
	internal class GameTrigger
	{
		internal static Dictionary<Type, GameTrigger.EvaluateContextDelegate> ContextFunctionMap = new Dictionary<Type, GameTrigger.EvaluateContextDelegate>()
	{
	  {
		typeof (AlwaysContext),
		new GameTrigger.EvaluateContextDelegate(GameTrigger.EvaluateAlwaysContext)
	  },
	  {
		typeof (StartContext),
		new GameTrigger.EvaluateContextDelegate(GameTrigger.EvaluateStartContext)
	  },
	  {
		typeof (EndContext),
		new GameTrigger.EvaluateContextDelegate(GameTrigger.EvaluateEndContext)
	  },
	  {
		typeof (RangeContext),
		new GameTrigger.EvaluateContextDelegate(GameTrigger.EvaluateRangeContext)
	  }
	};
		internal static Dictionary<Type, GameTrigger.EvaluateConditionDelegate> ConditionFunctionMap = new Dictionary<Type, GameTrigger.EvaluateConditionDelegate>()
	{
	  {
		typeof (GameOverCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateGameOverCondition)
	  },
	  {
		typeof (ScalarAmountCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateScalarAmountCondition)
	  },
	  {
		typeof (TriggerTriggeredCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateTriggerTriggeredCondition)
	  },
	  {
		typeof (SystemRangeCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateSystemRangeCondition)
	  },
	  {
		typeof (FleetRangeCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateFleetRangeCondition)
	  },
	  {
		typeof (ProvinceRangeCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateProvinceRangeCondition)
	  },
	  {
		typeof (ColonyDeathCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateColonyDeathCondition)
	  },
	  {
		typeof (PlanetDeathCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluatePlanetDeathCondition)
	  },
	  {
		typeof (FleetDeathCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateFleetDeathCondition)
	  },
	  {
		typeof (ShipDeathCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateShipDeathCondition)
	  },
	  {
		typeof (AdmiralDeathCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateAdmiralDeathCondition)
	  },
	  {
		typeof (PlayerDeathCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluatePlayerDeathCondition)
	  },
	  {
		typeof (AllianceFormedCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateAllianceFormedCondition)
	  },
	  {
		typeof (AllianceBrokenCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateAllianceBrokenCondition)
	  },
	  {
		typeof (GrandMenaceAppearedCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateGrandMenaceAppearedCondition)
	  },
	  {
		typeof (GrandMenaceDestroyedCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateGrandMenaceDestroyedCondition)
	  },
	  {
		typeof (ResourceAmountCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateResourceAmountCondition)
	  },
	  {
		typeof (PlanetAmountCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluatePlanetAmountCondition)
	  },
	  {
		typeof (BiosphereAmountCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateBiosphereAmountCondition)
	  },
	  {
		typeof (AllianceAmountCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateAllianceAmountCondition)
	  },
	  {
		typeof (PopulationAmountCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluatePopulationAmountCondition)
	  },
	  {
		typeof (FleetAmountCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateFleetAmountCondition)
	  },
	  {
		typeof (CommandPointAmountCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateCommandPointAmountCondition)
	  },
	  {
		typeof (TerrainRangeCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateTerrainRangeCondition)
	  },
	  {
		typeof (TerrainColonizedCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateTerrainColonizedCondition)
	  },
	  {
		typeof (PlanetColonizedCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluatePlanetColonizedCondition)
	  },
	  {
		typeof (TreatyBrokenCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateTreatyBrokenCondition)
	  },
	  {
		typeof (CivilianDeathCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateCivilianDeathCondition)
	  },
	  {
		typeof (MoralAmountCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateMoralAmountCondition)
	  },
	  {
		typeof (GovernmentTypeCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateGovernmentTypeCondition)
	  },
	  {
		typeof (RevelationBeginsCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateRevelationBeginsCondition)
	  },
	  {
		typeof (ResearchObtainedCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateResearchObtainedCondition)
	  },
	  {
		typeof (ClassBuiltCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateClassBuiltCondition)
	  },
	  {
		typeof (TradePointsAmountCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateTradePointsAmountCondition)
	  },
	  {
		typeof (IncomePerTurnAmountCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateIncomePerTurnAmountCondition)
	  },
	  {
		typeof (FactionEncounteredCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateFactionEncounteredCondition)
	  },
	  {
		typeof (WorldTypeCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateWorldTypeCondition)
	  },
	  {
		typeof (PlanetDevelopmentAmountCondition),
		new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluatePlanetDevelopmentAmountCondition)
	  }
	};
		internal static Dictionary<Type, GameTrigger.EvaluateActionDelegate> ActionFunctionMap = new Dictionary<Type, GameTrigger.EvaluateActionDelegate>()
	{
	  {
		typeof (GameOverAction),
		new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateGameOverAction)
	  },
	  {
		typeof (PointPerPlanetDeathAction),
		new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluatePointPerPlanetDeathAction)
	  },
	  {
		typeof (PointPerColonyDeathAction),
		new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluatePointPerColonyDeathAction)
	  },
	  {
		typeof (PointPerShipTypeAction),
		new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluatePointPerShipTypeAction)
	  },
	  {
		typeof (AddScalarToScalarAction),
		new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateAddScalarToScalarAction)
	  },
	  {
		typeof (SetScalarAction),
		new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateSetScalarAction)
	  },
	  {
		typeof (ChangeScalarAction),
		new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateChangeScalarAction)
	  },
	  {
		typeof (SpawnUnitAction),
		new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateSpawnUnitAction)
	  },
	  {
		typeof (DiplomacyChangedAction),
		new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateDiplomacyChangedAction)
	  },
	  {
		typeof (ColonyChangedAction),
		new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateColonyChangedAction)
	  },
	  {
		typeof (AIStrategyChangedAction),
		new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateAIStrategyChangedAction)
	  },
	  {
		typeof (ResearchAwardedAction),
		new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateResearchAwardedAction)
	  },
	  {
		typeof (AdmiralChangedAction),
		new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateAdmiralChangedAction)
	  },
	  {
		typeof (RebellionOccursAction),
		new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateRebellionOccursAction)
	  },
	  {
		typeof (RebellionEndsAction),
		new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateRebellionEndsAction)
	  },
	  {
		typeof (PlanetDestroyedAction),
		new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluatePlanetDestroyedAction)
	  },
	  {
		typeof (PlanetAddedAction),
		new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluatePlanetAddedAction)
	  },
	  {
		typeof (TerrainAppearsAction),
		new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateTerrainAppearsAction)
	  },
	  {
		typeof (TerrainDisappearsAction),
		new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateTerrainDisappearsAction)
	  },
	  {
		typeof (ChangeTreasuryAction),
		new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateChangeTreasuryAction)
	  },
	  {
		typeof (ChangeResourcesAction),
		new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateChangeResourcesAction)
	  },
	  {
		typeof (RemoveFleetAction),
		new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateRemoveFleetAction)
	  },
	  {
		typeof (AddWeaponAction),
		new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateAddWeaponAction)
	  },
	  {
		typeof (RemoveWeaponAction),
		new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateRemoveWeaponAction)
	  },
	  {
		typeof (AddSectionAction),
		new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateAddSectionAction)
	  },
	  {
		typeof (RemoveSectionAction),
		new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateRemoveSectionAction)
	  },
	  {
		typeof (AddModuleAction),
		new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateAddModuleAction)
	  },
	  {
		typeof (RemoveModuleAction),
		new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateRemoveModuleAction)
	  },
	  {
		typeof (ProvinceChangedAction),
		new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateProvinceChangedAction)
	  },
	  {
		typeof (SurrenderSystemAction),
		new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateSurrenderSystemAction)
	  },
	  {
		typeof (SurrenderEmpireAction),
		new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateSurrenderEmpireAction)
	  },
	  {
		typeof (StratModifierChangedAction),
		new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateStratModifierChangedAction)
	  },
	  {
		typeof (DisplayMessageAction),
		new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateDisplayMessageAction)
	  },
	  {
		typeof (MoveFleetAction),
		new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateMoveFleetAction)
	  }
	};

		internal static int GetTriggerTriggeredDepth(List<Trigger> triggerPool, Trigger t)
		{
			if (!GameTrigger.isTriggerTriggeredCondition(t))
				return 1;
			int val1 = 0;
			foreach (TriggerTriggeredCondition condition in t.Conditions)
			{
				TriggerTriggeredCondition ttc = condition;
				val1 = Math.Max(val1, GameTrigger.GetTriggerTriggeredDepth(triggerPool, triggerPool.First<Trigger>((Func<Trigger, bool>)(x => x.Name == ttc.TriggerName))) + 1);
			}
			return val1;
		}

		internal static bool isTriggerTriggeredCondition(Trigger t)
		{
			return t.Conditions.OfType<TriggerTriggeredCondition>().Count<TriggerTriggeredCondition>() > 0;
		}

		internal static void PushEvent(EventType et, object data, GameSession sim)
		{
			sim.TriggerEvents.Add(new EventStub()
			{
				EventType = et,
				Data = data
			});
		}

		internal static void ClearEvents(GameSession sim)
		{
			sim.TriggerEvents.Clear();
		}

		internal static bool Evaluate(TriggerContext tc, GameSession g, Trigger t)
		{
			if (GameTrigger.ContextFunctionMap.ContainsKey(tc.GetType()))
				return GameTrigger.ContextFunctionMap[tc.GetType()](tc, g, t);
			return false;
		}

		internal static bool EvaluateAlwaysContext(TriggerContext tc, GameSession g, Trigger t)
		{
			return true;
		}

		internal static bool EvaluateStartContext(TriggerContext tc, GameSession g, Trigger t)
		{
			StartContext startContext = tc as StartContext;
			return g.GameDatabase.GetTurnCount() >= startContext.StartTurn;
		}

		internal static bool EvaluateEndContext(TriggerContext tc, GameSession g, Trigger t)
		{
			EndContext endContext = tc as EndContext;
			return g.GameDatabase.GetTurnCount() <= endContext.EndTurn;
		}

		internal static bool EvaluateRangeContext(TriggerContext tc, GameSession g, Trigger t)
		{
			RangeContext rangeContext = tc as RangeContext;
			int turnCount = g.GameDatabase.GetTurnCount();
			return turnCount >= rangeContext.StartTurn && turnCount <= rangeContext.EndTurn;
		}

		internal static bool Evaluate(TriggerCondition tc, GameSession g, Trigger t)
		{
			if (GameTrigger.ConditionFunctionMap.ContainsKey(tc.GetType()))
				return GameTrigger.ConditionFunctionMap[tc.GetType()](tc, g, t);
			return false;
		}

		internal static bool EvaluateGameOverCondition(TriggerCondition tc, GameSession sim, Trigger t)
		{
			GameOverCondition goc = tc as GameOverCondition;
			EventStub eventStub = sim.TriggerEvents.FirstOrDefault<EventStub>((Func<EventStub, bool>)(x =>
		   {
			   if (x.EventType != EventType.EVNT_GAMEOVER)
				   return false;
			   if (!string.IsNullOrEmpty(goc.Reason))
				   return x.Data as string == goc.Reason;
			   return true;
		   }));
			if (eventStub == null)
				return false;
			sim.TriggerEvents.Remove(eventStub);
			return true;
		}

		internal static bool EvaluateScalarAmountCondition(
		  TriggerCondition tc,
		  GameSession sim,
		  Trigger t)
		{
			ScalarAmountCondition scalarAmountCondition = tc as ScalarAmountCondition;
			return sim.TriggerScalars.ContainsKey(scalarAmountCondition.Scalar) && (double)sim.TriggerScalars[scalarAmountCondition.Scalar] > (double)scalarAmountCondition.Value;
		}

		internal static bool EvaluateTriggerTriggeredCondition(
		  TriggerCondition tc,
		  GameSession sim,
		  Trigger t)
		{
			EventStub eventStub = sim.TriggerEvents.FirstOrDefault<EventStub>((Func<EventStub, bool>)(x =>
		   {
			   if (x.EventType == EventType.EVNT_TRIGGERTRIGGERED)
				   return x.Data as string == (tc as TriggerTriggeredCondition).TriggerName;
			   return false;
		   }));
			if (eventStub == null)
				return false;
			sim.TriggerEvents.Remove(eventStub);
			return true;
		}

		internal static bool EvaluateSystemRangeCondition(
		  TriggerCondition tc,
		  GameSession sim,
		  Trigger t)
		{
			SystemRangeCondition systemRangeCondition = tc as SystemRangeCondition;
			IEnumerable<FleetInfo> fleetInfosByPlayerId = sim.GameDatabase.GetFleetInfosByPlayerID(systemRangeCondition.PlayerSlot, FleetType.FL_NORMAL);
			StarSystemInfo starSystemInfo = sim.GameDatabase.GetStarSystemInfo(systemRangeCondition.SystemId);
			bool flag = false;
			foreach (FleetInfo fleetInfo in fleetInfosByPlayerId)
			{
				if ((double)(sim.GameDatabase.GetFleetLocation(fleetInfo.ID, false).Coords - starSystemInfo.Origin).Length <= (double)systemRangeCondition.Distance)
				{
					t.RangeTriggeredFleets.Add(fleetInfo);
					flag = true;
				}
			}
			return flag;
		}

		internal static bool EvaluateFleetRangeCondition(
		  TriggerCondition tc,
		  GameSession sim,
		  Trigger t)
		{
			bool flag = false;
			FleetRangeCondition fleetRangeCondition = tc as FleetRangeCondition;
			IEnumerable<FleetInfo> fleetInfosByPlayerId = sim.GameDatabase.GetFleetInfosByPlayerID(fleetRangeCondition.PlayerSlot, FleetType.FL_NORMAL);
			FleetInfo fleetInfoByFleetName = sim.GameDatabase.GetFleetInfoByFleetName(fleetRangeCondition.FleetName, FleetType.FL_NORMAL);
			foreach (FleetInfo fleetInfo in fleetInfosByPlayerId)
			{
				if ((double)(sim.GameDatabase.GetFleetLocation(fleetInfo.ID, false).Coords - sim.GameDatabase.GetFleetLocation(fleetInfoByFleetName.ID, false).Coords).Length <= (double)fleetRangeCondition.Distance)
				{
					t.RangeTriggeredFleets.Add(fleetInfo);
					flag = true;
				}
			}
			return flag;
		}

		internal static bool EvaluateProvinceRangeCondition(
		  TriggerCondition tc,
		  GameSession sim,
		  Trigger t)
		{
			bool flag = false;
			ProvinceRangeCondition prc = tc as ProvinceRangeCondition;
			IEnumerable<FleetInfo> fleetInfos = (IEnumerable<FleetInfo>)new List<FleetInfo>(sim.GameDatabase.GetFleetInfosByPlayerID(prc.PlayerSlot, FleetType.FL_NORMAL));
			foreach (StarSystemInfo starSystemInfo in (IEnumerable<StarSystemInfo>)new List<StarSystemInfo>(sim.GameDatabase.GetStarSystemInfos().Where<StarSystemInfo>((Func<StarSystemInfo, bool>)(x =>
		  {
			  int? provinceId1 = x.ProvinceID;
			  int provinceId2 = prc.ProvinceId;
			  if (provinceId1.GetValueOrDefault() == provinceId2)
				  return provinceId1.HasValue;
			  return false;
		  }))))
			{
				foreach (FleetInfo fleetInfo in fleetInfos)
				{
					if ((double)(sim.GameDatabase.GetFleetLocation(fleetInfo.ID, false).Coords - starSystemInfo.Origin).Length <= (double)prc.Distance)
					{
						t.RangeTriggeredFleets.Add(fleetInfo);
						flag = true;
					}
				}
			}
			return flag;
		}

		internal static bool EvaluateTerrainRangeCondition(
		  TriggerCondition tc,
		  GameSession sim,
		  Trigger t)
		{
			return false;
		}

		internal static bool EvaluateColonyDeathCondition(
		  TriggerCondition tc,
		  GameSession sim,
		  Trigger t)
		{
			ColonyDeathCondition cdc = tc as ColonyDeathCondition;
			int orbitId = sim.GameDatabase.GetStarSystemOrbitalObjectInfos(cdc.SystemId).First<OrbitalObjectInfo>((Func<OrbitalObjectInfo, bool>)(x => x.Name == cdc.OrbitName)).ID;
			EventStub eventStub = sim.TriggerEvents.FirstOrDefault<EventStub>((Func<EventStub, bool>)(x =>
		   {
			   if (x.EventType == EventType.EVNT_COLONYDIED)
				   return (int)x.Data == orbitId;
			   return false;
		   }));
			if (eventStub == null)
				return false;
			sim.TriggerEvents.Remove(eventStub);
			return true;
		}

		internal static bool EvaluatePlanetDeathCondition(
		  TriggerCondition tc,
		  GameSession sim,
		  Trigger t)
		{
			PlanetDeathCondition pdc = tc as PlanetDeathCondition;
			int orbitId = sim.GameDatabase.GetStarSystemOrbitalObjectInfos(pdc.SystemId).First<OrbitalObjectInfo>((Func<OrbitalObjectInfo, bool>)(x => x.Name == pdc.OrbitName)).ID;
			EventStub eventStub = sim.TriggerEvents.FirstOrDefault<EventStub>((Func<EventStub, bool>)(x =>
		   {
			   if (x.EventType == EventType.EVNT_PLANETDIED)
				   return (int)x.Data == orbitId;
			   return false;
		   }));
			if (eventStub == null)
				return false;
			sim.TriggerEvents.Remove(eventStub);
			return true;
		}

		internal static bool EvaluateFleetDeathCondition(TriggerCondition tc, GameSession g, Trigger t)
		{
			FleetDeathCondition fdc = tc as FleetDeathCondition;
			EventStub eventStub = g.TriggerEvents.FirstOrDefault<EventStub>((Func<EventStub, bool>)(x =>
		   {
			   if (x.EventType == EventType.EVNT_FLEETDIED)
				   return x.Data as string == fdc.FleetName;
			   return false;
		   }));
			if (eventStub == null)
				return false;
			g.TriggerEvents.Remove(eventStub);
			return true;
		}

		internal static bool EvaluateShipDeathCondition(TriggerCondition tc, GameSession g, Trigger t)
		{
			ShipDeathCondition fdc = tc as ShipDeathCondition;
			EventStub eventStub = g.TriggerEvents.FirstOrDefault<EventStub>((Func<EventStub, bool>)(x =>
		   {
			   if (x.EventType == EventType.EVNT_SHIPDIED)
				   return (ShipClass)x.Data == fdc.ShipClass;
			   return false;
		   }));
			if (eventStub == null)
				return false;
			g.TriggerEvents.Remove(eventStub);
			return true;
		}

		internal static bool EvaluateAdmiralDeathCondition(
		  TriggerCondition tc,
		  GameSession g,
		  Trigger t)
		{
			AdmiralDeathCondition adc = tc as AdmiralDeathCondition;
			EventStub eventStub = g.TriggerEvents.FirstOrDefault<EventStub>((Func<EventStub, bool>)(x =>
		   {
			   if (x.EventType == EventType.EVNT_ADMIRALDIED)
				   return x.Data as string == adc.AdmiralName;
			   return false;
		   }));
			if (eventStub == null)
				return false;
			g.TriggerEvents.Remove(eventStub);
			return true;
		}

		internal static bool EvaluatePlayerDeathCondition(
		  TriggerCondition tc,
		  GameSession g,
		  Trigger t)
		{
			PlayerDeathCondition pdc = tc as PlayerDeathCondition;
			EventStub eventStub = g.TriggerEvents.FirstOrDefault<EventStub>((Func<EventStub, bool>)(x =>
		   {
			   if (x.EventType == EventType.EVNT_PLAYERDIED)
				   return (int)x.Data == pdc.PlayerSlot;
			   return false;
		   }));
			if (eventStub == null)
				return false;
			g.TriggerEvents.Remove(eventStub);
			return true;
		}

		internal static bool EvaluateAllianceFormedCondition(
		  TriggerCondition tc,
		  GameSession g,
		  Trigger t)
		{
			return false;
		}

		internal static bool EvaluateAllianceBrokenCondition(
		  TriggerCondition tc,
		  GameSession g,
		  Trigger t)
		{
			return false;
		}

		internal static bool EvaluateGrandMenaceAppearedCondition(
		  TriggerCondition tc,
		  GameSession g,
		  Trigger t)
		{
			return false;
		}

		internal static bool EvaluateGrandMenaceDestroyedCondition(
		  TriggerCondition tc,
		  GameSession g,
		  Trigger t)
		{
			return false;
		}

		internal static bool EvaluateResourceAmountCondition(
		  TriggerCondition tc,
		  GameSession g,
		  Trigger t)
		{
			return false;
		}

		internal static bool EvaluatePlanetAmountCondition(
		  TriggerCondition tc,
		  GameSession g,
		  Trigger t)
		{
			PlanetAmountCondition pac = tc as PlanetAmountCondition;
			return g.GameDatabase.GetColonyInfos().Where<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == pac.PlayerSlot)).Count<ColonyInfo>() > pac.NumberOfPlanets;
		}

		internal static bool EvaluateBiosphereAmountCondition(
		  TriggerCondition tc,
		  GameSession g,
		  Trigger t)
		{
			BiosphereAmountCondition bac = tc as BiosphereAmountCondition;
			OrbitalObjectInfo ooi = g.GameDatabase.GetStarSystemOrbitalObjectInfos(bac.SystemId).First<OrbitalObjectInfo>((Func<OrbitalObjectInfo, bool>)(x => x.Name == bac.OrbitName));
			return (double)((IEnumerable<PlanetInfo>)g.GameDatabase.GetStarSystemPlanetInfos(bac.SystemId)).First<PlanetInfo>((Func<PlanetInfo, bool>)(x => x.ID == ooi.ID)).Biosphere >= (double)bac.BiosphereAmount;
		}

		internal static bool EvaluateAllianceAmountCondition(
		  TriggerCondition tc,
		  GameSession g,
		  Trigger t)
		{
			return false;
		}

		internal static bool EvaluatePopulationAmountCondition(
		  TriggerCondition tc,
		  GameSession g,
		  Trigger t)
		{
			PopulationAmountCondition pac = tc as PopulationAmountCondition;
			OrbitalObjectInfo orbitalObjectInfo = g.GameDatabase.GetStarSystemOrbitalObjectInfos(pac.SystemId).First<OrbitalObjectInfo>((Func<OrbitalObjectInfo, bool>)(x => x.Name == pac.OrbitName));
			double imperialPop = g.GameDatabase.GetColonyInfoForPlanet(orbitalObjectInfo.ID).ImperialPop;
			foreach (ColonyFactionInfo civilianPopulation in g.GameDatabase.GetCivilianPopulations(orbitalObjectInfo.ID))
				imperialPop += civilianPopulation.CivilianPop;
			return imperialPop >= (double)pac.PopulationNumber;
		}

		internal static bool EvaluateFleetAmountCondition(
		  TriggerCondition tc,
		  GameSession g,
		  Trigger t)
		{
			FleetAmountCondition fleetAmountCondition = tc as FleetAmountCondition;
			return g.GameDatabase.GetFleetInfosByPlayerID(fleetAmountCondition.PlayerSlot, FleetType.FL_NORMAL).Count<FleetInfo>() >= fleetAmountCondition.NumberOfFleets;
		}

		internal static bool EvaluateCommandPointAmountCondition(
		  TriggerCondition tc,
		  GameSession g,
		  Trigger t)
		{
			int num = 0;
			CommandPointAmountCondition pointAmountCondition = tc as CommandPointAmountCondition;
			foreach (FleetInfo fleetInfo in g.GameDatabase.GetFleetInfosByPlayerID(pointAmountCondition.PlayerSlot, FleetType.FL_NORMAL))
			{
				foreach (int shipID in g.GameDatabase.GetShipsByFleetID(fleetInfo.ID))
					num += g.GameDatabase.GetCommandPointCost(g.GameDatabase.GetShipInfo(shipID, false).DesignID);
			}
			return num >= pointAmountCondition.CommandPoints;
		}

		internal static bool EvaluateTerrainColonizedCondition(
		  TriggerCondition tc,
		  GameSession g,
		  Trigger t)
		{
			return false;
		}

		internal static bool EvaluatePlanetColonizedCondition(
		  TriggerCondition tc,
		  GameSession g,
		  Trigger t)
		{
			PlanetColonizedCondition pcc = tc as PlanetColonizedCondition;
			OrbitalObjectInfo orbitalObjectInfo = g.GameDatabase.GetStarSystemOrbitalObjectInfos(pcc.SystemId).First<OrbitalObjectInfo>((Func<OrbitalObjectInfo, bool>)(x => x.Name == pcc.OrbitName));
			ColonyInfo colonyInfoForPlanet = g.GameDatabase.GetColonyInfoForPlanet(orbitalObjectInfo.ID);
			if (colonyInfoForPlanet == null)
				return false;
			foreach (MissionInfo missionInfo in g.GameDatabase.GetMissionsByPlanetDest(orbitalObjectInfo.ID))
			{
				if (missionInfo.Type == MissionType.COLONIZATION)
				{
					FleetInfo fleetInfo = g.GameDatabase.GetFleetInfo(missionInfo.FleetID);
					if (fleetInfo.PlayerID != colonyInfoForPlanet.PlayerID)
						g.GameDatabase.ApplyDiplomacyReaction(colonyInfoForPlanet.PlayerID, fleetInfo.PlayerID, StratModifiers.DiplomacyReactionColonizeClaimedWorld, 1);
				}
			}
			return true;
		}

		internal static bool EvaluateTreatyBrokenCondition(
		  TriggerCondition tc,
		  GameSession g,
		  Trigger t)
		{
			return false;
		}

		internal static bool EvaluateCivilianDeathCondition(
		  TriggerCondition tc,
		  GameSession g,
		  Trigger t)
		{
			return false;
		}

		internal static bool EvaluateMoralAmountCondition(
		  TriggerCondition tc,
		  GameSession g,
		  Trigger t)
		{
			MoralAmountCondition mac = tc as MoralAmountCondition;
			int id = g.GameDatabase.GetStarSystemOrbitalObjectInfos(mac.SystemId).First<OrbitalObjectInfo>((Func<OrbitalObjectInfo, bool>)(x => x.Name == mac.OrbitName)).ID;
			int fid = g.GameDatabase.GetFactionIdFromName(mac.Faction);
			return (double)g.GameDatabase.GetCivilianPopulations(id).First<ColonyFactionInfo>((Func<ColonyFactionInfo, bool>)(x => x.FactionID == fid)).Morale <= (double)mac.MoralAmount;
		}

		internal static bool EvaluateGovernmentTypeCondition(
		  TriggerCondition tc,
		  GameSession g,
		  Trigger t)
		{
			GovernmentTypeCondition governmentTypeCondition = tc as GovernmentTypeCondition;
			return g.GameDatabase.GetGovernmentInfo(governmentTypeCondition.PlayerSlot).CurrentType == ScenarioEnumerations.GovernmentTypes[governmentTypeCondition.GovernmentType];
		}

		internal static bool EvaluateRevelationBeginsCondition(
		  TriggerCondition tc,
		  GameSession g,
		  Trigger t)
		{
			return false;
		}

		internal static bool EvaluateResearchObtainedCondition(
		  TriggerCondition tc,
		  GameSession g,
		  Trigger t)
		{
			ResearchObtainedCondition roc = tc as ResearchObtainedCondition;
			return g.GameDatabase.GetPlayerTechInfos(roc.PlayerSlot).Any<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x =>
		   {
			   if (x.TechFileID == roc.TechId)
				   return x.State == TechStates.Researched;
			   return false;
		   }));
		}

		internal static bool EvaluateClassBuiltCondition(TriggerCondition tc, GameSession g, Trigger t)
		{
			ClassBuiltCondition classBuiltCondition = tc as ClassBuiltCondition;
			foreach (FleetInfo fleetInfo in g.GameDatabase.GetFleetInfosByPlayerID(classBuiltCondition.PlayerSlot, FleetType.FL_NORMAL))
			{
				foreach (ShipInfo shipInfo in g.GameDatabase.GetShipInfoByFleetID(fleetInfo.ID, false))
				{
					if (shipInfo.DesignInfo.Class.ToString() == classBuiltCondition.Class)
						return true;
				}
			}
			return false;
		}

		internal static bool EvaluateTradePointsAmountCondition(
		  TriggerCondition tc,
		  GameSession g,
		  Trigger t)
		{
			return false;
		}

		internal static bool EvaluateIncomePerTurnAmountCondition(
		  TriggerCondition tc,
		  GameSession g,
		  Trigger t)
		{
			return false;
		}

		internal static bool EvaluateFactionEncounteredCondition(
		  TriggerCondition tc,
		  GameSession g,
		  Trigger t)
		{
			return false;
		}

		internal static bool EvaluateWorldTypeCondition(TriggerCondition tc, GameSession g, Trigger t)
		{
			return false;
		}

		internal static bool EvaluatePlanetDevelopmentAmountCondition(
		  TriggerCondition tc,
		  GameSession g,
		  Trigger t)
		{
			return false;
		}

		internal static bool Evaluate(TriggerAction ta, GameSession g, Trigger t)
		{
			if (GameTrigger.ActionFunctionMap.ContainsKey(ta.GetType()))
				return GameTrigger.ActionFunctionMap[ta.GetType()](ta, g, t);
			return false;
		}

		internal static bool EvaluateGameOverAction(TriggerAction tc, GameSession g, Trigger t)
		{
			GameTrigger.PushEvent(EventType.EVNT_GAMEOVER, (object)(tc as GameOverAction).Reason, g);
			return true;
		}

		internal static bool EvaluatePointPerPlanetDeathAction(
		  TriggerAction tc,
		  GameSession g,
		  Trigger t)
		{
			PointPerPlanetDeathAction planetDeathAction = tc as PointPerPlanetDeathAction;
			float num = (float)g.TriggerEvents.Where<EventStub>((Func<EventStub, bool>)(x => x.EventType == EventType.EVNT_PLANETDIED)).Count<EventStub>() * planetDeathAction.AmountPerPlanet;
			if (!g.TriggerScalars.ContainsKey(planetDeathAction.ScalarName))
				g.TriggerScalars.Add(planetDeathAction.ScalarName, 0.0f);
			Dictionary<string, float> triggerScalars;
			string scalarName;
			(triggerScalars = g.TriggerScalars)[scalarName = planetDeathAction.ScalarName] = triggerScalars[scalarName] + num;
			return true;
		}

		internal static bool EvaluatePointPerColonyDeathAction(
		  TriggerAction tc,
		  GameSession g,
		  Trigger t)
		{
			PointPerColonyDeathAction colonyDeathAction = tc as PointPerColonyDeathAction;
			float num = (float)g.TriggerEvents.Where<EventStub>((Func<EventStub, bool>)(x => x.EventType == EventType.EVNT_COLONYDIED)).Count<EventStub>() * colonyDeathAction.AmountPerColony;
			if (!g.TriggerScalars.ContainsKey(colonyDeathAction.ScalarName))
				g.TriggerScalars.Add(colonyDeathAction.ScalarName, 0.0f);
			Dictionary<string, float> triggerScalars;
			string scalarName;
			(triggerScalars = g.TriggerScalars)[scalarName = colonyDeathAction.ScalarName] = triggerScalars[scalarName] + num;
			return true;
		}

		internal static bool EvaluatePointPerShipTypeAction(TriggerAction tc, GameSession g, Trigger t)
		{
			PointPerShipTypeAction ppsta = tc as PointPerShipTypeAction;
			List<FleetInfo> fleetInfoList = new List<FleetInfo>();
			float num = 0.0f;
			if (ppsta.Fleet == ScenarioEnumerations.FleetsInRangeVariableName)
				fleetInfoList.AddRange((IEnumerable<FleetInfo>)t.RangeTriggeredFleets);
			else
				fleetInfoList.AddRange(g.GameDatabase.GetFleetInfos(FleetType.FL_NORMAL).Where<FleetInfo>((Func<FleetInfo, bool>)(x => x.Name == ppsta.Fleet)));
			foreach (FleetInfo fleetInfo in fleetInfoList)
			{
				foreach (ShipInfo shipInfo in new List<ShipInfo>(g.GameDatabase.GetShipInfoByFleetID(fleetInfo.ID, false)))
				{
					switch (ppsta.ShipType)
					{
						case "Cruiser":
							if (g.GameDatabase.GetDesignInfo(shipInfo.DesignID).Class == ShipClass.Cruiser)
							{
								num += ppsta.AmountPerShip;
								continue;
							}
							continue;
						case "Dreadnaught":
							if (g.GameDatabase.GetDesignInfo(shipInfo.DesignID).Class == ShipClass.Dreadnought)
							{
								num += ppsta.AmountPerShip;
								continue;
							}
							continue;
						case "Leviathan":
							if (g.GameDatabase.GetDesignInfo(shipInfo.DesignID).Class == ShipClass.Leviathan)
							{
								num += ppsta.AmountPerShip;
								continue;
							}
							continue;
						case "Colony":
							if (g.GameDatabase.GetDesignInfo(shipInfo.DesignID).Role == ShipRole.COLONIZER)
							{
								num += ppsta.AmountPerShip;
								continue;
							}
							continue;
						case "Supply":
							if (g.GameDatabase.GetDesignInfo(shipInfo.DesignID).Role == ShipRole.SUPPLY)
							{
								num += ppsta.AmountPerShip;
								continue;
							}
							continue;
						default:
							continue;
					}
				}
			}
			if (!g.TriggerScalars.ContainsKey(ppsta.ScalarName))
			{
				g.TriggerScalars.Add(ppsta.ScalarName, num);
			}
			else
			{
				Dictionary<string, float> triggerScalars;
				string scalarName;
				(triggerScalars = g.TriggerScalars)[scalarName = ppsta.ScalarName] = triggerScalars[scalarName] + num;
			}
			return true;
		}

		internal static bool EvaluateAddScalarToScalarAction(
		  TriggerAction tc,
		  GameSession g,
		  Trigger t)
		{
			AddScalarToScalarAction scalarToScalarAction = tc as AddScalarToScalarAction;
			if (!g.TriggerScalars.ContainsKey(scalarToScalarAction.ScalarAddedTo))
				g.TriggerScalars.Add(scalarToScalarAction.ScalarAddedTo, 0.0f);
			if (g.TriggerScalars.ContainsKey(scalarToScalarAction.ScalarToAdd))
			{
				Dictionary<string, float> triggerScalars;
				string scalarAddedTo;
				(triggerScalars = g.TriggerScalars)[scalarAddedTo = scalarToScalarAction.ScalarAddedTo] = triggerScalars[scalarAddedTo] + g.TriggerScalars[scalarToScalarAction.ScalarToAdd];
			}
			return true;
		}

		internal static bool EvaluateSetScalarAction(TriggerAction tc, GameSession g, Trigger t)
		{
			SetScalarAction setScalarAction = tc as SetScalarAction;
			if (!g.TriggerScalars.ContainsKey(setScalarAction.Scalar))
				g.TriggerScalars.Add(setScalarAction.Scalar, 0.0f);
			g.TriggerScalars[setScalarAction.Scalar] = setScalarAction.Value;
			return true;
		}

		internal static bool EvaluateChangeScalarAction(TriggerAction tc, GameSession g, Trigger t)
		{
			ChangeScalarAction changeScalarAction = tc as ChangeScalarAction;
			if (!g.TriggerScalars.ContainsKey(changeScalarAction.Scalar))
				g.TriggerScalars.Add(changeScalarAction.Scalar, 0.0f);
			Dictionary<string, float> triggerScalars;
			string scalar;
			(triggerScalars = g.TriggerScalars)[scalar = changeScalarAction.Scalar] = triggerScalars[scalar] + changeScalarAction.Value;
			return true;
		}

		internal static bool EvaluateDiplomacyChangedAction(TriggerAction tc, GameSession g, Trigger t)
		{
			return false;
		}

		internal static bool EvaluateColonyChangedAction(TriggerAction tc, GameSession g, Trigger t)
		{
			ColonyChangedAction cca = tc as ColonyChangedAction;
			OrbitalObjectInfo ooi = g.GameDatabase.GetStarSystemOrbitalObjectInfos(cca.SystemId).First<OrbitalObjectInfo>((Func<OrbitalObjectInfo, bool>)(x => x.Name == cca.OrbitName));
			ColonyInfo colony = g.GameDatabase.GetColonyInfos().First<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.OrbitalObjectID == ooi.ID));
			colony.PlayerID = cca.NewPlayer;
			g.GameDatabase.UpdateColony(colony);
			return true;
		}

		internal static bool EvaluateAIStrategyChangedAction(
		  TriggerAction tc,
		  GameSession g,
		  Trigger t)
		{
			return false;
		}

		internal static bool EvaluateResearchAwardedAction(TriggerAction tc, GameSession g, Trigger t)
		{
			ResearchAwardedAction researchAwardedAction = tc as ResearchAwardedAction;
			g.GameDatabase.UpdatePlayerTechState(researchAwardedAction.PlayerSlot, g.GameDatabase.GetTechID(researchAwardedAction.TechId), TechStates.Researched);
			return true;
		}

		internal static bool EvaluateAdmiralChangedAction(TriggerAction tc, GameSession g, Trigger t)
		{
			AdmiralChangedAction aca = tc as AdmiralChangedAction;
			AdmiralInfo admiral = g.GameDatabase.GetAdmiralInfos().First<AdmiralInfo>((Func<AdmiralInfo, bool>)(x => x.Name == aca.OldAdmiral.Name));
			admiral.Age = (float)aca.NewAdmiral.Age;
			admiral.EvasionBonus = (int)aca.NewAdmiral.EvasionRating;
			admiral.Gender = aca.NewAdmiral.Gender;
			admiral.HomeworldID = new int?(aca.NewAdmiral.HomePlanet);
			admiral.Name = aca.NewAdmiral.Name;
			admiral.PlayerID = aca.NewPlayer;
			admiral.Race = aca.NewAdmiral.Faction;
			admiral.ReactionBonus = (int)aca.NewAdmiral.ReactionRating;
			g.GameDatabase.UpdateAdmiralInfo(admiral);
			return true;
		}

		internal static bool EvaluateRebellionOccursAction(TriggerAction tc, GameSession g, Trigger t)
		{
			return false;
		}

		internal static bool EvaluateRebellionEndsAction(TriggerAction tc, GameSession g, Trigger t)
		{
			return false;
		}

		internal static bool EvaluatePlanetDestroyedAction(TriggerAction tc, GameSession g, Trigger t)
		{
			PlanetDestroyedAction pda = tc as PlanetDestroyedAction;
			OrbitalObjectInfo orbitalObjectInfo = g.GameDatabase.GetStarSystemOrbitalObjectInfos(pda.SystemId).First<OrbitalObjectInfo>((Func<OrbitalObjectInfo, bool>)(x => x.Name == pda.OrbitName));
			g.GameDatabase.RemoveOrbitalObject(orbitalObjectInfo.ID);
			return true;
		}

		internal static bool EvaluatePlanetAddedAction(TriggerAction tc, GameSession g, Trigger t)
		{
			Random random = new Random();
			PlanetAddedAction paa = tc as PlanetAddedAction;
			OrbitalObjectInfo orbitalObjectInfo = (OrbitalObjectInfo)null;
			if (!string.IsNullOrEmpty(paa.NewPlanet.Parent))
				orbitalObjectInfo = g.GameDatabase.GetStarSystemOrbitalObjectInfos(paa.SystemId).First<OrbitalObjectInfo>((Func<OrbitalObjectInfo, bool>)(x => x.Name == paa.NewPlanet.Parent));
			float orbitStep = StarSystemVars.Instance.StarOrbitStep;
			if (orbitalObjectInfo != null)
				orbitStep = g.GameDatabase.GetPlanetInfo(orbitalObjectInfo.ID) == null ? StarSystemVars.Instance.GasGiantOrbitStep : StarSystemVars.Instance.PlanetOrbitStep;
			float parentRadius = StarSystemVars.Instance.StarRadius(StellarClass.Parse(g.GameDatabase.GetStarSystemInfo(paa.SystemId).StellarClass).Size);
			if (orbitalObjectInfo != null)
				parentRadius = StarSystemVars.Instance.SizeToRadius(g.GameDatabase.GetPlanetInfo(orbitalObjectInfo.ID).Size);
			float eccentricity = paa.NewPlanet.Eccentricity.HasValue ? paa.NewPlanet.Eccentricity.Value : random.NextNormal(StarSystemVars.Instance.OrbitEccentricityRange);
			if (!paa.NewPlanet.Inclination.HasValue)
			{
				double num1 = (double)random.NextNormal(StarSystemVars.Instance.OrbitInclinationRange);
			}
			else
			{
				double num2 = (double)paa.NewPlanet.Inclination.Value;
			}
			float num3 = Orbit.CalcOrbitRadius(parentRadius, orbitStep, paa.NewPlanet.OrbitNumber);
			float x1 = Ellipse.CalcSemiMinorAxis(num3, eccentricity);
			g.GameDatabase.InsertPlanet(new int?(orbitalObjectInfo.ID), paa.SystemId, new OrbitalPath()
			{
				Scale = new Vector2(x1, num3),
				InitialAngle = random.NextSingle() % 6.283185f
			}, paa.NewPlanet.Name, paa.NewPlanet.PlanetType, new int?(), paa.NewPlanet.Suitability.Value, paa.NewPlanet.Biosphere.Value, paa.NewPlanet.Resources.Value, (float)paa.NewPlanet.Size.Value);
			return true;
		}

		internal static bool EvaluateTerrainAppearsAction(TriggerAction tc, GameSession g, Trigger t)
		{
			return false;
		}

		internal static bool EvaluateTerrainDisappearsAction(
		  TriggerAction tc,
		  GameSession g,
		  Trigger t)
		{
			return false;
		}

		internal static bool EvaluateChangeTreasuryAction(TriggerAction tc, GameSession g, Trigger t)
		{
			ChangeTreasuryAction changeTreasuryAction = tc as ChangeTreasuryAction;
			g.GameDatabase.UpdatePlayerSavings(changeTreasuryAction.Player, g.GameDatabase.GetPlayerInfo(changeTreasuryAction.Player).Savings + (double)changeTreasuryAction.AmountToAdd);
			return true;
		}

		internal static bool EvaluateChangeResourcesAction(TriggerAction tc, GameSession g, Trigger t)
		{
			ChangeResourcesAction cra = tc as ChangeResourcesAction;
			OrbitalObjectInfo orbitalObjectInfo = g.GameDatabase.GetStarSystemOrbitalObjectInfos(cra.SystemId).First<OrbitalObjectInfo>((Func<OrbitalObjectInfo, bool>)(x => x.Name == cra.OrbitName));
			PlanetInfo planetInfo = g.GameDatabase.GetPlanetInfo(orbitalObjectInfo.ID);
			planetInfo.Resources += (int)cra.AmountToAdd;
			g.GameDatabase.UpdatePlanet(planetInfo);
			return true;
		}

		internal static bool EvaluateSpawnUnitAction(TriggerAction tc, GameSession g, Trigger t)
		{
			SpawnUnitAction spawnUnitAction = tc as SpawnUnitAction;
			DesignInfo design = new DesignInfo();
			design.PlayerID = spawnUnitAction.PlayerSlot;
			design.Name = spawnUnitAction.ShipToAdd.Name;
			List<DesignSectionInfo> designSectionInfoList = new List<DesignSectionInfo>();
			foreach (Section section in spawnUnitAction.ShipToAdd.Sections)
			{
				DesignSectionInfo designSectionInfo = new DesignSectionInfo()
				{
					DesignInfo = design
				};
				designSectionInfo.FilePath = string.Format("factions\\{0}\\sections\\{1}", (object)spawnUnitAction.ShipToAdd.Faction, (object)section.SectionFile);
				List<WeaponBankInfo> weaponBankInfoList = new List<WeaponBankInfo>();
				foreach (Bank bank in section.Banks)
					weaponBankInfoList.Add(new WeaponBankInfo()
					{
						WeaponID = g.GameDatabase.GetWeaponID(bank.Weapon, spawnUnitAction.PlayerSlot),
						BankGUID = Guid.Parse(bank.GUID)
					});
				designSectionInfo.WeaponBanks = weaponBankInfoList;
				designSectionInfoList.Add(designSectionInfo);
			}
			design.DesignSections = designSectionInfoList.ToArray();
			g.GameDatabase.InsertDesignByDesignInfo(design);
			return false;
		}

		internal static bool EvaluateRemoveFleetAction(TriggerAction tc, GameSession g, Trigger t)
		{
			return false;
		}

		internal static bool EvaluateAddWeaponAction(TriggerAction tc, GameSession g, Trigger t)
		{
			AddWeaponAction addWeaponAction = tc as AddWeaponAction;
			LogicalWeapon logicalWeaponFromFile = WeaponLibrary.CreateLogicalWeaponFromFile(g.App, addWeaponAction.WeaponFile, -1);
			g.GameDatabase.InsertWeapon(logicalWeaponFromFile, addWeaponAction.Player);
			return true;
		}

		internal static bool EvaluateRemoveWeaponAction(TriggerAction tc, GameSession g, Trigger t)
		{
			RemoveWeaponAction removeWeaponAction = tc as RemoveWeaponAction;
			g.GameDatabase.RemoveWeapon(g.GameDatabase.GetWeaponID(removeWeaponAction.WeaponFile, removeWeaponAction.Player));
			return true;
		}

		internal static bool EvaluateAddSectionAction(TriggerAction tc, GameSession g, Trigger t)
		{
			AddSectionAction addSectionAction = tc as AddSectionAction;
			g.GameDatabase.InsertSectionAsset(addSectionAction.SectionFile, addSectionAction.Player);
			return true;
		}

		internal static bool EvaluateRemoveSectionAction(TriggerAction tc, GameSession g, Trigger t)
		{
			RemoveSectionAction removeSectionAction = tc as RemoveSectionAction;
			g.GameDatabase.RemoveSection(g.GameDatabase.GetSectionAssetID(removeSectionAction.SectionFile, removeSectionAction.Player));
			return true;
		}

		internal static bool EvaluateAddModuleAction(TriggerAction tc, GameSession g, Trigger t)
		{
			AddModuleAction addModuleAction = tc as AddModuleAction;
			string factionName = g.GameDatabase.GetFactionName(g.GameDatabase.GetPlayerInfo(addModuleAction.Player).FactionID);
			g.GameDatabase.InsertModule(ModuleLibrary.CreateLogicalModuleFromFile(addModuleAction.ModuleFile, factionName), addModuleAction.Player);
			return true;
		}

		internal static bool EvaluateRemoveModuleAction(TriggerAction tc, GameSession g, Trigger t)
		{
			RemoveModuleAction removeModuleAction = tc as RemoveModuleAction;
			g.GameDatabase.RemoveModule(g.GameDatabase.GetModuleID(removeModuleAction.ModuleFile, removeModuleAction.Player));
			return true;
		}

		internal static bool EvaluateProvinceChangedAction(TriggerAction tc, GameSession g, Trigger t)
		{
			ProvinceChangedAction provinceChangedAction = tc as ProvinceChangedAction;
			ProvinceInfo pi = g.GameDatabase.GetProvinceInfo(provinceChangedAction.ProvinceId);
			int playerId = pi.PlayerID;
			pi.PlayerID = provinceChangedAction.NewPlayer;
			foreach (StarSystemInfo starSystemInfo in g.GameDatabase.GetStarSystemInfos().Where<StarSystemInfo>((Func<StarSystemInfo, bool>)(x =>
		   {
			   int? provinceId = x.ProvinceID;
			   int id = pi.ID;
			   if (provinceId.GetValueOrDefault() == id)
				   return provinceId.HasValue;
			   return false;
		   })))
			{
				foreach (PlanetInfo systemPlanetInfo in g.GameDatabase.GetStarSystemPlanetInfos(starSystemInfo.ID))
				{
					ColonyInfo colonyInfoForPlanet = g.GameDatabase.GetColonyInfoForPlanet(systemPlanetInfo.ID);
					if (colonyInfoForPlanet.PlayerID == playerId)
					{
						colonyInfoForPlanet.PlayerID = provinceChangedAction.NewPlayer;
						g.GameDatabase.UpdateColony(colonyInfoForPlanet);
					}
				}
			}
			g.GameDatabase.UpdateProvinceInfo(pi);
			return false;
		}

		internal static bool EvaluateSurrenderSystemAction(TriggerAction tc, GameSession g, Trigger t)
		{
			SurrenderSystemAction surrenderSystemAction = tc as SurrenderSystemAction;
			IEnumerable<PlanetInfo> systemPlanetInfos = (IEnumerable<PlanetInfo>)g.GameDatabase.GetStarSystemPlanetInfos(surrenderSystemAction.SystemId);
			List<int> intList = new List<int>();
			foreach (PlanetInfo planetInfo in systemPlanetInfos)
			{
				ColonyInfo colonyInfoForPlanet = g.GameDatabase.GetColonyInfoForPlanet(planetInfo.ID);
				if (colonyInfoForPlanet != null)
				{
					if (colonyInfoForPlanet.PlayerID != surrenderSystemAction.NewPlayer && !intList.Contains(colonyInfoForPlanet.PlayerID))
						intList.Add(colonyInfoForPlanet.PlayerID);
					colonyInfoForPlanet.PlayerID = surrenderSystemAction.NewPlayer;
					g.GameDatabase.UpdateColony(colonyInfoForPlanet);
				}
			}
			foreach (int playerID in intList)
				Kerberos.Sots.GameStates.StarSystem.RemoveSystemPlayerColor(g.GameDatabase, surrenderSystemAction.SystemId, playerID);
			return true;
		}

		internal static bool EvaluateSurrenderEmpireAction(TriggerAction tc, GameSession g, Trigger t)
		{
			SurrenderEmpireAction sea = tc as SurrenderEmpireAction;
			List<ColonyInfo> list = g.GameDatabase.GetColonyInfos().Where<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == sea.SurrenderingPlayer)).ToList<ColonyInfo>();
			List<int> intList = new List<int>();
			foreach (ColonyInfo colony in list)
			{
				OrbitalObjectInfo orbitalObjectInfo = g.GameDatabase.GetOrbitalObjectInfo(colony.OrbitalObjectID);
				if (orbitalObjectInfo != null && !intList.Contains(orbitalObjectInfo.StarSystemID))
					intList.Add(orbitalObjectInfo.StarSystemID);
				colony.PlayerID = sea.CapturingPlayer;
				g.GameDatabase.UpdateColony(colony);
			}
			foreach (int systemID in intList)
				Kerberos.Sots.GameStates.StarSystem.RemoveSystemPlayerColor(g.GameDatabase, systemID, sea.SurrenderingPlayer);
			return false;
		}

		internal static bool EvaluateStratModifierChangedAction(
		  TriggerAction tc,
		  GameSession g,
		  Trigger t)
		{
			return false;
		}

		internal static bool EvaluateDisplayMessageAction(TriggerAction tc, GameSession g, Trigger t)
		{
			DisplayMessageAction displayMessageAction = tc as DisplayMessageAction;
			string str = "";
			string[] strArray1 = displayMessageAction.Message.Split('$');
			for (int index = 0; index < strArray1.Length; ++index)
			{
				if (index % 2 == 0)
				{
					str += strArray1[index];
				}
				else
				{
					string[] strArray2;
					if (strArray1[index].Contains<char>('|'))
						strArray2 = strArray1[index].Split('|');
					else
						strArray2 = new string[2]
						{
			  "{0}",
			  strArray1[index]
						};
					if (g.TriggerScalars.ContainsKey(strArray1[index]))
						str += string.Format(strArray2[0], (object)g.TriggerScalars[strArray2[1]]);
				}
			}
			g.GameDatabase.InsertTurnEvent(new TurnEvent()
			{
				EventType = TurnEventType.EV_SCRIPT_MESSAGE,
				EventDesc = str,
				TurnNumber = g.GameDatabase.GetTurnCount()
			});
			return false;
		}

		internal static bool EvaluateMoveFleetAction(TriggerAction tc, GameSession g, Trigger t)
		{
			return false;
		}

		internal delegate bool EvaluateContextDelegate(TriggerContext tc, GameSession g, Trigger t);

		internal delegate bool EvaluateConditionDelegate(TriggerCondition tc, GameSession g, Trigger t);

		internal delegate bool EvaluateActionDelegate(TriggerAction ta, GameSession g, Trigger t);
	}
}
