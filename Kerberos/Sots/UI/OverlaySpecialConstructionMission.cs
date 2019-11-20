// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.OverlaySpecialConstructionMission
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System.Collections.Generic;

namespace Kerberos.Sots.UI
{
	internal class OverlaySpecialConstructionMission : OverlayConstructionMission
	{
		public int TargetFleet { get; set; }

		public OverlaySpecialConstructionMission(
		  App game,
		  StarMapState state,
		  StarMap starmap,
		  SpecialConstructionMission smission = null,
		  string template = "OverlayGMBuildStationMission")
		  : base(game, state, starmap, smission, template, MissionType.SPECIAL_CONSTRUCT_STN)
		{
		}

		protected override void OnEnter()
		{
			base.OnEnter();
			this.App.UI.SetEnabled(this.App.UI.Path(this.ID, "gamePlaceStationMissionButton"), false);
			this.SyncAvailableStationTypes();
			this.SelectADefaultStation();
		}

		protected override void OnExit()
		{
			this.TargetFleet = 0;
			base.OnExit();
		}

		protected override bool CanConfirmMission()
		{
			return this.IsValidFleetID(this.SelectedFleet);
		}

		protected override void OnCommitMission()
		{
			OverlaySpecialConstructionMission.OnSpecialConstructionPlaced(this._app.Game, this.SelectedFleet, this.TargetFleet, this._useDirectRoute, this.GetDesignsToBuild(), this.SelectedStationType);
		}

		protected override List<StationType> GetAvailableTypes()
		{
			return new List<StationType>() { StationType.SCIENCE };
		}

		protected override void SelectADefaultStation()
		{
			this.SetSelectedStationType(StationType.SCIENCE);
		}

		public static void OnSpecialConstructionPlaced(
		  GameSession sim,
		  int selectedFleet,
		  int targetFleet,
		  bool useDirectRoute,
		  List<int> designsToBuild,
		  StationType stationType)
		{
			Kerberos.Sots.StarFleet.StarFleet.SetSpecialConstructionMission(sim, selectedFleet, targetFleet, useDirectRoute, designsToBuild, stationType);
			FleetInfo fleetInfo = sim.GameDatabase.GetFleetInfo(selectedFleet);
			AdmiralInfo admiralInfo = sim.GameDatabase.GetAdmiralInfo(fleetInfo.AdmiralID);
			if (admiralInfo != null)
			{
				string cueName = string.Format("STRAT_007-01_{0}_{1}ConstructionMissionConfirmation", (object)sim.GameDatabase.GetFactionName(sim.GameDatabase.GetPlayerFactionID(sim.LocalPlayer.ID)), (object)admiralInfo.GetAdmiralSoundCueContext(sim.AssetDatabase));
				sim.App.PostRequestSpeech(cueName, 50, 120, 0.0f);
			}
			sim.App.GetGameState<StarMapState>().RefreshStarmap(StarMapState.StarMapRefreshType.REFRESH_NORMAL);
		}
	}
}
