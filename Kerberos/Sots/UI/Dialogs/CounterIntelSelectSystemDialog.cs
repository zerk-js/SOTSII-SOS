// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.Dialogs.CounterIntelSelectSystemDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI.Dialogs
{
	internal class CounterIntelSelectSystemDialog : Dialog
	{
		private readonly string LeftContentList = "CIleftContent";
		private readonly string RightContentList = "CIrightContent";
		private readonly string SystemButton = "systembtn";
		private readonly string ToggleButton = "systemtoggle";
		private readonly string HeaderText = "headertxt";
		private readonly string DirectionTest = "dirtext";
		private List<SystemWidget> _systemWidgets = new List<SystemWidget>();
		private int intelMissionID;
		private readonly GameSession _game;
		private IntelMissionInfo missioninfo;
		private List<StarSystemInfo> SyncedSystems;
		private List<PlanetWidget> _planetWidgets;
		private Dictionary<int, string> _systemcardpanels;
		private Dictionary<int, bool> _checkedsystems;
		private Dictionary<int, string> SystemUnselectedPanels;
		private Dictionary<int, string> SystemSelectedPanels;
		private Dictionary<int, string> SystemContentSelectedPanels;

		public CounterIntelSelectSystemDialog(GameSession game, int intel_mission_id)
		  : base(game.App, "counterIntel_Standard")
		{
			this.intelMissionID = intel_mission_id;
			this.missioninfo = game.GameDatabase.GetIntelInfo(this.intelMissionID);
			this._game = game;
		}

		public override void Initialize()
		{
			this._systemcardpanels = new Dictionary<int, string>();
			this._checkedsystems = new Dictionary<int, bool>();
			this._planetWidgets = new List<PlanetWidget>();
			this.SystemUnselectedPanels = new Dictionary<int, string>();
			this.SystemSelectedPanels = new Dictionary<int, string>();
			this.SystemContentSelectedPanels = new Dictionary<int, string>();
			this._app.UI.SetEnabled(this.UI.Path(this.ID, "okbtn"), false);
			this.PopulateSystemList(true);
			this._game.UI.SetText(this.UI.Path(this.ID, this.HeaderText), App.Localize("@UI_COUNTER_INTEL_" + this.missioninfo.MissionType.ToString().ToUpper()));
			this._game.UI.SetText(this.UI.Path(this.ID, this.DirectionTest), App.Localize("@UI_COUNTER_INTEL_" + this.missioninfo.MissionType.ToString().ToUpper() + "_DIR"));
		}

		private void PopulateSystemList(bool SelectFirst = false)
		{
			List<StarSystemInfo> list = this._game.GameDatabase.GetVisibleStarSystemInfos(this._app.LocalPlayer.ID).Where<StarSystemInfo>((Func<StarSystemInfo, bool>)(x => this._game.GameDatabase.IsSurveyed(this._game.LocalPlayer.ID, x.ID))).ToList<StarSystemInfo>();
			this.SyncedSystems = list;
			foreach (StarSystemInfo starSystemInfo in list)
			{
				this._game.UI.AddItem(this.LeftContentList, "", starSystemInfo.ID, "", "TinySystemCard_Toggle");
				string itemGlobalId = this._game.UI.GetItemGlobalID(this.LeftContentList, "", starSystemInfo.ID, "");
				this._systemcardpanels.Add(starSystemInfo.ID, itemGlobalId);
				this._checkedsystems.Add(starSystemInfo.ID, false);
				this._game.UI.SetPropertyString(this._game.UI.Path(itemGlobalId, this.SystemButton), "id", this.SystemButton + "|" + (object)starSystemInfo.ID);
				this._game.UI.SetPropertyString(this._game.UI.Path(itemGlobalId, this.ToggleButton), "id", this.ToggleButton + "|" + (object)starSystemInfo.ID);
				string globalId1 = this._game.UI.GetGlobalID(this._game.UI.Path(itemGlobalId, "unselected"));
				string globalId2 = this._game.UI.GetGlobalID(this._game.UI.Path(itemGlobalId, "selected"));
				this._game.UI.GetGlobalID(this._game.UI.Path(itemGlobalId, "contentselected"));
				this.SystemSelectedPanels.Add(starSystemInfo.ID, globalId1);
				this.SystemUnselectedPanels.Add(starSystemInfo.ID, globalId2);
				this.SystemContentSelectedPanels.Add(starSystemInfo.ID, globalId2);
				this._game.UI.SetText(this._game.UI.Path(itemGlobalId, "itemName"), starSystemInfo.Name);
				Vector4 vector4 = StarHelper.CalcModelColor(new StellarClass(starSystemInfo.StellarClass));
				this._game.UI.SetPropertyColor(this._game.UI.Path(itemGlobalId, "colorGradient"), "color", new Vector3(vector4.X, vector4.Y, vector4.Z) * (float)byte.MaxValue);
			}
			if (!SelectFirst || !list.Any<StarSystemInfo>())
				return;
			this.SelectSystem(list.First<StarSystemInfo>().ID);
		}

		private void SelectSystem(int systemid)
		{
			this.SetSyncedSystem(this._game.GameDatabase.GetStarSystemInfo(systemid));
		}

		protected void SetSyncedSystem(StarSystemInfo system)
		{
			this._game.UI.ClearItems(this.RightContentList);
			foreach (SystemWidget systemWidget in this._systemWidgets)
				systemWidget.Terminate();
			this._systemWidgets.Clear();
			foreach (PlanetWidget planetWidget in this._planetWidgets)
				planetWidget.Terminate();
			this._planetWidgets.Clear();
			this._game.UI.ClearItems(this.RightContentList);
			List<PlanetInfo> list = ((IEnumerable<PlanetInfo>)this._game.GameDatabase.GetStarSystemPlanetInfos(system.ID)).ToList<PlanetInfo>();
			this._game.UI.AddItem(this.RightContentList, "", system.ID, "", "systemTitleCard");
			this._systemWidgets.Add(new SystemWidget(this._game.App, this._game.UI.GetItemGlobalID(this.RightContentList, "", system.ID, "")));
			this._systemWidgets.Last<SystemWidget>().Sync(system.ID);
			foreach (PlanetInfo planetInfo in list)
			{
				if (this._game.AssetDatabase.IsPotentialyHabitable(planetInfo.Type))
				{
					this._game.UI.AddItem(this.RightContentList, "", planetInfo.ID + 999999, "", "planetDetailsM_Card");
					this._planetWidgets.Add(new PlanetWidget(this._game.App, this._game.UI.GetItemGlobalID(this.RightContentList, "", planetInfo.ID + 999999, "")));
					this._planetWidgets.Last<PlanetWidget>().Sync(planetInfo.ID, false, false);
				}
				else if (this._game.AssetDatabase.IsGasGiant(planetInfo.Type))
				{
					this._game.UI.AddItem("system_list", "", planetInfo.ID + 999999, "", "gasgiantDetailsM_Card");
					this._planetWidgets.Add(new PlanetWidget(this._game.App, this._game.UI.GetItemGlobalID("system_list", "", planetInfo.ID + 999999, "")));
					this._planetWidgets.Last<PlanetWidget>().Sync(planetInfo.ID, false, false);
				}
				else if (this._game.AssetDatabase.IsMoon(planetInfo.Type))
				{
					this._game.UI.AddItem(this.RightContentList, "", planetInfo.ID + 999999, "", "moonDetailsM_Card");
					this._planetWidgets.Add(new PlanetWidget(this._game.App, this._game.UI.GetItemGlobalID(this.RightContentList, "", planetInfo.ID + 999999, "")));
					this._planetWidgets.Last<PlanetWidget>().Sync(planetInfo.ID, false, false);
				}
			}
		}

		private void SetChecked(int systemid, bool forcechecked = false)
		{
			foreach (StarSystemInfo syncedSystem in this.SyncedSystems)
			{
				if (this._checkedsystems[syncedSystem.ID] || forcechecked)
					this._game.UI.SetChecked(this._game.UI.Path(this._systemcardpanels[syncedSystem.ID], this.ToggleButton + "|" + (object)syncedSystem.ID), (syncedSystem.ID == systemid ? 1 : 0) != 0);
				this._checkedsystems[syncedSystem.ID] = syncedSystem.ID == systemid;
			}
			this._app.UI.SetEnabled(this.UI.Path(this.ID, "okbtn"), (this._checkedsystems.Any<KeyValuePair<int, bool>>((Func<KeyValuePair<int, bool>, bool>)(x => x.Value)) ? 1 : 0) != 0);
		}

		private void AutoSelect()
		{
			this.SetChecked(this.SyncedSystems[new Random().Next(0, this.SyncedSystems.Count)].ID, true);
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName.StartsWith(this.SystemButton))
					this.SelectSystem(int.Parse(panelName.Split('|')[1]));
				else if (panelName.StartsWith(this.ToggleButton))
					this.SetChecked(int.Parse(panelName.Split('|')[1]), false);
				else if (panelName == "autoselectbtn")
					this.AutoSelect();
				else if (panelName == "okbtn")
				{
					this.DeployCounterIntel();
					this._game.UI.CloseDialog((Dialog)this, true);
				}
			}
			int num = msgType == "list_sel_changed" ? 1 : 0;
		}

		private void DeployCounterIntel()
		{
			foreach (CounterIntelResponse counterIntelResponse in this._game.GameDatabase.GetCounterIntelResponses(this.intelMissionID).ToList<CounterIntelResponse>())
				this._app.GameDatabase.RemoveCounterIntelResponse(counterIntelResponse.ID);
			this._app.GameDatabase.InsertCounterIntelResponse(this.intelMissionID, false, this._checkedsystems.First<KeyValuePair<int, bool>>((Func<KeyValuePair<int, bool>, bool>)(x => x.Value)).Key.ToString());
		}

		protected override void OnUpdate()
		{
			foreach (PlanetWidget planetWidget in this._planetWidgets)
				planetWidget.Update();
			foreach (SystemWidget systemWidget in this._systemWidgets)
				systemWidget.Update();
		}

		public override string[] CloseDialog()
		{
			foreach (PlanetWidget planetWidget in this._planetWidgets)
				planetWidget.Terminate();
			foreach (SystemWidget systemWidget in this._systemWidgets)
				systemWidget.Terminate();
			return (string[])null;
		}
	}
}
