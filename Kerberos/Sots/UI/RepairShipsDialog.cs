// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.RepairShipsDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class RepairShipsDialog : Dialog
	{
		private FleetWidget _leftWidget;
		private FleetWidget _rightWidget;
		private FleetWidget _colonyWidget;
		private FleetWidget _suulkaWidget;
		private FleetWidget _suulkaDrainWidget;
		private int _systemID;
		private List<FleetInfo> _fleets;

		public RepairShipsDialog(App game, int systemID, List<FleetInfo> fleets, string template = "dialogRepairShips")
		  : base(game, template)
		{
			this._fleets = fleets;
			this._systemID = systemID;
		}

		public override void Initialize()
		{
			this._colonyWidget = new FleetWidget(this._app, this.UI.Path(this.ID, "repairWidgetColonyList"));
			List<PlanetInfo> list1 = ((IEnumerable<PlanetInfo>)this._app.GameDatabase.GetStarSystemPlanetInfos(this._systemID)).ToList<PlanetInfo>();
			List<PlanetInfo> planets1 = new List<PlanetInfo>();
			List<ColonyInfo> source = new List<ColonyInfo>();
			foreach (PlanetInfo planetInfo in list1)
			{
				ColonyInfo colonyInfoForPlanet = this._app.GameDatabase.GetColonyInfoForPlanet(planetInfo.ID);
				if (colonyInfoForPlanet != null)
				{
					source.Add(colonyInfoForPlanet);
					if (colonyInfoForPlanet.PlayerID == this._app.LocalPlayer.ID)
						planets1.Add(planetInfo);
				}
			}
			this._colonyWidget.SetSyncedPlanets(planets1);
			this._suulkaWidget = new FleetWidget(this._app, this.UI.Path(this.ID, "gameRightListS"));
			this._suulkaWidget.ShipFilter += new FleetWidget.FleetWidgetShipFilter(this.SuulkaListFilter);
			this._suulkaWidget.SuulkaMode = true;
			this._suulkaWidget.DisableTooltips = true;
			this._suulkaDrainWidget = new FleetWidget(this._app, this.UI.Path(this.ID, "repairWidgetColonyList"));
			List<PlanetInfo> planets2 = new List<PlanetInfo>();
			bool flag = this._fleets.Any<FleetInfo>((Func<FleetInfo, bool>)(x =>
		   {
			   if (x.IsNormalFleet)
				   return x.PlayerID != this._app.LocalPlayer.ID;
			   return false;
		   }));
			foreach (PlanetInfo planetInfo in list1)
			{
				PlanetInfo pi = planetInfo;
				ColonyInfo colonyInfo = source.FirstOrDefault<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.OrbitalObjectID == pi.ID));
				if ((!flag || colonyInfo == null || colonyInfo.PlayerID == this._app.LocalPlayer.ID) && (pi.Biosphere > 0 || colonyInfo != null))
					planets2.Add(pi);
			}
			this._suulkaDrainWidget.SetSyncedPlanets(planets2);
			this._leftWidget = new FleetWidget(this._app, this.UI.Path(this.ID, "gameLeftList"));
			this._rightWidget = new FleetWidget(this._app, this.UI.Path(this.ID, "gameRightList"));
			this._leftWidget.DisableTooltips = true;
			this._rightWidget.RidersEnabled = true;
			this._rightWidget.SeparateDefenseFleet = false;
			this._rightWidget.DisableTooltips = true;
			this._leftWidget.ShipFilter += new FleetWidget.FleetWidgetShipFilter(this.LeftListFilter);
			this._rightWidget.ShipFilter += new FleetWidget.FleetWidgetShipFilter(this.RightListFilter);
			this._leftWidget.ShowColonies = true;
			this._leftWidget.OnlyLocalPlayer = true;
			this._leftWidget.ListStations = true;
			this._rightWidget.ListStations = true;
			List<StationInfo> list2 = this._app.GameDatabase.GetStationForSystemAndPlayer(this._systemID, this._app.LocalPlayer.ID).ToList<StationInfo>();
			this._leftWidget.SetSyncedFleets(this._fleets);
			this._rightWidget.SetSyncedFleets(this._fleets);
			this._leftWidget.SetSyncedStations(list2);
			this._rightWidget.SetSyncedStations(list2);
			this._suulkaWidget.SetSyncedFleets(this._fleets);
			this._leftWidget.ShowEmptyFleets = false;
			this._rightWidget.ShowEmptyFleets = false;
			this._leftWidget.ShowFleetInfo = false;
			this._rightWidget.ShowFleetInfo = false;
			this._suulkaWidget.ShowEmptyFleets = false;
			this._suulkaWidget.ShowFleetInfo = false;
			this._rightWidget.RepairWidget = this._leftWidget;
			this._suulkaWidget.RepairWidget = this._suulkaDrainWidget;
			this._leftWidget.ShowRepairPoints = true;
			this._rightWidget.RepairMode = true;
			this._leftWidget.ExpandAll();
			this._rightWidget.ExpandAll();
			this._suulkaWidget.ExpandAll();
		}

		public FleetWidget.FilterShips LeftListFilter(ShipInfo ship, DesignInfo design)
		{
			FleetInfo fleetInfo = this._app.GameDatabase.GetFleetInfo(ship.FleetID);
			if ((fleetInfo != null ? fleetInfo.PlayerID : design.PlayerID) != this._app.LocalPlayer.ID)
				return FleetWidget.FilterShips.Ignore;
			int num = 0;
			foreach (DesignSectionInfo designSection in design.DesignSections)
			{
				ShipSectionAsset shipSectionAsset = this._app.AssetDatabase.GetShipSectionAsset(designSection.FilePath);
				num += shipSectionAsset.RepairPoints;
			}
			return num > 0 ? FleetWidget.FilterShips.Enable : FleetWidget.FilterShips.Ignore;
		}

		public FleetWidget.FilterShips RightListFilter(ShipInfo ship, DesignInfo design)
		{
			FleetInfo fleetInfo = this._app.GameDatabase.GetFleetInfo(ship.FleetID);
			if ((fleetInfo != null ? fleetInfo.PlayerID : design.PlayerID) != this._app.LocalPlayer.ID || Kerberos.Sots.StarFleet.StarFleet.DesignIsSuulka(this._app, design) || FleetWidget.IsWeaponBattleRider(design))
				return FleetWidget.FilterShips.Ignore;
			Kerberos.Sots.StarFleet.StarFleet.GetHealthAndHealthMax(this._app.Game, design, ship.ID);
			return FleetWidget.FilterShips.Enable;
		}

		public FleetWidget.FilterShips SuulkaListFilter(ShipInfo ship, DesignInfo design)
		{
			return Kerberos.Sots.StarFleet.StarFleet.DesignIsSuulka(this._app, design) ? FleetWidget.FilterShips.Enable : FleetWidget.FilterShips.Ignore;
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (!(msgType == "button_clicked"))
				return;
			if (panelName == "gameDoneButton")
				this._app.UI.CloseDialog((Dialog)this, true);
			else if (panelName == "gameRepairSuulkasButton")
			{
				this._app.UI.SetVisible(this._app.UI.Path(this.ID, "repairDialog"), false);
				this._app.UI.SetVisible(this._app.UI.Path(this.ID, "repairSuulkaDialog"), true);
			}
			else if (panelName == "gameBackButton")
			{
				this._app.UI.SetVisible(this._app.UI.Path(this.ID, "repairDialog"), true);
				this._app.UI.SetVisible(this._app.UI.Path(this.ID, "repairSuulkaDialog"), false);
			}
			else if (panelName == "gameUndoAllButton")
				this._rightWidget.UndoAll();
			else if (panelName == "gameRepairAllButton")
			{
				this._rightWidget.RepairAll();
			}
			else
			{
				if (!(panelName == "gameConfirmRepairsButton"))
					return;
				this._rightWidget.ConfirmRepairs();
			}
		}

		public override string[] CloseDialog()
		{
			this._leftWidget.Dispose();
			this._rightWidget.Dispose();
			this._suulkaWidget.Dispose();
			this._colonyWidget.Dispose();
			this._suulkaDrainWidget.Dispose();
			this._app.GetGameState<StarMapState>()?.RefreshSystemInterface();
			return (string[])null;
		}
	}
}
