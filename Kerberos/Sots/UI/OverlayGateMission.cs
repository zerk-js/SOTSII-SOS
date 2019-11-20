// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.OverlayGateMission
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.GameStates;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class OverlayGateMission : OverlayMission
	{
		public OverlayGateMission(App game, StarMapState state, StarMap starmap, string template = "OverlaySurveyMission")
		  : base(game, state, starmap, MissionType.GATE, template)
		{
		}

		protected override void OnCanConfirmMissionChanged(bool newValue)
		{
		}

		protected override bool CanConfirmMission()
		{
			if (this.IsValidFleetID(this.SelectedFleet) && this.TargetSystem != 0)
				return Kerberos.Sots.StarFleet.StarFleet.CanDoGateMissionToTarget(this.App.Game, this.TargetSystem, this.SelectedFleet, new float?(), new float?(), new float?());
			return false;
		}

		protected override void OnEnter()
		{
			base.OnEnter();
			StarSystemUI.SyncSystemDetailsWidget(this.App, "systemDetailsWidget", this.TargetSystem, false, true);
		}

		protected override void OnCommitMission()
		{
			Kerberos.Sots.StarFleet.StarFleet.SetGateMission(this.App.Game, this.SelectedFleet, this.TargetSystem, this._useDirectRoute, this.GetDesignsToBuild(), new int?());
			this.App.GetGameState<StarMapState>().RefreshMission();
		}

		protected override string GetMissionDetailsTitle()
		{
			return string.Format(App.Localize("@UI_GATE_OVERLAY_MISSION_TITLE"), (object)this.App.GameDatabase.GetStarSystemInfo(this.TargetSystem).Name.ToUpperInvariant());
		}

		protected override void OnRefreshMissionDetails(MissionEstimate estimate)
		{
			this.AddCommonMissionTimes(estimate);
			string hint = App.Localize("@UI_GATE_OVERLAY_MISSION_HINT");
			this.AddMissionTime(2, App.Localize("@UI_MISSION_GATE"), estimate.TurnsAtTarget, hint);
			this.AddMissionCost(estimate);
			this.UpdateCanConfirmMission();
		}

		protected override IEnumerable<int> GetMissionTargetPlanets()
		{
			return Enumerable.Empty<int>();
		}
	}
}
