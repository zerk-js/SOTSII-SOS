// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.OverlayPiracyMission
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class OverlayPiracyMission : OverlayMission
	{
		public OverlayPiracyMission(App game, StarMapState state, StarMap starmap, string template = "OverlaySurveyMission")
		  : base(game, state, starmap, MissionType.PIRACY, template)
		{
		}

		protected override void OnCanConfirmMissionChanged(bool newValue)
		{
		}

		protected override bool CanConfirmMission()
		{
			if (this.IsValidFleetID(this.SelectedFleet))
				return this.TargetSystem != 0;
			return false;
		}

		protected override void OnEnter()
		{
			base.OnEnter();
			StarSystemUI.SyncSystemDetailsWidget(this.App, "systemDetailsWidget", this.TargetSystem, false, true);
		}

		protected override void OnCommitMission()
		{
			Kerberos.Sots.StarFleet.StarFleet.SetPiracyMission(this.App.Game, this.SelectedFleet, this.TargetSystem, this._useDirectRoute, 0, 0, this.GetDesignsToBuild());
			AdmiralInfo admiralInfo = this.App.GameDatabase.GetAdmiralInfo(this.App.GameDatabase.GetFleetInfo(this._selectedFleet).AdmiralID);
			if (admiralInfo != null)
				this.App.PostRequestSpeech(string.Format("STRAT_006-01_{0}_{1}StrikeMissionConfirmation", (object)this.App.GameDatabase.GetFactionName(this.App.GameDatabase.GetPlayerFactionID(this.App.LocalPlayer.ID)), (object)admiralInfo.GetAdmiralSoundCueContext(this.App.AssetDatabase)), 50, 120, 0.0f);
			this.App.GetGameState<StarMapState>().RefreshMission();
		}

		protected override string GetMissionDetailsTitle()
		{
			return string.Format(App.Localize("@UI_PIRACY_OVERLAY_MISSION_TITLE"), (object)this.App.GameDatabase.GetStarSystemInfo(this.TargetSystem).Name.ToUpperInvariant());
		}

		protected override void OnRefreshMissionDetails(MissionEstimate estimate)
		{
			this.AddCommonMissionTimes(estimate);
			string hint = App.Localize("@UI_PIRACY_OVERLAY_MISSION_HINT");
			this.AddMissionTime(2, App.Localize("@UI_PIRACY_TIME"), estimate.TurnsAtTarget, hint);
			this.AddMissionCost(estimate);
		}

		protected override IEnumerable<int> GetMissionTargetPlanets()
		{
			return Enumerable.Empty<int>();
		}
	}
}
