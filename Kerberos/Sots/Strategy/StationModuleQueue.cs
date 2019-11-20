// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.StationModuleQueue
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Data.TechnologyFramework;
using Kerberos.Sots.Framework;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Strategy
{
	internal class StationModuleQueue
	{
		public static string GetModuleFactionDefault(
		  ModuleEnums.StationModuleType type,
		  Faction defaultFaction)
		{
			string str = AssetDatabase.GetModuleFactionName(type);
			if (str.Length == 0)
				str = defaultFaction.Name;
			return str;
		}

		internal static IEnumerable<StationModules.StationModule> EnumerateUniqueStationModules(
		  GameSession game,
		  StationInfo station)
		{
			HashSet<ModuleEnums.StationModuleType> consideredTypes = new HashSet<ModuleEnums.StationModuleType>();
			foreach (LogicalModuleMount stationModuleMount in game.GetStationModuleMounts(station))
			{
				LogicalModuleMount mount = stationModuleMount;
				List<StationModules.StationModule> matchingModules = ((IEnumerable<StationModules.StationModule>)StationModules.Modules).Where<StationModules.StationModule>((Func<StationModules.StationModule, bool>)(val => val.SlotType == mount.ModuleType)).ToList<StationModules.StationModule>();
				Player stationPlayer = game.GetPlayerObject(station.DesignInfo.PlayerID);
				foreach (StationModules.StationModule stationModule in matchingModules)
				{
					if (!consideredTypes.Contains(stationModule.SMType) && stationModule.SMType != ModuleEnums.StationModuleType.AlienHabitation && stationModule.SMType != ModuleEnums.StationModuleType.LargeAlienHabitation)
					{
						consideredTypes.Add(stationModule.SMType);
						string faction = AssetDatabase.GetModuleFactionName(stationModule.SMType);
						if (faction.Length > 0)
						{
							if (!(faction == "zuul") || !stationModule.SlotType.ToString().ToUpper().Contains("ALIEN"))
							{
								IEnumerable<PlayerInfo> list = (IEnumerable<PlayerInfo>)game.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>();
								if (list.Where<PlayerInfo>((Func<PlayerInfo, bool>)(x => game.GameDatabase.GetFactionName(x.FactionID) == faction)).Count<PlayerInfo>() > 0)
								{
									bool flag = false;
									foreach (PlayerInfo playerInfo in list.Where<PlayerInfo>((Func<PlayerInfo, bool>)(x =>
								   {
									   if (game.GameDatabase.GetFactionName(x.FactionID) == faction)
										   return x.ID != stationPlayer.ID;
									   return false;
								   })))
									{
										flag = game.GameDatabase.GetDiplomacyInfo(stationPlayer.ID, playerInfo.ID).isEncountered;
										if (flag)
											break;
									}
									if (faction != stationPlayer.Faction.Name && !flag || faction != stationPlayer.Faction.Name && flag && stationModule.SMType.ToString().ToUpper().Contains("FOREIGN") || faction == stationPlayer.Faction.Name && !flag && stationModule.SMType.ToString().ToUpper().Contains("FOREIGN"))
										continue;
								}
								else
									continue;
							}
							else
								continue;
						}
						yield return stationModule;
					}
				}
			}
		}

		internal static void InitializeQueuedItemMap(
		  GameSession game,
		  StationInfo station,
		  Dictionary<ModuleEnums.StationModuleType, int> queuedItemMap)
		{
			queuedItemMap.Clear();
			foreach (StationModules.StationModule uniqueStationModule in StationModuleQueue.EnumerateUniqueStationModules(game, station))
				queuedItemMap.Add(uniqueStationModule.SMType, 0);
		}

		internal static void AutoFillModules(GameSession game, StationInfo station, Dictionary<ModuleEnums.StationModuleType, int> queuedItemMap)
		{
			if (queuedItemMap.Count > 0)
			{
				ShipSectionAsset shipSectionAsset = game.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == station.DesignInfo.DesignSections[0].FilePath);
				shipSectionAsset.Modules.ToList<LogicalModuleMount>();
				List<DesignModuleInfo> modules = station.DesignInfo.DesignSections[0].Modules;
				List<DesignModuleInfo> source = game.GameDatabase.GetQueuedStationModules(station.DesignInfo.DesignSections[0]).ToList<DesignModuleInfo>();
				StationInfo stationInfo = game.GameDatabase.GetStationInfo(station.OrbitalObjectID);
				StationModuleQueue.ModuleRequirements source2 = new StationModuleQueue.ModuleRequirements(game, stationInfo, queuedItemMap);
				Player playerObject = game.GetPlayerObject(stationInfo.PlayerID);
				foreach (KeyValuePair<ModuleEnums.StationModuleType, KeyValuePair<IEnumerable<ModuleEnums.StationModuleType>, int>> keyValuePair in from x in source2
																																					where x.Value.Value > 0
																																					select x)
				{
					KeyValuePair<IEnumerable<ModuleEnums.StationModuleType>, int> req = keyValuePair.Value;
					int i = req.Value - source.Where((DesignModuleInfo x) => req.Key.Any((ModuleEnums.StationModuleType y) => y == x.StationModuleType.Value)).Count<DesignModuleInfo>() - queuedItemMap.Where((KeyValuePair<ModuleEnums.StationModuleType, int> x) => req.Key.Any((ModuleEnums.StationModuleType y) => y == x.Key)).Sum((KeyValuePair<ModuleEnums.StationModuleType, int> x) => x.Value);
					while (i > 0)
					{
						int num = i;
						List<ModuleEnums.StationModuleType> list = req.Key.ToList<ModuleEnums.StationModuleType>();
						if (list.Count == 0)
						{
							break;
						}
						list.Shuffle<ModuleEnums.StationModuleType>();
						if (keyValuePair.Key == ModuleEnums.StationModuleType.Lab)
						{
							int playerResearchingTechID = game.GameDatabase.GetPlayerResearchingTechID(playerObject.ID);
							if (playerResearchingTechID != 0)
							{
								string stringTechId = game.GameDatabase.GetTechFileID(playerResearchingTechID);
								Tech tech = game.AssetDatabase.MasterTechTree.Technologies.First((Tech x) => x.Id == stringTechId);
								ModuleEnums.StationModuleType item;
								if (list.ExistsFirst((ModuleEnums.StationModuleType x) => x.ToString().Contains(tech.Family), out item))
								{
									list.Remove(item);
									list.Insert(0, item);
								}
							}
						}
						using (List<ModuleEnums.StationModuleType>.Enumerator enumerator2 = list.GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								ModuleEnums.StationModuleType moduleType = enumerator2.Current;
								int num2 = req.Value - source.Where((DesignModuleInfo x) => moduleType == x.StationModuleType.Value).Count<DesignModuleInfo>() - queuedItemMap.Where((KeyValuePair<ModuleEnums.StationModuleType, int> x) => moduleType == x.Key).Sum((KeyValuePair<ModuleEnums.StationModuleType, int> x) => x.Value);
								if (num2 > 0)
								{
									ModuleEnums.StationModuleType moduleType2;
									queuedItemMap[moduleType2 = moduleType] = queuedItemMap[moduleType2] + 1;
									i--;
									if (i <= 0)
									{
										break;
									}
								}
							}
						}
						if (i >= num)
						{
							break;
						}
					}
				}
			}
		}
		internal static void ConfirmStationQueuedItems(
	  GameSession game,
	  StationInfo station,
	  Dictionary<ModuleEnums.StationModuleType, int> queuedItemMap)
		{
			List<LogicalModuleMount> stationModuleMounts = game.GetAvailableStationModuleMounts(station);
			foreach (KeyValuePair<ModuleEnums.StationModuleType, int> keyValuePair in queuedItemMap.ToList<KeyValuePair<ModuleEnums.StationModuleType, int>>())
			{
				KeyValuePair<ModuleEnums.StationModuleType, int> thing = keyValuePair;
				int num = thing.Value;
				while (num > 0)
				{
					List<DesignModuleInfo> list1 = game.GameDatabase.GetQueuedStationModules(station.DesignInfo.DesignSections[0]).ToList<DesignModuleInfo>();
					List<DesignModuleInfo> modules = station.DesignInfo.DesignSections[0].Modules;
					List<LogicalModuleMount> list2 = stationModuleMounts.Where<LogicalModuleMount>((Func<LogicalModuleMount, bool>)(x => x.ModuleType == AssetDatabase.StationModuleTypeToMountTypeMap[thing.Key].ToString())).ToList<LogicalModuleMount>();
					bool flag = false;
					foreach (LogicalModuleMount logicalModuleMount in list2)
					{
						LogicalModuleMount mount = logicalModuleMount;
						if (list1.Where<DesignModuleInfo>((Func<DesignModuleInfo, bool>)(x => x.MountNodeName == mount.NodeName)).Count<DesignModuleInfo>() == 0 && modules.Where<DesignModuleInfo>((Func<DesignModuleInfo, bool>)(x => x.MountNodeName == mount.NodeName)).Count<DesignModuleInfo>() == 0)
						{
							List<PlayerInfo> list3 = game.GameDatabase.GetPlayerInfos().ToList<PlayerInfo>();
							Player playerObject = game.GetPlayerObject(station.PlayerID);
							string moduleFactionDefault = StationModuleQueue.GetModuleFactionDefault(thing.Key, playerObject.Faction);
							int moduleId = game.GameDatabase.GetModuleID(game.AssetDatabase.GetStationModuleAsset(thing.Key, moduleFactionDefault), list3.First<PlayerInfo>((Func<PlayerInfo, bool>)(x => game.GameDatabase.GetFactionName(x.FactionID) == moduleFactionDefault)).ID);
							game.GameDatabase.InsertQueuedStationModule(station.DesignInfo.DesignSections[0].ID, moduleId, new int?(), mount.NodeName, thing.Key);
							--num;
							flag = true;
							break;
						}
					}
					if (!flag)
						break;
				}
				queuedItemMap[thing.Key] = 0;
			}
		}

		internal static void UpdateStationMapsForFaction(string faction)
		{
			foreach (StationModules.StationModule module in StationModules.Modules)
			{
				if (module.SMType == ModuleEnums.StationModuleType.HumanHabitation)
					module.SlotType = faction == "human" ? "Habitation" : "AlienHabitation";
				if (module.SMType == ModuleEnums.StationModuleType.HiverHabitation)
					module.SlotType = faction == "hiver" ? "Habitation" : "AlienHabitation";
				if (module.SMType == ModuleEnums.StationModuleType.TarkasHabitation)
					module.SlotType = faction == "tarkas" ? "Habitation" : "AlienHabitation";
				if (module.SMType == ModuleEnums.StationModuleType.MorrigiHabitation)
					module.SlotType = faction == "morrigi" ? "Habitation" : "AlienHabitation";
				if (module.SMType == ModuleEnums.StationModuleType.LiirHabitation)
					module.SlotType = faction == "liir_zuul" ? "Habitation" : "AlienHabitation";
				if (module.SMType == ModuleEnums.StationModuleType.ZuulHabitation)
					module.SlotType = faction == "zuul" ? "Habitation" : "AlienHabitation";
				if (module.SMType == ModuleEnums.StationModuleType.LoaHabitation)
					module.SlotType = faction == "loa" ? "Habitation" : "AlienHabitation";
				if (module.SMType == ModuleEnums.StationModuleType.HumanLargeHabitation)
					module.SlotType = faction == "human" ? "LargeHabitation" : "LargeAlienHabitation";
				if (module.SMType == ModuleEnums.StationModuleType.HiverLargeHabitation)
					module.SlotType = faction == "hiver" ? "LargeHabitation" : "LargeAlienHabitation";
				if (module.SMType == ModuleEnums.StationModuleType.TarkasLargeHabitation)
					module.SlotType = faction == "tarkas" ? "LargeHabitation" : "LargeAlienHabitation";
				if (module.SMType == ModuleEnums.StationModuleType.MorrigiLargeHabitation)
					module.SlotType = faction == "morrigi" ? "LargeHabitation" : "LargeAlienHabitation";
				if (module.SMType == ModuleEnums.StationModuleType.LiirLargeHabitation)
					module.SlotType = faction == "liir_zuul" ? "LargeHabitation" : "LargeAlienHabitation";
				if (module.SMType == ModuleEnums.StationModuleType.ZuulLargeHabitation)
					module.SlotType = faction == "zuul" ? "LargeHabitation" : "LargeAlienHabitation";
				if (module.SMType == ModuleEnums.StationModuleType.LoaLargeHabitation)
					module.SlotType = faction == "loa" ? "LargeHabitation" : "LargeAlienHabitation";
			}
			AssetDatabase.StationModuleTypeToMountTypeMap[ModuleEnums.StationModuleType.HumanHabitation] = faction == "human" ? ModuleEnums.ModuleSlotTypes.Habitation : ModuleEnums.ModuleSlotTypes.AlienHabitation;
			AssetDatabase.StationModuleTypeToMountTypeMap[ModuleEnums.StationModuleType.HiverHabitation] = faction == "hiver" ? ModuleEnums.ModuleSlotTypes.Habitation : ModuleEnums.ModuleSlotTypes.AlienHabitation;
			AssetDatabase.StationModuleTypeToMountTypeMap[ModuleEnums.StationModuleType.TarkasHabitation] = faction == "tarkas" ? ModuleEnums.ModuleSlotTypes.Habitation : ModuleEnums.ModuleSlotTypes.AlienHabitation;
			AssetDatabase.StationModuleTypeToMountTypeMap[ModuleEnums.StationModuleType.MorrigiHabitation] = faction == "morrigi" ? ModuleEnums.ModuleSlotTypes.Habitation : ModuleEnums.ModuleSlotTypes.AlienHabitation;
			AssetDatabase.StationModuleTypeToMountTypeMap[ModuleEnums.StationModuleType.LiirHabitation] = faction == "liir_zuul" ? ModuleEnums.ModuleSlotTypes.Habitation : ModuleEnums.ModuleSlotTypes.AlienHabitation;
			AssetDatabase.StationModuleTypeToMountTypeMap[ModuleEnums.StationModuleType.ZuulHabitation] = faction == "zuul" ? ModuleEnums.ModuleSlotTypes.Habitation : ModuleEnums.ModuleSlotTypes.AlienHabitation;
			AssetDatabase.StationModuleTypeToMountTypeMap[ModuleEnums.StationModuleType.LoaHabitation] = faction == "loa" ? ModuleEnums.ModuleSlotTypes.Habitation : ModuleEnums.ModuleSlotTypes.AlienHabitation;
			AssetDatabase.StationModuleTypeToMountTypeMap[ModuleEnums.StationModuleType.HumanLargeHabitation] = faction == "human" ? ModuleEnums.ModuleSlotTypes.LargeHabitation : ModuleEnums.ModuleSlotTypes.LargeAlienHabitation;
			AssetDatabase.StationModuleTypeToMountTypeMap[ModuleEnums.StationModuleType.HiverLargeHabitation] = faction == "hiver" ? ModuleEnums.ModuleSlotTypes.LargeHabitation : ModuleEnums.ModuleSlotTypes.LargeAlienHabitation;
			AssetDatabase.StationModuleTypeToMountTypeMap[ModuleEnums.StationModuleType.TarkasLargeHabitation] = faction == "tarkas" ? ModuleEnums.ModuleSlotTypes.LargeHabitation : ModuleEnums.ModuleSlotTypes.LargeAlienHabitation;
			AssetDatabase.StationModuleTypeToMountTypeMap[ModuleEnums.StationModuleType.MorrigiLargeHabitation] = faction == "morrigi" ? ModuleEnums.ModuleSlotTypes.LargeHabitation : ModuleEnums.ModuleSlotTypes.LargeAlienHabitation;
			AssetDatabase.StationModuleTypeToMountTypeMap[ModuleEnums.StationModuleType.LiirLargeHabitation] = faction == "liir_zuul" ? ModuleEnums.ModuleSlotTypes.LargeHabitation : ModuleEnums.ModuleSlotTypes.LargeAlienHabitation;
			AssetDatabase.StationModuleTypeToMountTypeMap[ModuleEnums.StationModuleType.ZuulLargeHabitation] = faction == "zuul" ? ModuleEnums.ModuleSlotTypes.LargeHabitation : ModuleEnums.ModuleSlotTypes.LargeAlienHabitation;
			AssetDatabase.StationModuleTypeToMountTypeMap[ModuleEnums.StationModuleType.LoaLargeHabitation] = faction == "loa" ? ModuleEnums.ModuleSlotTypes.LargeHabitation : ModuleEnums.ModuleSlotTypes.LargeAlienHabitation;
		}

		private class ModuleRequirements : Dictionary<ModuleEnums.StationModuleType, KeyValuePair<IEnumerable<ModuleEnums.StationModuleType>, int>>
		{
			public ModuleRequirements(
			  GameSession game,
			  StationInfo station,
			  Dictionary<ModuleEnums.StationModuleType, int> queuedItemMap)
			{
				Dictionary<ModuleEnums.StationModuleType, int> requiredModules = new Dictionary<ModuleEnums.StationModuleType, int>();
				double stationUpgradeProgress = (double)game.GetStationUpgradeProgress(station, out requiredModules);
				string name = game.GetPlayerObject(station.PlayerID).Faction.Name;
				foreach (KeyValuePair<ModuleEnums.StationModuleType, int> keyValuePair in requiredModules)
					this.Add(keyValuePair.Key, new KeyValuePair<IEnumerable<ModuleEnums.StationModuleType>, int>(AssetDatabase.ResolveSpecificStationModuleTypes(name, keyValuePair.Key).Where<ModuleEnums.StationModuleType>((Func<ModuleEnums.StationModuleType, bool>)(x => queuedItemMap.ContainsKey(x))), keyValuePair.Value));
			}
		}
	}
}
