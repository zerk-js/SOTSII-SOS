// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.StarSystemState
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.StarMapFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.UI;

namespace Kerberos.Sots.GameStates
{
	internal class StarSystemState : BasicStarSystemState, IKeyBindListener
	{
		private string _confirmAbandon = "";
		private const string UIEmpireSummaryButton = "gameEmpireSummaryButton";
		private const string UIAbandonColony = "btnAbandon";
		private const string UIOpenSystemButton = "btnSystemOpen";
		private BudgetPiechart _piechart;

		public StarSystemState(App game)
		  : base(game)
		{
		}

		private void InitializeClimateSlider(string sliderId)
		{
			float minSuitability = Constants.MinSuitability;
			float maxSuitability = Constants.MaxSuitability;
			double num1 = ((double)maxSuitability - (double)minSuitability) / 2.0;
			int minValue = (int)-(double)maxSuitability;
			int maxValue = (int)maxSuitability;
			int num2 = 0;
			this.App.UI.InitializeSlider(sliderId, minValue, maxValue, num2);
		}

		protected override void OnPrepare(GameState prev, object[] stateParams)
		{
			base.OnPrepare(prev, stateParams);
			this.App.UI.LoadScreen("StarSystem");
		}

		protected override void OnEnter()
		{
			base.OnEnter();
			this.App.UI.SetScreen("StarSystem");
			this._piechart = new BudgetPiechart(this.App.UI, "piechart", this.App.AssetDatabase);
			EmpireBarUI.SyncTitleBar(this.App, "gameEmpireBar", this._piechart);
			EmpireBarUI.SyncTitleFrame(this.App);
			this.InitializeClimateSlider(this.App.UI.Path("colonyControl", "partClimateSlider"));
			this.App.UI.SetPropertyBool(this.App.UI.Path("colonyControl", "partTradeSlider"), "only_user_events", true);
			this.App.UI.SetPropertyBool(this.App.UI.Path("colonyControl", "partTerraSlider"), "only_user_events", true);
			this.App.UI.SetPropertyBool(this.App.UI.Path("colonyControl", "partInfraSlider"), "only_user_events", true);
			this.App.UI.SetPropertyBool(this.App.UI.Path("colonyControl", "partShipConSlider"), "only_user_events", true);
			this.App.UI.SetPropertyBool(this.App.UI.Path("colonyControl", "partCivSlider"), "only_user_events", true);
			this.App.UI.SetPropertyBool(this.App.UI.Path("colonyControl", "partOverDevelopment"), "only_user_events", true);
			this.App.UI.SetPropertyBool("gameSystemContentsList", "only_user_events", true);
			this.App.UI.InitializeSlider(this.App.UI.Path("colonyControl", "partCivSlider"), 0, 100, 50);
			this.App.UI.InitializeSlider(this.App.UI.Path("colonyControl", "partTerraSlider"), 0, 100, 50);
			this.App.UI.InitializeSlider(this.App.UI.Path("colonyControl", "partInfraSlider"), 0, 100, 50);
			this.App.UI.InitializeSlider(this.App.UI.Path("colonyControl", "partShipConSlider"), 0, 100, 50);
			this.App.UI.InitializeSlider(this.App.UI.Path("colonyControl", "partOverDevelopment"), 0, 100, 50);
			this.InitializeClimateSlider(this.App.UI.Path("gamePlanetDetails", "partClimateSlider"));
			StarSystemUI.SyncSystemDetailsWidget(this.App, "systemDetailsWidget", this.CurrentSystem, false, true);
			StarSystemUI.SyncPlanetListWidget(this.App.Game, "planetListWidget", this.App.GameDatabase.GetStarSystemPlanets(this.CurrentSystem));
			StarSystemUI.SyncColonyDetailsWidget(this.App.Game, "colonyDetailsWidget", this.SelectedObject, "");
			StarSystemUI.SyncPlanetDetailsWidget(this.App.Game, "planetDetailsWidget", this.CurrentSystem, this.SelectedObject, this.GetPlanetViewGameObject(this.CurrentSystem, this.SelectedObject), this._planetView);
			this.App.HotKeyManager.AddListener((IKeyBindListener)this);
		}

		protected override void OnExit(GameState prev, ExitReason reason)
		{
			this.App.HotKeyManager.RemoveListener((IKeyBindListener)this);
			base.OnExit(prev, reason);
			this._piechart = (BudgetPiechart)null;
		}

		protected override void OnUIGameEvent(string eventName, string[] eventParams)
		{
			this._piechart.TryGameEvent(eventName, eventParams);
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (this._piechart.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self))
				return;
			if (msgType == "slider_value")
			{
				if (this.SelectedObject == StarSystemDetailsUI.StarItemID)
					return;
				if (StarSystemDetailsUI.IsOutputRateSlider(panelName))
				{
					StarSystemDetailsUI.SetOutputRate(this.App, this.SelectedObject, panelName, msgParams[0]);
					StarSystemUI.SyncColonyDetailsWidget(this.App.Game, "colonyDetailsWidget", this.SelectedObject, "");
				}
				else if (panelName == "partOverharvestSlider")
				{
					ColonyInfo colonyInfoForPlanet = this.App.GameDatabase.GetColonyInfoForPlanet(this.SelectedObject);
					colonyInfoForPlanet.OverharvestRate = StarSystemDetailsUI.SliderValueToOutputRate(int.Parse(msgParams[0]));
					this.App.GameDatabase.UpdateColony(colonyInfoForPlanet);
					StarSystemUI.SyncColonyDetailsWidget(this.App.Game, "colonyDetailsWidget", this.SelectedObject, "");
				}
				else
				{
					if (!(panelName == "gameEmpireResearchSlider"))
						return;
					StarMapState.SetEmpireResearchRate(this.App.Game, msgParams[0], this._piechart);
				}
			}
			else if (msgType == "button_clicked")
			{
				if (panelName == "gameEmpireSummaryButton")
					this.App.SwitchGameState<EmpireSummaryState>();
				else if (panelName == "btnAbandon")
				{
					this._confirmAbandon = this.App.UI.CreateDialog((Dialog)new GenericQuestionDialog(this.App, "@UI_DIALOGCONFIRMABANDON_TITLE", "@UI_DIALOGCONFIRMABANDON_DESC", "dialogGenericQuestion"), null);
				}
				else
				{
					if (!(panelName == "btnSystemOpen"))
						return;
					bool isOpen = !this.App.GameDatabase.GetStarSystemInfo(this.CurrentSystem).IsOpen;
					this.App.GameDatabase.UpdateStarSystemOpen(this.CurrentSystem, isOpen);
					this.App.UI.SetVisible("SystemDetailsWidget.ClosedSystem", !isOpen);
					this.App.Game.OCSystemToggleData.SystemToggled(this.App.LocalPlayer.ID, this.CurrentSystem, isOpen);
				}
			}
			else
			{
				if (!(msgType == "dialog_closed") || !(panelName == this._confirmAbandon) || !bool.Parse(msgParams[0]))
					return;
				PlanetInfo planetInfo = this.App.GameDatabase.GetPlanetInfo(this.SelectedObject);
				if (planetInfo == null)
					return;
				GameSession.AbandonColony(this.App, this.App.GameDatabase.GetColonyInfoForPlanet(planetInfo.ID).ID);
			}
		}

		public bool OnKeyBindPressed(HotKeyManager.HotKeyActions action, string gamestates)
		{
			if (gamestates.Contains(this.Name))
			{
				switch (action)
				{
					case HotKeyManager.HotKeyActions.State_Starmap:
						this.App.UI.LockUI();
						this.App.SwitchGameState<StarMapState>();
						return true;
					case HotKeyManager.HotKeyActions.State_BuildScreen:
						return false;
					case HotKeyManager.HotKeyActions.State_DesignScreen:
						this.App.UI.LockUI();
						this.App.SwitchGameState<DesignScreenState>((object)false, (object)this.Name);
						return true;
					case HotKeyManager.HotKeyActions.State_ResearchScreen:
						this.App.UI.LockUI();
						this.App.SwitchGameState<ResearchScreenState>();
						return true;
					case HotKeyManager.HotKeyActions.State_ComparativeAnalysysScreen:
						return false;
					case HotKeyManager.HotKeyActions.State_EmpireSummaryScreen:
						this.App.UI.LockUI();
						this.App.SwitchGameState<EmpireSummaryState>();
						return true;
					case HotKeyManager.HotKeyActions.State_SotspediaScreen:
						this.App.UI.LockUI();
						this.App.SwitchGameState<SotspediaState>();
						return true;
					case HotKeyManager.HotKeyActions.State_DiplomacyScreen:
						this.App.UI.LockUI();
						this.App.SwitchGameState<DiplomacyScreenState>();
						return true;
				}
			}
			return false;
		}
	}
}
