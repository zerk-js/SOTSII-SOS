// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.BasicStarSystemState
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.GameStates
{
	internal abstract class BasicStarSystemState : GameState
	{
		private readonly Random _random = new Random();
		private readonly List<IGameObject> _objects = new List<IGameObject>();
		protected const string UISystemMap = "partMiniSystem";
		protected const string UIEmpireBar = "gameEmpireBar";
		protected const string UIContentsList = "gameSystemContentsList";
		protected const string UIBackButton = "gameExitButton";
		protected const string UISystemDetails = "SystemView";
		protected const string UIColonyDetailsWidget = "colonyDetailsWidget";
		protected const string UISystemDetailsWidget = "systemDetailsWidget";
		protected const string UIPlanetDetailsWidget = "planetDetailsWidget";
		protected const string UIPlanetListWidget = "planetListWidget";
		protected StarSystem _starsystem;
		private Sky _sky;
		private OrbitCameraController _camera;
		private GameObjectSet _crits;
		private int _currentSystem;
		protected PlanetView _planetView;
		private StellarBody _cachedPlanet;
		private bool _cachedPlanetReady;
		private StarModel _cachedStar;
		private bool _cachedStarReady;

		protected OrbitCameraController Camera
		{
			get
			{
				return this._camera;
			}
		}

		public int SelectedObject { get; set; }

		public StarSystem StarSystem
		{
			get
			{
				return this._starsystem;
			}
		}

		public int CurrentSystem
		{
			get
			{
				return this._currentSystem;
			}
			private set
			{
				this._currentSystem = value;
			}
		}

		protected override void OnPrepare(GameState prev, object[] stateParams)
		{
			if (this.App.GameDatabase == null)
			{
				this.App.NewGame(new Random(12345));
				if (stateParams == null || stateParams.Length == 0)
				{
					int orbitalObjectID = this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID).Homeworld.Value;
					stateParams = new object[2]
					{
			(object) this.App.GameDatabase.GetOrbitalObjectInfo(orbitalObjectID).StarSystemID,
			(object) orbitalObjectID
					};
				}
			}
			int stateParam1 = (int)stateParams[0];
			int stateParam2 = (int)stateParams[1];
			this._crits = new GameObjectSet(this.App);
			this._sky = new Sky(this.App, SkyUsage.InSystem, stateParam1);
			this._crits.Add((IGameObject)this._sky);
			this._camera = this._crits.Add<OrbitCameraController>();
			this._planetView = this._crits.Add<PlanetView>();
			this._starsystem = new StarSystem(this.App, 1f, stateParam1, Vector3.Zero, true, (CombatSensor)null, false, 0, this is DefenseManagerState, true);
			this._crits.Add((IGameObject)this._starsystem);
			this._starsystem.PostSetProp("CameraController", (IGameObject)this._camera);
			this._starsystem.PostSetProp("InputEnabled", true);
			this.CurrentSystem = stateParam1;
			this.SelectedObject = stateParam2;
		}

		protected override void OnEnter()
		{
			this.App.UI.GameEvent += new UIEventGameEvent(this.UICommChannel_GameEvent);
			float num = 0.0f;
			foreach (OrbitalObjectInfo orbitalObjectInfo in this.App.GameDatabase.GetStarSystemOrbitalObjectInfos(this.CurrentSystem))
			{
				float length = orbitalObjectInfo.OrbitalPath.Scale.Length;
				if ((double)num < (double)length)
					num = length;
			}
			this._camera.MaxDistance = num * 4f;
			this._camera.DesiredDistance = num * 0.2f;
			this._camera.DesiredPitch = MathHelper.DegreesToRadians(-25f);
			this._camera.SnapToDesiredPosition();
			this._camera.DesiredDistance = num * 1.6f;
			this._crits.Activate();
			if (this.SelectedObject == 0)
				this.SelectedObject = this._starsystem.ObjectMap.Forward.Values.First<int>();
			if (this.SelectedObject == 0)
				return;
			int selectedObject = this.SelectedObject;
			this.SelectedObject = 0;
			this.SetSelectedObject(selectedObject, string.Empty);
			this.Focus(this.SelectedObject);
		}

		protected IGameObject GetPlanetViewGameObject(int systemId, int orbitId)
		{
			IGameObject gameObject = (IGameObject)null;
			if (systemId != 0)
			{
				if (orbitId > 0)
				{
					PlanetInfo planetInfo = this.App.GameDatabase.GetPlanetInfo(orbitId);
					if (planetInfo != null)
					{
						this.CachePlanet(planetInfo);
						gameObject = (IGameObject)this._cachedPlanet;
					}
				}
				else
				{
					this.CacheStar(this.App.GameDatabase.GetStarSystemInfo(systemId));
					gameObject = (IGameObject)this._cachedStar;
				}
			}
			return gameObject;
		}

		private void CacheStar(StarSystemInfo systemInfo)
		{
			if (this._cachedStar != null)
			{
				this.App.ReleaseObject((IGameObject)this._cachedStar);
				this._cachedStar = (StarModel)null;
			}
			this._cachedStarReady = false;
			this._cachedStar = StarSystem.CreateStar(this.App, Vector3.Zero, systemInfo, 1f, false);
			this._cachedStar.PostSetProp("AutoDraw", false);
		}

		private void CachePlanet(PlanetInfo planetInfo)
		{
			if (planetInfo == null)
				return;
			if (this._cachedPlanet != null)
			{
				this.App.ReleaseObject((IGameObject)this._cachedPlanet);
				this._cachedPlanet = (StellarBody)null;
			}
			this._cachedPlanetReady = false;
			this._cachedPlanet = StarSystem.CreatePlanet(this.App.Game, Vector3.Zero, planetInfo, Matrix.Identity, 1f, false, StarSystem.TerrestrialPlanetQuality.High);
			this._cachedPlanet.PostSetProp("AutoDraw", false);
			this._crits.Add((IGameObject)this._cachedPlanet);
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

		private void UICommChannel_GameEvent(string eventName, string[] eventParams)
		{
			if (eventName == "ObjectClicked")
				this.ProcessGameEvent_ObjectClicked(eventParams);
			this.OnUIGameEvent(eventName, eventParams);
		}

		protected abstract void OnUIGameEvent(string eventName, string[] eventParams);

		private void ProcessGameEvent_ObjectClicked(string[] eventParams)
		{
			IGameObject gameObject = this.App.GetGameObject(int.Parse(eventParams[0]));
			if (gameObject == null)
				return;
			int orbitalId = 0;
			this._starsystem.ObjectMap.Forward.TryGetValue(gameObject, out orbitalId);
			this.SetSelectedObject(orbitalId, string.Empty);
		}

		protected virtual void OnBack()
		{
			if (this.App.PreviousState is EmpireSummaryState || this.App.PreviousState is DesignScreenState)
				this.App.SwitchGameState<StarMapState>();
			else
				this.App.SwitchGameState(this.App.PreviousState);
		}

		protected override void UICommChannel_OnPanelMessage(
		  string panelName,
		  string msgType,
		  string[] msgParams)
		{
			if (msgType == "mapicon_clicked")
			{
				if (panelName == "partMiniSystem")
				{
					int orbitalId = int.Parse(msgParams[0]);
					this.SetSelectedObject(orbitalId, "partMiniSystem");
					this.Focus(orbitalId);
				}
			}
			else if (msgType == "button_clicked")
			{
				if (panelName == "gameExitButton")
					this.OnBack();
			}
			else if (msgType == "list_sel_changed")
			{
				if (panelName == "gamePlanetList")
				{
					int orbitalId = 0;
					if (msgParams.Length > 0 && !string.IsNullOrEmpty(msgParams[0]))
						orbitalId = int.Parse(msgParams[0]);
					if (orbitalId != this.SelectedObject)
					{
						this.SetSelectedObject(orbitalId, "gameSystemContentsList");
						this.Focus(orbitalId);
					}
				}
			}
			else if (msgType == "OutputRatesChanged")
				throw new NotImplementedException();
			this.OnPanelMessage(panelName, msgType, msgParams);
		}

		protected abstract void OnPanelMessage(string panelName, string msgType, string[] msgParams);

		protected override void OnExit(GameState prev, ExitReason reason)
		{
			this.App.UI.GameEvent -= new UIEventGameEvent(this.UICommChannel_GameEvent);
			this._starsystem = (StarSystem)null;
			if (this._crits == null)
				return;
			this._crits.Dispose();
			this._crits = (GameObjectSet)null;
		}

		protected override void OnUpdate()
		{
			this.UpdateCachedPlanet();
			this.UpdateCachedStar();
		}

		public override bool IsReady()
		{
			if (this._crits.IsReady())
				return base.IsReady();
			return false;
		}

		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}

		public BasicStarSystemState(App game)
		  : base(game)
		{
		}

		private void Focus(int orbitalId)
		{
			if (orbitalId == 0)
				return;
			this._camera.TargetID = this._starsystem.ObjectMap.Reverse[orbitalId].ObjectID;
		}

		protected void SetSelectedObject(int orbitalId, string trigger)
		{
			this.SelectedObject = orbitalId;
			if (trigger != "gameSystemContentsList")
			{
				if (this.SelectedObject == 0)
					this.App.UI.ClearSelection("gameSystemContentsList");
				else if (this.SelectedObject == StarSystemDetailsUI.StarItemID)
				{
					this.App.UI.SetSelection("gameSystemContentsList", this.SelectedObject);
				}
				else
				{
					OrbitalObjectInfo orbitalObjectInfo = this.App.GameDatabase.GetOrbitalObjectInfo(this.SelectedObject);
					if (!orbitalObjectInfo.ParentID.HasValue)
					{
						this.App.UI.SetSelection("gameSystemContentsList", this.SelectedObject);
					}
					else
					{
						IGameObject planetViewObject;
						this._starsystem.PlanetMap.Reverse.TryGetValue(orbitalObjectInfo.ParentID.Value, out planetViewObject);
						StarSystemUI.SyncPlanetDetailsWidget(this.App.Game, "planetDetailsWidget", this._currentSystem, orbitalId, planetViewObject, this._planetView);
						if (planetViewObject != null)
							this.App.UI.SetSelection("gameSystemContentsList", orbitalObjectInfo.ParentID.Value);
						else
							this.App.UI.ClearSelection("gameSystemContentsList");
					}
				}
				this.Focus(this.SelectedObject);
			}
			StarSystemUI.SyncPlanetDetailsWidget(this.App.Game, "planetDetailsWidget", this.CurrentSystem, this.SelectedObject, this.GetPlanetViewGameObject(this.CurrentSystem, this.SelectedObject), this._planetView);
			StarSystemUI.SyncColonyDetailsWidget(this.App.Game, "colonyDetailsWidget", this.SelectedObject, "");
		}
	}
}
