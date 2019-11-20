// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.ScenarioEnumerations
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class ScenarioEnumerations
	{
		public static string FleetsInRangeVariableName = "$FleetsInRange$";
		public static string[] FleetConditionShipTypes = new string[5]
		{
	  "Cruiser",
	  "Dreadnaught",
	  "Leviathan",
	  "Colony",
	  "Supply"
		};
		public static Dictionary<string, GovernmentInfo.GovernmentType> GovernmentTypes = new Dictionary<string, GovernmentInfo.GovernmentType>()
	{
	  {
		"Centrism",
		GovernmentInfo.GovernmentType.Centrism
	  },
	  {
		"Communalism",
		GovernmentInfo.GovernmentType.Communalism
	  },
	  {
		"Junta",
		GovernmentInfo.GovernmentType.Junta
	  },
	  {
		"Plutocracy",
		GovernmentInfo.GovernmentType.Plutocracy
	  },
	  {
		"Socialism",
		GovernmentInfo.GovernmentType.Socialism
	  },
	  {
		"Mercantilism",
		GovernmentInfo.GovernmentType.Mercantilism
	  },
	  {
		"Cooperativism",
		GovernmentInfo.GovernmentType.Cooperativism
	  },
	  {
		"Anarchism",
		GovernmentInfo.GovernmentType.Anarchism
	  },
	  {
		"Liberationism",
		GovernmentInfo.GovernmentType.Liberationism
	  }
	};
		public static Dictionary<string, Type> ContextTypeMap = new Dictionary<string, Type>()
	{
	  {
		"AlwaysContext",
		typeof (AlwaysContext)
	  },
	  {
		"StartContext",
		typeof (StartContext)
	  },
	  {
		"EndContext",
		typeof (EndContext)
	  },
	  {
		"RangeContext",
		typeof (RangeContext)
	  }
	};
		public static Dictionary<string, Type> ConditionTypeMap = new Dictionary<string, Type>()
	{
	  {
		"GameOver",
		typeof (GameOverCondition)
	  },
	  {
		"ScalarAmount",
		typeof (ScalarAmountCondition)
	  },
	  {
		"TriggerTriggered",
		typeof (TriggerTriggeredCondition)
	  },
	  {
		"SystemRange",
		typeof (SystemRangeCondition)
	  },
	  {
		"FleetRange",
		typeof (FleetRangeCondition)
	  },
	  {
		"ProvinceRange",
		typeof (ProvinceRangeCondition)
	  },
	  {
		"ColonyDeath",
		typeof (ColonyDeathCondition)
	  },
	  {
		"PlanetDeath",
		typeof (PlanetDeathCondition)
	  },
	  {
		"FleetDeath",
		typeof (FleetDeathCondition)
	  },
	  {
		"ShipDeath",
		typeof (ShipDeathCondition)
	  },
	  {
		"AdmiralDeath",
		typeof (AdmiralDeathCondition)
	  },
	  {
		"PlayerDeath",
		typeof (PlayerDeathCondition)
	  },
	  {
		"AllianceFormed",
		typeof (AllianceFormedCondition)
	  },
	  {
		"AllianceBroken",
		typeof (AllianceBrokenCondition)
	  },
	  {
		"GrandMenaceAppeared",
		typeof (GrandMenaceAppearedCondition)
	  },
	  {
		"GrandMenaceDestroyed",
		typeof (GrandMenaceDestroyedCondition)
	  },
	  {
		"ResourceAmount",
		typeof (ResourceAmountCondition)
	  },
	  {
		"PlanetAmount",
		typeof (PlanetAmountCondition)
	  },
	  {
		"BiosphereAmount",
		typeof (BiosphereAmountCondition)
	  },
	  {
		"AllianceAmount",
		typeof (AllianceAmountCondition)
	  },
	  {
		"PopulationAmount",
		typeof (PopulationAmountCondition)
	  },
	  {
		"FleetAmount",
		typeof (FleetAmountCondition)
	  },
	  {
		"CommandPointAmount",
		typeof (CommandPointAmountCondition)
	  },
	  {
		"TerrainRange",
		typeof (TerrainRangeCondition)
	  },
	  {
		"TerrainColonized",
		typeof (TerrainColonizedCondition)
	  },
	  {
		"PlanetColonized",
		typeof (PlanetColonizedCondition)
	  },
	  {
		"TreatyBroken",
		typeof (TreatyBrokenCondition)
	  },
	  {
		"CivilianDeath",
		typeof (CivilianDeathCondition)
	  },
	  {
		"MoralAmount",
		typeof (MoralAmountCondition)
	  },
	  {
		"GovernmentType",
		typeof (GovernmentTypeCondition)
	  },
	  {
		"RevelationBegins",
		typeof (RevelationBeginsCondition)
	  },
	  {
		"ResearchObtained",
		typeof (ResearchObtainedCondition)
	  },
	  {
		"ClassBuilt",
		typeof (ClassBuiltCondition)
	  },
	  {
		"TradePointsAmount",
		typeof (TradePointsAmountCondition)
	  },
	  {
		"IncomePerTurn",
		typeof (IncomePerTurnAmountCondition)
	  },
	  {
		"FactionEncountered",
		typeof (FactionEncounteredCondition)
	  },
	  {
		"WorldType",
		typeof (WorldTypeCondition)
	  },
	  {
		"PlanetDevelopmentAmount",
		typeof (PlanetDevelopmentAmountCondition)
	  }
	};
		public static Dictionary<string, Type> ActionTypeMap = new Dictionary<string, Type>()
	{
	  {
		"GameOverAction",
		typeof (GameOverAction)
	  },
	  {
		"PointPerPlanetDeathAction",
		typeof (PointPerPlanetDeathAction)
	  },
	  {
		"PointPerColonyDeathAction",
		typeof (PointPerColonyDeathAction)
	  },
	  {
		"PointPerShipTypeAction",
		typeof (PointPerShipTypeAction)
	  },
	  {
		"AddScalarToScalarAction",
		typeof (AddScalarToScalarAction)
	  },
	  {
		"SetScalar",
		typeof (SetScalarAction)
	  },
	  {
		"ChangeScalarAction",
		typeof (ChangeScalarAction)
	  },
	  {
		"SpawnUnit",
		typeof (SpawnUnitAction)
	  },
	  {
		"DiplomacyChanged",
		typeof (DiplomacyChangedAction)
	  },
	  {
		"ColonyChanged",
		typeof (ColonyChangedAction)
	  },
	  {
		"AIStrategyChanged",
		typeof (AIStrategyChangedAction)
	  },
	  {
		"ResearchAwarded",
		typeof (ResearchAwardedAction)
	  },
	  {
		"AdmiralChanged",
		typeof (AdmiralChangedAction)
	  },
	  {
		"RebellionOccurs",
		typeof (RebellionOccursAction)
	  },
	  {
		"RebellionEnds",
		typeof (RebellionEndsAction)
	  },
	  {
		"PlanetDestroyed",
		typeof (PlanetDestroyedAction)
	  },
	  {
		"PlanetAdded",
		typeof (PlanetAddedAction)
	  },
	  {
		"TerrainAppears",
		typeof (TerrainAppearsAction)
	  },
	  {
		"TerrainDisappears",
		typeof (TerrainDisappearsAction)
	  },
	  {
		"ChangedTreasury",
		typeof (ChangeTreasuryAction)
	  },
	  {
		"ChangeResources",
		typeof (ChangeResourcesAction)
	  },
	  {
		"RemoveFleetAction",
		typeof (RemoveFleetAction)
	  },
	  {
		"AddWeapon",
		typeof (AddWeaponAction)
	  },
	  {
		"RemoveWeapon",
		typeof (RemoveWeaponAction)
	  },
	  {
		"AddSection",
		typeof (AddSectionAction)
	  },
	  {
		"RemoveSection",
		typeof (RemoveSectionAction)
	  },
	  {
		"AddModule",
		typeof (AddModuleAction)
	  },
	  {
		"RemoveModule",
		typeof (RemoveModuleAction)
	  },
	  {
		"ProvinceChanged",
		typeof (ProvinceChangedAction)
	  },
	  {
		"SurrenderSystem",
		typeof (SurrenderSystemAction)
	  },
	  {
		"SurrenderEmpire",
		typeof (SurrenderEmpireAction)
	  },
	  {
		"StratModifierChanged",
		typeof (StratModifierChangedAction)
	  },
	  {
		"DisplayMessage",
		typeof (DisplayMessageAction)
	  },
	  {
		"MoveFleet",
		typeof (MoveFleetAction)
	  }
	};
		public static Dictionary<string, StationType> StationTypes = new Dictionary<string, StationType>()
	{
	  {
		"Civilian",
		StationType.CIVILIAN
	  },
	  {
		"Diplomatic",
		StationType.DIPLOMATIC
	  },
	  {
		"Naval",
		StationType.NAVAL
	  },
	  {
		"Science",
		StationType.SCIENCE
	  },
	  {
		"Gate",
		StationType.GATE
	  }
	};
		public static string[] PlayerRelations = new string[1]
		{
	  "Aggressive"
		};
		public static string[] DiplomacyRules = new string[4]
		{
	  "Alliance",
	  "Non-Aggression Pact",
	  "Cease-Fire Agreement",
	  "War"
		};
		public static string[] Factions = new string[7]
		{
	  "",
	  "hiver",
	  "human",
	  "liir_zuul",
	  "morrigi",
	  "tarkas",
	  "zuul"
		};
		public static string[] Races = new string[7]
		{
	  "hiver",
	  "hordezuul",
	  "human",
	  "liir",
	  "morrigi",
	  "presterzuul",
	  "tarka"
		};
		public static int[] StationStages = new int[5]
		{
	  1,
	  2,
	  3,
	  4,
	  5
		};
		public static string[] AdmiralGenders = new string[4]
		{
	  "",
	  "Male",
	  "Female",
	  "Other"
		};
		public static string[] AIDifficulty = new string[4]
		{
	  "",
	  "Easy",
	  "Normal",
	  "Difficult"
		};

		public static List<string> GetRacesForFaction(string faction)
		{
			List<string> stringList = new List<string>();
			switch (faction)
			{
				case "hiver":
					stringList.Add("hiver");
					break;
				case "human":
					stringList.Add("human");
					break;
				case "liir_zuul":
					stringList.Add("liir");
					stringList.Add("presterzuul");
					break;
				case "morrigi":
					stringList.Add("morrigi");
					stringList.Add("human");
					stringList.Add("tarka");
					stringList.Add("hiver");
					stringList.Add("presterzuul");
					stringList.Add("liir");
					break;
				case "tarkas":
					stringList.Add("tarka");
					break;
				case "zuul":
					stringList.Add("hordezuul");
					break;
				case "loa":
					stringList.Add("loa");
					break;
			}
			return stringList;
		}

		public static List<string> GetFactionsForRace(string race)
		{
			List<string> stringList = new List<string>();
			switch (race)
			{
				case "hiver":
					stringList.Add("hiver");
					stringList.Add("morrigi");
					break;
				case "human":
					stringList.Add("human");
					stringList.Add("morrigi");
					break;
				case "hordezuul":
					stringList.Add("zuul");
					break;
				case "liir":
					stringList.Add("liir_zuul");
					stringList.Add("morrigi");
					break;
				case "morrigi":
					stringList.Add("morrigi");
					break;
				case "presterzuul":
					stringList.Add("liir_zuul");
					stringList.Add("morrigi");
					break;
				case "tarka":
					stringList.Add("tarkas");
					stringList.Add("morrigi");
					break;
			}
			return stringList;
		}
	}
}
