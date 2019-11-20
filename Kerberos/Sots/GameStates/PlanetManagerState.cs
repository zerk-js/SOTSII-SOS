// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.PlanetManagerState
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.UI;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.GameStates
{
	internal class PlanetManagerState : GameState
	{
		private static readonly string UIExitButton = "gameExitButton";
		private Dictionary<ModuleEnums.StationModuleType, int> _queuedItemMap = new Dictionary<ModuleEnums.StationModuleType, int>();
		private List<SystemWidget> _systemWidgets = new List<SystemWidget>();
		private List<PlanetWidget> _planetWidgets;
		private int _targetSystem;
		private PlanetManagerState.PlanetFilterMode _currentFilterMode;

		public PlanetManagerState(App app)
		  : base(app)
		{
		}

		public static bool CanOpen(GameSession game, int targetSystemId)
		{
			return game.GameDatabase.GetStationInfosByPlayerID(game.LocalPlayer.ID).Count<StationInfo>() > 0;
		}

		protected override void OnPrepare(GameState prev, object[] parms)
		{
			this.App.UI.LoadScreen("PlanetManager");
			if (this.App.GameDatabase == null)
			{
				this.App.NewGame();
				this._targetSystem = this.App.GameDatabase.GetPlayerHomeworld(this.App.LocalPlayer.ID).SystemID;
			}
			else if (((IEnumerable<object>)parms).Count<object>() > 0)
				this._targetSystem = (int)parms[0];
			else
				this._targetSystem = this.App.GameDatabase.GetPlayerHomeworld(this.App.LocalPlayer.ID).SystemID;
		}

		protected override void OnEnter()
		{
			this.App.UI.UnlockUI();
			this.App.UI.SetScreen("PlanetManager");
			this.App.UI.AddItem("filterDropdown", "", 0, App.Localize("@UI_PLANET_MANAGER_ALL_PLANETS"));
			this.App.UI.AddItem("filterDropdown", "", 1, App.Localize("@UI_PLANET_MANAGER_SURVEYED_PLANETS"));
			this.App.UI.AddItem("filterDropdown", "", 2, App.Localize("@UI_PLANET_MANAGER_OWNED_PLANETS"));
			this.App.UI.AddItem("filterDropdown", "", 3, App.Localize("@UI_PLANET_MANAGER_ENEMY_PLANETS"));
			this.App.UI.SetSelection("filterDropdown", 0);
			this._currentFilterMode = PlanetManagerState.PlanetFilterMode.AllPlanets;
			this.App.UI.SetPropertyBool("gameExitButton", "lockout_button", true);
			EmpireBarUI.SyncTitleFrame(this.App);
			this._planetWidgets = new List<PlanetWidget>();
			this.SyncPlanetList();
		}

		protected void SetSyncedSystem(StarSystemInfo system)
		{
			this.App.UI.ClearItems("system_list");
			foreach (SystemWidget systemWidget in this._systemWidgets)
				systemWidget.Terminate();
			foreach (PlanetWidget planetWidget in this._planetWidgets)
				planetWidget.Terminate();
			this._planetWidgets.Clear();
			List<PlanetInfo> planetInfoList = this.FilteredPlanetList(system);
			this.App.UI.AddItem("system_list", "", system.ID, "", "systemTitleCard");
			this._systemWidgets.Add(new SystemWidget(this.App, this.App.UI.GetItemGlobalID("system_list", "", system.ID, "")));
			this._systemWidgets.Last<SystemWidget>().Sync(system.ID);
			foreach (PlanetInfo planetInfo in planetInfoList)
			{
				if (this.App.AssetDatabase.IsPotentialyHabitable(planetInfo.Type))
				{
					this.App.UI.AddItem("system_list", "", planetInfo.ID + 999999, "", "planetDetailsM_Card");
					this._planetWidgets.Add(new PlanetWidget(this.App, this.App.UI.GetItemGlobalID("system_list", "", planetInfo.ID + 999999, "")));
					this._planetWidgets.Last<PlanetWidget>().Sync(planetInfo.ID, false, false);
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
			List<PlanetInfo> list = this.App.GameDatabase.GetPlanetInfosOrbitingStar(system.ID).ToList<PlanetInfo>();
			List<PlanetInfo> planetInfoList = new List<PlanetInfo>();
			foreach (PlanetInfo planetInfo in list)
			{
				if (this.App.GameDatabase.IsSurveyed(this.App.LocalPlayer.ID, system.ID))
				{
					if (this._currentFilterMode == PlanetManagerState.PlanetFilterMode.AllPlanets)
						planetInfoList.Add(planetInfo);
					else if (this._currentFilterMode == PlanetManagerState.PlanetFilterMode.SurveyedPlanets)
					{
						if (this.App.GameDatabase.GetColonyIntelForPlanet(this.App.LocalPlayer.ID, planetInfo.ID) == null)
							planetInfoList.Add(planetInfo);
					}
					else if (this._currentFilterMode == PlanetManagerState.PlanetFilterMode.OwnedPlanets)
					{
						ColonyInfo colonyInfoForPlanet = this.App.GameDatabase.GetColonyInfoForPlanet(planetInfo.ID);
						if (colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == this.App.LocalPlayer.ID)
							planetInfoList.Add(planetInfo);
					}
					else if (this._currentFilterMode == PlanetManagerState.PlanetFilterMode.EnemyPlanets)
					{
						AIColonyIntel colonyIntelForPlanet = this.App.GameDatabase.GetColonyIntelForPlanet(this.App.LocalPlayer.ID, planetInfo.ID);
						if (colonyIntelForPlanet != null && colonyIntelForPlanet.OwningPlayerID != this.App.LocalPlayer.ID)
							planetInfoList.Add(planetInfo);
					}
				}
			}
			return planetInfoList;
		}

		protected void SyncPlanetList()
		{
			this.App.UI.ClearItems("sys_list_left");
			List<StarSystemInfo> list = this.App.GameDatabase.GetStarSystemInfos().ToList<StarSystemInfo>();
			bool flag = true;
			foreach (StarSystemInfo system in list)
			{
				if (this.FilteredPlanetList(system).Count > 0)
				{
					this.App.UI.AddItem("sys_list_left", "", system.ID, system.Name);
					if (flag)
					{
						this.SetSyncedSystem(system);
						flag = false;
					}
				}
			}
		}

		protected override void OnExit(GameState next, ExitReason reason)
		{
			foreach (PlanetWidget planetWidget in this._planetWidgets)
				planetWidget.Terminate();
			foreach (SystemWidget systemWidget in this._systemWidgets)
				systemWidget.Terminate();
		}

		protected override void OnUpdate()
		{
			foreach (PlanetWidget planetWidget in this._planetWidgets)
				planetWidget.Update();
			foreach (SystemWidget systemWidget in this._systemWidgets)
				systemWidget.Update();
		}

		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}

		protected override void UICommChannel_OnPanelMessage(
		  string panelName,
		  string msgType,
		  string[] msgParams)
		{
			if (msgType == "list_sel_changed")
			{
				if (panelName == "filterDropdown")
				{
					this._currentFilterMode = (PlanetManagerState.PlanetFilterMode)int.Parse(msgParams[0]);
					this.SyncPlanetList();
				}
				else if (panelName == "sys_list_left")
				{
					StarSystemInfo starSystemInfo = this.App.GameDatabase.GetStarSystemInfo(int.Parse(msgParams[0]));
					if (starSystemInfo != (StarSystemInfo)null)
						this.SetSyncedSystem(starSystemInfo);
				}
			}
			if (!(msgType == "button_clicked") || !(panelName == PlanetManagerState.UIExitButton))
				return;
			this.App.SwitchGameState<StarMapState>();
		}

		private enum PlanetFilterMode
		{
			AllPlanets,
			SurveyedPlanets,
			OwnedPlanets,
			EnemyPlanets,
		}
	}
}
