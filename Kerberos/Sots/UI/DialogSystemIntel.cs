// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.DialogSystemIntel
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class DialogSystemIntel : Dialog
	{
		private List<SystemWidget> _systemWidgets = new List<SystemWidget>();
		public const string OKButton = "okButton";
		private int _systemID;
		private List<PlanetWidget> _planetWidgets;
		private App App;
		private SystemWidget _systemWidget;
		private Kerberos.Sots.GameStates.StarSystem _starsystem;
		private OrbitCameraController _camera;
		private Sky _sky;
		private GameObjectSet _crits;
		private bool _critsInitialized;
		private PlayerInfo _targetPlayer;
		private string _descriptor;
		private DialogSystemIntel.PlanetFilterMode _currentFilterMode;

		public DialogSystemIntel(App app, int systemid, PlayerInfo targetPlayer, string descriptor)
		  : base(app, "dialogSystemIntelEvent")
		{
			this._descriptor = descriptor;
			this._systemID = systemid;
			this.App = app;
			this._targetPlayer = targetPlayer;
		}

		public override void Initialize()
		{
			this._systemWidget = new SystemWidget(this.App, this.App.UI.Path(this.ID, "starDetailsCard"));
			StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(this._systemID);
			this._app.UI.SetText(this._app.UI.Path(this.ID, "intel_desc"), this._descriptor);
			this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, "playerAvatar"), "sprite", Path.GetFileNameWithoutExtension(this._targetPlayer.AvatarAssetPath));
			this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, "playerBadge"), "sprite", Path.GetFileNameWithoutExtension(this._targetPlayer.BadgeAssetPath));
			this._app.UI.SetVisible(this._app.UI.Path(this.ID, "system_map"), true);
			this._app.UI.SetVisible(this._app.UI.Path(this.ID, "gameStarSystemViewport"), false);
			this._app.UI.AddItem(this._app.UI.Path(this.ID, "gameViewportList"), "", 0, App.Localize("@SYSTEMDETAILS_SYS_MAP"));
			this._app.UI.AddItem(this._app.UI.Path(this.ID, "gameViewportList"), "", 1, "System");
			this._app.UI.SetSelection(this._app.UI.Path(this.ID, "gameViewportList"), 0);
			this._crits = new GameObjectSet(this._app);
			this._camera = new OrbitCameraController(this._app);
			this._sky = new Sky(this._app, SkyUsage.InSystem, this._systemID);
			this._starsystem = new Kerberos.Sots.GameStates.StarSystem(this.App, 1f, this._systemID, Vector3.Zero, true, (CombatSensor)null, false, 0, false, true);
			this._starsystem.SetAutoDrawEnabled(false);
			this._starsystem.SetCamera(this._camera);
			this._starsystem.SetInputEnabled(true);
			this._starsystem.PostObjectAddObjects((IGameObject)this._sky);
			foreach (IGameObject state in this._starsystem.Crits.Objects.Where<IGameObject>((Func<IGameObject, bool>)(x =>
		   {
			   if (!(x is StellarBody))
				   return x is StarModel;
			   return true;
		   })))
				state.PostSetProp("AutoDrawEnabled", false);
			this._crits.Add((IEnumerable<IGameObject>)new IGameObject[3]
			{
		(IGameObject) this._camera,
		(IGameObject) this._sky,
		(IGameObject) this._starsystem
			});
			this._app.UI.Send((object)"SetGameObject", (object)this._app.UI.Path(this.ID, "gameStarSystemViewport"), (object)this._starsystem.ObjectID);
			this._critsInitialized = false;
			this._camera.PostSetLook(new Vector3(0.0f, 0.0f, 0.0f));
			this._camera.PostSetPosition(new Vector3(0.0f, 100000f, 0.0f));
			this._camera.MaxDistance = 500000f;
			this._camera.MinDistance = 100000f;
			StarSystemUI.SyncSystemDetailsWidget(this._app, this._app.UI.Path(this.ID, "system_details"), this._systemID, false, true);
			StarSystemMapUI.Sync(this._app, this._systemID, this._app.UI.Path(this.ID, "system_map"), true);
			this._currentFilterMode = DialogSystemIntel.PlanetFilterMode.AllPlanets;
			this._planetWidgets = new List<PlanetWidget>();
			this.SetSyncedSystem(starSystemInfo);
			this._systemWidget.Sync(this._systemID);
		}

		protected override void OnUpdate()
		{
			if (!this._critsInitialized && this._crits.IsReady())
			{
				this._critsInitialized = true;
				this._crits.Activate();
			}
			foreach (PlanetWidget planetWidget in this._planetWidgets)
				planetWidget.Update();
			foreach (SystemWidget systemWidget in this._systemWidgets)
				systemWidget.Update();
			this._systemWidget.Update();
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
					if (this._currentFilterMode == DialogSystemIntel.PlanetFilterMode.AllPlanets)
						planetInfoList.Add(planetInfo);
					else if (this._currentFilterMode == DialogSystemIntel.PlanetFilterMode.SurveyedPlanets)
					{
						if (this.App.GameDatabase.GetColonyIntelForPlanet(this.App.LocalPlayer.ID, planetInfo.ID) == null)
							planetInfoList.Add(planetInfo);
					}
					else if (this._currentFilterMode == DialogSystemIntel.PlanetFilterMode.OwnedPlanets)
					{
						AIColonyIntel colonyIntelForPlanet = this.App.GameDatabase.GetColonyIntelForPlanet(this.App.LocalPlayer.ID, planetInfo.ID);
						if (colonyIntelForPlanet != null && colonyIntelForPlanet.OwningPlayerID == this.App.LocalPlayer.ID)
							planetInfoList.Add(planetInfo);
					}
					else if (this._currentFilterMode == DialogSystemIntel.PlanetFilterMode.EnemyPlanets)
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
				if (!(panelName == "detailbutton"))
					;
			}
			else if (msgType == "list_sel_changed")
			{
				if (panelName == "gamePlanetList")
				{
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
					if (!(panelName == "gameViewportList"))
						return;
					this.SetColonyViewMode(int.Parse(msgParams[0]));
				}
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
