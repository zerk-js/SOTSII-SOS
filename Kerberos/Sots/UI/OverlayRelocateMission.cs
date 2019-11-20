// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.OverlayRelocateMission
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
	internal class OverlayRelocateMission : OverlayMission
	{
		private FleetWidget _RelocatefleetWidget;
		private bool CaravanMode;
		private int? CaravanFleet;
		private int? SelectedCaravanSourceSystem;

		public OverlayRelocateMission(App game, StarMapState state, StarMap starmap, string template = "OverlayRelocateMission")
		  : base(game, state, starmap, MissionType.RELOCATION, template)
		{
		}

		protected override void OnCanConfirmMissionChanged(bool newValue)
		{
		}

		protected override bool CanConfirmMission()
		{
			if (this.CaravanMode && this.CaravanFleet.HasValue)
				return this.App.GameDatabase.GetShipInfoByFleetID(this.CaravanFleet.Value, false).Any<ShipInfo>();
			FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(this.SelectedFleet);
			if (fleetInfo == null)
				return false;
			if (Kerberos.Sots.StarFleet.StarFleet.IsSuulkaFleet(this.App.GameDatabase, fleetInfo) || Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(this.App.Game, fleetInfo))
				return true;
			if (fleetInfo.AdmiralID == 0)
				return false;
			bool flag = fleetInfo.SupportingSystemID == this.TargetSystem;
			if (this.IsValidFleetID(this.SelectedFleet) && Kerberos.Sots.StarFleet.StarFleet.CanSystemSupportFleet(this.App.Game, this.TargetSystem, this.SelectedFleet))
				return !flag;
			return false;
		}

		protected override void OnEnter()
		{
			base.OnEnter();
			StarSystemUI.SyncSystemDetailsWidget(this.App, "systemDetailsWidget", this.TargetSystem, false, true);
			if (this.MissionType == MissionType.RELOCATION)
				this._RelocatefleetWidget = new FleetWidget(this.App, this.App.UI.Path(this.ID, "gameRelocateFleet"));
			if (this.MissionType == MissionType.RELOCATION)
			{
				this._RelocatefleetWidget.MissionMode = MissionType.NO_MISSION;
				this._fleetWidget.LinkWidget(this._RelocatefleetWidget);
				this._RelocatefleetWidget.LinkWidget(this._fleetWidget);
				this._RelocatefleetWidget.OnFleetsModified += new FleetWidget.FleetsModifiedDelegate(this.FleetsModified);
			}
			this.CaravanMode = false;
			this.CaravanFleet = new int?();
			this.SelectedCaravanSourceSystem = new int?();
			if (this._app.AssetDatabase.GetFaction(this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID).FactionID).Name == "loa" || this._fleetCentric)
			{
				this._app.UI.SetVisible(this.UI.Path(this.ID, "RelocateAssetsBTN"), false);
				this._app.UI.SetEnabled(this.UI.Path(this.ID, "RelocateAssetsBTN"), false);
			}
			else
			{
				this._app.UI.SetVisible(this.UI.Path(this.ID, "RelocateAssetsBTN"), true);
				if (this.CanDoCaravan())
					this._app.UI.SetEnabled(this.UI.Path(this.ID, "RelocateAssetsBTN"), true);
				else
					this._app.UI.SetEnabled(this.UI.Path(this.ID, "RelocateAssetsBTN"), false);
			}
			this._app.UI.SetVisible(this.UI.Path(this.ID, "caravanFade"), false);
		}

		private void FleetsModified(App app, int[] modifiedFleetIds)
		{
			int? nullable1 = new int?(modifiedFleetIds[0]);
			int? nullable2 = new int?(modifiedFleetIds[1]);
			FleetInfo fleetInfo1 = (FleetInfo)null;
			if (nullable2.HasValue)
				fleetInfo1 = this._app.GameDatabase.GetFleetInfo(nullable2.Value);
			if (fleetInfo1 != null && fleetInfo1.Type == FleetType.FL_CARAVAN)
				return;
			StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(fleetInfo1.SystemID);
			if (starSystemInfo != (StarSystemInfo)null)
			{
				int? caravanSourceSystem = this.SelectedCaravanSourceSystem;
				int id = starSystemInfo.ID;
				if ((caravanSourceSystem.GetValueOrDefault() != id ? 1 : (!caravanSourceSystem.HasValue ? 1 : 0)) != 0)
				{
					this.SelectedCaravanSourceSystem = new int?(starSystemInfo.ID);
					if (this.SelectedCaravanSourceSystem.HasValue)
					{
						List<int> fleets = this._app.GameDatabase.GetFleetsByPlayerAndSystem(this._app.LocalPlayer.ID, this.SelectedCaravanSourceSystem.Value, FleetType.FL_RESERVE).Select<FleetInfo, int>((Func<FleetInfo, int>)(x => x.ID)).ToList<int>();
						Dictionary<int, bool> values = new Dictionary<int, bool>();
						foreach (int key in this._fleetWidget.SyncedFleets.Where<int>((Func<int, bool>)(x => !fleets.Contains(x))))
							values.Add(key, false);
						this._fleetWidget.SetVisibleFleets(values);
						FleetInfo fleetInfo2 = this._app.GameDatabase.GetFleetInfo(this.CaravanFleet.Value);
						if (fleetInfo2 != null)
						{
							fleetInfo2.SystemID = this.SelectedCaravanSourceSystem.Value;
							fleetInfo2.SupportingSystemID = this.SelectedCaravanSourceSystem.Value;
							this._app.GameDatabase.UpdateFleetInfo(fleetInfo2);
							this._app.GameDatabase.UpdateFleetLocation(fleetInfo2.ID, this.SelectedCaravanSourceSystem.Value, new int?());
						}
					}
					else
						this._fleetWidget.SetSyncedFleets(0);
				}
			}
			this.UpdateCanConfirmMission();
		}

		protected override void OnCommitMission()
		{
			if (this.CaravanMode && this.CaravanFleet.HasValue)
				this.SelectedFleet = this.CaravanFleet.Value;
			if (this._app.LocalPlayer.Faction.Name == "loa")
			{
				Kerberos.Sots.StarFleet.StarFleet.ConvertFleetIntoLoaCubes(this._app.Game, this._selectedFleet);
				Kerberos.Sots.StarFleet.StarFleet.BuildFleetFromComposition(this._app.Game, this._selectedFleet, MissionType.NO_MISSION);
				this.RebuildShipLists(this.SelectedFleet);
			}
			Kerberos.Sots.StarFleet.StarFleet.SetRelocationMission(this.App.Game, this.SelectedFleet, this.TargetSystem, this._useDirectRoute, this.GetDesignsToBuild());
			FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(this._selectedFleet);
			AdmiralInfo admiralInfo = this.App.GameDatabase.GetAdmiralInfo(fleetInfo.AdmiralID);
			if (admiralInfo != null)
				this.App.PostRequestSpeech(string.Format("STRAT_011-01_{0}_{1}TransferMissionConfirmation", (object)this.App.GameDatabase.GetFactionName(this.App.GameDatabase.GetPlayerFactionID(this.App.LocalPlayer.ID)), (object)admiralInfo.GetAdmiralSoundCueContext(this.App.AssetDatabase)), 50, 120, 0.0f);
			if (this.CaravanMode && this.CaravanFleet.HasValue)
			{
				List<FreighterInfo> list = this.App.GameDatabase.GetFreighterInfosForSystem(fleetInfo.SystemID).Where<FreighterInfo>((Func<FreighterInfo, bool>)(x =>
			   {
				   if (x.PlayerId == this.App.LocalPlayer.ID)
					   return x.IsPlayerBuilt;
				   return false;
			   })).ToList<FreighterInfo>();
				foreach (ShipInfo shipInfo in this.App.GameDatabase.GetShipInfoByFleetID(this.CaravanFleet.Value, false).ToList<ShipInfo>())
				{
					ShipInfo ship = shipInfo;
					if (list.Any<FreighterInfo>((Func<FreighterInfo, bool>)(x => x.ShipId == ship.ID)))
						this.App.GameDatabase.RemoveFreighterInfo(list.First<FreighterInfo>((Func<FreighterInfo, bool>)(x => x.ShipId == ship.ID)).ID);
				}
			}
			this.App.GetGameState<StarMapState>().RefreshMission();
			this.CaravanFleet = new int?();
			this.SelectedCaravanSourceSystem = new int?();
			if (this._fleetWidget.SyncedFleets != null)
				this._fleetWidget.SetSyncedFleets(this._fleetWidget.SyncedFleets);
			if (this._RelocatefleetWidget.SyncedFleets != null)
				this._RelocatefleetWidget.SetSyncedFleets(this._RelocatefleetWidget.SyncedFleets);
			this.EnterAssetRelocateMode(false);
		}

		protected override string GetMissionDetailsTitle()
		{
			return string.Format(App.Localize("@UI_RELOCATE_OVERLAY_MISSION_TITLE"), (object)this.App.GameDatabase.GetStarSystemInfo(this.TargetSystem).Name.ToUpperInvariant());
		}

		protected override void OnRefreshMissionDetails(MissionEstimate estimate)
		{
			string empty = string.Empty;
			string hint = this.CanConfirm ? App.Localize("@RELOCATIONMISSION_HINT") : App.Localize("@UI_RELOCATION_INSUFFICIENT_SUPPORT");
			this.AddMissionTime(2, App.Localize("@MISSIONWIDGET_RELOCATION_TIME"), estimate.TurnsToTarget, hint);
		}

		protected override IEnumerable<int> GetMissionTargetPlanets()
		{
			return Enumerable.Empty<int>();
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "RelocateAssetsBTN")
					this.EnterAssetRelocateMode(!this.CaravanMode);
				this.UpdateCanConfirmMission();
			}
			base.OnPanelMessage(panelName, msgType, msgParams);
		}

		private bool CanDoCaravan()
		{
			if (Kerberos.Sots.StarFleet.StarFleet.HasRelocatableDefResAssetsInRange(this._app.Game, this.TargetSystem))
				return true;
			if (this.App.GameDatabase.GetFreighterInfosBuiltByPlayer(this.App.LocalPlayer.ID).Any<FreighterInfo>((Func<FreighterInfo, bool>)(x => x.SystemId != this.TargetSystem)))
				return this.App.GameDatabase.GetStationForSystem(this.TargetSystem).Any<StationInfo>((Func<StationInfo, bool>)(x => x.DesignInfo.StationType == StationType.CIVILIAN));
			return false;
		}

		private void EnterAssetRelocateMode(bool value)
		{
			this.CaravanMode = value;
			if (this.CaravanMode)
			{
				this._fleetWidget.DisableTooltips = true;
				List<StarSystemInfo> list = this._app.GameDatabase.GetVisibleStarSystemInfos(this._app.LocalPlayer.ID).ToList<StarSystemInfo>();
				if (!list.Any<StarSystemInfo>())
					return;
				int newAdmiral = GameSession.GenerateNewAdmiral(this._app.AssetDatabase, this._app.LocalPlayer.ID, this._app.GameDatabase, new AdmiralInfo.TraitType?(), this._app.Game.NamesPool);
				int systemId = this._app.GameDatabase.GetHomeworlds().FirstOrDefault<HomeworldInfo>((Func<HomeworldInfo, bool>)(x => x.PlayerID == this._app.LocalPlayer.ID)).SystemID;
				this.CaravanFleet = new int?(this._app.GameDatabase.InsertFleet(this._app.LocalPlayer.ID, newAdmiral, systemId, systemId, App.Localize("@FLEET_CARAVAN_NAME"), FleetType.FL_CARAVAN));
				this._RelocatefleetWidget.SetSyncedFleets(this.CaravanFleet.Value);
				List<int> fleets = new List<int>();
				foreach (StarSystemInfo starSystemInfo in list)
				{
					StarSystemInfo sysinf = starSystemInfo;
					if (sysinf.ID != this.TargetSystem)
					{
						int? reserveFleetId = this._app.GameDatabase.GetReserveFleetID(this._app.LocalPlayer.ID, sysinf.ID);
						if (reserveFleetId.HasValue && this.App.GameDatabase.GetStationForSystem(this.TargetSystem).Any<StationInfo>((Func<StationInfo, bool>)(x => x.DesignInfo.StationType == StationType.CIVILIAN)))
						{
							foreach (FreighterInfo freighterInfo in this._app.GameDatabase.GetFreighterInfosBuiltByPlayer(this._app.LocalPlayer.ID).Where<FreighterInfo>((Func<FreighterInfo, bool>)(x =>
						   {
							   if (x.SystemId == sysinf.ID)
								   return x.IsPlayerBuilt;
							   return false;
						   })).ToList<FreighterInfo>())
								this._app.GameDatabase.TransferShip(freighterInfo.ShipId, reserveFleetId.Value);
						}
						int? defenseFleetId = this._app.GameDatabase.GetDefenseFleetID(sysinf.ID, this._app.LocalPlayer.ID);
						if (reserveFleetId.HasValue && this._app.GameDatabase.GetShipsByFleetID(reserveFleetId.Value).Any<int>())
							fleets.Add(reserveFleetId.Value);
						if (defenseFleetId.HasValue && this._app.GameDatabase.GetShipsByFleetID(defenseFleetId.Value).Any<int>())
							fleets.Add(defenseFleetId.Value);
					}
				}
				this._fleetWidget.MissionMode = MissionType.NO_MISSION;
				this._fleetWidget.SetSyncedFleets(fleets);
				this._app.UI.SetVisible(this.UI.Path(this.ID, "caravanFade"), true);
			}
			else
			{
				this._fleetWidget.DisableTooltips = false;
				this._app.UI.SetVisible(this.UI.Path(this.ID, "caravanFade"), false);
				this.clearCaravanFleet();
				this._fleetWidget.MissionMode = MissionType.RELOCATION;
				this.RefreshUI(this.TargetSystem);
			}
		}

		private void clearCaravanFleet()
		{
			if (this.CaravanFleet.HasValue)
			{
				FleetInfo fleetInfo1 = this._app.GameDatabase.GetFleetInfo(this.CaravanFleet.Value);
				List<ShipInfo> list1 = this._app.GameDatabase.GetShipInfoByFleetID(fleetInfo1.ID, true).ToList<ShipInfo>();
				List<FreighterInfo> list2 = this._app.GameDatabase.GetFreighterInfosBuiltByPlayer(this._app.LocalPlayer.ID).Where<FreighterInfo>((Func<FreighterInfo, bool>)(x => x.IsPlayerBuilt)).ToList<FreighterInfo>();
				foreach (ShipInfo shipInfo in list1)
				{
					ShipInfo ship = shipInfo;
					if (list2.Any<FreighterInfo>((Func<FreighterInfo, bool>)(x => x.ShipId == ship.ID)))
						this._app.GameDatabase.TransferShip(ship.ID, this._app.GameDatabase.InsertOrGetLimboFleetID(fleetInfo1.SystemID, this._app.LocalPlayer.ID));
					else if (ship.IsSDB() || ship.IsPlatform())
						this._app.GameDatabase.TransferShip(ship.ID, this._app.GameDatabase.InsertOrGetDefenseFleetInfo(fleetInfo1.SystemID, this._app.LocalPlayer.ID).ID);
					else
						this._app.GameDatabase.TransferShip(ship.ID, this._app.GameDatabase.InsertOrGetReserveFleetInfo(fleetInfo1.SystemID, this._app.LocalPlayer.ID).ID);
				}
				foreach (FreighterInfo freighterInfo in list2)
				{
					ShipInfo shipInfo = this._app.GameDatabase.GetShipInfo(freighterInfo.ShipId, false);
					FleetInfo fleetInfo2 = this._app.GameDatabase.GetFleetInfo(shipInfo.FleetID);
					if (fleetInfo2 != null && fleetInfo2.IsReserveFleet && !fleetInfo2.IsLimboFleet)
						this._app.GameDatabase.TransferShip(shipInfo.ID, this._app.GameDatabase.InsertOrGetLimboFleetID(freighterInfo.SystemId, freighterInfo.PlayerId));
				}
				this._app.GameDatabase.RemoveAdmiral(fleetInfo1.AdmiralID);
				this._app.GameDatabase.RemoveFleet(fleetInfo1.ID);
				this._RelocatefleetWidget.SetSyncedFleets(0);
				this.CaravanFleet = new int?();
			}
			else
			{
				foreach (FreighterInfo freighterInfo in this._app.GameDatabase.GetFreighterInfosBuiltByPlayer(this._app.LocalPlayer.ID).Where<FreighterInfo>((Func<FreighterInfo, bool>)(x => x.IsPlayerBuilt)).ToList<FreighterInfo>())
				{
					ShipInfo shipInfo = this._app.GameDatabase.GetShipInfo(freighterInfo.ShipId, false);
					FleetInfo fleetInfo = this._app.GameDatabase.GetFleetInfo(shipInfo.FleetID);
					if (fleetInfo != null && fleetInfo.IsReserveFleet && !fleetInfo.IsLimboFleet)
						this._app.GameDatabase.TransferShip(shipInfo.ID, this._app.GameDatabase.InsertOrGetLimboFleetID(freighterInfo.SystemId, freighterInfo.PlayerId));
				}
			}
			this.SelectedCaravanSourceSystem = new int?();
		}

		protected override void OnExit()
		{
			this.clearCaravanFleet();
			this.CaravanMode = false;
			base.OnExit();
		}

		public override string[] CloseDialog()
		{
			if (this._RelocatefleetWidget != null)
			{
				this._fleetWidget.UnlinkWidgets();
				this._RelocatefleetWidget.UnlinkWidgets();
				this._RelocatefleetWidget.SetSyncedFleets(0);
				this._RelocatefleetWidget.SelectedFleet = 0;
				this._RelocatefleetWidget.OnFleetsModified -= new FleetWidget.FleetsModifiedDelegate(this.FleetsModified);
				this._RelocatefleetWidget.Dispose();
			}
			this.App.UI.PurgeFleetWidgetCache();
			this._RelocatefleetWidget = (FleetWidget)null;
			return base.CloseDialog();
		}
	}
}
