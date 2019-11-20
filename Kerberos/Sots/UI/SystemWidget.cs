// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.SystemWidget
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;

namespace Kerberos.Sots.UI
{
	internal class SystemWidget
	{
		private string _rootPanel = "";
		private int _systemID;
		public App App;
		private StarModel _cachedStar;
		private StarSystemInfo _cachedStarInfo;
		private bool _cachedStarReady;
		private PlanetView _planetView;
		private GameObjectSet _crits;
		private bool _initialized;
		private bool _starViewLinked;
		private static int kWidgetID;
		private int _widgetID;

		public SystemWidget(App app, string rootPanel)
		{
			this._rootPanel = rootPanel;
			this.App = app;
			this._crits = new GameObjectSet(this.App);
			this._planetView = this._crits.Add<PlanetView>();
			this.App.UI.PanelMessage += new UIEventPanelMessage(this.UICommChannel_OnPanelMessage);
			++SystemWidget.kWidgetID;
			this._widgetID = SystemWidget.kWidgetID;
		}

		public string GetRootPanel()
		{
			return this._rootPanel;
		}

		public void Sync(int systemID)
		{
			StarSystemInfo starSystemInfo = this.App.GameDatabase.GetStarSystemInfo(systemID);
			if (starSystemInfo == (StarSystemInfo)null)
				return;
			this._systemID = systemID;
			this.CacheStar(starSystemInfo);
			StarSystemUI.SyncStarDetailsControl(this.App.Game, this._rootPanel, systemID);
			StarSystemUI.SyncStarDetailsStations(this.App.Game, this._rootPanel, systemID, this.App.LocalPlayer.ID);
			Vector4 vector4 = StarHelper.CalcModelColor(new StellarClass(starSystemInfo.StellarClass));
			this.App.UI.SetPropertyColorNormalized(this.App.UI.Path(this._rootPanel, "colorGradient"), "color", vector4.X, vector4.Y, vector4.Z, 0.5f);
			this._initialized = false;
		}

		private void CacheStar(StarSystemInfo systemInfo)
		{
			if (this._cachedStar != null)
			{
				if (systemInfo == this._cachedStarInfo)
					return;
				this.App.ReleaseObject((IGameObject)this._cachedStar);
				this._cachedStar = (StarModel)null;
			}
			this._cachedStarInfo = systemInfo;
			this._cachedStarReady = false;
			this._cachedStar = Kerberos.Sots.GameStates.StarSystem.CreateStar(this.App, Vector3.Zero, systemInfo, 1f, false);
			this._cachedStar.PostSetProp("AutoDraw", false);
		}

		public void Update()
		{
			if (this._crits == null || !this._crits.IsReady())
				return;
			if (this._cachedStar != null && !this._cachedStarReady && this._cachedStar.ObjectStatus != GameObjectStatus.Pending)
			{
				this._cachedStarReady = true;
				this._cachedStar.Active = true;
			}
			if (!this._cachedStarReady || this._initialized)
				return;
			this._planetView.PostSetProp("Planet", this._cachedStar != null ? this._cachedStar.ObjectID : 0);
			if (!this._starViewLinked)
			{
				this.App.UI.Send((object)"SetGameObject", (object)this.App.UI.Path(this._rootPanel, "contentPreview.desc_viewport"), (object)this._planetView.ObjectID);
				this._starViewLinked = true;
			}
			this.App.UI.SetVisible(this.App.UI.Path(this._rootPanel, "loadingCircle"), false);
			this._initialized = true;
		}

		protected void UICommChannel_OnPanelMessage(
		  string panelName,
		  string msgType,
		  string[] msgParams)
		{
		}

		public void Terminate()
		{
			if (this._cachedStar != null)
				this._cachedStar.Dispose();
			this._crits.Dispose();
			this.App.UI.PanelMessage -= new UIEventPanelMessage(this.UICommChannel_OnPanelMessage);
		}
	}
}
