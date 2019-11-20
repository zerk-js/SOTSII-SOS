// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.StarMapState
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.TechnologyFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using Kerberos.Sots.UI;
using Kerberos.Sots.UI.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.GameStates
{
	internal class StarMapState : GameState, IKeyBindListener
	{
		private bool _uiEnabled = true;
		private bool _showInterface = true;
		private bool _rightClickEnabled = true;
		private int _TurnLastUpdated = -1;
		public const string StarSystemPopupPanelID = "StarSystemPopup";
		private const string UISystemDetailsWidget = "systemDetailsWidget";
		private const string UIPlanetDetailsWidget = "planetDetailsWidget";
		private const string UIFleetAndPlanetDetailsWidget = "fleetAndPlanetDetailsWidget";
		public const string UITurnCount = "turn_count";
		private const string DebugTestCombatButton = "debugTestCombatButton";
		private const string UIEmpireBar = "gameEmpireBar";
		private const string UIColonyDetailsWidget = "colonyDetailsWidget";
		private const string UIColonyEstablished = "colony_event_dialog";
		private const string UIExitButton = "gameExitButton";
		private const string UIOptionsButton = "gameOptionsButton";
		private const string UISaveGameButton = "gameSaveGameButton";
		private const string UIEndTurnButton = "gameEndTurnButton";
		private const string UIEmpireSummaryButton = "gameEmpireSummaryButton";
		private const string UIResearchButton = "gameResearchButton";
		private const string UIDiplomacyButton = "gameDiplomacyButton";
		private const string UIAbandonColony = "btnAbandon";
		private const string UIHardenStructuresButton = "partHardenedStructure";
		private const string UIDesignButton = "gameDesignButton";
		private const string UIRepairButton = "gameRepairButton";
		private const string UIBuildButton = "gameBuildButton";
		private const string UISystemButton = "gameSystemButton";
		private const string UISotspediaButton = "gameSotspediaButton";
		private const string UIProvinceModeButton = "gameProvinceModeButton";
		private const string UIEventHistoryButton = "gameEventHistoryButton";
		private const string UIBattleRiderManagerButton = "gameBattleRiderManagerButton";
		private const string UITutorialButton = "gameTutorialButton";
		private const string UICloseTutorialButton = "starMapTutImage";
		private const string UIOpenSystemButton = "btnSystemOpen";
		private const string UIEventImageButton = "btnturnEventImageButton";
		private const string UIFleetCancelMissionButton = "fleetCancelButton";
		private const string UIFleetInterceptButton = "fleetInterceptButton";
		private const string UIFleetBuildStationButton = "fleetBuildStationButton";
		private const string UISurveyButton = "gameSurveyButton";
		private const string UIColonizeButton = "gameColonizeButton";
		private const string UIEvacuateButton = "gameEvacuateButton";
		private const string UIRelocateButton = "gameRelocateButton";
		private const string UIPatrolButton = "gamePatrolButton";
		private const string UIInterdictButton = "gameInterdictButton";
		private const string UIStrikeButton = "gameStrikeButton";
		private const string UIInvadeButton = "gameInvadeButton";
		private const string UISupportButton = "gameSupportButton";
		private const string UIConstructStationButton = "gameConstructStationButton";
		private const string UIUpgradeStationButton = "gameUpgradeStationButton";
		private const string UIStationManagerButton = "gameStationManagerButton";
		private const string UIContextStationManagerButton = "gameContextStationManagerButton";
		private const string UIPlanetManagerButton = "gamePlanetSummaryButton";
		private const string UIPopulationManagerButton = "gamePopulationManagerButton";
		private const string UIComparativeAnalysysButton = "gameComparativeAnalysysButton";
		private const string UIFleetSummaryButton = "gameFleetSummaryButton";
		private const string UIFleetManagerButton = "gameFleetManagerButton";
		private const string UIContextFleetManagerButton = "gameContextFleetManagerButton";
		private const string UIDefenseManagerButton = "gameDefenseManagerButton";
		private const string UIGateButton = "gameGateButton";
		private const string UIPiracyButton = "gamePiracyButton";
		private const string UINPGButton = "gameNPGButton";
		private StarMapStateMode _mode;
		private GameObjectSet _crits;
		private ArrowPainter _painter;
		private Sky _sky;
		private StarMap _starmap;
		private PlanetView _planetView;
		private StellarBody _cachedPlanet;
		private PlanetInfo _cachedPlanetInfo;
		private bool _cachedPlanetReady;
		private StarModel _cachedStar;
		private StarSystemInfo _cachedStarInfo;
		private bool _cachedStarReady;
		private string _contextMenuID;
		private string _researchContextID;
		private string _fleetContextMenuID;
		private string _enemyContextMenuID;
		private string _enemyGMStationContextMenuID;
		private bool _isProvinceMode;
		private FleetWidget _fleetWidget;
		private BudgetPiechart _piechart;
		private ColonizeDialog _colonizeDialog;
		private ColonySelfSufficientDialog _selfSufficientDialog;
		private RequestRequestedDialog _requestRequestedDialog;
		private DemandRequestedDialog _demandRequestedDialog;
		private Dialog _treatyRequestedDialog;
		private GenericTextDialog _requestAcceptedDialog;
		private GenericTextDialog _requestDeclinedDialog;
		private GenericTextDialog _demandAcceptedDialog;
		private GenericTextDialog _demandDeclinedDialog;
		private GenericTextDialog _treatyAcceptedDialog;
		private GenericTextDialog _treatyDeclinedDialog;
		private GenericTextDialog _treatyExpiredDialog;
		private LimitationTreatyBrokenDialog _treatyBrokenDialogOffender;
		private LimitationTreatyBrokenDialog _treatyBrokenDialogVictim;
		private bool _eventDialogShown;
		private PlayerWidget _playerWidget;
		private StationBuiltDialog _stationDialog;
		internal StarMapViewFilter _lastFilterSelection;
		private int _simNewTurnTick;
		private TurnEvent _currentEvent;
		private StarMapSystem _contextsystem;
		private PlanetWidget _planetWidget;
		private TechCube _techCube;
		private string _surveyDialog;
		private string _researchCompleteDialog;
		private string _feasibilityCompleteDialog;
		private string _superWorldDialog;
		private string _confirmAbandon;
		private string _suulkaArrivalDialog;
		private string _endTurnConfirmDialog;
		public StarMapState.ObjectSelectionChangedDelegate OnObjectSelectionChanged;
		private ESMDialogState _dialogState;
		private string _enteredColonyName;
		private int _selectedPlanet;
		private int _colonyEstablishedPlanet;
		private int _colonyEstablishedSystem;
		private int _contextMenuSystem;
		private int _fleetContextFleet;
		private int _lastSelectedFleet;
		private string DeleteFrieghterConfirm;
		private int _selectedfreighter;
		private int _prevNumStations;
		private Dictionary<string, OverlayMission> _missionOverlays;
		private OverlayMission _reactionOverlay;
		private bool _initialized;
		private int _dialogCounter;

		public bool EnableFleetCheck { set; get; }

		public bool ShowInterface
		{
			get
			{
				return this._showInterface;
			}
			set
			{
				if (this._showInterface == value)
					return;
				this._showInterface = value;
				this.App.UI.SetVisible("bottomBarWidget", this._showInterface);
				this.App.UI.SetVisible("topLeftWidget", this._showInterface);
				this.App.UI.SetVisible("leftSideWidget", this._showInterface);
				if (!this._showInterface)
					return;
				this.RefreshSystemInterface();
			}
		}

		public StarMap StarMap
		{
			get
			{
				return this._starmap;
			}
		}

		public StarMapSystem ContextSystem
		{
			get
			{
				return this._contextsystem;
			}
			set
			{
			}
		}

		public bool RightClickEnabled
		{
			get
			{
				return this._rightClickEnabled;
			}
			set
			{
				if (this._rightClickEnabled == value)
					return;
				this._rightClickEnabled = value;
			}
		}

		private string EnteredColonyName
		{
			get
			{
				return this._enteredColonyName;
			}
		}

		private int SelectedPlanet
		{
			get
			{
				return this._selectedPlanet;
			}
			set
			{
				this._selectedPlanet = value;
			}
		}

		private int ColonyEstablishedPlanet
		{
			get
			{
				return this._colonyEstablishedPlanet;
			}
			set
			{
				this._colonyEstablishedPlanet = value;
			}
		}

		private int ColonyEstablishedSystem
		{
			get
			{
				return this._colonyEstablishedSystem;
			}
			set
			{
				this._colonyEstablishedSystem = value;
			}
		}

		public void ShowOverlay(MissionType missionType, int targetSystem)
		{
			this._missionOverlays.Values.FirstOrDefault<OverlayMission>((Func<OverlayMission, bool>)(x => x.MissionType == missionType))?.Show(targetSystem);
		}

		public void ShowColonizePlanetOverlay(int targetSystem, int targetPlanet)
		{
			OverlayColonizeMission overlayColonizeMission = (OverlayColonizeMission)this._missionOverlays.Values.FirstOrDefault<OverlayMission>((Func<OverlayMission, bool>)(x => x.MissionType == MissionType.COLONIZATION));
			if (overlayColonizeMission == null)
				return;
			overlayColonizeMission.SetSelectedPlanet(targetPlanet);
			overlayColonizeMission.Show(targetSystem);
		}

		public void ShowFleetCentricOverlay(MissionType missionType, int fleetid)
		{
			this._missionOverlays.Values.FirstOrDefault<OverlayMission>((Func<OverlayMission, bool>)(x => x.MissionType == missionType))?.ShowFleetCentric(fleetid);
		}

		public void ShowReactionOverlay(int targetsystem)
		{
			this._reactionOverlay.Show(targetsystem);
		}

		public bool OverlayActive()
		{
			foreach (OverlayMission overlayMission in this._missionOverlays.Values)
			{
				if (overlayMission.GetShown())
					return true;
			}
			return this._reactionOverlay.GetShown();
		}

		public T GetOverlay<T>() where T : OverlayMission
		{
			return this._missionOverlays.Values.FirstOrDefault<OverlayMission>((Func<OverlayMission, bool>)(x => x is T)) as T;
		}

		internal void SyncFreighterInterface()
		{
			this.App.UI.ClearSelection("freighterList");
			List<FreighterInfo> list1 = this.App.GameDatabase.GetFreighterInfosForSystem(this.SelectedSystem).Where<FreighterInfo>((Func<FreighterInfo, bool>)(x =>
		   {
			   if (x.PlayerId == this.App.LocalPlayer.ID)
				   return x.IsPlayerBuilt;
			   return false;
		   })).ToList<FreighterInfo>();
			List<BuildOrderInfo> list2 = this.App.GameDatabase.GetBuildOrdersForSystem(this.SelectedSystem).Where<BuildOrderInfo>((Func<BuildOrderInfo, bool>)(x =>
		   {
			   if (this.App.GameDatabase.GetDesignInfo(x.DesignID).PlayerID == this.App.LocalPlayer.ID)
				   return ((IEnumerable<DesignSectionInfo>)this.App.GameDatabase.GetDesignInfo(x.DesignID).DesignSections).Any<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(j => j.ShipSectionAsset.FreighterSpace > 0));
			   return false;
		   })).ToList<BuildOrderInfo>();
			this.App.UI.ClearItems("freighterList");
			foreach (FreighterInfo freighterInfo in list1)
			{
				this.App.UI.AddItem("freighterList", "", freighterInfo.ShipId, "");
				string itemGlobalId = this.App.UI.GetItemGlobalID("freighterList", "", freighterInfo.ShipId, "");
				this.App.UI.SetText(this.App.UI.Path(itemGlobalId, "designName"), freighterInfo.Design.Name);
				this.App.UI.SetVisible(this.App.UI.Path(itemGlobalId, "designDeleteButton"), true);
				this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "designDeleteButton"), "id", "freighterdeletebtn|" + freighterInfo.ShipId.ToString());
				this.App.UI.SetItemPropertyColor("freighterList", string.Empty, freighterInfo.ShipId, "designName", "color", new Vector3(11f, 157f, 194f));
			}
			int userItemId = 1000000;
			foreach (BuildOrderInfo buildOrderInfo in list2)
			{
				this.App.UI.AddItem("freighterList", "", userItemId, "");
				string itemGlobalId = this.App.UI.GetItemGlobalID("freighterList", "", userItemId, "");
				this.App.UI.SetText(this.App.UI.Path(itemGlobalId, "designName"), this.App.GameDatabase.GetDesignInfo(buildOrderInfo.DesignID).Name);
				this.App.UI.SetVisible(this.App.UI.Path(itemGlobalId, "designDeleteButton"), false);
				this.App.UI.SetItemPropertyColor("freighterList", string.Empty, userItemId, "designName", "color", new Vector3(200f, 150f, 40f));
				++userItemId;
			}
		}

		internal void RefreshSystemInterface()
		{
			this.OnFleetWidgetFleetSelected(this.App, 0);
			if (!this._showInterface)
				return;
			this.App.UI.ClearSelection("freighterList");
			if (this._lastFilterSelection == StarMapViewFilter.VF_TRADE)
			{
				this.SyncFreighterInterface();
				this.App.UI.SetVisible("fleetAndPlanetDetailsWidget", false);
				this.App.UI.SetVisible("colonyDetailsWidget", false);
				this.App.UI.SetVisible("planetDetailsWidget", false);
				this.App.UI.SetVisible("systemDetailsWidget", false);
				IEnumerable<ColonyInfo> source = this.App.GameDatabase.GetColonyInfosForSystem(this.SelectedSystem).Where<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == this.App.LocalPlayer.ID));
				if (source.Count<ColonyInfo>() > 0)
				{
					this.App.UI.SetVisible("tradePopup", true);
					this.App.UI.ClearItems("tradePlanetList");
					foreach (ColonyInfo colonyInfo in source)
					{
						OrbitalObjectInfo orbitalObjectInfo = this.App.GameDatabase.GetOrbitalObjectInfo(colonyInfo.OrbitalObjectID);
						if (orbitalObjectInfo != null)
						{
							this.App.UI.AddItem("tradePlanetList", "", colonyInfo.ID, "");
							string itemGlobalId = this.App.UI.GetItemGlobalID("tradePlanetList", "", colonyInfo.ID, "");
							FleetUI.SyncExistingPlanet(this.App.Game, this.App.UI.Path(itemGlobalId, "planetCard"), orbitalObjectInfo);
							this.App.UI.SetSliderValue(this.App.UI.Path(itemGlobalId, "partTradeSlider"), (int)((double)colonyInfo.TradeRate * 100.0));
							this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "partTradeSlider"), "id", "meTradeSlider|" + colonyInfo.OrbitalObjectID.ToString());
							this.App.UI.ClearSliderNotches(this.App.UI.Path(itemGlobalId, "meTradeSlider|" + colonyInfo.OrbitalObjectID.ToString()));
							foreach (double num in this.App.Game.GetTradeRatesForWholeExportsForColony(colonyInfo.ID))
								this.App.UI.AddSliderNotch(this.App.UI.Path(itemGlobalId, "meTradeSlider|" + colonyInfo.OrbitalObjectID.ToString()), (int)(num * 100.0));
							this.App.UI.ForceLayout("tradePopup");
							this.App.UI.ForceLayout("tradePopup");
						}
					}
				}
				else
					this.App.UI.SetVisible("tradePopup", false);
			}
			else
			{
				if (this.SelectedFleet != 0)
				{
					this.App.UI.SetVisible("colonyDetailsWidget", false);
					this.App.UI.SetVisible("planetDetailsWidget", false);
					this.App.UI.SetVisible("systemDetailsWidget", false);
					this.App.UI.SetEnabled("planetsTab", false);
					this.App.UI.SetVisible("partSystemPlanets", false);
					this.App.UI.SetChecked("planetsTab", false);
					this.App.UI.SetChecked("fleetsTab", true);
					this.App.UI.SetVisible("partSystemFleets", true);
					this.App.UI.SetVisible("fleetAndPlanetDetailsWidget", true);
					this._fleetWidget.ListStations = false;
					this._fleetWidget.SetSyncedStations(new List<StationInfo>());
					this._fleetWidget.SetSyncedFleets(this.SelectedFleet);
				}
				else
				{
					StarSystemUI.SyncSystemDetailsWidget(this.App, "systemDetailsWidget", this.SelectedSystem, true, true);
					StarSystemUI.SyncPlanetDetailsWidget(this.App.Game, "planetDetailsWidget", this.SelectedSystem, this.SelectedPlanet, this.GetPlanetViewGameObject(this.SelectedSystem, this.SelectedPlanet), this._planetView);
					StarSystemUI.SyncColonyDetailsWidget(this.App.Game, "colonyDetailsWidget", this.SelectedPlanet, "");
					FleetUI.SyncFleetAndPlanetListWidget(this.App.Game, "fleetAndPlanetDetailsWidget", this.SelectedSystem, true);
					this.App.UI.SetVisible("planetDetailsWidget", true);
					if (this.SelectedSystem != 0 && this.IsSystemVisible(this.SelectedSystem))
					{
						this._fleetWidget.SetSyncedFleets(this.App.GameDatabase.GetFleetInfoBySystemID(this.SelectedSystem, FleetType.FL_ALL).Where<FleetInfo>((Func<FleetInfo, bool>)(x =>
					   {
						   if (x.Type == FleetType.FL_RESERVE)
							   return x.PlayerID == this.App.LocalPlayer.ID;
						   return true;
					   })).ToList<FleetInfo>());
						this._fleetWidget.ListStations = true;
						this._fleetWidget.SetSyncedStations(this.App.GameDatabase.GetStationForSystemAndPlayer(this.SelectedSystem, this.App.LocalPlayer.ID).ToList<StationInfo>());
					}
					else
					{
						this._fleetWidget.SetSyncedFleets(new List<int>());
						this._fleetWidget.ListStations = true;
						this._fleetWidget.SetSyncedStations(new List<int>());
					}
				}
				this.App.UI.SetVisible("tradePopup", false);
			}
			this.App.UI.SetVisible("gameComparativeAnalysysButton", this.App.GameDatabase.GetStratModifier<bool>(StratModifiers.ComparativeAnalysys, this.App.LocalPlayer.ID));
			bool propertyValue = this.App.GameDatabase.GetStratModifier<bool>(StratModifiers.ComparativeAnalysys, this.App.LocalPlayer.ID) && this.App.GameDatabase.GetDesignsEncountered(this.App.LocalPlayer.ID).Any<DesignInfo>((Func<DesignInfo, bool>)(x => x.Class != ShipClass.Station));
			this.App.UI.SetEnabled("gameComparativeAnalysysButton", propertyValue);
			this.App.UI.SetPropertyBool("gameComparativeAnalysysButton", "lockout_button", propertyValue);
			this.App.UI.SetVisible("gameBattleRiderManagerButton", this.App.GameDatabase.PlayerHasTech(this.App.LocalPlayer.ID, "BRD_BattleRiders"));
			if (this.SelectedSystem == 0)
				return;
			this._fleetWidget.ListStations = this.SelectedFleet == 0;
			List<StationInfo> stations = new List<StationInfo>();
			if (this._fleetWidget.ListStations)
				stations = this.App.GameDatabase.GetStationForSystemAndPlayer(this.SelectedSystem, this.App.LocalPlayer.ID).ToList<StationInfo>();
			this._fleetWidget.SetSyncedStations(stations);
		}

		private bool IsSystemVisible(int systemId)
		{
			bool flag1 = this.App.GameDatabase.IsSurveyed(this.App.LocalPlayer.ID, systemId);
			List<FleetInfo> list1 = this.App.GameDatabase.GetFleetInfoBySystemID(systemId, FleetType.FL_NORMAL | FleetType.FL_RESERVE | FleetType.FL_GATE).ToList<FleetInfo>();
			List<StationInfo> list2 = this.App.GameDatabase.GetStationForSystemAndPlayer(systemId, this.App.LocalPlayer.ID).ToList<StationInfo>();
			bool flag2 = true;
			if (!this.App.Game.SystemHasPlayerColony(systemId, this.App.LocalPlayer.ID) && !StarMap.IsInRange(this.App.GameDatabase, this.App.LocalPlayer.ID, this.App.GameDatabase.GetStarSystemInfo(systemId), (Dictionary<int, List<ShipInfo>>)null))
				flag2 = false;
			return (list1.Count != 0 || list2.Count != 0 || flag1) && (flag2 || flag1);
		}

		private int SelectedSystem
		{
			get
			{
				StarSystemInfo selectedObject = this.SelectedObject as StarSystemInfo;
				if (!(selectedObject != (StarSystemInfo)null))
					return 0;
				return selectedObject.ID;
			}
		}

		public int GetSelectedSystem()
		{
			StarSystemInfo selectedObject = this.SelectedObject as StarSystemInfo;
			if (!(selectedObject != (StarSystemInfo)null))
				return 0;
			return selectedObject.ID;
		}

		public void SetSelectedSystem(int systemId, bool fleetTab = false)
		{
			if (!this._starmap.Systems.Reverse.ContainsKey(systemId))
				return;
			StarMapSystem starMapSystem = this._starmap.Systems.Reverse[systemId];
			this.SelectObject((IGameObject)starMapSystem);
			this.StarMap.SetFocus((IGameObject)starMapSystem);
			StarSystemUI.SyncColonyDetailsWidget(this.App.Game, "colonyDetailsWidget", this._selectedPlanet, "");
			if (!fleetTab)
				return;
			this.App.UI.SetVisible("partSystemPlanets", !FleetUI.ShowFleetListDefault);
			this.App.UI.SetChecked("planetsTab", !FleetUI.ShowFleetListDefault);
			this.App.UI.SetChecked("fleetsTab", FleetUI.ShowFleetListDefault);
			this.App.UI.SetVisible("partSystemFleets", FleetUI.ShowFleetListDefault);
		}

		public void SetSelectedFleet(int fleetId)
		{
			if (!this._starmap.Fleets.Reverse.ContainsKey(fleetId))
				return;
			StarMapFleet starMapFleet = this._starmap.Fleets.Reverse[fleetId];
			this.StarMap.SetFocus((IGameObject)starMapFleet);
			this.StarMap.Select((IGameObject)starMapFleet);
		}

		private int SelectedFleet
		{
			get
			{
				FleetInfo selectedObject = this.SelectedObject as FleetInfo;
				if (selectedObject == null)
					return 0;
				return selectedObject.ID;
			}
		}

		private object SelectedObject
		{
			get
			{
				return this.App.Game.StarMapSelectedObject;
			}
			set
			{
				if (this.App.Game.StarMapSelectedObject == value)
					return;
				this._selectedPlanet = 0;
				this.App.Game.StarMapSelectedObject = value;
				if (this.OnObjectSelectionChanged != null)
				{
					StarSystemInfo selectedObject = this.SelectedObject as StarSystemInfo;
					this.OnObjectSelectionChanged(this.App, selectedObject != (StarSystemInfo)null ? selectedObject.ID : 0);
				}
				this.RefreshSystemInterface();
			}
		}

		public static bool Load(App game)
		{
			return game.SwitchGameStateViaLoadingScreen((Action)null, (LoadingFinishedDelegate)null, (GameState)game.GetGameState<StarMapState>(), (object[])null);
		}

		public StarMapState(App game)
		  : base(game)
		{
		}

		public void ClearStarmapFleetArrows()
		{
			this._painter.ClearSections();
		}

		public void AddMissionFleetArrow(Vector3 Origin, Vector3 Target, Vector3 Color)
		{
			this._painter.AddSection(new List<Vector3>()
	  {
		Origin,
		Target
	  }, APStyle.FLEET_ARROW, 0, new Vector3?(Color));
		}

		public void AddMissionFleetArrow(List<Vector3> path, Vector3 Color)
		{
			this._painter.AddSection(path, APStyle.FLEET_ARROW, 0, new Vector3?(Color));
		}

		protected void SyncFleetArrows()
		{
			this._painter.ClearSections();
			bool flag1 = this.App.GameDatabase.PlayerHasTech(this.App.LocalPlayer.ID, "CCC_Node_Tracking:_Zuul");
			bool flag2 = this.App.GameDatabase.PlayerHasTech(this.App.LocalPlayer.ID, "CCC_Node_Tracking:_Human");
			IEnumerable<NodeLineInfo> nodeLines = this.App.GameDatabase.GetNodeLines();
			IEnumerable<NodeLineInfo> exploredNodeLines = this.App.GameDatabase.GetExploredNodeLines(this.App.LocalPlayer.ID);
			foreach (NodeLineInfo nodeLineInfo in nodeLines)
			{
				NodeLineInfo nodeLine = nodeLineInfo;
				if (!nodeLine.IsLoaLine)
				{
					bool flag3 = false;
					if (this.App.LocalPlayer.Faction.Name != "zuul" && flag1 || this.App.LocalPlayer.Faction.Name != "human" && flag2)
					{
						foreach (MoveOrderInfo ordersBetweenSystem in this.App.Game.GetMoveOrdersBetweenSystems(nodeLine.System1ID, nodeLine.System2ID))
						{
							FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(ordersBetweenSystem.FleetID);
							if (fleetInfo.PlayerID != this.App.LocalPlayer.ID && StarMap.IsInRange(this.App.Game.GameDatabase, this.App.LocalPlayer.ID, this.App.GameDatabase.GetFleetLocation(fleetInfo.ID, false).Coords, 1f, (Dictionary<int, List<ShipInfo>>)null))
							{
								flag3 = true;
								break;
							}
						}
					}
					StarSystemInfo starSystemInfo1 = this.App.GameDatabase.GetStarSystemInfo(nodeLine.System1ID);
					StarSystemInfo starSystemInfo2 = this.App.GameDatabase.GetStarSystemInfo(nodeLine.System2ID);
					if (!nodeLine.IsPermenant)
					{
						if (flag3 && flag1 || this.App.LocalPlayer.Faction.Name == "zuul")
							this._painter.AddSection(new List<Vector3>()
			  {
				starSystemInfo1.Origin,
				starSystemInfo2.Origin
			  }, APStyle.BORE_LINE, nodeLine.Health, new Vector3?());
					}
					else if (flag3 && flag2 || this.App.LocalPlayer.Faction.Name == "human" && exploredNodeLines.Any<NodeLineInfo>((Func<NodeLineInfo, bool>)(x =>
				   {
					   if (x.ID == nodeLine.ID)
						   return x.Health == -1;
					   return false;
				   })))
						this._painter.AddSection(new List<Vector3>()
			{
			  starSystemInfo1.Origin,
			  starSystemInfo2.Origin
			}, APStyle.NODE_LINE, nodeLine.Health, new Vector3?());
				}
			}
			foreach (FleetInfo fi in this.App.GameDatabase.GetFleetInfosByPlayerID(this.App.LocalPlayer.ID, FleetType.FL_NORMAL | FleetType.FL_CARAVAN))
			{
				MissionInfo missionByFleetId = this.App.GameDatabase.GetMissionByFleetID(fi.ID);
				if (missionByFleetId != null && (missionByFleetId.Type != MissionType.PIRACY || this.App.GameDatabase.PirateFleetVisibleToPlayer(fi.ID, this.App.LocalPlayer.ID)))
				{
					FleetLocation fleetLocation = this.App.GameDatabase.GetFleetLocation(fi.ID, true);
					if (GameSession.FleetHasBore(this.App.GameDatabase, fi.ID) && this.App.LocalPlayer.Faction.CanUseNodeLine(new bool?(false)))
					{
						List<Vector3> path = new List<Vector3>();
						MoveOrderInfo orderInfoByFleetId = this.App.GameDatabase.GetMoveOrderInfoByFleetID(fi.ID);
						if (orderInfoByFleetId != null && orderInfoByFleetId.FromSystemID != 0 && (orderInfoByFleetId.ToSystemID != 0 && !GameSession.SystemsLinkedByNonPermenantNodes(this.App.Game, orderInfoByFleetId.FromSystemID, orderInfoByFleetId.ToSystemID)) && !this.App.GameDatabase.GetStarSystemInfo(orderInfoByFleetId.FromSystemID).IsDeepSpace)
						{
							path.Add(this.App.GameDatabase.GetStarSystemOrigin(orderInfoByFleetId.FromSystemID));
							path.Add(fleetLocation.Coords);
							this._painter.AddSection(path, APStyle.BORE_LINE, 1000, new Vector3?());
						}
					}
					this.App.GameDatabase.GetNodeLines();
					WaypointInfo waypointForMission = this.App.GameDatabase.GetNextWaypointForMission(missionByFleetId.ID);
					if (waypointForMission != null)
					{
						PlayerInfo playerInfo = this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID);
						if ((missionByFleetId.Type == MissionType.INTERCEPT || missionByFleetId.Type == MissionType.SPECIAL_CONSTRUCT_STN) && (waypointForMission.Type == WaypointType.DoMission && missionByFleetId.TargetFleetID != 0))
						{
							List<Vector3> path = new List<Vector3>();
							path.Add(this.App.GameDatabase.GetFleetLocation(fi.ID, true).Coords);
							path.Add(this.App.GameDatabase.GetFleetLocation(missionByFleetId.TargetFleetID, true).Coords);
							if (StarMap.IsInRange(this.App.GameDatabase, fi.PlayerID, path[1], 1f, (Dictionary<int, List<ShipInfo>>)null))
								this._painter.AddSection(path, APStyle.FLEET_ARROW, 0, new Vector3?(playerInfo.PrimaryColor));
						}
						else if (waypointForMission.Type == WaypointType.TravelTo || waypointForMission.Type == WaypointType.ReturnHome)
						{
							List<Vector3> path = new List<Vector3>();
							int toSystem = waypointForMission.SystemID.HasValue ? waypointForMission.SystemID.Value : this.App.Game.GameDatabase.GetHomeSystem(this.App.Game, waypointForMission.MissionID, fi);
							int fromSystem;
							if (fi.SystemID == 0)
							{
								MoveOrderInfo orderInfoByFleetId = this.App.GameDatabase.GetMoveOrderInfoByFleetID(fi.ID);
								path.Add(fleetLocation.Coords);
								if (orderInfoByFleetId.ToSystemID == 0)
									path.Add(orderInfoByFleetId.ToCoords);
								else
									path.Add(this.App.GameDatabase.GetStarSystemOrigin(orderInfoByFleetId.ToSystemID));
								fromSystem = orderInfoByFleetId.ToSystemID;
							}
							else
								fromSystem = fi.SystemID;
							if (fromSystem != toSystem && fromSystem != 0)
							{
								int tripTime;
								float tripDistance;
								foreach (int systemId in Kerberos.Sots.StarFleet.StarFleet.GetBestTravelPath(this.App.Game, fi.ID, fromSystem, toSystem, out tripTime, out tripDistance, missionByFleetId.UseDirectRoute, new float?(), new float?()))
									path.Add(this.App.GameDatabase.GetStarSystemOrigin(systemId));
							}
							this._painter.AddSection(path, APStyle.FLEET_ARROW, 0, new Vector3?(playerInfo.PrimaryColor));
						}
					}
				}
			}
			foreach (FleetInfo fleetInfo in (IEnumerable<FleetInfo>)this.App.GameDatabase.GetFleetInfos(FleetType.FL_NORMAL).ToList<FleetInfo>().Where<FleetInfo>((Func<FleetInfo, bool>)(x =>
		  {
			  if (x.PlayerID != this.App.LocalPlayer.ID)
				  return StarMap.IsInRange(this.App.GameDatabase, this.App.LocalPlayer.ID, this.App.GameDatabase.GetFleetLocation(x.ID, false).Coords, 1f, (Dictionary<int, List<ShipInfo>>)null);
			  return false;
		  })).ToList<FleetInfo>())
			{
				MissionInfo missionByFleetId = this.App.GameDatabase.GetMissionByFleetID(fleetInfo.ID);
				if (missionByFleetId == null || missionByFleetId.Type != MissionType.PIRACY || this.App.GameDatabase.PirateFleetVisibleToPlayer(fleetInfo.ID, this.App.LocalPlayer.ID))
				{
					MoveOrderInfo orderInfoByFleetId = this.App.GameDatabase.GetMoveOrderInfoByFleetID(fleetInfo.ID);
					PlayerInfo playerInfo = this.App.GameDatabase.GetPlayerInfo(fleetInfo.PlayerID);
					if (orderInfoByFleetId != null)
					{
						if (orderInfoByFleetId.ToSystemID != 0)
						{
							if (StarMap.IsInRange(this.App.GameDatabase, this.App.LocalPlayer.ID, orderInfoByFleetId.ToSystemID))
								this._painter.AddSection(new List<Vector3>()
				{
				  this.App.GameDatabase.GetFleetLocation(fleetInfo.ID, true).Coords,
				  this.App.GameDatabase.GetStarSystemOrigin(orderInfoByFleetId.ToSystemID)
				}, APStyle.FLEET_ARROW, 0, new Vector3?(playerInfo.PrimaryColor));
						}
						else if (StarMap.IsInRange(this.App.GameDatabase, this.App.LocalPlayer.ID, orderInfoByFleetId.ToCoords, 1f, (Dictionary<int, List<ShipInfo>>)null))
							this._painter.AddSection(new List<Vector3>()
			  {
				this.App.GameDatabase.GetFleetLocation(fleetInfo.ID, true).Coords,
				orderInfoByFleetId.ToCoords
			  }, APStyle.FLEET_ARROW, 0, new Vector3?(playerInfo.PrimaryColor));
					}
				}
			}
		}

		public void RefreshCameraControl()
		{
			this._starmap.SetCamera(this.App.Game.StarMapCamera);
		}

		protected override void OnPrepare(GameState prev, object[] parms)
		{
			if (prev == this.App.GetGameState<LoadingScreenState>())
				prev = this.App.GetGameState<LoadingScreenState>().PreviousState;
			if (this._initialized && (prev == this.App.GetGameState<MainMenuState>() || prev == this.App.GetGameState<StarMapLobbyState>() || prev == null))
				this.Reset();
			this._simNewTurnTick = 200;
			if (this._initialized)
				return;
			if (this.App.GameDatabase == null)
				this.App.NewGame();
			this._crits = new GameObjectSet(this.App);
			this._sky = new Sky(this.App, SkyUsage.StarMap, 0);
			this._crits.Add((IGameObject)this._sky);
			this._planetView = this._crits.Add<PlanetView>();
			this._starmap = new StarMap(this.App, this.App.Game, this._sky);
			this._starmap.SetCamera(this.App.Game.StarMapCamera);
			this._contextMenuID = this.App.UI.CreatePanelFromTemplate("StarMapContextMenu", null);
			this._researchContextID = this.App.UI.CreatePanelFromTemplate("ResearchContextMenu", null);
			this._fleetContextMenuID = this.App.UI.CreatePanelFromTemplate("StarMapFleetContextMenu", null);
			this._enemyContextMenuID = this.App.UI.CreatePanelFromTemplate("StarMapEnemyContextMenu", null);
			this._enemyGMStationContextMenuID = this.App.UI.CreatePanelFromTemplate("StarMapGMStationTargetContextMenu", null);
			this._painter = new ArrowPainter(this.App);
			this._contextsystem = (StarMapSystem)null;
			this.SyncFleetArrows();
			this.App.UI.LoadScreen("StarMap");
			this._starmap.Initialize(this._crits);
			this._crits.Add((IGameObject)this._starmap);
			this._dialogState = ESMDialogState.ESMD_None;
			this._fleetWidget = new FleetWidget(this.App, "StarMap.partSystemFleets");
			this._fleetWidget.EnableCreateFleetButton = true;
			this._fleetWidget.ScrapEnabled = true;
			this._fleetWidget.ShipSelectionEnabled = false;
			this._fleetWidget.SeparateDefenseFleet = false;
			this._fleetWidget.EnableMissionButtons = true;
			this._fleetWidget.OnFleetSelectionChanged += new FleetWidget.FleetSelectionChangedDelegate(this.OnFleetWidgetFleetSelected);
			this._playerWidget = new PlayerWidget(this.App, this.App.UI, "playerDropdown");
			this._piechart = new BudgetPiechart(this.App.UI, "piechart", this.App.AssetDatabase);
			this._planetWidget = new PlanetWidget(this.App, "systemDetailsWidget");
			this._techCube = new TechCube(this.App);
			this._crits.Add((IGameObject)this._techCube);
			this._missionOverlays = new Dictionary<string, OverlayMission>();
			this._missionOverlays["gameSurveyButton"] = (OverlayMission)new OverlaySurveyMission(this.App, this, this._starmap, "OverlaySurveyMission");
			this._missionOverlays["gameColonizeButton"] = (OverlayMission)new OverlayColonizeMission(this.App, this, this._starmap, "OverlayColonizeMission");
			this._missionOverlays["gameEvacuateButton"] = (OverlayMission)new OverlayEvacuationMission(this.App, this, this._starmap, "OverlayEvacuationMission");
			this._missionOverlays["gameStrikeButton"] = (OverlayMission)new OverlayStrikeMission(this.App, this, this._starmap, "OverlaySurveyMission");
			this._missionOverlays["gameRelocateButton"] = (OverlayMission)new OverlayRelocateMission(this.App, this, this._starmap, "OverlayRelocateMission");
			this._missionOverlays["gameGateButton"] = (OverlayMission)new OverlayGateMission(this.App, this, this._starmap, "OverlaySurveyMission");
			this._missionOverlays["gamePatrolButton"] = (OverlayMission)new OverlayPatrolMission(this.App, this, this._starmap, "OverlaySurveyMission");
			this._missionOverlays["gameInterdictButton"] = (OverlayMission)new OverlayInterdictMission(this.App, this, this._starmap, "OverlaySurveyMission");
			this._missionOverlays["fleetInterceptButton"] = (OverlayMission)new OverlayInterceptMission(this.App, this, this._starmap, "OverlayInterceptMission");
			this._missionOverlays["fleetBuildStationButton"] = (OverlayMission)new OverlaySpecialConstructionMission(this.App, this, this._starmap, (SpecialConstructionMission)null, "OverlayGMBuildStationMission");
			this._missionOverlays["gameInvadeButton"] = (OverlayMission)new OverlayInvasionMission(this.App, this, this._starmap, "OverlayColonizeMission");
			this._missionOverlays["gameSupportButton"] = (OverlayMission)new OverlaySupportMission(this.App, this, this._starmap, "OverlaySupportMission");
			this._missionOverlays["gameConstructStationButton"] = (OverlayMission)new OverlayConstructionMission(this.App, this, this._starmap, (SpecialConstructionMission)null, "OverlayStationMission", MissionType.CONSTRUCT_STN);
			this._missionOverlays["gameUpgradeStationButton"] = (OverlayMission)new OverlayUpgradeMission(this.App, this, this._starmap);
			this._missionOverlays["gamePiracyButton"] = (OverlayMission)new OverlayPiracyMission(this.App, this, this._starmap, "OverlaySurveyMission");
			this._missionOverlays["gameNPGButton"] = (OverlayMission)new OverlayDeployAccelMission(this.App, this, this._starmap, "OverlayAcceleratorMission");
			this._reactionOverlay = (OverlayMission)new OverlayReactionlMission(this.App, this, this._starmap, "OverlayReaction");
			this._initialized = true;
		}

		public void OnFleetWidgetFleetSelected(App game, int selectedFleet)
		{
			if (this._lastSelectedFleet == selectedFleet)
				return;
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(selectedFleet);
			if (fleetInfo != null && fleetInfo.PlayerID == game.LocalPlayer.ID && fleetInfo.Type == FleetType.FL_NORMAL)
			{
				List<int> list = Kerberos.Sots.StarFleet.StarFleet.CollectAvailableSystemsForFleetMission(this.App.GameDatabase, this.App.Game, fleetInfo.ID, MissionType.SURVEY, false).ToList<int>();
				foreach (int key in this._starmap.Systems.Reverse.Keys)
					this._starmap.Systems.Reverse[key].SetIsEnabled(list.Contains(key));
			}
			else
			{
				foreach (int key in this._starmap.Systems.Reverse.Keys)
					this._starmap.Systems.Reverse[key].SetIsEnabled(true);
			}
			this._lastSelectedFleet = selectedFleet;
		}

		private void UIHandleCoreEventBehaviour(string eventName, string[] eventParams)
		{
			if (eventName == "ObjectClicked")
			{
				this.ProcessGameEvent_ObjectClicked(eventParams);
			}
			else
			{
				if (!(eventName == "MouseOver"))
					return;
				this.ProcessGameEvent_MouseOver(eventParams);
			}
		}

		private void UICommChannel_GameEvent(string eventName, string[] eventParams)
		{
			this.UIHandleCoreEventBehaviour(eventName, eventParams);
			if (!this._uiEnabled)
				return;
			this._piechart.TryGameEvent(eventName, eventParams);
			if (!(eventName == "ContextMenu") || !this.RightClickEnabled)
				return;
			this.ProcessGameEvent_ContextMenu(eventParams);
		}

		private void SetSelectedPlanet(int value, string trigger)
		{
			if (this._selectedPlanet == value)
				return;
			this._selectedPlanet = value;
			StarSystemUI.SyncPlanetDetailsWidget(this.App.Game, "planetDetailsWidget", this.SelectedSystem, this._selectedPlanet, this.GetPlanetViewGameObject(this.SelectedSystem, this._selectedPlanet), this._planetView);
			this._planetWidget.Sync(value, false, false);
			if (!this._uiEnabled)
				return;
			StarSystemUI.SyncColonyDetailsWidget(this.App.Game, "colonyDetailsWidget", this._selectedPlanet, "");
		}

		public GameObjectSet GetCrits()
		{
			return this._crits;
		}

		public void SetProvinceMode(bool isProvinceMode)
		{
			this._isProvinceMode = isProvinceMode;
			if (this._isProvinceMode)
			{
				this.App.UI.SetChecked("gameProvinceModeButton", false);
				this._mode = (StarMapStateMode)new ProvinceEditStarMapStateMode(this.App.Game, this, this._starmap);
				this._mode.Initialize();
			}
			else
			{
				this.App.UI.SetChecked("gameProvinceModeButton", true);
				if (this._mode == null)
					return;
				this._mode.Terminate();
				this._mode = (StarMapStateMode)null;
			}
		}

		private object EngineObjectToDatabaseObject(IGameObject obj)
		{
			int systemId;
			if (obj is StarMapSystem && this._starmap.Systems.Forward.TryGetValue((StarMapSystem)obj, out systemId))
				return (object)this.App.GameDatabase.GetStarSystemInfo(systemId);
			int fleetID;
			if (obj is StarMapFleet && this._starmap.Fleets.Forward.TryGetValue((StarMapFleet)obj, out fleetID))
				return (object)this.App.GameDatabase.GetFleetInfo(fleetID);
			return (object)null;
		}

		private FleetInfo EngineObjectToSystem(IGameObject obj)
		{
			return (FleetInfo)null;
		}

		private void SelectObject(IGameObject o)
		{
			this.SelectedObject = this.EngineObjectToDatabaseObject(o);
			if (this.SelectedSystemHasFriendlyColonyScreen())
				this.App.PostRequestGuiSound("starmap_selectcolonysystem");
			else
				this.App.PostRequestGuiSound("starmap_selectsystem");
			if (!this.App.GameSettings.SeperateStarMapFocus)
				this._starmap.SetFocus(o);
			if (!this._uiEnabled)
				return;
			this.App.UI.SetEnabled("gameRepairButton", this.CanOpenRepairScreen());
			this.App.UI.SetEnabled("gameBuildButton", this.CanOpenBuildScreen());
			this.App.UI.SetEnabled("gameFleetManagerButton", this.CanOpenFleetManager(0));
			this.App.UI.SetEnabled("gameBattleRiderManagerButton", this.CanOpenRiderManager());
		}

		private void ProcessGameEvent_ContextMenu(string[] eventParams)
		{
			if (!this._uiEnabled)
				return;
			int id1 = int.Parse(eventParams[0]);
			if (id1 == 0)
				return;
			IGameObject gameObject = this.App.GetGameObject(id1);
			if (gameObject is StarMapFleet)
			{
				StarMapFleet fleet = (StarMapFleet)gameObject;
				if (fleet.PlayerID != this.App.LocalPlayer.ID)
				{
					bool flag1 = this.App.Game.ScriptModules.NeutronStar != null && this.App.Game.ScriptModules.NeutronStar.PlayerID == fleet.PlayerID;
					bool flag2 = this.App.Game.ScriptModules.Gardeners != null && this.App.Game.ScriptModules.Gardeners.PlayerID == fleet.PlayerID;
					if (flag1 || flag2)
					{
						int systemID = 0;
						if (flag1)
						{
							NeutronStarInfo neutronStarInfo = this.App.GameDatabase.GetNeutronStarInfos().FirstOrDefault<NeutronStarInfo>((Func<NeutronStarInfo, bool>)(x => x.FleetId == fleet.FleetID));
							if (neutronStarInfo != null && neutronStarInfo.DeepSpaceSystemId.HasValue)
								systemID = neutronStarInfo.DeepSpaceSystemId.Value;
						}
						else
						{
							GardenerInfo gardenerInfo = this.App.GameDatabase.GetGardenerInfos().FirstOrDefault<GardenerInfo>((Func<GardenerInfo, bool>)(x => x.FleetId == fleet.FleetID));
							if (gardenerInfo != null && gardenerInfo.DeepSpaceSystemId.HasValue)
								systemID = gardenerInfo.DeepSpaceSystemId.Value;
						}
						List<StationInfo> list = this.App.GameDatabase.GetStationForSystem(systemID).ToList<StationInfo>();
						if (list.Count == 0)
						{
							this._fleetContextFleet = fleet.FleetID;
							this.App.UI.AutoSize(this._enemyGMStationContextMenuID);
							FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(fleet.FleetID);
							this.App.UI.SetEnabled(this.App.UI.Path(this._enemyGMStationContextMenuID, "fleetBuildStationButton"), (this.App.AssetDatabase.GetFaction(this.App.GameDatabase.GetPlayerFactionID(fleet.PlayerID)).CanUseNodeLine(new bool?()) ? 0 : (fleetInfo.Type != FleetType.FL_CARAVAN ? 1 : 0)) != 0);
							this.App.UI.ShowTooltip(this._enemyGMStationContextMenuID, float.Parse(eventParams[1]), float.Parse(eventParams[2]));
							return;
						}
						if (list.Any<StationInfo>((Func<StationInfo, bool>)(x => x.PlayerID == this.App.LocalPlayer.ID)))
							return;
					}
					this._fleetContextFleet = fleet.FleetID;
					this.App.UI.AutoSize(this._enemyContextMenuID);
					FleetInfo fleetInfo1 = this.App.GameDatabase.GetFleetInfo(fleet.FleetID);
					Faction faction = this.App.AssetDatabase.GetFaction(this.App.GameDatabase.GetPlayerFactionID(fleet.PlayerID));
					this.App.UI.SetEnabled(this.App.UI.Path(this._enemyContextMenuID, "fleetInterceptButton"), (fleetInfo1.IsAcceleratorFleet || !faction.CanUseNodeLine(new bool?()) ? (fleetInfo1.Type != FleetType.FL_CARAVAN ? 1 : 0) : 0) != 0);
					this.App.UI.ShowTooltip(this._enemyContextMenuID, float.Parse(eventParams[1]), float.Parse(eventParams[2]));
				}
				else
				{
					this._fleetContextFleet = fleet.FleetID;
					this.App.UI.AutoSize(this._fleetContextMenuID);
					MissionInfo missionByFleetId = this.App.GameDatabase.GetMissionByFleetID(this._fleetContextFleet);
					this.App.UI.SetEnabled(this.App.UI.Path(this._fleetContextMenuID, "fleetCancelButton"), (missionByFleetId == null || missionByFleetId.Type == MissionType.RETURN || missionByFleetId.Type == MissionType.RETREAT ? 0 : (this.App.GameDatabase.GetFleetInfo(this._fleetContextFleet).Type != FleetType.FL_CARAVAN ? 1 : 0)) != 0);
					this.App.UI.ShowTooltip(this._fleetContextMenuID, float.Parse(eventParams[1]), float.Parse(eventParams[2]));
				}
			}
			else
			{
				if (!(gameObject is StarMapSystem))
					return;
				int num1 = this._starmap.Systems.Forward[(StarMapSystem)gameObject];
				this._contextMenuSystem = num1;
				StarSystemInfo starSystemInfo = this.App.GameDatabase.GetStarSystemInfo(num1);
				string panelId1 = this.App.UI.Path(this._contextMenuID, "gameSurveyButton");
				string panelId2 = this.App.UI.Path(this._contextMenuID, "gameColonizeButton");
				string panelId3 = this.App.UI.Path(this._contextMenuID, "gameEvacuateButton");
				string panelId4 = this.App.UI.Path(this._contextMenuID, "gameRelocateButton");
				string panelId5 = this.App.UI.Path(this._contextMenuID, "gamePatrolButton");
				string panelId6 = this.App.UI.Path(this._contextMenuID, "gameInterdictButton");
				string panelId7 = this.App.UI.Path(this._contextMenuID, "gameStrikeButton");
				string panelId8 = this.App.UI.Path(this._contextMenuID, "gameInvadeButton");
				string panelId9 = this.App.UI.Path(this._contextMenuID, "gameSupportButton");
				string panelId10 = this.App.UI.Path(this._contextMenuID, "gameConstructStationButton");
				string panelId11 = this.App.UI.Path(this._contextMenuID, "gameUpgradeStationButton");
				string panelId12 = this.App.UI.Path(this._contextMenuID, "gameContextFleetManagerButton");
				string panelId13 = this.App.UI.Path(this._contextMenuID, "gameDefenseManagerButton");
				string panelId14 = this.App.UI.Path(this._contextMenuID, "gameContextStationManagerButton");
				string panelId15 = this.App.UI.Path(this._contextMenuID, "gameGateButton");
				string panelId16 = this.App.UI.Path(this._contextMenuID, "gamePiracyButton");
				string panelId17 = this.App.UI.Path(this._contextMenuID, "gameNPGButton");
				bool flag1 = this.App.Game.SystemHasPlayerColony(num1, this.App.LocalPlayer.ID);
				bool flag2 = this.App.GameDatabase.GetNavalStationForSystemAndPlayer(num1, this.App.LocalPlayer.ID) != null;
				bool flag3 = Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(this.App.Game, this.App.LocalPlayer.ID, num1, MissionType.SURVEY, true).Any<FleetInfo>();
				bool flag4 = Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(this.App.Game, this.App.LocalPlayer.ID, num1, MissionType.COLONIZATION, true).Any<FleetInfo>() && StarSystemDetailsUI.CollectPlanetListItemsForColonizeMission(this.App, num1, this.App.LocalPlayer.ID).Count<int>() > 0;
				bool flag5 = this.App.GameDatabase.HasEndOfFleshExpansion() && Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(this.App.Game, this.App.LocalPlayer.ID, num1, MissionType.EVACUATE, true).Any<FleetInfo>() && StarSystemDetailsUI.CollectPlanetListItemsForEvacuateMission(this.App, num1, this.App.LocalPlayer.ID).Count<int>() > 0;
				bool flag6 = Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(this.App.Game, this.App.LocalPlayer.ID, num1, MissionType.RELOCATION, true).Any<FleetInfo>() || Kerberos.Sots.StarFleet.StarFleet.HasRelocatableDefResAssetsInRange(this.App.Game, num1);
				bool flag7 = Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(this.App.Game, this.App.LocalPlayer.ID, num1, MissionType.PATROL, true).Any<FleetInfo>();
				bool flag8 = Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(this.App.Game, this.App.LocalPlayer.ID, num1, MissionType.INTERDICTION, true).Any<FleetInfo>();
				bool flag9 = Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(this.App.Game, this.App.LocalPlayer.ID, num1, MissionType.STRIKE, true).Any<FleetInfo>();
				bool flag10 = Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(this.App.Game, this.App.LocalPlayer.ID, num1, MissionType.INVASION, true).Any<FleetInfo>();
				bool flag11 = Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(this.App.Game, this.App.LocalPlayer.ID, num1, MissionType.SUPPORT, true).Any<FleetInfo>();
				int num2;
				if (Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(this.App.Game, this.App.LocalPlayer.ID, num1, MissionType.CONSTRUCT_STN, true).Any<FleetInfo>() && StarSystem.GetSystemCanSupportStations(this.App.Game, num1, this.App.LocalPlayer.ID).Where<StationType>((Func<StationType, bool>)(j => j != StationType.DEFENCE)).Count<StationType>() > 0)
				{
					int? systemOwningPlayer = this.App.GameDatabase.GetSystemOwningPlayer(num1);
					if (systemOwningPlayer.HasValue)
					{
						systemOwningPlayer = this.App.GameDatabase.GetSystemOwningPlayer(num1);
						int id2 = this.App.LocalPlayer.ID;
						if ((systemOwningPlayer.GetValueOrDefault() != id2 ? 0 : (systemOwningPlayer.HasValue ? 1 : 0)) == 0)
						{
							num2 = StarMapState.SystemHasIndependentColony(this.App.Game, num1) ? 1 : 0;
							goto label_25;
						}
					}
					num2 = 1;
				}
				else
					num2 = 0;
				label_25:
				bool flag12 = num2 != 0;
				bool flag13 = Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(this.App.Game, this.App.LocalPlayer.ID, num1, MissionType.CONSTRUCT_STN, true).Any<FleetInfo>() && this.App.Game.GetUpgradableStations(this.App.GameDatabase.GetStationForSystemAndPlayer(num1, this.App.LocalPlayer.ID).ToList<StationInfo>()).Count > 0;
				bool flag14 = Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(this.App.Game, this.App.LocalPlayer.ID, num1, MissionType.GATE, true).Any<FleetInfo>();
				bool flag15 = this.App.GameDatabase.GetFleetInfoBySystemID(num1, FleetType.FL_NORMAL).Any<FleetInfo>() && flag1 || flag2;
				bool flag16 = this.App.GameDatabase.GetStationForSystemAndPlayer(num1, this.App.LocalPlayer.ID).Any<StationInfo>() && (flag1 || flag2);
				bool flag17 = GameSession.PlayerPresentInSystem(this.App.GameDatabase, this.App.LocalPlayer.ID, num1);
				bool flag18 = Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(this.App.Game, this.App.LocalPlayer.ID, num1, MissionType.PIRACY, true).Any<FleetInfo>() && !this.App.GameDatabase.GetMissionsBySystemDest(num1).Any<MissionInfo>((Func<MissionInfo, bool>)(x => x.Type == MissionType.PIRACY));
				bool flag19 = Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(this.App.Game, this.App.LocalPlayer.ID, num1, MissionType.DEPLOY_NPG, true).Any<FleetInfo>();
				this.App.UI.SetEnabled(panelId1, flag3);
				this.App.UI.SetEnabled(panelId2, flag4);
				this.App.UI.SetEnabled(panelId3, flag5);
				this.App.UI.SetEnabled(panelId4, flag6);
				this.App.UI.SetEnabled(panelId5, flag7);
				this.App.UI.SetEnabled(panelId6, flag8);
				this.App.UI.SetEnabled(panelId7, flag9);
				this.App.UI.SetEnabled(panelId8, flag10);
				this.App.UI.SetEnabled(panelId9, flag11);
				this.App.UI.SetEnabled(panelId10, flag12);
				this.App.UI.SetEnabled(panelId11, flag13);
				this.App.UI.SetEnabled(panelId12, flag15);
				this.App.UI.SetEnabled(panelId13, flag17);
				this.App.UI.SetEnabled(panelId14, flag16);
				this.App.UI.SetEnabled(panelId16, flag18);
				this.App.UI.SetVisible(panelId15, flag14);
				this.App.UI.SetEnabled(panelId15, flag14);
				this.App.UI.SetVisible(panelId17, flag19);
				this.App.UI.SetEnabled(panelId17, flag19);
				this.App.UI.SetVisible(panelId3, this.App.GameDatabase.HasEndOfFleshExpansion());
				this.App.UI.SetEnabled(panelId3, flag5);
				this.App.UI.SetPropertyBool(panelId1, "lockout_button", true);
				this.App.UI.SetPropertyBool(panelId2, "lockout_button", true);
				this.App.UI.SetPropertyBool(panelId3, "lockout_button", true);
				this.App.UI.SetPropertyBool(panelId4, "lockout_button", true);
				this.App.UI.SetPropertyBool(panelId5, "lockout_button", true);
				this.App.UI.SetPropertyBool(panelId6, "lockout_button", true);
				this.App.UI.SetPropertyBool(panelId7, "lockout_button", true);
				this.App.UI.SetPropertyBool(panelId9, "lockout_button", true);
				this.App.UI.SetPropertyBool(panelId8, "lockout_button", true);
				this.App.UI.SetPropertyBool(panelId10, "lockout_button", true);
				this.App.UI.SetPropertyBool(panelId12, "lockout_button", true);
				this.App.UI.SetPropertyBool(panelId13, "lockout_button", true);
				this.App.UI.SetPropertyBool(panelId14, "lockout_button", true);
				this.App.UI.SetPropertyBool(panelId16, "lockout_button", true);
				this.App.UI.AutoSize(this._contextMenuID);
				this.App.UI.ShowTooltip(this._contextMenuID, float.Parse(eventParams[1]), float.Parse(eventParams[2]));
				this._starmap.PostSetProp("ContextMenu", (object)this._contextMenuID, (object)this._starmap.Systems.Reverse[this._contextMenuSystem]);
				this.App.UI.SetPropertyString(this.App.UI.Path(this._contextMenuID, "system_name"), "text", starSystemInfo.Name);
				this.App.UI.ForceLayout(this._contextMenuID);
				this.App.UI.ForceLayout(this._contextMenuID);
			}
		}

		private void ShowResearchContext()
		{
			if (!this._uiEnabled)
				return;
			this.App.UI.SetPropertyBool(this.App.UI.Path(this._researchContextID, "gameResearchButton"), "lockout_button", true);
			this.App.UI.SetPropertyBool(this.App.UI.Path(this._researchContextID, "gameSalvageProjectsButton"), "lockout_button", true);
			this.App.UI.SetPropertyBool(this.App.UI.Path(this._researchContextID, "gameSpecialProjectsButton"), "lockout_button", true);
			this.App.UI.AutoSize(this._researchContextID);
			this.App.UI.ShowTooltip(this._researchContextID, 100f, 100f);
		}

		private void ProcessGameEvent_ObjectClicked(string[] eventParams)
		{
			int id = int.Parse(eventParams[0]);
			if (this._mode == null || !this._mode.OnGameObjectClicked(this.App.GetGameObject(id)))
				this.SelectObject(this.App.GetGameObject(id));
			else
				this.SelectedObject = (object)null;
		}

		internal static void UpdateGateUI(GameSession game, string panelName)
		{
			int totalGateCapacity = GameSession.GetTotalGateCapacity(game, game.LocalPlayer.ID);
			if (totalGateCapacity == 0)
			{
				game.UI.SetVisible(panelName, false);
			}
			else
			{
				game.UI.SetVisible(panelName, true);
				string panelId1 = game.UI.Path(panelName, "gateTotalPower");
				game.UI.SetPropertyString(panelId1, "text", totalGateCapacity.ToString());
				string panelId2 = game.UI.Path(panelName, "gateUsedPower");
				game.UI.SetPropertyString(panelId2, "text", GameSession.GetTotalGateUsage(game, game.LocalPlayer.ID).ToString());
			}
		}

		internal static void UpdateNPGUI(GameSession game, string panelName)
		{
			int cubeMassForTransit = Kerberos.Sots.StarFleet.StarFleet.GetMaxLoaFleetCubeMassForTransit(game, game.LocalPlayer.ID);
			if (cubeMassForTransit == 0)
			{
				game.UI.SetVisible(panelName, false);
			}
			else
			{
				game.UI.SetVisible(panelName, true);
				string panelId = game.UI.Path(panelName, "gateCapacity");
				game.UI.SetPropertyString(panelId, "text", cubeMassForTransit.ToString("N0"));
			}
		}

		private static void ShowStarSystemPopup(
		  string tooltipPanelId,
		  App game,
		  StarMap starmap,
		  int gameObjectId)
		{
			StarMapSystem key = (StarMapSystem)null;
			if (gameObjectId != 0)
				key = game.GetGameObject<StarMapSystem>(gameObjectId);
			if (key == null || !starmap.Systems.Forward.ContainsKey(key))
				return;
			int num = starmap.Systems.Forward[key];
			StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(num);
			string panelId = game.UI.Path(tooltipPanelId, "partSystemName");
			game.UI.SetText(panelId, starSystemInfo.Name);
			if (!game.GameDatabase.IsSurveyed(game.LocalPlayer.ID, num))
				game.UI.SetPropertyColor(panelId, "color", 100f, 100f, 100f);
			else
				game.UI.SetPropertyColor(panelId, "color", (float)byte.MaxValue, (float)byte.MaxValue, (float)byte.MaxValue);
			string mapPanelId = game.UI.Path(tooltipPanelId, "partMiniSystem");
			StarSystemMapUI.Sync(game, num, mapPanelId, false);
			game.UI.SetVisible(tooltipPanelId, true);
		}

		private void ProcessGameEvent_MouseOver(string[] eventParams)
		{
			int num = int.Parse(eventParams[0]);
			if (this._mode != null && this._mode.OnGameObjectMouseOver(this.App.GetGameObject(num)) || this._colonizeDialog != null)
				return;
			if (num == 0)
				this.App.UI.SetVisible("StarSystemPopup", false);
			else
				StarMapState.ShowStarSystemPopup("StarSystemPopup", this.App, this._starmap, num);
		}

		private bool CanOpenFleetManager(int systemid = 0)
		{
			if (systemid == 0)
				return FleetManagerState.CanOpen(this.App.Game, this.SelectedSystem);
			return FleetManagerState.CanOpen(this.App.Game, systemid);
		}

		private bool CanOpenRiderManager()
		{
			return RiderManagerState.CanOpen(this.App.Game, this.SelectedSystem);
		}

		private void OpenFleetManager(int systemid = 0)
		{
			if (systemid == 0)
				this.App.SwitchGameState<FleetManagerState>((object)this.SelectedSystem);
			else
				this.App.SwitchGameState<FleetManagerState>((object)systemid);
		}

		private bool CanOpenStationManager()
		{
			return StationManagerDialog.CanOpen(this.App.Game, this.SelectedSystem);
		}

		private bool CanOpenPlanetManager()
		{
			return PlanetManagerState.CanOpen(this.App.Game, this.SelectedSystem);
		}

		private void OpenPlanetManager()
		{
			this.App.SwitchGameState<PlanetManagerState>((object)this.SelectedSystem);
		}

		private void PopulateViewFilterList()
		{
			this.App.UI.ClearItems("viewModeDropdown");
			this.App.UI.AddItem("viewModeDropdown", "", 0, App.Localize("@UI_STARMAPVIEW_NORMAL_VIEW"));
			this.App.UI.AddItem("viewModeDropdown", "", 1, App.Localize("@UI_STARMAPVIEW_SURVEY_DISPLAY"));
			this.App.UI.AddItem("viewModeDropdown", "", 2, App.Localize("@UI_STARMAPVIEW_PROVINCE_DISPLAY"));
			this.App.UI.AddItem("viewModeDropdown", "", 3, App.Localize("@UI_STARMAPVIEW_SUPPORT_RANGE_DISPLAY"));
			this.App.UI.AddItem("viewModeDropdown", "", 4, App.Localize("@UI_STARMAPVIEW_SENSOR_RANGE_DISPLAY"));
			this.App.UI.AddItem("viewModeDropdown", "", 5, App.Localize("@UI_STARMAPVIEW_TERRAIN_DISPLAY"));
			if (!this.App.GetStratModifier<bool>(StratModifiers.EnableTrade, this.App.LocalPlayer.ID))
				return;
			this.App.UI.AddItem("viewModeDropdown", "", 6, App.Localize("@UI_STARMAPVIEW_TRADE_DISPLAY"));
		}

		private void UIHandleCoreBehaviour(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "gameEndTurnButton")
					this.SetUIEnabled(!this._uiEnabled);
				else if (panelName == "gameExitButton")
					this.App.UI.CreateDialog((Dialog)new MainMenuDialog(this.App), null);
				else if (panelName == "gameTutorialButton")
				{
					if (this._lastFilterSelection == StarMapViewFilter.VF_TRADE)
						this.App.UI.SetVisible("TradeTutorial", true);
					else
						this.App.UI.SetVisible("StarMapTutorial", true);
				}
				else if (panelName == "starMapTutImage")
				{
					this.App.UI.SetVisible("StarMapTutorial", false);
					this.App.UI.SetVisible("TradeTutorial", false);
				}
				else
				{
					if (FleetUI.HandleFleetAndPlanetWidgetInput(this.App, "fleetAndPlanetDetailsWidget", panelName))
						return;
					if (panelName == "gameEventHistoryButton")
						this.App.UI.CreateDialog((Dialog)new EventHistoryDialog(this.App), null);
					else if (panelName == "turnEventNext")
					{
						this.ShowNextEvent(false);
						this.FocusLastEvent();
					}
					else if (panelName == "turnEventPrevious")
					{
						this.ShowNextEvent(true);
						this.FocusLastEvent();
					}
					else
					{
						if (!(panelName == "btnturnEventImageButton"))
							return;
						this.FocusLastEvent();
					}
				}
			}
			else if (msgType == "list_sel_changed")
			{
				if (panelName == "partSystemPlanets")
				{
					int num = 0;
					if (msgParams.Length != 0 && !string.IsNullOrEmpty(msgParams[0]))
						num = int.Parse(msgParams[0]);
					this.SetSelectedPlanet(num, panelName);
				}
				else
				{
					if (!(panelName == "viewModeDropdown"))
						return;
					StarMapViewFilter starMapViewFilter = (StarMapViewFilter)int.Parse(msgParams[0]);
					this._starmap.ViewFilter = starMapViewFilter;
					this._lastFilterSelection = starMapViewFilter;
					this.RefreshSystemInterface();
				}
			}
			else
			{
				if (!(msgType == "endturn_activated"))
					return;
				this.EndTurn(false);
			}
		}

		public void FocusLastEvent()
		{
			if (this._currentEvent == null || this._currentEvent.SystemID == 0)
				return;
			StarSystemInfo starSystemInfo = this.App.GameDatabase.GetStarSystemInfo(this._currentEvent.SystemID);
			if (starSystemInfo == (StarSystemInfo)null || starSystemInfo.IsDeepSpace)
				return;
			StarMapSystem starMapSystem = this._starmap.Systems.Reverse[this._currentEvent.SystemID];
			this.SelectObject((IGameObject)starMapSystem);
			this.StarMap.SetFocus((IGameObject)starMapSystem);
			this.StarMap.Select((IGameObject)starMapSystem);
			this.SelectedObject = (object)this.App.GameDatabase.GetStarSystemInfo(this._currentEvent.SystemID);
			if (this._currentEvent.ColonyID == 0)
				return;
			ColonyInfo colonyInfo = this.App.GameDatabase.GetColonyInfo(this._currentEvent.ColonyID);
			if (colonyInfo == null)
				return;
			this.SetSelectedPlanet(colonyInfo.OrbitalObjectID, "");
		}

		public void ShowUpgradeMissionOverlay(int targetSystem)
		{
			this._missionOverlays["gameUpgradeStationButton"].Show(targetSystem);
		}

		public OverlayUpgradeMission GetUpgradeMissionOverlay()
		{
			return (OverlayUpgradeMission)this._missionOverlays["gameUpgradeStationButton"];
		}

		protected override void UICommChannel_OnPanelMessage(
		  string panelName,
		  string msgType,
		  string[] msgParams)
		{
			this.UIHandleCoreBehaviour(panelName, msgType, msgParams);
			if (!this._uiEnabled || this._piechart != null && this._piechart.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self))
				return;
			if (msgType == "mapicon_clicked")
			{
				if (!(panelName == "partMiniSystem") || !this.App.GameDatabase.IsSurveyed(this.App.LocalPlayer.ID, this.SelectedSystem))
					return;
				this.SetSelectedPlanet(int.Parse(msgParams[0]), panelName);
			}
			else if (msgType == "mapicon_clicked_station")
			{
				if (!(panelName == "partMiniSystem"))
					return;
				int.Parse(msgParams[0]);
				int.Parse(msgParams[1]);
			}
			else if (msgType == "mapicon_dblclicked")
			{
				if (!(panelName == "partMiniSystem") || this.OverlayActive())
					return;
				int.Parse(msgParams[0]);
				if (!this.App.GameDatabase.IsSurveyed(this.App.LocalPlayer.ID, this.SelectedSystem))
					return;
				this.OpenSystemView();
			}
			else if (msgType == "button_rclicked")
			{
				if (!(panelName == "gameResearchButton"))
					return;
				this.ShowResearchContext();
			}
			else if (msgType == "button_clicked")
			{
				if (this._mode != null && this._mode.OnUIButtonPressed(panelName))
					return;
				if (panelName == "gameResearchButton" || panelName == "researchCubeButton")
					this.App.SwitchGameState("ResearchScreenState");
				else if (panelName == "gameSalvageProjectsButton")
					this.App.UI.CreateDialog((Dialog)new SalvageProjectDialog(this.App, "dialogSpecialProjects"), null);
				else if (panelName == "gameSpecialProjectsButton")
					this.App.UI.CreateDialog((Dialog)new SpecialProjectDialog(this.App, "dialogSpecialProjects"), null);
				else if (panelName == "btnAbandon")
				{
					if (this.App.GameDatabase.GetColonyInfoForPlanet(this._selectedPlanet) == null)
						return;
					this._confirmAbandon = this.App.UI.CreateDialog((Dialog)new GenericQuestionDialog(this.App, "@UI_DIALOGCONFIRMABANDON_TITLE", "@UI_DIALOGCONFIRMABANDON_DESC", "dialogGenericQuestion"), null);
				}
				else if (panelName == "gameBattleRiderManagerButton")
				{
					this.App.UI.LockUI();
					this.OpenBattleRiderManagerScreen();
				}
				else if (panelName == "gameDiplomacyButton")
				{
					this.App.UI.LockUI();
					this.OpenDiplomacyScreen();
				}
				else if (panelName == "gameDesignButton")
				{
					this.App.UI.LockUI();
					this.OpenDesignScreen();
				}
				else if (panelName == "gameBuildButton")
				{
					this.App.UI.LockUI();
					this.OpenBuildScreen();
				}
				else if (panelName == "gameRepairButton")
				{
					if (this.SelectedSystem != 0)
						this.App.UI.CreateDialog((Dialog)new RepairShipsDialog(this.App, this.SelectedSystem, this.App.GameDatabase.GetFleetInfoBySystemID(this.SelectedSystem, FleetType.FL_ALL).ToList<FleetInfo>(), "dialogRepairShips"), null);
					else
						this.App.UI.CreateDialog((Dialog)new RepairShipsDialog(this.App, this.SelectedSystem, new List<FleetInfo>()
			{
			  this.App.GameDatabase.GetFleetInfo(this.SelectedFleet)
			}, "dialogRepairShips"), null);
				}
				else if (panelName == "btnSystemOpen")
				{
					bool isOpen = !this.App.GameDatabase.GetStarSystemInfo(this.SelectedSystem).IsOpen;
					this.App.GameDatabase.UpdateStarSystemOpen(this.SelectedSystem, isOpen);
					this.App.UI.SetVisible("SystemDetailsWidget.ClosedSystem", !isOpen);
					this.App.Game.OCSystemToggleData.SystemToggled(this.App.LocalPlayer.ID, this.SelectedSystem, isOpen);
				}
				else if (panelName == "gameSystemButton")
				{
					this.App.UI.LockUI();
					this.OpenSystemView();
				}
				else if (panelName == "gameSotspediaButton")
				{
					this.App.UI.LockUI();
					this.App.SwitchGameState("SotspediaState");
				}
				else if (panelName == "gameProvinceModeButton")
					this.SetProvinceMode(!this._isProvinceMode);
				else if (panelName == "gameEmpireSummaryButton")
				{
					this.App.UI.LockUI();
					this.App.SwitchGameState<EmpireSummaryState>();
				}
				else if (panelName == "debugTestCombatButton")
					this.DebugTestCombat();
				else if (panelName == "gameUpgradeStationButton")
					this._missionOverlays["gameUpgradeStationButton"].Show(this._contextMenuSystem);
				else if (panelName == "partHardenedStructure")
				{
					string panelName1 = this._dialogState == ESMDialogState.ESMD_ColonyEstablished ? "colony_event_dialog" : "colonyDetailsWidget";
					int num = this._dialogState == ESMDialogState.ESMD_ColonyEstablished ? this._colonyEstablishedPlanet : this.SelectedPlanet;
					ColonyInfo colonyInfoForPlanet = this.App.GameDatabase.GetColonyInfoForPlanet(num);
					colonyInfoForPlanet.isHardenedStructures = !colonyInfoForPlanet.isHardenedStructures;
					this.App.GameDatabase.UpdateColony(colonyInfoForPlanet);
					StarSystemUI.SyncColonyDetailsWidget(this.App.Game, panelName1, num, panelName);
				}
				else if (panelName == "gameStationManagerButton")
					this.App.UI.CreateDialog((Dialog)new StationManagerDialog(this.App, this, 0, "dialogStationManager"), null);
				else if (panelName == "gameContextStationManagerButton")
					this.App.UI.CreateDialog((Dialog)new StationManagerDialog(this.App, this, this._contextMenuSystem, "dialogStationManager"), null);
				else if (panelName == "gamePlanetSummaryButton")
					this.App.UI.CreateDialog((Dialog)new PlanetManagerDialog(this.App, "dialogPlanetManager"), null);
				else if (panelName == "gamePopulationManagerButton")
					this.App.UI.CreateDialog((Dialog)new PopulationManagerDialog(this.App, this.SelectedSystem, "dialogPopulationManager"), null);
				else if (panelName == "gameComparativeAnalysysButton")
				{
					this.App.UI.LockUI();
					this.App.SwitchGameState<ComparativeAnalysysState>((object)false, (object)nameof(StarMapState));
				}
				else if (panelName == "gameFleetSummaryButton")
					this.App.UI.CreateDialog((Dialog)new FleetSummaryDialog(this.App, "FleetSummaryDialog"), null);
				else if (panelName == "gameFleetManagerButton")
				{
					if (!this.CanOpenFleetManager(0))
						return;
					this.App.UI.LockUI();
					this.OpenFleetManager(0);
				}
				else if (panelName == "gameContextFleetManagerButton")
				{
					if (!this.CanOpenFleetManager(this._contextMenuSystem))
						return;
					this.App.UI.LockUI();
					this.OpenFleetManager(this._contextMenuSystem);
				}
				else if (panelName == "gameDefenseManagerButton")
				{
					this.App.UI.LockUI();
					this.App.SwitchGameState<DefenseManagerState>((object)this._contextMenuSystem);
				}
				else if (panelName == "fleetCancelButton")
				{
					FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(this._fleetContextFleet);
					AdmiralInfo admiralInfo = this.App.GameDatabase.GetAdmiralInfo(fleetInfo.AdmiralID);
					if (admiralInfo != null)
						this.App.PostRequestSpeech(string.Format("STRAT_008-01_{0}_{1}UniversalMissionNegation", (object)this.App.GameDatabase.GetFactionName(this.App.GameDatabase.GetPlayerFactionID(this.App.LocalPlayer.ID)), (object)admiralInfo.GetAdmiralSoundCueContext(this.App.AssetDatabase)), 50, 120, 0.0f);
					Kerberos.Sots.StarFleet.StarFleet.CancelMission(this.App.Game, fleetInfo, true);
					StarMapState.UpdateGateUI(this.App.Game, "gameGateInfo");
				}
				else if (panelName == "fleetInterceptButton")
				{
					((OverlayInterceptMission)this._missionOverlays["fleetInterceptButton"]).TargetFleet = this._fleetContextFleet;
					this._missionOverlays["fleetInterceptButton"].Show(this._contextMenuSystem);
				}
				else if (panelName == "fleetBuildStationButton")
				{
					((OverlaySpecialConstructionMission)this._missionOverlays["fleetBuildStationButton"]).TargetFleet = this._fleetContextFleet;
					this._missionOverlays["fleetBuildStationButton"].Show(this._contextMenuSystem);
				}
				else if (this._missionOverlays.ContainsKey(panelName))
					this._missionOverlays[panelName].Show(this._contextMenuSystem);
				else if (panelName == "Build_Freighter")
				{
					List<DesignInfo> list = this.App.GameDatabase.GetDesignInfosForPlayer(this.App.LocalPlayer.ID, RealShipClasses.Cruiser, true).Where<DesignInfo>((Func<DesignInfo, bool>)(x =>
				   {
					   if (((IEnumerable<DesignSectionInfo>)x.DesignSections).Any<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(j => j.ShipSectionAsset.FreighterSpace > 0)))
						   return x.isPrototyped;
					   return false;
				   })).ToList<DesignInfo>();
					DesignInfo designInfo1 = (DesignInfo)null;
					foreach (DesignInfo designInfo2 in list)
					{
						if (designInfo1 == null)
							designInfo1 = designInfo2;
						else if (designInfo2.DesignDate > designInfo1.DesignDate)
							designInfo1 = designInfo2;
					}
					if (designInfo1 == null)
						return;
					int invoiceId = this.App.GameDatabase.InsertInvoice("Freighter", this.App.LocalPlayer.ID, false);
					int num = this.App.GameDatabase.InsertInvoiceInstance(this.App.LocalPlayer.ID, this.SelectedSystem, "Freighter");
					this.App.GameDatabase.InsertInvoiceBuildOrder(invoiceId, designInfo1.ID, designInfo1.Name, 0);
					this.App.GameDatabase.InsertBuildOrder(this.SelectedSystem, designInfo1.ID, 0, 0, designInfo1.Name, designInfo1.GetPlayerProductionCost(this.App.GameDatabase, this.App.LocalPlayer.ID, false, new float?()), new int?(num), new int?(), 0);
					this.SyncFreighterInterface();
				}
				else if (panelName.StartsWith("tickerEventButton"))
				{
					string[] strArray = panelName.Split('|');
					if (((IEnumerable<string>)strArray).Count<string>() != 2)
						return;
					int eventid = int.Parse(strArray[1]);
					if (!this.App.Game.TurnEvents.Any<TurnEvent>((Func<TurnEvent, bool>)(x => x.ID == eventid)))
						return;
					this.ShowEvent(eventid);
				}
				else if (panelName.StartsWith("freighterdeletebtn"))
				{
					ShipInfo shipInfo = this.App.GameDatabase.GetShipInfo(int.Parse(panelName.Split('|')[1]), false);
					if (shipInfo == null)
						return;
					this.DeleteFrieghterConfirm = this.App.UI.CreateDialog((Dialog)new GenericQuestionDialog(this.App, "Scrap Freighter", "Confirm Scrap Freighter.", "dialogGenericQuestion"), null);
					this._selectedfreighter = shipInfo.ID;
				}
				else
				{
					if (!(panelName == "btnChat"))
						return;
					this.App.Network.SetChatWidgetVisibility(new bool?());
					this.App.UI.SetPropertyBool("btnChat", "flashing", false);
				}
			}
			else if (msgType == "ChatMessage")
				this.App.UI.SetPropertyBool("btnChat", "flashing", true);
			else if (msgType == "mouse_enter")
			{
				if (!(panelName == "moraleeventtooltipover"))
					return;
				int x = int.Parse(msgParams[0]);
				int y = int.Parse(msgParams[1]);
				ColonyInfo colonyInfoForPlanet = this.App.GameDatabase.GetColonyInfoForPlanet(this.SelectedPlanet);
				if (colonyInfoForPlanet == null || this.App.LocalPlayer.ID != colonyInfoForPlanet.PlayerID)
					return;
				StarSystemUI.ShowMoraleEventToolTip(this.App.Game, colonyInfoForPlanet.ID, x, y);
			}
			else
			{
				if (msgType == "mouse_leave")
					return;
				if (msgType == "slider_value")
				{
					if (panelName.Contains("meTradeSlider"))
					{
						int num = int.Parse(panelName.Split('|')[1]);
						OrbitalObjectInfo orbitalObjectInfo = this.App.GameDatabase.GetOrbitalObjectInfo(num);
						if (orbitalObjectInfo == null)
							return;
						StarSystemDetailsUI.SetOutputRate(this.App, num, panelName, msgParams[0]);
						this._starmap.UpdateSystemTrade(orbitalObjectInfo.StarSystemID);
					}
					else if (panelName == "gameEmpireResearchSlider")
					{
						StarMapState.SetEmpireResearchRate(this.App.Game, msgParams[0], this._piechart);
						this._techCube.SpinSpeed = (float)int.Parse(msgParams[0]) * (1f / 500f);
						this.UpdateTechCubeToolTip();
					}
					else
					{
						if (this.SelectedPlanet == StarSystemDetailsUI.StarItemID || this.SelectedPlanet == 0)
							return;
						string panelName1 = this._dialogState == ESMDialogState.ESMD_ColonyEstablished ? "colony_event_dialog" : "colonyDetailsWidget";
						if (StarSystemDetailsUI.IsOutputRateSlider(panelName))
						{
							StarSystemDetailsUI.SetOutputRate(this.App, this.SelectedPlanet, panelName, msgParams[0]);
							StarSystemUI.SyncColonyDetailsWidget(this.App.Game, panelName1, this.SelectedPlanet, panelName);
							this._starmap.UpdateSystemTrade(this.App.GameDatabase.GetOrbitalObjectInfo(this.SelectedPlanet).StarSystemID);
						}
						else if (panelName == "partOverharvestSlider")
						{
							ColonyInfo colonyInfoForPlanet = this.App.GameDatabase.GetColonyInfoForPlanet(this.SelectedPlanet);
							colonyInfoForPlanet.OverharvestRate = StarSystemDetailsUI.SliderValueToOutputRate(int.Parse(msgParams[0]));
							this.App.GameDatabase.UpdateColony(colonyInfoForPlanet);
							StarSystemUI.SyncColonyDetailsWidget(this.App.Game, panelName1, this.SelectedPlanet, panelName);
						}
						else if (panelName == "partCivSlider")
						{
							ColonyInfo colonyInfoForPlanet = this.App.GameDatabase.GetColonyInfoForPlanet(this.SelectedPlanet);
							colonyInfoForPlanet.CivilianWeight = StarSystemDetailsUI.SliderValueToOutputRate(int.Parse(msgParams[0]));
							this.App.GameDatabase.UpdateColony(colonyInfoForPlanet);
							StarSystemUI.SyncColonyDetailsWidget(this.App.Game, panelName1, this.SelectedPlanet, panelName);
						}
						else
						{
							if (!(panelName == "partWorkRate"))
								return;
							ColonyInfo colonyInfoForPlanet = this.App.GameDatabase.GetColonyInfoForPlanet(this.SelectedPlanet);
							colonyInfoForPlanet.SlaveWorkRate = StarSystemDetailsUI.SliderValueToOutputRate(int.Parse(msgParams[0]));
							this.App.GameDatabase.UpdateColony(colonyInfoForPlanet);
							StarSystemUI.SyncColonyDetailsWidget(this.App.Game, panelName1, this.SelectedPlanet, panelName);
						}
					}
				}
				else if (msgType == "slider_notched")
				{
					if (panelName.Contains("meTradeSlider"))
					{
						int orbitalObjectID = int.Parse(panelName.Split('|')[1]);
						if (this.App.GameDatabase.GetOrbitalObjectInfo(orbitalObjectID) != null)
						{
							ColonyInfo colonyInfoForPlanet = this.App.GameDatabase.GetColonyInfoForPlanet(orbitalObjectID);
							if (colonyInfoForPlanet != null)
								PlanetWidget.UpdateTradeSliderNotchInfo(this.App, colonyInfoForPlanet.ID, int.Parse(msgParams[0]));
						}
					}
					if (!panelName.Contains("partTradeSlider"))
						return;
					ColonyInfo colonyInfoForPlanet1 = this.App.GameDatabase.GetColonyInfoForPlanet(this.SelectedPlanet);
					if (colonyInfoForPlanet1 == null || colonyInfoForPlanet1.PlayerID != this.App.LocalPlayer.ID)
						return;
					PlanetWidget.UpdateTradeSliderNotchInfo(this.App, colonyInfoForPlanet1.ID, int.Parse(msgParams[0]));
				}
				else if (msgType == "list_item_dblclk")
				{
					if (!(panelName == "partSystemPlanets") || !this.App.GameDatabase.IsSurveyed(this.App.LocalPlayer.ID, this.SelectedSystem))
						return;
					this.OpenSystemView();
				}
				else if (msgType == "text_changed")
				{
					if (!panelName.StartsWith("gameColonyName"))
						return;
					this._enteredColonyName = msgParams[0];
					this.App.UI.SetEnabled(this.App.UI.Path("colony_event_dialog", "event_dialog_close"), !string.IsNullOrWhiteSpace(this._enteredColonyName) && this._enteredColonyName.Length > 0);
				}
				else
				{
					if (!(msgType == "dialog_closed") || this._mode != null && this._mode.OnUIDialogClosed(panelName, msgParams))
						return;
					if (this._eventDialogShown)
					{
						this._eventDialogShown = false;
						this._colonizeDialog = (ColonizeDialog)null;
						this._stationDialog = (StationBuiltDialog)null;
						this.ShowNextEvent(false);
						this._dialogState = ESMDialogState.ESMD_None;
						if (!(panelName == this._feasibilityCompleteDialog))
							return;
						this.RefreshTechCube();
					}
					else if (panelName == this._confirmAbandon)
					{
						if (!bool.Parse(msgParams[0]))
							return;
						ColonyInfo colonyInfoForPlanet = this.App.GameDatabase.GetColonyInfoForPlanet(this._selectedPlanet);
						if (colonyInfoForPlanet != null)
							GameSession.AbandonColony(this.App, colonyInfoForPlanet.ID);
						this.RefreshSystemInterface();
					}
					else if (panelName == this.DeleteFrieghterConfirm)
					{
						if (!bool.Parse(msgParams[0]))
							return;
						if (this.App.GameDatabase.GetShipInfo(this._selectedfreighter, false) != null)
							this.App.GameDatabase.RemoveShip(this._selectedfreighter);
						this.DeleteFrieghterConfirm = null;
						this.SyncFreighterInterface();
					}
					else
					{
						if (!(panelName == this._endTurnConfirmDialog))
							return;
						if (bool.Parse(msgParams[0]))
						{
							this.SetUIEnabled(false);
							this.EndTurn(true);
						}
						else
						{
							this._dialogCounter = 0;
							this.EnableNextTurnButton(true);
						}
					}
				}
			}
		}

		public static void SetEmpireResearchRate(
		  GameSession game,
		  string value,
		  BudgetPiechart piechart)
		{
			float num = ((float)int.Parse(value) / 100f).Clamp(0.0f, 1f);
			PlayerInfo playerInfo = game.GameDatabase.GetPlayerInfo(game.LocalPlayer.ID);
			playerInfo.RateGovernmentResearch = 1f - num;
			if (game.GameDatabase.GetSliderNotchSettingInfo(playerInfo.ID, UISlidertype.SecuritySlider) != null)
			{
				Budget budget = Budget.GenerateBudget(game, playerInfo, (IEnumerable<DesignInfo>)null, BudgetProjection.Pessimistic);
				EmpireSummaryState.DistributeGovernmentSpending(game, EmpireSummaryState.GovernmentSpendings.Security, (float)Math.Min((double)budget.RequiredSecurity / 100.0, 1.0), playerInfo);
			}
			else
				game.GameDatabase.UpdatePlayerSliders(game, playerInfo);
			if (piechart == null)
				return;
			Budget budget1 = Budget.GenerateBudget(game, playerInfo, (IEnumerable<DesignInfo>)null, BudgetProjection.Pessimistic);
			piechart.SetSlices(budget1);
		}

		private void DebugTestCombat()
		{
			if (this.SelectedSystem == 0)
				return;
			this.App.LaunchCombat(this.App.Game, new PendingCombat()
			{
				SystemID = this.SelectedSystem
			}, true, false, true);
		}

		public void EndTurn(bool forceConfirm = false)
		{
			if (!forceConfirm && this.App.GameDatabase.GetPlayerResearchingTechID(this.App.LocalPlayer.ID) == 0 && (this.App.GameDatabase.GetPlayerFeasibilityStudyTechId(this.App.LocalPlayer.ID) == 0 && this._dialogCounter == 0) && !this.App.LocalPlayer.IsAI())
			{
				this._endTurnConfirmDialog = this.App.UI.CreateDialog((Dialog)new GenericQuestionDialog(this.App, "No Research", "You have no active reasearch projects, are you sure you want to end turn?", "dialogGenericQuestion"), null);
				++this._dialogCounter;
				this.SetUIEnabled(true);
			}
			else
			{
				if (!forceConfirm && this.EnableFleetCheck && this._dialogCounter == 0 || this.EnableFleetCheck && this.App.GameDatabase.GetPlayerResearchingTechID(this.App.LocalPlayer.ID) == 0 && (this.App.GameDatabase.GetPlayerFeasibilityStudyTechId(this.App.LocalPlayer.ID) == 0 && this._dialogCounter > 0))
				{
					foreach (FleetInfo fleetInfo in this.App.GameDatabase.GetFleetInfosByPlayerID(this.App.LocalPlayer.ID, FleetType.FL_NORMAL).ToList<FleetInfo>())
					{
						if (this.App.GameDatabase.GetMissionByFleetID(fleetInfo.ID) == null)
						{
							this._endTurnConfirmDialog = this.App.UI.CreateDialog((Dialog)new GenericQuestionDialog(this.App, "Inactive fleets", "You have inactive fleets, are you sure you want to end turn?", "dialogGenericQuestion"), null);
							this.SetUIEnabled(true);
							this.SetSelectedSystem(fleetInfo.SystemID, true);
							this._dialogCounter = 0;
							return;
						}
					}
				}
				if (this.SelectedFleet != 0)
				{
					this._starmap.PostSetProp("SelectEnabled", false);
					this._starmap.PostSetProp("SelectEnabled", true);
					this.SelectedObject = (object)null;
				}
				this.EnableNextTurnButton(false);
				this.App.Game.UpdateOpenCloseSystemToggle();
				App.Commands.EndTurn.Trigger();
				if (this.App.GameSetup.IsMultiplayer)
					this.App.Network.EndTurn();
				if (GameSession.SimAITurns > 0)
					--GameSession.SimAITurns;
				this._dialogCounter = 0;
			}
		}

		public void ShowProcessingFlash()
		{
			this.App.UI.SetVisible("TurnUpdate", true);
			this.App.UI.SetVisible("TurnUpdate2", true);
		}

		private void EnableNextTurnButton(bool val)
		{
			this.App.UI.SetEnabled("gameEndTurnButton", val);
			this.SetUIEnabled(val);
		}

		public void TurnStarted()
		{
			this.App.UI.UnlockUI();
			this.EnableNextTurnButton(true);
			this.RefreshStarmap(StarMapState.StarMapRefreshType.REFRESH_NORMAL);
			this.ShowNextEvent(false);
			TurnEventUI.SyncTurnEventTicker(this.App.Game, "event_list");
			this.App.UI.SetVisible("TurnUpdateText", false);
			this.App.UI.SetText("UpdateLabel", App.Localize("@TURN") + " " + (object)this.App.GameDatabase.GetTurnCount());
			this.ShowProcessingFlash();
			this._simNewTurnTick = 200;
		}

		public void TurnEnded()
		{
			this.App.UserProfile.SaveProfile();
			this.App.TurnEvents.Clear();
			this._currentEvent = (TurnEvent)null;
			if (this.App.CurrentState != this)
				return;
			this.App.UI.SetVisible("TurnUpdateText", true);
			this.App.UI.SetText("UpdateLabel", "PROCESSING...");
			this.ShowProcessingFlash();
			this.App.UI.LockUI();
			this.App.UI.Update();
			this.RefreshSystemInterface();
			this.PopulateViewFilterList();
		}

		public void SetUIEnabled(bool val)
		{
			this._uiEnabled = val;
			this.App.UI.SetEnabled("gameEmpireSummaryButton", val);
			this.App.UI.SetEnabled("gameResearchButton", val);
			this.App.UI.SetEnabled("gameDiplomacyButton", val);
			this.App.UI.SetEnabled("btnAbandon", val);
			this.App.UI.SetEnabled("partHardenedStructure", val);
			this.App.UI.SetEnabled("gameDesignButton", val);
			this.App.UI.SetEnabled("gameRepairButton", val);
			this.App.UI.SetEnabled("gameBuildButton", val);
			this.App.UI.SetEnabled("gameSystemButton", val);
			this.App.UI.SetEnabled("gameSotspediaButton", val);
			this.App.UI.SetEnabled("gameProvinceModeButton", val);
			this.App.UI.SetEnabled("gameBattleRiderManagerButton", val);
			this.App.UI.SetEnabled("fleetCancelButton", val);
			this.App.UI.SetEnabled("gameSurveyButton", val);
			this.App.UI.SetEnabled("gameColonizeButton", val);
			this.App.UI.SetEnabled("gameEvacuateButton", val);
			this.App.UI.SetEnabled("gameRelocateButton", val);
			this.App.UI.SetEnabled("gamePatrolButton", val);
			this.App.UI.SetEnabled("gameInterdictButton", val);
			this.App.UI.SetEnabled("gameStrikeButton", val);
			this.App.UI.SetEnabled("gameInvadeButton", val);
			this.App.UI.SetEnabled("gameConstructStationButton", val);
			this.App.UI.SetEnabled("gameUpgradeStationButton", val);
			this.App.UI.SetEnabled("gameStationManagerButton", val);
			this.App.UI.SetEnabled("gameFleetManagerButton", val);
			this.App.UI.SetEnabled("gameFleetSummaryButton", val);
			this.App.UI.SetEnabled("gameDefenseManagerButton", val);
			this.App.UI.SetEnabled("gameGateButton", val);
			this.App.UI.SetEnabled("gamePiracyButton", val);
			this.App.UI.SetEnabled("gameNPGButton", val);
			this.App.UI.SetEnabled("gameEventHistoryButton", val);
			this.App.UI.SetEnabled("gameEmpireResearchSlider", val);
			this.App.UI.SetEnabled("gamePlanetSummaryButton", val);
			this.App.UI.SetEnabled("gamePopulationManagerButton", val);
			bool flag = this.App.GameDatabase.GetStratModifier<bool>(StratModifiers.ComparativeAnalysys, this.App.LocalPlayer.ID) && this.App.GameDatabase.GetDesignsEncountered(this.App.LocalPlayer.ID).Any<DesignInfo>((Func<DesignInfo, bool>)(x => x.Class != ShipClass.Station));
			this.App.UI.SetEnabled("gameComparativeAnalysysButton", val && flag);
			this.App.UI.SetVisible("colonyDetailsWidget", false);
			this._fleetWidget.SetEnabled(val);
			this.App.HotKeyManager.SetEnabled(true);
			if (!val)
				return;
			this.App.UI.SetEnabled("gameRepairButton", this.CanOpenRepairScreen());
			this.App.UI.SetEnabled("gameBuildButton", this.CanOpenBuildScreen());
			this.App.UI.SetEnabled("gameFleetManagerButton", this.CanOpenFleetManager(0));
			this.App.UI.SetEnabled("gameStationManagerButton", this.CanOpenStationManager());
		}

		public void ShowEvent(int eventid)
		{
			if (this.App.TurnEvents.Count <= 0 || !this.App.TurnEvents.Any<TurnEvent>((Func<TurnEvent, bool>)(x => x.ID == eventid)))
				return;
			this._currentEvent = this.App.TurnEvents.FirstOrDefault<TurnEvent>((Func<TurnEvent, bool>)(x => x.ID == eventid));
			if (this._currentEvent.ShowsDialog && !this._currentEvent.dialogShown)
			{
				this.ShowEventDialog(this._currentEvent);
				this._eventDialogShown = true;
				this._currentEvent.dialogShown = true;
				this.App.GameDatabase.UpdateTurnEventDialogShown(this._currentEvent.ID, true);
			}
			TurnEventUI.SyncTurnEventWidget(this.App.Game, "turnEventWidget", this._currentEvent);
			this.App.PostRequestGuiSound("starmap_messagealert");
			this._currentEvent.EventViewed = true;
			this.App.UI.SetVisible(this.App.UI.Path("turnEventNext", "newEventFlash"), (this.App.Game.TurnEvents.Any<TurnEvent>((Func<TurnEvent, bool>)(x => !x.EventViewed)) ? 1 : 0) != 0);
		}

		public void ShowNextEvent(bool reverse = false)
		{
			if (this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID).isDefeated)
				this.App.UI.CreateDialog((Dialog)new EndGameDialog(this.App, App.Localize("@UI_ENDGAME_DEFEAT"), string.Format(App.Localize("@UI_ENDGAME_DEFEAT_CONQUEST"), (object)this.App.GameSetup.Players[this.App.Game.LocalPlayer.ID - 1].EmpireName), "loseScreen"), null);
			else if (this.App.TurnEvents.Count > 0)
			{
				if (this.App.TurnEvents.Any<TurnEvent>())
				{
					if (this._currentEvent != null)
					{
						int index = this.App.Game.TurnEvents.FindIndex((Predicate<TurnEvent>)(te => te.ID == this._currentEvent.ID)) + (reverse ? -1 : 1);
						if (index >= 0 && index <= this.App.Game.TurnEvents.Count - 1)
							this._currentEvent = this.App.Game.TurnEvents[index];
					}
					else
						this._currentEvent = this.App.Game.TurnEvents[0];
				}
				if (this._currentEvent.ShowsDialog && !this._currentEvent.dialogShown)
				{
					this.ShowEventDialog(this._currentEvent);
					this._eventDialogShown = true;
					this._currentEvent.dialogShown = true;
					this.App.GameDatabase.UpdateTurnEventDialogShown(this._currentEvent.ID, true);
				}
				TurnEventUI.SyncTurnEventWidget(this.App.Game, "turnEventWidget", this._currentEvent);
				this.App.PostRequestGuiSound("starmap_messagealert");
				if (!string.IsNullOrEmpty(this._currentEvent.EventSoundCueName) && this._currentEvent.PlayerID == this.App.LocalPlayer.ID && !this._currentEvent.EventViewed)
				{
					this.App.PostRequestSpeech(this._currentEvent.EventSoundCueName, 50, 120, 0.0f);
					this._currentEvent.EventSoundCueName = string.Empty;
					this.App.GameDatabase.UpdateTurnEventSoundQue(this._currentEvent.ID, this._currentEvent.EventSoundCueName);
				}
				this._currentEvent.EventViewed = true;
			}
			else
			{
				TurnEventUI.SyncTurnEventWidget(this.App.Game, "turnEventWidget", (TurnEvent)null);
				this._currentEvent = (TurnEvent)null;
			}
			this.App.UI.SetVisible(this.App.UI.Path("turnEventNext", "newEventFlash"), (this.App.Game.TurnEvents.Any<TurnEvent>((Func<TurnEvent, bool>)(x => !x.EventViewed)) ? 1 : 0) != 0);
		}

		private void ShowEventDialog(TurnEvent turnEvent)
		{
			if (GameSession.SimAITurns > 0)
				return;
			if (turnEvent.EventType == TurnEventType.EV_COMBAT_WIN || turnEvent.EventType == TurnEventType.EV_COMBAT_LOSS || turnEvent.EventType == TurnEventType.EV_COMBAT_DRAW)
				this.App.UI.CreateDialog((Dialog)new DialogPostCombat(this.App, turnEvent.SystemID, turnEvent.CombatID, turnEvent.TurnNumber, "postCombat"), null);
			if (turnEvent.EventType == TurnEventType.EV_SUULKA_ARRIVES)
				this._suulkaArrivalDialog = this.App.UI.CreateDialog((Dialog)new SuulkaArrivalDialog(this.App, turnEvent.SystemID), null);
			else if (turnEvent.EventType == TurnEventType.EV_REQUEST_REQUESTED)
			{
				this._requestRequestedDialog = new RequestRequestedDialog(this.App, this.App.GameDatabase.GetRequestInfo(turnEvent.TreatyID), "dialogRequested");
				this.App.UI.CreateDialog((Dialog)this._requestRequestedDialog, null);
			}
			else if (turnEvent.EventType == TurnEventType.EV_REQUEST_ACCEPTED)
			{
				this._requestAcceptedDialog = new GenericTextDialog(this.App, App.Localize("@UI_DIPLOMACY_REQUEST_ACCEPTED_TITLE"), turnEvent.GetEventMessage(this.App.Game), "dialogGenericMessage");
				this.App.UI.CreateDialog((Dialog)this._requestAcceptedDialog, null);
			}
			else if (turnEvent.EventType == TurnEventType.EV_REQUEST_DECLINED)
			{
				this._requestDeclinedDialog = new GenericTextDialog(this.App, App.Localize("@UI_DIPLOMACY_REQUEST_DECLINED_TITLE"), turnEvent.GetEventMessage(this.App.Game), "dialogGenericMessage");
				this.App.UI.CreateDialog((Dialog)this._requestDeclinedDialog, null);
			}
			else if (turnEvent.EventType == TurnEventType.EV_DEMAND_REQUESTED)
			{
				this._demandRequestedDialog = new DemandRequestedDialog(this.App, this.App.GameDatabase.GetDemandInfo(turnEvent.TreatyID), "dialogRequested");
				this.App.UI.CreateDialog((Dialog)this._demandRequestedDialog, null);
			}
			else if (turnEvent.EventType == TurnEventType.EV_DEMAND_ACCEPTED)
			{
				this._demandAcceptedDialog = new GenericTextDialog(this.App, App.Localize("@UI_DIPLOMACY_DEMAND_ACCEPTED_TITLE"), turnEvent.GetEventMessage(this.App.Game), "dialogGenericMessage");
				this.App.UI.CreateDialog((Dialog)this._demandAcceptedDialog, null);
			}
			else if (turnEvent.EventType == TurnEventType.EV_DEMAND_DECLINED)
			{
				this._demandDeclinedDialog = new GenericTextDialog(this.App, App.Localize("@UI_DIPLOMACY_DEMAND_DECLINED_TITLE"), turnEvent.GetEventMessage(this.App.Game), "dialogGenericMessage");
				this.App.UI.CreateDialog((Dialog)this._demandDeclinedDialog, null);
			}
			else if (turnEvent.EventType == TurnEventType.EV_TREATY_REQUESTED)
			{
				TreatyInfo treatyInfo = this.App.GameDatabase.GetTreatyInfos().ToList<TreatyInfo>().FirstOrDefault<TreatyInfo>((Func<TreatyInfo, bool>)(x => x.ID == turnEvent.TreatyID));
				if (treatyInfo == null)
					return;
				if (treatyInfo.Type == TreatyType.Limitation)
				{
					this._treatyRequestedDialog = (Dialog)new LimitationTreatyRequestedDialog(this.App, turnEvent.TreatyID);
					this.App.UI.CreateDialog(this._treatyRequestedDialog, null);
				}
				else
				{
					this._treatyRequestedDialog = (Dialog)new TreatyRequestedDialog(this.App, turnEvent.TreatyID);
					this.App.UI.CreateDialog(this._treatyRequestedDialog, null);
				}
			}
			else if (turnEvent.EventType == TurnEventType.EV_TREATY_ACCEPTED)
			{
				this._treatyAcceptedDialog = new GenericTextDialog(this.App, turnEvent.GetEventName(this.App.Game), turnEvent.GetEventMessage(this.App.Game), "dialogGenericMessage");
				this.App.UI.CreateDialog((Dialog)this._treatyAcceptedDialog, null);
			}
			else if (turnEvent.EventType == TurnEventType.EV_TREATY_DECLINED)
			{
				this._treatyDeclinedDialog = new GenericTextDialog(this.App, turnEvent.GetEventName(this.App.Game), turnEvent.GetEventMessage(this.App.Game), "dialogGenericMessage");
				this.App.UI.CreateDialog((Dialog)this._treatyDeclinedDialog, null);
			}
			else if (turnEvent.EventType == TurnEventType.EV_TREATY_EXPIRED)
			{
				this._treatyExpiredDialog = new GenericTextDialog(this.App, turnEvent.GetEventName(this.App.Game), turnEvent.GetEventMessage(this.App.Game), "dialogGenericMessage");
				this.App.UI.CreateDialog((Dialog)this._treatyExpiredDialog, null);
			}
			else if (turnEvent.EventType == TurnEventType.EV_TREATY_BROKEN_OFFENDER)
			{
				this._treatyBrokenDialogOffender = new LimitationTreatyBrokenDialog(this.App, turnEvent.TreatyID, true);
				this.App.UI.CreateDialog((Dialog)this._treatyBrokenDialogOffender, null);
			}
			else if (turnEvent.EventType == TurnEventType.EV_TREATY_BROKEN_VICTIM)
			{
				this._treatyBrokenDialogVictim = new LimitationTreatyBrokenDialog(this.App, turnEvent.TreatyID, false);
				this.App.UI.CreateDialog((Dialog)this._treatyBrokenDialogVictim, null);
			}
			else if (turnEvent.EventType == TurnEventType.EV_COLONY_SELF_SUFFICIENT)
			{
				if (this.App.GameDatabase.GetFleetInfo(turnEvent.FleetID) != null)
				{
					this._selfSufficientDialog = new ColonySelfSufficientDialog(this.App, turnEvent.OrbitalID, turnEvent.MissionID);
					this.App.UI.CreateDialog((Dialog)this._selfSufficientDialog, null);
				}
				else
					this.ShowNextEvent(false);
			}
			else if (turnEvent.EventType == TurnEventType.EV_COLONY_ESTABLISHED)
			{
				this._colonyEstablishedPlanet = turnEvent.OrbitalID;
				this._colonyEstablishedSystem = turnEvent.SystemID;
				this._dialogState = ESMDialogState.ESMD_ColonyEstablished;
				this._colonizeDialog = new ColonizeDialog(this.App, turnEvent.OrbitalID, false);
				this.App.UI.CreateDialog((Dialog)this._colonizeDialog, null);
			}
			else if (turnEvent.EventType == TurnEventType.EV_STATION_BUILT && this.App.GameDatabase.GetOrbitalObjectInfo(turnEvent.OrbitalID) != null)
			{
				this._stationDialog = new StationBuiltDialog(this.App, turnEvent.OrbitalID);
				this.App.UI.CreateDialog((Dialog)this._stationDialog, null);
			}
			else if (turnEvent.EventType == TurnEventType.EV_RESEARCH_COMPLETE)
				this._researchCompleteDialog = this.App.UI.CreateDialog((Dialog)new ResearchCompleteDialog(this.App, turnEvent.TechID), null);
			else if (turnEvent.EventType == TurnEventType.EV_SALVAGE_PROJECT_COMPLETE)
				this._researchCompleteDialog = this.App.UI.CreateDialog((Dialog)new SalvageCompleteDialog(this.App, turnEvent.TechID), null);
			else if (turnEvent.EventType == TurnEventType.EV_FEASIBILITY_STUDY_COMPLETE)
				this._feasibilityCompleteDialog = this.App.UI.CreateDialog((Dialog)new FeasibilityCompleteDialog(this.App, turnEvent), null);
			else if (turnEvent.EventType == TurnEventType.EV_SYSTEM_SURVEYED)
				this._surveyDialog = this.App.UI.CreateDialog((Dialog)new SystemSurveyDialog(this.App, turnEvent.SystemID, turnEvent.FleetID), null);
			else if (turnEvent.EventType == TurnEventType.EV_SURVEY_INDEPENDENT_RACE_FOUND)
			{
				this.App.UI.CreateDialog((Dialog)new IndependentFoundDialog(this.App, turnEvent.SystemID, turnEvent.ColonyID, turnEvent.TargetPlayerID), null);
			}
			else
			{
				if (turnEvent.EventType == TurnEventType.EV_SCRIPT_MESSAGE || turnEvent.EventType == TurnEventType.EV_NO_RESEARCH)
					return;
				if (turnEvent.EventType == TurnEventType.EV_SUPERWORLD_COMPLETE)
					this._superWorldDialog = this.App.UI.CreateDialog((Dialog)new SuperWorldDialog(this.App, turnEvent.ColonyID), null);
				else if (turnEvent.EventType == TurnEventType.EV_INTEL_MISSION_HIGHEST_TRADE_SYSTEM)
					this.App.UI.CreateDialog((Dialog)new DialogSystemIntel(this.App, turnEvent.SystemID, this.App.GameDatabase.GetPlayerInfo(turnEvent.TargetPlayerID), turnEvent.GetEventMessage(this.App.Game)), null);
				else if (turnEvent.EventType == TurnEventType.EV_INTEL_MISSION_NEWEST_COLONY_SYSTEM)
					this.App.UI.CreateDialog((Dialog)new DialogSystemIntel(this.App, turnEvent.SystemID, this.App.GameDatabase.GetPlayerInfo(turnEvent.TargetPlayerID), turnEvent.GetEventMessage(this.App.Game)), null);
				else if (turnEvent.EventType == TurnEventType.EV_INTEL_MISSION_RANDOM_SYSTEM)
					this.App.UI.CreateDialog((Dialog)new DialogSystemIntel(this.App, turnEvent.SystemID, this.App.GameDatabase.GetPlayerInfo(turnEvent.TargetPlayerID), turnEvent.GetEventMessage(this.App.Game)), null);
				else if (turnEvent.EventType == TurnEventType.EV_INTEL_MISSION_CRITICAL_SUCCESS)
					this.App.UI.CreateDialog((Dialog)new IntelCriticalSuccessDialog(this.App.Game, turnEvent.TargetPlayerID), null);
				else if (turnEvent.EventType == TurnEventType.EV_INTEL_MISSION_CURRENT_TECH)
					this.App.UI.CreateDialog((Dialog)new DialogTechIntel(this.App, turnEvent.TechID, this.App.GameDatabase.GetPlayerInfo(turnEvent.TargetPlayerID)), null);
				else if (turnEvent.EventType == TurnEventType.EV_VN_HW_DEFEATED)
				{
					this.App.UI.CreateDialog((Dialog)new GenericTextDialog(this.App, App.Localize("@UI_VN_HOMEWORLD_DEFEATED"), turnEvent.GetEventMessage(this.App.Game), "dialogGenericMessage"), null);
				}
				else
				{
					if (turnEvent.EventType == TurnEventType.EV_COUNTER_INTEL_CRITICAL_SUCCESS || turnEvent.EventType == TurnEventType.EV_COUNTER_INTEL_SUCCESS)
						return;
					if (turnEvent.EventType == TurnEventType.EV_SUPER_NOVA_TURN || turnEvent.EventType == TurnEventType.EV_SUPER_NOVA_DESTROYED_SYSTEM)
					{
						this.App.UI.CreateDialog((Dialog)new DialogSuperNova(this.App, turnEvent.Param1, turnEvent.ArrivalTurns, turnEvent.NumShips), null);
					}
					else
					{
						if (turnEvent.EventType != TurnEventType.EV_NEUTRON_STAR_NEARBY)
							return;
						this.App.UI.CreateDialog((Dialog)new DialogNeutronStar(this.App, turnEvent.NumShips), null);
					}
				}
			}
		}

		private bool SelectedSystemHasFriendlyColonyScreen()
		{
			int selectedSystem = this.SelectedSystem;
			return selectedSystem != 0 && this.App.GameDatabase.GetPlayerColonySystemIDs(this.App.LocalPlayer.ID).Contains<int>(selectedSystem);
		}

		public static bool SystemHasIndependentColony(GameSession App, int system)
		{
			int systemID = system;
			if (systemID == 0 || !App.GameDatabase.IsSurveyed(App.LocalPlayer.ID, systemID))
				return false;
			foreach (ColonyInfo colonyInfo in App.GameDatabase.GetColonyInfosForSystem(systemID).ToList<ColonyInfo>())
			{
				PlayerInfo playerInfo = App.GameDatabase.GetPlayerInfo(colonyInfo.PlayerID);
				if (App.AssetDatabase.GetFaction(playerInfo.FactionID).IsIndependent())
					return true;
			}
			return false;
		}

		private bool CanOpenBuildScreen()
		{
			return this.SelectedSystemHasFriendlyColonyScreen();
		}

		private bool CanOpenRepairScreen()
		{
			return true;
		}

		private void OpenBuildScreen()
		{
			if (!this.CanOpenBuildScreen())
				return;
			this.App.SwitchGameState<BuildScreenState>((object)this.SelectedSystem);
		}

		private void OpenDesignScreen()
		{
			this.App.SwitchGameState<DesignScreenState>((object)false, (object)nameof(StarMapState));
		}

		private void OpenBattleRiderManagerScreen()
		{
			this.App.SwitchGameState<RiderManagerState>((object)this.SelectedSystem);
		}

		private void OpenDiplomacyScreen()
		{
			this.App.SwitchGameState<DiplomacyScreenState>();
		}

		private void OpenSystemView()
		{
			this.App.SwitchGameState<StarSystemState>((object)this.SelectedSystem, (object)this.SelectedPlanet);
		}

		private void CacheStar(StarSystemInfo systemInfo)
		{
			if (this._cachedStar != null)
			{
				if (systemInfo == this._cachedStarInfo)
					return;
				this.App.ReleaseObject((IGameObject)this._cachedStar);
				this._cachedStar = (StarModel)null;
			}
			this._cachedStarInfo = systemInfo;
			this._cachedStarReady = false;
			this._cachedStar = StarSystem.CreateStar(this.App, Vector3.Zero, systemInfo, 1f, false);
			this._cachedStar.PostSetProp("AutoDraw", false);
		}

		private void CachePlanet(PlanetInfo planetInfo)
		{
			if (this._cachedPlanet != null)
			{
				if (PlanetInfo.AreSame(planetInfo, this._cachedPlanetInfo))
					return;
				this.App.ReleaseObject((IGameObject)this._cachedPlanet);
				this._cachedPlanet = (StellarBody)null;
			}
			this._cachedPlanetInfo = planetInfo;
			this._cachedPlanetReady = false;
			this._cachedPlanet = StarSystem.CreatePlanet(this.App.Game, Vector3.Zero, planetInfo, Matrix.Identity, 1f, false, StarSystem.TerrestrialPlanetQuality.High);
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

		public void ClearSelectedObject()
		{
			this.SelectedObject = (object)null;
		}

		public void RefreshMission()
		{
			this.SyncFleetArrows();
		}

		public void RefreshStarmap(StarMapState.StarMapRefreshType refreshtype = StarMapState.StarMapRefreshType.REFRESH_NORMAL)
		{
			EmpireBarUI.SyncTitleBar(this.App, "gameEmpireBar", this._piechart);
			this.App.UI.SetPropertyString("turn_count", "text", string.Format("{0} {1}", (object)App.Localize("@UI_GENERAL_TURN"), (object)this.App.GameDatabase.GetTurnCount()));
			if (this._TurnLastUpdated != this.App.GameDatabase.GetTurnCount() || refreshtype == StarMapState.StarMapRefreshType.REFRESH_ALL)
			{
				this._starmap.Sync(this._crits);
				this.SyncFleetArrows();
				this._TurnLastUpdated = this.App.GameDatabase.GetTurnCount();
			}
			this._planetWidget.Sync(this._selectedPlanet, false, false);
			StarMapState.UpdateGateUI(this.App.Game, "gameGateInfo");
			StarMapState.UpdateNPGUI(this.App.Game, "gameNGPInfo");
			this.RefreshEmpireBarColor();
			this.RefreshTechCube();
			int count = this.App.Game.GetUpgradableStations(this.App.GameDatabase.GetStationInfosByPlayerID(this.App.LocalPlayer.ID).ToList<StationInfo>()).Count;
			this.App.UI.SetText("numStationsReady", count.ToString());
			this.App.UI.SetVisible("numStationsReady", count > 0);
			if (count > this._prevNumStations)
				this.App.UI.SetPropertyBool("gameStationManagerButton", "flashing", true);
			else
				this.App.UI.SetPropertyBool("gameStationManagerButton", "flashing", false);
			this._prevNumStations = count;
			this.RefreshSystemInterface();
			App.Log.Warn("Starmap refreshed.", "state");
		}

		private void RefreshTechCube()
		{
			if (this._techCube != null)
			{
				this._techCube.SpinSpeed = (float)((1.0 - (double)this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID).RateGovernmentResearch) * 100.0 * (1.0 / 500.0));
				this._techCube.UpdateResearchProgress();
				this._techCube.RefreshResearchingTech();
			}
			if (this.App.GameDatabase.GetPlayerResearchingTechID(this.App.LocalPlayer.ID) == 0 && this.App.GameDatabase.GetPlayerFeasibilityStudyTechId(this.App.LocalPlayer.ID) == 0)
				this.App.UI.SetPropertyBool("gameResearchButton", "flashing", true);
			else
				this.App.UI.SetPropertyBool("gameResearchButton", "flashing", false);
			if (this.App.LocalPlayer.Faction.Name == "loa" && this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID).Savings < 0.0)
			{
				this.App.UI.SetVisible(this.App.UI.Path("gameResearchButton", "warning"), true);
				this.App.UI.SetEnabled("gameEmpireResearchSlider", false);
			}
			else
			{
				this.App.UI.SetVisible(this.App.UI.Path("gameResearchButton", "warning"), false);
				this.App.UI.SetEnabled("gameEmpireResearchSlider", true);
			}
			this.UpdateTechCubeToolTip();
		}

		private void UpdateTechCubeToolTip()
		{
			string str1 = App.Localize("@UI_RESEARCH_RESEARCHING");
			bool flag = true;
			int techId = this.App.GameDatabase.GetPlayerResearchingTechID(this.App.LocalPlayer.ID);
			if (techId == 0)
			{
				techId = this.App.GameDatabase.GetPlayerFeasibilityStudyTechId(this.App.LocalPlayer.ID);
				str1 = App.Localize("@UI_RESEARCH_STUDYING");
				flag = false;
			}
			string techIdStr = this.App.GameDatabase.GetTechFileID(techId);
			PlayerTechInfo playerTechInfo = this.App.GameDatabase.GetPlayerTechInfo(this.App.LocalPlayer.ID, techId);
			Tech tech = this.App.AssetDatabase.MasterTechTree.Technologies.FirstOrDefault<Tech>((Func<Tech, bool>)(x => x.Id == techIdStr));
			if (tech != null && playerTechInfo != null)
			{
				string str2 = "";
				if (flag)
					str2 = " -  " + ResearchScreenState.GetTurnsToCompleteString(this.App, this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID), playerTechInfo);
				this.App.UI.SetTooltip("researchCubeButton", str1 + " " + tech.Name + str2);
			}
			else
				this.App.UI.SetTooltip("researchCubeButton", App.Localize("@UI_TOOLTIP_RESEARCHCUBE"));
		}

		private void RefreshEmpireBarColor()
		{
			Vector3 primaryColor = this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID).PrimaryColor;
			Vector3 secondaryColor = this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID).SecondaryColor;
			this.App.UI.SetPropertyColorNormalized(this.App.UI.Path("gameEmpireBar", "LC"), "color", primaryColor);
			this.App.UI.SetPropertyColorNormalized(this.App.UI.Path("gameEmpireBar", "RC"), "color", primaryColor);
			this.App.UI.SetPropertyColorNormalized(this.App.UI.Path("gameEmpireBar", "BG"), "color", primaryColor);
			this.App.UI.SetPropertyColorNormalized(this.App.UI.Path("gameEmpireBar", "RC2"), "color", secondaryColor);
			this.App.UI.SetPropertyColorNormalized(this.App.UI.Path("gameEmpireBar", "boxback.TLC"), "color", primaryColor * 0.5f);
			this.App.UI.SetPropertyColorNormalized(this.App.UI.Path("gameEmpireBar", "boxback.TRC"), "color", primaryColor * 0.5f);
			this.App.UI.SetPropertyColorNormalized(this.App.UI.Path("gameEmpireBar", "boxback.BLC"), "color", primaryColor * 0.5f);
			this.App.UI.SetPropertyColorNormalized(this.App.UI.Path("gameEmpireBar", "boxback.BRC"), "color", primaryColor * 0.5f);
			this.App.UI.SetPropertyColorNormalized(this.App.UI.Path("gameEmpireBar", "boxback.BG1"), "color", primaryColor * 0.5f);
			this.App.UI.SetPropertyColorNormalized(this.App.UI.Path("gameEmpireBar", "boxback.BG2"), "color", primaryColor * 0.5f);
			this.App.UI.SetPropertyColorNormalized(this.App.UI.Path("gameEmpireBar", "boxback.BG3"), "color", primaryColor * 0.5f);
			this.App.UI.SetPropertyColorNormalized(this.App.UI.Path("gameEmpireBar", "boxback.BG4"), "color", primaryColor * 0.5f);
			this.App.UI.SetPropertyColorNormalized(this.App.UI.Path("gameEmpireBar", "boxback.BG5"), "color", primaryColor * 0.5f);
			this.App.UI.SetPropertyColorNormalized(this.App.UI.Path("gameEmpireBar", "boxback.BOL1"), "color", primaryColor);
			this.App.UI.SetPropertyColorNormalized(this.App.UI.Path("gameEmpireBar", "boxback.BOL2"), "color", primaryColor);
			this.App.UI.SetPropertyColorNormalized(this.App.UI.Path("gameEmpireBar", "boxback.BOL3"), "color", primaryColor);
			this.App.UI.SetPropertyColorNormalized(this.App.UI.Path("gameEmpireBar", "boxback.BOL4"), "color", primaryColor);
			this.App.UI.SetPropertyColorNormalized(this.App.UI.Path("gameEmpireBar", "boxback.BOL5"), "color", primaryColor);
			this.App.UI.SetPropertyColorNormalized(this.App.UI.Path("gameEmpireBar", "boxback.BOL6"), "color", primaryColor);
			this.App.UI.SetPropertyColorNormalized(this.App.UI.Path("gameEmpireBar", "boxback.BOL7"), "color", primaryColor);
			this.App.UI.SetPropertyColorNormalized(this.App.UI.Path("gameEmpireBar", "boxback.BOL8"), "color", primaryColor);
		}

		public void SetEndTurnTimeout(int delay)
		{
			this.App.UI.SetPropertyInt(this.App.UI.Path("gameEndTurnButton"), "timeout", delay);
		}

		public void SetOffsetViewMode(bool enabled)
		{
			this.App.UI.SetVisible("OH_StarMap", !enabled);
			this.App.UI.SetVisible("OH_StarMap_Offset", enabled);
		}

		protected override void OnEnter()
		{
			this.App.UI.UnlockUI();
			this.App.UI.SetScreen("StarMap");
			this.App.UI.Send((object)"SetGameObject", (object)"OH_StarMap", (object)this._starmap.ObjectID);
			this.App.UI.Send((object)"SetGameObject", (object)"OH_StarMap_Offset", (object)this._starmap.ObjectID);
			this.RefreshCameraControl();
			this.SetProvinceMode(false);
			this.PopulateViewFilterList();
			if (this.App.UserProfile.AutoPlaceDefenseAssets != this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID).AutoPlaceDefenseAssets)
				this.App.GameDatabase.UpdatePlayerAutoPlaceDefenses(this.App.LocalPlayer.ID, this.App.UserProfile.AutoPlaceDefenseAssets);
			if (this.App.UserProfile.AutoRepairFleets != this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID).AutoRepairShips)
				this.App.GameDatabase.UpdatePlayerAutoRepairFleets(this.App.LocalPlayer.ID, this.App.UserProfile.AutoRepairFleets);
			if (this.App.UserProfile.AutoUseGoop != this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID).AutoUseGoopModules)
				this.App.GameDatabase.UpdatePlayerAutoUseGoop(this.App.LocalPlayer.ID, this.App.UserProfile.AutoUseGoop);
			if (this.App.UserProfile.AutoUseJoker != this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID).AutoUseJokerModules)
				this.App.GameDatabase.UpdatePlayerAutoUseJoker(this.App.LocalPlayer.ID, this.App.UserProfile.AutoUseJoker);
			if (this.App.UserProfile.AutoAOE != this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID).AutoAoe)
				this.App.GameDatabase.UpdatePlayerAutoUseAOE(this.App.LocalPlayer.ID, this.App.UserProfile.AutoAOE);
			if (this.App.UserProfile.AutoPatrol != this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID).AutoPatrol)
				this.App.GameDatabase.UpdatePlayerAutoPatrol(this.App.LocalPlayer.ID, this.App.UserProfile.AutoPatrol);
			if (!this.App.Game.HomeworldNamed && this.App.GameDatabase.GetTurnCount() == 1)
			{
				int? nullable = new int?(this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID).Homeworld.Value);
				if (nullable.HasValue)
				{
					this._eventDialogShown = true;
					this._colonizeDialog = new ColonizeDialog(this.App, nullable.Value, true);
					this.App.UI.CreateDialog((Dialog)this._colonizeDialog, null);
					this.App.Game.HomeworldNamed = true;
					this._dialogState = ESMDialogState.ESMD_None;
					this.SetSelectedSystem(this.App.GameDatabase.GetPlayerHomeworld(this.App.LocalPlayer.ID).SystemID, false);
				}
			}
			this.App.UI.GameEvent += new UIEventGameEvent(this.UICommChannel_GameEvent);
			EmpireBarUI.SyncTitleBar(this.App, "gameEmpireBar", this._piechart);
			EmpireBarUI.SyncTitleFrame(this.App);
			this.App.UI.SetPropertyBool("gameDesignButton", "lockout_button", true);
			this.App.UI.SetPropertyBool("gameResearchButton", "lockout_button", true);
			this.App.UI.SetPropertyBool("gameStationManagerButton", "lockout_button", true);
			this.App.UI.SetPropertyBool("gamePlanetSummaryButton", "lockout_button", true);
			this.App.UI.SetPropertyBool("gamePopulationManagerButton", "lockout_button", true);
			this.App.UI.SetPropertyBool(this.App.UI.Path("colonyDetailsWidget", "partTradeSlider"), "only_user_events", true);
			this.App.UI.SetPropertyBool(this.App.UI.Path("colonyDetailsWidget", "partTerraSlider"), "only_user_events", true);
			this.App.UI.SetPropertyBool(this.App.UI.Path("colonyDetailsWidget", "partInfraSlider"), "only_user_events", true);
			this.App.UI.SetPropertyBool(this.App.UI.Path("colonyDetailsWidget", "partShipConSlider"), "only_user_events", true);
			this.App.UI.SetPropertyBool(this.App.UI.Path("colonyDetailsWidget", "partCivSlider"), "only_user_events", true);
			this.App.UI.SetPropertyBool(this.App.UI.Path("colonyDetailsWidget", "partOverDevelopment"), "only_user_events", true);
			this.App.UI.SetPropertyBool(this.App.UI.Path("colony_event_dialog", "partTradeSlider"), "only_user_events", true);
			this.App.UI.SetPropertyBool(this.App.UI.Path("colony_event_dialog", "partTerraSlider"), "only_user_events", true);
			this.App.UI.SetPropertyBool(this.App.UI.Path("colony_event_dialog", "partInfraSlider"), "only_user_events", true);
			this.App.UI.SetPropertyBool(this.App.UI.Path("colony_event_dialog", "partShipConSlider"), "only_user_events", true);
			this.App.UI.SetPropertyBool(this.App.UI.Path("colony_event_dialog", "partCivSlider"), "only_user_events", true);
			this.App.UI.SetPropertyBool(this.App.UI.Path("colony_event_dialog", "partOverDevelopment"), "only_user_events", true);
			this.SetEndTurnTimeout(this.App.GameSettings.EndTurnDelay);
			this.EnableFleetCheck = this.App.GameSettings.CheckForInactiveFleets;
			PlayerInfo playerInfo = this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID);
			if (this.SelectedSystem == 0 && playerInfo.Homeworld.HasValue)
			{
				OrbitalObjectInfo orbitalObjectInfo = this.App.GameDatabase.GetOrbitalObjectInfo(playerInfo.Homeworld.Value);
				this.SelectedObject = (object)this.App.GameDatabase.GetStarSystemInfo(orbitalObjectInfo.StarSystemID);
				this.SetSelectedSystem(orbitalObjectInfo.StarSystemID, false);
			}
			this._crits.Activate();
			EmpireBarUI.SyncTitleBar(this.App, "gameEmpireBar", this._piechart);
			this.App.PostPlayMusic(string.Format("Ambient_{0}", (object)this.App.GameDatabase.GetFactionName(this.App.GameDatabase.GetPlayerFactionID(this.App.LocalPlayer.ID))));
			if (ScriptHost.AllowConsole)
				this.App.UI.SetVisible("debugTestCombatButton", true);
			else
				this.App.UI.SetVisible("debugTestCombatButton", false);
			this.App.Network.EnableChatWidgetPlayerList(true);
			this.App.UI.SetSelection("viewModeDropdown", (int)this._lastFilterSelection);
			this._starmap.ViewFilter = this._lastFilterSelection;
			if (!this._playerWidget.Initialized)
				this._playerWidget.Initialize();
			if (this.App.PreviousState == this.App.GetGameState<CombatState>() || this.App.PreviousState == this.App.GetGameState<SimCombatState>())
			{
				PendingCombat currentCombat = this.App.Game.GetCurrentCombat();
				if (this.App.GameSetup.IsMultiplayer && currentCombat != null)
					this.App.Network.CombatComplete(currentCombat.ConflictID);
				this.App.Game.CombatComplete();
			}
			if (this.App.Game.TurnEvents.Count == 0)
			{
				this.App.Game.TurnEvents = this.App.GameDatabase.GetTurnEventsByTurnNumber(this.App.GameDatabase.GetTurnCount() - 1, this.App.LocalPlayer.ID).OrderByDescending<TurnEvent, int>((Func<TurnEvent, int>)(x =>
			   {
				   if (!x.ShowsDialog)
					   return 0;
				   return !x.IsCombatEvent ? 1 : 2;
			   })).ToList<TurnEvent>();
				for (int index = 0; index < this.App.Game.TurnEvents.Count<TurnEvent>() - 1; ++index)
				{
					this.App.Game.TurnEvents[index].dialogShown = true;
					if (!this.App.Game.TurnEvents[index].ShowsDialog && this.App.Game.TurnEvents[index + 1].ShowsDialog)
					{
						TurnEvent turnEvent = this.App.Game.TurnEvents[index + 1];
						this.App.Game.TurnEvents[index + 1] = this.App.Game.TurnEvents[index];
						this.App.Game.TurnEvents[index] = turnEvent;
						index = -1;
					}
				}
			}
			if (this.App.Game.State == SimState.SS_PLAYER)
				this.TurnStarted();
			else
				this.EnableNextTurnButton(false);
			this.RefreshStarmap(StarMapState.StarMapRefreshType.REFRESH_NORMAL);
			FleetUI.SyncFleetAndPlanetListWidget(this.App.Game, "fleetAndPlanetDetailsWidget", this.SelectedSystem, true);
			EncounterDialog._starmap = this._starmap;
			this.App.HotKeyManager.AddListener((IKeyBindListener)this);
		}

		public void Reset()
		{
			this.App.UI.GameEvent -= new UIEventGameEvent(this.UICommChannel_GameEvent);
			this._piechart = (BudgetPiechart)null;
			this._fleetWidget.OnFleetSelectionChanged -= new FleetWidget.FleetSelectionChangedDelegate(this.OnFleetWidgetFleetSelected);
			this._fleetWidget.Dispose();
			if (this._crits != null)
			{
				this._crits.Dispose();
				this._crits = (GameObjectSet)null;
			}
			this._painter.Dispose();
			this._starmap = (StarMap)null;
			this._planetView = (PlanetView)null;
			if (this._cachedPlanet != null)
				this.App.ReleaseObject((IGameObject)this._cachedPlanet);
			this._cachedPlanet = (StellarBody)null;
			this._cachedPlanetReady = false;
			if (this._cachedStar != null)
				this.App.ReleaseObject((IGameObject)this._cachedStar);
			this._cachedStar = (StarModel)null;
			this._cachedStarReady = false;
			this._playerWidget.Terminate();
			this._planetWidget.Terminate();
			foreach (Dialog dialog in this._missionOverlays.Values)
				this.App.UI.CloseDialog(dialog, true);
			this._missionOverlays = (Dictionary<string, OverlayMission>)null;
			this.App.UI.CloseDialog((Dialog)this._reactionOverlay, true);
			this._reactionOverlay = (OverlayMission)null;
			if (this._starmap != null)
				this._starmap.ViewFilter = StarMapViewFilter.VF_STANDARD;
			this._lastFilterSelection = StarMapViewFilter.VF_STANDARD;
			this._TurnLastUpdated = -1;
			this.App.UI.DestroyPanel(this._contextMenuID);
			this.App.UI.DestroyPanel(this._researchContextID);
			this.App.UI.DestroyPanel(this._fleetContextMenuID);
			this.App.UI.DestroyPanel(this._enemyContextMenuID);
			this.App.UI.DestroyPanel(this._enemyGMStationContextMenuID);
			this.App.UI.DeleteScreen("StarMap");
			this.App.UI.PurgeFleetWidgetCache();
			this._initialized = false;
		}

		protected override void OnExit(GameState prev, ExitReason reason)
		{
			this.App.HotKeyManager.RemoveListener((IKeyBindListener)this);
			if (!this._initialized)
				return;
			EncounterDialog._starmap = (StarMap)null;
			this._crits.Deactivate();
			this.App.UI.GameEvent -= new UIEventGameEvent(this.UICommChannel_GameEvent);
			if (this._cachedPlanet != null)
				this._cachedPlanet.Active = false;
			this._cachedPlanetReady = false;
			if (this._cachedStar != null)
				this._cachedStar.Active = false;
			this._cachedStarReady = false;
			foreach (Dialog dialog in this._missionOverlays.Values)
				this.App.UI.CloseDialog(dialog, true);
		}

		protected override void OnUpdate()
		{
			if (this._colonizeDialog != null)
				this._colonizeDialog.Update();
			this._planetWidget.Update();
			this.UpdateCachedPlanet();
			this.UpdateCachedStar();
			this._playerWidget.Sync();
			this._fleetWidget.OnUpdate();
			if (this._painter.ObjectStatus == GameObjectStatus.Ready && !this._painter.Active)
			{
				this._painter.Active = true;
				this._starmap.PostObjectAddObjects((IGameObject)this._painter);
			}
			if (GameSession.SimAITurns <= 0 || this._simNewTurnTick <= 0)
				return;
			--this._simNewTurnTick;
			if (this._simNewTurnTick > 0)
				return;
			this.EndTurn(false);
		}

		public override bool IsReady()
		{
			return this._crits != null && this._crits.IsReady() && base.IsReady();
		}

		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}

		public bool OnKeyBindPressed(HotKeyManager.HotKeyActions action, string gamestates)
		{
			if (gamestates.Contains(this.Name) && (!this._missionOverlays.Any<KeyValuePair<string, OverlayMission>>((Func<KeyValuePair<string, OverlayMission>, bool>)(x => x.Value.GetShown())) && this.App.UI.GetTopDialog() == null))
			{
				switch (action)
				{
					case HotKeyManager.HotKeyActions.State_Starmap:
						this.App.UI.LockUI();
						this.App.SwitchGameState<StarMapState>();
						return true;
					case HotKeyManager.HotKeyActions.State_BuildScreen:
						if (this.SelectedSystem == 0 || !this.CanOpenBuildScreen())
							return false;
						this.App.UI.LockUI();
						this.App.SwitchGameState<BuildScreenState>((object)this.SelectedSystem);
						return true;
					case HotKeyManager.HotKeyActions.State_DesignScreen:
						this.App.UI.LockUI();
						this.OpenDesignScreen();
						return true;
					case HotKeyManager.HotKeyActions.State_ResearchScreen:
						this.App.UI.LockUI();
						this.App.SwitchGameState<ResearchScreenState>();
						return true;
					case HotKeyManager.HotKeyActions.State_ComparativeAnalysysScreen:
						if (this.App.GameDatabase.GetStratModifier<bool>(StratModifiers.ComparativeAnalysys, this.App.LocalPlayer.ID) && this.App.GameDatabase.GetDesignsEncountered(this.App.LocalPlayer.ID).Any<DesignInfo>((Func<DesignInfo, bool>)(x => x.Class != ShipClass.Station)))
						{
							this.App.UI.LockUI();
							this.App.SwitchGameState<ComparativeAnalysysState>((object)false, (object)nameof(StarMapState));
						}
						return true;
					case HotKeyManager.HotKeyActions.State_EmpireSummaryScreen:
						this.App.UI.LockUI();
						this.App.SwitchGameState<EmpireSummaryState>();
						return true;
					case HotKeyManager.HotKeyActions.State_SotspediaScreen:
						this.App.UI.LockUI();
						this.App.SwitchGameState<SotspediaState>();
						return true;
					case HotKeyManager.HotKeyActions.State_StarSystemScreen:
						if (this.SelectedSystem == 0)
							return false;
						this.App.UI.LockUI();
						this.App.SwitchGameState<StarSystemState>((object)this.SelectedSystem, (object)this.SelectedPlanet);
						return true;
					case HotKeyManager.HotKeyActions.State_FleetManagerScreen:
						if (this.SelectedSystem == 0 || !this.CanOpenFleetManager(this.SelectedSystem))
							return false;
						this.App.UI.LockUI();
						this.App.SwitchGameState<FleetManagerState>((object)this.SelectedSystem);
						return true;
					case HotKeyManager.HotKeyActions.State_DefenseManagerScreen:
						if (this.SelectedSystem == 0 || !GameSession.PlayerPresentInSystem(this.App.GameDatabase, this.App.LocalPlayer.ID, this.SelectedSystem))
							return false;
						this.App.UI.LockUI();
						this.App.SwitchGameState<DefenseManagerState>((object)this.SelectedSystem);
						return true;
					case HotKeyManager.HotKeyActions.State_BattleRiderScreen:
						if (this.SelectedSystem == 0 || !this.CanOpenRiderManager() || !this.App.GameDatabase.PlayerHasTech(this.App.LocalPlayer.ID, "BRD_BattleRiders"))
							return false;
						this.App.UI.LockUI();
						this.App.SwitchGameState<RiderManagerState>((object)this.SelectedSystem);
						return true;
					case HotKeyManager.HotKeyActions.State_DiplomacyScreen:
						this.App.UI.LockUI();
						this.App.SwitchGameState<DiplomacyScreenState>();
						return true;
					case HotKeyManager.HotKeyActions.Starmap_EndTurn:
						this.EndTurn(false);
						return true;
					case HotKeyManager.HotKeyActions.Starmap_NextFleet:
						List<FleetInfo> list1 = this.App.GameDatabase.GetFleetInfosByPlayerID(this.App.LocalPlayer.ID, FleetType.FL_NORMAL | FleetType.FL_CARAVAN).Where<FleetInfo>((Func<FleetInfo, bool>)(x => x.SystemID == 0)).ToList<FleetInfo>().OrderBy<FleetInfo, string>((Func<FleetInfo, string>)(x => x.Name)).ToList<FleetInfo>();
						if (list1.Any<FleetInfo>())
						{
							int sel = this.SelectedFleet;
							sel = !list1.Any<FleetInfo>((Func<FleetInfo, bool>)(x => x.ID == sel)) || list1.Last<FleetInfo>().ID == sel ? list1.First<FleetInfo>().ID : list1[list1.IndexOf(list1.FirstOrDefault<FleetInfo>((Func<FleetInfo, bool>)(x => x.ID == sel))) + 1].ID;
							this.SetSelectedFleet(sel);
						}
						return true;
					case HotKeyManager.HotKeyActions.Starmap_LastFleet:
						List<FleetInfo> list2 = this.App.GameDatabase.GetFleetInfosByPlayerID(this.App.LocalPlayer.ID, FleetType.FL_NORMAL | FleetType.FL_CARAVAN).Where<FleetInfo>((Func<FleetInfo, bool>)(x => x.SystemID == 0)).ToList<FleetInfo>().OrderByDescending<FleetInfo, string>((Func<FleetInfo, string>)(x => x.Name)).ToList<FleetInfo>();
						if (list2.Any<FleetInfo>())
						{
							int sel = this.SelectedFleet;
							sel = !list2.Any<FleetInfo>((Func<FleetInfo, bool>)(x => x.ID == sel)) || list2.Last<FleetInfo>().ID == sel ? list2.First<FleetInfo>().ID : list2[list2.IndexOf(list2.FirstOrDefault<FleetInfo>((Func<FleetInfo, bool>)(x => x.ID == sel))) + 1].ID;
							this.SetSelectedFleet(sel);
						}
						return true;
					case HotKeyManager.HotKeyActions.Starmap_NextIdleFleet:
						List<FleetInfo> list3 = this.App.GameDatabase.GetFleetInfosByPlayerID(this.App.LocalPlayer.ID, FleetType.FL_NORMAL).Where<FleetInfo>((Func<FleetInfo, bool>)(x =>
					   {
						   if (x.SystemID != 0)
							   return this.App.GameDatabase.GetMissionByFleetID(x.ID) == null;
						   return false;
					   })).ToList<FleetInfo>().OrderBy<FleetInfo, string>((Func<FleetInfo, string>)(x => x.Name)).ToList<FleetInfo>();
						if (list3.Any<FleetInfo>())
						{
							int sel = this.SelectedFleet;
							sel = !list3.Any<FleetInfo>((Func<FleetInfo, bool>)(x => x.ID == sel)) || list3.Last<FleetInfo>().ID == sel ? list3.First<FleetInfo>().ID : list3[list3.IndexOf(list3.FirstOrDefault<FleetInfo>((Func<FleetInfo, bool>)(x => x.ID == sel))) + 1].ID;
							this.SetSelectedFleet(sel);
						}
						return true;
					case HotKeyManager.HotKeyActions.Starmap_LastIdleFleet:
						List<FleetInfo> list4 = this.App.GameDatabase.GetFleetInfosByPlayerID(this.App.LocalPlayer.ID, FleetType.FL_NORMAL).Where<FleetInfo>((Func<FleetInfo, bool>)(x =>
					   {
						   if (x.SystemID != 0)
							   return this.App.GameDatabase.GetMissionByFleetID(x.ID) == null;
						   return false;
					   })).ToList<FleetInfo>().OrderByDescending<FleetInfo, string>((Func<FleetInfo, string>)(x => x.Name)).ToList<FleetInfo>();
						if (list4.Any<FleetInfo>())
						{
							int sel = this.SelectedFleet;
							sel = !list4.Any<FleetInfo>((Func<FleetInfo, bool>)(x => x.ID == sel)) || list4.Last<FleetInfo>().ID == sel ? list4.First<FleetInfo>().ID : list4[list4.IndexOf(list4.FirstOrDefault<FleetInfo>((Func<FleetInfo, bool>)(x => x.ID == sel))) + 1].ID;
							this.SetSelectedFleet(sel);
						}
						return true;
					case HotKeyManager.HotKeyActions.Starmap_NextSystem:
						List<StarSystemInfo> list5 = this.App.GameDatabase.GetStarSystemInfos().Where<StarSystemInfo>((Func<StarSystemInfo, bool>)(x =>
					   {
						   int? systemOwningPlayer = this.App.GameDatabase.GetSystemOwningPlayer(x.ID);
						   int id = this.App.LocalPlayer.ID;
						   return systemOwningPlayer.GetValueOrDefault() == id & systemOwningPlayer.HasValue;
					   })).ToList<StarSystemInfo>().OrderBy<StarSystemInfo, string>((Func<StarSystemInfo, string>)(x => x.Name)).ToList<StarSystemInfo>();
						if (list5.Any<StarSystemInfo>())
						{
							int sel = this.SelectedSystem;
							sel = !list5.Any<StarSystemInfo>((Func<StarSystemInfo, bool>)(x => x.ID == sel)) || list5.Last<StarSystemInfo>().ID == sel ? list5.First<StarSystemInfo>().ID : list5[list5.IndexOf(list5.FirstOrDefault<StarSystemInfo>((Func<StarSystemInfo, bool>)(x => x.ID == sel))) + 1].ID;
							this.SetSelectedSystem(sel, false);
						}
						return true;
					case HotKeyManager.HotKeyActions.Starmap_LastSystem:
						List<StarSystemInfo> list6 = this.App.GameDatabase.GetStarSystemInfos().Where<StarSystemInfo>((Func<StarSystemInfo, bool>)(x =>
					   {
						   int? systemOwningPlayer = this.App.GameDatabase.GetSystemOwningPlayer(x.ID);
						   int id = this.App.LocalPlayer.ID;
						   return systemOwningPlayer.GetValueOrDefault() == id & systemOwningPlayer.HasValue;
					   })).ToList<StarSystemInfo>().OrderByDescending<StarSystemInfo, string>((Func<StarSystemInfo, string>)(x => x.Name)).ToList<StarSystemInfo>();
						if (list6.Any<StarSystemInfo>())
						{
							int sel = this.SelectedSystem;
							sel = !list6.Any<StarSystemInfo>((Func<StarSystemInfo, bool>)(x => x.ID == sel)) || list6.Last<StarSystemInfo>().ID == sel ? list6.First<StarSystemInfo>().ID : list6[list6.IndexOf(list6.FirstOrDefault<StarSystemInfo>((Func<StarSystemInfo, bool>)(x => x.ID == sel))) + 1].ID;
							this.SetSelectedSystem(sel, false);
						}
						return true;
					case HotKeyManager.HotKeyActions.Starmap_NextIncomingFleet:
						List<int> list7 = this.App.GameDatabase.GetColonyInfos().Where<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == this.App.LocalPlayer.ID)).Select<ColonyInfo, int>((Func<ColonyInfo, int>)(x => x.CachedStarSystemID)).ToList<int>();
						List<FleetInfo> list8 = this.App.GameDatabase.GetFleetInfos(FleetType.FL_NORMAL).Where<FleetInfo>((Func<FleetInfo, bool>)(x =>
					   {
						   if (x.SystemID == 0 && x.PlayerID != this.App.LocalPlayer.ID)
							   return this.App.GameDatabase.GetDiplomacyStateBetweenPlayers(x.PlayerID, this.App.LocalPlayer.ID) == DiplomacyState.WAR;
						   return false;
					   })).ToList<FleetInfo>();
						List<FleetInfo> source = new List<FleetInfo>();
						foreach (FleetInfo fleetInfo in list8)
						{
							MoveOrderInfo orderInfoByFleetId = this.App.GameDatabase.GetMoveOrderInfoByFleetID(fleetInfo.ID);
							if (orderInfoByFleetId != null && list7.Contains(orderInfoByFleetId.ToSystemID))
								source.Add(fleetInfo);
						}
						List<FleetInfo> list9 = source.OrderByDescending<FleetInfo, string>((Func<FleetInfo, string>)(x => x.Name)).ToList<FleetInfo>();
						if (list9.Any<FleetInfo>())
						{
							int sel = this.SelectedFleet;
							sel = !list9.Any<FleetInfo>((Func<FleetInfo, bool>)(x => x.ID == sel)) || list9.Last<FleetInfo>().ID == sel ? list9.First<FleetInfo>().ID : list9[list9.IndexOf(list9.FirstOrDefault<FleetInfo>((Func<FleetInfo, bool>)(x => x.ID == sel))) + 1].ID;
							this.SetSelectedFleet(sel);
						}
						return true;
					case HotKeyManager.HotKeyActions.Starmap_OpenFleetManager:
						if (this.App.UI.GetTopDialog() == null)
							this.App.UI.CreateDialog((Dialog)new FleetSummaryDialog(this.App, "FleetSummaryDialog"), null);
						return true;
					case HotKeyManager.HotKeyActions.Starmap_OpenPlanetManager:
						if (this.App.UI.GetTopDialog() == null)
							this.App.UI.CreateDialog((Dialog)new PlanetManagerDialog(this.App, "dialogPlanetManager"), null);
						return true;
					case HotKeyManager.HotKeyActions.Starmap_OpenStationManager:
						if (this.App.UI.GetTopDialog() == null)
							this.App.UI.CreateDialog((Dialog)new StationManagerDialog(this.App, this, 0, "dialogStationManager"), null);
						return true;
					case HotKeyManager.HotKeyActions.Starmap_OpenRepairScreen:
						if (this.App.UI.GetTopDialog() == null)
						{
							if (this.SelectedSystem != 0)
								this.App.UI.CreateDialog((Dialog)new RepairShipsDialog(this.App, this.SelectedSystem, this.App.GameDatabase.GetFleetInfoBySystemID(this.SelectedSystem, FleetType.FL_ALL).ToList<FleetInfo>(), "dialogRepairShips"), null);
							else if (this.SelectedFleet != 0)
								this.App.UI.CreateDialog((Dialog)new RepairShipsDialog(this.App, this.SelectedSystem, new List<FleetInfo>()
				{
				  this.App.GameDatabase.GetFleetInfo(this.SelectedFleet)
				}, "dialogRepairShips"), null);
						}
						return true;
					case HotKeyManager.HotKeyActions.Starmap_OpenPopulationManager:
						if (this.App.UI.GetTopDialog() == null)
							this.App.UI.CreateDialog((Dialog)new PopulationManagerDialog(this.App, this.SelectedSystem, "dialogPopulationManager"), null);
						return true;
					case HotKeyManager.HotKeyActions.Starmap_NormalViewFilter:
						StarMapViewFilter starMapViewFilter1 = StarMapViewFilter.VF_STANDARD;
						this._starmap.ViewFilter = starMapViewFilter1;
						this._lastFilterSelection = starMapViewFilter1;
						this.RefreshSystemInterface();
						return true;
					case HotKeyManager.HotKeyActions.Starmap_SurveyViewFilter:
						StarMapViewFilter starMapViewFilter2 = StarMapViewFilter.VF_SURVEY;
						this._starmap.ViewFilter = starMapViewFilter2;
						this._lastFilterSelection = starMapViewFilter2;
						this.RefreshSystemInterface();
						return true;
					case HotKeyManager.HotKeyActions.Starmap_ProvinceFilter:
						StarMapViewFilter starMapViewFilter3 = StarMapViewFilter.VF_PROVINCE;
						this._starmap.ViewFilter = starMapViewFilter3;
						this._lastFilterSelection = starMapViewFilter3;
						this.RefreshSystemInterface();
						return true;
					case HotKeyManager.HotKeyActions.Starmap_SupportRangeFilter:
						StarMapViewFilter starMapViewFilter4 = StarMapViewFilter.VF_SUPPORT_RANGE;
						this._starmap.ViewFilter = starMapViewFilter4;
						this._lastFilterSelection = starMapViewFilter4;
						this.RefreshSystemInterface();
						return true;
					case HotKeyManager.HotKeyActions.Starmap_SensorRangeFilter:
						StarMapViewFilter starMapViewFilter5 = StarMapViewFilter.VF_SENSOR_RANGE;
						this._starmap.ViewFilter = starMapViewFilter5;
						this._lastFilterSelection = starMapViewFilter5;
						this.RefreshSystemInterface();
						return true;
					case HotKeyManager.HotKeyActions.Starmap_TerrainFilter:
						StarMapViewFilter starMapViewFilter6 = StarMapViewFilter.VF_TERRAIN;
						this._starmap.ViewFilter = starMapViewFilter6;
						this._lastFilterSelection = starMapViewFilter6;
						this.RefreshSystemInterface();
						return true;
					case HotKeyManager.HotKeyActions.Starmap_TradeViewFilter:
						if (this.App.GetStratModifier<bool>(StratModifiers.EnableTrade, this.App.LocalPlayer.ID))
						{
							StarMapViewFilter starMapViewFilter7 = StarMapViewFilter.VF_TRADE;
							this._starmap.ViewFilter = starMapViewFilter7;
							this._lastFilterSelection = starMapViewFilter7;
							this.RefreshSystemInterface();
						}
						return true;
					case HotKeyManager.HotKeyActions.Starmap_NextNewsEvent:
						this.ShowNextEvent(false);
						this.FocusLastEvent();
						return true;
					case HotKeyManager.HotKeyActions.Starmap_LastNewsEvent:
						this.ShowNextEvent(true);
						this.FocusLastEvent();
						return true;
					case HotKeyManager.HotKeyActions.Starmap_OpenMenu:
						if (this.App.UI.GetTopDialog() == null)
							this.App.UI.CreateDialog((Dialog)new MainMenuDialog(this.App), null);
						return true;
				}
			}
			return false;
		}

		public delegate void ObjectSelectionChangedDelegate(App app, int SelectedObject);

		public enum StarMapRefreshType
		{
			REFRESH_NORMAL,
			REFRESH_ALL,
		}
	}
}
