// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.StationBuiltDialogAlt
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class StationBuiltDialogAlt : Dialog
	{
		public const string OKButton = "event_dialog_close";
		private int _stationID;
		private OrbitCameraController _cameraReduced;
		private StarMap _starmapReduced;
		private Sky _sky;
		private GameObjectSet _crits;
		private PlanetView _planetView;
		private StellarBody _cachedPlanet;
		private StarSystemDummyOccupant _stationModel;
		private string _enteredStationName;
		private Vector3 _trans;

		public StationBuiltDialogAlt(App game, int stationid, Vector3 trans)
		  : base(game, "dialogStationBuiltAlt")
		{
			this._stationID = stationid;
			this._trans = trans;
		}

		public override void Initialize()
		{
			this._sky = new Sky(this._app, SkyUsage.StarMap, 0);
			this._crits = new GameObjectSet(this._app);
			this._planetView = this._crits.Add<PlanetView>();
			this._crits.Add((IGameObject)this._sky);
			StationInfo stationInfo = this._app.GameDatabase.GetStationInfo(this._stationID);
			OrbitalObjectInfo orbitalObjectInfo1 = this._app.GameDatabase.GetOrbitalObjectInfo(this._stationID);
			OrbitalObjectInfo orbitalObjectInfo2 = this._app.GameDatabase.GetOrbitalObjectInfo(orbitalObjectInfo1.ParentID.Value);
			this._app.UI.SetText(this._app.UI.Path(this.ID, "station_class"), string.Format(App.Localize("@STATION_LEVEL"), (object)stationInfo.DesignInfo.StationLevel.ToString(), (object)stationInfo.DesignInfo.StationType.ToString()));
			this._app.UI.SetText(this._app.UI.Path(this.ID, "upkeep_cost"), string.Format(App.Localize("@STATION_UPKEEP_COST"), (object)GameSession.CalculateStationUpkeepCost(this._app.GameDatabase, this._app.AssetDatabase, stationInfo).ToString()));
			if (stationInfo.DesignInfo.StationType == StationType.NAVAL)
				this._app.UI.SetText(this._app.UI.Path(this.ID, "naval_capacity"), string.Format(App.Localize("@STATION_FLEET_CAPACITY"), (object)this._app.GameDatabase.GetSystemSupportedCruiserEquivalent(this._app.Game, orbitalObjectInfo2.StarSystemID, this._app.LocalPlayer.ID).ToString()));
			else
				this._app.UI.SetVisible(this._app.UI.Path(this.ID, "naval_capacity"), false);
			StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(orbitalObjectInfo2.StarSystemID);
			this._app.UI.SetText("gameStationName", orbitalObjectInfo1.Name);
			this._enteredStationName = orbitalObjectInfo2.Name;
			this._app.UI.SetText(this._app.UI.Path(this.ID, "system_name"), string.Format(App.Localize("@STATION_BUILT"), (object)starSystemInfo.Name).ToUpperInvariant());
			this._cameraReduced = new OrbitCameraController(this._app);
			this._cameraReduced.MinDistance = 1002.5f;
			this._cameraReduced.MaxDistance = 10000f;
			this._cameraReduced.DesiredDistance = 2000f;
			this._cameraReduced.DesiredYaw = MathHelper.DegreesToRadians(45f);
			this._cameraReduced.DesiredPitch = -MathHelper.DegreesToRadians(25f);
			this._cameraReduced.SnapToDesiredPosition();
			this._starmapReduced = new StarMap(this._app, this._app.Game, this._sky);
			this._starmapReduced.Initialize(this._crits);
			this._starmapReduced.SetCamera(this._cameraReduced);
			this._starmapReduced.FocusEnabled = false;
			this._starmapReduced.PostSetProp("Selected", this._starmapReduced.Systems.Reverse[starSystemInfo.ID].ObjectID);
			this._starmapReduced.PostSetProp("CullCenter", this._app.GameDatabase.GetStarSystemInfo(starSystemInfo.ID).Origin);
			this._starmapReduced.PostSetProp("CullRadius", 15f);
			DesignInfo di = DesignLab.CreateStationDesignInfo(this._app.AssetDatabase, this._app.GameDatabase, this._app.LocalPlayer.ID, stationInfo.DesignInfo.StationType, stationInfo.DesignInfo.StationLevel, false);
			this._stationModel = new StarSystemDummyOccupant(this._app, this._app.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == di.DesignSections[0].FilePath)).ModelName, stationInfo.DesignInfo.StationType);
			this._stationModel.PostSetScale(1f / 500f);
			this._stationModel.PostSetPosition(this._trans);
		}

		public void Update()
		{
			if (this._starmapReduced != null && !this._starmapReduced.Active && this._starmapReduced.ObjectStatus != GameObjectStatus.Pending)
				this._starmapReduced.Active = true;
			if (this._cameraReduced != null && !this._cameraReduced.Active && this._cameraReduced.ObjectStatus != GameObjectStatus.Pending)
				this._cameraReduced.Active = true;
			if (this._stationModel == null || this._stationModel.Active || this._stationModel.ObjectStatus == GameObjectStatus.Pending)
				return;
			this._stationModel.Active = true;
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (!(panelName == "event_dialog_close"))
					return;
				OrbitalObjectInfo orbitalObjectInfo = this._app.GameDatabase.GetOrbitalObjectInfo(this._stationID);
				if (orbitalObjectInfo != null)
				{
					orbitalObjectInfo.Name = this._enteredStationName;
					this._app.GameDatabase.UpdateOrbitalObjectInfo(orbitalObjectInfo);
				}
				if (!string.IsNullOrWhiteSpace(this._enteredStationName) && this._enteredStationName.Count<char>() > 0)
					this._app.UI.CloseDialog((Dialog)this, true);
				else
					this._app.UI.CreateDialog((Dialog)new GenericTextDialog(this._app, App.Localize("@INVALID_STATION_NAME"), App.Localize("@INVALID_STATION_NAME_TEXT"), "dialogGenericMessage"), null);
			}
			else
			{
				if (!(msgType == "text_changed") || !(panelName == "gameStationName"))
					return;
				this._enteredStationName = msgParams[0];
			}
		}

		public override string[] CloseDialog()
		{
			if (this._cachedPlanet != null)
				this._app.ReleaseObject((IGameObject)this._cachedPlanet);
			this._cachedPlanet = (StellarBody)null;
			this._crits.Dispose();
			this._starmapReduced.Dispose();
			this._cameraReduced.Dispose();
			this._stationModel.Dispose();
			return (string[])null;
		}
	}
}
