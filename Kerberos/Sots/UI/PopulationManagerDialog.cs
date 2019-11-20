// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.PopulationManagerDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.GameStates;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class PopulationManagerDialog : Dialog
	{
		private static readonly string UIExitButton = "okButton";
		private Dictionary<ModuleEnums.StationModuleType, int> _queuedItemMap = new Dictionary<ModuleEnums.StationModuleType, int>();
		private List<SystemWidget> _systemWidgets = new List<SystemWidget>();
		private List<PlanetWidget> _planetWidgets;
		private App App;
		private int initialsystemid;

		public PopulationManagerDialog(App game, int systemid = 0, string template = "dialogPopulationManager")
		  : base(game, template)
		{
			this.App = game;
			this.initialsystemid = systemid;
		}

		public override void Initialize()
		{
			this.App.UI.UnlockUI();
			this.App.UI.SetListCleanClear("system_list", true);
			EmpireBarUI.SyncTitleFrame(this.App);
			this._planetWidgets = new List<PlanetWidget>();
			this.SyncPlanetList();
		}

		protected override void OnUpdate()
		{
			foreach (PlanetWidget planetWidget in this._planetWidgets)
				planetWidget.Update();
			foreach (SystemWidget systemWidget in this._systemWidgets)
				systemWidget.Update();
		}

		protected void SetSyncedSystem(StarSystemInfo system)
		{
			if (this._app.CurrentState.Name == "StarMapState")
			{
				StarMapState currentState = (StarMapState)this._app.CurrentState;
				currentState.StarMap.SetFocus((IGameObject)currentState.StarMap.Systems.Reverse[system.ID]);
				currentState.StarMap.Select((IGameObject)currentState.StarMap.Systems.Reverse[system.ID]);
			}
			this.App.UI.ClearItems("system_list");
			foreach (SystemWidget systemWidget in this._systemWidgets)
				systemWidget.Terminate();
			this._systemWidgets.Clear();
			foreach (PlanetWidget planetWidget in this._planetWidgets)
				planetWidget.Terminate();
			this._planetWidgets.Clear();
			this.App.UI.ClearItems("system_list");
			List<PlanetInfo> planetInfoList = this.FilteredPlanetList(system);
			this.App.UI.AddItem("system_list", "", system.ID, "", "systemTitleCard");
			this._systemWidgets.Add(new SystemWidget(this.App, this.App.UI.GetItemGlobalID("system_list", "", system.ID, "")));
			this._systemWidgets.Last<SystemWidget>().Sync(system.ID);
			foreach (PlanetInfo planetInfo in planetInfoList)
			{
				if (this.App.AssetDatabase.IsPotentialyHabitable(planetInfo.Type))
				{
					this.App.UI.AddItem("system_list", "", planetInfo.ID + 999999, "", "planetDetailsPop_Card");
					this._planetWidgets.Add(new PlanetWidget(this.App, this.App.UI.GetItemGlobalID("system_list", "", planetInfo.ID + 999999, "")));
					this._planetWidgets.Last<PlanetWidget>().Sync(planetInfo.ID, true, false);
				}
				else if (this.App.AssetDatabase.IsGasGiant(planetInfo.Type))
				{
					this.App.UI.AddItem("system_list", "", planetInfo.ID + 999999, "", "gasgiantDetailsM_Card");
					this._planetWidgets.Add(new PlanetWidget(this.App, this.App.UI.GetItemGlobalID("system_list", "", planetInfo.ID + 999999, "")));
					this._planetWidgets.Last<PlanetWidget>().Sync(planetInfo.ID, false, false);
				}
				else if (this.App.AssetDatabase.IsMoon(planetInfo.Type))
				{
					this.App.UI.AddItem("system_list", "", planetInfo.ID + 999999, "", "moonDetailsM_Card");
					this._planetWidgets.Add(new PlanetWidget(this.App, this.App.UI.GetItemGlobalID("system_list", "", planetInfo.ID + 999999, "")));
					this._planetWidgets.Last<PlanetWidget>().Sync(planetInfo.ID, false, false);
				}
			}
		}

		private List<PlanetInfo> FilteredPlanetList(StarSystemInfo system)
		{
			List<PlanetInfo> list = ((IEnumerable<PlanetInfo>)this.App.GameDatabase.GetStarSystemPlanetInfos(system.ID)).ToList<PlanetInfo>();
			List<PlanetInfo> planetInfoList = new List<PlanetInfo>();
			foreach (PlanetInfo planetInfo in list)
			{
				if (this.App.GameDatabase.IsSurveyed(this.App.LocalPlayer.ID, system.ID))
				{
					AIColonyIntel colonyIntelForPlanet = this.App.GameDatabase.GetColonyIntelForPlanet(this.App.LocalPlayer.ID, planetInfo.ID);
					if (colonyIntelForPlanet != null && colonyIntelForPlanet.OwningPlayerID == this.App.LocalPlayer.ID)
						planetInfoList.Add(planetInfo);
				}
			}
			return planetInfoList;
		}

		protected void SyncPlanetList()
		{
			this.App.UI.ClearItems("sys_list_left");
			List<StarSystemInfo> list = this.App.GameDatabase.GetStarSystemInfos().ToList<StarSystemInfo>();
			list.Sort((Comparison<StarSystemInfo>)((x, y) => string.Compare(x.Name, y.Name)));
			bool flag = true;
			foreach (StarSystemInfo system in list)
			{
				if (this.FilteredPlanetList(system).Count > 0)
				{
					this.App.UI.AddItem("sys_list_left", "", system.ID, system.Name);
					if (system.ID == this.initialsystemid || flag && this.initialsystemid == 0)
					{
						this.SetSyncedSystem(system);
						this.App.UI.SetSelection("sys_list_left", system.ID);
						flag = false;
					}
				}
			}
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "list_sel_changed" && panelName == "sys_list_left")
			{
				StarSystemInfo starSystemInfo = this.App.GameDatabase.GetStarSystemInfo(int.Parse(msgParams[0]));
				if (starSystemInfo != (StarSystemInfo)null)
					this.SetSyncedSystem(starSystemInfo);
			}
			if (!(msgType == "button_clicked") || !(panelName == PopulationManagerDialog.UIExitButton))
				return;
			this.App.UI.CloseDialog((Dialog)this, true);
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
