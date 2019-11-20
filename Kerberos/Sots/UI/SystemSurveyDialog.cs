// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.SystemSurveyDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class SystemSurveyDialog : Dialog
	{
		private List<SystemWidget> _systemWidgets = new List<SystemWidget>();
		private string _colonytrapDialog = "";
		public const string OKButton = "okButton";
		public const string TRAPButton = "colonyTrapper";
		private int _systemID;
		private List<PlanetWidget> _planetWidgets;
		private App App;
		private SystemWidget _systemWidget;
		private Kerberos.Sots.GameStates.StarSystem _starsystem;
		private OrbitCameraController _camera;
		private Sky _sky;
		private GameObjectSet _crits;
		private bool _critsInitialized;
		private int _fleetID;
		private SystemSurveyDialog.PlanetFilterMode _currentFilterMode;
		private static bool FORCETRAPHACK;

		public SystemSurveyDialog(App app, int systemid, int fleetID)
		  : base(app, "dialogSurveyEvent")
		{
			this._systemID = systemid;
			this.App = app;
			this._fleetID = fleetID;
		}

		public override void Initialize()
		{
			this._systemWidget = new SystemWidget(this.App, this.App.UI.Path(this.ID, "starDetailsCard"));
			StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(this._systemID);
			this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, "system_title"), "text", App.Localize("@SURVEY_OF") + " " + string.Format(App.Localize("@SURVEY_SYSTEM_THINGY"), (object)starSystemInfo.Name).ToUpperInvariant());
			this._app.UI.SetVisible(this._app.UI.Path(this.ID, "system_map"), true);
			this._app.UI.SetVisible(this._app.UI.Path(this.ID, "gameStarSystemViewport"), false);
			this._app.UI.AddItem(this._app.UI.Path(this.ID, "gameViewportList"), "", 0, App.Localize("@SYSTEMDETAILS_SYS_MAP"));
			this._app.UI.SetSelection(this._app.UI.Path(this.ID, "gameViewportList"), 0);
			StarSystemUI.SyncSystemDetailsWidget(this._app, this._app.UI.Path(this.ID, "system_details"), this._systemID, false, true);
			StarSystemMapUI.Sync(this._app, this._systemID, this._app.UI.Path(this.ID, "system_map"), true);
			this._currentFilterMode = SystemSurveyDialog.PlanetFilterMode.AllPlanets;
			this._planetWidgets = new List<PlanetWidget>();
			this.SetSyncedSystem(starSystemInfo);
			this._systemWidget.Sync(this._systemID);
			this.UpdateCanPlaceTraps();
		}

		protected override void OnUpdate()
		{
			foreach (PlanetWidget planetWidget in this._planetWidgets)
				planetWidget.Update();
			foreach (SystemWidget systemWidget in this._systemWidgets)
				systemWidget.Update();
			this._systemWidget.Update();
		}

		protected void UpdateCanPlaceTraps()
		{
			PlayerInfo playerInfo = this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID);
			if (!SystemSurveyDialog.FORCETRAPHACK && (this._fleetID == 0 || !(this.App.AssetDatabase.GetFaction(playerInfo.FactionID).Name.ToLower() == "morrigi")))
				return;
			bool flag = false;
			foreach (PlanetWidget planetWidget in this._planetWidgets)
			{
				PlanetInfo planetInfo = this._app.GameDatabase.GetPlanetInfo(planetWidget.GetPlanetID());
				if (planetInfo != null && this.App.AssetDatabase.IsPotentialyHabitable(planetInfo.Type) && this.App.GameDatabase.GetColonyInfoForPlanet(planetInfo.ID) == null)
					flag = true;
			}
			List<ShipInfo> shipInfoList = new List<ShipInfo>();
			foreach (ShipInfo shipInfo in this.App.GameDatabase.GetShipInfoByFleetID(this._fleetID, true).ToList<ShipInfo>())
			{
				if (((IEnumerable<DesignSectionInfo>)shipInfo.DesignInfo.DesignSections).Select<DesignSectionInfo, ShipSectionAsset>((Func<DesignSectionInfo, ShipSectionAsset>)(x => x.ShipSectionAsset)).FirstOrDefault<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.IsTrapShip)) != null)
					shipInfoList.Add(shipInfo);
			}
			if (shipInfoList.Count > 0 && flag)
				this.App.UI.SetVisible(this.App.UI.Path(this.ID, "colonyTrapper"), true);
			else
				this.App.UI.SetVisible(this.App.UI.Path(this.ID, "colonyTrapper"), false);
		}

		protected void SetSyncedSystem(StarSystemInfo system)
		{
			this.App.UI.ClearItems("system_list");
			this.App.UI.ClearDisabledItems("system_list");
			foreach (SystemWidget systemWidget in this._systemWidgets)
				systemWidget.Terminate();
			foreach (PlanetWidget planetWidget in this._planetWidgets)
				planetWidget.Terminate();
			this._planetWidgets.Clear();
			List<PlanetInfo> planetInfoList = this.FilteredPlanetList(system);
			this.App.UI.AddItem("system_list", "", system.ID, "", "systemTitleCard");
			this._systemWidgets.Add(new SystemWidget(this.App, this.App.UI.GetItemGlobalID("system_list", "", system.ID, "")));
			this._systemWidgets.Last<SystemWidget>().Sync(system.ID);
			foreach (PlanetInfo planetInfo in planetInfoList)
			{
				if (this.App.AssetDatabase.IsPotentialyHabitable(planetInfo.Type))
				{
					this.App.UI.AddItem("system_list", "", planetInfo.ID + 999999, "", "planetDetailsM_Card");
					this._planetWidgets.Add(new PlanetWidget(this.App, this.App.UI.GetItemGlobalID("system_list", "", planetInfo.ID + 999999, "")));
					this._planetWidgets.Last<PlanetWidget>().Sync(planetInfo.ID, false, false);
				}
				else if (this.App.AssetDatabase.IsGasGiant(planetInfo.Type))
				{
					this.App.UI.AddItem("system_list", "", planetInfo.ID + 999999, "", "gasgiantDetailsM_Card");
					this._planetWidgets.Add(new PlanetWidget(this.App, this.App.UI.GetItemGlobalID("system_list", "", planetInfo.ID + 999999, "")));
					this._planetWidgets.Last<PlanetWidget>().Sync(planetInfo.ID, false, false);
				}
				else if (this.App.AssetDatabase.IsMoon(planetInfo.Type))
				{
					this.App.UI.AddItem("system_list", "", planetInfo.ID + 999999, "", "moonDetailsM_Card");
					this._planetWidgets.Add(new PlanetWidget(this.App, this.App.UI.GetItemGlobalID("system_list", "", planetInfo.ID + 999999, "")));
					this._planetWidgets.Last<PlanetWidget>().Sync(planetInfo.ID, false, false);
				}
			}
		}

		private List<PlanetInfo> FilteredPlanetList(StarSystemInfo system)
		{
			List<PlanetInfo> list = ((IEnumerable<PlanetInfo>)this.App.GameDatabase.GetStarSystemPlanetInfos(system.ID)).ToList<PlanetInfo>();
			List<PlanetInfo> planetInfoList = new List<PlanetInfo>();
			foreach (PlanetInfo planetInfo in list)
			{
				if (this.App.GameDatabase.IsSurveyed(this.App.LocalPlayer.ID, system.ID))
				{
					if (this._currentFilterMode == SystemSurveyDialog.PlanetFilterMode.AllPlanets)
						planetInfoList.Add(planetInfo);
					else if (this._currentFilterMode == SystemSurveyDialog.PlanetFilterMode.SurveyedPlanets)
					{
						if (this.App.GameDatabase.GetColonyIntelForPlanet(this.App.LocalPlayer.ID, planetInfo.ID) == null)
							planetInfoList.Add(planetInfo);
					}
					else if (this._currentFilterMode == SystemSurveyDialog.PlanetFilterMode.OwnedPlanets)
					{
						AIColonyIntel colonyIntelForPlanet = this.App.GameDatabase.GetColonyIntelForPlanet(this.App.LocalPlayer.ID, planetInfo.ID);
						if (colonyIntelForPlanet != null && colonyIntelForPlanet.OwningPlayerID == this.App.LocalPlayer.ID)
							planetInfoList.Add(planetInfo);
					}
					else if (this._currentFilterMode == SystemSurveyDialog.PlanetFilterMode.EnemyPlanets)
					{
						AIColonyIntel colonyIntelForPlanet = this.App.GameDatabase.GetColonyIntelForPlanet(this.App.LocalPlayer.ID, planetInfo.ID);
						if (colonyIntelForPlanet != null && colonyIntelForPlanet.OwningPlayerID != this.App.LocalPlayer.ID)
							planetInfoList.Add(planetInfo);
					}
				}
			}
			return planetInfoList;
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "okButton")
					this._app.UI.CloseDialog((Dialog)this, true);
				if (panelName == "detailbutton" || !(panelName == "placeColonyTrapsbtn"))
					return;
				this._colonytrapDialog = this._app.UI.CreateDialog((Dialog)new DialogColonyTrap(this._app, this._systemID, this._fleetID), null);
			}
			else if (msgType == "dialog_closed")
			{
				if (!(panelName == this._colonytrapDialog))
					return;
				this.UpdateCanPlaceTraps();
			}
			else if (msgType == "list_sel_changed")
			{
				if (!(panelName == "gamePlanetList"))
					return;
				int orbitalObjectID = int.Parse(msgParams[0]);
				OrbitalObjectInfo orbitalObjectInfo = this._app.GameDatabase.GetOrbitalObjectInfo(orbitalObjectID);
				bool flag = false;
				if (orbitalObjectInfo != null && orbitalObjectInfo.ParentID.HasValue)
				{
					PlanetInfo planetInfo = this._app.GameDatabase.GetPlanetInfo(orbitalObjectInfo.ParentID.Value);
					if (planetInfo != null)
					{
						this._app.UI.Send((object)"SetSel", (object)this._app.UI.Path(this.ID, "system_map"), (object)1, (object)planetInfo.ID);
						flag = true;
					}
				}
				if (flag)
					return;
				this._app.UI.Send((object)"SetSel", (object)this._app.UI.Path(this.ID, "system_map"), (object)1, (object)orbitalObjectID);
			}
			else
			{
				if (!(msgType == "mapicon_clicked"))
					return;
				this._app.UI.Send((object)"SetSel", (object)this._app.UI.Path(this.ID, "planetListWidget"), (object)int.Parse(msgParams[0]));
			}
		}

		private void SetColonyViewMode(int mode)
		{
			if (mode == 0)
			{
				this._app.UI.SetVisible(this._app.UI.Path(this.ID, "system_map"), true);
				this._app.UI.SetVisible(this._app.UI.Path(this.ID, "gameStarSystemViewport"), false);
			}
			else
			{
				this._app.UI.SetVisible(this._app.UI.Path(this.ID, "system_map"), false);
				this._app.UI.SetVisible(this._app.UI.Path(this.ID, "gameStarSystemViewport"), true);
			}
		}

		public override string[] CloseDialog()
		{
			if (this._crits != null)
				this._crits.Dispose();
			this._crits = (GameObjectSet)null;
			this._starsystem = (Kerberos.Sots.GameStates.StarSystem)null;
			this._camera = (OrbitCameraController)null;
			this._sky = (Sky)null;
			foreach (PlanetWidget planetWidget in this._planetWidgets)
				planetWidget.Terminate();
			foreach (SystemWidget systemWidget in this._systemWidgets)
				systemWidget.Terminate();
			this._systemWidget.Terminate();
			return (string[])null;
		}

		private enum PlanetFilterMode
		{
			AllPlanets,
			SurveyedPlanets,
			OwnedPlanets,
			EnemyPlanets,
		}
	}
}
