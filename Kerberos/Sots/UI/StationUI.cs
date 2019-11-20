// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.StationUI
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	public class StationUI
	{
		public static Dictionary<string, ModuleEnums.StationModuleType> ModuleToStationModuleTypeMap = new Dictionary<string, ModuleEnums.StationModuleType>()
	{
	  {
		"moduleEWPLab",
		ModuleEnums.StationModuleType.EWPLab
	  },
	  {
		"moduleTRPLab",
		ModuleEnums.StationModuleType.TRPLab
	  },
	  {
		"moduleNRGLab",
		ModuleEnums.StationModuleType.NRGLab
	  },
	  {
		"moduleWARLab",
		ModuleEnums.StationModuleType.WARLab
	  },
	  {
		"moduleBALLab",
		ModuleEnums.StationModuleType.BALLab
	  },
	  {
		"moduleBIOLab",
		ModuleEnums.StationModuleType.BIOLab
	  },
	  {
		"moduleINDLab",
		ModuleEnums.StationModuleType.INDLab
	  },
	  {
		"moduleCCCLab",
		ModuleEnums.StationModuleType.CCCLab
	  },
	  {
		"moduleDRVLab",
		ModuleEnums.StationModuleType.DRVLab
	  },
	  {
		"modulePOLLab",
		ModuleEnums.StationModuleType.POLLab
	  },
	  {
		"modulePSILab",
		ModuleEnums.StationModuleType.PSILab
	  },
	  {
		"moduleENGLab",
		ModuleEnums.StationModuleType.ENGLab
	  },
	  {
		"moduleBRDLab",
		ModuleEnums.StationModuleType.BRDLab
	  },
	  {
		"moduleSLDLab",
		ModuleEnums.StationModuleType.SLDLab
	  },
	  {
		"moduleCYBLab",
		ModuleEnums.StationModuleType.CYBLab
	  },
	  {
		"moduleSensor",
		ModuleEnums.StationModuleType.Sensor
	  },
	  {
		"moduleCustoms",
		ModuleEnums.StationModuleType.Customs
	  },
	  {
		"moduleCombat",
		ModuleEnums.StationModuleType.Combat
	  },
	  {
		"moduleRepair",
		ModuleEnums.StationModuleType.Repair
	  },
	  {
		"moduleWarehouse",
		ModuleEnums.StationModuleType.Warehouse
	  },
	  {
		"moduleCommand",
		ModuleEnums.StationModuleType.Command
	  },
	  {
		"moduleDock",
		ModuleEnums.StationModuleType.Dock
	  },
	  {
		"moduleHumanHabitation",
		ModuleEnums.StationModuleType.HumanHabitation
	  },
	  {
		"moduleTarkasHabitation",
		ModuleEnums.StationModuleType.TarkasHabitation
	  },
	  {
		"moduleLiirHabitation",
		ModuleEnums.StationModuleType.LiirHabitation
	  },
	  {
		"moduleHiverHabitation",
		ModuleEnums.StationModuleType.HiverHabitation
	  },
	  {
		"moduleMorrigiHabitation",
		ModuleEnums.StationModuleType.MorrigiHabitation
	  },
	  {
		"moduleZuulHabitation",
		ModuleEnums.StationModuleType.ZuulHabitation
	  },
	  {
		"moduleLoaHabitation",
		ModuleEnums.StationModuleType.LoaHabitation
	  },
	  {
		"moduleHumanTrade",
		ModuleEnums.StationModuleType.HumanTradeModule
	  },
	  {
		"moduleTarkaTrade",
		ModuleEnums.StationModuleType.TarkasTradeModule
	  },
	  {
		"moduleLiirTrade",
		ModuleEnums.StationModuleType.LiirTradeModule
	  },
	  {
		"moduleHiverTrade",
		ModuleEnums.StationModuleType.HiverTradeModule
	  },
	  {
		"moduleMorrigiTrade",
		ModuleEnums.StationModuleType.MorrigiTradeModule
	  },
	  {
		"moduleZuulTrade",
		ModuleEnums.StationModuleType.ZuulTradeModule
	  },
	  {
		"moduleLoaTrade",
		ModuleEnums.StationModuleType.LoaTradeModule
	  },
	  {
		"moduleTerraform",
		ModuleEnums.StationModuleType.Terraform
	  },
	  {
		"moduleHumanLargeHabitation",
		ModuleEnums.StationModuleType.HumanLargeHabitation
	  },
	  {
		"moduleTarkasLargeHabitation",
		ModuleEnums.StationModuleType.TarkasLargeHabitation
	  },
	  {
		"moduleLiirLargeHabitation",
		ModuleEnums.StationModuleType.LiirLargeHabitation
	  },
	  {
		"moduleHiverLargeHabitation",
		ModuleEnums.StationModuleType.HiverLargeHabitation
	  },
	  {
		"moduleMorrigiLargeHabitation",
		ModuleEnums.StationModuleType.MorrigiLargeHabitation
	  },
	  {
		"moduleZuulLargeHabitation",
		ModuleEnums.StationModuleType.ZuulLargeHabitation
	  },
	  {
		"moduleLoaLargeHabitation",
		ModuleEnums.StationModuleType.LoaLargeHabitation
	  },
	  {
		"moduleBastion",
		ModuleEnums.StationModuleType.Bastion
	  },
	  {
		"moduleAmp",
		ModuleEnums.StationModuleType.Amp
	  },
	  {
		"moduleDefence",
		ModuleEnums.StationModuleType.Defence
	  },
	  {
		"moduleGateLab",
		ModuleEnums.StationModuleType.GateLab
	  }
	};
		public static Dictionary<string, string> ModuleTypeMap = new Dictionary<string, string>()
	{
	  {
		"moduleHiverTrade",
		"Trade"
	  },
	  {
		"moduleTarkaTrade",
		"Trade"
	  },
	  {
		"moduleHumanTrade",
		"Trade"
	  },
	  {
		"moduleMorrigiTrade",
		"Trade"
	  },
	  {
		"moduleLiirTrade",
		"Trade"
	  },
	  {
		"moduleLoaTrade",
		"Trade"
	  },
	  {
		"moduleEWPLab",
		"Lab"
	  },
	  {
		"moduleTRPLab",
		"Lab"
	  },
	  {
		"moduleNRGLab",
		"Lab"
	  },
	  {
		"moduleWARLab",
		"Lab"
	  },
	  {
		"moduleBALLab",
		"Lab"
	  },
	  {
		"moduleBIOLab",
		"Lab"
	  },
	  {
		"moduleINDLab",
		"Lab"
	  },
	  {
		"moduleCCCLab",
		"Lab"
	  },
	  {
		"moduleDRVLab",
		"Lab"
	  },
	  {
		"modulePOLLab",
		"Lab"
	  },
	  {
		"modulePSILab",
		"Lab"
	  },
	  {
		"moduleENGLab",
		"Lab"
	  },
	  {
		"moduleBRDLab",
		"Lab"
	  },
	  {
		"moduleSLDLab",
		"Lab"
	  },
	  {
		"moduleCYBLab",
		"Lab"
	  },
	  {
		"moduleSensor",
		"Sensor"
	  },
	  {
		"moduleCustoms",
		"Customs"
	  },
	  {
		"moduleCombat",
		"Combat"
	  },
	  {
		"moduleRepair",
		"Repair"
	  },
	  {
		"moduleWarehouse",
		"Warehouse"
	  },
	  {
		"moduleCommand",
		"Command"
	  },
	  {
		"moduleDock",
		"Dock"
	  },
	  {
		"moduleHumanHabitation",
		"Habitation"
	  },
	  {
		"moduleTarkasHabitation",
		"Habitation"
	  },
	  {
		"moduleLiirHabitation",
		"Habitation"
	  },
	  {
		"moduleHiverHabitation",
		"Habitation"
	  },
	  {
		"moduleMorrigiHabitation",
		"Habitation"
	  },
	  {
		"moduleZuulHabitation",
		"Habitation"
	  },
	  {
		"moduleLoaHabitation",
		"Habitation"
	  },
	  {
		"moduleTerraform",
		"Terraform"
	  },
	  {
		"moduleHumanLargeHabitation",
		"LargeHabitation"
	  },
	  {
		"moduleTarkasLargeHabitation",
		"LargeHabitation"
	  },
	  {
		"moduleLiirLargeHabitation",
		"LargeHabitation"
	  },
	  {
		"moduleHiverLargeHabitation",
		"LargeHabitation"
	  },
	  {
		"moduleMorrigiLargeHabitation",
		"LargeHabitation"
	  },
	  {
		"moduleZuulLargeHabitation",
		"LargeHabitation"
	  },
	  {
		"moduleLoaLargeHabitation",
		"LargeHabitation"
	  },
	  {
		"moduleBastion",
		"Bastion"
	  },
	  {
		"moduleAmp",
		"Gate"
	  },
	  {
		"moduleDefence",
		"Defence"
	  },
	  {
		"moduleGateLab",
		"GateLab"
	  }
	};
		private static readonly string[] UIDefenceStationModules = new string[1]
		{
	  "moduleDefence"
		};
		private static readonly string[] UIMiningStationModules = new string[1]
		{
	  "moduleMining"
		};
		private static readonly string[] UINavalStationModules = new string[6]
		{
	  "moduleSensor",
	  "moduleWarehouse",
	  "moduleRepair",
	  "moduleCommand",
	  "moduleDock",
	  "moduleCombat"
		};
		private static readonly string[] UIScienceStationModules = new string[25]
		{
	  "moduleHumanHabitation",
	  "moduleTarkasHabitation",
	  "moduleLiirHabitation",
	  "moduleHiverHabitation",
	  "moduleMorrigiHabitation",
	  "moduleZuulHabitation",
	  "moduleLoaHabitation",
	  "moduleSensor",
	  "moduleDock",
	  "moduleWarehouse",
	  "moduleEWPLab",
	  "moduleTRPLab",
	  "moduleNRGLab",
	  "moduleWARLab",
	  "moduleBALLab",
	  "moduleBIOLab",
	  "moduleINDLab",
	  "moduleCCCLab",
	  "moduleDRVLab",
	  "modulePOLLab",
	  "modulePSILab",
	  "moduleENGLab",
	  "moduleBRDLab",
	  "moduleSLDLab",
	  "moduleCYBLab"
		};
		private static readonly string[] UIDiplomaticStationModules = new string[17]
		{
	  "moduleSensor",
	  "moduleDock",
	  "moduleCustoms",
	  "moduleHumanHabitation",
	  "moduleTarkasHabitation",
	  "moduleLiirHabitation",
	  "moduleHiverHabitation",
	  "moduleMorrigiHabitation",
	  "moduleZuulHabitation",
	  "moduleLoaHabitation",
	  "moduleHumanLargeHabitation",
	  "moduleTarkasLargeHabitation",
	  "moduleLiirLargeHabitation",
	  "moduleHiverLargeHabitation",
	  "moduleMorrigiLargeHabitation",
	  "moduleZuulLargeHabitation",
	  "moduleLoaLargeHabitation"
		};
		private static readonly string[] UICivilianStationModules = new string[24]
		{
	  "moduleTerraform",
	  "moduleDock",
	  "moduleWarehouse",
	  "moduleSensor",
	  "moduleHiverTrade",
	  "moduleTarkaTrade",
	  "moduleHumanTrade",
	  "moduleMorrigiTrade",
	  "moduleLiirTrade",
	  "moduleLoaTrade",
	  "moduleHumanHabitation",
	  "moduleTarkasHabitation",
	  "moduleLiirHabitation",
	  "moduleHiverHabitation",
	  "moduleMorrigiHabitation",
	  "moduleZuulHabitation",
	  "moduleLoaHabitation",
	  "moduleHumanLargeHabitation",
	  "moduleTarkasLargeHabitation",
	  "moduleLiirLargeHabitation",
	  "moduleHiverLargeHabitation",
	  "moduleMorrigiLargeHabitation",
	  "moduleZuulLargeHabitation",
	  "moduleLoaLargeHabitation"
		};
		private static readonly string[] UIGateStationModules = new string[7]
		{
	  "moduleHiverHabitation",
	  "moduleDock",
	  "moduleBastion",
	  "moduleSensor",
	  "moduleAmp",
	  "moduleDefence",
	  "moduleGateLab"
		};
		public static Dictionary<StationType, string[]> StationModuleMap = new Dictionary<StationType, string[]>()
	{
	  {
		StationType.CIVILIAN,
		StationUI.UICivilianStationModules
	  },
	  {
		StationType.DEFENCE,
		StationUI.UIDefenceStationModules
	  },
	  {
		StationType.DIPLOMATIC,
		StationUI.UIDiplomaticStationModules
	  },
	  {
		StationType.GATE,
		StationUI.UIGateStationModules
	  },
	  {
		StationType.MINING,
		StationUI.UIMiningStationModules
	  },
	  {
		StationType.NAVAL,
		StationUI.UINavalStationModules
	  },
	  {
		StationType.SCIENCE,
		StationUI.UIScienceStationModules
	  }
	};
		private static Dictionary<StationType, string> StationModulePanelMap = new Dictionary<StationType, string>()
	{
	  {
		StationType.CIVILIAN,
		"civilianModules"
	  },
	  {
		StationType.DEFENCE,
		""
	  },
	  {
		StationType.DIPLOMATIC,
		"diplomaticModules"
	  },
	  {
		StationType.GATE,
		"gateModules"
	  },
	  {
		StationType.MINING,
		""
	  },
	  {
		StationType.NAVAL,
		"navalModules"
	  },
	  {
		StationType.SCIENCE,
		"scienceModules"
	  }
	};
		private static Dictionary<StationType, string> StationIconMap = new Dictionary<StationType, string>()
	{
	  {
		StationType.CIVILIAN,
		"stationicon_civilian"
	  },
	  {
		StationType.DEFENCE,
		"stationicon_SDB"
	  },
	  {
		StationType.DIPLOMATIC,
		"stationicon_diplomatic"
	  },
	  {
		StationType.GATE,
		"stationicon_gate"
	  },
	  {
		StationType.MINING,
		"stationicon_mining"
	  },
	  {
		StationType.NAVAL,
		"stationicon_naval"
	  },
	  {
		StationType.SCIENCE,
		"stationicon_science"
	  }
	};

		private static string GetStationIcon(StationType type, bool zuul)
		{
			string str = StationUI.StationIconMap[type];
			if (zuul && type == StationType.CIVILIAN)
				str = "stationicon_slave";
			else if (zuul && type == StationType.DIPLOMATIC)
				str = "stationicon_tribute";
			return str;
		}

		private static void SyncModuleItemControl(
		  GameSession game,
		  string panelName,
		  int modulesBuilt,
		  int modulesQueued,
		  int modulesAvailable,
		  string itemId)
		{
			if (itemId != null)
			{
				game.UI.SetPropertyString(game.UI.Path(panelName, "module_up"), "id", string.Format("{0}|{1}|module_up", (object)itemId, (object)((IEnumerable<string>)panelName.Split('.')).Last<string>()));
				game.UI.SetPropertyString(game.UI.Path(panelName, "module_down"), "id", string.Format("{0}|{1}|module_down", (object)itemId, (object)((IEnumerable<string>)panelName.Split('.')).Last<string>()));
			}
			game.UI.SetPropertyString(game.UI.Path(panelName, "module_que"), "text", modulesQueued.ToString());
			game.UI.SetPropertyString(game.UI.Path(panelName, "module_built"), "text", string.Format("{0}/{1}", (object)modulesBuilt, (object)(modulesQueued + modulesBuilt + modulesAvailable)));
		}

		private static void SyncStationDetailsControl(
		  GameSession game,
		  string panelName,
		  StationInfo station)
		{
			ShipSectionAsset shipSectionAsset = game.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == station.DesignInfo.DesignSections[0].FilePath));
			int crew = shipSectionAsset.Crew;
			float structure = station.DesignInfo.Structure;
			int stationUpkeepCost = GameSession.CalculateStationUpkeepCost(game.GameDatabase, game.AssetDatabase, station);
			float tacticalSensorRange = shipSectionAsset.TacticalSensorRange;
			float strategicSensorRange = shipSectionAsset.StrategicSensorRange;
			foreach (DesignModuleInfo module in station.DesignInfo.DesignSections[0].Modules)
			{
				DesignModuleInfo dmi = module;
				LogicalModule logicalModule = game.AssetDatabase.Modules.First<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == game.GameDatabase.GetModuleAsset(dmi.ModuleID)));
				crew += logicalModule.Crew;
				tacticalSensorRange += logicalModule.SensorBonus;
			}
			string propertyValue1 = crew.ToString();
			string propertyValue2 = strategicSensorRange.ToString();
			string propertyValue3 = tacticalSensorRange.ToString();
			string propertyValue4 = stationUpkeepCost.ToString();
			string propertyValue5 = structure.ToString();
			game.UI.SetPropertyString(game.UI.Path(panelName, "popVal"), "text", propertyValue1);
			game.UI.SetPropertyString(game.UI.Path(panelName, "stratVal"), "text", propertyValue2);
			game.UI.SetPropertyString(game.UI.Path(panelName, "tactVal"), "text", propertyValue3);
			game.UI.SetPropertyString(game.UI.Path(panelName, "maintVal"), "text", propertyValue4);
			game.UI.SetPropertyString(game.UI.Path(panelName, "structVal"), "text", propertyValue5);
		}

		internal static void SyncStationModulesControl(
		  GameSession game,
		  string panelName,
		  StationInfo station,
		  string itemId = null)
		{
			foreach (KeyValuePair<StationType, string> stationModulePanel in StationUI.StationModulePanelMap)
				game.UI.SetVisible(game.UI.Path(panelName, stationModulePanel.Value), (stationModulePanel.Key == station.DesignInfo.StationType ? 1 : 0) != 0);
			string str1 = game.UI.Path(panelName, StationUI.StationModulePanelMap[station.DesignInfo.StationType]);
			List<LogicalModuleMount> list1 = ((IEnumerable<LogicalModuleMount>)game.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == station.DesignInfo.DesignSections[0].FilePath)).Modules).ToList<LogicalModuleMount>();
			List<DesignModuleInfo> builtModules = station.DesignInfo.DesignSections[0].Modules;
			List<DesignModuleInfo> queuedModules = game.GameDatabase.GetQueuedStationModules(station.DesignInfo.DesignSections[0]).ToList<DesignModuleInfo>();
			foreach (string str2 in StationUI.StationModuleMap[station.DesignInfo.StationType])
			{
				string s = str2;
				List<LogicalModuleMount> list2 = list1.Where<LogicalModuleMount>((Func<LogicalModuleMount, bool>)(x => x.ModuleType == StationUI.ModuleTypeMap[s])).ToList<LogicalModuleMount>();
				List<LogicalModuleMount> list3 = list2.Where<LogicalModuleMount>((Func<LogicalModuleMount, bool>)(x => builtModules.Any<DesignModuleInfo>((Func<DesignModuleInfo, bool>)(y => y.MountNodeName == x.NodeName)))).ToList<LogicalModuleMount>();
				List<LogicalModuleMount> list4 = list2.Where<LogicalModuleMount>((Func<LogicalModuleMount, bool>)(x => queuedModules.Any<DesignModuleInfo>((Func<DesignModuleInfo, bool>)(y => y.MountNodeName == x.NodeName)))).ToList<LogicalModuleMount>();
				List<DesignModuleInfo> list5 = builtModules.Where<DesignModuleInfo>((Func<DesignModuleInfo, bool>)(x =>
			   {
				   ModuleEnums.StationModuleType? stationModuleType1 = x.StationModuleType;
				   ModuleEnums.StationModuleType stationModuleType2 = StationUI.ModuleToStationModuleTypeMap[s];
				   if (stationModuleType1.GetValueOrDefault() == stationModuleType2)
					   return stationModuleType1.HasValue;
				   return false;
			   })).ToList<DesignModuleInfo>();
				List<DesignModuleInfo> list6 = queuedModules.Where<DesignModuleInfo>((Func<DesignModuleInfo, bool>)(x =>
			   {
				   ModuleEnums.StationModuleType? stationModuleType1 = x.StationModuleType;
				   ModuleEnums.StationModuleType stationModuleType2 = StationUI.ModuleToStationModuleTypeMap[s];
				   if (stationModuleType1.GetValueOrDefault() == stationModuleType2)
					   return stationModuleType1.HasValue;
				   return false;
			   })).ToList<DesignModuleInfo>();
				int count1 = list5.Count;
				int count2 = list6.Count;
				int modulesAvailable = list2.Count - list3.Count - list4.Count;
				StationUI.SyncModuleItemControl(game, game.UI.Path(str1, s), count1, count2, modulesAvailable, itemId);
			}
		}

		internal static void SyncStationDetailsWidget(
		  GameSession game,
		  string panelName,
		  int stationID,
		  bool updateButtonIds)
		{
			StationInfo station = game.GameDatabase.GetStationInfo(stationID);
			StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(game.GameDatabase.GetOrbitalObjectInfo(station.OrbitalObjectID).StarSystemID);
			Dictionary<ModuleEnums.StationModuleType, int> requiredModules1;
			float stationUpgradeProgress1 = game.GetStationUpgradeProgress(station, out requiredModules1);
			string propertyValue = string.Format("{0}|station_upgrade", (object)station.OrbitalObjectID);
			if (updateButtonIds)
				game.UI.SetPropertyString(game.UI.Path(panelName, "station_upgrade"), "id", propertyValue);
			game.UI.SetPropertyString(game.UI.Path(panelName, "station_system"), "text", starSystemInfo.Name);
			game.UI.SetPropertyString(game.UI.Path(panelName, "station_name"), "text", game.GameDatabase.GetOrbitalObjectInfo(station.OrbitalObjectID).Name);
			game.UI.SetPropertyString(game.UI.Path(panelName, "station_preview"), "sprite", StationUI.GetStationIcon(station.DesignInfo.StationType, game.GameDatabase.GetFactionName(game.GameDatabase.GetPlayerFactionID(station.PlayerID)) == "zuul"));
			game.UI.SetPropertyString(game.UI.Path(panelName, "station_level"), "text", string.Format("{0} {1}", (object)App.Localize("@UI_STATIONMANAGER_STAGE"), (object)station.DesignInfo.StationLevel));
			game.UI.SetPropertyString(game.UI.Path(panelName, "stationLevel"), "text", string.Format("{0}", (object)station.DesignInfo.StationLevel));
			game.UI.SetEnabled(game.UI.Path(panelName, propertyValue), ((double)stationUpgradeProgress1 == 1.0 ? 1 : 0) != 0);
			game.UI.SetPropertyInt(game.UI.Path(panelName, "station_progress"), "value", (int)((double)stationUpgradeProgress1 * 100.0));
			game.UI.SetPropertyString(game.UI.Path(panelName, "itemSubTitle"), "text", station.DesignInfo.StationType.ToDisplayText(game.GameDatabase.GetFactionName(game.GameDatabase.GetPlayerFactionID(station.PlayerID))) + " | " + station.DesignInfo.Name);
			game.UI.SetPropertyString(game.UI.Path(panelName, "stationPopulation"), "text", station.DesignInfo.CrewRequired.ToString("N0"));
			int[] healthAndHealthMax = Kerberos.Sots.StarFleet.StarFleet.GetHealthAndHealthMax(game, station.DesignInfo, station.ShipID);
			game.UI.SetPropertyString(game.UI.Path(panelName, "stationStructure"), "text", healthAndHealthMax[0].ToString() + "/" + (object)healthAndHealthMax[1]);
			game.UI.SetPropertyString(game.UI.Path(panelName, "stationUpkeep"), "text", GameSession.CalculateStationUpkeepCost(game.GameDatabase, game.AssetDatabase, station).ToString("N0"));
			string str1 = station.GetBaseStratSensorRange().ToString();
			float stratSensorRange = game.GameDatabase.GetStationAdditionalStratSensorRange(station);
			if ((double)stratSensorRange > 0.0)
				str1 = str1 + "(+" + stratSensorRange.ToString() + ")";
			string str2 = GameSession.GetStationBaseTacSensorRange(game, station).ToString("N0");
			float additionalTacSensorRange = GameSession.GetStationAdditionalTacSensorRange(game, station);
			if ((double)additionalTacSensorRange > 0.0)
				str2 = str2 + "(+" + additionalTacSensorRange.ToString("N0") + ")";
			game.UI.SetPropertyString(game.UI.Path(panelName, "stationStratSensorRange"), "text", str1 + " ly");
			game.UI.SetPropertyString(game.UI.Path(panelName, "stationTacSensorRange"), "text", str2 + " km");
			StationUI.SyncStationDetailsControl(game, game.UI.Path(panelName, "generalstats"), station);
			if (updateButtonIds)
				StationUI.SyncStationModulesControl(game, game.UI.Path(panelName, "module_details"), station, stationID.ToString());
			else
				StationUI.SyncStationModulesControl(game, game.UI.Path(panelName, "module_details"), station, null);
			((IEnumerable<LogicalModuleMount>)game.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == station.DesignInfo.DesignSections[0].FilePath)).Modules).ToList<LogicalModuleMount>();
			List<DesignModuleInfo> modules = station.DesignInfo.DesignSections[0].Modules;
			if (station.DesignInfo.StationLevel == 5)
			{
				game.UI.SetPropertyInt(game.UI.Path(panelName, "upgradeProgress"), "value", 100);
				game.UI.SetPropertyColorNormalized(game.UI.Path(panelName, "upgradeProgress.overlay_idle.image"), "color", 0.8f, 0.7f, 0.0f);
			}
			else
			{
				Dictionary<ModuleEnums.StationModuleType, int> requiredModules2 = new Dictionary<ModuleEnums.StationModuleType, int>();
				float stationUpgradeProgress2 = game.GetStationUpgradeProgress(station, out requiredModules2);
				game.UI.SetPropertyInt(game.UI.Path(panelName, "upgradeProgress"), "value", (int)((double)stationUpgradeProgress2 * 100.0));
				game.UI.SetPropertyColorNormalized(game.UI.Path(panelName, "upgradeProgress.overlay_idle.image"), "color", 0.0f, 0.4f, 0.9f);
			}
		}
	}
}
