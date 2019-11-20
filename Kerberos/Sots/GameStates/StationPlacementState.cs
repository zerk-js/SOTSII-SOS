// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.StationPlacementState
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.GameStates
{
	internal class StationPlacementState : BasicStarSystemState
	{
		protected static readonly string UICancelButton = "gameCancelMissionButton";
		protected static readonly string UICommitButton = "gameConfirmMissionButton";
		private const string UIMissionAdmiralName = "gameAdmiralName";
		private const string UIMissionAdmiralFleet = "gameAdmiralFleet";
		private const string UIMissionAdmiralSkills = "gameAdmiralSkills";
		private const string UIMissionAdmiralAvatar = "gameAdmiralAvatar";
		protected const int UIItemMissionTotalTime = 0;
		protected const int UIItemMissionTravelTime = 1;
		protected const int UIItemMissionTime = 2;
		protected const int UIItemMissionBuildTime = 3;
		protected const int UIItemMissionCostSeparator = 4;
		protected const int UIItemMissionCost = 5;
		protected const int UIItemMissionSupportTime = 6;
		private int _targetSystemID;
		private int _selectedPlanetID;
		private GameSession _sim;
		private int _selectedFleetID;
		private List<int> _designsToBuild;
		private StationType _stationType;
		private GameObjectSet _crits;
		private GameObjectSet _dummies;
		private MissionEstimate _missionEstimate;
		private StationPlacement _manager;
		private BudgetPiechart _piechart;
		private bool _useDirectRoute;
		private int? _rebase;

		public StationPlacementState(App game)
		  : base(game)
		{
		}

		protected override void OnBack()
		{
			StarMapState gameState = this.App.GetGameState<StarMapState>();
			this.App.SwitchGameState((GameState)gameState);
			gameState.ShowOverlay(MissionType.CONSTRUCT_STN, this._targetSystemID);
		}

		public static string GetAdmiralTraitText(AdmiralInfo.TraitType trait)
		{
			return App.Localize(string.Format("@ADMIRALTRAITS_{0}", (object)trait.ToString().ToUpper()));
		}

		public static string GetAdmiralTraitsString(App game, int admiralId)
		{
			string empty = string.Empty;
			foreach (AdmiralInfo.TraitType traitType in (AdmiralInfo.TraitType[])Enum.GetValues(typeof(AdmiralInfo.TraitType)))
			{
				if (game.GameDatabase.GetLevelForAdmiralTrait(admiralId, traitType) > 0)
				{
					if (!string.IsNullOrEmpty(empty))
						empty += ", ";
					empty += StationPlacementState.GetAdmiralTraitText(traitType);
				}
			}
			return empty;
		}

		public static void RefreshFleetAdmiralDetails(App game, int fleetId)
		{
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetId);
			string str1 = game.GameDatabase.GetFactionName(game.GameDatabase.GetFleetFaction(fleetId));
			string str2 = string.Empty;
			string str3 = string.Empty;
			if (fleetInfo.AdmiralID != 0)
			{
				AdmiralInfo admiralInfo = game.GameDatabase.GetAdmiralInfo(fleetInfo.AdmiralID);
				str2 = admiralInfo.Name;
				str3 = StationPlacementState.GetAdmiralTraitsString(game, fleetInfo.AdmiralID);
				str1 = admiralInfo.Race;
			}
			string upperInvariant1 = string.Format(App.Localize("@MISSIONWIDGET_ADMIRAL"), (object)str2).ToUpperInvariant();
			game.UI.SetText("gameAdmiralName", upperInvariant1);
			string upperInvariant2 = string.Format(App.Localize("@MISSIONWIDGET_FLEET"), (object)fleetInfo.Name).ToUpperInvariant();
			game.UI.SetText("gameAdmiralFleet", upperInvariant2);
			string text = string.Format(App.Localize("@MISSIONWIDGET_ADMIRAL_TRAITS"), (object)str3);
			game.UI.SetText("gameAdmiralSkills", text);
			string propertyValue = string.Format("admiral_{0}", (object)str1);
			if (fleetInfo.AdmiralID != 0)
				propertyValue = Kerberos.Sots.StarFleet.StarFleet.GetAdmiralAvatar(game, fleetInfo.AdmiralID);
			game.UI.SetPropertyString("gameAdmiralAvatar", "sprite", propertyValue);
		}

		protected override void OnPrepare(GameState prev, object[] stateParams)
		{
			base.OnPrepare(prev, stateParams);
			this._targetSystemID = (int)stateParams[0];
			this._selectedPlanetID = (int)stateParams[1];
			this._sim = (GameSession)stateParams[2];
			this._selectedFleetID = (int)stateParams[3];
			this._designsToBuild = (List<int>)stateParams[4];
			this._stationType = (StationType)stateParams[5];
			this._missionEstimate = (MissionEstimate)stateParams[6];
			this._useDirectRoute = (bool)stateParams[7];
			this._rebase = (int?)stateParams[8];
			this._crits = new GameObjectSet(this.App);
			this._dummies = new GameObjectSet(this.App);
			DesignInfo di = DesignLab.CreateStationDesignInfo(this.App.AssetDatabase, this.App.GameDatabase, this.App.LocalPlayer.ID, this._stationType, 1, false);
			StarSystemDummyOccupant systemDummyOccupant = new StarSystemDummyOccupant(this.App, this.App.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == di.DesignSections[0].FilePath)).ModelName, this._stationType);
			this._dummies.Add((IGameObject)systemDummyOccupant);
			this._starsystem.PostObjectAddObjects((IGameObject)systemDummyOccupant);
			this._manager = new StationPlacement(this.App, this.App.LocalPlayer.Faction.Name == "zuul");
			this._manager.PostSetProp("SetStarSystem", (IGameObject)this._starsystem);
			this._manager.PostSetProp("SetPlacementStamp", (IGameObject)systemDummyOccupant);
			this._manager.PostSetProp("SetMissionType", (object)this._stationType.ToFlags());
			this.App.UI.LoadScreen("StationPlacement");
		}

		protected override void OnEnter()
		{
			base.OnEnter();
			this._crits.Activate();
			this._dummies.Activate();
			this.App.UI.UnlockUI();
			this.App.UI.SetScreen("StationPlacement");
			this.App.UI.SetPropertyBool("gameCancelMissionButton", "lockout_button", true);
			this.App.UI.SetPropertyBool("gameConfirmMissionButton", "lockout_button", true);
			this.App.UI.SetPropertyBool("gameExitButton", "lockout_button", true);
			this._piechart = new BudgetPiechart(this.App.UI, "piechart", this.App.AssetDatabase);
			EmpireBarUI.SyncTitleFrame(this.App);
			EmpireBarUI.SyncTitleBar(this.App, "gameEmpireBar", this._piechart);
			this.Camera.DesiredPitch = MathHelper.DegreesToRadians(-40f);
			this.Camera.DesiredDistance = 80000f;
			this.Camera.MinPitch = MathHelper.DegreesToRadians(-60f);
			this.Camera.MaxPitch = MathHelper.DegreesToRadians(-20f);
			this._manager.Active = true;
			this._starsystem.SetAutoDrawEnabled(false);
			bool flag = false;
			if (this._stationType == StationType.MINING)
			{
				int? planetForStation = StarSystem.GetSuitablePlanetForStation(this.App.Game, this.App.LocalPlayer.ID, this._targetSystemID, this._stationType);
				if (planetForStation.HasValue)
				{
					this.SetSelectedObject(planetForStation.Value, "");
					flag = true;
				}
			}
			else
			{
				foreach (PlanetInfo planetInfo in this.App.GameDatabase.GetPlanetInfosOrbitingStar(this._targetSystemID))
				{
					if (planetInfo != null)
					{
						ColonyInfo colonyInfoForPlanet = this.App.GameDatabase.GetColonyInfoForPlanet(planetInfo.ID);
						if (colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == this.App.LocalPlayer.ID)
						{
							this.SetSelectedObject(planetInfo.ID, "");
							flag = true;
							break;
						}
					}
				}
			}
			if (!flag)
				this.SetSelectedObject(-1, "");
			StationPlacementState.RefreshFleetAdmiralDetails(this.App, this._selectedFleetID);
			OverlayConstructionMission.RefreshMissionUI(this.App, this._selectedPlanetID, this._targetSystemID);
			OverlayConstructionMission.ReRefreshMissionDetails(this.App, this._missionEstimate);
			this.App.UI.AutoSizeContents("gameMissionDetails");
		}

		protected override void OnExit(GameState prev, ExitReason reason)
		{
			this._piechart = (BudgetPiechart)null;
			this._manager.Dispose();
			this._crits.Dispose();
			this._crits = (GameObjectSet)null;
			this._dummies.Dispose();
			this._dummies = (GameObjectSet)null;
			this._sim = (GameSession)null;
			this._designsToBuild = (List<int>)null;
			base.OnExit(prev, reason);
		}

		public override bool IsReady()
		{
			if (!this._crits.IsReady() || !this._dummies.IsReady() || !base.IsReady())
				return false;
			return base.IsReady();
		}

		protected override void OnUIGameEvent(string eventName, string[] eventParams)
		{
			this._piechart.TryGameEvent(eventName, eventParams);
			if (eventName == "StationSpotSelected")
			{
				this._selectedPlanetID = int.Parse(eventParams[0]);
				this.Camera.DesiredDistance = 4000f;
				this.Camera.PostSetProp("TargetID", 0);
				this.Camera.TargetPosition = new Vector3(float.Parse(eventParams[1]), float.Parse(eventParams[2]), float.Parse(eventParams[3]));
			}
			if (!(eventName == "StationSpotUnselected"))
				return;
			this._selectedPlanetID = int.Parse(eventParams[0]);
			this.Camera.DesiredDistance = 30000f;
			this.Camera.PostSetProp("TargetID", 0);
			this.Camera.TargetPosition = new Vector3(float.Parse(eventParams[1]), float.Parse(eventParams[2]), float.Parse(eventParams[3]));
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (this._piechart.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self) || !(msgType == "button_clicked"))
				return;
			if (panelName == StationPlacementState.UICancelButton)
			{
				this.App.GetGameState<StarMapState>().ShowInterface = true;
				this.App.GetGameState<StarMapState>().RightClickEnabled = true;
				this.App.UI.Send((object)"SetWidthProp", (object)"OH_StarMap", (object)"parent:width");
				this.App.SwitchGameState<StarMapState>();
			}
			else
			{
				if (!(panelName == StationPlacementState.UICommitButton))
					return;
				OverlayConstructionMission.OnConstructionPlaced(this._sim, this._selectedFleetID, this._targetSystemID, this._useDirectRoute, this._selectedPlanetID, this._designsToBuild, this._stationType, this._rebase, true);
				this.App.GetGameState<StarMapState>().ShowInterface = true;
				this.App.GetGameState<StarMapState>().RightClickEnabled = true;
				this.App.UI.Send((object)"SetWidthProp", (object)"OH_StarMap", (object)"parent:width");
				this.App.SwitchGameState<StarMapState>();
			}
		}
	}
}
