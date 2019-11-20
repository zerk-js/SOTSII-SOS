// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.ColonySelfSufficientDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.StarFleet;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class ColonySelfSufficientDialog : Dialog
	{
		private const string OkButton = "btnOk";
		private const string SupportButton = "btnSupport";
		private OrbitalObjectInfo orbitalObject;
		private PlanetInfo planet;
		private ColonyInfo colony;
		private MissionInfo mission;
		private PlanetWidget planetwidget;

		public ColonySelfSufficientDialog(App game, int planetId, int missionId)
		  : base(game, "dialogColonySelfSufficientEvent")
		{
			this.orbitalObject = game.GameDatabase.GetOrbitalObjectInfo(planetId);
			this.planet = game.GameDatabase.GetPlanetInfo(planetId);
			this.colony = game.GameDatabase.GetColonyInfoForPlanet(planetId);
			this.mission = game.GameDatabase.GetMissionInfo(missionId);
		}

		public override void Initialize()
		{
			if (this.mission != null)
			{
				this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, "lblDesc"), "text", string.Format(App.Localize("@UI_DIALOGSELFSUFFICIENT_DESC"), (object)this.orbitalObject.Name, (object)this._app.GameDatabase.GetStarSystemInfo(this.orbitalObject.StarSystemID).Name, (object)this._app.Game.GetNumSupportTrips(this.mission), (object)this._app.GameDatabase.GetFleetInfo(this.mission.FleetID).Name));
				this._app.UI.SetVisible(this._app.UI.Path(this.ID, "btnSupport"), true);
				this._app.UI.SetVisible(this._app.UI.Path(this.ID, "lblDesc"), true);
			}
			else
			{
				this._app.UI.SetVisible(this._app.UI.Path(this.ID, "btnSupport"), false);
				this._app.UI.SetVisible(this._app.UI.Path(this.ID, "lblDesc"), false);
			}
			this.planetwidget = new PlanetWidget(this._app, this._app.UI.Path(this.ID, "planetcard"));
			this.planetwidget.Sync(this.planet.ID, false, false);
		}

		protected override void OnUpdate()
		{
			if (this.planetwidget == null)
				return;
			this.planetwidget.Update();
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (!(msgType == "button_clicked"))
				return;
			if (panelName == "btnOk")
			{
				if (this.mission != null)
				{
					FleetInfo fleetInfo = this._app.GameDatabase.GetFleetInfo(this.mission.FleetID);
					if (fleetInfo == null)
					{
						this._app.GameDatabase.RemoveMission(this.mission.ID);
						return;
					}
					AdmiralInfo admiralInfo = this._app.GameDatabase.GetAdmiralInfo(fleetInfo.AdmiralID);
					if (admiralInfo != null)
						this._app.PostRequestSpeech(string.Format("STRAT_009-01_{0}_{1}UniversalMissionComplete", (object)this._app.GameDatabase.GetFactionName(this._app.GameDatabase.GetPlayerFactionID(this._app.LocalPlayer.ID)), (object)admiralInfo.GetAdmiralSoundCueContext(this._app.AssetDatabase)), 50, 120, 0.0f);
				}
				this._app.UI.CloseDialog((Dialog)this, true);
			}
			else
			{
				if (!(panelName == "btnSupport"))
					return;
				List<WaypointInfo> list = this._app.GameDatabase.GetWaypointsByMissionID(this.mission.ID).ToList<WaypointInfo>();
				foreach (WaypointInfo waypointInfo in list)
					this._app.GameDatabase.RemoveWaypoint(waypointInfo.ID);
				FleetInfo fleetInfo = this._app.GameDatabase.GetFleetInfo(this.mission.FleetID);
				int numSupportTrips = this._app.Game.GetNumSupportTrips(this.mission);
				for (int index = 0; index < numSupportTrips; ++index)
				{
					if (this.mission.TargetSystemID != fleetInfo.SupportingSystemID)
						this._app.GameDatabase.InsertWaypoint(this.mission.ID, WaypointType.TravelTo, new int?(this.mission.TargetSystemID));
					this._app.GameDatabase.InsertWaypoint(this.mission.ID, WaypointType.DoMission, new int?());
					if (this.mission.TargetSystemID != fleetInfo.SupportingSystemID)
						this._app.GameDatabase.InsertWaypoint(this.mission.ID, WaypointType.ReturnHome, new int?());
				}
				this._app.GameDatabase.InsertWaypoint(this.mission.ID, WaypointType.CheckSupportColony, new int?());
				foreach (WaypointInfo waypointInfo in list)
					this._app.GameDatabase.InsertWaypoint(this.mission.ID, waypointInfo.Type, waypointInfo.SystemID);
				this._app.UI.CloseDialog((Dialog)this, true);
			}
		}

		public override string[] CloseDialog()
		{
			this.planetwidget.Terminate();
			return (string[])null;
		}
	}
}
