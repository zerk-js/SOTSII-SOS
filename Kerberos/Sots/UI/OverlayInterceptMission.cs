// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.OverlayInterceptMission
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System.Collections.Generic;

namespace Kerberos.Sots.UI
{
	internal class OverlayInterceptMission : OverlayMission
	{
		private const string UIFleetDetailsWidget = "fleetDetailsWidget";

		public int TargetFleet { get; set; }

		public OverlayInterceptMission(App game, StarMapState state, StarMap starmap, string template = "OverlayInterceptMission")
		  : base(game, state, starmap, MissionType.INTERCEPT, template)
		{
		}

		protected override IEnumerable<int> GetMissionTargetPlanets()
		{
			return (IEnumerable<int>)new List<int>();
		}

		protected override bool CanConfirmMission()
		{
			return this.IsValidFleetID(this.SelectedFleet);
		}

		protected override void OnCommitMission()
		{
			Kerberos.Sots.StarFleet.StarFleet.SetFleetInterceptMission(this.App.Game, this.SelectedFleet, this.TargetFleet, this._useDirectRoute, this.GetDesignsToBuild());
			AdmiralInfo admiralInfo = this.App.GameDatabase.GetAdmiralInfo(this.App.GameDatabase.GetFleetInfo(this._selectedFleet).AdmiralID);
			if (admiralInfo != null)
				this.App.PostRequestSpeech(string.Format("STRAT_010-01_{0}_{1}InterceptMissionConfirmation", (object)this.App.GameDatabase.GetFactionName(this.App.GameDatabase.GetPlayerFactionID(this.App.LocalPlayer.ID)), (object)admiralInfo.GetAdmiralSoundCueContext(this.App.AssetDatabase)), 50, 120, 0.0f);
			this.App.GetGameState<StarMapState>().RefreshMission();
		}

		protected override string GetMissionDetailsTitle()
		{
			return string.Format(App.Localize("@MISSIONWIDGET_INTERCEPT_FLEET_NAME"), (object)this._app.GameDatabase.GetFleetInfo(this.TargetFleet).Name);
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (FleetUI.HandleFleetAndPlanetWidgetInput(this.App, "fleetDetailsWidget", panelName))
				this.UpdateCanConfirmMission();
			else if (msgType == "list_sel_changed")
			{
				if (panelName == "gameFleetList" && msgParams.Length > 0 && !string.IsNullOrEmpty(msgParams[0]))
					this.SelectedFleet = int.Parse(msgParams[0]);
				this.UpdateCanConfirmMission();
			}
			else
				base.OnPanelMessage(panelName, msgType, msgParams);
		}

		protected override void OnCanConfirmMissionChanged(bool newValue)
		{
			if (newValue)
			{
				this.App.UI.SetEnabled("gameConfirmMissionButton", true);
				this.RefreshMissionDetails(StationType.INVALID_TYPE, 1);
			}
			else
				this.App.UI.SetEnabled("gameConfirmMissionButton", false);
		}

		protected override void OnRefreshMissionDetails(MissionEstimate estimate)
		{
			this.AddCommonMissionTimes(estimate);
			this.AddMissionCost(estimate);
		}

		protected override void OnEnter()
		{
			base.OnEnter();
			this.App.UI.SetEnabled("gameConfirmMissionButton", false);
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();
		}

		protected override void OnExit()
		{
			base.OnExit();
		}
	}
}
