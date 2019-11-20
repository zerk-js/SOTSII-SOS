// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.IndependentFoundDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class IndependentFoundDialog : Dialog
	{
		private List<SystemWidget> _systemWidgets = new List<SystemWidget>();
		public const string IgnoreButton = "ignoreButton";
		public const string AttackButton = "attackButton";
		public const string DiploButton = "diploButton";
		private int _systemID;
		private int _colonyID;
		private int _playerID;
		private int _planetID;
		private List<PlanetWidget> _planetWidgets;

		public IndependentFoundDialog(App game, int systemid, int colonyid, int playerid)
		  : base(game, "dialogIndependentFoundEvent")
		{
			this._systemID = systemid;
			this._colonyID = colonyid;
			this._playerID = playerid;
		}

		public override void Initialize()
		{
			StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(this._systemID);
			ColonyInfo colonyInfo = this._app.GameDatabase.GetColonyInfo(this._colonyID);
			PlanetInfo planetInfo = this._app.GameDatabase.GetPlanetInfo(colonyInfo.OrbitalObjectID);
			PlayerInfo playerInfo = this._app.GameDatabase.GetPlayerInfo(colonyInfo.PlayerID);
			this._playerID = playerInfo.ID;
			this._planetID = colonyInfo.OrbitalObjectID;
			this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, "title"), "text", App.Localize("@UI_INDEPENDENT_CIVILIZATION_FOUND_TITLE").ToUpperInvariant());
			this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, "faction_name"), "text", playerInfo.Name);
			this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, "tech_level"), "text", "");
			this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, "avatar"), "texture", playerInfo.AvatarAssetPath);
			this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, "tech_level"), "text", App.Localize("@UI_INDEPENDENT_TECH_" + playerInfo.Name.ToUpper()));
			StarSystemMapUI.Sync(this._app, this._systemID, this._app.UI.Path(this.ID, "system_map"), true);
			this._app.UI.SetEnabled(this._app.UI.Path(this.ID, "diploButton"), (Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(this._app.Game, this._app.LocalPlayer.ID, this._systemID, MissionType.CONSTRUCT_STN, true).Any<FleetInfo>() ? 1 : 0) != 0);
			this._planetWidgets = new List<PlanetWidget>();
			this.SetSyncedSystem(starSystemInfo, planetInfo);
		}

		protected void SetSyncedSystem(StarSystemInfo system, PlanetInfo planet)
		{
			this._app.UI.ClearItems("system_list");
			foreach (SystemWidget systemWidget in this._systemWidgets)
				systemWidget.Terminate();
			foreach (PlanetWidget planetWidget in this._planetWidgets)
				planetWidget.Terminate();
			this._planetWidgets.Clear();
			this._app.UI.AddItem("system_list", "", system.ID, "", "systemTitleCard");
			this._systemWidgets.Add(new SystemWidget(this._app, this._app.UI.GetItemGlobalID("system_list", "", system.ID, "")));
			this._systemWidgets.Last<SystemWidget>().Sync(system.ID);
			this._app.UI.AddItem("system_list", "", planet.ID + 999999, "", "planetDetailsM_INDY_Card");
			this._planetWidgets.Add(new PlanetWidget(this._app, this._app.UI.GetItemGlobalID("system_list", "", planet.ID + 999999, "")));
			this._planetWidgets.Last<PlanetWidget>().Sync(planet.ID, false, false);
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (!(msgType == "button_clicked"))
				return;
			if (panelName == "ignoreButton")
				this._app.UI.CloseDialog((Dialog)this, true);
			if (panelName == "attackButton")
			{
				this._app.Game.DeclareWarFormally(this._app.Game.LocalPlayer.ID, this._playerID);
				this._app.UI.CloseDialog((Dialog)this, true);
			}
			if (!(panelName == "diploButton"))
				return;
			StarMapState currentState = (StarMapState)this._app.CurrentState;
			new OverlayConstructionMission(this._app, currentState, currentState.StarMap, new SpecialConstructionMission()
			{
				_project = SpecialProjectType.IndependentStudy,
				_forcedStationType = StationType.SCIENCE,
				_targetplayerid = this._playerID,
				_targetsystemid = this._systemID,
				_targetplanet = this._planetID
			}, "OverlayStationMission", MissionType.CONSTRUCT_STN).Show(this._systemID);
			this._app.UI.CloseDialog((Dialog)this, true);
		}

		protected override void OnUpdate()
		{
			foreach (PlanetWidget planetWidget in this._planetWidgets)
				planetWidget.Update();
			foreach (SystemWidget systemWidget in this._systemWidgets)
				systemWidget.Update();
		}

		public override string[] CloseDialog()
		{
			foreach (PlanetWidget planetWidget in this._planetWidgets)
				planetWidget.Terminate();
			foreach (SystemWidget systemWidget in this._systemWidgets)
				systemWidget.Terminate();
			return (string[])null;
		}
	}
}
