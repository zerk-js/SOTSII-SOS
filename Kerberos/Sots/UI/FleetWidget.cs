// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.FleetWidget
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kerberos.Sots.UI
{
	[GameObjectType(InteropGameObjectType.IGOT_FLEETWIDGET)]
	internal class FleetWidget : GameObject, IDisposable
	{
		private bool _enabled = true;
		private bool _enableAdmiralButton = true;
		private bool _enableRightClick = true;
		private bool _separateDefenseFleet = true;
		private bool _showEmptyFleets = true;
		private bool _showFleetInfo = true;
		private List<int> AccountedSystems = new List<int>();
		private List<int> _shipsToScrap = new List<int>();
		private App _game;
		public FleetWidget.FleetSelectionChangedDelegate OnFleetSelectionChanged;
		public FleetWidget.FleetsModifiedDelegate OnFleetsModified;
		private ShipTooltip _ShipToolTip;
		private bool _contentChanged;
		private bool _fleetsChanged;
		private bool _planetsChanged;
		private bool _confirmRepairs;
		private bool _repairModeChanged;
		private List<int> _syncedFleets;
		private List<int> _syncedStations;
		private List<int> _syncedPlanets;
		private string _rootName;
		private int _widgetID;
		private bool _ready;
		private bool _expandAll;
		private bool _hasSuulka;
		private List<FleetWidget> _linkedWidgets;
		private bool _ridersEnabled;
		private bool _showColonies;
		private bool _onlyLocalPlayer;
		private bool _listStations;
		private bool _suulkaMode;
		private bool _ShowPiracyFleets;
		private bool _enableMissionButtons;
		private bool _enemySelectionEnabled;
		private bool _DefenseFleetUpdated;
		private int _selectedFleet;
		private bool _shipSelectionEnabled;
		private MissionType _missionMode;
		private bool _showRepairPoints;
		private bool _repairMode;
		private bool _jumboMode;
		private bool _preferredSelectMode;
		private bool _scrapEnabled;
		private bool _repairAll;
		private bool _undoAll;
		private int _selected;
		private FleetWidget _repairWidget;
		private string _contextMenuID;
		private string _shipcontextMenuID;
		private int _contextSlot;
		private string _fleetNameDialog;
		private string _dissolveFleetDialog;
		private string _cancelMissionDialog;
		private string _retrofitShipDialog;
		private static int _nextWidgetID;
		public bool EnableCreateFleetButton;
		private string _scrapDialog;
		public bool DisableTooltips;
		private string _LoaCubeTransferDialog;
		private static string createfleetpanel;
		private static string admiralPanel;
		private static string SelectCompositionPanel;

		public bool ContentChanged
		{
			get
			{
				return this._contentChanged;
			}
		}

		public List<int> SyncedFleets
		{
			get
			{
				return this._syncedFleets;
			}
		}

		public List<int> SyncedStations
		{
			get
			{
				return this._syncedStations;
			}
		}

		public List<int> SyncedPlanets
		{
			get
			{
				return this._syncedPlanets;
			}
		}

		public event FleetWidget.FleetWidgetShipFilter ShipFilter;

		public event FleetWidget.FleetWidgetFleetFilter FleetFilter;

		public bool RidersEnabled
		{
			get
			{
				return this._ridersEnabled;
			}
			set
			{
				this._ridersEnabled = value;
			}
		}

		public bool ShowColonies
		{
			get
			{
				return this._showColonies;
			}
			set
			{
				this._showColonies = value;
			}
		}

		public bool OnlyLocalPlayer
		{
			get
			{
				return this._onlyLocalPlayer;
			}
			set
			{
				this._onlyLocalPlayer = value;
			}
		}

		public bool ListStations
		{
			get
			{
				return this._listStations;
			}
			set
			{
				this._listStations = value;
			}
		}

		public bool SuulkaMode
		{
			get
			{
				return this._suulkaMode;
			}
			set
			{
				this._suulkaMode = value;
			}
		}

		public bool ShowPiracyFleets
		{
			get
			{
				return this._ShowPiracyFleets;
			}
			set
			{
				this._ShowPiracyFleets = value;
			}
		}

		public bool EnableAdmiralButton
		{
			get
			{
				return this._enableAdmiralButton;
			}
			set
			{
				this._enableAdmiralButton = value;
			}
		}

		public bool EnableRightClick
		{
			get
			{
				return this._enableRightClick;
			}
			set
			{
				this._enableRightClick = value;
			}
		}

		public bool EnableMissionButtons
		{
			get
			{
				return this._enableMissionButtons;
			}
			set
			{
				this._enableMissionButtons = value;
			}
		}

		public bool EnemySelectionEnabled
		{
			get
			{
				return this._enemySelectionEnabled;
			}
			set
			{
				this._enemySelectionEnabled = value;
			}
		}

		public bool SeparateDefenseFleet
		{
			get
			{
				return this._separateDefenseFleet;
			}
			set
			{
				this._separateDefenseFleet = value;
				this.PostSetProp(nameof(SeparateDefenseFleet), value);
			}
		}

		public bool DefenseFleetUpdated
		{
			get
			{
				return this._DefenseFleetUpdated;
			}
			set
			{
				this._DefenseFleetUpdated = value;
			}
		}

		public int SelectedFleet
		{
			get
			{
				return this._selectedFleet;
			}
			set
			{
				this._selectedFleet = value;
				this._contentChanged = true;
				this.Refresh();
			}
		}

		public bool ShipSelectionEnabled
		{
			get
			{
				return this._shipSelectionEnabled;
			}
			set
			{
				this._shipSelectionEnabled = value;
				this._contentChanged = true;
				this.Refresh();
			}
		}

		public MissionType MissionMode
		{
			get
			{
				return this._missionMode;
			}
			set
			{
				this._missionMode = value;
				this._contentChanged = true;
				this.Refresh();
			}
		}

		public bool ShowEmptyFleets
		{
			get
			{
				return this._showEmptyFleets;
			}
			set
			{
				this._showEmptyFleets = value;
				this._contentChanged = true;
				this.Refresh();
			}
		}

		public bool ShowFleetInfo
		{
			get
			{
				return this._showFleetInfo;
			}
			set
			{
				this._showFleetInfo = value;
				this._contentChanged = true;
				this.Refresh();
			}
		}

		public bool ShowRepairPoints
		{
			get
			{
				return this._showRepairPoints;
			}
			set
			{
				this._showRepairPoints = value;
				this._contentChanged = true;
				this.Refresh();
			}
		}

		public bool RepairMode
		{
			get
			{
				return this._repairMode;
			}
			set
			{
				this._repairMode = value;
				this._repairModeChanged = true;
				this._contentChanged = true;
				this.Refresh();
			}
		}

		public bool JumboMode
		{
			get
			{
				return this._jumboMode;
			}
			set
			{
				this._jumboMode = value;
				this._contentChanged = true;
				this.Refresh();
			}
		}

		public bool PreferredSelectMode
		{
			get
			{
				return this._preferredSelectMode;
			}
			set
			{
				this._preferredSelectMode = value;
				this._contentChanged = true;
				this.Refresh();
			}
		}

		public bool ScrapEnabled
		{
			get
			{
				return this._scrapEnabled;
			}
			set
			{
				this._scrapEnabled = value;
				this._contentChanged = true;
				this.Refresh();
			}
		}

		public int Selected
		{
			get
			{
				return this._selected;
			}
			set
			{
				this._selected = value;
				this._contentChanged = true;
				this.Refresh();
			}
		}

		public FleetWidget RepairWidget
		{
			get
			{
				return this._repairWidget;
			}
			set
			{
				this._repairWidget = value;
				this._contentChanged = true;
				this.Refresh();
			}
		}

		public FleetWidget(App game, string rootList)
		{
			this._game = game;
			game.AddExistingObject((IGameObject)this, (object)rootList, (object)FleetWidget._nextWidgetID, (object)this._game.LocalPlayer.ID);
			this._widgetID = FleetWidget._nextWidgetID;
			++FleetWidget._nextWidgetID;
			this._rootName = rootList;
			this._game.UI.GameEvent += new UIEventGameEvent(this.UICommChannel_GameEvent);
			this._game.UI.PanelMessage += new UIEventPanelMessage(this.UICommChannel_OnPanelMessage);
			this.ShipFilter += new FleetWidget.FleetWidgetShipFilter(this.DefaultShipFilter);
			this._linkedWidgets = new List<FleetWidget>();
			this._contextMenuID = this.App.UI.CreatePanelFromTemplate("FleetContextMenu", null);
			this._ShipToolTip = new ShipTooltip(this.App);
			this.App.UI.SetPropertyString(this.App.UI.Path(this._contextMenuID, "gameRenameFleetButton"), "id", "gameRenameFleetButton|" + this._widgetID.ToString());
			this.App.UI.SetPropertyString(this.App.UI.Path(this._contextMenuID, "gameDissolveFleetButton"), "id", "gameDissolveFleetButton|" + this._widgetID.ToString());
			this.App.UI.SetPropertyString(this.App.UI.Path(this._contextMenuID, "gameCancelMissionButton"), "id", "gameCancelMissionButton|" + this._widgetID.ToString());
			this.App.UI.SetPropertyString(this.App.UI.Path(this._contextMenuID, "gameFleetLoaDissolveToCube"), "id", "gameFleetLoaDissolveToCube|" + this._widgetID.ToString());
			this.App.UI.SetPropertyString(this.App.UI.Path(this._contextMenuID, "gameFleetSetLoaComposition"), "id", "gameFleetSetLoaComposition|" + this._widgetID.ToString());
			this.App.UI.SetPropertyString(this.App.UI.Path(this._contextMenuID, "gameFleetSurveyMissionButton"), "id", "gameFleetSurveyMissionButton|" + this._widgetID.ToString());
			this.App.UI.SetPropertyString(this.App.UI.Path(this._contextMenuID, "gameFleetColonizeMissionButton"), "id", "gameFleetColonizeMissionButton|" + this._widgetID.ToString());
			this.App.UI.SetPropertyString(this.App.UI.Path(this._contextMenuID, "gameFleetEvacuateButton"), "id", "gameFleetEvacuateButton|" + this._widgetID.ToString());
			this.App.UI.SetPropertyString(this.App.UI.Path(this._contextMenuID, "gameFleetSuportMissionButton"), "id", "gameFleetSuportMissionButton|" + this._widgetID.ToString());
			this.App.UI.SetPropertyString(this.App.UI.Path(this._contextMenuID, "gameFleetRelocateMissionButton"), "id", "gameFleetRelocateMissionButton|" + this._widgetID.ToString());
			this.App.UI.SetPropertyString(this.App.UI.Path(this._contextMenuID, "gameFleetPatrolMissionButton"), "id", "gameFleetPatrolMissionButton|" + this._widgetID.ToString());
			this.App.UI.SetPropertyString(this.App.UI.Path(this._contextMenuID, "gameFleetInterdictMissionButton"), "id", "gameFleetInterdictMissionButton|" + this._widgetID.ToString());
			this.App.UI.SetPropertyString(this.App.UI.Path(this._contextMenuID, "gameFleetInvadeMissionButton"), "id", "gameFleetInvadeMissionButton|" + this._widgetID.ToString());
			this.App.UI.SetPropertyString(this.App.UI.Path(this._contextMenuID, "gameFleetStrikeMissionButton"), "id", "gameFleetStrikeMissionButton|" + this._widgetID.ToString());
			this.App.UI.SetPropertyString(this.App.UI.Path(this._contextMenuID, "gameFleetPiracyButton"), "id", "gameFleetPiracyButton|" + this._widgetID.ToString());
			this._shipcontextMenuID = this.App.UI.CreatePanelFromTemplate("FleetShipContextMenu", null);
			this.App.UI.SetPropertyString(this.App.UI.Path(this._shipcontextMenuID, "gameRetrofitShip"), "id", "gameRetrofitShip|" + this._widgetID.ToString());
		}

		public void Refresh()
		{
			if (!this._contentChanged || !this._ready)
				return;
			this.PostSetProp("RefreshEnabled", false);
			this.PostSetProp("SetJumboMode", this._jumboMode);
			this.PostSetProp("EnemySelectionEnabled", this._enemySelectionEnabled);
			this.PostSetProp("SetScrapEnabled", this._scrapEnabled);
			if (this._missionMode != MissionType.NO_MISSION)
				this.SetMissionMode(true);
			else
				this.SetMissionMode(false);
			if (this._fleetsChanged)
			{
				this.ClearFleets();
				this.AccountedSystems.Clear();
				List<FleetInfo> source = new List<FleetInfo>();
				foreach (int syncedFleet in this._syncedFleets)
				{
					FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(syncedFleet);
					if (fleetInfo != null)
						source.Add(fleetInfo);
				}
				List<FleetInfo> list = source.OrderBy<FleetInfo, int>((Func<FleetInfo, int>)(x => x.SystemID)).ToList<FleetInfo>();
				FleetInfo fleet1 = list.FirstOrDefault<FleetInfo>((Func<FleetInfo, bool>)(x =>
			   {
				   if (x.IsReserveFleet)
					   return x.PlayerID == this.App.LocalPlayer.ID;
				   return false;
			   }));
				if (fleet1 != null)
				{
					if (!this.AccountedSystems.Contains(fleet1.SystemID))
					{
						this.AccountedSystems.Add(fleet1.SystemID);
						this.SyncSystem(this.App.GameDatabase.GetStarSystemInfo(fleet1.SystemID));
					}
					this.SyncFleet(fleet1);
				}
				FleetInfo fleet2 = list.FirstOrDefault<FleetInfo>((Func<FleetInfo, bool>)(x =>
			   {
				   if (x.IsDefenseFleet)
					   return x.PlayerID == this.App.LocalPlayer.ID;
				   return false;
			   }));
				if (fleet2 != null)
				{
					if (!this.AccountedSystems.Contains(fleet2.SystemID))
					{
						this.AccountedSystems.Add(fleet2.SystemID);
						this.SyncSystem(this.App.GameDatabase.GetStarSystemInfo(fleet2.SystemID));
					}
					this.SyncFleet(fleet2);
				}
				if (this._listStations)
				{
					List<StationInfo> stationInfos = new List<StationInfo>();
					foreach (int syncedStation in this._syncedStations)
					{
						StationInfo stationInfo = this.App.GameDatabase.GetStationInfo(syncedStation);
						if (stationInfo != null)
							stationInfos.Add(stationInfo);
					}
					this.SyncStations(stationInfos);
				}
				foreach (FleetInfo fleet3 in list)
				{
					if (fleet3 != fleet1 && (fleet3.Type != FleetType.FL_RESERVE || fleet3.PlayerID == this.App.LocalPlayer.ID) && (fleet3 != fleet2 && (fleet3.Type != FleetType.FL_DEFENSE || fleet3.PlayerID == this.App.LocalPlayer.ID)))
					{
						if (!this.AccountedSystems.Contains(fleet3.SystemID))
						{
							this.AccountedSystems.Add(fleet3.SystemID);
							this.SyncSystem(this.App.GameDatabase.GetStarSystemInfo(fleet3.SystemID));
						}
						this.SyncFleet(fleet3);
					}
				}
				this._fleetsChanged = false;
			}
			if (this._planetsChanged)
			{
				this.ClearPlanets();
				this.SyncPlanets();
				this._planetsChanged = false;
			}
			this.PostSetProp("SetSelected", this._selected);
			this.PostSetProp("ShowFleetInfo", this._showFleetInfo);
			this.PostSetProp("ShipSelectionEnabled", this._shipSelectionEnabled);
			this.PostSetProp("SetPreferredSelectMode", this._preferredSelectMode);
			if (this._hasSuulka)
				this.App.UI.SetVisible("gameRepairSuulkasButton", true);
			if (this._repairModeChanged)
			{
				this.PostSetProp("SetRepairMode", this._repairMode);
				this._repairModeChanged = false;
			}
			this.PostSetProp("ShowRepairPoints", this._showRepairPoints);
			this.PostSetProp("RepairWidget", (IGameObject)this._repairWidget);
			if (this._expandAll)
			{
				this.PostSetProp("ExpandAll");
				this._expandAll = false;
			}
			this.PostSetProp("Enabled", this._enabled);
			if (this._repairAll)
			{
				this.PostSetProp("RepairAll");
				this._repairAll = false;
			}
			if (this._undoAll)
			{
				this.PostSetProp("UndoAll");
				this._undoAll = false;
			}
			if (this._confirmRepairs)
			{
				this.PostSetProp("ConfirmRepairs");
				this._confirmRepairs = false;
			}
			this._contentChanged = false;
			this.PostSetProp("RefreshEnabled", true);
			this.SetShowEmptyFleets(this._showEmptyFleets);
		}

		public void SetVisibleFleets(Dictionary<int, bool> values)
		{
			if (values.Count <= 0)
				return;
			List<object> objectList = new List<object>();
			objectList.Add((object)values.Count);
			foreach (int key in values.Keys)
			{
				objectList.Add((object)key);
				objectList.Add((object)values[key]);
			}
			this.PostSetProp("SetFleetsVisible", objectList.ToArray());
		}

		public void SetSyncedFleets(List<FleetInfo> fleets)
		{
			List<int> fleets1 = new List<int>();
			foreach (FleetInfo fleet in fleets)
				fleets1.Add(fleet.ID);
			this.SetSyncedFleets(fleets1);
		}

		public void SetSyncedFleets(List<int> fleets)
		{
			this._syncedFleets = fleets;
			this._contentChanged = true;
			this._fleetsChanged = true;
			this.Refresh();
		}

		public void SetSyncedFleets(int fleet)
		{
			this.SetSyncedFleets(new List<int>() { fleet });
		}

		public void SetSyncedStations(List<StationInfo> stations)
		{
			List<int> stations1 = new List<int>();
			foreach (StationInfo station in stations)
				stations1.Add(station.OrbitalObjectID);
			this.SetSyncedStations(stations1);
		}

		public void SetSyncedStations(List<int> stations)
		{
			this._syncedStations = stations;
			this._contentChanged = true;
			this._fleetsChanged = true;
			this.Refresh();
		}

		public void SetSyncedPlanets(List<PlanetInfo> planets)
		{
			List<int> planets1 = new List<int>();
			foreach (PlanetInfo planet in planets)
				planets1.Add(planet.ID);
			this.SetSyncedPlanets(planets1);
		}

		public void SetSyncedPlanets(List<int> planets)
		{
			this._syncedPlanets = planets;
			this._contentChanged = true;
			this._planetsChanged = true;
			this.Refresh();
		}

		public void SyncFleetInfo(FleetInfo fleet)
		{
			if (fleet == null || this.FleetFilter != null && this.FleetFilter(fleet) != FleetWidget.FilterShips.Enable)
				return;
			if (this._separateDefenseFleet && fleet.IsDefenseFleet)
				this._DefenseFleetUpdated = true;
			bool flag1 = fleet.PlayerID != this.App.LocalPlayer.ID;
			IEnumerable<ShipInfo> shipInfoByFleetId = this._game.GameDatabase.GetShipInfoByFleetID(fleet.ID, true);
			MissionInfo missionByFleetId = this._game.GameDatabase.GetMissionByFleetID(fleet.ID);
			string str1 = "";
			int num1 = 0;
			if (fleet.AdmiralID != 0)
			{
				AdmiralInfo admiralInfo = this._game.GameDatabase.GetAdmiralInfo(fleet.AdmiralID);
				str1 = admiralInfo.Name;
				num1 = admiralInfo.ID;
			}
			int num2 = 0;
			if (this._game.AssetDatabase.GetFaction(this._game.GameDatabase.GetPlayerInfo(fleet.PlayerID).FactionID).CanUseAccelerators())
				num2 = Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(this._game.Game, fleet.ID);
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			int num7 = 0;
			foreach (ShipInfo shipInfo in shipInfoByFleetId)
			{
				switch (shipInfo.DesignInfo.Class)
				{
					case ShipClass.Cruiser:
						++num5;
						continue;
					case ShipClass.Dreadnought:
						++num6;
						continue;
					case ShipClass.Leviathan:
						++num7;
						continue;
					case ShipClass.BattleRider:
						++num3;
						continue;
					case ShipClass.Station:
						++num4;
						continue;
					default:
						continue;
				}
			}
			string str2 = "CR";
			if (num5 > 0)
				str2 = "CR_CMD";
			if (num6 > 0)
				str2 = "DN_CMD";
			if (num7 > 0)
				str2 = "LV_CMD";
			int systemId1 = fleet.SystemID;
			int systemId2 = 0;
			MoveOrderInfo orderInfoByFleetId = this._game.GameDatabase.GetMoveOrderInfoByFleetID(fleet.ID);
			if (orderInfoByFleetId != null)
			{
				systemId1 = (double)orderInfoByFleetId.Progress != 0.0 ? 0 : orderInfoByFleetId.FromSystemID;
				systemId2 = orderInfoByFleetId.ToSystemID;
			}
			if (missionByFleetId != null)
			{
				WaypointInfo waypointForMission = this._game.GameDatabase.GetNextWaypointForMission(missionByFleetId.ID);
				if (waypointForMission != null)
				{
					if (waypointForMission.Type == WaypointType.TravelTo)
						systemId2 = waypointForMission.SystemID.HasValue ? waypointForMission.SystemID.Value : 0;
					else if (waypointForMission.Type == WaypointType.ReturnHome)
						systemId2 = fleet.SupportingSystemID;
				}
			}
			string str3 = "Deep Space";
			if (systemId1 != 0)
				str3 = string.Format("{0}", (object)this._game.GameDatabase.GetStarSystemInfo(systemId1).Name);
			string str4 = "None";
			if (systemId2 != 0)
				str4 = string.Format("{0}", (object)this._game.GameDatabase.GetStarSystemInfo(systemId2).Name);
			string str5 = "None";
			if (fleet.SupportingSystemID != 0)
				str5 = string.Format("{0}", (object)this._game.GameDatabase.GetStarSystemInfo(fleet.SupportingSystemID).Name);
			int fleetEndurance = Kerberos.Sots.StarFleet.StarFleet.GetFleetEndurance(this._game.Game, fleet.ID);
			if (!this._game.Game.IsFleetInSupplyRange(fleet.ID))
				fleetEndurance *= -1;
			int num8 = -1;
			if (this._game.GameDatabase.GetFactionName(this._game.GameDatabase.GetPlayerInfo(fleet.PlayerID).FactionID) == "hiver")
				num8 = this._game.GameDatabase.GetFleetCruiserEquivalent(fleet.ID);
			int playerId = fleet.PlayerID;
			int id = this.App.LocalPlayer.ID;
			bool flag2 = false;
			string str6 = fleet.Name;
			string str7 = "";
			int num9 = 0;
			int num10 = 0;
			if (missionByFleetId != null)
			{
				str7 = GameSession.GetMissionDesc(this.App.GameDatabase, missionByFleetId);
				num9 = missionByFleetId.Duration;
				num10 = Kerberos.Sots.StarFleet.StarFleet.GetTurnsRemainingForMissionFleet(this.App.Game, fleet);
				if (this._game.GameDatabase.GetMissionByFleetID(fleet.ID) != null && this._game.GameDatabase.GetMissionByFleetID(fleet.ID).Type == MissionType.PIRACY && !this._game.GameDatabase.PirateFleetVisibleToPlayer(fleet.ID, this._game.LocalPlayer.ID))
				{
					str6 = "Pirate Raiders";
					flag2 = true;
				}
			}
			float fleetTravelSpeed1 = Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(this._game.Game, fleet.ID, shipInfoByFleetId, false);
			float fleetTravelSpeed2 = Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(this._game.Game, fleet.ID, shipInfoByFleetId, true);
			float num11 = (double)fleetTravelSpeed2 <= 0.0 ? fleetTravelSpeed1 : fleetTravelSpeed2;
			this.PostSetProp(nameof(SyncFleetInfo), (object)fleet.Type, (object)fleet.ID, (object)fleet.PlayerID, (object)str6, flag2 ? (object)"" : (object)str1, (object)(flag2 ? 0 : num1), (object)str2, (object)str5, (object)str3, (object)systemId1, (object)str4, (object)fleetEndurance, (object)num8, flag2 ? (object)"" : (object)str7, (object)num9, (object)num11, (object)(flag2 ? this._game.GameDatabase.GetPlayerInfo(this._game.Game.ScriptModules.Pirates.PlayerID).PrimaryColor : this._game.GameDatabase.GetPlayerInfo(fleet.PlayerID).PrimaryColor), (object)(!this._enabled ? false : (!flag1 ? true : (this._enemySelectionEnabled ? true : false))), (object)fleet.Preferred, (object)num2, (object)num10);
		}

		private void SyncStationListInfo(int systemID, int playerID)
		{
			if (systemID == 0)
				return;
			string str = string.Format("{0}", (object)this._game.GameDatabase.GetStarSystemInfo(systemID).Name);
			this.PostSetProp(nameof(SyncStationListInfo), (object)FleetType.FL_STATION, (object)systemID, (object)playerID, (object)App.Localize("@FLEET_STATION_NAME"), (object)"", (object)str, (object)systemID, (object)this._game.GameDatabase.GetPlayerInfo(playerID).PrimaryColor, (object)this._enabled, (object)false);
		}

		public static bool IsBattleRider(DesignInfo design)
		{
			if (design == null)
				return false;
			DesignSectionInfo designSectionInfo = ((IEnumerable<DesignSectionInfo>)design.DesignSections).FirstOrDefault<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(x => x.ShipSectionAsset.Type == ShipSectionType.Mission));
			if (designSectionInfo != null)
				return designSectionInfo.ShipSectionAsset.IsBattleRider;
			return false;
		}

		public static bool IsWeaponBattleRider(DesignInfo design)
		{
			if (design == null)
				return false;
			DesignSectionInfo designSectionInfo = ((IEnumerable<DesignSectionInfo>)design.DesignSections).FirstOrDefault<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(x => x.ShipSectionAsset.Type == ShipSectionType.Mission));
			if (designSectionInfo != null && designSectionInfo.ShipSectionAsset.IsBattleRider)
				return ShipSectionAsset.IsWeaponBattleRiderClass(designSectionInfo.ShipSectionAsset.RealClass);
			return false;
		}

		public FleetWidget.FilterShips DefaultShipFilter(ShipInfo ship, DesignInfo design)
		{
			return FleetWidget.IsBattleRider(design) && (!this._ridersEnabled || FleetWidget.IsWeaponBattleRider(design)) ? FleetWidget.FilterShips.Ignore : FleetWidget.FilterShips.Enable;
		}

		public void LinkWidget(FleetWidget FleetWidget)
		{
			this._linkedWidgets.Add(FleetWidget);
			this.PostSetProp(nameof(LinkWidget), (IGameObject)FleetWidget);
		}

		public void UnLinkWidget(FleetWidget fleetwidget)
		{
			this._linkedWidgets.Remove(fleetwidget);
			this.PostSetProp("UnlinkWidget", (IGameObject)fleetwidget);
		}

		public void UnlinkWidgets()
		{
			this._linkedWidgets.Clear();
			this.PostSetProp(nameof(UnlinkWidgets));
		}

		private void ClearFleets()
		{
			this.PostSetProp(nameof(ClearFleets));
		}

		private void ClearPlanets()
		{
			this.PostSetProp(nameof(ClearPlanets));
		}

		private void SetMissionMode(bool val)
		{
			this.PostSetProp("MissionMode", val);
		}

		private void SetShowEmptyFleets(bool val)
		{
			this.PostSetProp("ShowEmptyFleets", val);
		}

		public void ExpandAll()
		{
			this._expandAll = true;
			this._contentChanged = true;
			this.Refresh();
		}

		public void SetEnabled(bool enabled)
		{
			this._enabled = enabled;
			this._contentChanged = true;
			this.Refresh();
		}

		public void RepairAll()
		{
			this._repairAll = true;
			this._contentChanged = true;
			this.Refresh();
		}

		public void UndoAll()
		{
			this._undoAll = true;
			this._contentChanged = true;
			this.Refresh();
		}

		public void ConfirmRepairs()
		{
			this._confirmRepairs = true;
			this._contentChanged = true;
			this.Refresh();
		}

		private void SyncPlanets()
		{
			if (this._syncedPlanets == null)
				return;
			List<object> objectList = new List<object>();
			objectList.Add((object)this._syncedPlanets.Count);
			foreach (int syncedPlanet in this._syncedPlanets)
			{
				PlanetInfo planetInfo = this.App.GameDatabase.GetPlanetInfo(syncedPlanet);
				ColonyInfo colonyInfo = this.App.GameDatabase.GetColonyInfo(syncedPlanet);
				double num = 0.0;
				if (colonyInfo != null)
					num = colonyInfo.ImperialPop;
				objectList.Add((object)syncedPlanet);
				objectList.Add((object)this.App.GameDatabase.GetOrbitalObjectInfo(syncedPlanet).Name);
				objectList.Add((object)(this.App.GameDatabase.GetCivilianPopulation(syncedPlanet, 0, false) + num));
				objectList.Add((object)planetInfo.Biosphere);
			}
			this.PostSetProp(nameof(SyncPlanets), objectList.ToArray());
		}

		private void SyncSuulkas(FleetInfo fleet)
		{
			IEnumerable<ShipInfo> shipInfoByFleetId = this._game.GameDatabase.GetShipInfoByFleetID(fleet.ID, false);
			List<object> objectList = new List<object>();
			objectList.Add((object)fleet.Type);
			objectList.Add((object)fleet.ID);
			int count = objectList.Count;
			int num1 = 0;
			foreach (ShipInfo ship in shipInfoByFleetId)
			{
				DesignInfo designInfo1 = ship.DesignInfo;
				if (Kerberos.Sots.StarFleet.StarFleet.DesignIsSuulka(this.App, designInfo1))
					this._hasSuulka = true;
				if (this.ShipFilter == null || this.ShipFilter(ship, designInfo1) != FleetWidget.FilterShips.Ignore)
				{
					num1 += 2;
					objectList.Add((object)true);
					objectList.Add((object)ship.DesignID);
					objectList.Add((object)ship.ID);
					objectList.Add((object)(designInfo1.Name + " Psionics"));
					objectList.Add((object)ship.ShipName);
					objectList.Add((object)false);
					objectList.Add((object)false);
					objectList.Add((object)"");
					objectList.Add((object)"");
					objectList.Add((object)0);
					objectList.Add((object)false);
					objectList.Add((object)false);
					objectList.Add((object)0);
					objectList.Add((object)0);
					objectList.Add((object)true);
					objectList.Add((object)ship.PsionicPower);
					objectList.Add((object)(int)designInfo1.DesignSections[0].ShipSectionAsset.PsionicPowerLevel);
					objectList.Add((object)0);
					objectList.Add((object)0);
					objectList.Add((object)0);
					objectList.Add((object)0);
					objectList.Add((object)0);
					objectList.Add((object)2);
					objectList.Add((object)0);
					objectList.Add((object)0);
					objectList.Add((object)true);
					objectList.Add((object)(ship.DesignID + 1));
					objectList.Add((object)ship.ID);
					objectList.Add((object)(designInfo1.Name + " Structure"));
					objectList.Add((object)ship.ShipName);
					objectList.Add((object)false);
					objectList.Add((object)false);
					objectList.Add((object)"");
					objectList.Add((object)"");
					objectList.Add((object)0);
					objectList.Add((object)false);
					objectList.Add((object)false);
					objectList.Add((object)0);
					objectList.Add((object)0);
					objectList.Add((object)true);
					int num2 = 0;
					int num3 = 0;
					int designID = 0;
					int num4 = 0;
					ShipClass shipClass = ShipClass.Leviathan;
					BattleRiderTypes battleRiderTypes = BattleRiderTypes.Unspecified;
					List<SectionInstanceInfo> list1 = this.App.GameDatabase.GetShipSectionInstances(ship.ID).ToList<SectionInstanceInfo>();
					if (list1.Count != designInfo1.DesignSections.Length)
						throw new InvalidDataException(string.Format("Mismatched design section vs ship section instance count for designId={0} and shipId={1}.", (object)designInfo1.ID, (object)ship.ID));
					for (int index1 = 0; index1 < ((IEnumerable<DesignSectionInfo>)designInfo1.DesignSections).Count<DesignSectionInfo>(); ++index1)
					{
						ShipSectionAsset shipSectionAsset1 = this.App.AssetDatabase.GetShipSectionAsset(designInfo1.DesignSections[index1].FilePath);
						List<string> techs = new List<string>();
						if (designInfo1.DesignSections[index1].Techs.Count > 0)
						{
							foreach (int tech in designInfo1.DesignSections[index1].Techs)
								techs.Add(this.App.GameDatabase.GetTechFileID(tech));
						}
						num3 += Ship.GetStructureWithTech(this._game.AssetDatabase, techs, shipSectionAsset1.Structure);
						num2 += list1[index1].Structure;
						if (shipSectionAsset1.Type == ShipSectionType.Mission)
						{
							shipClass = shipSectionAsset1.Class;
							battleRiderTypes = shipSectionAsset1.BattleRiderType;
						}
						Dictionary<ArmorSide, DamagePattern> armorInstances = this.App.GameDatabase.GetArmorInstances(list1[index1].ID);
						if (armorInstances.Count > 0)
						{
							for (int index2 = 0; index2 < 4; ++index2)
							{
								num3 += armorInstances[(ArmorSide)index2].Width * armorInstances[(ArmorSide)index2].Height * 3;
								for (int x = 0; x < armorInstances[(ArmorSide)index2].Width; ++x)
								{
									for (int y = 0; y < armorInstances[(ArmorSide)index2].Height; ++y)
									{
										if (!armorInstances[(ArmorSide)index2].GetValue(x, y))
											num2 += 3;
									}
								}
							}
						}
						foreach (LogicalMount mount1 in shipSectionAsset1.Mounts)
						{
							LogicalMount mount = mount1;
							if (WeaponEnums.IsWeaponBattleRider(mount.Bank.TurretClass))
							{
								if (designID == 0)
								{
									WeaponBankInfo weaponBankInfo = designInfo1.DesignSections[index1].WeaponBanks.FirstOrDefault<WeaponBankInfo>((Func<WeaponBankInfo, bool>)(x =>
								   {
									   if (!(x.BankGUID == mount.Bank.GUID) || !x.DesignID.HasValue)
										   return false;
									   int? designId = x.DesignID;
									   if (designId.GetValueOrDefault() == 0)
										   return !designId.HasValue;
									   return true;
								   }));
									designID = weaponBankInfo == null || !weaponBankInfo.DesignID.HasValue ? 0 : weaponBankInfo.DesignID.Value;
								}
								++num4;
							}
						}
						List<ModuleInstanceInfo> list2 = this.App.GameDatabase.GetModuleInstances(list1[index1].ID).ToList<ModuleInstanceInfo>();
						List<DesignModuleInfo> module = designInfo1.DesignSections[index1].Modules;
						if (list2.Count == module.Count)
						{
							for (int mod = 0; mod < module.Count; ++mod)
							{
								ModuleInstanceInfo moduleInstanceInfo = list2.FirstOrDefault<ModuleInstanceInfo>((Func<ModuleInstanceInfo, bool>)(x => x.ModuleNodeID == module[mod].MountNodeName));
								string modAsset = this.App.GameDatabase.GetModuleAsset(module[mod].ModuleID);
								LogicalModule logicalModule = this.App.AssetDatabase.Modules.Where<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == modAsset)).First<LogicalModule>();
								num3 += (int)logicalModule.Structure;
								num2 += moduleInstanceInfo != null ? moduleInstanceInfo.Structure : (int)logicalModule.Structure;
								if (module[mod].DesignID.HasValue)
								{
									foreach (LogicalMount mount in logicalModule.Mounts)
									{
										if (WeaponEnums.IsWeaponBattleRider(mount.Bank.TurretClass))
										{
											if (designID == 0)
												designID = module[mod].DesignID.Value;
											++num4;
										}
									}
								}
							}
						}
						foreach (WeaponInstanceInfo weaponInstanceInfo in this.App.GameDatabase.GetWeaponInstances(list1[index1].ID).ToList<WeaponInstanceInfo>())
						{
							num3 += (int)weaponInstanceInfo.MaxStructure;
							num2 += (int)weaponInstanceInfo.Structure;
						}
						List<ShipInfo> list3 = this.App.GameDatabase.GetBattleRidersByParentID(ship.ID).ToList<ShipInfo>();
						if (num4 > 0)
						{
							int num5 = num4;
							foreach (ShipInfo shipInfo in list3)
							{
								DesignInfo designInfo2 = this.App.GameDatabase.GetDesignInfo(shipInfo.DesignID);
								if (designInfo2 != null)
								{
									DesignSectionInfo designSectionInfo = ((IEnumerable<DesignSectionInfo>)designInfo2.DesignSections).FirstOrDefault<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(x => x.ShipSectionAsset.Type == ShipSectionType.Mission));
									if (designSectionInfo != null && ShipSectionAsset.IsBattleRiderClass(designSectionInfo.ShipSectionAsset.RealClass))
										--num5;
								}
							}
							int num6 = 0;
							if (designID != 0)
							{
								foreach (DesignSectionInfo designSection in this.App.GameDatabase.GetDesignInfo(designID).DesignSections)
								{
									ShipSectionAsset shipSectionAsset2 = this.App.AssetDatabase.GetShipSectionAsset(designSection.FilePath);
									num6 = shipSectionAsset2.Structure;
									int repairPoints = shipSectionAsset2.RepairPoints;
									if (shipSectionAsset2.Armor.Length > 0)
									{
										for (int index2 = 0; index2 < 4; ++index2)
											num6 += shipSectionAsset2.Armor[index2].X * shipSectionAsset2.Armor[index2].Y * 3;
									}
								}
							}
							num3 += num6 * num4;
							num2 += num6 * (num4 - num5);
						}
					}
					objectList.Add((object)num2);
					objectList.Add((object)num3);
					objectList.Add((object)0);
					objectList.Add((object)0);
					objectList.Add((object)0);
					objectList.Add((object)0);
					objectList.Add((object)0);
					objectList.Add((object)3);
					objectList.Add((object)(int)shipClass);
					objectList.Add((object)(int)battleRiderTypes);
				}
			}
			objectList.Insert(count, (object)num1);
			objectList.Add((object)0);
			this.PostSetProp("SyncShips", objectList.ToArray());
		}

		private void SyncSystem(StarSystemInfo system)
		{
			if (!(system != (StarSystemInfo)null) || system.IsDeepSpace)
				return;
			Vector4 vector4 = StarHelper.CalcModelColor(new StellarClass(system.StellarClass));
			bool flag = this.EnableCreateFleetButton && this.App.GameDatabase.GetRemainingSupportPoints(this.App.Game, system.ID, this.App.LocalPlayer.ID) > 0 && (this.AdmiralAvailable(this.App.LocalPlayer.ID, system.ID) && this.CommandShipAvailable(this.App.LocalPlayer.ID, system.ID)) && (this.App.CurrentState == this.App.GetGameState<StarMapState>() || this.App.CurrentState == this.App.GetGameState<FleetManagerState>());
			this.PostSetProp(nameof(SyncSystem), (object)system.ID, (object)system.Name, (object)vector4.X, (object)vector4.Y, (object)vector4.Z, (object)flag);
		}

		private void SyncFleet(FleetInfo fleet)
		{
			if (fleet == null || this._game.GameDatabase.GetMissionByFleetID(fleet.ID) != null && this._game.GameDatabase.GetMissionByFleetID(fleet.ID).Type == MissionType.PIRACY && (!this._game.GameDatabase.PirateFleetVisibleToPlayer(fleet.ID, this._game.LocalPlayer.ID) && !this.ShowPiracyFleets))
				return;
			this.SyncFleetInfo(fleet);
			if (this.SuulkaMode)
			{
				this.SyncSuulkas(fleet);
			}
			else
			{
				List<ShipInfo> list1 = this._game.GameDatabase.GetShipInfoByFleetID(fleet.ID, false).ToList<ShipInfo>();
				List<object> objectList = new List<object>();
				objectList.Add((object)fleet.Type);
				objectList.Add((object)fleet.ID);
				int count1 = objectList.Count;
				int num1 = 0;
				List<int> intList1 = (List<int>)null;
				if (this._missionMode != MissionType.NO_MISSION && fleet.Type != FleetType.FL_RESERVE)
				{
					intList1 = Kerberos.Sots.StarFleet.StarFleet.GetMissionCapableShips(this._game.Game, fleet.ID, this._missionMode);
					List<int> intList2 = new List<int>();
					if (intList1.Count == 0)
					{
						if (!this._game.GetStratModifier<bool>(StratModifiers.MutableFleets, fleet.PlayerID))
							intList2 = DesignLab.GetMissionRequiredDesigns(this._game.Game, this._missionMode, this._game.LocalPlayer.ID);
						foreach (int designID in intList2)
						{
							++num1;
							objectList.Add((object)true);
							DesignInfo designInfo = this.App.GameDatabase.GetDesignInfo(designID);
							objectList.Add((object)designID);
							objectList.Add((object)-1);
							objectList.Add((object)designInfo.Name);
							objectList.Add((object)designInfo.Name);
							objectList.Add((object)false);
							objectList.Add((object)false);
							objectList.Add((object)"");
							objectList.Add((object)"");
							objectList.Add((object)0);
							objectList.Add((object)false);
							objectList.Add((object)false);
							objectList.Add((object)this.App.GameDatabase.GetCommandPointCost(designInfo.ID));
							objectList.Add((object)this.App.GameDatabase.GetDesignCommandPointQuota(this.App.AssetDatabase, designInfo.ID));
							objectList.Add((object)true);
							objectList.Add((object)0);
							objectList.Add((object)0);
							objectList.Add((object)0);
							objectList.Add((object)0);
							objectList.Add((object)0);
							objectList.Add((object)0);
							objectList.Add((object)0);
							objectList.Add((object)0);
							objectList.Add((object)designInfo.Class);
							objectList.Add((object)((IEnumerable<DesignSectionInfo>)designInfo.DesignSections).First<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(x => x.ShipSectionAsset.Type == ShipSectionType.Mission)).ShipSectionAsset.BattleRiderType);
						}
					}
				}
				foreach (ShipInfo ship in list1)
				{
					DesignInfo designInfo1 = this.App.GameDatabase.GetDesignInfo(ship.DesignID);
					bool flag1 = true;
					if (this.ShipFilter != null)
					{
						switch (this.ShipFilter(ship, designInfo1))
						{
							case FleetWidget.FilterShips.Disable:
								flag1 = false;
								break;
							case FleetWidget.FilterShips.Ignore:
								continue;
						}
					}
					++num1;
					if (intList1 != null && this._missionMode != MissionType.NO_MISSION)
						objectList.Add((object)intList1.Contains(ship.ID));
					else
						objectList.Add((object)true);
					objectList.Add((object)ship.DesignID);
					objectList.Add((object)ship.ID);
					objectList.Add((object)designInfo1.Name);
					objectList.Add((object)ship.ShipName);
					bool flag2 = false;
					string str1 = "";
					string str2 = "";
					bool flag3 = false;
					bool flag4 = ship.IsPoliceShip();
					bool flag5 = false;
					foreach (DesignSectionInfo designSection in designInfo1.DesignSections)
					{
						PlatformTypes? platformType = designSection.ShipSectionAsset.GetPlatformType();
						if (platformType.HasValue)
						{
							platformType = designSection.ShipSectionAsset.GetPlatformType();
							str2 = platformType.ToString();
						}
						if (designSection.FilePath.Contains("minelayer"))
						{
							flag2 = true;
							using (List<WeaponBankInfo>.Enumerator enumerator = designSection.WeaponBanks.GetEnumerator())
							{
								while (enumerator.MoveNext())
								{
									string wasset = this.App.GameDatabase.GetWeaponAsset(enumerator.Current.WeaponID.Value);
									if (wasset.Contains("Min_"))
									{
										LogicalWeapon logicalWeapon = this.App.AssetDatabase.Weapons.FirstOrDefault<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.FileName == wasset));
										if (logicalWeapon != null)
										{
											str1 = logicalWeapon.IconSpriteName;
											break;
										}
									}
								}
								break;
							}
						}
						else
						{
							if (designSection.FilePath.ToLower().Contains("_sdb"))
							{
								flag3 = true;
								break;
							}
							if (designSection.ShipSectionAsset.isPolice)
							{
								flag4 = true;
								break;
							}
							if (designSection.ShipSectionAsset.IsSuperTransport)
							{
								flag5 = true;
								break;
							}
						}
					}
					objectList.Add((object)flag2);
					objectList.Add((object)flag3);
					objectList.Add((object)str1);
					objectList.Add((object)str2);
					objectList.Add((object)ship.LoaCubes);
					objectList.Add((object)flag5);
					objectList.Add((object)flag4);
					objectList.Add((object)this.App.GameDatabase.GetShipCommandPointCost(ship.ID, true));
					objectList.Add((object)this.App.GameDatabase.GetDesignCommandPointQuota(this.App.AssetDatabase, designInfo1.ID));
					objectList.Add((object)flag1);
					int num2 = 0;
					int num3 = 0;
					int num4 = 0;
					int num5 = 0;
					int num6 = 0;
					int num7 = 0;
					int num8 = 0;
					int designID = 0;
					BattleRiderTypes battleRiderTypes = BattleRiderTypes.Unspecified;
					List<SectionInstanceInfo> list2 = this.App.GameDatabase.GetShipSectionInstances(ship.ID).ToList<SectionInstanceInfo>();
					for (int index1 = 0; index1 < ((IEnumerable<DesignSectionInfo>)designInfo1.DesignSections).Count<DesignSectionInfo>(); ++index1)
					{
						ShipSectionAsset shipSectionAsset = this.App.AssetDatabase.GetShipSectionAsset(designInfo1.DesignSections[index1].FilePath);
						List<string> techs = new List<string>();
						if (designInfo1.DesignSections[index1].Techs.Count > 0)
						{
							foreach (int tech in designInfo1.DesignSections[index1].Techs)
								techs.Add(this.App.GameDatabase.GetTechFileID(tech));
						}
						float structure = (float)list2[index1].Structure;
						num5 += Ship.GetStructureWithTech(this._game.AssetDatabase, techs, shipSectionAsset.Structure);
						num4 += list2[index1].Structure;
						num6 += shipSectionAsset.ConstructionPoints;
						num7 += shipSectionAsset.ColonizationSpace;
						if (shipSectionAsset.Type == ShipSectionType.Mission)
							battleRiderTypes = shipSectionAsset.BattleRiderType;
						Dictionary<ArmorSide, DamagePattern> armorInstances = this.App.GameDatabase.GetArmorInstances(list2[index1].ID);
						if (armorInstances.Count > 0)
						{
							for (int index2 = 0; index2 < 4; ++index2)
							{
								num5 += armorInstances[(ArmorSide)index2].Width * armorInstances[(ArmorSide)index2].Height * 3;
								for (int x = 0; x < armorInstances[(ArmorSide)index2].Width; ++x)
								{
									for (int y = 0; y < armorInstances[(ArmorSide)index2].Height; ++y)
									{
										if (!armorInstances[(ArmorSide)index2].GetValue(x, y))
											num4 += 3;
									}
								}
							}
						}
						foreach (LogicalMount mount1 in shipSectionAsset.Mounts)
						{
							LogicalMount mount = mount1;
							if (WeaponEnums.IsWeaponBattleRider(mount.Bank.TurretClass))
							{
								if (designID == 0)
								{
									WeaponBankInfo weaponBankInfo = designInfo1.DesignSections[index1].WeaponBanks.FirstOrDefault<WeaponBankInfo>((Func<WeaponBankInfo, bool>)(x =>
								   {
									   if (!(x.BankGUID == mount.Bank.GUID) || !x.DesignID.HasValue)
										   return false;
									   int? designId = x.DesignID;
									   if (designId.GetValueOrDefault() == 0)
										   return !designId.HasValue;
									   return true;
								   }));
									designID = weaponBankInfo == null || !weaponBankInfo.DesignID.HasValue ? 0 : weaponBankInfo.DesignID.Value;
								}
								++num8;
							}
						}
						List<ModuleInstanceInfo> list3 = this.App.GameDatabase.GetModuleInstances(list2[index1].ID).ToList<ModuleInstanceInfo>();
						List<DesignModuleInfo> module = designInfo1.DesignSections[index1].Modules;
						if (list3.Count == module.Count)
						{
							for (int mod = 0; mod < module.Count; ++mod)
							{
								ModuleInstanceInfo moduleInstanceInfo = list3.FirstOrDefault<ModuleInstanceInfo>((Func<ModuleInstanceInfo, bool>)(x => x.ModuleNodeID == module[mod].MountNodeName));
								if (moduleInstanceInfo != null)
								{
									string modAsset = this.App.GameDatabase.GetModuleAsset(module[mod].ModuleID);
									LogicalModule logicalModule = this.App.AssetDatabase.Modules.Where<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == modAsset)).First<LogicalModule>();
									num5 += (int)logicalModule.Structure;
									num4 += moduleInstanceInfo != null ? moduleInstanceInfo.Structure : (int)logicalModule.Structure;
									num3 += logicalModule.RepairPointsBonus;
									if ((double)moduleInstanceInfo.Structure > 0.0)
									{
										num2 += moduleInstanceInfo.RepairPoints;
										structure += logicalModule.StructureBonus;
									}
									if (module[mod].DesignID.HasValue)
									{
										foreach (LogicalMount mount in logicalModule.Mounts)
										{
											if (WeaponEnums.IsWeaponBattleRider(mount.Bank.TurretClass))
											{
												if (designID == 0)
													designID = module[mod].DesignID.Value;
												++num8;
											}
										}
									}
								}
							}
						}
						num3 += shipSectionAsset.RepairPoints;
						if ((double)structure > 0.0)
							num2 += list2[index1].RepairPoints;
						foreach (WeaponInstanceInfo weaponInstanceInfo in this.App.GameDatabase.GetWeaponInstances(list2[index1].ID).ToList<WeaponInstanceInfo>())
						{
							num5 += (int)weaponInstanceInfo.MaxStructure;
							num4 += (int)weaponInstanceInfo.Structure;
						}
					}
					List<ShipInfo> list4 = this.App.GameDatabase.GetBattleRidersByParentID(ship.ID).ToList<ShipInfo>();
					if (num8 > 0)
					{
						int num9 = num8;
						foreach (ShipInfo shipInfo in list4)
						{
							DesignInfo designInfo2 = this.App.GameDatabase.GetDesignInfo(shipInfo.DesignID);
							if (designInfo2 != null)
							{
								DesignSectionInfo designSectionInfo = ((IEnumerable<DesignSectionInfo>)designInfo2.DesignSections).FirstOrDefault<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(x => x.ShipSectionAsset.Type == ShipSectionType.Mission));
								if (designSectionInfo != null && ShipSectionAsset.IsBattleRiderClass(designSectionInfo.ShipSectionAsset.RealClass))
									--num9;
							}
						}
						int num10 = 0;
						int num11 = 0;
						if (designID != 0)
						{
							foreach (DesignSectionInfo designSection in this.App.GameDatabase.GetDesignInfo(designID).DesignSections)
							{
								ShipSectionAsset shipSectionAsset = this.App.AssetDatabase.GetShipSectionAsset(designSection.FilePath);
								num10 = shipSectionAsset.Structure;
								num11 = shipSectionAsset.RepairPoints;
								if (shipSectionAsset.Armor.Length > 0)
								{
									for (int index = 0; index < 4; ++index)
										num10 += shipSectionAsset.Armor[index].X * shipSectionAsset.Armor[index].Y * 3;
								}
							}
						}
						num5 += num10 * num8;
						num3 += num11 * num8;
						num4 += num10 * (num8 - num9);
						num2 += num11 * (num8 - num9);
					}
					if (list2.Count != designInfo1.DesignSections.Length)
						throw new InvalidDataException(string.Format("Mismatched design section vs ship section instance count for designId={0} and shipId={1}.", (object)designInfo1.ID, (object)ship.ID));
					objectList.Add((object)num4);
					objectList.Add((object)num5);
					objectList.Add((object)num2);
					objectList.Add((object)num3);
					objectList.Add((object)num6);
					objectList.Add((object)num7);
					objectList.Add((object)list4.Count<ShipInfo>());
					foreach (ShipInfo shipInfo in list4)
						objectList.Add((object)shipInfo.ID);
					objectList.Add((object)0);
					objectList.Add((object)designInfo1.Class);
					objectList.Add((object)(int)battleRiderTypes);
				}
				objectList.Insert(count1, (object)num1);
				bool flag = fleet.Type == FleetType.FL_RESERVE;
				int count2 = objectList.Count;
				int num12 = 0;
				if (flag && this._showColonies)
				{
					foreach (ColonyInfo colonyInfo in !this._onlyLocalPlayer ? this.App.GameDatabase.GetColonyInfosForSystem(fleet.SystemID).ToList<ColonyInfo>() : this.App.GameDatabase.GetColonyInfosForSystem(fleet.SystemID).ToList<ColonyInfo>().Where<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == this.App.LocalPlayer.ID)).ToList<ColonyInfo>())
					{
						objectList.Add((object)colonyInfo.ID);
						objectList.Add((object)this.App.GameDatabase.GetOrbitalObjectInfo(colonyInfo.OrbitalObjectID).Name);
						objectList.Add((object)colonyInfo.RepairPoints);
						objectList.Add((object)colonyInfo.RepairPointsMax);
						++num12;
					}
				}
				objectList.Insert(count2, (object)num12);
				this.PostSetProp("SyncShips", objectList.ToArray());
			}
		}

		private void SyncStations(List<StationInfo> stationInfos)
		{
			StationInfo stationInfo1 = stationInfos.FirstOrDefault<StationInfo>();
			if (stationInfo1 == null)
				return;
			int playerId = stationInfo1.PlayerID;
			int systemID = 0;
			OrbitalObjectInfo orbitalObjectInfo = this.App.GameDatabase.GetOrbitalObjectInfo(stationInfo1.OrbitalObjectID);
			if (orbitalObjectInfo != null)
				systemID = orbitalObjectInfo.StarSystemID;
			this.SyncStationListInfo(systemID, playerId);
			List<object> objectList = new List<object>();
			objectList.Add((object)FleetType.FL_STATION);
			objectList.Add((object)systemID);
			int count = objectList.Count;
			int num1 = 0;
			foreach (StationInfo stationInfo2 in stationInfos)
			{
				DesignInfo designInfo = stationInfo2.DesignInfo;
				objectList.Add((object)true);
				objectList.Add((object)stationInfo2.DesignInfo.ID);
				objectList.Add((object)stationInfo2.ShipID);
				objectList.Add((object)designInfo.Name);
				objectList.Add((object)designInfo.Name);
				objectList.Add((object)false);
				objectList.Add((object)false);
				objectList.Add((object)"");
				objectList.Add((object)"");
				objectList.Add((object)0);
				objectList.Add((object)false);
				objectList.Add((object)false);
				objectList.Add((object)0);
				objectList.Add((object)0);
				objectList.Add((object)true);
				int num2 = 0;
				int num3 = 0;
				int num4 = 0;
				int num5 = 0;
				List<SectionInstanceInfo> list = this.App.GameDatabase.GetShipSectionInstances(stationInfo2.ShipID).ToList<SectionInstanceInfo>();
				for (int index1 = 0; index1 < designInfo.DesignSections.Length; ++index1)
				{
					ShipSectionAsset shipSectionAsset = this.App.AssetDatabase.GetShipSectionAsset(designInfo.DesignSections[index1].FilePath);
					List<string> techs = new List<string>();
					if (designInfo.DesignSections[index1].Techs.Count > 0)
					{
						foreach (int tech in designInfo.DesignSections[index1].Techs)
							techs.Add(this.App.GameDatabase.GetTechFileID(tech));
					}
					num5 += Ship.GetStructureWithTech(this._game.AssetDatabase, techs, shipSectionAsset.Structure);
					bool flag = designInfo.DesignSections.Length == list.Count;
					int num6 = flag ? list[index1].Structure : designInfo.DesignSections[index1].ShipSectionAsset.Structure;
					num4 += num6;
					if (num4 > num5)
						num4 = num5;
					Dictionary<ArmorSide, DamagePattern> dictionary = flag ? this.App.GameDatabase.GetArmorInstances(list[index1].ID) : new Dictionary<ArmorSide, DamagePattern>();
					if (dictionary.Count > 0)
					{
						for (int index2 = 0; index2 < 4; ++index2)
						{
							num5 += dictionary[(ArmorSide)index2].Width * dictionary[(ArmorSide)index2].Height * 3;
							for (int x = 0; x < dictionary[(ArmorSide)index2].Width; ++x)
							{
								for (int y = 0; y < dictionary[(ArmorSide)index2].Height; ++y)
								{
									if (!dictionary[(ArmorSide)index2].GetValue(x, y))
										num4 += 3;
								}
							}
						}
					}
					List<ModuleInstanceInfo> moduleInstanceInfoList = flag ? this.App.GameDatabase.GetModuleInstances(list[index1].ID).ToList<ModuleInstanceInfo>() : new List<ModuleInstanceInfo>();
					List<DesignModuleInfo> modules = designInfo.DesignSections[index1].Modules;
					if (moduleInstanceInfoList.Count == modules.Count)
					{
						for (int index2 = 0; index2 < modules.Count; ++index2)
						{
							string modAsset = this.App.GameDatabase.GetModuleAsset(modules[index2].ModuleID);
							LogicalModule logicalModule = this.App.AssetDatabase.Modules.Where<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == modAsset)).First<LogicalModule>();
							num5 += (int)logicalModule.Structure;
							num3 += logicalModule.RepairPointsBonus;
							num4 += moduleInstanceInfoList[index2].Structure;
							if ((double)moduleInstanceInfoList[index2].Structure > 0.0)
							{
								num2 += moduleInstanceInfoList[index2].RepairPoints;
								num6 += (int)logicalModule.StructureBonus;
							}
						}
					}
					foreach (WeaponInstanceInfo weaponInstanceInfo in this.App.GameDatabase.GetWeaponInstances(list[index1].ID).ToList<WeaponInstanceInfo>())
					{
						num5 += (int)weaponInstanceInfo.MaxStructure;
						num4 += (int)weaponInstanceInfo.Structure;
					}
					num3 += shipSectionAsset.RepairPoints;
					if (num6 > 0)
						num2 += flag ? list[index1].RepairPoints : designInfo.DesignSections[index1].ShipSectionAsset.RepairPoints;
				}
				objectList.Add((object)num4);
				objectList.Add((object)num5);
				objectList.Add((object)num2);
				objectList.Add((object)num3);
				objectList.Add((object)0);
				objectList.Add((object)0);
				objectList.Add((object)0);
				objectList.Add((object)false);
				objectList.Add((object)4);
				objectList.Add((object)0);
				++num1;
			}
			objectList.Insert(count, (object)num1);
			objectList.Add((object)0);
			this.PostSetProp("SyncShips", objectList.ToArray());
		}

		private void ShowFleetPopup(string[] eventParams)
		{
			this.App.UI.AutoSize(this._contextMenuID);
			this._contextSlot = int.Parse(eventParams[1]);
			FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(this._contextSlot);
			if (fleetInfo == null)
				return;
			MissionInfo missionByFleetId = this.App.GameDatabase.GetMissionByFleetID(fleetInfo.ID);
			this.App.UI.SetEnabled(this.App.UI.Path(this._contextMenuID, "gameRenameFleetButton|" + this._widgetID.ToString()), (!Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(this.App.Game, fleetInfo) ? 1 : 0) != 0);
			this.App.UI.SetEnabled(this.App.UI.Path(this._contextMenuID, "gameCancelMissionButton|" + this._widgetID.ToString()), (!fleetInfo.IsNormalFleet ? 0 : (missionByFleetId == null || missionByFleetId.Type == MissionType.RETURN ? 0 : (missionByFleetId.Type != MissionType.RETREAT ? 1 : 0))) != 0);
			this.App.UI.SetEnabled(this.App.UI.Path(this._contextMenuID, "gameDissolveFleetButton|" + this._widgetID.ToString()), (!fleetInfo.IsNormalFleet && !fleetInfo.IsAcceleratorFleet && !fleetInfo.IsGateFleet || missionByFleetId != null ? 0 : (!Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(this.App.Game, fleetInfo) ? 1 : 0)) != 0);
			bool flag1 = fleetInfo.IsNormalFleet && this._enableMissionButtons && (missionByFleetId == null && Kerberos.Sots.StarFleet.StarFleet.CollectAvailableSystemsForFleetMission(this.App.GameDatabase, this.App.Game, fleetInfo.ID, MissionType.SURVEY, true).Any<int>()) && (this.App.CurrentState.Name == "StarMapState" && this._missionMode == MissionType.NO_MISSION) && this._game.GameDatabase.HasEndOfFleshExpansion();
			this.App.UI.SetEnabled(this.App.UI.Path(this._contextMenuID, "gameFleetSurveyMissionButton|" + this._widgetID.ToString()), (flag1 ? 1 : 0) != 0);
			this.App.UI.SetVisible(this.App.UI.Path(this._contextMenuID, "gameFleetSurveyMissionButton|" + this._widgetID.ToString()), (flag1 ? 1 : 0) != 0);
			bool flag2 = fleetInfo.IsNormalFleet && this._enableMissionButtons && (missionByFleetId == null && Kerberos.Sots.StarFleet.StarFleet.CollectAvailableSystemsForFleetMission(this.App.GameDatabase, this.App.Game, fleetInfo.ID, MissionType.COLONIZATION, true).Any<int>()) && (this.App.CurrentState.Name == "StarMapState" && this._missionMode == MissionType.NO_MISSION) && this._game.GameDatabase.HasEndOfFleshExpansion();
			this.App.UI.SetEnabled(this.App.UI.Path(this._contextMenuID, "gameFleetColonizeMissionButton|" + this._widgetID.ToString()), (flag2 ? 1 : 0) != 0);
			this.App.UI.SetVisible(this.App.UI.Path(this._contextMenuID, "gameFleetColonizeMissionButton|" + this._widgetID.ToString()), (flag2 ? 1 : 0) != 0);
			bool flag3 = fleetInfo.IsNormalFleet && this._enableMissionButtons && (missionByFleetId == null && Kerberos.Sots.StarFleet.StarFleet.CollectAvailableSystemsForFleetMission(this.App.GameDatabase, this.App.Game, fleetInfo.ID, MissionType.EVACUATE, true).Any<int>()) && (this.App.CurrentState.Name == "StarMapState" && this._missionMode == MissionType.NO_MISSION) && this._game.GameDatabase.HasEndOfFleshExpansion();
			this.App.UI.SetEnabled(this.App.UI.Path(this._contextMenuID, "gameFleetEvacuateButton|" + this._widgetID.ToString()), (flag3 ? 1 : 0) != 0);
			this.App.UI.SetVisible(this.App.UI.Path(this._contextMenuID, "gameFleetEvacuateButton|" + this._widgetID.ToString()), (flag3 ? 1 : 0) != 0);
			bool flag4 = fleetInfo.IsNormalFleet && this._enableMissionButtons && (missionByFleetId == null && Kerberos.Sots.StarFleet.StarFleet.CollectAvailableSystemsForFleetMission(this.App.GameDatabase, this.App.Game, fleetInfo.ID, MissionType.SUPPORT, true).Any<int>()) && (this.App.CurrentState.Name == "StarMapState" && this._missionMode == MissionType.NO_MISSION) && this._game.GameDatabase.HasEndOfFleshExpansion();
			this.App.UI.SetEnabled(this.App.UI.Path(this._contextMenuID, "gameFleetSuportMissionButton|" + this._widgetID.ToString()), (flag4 ? 1 : 0) != 0);
			this.App.UI.SetVisible(this.App.UI.Path(this._contextMenuID, "gameFleetSuportMissionButton|" + this._widgetID.ToString()), (flag4 ? 1 : 0) != 0);
			bool flag5 = fleetInfo.IsNormalFleet && this._enableMissionButtons && (missionByFleetId == null && Kerberos.Sots.StarFleet.StarFleet.CollectAvailableSystemsForFleetMission(this.App.GameDatabase, this.App.Game, fleetInfo.ID, MissionType.RELOCATION, true).Any<int>()) && (this.App.CurrentState.Name == "StarMapState" && this._missionMode == MissionType.NO_MISSION) && this._game.GameDatabase.HasEndOfFleshExpansion();
			this.App.UI.SetEnabled(this.App.UI.Path(this._contextMenuID, "gameFleetRelocateMissionButton|" + this._widgetID.ToString()), (flag5 ? 1 : 0) != 0);
			this.App.UI.SetVisible(this.App.UI.Path(this._contextMenuID, "gameFleetRelocateMissionButton|" + this._widgetID.ToString()), (flag5 ? 1 : 0) != 0);
			bool flag6 = fleetInfo.IsNormalFleet && this._enableMissionButtons && (missionByFleetId == null && Kerberos.Sots.StarFleet.StarFleet.CollectAvailableSystemsForFleetMission(this.App.GameDatabase, this.App.Game, fleetInfo.ID, MissionType.PATROL, true).Any<int>()) && (this.App.CurrentState.Name == "StarMapState" && this._missionMode == MissionType.NO_MISSION) && this._game.GameDatabase.HasEndOfFleshExpansion();
			this.App.UI.SetEnabled(this.App.UI.Path(this._contextMenuID, "gameFleetPatrolMissionButton|" + this._widgetID.ToString()), (flag6 ? 1 : 0) != 0);
			this.App.UI.SetVisible(this.App.UI.Path(this._contextMenuID, "gameFleetPatrolMissionButton|" + this._widgetID.ToString()), (flag6 ? 1 : 0) != 0);
			bool flag7 = fleetInfo.IsNormalFleet && this._enableMissionButtons && (missionByFleetId == null && Kerberos.Sots.StarFleet.StarFleet.CollectAvailableSystemsForFleetMission(this.App.GameDatabase, this.App.Game, fleetInfo.ID, MissionType.INTERDICTION, true).Any<int>()) && (this.App.CurrentState.Name == "StarMapState" && this._missionMode == MissionType.NO_MISSION) && this._game.GameDatabase.HasEndOfFleshExpansion();
			this.App.UI.SetEnabled(this.App.UI.Path(this._contextMenuID, "gameFleetInterdictMissionButton|" + this._widgetID.ToString()), (flag7 ? 1 : 0) != 0);
			this.App.UI.SetVisible(this.App.UI.Path(this._contextMenuID, "gameFleetInterdictMissionButton|" + this._widgetID.ToString()), (flag7 ? 1 : 0) != 0);
			bool flag8 = fleetInfo.IsNormalFleet && this._enableMissionButtons && (missionByFleetId == null && Kerberos.Sots.StarFleet.StarFleet.CollectAvailableSystemsForFleetMission(this.App.GameDatabase, this.App.Game, fleetInfo.ID, MissionType.INVASION, true).Any<int>()) && (this.App.CurrentState.Name == "StarMapState" && this._missionMode == MissionType.NO_MISSION) && this._game.GameDatabase.HasEndOfFleshExpansion();
			this.App.UI.SetEnabled(this.App.UI.Path(this._contextMenuID, "gameFleetInvadeMissionButton|" + this._widgetID.ToString()), (flag8 ? 1 : 0) != 0);
			this.App.UI.SetVisible(this.App.UI.Path(this._contextMenuID, "gameFleetInvadeMissionButton|" + this._widgetID.ToString()), (flag8 ? 1 : 0) != 0);
			bool flag9 = fleetInfo.IsNormalFleet && this._enableMissionButtons && (missionByFleetId == null && Kerberos.Sots.StarFleet.StarFleet.CollectAvailableSystemsForFleetMission(this.App.GameDatabase, this.App.Game, fleetInfo.ID, MissionType.STRIKE, true).Any<int>()) && (this.App.CurrentState.Name == "StarMapState" && this._missionMode == MissionType.NO_MISSION) && this._game.GameDatabase.HasEndOfFleshExpansion();
			this.App.UI.SetEnabled(this.App.UI.Path(this._contextMenuID, "gameFleetStrikeMissionButton|" + this._widgetID.ToString()), (flag9 ? 1 : 0) != 0);
			this.App.UI.SetVisible(this.App.UI.Path(this._contextMenuID, "gameFleetStrikeMissionButton|" + this._widgetID.ToString()), (flag9 ? 1 : 0) != 0);
			bool flag10 = fleetInfo.IsNormalFleet && this._enableMissionButtons && (missionByFleetId == null && Kerberos.Sots.StarFleet.StarFleet.CollectAvailableSystemsForFleetMission(this.App.GameDatabase, this.App.Game, fleetInfo.ID, MissionType.PIRACY, true).Any<int>()) && (this.App.CurrentState.Name == "StarMapState" && this._missionMode == MissionType.NO_MISSION) && this._game.GameDatabase.HasEndOfFleshExpansion();
			this.App.UI.SetEnabled(this.App.UI.Path(this._contextMenuID, "gameFleetPiracyButton|" + this._widgetID.ToString()), (flag10 ? 1 : 0) != 0);
			this.App.UI.SetVisible(this.App.UI.Path(this._contextMenuID, "gameFleetPiracyButton|" + this._widgetID.ToString()), (flag10 ? 1 : 0) != 0);
			bool flag11 = this.App.AssetDatabase.GetFaction(this.App.GameDatabase.GetPlayerInfo(fleetInfo.PlayerID).FactionID).Name == "loa" && this._game.GameDatabase.HasEndOfFleshExpansion() && fleetInfo.Type != FleetType.FL_ACCELERATOR;
			this.App.UI.SetEnabled(this.App.UI.Path(this._contextMenuID, "gameFleetLoaDissolveToCube|" + this._widgetID.ToString()), (flag11 ? 1 : 0) != 0);
			this.App.UI.SetVisible(this.App.UI.Path(this._contextMenuID, "gameFleetLoaDissolveToCube|" + this._widgetID.ToString()), (flag11 ? 1 : 0) != 0);
			this.App.UI.SetEnabled(this.App.UI.Path(this._contextMenuID, "gameFleetSetLoaComposition|" + this._widgetID.ToString()), (flag11 ? 1 : 0) != 0);
			this.App.UI.SetVisible(this.App.UI.Path(this._contextMenuID, "gameFleetSetLoaComposition|" + this._widgetID.ToString()), (flag11 ? 1 : 0) != 0);
			this.App.UI.AutoSize(this._contextMenuID);
			this.App.UI.ForceLayout(this._contextMenuID);
			this.App.UI.ShowTooltip(this._contextMenuID, float.Parse(eventParams[2]), float.Parse(eventParams[3]));
		}

		private void ShowShipPopup(string[] eventParams)
		{
			this.App.UI.AutoSize(this._shipcontextMenuID);
			this._contextSlot = int.Parse(eventParams[3]);
			ShipInfo shipInfo = this.App.GameDatabase.GetShipInfo(this._contextSlot, true);
			if (shipInfo == null || shipInfo.DesignInfo.PlayerID != this.App.LocalPlayer.ID)
				return;
			bool flag = false;
			if (shipInfo.DesignInfo.Class == ShipClass.Station)
				flag = true;
			FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(shipInfo.FleetID);
			MissionInfo missionByFleetId = this.App.GameDatabase.GetMissionByFleetID(shipInfo.FleetID);
			if (fleetInfo != null)
			{
				this.App.UI.SetEnabled(this.App.UI.Path(this._shipcontextMenuID, "gameRetrofitShip|" + this._widgetID.ToString()), (Kerberos.Sots.StarFleet.StarFleet.IsNewestRetrofit(shipInfo.DesignInfo, this.App.GameDatabase.GetVisibleDesignInfosForPlayer(this.App.LocalPlayer.ID)) || !Kerberos.Sots.StarFleet.StarFleet.SystemSupportsRetrofitting(this.App, fleetInfo.SystemID, this.App.LocalPlayer.ID) || !Kerberos.Sots.StarFleet.StarFleet.FleetCanFunctionWithoutShip(this.App, fleetInfo.ID, shipInfo.ID) ? 0 : (missionByFleetId == null ? 1 : 0)) != 0);
				this.App.UI.ShowTooltip(this._shipcontextMenuID, float.Parse(eventParams[1]), float.Parse(eventParams[2]));
			}
			else
			{
				if (!flag)
					return;
				this.App.UI.SetEnabled(this.App.UI.Path(this._shipcontextMenuID, "gameRetrofitShip|" + this._widgetID.ToString()), (Kerberos.Sots.StarFleet.StarFleet.CanRetrofitStation(this.App, shipInfo.ID) ? 1 : 0) != 0);
				this.App.UI.ShowTooltip(this._shipcontextMenuID, float.Parse(eventParams[1]), float.Parse(eventParams[2]));
			}
		}

		private void DissolveFleet(int src, int tgt)
		{
			FleetInfo fleetInfo1 = this._game.GameDatabase.GetFleetInfo(src);
			FleetInfo fleetInfo2 = this._game.GameDatabase.GetFleetInfo(tgt);
			if (fleetInfo2 != null)
			{
				foreach (ShipInfo shipInfo in (IEnumerable<ShipInfo>)this._game.GameDatabase.GetShipInfoByFleetID(fleetInfo1.ID, false).ToList<ShipInfo>())
					this._game.GameDatabase.TransferShip(shipInfo.ID, fleetInfo2.ID);
				if (!fleetInfo1.IsReserveFleet)
					this._game.GameDatabase.RemoveFleet(src);
			}
			this._contentChanged = true;
			this._fleetsChanged = true;
			this._syncedFleets.Remove(src);
			this.Refresh();
		}

		public void SetFleetsChanged()
		{
			this._fleetsChanged = true;
		}

		public void OnUpdate()
		{
			this._ShipToolTip.Update();
		}

		private void UICommChannel_GameEvent(string eventName, string[] eventParams)
		{
			if (eventName == "FleetWidgetReady" && int.Parse(eventParams[0]) == this._widgetID)
			{
				this._ready = true;
				this.Refresh();
			}
			if (!this._enabled)
				return;
			if (eventName == "ShipHovered" && int.Parse(eventParams[0]) == this._widgetID)
			{
				if (this.DisableTooltips)
					return;
				int num = int.Parse(eventParams[1]);
				if (!(eventParams[4] != this._shipcontextMenuID) || num == -1 || this._ShipToolTip.GetShipID() == num && this._ShipToolTip.isvalid())
					return;
				ShipInfo shipInfo = this.App.GameDatabase.GetShipInfo(num, false);
				if (shipInfo == null || this.App.GameDatabase.GetFleetInfo(shipInfo.FleetID).PlayerID != this.App.LocalPlayer.ID)
					return;
				this._ShipToolTip.Initialize();
				this.App.UI.ShowTooltip(this._ShipToolTip.GetPanelID(), float.Parse(eventParams[2]), float.Parse(eventParams[3]) - (float)Math.Floor(77.5));
				this._ShipToolTip.SyncShipTooltip(num);
			}
			else if (eventName == "ShipLeft")
			{
				if (eventParams[1] != this._shipcontextMenuID)
					this.App.UI.HideTooltip();
				this._ShipToolTip.Dispose(true);
			}
			else if (eventName == "ScrapShipsEvent" && int.Parse(eventParams[0]) == this._widgetID)
			{
				this._shipsToScrap.Clear();
				int num = int.Parse(eventParams[1]);
				for (int index = 0; index < num; ++index)
					this._shipsToScrap.Add(int.Parse(eventParams[2 + index]));
				this._scrapDialog = this.App.UI.CreateDialog((Dialog)new GenericQuestionDialog(this.App, App.Localize("@UI_FLEET_DIALOG_SCRAPSHIPS_TITLE"), string.Format(App.Localize("@UI_FLEET_DIALOG_SCRAPSHIPS_DESC"), (object)num.ToString()), "dialogGenericQuestion"), null);
			}
			else if (eventName == "FleetContextMenu" && int.Parse(eventParams[0]) == this._widgetID)
			{
				if (!this._enableRightClick)
					return;
				this.ShowFleetPopup(eventParams);
			}
			else
			{
				if (eventName == "FleetShipContextMenu" && int.Parse(eventParams[0]) == this._widgetID)
					return;
				if (eventName == "ListContextMenu" && eventParams[4] == "shipList")
					this.ShowShipPopup(eventParams);
				else if (eventName == "FleetTransferEvent" && int.Parse(eventParams[0]) == this._widgetID)
				{
					int num1 = int.Parse(eventParams[1]);
					int num2 = int.Parse(eventParams[2]);
					int num3 = int.Parse(eventParams[3]);
					if (this._game.GameDatabase.GetShipInfo(num1, true).DesignInfo.IsLoaCube())
					{
						this._LoaCubeTransferDialog = this.App.UI.CreateDialog((Dialog)new DialogLoaShipTransfer(this.App, num2, num3, num1, 1), null);
					}
					else
					{
						this._game.GameDatabase.TransferShip(num1, num2);
						if (this.SyncedFleets.Contains(num2))
							this.SyncFleetInfo(this.App.GameDatabase.GetFleetInfo(num2));
						if (this.SyncedFleets.Contains(num3))
							this.SyncFleetInfo(this.App.GameDatabase.GetFleetInfo(num3));
						foreach (FleetWidget linkedWidget in this._linkedWidgets)
						{
							if (linkedWidget.SyncedFleets != null && linkedWidget.SyncedFleets.Count<int>() >= 0)
							{
								if (linkedWidget.SyncedFleets.Contains(num2))
									linkedWidget.SyncFleetInfo(this.App.GameDatabase.GetFleetInfo(num2));
								if (linkedWidget.SyncedFleets.Contains(num3))
									linkedWidget.SyncFleetInfo(this.App.GameDatabase.GetFleetInfo(num3));
							}
						}
						if (this.OnFleetsModified == null)
							return;
						this.OnFleetsModified(this.App, new int[2]
						{
			  num2,
			  num3
						});
					}
				}
				else if (eventName == "FleetDissolveEvent" && int.Parse(eventParams[0]) == this._widgetID)
					this.DissolveFleet(int.Parse(eventParams[1]), int.Parse(eventParams[2]));
				else if (eventName == "CannotModifyEvent" && int.Parse(eventParams[0]) == this._widgetID)
				{
					FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(int.Parse(eventParams[1]));
					if (fleetInfo == null)
						return;
					this.App.UI.CreateDialog((Dialog)new GenericTextDialog(this.App, App.Localize("@UI_FLEET_DIALOG_CANNOT_MODIFY_TITLE"), string.Format(App.Localize("@UI_FLEET_DIALOG_CANNOT_MODIFY_DESC"), (object)fleetInfo.Name), "dialogGenericMessage"), null);
				}
				else if (eventName == "SupportLimitViolationEvent" && int.Parse(eventParams[0]) == this._widgetID)
				{
					if (this.App.GameDatabase.GetFleetInfo(int.Parse(eventParams[1])) == null)
						return;
					this.App.UI.CreateDialog((Dialog)new GenericTextDialog(this.App, App.Localize("@UI_FLEET_DIALOG_CANNOT_COMPLY_TITLE"), App.Localize("@UI_FLEET_DIALOG_CANNOT_COMPLY_DESC"), "dialogGenericMessage"), null);
				}
				else if (eventName == "DifferentSystemsEvent" && int.Parse(eventParams[0]) == this._widgetID)
				{
					int fleetID1 = int.Parse(eventParams[1]);
					int fleetID2 = int.Parse(eventParams[2]);
					FleetInfo fleetInfo1 = this.App.GameDatabase.GetFleetInfo(fleetID1);
					FleetInfo fleetInfo2 = this.App.GameDatabase.GetFleetInfo(fleetID2);
					if (fleetInfo1 == null || fleetInfo2 == null)
						return;
					this.App.UI.CreateDialog((Dialog)new GenericTextDialog(this.App, App.Localize("@UI_FLEET_DIALOG_CANNOT_TRANSFER_TITLE"), string.Format(App.Localize("@UI_FLEET_DIALOG_CANNOT_TRANSFER_DESC"), (object)fleetInfo1.Name, (object)fleetInfo2.Name), "dialogGenericMessage"), null);
				}
				else if (eventName == "SelectionChanged" && int.Parse(eventParams[0]) == this._widgetID)
				{
					this._selectedFleet = int.Parse(eventParams[1]);
					if (this.OnFleetSelectionChanged == null)
						return;
					this.OnFleetSelectionChanged(this._game, this._selectedFleet);
				}
				else if (eventName == "SetPreferred" && int.Parse(eventParams[0]) == this._widgetID)
				{
					this._selectedFleet = int.Parse(eventParams[1]);
					FleetInfo fleetInfo1 = this.App.GameDatabase.GetFleetInfo(this._selectedFleet);
					if (fleetInfo1 == null)
						return;
					foreach (FleetInfo fleetInfo2 in this.App.GameDatabase.GetFleetsByPlayerAndSystem(fleetInfo1.PlayerID, fleetInfo1.SystemID, FleetType.FL_NORMAL).ToList<FleetInfo>())
					{
						if (fleetInfo2.ID != this._selectedFleet)
							this.App.GameDatabase.UpdateFleetPreferred(fleetInfo2.ID, false);
						else
							this.App.GameDatabase.UpdateFleetPreferred(fleetInfo2.ID, true);
					}
				}
				else if (eventName == "ConfirmRepairs" && int.Parse(eventParams[0]) == this._widgetID)
				{
					int index1 = 1;
					int num1 = int.Parse(eventParams[index1]);
					int index2 = index1 + 1;
					for (int index3 = 0; index3 < num1; ++index3)
					{
						Kerberos.Sots.StarFleet.StarFleet.RepairShip(this.App, this.App.GameDatabase.GetShipInfo(int.Parse(eventParams[index2]), true), int.Parse(eventParams[index2 + 1]));
						index2 += 2;
					}
					int num2 = int.Parse(eventParams[index2]);
					int index4 = index2 + 1;
					for (int index3 = 0; index3 < num2; ++index3)
					{
						bool flag = int.Parse(eventParams[index4]) > 0;
						int num3 = int.Parse(eventParams[index4 + 1]);
						int num4 = int.Parse(eventParams[index4 + 2]);
						index4 += 3;
						if (flag)
						{
							ColonyInfo colonyInfo = this.App.GameDatabase.GetColonyInfo(num3);
							if (colonyInfo != null)
							{
								colonyInfo.RepairPoints = num4;
								this.App.GameDatabase.UpdateColony(colonyInfo);
							}
						}
						else
						{
							int num5 = 0;
							ShipInfo shipInfo = this.App.GameDatabase.GetShipInfo(num3, true);
							List<SectionInstanceInfo> list = this.App.GameDatabase.GetShipSectionInstances(num3).ToList<SectionInstanceInfo>();
							Dictionary<SectionInstanceInfo, List<ModuleInstanceInfo>> dictionary1 = new Dictionary<SectionInstanceInfo, List<ModuleInstanceInfo>>();
							Dictionary<SectionInstanceInfo, int> dictionary2 = new Dictionary<SectionInstanceInfo, int>();
							int num6 = ((IEnumerable<DesignSectionInfo>)shipInfo.DesignInfo.DesignSections).Count<DesignSectionInfo>();
							for (int index5 = 0; index5 < num6; ++index5)
							{
								dictionary2.Add(list[index5], list[index5].Structure);
								num5 += list[index5].RepairPoints;
								dictionary1.Add(list[index5], this.App.GameDatabase.GetModuleInstances(list[index5].ID).ToList<ModuleInstanceInfo>());
								if (dictionary1[list[index5]].Count == shipInfo.DesignInfo.DesignSections[index5].Modules.Count<DesignModuleInfo>())
								{
									for (int index6 = 0; index6 < dictionary1[list[index5]].Count; ++index6)
									{
										if (dictionary1[list[index5]][index6].Structure > 0)
										{
											num5 += dictionary1[list[index5]][index6].RepairPoints;
											string modAsset = this.App.GameDatabase.GetModuleAsset(shipInfo.DesignInfo.DesignSections[index5].Modules[index6].ModuleID);
											LogicalModule logicalModule = this.App.AssetDatabase.Modules.Where<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == modAsset)).First<LogicalModule>();
											Dictionary<SectionInstanceInfo, int> dictionary3;
											SectionInstanceInfo index7;
											(dictionary3 = dictionary2)[index7 = list[index5]] = dictionary3[index7] + (int)logicalModule.StructureBonus;
										}
									}
								}
							}
							int num7 = num5 - num4;
							if (num7 > 0)
							{
								foreach (SectionInstanceInfo section in list)
								{
									if (dictionary2[section] > 0)
									{
										if (section.RepairPoints > 0)
										{
											if (section.RepairPoints - num7 > 0)
											{
												section.RepairPoints -= num7;
												num7 = 0;
											}
											else
											{
												num7 -= section.RepairPoints;
												section.RepairPoints = 0;
											}
											this._game.GameDatabase.UpdateSectionInstance(section);
										}
										if (num7 > 0)
										{
											foreach (ModuleInstanceInfo module in dictionary1[section])
											{
												if (module.Structure > 0 && module.RepairPoints > 0)
												{
													if (module.RepairPoints - num7 > 0)
													{
														module.RepairPoints -= num7;
														num7 = 0;
													}
													else
													{
														num7 -= module.RepairPoints;
														module.RepairPoints = 0;
													}
													this._game.GameDatabase.UpdateModuleInstance(module);
												}
												if (num7 <= 0)
													break;
											}
										}
										if (num7 <= 0)
											break;
									}
								}
							}
						}
					}
				}
				else if (eventName == "ConfirmSuulkaStructure" && int.Parse(eventParams[0]) == this._widgetID)
				{
					int shipID = int.Parse(eventParams[1]);
					int points = int.Parse(eventParams[2]);
					int orbitalObjectID = int.Parse(eventParams[3]);
					double num1 = double.Parse(eventParams[4]) - double.Parse(eventParams[5]);
					ShipInfo shipInfo = this.App.GameDatabase.GetShipInfo(shipID, true);
					PlanetInfo planetInfo = this.App.GameDatabase.GetPlanetInfo(orbitalObjectID);
					if (shipInfo == null || planetInfo == null)
						return;
					Kerberos.Sots.StarFleet.StarFleet.RepairShip(this.App, shipInfo, points);
					ColonyInfo colonyInfoForPlanet = this.App.GameDatabase.GetColonyInfoForPlanet(planetInfo.ID);
					if (colonyInfoForPlanet == null)
						return;
					double civilianPopulation = this.App.GameDatabase.GetCivilianPopulation(planetInfo.ID, 0, false);
					double num2 = 0.0;
					if (colonyInfoForPlanet != null)
						num2 = colonyInfoForPlanet.ImperialPop;
					double num3;
					double num4;
					if (num1 > civilianPopulation)
					{
						num3 = num1 - civilianPopulation;
						num4 = 0.0;
					}
					else
					{
						num4 = civilianPopulation - num1;
						num3 = 0.0;
					}
					double num5;
					if (num3 > num2)
					{
						double num6 = num3 - num2;
						num5 = 0.0;
					}
					else
						num5 = num2 - num3;
					IEnumerable<ColonyFactionInfo> civilianPopulations = this.App.GameDatabase.GetCivilianPopulations(planetInfo.ID);
					double num7 = num4 / (double)civilianPopulations.Count<ColonyFactionInfo>();
					foreach (ColonyFactionInfo civPop in civilianPopulations)
					{
						civPop.CivilianPop = num7;
						this.App.GameDatabase.UpdateCivilianPopulation(civPop);
					}
					colonyInfoForPlanet.ImperialPop = num5;
					this.App.GameDatabase.UpdateColony(colonyInfoForPlanet);
					SuulkaType suulkaType = shipInfo.DesignInfo.DesignSections[0].ShipSectionAsset.SuulkaType;
					this.App.PostRequestSpeech(string.Format("STRAT_118-0{0}_{1}_SuulkaLifeDrain", (object)(suulkaType == SuulkaType.TheBlack ? 1 : (int)suulkaType), (object)this.App.GameDatabase.GetFactionName(this.App.GameDatabase.GetPlayerFactionID(this.App.LocalPlayer.ID))), 50, 120, 0.0f);
				}
				else
				{
					if (!(eventName == "ConfirmSuulkaPsi") || int.Parse(eventParams[0]) != this._widgetID)
						return;
					int shipID = int.Parse(eventParams[1]);
					int num1 = int.Parse(eventParams[2]);
					int orbitalObjectID = int.Parse(eventParams[3]);
					int num2 = int.Parse(eventParams[4]);
					ShipInfo shipInfo = this.App.GameDatabase.GetShipInfo(shipID, true);
					PlanetInfo planetInfo = this.App.GameDatabase.GetPlanetInfo(orbitalObjectID);
					if (shipInfo == null || planetInfo == null)
						return;
					shipInfo.PsionicPower += num1;
					this.App.GameDatabase.UpdateShipPsionicPower(shipInfo.ID, shipInfo.PsionicPower);
					planetInfo.Biosphere = num2;
					this.App.GameDatabase.UpdatePlanet(planetInfo);
					SuulkaType suulkaType = shipInfo.DesignInfo.DesignSections[0].ShipSectionAsset.SuulkaType;
					this.App.PostRequestSpeech(string.Format("STRAT_119-0{0}_{1}_SuulkaPsiDrain", (object)(suulkaType == SuulkaType.TheBlack ? 1 : (int)suulkaType), (object)this.App.GameDatabase.GetFactionName(this.App.GameDatabase.GetPlayerFactionID(this.App.LocalPlayer.ID))), 50, 120, 0.0f);
				}
			}
		}

		private bool AdmiralAvailable(int playerid, int systemid)
		{
			return this.App.GameDatabase.GetAdmiralInfosForPlayer(playerid).Where<AdmiralInfo>((Func<AdmiralInfo, bool>)(x => this.App.GameDatabase.GetFleetInfoByAdmiralID(x.ID, FleetType.FL_NORMAL) == null)).Any<AdmiralInfo>();
		}

		private bool CommandShipAvailable(int playerid, int systemid)
		{
			ShipInfo shipInfo1 = (ShipInfo)null;
			int? reserveFleetId = this.App.GameDatabase.GetReserveFleetID(this.App.LocalPlayer.ID, systemid);
			if (reserveFleetId.HasValue)
			{
				foreach (ShipInfo shipInfo2 in this.App.GameDatabase.GetShipInfoByFleetID(reserveFleetId.Value, false))
				{
					if (this.App.LocalPlayer.Faction.Name == "loa")
					{
						shipInfo1 = shipInfo2;
						break;
					}
					if (this.App.GameDatabase.GetShipCommandPointQuota(shipInfo2.ID) > 0)
					{
						shipInfo1 = shipInfo2;
						break;
					}
				}
			}
			return shipInfo1 != null;
		}

		protected void UICommChannel_OnPanelMessage(
		  string panelName,
		  string msgType,
		  string[] msgParams)
		{
			if (!this._enabled)
				return;
			if (msgType == "button_clicked")
			{
				if (panelName.StartsWith("gameRenameFleetButton"))
				{
					string[] strArray = panelName.Split('|');
					if (((IEnumerable<string>)strArray).Count<string>() <= 1 || int.Parse(strArray[1]) != this._widgetID)
						return;
					FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(this._contextSlot);
					if (fleetInfo == null)
						return;
					this._fleetNameDialog = this.App.UI.CreateDialog((Dialog)new GenericTextEntryDialog(this.App, "ENTER FLEET NAME", "Enter a name for your fleet:", fleetInfo.Name, 40, 0, true, EditBoxFilterMode.None), null);
				}
				else if (panelName.StartsWith("admiralbutton"))
				{
					string[] strArray = panelName.Split('|');
					if (((IEnumerable<string>)strArray).Count<string>() <= 1 || !this._enableAdmiralButton)
						return;
					int admiralID = int.Parse(strArray[1]);
					if (this._game.GameDatabase.GetAdmiralInfo(admiralID) == null || FleetWidget.admiralPanel != null)
						return;
					FleetWidget.admiralPanel = this._game.UI.CreateDialog((Dialog)new AdmiralInfoDialog(this._game, admiralID, "admiralPopUp"), null);
				}
				else if (panelName.StartsWith("createfleetbutton"))
				{
					string[] strArray = panelName.Split('|');
					if (((IEnumerable<string>)strArray).Count<string>() <= 1)
						return;
					bool flag1 = this.AdmiralAvailable(this.App.LocalPlayer.ID, int.Parse(strArray[1]));
					bool flag2 = this.CommandShipAvailable(this.App.LocalPlayer.ID, int.Parse(strArray[1]));
					if (!flag1 || !flag2)
					{
						this.App.UI.CreateDialog((Dialog)new GenericTextDialog(this.App, App.Localize("@UI_FLEET_DIALOG_CANNOTCREATEFLEET_TITLE"), App.Localize("@UI_FLEET_DIALOG_CANNOTCREATEFLEET_DESC"), "dialogGenericMessage"), null);
					}
					else
					{
						if (FleetWidget.createfleetpanel != null)
							return;
						FleetWidget.createfleetpanel = this.App.UI.CreateDialog((Dialog)new SelectAdmiralDialog(this.App, int.Parse(strArray[1]), "dialogSelectAdmiral"), null);
					}
				}
				else if (panelName.StartsWith("gameDissolveFleetButton"))
				{
					string[] strArray = panelName.Split('|');
					if (((IEnumerable<string>)strArray).Count<string>() <= 1 || int.Parse(strArray[1]) != this._widgetID)
						return;
					FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(this._contextSlot);
					if (fleetInfo == null)
						return;
					this._dissolveFleetDialog = this.App.UI.CreateDialog((Dialog)new GenericQuestionDialog(this.App, App.Localize("@UI_FLEET_DIALOG_DISSOLVEFLEET_TITLE"), string.Format(App.Localize("@UI_FLEET_DIALOG_DISSOLVEFLEET_DESC"), (object)fleetInfo.Name), "dialogGenericQuestion"), null);
				}
				else if (panelName.StartsWith("gameCancelMissionButton"))
				{
					string[] strArray = panelName.Split('|');
					if (((IEnumerable<string>)strArray).Count<string>() <= 1 || int.Parse(strArray[1]) != this._widgetID || this.App.GameDatabase.GetFleetInfo(this._contextSlot) == null)
						return;
					this._cancelMissionDialog = this.App.UI.CreateDialog((Dialog)new GenericQuestionDialog(this.App, App.Localize("@UI_FLEET_DIALOG_CANCELMISSION_TITLE"), App.Localize("@CANCELMISSIONTEXT"), "dialogGenericQuestion"), null);
				}
				else if (panelName.StartsWith("gameFleetSurveyMissionButton"))
				{
					string[] strArray = panelName.Split('|');
					if (((IEnumerable<string>)strArray).Count<string>() <= 1 || int.Parse(strArray[1]) != this._widgetID)
						return;
					FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(this._contextSlot);
					if (fleetInfo == null || !(this.App.CurrentState.Name == "StarMapState"))
						return;
					((StarMapState)this.App.CurrentState).ShowFleetCentricOverlay(MissionType.SURVEY, fleetInfo.ID);
				}
				else if (panelName.StartsWith("gameFleetLoaDissolveToCube"))
				{
					FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(this._contextSlot);
					if (fleetInfo == null)
						return;
					Kerberos.Sots.StarFleet.StarFleet.ConvertFleetIntoLoaCubes(this.App.Game, fleetInfo.ID);
					this._fleetsChanged = true;
					this._contentChanged = true;
					this.Refresh();
				}
				else if (panelName.StartsWith("gameFleetSetLoaComposition"))
				{
					FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(this._contextSlot);
					if (fleetInfo == null)
						return;
					MissionInfo missionByFleetId = this.App.GameDatabase.GetMissionByFleetID(fleetInfo.ID);
					FleetWidget.SelectCompositionPanel = this.App.UI.CreateDialog((Dialog)new DialogLoaFleetSelector(this.App, missionByFleetId != null ? missionByFleetId.Type : MissionType.NO_MISSION, fleetInfo, false), null);
				}
				else if (panelName.StartsWith("gameFleetColonizeMissionButton"))
				{
					string[] strArray = panelName.Split('|');
					if (((IEnumerable<string>)strArray).Count<string>() <= 1 || int.Parse(strArray[1]) != this._widgetID)
						return;
					FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(this._contextSlot);
					if (fleetInfo == null || !(this.App.CurrentState.Name == "StarMapState"))
						return;
					((StarMapState)this.App.CurrentState).ShowFleetCentricOverlay(MissionType.COLONIZATION, fleetInfo.ID);
				}
				else if (panelName.StartsWith("gameFleetEvacuateButton"))
				{
					string[] strArray = panelName.Split('|');
					if (((IEnumerable<string>)strArray).Count<string>() <= 1 || int.Parse(strArray[1]) != this._widgetID)
						return;
					FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(this._contextSlot);
					if (fleetInfo == null || !(this.App.CurrentState.Name == "StarMapState"))
						return;
					((StarMapState)this.App.CurrentState).ShowFleetCentricOverlay(MissionType.EVACUATE, fleetInfo.ID);
				}
				else if (panelName.StartsWith("gameFleetSuportMissionButton"))
				{
					string[] strArray = panelName.Split('|');
					if (((IEnumerable<string>)strArray).Count<string>() <= 1 || int.Parse(strArray[1]) != this._widgetID)
						return;
					FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(this._contextSlot);
					if (fleetInfo == null || !(this.App.CurrentState.Name == "StarMapState"))
						return;
					((StarMapState)this.App.CurrentState).ShowFleetCentricOverlay(MissionType.SUPPORT, fleetInfo.ID);
				}
				else if (panelName.StartsWith("gameFleetRelocateMissionButton"))
				{
					string[] strArray = panelName.Split('|');
					if (((IEnumerable<string>)strArray).Count<string>() <= 1 || int.Parse(strArray[1]) != this._widgetID)
						return;
					FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(this._contextSlot);
					if (fleetInfo == null || !(this.App.CurrentState.Name == "StarMapState"))
						return;
					((StarMapState)this.App.CurrentState).ShowFleetCentricOverlay(MissionType.RELOCATION, fleetInfo.ID);
				}
				else if (panelName.StartsWith("gameFleetPatrolMissionButton"))
				{
					string[] strArray = panelName.Split('|');
					if (((IEnumerable<string>)strArray).Count<string>() <= 1 || int.Parse(strArray[1]) != this._widgetID)
						return;
					FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(this._contextSlot);
					if (fleetInfo == null || !(this.App.CurrentState.Name == "StarMapState"))
						return;
					((StarMapState)this.App.CurrentState).ShowFleetCentricOverlay(MissionType.PATROL, fleetInfo.ID);
				}
				else if (panelName.StartsWith("gameFleetInterdictMissionButton"))
				{
					string[] strArray = panelName.Split('|');
					if (((IEnumerable<string>)strArray).Count<string>() <= 1 || int.Parse(strArray[1]) != this._widgetID)
						return;
					FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(this._contextSlot);
					if (fleetInfo == null || !(this.App.CurrentState.Name == "StarMapState"))
						return;
					((StarMapState)this.App.CurrentState).ShowFleetCentricOverlay(MissionType.INTERDICTION, fleetInfo.ID);
				}
				else if (panelName.StartsWith("gameFleetInvadeMissionButton"))
				{
					string[] strArray = panelName.Split('|');
					if (((IEnumerable<string>)strArray).Count<string>() <= 1 || int.Parse(strArray[1]) != this._widgetID)
						return;
					FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(this._contextSlot);
					if (fleetInfo == null || !(this.App.CurrentState.Name == "StarMapState"))
						return;
					((StarMapState)this.App.CurrentState).ShowFleetCentricOverlay(MissionType.INVASION, fleetInfo.ID);
				}
				else if (panelName.StartsWith("gameFleetStrikeMissionButton"))
				{
					string[] strArray = panelName.Split('|');
					if (((IEnumerable<string>)strArray).Count<string>() <= 1 || int.Parse(strArray[1]) != this._widgetID)
						return;
					FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(this._contextSlot);
					if (fleetInfo == null || !(this.App.CurrentState.Name == "StarMapState"))
						return;
					((StarMapState)this.App.CurrentState).ShowFleetCentricOverlay(MissionType.STRIKE, fleetInfo.ID);
				}
				else if (panelName.StartsWith("gameFleetPiracyButton"))
				{
					string[] strArray = panelName.Split('|');
					if (((IEnumerable<string>)strArray).Count<string>() <= 1 || int.Parse(strArray[1]) != this._widgetID)
						return;
					FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(this._contextSlot);
					if (fleetInfo == null || !(this.App.CurrentState.Name == "StarMapState"))
						return;
					((StarMapState)this.App.CurrentState).ShowFleetCentricOverlay(MissionType.PIRACY, fleetInfo.ID);
				}
				else
				{
					if (!panelName.StartsWith("gameRetrofitShip"))
						return;
					string[] strArray = panelName.Split('|');
					if (((IEnumerable<string>)strArray).Count<string>() <= 1 || int.Parse(strArray[1]) != this._widgetID)
						return;
					ShipInfo shipInfo = this.App.GameDatabase.GetShipInfo(this._contextSlot, true);
					if (shipInfo == null)
						return;
					if (shipInfo.DesignInfo.Class == ShipClass.Station && shipInfo.DesignInfo.StationType != StationType.INVALID_TYPE)
						this._retrofitShipDialog = this.App.UI.CreateDialog((Dialog)new RetrofitStationDialog(this.App, shipInfo), null);
					else
						this._retrofitShipDialog = this.App.UI.CreateDialog((Dialog)new RetrofitShipDialog(this.App, shipInfo), null);
				}
			}
			else
			{
				if (!(msgType == "dialog_closed"))
					return;
				if (panelName == this._fleetNameDialog)
				{
					if (!bool.Parse(msgParams[0]) || msgParams[1].Length <= 0)
						return;
					FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(this._contextSlot);
					if (fleetInfo == null)
						return;
					fleetInfo.Name = msgParams[1];
					this.App.GameDatabase.UpdateFleetInfo(fleetInfo);
					this._fleetsChanged = true;
					this._contentChanged = true;
					this.Refresh();
				}
				else if (panelName == this._LoaCubeTransferDialog)
				{
					if (((IEnumerable<string>)msgParams).Count<string>() != 4)
						return;
					int fleetID1 = int.Parse(msgParams[0]);
					int fleetID2 = int.Parse(msgParams[1]);
					int shipID = int.Parse(msgParams[2]);
					int Loacubes = int.Parse(msgParams[3]);
					ShipInfo shipInfo1 = this.App.GameDatabase.GetShipInfo(shipID, true);
					ShipInfo shipInfo2 = this.App.GameDatabase.GetShipInfoByFleetID(fleetID1, false).FirstOrDefault<ShipInfo>((Func<ShipInfo, bool>)(x => x.DesignInfo.IsLoaCube()));
					if (shipInfo2 == null)
						this.App.GameDatabase.InsertShip(fleetID1, shipInfo1.DesignInfo.ID, "Cube", (ShipParams)0, new int?(), Loacubes);
					else
						this.App.GameDatabase.UpdateShipLoaCubes(shipInfo2.ID, shipInfo2.LoaCubes + Loacubes);
					if (shipInfo1.LoaCubes <= Loacubes)
					{
						int fleetId = shipInfo1.FleetID;
						this.App.GameDatabase.RemoveShip(shipInfo1.ID);
						if (this.App.GameDatabase.GetShipsByFleetID(fleetId).Count<int>() == 0 && this.App.GameDatabase.GetFleetInfo(fleetId).Type != FleetType.FL_RESERVE)
							this.App.GameDatabase.RemoveFleet(fleetId);
					}
					else
						this.App.GameDatabase.UpdateShipLoaCubes(shipInfo1.ID, shipInfo1.LoaCubes - Loacubes);
					if (this.SyncedFleets.Contains(fleetID1))
						this.SyncFleetInfo(this.App.GameDatabase.GetFleetInfo(fleetID1));
					if (this.SyncedFleets.Contains(fleetID2))
						this.SyncFleetInfo(this.App.GameDatabase.GetFleetInfo(fleetID2));
					foreach (FleetWidget linkedWidget in this._linkedWidgets)
					{
						if (linkedWidget.SyncedFleets != null && linkedWidget.SyncedFleets.Count<int>() >= 0)
						{
							if (linkedWidget.SyncedFleets.Contains(fleetID1))
								linkedWidget.SyncFleetInfo(this.App.GameDatabase.GetFleetInfo(fleetID1));
							if (linkedWidget.SyncedFleets.Contains(fleetID2))
								linkedWidget.SyncFleetInfo(this.App.GameDatabase.GetFleetInfo(fleetID2));
						}
					}
					if (this.OnFleetsModified != null)
						this.OnFleetsModified(this.App, new int[2]
						{
			  fleetID1,
			  fleetID2
						});
					this._contentChanged = true;
					this._fleetsChanged = true;
					this.Refresh();
				}
				else if (panelName == FleetWidget.SelectCompositionPanel)
				{
					if (!(msgParams[0] != ""))
						return;
					int num = int.Parse(msgParams[0]);
					if (num == 0)
						return;
					FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(this._contextSlot);
					if (fleetInfo == null)
						return;
					Kerberos.Sots.StarFleet.StarFleet.ConvertFleetIntoLoaCubes(this.App.Game, fleetInfo.ID);
					this.App.GameDatabase.UpdateFleetCompositionID(fleetInfo.ID, new int?(num));
					Kerberos.Sots.StarFleet.StarFleet.BuildFleetFromComposition(this.App.Game, fleetInfo.ID, MissionType.NO_MISSION);
					this._fleetsChanged = true;
					this._contentChanged = true;
					this.Refresh();
				}
				else if (panelName == this._retrofitShipDialog)
				{
					this._fleetsChanged = true;
					this._contentChanged = true;
					this.Refresh();
				}
				else if (panelName == FleetWidget.admiralPanel)
					FleetWidget.admiralPanel = null;
				else if (panelName == FleetWidget.createfleetpanel)
					FleetWidget.createfleetpanel = null;
				else if (panelName == this._scrapDialog)
				{
					if (!bool.Parse(msgParams[0]))
						return;
					List<string> stringList = new List<string>();
					int fleetID = 0;
					bool flag = false;
					foreach (int num in this._shipsToScrap)
					{
						int ship = num;
						stringList.Add(this.App.GameDatabase.GetShipInfo(ship, false).ShipName);
						fleetID = this.App.GameDatabase.GetShipInfo(ship, false).FleetID;
						if (this.App.GameDatabase.GetShipInfo(ship, true).DesignInfo.Class == ShipClass.Station && !this.App.GameDatabase.GetShipInfo(ship, true).IsPlatform())
						{
							StationInfo stationInfo = this.App.GameDatabase.GetStationInfos().FirstOrDefault<StationInfo>((Func<StationInfo, bool>)(x => x.ShipID == ship));
							if (stationInfo != null)
							{
								this.App.GameDatabase.DestroyStation(this.App.Game, stationInfo.OrbitalObjectID, 0);
								flag = true;
							}
						}
						else
							this.App.GameDatabase.RemoveShip(ship);
						FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(fleetID);
						if (fleetInfo != null && fleetInfo.Type != FleetType.FL_DEFENSE && (fleetInfo.Type != FleetType.FL_RESERVE && fleetInfo.Type != FleetType.FL_STATION) && !this.App.GameDatabase.GetShipsByFleetID(fleetID).Any<int>())
						{
							this.App.GameDatabase.RemoveFleet(fleetID);
							this._syncedFleets.Remove(fleetID);
						}
					}
					this.SetSyncedFleets(this._syncedFleets);
					if (flag && this.App.CurrentState.Name == "StarMapState")
						((StarMapState)this.App.CurrentState).RefreshStarmap(StarMapState.StarMapRefreshType.REFRESH_ALL);
					if (stringList.Count <= 0)
						return;
					string str1 = "";
					for (int index = 0; index < stringList.Count - 1; ++index)
						str1 = str1 + stringList[index] + ", ";
					string str2 = str1 + stringList[stringList.Count - 1];
					this.App.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_SHIPS_RECYCLED,
						EventMessage = TurnEventMessage.EM_SHIPS_RECYCLED,
						PlayerID = this.App.LocalPlayer.ID,
						SystemID = fleetID != 0 ? (this.App.GameDatabase.GetFleetInfo(fleetID) != null ? this.App.GameDatabase.GetFleetInfo(fleetID).SystemID : 0) : 0,
						NamesList = str2,
						TurnNumber = this.App.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
				}
				else if (panelName == this._dissolveFleetDialog)
				{
					if (!bool.Parse(msgParams[0]))
						return;
					FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(this._contextSlot);
					if (fleetInfo == null)
						return;
					int? reserveFleetId = this.App.GameDatabase.GetReserveFleetID(this.App.LocalPlayer.ID, fleetInfo.SystemID);
					if (reserveFleetId.HasValue)
					{
						this.DissolveFleet(fleetInfo.ID, reserveFleetId.Value);
						if (this.App.CurrentState.Name == "StarMapState")
							((StarMapState)this.App.CurrentState).RefreshStarmap(StarMapState.StarMapRefreshType.REFRESH_ALL);
					}
					if (!fleetInfo.IsAcceleratorFleet && !fleetInfo.IsGateFleet || reserveFleetId.HasValue)
						return;
					this.App.GameDatabase.RemoveFleet(fleetInfo.ID);
					if (!(this.App.CurrentState.Name == "StarMapState"))
						return;
					((StarMapState)this.App.CurrentState).RefreshStarmap(StarMapState.StarMapRefreshType.REFRESH_ALL);
				}
				else
				{
					if (!(panelName == this._cancelMissionDialog) || !bool.Parse(msgParams[0]))
						return;
					FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(this._contextSlot);
					if (fleetInfo == null)
						return;
					AdmiralInfo admiralInfo = this.App.GameDatabase.GetAdmiralInfo(fleetInfo.AdmiralID);
					if (admiralInfo != null)
						this.App.PostRequestSpeech(string.Format("STRAT_008-01_{0}_{1}UniversalMissionNegation", (object)this.App.GameDatabase.GetFactionName(this.App.GameDatabase.GetPlayerFactionID(this.App.LocalPlayer.ID)), (object)admiralInfo.GetAdmiralSoundCueContext(this.App.AssetDatabase)), 50, 120, 0.0f);
					Kerberos.Sots.StarFleet.StarFleet.CancelMission(this.App.Game, fleetInfo, true);
					this._fleetsChanged = true;
					this._contentChanged = true;
					this.Refresh();
					if (!typeof(StarMapState).IsAssignableFrom(this.App.CurrentState.GetType()))
						return;
					StarMapState.UpdateGateUI(this.App.Game, "gameGateInfo");
				}
			}
		}

		public void Dispose()
		{
			this.App.UI.DestroyPanel(this._contextMenuID);
			this.App.UI.DestroyPanel(this._shipcontextMenuID);
			this._ShipToolTip.Dispose(false);
			this._game.UI.GameEvent -= new UIEventGameEvent(this.UICommChannel_GameEvent);
			this._game.UI.PanelMessage -= new UIEventPanelMessage(this.UICommChannel_OnPanelMessage);
			if (this._game == null)
				return;
			this._game.ReleaseObject((IGameObject)this);
		}

		public delegate void FleetSelectionChangedDelegate(App app, int SelectedFleetId);

		public delegate void FleetsModifiedDelegate(App app, int[] modifiedFleetIds);

		public enum FilterShips
		{
			Enable,
			Disable,
			Ignore,
		}

		public delegate FleetWidget.FilterShips FleetWidgetShipFilter(
		  ShipInfo ship,
		  DesignInfo design);

		public delegate FleetWidget.FilterShips FleetWidgetFleetFilter(FleetInfo fleet);
	}
}
