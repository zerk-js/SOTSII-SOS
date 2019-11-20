// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.OverlayEvacuationMission
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class OverlayEvacuationMission : OverlayMission
	{
		private const string UIPlanetDetailsWidget = "planetDetailsWidget";
		private const string UIFleetDetailsWidget = "fleetDetailsWidget";
		private const string UIPlanetListWidget = "gamePlanetList";
		private PlanetView _planetView;
		private StellarBody _cachedPlanet;
		private bool _cachedPlanetReady;
		private StarModel _cachedStar;
		private bool _cachedStarReady;
		private int _selectedPlanetToEvacuate;
		private PlanetWidget _planetWidget;

		public OverlayEvacuationMission(
		  App game,
		  StarMapState state,
		  StarMap starmap,
		  string template = "OverlayEvacuationMission")
		  : base(game, state, starmap, MissionType.EVACUATE, template)
		{
		}

		protected override bool CanConfirmMission()
		{
			if (!this.IsValidFleetID(this.SelectedFleet) || Kerberos.Sots.StarFleet.StarFleet.GetNumColonizationShips(this._app.Game, this.SelectedFleet) <= 0)
				return false;
			ColonyInfo colonyInfoForPlanet = this.App.GameDatabase.GetColonyInfoForPlanet(this._selectedPlanetToEvacuate);
			if (colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == this.App.LocalPlayer.ID && this.App.GameDatabase.GetCivilianPopulation(this._selectedPlanetToEvacuate, 0, false) > 0.0)
				return this.IsValidFleetID(this.SelectedFleet);
			return false;
		}

		protected override void OnCommitMission()
		{
			if (this._selectedPlanetToEvacuate == 0)
			{
				double num1 = 0.0;
				foreach (int num2 in this.App.GameDatabase.GetStarSystemPlanets(this.TargetSystem).ToList<int>())
				{
					ColonyInfo colonyInfoForPlanet = this.App.GameDatabase.GetColonyInfoForPlanet(num2);
					if (colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == this.App.LocalPlayer.ID)
					{
						double civilianPopulation = this.App.GameDatabase.GetCivilianPopulation(num2, 0, false);
						if (civilianPopulation > num1)
						{
							this._selectedPlanetToEvacuate = num2;
							num1 = civilianPopulation;
						}
					}
				}
			}
			if (this._selectedPlanetToEvacuate == 0)
				return;
			Kerberos.Sots.StarFleet.StarFleet.SetEvacuationMission(this.App.Game, this.SelectedFleet, this.TargetSystem, this._useDirectRoute, this._selectedPlanetToEvacuate, this.GetDesignsToBuild());
			AdmiralInfo admiralInfo = this.App.GameDatabase.GetAdmiralInfo(this.App.GameDatabase.GetFleetInfo(this._selectedFleet).AdmiralID);
			if (admiralInfo != null)
				this.App.PostRequestSpeech(string.Format("STRAT_002-01_{0}_{1}EvacuationMissionConfirmation", (object)this.App.GameDatabase.GetFactionName(this.App.GameDatabase.GetPlayerFactionID(this.App.LocalPlayer.ID)), (object)admiralInfo.GetAdmiralSoundCueContext(this.App.AssetDatabase)), 50, 120, 0.0f);
			FleetUI.SyncPlanetListControl(this.App.Game, this.App.UI.Path(this.ID, "overlayPlanetList"), this.GetMissionTargetPlanets());
			this.App.GetGameState<StarMapState>().RefreshStarmap(StarMapState.StarMapRefreshType.REFRESH_NORMAL);
		}

		protected override string GetMissionDetailsTitle()
		{
			return string.Format(App.Localize("@MISSIONWIDGET_EVACUATE_PLANET_NAME"), (object)(this._selectedPlanetToEvacuate == 0 ? this.App.GameDatabase.GetStarSystemInfo(this.TargetSystem).Name : this.App.GameDatabase.GetOrbitalObjectInfo(this._selectedPlanetToEvacuate).Name).ToUpperInvariant());
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (FleetUI.HandleFleetAndPlanetWidgetInput(this.App, "fleetDetailsWidget", panelName))
				this.UpdateCanConfirmMission();
			else if (msgType == "mapicon_clicked")
			{
				if (!(panelName == "partMiniSystem") || !this.App.GameDatabase.IsSurveyed(this.App.LocalPlayer.ID, this.TargetSystem))
					return;
				this.SetSelectedPlanet(int.Parse(msgParams[0]), panelName);
			}
			else if (msgType == "list_sel_changed")
			{
				if (panelName == "overlayPlanetList")
				{
					int num = 0;
					if (msgParams.Length > 0 && !string.IsNullOrEmpty(msgParams[0]))
						num = int.Parse(msgParams[0]);
					this.SetSelectedPlanet(num, panelName);
				}
				else if (panelName == "gameFleetList" && msgParams.Length > 0 && !string.IsNullOrEmpty(msgParams[0]))
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

		private void SetSelectedPlanet(int value, string trigger)
		{
			if (this._selectedPlanetToEvacuate == value)
				return;
			this._selectedPlanetToEvacuate = value;
			this._planetWidget.Sync(this._selectedPlanetToEvacuate, false, false);
			StarSystemUI.SyncPlanetDetailsWidget(this.App.Game, "planetDetailsWidget", this.TargetSystem, this._selectedPlanetToEvacuate, this.GetPlanetViewGameObject(this.TargetSystem, this._selectedPlanetToEvacuate), this._planetView);
			this.RefreshMissionDetails(StationType.INVALID_TYPE, 1);
			this.UpdateCanConfirmMission();
		}

		protected override void OnRefreshMissionDetails(MissionEstimate estimate)
		{
			this.AddCommonMissionTimes(estimate);
			string hint1 = App.Localize("@MISSIONWIDGET_TOOLTIP_EVACUATION_TIME");
			this.AddMissionTime(2, App.Localize("@MISSIONWIDGET_EVACUATION_TIME"), estimate.TurnsAtTarget, hint1);
			string hint2 = App.Localize("@EVACUATEMISSION_HINT");
			this.AddMissionTime(6, App.Localize("@MISSIONWIDGET_SUPPORT_TIME"), estimate.TurnsColonySupport, hint2);
			this.AddMissionCost(estimate);
			int num = estimate.TurnsTillPhase1Completion;
			if (num < 1)
				num = 1;
			string str = num == 1 ? App.Localize("@UI_GENERAL_TURN") : App.Localize("@UI_GENERAL_TURNS");
			this.AddMissionNote(string.Format(App.Localize("@MISSIONWIDGET_PHASE_ONE"), (object)num, (object)str));
			FleetUI.SyncPlanetListControl(this.App.Game, this.App.UI.Path(this.ID, "overlayPlanetList"), this.GetMissionTargetPlanets());
		}

		protected override void OnEnter()
		{
			base.OnEnter();
			this._planetWidget = new PlanetWidget(this.App, this.App.UI.Path(this.ID, "planetDetailsCard"));
			IEnumerable<int> missionTargetPlanets = this.GetMissionTargetPlanets();
			FleetUI.SyncPlanetListControl(this.App.Game, this.App.UI.Path(this.ID, "overlayPlanetList"), missionTargetPlanets);
			this.App.UI.SetEnabled("gameConfirmMissionButton", false);
			if (missionTargetPlanets.Count<int>() <= 0)
				return;
			this.SetSelectedPlanet(missionTargetPlanets.First<int>(), "");
		}

		protected override void OnUpdate()
		{
			if (this._planetWidget != null)
				this._planetWidget.Update();
			base.OnUpdate();
		}

		protected override void OnExit()
		{
			this._planetView = (PlanetView)null;
			this._planetWidget.Terminate();
			if (this._cachedPlanet != null)
				this.App.ReleaseObject((IGameObject)this._cachedPlanet);
			this._cachedPlanet = (StellarBody)null;
			this._cachedPlanetReady = false;
			if (this._cachedStar != null)
				this.App.ReleaseObject((IGameObject)this._cachedStar);
			this._cachedStar = (StarModel)null;
			this._cachedStarReady = false;
			this._selectedPlanetToEvacuate = 0;
			base.OnExit();
		}

		protected override IEnumerable<int> GetMissionTargetPlanets()
		{
			return StarSystemDetailsUI.CollectPlanetListItemsForEvacuateMission(this.App, this.TargetSystem, this.App.LocalPlayer.ID);
		}

		private void CacheStar(StarSystemInfo systemInfo)
		{
			if (this._cachedStar != null)
			{
				this.App.ReleaseObject((IGameObject)this._cachedStar);
				this._cachedStar = (StarModel)null;
			}
			this._cachedStarReady = false;
			this._cachedStar = Kerberos.Sots.GameStates.StarSystem.CreateStar(this.App, Vector3.Zero, systemInfo, 1f, false);
			this._cachedStar.PostSetProp("AutoDraw", false);
		}

		private void CachePlanet(PlanetInfo planetInfo)
		{
			if (this._cachedPlanet != null)
			{
				this.App.ReleaseObject((IGameObject)this._cachedPlanet);
				this._cachedPlanet = (StellarBody)null;
			}
			this._cachedPlanetReady = false;
			this._cachedPlanet = Kerberos.Sots.GameStates.StarSystem.CreatePlanet(this.App.Game, Vector3.Zero, planetInfo, Matrix.Identity, 1f, false, Kerberos.Sots.GameStates.StarSystem.TerrestrialPlanetQuality.High);
			this._cachedPlanet.PostSetProp("AutoDraw", false);
		}

		private void UpdateCachedPlanet()
		{
			if (this._cachedPlanet == null || this._cachedPlanetReady || this._cachedPlanet.ObjectStatus == GameObjectStatus.Pending)
				return;
			this._cachedPlanetReady = true;
			this._cachedPlanet.Active = true;
		}

		private void UpdateCachedStar()
		{
			if (this._cachedStar == null || this._cachedStarReady || this._cachedStar.ObjectStatus == GameObjectStatus.Pending)
				return;
			this._cachedStarReady = true;
			this._cachedStar.Active = true;
		}

		private IGameObject GetPlanetViewGameObject(int systemId, int orbitId)
		{
			IGameObject gameObject = (IGameObject)null;
			if (systemId != 0)
			{
				if (orbitId > 0)
				{
					this.CachePlanet(this.App.GameDatabase.GetPlanetInfo(orbitId));
					gameObject = (IGameObject)this._cachedPlanet;
				}
				else
				{
					this.CacheStar(this.App.GameDatabase.GetStarSystemInfo(systemId));
					gameObject = (IGameObject)this._cachedStar;
				}
			}
			return gameObject;
		}
	}
}
