// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.OverlayConstructionMission
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
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class OverlayConstructionMission : OverlayMission
	{
		protected Dictionary<StationType, string> UIStationTypeButtons = new Dictionary<StationType, string>()
	{
	  {
		StationType.DIPLOMATIC,
		"stationDiplomatic"
	  },
	  {
		StationType.CIVILIAN,
		"stationCivilian"
	  },
	  {
		StationType.MINING,
		"stationMining"
	  },
	  {
		StationType.DEFENCE,
		"expand"
	  },
	  {
		StationType.SCIENCE,
		"stationScience"
	  },
	  {
		StationType.NAVAL,
		"stationNaval"
	  },
	  {
		StationType.GATE,
		"stationGate"
	  }
	};
		private const string UIDiplomaticStationButton = "stationDiplomatic";
		private const string UICivilianStationButton = "stationCivilian";
		private const string UIMiningStationButton = "stationMining";
		private const string UIScienceStationButton = "stationScience";
		private const string UINavalStationButton = "stationNaval";
		private const string UIGateStationButton = "stationGate";
		private const string UIPlanetDetailsWidget = "planetDetailsWidget";
		private const string UIDefenceStationButton = "expand";
		protected StationType SelectedStationType;
		protected SpecialConstructionMission _superspecialsecretmission;

		public OverlayConstructionMission(
		  App game,
		  StarMapState state,
		  StarMap starmap,
		  SpecialConstructionMission smission = null,
		  string template = "OverlayStationMission",
		  MissionType mt = MissionType.CONSTRUCT_STN)
		  : base(game, state, starmap, mt, template)
		{
			this._superspecialsecretmission = smission;
		}

		protected override void OnCanConfirmMissionChanged(bool newValue)
		{
			this.App.UI.SetEnabled(this.App.UI.Path(this.ID, "gamePlaceStationMissionButton"), (newValue ? 1 : 0) != 0);
		}

		protected override bool CanConfirmMission()
		{
			if (this.IsValidFleetID(this.SelectedFleet) && this.TargetSystem != 0)
				return this.SelectedStationType != StationType.INVALID_TYPE;
			return false;
		}

		protected override void OnEnter()
		{
			base.OnEnter();
			this.App.UI.SetEnabled(this.App.UI.Path(this.ID, "gamePlaceStationMissionButton"), false);
			this.SelectedPlanet = this.GetMissionTargetPlanets().FirstOrDefault<int>();
			this.SetSelectedPlanet(this.SelectedPlanet, string.Empty);
			this.SyncAvailableStationTypes();
			this.SelectADefaultStation();
			if (this._superspecialsecretmission == null || this._superspecialsecretmission._forcedStationType == StationType.INVALID_TYPE)
				return;
			this.SelectedStationType = this._superspecialsecretmission._forcedStationType;
		}

		protected override StationType GetSelectedStationtype()
		{
			return this.SelectedStationType;
		}

		protected override void OnExit()
		{
			this._selectedPlanet = 0;
			base.OnExit();
		}

		protected override void OnCommitMission()
		{
			if (this._app.LocalPlayer.Faction.Name == "loa")
			{
				Kerberos.Sots.StarFleet.StarFleet.ConvertFleetIntoLoaCubes(this._app.Game, this._selectedFleet);
				Kerberos.Sots.StarFleet.StarFleet.BuildFleetFromComposition(this._app.Game, this._selectedFleet, MissionType.CONSTRUCT_STN);
				this.RebuildShipLists(this.SelectedFleet);
			}
			if (this._superspecialsecretmission != null)
			{
				OverlayConstructionMission.OnConstructionPlaced(this._app.Game, this.SelectedFleet, this.TargetSystem, this._useDirectRoute, this._superspecialsecretmission._targetplanet, this.GetDesignsToBuild(), this.SelectedStationType, this.RebaseTarget, false);
			}
			else
			{
				int? planetForStation = Kerberos.Sots.GameStates.StarSystem.GetSuitablePlanetForStation(this._app.Game, this._app.LocalPlayer.ID, this.TargetSystem, this.SelectedStationType);
				if (!planetForStation.HasValue)
					return;
				OverlayConstructionMission.OnConstructionPlaced(this._app.Game, this.SelectedFleet, this.TargetSystem, this._useDirectRoute, planetForStation.Value, this.GetDesignsToBuild(), this.SelectedStationType, this.RebaseTarget, false);
			}
		}

		protected override string GetMissionDetailsTitle()
		{
			return App.Localize("@MISSIONTITLE_STATION_CONSTRUCTION_MISSION");
		}

		protected override void OnRefreshMissionDetails(MissionEstimate estimate)
		{
			OverlayConstructionMission.ReRefreshMissionDetails(this._app, estimate);
		}

		protected override IEnumerable<int> GetMissionTargetPlanets()
		{
			return StarSystemDetailsUI.CollectPlanetListItemsForConstructionMission(this._app, this.TargetSystem);
		}

		public static void RefreshMissionUI(App game, int selectedPlanet, int targetSystem)
		{
			game.UI.SetText("gameMissionTitle", App.Localize("@MISSIONTITLE_CONSTRUCTION_MISSION"));
			game.UI.ClearItems("gameMissionTimes");
			game.UI.ClearItems("gameMissionNotes");
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName.Contains("_z"))
					panelName = panelName.Substring(0, panelName.Count<char>() - 2);
				if (this.UIStationTypeButtons.ContainsValue(panelName))
				{
					this.SetSelectedStationType(this.UIStationTypeButtons.First<KeyValuePair<StationType, string>>((Func<KeyValuePair<StationType, string>, bool>)(x => x.Value == panelName)).Key);
					this.RefreshMissionDetails(this.SelectedStationType, 1);
					this.UpdateCanConfirmMission();
				}
				if (panelName == "gamePlaceStationMissionButton")
				{
					if (this._app.LocalPlayer.Faction.Name == "loa")
					{
						Kerberos.Sots.StarFleet.StarFleet.ConvertFleetIntoLoaCubes(this._app.Game, this._selectedFleet);
						Kerberos.Sots.StarFleet.StarFleet.BuildFleetFromComposition(this._app.Game, this._selectedFleet, MissionType.CONSTRUCT_STN);
						this.RebuildShipLists(this.SelectedFleet);
					}
					this._app.SwitchGameState<StationPlacementState>((object)this.TargetSystem, (object)this.SelectedPlanet, (object)this._app.Game, (object)this.SelectedFleet, (object)this.GetDesignsToBuild(), (object)this.SelectedStationType, (object)this._missionEstimate, (object)this._useDirectRoute, (object)this.RebaseTarget);
					this.Hide();
					this.App.GetGameState<StarMapState>().RightClickEnabled = false;
				}
			}
			else if (msgType == "checkbox_clicked")
			{
				bool flag = int.Parse(msgParams[0]) > 0;
				if (panelName == "gameRebaseToggleStn")
				{
					this.ReBaseToggle = flag;
					this.RefreshMissionDetails(this.SelectedStationType, 1);
				}
			}
			base.OnPanelMessage(panelName, msgType, msgParams);
		}

		private void SetSelectedPlanet(int value, string trigger)
		{
			if (this._selectedPlanet == value)
				return;
			this._selectedPlanet = value;
			this.RefreshMissionDetails(StationType.INVALID_TYPE, 1);
			this.UpdateCanConfirmMission();
		}

		protected void SetSelectedStationType(StationType type)
		{
			this.SelectedStationType = type;
			foreach (KeyValuePair<StationType, string> stationTypeButton in this.UIStationTypeButtons)
			{
				if (this._app.LocalPlayer.Faction.Name != "zuul" || stationTypeButton.Key == StationType.SCIENCE || (stationTypeButton.Key == StationType.NAVAL || stationTypeButton.Key == StationType.DEFENCE) || stationTypeButton.Key == StationType.GATE)
				{
					if (stationTypeButton.Key != type)
						this._app.UI.SetChecked(this._app.UI.Path(this.ID, stationTypeButton.Value), false);
					else
						this._app.UI.SetChecked(this._app.UI.Path(this.ID, stationTypeButton.Value), true);
				}
				else if (stationTypeButton.Key != type)
					this._app.UI.SetChecked(this._app.UI.Path(this.ID, stationTypeButton.Value) + "_z", false);
				else
					this._app.UI.SetChecked(this._app.UI.Path(this.ID, stationTypeButton.Value) + "_z", true);
			}
		}

		protected virtual List<StationType> GetAvailableTypes()
		{
			return Kerberos.Sots.GameStates.StarSystem.GetSystemCanSupportStations(this._app.Game, this.TargetSystem, this._app.LocalPlayer.ID);
		}

		protected void SyncAvailableStationTypes()
		{
			List<StationType> availableTypes = this.GetAvailableTypes();
			if (this._app.LocalPlayer.Faction.CanUseGate() && this._app.GameDatabase.PlayerHasTech(this._app.LocalPlayer.ID, "DRV_Gate_Stations"))
			{
				this._app.UI.SetVisible(this._app.UI.Path(this.ID, this.UIStationTypeButtons[StationType.GATE]), true);
				this._app.UI.SetEnabled(this._app.UI.Path(this.ID, this.UIStationTypeButtons[StationType.GATE]), (!this._app.GameDatabase.GetStationForSystemAndPlayer(this.TargetSystem, this._app.LocalPlayer.ID).Any<StationInfo>((Func<StationInfo, bool>)(x => x.DesignInfo.StationType == StationType.GATE)) ? 1 : 0) != 0);
			}
			else
				this._app.UI.SetVisible(this._app.UI.Path(this.ID, this.UIStationTypeButtons[StationType.GATE]), false);
			if (this._app.LocalPlayer.Faction.Name == "zuul")
			{
				this._app.UI.SetVisible(this._app.UI.Path(this.ID, "contents"), false);
				this._app.UI.SetVisible(this._app.UI.Path(this.ID, "zuul_contents"), true);
			}
			foreach (KeyValuePair<StationType, string> stationTypeButton in this.UIStationTypeButtons)
			{
				if (this._superspecialsecretmission != null && this._superspecialsecretmission._forcedStationType != StationType.INVALID_TYPE)
				{
					if (this._app.LocalPlayer.Faction.Name != "zuul" || stationTypeButton.Key == StationType.SCIENCE || (stationTypeButton.Key == StationType.NAVAL || stationTypeButton.Key == StationType.DEFENCE) || stationTypeButton.Key == StationType.GATE)
					{
						if (stationTypeButton.Key == this._superspecialsecretmission._forcedStationType)
						{
							this._app.UI.SetEnabled(this._app.UI.Path(this.ID, stationTypeButton.Value), true);
							this._app.UI.SetChecked(this._app.UI.Path(this.ID, stationTypeButton.Value), true);
						}
						else
						{
							this._app.UI.SetEnabled(this._app.UI.Path(this.ID, stationTypeButton.Value), false);
							this._app.UI.SetChecked(this._app.UI.Path(this.ID, stationTypeButton.Value), false);
						}
					}
					else if (stationTypeButton.Key == this._superspecialsecretmission._forcedStationType)
					{
						this._app.UI.SetEnabled(this._app.UI.Path(this.ID, stationTypeButton.Value) + "_z", true);
						this._app.UI.SetChecked(this._app.UI.Path(this.ID, stationTypeButton.Value) + "_z", true);
					}
					else
					{
						this._app.UI.SetEnabled(this._app.UI.Path(this.ID, stationTypeButton.Value) + "_z", false);
						this._app.UI.SetChecked(this._app.UI.Path(this.ID, stationTypeButton.Value) + "_z", false);
					}
				}
				else if (this._app.LocalPlayer.Faction.Name != "zuul" || stationTypeButton.Key == StationType.SCIENCE || (stationTypeButton.Key == StationType.NAVAL || stationTypeButton.Key == StationType.DEFENCE) || stationTypeButton.Key == StationType.GATE)
				{
					if (availableTypes.Contains(stationTypeButton.Key))
					{
						this._app.UI.SetEnabled(this._app.UI.Path(this.ID, stationTypeButton.Value), true);
					}
					else
					{
						this._app.UI.SetEnabled(this._app.UI.Path(this.ID, stationTypeButton.Value), false);
						this._app.UI.SetChecked(this._app.UI.Path(this.ID, stationTypeButton.Value), false);
					}
				}
				else if (availableTypes.Contains(stationTypeButton.Key))
				{
					this._app.UI.SetEnabled(this._app.UI.Path(this.ID, stationTypeButton.Value) + "_z", true);
				}
				else
				{
					this._app.UI.SetEnabled(this._app.UI.Path(this.ID, stationTypeButton.Value) + "_z", false);
					this._app.UI.SetChecked(this._app.UI.Path(this.ID, stationTypeButton.Value) + "_z", false);
				}
			}
		}

		protected virtual void SelectADefaultStation()
		{
			List<StationType> canSupportStations = Kerberos.Sots.GameStates.StarSystem.GetSystemCanSupportStations(this._app.Game, this.TargetSystem, this._app.LocalPlayer.ID);
			foreach (KeyValuePair<StationType, string> stationTypeButton in this.UIStationTypeButtons)
			{
				if (canSupportStations.Contains(stationTypeButton.Key))
				{
					this.SetSelectedStationType(stationTypeButton.Key);
					break;
				}
			}
		}

		public static void OnConstructionPlaced(
		  GameSession sim,
		  int selectedFleet,
		  int targetSystem,
		  bool useDirectRoute,
		  int selectedPlanet,
		  List<int> designsToBuild,
		  StationType stationType,
		  int? ReBaseTarget,
		  bool AskForCompo)
		{
			if (selectedPlanet == 0)
				return;
			Kerberos.Sots.StarFleet.StarFleet.SetConstructionMission(sim, selectedFleet, targetSystem, useDirectRoute, selectedPlanet, designsToBuild, stationType, ReBaseTarget);
			FleetInfo fleetInfo = sim.GameDatabase.GetFleetInfo(selectedFleet);
			AdmiralInfo admiralInfo = sim.GameDatabase.GetAdmiralInfo(fleetInfo.AdmiralID);
			if (admiralInfo != null)
			{
				string cueName = string.Format("STRAT_007-01_{0}_{1}ConstructionMissionConfirmation", (object)sim.GameDatabase.GetFactionName(sim.GameDatabase.GetPlayerFactionID(sim.LocalPlayer.ID)), (object)admiralInfo.GetAdmiralSoundCueContext(sim.AssetDatabase));
				sim.App.PostRequestSpeech(cueName, 50, 120, 0.0f);
			}
			if (sim.LocalPlayer.Faction == sim.AssetDatabase.GetFaction("loa") && AskForCompo)
				sim.UI.CreateDialog((Dialog)new DialogLoaFleetSelector(sim.App, MissionType.CONSTRUCT_STN, sim.GameDatabase.GetFleetInfo(selectedFleet), true), null);
			sim.App.GetGameState<StarMapState>().RefreshMission();
		}

		public static void ReRefreshMissionDetails(App game, MissionEstimate estimate)
		{
			OverlayMission.AddCommonMissionTimes(game, estimate);
			string hint = App.Localize("@MISSIONWIDGET_TOOLTIP_CONSTRUCTION_TIME");
			OverlayMission.AddMissionTime(game, 2, App.Localize("@MISSIONWIDGET_CONSTRUCTION_TIME"), estimate.TurnsAtTarget, hint);
			OverlayMission.AddMissionCost(game, estimate);
		}
	}
}
