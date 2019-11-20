// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.TestPlanetState
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.Strategy.InhabitedPlanet;

namespace Kerberos.Sots.GameStates
{
	internal class TestPlanetState : GameState
	{
		private bool _planetDirty = true;
		private GameObjectSet _crits;
		private Sky _sky;
		private OrbitCameraController _camera;
		private StellarBody _planet;
		private bool _planetReady;
		private string[] _types;
		private string[] _factions;
		private string _faction;
		private string _type;
		private int _hazard;
		private double _population;
		private float _biosphere;
		private int _typeVariant;
		private int _seed;
		private int _rotation;

		protected override void OnPrepare(GameState prev, object[] parms)
		{
			this._crits = new GameObjectSet(this.App);
			this._sky = new Sky(this.App, SkyUsage.InSystem, 0);
			this._crits.Add((IGameObject)this._sky);
			this._camera = this._crits.Add<OrbitCameraController>();
			this._types = this.App.AssetDatabase.PlanetGenerationRules.GetStellarBodyTypes();
			this._factions = this.App.AssetDatabase.PlanetGenerationRules.GetFactions();
			this.App.UI.LoadScreen("TestPlanet");
		}

		protected override void OnEnter()
		{
			this.App.UI.SetScreen("TestPlanet");
			this.App.UI.PanelMessage += new UIEventPanelMessage(this.UI_PanelMessage);
			this._camera.MaxDistance = 200000f;
			this._camera.DesiredDistance = 50000f;
			this.App.UI.ClearItems("factionList");
			for (int userItemId = 0; userItemId < this._factions.Length; ++userItemId)
				this.App.UI.AddItem("factionList", string.Empty, userItemId, this._factions[userItemId]);
			for (int userItemId = 0; userItemId < this._types.Length; ++userItemId)
				this.App.UI.AddItem("typeList", string.Empty, userItemId, this._types[userItemId]);
			this.App.UI.SetSelection("factionList", 0);
			this.App.UI.SetSelection("typeList", 0);
			this.App.UI.InitializeSlider("partRotationSlider", 0, 360, 0);
			this.App.UI.InitializeSlider("partHazardSlider", 0, 1500, 0);
			this.App.UI.InitializeSlider("partPopulationSlider", 0, 1000, 0);
			this.App.UI.InitializeSlider("partBiosphereSlider", 0, 1000, 0);
			this.App.UI.InitializeSlider("partRandomSeedSlider", 0, 1000, 0);
			this.App.UI.InitializeSlider("partTypeVariantSlider", 0, 20, 0);
			this._type = this._types[0];
			this._faction = this._factions[0];
			this._crits.Activate();
		}

		private void Regenerate()
		{
			if (this._planet != null)
				this.App.ReleaseObject((IGameObject)this._planet);
			this._planetReady = false;
			this._planet = StellarBody.Create(this.App, this.App.AssetDatabase.PlanetGenerationRules.GetStellarBodyParams(string.Empty, Vector3.Zero, 5000f, this._seed, 0, this._type, (float)this._hazard, 750f, this._faction, this._biosphere, this._population, new int?(this._typeVariant), ColonyStage.Open, SystemColonyType.Normal));
			this._planet.PostSetProp("Rotation", MathHelper.DegreesToRadians((float)this._rotation));
		}

		private void UI_PanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "list_sel_changed")
			{
				if (panelName == "factionList")
				{
					this._faction = string.IsNullOrEmpty(msgParams[0]) ? null : this._factions[int.Parse(msgParams[0])];
					this._planetDirty = true;
				}
				else
				{
					if (!(panelName == "typeList"))
						return;
					this._type = string.IsNullOrEmpty(msgParams[0]) ? null : this._types[int.Parse(msgParams[0])];
					this._planetDirty = true;
				}
			}
			else if (msgType == "slider_value")
			{
				if (panelName == "partRotationSlider")
				{
					this._rotation = int.Parse(msgParams[0]);
					this._planetDirty = true;
				}
				else if (panelName == "partHazardSlider")
				{
					this._hazard = int.Parse(msgParams[0]);
					this._planetDirty = true;
				}
				else if (panelName == "partPopulationSlider")
				{
					this._population = (double)int.Parse(msgParams[0]) * 1000000.0;
					this._planetDirty = true;
				}
				else if (panelName == "partBiosphereSlider")
				{
					this._biosphere = (float)int.Parse(msgParams[0]);
					this._planetDirty = true;
				}
				else if (panelName == "partRandomSeedSlider")
				{
					this._seed = int.Parse(msgParams[0]);
					this._planetDirty = true;
				}
				else
				{
					if (!(panelName == "partTypeVariantSlider"))
						return;
					this._typeVariant = int.Parse(msgParams[0]);
					this._planetDirty = true;
				}
			}
			else
			{
				if (!(msgType == "button_clicked") || !("regenerateButton" == panelName))
					return;
				this._planetDirty = true;
				this.App.AssetDatabase.PlanetGenerationRules.Reload();
			}
		}

		protected override void OnExit(GameState prev, ExitReason reason)
		{
			if (this._crits != null)
				this._crits.Dispose();
			if (this._planet != null)
				this.App.ReleaseObject((IGameObject)this._planet);
			this._planet = (StellarBody)null;
		}

		protected override void OnUpdate()
		{
			if (this._planetDirty)
			{
				this._planetDirty = false;
				this.Regenerate();
			}
			if (this._planetReady || this._planet == null || this._planet.ObjectStatus == GameObjectStatus.Pending)
				return;
			this._planetReady = true;
			this._planet.PostSetActive(true);
		}

		public override bool IsReady()
		{
			if (this._crits != null && this._crits.IsReady())
				return base.IsReady();
			return false;
		}

		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}

		public TestPlanetState(App game)
		  : base(game)
		{
		}
	}
}
