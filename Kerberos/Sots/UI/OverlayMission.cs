// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.OverlayMission
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal abstract class OverlayMission : Dialog
	{
		private readonly string SystemButton = "systembtn";
		private int _nextMissionNoteItemId = 1;
		private readonly List<ShipItem> _vitals = new List<ShipItem>();
		private readonly List<ShipItem> _escorts = new List<ShipItem>();
		private readonly List<ShipItem> _additionalShips = new List<ShipItem>();
		protected const string UIFleetList = "gameFleetList";
		protected const string UISystemList = "gameSystemList";
		protected const string UIRelocateFleetList = "gameRelocateFleet";
		protected const string UISystemMap = "partMiniSystem";
		protected const string UIAvailableFleetsList = "invalid";
		protected const string UIFleetVitalsList = "invalid";
		protected const string UIFleetEscortsList = "invalid";
		protected const string UISystemDetails = "systemDetailsWidget";
		protected const string UIMissionTimes = "gameMissionTimes";
		protected const string UIMissionAdmiralName = "gameAdmiralName";
		protected const string UIMissionAdmiralFleet = "gameAdmiralFleet";
		protected const string UIMissionAdmiralSkills = "gameAdmiralSkills";
		protected const string UIMissionAdmiralAvatar = "gameAdmiralAvatar";
		protected const string UIMissionTitle = "gameMissionTitle";
		protected const string UIConfirmMissionButton = "gameConfirmMissionButton";
		protected const string UIConfirmMissionAndContinueButton = "gameConfirmAndContinueMissionButton";
		protected const string UICancelMissionButton = "gameCancelMissionButton";
		protected const string UIMissionPlanetDetails = "gameMissionPlanet";
		protected const string UIMissionNotes = "gameMissionNotes";
		protected const string UIToggleRebase = "gameRebaseToggle";
		protected const string UIToggleRebaseStn = "gameRebaseToggleStn";
		protected const string UI2bbackground = "2B_Background";
		protected const string UI3bbackground = "3B_Background";
		protected const string UILYSlider = "LYSlider";
		protected const string UILYLabel = "right_label";
		protected const string UIAccelListBox = "AccelListBox";
		protected const string UIAccelList = "AccelList";
		protected const string UIPlaceAccelButton = "gamePlaceAccelerator";
		protected const int UIItemMissionTotalTime = 0;
		protected const int UIItemMissionTravelTime = 1;
		protected const int UIItemMissionTime = 2;
		protected const int UIItemMissionBuildTime = 3;
		protected const int UIItemMissionCostSeparator = 4;
		protected const int UIItemMissionCost = 5;
		protected const int UIItemMissionSupportTime = 6;
		protected App App;
		private IGameObject _selectedObject;
		protected int _selectedFleet;
		protected int _selectedPlanet;
		protected MissionEstimate _missionEstimate;
		protected FleetWidget _fleetWidget;
		private bool _canConfirm;
		public bool BlindEnter;
		protected bool _useDirectRoute;
		private string _zuulConfirm;
		private string _zuulBoreMissing;
		private string _zuulBoreRoute;
		private string _loaDirectRoute;
		private string _loaShearFleetConfirm;
		private string _loaFleetCompoSel;
		protected SystemWidget _systemWidget;
		private MissionType _missionType;
		private List<int> _ActiveSystems;
		protected bool _fleetCentric;
		protected bool _canExit;
		private int _TargetSystem;
		private int _rebasetarget;
		protected bool PathDrawEnabled;
		private bool _rebaseToggle;
		protected StarMap _starMap;
		private StarMapState _starMapState;
		private StarMapViewFilter _lastFilter;
		private bool _shown;

		private string MissionScreen { get; set; }

		public int TargetSystem
		{
			get
			{
				return this._TargetSystem;
			}
			set
			{
				if (this._rebaseToggle)
				{
					this.RebaseTarget = new int?(value);
					this.UpdateRebaseUI();
				}
				else
					this._TargetSystem = value;
			}
		}

		protected int? RebaseTarget
		{
			get
			{
				if (this._rebasetarget != 0)
					return new int?(this._rebasetarget);
				return new int?();
			}
			set
			{
				this._rebasetarget = value.Value;
			}
		}

		public bool CanConfirm
		{
			get
			{
				return this._canConfirm;
			}
		}

		public MissionType MissionType
		{
			get
			{
				return this._missionType;
			}
		}

		protected abstract bool CanConfirmMission();

		protected abstract void OnCommitMission();

		protected abstract void OnCanConfirmMissionChanged(bool newValue);

		protected abstract string GetMissionDetailsTitle();

		protected abstract void OnRefreshMissionDetails(MissionEstimate estimate);

		protected abstract IEnumerable<int> GetMissionTargetPlanets();

		protected bool IsValidFleetID(int fleetID)
		{
			return fleetID != 0 && !this.App.GameDatabase.GetFleetInfo(fleetID).IsReserveFleet;
		}

		protected bool ReBaseToggle
		{
			get
			{
				return this._rebaseToggle;
			}
			set
			{
				if (this._rebaseToggle == value)
					return;
				this.RebaseMode(value);
			}
		}

		public int SelectedPlanet
		{
			get
			{
				return this._selectedPlanet;
			}
			set
			{
				if (this._selectedPlanet == value)
					return;
				this._selectedPlanet = value;
				bool flag = false;
				this.App.GameDatabase.IsSurveyed(this.App.LocalPlayer.ID, this.SelectedSystem);
				if (this.SelectedPlanet != StarSystemDetailsUI.StarItemID)
				{
					PlanetInfo planetInfo = this.App.GameDatabase.GetPlanetInfo(this.SelectedPlanet);
					if (planetInfo != null && StellarBodyTypes.IsTerrestrial(planetInfo.Type.ToLowerInvariant()))
						flag = true;
				}
				this.App.UI.SetVisible("gameMissionPlanet", flag);
				this.RefreshMissionDetails(StationType.INVALID_TYPE, 1);
			}
		}

		private int SelectedSystem
		{
			get
			{
				StarMapSystem selectedObject = this._selectedObject as StarMapSystem;
				int num;
				if (selectedObject == null || !this.App.GetGameState<StarMapState>().StarMap.Systems.Forward.TryGetValue(selectedObject, out num))
					return 0;
				return num;
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
				if (this._selectedFleet == value)
					return;
				this._selectedFleet = value;
				this.RebuildShipLists(this.SelectedFleet);
				this.RefreshMissionDetails(this.GetSelectedStationtype(), 1);
				this.UpdateCanConfirmMission();
				this._starMapState.ClearStarmapFleetArrows();
				this._starMap.ClearSystemEffects();
				FleetInfo fleetInfo = this._app.GameDatabase.GetFleetInfo(this._selectedFleet);
				List<Vector3> path = new List<Vector3>();
				if (fleetInfo != null)
				{
					PlayerInfo playerInfo = this._app.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID);
					StarSystemInfo starSystemInfo1 = this._app.GameDatabase.GetStarSystemInfo(fleetInfo.SystemID);
					Vector3? nullable = new Vector3?();
					if (this.MissionType == MissionType.INTERCEPT || this.MissionType == MissionType.SPECIAL_CONSTRUCT_STN)
					{
						int target = this.MissionType == MissionType.SPECIAL_CONSTRUCT_STN ? ((OverlaySpecialConstructionMission)this).TargetFleet : ((OverlayInterceptMission)this).TargetFleet;
						if (this._starMap.Fleets.Reverse.ContainsKey(target))
						{
							StarMapFleet starMapFleet = this._starMap.Fleets.Reverse.FirstOrDefault<KeyValuePair<int, StarMapFleet>>((Func<KeyValuePair<int, StarMapFleet>, bool>)(x => x.Key == target)).Value;
							if (starMapFleet != null)
								nullable = new Vector3?(starMapFleet.Position);
						}
					}
					else
					{
						StarSystemInfo starSystemInfo2 = this._app.GameDatabase.GetStarSystemInfo(this.TargetSystem);
						if (starSystemInfo2 != (StarSystemInfo)null && starSystemInfo1 != (StarSystemInfo)null)
						{
							this._starMap.SetMissionEffectTarget(starSystemInfo2, true);
							nullable = new Vector3?(starSystemInfo2.Origin);
							int tripTime;
							float tripDistance;
							foreach (int systemId in Kerberos.Sots.StarFleet.StarFleet.GetBestTravelPath(this.App.Game, fleetInfo.ID, starSystemInfo1.ID, starSystemInfo2.ID, out tripTime, out tripDistance, false, new float?(), new float?()))
								path.Add(this.App.GameDatabase.GetStarSystemOrigin(systemId));
						}
					}
					if (starSystemInfo1 != (StarSystemInfo)null && nullable.HasValue && this.PathDrawEnabled)
					{
						if (path.Count == 0)
							this._starMapState.AddMissionFleetArrow(starSystemInfo1.Origin, nullable.Value, playerInfo.PrimaryColor);
						else
							this._starMapState.AddMissionFleetArrow(path, playerInfo.PrimaryColor);
					}
				}
				if (this.ReBaseToggle)
					this.ReBaseToggle = false;
				this.RebaseTarget = new int?(0);
				this.UpdateRebaseUI();
			}
		}

		public static string GetAdmiralTraitText(AdmiralInfo.TraitType trait)
		{
			switch (trait)
			{
				case AdmiralInfo.TraitType.Thrifty:
					return App.Localize("@ADMIRALTRAITS_THRIFTY");
				case AdmiralInfo.TraitType.Wastrel:
					return App.Localize("@ADMIRALTRAITS_WASTREL");
				case AdmiralInfo.TraitType.Pathfinder:
					return App.Localize("@ADMIRALTRAITS_PATHFINDER");
				case AdmiralInfo.TraitType.Slippery:
					return App.Localize("@ADMIRALTRAITS_SLIPPERY");
				case AdmiralInfo.TraitType.Livingstone:
					return App.Localize("@ADMIRALTRAITS_LIVINGSTONE");
				case AdmiralInfo.TraitType.Conscript:
					return App.Localize("@ADMIRALTRAITS_CONSCRIPT");
				case AdmiralInfo.TraitType.TrueBeliever:
					return App.Localize("@ADMIRALTRAITS_TRUEBELIEVER");
				case AdmiralInfo.TraitType.GoodShepherd:
					return App.Localize("@ADMIRALTRAITS_GOODSHEPHERD");
				case AdmiralInfo.TraitType.BadShepherd:
					return App.Localize("@ADMIRALTRAITS_BADSHEPHERD");
				case AdmiralInfo.TraitType.GreenThumb:
					return App.Localize("@ADMIRALTRAITS_GREENTHUMB");
				case AdmiralInfo.TraitType.BlackThumb:
					return App.Localize("@ADMIRALTRAITS_BLACKTHUMB");
				case AdmiralInfo.TraitType.DrillSergeant:
					return App.Localize("@ADMIRALTRAITS_DRILLSERGEANT");
				case AdmiralInfo.TraitType.Vigilant:
					return App.Localize("@ADMIRALTRAITS_VIGILANT");
				case AdmiralInfo.TraitType.Architect:
					return App.Localize("@ADMIRALTRAITS_ARCHITECT");
				case AdmiralInfo.TraitType.Inquisitor:
					return App.Localize("@ADMIRALTRAITS_INQUISITOR");
				case AdmiralInfo.TraitType.Evangelist:
					return App.Localize("@ADMIRALTRAITS_EVANGELIST");
				case AdmiralInfo.TraitType.HeadHunter:
					return App.Localize("@ADMIRALTRAITS_HEAD_HUNTER");
				case AdmiralInfo.TraitType.TrueGrit:
					return App.Localize("@ADMIRALTRAITS_TRUEGRIT");
				case AdmiralInfo.TraitType.Hunter:
					return App.Localize("@ADMIRALTRAITS_HUNTER");
				case AdmiralInfo.TraitType.Defender:
					return App.Localize("@ADMIRALTRAITS_DEFENDER");
				case AdmiralInfo.TraitType.Attacker:
					return App.Localize("@ADMIRALTRAITS_ATTACKER");
				case AdmiralInfo.TraitType.ArtilleryExpert:
					return App.Localize("@ADMIRALTRAITS_ARTILLERYEXPERT");
				case AdmiralInfo.TraitType.Psion:
					return App.Localize("@ADMIRALTRAITS_PSION");
				case AdmiralInfo.TraitType.Skeptic:
					return App.Localize("@ADMIRALTRAITS_SKEPTIC");
				case AdmiralInfo.TraitType.MediaHero:
					return App.Localize("@ADMIRALTRAITS_MEDIAHERO");
				case AdmiralInfo.TraitType.GloryHound:
					return App.Localize("@ADMIRALTRAITS_GLORYHOUND");
				case AdmiralInfo.TraitType.Sherman:
					return App.Localize("@ADMIRALTRAITS_SHERMAN");
				case AdmiralInfo.TraitType.Technophobe:
					return App.Localize("@ADMIRALTRAITS_TECHNOPHOBE");
				case AdmiralInfo.TraitType.Elite:
					return App.Localize("@ADMIRALTRAITS_ELITE");
				default:
					throw new ArgumentOutOfRangeException(nameof(trait));
			}
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
					empty += OverlayMission.GetAdmiralTraitText(traitType);
				}
			}
			return empty;
		}

		public void UpdateRebaseUI()
		{
			if (this.SelectedFleet != 0)
			{
				if (this.RebaseTarget.HasValue)
				{
					StarSystemInfo starSystemInfo = this.App.GameDatabase.GetStarSystemInfo(this.RebaseTarget.Value);
					this.App.UI.SetVisible(this.UI.Path(this.ID, "gameRebaseTarget"), true);
					this.App.UI.SetText(this.UI.Path(this.ID, "gameRebaseTarget"), string.Format(App.Localize("@UI_FLEET_REBASE_DESC"), (object)starSystemInfo.Name));
					if (this._rebaseToggle)
						this.App.UI.SetText(this.UI.Path(this.ID, "gameEnterRebase"), App.Localize("@UI_DONE_REBASE"));
					else
						this.App.UI.SetText(this.UI.Path(this.ID, "gameEnterRebase"), App.Localize("@UI_REBASE_TARGET"));
				}
				else if (this._rebaseToggle)
				{
					this.App.UI.SetVisible(this.UI.Path(this.ID, "gameRebaseTarget"), true);
					this.App.UI.SetText(this.UI.Path(this.ID, "gameRebaseTarget"), App.Localize("@UI_SELECT_REBASE_SYSTEM"));
					this.App.UI.SetText(this.UI.Path(this.ID, "gameEnterRebase"), App.Localize("@UI_DONE_REBASE"));
				}
				else
				{
					this.App.UI.SetVisible(this.UI.Path(this.ID, "gameRebaseTarget"), false);
					this.App.UI.SetText(this.UI.Path(this.ID, "gameEnterRebase"), App.Localize("@UI_REBASE_TARGET"));
				}
			}
			else
			{
				this.App.UI.SetVisible(this.UI.Path(this.ID, "gameRebaseTarget"), false);
				this.App.UI.SetText(this.UI.Path(this.ID, "gameEnterRebase"), App.Localize("@UI_REBASE_TARGET"));
			}
		}

		public void RebaseMode(bool mode)
		{
			if (mode)
			{
				this.CompileListOfRelocatableSystemsForFleet(this.SelectedFleet, true);
				this._starMap.SelectEnabled = true;
				this._starMap.FocusEnabled = true;
				if (Kerberos.Sots.StarFleet.StarFleet.CanDoRelocationMissionToTarget(this._app.Game, this.TargetSystem, this.SelectedFleet) || this.MissionType == MissionType.COLONIZATION)
					this.RebaseTarget = new int?(this.TargetSystem);
			}
			else
			{
				if (this._starMap.Systems.Reverse.ContainsKey(this.TargetSystem))
				{
					StarMapSystem starMapSystem = this._starMap.Systems.Reverse.FirstOrDefault<KeyValuePair<int, StarMapSystem>>((Func<KeyValuePair<int, StarMapSystem>, bool>)(x => x.Key == this.TargetSystem)).Value;
					this._starMap.Select((IGameObject)starMapSystem);
					this._starMap.SetFocus((IGameObject)starMapSystem);
				}
				this._starMap.SelectEnabled = false;
				this._starMap.FocusEnabled = false;
				this.SystemCentricSystemUIRefresh();
			}
			this._rebaseToggle = mode;
			this.UpdateRebaseUI();
		}

		public List<int> CompileListOfRelocatableSystemsForFleet(int fleetid, bool updatestarmap = true)
		{
			if (updatestarmap)
			{
				foreach (int key in this._starMap.Systems.Reverse.Keys)
				{
					this._starMap.Systems.Reverse[key].SetIsEnabled(false);
					this._starMap.Systems.Reverse[key].SetIsSelectable(false);
				}
			}
			List<StarSystemInfo> list = this.App.GameDatabase.GetStarSystemInfos().ToList<StarSystemInfo>();
			FleetInfo fleetInfo = this._app.GameDatabase.GetFleetInfo(fleetid);
			List<int> intList = new List<int>();
			foreach (int systemId in list.Select<StarSystemInfo, int>((Func<StarSystemInfo, int>)(x => x.ID)))
			{
				if (Kerberos.Sots.StarFleet.StarFleet.CanDoRelocationMissionToTarget(this._app.Game, systemId, fleetid))
					intList.Add(systemId);
			}
			if (!intList.Contains(fleetInfo.SupportingSystemID))
				intList.Add(fleetInfo.SupportingSystemID);
			if (updatestarmap)
			{
				foreach (int key in this._starMap.Systems.Reverse.Keys)
				{
					this._starMap.Systems.Reverse[key].SetIsEnabled(intList.Contains(key));
					this._starMap.Systems.Reverse[key].SetIsSelectable(intList.Contains(key));
				}
			}
			return intList;
		}

		public static void RefreshFleetAdmiralDetails(
		  App _app,
		  string ID,
		  int fleetId,
		  string Element = "admiralDetails")
		{
			FleetInfo fleetInfo = _app.GameDatabase.GetFleetInfo(fleetId);
			_app.GameDatabase.GetFactionName(_app.GameDatabase.GetFleetFaction(fleetId));
			string empty1 = string.Empty;
			string empty2 = string.Empty;
			if (fleetInfo.AdmiralID == 0)
				return;
			_app.UI.SetVisible(_app.UI.Path(ID, Element, "traitsLabel"), true);
			AdmiralInfo admiralInfo = _app.GameDatabase.GetAdmiralInfo(fleetInfo.AdmiralID);
			string name = admiralInfo.Name;
			OverlayMission.GetAdmiralTraitsString(_app, fleetInfo.AdmiralID);
			string race = admiralInfo.Race;
			string str = "Deep Space";
			if (admiralInfo.HomeworldID.HasValue)
			{
				OrbitalObjectInfo orbitalObjectInfo = _app.GameDatabase.GetOrbitalObjectInfo(admiralInfo.HomeworldID.Value);
				if (orbitalObjectInfo != null)
				{
					if (_app.GameDatabase.GetFleetInfoByAdmiralID(admiralInfo.ID, FleetType.FL_NORMAL) != null)
					{
						StarSystemInfo starSystemInfo = _app.GameDatabase.GetStarSystemInfo(fleetInfo.SystemID);
						if (starSystemInfo != (StarSystemInfo)null)
							str = starSystemInfo.Name;
					}
					else
						str = _app.GameDatabase.GetStarSystemInfo(orbitalObjectInfo.StarSystemID).Name;
				}
			}
			_app.UI.SetPropertyString(_app.UI.Path(ID, "admiralName"), "text", string.Format(App.Localize("@ADMIRAL_DIALOG_NAME_COLON"), (object)admiralInfo.Name));
			_app.UI.SetPropertyString(_app.UI.Path(ID, "admiralLocation"), "text", string.Format(App.Localize("@ADMIRAL_DIALOG_LOCATION_COLON"), (object)str));
			_app.UI.SetPropertyString(_app.UI.Path(ID, "admiralAge"), "text", string.Format(App.Localize("@ADMIRAL_DIALOG_AGE_COLON"), (object)((int)admiralInfo.Age).ToString()));
			string admiralAvatar = Kerberos.Sots.StarFleet.StarFleet.GetAdmiralAvatar(_app, admiralInfo.ID);
			_app.UI.SetPropertyString(_app.UI.Path(ID, "admiralImage"), "sprite", admiralAvatar);
			IEnumerable<AdmiralInfo.TraitType> admiralTraits = _app.GameDatabase.GetAdmiralTraits(admiralInfo.ID);
			_app.UI.ClearItems(_app.UI.Path(ID, "admiralTraits"));
			int userItemId = 0;
			foreach (AdmiralInfo.TraitType traitType in admiralTraits)
			{
				string admiralTraitText = OverlayMission.GetAdmiralTraitText(traitType);
				if (traitType != admiralTraits.Last<AdmiralInfo.TraitType>())
					admiralTraitText += ", ";
				_app.UI.AddItem(_app.UI.Path(ID, "admiralTraits"), "", userItemId, "");
				string itemGlobalId = _app.UI.GetItemGlobalID(_app.UI.Path(ID, "admiralTraits"), "", userItemId, "");
				++userItemId;
				_app.UI.SetPropertyString(itemGlobalId, "text", admiralTraitText);
				if (AdmiralInfo.IsGoodTrait(traitType))
					_app.UI.SetPropertyColorNormalized(itemGlobalId, "color", new Vector3(0.0f, 1f, 0.0f));
				else
					_app.UI.SetPropertyColorNormalized(itemGlobalId, "color", new Vector3(1f, 0.0f, 0.0f));
				_app.UI.SetTooltip(itemGlobalId, AdmiralInfo.GetTraitDescription(traitType, _app.GameDatabase.GetLevelForAdmiralTrait(admiralInfo.ID, traitType)));
			}
		}

		protected void RebuildShipLists(int fleetId)
		{
			FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(fleetId);
			if (fleetInfo == null)
				return;
			IEnumerable<ShipInfo> shipInfoByFleetId = this.App.GameDatabase.GetShipInfoByFleetID(fleetInfo.ID, false);
			List<int> missionCapableShips = Kerberos.Sots.StarFleet.StarFleet.GetMissionCapableShips(this.App.Game, fleetInfo.ID, this._missionType);
			this._vitals.Clear();
			this._escorts.Clear();
			List<int> intList = (List<int>)null;
			if (missionCapableShips.Count == 0)
				intList = DesignLab.GetMissionRequiredDesigns(this.App.Game, this._missionType, this.App.LocalPlayer.ID);
			if (intList != null)
			{
				foreach (int designID in intList)
				{
					ShipItem shipItem = new ShipItem(new ShipInfo()
					{
						DesignID = designID,
						DesignInfo = this.App.GameDatabase.GetDesignInfo(designID)
					});
					++shipItem.NumAdded;
					this._vitals.Add(shipItem);
				}
			}
			foreach (ShipInfo shipInfo in shipInfoByFleetId)
			{
				if (missionCapableShips.Contains(shipInfo.ID))
					this._vitals.Add(new ShipItem(shipInfo));
				else
					this._escorts.Add(new ShipItem(shipInfo));
			}
		}

		protected void SyncShipListEscorts()
		{
			this.App.UI.ClearItems("invalid");
			foreach (ShipItem escort in this._escorts)
				this.App.UI.AddItem("invalid", "name", escort.ShipID, escort.Name);
		}

		protected void AddCommonMissionTimes(MissionEstimate estimate)
		{
			OverlayMission.AddCommonMissionTimes(this.App, estimate);
		}

		public static void AddCommonMissionTimes(App game, MissionEstimate estimate)
		{
			OverlayMission.AddMissionTime(game, 0, App.Localize("@MISSIONWIDGET_TOTAL_MISSION_TIME"), estimate.TotalTurns, string.Empty);
			if (estimate.TurnsForConstruction > 0)
				OverlayMission.AddMissionTime(game, 3, App.Localize("@MISSIONWIDGET_BUILD_TIME"), estimate.TurnsForConstruction, string.Empty);
			if (estimate.TotalTravelTurns <= 0)
				return;
			string hint = App.Localize("@SURVEYMISSION_HINT");
			OverlayMission.AddMissionTime(game, 1, App.Localize("@MISSIONWIDGET_TRAVEL_TIME"), estimate.TotalTravelTurns, hint);
		}

		public static void AddMissionEstimate(
		  App game,
		  int itemId,
		  string label,
		  string value,
		  string units,
		  string incButtonId,
		  string decButtonId,
		  string hint)
		{
			game.UI.AddItem("gameMissionTimes", string.Empty, itemId, string.Empty);
			game.UI.SetItemPropertyString("gameMissionTimes", string.Empty, itemId, nameof(label), "text", label);
			game.UI.SetItemPropertyString("gameMissionTimes", string.Empty, itemId, nameof(value), "text", value);
			game.UI.SetItemPropertyString("gameMissionTimes", string.Empty, itemId, nameof(units), "text", units);
			if (!string.IsNullOrEmpty(hint))
			{
				game.UI.SetItemPropertyString("gameMissionTimes", string.Empty, itemId, nameof(hint), "text", hint);
				game.UI.SetVisible(game.UI.Path(game.UI.GetItemGlobalID("gameMissionTimes", string.Empty, itemId, string.Empty), nameof(hint)), true);
			}
			else
				game.UI.SetVisible(game.UI.Path(game.UI.GetItemGlobalID("gameMissionTimes", string.Empty, itemId, string.Empty), nameof(hint)), false);
		}

		protected void AddMissionTime(int itemId, string label, int numTurns, string hint)
		{
			OverlayMission.AddMissionTime(this.App, itemId, label, numTurns, hint);
		}

		public static void AddMissionTime(
		  App game,
		  int itemId,
		  string label,
		  int numTurns,
		  string hint)
		{
			string str = string.Format("{0}", (object)numTurns);
			string units = numTurns == 1 ? App.Localize("@TURN") : App.Localize("@TURNS");
			OverlayMission.AddMissionEstimate(game, itemId, label, str, units, string.Empty, string.Empty, hint);
		}

		protected void AddMissionNote(string note)
		{
			this.App.UI.AddItem("gameMissionNotes", string.Empty, this._nextMissionNoteItemId++, note);
		}

		protected void AddMissionCost(MissionEstimate estimate)
		{
			OverlayMission.AddMissionCost(this.App, estimate);
		}

		public static void AddMissionCost(App game, MissionEstimate estimate)
		{
			string str = string.Format("{0}", (object)estimate.ConstructionCost.ToString("N0"));
			OverlayMission.AddMissionEstimate(game, 4, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			OverlayMission.AddMissionEstimate(game, 5, App.Localize("@MISSIONWIDGET_COST"), str, App.Localize("@MISSIONWIDGET_CREDITS"), string.Empty, string.Empty, string.Empty);
		}

		protected List<int> GetDesignsToBuild()
		{
			List<int> designs = new List<int>();
			this._vitals.ForEach((Action<ShipItem>)(x => designs.AddRange(Enumerable.Repeat<int>(x.DesignID, x.NumAdded))));
			this._escorts.ForEach((Action<ShipItem>)(x => designs.AddRange(Enumerable.Repeat<int>(x.DesignID, x.NumAdded))));
			this._additionalShips.ForEach((Action<ShipItem>)(x => designs.AddRange(Enumerable.Repeat<int>(x.DesignID, x.NumAdded))));
			return designs;
		}

		protected virtual void RefreshMissionDetails(StationType type = StationType.INVALID_TYPE, int stationLevel = 1)
		{
			if (this.TargetSystem == 0)
				return;
			string missionDetailsTitle = this.GetMissionDetailsTitle();
			if (this.SelectedFleet != 0)
			{
				if (this.MissionType != MissionType.REACTION)
					OverlayMission.RefreshFleetAdmiralDetails(this.App, this.ID, this.SelectedFleet, "admiralDetails");
				this.App.UI.ClearItems("gameMissionTimes");
				this.App.UI.ClearItems("gameMissionNotes");
				this._missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this.App.Game, this._missionType, type, this.SelectedFleet, this.TargetSystem, this.SelectedPlanet, this.GetDesignsToBuild(), stationLevel, this.ReBaseToggle, new float?(), new float?());
				missionDetailsTitle += string.Format(App.Localize("@UI_MISSION_ETA_TURNS"), (object)this._missionEstimate.TotalTurns);
				this.OnRefreshMissionDetails(this._missionEstimate);
				this.App.UI.AutoSizeContents("gameMissionDetails");
			}
			this.App.UI.SetText(this.App.UI.Path(this.ID, "gameMissionTitle"), missionDetailsTitle);
		}

		private void RefreshConstructionCount()
		{
			IEnumerable<FleetInfo> fleetInfos = this.CollectAvailableFleets(this._missionType, this.TargetSystem);
			this.App.GameDatabase.GetStarSystemInfo(this.TargetSystem);
			foreach (FleetInfo fleetInfo in fleetInfos)
			{
				string str = string.Format("FleetItem{0}", (object)fleetInfo.ID);
				IEnumerable<ShipInfo> shipInfoByFleetId = this.App.GameDatabase.GetShipInfoByFleetID(fleetInfo.ID, false);
				string listId = this.App.UI.Path("invalid", str, "invalid");
				foreach (ShipItem vital in this._vitals)
				{
					ShipItem item = vital;
					int num = shipInfoByFleetId.Count<ShipInfo>((Func<ShipInfo, bool>)(x => x.DesignID == item.DesignID));
					this.App.UI.SetItemPropertyString(listId, "count", item.ShipID, string.Empty, "text", string.Concat((object)(num + item.NumAdded)));
				}
			}
		}

		public OverlayMission(
		  App game,
		  StarMapState state,
		  StarMap starmap,
		  MissionType missionType,
		  string template = "OverlaySurveyMission")
		  : base(game, template)
		{
			this._starMapState = state;
			this._starMap = starmap;
			this._starMapState.OnObjectSelectionChanged += new StarMapState.ObjectSelectionChangedDelegate(this.OnStarmapSelectedObjectChanged);
			this.App = game;
			this._missionType = missionType;
		}

		public void Show(int system)
		{
			if (this.ID != "")
				this.App.UI.DestroyPanel(this.ID);
			this.SetID(Guid.NewGuid().ToString());
			this.App.UI.CreateOverlay((Dialog)this, null);
			this._fleetCentric = false;
			this.TargetSystem = system;
			this.App.UI.ShowOverlay((Dialog)this);
		}

		public void ShowFleetCentric(int fleetid)
		{
			if (this.ID != "")
				this.App.UI.DestroyPanel(this.ID);
			this.SetID(Guid.NewGuid().ToString());
			this.App.UI.CreateOverlay((Dialog)this, null);
			this._fleetCentric = true;
			this.TargetSystem = 0;
			this.SelectedFleet = fleetid;
			this.App.UI.ShowOverlay((Dialog)this);
		}

		public bool GetShown()
		{
			return this._shown;
		}

		public void Hide()
		{
			if (!this._canExit)
				return;
			this.App.UI.Send((object)"SetWidthProp", (object)"OH_StarMap", (object)"parent:width");
			this.App.GetGameState<StarMapState>().ShowInterface = true;
			this.App.GetGameState<StarMapState>().RightClickEnabled = true;
			this.App.GetGameState<StarMapState>().RefreshStarmap(StarMapState.StarMapRefreshType.REFRESH_NORMAL);
			this.OnExit();
			this._shown = false;
			this.App.UI.CloseDialog((Dialog)this, false);
		}

		public override void Initialize()
		{
			this._fleetWidget = new FleetWidget(this.App, this.App.UI.Path(this.ID, "gameFleetList"));
			this._systemWidget = new SystemWidget(this.App, this.App.UI.Path(this.ID, "starDetailsCard"));
		}

		protected virtual void OnEnter()
		{
			this._canConfirm = false;
			this._useDirectRoute = false;
			this._canExit = true;
			this.ReBaseToggle = false;
			if (!this._fleetCentric)
				this.SelectedFleet = 0;
			this.PathDrawEnabled = true;
			if (this.MissionType == MissionType.SURVEY || this.MissionType == MissionType.INTERCEPT || (this.MissionType == MissionType.PIRACY || this.MissionType == MissionType.DEPLOY_NPG) || this._fleetCentric)
			{
				this.App.UI.SetVisible(this.UI.Path(this.ID, "gameConfirmAndContinueMissionButton"), false);
				this.App.UI.SetVisible(this.UI.Path(this.ID, "2B_Background"), true);
				this.App.UI.SetVisible(this.UI.Path(this.ID, "3B_Background"), false);
			}
			else
			{
				this.App.UI.SetVisible(this.UI.Path(this.ID, "gameConfirmAndContinueMissionButton"), true);
				this.App.UI.SetVisible(this.UI.Path(this.ID, "2B_Background"), false);
				this.App.UI.SetVisible(this.UI.Path(this.ID, "3B_Background"), true);
			}
			this.App.UI.SetVisible(this.UI.Path(this.ID, "LYSlider"), (this.MissionType == MissionType.DEPLOY_NPG ? 1 : 0) != 0);
			if (this.MissionType == MissionType.INTERCEPT)
			{
				int target = ((OverlayInterceptMission)this).TargetFleet;
				if (this._starMap.Fleets.Reverse.ContainsKey(target))
					this._starMap.Select((IGameObject)this._starMap.Fleets.Reverse.FirstOrDefault<KeyValuePair<int, StarMapFleet>>((Func<KeyValuePair<int, StarMapFleet>, bool>)(x => x.Key == target)).Value);
			}
			else
			{
				if (this._fleetCentric && this.TargetSystem == 0)
					this.TargetSystem = this._app.GameDatabase.GetFleetInfo(this.SelectedFleet).SystemID;
				if (this._starMap.Systems.Reverse.ContainsKey(this.TargetSystem))
					this._starMap.Select((IGameObject)this._starMap.Systems.Reverse.FirstOrDefault<KeyValuePair<int, StarMapSystem>>((Func<KeyValuePair<int, StarMapSystem>, bool>)(x => x.Key == this.TargetSystem)).Value);
			}
			this._starMap.SelectEnabled = this._fleetCentric;
			this._starMap.FocusEnabled = this._fleetCentric;
			this.App.UI.GameEvent += new UIEventGameEvent(this.UICommChannel_GameEvent);
			this._fleetWidget.MissionMode = this._missionType;
			this._fleetWidget.EnemySelectionEnabled = false;
			List<FleetInfo> list = Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(this.App.Game, this.App.LocalPlayer.ID, this.TargetSystem, this._missionType, true).ToList<FleetInfo>();
			this._fleetWidget.SetSyncedFleets(list);
			List<int> intList1 = new List<int>();
			List<int> intList2 = new List<int>();
			intList1.Add(this.TargetSystem);
			foreach (FleetInfo fleetInfo in list)
			{
				if (!intList1.Contains(fleetInfo.SystemID))
					intList1.Add(fleetInfo.SystemID);
			}
			if (this._fleetCentric)
			{
				foreach (int num in Kerberos.Sots.StarFleet.StarFleet.CollectAvailableSystemsForFleetMission(this.App.Game.GameDatabase, this.App.Game, this.SelectedFleet, this._fleetWidget.MissionMode, false))
				{
					if (!intList1.Contains(num))
						intList1.Add(num);
					if (!intList2.Contains(num))
						intList2.Add(num);
				}
				if (intList2.Any<int>())
				{
					this.TargetSystem = intList2.First<int>();
					this.FocusOnStarSystem(this.TargetSystem);
				}
				if (this._systemWidget != null)
					this._systemWidget.Sync(this.TargetSystem);
				this._fleetWidget.SetSyncedFleets(this.SelectedFleet);
				this._fleetWidget.SelectedFleet = this.SelectedFleet;
			}
			foreach (int key in this._starMap.Systems.Reverse.Keys)
			{
				this._starMap.Systems.Reverse[key].SetIsEnabled(intList1.Contains(key));
				this._starMap.Systems.Reverse[key].SetIsSelectable(intList2.Contains(key));
			}
			foreach (int key in this._starMap.Fleets.Reverse.Keys)
				this._starMap.Fleets.Reverse[key].SetIsSelectable(false);
			this._app.UI.SetVisible(this.UI.Path(this.ID, "fleetMission"), (this._fleetCentric ? 1 : 0) != 0);
			this._app.UI.SetVisible(this.UI.Path(this.ID, "systemMission"), (!this._fleetCentric ? 1 : 0) != 0);
			this._app.UI.SetVisible(this.UI.Path(this.ID, "gameFleetList"), (!this._fleetCentric ? 1 : 0) != 0);
			this.BuildSystemList(intList2);
			this.UIListSelectSystem(this.TargetSystem);
			this._lastFilter = this._starMap.ViewFilter;
			this._starMap.ViewFilter = StarMapViewFilter.VF_MISSION;
			this._starMapState.ClearStarmapFleetArrows();
			this._starMap.ClearSystemEffects();
			this.UpdateCanConfirmMission();
			this.UpdateRebaseUI();
		}

		private void BuildSystemList(List<int> selectablesystems)
		{
			foreach (int selectablesystem in selectablesystems)
			{
				StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(selectablesystem);
				this.App.UI.AddItem(this.App.UI.Path(this.ID, "gameSystemList"), "", selectablesystem, "A sys", "TinySystemCard_Button");
				string itemGlobalId = this._app.UI.GetItemGlobalID(this.App.UI.Path(this.ID, "gameSystemList"), "", starSystemInfo.ID, "");
				this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId, this.SystemButton), "id", this.SystemButton + "|" + (object)starSystemInfo.ID);
				this._app.UI.GetGlobalID(this._app.UI.Path(itemGlobalId, "unselected"));
				this._app.UI.GetGlobalID(this._app.UI.Path(itemGlobalId, "selected"));
				this._app.UI.GetGlobalID(this._app.UI.Path(itemGlobalId, "contentselected"));
				this._app.UI.SetText(this._app.UI.Path(itemGlobalId, "itemName"), starSystemInfo.Name);
				Vector4 vector4 = StarHelper.CalcModelColor(new StellarClass(starSystemInfo.StellarClass));
				this._app.UI.SetPropertyColor(this._app.UI.Path(itemGlobalId, "colorGradient"), "color", new Vector3(vector4.X, vector4.Y, vector4.Z) * (float)byte.MaxValue);
			}
			this._ActiveSystems = selectablesystems;
		}

		private void UIListSelectSystem(int selectedsystem)
		{
			foreach (int activeSystem in this._ActiveSystems)
			{
				string itemGlobalId = this._app.UI.GetItemGlobalID(this.App.UI.Path(this.ID, "gameSystemList"), "", this._app.GameDatabase.GetStarSystemInfo(activeSystem).ID, "");
				string globalId1 = this._app.UI.GetGlobalID(this._app.UI.Path(itemGlobalId, "unselected"));
				string globalId2 = this._app.UI.GetGlobalID(this._app.UI.Path(itemGlobalId, "selected"));
				this._app.UI.GetGlobalID(this._app.UI.Path(itemGlobalId, "contentselected"));
				this._app.UI.SetVisible(globalId1, activeSystem != selectedsystem);
				this._app.UI.SetVisible(globalId2, activeSystem == selectedsystem);
			}
		}

		public void FocusOnStarSystem(int TargetSystem)
		{
			if (!this._starMap.Systems.Reverse.ContainsKey(TargetSystem))
				return;
			this._starMap.Select((IGameObject)this._starMap.Systems.Reverse.FirstOrDefault<KeyValuePair<int, StarMapSystem>>((Func<KeyValuePair<int, StarMapSystem>, bool>)(x => x.Key == TargetSystem)).Value);
		}

		protected virtual void SystemCentricSystemUIRefresh()
		{
			foreach (int key in this._starMap.Systems.Reverse.Keys)
			{
				this._starMap.Systems.Reverse[key].SetIsEnabled(false);
				this._starMap.Systems.Reverse[key].SetIsSelectable(false);
			}
			List<FleetInfo> list = Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(this.App.Game, this.App.LocalPlayer.ID, this.TargetSystem, this._missionType, true).ToList<FleetInfo>();
			List<int> intList = new List<int>();
			intList.Add(this.TargetSystem);
			foreach (FleetInfo fleetInfo in list)
			{
				if (!intList.Contains(fleetInfo.SystemID))
					intList.Add(fleetInfo.SystemID);
			}
			foreach (int key in this._starMap.Systems.Reverse.Keys)
				this._starMap.Systems.Reverse[key].SetIsEnabled(intList.Contains(key));
		}

		protected virtual void OnExit()
		{
			foreach (int key in this._starMap.Systems.Reverse.Keys)
			{
				this._starMap.Systems.Reverse[key].SetIsEnabled(true);
				this._starMap.Systems.Reverse[key].SetIsSelectable(true);
			}
			foreach (int key in this._starMap.Fleets.Reverse.Keys)
				this._starMap.Fleets.Reverse[key].SetIsSelectable(true);
			this._starMap.ViewFilter = this._lastFilter;
			this._starMapState._lastFilterSelection = this._lastFilter;
			this._selectedObject = (IGameObject)null;
			this.SelectedFleet = 0;
			this.SelectedPlanet = 0;
			this.TargetSystem = 0;
			this._fleetWidget.SelectedFleet = 0;
			this._starMap.SelectEnabled = true;
			this._starMap.FocusEnabled = true;
			this._starMapState.RefreshMission();
		}

		protected virtual StationType GetSelectedStationtype()
		{
			return StationType.INVALID_TYPE;
		}

		protected bool CheckZuulHasBore()
		{
			FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(this._selectedFleet);
			if (!GameSession.FleetHasBore(this.App.GameDatabase, this._selectedFleet))
				return Kerberos.Sots.StarFleet.StarFleet.GetNodeTravelPath(this.App.GameDatabase, fleetInfo.SystemID, this.TargetSystem, fleetInfo.PlayerID, false, true, false).Count<int>() == 0;
			return false;
		}

		protected bool CheckZuulDirectRoute()
		{
			FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(this._selectedFleet);
			int tripTime1 = 0;
			int tripTime2 = 0;
			float tripDistance1 = 0.0f;
			float tripDistance2 = 0.0f;
			Kerberos.Sots.StarFleet.StarFleet.GetBestTravelPath(this.App.Game, this._selectedFleet, fleetInfo.SystemID, this.TargetSystem, out tripTime1, out tripDistance1, true, new float?(), new float?());
			Kerberos.Sots.StarFleet.StarFleet.GetBestTravelPath(this.App.Game, this._selectedFleet, fleetInfo.SystemID, this.TargetSystem, out tripTime2, out tripDistance2, false, new float?(), new float?());
			return (double)tripDistance1 != (double)tripDistance2;
		}

		protected bool CheckZuulNodeHealth()
		{
			StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(this.App.GameDatabase.GetFleetInfo(this._selectedFleet).SystemID);
			if (this._useDirectRoute)
				return this.App.GameDatabase.GetExploredNodeLinesFromSystem(this.App.LocalPlayer.ID, starSystemInfo.ID).Where<NodeLineInfo>((Func<NodeLineInfo, bool>)(x => x.Health > -1)).Count<NodeLineInfo>() >= GameSession.GetPlayerSystemBoreLineLimit(this.App.GameDatabase, this.App.LocalPlayer.ID);
			return false;
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "gameCancelMissionButton")
				{
					this._canExit = true;
					this.Hide();
				}
				else if (panelName == "gameConfirmMissionButton")
				{
					this._canExit = true;
					this.TryCommitMission();
				}
				else if (panelName == "gameConfirmAndContinueMissionButton")
				{
					this._canExit = false;
					this.TryCommitMission();
				}
				else if (panelName == "gameEnterRebase")
				{
					this.ReBaseToggle = !this.ReBaseToggle;
					this.UpdateCanConfirmMission();
				}
				else if (panelName.StartsWith(this.SystemButton))
				{
					int sysid = int.Parse(panelName.Split('|')[1]);
					if (!this._starMap.Systems.Reverse.ContainsKey(sysid))
						return;
					StarMapSystem starMapSystem = this._starMap.Systems.Reverse.FirstOrDefault<KeyValuePair<int, StarMapSystem>>((Func<KeyValuePair<int, StarMapSystem>, bool>)(x => x.Key == sysid)).Value;
					this._starMap.Select((IGameObject)starMapSystem);
					this._starMap.SetFocus((IGameObject)starMapSystem);
				}
				else if (panelName == "planetsTab")
				{
					this.App.UI.SetVisible(this.App.UI.Path(this.ID, "gameSystemList"), true);
					this.App.UI.SetVisible(this.App.UI.Path(this.ID, "gameFleetList"), false);
					this.App.UI.SetChecked(this.App.UI.Path(this.ID, "fleetMission", "planetsTab"), true);
					this.App.UI.SetChecked(this.App.UI.Path(this.ID, "fleetMission", "fleetsTab"), false);
				}
				else
				{
					if (!(panelName == "fleetsTab"))
						return;
					this.App.UI.SetVisible(this.App.UI.Path(this.ID, "gameSystemList"), false);
					this.App.UI.SetVisible(this.App.UI.Path(this.ID, "gameFleetList"), true);
					this.App.UI.SetChecked(this.App.UI.Path(this.ID, "fleetMission", "planetsTab"), false);
					this.App.UI.SetChecked(this.App.UI.Path(this.ID, "fleetMission", "fleetsTab"), true);
				}
			}
			else
			{
				if (msgType == "list_sel_changed")
					return;
				if (msgType == "dialog_closed")
				{
					if (panelName == this._loaDirectRoute)
					{
						this._useDirectRoute = msgParams[0] == "True";
						this.LoaTryCommitMission();
					}
					else if (panelName == this._zuulBoreMissing)
					{
						switch ((GenericYesNoDialog.YesNoDialogResult)Enum.Parse(typeof(GenericYesNoDialog.YesNoDialogResult), msgParams[0]))
						{
							case GenericYesNoDialog.YesNoDialogResult.Yes:
								this.CommitMission();
								break;
							case GenericYesNoDialog.YesNoDialogResult.No:
								DesignInfo designInfo1 = (DesignInfo)null;
								foreach (DesignInfo designInfo2 in this.App.GameDatabase.GetDesignInfosForPlayer(this.App.LocalPlayer.ID))
								{
									foreach (DesignSectionInfo designSection in designInfo2.DesignSections)
									{
										if (this.App.GameDatabase.AssetDatabase.GetShipSectionAsset(designSection.FilePath).IsBoreShip)
										{
											designInfo1 = designInfo2;
											break;
										}
									}
									if (designInfo1 != null)
										break;
								}
								if (designInfo1 != null)
								{
									ShipItem shipItem = new ShipItem(new ShipInfo()
									{
										DesignID = designInfo1.ID,
										DesignInfo = designInfo1
									});
									++shipItem.NumAdded;
									this._additionalShips.Add(shipItem);
								}
								StarSystemInfo starSystemInfo1 = this._app.GameDatabase.GetStarSystemInfo(this.App.GameDatabase.GetFleetInfo(this._selectedFleet).SystemID);
								if (this.CheckZuulDirectRoute())
								{
									this._zuulBoreRoute = this.App.UI.CreateDialog((Dialog)new GenericQuestionDialog(this.App, App.Localize("@UI_MISSION_CONFIRMATION"), App.Localize("@UI_CONFIRM_BORE_LINE_MISSION"), "dialogBoreRouteQuestion"), null);
									break;
								}
								if (this.CheckZuulNodeHealth())
								{
									this._zuulConfirm = this.App.UI.CreateDialog((Dialog)new GenericQuestionDialog(this.App, App.Localize("@UI_MISSION_CONFIRMATION"), string.Format(App.Localize("@UI_CONFIRM_BORE_LINE_MISSION_MAX"), (object)starSystemInfo1.Name), "dialogGenericQuestion"), null);
									break;
								}
								this.CommitMission();
								break;
						}
					}
					else if (panelName == this._zuulBoreRoute)
					{
						this._useDirectRoute = msgParams[0] == "True";
						StarSystemInfo starSystemInfo2 = this._app.GameDatabase.GetStarSystemInfo(this.App.GameDatabase.GetFleetInfo(this._selectedFleet).SystemID);
						if (this.CheckZuulNodeHealth())
							this._zuulConfirm = this.App.UI.CreateDialog((Dialog)new GenericQuestionDialog(this.App, App.Localize("@UI_MISSION_CONFIRMATION"), string.Format(App.Localize("@UI_CONFIRM_BORE_LINE_MISSION_MAX"), (object)starSystemInfo2.Name), "dialogGenericQuestion"), null);
						else
							this.CommitMission();
					}
					else if (panelName == this._loaShearFleetConfirm)
					{
						if (msgParams[0] == "True")
						{
							this._app.Game.CheckLoaFleetGateCompliancy(this._app.GameDatabase.GetFleetInfo(this._selectedFleet));
							this._fleetWidget.Refresh();
						}
						this._loaFleetCompoSel = this.App.UI.CreateDialog((Dialog)new DialogLoaFleetSelector(this.App, this._missionType, this._app.GameDatabase.GetFleetInfo(this._selectedFleet), false), null);
					}
					else if (panelName == this._loaFleetCompoSel)
					{
						if (!(msgParams[0] != ""))
							return;
						this._app.GameDatabase.UpdateFleetCompositionID(this._selectedFleet, new int?(int.Parse(msgParams[0])));
						Kerberos.Sots.StarFleet.StarFleet.BuildFleetFromComposition(this._app.Game, this._selectedFleet, MissionType.NO_MISSION);
						this.CommitMission();
					}
					else
					{
						if (!(panelName == this._zuulConfirm) || !(msgParams[0] == "True") || !this.CanConfirmMission())
							return;
						this.CommitMission();
					}
				}
				else if (msgType == "checkbox_clicked")
				{
					bool flag = int.Parse(msgParams[0]) > 0;
					if (!(panelName == "gameRebaseToggle"))
						return;
					this.ReBaseToggle = flag;
					this.RefreshMissionDetails(StationType.INVALID_TYPE, 1);
				}
				else
				{
					if (!(msgType == "dialog_opened") || !(panelName == this.ID))
						return;
					this.App.UI.SetVisible(this.ID, true);
					this.App.UI.UnlockUI();
					this.App.GetGameState<StarMapState>().ShowInterface = false;
					this.App.GetGameState<StarMapState>().RightClickEnabled = false;
					this.App.UI.Send((object)"SetWidthProp", (object)"OH_StarMap", (object)"parent:width+280");
					this.TargetSystem = this.TargetSystem;
					StarSystemUI.SyncSystemDetailsWidget(this.App, this.App.UI.Path(this.ID, "systemDetailsWidget"), this.TargetSystem, false, true);
					if (this._systemWidget != null)
						this._systemWidget.Sync(this.TargetSystem);
					this.App.UI.SetVisible(this.App.UI.Path(this.ID, "admiralDetails", "traitsLabel"), false);
					this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, "admiralImage"), "sprite", this.GetDefaultFactionAdmiralSprite(this._app.LocalPlayer.Faction));
					this.OnEnter();
					this.RefreshMissionDetails(StationType.INVALID_TYPE, 1);
					this._shown = true;
				}
			}
		}

		private string GetDefaultFactionAdmiralSprite(Faction faction)
		{
			string empty = string.Empty;
			string str;
			switch (faction.Name)
			{
				case "zuul":
					str = "hordezuul";
					break;
				case "liir_zuul":
					str = "liir";
					break;
				default:
					str = faction.Name;
					break;
			}
			return string.Format("admiral_{0}", (object)str);
		}

		private void CommitMission()
		{
			MissionInfo missionByFleetId = this.App.GameDatabase.GetMissionByFleetID(this._selectedFleet);
			if (missionByFleetId != null)
			{
				foreach (WaypointInfo waypointInfo in this.App.GameDatabase.GetWaypointsByMissionID(missionByFleetId.ID))
					this.App.GameDatabase.RemoveWaypoint(waypointInfo.ID);
				this.App.GameDatabase.RemoveMission(missionByFleetId.ID);
			}
			this.OnCommitMission();
			this.RefreshUI(this.TargetSystem);
			this.Hide();
		}

		public override string[] CloseDialog()
		{
			this.App.UI.GameEvent -= new UIEventGameEvent(this.UICommChannel_GameEvent);
			this._fleetWidget.UnlinkWidgets();
			this._fleetWidget.Dispose();
			this._escorts.Clear();
			this._vitals.Clear();
			this._systemWidget.Terminate();
			return (string[])null;
		}

		protected override void OnUpdate()
		{
			if (!this._shown)
				return;
			this.SelectedFleet = this._fleetWidget.SelectedFleet;
			this._systemWidget.Update();
		}

		public void OnStarmapSelectedObjectChanged(App game, int objectid)
		{
			if (!this._shown || this.TargetSystem == objectid || (objectid == 0 || this.MissionType == MissionType.REACTION))
				return;
			this.RefreshUI(objectid);
		}

		public void RefreshUI(int targetsystem)
		{
			this.TargetSystem = targetsystem;
			if (this.ReBaseToggle)
				return;
			if (this._fleetCentric)
			{
				this._fleetWidget.SetSyncedFleets(this.SelectedFleet);
				this.UIListSelectSystem(this.TargetSystem);
			}
			else
			{
				List<FleetInfo> list1 = Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(this.App.Game, this.App.LocalPlayer.ID, this.TargetSystem, this._missionType, true).ToList<FleetInfo>();
				List<int> list2 = list1.Select<FleetInfo, int>((Func<FleetInfo, int>)(x => x.ID)).ToList<int>();
				if (list2.Except<int>((IEnumerable<int>)this._fleetWidget.SyncedFleets).Count<int>() != 0 || this._fleetWidget.SyncedFleets.Except<int>((IEnumerable<int>)list2).Count<int>() != 0)
					this._fleetWidget.SetSyncedFleets(list1);
			}
			if (!this._fleetCentric)
				this.SelectedFleet = 0;
			this.SelectedPlanet = 0;
			this._fleetWidget.SelectedFleet = this.SelectedFleet;
			if (this._systemWidget != null)
				this._systemWidget.Sync(this.TargetSystem);
			this.RefreshMissionDetails(StationType.INVALID_TYPE, 1);
			this.UpdateCanConfirmMission();
		}

		public void UpdateCanRebaseToTarget()
		{
			if (this.MissionType == MissionType.GATE || this.MissionType == MissionType.INTERCEPT || (this.MissionType == MissionType.INTERDICTION || this.MissionType == MissionType.INVASION) || (this.MissionType == MissionType.RELOCATION || this.MissionType == MissionType.STRIKE || this.MissionType == MissionType.PIRACY))
			{
				this.ReBaseToggle = false;
				this._app.UI.SetVisible(this.UI.Path(this.ID, "gameRebaseToggle"), false);
				this._app.UI.SetVisible(this.UI.Path(this.ID, "gameRebaseToggleStn"), false);
				this._app.UI.SetVisible(this.UI.Path(this.ID, "gameEnterRebase"), false);
			}
			else
			{
				this._app.UI.SetVisible(this.UI.Path(this.ID, "gameRebaseToggle"), true);
				this._app.UI.SetVisible(this.UI.Path(this.ID, "gameRebaseToggleStn"), true);
				this._app.UI.SetVisible(this.UI.Path(this.ID, "gameEnterRebase"), true);
				bool flag = false;
				if (this.SelectedFleet != 0)
					flag = this.CompileListOfRelocatableSystemsForFleet(this.SelectedFleet, false).Any<int>();
				this._app.UI.SetEnabled(this.UI.Path(this.ID, "gameEnterRebase"), (flag ? 1 : 0) != 0);
				if (flag)
					return;
				this._rebaseToggle = false;
			}
		}

		private void UICommChannel_GameEvent(string eventName, string[] eventParams)
		{
		}

		protected bool LoaTryCommitMission()
		{
			if (this.App.Game.LocalPlayer.Faction == this.App.AssetDatabase.GetFaction("loa"))
			{
				FleetInfo fleetInfo = this._app.GameDatabase.GetFleetInfo(this._selectedFleet);
				int fleetLoaCubeValue = Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(this.App.Game, this._selectedFleet);
				if (fleetInfo.SystemID != this._TargetSystem && fleetLoaCubeValue > Kerberos.Sots.StarFleet.StarFleet.GetMaxLoaFleetCubeMassForTransit(this.App.Game, this.App.LocalPlayer.ID))
					this._loaShearFleetConfirm = this.App.UI.CreateDialog((Dialog)new GenericQuestionDialog(this.App, App.Localize("@UI_LOA_CUBE_GATE_VIOLATION"), string.Format(App.Localize("@UI_LOA_CUBE_GATE_VIOLATION_DESC"), (object)fleetLoaCubeValue.ToString("N0"), (object)Kerberos.Sots.StarFleet.StarFleet.GetMaxLoaFleetCubeMassForTransit(this.App.Game, this.App.LocalPlayer.ID).ToString("N0")), "dialogShearFleetQuestion"), null);
				else
					this._loaFleetCompoSel = this.App.UI.CreateDialog((Dialog)new DialogLoaFleetSelector(this.App, this._missionType, this._app.GameDatabase.GetFleetInfo(this._selectedFleet), false), null);
				return false;
			}
			this.CommitMission();
			return true;
		}

		protected bool TryCommitMission()
		{
			this._additionalShips.Clear();
			if (this.CanConfirmMission() && this.MissionType != MissionType.REACTION)
			{
				if (this.App.Game.LocalPlayer.Faction == this.App.AssetDatabase.GetFaction("loa"))
				{
					if (!this.CheckZuulDirectRoute() || this._missionType == MissionType.DEPLOY_NPG)
						return this.LoaTryCommitMission();
					this._loaDirectRoute = this.App.UI.CreateDialog((Dialog)new GenericQuestionDialog(this.App, App.Localize("@UI_MISSION_CONFIRMATION"), App.Localize("@UI_CONFIRM_DIRECT_PATH"), "dialogDirectRouteQuestion"), null);
					return false;
				}
				if ((this.App.Game.LocalPlayer.Faction == this.App.AssetDatabase.GetFaction("zuul") || this.App.Game.LocalPlayer.Faction == this.App.AssetDatabase.GetFaction("loa")) && (this._selectedFleet != 0 && !Kerberos.Sots.StarFleet.StarFleet.IsSuulkaFleet(this.App.GameDatabase, this.App.GameDatabase.GetFleetInfo(this._selectedFleet))))
				{
					if (this.App.GameDatabase.GetMissionByFleetID(this._selectedFleet) == null && this.CheckZuulHasBore())
					{
						this.App.UI.UnlockUI();
						this._zuulBoreMissing = this.App.UI.CreateDialog((Dialog)new GenericYesNoDialog(this.App, App.Localize("@UI_MISSION_CONFIRMATION"), App.Localize("@UI_CONFIRM_BORE_LINE_MISSION_NOBORE"), "dialogNoBoreQuestion"), null);
						return false;
					}
					if (this.CheckZuulDirectRoute() && GameSession.FleetHasBore(this.App.GameDatabase, this._selectedFleet))
					{
						this._zuulBoreRoute = this.App.UI.CreateDialog((Dialog)new GenericQuestionDialog(this.App, App.Localize("@UI_MISSION_CONFIRMATION"), App.Localize("@UI_CONFIRM_BORE_LINE_MISSION"), "dialogBoreRouteQuestion"), null);
						return false;
					}
				}
				this.CommitMission();
				return true;
			}
			if (this.MissionType != MissionType.REACTION)
				return false;
			this.OnCommitMission();
			this.Hide();
			return true;
		}

		protected void UpdateCanConfirmMission()
		{
			this.CanConfirmMissionChanged(this.CanConfirmMission() && !this.ReBaseToggle);
			this.UpdateCanRebaseToTarget();
		}

		private void CanConfirmMissionChanged(bool newValue)
		{
			this._canConfirm = newValue;
			this.App.UI.SetEnabled("gameConfirmMissionButton", newValue);
			this.App.UI.SetEnabled("gameConfirmAndContinueMissionButton", newValue);
			this.OnCanConfirmMissionChanged(newValue);
		}

		private FleetInfo GetBestFleet(
		  MissionType missionType,
		  int systemId,
		  IEnumerable<FleetInfo> fleets)
		{
			return fleets.FirstOrDefault<FleetInfo>();
		}

		private IEnumerable<FleetInfo> CollectAvailableFleets(
		  MissionType missionType,
		  int systemId)
		{
			return Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(this.App.Game, this.App.LocalPlayer.ID, systemId, missionType, true);
		}
	}
}
