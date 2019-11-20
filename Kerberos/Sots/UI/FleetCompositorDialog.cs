// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.FleetCompositorDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.GameStates;
using System;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class FleetCompositorDialog : Dialog
	{
		private int _systemid;
		private App App;
		private int fleetid;
		private FleetWidget fleetlist;
		private FleetWidget workingfleet;

		public FleetCompositorDialog(App game, int systemID, int fleetID, string template = "dialogFleetCompositor")
		  : base(game, template)
		{
			this._systemid = systemID;
			this.App = game;
			this.fleetid = fleetID;
		}

		public override void Initialize()
		{
			this.fleetlist = new FleetWidget(this._app, this.UI.Path(this.ID, "gameFleetList"));
			this.workingfleet = new FleetWidget(this._app, this.UI.Path(this.ID, "gameWorkingFleet"));
			this.fleetlist.DisableTooltips = true;
			this.workingfleet.DisableTooltips = true;
			this.fleetlist.LinkWidget(this.workingfleet);
			this.workingfleet.LinkWidget(this.fleetlist);
			if (this._app.CurrentState == this._app.GetGameState<FleetManagerState>())
				this._app.UI.SetVisible(this._app.UI.Path(this.ID, "FleetManager"), false);
			else
				this._app.UI.SetVisible(this._app.UI.Path(this.ID, "FleetManager"), true);
			this.fleetlist.SetSyncedFleets(this._app.GameDatabase.GetFleetsByPlayerAndSystem(this.App.LocalPlayer.ID, this._systemid, FleetType.FL_NORMAL | FleetType.FL_RESERVE | FleetType.FL_DEFENSE).Where<FleetInfo>((Func<FleetInfo, bool>)(x => x.ID != this.fleetid)).ToList<FleetInfo>());
			this.workingfleet.SetSyncedFleets(this.fleetid);
			OverlayMission.RefreshFleetAdmiralDetails(this.App, this.ID, this.fleetid, "admiralDetails");
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (!(msgType == "button_clicked"))
				return;
			if (panelName == "OkButton")
			{
				if (this._app.CurrentState == this._app.GetGameState<StarMapState>())
					this._app.GetGameState<StarMapState>().RefreshStarmap(StarMapState.StarMapRefreshType.REFRESH_NORMAL);
				if (this._app.CurrentState == this._app.GetGameState<FleetManagerState>())
					this._app.GetGameState<FleetManagerState>().Refresh();
				this.App.UI.CloseDialog((Dialog)this, true);
			}
			else
			{
				if (!(panelName == "FleetManager"))
					return;
				this.App.UI.CloseDialog((Dialog)this, true);
				this.App.SwitchGameState<FleetManagerState>((object)this._systemid);
			}
		}

		public override string[] CloseDialog()
		{
			this.fleetlist.Dispose();
			this.workingfleet.Dispose();
			return (string[])null;
		}
	}
}
