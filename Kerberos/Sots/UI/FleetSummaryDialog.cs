// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.FleetSummaryDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class FleetSummaryDialog : Dialog
	{
		private Dictionary<ModuleEnums.StationModuleType, int> _queuedItemMap = new Dictionary<ModuleEnums.StationModuleType, int>();
		private Dictionary<ShipRole, bool> _interestedRoles = new Dictionary<ShipRole, bool>();
		public const string OKButton = "okButton";
		private App App;
		private FleetWidget _mainWidget;
		private List<FleetInfo> _fleets;
		private FleetSummaryDialog.FilterFleets _currentFilterMode;

		public FleetSummaryDialog(App game, string template = "FleetSummaryDialog")
		  : base(game, template)
		{
			this.App = game;
		}

		public FleetSummaryDialog.FilterFleets CurrentFilterMode
		{
			get
			{
				return this._currentFilterMode;
			}
			set
			{
				if (this._currentFilterMode == value)
					return;
				this._currentFilterMode = value;
				this.RefreshFleets();
			}
		}

		private void RefreshFleets()
		{
			if (this._fleets == null)
				this._fleets = this._app.GameDatabase.GetFleetInfosByPlayerID(this.App.LocalPlayer.ID, FleetType.FL_NORMAL | FleetType.FL_RESERVE).ToList<FleetInfo>();
			List<FleetInfo> list = this._fleets.Where<FleetInfo>((Func<FleetInfo, bool>)(x => this.IsKeeperFleet(x))).ToList<FleetInfo>();
			switch (this._currentFilterMode)
			{
				case FleetSummaryDialog.FilterFleets.NormalFleet:
					list = list.Where<FleetInfo>((Func<FleetInfo, bool>)(x => x.Type == FleetType.FL_NORMAL)).ToList<FleetInfo>();
					break;
				case FleetSummaryDialog.FilterFleets.Name:
					list = list.OrderBy<FleetInfo, string>((Func<FleetInfo, string>)(x => x.Name)).ToList<FleetInfo>();
					break;
				case FleetSummaryDialog.FilterFleets.Admiral:
					list = list.OrderBy<FleetInfo, string>((Func<FleetInfo, string>)(x =>
				   {
					   if (x.AdmiralID == 0)
						   return "";
					   return this._app.GameDatabase.GetAdmiralInfo(x.AdmiralID).Name;
				   })).ToList<FleetInfo>();
					break;
				case FleetSummaryDialog.FilterFleets.BaseSystem:
					list = list.OrderBy<FleetInfo, string>((Func<FleetInfo, string>)(x =>
				   {
					   if (x.SupportingSystemID == 0)
						   return "";
					   return this._app.GameDatabase.GetStarSystemInfo(x.SupportingSystemID).Name;
				   })).ToList<FleetInfo>();
					break;
				case FleetSummaryDialog.FilterFleets.UnitCount:
					list = list.OrderByDescending<FleetInfo, int>((Func<FleetInfo, int>)(x => this._app.GameDatabase.GetShipsByFleetID(x.ID).Count<int>())).ToList<FleetInfo>();
					break;
				case FleetSummaryDialog.FilterFleets.Mission:
					list = list.Where<FleetInfo>((Func<FleetInfo, bool>)(x => x.Type == FleetType.FL_NORMAL)).OrderByDescending<FleetInfo, bool>((Func<FleetInfo, bool>)(x => this._app.GameDatabase.GetMissionByFleetID(x.ID) != null)).ToList<FleetInfo>();
					break;
				case FleetSummaryDialog.FilterFleets.NoMission:
					list = list.Where<FleetInfo>((Func<FleetInfo, bool>)(x =>
				   {
					   if (x.Type == FleetType.FL_NORMAL)
						   return this._app.GameDatabase.GetMissionByFleetID(x.ID) == null;
					   return false;
				   })).ToList<FleetInfo>();
					break;
			}
			this._mainWidget.SetSyncedFleets(list);
		}

		private bool IsKeeperFleet(FleetInfo fleet)
		{
			List<ShipInfo> list = this.App.GameDatabase.GetShipInfoByFleetID(fleet.ID, true).ToList<ShipInfo>();
			if (list.Count == 0)
				return false;
			if (!this._interestedRoles.ContainsValue(true))
				return true;
			foreach (ShipInfo shipInfo in list)
			{
				bool flag;
				if (!this._interestedRoles.TryGetValue(shipInfo.DesignInfo.Role, out flag))
					flag = false;
				if (flag)
					return true;
			}
			return false;
		}

		public override void Initialize()
		{
			this.App.UI.UnlockUI();
			this._mainWidget = new FleetWidget(this.App, this.UI.Path(this.ID, "fleetList"));
			this._mainWidget.JumboMode = true;
			this.App.UI.AddItem("filterDropdown", "", 0, App.Localize("@UI_GENERAL_ALL"));
			this.App.UI.AddItem("filterDropdown", "", 1, App.Localize("@UI_GENERAL_NORMAL_FLEETS"));
			this.App.UI.AddItem("filterDropdown", "", 2, App.Localize("@UI_GENERAL_NAME"));
			this.App.UI.AddItem("filterDropdown", "", 3, App.Localize("@UI_GENERAL_ADMIRAL"));
			this.App.UI.AddItem("filterDropdown", "", 4, App.Localize("@UI_GENERAL_BASE_SYSTEM"));
			this.App.UI.AddItem("filterDropdown", "", 5, App.Localize("@UI_GENERAL_UNITCOUNT"));
			this.App.UI.AddItem("filterDropdown", "", 6, App.Localize("@UI_GENERAL_MISSION"));
			this.App.UI.AddItem("filterDropdown", "", 7, App.Localize("@UI_MISSIONFLEET_NO_MISSION"));
			this.App.UI.SetSelection("filterDropdown", 0);
			this._currentFilterMode = FleetSummaryDialog.FilterFleets.All;
			this._mainWidget.OnFleetSelectionChanged = new FleetWidget.FleetSelectionChangedDelegate(FleetSummaryDialog.FleetSelectionChanged);
			this.RefreshFleets();
		}

		public static void FleetSelectionChanged(App App, int fleetid)
		{
			StarMapState currentState = (StarMapState)App.CurrentState;
			if (currentState == null || currentState.StarMap == null || !currentState.StarMap.Fleets.Reverse.ContainsKey(fleetid))
				return;
			StarMapFleet starMapFleet = currentState.StarMap.Fleets.Reverse[fleetid];
			if (starMapFleet == null)
				return;
			currentState.StarMap.SetFocus((IGameObject)starMapFleet);
			currentState.StarMap.Select((IGameObject)starMapFleet);
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (!(panelName == "okButton"))
					return;
				this._app.UI.CloseDialog((Dialog)this, true);
			}
			else
			{
				if (msgType == "list_item_dblclk")
					return;
				if (msgType == "list_sel_changed")
				{
					if (!(panelName == "filterDropdown"))
						return;
					this.CurrentFilterMode = (FleetSummaryDialog.FilterFleets)int.Parse(msgParams[0]);
				}
				else
				{
					if (!(msgType == "checkbox_clicked"))
						return;
					bool flag = int.Parse(msgParams[0]) > 0;
					if (panelName == "hasColonizer")
					{
						this._interestedRoles[ShipRole.COLONIZER] = flag;
						this.RefreshFleets();
					}
					else if (panelName == "hasConstructor")
					{
						this._interestedRoles[ShipRole.CONSTRUCTOR] = flag;
						this.RefreshFleets();
					}
					else
					{
						if (!(panelName == "hasSupply"))
							return;
						this._interestedRoles[ShipRole.SUPPLY] = flag;
						this.RefreshFleets();
					}
				}
			}
		}

		public override string[] CloseDialog()
		{
			this._mainWidget.Dispose();
			this._mainWidget = (FleetWidget)null;
			return (string[])null;
		}

		public enum FilterFleets
		{
			All,
			NormalFleet,
			Name,
			Admiral,
			BaseSystem,
			UnitCount,
			Mission,
			NoMission,
		}
	}
}
