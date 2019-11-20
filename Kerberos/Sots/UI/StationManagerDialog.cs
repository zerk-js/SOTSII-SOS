// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.StationManagerDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class StationManagerDialog : Dialog
	{
		private static Dictionary<StationType, bool> StationViewFilter = new Dictionary<StationType, bool>()
	{
	  {
		StationType.CIVILIAN,
		true
	  },
	  {
		StationType.DEFENCE,
		true
	  },
	  {
		StationType.DIPLOMATIC,
		true
	  },
	  {
		StationType.GATE,
		true
	  },
	  {
		StationType.MINING,
		true
	  },
	  {
		StationType.NAVAL,
		true
	  },
	  {
		StationType.SCIENCE,
		true
	  }
	};
		private List<SystemWidget> _systemWidgets = new List<SystemWidget>();
		private Dictionary<ModuleEnums.StationModuleType, int> _queuedItemMap = new Dictionary<ModuleEnums.StationModuleType, int>();
		public const string OKButton = "okButton";
		private App App;
		private StarMapState _starmap;
		private StationInfo _selectedStation;
		private StationType _currentFilterMode;
		private int _systemID;

		public StationManagerDialog(App game, StarMapState starmap, int systemid = 0, string template = "dialogStationManager")
		  : base(game, template)
		{
			this._starmap = starmap;
			this.App = game;
			this._systemID = systemid;
		}

		public override void Initialize()
		{
			this.App.UI.UnlockUI();
			this.App.UI.AddItem("filterDropdown", "", 0, App.Localize("@UI_STATION_MANAGER_ALL_STATIONS"));
			this.App.UI.AddItem("filterDropdown", "", 1, App.Localize("@UI_STATION_MANAGER_NAVAL"));
			this.App.UI.AddItem("filterDropdown", "", 2, App.Localize("@UI_STATION_MANAGER_SCIENCE"));
			this.App.UI.AddItem("filterDropdown", "", 3, App.Localize("@UI_STATION_MANAGER_CIVILIAN"));
			this.App.UI.AddItem("filterDropdown", "", 4, App.Localize("@UI_STATION_MANAGER_DIPLOMATIC"));
			this.App.UI.AddItem("filterDropdown", "", 5, App.Localize("@UI_STATION_MANAGER_GATE"));
			this.App.UI.AddItem("filterDropdown", "", 6, App.Localize("@UI_STATION_MANAGER_MINING"));
			this.App.UI.SetSelection("filterDropdown", 0);
			this._currentFilterMode = StationType.INVALID_TYPE;
		}

		public static bool CanOpen(GameSession game, int targetSystemId)
		{
			return game.GameDatabase.GetStationInfosByPlayerID(game.LocalPlayer.ID).Count<StationInfo>() > 0;
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked" && panelName == "okButton")
				this._app.UI.CloseDialog((Dialog)this, true);
			if (msgType == "mouse_enter")
			{
				string[] strArray = panelName.Split('|');
				int orbitalObjectID = int.Parse(strArray[0]);
				ModuleEnums.StationModuleType type = (ModuleEnums.StationModuleType)Enum.Parse(typeof(ModuleEnums.StationModuleType), strArray[1]);
				StationInfo stationInfo = this.App.GameDatabase.GetStationInfo(orbitalObjectID);
				IEnumerable<StationModules.StationModule> source = ((IEnumerable<StationModules.StationModule>)StationModules.Modules).Where<StationModules.StationModule>((Func<StationModules.StationModule, bool>)(x => x.SMType == type));
				if (source.Count<StationModules.StationModule>() > 0)
				{
					string upper = stationInfo.DesignInfo.StationType.ToDisplayText(this.App.GameDatabase.GetFactionName(this.App.GameDatabase.GetPlayerFactionID(stationInfo.PlayerID))).ToUpper();
					this.App.UI.SetPropertyString("moduleDescriptionText", "text", App.Localize(string.Format(source.ElementAt<StationModules.StationModule>(0).Description, (object)upper)));
				}
			}
			else
			{
				int num1 = msgType == "mouse_leave" ? 1 : 0;
			}
			if (msgType == "list_sel_changed")
			{
				if (panelName == "station_list")
					this.PopulateModulesList(this.App.GameDatabase.GetStationInfo(int.Parse(msgParams[0])));
				else if (panelName == "filterDropdown")
				{
					this._currentFilterMode = (StationType)int.Parse(msgParams[0]);
					this.SyncStationList();
				}
			}
			if (!(msgType == "button_clicked"))
				return;
			if (panelName == "upgradeButton")
			{
				OrbitalObjectInfo orbitalObjectInfo = this._app.GameDatabase.GetOrbitalObjectInfo(this._selectedStation.OrbitalObjectID);
				this._starmap.GetUpgradeMissionOverlay().StartSelect = orbitalObjectInfo.ID;
				this._app.UI.CloseDialog((Dialog)this, true);
				this._starmap.ShowUpgradeMissionOverlay(orbitalObjectInfo.StarSystemID);
			}
			else if (panelName.StartsWith("modque"))
			{
				ModuleEnums.StationModuleType moduleID = (ModuleEnums.StationModuleType)int.Parse(panelName.Split('|')[1]);
				List<LogicalModuleMount> stationModuleMounts = this.App.Game.GetAvailableStationModuleMounts(this.App.GameDatabase.GetStationInfo(this._selectedStation.OrbitalObjectID));
				List<DesignModuleInfo> queuedModules = this.App.GameDatabase.GetQueuedStationModules(this._selectedStation.DesignInfo.DesignSections[0]).ToList<DesignModuleInfo>();
				stationModuleMounts.Where<LogicalModuleMount>((Func<LogicalModuleMount, bool>)(x => queuedModules.Any<DesignModuleInfo>((Func<DesignModuleInfo, bool>)(y => y.MountNodeName == x.NodeName)))).ToList<LogicalModuleMount>();
				DesignModuleInfo designModuleInfo = queuedModules.FirstOrDefault<DesignModuleInfo>((Func<DesignModuleInfo, bool>)(x =>
			   {
				   ModuleEnums.StationModuleType? stationModuleType1 = x.StationModuleType;
				   ModuleEnums.StationModuleType stationModuleType2 = moduleID;
				   if (stationModuleType1.GetValueOrDefault() == stationModuleType2)
					   return stationModuleType1.HasValue;
				   return false;
			   }));
				if (designModuleInfo != null)
					this.App.GameDatabase.RemoveQueuedStationModule(designModuleInfo.ID);
				this.SyncModuleItems();
				this.SyncBuildQueue();
			}
			else if (panelName == "filterDiplomatic")
			{
				StationManagerDialog.StationViewFilter[StationType.DIPLOMATIC] = !StationManagerDialog.StationViewFilter[StationType.DIPLOMATIC];
				this.SyncStationList();
			}
			else if (panelName == "filterScience")
			{
				StationManagerDialog.StationViewFilter[StationType.SCIENCE] = !StationManagerDialog.StationViewFilter[StationType.SCIENCE];
				this.SyncStationList();
			}
			else if (panelName == "filterCivilian")
			{
				StationManagerDialog.StationViewFilter[StationType.CIVILIAN] = !StationManagerDialog.StationViewFilter[StationType.CIVILIAN];
				this.SyncStationList();
			}
			else if (panelName == "filterNaval")
			{
				StationManagerDialog.StationViewFilter[StationType.NAVAL] = !StationManagerDialog.StationViewFilter[StationType.NAVAL];
				this.SyncStationList();
			}
			else if (panelName == "filterMining")
			{
				StationManagerDialog.StationViewFilter[StationType.MINING] = !StationManagerDialog.StationViewFilter[StationType.MINING];
				this.SyncStationList();
			}
			else if (panelName == "filterSDS")
			{
				StationManagerDialog.StationViewFilter[StationType.DEFENCE] = !StationManagerDialog.StationViewFilter[StationType.DEFENCE];
				this.SyncStationList();
			}
			else if (panelName == "filterGate")
			{
				StationManagerDialog.StationViewFilter[StationType.GATE] = !StationManagerDialog.StationViewFilter[StationType.GATE];
				this.SyncStationList();
			}
			else if (panelName == "confirmOrderButton")
			{
				StationInfo stationInfo = this.App.GameDatabase.GetStationInfo(this._selectedStation.OrbitalObjectID);
				StationModuleQueue.UpdateStationMapsForFaction(this.App.LocalPlayer.Faction.Name);
				StationModuleQueue.ConfirmStationQueuedItems(this.App.Game, stationInfo, this._queuedItemMap);
				this.SyncModuleItems();
				this.SyncBuildQueue();
			}
			else if (panelName.EndsWith("module_up"))
			{
				string[] strArray = panelName.Split('|');
				int orbitalObjectID = int.Parse(strArray[0]);
				ModuleEnums.StationModuleType type = (ModuleEnums.StationModuleType)Enum.Parse(typeof(ModuleEnums.StationModuleType), strArray[1]);
				StationInfo stationInfo = this.App.GameDatabase.GetStationInfo(orbitalObjectID);
				this.App.GameDatabase.GetModuleID(this.App.AssetDatabase.GetStationModuleAsset(type, this.App.Game.LocalPlayer.Faction.Name), this.App.Game.LocalPlayer.ID);
				List<LogicalModuleMount> stationModuleMounts = this.App.Game.GetAvailableStationModuleMounts(stationInfo);
				List<DesignModuleInfo> list = this.App.GameDatabase.GetQueuedStationModules(this._selectedStation.DesignInfo.DesignSections[0]).ToList<DesignModuleInfo>();
				list.Where<DesignModuleInfo>((Func<DesignModuleInfo, bool>)(x =>
			   {
				   ModuleEnums.StationModuleType? stationModuleType1 = x.StationModuleType;
				   ModuleEnums.StationModuleType stationModuleType2 = type;
				   if (stationModuleType1.GetValueOrDefault() == stationModuleType2)
					   return stationModuleType1.HasValue;
				   return false;
			   })).ToList<DesignModuleInfo>();
				int num2 = list.Where<DesignModuleInfo>((Func<DesignModuleInfo, bool>)(x => AssetDatabase.StationModuleTypeToMountTypeMap[x.StationModuleType.Value] == AssetDatabase.StationModuleTypeToMountTypeMap[type])).Count<DesignModuleInfo>();
				if (this._queuedItemMap.Where<KeyValuePair<ModuleEnums.StationModuleType, int>>((Func<KeyValuePair<ModuleEnums.StationModuleType, int>, bool>)(x => AssetDatabase.StationModuleTypeToMountTypeMap[x.Key] == AssetDatabase.StationModuleTypeToMountTypeMap[type])).Sum<KeyValuePair<ModuleEnums.StationModuleType, int>>((Func<KeyValuePair<ModuleEnums.StationModuleType, int>, int>)(x => x.Value)) >= stationModuleMounts.Where<LogicalModuleMount>((Func<LogicalModuleMount, bool>)(x => x.ModuleType == AssetDatabase.StationModuleTypeToMountTypeMap[type].ToString())).Count<LogicalModuleMount>() - num2)
					return;
				Dictionary<ModuleEnums.StationModuleType, int> queuedItemMap;
				ModuleEnums.StationModuleType index;
				(queuedItemMap = this._queuedItemMap)[index = type] = queuedItemMap[index] + 1;
				this.SyncModuleItems();
			}
			else if (panelName.EndsWith("module_down"))
			{
				string[] strArray = panelName.Split('|');
				int.Parse(strArray[0]);
				ModuleEnums.StationModuleType index1 = (ModuleEnums.StationModuleType)Enum.Parse(typeof(ModuleEnums.StationModuleType), strArray[1]);
				if (this._queuedItemMap[index1] <= 0)
					return;
				Dictionary<ModuleEnums.StationModuleType, int> queuedItemMap;
				ModuleEnums.StationModuleType index2;
				(queuedItemMap = this._queuedItemMap)[index2 = index1] = queuedItemMap[index2] - 1;
				this.SyncModuleItems();
			}
			else if (panelName.EndsWith("module_max"))
			{
				string[] strArray = panelName.Split('|');
				int orbitalObjectID = int.Parse(strArray[0]);
				ModuleEnums.StationModuleType type = (ModuleEnums.StationModuleType)Enum.Parse(typeof(ModuleEnums.StationModuleType), strArray[1]);
				StationInfo stationInfo = this.App.GameDatabase.GetStationInfo(orbitalObjectID);
				this.App.GameDatabase.GetModuleID(this.App.AssetDatabase.GetStationModuleAsset(type, this.App.Game.LocalPlayer.Faction.Name), this.App.Game.LocalPlayer.ID);
				List<LogicalModuleMount> stationModuleMounts = this.App.Game.GetAvailableStationModuleMounts(stationInfo);
				List<DesignModuleInfo> list = this.App.GameDatabase.GetQueuedStationModules(this._selectedStation.DesignInfo.DesignSections[0]).ToList<DesignModuleInfo>();
				list.Where<DesignModuleInfo>((Func<DesignModuleInfo, bool>)(x =>
			   {
				   ModuleEnums.StationModuleType? stationModuleType1 = x.StationModuleType;
				   ModuleEnums.StationModuleType stationModuleType2 = type;
				   if (stationModuleType1.GetValueOrDefault() == stationModuleType2)
					   return stationModuleType1.HasValue;
				   return false;
			   })).ToList<DesignModuleInfo>();
				int num2 = list.Where<DesignModuleInfo>((Func<DesignModuleInfo, bool>)(x => AssetDatabase.StationModuleTypeToMountTypeMap[x.StationModuleType.Value] == AssetDatabase.StationModuleTypeToMountTypeMap[type])).Count<DesignModuleInfo>();
				int num3 = stationModuleMounts.Where<LogicalModuleMount>((Func<LogicalModuleMount, bool>)(x => x.ModuleType == AssetDatabase.StationModuleTypeToMountTypeMap[type].ToString())).Count<LogicalModuleMount>();
				int num4 = this._queuedItemMap.Where<KeyValuePair<ModuleEnums.StationModuleType, int>>((Func<KeyValuePair<ModuleEnums.StationModuleType, int>, bool>)(x => AssetDatabase.StationModuleTypeToMountTypeMap[x.Key] == AssetDatabase.StationModuleTypeToMountTypeMap[type])).Sum<KeyValuePair<ModuleEnums.StationModuleType, int>>((Func<KeyValuePair<ModuleEnums.StationModuleType, int>, int>)(x => x.Value));
				if (num4 >= num3 - num2)
					return;
				Dictionary<ModuleEnums.StationModuleType, int> queuedItemMap;
				ModuleEnums.StationModuleType index;
				(queuedItemMap = this._queuedItemMap)[index = type] = queuedItemMap[index] + (num3 - num2 - num4);
				this.SyncModuleItems();
			}
			else
			{
				if (!(panelName == "autoUpgradeButton"))
					return;
				this.AutoFillModules();
			}
		}

		public override string[] CloseDialog()
		{
			foreach (SystemWidget systemWidget in this._systemWidgets)
				systemWidget.Terminate();
			return (string[])null;
		}

		protected void SyncStationList()
		{
			this.App.UI.ClearItems("station_list");
			this.App.UI.ClearItems("stationModules");
			this.App.UI.ClearItems("moduleQue");
			this.App.UI.SetText("queueCost", "");
			this.App.UI.SetText("turnsToComplete", "");
			this.App.UI.SetPropertyString("moduleDescriptionText", "text", "");
			List<StationInfo> source = this._systemID == 0 ? this.App.GameDatabase.GetStationInfosByPlayerID(this.App.Game.LocalPlayer.ID).ToList<StationInfo>() : this.App.GameDatabase.GetStationForSystemAndPlayer(this._systemID, this.App.Game.LocalPlayer.ID).ToList<StationInfo>();
			foreach (StationInfo stationInfo in new List<StationInfo>((IEnumerable<StationInfo>)source))
			{
				if (!StationManagerDialog.StationViewFilter[stationInfo.DesignInfo.StationType])
					source.Remove(stationInfo);
			}
			foreach (SystemWidget systemWidget in this._systemWidgets)
				systemWidget.Terminate();
			this._systemWidgets.Clear();
			List<int> intList = new List<int>();
			source.RemoveAll((Predicate<StationInfo>)(x => this.App.GameDatabase.GetOrbitalObjectInfo(x.OrbitalObjectID) == null));
			foreach (StationInfo stationInfo in source.OrderBy<StationInfo, string>((Func<StationInfo, string>)(x => this.App.GameDatabase.GetStarSystemInfo(this.App.GameDatabase.GetOrbitalObjectInfo(x.OrbitalObjectID).StarSystemID).Name)).ToList<StationInfo>())
			{
				if (stationInfo.DesignInfo.StationLevel > 0 && (stationInfo.DesignInfo.StationType == this._currentFilterMode || this._currentFilterMode == StationType.INVALID_TYPE))
				{
					int starSystemId = this.App.GameDatabase.GetOrbitalObjectInfo(stationInfo.OrbitalObjectID).StarSystemID;
					if (!intList.Contains(starSystemId))
					{
						StarSystemInfo starSystemInfo = this.App.GameDatabase.GetStarSystemInfo(starSystemId);
						if (!(starSystemInfo == (StarSystemInfo)null) && !starSystemInfo.IsDeepSpace)
						{
							this.App.UI.AddItem("station_list", "", starSystemId + 999999, "", "systemTitleCard");
							string itemGlobalId = this.App.UI.GetItemGlobalID("station_list", "", starSystemId + 999999, "");
							intList.Add(starSystemId);
							this._systemWidgets.Add(new SystemWidget(this.App, itemGlobalId));
							this._systemWidgets.Last<SystemWidget>().Sync(starSystemId);
						}
						else
							continue;
					}
					this.App.UI.AddItem("station_list", string.Empty, stationInfo.OrbitalObjectID, string.Empty, "navalStation_DetailsCard");
					this._selectedStation = stationInfo;
					this.SyncStationProgress();
					StationUI.SyncStationDetailsWidget(this.App.Game, this.App.UI.GetItemGlobalID("station_list", string.Empty, stationInfo.OrbitalObjectID, string.Empty), stationInfo.OrbitalObjectID, true);
				}
			}
		}

		protected override void OnUpdate()
		{
			foreach (SystemWidget systemWidget in this._systemWidgets)
				systemWidget.Update();
		}

		protected void SyncModuleItems()
		{
			List<LogicalModuleMount> list1 = ((IEnumerable<LogicalModuleMount>)this.App.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == this._selectedStation.DesignInfo.DesignSections[0].FilePath)).Modules).ToList<LogicalModuleMount>();
			List<DesignModuleInfo> builtModules = this._selectedStation.DesignInfo.DesignSections[0].Modules;
			List<DesignModuleInfo> queuedModules = this.App.GameDatabase.GetQueuedStationModules(this._selectedStation.DesignInfo.DesignSections[0]).ToList<DesignModuleInfo>();
			StationInfo stationInfo = this.App.GameDatabase.GetStationInfo(this._selectedStation.OrbitalObjectID);
			Dictionary<ModuleEnums.StationModuleType, int> requiredModules = new Dictionary<ModuleEnums.StationModuleType, int>();
			double stationUpgradeProgress = (double)this.App.Game.GetStationUpgradeProgress(stationInfo, out requiredModules);
			int num1 = 0;
			int num2 = 0;
			for (int index = 0; index < 70; ++index)
			{
				ModuleEnums.StationModuleType type = (ModuleEnums.StationModuleType)index;
				if (this._queuedItemMap.ContainsKey(type))
				{
					List<StationModules.StationModule> matchingModules = ((IEnumerable<StationModules.StationModule>)StationModules.Modules).Where<StationModules.StationModule>((Func<StationModules.StationModule, bool>)(val => val.SMType == type)).ToList<StationModules.StationModule>();
					if (matchingModules.Count > 0)
					{
						List<LogicalModuleMount> list2 = list1.Where<LogicalModuleMount>((Func<LogicalModuleMount, bool>)(x => x.ModuleType == matchingModules[0].SlotType)).ToList<LogicalModuleMount>();
						if (list2.Count > 0)
						{
							num1 += list2.Count<LogicalModuleMount>();
							List<LogicalModuleMount> list3 = list2.Where<LogicalModuleMount>((Func<LogicalModuleMount, bool>)(x => builtModules.Any<DesignModuleInfo>((Func<DesignModuleInfo, bool>)(y => y.MountNodeName == x.NodeName)))).ToList<LogicalModuleMount>();
							List<LogicalModuleMount> list4 = list2.Where<LogicalModuleMount>((Func<LogicalModuleMount, bool>)(x => queuedModules.Any<DesignModuleInfo>((Func<DesignModuleInfo, bool>)(y => y.MountNodeName == x.NodeName)))).ToList<LogicalModuleMount>();
							List<DesignModuleInfo> list5 = builtModules.Where<DesignModuleInfo>((Func<DesignModuleInfo, bool>)(x =>
						   {
							   ModuleEnums.StationModuleType? stationModuleType1 = x.StationModuleType;
							   ModuleEnums.StationModuleType stationModuleType2 = type;
							   if (stationModuleType1.GetValueOrDefault() == stationModuleType2)
								   return stationModuleType1.HasValue;
							   return false;
						   })).ToList<DesignModuleInfo>();
							List<DesignModuleInfo> list6 = queuedModules.Where<DesignModuleInfo>((Func<DesignModuleInfo, bool>)(x =>
						   {
							   ModuleEnums.StationModuleType? stationModuleType1 = x.StationModuleType;
							   ModuleEnums.StationModuleType stationModuleType2 = type;
							   if (stationModuleType1.GetValueOrDefault() == stationModuleType2)
								   return stationModuleType1.HasValue;
							   return false;
						   })).ToList<DesignModuleInfo>();
							int num3 = this._queuedItemMap.Where<KeyValuePair<ModuleEnums.StationModuleType, int>>((Func<KeyValuePair<ModuleEnums.StationModuleType, int>, bool>)(x =>
						   {
							   if (AssetDatabase.StationModuleTypeToMountTypeMap[x.Key] == AssetDatabase.StationModuleTypeToMountTypeMap[type])
								   return x.Key != type;
							   return false;
						   })).Sum<KeyValuePair<ModuleEnums.StationModuleType, int>>((Func<KeyValuePair<ModuleEnums.StationModuleType, int>, int>)(x => x.Value));
							int count1 = list5.Count;
							num2 += count1;
							int count2 = list6.Count;
							int num4 = list2.Count - list3.Count - list4.Count - num3;
							if (count2 + count1 + num4 >= 0)
							{
								string propertyValue1 = count2.ToString();
								if (this._queuedItemMap[type] > 0)
								{
									string propertyValue2 = propertyValue1 + "~0,255,0,255|+" + this._queuedItemMap[type].ToString() + "~";
									this.App.UI.SetPropertyString(this.App.UI.Path("module" + ((ModuleEnums.StationModuleType)index).ToString(), "module_que_plus"), "text", propertyValue2);
									this.App.UI.SetVisible(this.App.UI.Path("module" + ((ModuleEnums.StationModuleType)index).ToString(), "module_que_plus"), true);
									this.App.UI.SetVisible(this.App.UI.Path("module" + ((ModuleEnums.StationModuleType)index).ToString(), "module_que"), false);
								}
								else
								{
									this.App.UI.SetPropertyString(this.App.UI.Path("module" + ((ModuleEnums.StationModuleType)index).ToString(), "module_que"), "text", propertyValue1);
									this.App.UI.SetVisible(this.App.UI.Path("module" + ((ModuleEnums.StationModuleType)index).ToString(), "module_que_plus"), false);
									this.App.UI.SetVisible(this.App.UI.Path("module" + ((ModuleEnums.StationModuleType)index).ToString(), "module_que"), true);
								}
								this.App.UI.SetPropertyString(this.App.UI.Path("module" + ((ModuleEnums.StationModuleType)index).ToString(), "module_built"), "text", string.Format("{0}/{1}", (object)count1, (object)(count2 + count1 + num4)));
								string propertyValue3 = "";
								if (count2 + count1 + num4 > 0)
								{
									List<KeyValuePair<ModuleEnums.StationModuleType, int>> list7 = requiredModules.Where<KeyValuePair<ModuleEnums.StationModuleType, int>>((Func<KeyValuePair<ModuleEnums.StationModuleType, int>, bool>)(x => x.Key.ToString() == AssetDatabase.StationModuleTypeToMountTypeMap[type].ToString())).ToList<KeyValuePair<ModuleEnums.StationModuleType, int>>();
									if (list7.Count<KeyValuePair<ModuleEnums.StationModuleType, int>>() > 0)
										propertyValue3 = string.Format("{0} req.", (object)list7.ElementAt<KeyValuePair<ModuleEnums.StationModuleType, int>>(0).Value);
								}
								this.App.UI.SetPropertyString(this.App.UI.Path("module" + ((ModuleEnums.StationModuleType)index).ToString(), "module_req"), "text", propertyValue3);
							}
						}
					}
				}
			}
			this.SyncBuildQueue();
		}

		private void AutoFillModules()
		{
			StationModuleQueue.UpdateStationMapsForFaction(this.App.LocalPlayer.Faction.Name);
			StationModuleQueue.AutoFillModules(this.App.Game, this._selectedStation, this._queuedItemMap);
			if (this._queuedItemMap.Count <= 0)
				return;
			this.SyncModuleItems();
		}

		protected void SyncStationProgress()
		{
			string itemGlobalId = this.App.UI.GetItemGlobalID("station_list", "", this.App.GameDatabase.GetStationInfo(this._selectedStation.OrbitalObjectID).OrbitalObjectID, "");
			bool flag = Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(this.App.Game, this.App.LocalPlayer.ID, this.App.GameDatabase.GetOrbitalObjectInfo(this._selectedStation.OrbitalObjectID).StarSystemID, MissionType.CONSTRUCT_STN, true).Any<FleetInfo>();
			this.App.UI.SetEnabled(this.App.UI.Path(itemGlobalId, "upgradeButton"), (flag ? 1 : 0) != 0);
			this.App.UI.SetVisible(this.App.UI.Path(itemGlobalId, "upgradeIndicator"), false);
			this.App.UI.SetVisible(this.App.UI.Path(itemGlobalId, "upgradeButton"), false);
			this.App.UI.SetPropertyBool(this.App.UI.Path(itemGlobalId, "upgradeButton"), "lockout_button", true);
			if (this.App.Game.StationIsUpgrading(this._selectedStation))
			{
				this.App.UI.SetVisible(this.App.UI.Path(itemGlobalId, "upgradeIndicator"), true);
				this.App.UI.SetVisible(this.App.UI.Path(itemGlobalId, "upgradeButton"), false);
			}
			else
			{
				if (!this.App.Game.StationIsUpgradable(this._selectedStation))
					return;
				this.App.UI.SetVisible(this.App.UI.Path(itemGlobalId, "upgradeButton"), true);
				this.App.UI.SetVisible(this.App.UI.Path(itemGlobalId, "upgradingIndicator"), false);
			}
		}

		protected void SyncBuildQueue()
		{
			this.App.UI.ClearItems("moduleQue");
			StationInfo si = this.App.GameDatabase.GetStationInfo(this._selectedStation.OrbitalObjectID);
			List<DesignModuleInfo> list = this.App.GameDatabase.GetQueuedStationModules(si.DesignInfo.DesignSections[0]).ToList<DesignModuleInfo>();
			int num = 0;
			foreach (DesignModuleInfo designModuleInfo in list)
			{
				DesignModuleInfo module = designModuleInfo;
				this.App.UI.AddItem("moduleQue", "", module.ID, "");
				string itemGlobalId = this.App.UI.GetItemGlobalID("moduleQue", "", module.ID, "");
				StationModules.StationModule stationModule = ((IEnumerable<StationModules.StationModule>)StationModules.Modules).Where<StationModules.StationModule>((Func<StationModules.StationModule, bool>)(x =>
			  {
				  ModuleEnums.StationModuleType smType = x.SMType;
				  ModuleEnums.StationModuleType? stationModuleType = module.StationModuleType;
				  if (smType == stationModuleType.GetValueOrDefault())
					  return stationModuleType.HasValue;
				  return false;
			  })).First<StationModules.StationModule>();
				StationModuleQueue.UpdateStationMapsForFaction(this.App.LocalPlayer.Faction.Name);
				LogicalModule logicalModule = this.App.AssetDatabase.Modules.First<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == this.App.AssetDatabase.GetStationModuleAsset(module.StationModuleType.Value, StationModuleQueue.GetModuleFactionDefault(module.StationModuleType.Value, this.App.Game.GetPlayerObject(si.PlayerID).Faction))));
				num += logicalModule.SavingsCost;
				this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "moduleName"), "text", stationModule.Name + " - $" + logicalModule.SavingsCost.ToString("N0"));
				this.App.UI.SetVisible(this.App.UI.Path(itemGlobalId, "deleteButton"), true);
				this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "deleteButton"), "id", "modque|" + ((int)module.StationModuleType.Value).ToString());
				this.App.UI.SetPropertyColor(this.App.UI.Path(itemGlobalId, "moduleName"), "color", new Vector3((float)byte.MaxValue, (float)byte.MaxValue, (float)byte.MaxValue));
			}
			int userItemId = 999000;
			foreach (KeyValuePair<ModuleEnums.StationModuleType, int> keyValuePair in this._queuedItemMap.ToList<KeyValuePair<ModuleEnums.StationModuleType, int>>().Where<KeyValuePair<ModuleEnums.StationModuleType, int>>((Func<KeyValuePair<ModuleEnums.StationModuleType, int>, bool>)(x => x.Value > 0)))
			{
				KeyValuePair<ModuleEnums.StationModuleType, int> thing = keyValuePair;
				for (int index = 0; index < thing.Value; ++index)
				{
					this.App.UI.AddItem("moduleQue", "", userItemId, "");
					string itemGlobalId = this.App.UI.GetItemGlobalID("moduleQue", "", userItemId, "");
					StationModules.StationModule stationModule = ((IEnumerable<StationModules.StationModule>)StationModules.Modules).Where<StationModules.StationModule>((Func<StationModules.StationModule, bool>)(x => x.SMType == thing.Key)).First<StationModules.StationModule>();
					StationModuleQueue.UpdateStationMapsForFaction(this.App.LocalPlayer.Faction.Name);
					LogicalModule logicalModule = this.App.AssetDatabase.Modules.First<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == this.App.AssetDatabase.GetStationModuleAsset(thing.Key, StationModuleQueue.GetModuleFactionDefault(thing.Key, this.App.Game.GetPlayerObject(si.PlayerID).Faction))));
					num += logicalModule.SavingsCost;
					this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "moduleName"), "text", stationModule.Name + " - $" + logicalModule.SavingsCost.ToString("N0"));
					this.App.UI.SetPropertyColor(this.App.UI.Path(itemGlobalId, "moduleName"), "color", new Vector3((float)byte.MaxValue, 200f, 50f));
					this.App.UI.SetVisible(this.App.UI.Path(itemGlobalId, "deleteButton"), false);
					++userItemId;
				}
			}
			this.App.UI.SetText("queueCost", "$" + num.ToString("N0"));
			this.App.UI.SetText("turnsToComplete", list.Count.ToString() + " " + App.Localize("@UI_GENERAL_TURNS"));
		}

		protected void PopulateModulesList(StationInfo station)
		{
			this.App.UI.SetPropertyString("moduleDescriptionText", "text", "");
			this._selectedStation = station;
			StationModuleQueue.UpdateStationMapsForFaction(this.App.LocalPlayer.Faction.Name);
			List<LogicalModuleMount> stationModuleMounts = this.App.Game.GetStationModuleMounts(station);
			this.App.UI.ClearItems("stationModules");
			StationModuleQueue.UpdateStationMapsForFaction(this.App.LocalPlayer.Faction.Name);
			StationModuleQueue.InitializeQueuedItemMap(this.App.Game, station, this._queuedItemMap);
			int userItemId = 0;
			foreach (StationModules.StationModule uniqueStationModule in StationModuleQueue.EnumerateUniqueStationModules(this.App.Game, station))
			{
				this.App.UI.AddItem("stationModules", "", userItemId, "");
				string itemGlobalId = this.App.UI.GetItemGlobalID("stationModules", "", userItemId, "");
				this.App.UI.SetPropertyString(itemGlobalId, "id", "module" + (object)uniqueStationModule.SMType);
				this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "huverbuttin"), "id", station.OrbitalObjectID.ToString() + "|" + (object)uniqueStationModule.SMType);
				this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "module_label"), "text", uniqueStationModule.Name);
				this.App.UI.SetVisible(this.App.UI.Path(itemGlobalId, "module_up"), true);
				this.App.UI.SetVisible(this.App.UI.Path(itemGlobalId, "module_down"), true);
				this.App.UI.SetVisible(this.App.UI.Path(itemGlobalId, "module_max"), true);
				this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "module_up"), "id", station.OrbitalObjectID.ToString() + "|" + uniqueStationModule.SMType.ToString() + "|module_up");
				this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "module_down"), "id", station.OrbitalObjectID.ToString() + "|" + uniqueStationModule.SMType.ToString() + "|module_down");
				this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "module_max"), "id", station.OrbitalObjectID.ToString() + "|" + uniqueStationModule.SMType.ToString() + "|module_max");
				++userItemId;
			}
			if (stationModuleMounts.FirstOrDefault<LogicalModuleMount>((Func<LogicalModuleMount, bool>)(x => x.ModuleType == "AlienHabitation")) != null && this._queuedItemMap.Where<KeyValuePair<ModuleEnums.StationModuleType, int>>((Func<KeyValuePair<ModuleEnums.StationModuleType, int>, bool>)(x => AssetDatabase.StationModuleTypeToMountTypeMap[x.Key].ToString() == "AlienHabitation")).Count<KeyValuePair<ModuleEnums.StationModuleType, int>>() == 0)
				this.AddBlankModule(userItemId++, station, ModuleEnums.StationModuleType.AlienHabitation);
			if (stationModuleMounts.FirstOrDefault<LogicalModuleMount>((Func<LogicalModuleMount, bool>)(x => x.ModuleType == "LargeAlienHabitation")) != null && this._queuedItemMap.Where<KeyValuePair<ModuleEnums.StationModuleType, int>>((Func<KeyValuePair<ModuleEnums.StationModuleType, int>, bool>)(x => AssetDatabase.StationModuleTypeToMountTypeMap[x.Key].ToString() == "LargeAlienHabitation")).Count<KeyValuePair<ModuleEnums.StationModuleType, int>>() == 0)
			{
				int cur = userItemId;
				int num = cur + 1;
				this.AddBlankModule(cur, station, ModuleEnums.StationModuleType.LargeAlienHabitation);
			}
			this.SyncModuleItems();
			this.SyncBuildQueue();
		}

		public void AddBlankModule(int cur, StationInfo station, ModuleEnums.StationModuleType type)
		{
			this.App.UI.AddItem("stationModules", "", cur, "");
			string itemGlobalId = this.App.UI.GetItemGlobalID("stationModules", "", cur, "");
			this.App.UI.SetPropertyString(itemGlobalId, "id", "nullfun");
			this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "huverbuttin"), "id", station.OrbitalObjectID.ToString() + "|" + (object)type);
			if (type == ModuleEnums.StationModuleType.AlienHabitation)
				this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "module_label"), "text", App.Localize("@UI_STATIONDETAILS_ALIENHAB"));
			else if (type == ModuleEnums.StationModuleType.LargeAlienHabitation)
				this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "module_label"), "text", App.Localize("@UI_STATIONDETAILS_LGALIENHAB"));
			this.App.UI.SetVisible(this.App.UI.Path(itemGlobalId, "module_que_plus"), false);
			this.App.UI.SetVisible(this.App.UI.Path(itemGlobalId, "module_que"), false);
			this.App.UI.SetVisible(this.App.UI.Path(itemGlobalId, "module_up"), false);
			this.App.UI.SetVisible(this.App.UI.Path(itemGlobalId, "module_down"), false);
			this.App.UI.SetVisible(this.App.UI.Path(itemGlobalId, "module_max"), false);
			this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "module_built"), "text", "");
			Dictionary<ModuleEnums.StationModuleType, int> requiredModules = new Dictionary<ModuleEnums.StationModuleType, int>();
			double stationUpgradeProgress = (double)this.App.Game.GetStationUpgradeProgress(station, out requiredModules);
			string propertyValue = "";
			List<KeyValuePair<ModuleEnums.StationModuleType, int>> list = requiredModules.Where<KeyValuePair<ModuleEnums.StationModuleType, int>>((Func<KeyValuePair<ModuleEnums.StationModuleType, int>, bool>)(x => x.Key.ToString() == type.ToString())).ToList<KeyValuePair<ModuleEnums.StationModuleType, int>>();
			if (list.Count<KeyValuePair<ModuleEnums.StationModuleType, int>>() > 0)
				propertyValue = string.Format("{0} req.", (object)list.ElementAt<KeyValuePair<ModuleEnums.StationModuleType, int>>(0).Value);
			this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "module_req"), "text", propertyValue);
		}
	}
}
