// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.EncounterDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class EncounterDialog : Dialog
	{
		public static readonly string UIPostCombatSummaryPanel = "pnlPostCombatSummary";
		public static readonly string UIEncounterSummaryPanel = "pnlEncounterSummary";
		public static readonly string UIEncounterListPanel = "pnlPendingCombats";
		public static readonly string UIPreviousAlly = "btnPreviousAlly";
		public static readonly string UINextAlly = "btnNextAlly";
		public static readonly string UIPreviousEnemy = "btnPreviousEnemy";
		public static readonly string UINextEnemy = "btnNextEnemy";
		public static readonly string UICombatHeaderLabel = "lblHeader";
		public static readonly string UIInfoPanel = "pnlInfo";
		public static readonly string UIInfoList = "lstInfo";
		public static readonly string UIPostCombatImage = "imgPostCombatEvent";
		public static readonly string UIAlliedPlayerCard = "pnlAlliedPlayer";
		public static readonly string UIEnemyPlayerCard = "pnlEnemyPlayer";
		public static readonly string UIZoneRender = "pnlZoneRender";
		public static readonly string UIAlliedDamagePanel = "pnlAlliedDamage";
		public static readonly string UIAlliedDamageList = "lstAlliedDamage";
		public static readonly string UIEnemyDamagePanel = "pnlEnemyDamage";
		public static readonly string UIEnemyDamageList = "lstEnemyDamage";
		public static readonly string UIWeaponIcon = "imgWeaponIcon";
		public static readonly string UIWeaponDamage = "lblWeaponDamage";
		public static readonly string UIEncounterTitle = "lblEncounterTitle";
		public static readonly string UIMiniMapPanel = "pnlMiniMap";
		public static readonly string UIMiniMapPart = "partMiniSystem";
		public static readonly string UIInhabitantsPanel = "pnlPendingCombats";
		public static readonly string UIInhabitantsList = "lstCombats";
		public static readonly string UILocalPlayer = "pnlLocalPlayer";
		public static readonly string UIEncounterPlayer = "pnlEncounterPlayer";
		public static readonly string UILocalFleets = "pnlLocalFleets";
		public static readonly string UIEncounterFleets = "pnlEncounterFleets";
		public static readonly string UILocalFleetList = "lstLocalFleets";
		public static readonly string UIEncounterFleetList = "lstEncounterFleets";
		public static readonly string UIFleetList = "lstFleets";
		public static readonly string UIManualResolveButton = "btnManualResolve";
		public static readonly string UIAutoResolveButton = "btnAutoResolve";
		public static readonly string UIResponseResolveButton = "btnResponseResolve";
		public static readonly string UIAggressiveStanceButton = "btnAggressive";
		public static readonly string UIPassiveStanceButton = "btnPassive";
		public static readonly Dictionary<string, string> ResolutionIcons = new Dictionary<string, string>()
	{
	  {
		EncounterDialog.UIManualResolveButton,
		"imgManualResolve"
	  },
	  {
		EncounterDialog.UIAutoResolveButton,
		"imgAutoResolve"
	  },
	  {
		EncounterDialog.UIResponseResolveButton,
		"imgResponseResolve"
	  }
	};
		public static readonly string UIEncounters = "lstEncounters";
		public static readonly string UIEncounterSystemName = "lblSystemName";
		public static readonly string UIStartCombatButton = "btnStartCombat";
		public static StarMap _starmap = (StarMap)null;
		private List<PendingCombat> _pendingCombats = new List<PendingCombat>();
		private List<EncounterUIContainer> _EncounterUI = new List<EncounterUIContainer>();
		private List<SystemWidget> _systemWidgets = new List<SystemWidget>();
		private bool _visible = true;
		private PendingCombat _selectedCombat;
		private FleetWidget _preCombatLocalFleetWidget;
		private FleetWidget _preCombatEncounterFleetWidget;
		private FleetWidget _postCombatLocalFleetWidget;
		private FleetWidget _postCombatEncounterFleetWidget;
		private int _allyIndex;
		private int _enemyIndex;

		public EncounterDialog(App game, List<PendingCombat> PendingCombats)
		  : base(game, "EncounterPopup")
		{
			this._pendingCombats.Clear();
			this._pendingCombats.AddRange((IEnumerable<PendingCombat>)PendingCombats);
			if (this._pendingCombats.Any<PendingCombat>() && EncounterDialog._starmap.Systems != null && EncounterDialog._starmap.Systems.Reverse.ContainsKey(this._pendingCombats.First<PendingCombat>().SystemID))
				this._app.GetGameState<StarMapState>().StarMap.SetFocus((IGameObject)EncounterDialog._starmap.Systems.Reverse[this._pendingCombats.First<PendingCombat>().SystemID]);
			game.HotKeyManager.SetEnabled(false);
		}

		private void SetResolveType(string panel, ResolutionType type)
		{
			this.HideResolveImages(panel);
			if (!this._selectedCombat.CombatResolutionSelections.ContainsKey(this._app.LocalPlayer.ID))
				this._selectedCombat.CombatResolutionSelections.Add(this._app.LocalPlayer.ID, type);
			else
				this._selectedCombat.CombatResolutionSelections[this._app.LocalPlayer.ID] = type;
			switch (type)
			{
				case ResolutionType.FIGHT:
					this._app.UI.SetVisible(this._app.UI.Path(panel, EncounterDialog.ResolutionIcons[EncounterDialog.UIManualResolveButton]), true);
					break;
				case ResolutionType.AUTO_RESOLVE:
					this._app.UI.SetVisible(this._app.UI.Path(panel, EncounterDialog.ResolutionIcons[EncounterDialog.UIAutoResolveButton]), true);
					break;
				case ResolutionType.FIGHT_ON_FIGHT:
					this._app.UI.SetVisible(this._app.UI.Path(panel, EncounterDialog.ResolutionIcons[EncounterDialog.UIResponseResolveButton]), true);
					break;
			}
			this.UpdateCombatListResolveButtons(this._EncounterUI.FirstOrDefault<EncounterUIContainer>((Func<EncounterUIContainer, bool>)(x => x._combat.ConflictID == this._selectedCombat.ConflictID)), type);
		}

		private void HideResolveImages(string panel)
		{
			foreach (string str in EncounterDialog.ResolutionIcons.Values)
				this._app.UI.SetVisible(this._app.UI.Path(panel, str), false);
		}

		private void SetStanceType(string panel, AutoResolveStance stance)
		{
			if (!this._selectedCombat.CombatStanceSelections.ContainsKey(this._app.LocalPlayer.ID))
				this._selectedCombat.CombatStanceSelections.Add(this._app.LocalPlayer.ID, stance);
			else
				this._selectedCombat.CombatStanceSelections[this._app.LocalPlayer.ID] = stance;
			switch (stance)
			{
				case AutoResolveStance.PASSIVE:
					this._app.UI.SetChecked(this._app.UI.Path(panel, EncounterDialog.UIAggressiveStanceButton), false);
					this._app.UI.SetChecked(this._app.UI.Path(panel, EncounterDialog.UIPassiveStanceButton), true);
					break;
				case AutoResolveStance.AGGRESSIVE:
					this._app.UI.SetChecked(this._app.UI.Path(panel, EncounterDialog.UIPassiveStanceButton), false);
					this._app.UI.SetChecked(this._app.UI.Path(panel, EncounterDialog.UIAggressiveStanceButton), true);
					break;
			}
		}

		protected override void OnUpdate()
		{
			this._app.HotKeyManager.SetEnabled(false);
			if (this._app.CurrentState.Name != "StarMapState")
			{
				if (this._visible)
				{
					this.SetVisible(false);
					this._app.UI.Send((object)"PopFocus", (object)this.ID);
					this._visible = false;
				}
			}
			else if (!this._visible)
			{
				this.SetVisible(true);
				this._app.UI.Send((object)"PushFocus", (object)this.ID);
				this._visible = true;
			}
			this._app.GetGameState<StarMapState>().ShowInterface = !this._visible;
			foreach (SystemWidget systemWidget in this._systemWidgets)
				systemWidget.Update();
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (!(msgType == "dialog_opened"))
			{
				if (msgType == "list_sel_changed")
				{
					if (panelName == EncounterDialog.UIEncounters)
					{
						this._selectedCombat = this._pendingCombats.First<PendingCombat>((Func<PendingCombat, bool>)(x => x.SystemID == int.Parse(msgParams[0])));
						if (EncounterDialog._starmap != null && EncounterDialog._starmap.Systems.Reverse.ContainsKey(this._selectedCombat.SystemID))
						{
							EncounterDialog._starmap.SetFocus((IGameObject)EncounterDialog._starmap.Systems.Reverse[this._selectedCombat.SystemID]);
							EncounterDialog._starmap.Select((IGameObject)EncounterDialog._starmap.Systems.Reverse[this._selectedCombat.SystemID]);
						}
						this._allyIndex = 0;
						this._enemyIndex = 0;
						this.SyncEncounterPopup(false, false);
					}
				}
				else if (msgType == "button_clicked")
				{
					if (panelName == EncounterDialog.UIManualResolveButton)
						this.SetResolveType(this.ID, ResolutionType.FIGHT);
					else if (panelName == EncounterDialog.UIAutoResolveButton)
						this.SetResolveType(this.ID, ResolutionType.AUTO_RESOLVE);
					else if (panelName == EncounterDialog.UIResponseResolveButton)
						this.SetResolveType(this.ID, ResolutionType.FIGHT_ON_FIGHT);
					else if (panelName == EncounterDialog.UIAggressiveStanceButton)
						this.SetStanceType(this.ID, AutoResolveStance.AGGRESSIVE);
					else if (panelName == EncounterDialog.UIPassiveStanceButton)
						this.SetStanceType(this.ID, AutoResolveStance.PASSIVE);
					else if (panelName == "btnCombatManager")
						this._app.SwitchGameState<DefenseManagerState>((object)this._selectedCombat.SystemID);
					else if (panelName == EncounterDialog.UIStartCombatButton)
					{
						this._app.GetGameState<StarMapState>().ShowInterface = true;
						this._app.UI.CloseDialog((Dialog)this, true);
						if (this._app.Game.GetPendingCombats().Any<PendingCombat>((Func<PendingCombat, bool>)(x => x.CombatResults == null)))
						{
							this._app.Game.OrderCombatsByResponse();
							if (this._app.GameSetup.IsMultiplayer)
								this._app.Network.SendCombatResponses((IEnumerable<PendingCombat>)this._pendingCombats, this._app.LocalPlayer.ID);
							else
								this._app.Game.LaunchNextCombat();
						}
						else if (!this._app.GameSetup.IsMultiplayer)
						{
							this._app.Game.GetPendingCombats().Clear();
							this._app.Game.NextTurn();
						}
					}
					else if (panelName == EncounterDialog.UIPreviousAlly)
					{
						--this._allyIndex;
						if (this._selectedCombat.CombatResults != null)
							this.SyncPostCombatPanel(this._app, EncounterDialog.UIPostCombatSummaryPanel, this._selectedCombat, false, false);
						else
							this.SyncEncounterPanel(this._app, EncounterDialog.UIEncounterSummaryPanel, this._selectedCombat);
					}
					else if (panelName == EncounterDialog.UINextAlly)
					{
						++this._allyIndex;
						if (this._selectedCombat.CombatResults != null)
							this.SyncPostCombatPanel(this._app, EncounterDialog.UIPostCombatSummaryPanel, this._selectedCombat, false, false);
						else
							this.SyncEncounterPanel(this._app, EncounterDialog.UIEncounterSummaryPanel, this._selectedCombat);
					}
					else if (panelName == EncounterDialog.UIPreviousEnemy)
					{
						--this._enemyIndex;
						if (this._selectedCombat.CombatResults != null)
							this.SyncPostCombatPanel(this._app, EncounterDialog.UIPostCombatSummaryPanel, this._selectedCombat, false, false);
						else
							this.SyncEncounterPanel(this._app, EncounterDialog.UIEncounterSummaryPanel, this._selectedCombat);
					}
					else if (panelName == EncounterDialog.UINextEnemy)
					{
						++this._enemyIndex;
						if (this._selectedCombat.CombatResults != null)
							this.SyncPostCombatPanel(this._app, EncounterDialog.UIPostCombatSummaryPanel, this._selectedCombat, false, false);
						else
							this.SyncEncounterPanel(this._app, EncounterDialog.UIEncounterSummaryPanel, this._selectedCombat);
					}
					else if (this._EncounterUI.FirstOrDefault<EncounterUIContainer>((Func<EncounterUIContainer, bool>)(x => x.SystemButtonID == panelName))._panels != null)
					{
						EncounterUIContainer encounterUiContainer1 = this._EncounterUI.FirstOrDefault<EncounterUIContainer>((Func<EncounterUIContainer, bool>)(x =>
					   {
						   if (x.SystemButtonID == panelName)
							   return x.NumEnounters == 1;
						   return false;
					   }));
						if (this._selectedCombat.SystemID == encounterUiContainer1._combat.SystemID)
						{
							int targetcombat = this._EncounterUI.FirstOrDefault<EncounterUIContainer>((Func<EncounterUIContainer, bool>)(x => x._combat == this._selectedCombat)).NumEnounters + 1;
							if (targetcombat > 3)
								targetcombat = 1;
							EncounterUIContainer encounterUiContainer2 = this._EncounterUI.FirstOrDefault<EncounterUIContainer>((Func<EncounterUIContainer, bool>)(x =>
						   {
							   if (x._combat.SystemID == this._selectedCombat.SystemID)
								   return x.NumEnounters == targetcombat;
							   return false;
						   }));
							if (encounterUiContainer2._panels == null)
								encounterUiContainer2 = encounterUiContainer1;
							if (this._selectedCombat != encounterUiContainer2._combat)
							{
								this._selectedCombat = encounterUiContainer2._combat;
								if (EncounterDialog._starmap != null && EncounterDialog._starmap.Systems.Reverse.ContainsKey(this._selectedCombat.SystemID))
								{
									EncounterDialog._starmap.SetFocus((IGameObject)EncounterDialog._starmap.Systems.Reverse[this._selectedCombat.SystemID]);
									EncounterDialog._starmap.Select((IGameObject)EncounterDialog._starmap.Systems.Reverse[this._selectedCombat.SystemID]);
								}
								this._allyIndex = 0;
								this._enemyIndex = 0;
								this.SyncEncounterPopup(false, false);
							}
						}
						else if (this._selectedCombat != encounterUiContainer1._combat)
						{
							this._selectedCombat = encounterUiContainer1._combat;
							if (EncounterDialog._starmap != null && EncounterDialog._starmap.Systems.Reverse.ContainsKey(this._selectedCombat.SystemID))
							{
								EncounterDialog._starmap.SetFocus((IGameObject)EncounterDialog._starmap.Systems.Reverse[this._selectedCombat.SystemID]);
								EncounterDialog._starmap.Select((IGameObject)EncounterDialog._starmap.Systems.Reverse[this._selectedCombat.SystemID]);
							}
							this._allyIndex = 0;
							this._enemyIndex = 0;
							this.SyncEncounterPopup(false, false);
						}
					}
				}
			}
			foreach (EncounterUIContainer encounterUiContainer in this._EncounterUI)
			{
				if (encounterUiContainer._panels != null)
					PanelBinding.TryPanelMessage((IEnumerable<PanelBinding>)encounterUiContainer._panels, panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self);
			}
		}

		public override void Initialize()
		{
			base.Initialize();
			foreach (PendingCombat pendingCombat in this._pendingCombats)
			{
				if (!pendingCombat.CombatResolutionSelections.ContainsKey(this._app.LocalPlayer.ID))
					pendingCombat.CombatResolutionSelections.Add(this._app.LocalPlayer.ID, ResolutionType.FIGHT);
				if (!pendingCombat.CombatStanceSelections.ContainsKey(this._app.LocalPlayer.ID))
					pendingCombat.CombatStanceSelections.Add(this._app.LocalPlayer.ID, AutoResolveStance.AGGRESSIVE);
				if (!pendingCombat.SelectedPlayerFleets.ContainsKey(this._app.LocalPlayer.ID))
				{
					int num = 0;
					List<FleetInfo> list = this._app.GameDatabase.GetFleetsByPlayerAndSystem(this._app.LocalPlayer.ID, pendingCombat.SystemID, FleetType.FL_NORMAL).ToList<FleetInfo>();
					if (list != null && list.Count > 0)
						num = list.First<FleetInfo>().ID;
					pendingCombat.SelectedPlayerFleets.Add(this._app.LocalPlayer.ID, num);
				}
			}
			this._preCombatLocalFleetWidget = new FleetWidget(this._app, this._app.UI.Path(EncounterDialog.UIEncounterSummaryPanel, EncounterDialog.UILocalFleets, EncounterDialog.UILocalFleetList));
			this._preCombatLocalFleetWidget.EnableAdmiralButton = false;
			this._preCombatLocalFleetWidget.EnableRightClick = false;
			this._preCombatEncounterFleetWidget = new FleetWidget(this._app, this._app.UI.Path(EncounterDialog.UIEncounterSummaryPanel, EncounterDialog.UIEncounterFleets, EncounterDialog.UIEncounterFleetList));
			this._preCombatEncounterFleetWidget.SetEnabled(false);
			this._preCombatEncounterFleetWidget.ShowPiracyFleets = true;
			this._preCombatEncounterFleetWidget.EnableRightClick = false;
			this._preCombatEncounterFleetWidget.EnableAdmiralButton = false;
			this._postCombatLocalFleetWidget = new FleetWidget(this._app, this._app.UI.Path(EncounterDialog.UIPostCombatSummaryPanel, EncounterDialog.UILocalFleets, EncounterDialog.UIFleetList));
			this._postCombatLocalFleetWidget.EnableAdmiralButton = false;
			this._postCombatLocalFleetWidget.EnableRightClick = false;
			this._postCombatEncounterFleetWidget = new FleetWidget(this._app, this._app.UI.Path(EncounterDialog.UIPostCombatSummaryPanel, EncounterDialog.UIEncounterFleets, EncounterDialog.UIFleetList));
			this._postCombatEncounterFleetWidget.SetEnabled(false);
			this._postCombatEncounterFleetWidget.ShowPiracyFleets = true;
			this._postCombatEncounterFleetWidget.EnableRightClick = false;
			this._postCombatEncounterFleetWidget.EnableAdmiralButton = false;
			this.SyncEncounterPopup(true, true);
		}

		public override string[] CloseDialog()
		{
			foreach (SystemWidget systemWidget in this._systemWidgets)
				systemWidget.Terminate();
			if (this._selectedCombat.FleetIDs.Contains(this._preCombatLocalFleetWidget.SelectedFleet))
				this._selectedCombat.SelectedPlayerFleets[this._app.LocalPlayer.ID] = this._preCombatLocalFleetWidget.SelectedFleet;
			this._postCombatEncounterFleetWidget.Dispose();
			this._postCombatLocalFleetWidget.Dispose();
			this._preCombatEncounterFleetWidget.Dispose();
			this._preCombatLocalFleetWidget.Dispose();
			return (string[])null;
		}

		public void UpdateCombatListResolveButtons(EncounterUIContainer cont, ResolutionType type)
		{
			cont._combat.CombatResolutionSelections[this._app.LocalPlayer.ID] = type;
			foreach (Button panel in cont._panels)
			{
				if (panel.ID.Contains("Stance"))
					this._app.UI.SetVisible(panel.ID, false);
			}
			switch (cont._combat.CombatResolutionSelections[this._app.LocalPlayer.ID])
			{
				case ResolutionType.FIGHT:
					this._app.UI.SetVisible(((IEnumerable<PanelBinding>)cont._panels).FirstOrDefault<PanelBinding>((Func<PanelBinding, bool>)(x => x.ID.Contains("Stancem"))).ID, true);
					break;
				case ResolutionType.AUTO_RESOLVE:
					this._app.UI.SetVisible(((IEnumerable<PanelBinding>)cont._panels).FirstOrDefault<PanelBinding>((Func<PanelBinding, bool>)(x => x.ID.Contains("Stancea"))).ID, true);
					break;
				case ResolutionType.FIGHT_ON_FIGHT:
					this._app.UI.SetVisible(((IEnumerable<PanelBinding>)cont._panels).FirstOrDefault<PanelBinding>((Func<PanelBinding, bool>)(x => x.ID.Contains("Stancer"))).ID, true);
					break;
			}
		}

		private void CycleStanceOnCombat(object sender, EventArgs e)
		{
			EncounterUIContainer cont = this._EncounterUI.FirstOrDefault<EncounterUIContainer>((Func<EncounterUIContainer, bool>)(x => ((IEnumerable<PanelBinding>)x._panels).FirstOrDefault<PanelBinding>((Func<PanelBinding, bool>)(k => k.ID == ((PanelBinding)sender).ID)) != null));
			if (!cont._combat.CombatResolutionSelections.ContainsKey(this._app.LocalPlayer.ID))
			{
				cont._combat.CombatResolutionSelections.Add(this._app.LocalPlayer.ID, ResolutionType.FIGHT);
			}
			else
			{
				switch (cont._combat.CombatResolutionSelections[this._app.LocalPlayer.ID])
				{
					case ResolutionType.FIGHT:
						cont._combat.CombatResolutionSelections[this._app.LocalPlayer.ID] = ResolutionType.AUTO_RESOLVE;
						break;
					case ResolutionType.AUTO_RESOLVE:
						cont._combat.CombatResolutionSelections[this._app.LocalPlayer.ID] = ResolutionType.FIGHT_ON_FIGHT;
						break;
					case ResolutionType.FIGHT_ON_FIGHT:
						cont._combat.CombatResolutionSelections[this._app.LocalPlayer.ID] = ResolutionType.FIGHT;
						break;
				}
			}
			this.UpdateCombatListResolveButtons(cont, cont._combat.CombatResolutionSelections[this._app.LocalPlayer.ID]);
			if (this._selectedCombat != cont._combat)
				return;
			this.SetResolveType(this.ID, cont._combat.CombatResolutionSelections[this._app.LocalPlayer.ID]);
		}

		private void SelectCombat(object sender, EventArgs e)
		{
			this._selectedCombat = this._EncounterUI.FirstOrDefault<EncounterUIContainer>((Func<EncounterUIContainer, bool>)(x => ((IEnumerable<PanelBinding>)x._panels).FirstOrDefault<PanelBinding>((Func<PanelBinding, bool>)(k => k.ID == ((PanelBinding)sender).ID)) != null))._combat;
			if (EncounterDialog._starmap != null && EncounterDialog._starmap.Systems.Reverse.ContainsKey(this._selectedCombat.SystemID))
			{
				EncounterDialog._starmap.SetFocus((IGameObject)EncounterDialog._starmap.Systems.Reverse[this._selectedCombat.SystemID]);
				EncounterDialog._starmap.Select((IGameObject)EncounterDialog._starmap.Systems.Reverse[this._selectedCombat.SystemID]);
			}
			this._allyIndex = 0;
			this._enemyIndex = 0;
			this.SyncEncounterPopup(false, false);
		}

		public void SyncEncounterList(App game, string panelName, List<PendingCombat> pendingCombats)
		{
			game.UI.ClearItems(EncounterDialog.UIEncounters);
			foreach (PendingCombat pendingCombat in pendingCombats)
			{
				PendingCombat pc = pendingCombat;
				if (pc.PlayersInCombat.Contains(this._app.LocalPlayer.ID))
				{
					EncounterUIContainer encounterUiContainer1 = new EncounterUIContainer();
					EncounterUIContainer encounterUiContainer2 = this._EncounterUI.FirstOrDefault<EncounterUIContainer>((Func<EncounterUIContainer, bool>)(x => x._combat.SystemID == pc.SystemID));
					if (encounterUiContainer2._panels == null)
					{
						game.UI.AddItem(game.UI.Path(panelName, EncounterDialog.UIInhabitantsPanel, EncounterDialog.UIInhabitantsList), "", pc.SystemID, "");
						string itemGlobalId = this._app.UI.GetItemGlobalID(game.UI.Path(panelName, EncounterDialog.UIInhabitantsPanel, EncounterDialog.UIInhabitantsList), "", pc.SystemID, "");
						StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(pc.SystemID);
						if (starSystemInfo == (StarSystemInfo)null || starSystemInfo.IsDeepSpace)
						{
							this._app.UI.SetVisible(this._app.UI.GetGlobalID(game.UI.Path(itemGlobalId, "systemDeepspace")), true);
							this._app.UI.SetText(game.UI.Path(itemGlobalId, "title"), starSystemInfo.Name);
						}
						else
						{
							this._systemWidgets.Add(new SystemWidget(this._app, itemGlobalId));
							this._systemWidgets.Last<SystemWidget>().Sync(pc.SystemID);
							HomeworldInfo homeworldInfo = this._app.GameDatabase.GetHomeworlds().ToList<HomeworldInfo>().FirstOrDefault<HomeworldInfo>((Func<HomeworldInfo, bool>)(x => x.SystemID == pc.SystemID));
							int? systemOwningPlayer = this._app.GameDatabase.GetSystemOwningPlayer(pc.SystemID);
							PlayerInfo Owner = this._app.GameDatabase.GetPlayerInfo(systemOwningPlayer.HasValue ? systemOwningPlayer.Value : 0);
							if (homeworldInfo != null && homeworldInfo.SystemID != 0)
							{
								string globalId = this._app.UI.GetGlobalID(game.UI.Path(itemGlobalId, "systemHome"));
								this._app.UI.SetVisible(globalId, true);
								this._app.UI.SetPropertyColor(globalId, "color", this._app.GameDatabase.GetPlayerInfo(homeworldInfo.PlayerID).PrimaryColor * (float)byte.MaxValue);
							}
							else if (Owner != null && game.GameDatabase.GetProvinceInfos().Where<ProvinceInfo>((Func<ProvinceInfo, bool>)(x =>
						   {
							   if (x.CapitalSystemID != pc.SystemID || x.PlayerID != Owner.ID)
								   return false;
							   int capitalSystemId = x.CapitalSystemID;
							   int? homeworld = Owner.Homeworld;
							   if (capitalSystemId == homeworld.GetValueOrDefault())
								   return !homeworld.HasValue;
							   return true;
						   })).Any<ProvinceInfo>())
							{
								string globalId = this._app.UI.GetGlobalID(game.UI.Path(itemGlobalId, "systemCapital"));
								this._app.UI.SetVisible(globalId, true);
								this._app.UI.SetPropertyColor(globalId, "color", Owner.PrimaryColor * (float)byte.MaxValue);
							}
							else
							{
								string globalId = this._app.UI.GetGlobalID(game.UI.Path(itemGlobalId, "systemOwnership"));
								this._app.UI.SetVisible(globalId, true);
								if (Owner != null)
									this._app.UI.SetPropertyColor(globalId, "color", Owner.PrimaryColor * (float)byte.MaxValue);
							}
						}
						encounterUiContainer1.CombatListID = game.UI.Path(itemGlobalId, "lstcombatinstances");
						encounterUiContainer1.SystemItemID = itemGlobalId;
						encounterUiContainer1.NumEnounters = pc.CardID;
						this._app.UI.SetPropertyString(game.UI.Path(itemGlobalId, "systemButton"), "id", "SelectSystem:" + pc.SystemID.ToString());
						encounterUiContainer1.SystemButtonID = "SelectSystem:" + pc.SystemID.ToString();
					}
					else
					{
						encounterUiContainer1.CombatListID = encounterUiContainer2.CombatListID;
						encounterUiContainer1.SystemItemID = encounterUiContainer2.SystemItemID;
						encounterUiContainer1.SystemButtonID = encounterUiContainer2.SystemItemID;
						encounterUiContainer1.NumEnounters = pc.CardID;
					}
					game.UI.AddItem(encounterUiContainer1.CombatListID, "", 999 * encounterUiContainer1.NumEnounters, "");
					string itemGlobalId1 = this._app.UI.GetItemGlobalID(game.UI.Path(encounterUiContainer1.CombatListID, "lstcombatinstances"), "", 999 * encounterUiContainer1.NumEnounters, "");
					string globalId1 = this._app.UI.GetGlobalID(game.UI.Path(itemGlobalId1, "btnManualResolve"));
					string globalId2 = this._app.UI.GetGlobalID(game.UI.Path(itemGlobalId1, "btnAutoResolve"));
					string globalId3 = this._app.UI.GetGlobalID(game.UI.Path(itemGlobalId1, "btnEnemyResolve"));
					encounterUiContainer1.InstancePanel = itemGlobalId1;
					Button button1 = new Button(game.UI, globalId1, null);
					button1.SetID("Stancem(sid:" + pc.SystemID.ToString() + "xcid:" + encounterUiContainer1.NumEnounters.ToString() + ")");
					button1.Clicked += new EventHandler(this.CycleStanceOnCombat);
					this._app.UI.SetPropertyString(globalId1, "id", "Stancem(sid:" + pc.SystemID.ToString() + "xcid:" + encounterUiContainer1.NumEnounters.ToString() + ")");
					Button button2 = new Button(game.UI, globalId2, null);
					button2.SetID("Stancea(sid:" + pc.SystemID.ToString() + "xcid:" + encounterUiContainer1.NumEnounters.ToString() + ")");
					button2.Clicked += new EventHandler(this.CycleStanceOnCombat);
					this._app.UI.SetPropertyString(globalId2, "id", "Stancea(sid:" + pc.SystemID.ToString() + "xcid:" + encounterUiContainer1.NumEnounters.ToString() + ")");
					Button button3 = new Button(game.UI, globalId3, null);
					button3.SetID("Stancer(sid:" + pc.SystemID.ToString() + "xcid:" + encounterUiContainer1.NumEnounters.ToString() + ")");
					button3.Clicked += new EventHandler(this.CycleStanceOnCombat);
					this._app.UI.SetPropertyString(globalId3, "id", "Stancer(sid:" + pc.SystemID.ToString() + "xcid:" + encounterUiContainer1.NumEnounters.ToString() + ")");
					string globalId4 = this._app.UI.GetGlobalID(game.UI.Path(itemGlobalId1, "CombatSelButton"));
					Button button4 = new Button(game.UI, globalId4, null);
					button4.SetID("Combat(sid:" + pc.SystemID.ToString() + "xcid:" + encounterUiContainer1.NumEnounters.ToString() + ")");
					button4.Clicked += new EventHandler(this.SelectCombat);
					this._app.UI.SetPropertyString(globalId4, "id", "Combat(sid:" + pc.SystemID.ToString() + "xcid:" + encounterUiContainer1.NumEnounters.ToString() + ")");
					game.UI.SetText(game.UI.Path(itemGlobalId1, "combatnum"), encounterUiContainer1.NumEnounters.ToString());
					encounterUiContainer1._combat = pc;
					encounterUiContainer1._panels = new PanelBinding[4]
					{
			(PanelBinding) button1,
			(PanelBinding) button2,
			(PanelBinding) button3,
			(PanelBinding) button4
					};
					this._EncounterUI.Insert(this._EncounterUI.Count, encounterUiContainer1);
				}
			}
			this._selectedCombat = pendingCombats.FirstOrDefault<PendingCombat>((Func<PendingCombat, bool>)(x => x.CombatResults == null));
			if (this._selectedCombat == null)
				this._selectedCombat = pendingCombats.Last<PendingCombat>();
			game.UI.SetSelection(EncounterDialog.UIEncounters, this._selectedCombat.SystemID);
		}

		public void OnLocalFleetChanged(App game, int selectedFleet)
		{
			List<int> list = this._selectedCombat.PlayersInCombat.Where<int>((Func<int, bool>)(x =>
		   {
			   if (x != game.Game.LocalPlayer.ID)
				   return game.GameDatabase.GetDiplomacyStateBetweenPlayers(game.Game.LocalPlayer.ID, x) == DiplomacyState.ALLIED;
			   return true;
		   })).ToList<int>();
			int? nullable1 = new int?();
			if (list.Count > 1)
			{
				this._allyIndex = Math.Min(Math.Max(this._allyIndex, 0), list.Count);
				int? nullable2;
				if (this._allyIndex != 0)
				{
					int? nullable3 = new int?(this._allyIndex);
					nullable2 = nullable3.HasValue ? new int?(nullable3.GetValueOrDefault() - 1) : new int?();
				}
				else
					nullable2 = new int?();
				nullable1 = nullable2;
			}
			else
				nullable1 = new int?(0);
			if (nullable1.HasValue && list[nullable1.Value] != game.LocalPlayer.ID)
				return;
			this._selectedCombat.SelectedPlayerFleets[game.LocalPlayer.ID] = selectedFleet;
		}

		public void SyncEncounterPanel(App game, string panelName, PendingCombat pendingCombat)
		{
			if (!pendingCombat.PlayersInCombat.Contains(this._app.LocalPlayer.ID))
			{
				this._selectedCombat = this._pendingCombats.First<PendingCombat>((Func<PendingCombat, bool>)(x => x.PlayersInCombat.Contains(this._app.LocalPlayer.ID)));
				pendingCombat = this._selectedCombat;
			}
			foreach (EncounterUIContainer encounterUiContainer in this._EncounterUI)
			{
				this._app.UI.SetVisible(game.UI.Path(encounterUiContainer.SystemItemID, "selectionoverlay"), false);
				this._app.UI.SetVisible(game.UI.Path(encounterUiContainer.InstancePanel, "selectionoverlay"), false);
			}
			EncounterUIContainer encounterUiContainer1 = this._EncounterUI.FirstOrDefault<EncounterUIContainer>((Func<EncounterUIContainer, bool>)(x => x._combat == pendingCombat));
			if (encounterUiContainer1._panels != null)
			{
				this._app.UI.SetVisible(game.UI.Path(encounterUiContainer1.SystemItemID, "selectionoverlay"), true);
				this._app.UI.SetVisible(game.UI.Path(encounterUiContainer1.InstancePanel, "selectionoverlay"), true);
			}
			List<int> AlliedPlayers = pendingCombat.PlayersInCombat.Where<int>((Func<int, bool>)(x =>
		   {
			   if (x != game.Game.LocalPlayer.ID)
				   return game.GameDatabase.GetDiplomacyStateBetweenPlayers(game.Game.LocalPlayer.ID, x) == DiplomacyState.ALLIED;
			   return true;
		   })).ToList<int>();
			if (pendingCombat.Type == CombatType.CT_Piracy)
			{
				foreach (int fleetId in pendingCombat.FleetIDs)
				{
					FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetId);
					MissionInfo missionByFleetId = game.GameDatabase.GetMissionByFleetID(fleetInfo.ID);
					if (missionByFleetId != null && missionByFleetId.Type == MissionType.PIRACY)
					{
						if (fleetInfo.PlayerID == game.LocalPlayer.ID)
						{
							AlliedPlayers.Clear();
							AlliedPlayers.Add(game.Game.LocalPlayer.ID);
						}
						else
							AlliedPlayers.Remove(fleetInfo.PlayerID);
					}
				}
			}
			List<int> EnemyPlayers = pendingCombat.PlayersInCombat.Where<int>((Func<int, bool>)(x => !AlliedPlayers.Contains(x))).ToList<int>();
			int? selectedAlly = new int?();
			int? selectedEnemy = new int?();
			if (AlliedPlayers.Count > 1)
			{
				this._allyIndex = Math.Min(Math.Max(this._allyIndex, 0), AlliedPlayers.Count);
				int? nullable1;
				if (this._allyIndex != 0)
				{
					int? nullable2 = new int?(this._allyIndex);
					nullable1 = nullable2.HasValue ? new int?(nullable2.GetValueOrDefault() - 1) : new int?();
				}
				else
					nullable1 = new int?();
				selectedAlly = nullable1;
				this._app.UI.SetEnabled(game.UI.Path(panelName, EncounterDialog.UINextAlly), (this._allyIndex < AlliedPlayers.Count ? 1 : 0) != 0);
				this._app.UI.SetEnabled(game.UI.Path(panelName, EncounterDialog.UIPreviousAlly), (this._allyIndex >= 1 ? 1 : 0) != 0);
			}
			else
			{
				selectedAlly = new int?(0);
				this._app.UI.SetEnabled(game.UI.Path(panelName, EncounterDialog.UINextAlly), false);
				this._app.UI.SetEnabled(game.UI.Path(panelName, EncounterDialog.UIPreviousAlly), false);
			}
			this._app.UI.SetVisible(game.UI.Path(panelName, EncounterDialog.UINextAlly), (AlliedPlayers.Count > 0 ? 1 : 0) != 0);
			this._app.UI.SetVisible(game.UI.Path(panelName, EncounterDialog.UIPreviousAlly), (AlliedPlayers.Count > 0 ? 1 : 0) != 0);
			if (EnemyPlayers.Count > 0)
			{
				if (EnemyPlayers.Count > 1)
				{
					this._enemyIndex = Math.Min(Math.Max(this._enemyIndex, 0), EnemyPlayers.Count);
					int? nullable1;
					if (this._enemyIndex != 0)
					{
						int? nullable2 = new int?(this._enemyIndex);
						nullable1 = nullable2.HasValue ? new int?(nullable2.GetValueOrDefault() - 1) : new int?();
					}
					else
						nullable1 = new int?();
					selectedEnemy = nullable1;
					this._app.UI.SetEnabled(game.UI.Path(panelName, EncounterDialog.UINextEnemy), (this._enemyIndex < EnemyPlayers.Count ? 1 : 0) != 0);
					this._app.UI.SetEnabled(game.UI.Path(panelName, EncounterDialog.UIPreviousEnemy), (this._enemyIndex >= 1 ? 1 : 0) != 0);
				}
				else
				{
					selectedEnemy = new int?(0);
					this._app.UI.SetEnabled(game.UI.Path(panelName, EncounterDialog.UINextEnemy), false);
					this._app.UI.SetEnabled(game.UI.Path(panelName, EncounterDialog.UIPreviousEnemy), false);
				}
			}
			this._app.UI.SetVisible(game.UI.Path(panelName, EncounterDialog.UINextEnemy), (EnemyPlayers.Count > 0 ? 1 : 0) != 0);
			this._app.UI.SetVisible(game.UI.Path(panelName, EncounterDialog.UIPreviousEnemy), (EnemyPlayers.Count > 0 ? 1 : 0) != 0);
			if (!selectedAlly.HasValue || AlliedPlayers[selectedAlly.Value] == game.LocalPlayer.ID)
			{
				this._preCombatLocalFleetWidget.OnFleetSelectionChanged += new FleetWidget.FleetSelectionChangedDelegate(this.OnLocalFleetChanged);
				this._preCombatLocalFleetWidget.Selected = pendingCombat.SelectedPlayerFleets[this._app.LocalPlayer.ID];
			}
			else
			{
				this._preCombatLocalFleetWidget.OnFleetSelectionChanged -= new FleetWidget.FleetSelectionChangedDelegate(this.OnLocalFleetChanged);
				this._preCombatLocalFleetWidget.Selected = -1;
			}
			this.SetResolveType(this.ID, pendingCombat.CombatResolutionSelections[game.LocalPlayer.ID]);
			this.SetStanceType(this.ID, pendingCombat.CombatStanceSelections[game.LocalPlayer.ID]);
			StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(pendingCombat.SystemID);
			game.UI.SetVisible(game.UI.Path(this.ID, "systemHomemain"), false);
			game.UI.SetVisible(game.UI.Path(this.ID, "systemCapitalmain"), false);
			game.UI.SetVisible(game.UI.Path(this.ID, "systemOwnershipmain"), false);
			game.UI.SetVisible(game.UI.Path(this.ID, "systemDeepspacemain"), false);
			if (starSystemInfo == (StarSystemInfo)null || starSystemInfo.IsDeepSpace)
			{
				game.UI.SetVisible(game.UI.Path(this.ID, "btnCombatManager"), false);
				game.UI.SetVisible(game.UI.Path(this.ID, "systemDeepspacemain"), true);
			}
			else
			{
				int? systemOwningPlayer = game.GameDatabase.GetSystemOwningPlayer(pendingCombat.SystemID);
				PlayerInfo SystemOwner = game.GameDatabase.GetPlayerInfo(systemOwningPlayer.HasValue ? systemOwningPlayer.Value : 0);
				if (SystemOwner != null)
				{
					HomeworldInfo homeworldInfo = this._app.GameDatabase.GetHomeworlds().FirstOrDefault<HomeworldInfo>((Func<HomeworldInfo, bool>)(x =>
				   {
					   if (x.SystemID == pendingCombat.SystemID)
						   return x.PlayerID == SystemOwner.ID;
					   return false;
				   }));
					Vector3 vector3 = SystemOwner.PrimaryColor * (float)byte.MaxValue;
					if (homeworldInfo != null && homeworldInfo.SystemID == pendingCombat.SystemID)
					{
						game.UI.SetVisible(game.UI.Path(this.ID, "systemHomemain"), true);
						game.UI.SetPropertyColor(game.UI.Path(this.ID, "systemHomemain"), "color", vector3);
					}
					else if (game.GameDatabase.GetProvinceInfos().Where<ProvinceInfo>((Func<ProvinceInfo, bool>)(x =>
				   {
					   if (x.PlayerID != SystemOwner.ID || x.CapitalSystemID != pendingCombat.SystemID)
						   return false;
					   int capitalSystemId = x.CapitalSystemID;
					   int? homeworld = SystemOwner.Homeworld;
					   if (capitalSystemId == homeworld.GetValueOrDefault())
						   return !homeworld.HasValue;
					   return true;
				   })).Any<ProvinceInfo>())
					{
						game.UI.SetVisible(game.UI.Path(this.ID, "systemCapitalmain"), true);
						game.UI.SetPropertyColor(game.UI.Path(this.ID, "systemCapitalmain"), "color", vector3);
					}
					else
					{
						game.UI.SetVisible(game.UI.Path(this.ID, "systemOwnershipmain"), true);
						game.UI.SetPropertyColor(game.UI.Path(this.ID, "systemOwnershipmain"), "color", vector3);
					}
				}
				if (game.Game.IsMultiplayer)
				{
					game.UI.SetEnabled(game.UI.Path(this.ID, "btnCombatManager"), false);
					game.UI.SetTooltip(game.UI.Path(this.ID, "btnCombatManager"), "Combat Manager Disabled in Multiplayer");
				}
				else
				{
					game.UI.SetEnabled(game.UI.Path(this.ID, "btnCombatManager"), true);
					game.UI.SetTooltip(game.UI.Path(this.ID, "btnCombatManager"), "");
				}
			}
			List<ColonyInfo> list1 = game.GameDatabase.GetColonyInfosForSystem(pendingCombat.SystemID).ToList<ColonyInfo>();
			List<StationInfo> list2 = game.GameDatabase.GetStationForSystem(pendingCombat.SystemID).ToList<StationInfo>();
			List<int> intList = new List<int>();
			foreach (StationInfo stationInfo in list2)
			{
				if (!intList.Contains(stationInfo.PlayerID))
					intList.Add(stationInfo.PlayerID);
			}
			foreach (ColonyInfo colonyInfo in list1)
			{
				if (!intList.Contains(colonyInfo.PlayerID))
					intList.Add(colonyInfo.PlayerID);
			}
			List<FleetInfo> list3 = game.GameDatabase.GetFleetInfoBySystemID(pendingCombat.SystemID, FleetType.FL_NORMAL).ToList<FleetInfo>();
			StarSystemMapUI.Sync(game, pendingCombat.SystemID, game.UI.Path(panelName, EncounterDialog.UIMiniMapPanel, EncounterDialog.UIMiniMapPart), false);
			if (AlliedPlayers.Count > 1 && !selectedAlly.HasValue)
			{
				StarmapUI.SyncPlayerCard(game, game.UI.Path(panelName, EncounterDialog.UILocalPlayer), App.Localize("@UI_POST_COMBAT_ALLIANCE"), App.Localize("@DIPLO_REACTION_LOVE"), "", "");
				this._preCombatLocalFleetWidget.SetSyncedFleets(list3.Where<FleetInfo>((Func<FleetInfo, bool>)(x =>
			   {
				   if (AlliedPlayers.Contains(x.PlayerID) && this._selectedCombat.FleetIDs.Contains(x.ID))
					   return !Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(game.Game, x);
				   return false;
			   })).ToList<FleetInfo>());
			}
			else
			{
				StarmapUI.SyncPlayerCard(game, game.UI.Path(panelName, EncounterDialog.UILocalPlayer), AlliedPlayers[selectedAlly.Value]);
				this._preCombatLocalFleetWidget.SetSyncedFleets(list3.Where<FleetInfo>((Func<FleetInfo, bool>)(x =>
			   {
				   if (x.PlayerID == AlliedPlayers[selectedAlly.Value] && this._selectedCombat.FleetIDs.Contains(x.ID))
					   return !Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(game.Game, x);
				   return false;
			   })).ToList<FleetInfo>());
			}
			if (EnemyPlayers.Count > 0)
			{
				if (EnemyPlayers.Count > 1 && !selectedEnemy.HasValue)
				{
					StarmapUI.SyncPlayerCard(game, game.UI.Path(panelName, EncounterDialog.UIEncounterPlayer), App.Localize("@UI_POST_COMBAT_ENEMIES"), App.Localize("@DIPLO_REACTION_HATE"), "", "");
					this._preCombatEncounterFleetWidget.SetSyncedFleets(list3.Where<FleetInfo>((Func<FleetInfo, bool>)(x =>
				   {
					   if (EnemyPlayers.Contains(x.PlayerID))
						   return this._selectedCombat.FleetIDs.Contains(x.ID);
					   return false;
				   })).ToList<FleetInfo>());
				}
				else
				{
					int num = EnemyPlayers[selectedEnemy.Value];
					List<int> piracyfleet = new List<int>();
					if (this._selectedCombat.Type == CombatType.CT_Piracy)
					{
						foreach (int fleetId in this._selectedCombat.FleetIDs)
						{
							FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetId);
							if (fleetInfo.PlayerID == num)
							{
								MissionInfo missionByFleetId = game.GameDatabase.GetMissionByFleetID(fleetInfo.ID);
								if (missionByFleetId != null && missionByFleetId.Type == MissionType.PIRACY && !game.GameDatabase.PirateFleetVisibleToPlayer(fleetId, game.LocalPlayer.ID))
								{
									piracyfleet.Add(fleetId);
									break;
								}
							}
						}
					}
					StarmapUI.SyncPlayerCard(game, game.UI.Path(panelName, EncounterDialog.UIEncounterPlayer), !piracyfleet.Any<int>() ? EnemyPlayers[selectedEnemy.Value] : game.Game.ScriptModules.Pirates.PlayerID);
					this._preCombatEncounterFleetWidget.SetSyncedFleets(list3.Where<FleetInfo>((Func<FleetInfo, bool>)(x =>
				   {
					   if (x.PlayerID != EnemyPlayers[selectedEnemy.Value])
						   return false;
					   if (piracyfleet.Any<int>())
						   return piracyfleet.Contains(x.ID);
					   return this._selectedCombat.FleetIDs.Contains(x.ID);
				   })).ToList<FleetInfo>());
				}
			}
			game.UI.SetPropertyString(game.UI.Path(panelName, EncounterDialog.UIEncounterTitle), "text", string.Format("{0} {1}", (object)App.Localize("@UI_STARMAP_ENCOUNTER_AT"), (object)starSystemInfo.Name));
		}

		public void SyncEncounterPopup(bool playSpeech, bool refreshList = true)
		{
			if (refreshList)
				this.SyncEncounterList(this._app, this._app.UI.Path(this.ID, EncounterDialog.UIEncounterListPanel), this._pendingCombats);
			if (this._selectedCombat == null)
			{
				this._selectedCombat = this._pendingCombats.First<PendingCombat>((Func<PendingCombat, bool>)(x => x.PlayersInCombat.Contains(this._app.LocalPlayer.ID)));
				if (EncounterDialog._starmap != null && EncounterDialog._starmap.Systems.Reverse.ContainsKey(this._selectedCombat.SystemID))
				{
					EncounterDialog._starmap.SetFocus((IGameObject)EncounterDialog._starmap.Systems.Reverse[this._selectedCombat.SystemID]);
					EncounterDialog._starmap.Select((IGameObject)EncounterDialog._starmap.Systems.Reverse[this._selectedCombat.SystemID]);
				}
				this._allyIndex = 0;
				this._enemyIndex = 0;
				this.SyncEncounterPopup(false, false);
			}
			if (this._selectedCombat.CombatResults == null)
			{
				string str = this._app.UI.Path(this.ID, EncounterDialog.UIEncounterSummaryPanel);
				this._app.UI.SetVisible(str, true);
				this._app.UI.SetVisible(this._app.UI.Path(this.ID, EncounterDialog.UIPostCombatSummaryPanel), false);
				this.SyncEncounterPanel(this._app, str, this._selectedCombat);
			}
			else
			{
				string str = this._app.UI.Path(this.ID, EncounterDialog.UIPostCombatSummaryPanel);
				this._app.UI.SetVisible(str, true);
				this._app.UI.SetVisible(this._app.UI.Path(this.ID, EncounterDialog.UIEncounterSummaryPanel), false);
				this.SyncPostCombatPanel(this._app, str, this._selectedCombat, playSpeech, refreshList);
			}
		}

		public void SyncWeaponDamageList(
		  App game,
		  string panelName,
		  Dictionary<int, float> damageTable)
		{
			game.UI.ClearItems(panelName);
			int userItemId = 0;
			foreach (KeyValuePair<int, float> keyValuePair in damageTable)
			{
				KeyValuePair<int, float> kvp = keyValuePair;
				string iconSpriteName = game.AssetDatabase.Weapons.First<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.UniqueWeaponID == kvp.Key)).IconSpriteName;
				game.UI.AddItem(panelName, string.Empty, userItemId, string.Empty);
				string itemGlobalId = game.UI.GetItemGlobalID(panelName, string.Empty, userItemId, string.Empty);
				game.UI.SetPropertyString(game.UI.Path(itemGlobalId, EncounterDialog.UIWeaponIcon), "sprite", iconSpriteName);
				game.UI.SetPropertyString(game.UI.Path(itemGlobalId, EncounterDialog.UIWeaponDamage), "text", Math.Floor((double)kvp.Value).ToString("N0"));
				++userItemId;
			}
		}

		public void SyncPostCombatPanel(
		  App game,
		  string panelName,
		  PendingCombat pendingCombat,
		  bool playSpeech,
		  bool postGameEvents = false)
		{
			List<int> AlliedPlayers = pendingCombat.PlayersInCombat.Where<int>((Func<int, bool>)(x =>
		   {
			   if (x != game.Game.LocalPlayer.ID)
				   return game.GameDatabase.GetDiplomacyStateBetweenPlayers(game.Game.LocalPlayer.ID, x) == DiplomacyState.ALLIED;
			   return true;
		   })).ToList<int>();
			List<int> EnemyPlayers = pendingCombat.PlayersInCombat.Where<int>((Func<int, bool>)(x => !AlliedPlayers.Contains(x))).ToList<int>();
			PostCombatData combatResults = pendingCombat.CombatResults;
			combatResults.SystemId = new int?(pendingCombat.SystemID);
			StarSystemMapUI.Sync(game, pendingCombat.SystemID, game.UI.Path(panelName, "partMiniSystem"), false);
			List<Player> list = GameSession.GetPlayersWithCombatAssets(game, pendingCombat.SystemID).ToList<Player>();
			bool flag1 = list.Any<Player>((Func<Player, bool>)(x => game.GameDatabase.GetDiplomacyStateBetweenPlayers(x.ID, game.LocalPlayer.ID) == DiplomacyState.WAR));
			bool flag2 = list.Any<Player>((Func<Player, bool>)(x =>
		   {
			   if (x.ID != game.LocalPlayer.ID)
				   return game.GameDatabase.GetDiplomacyStateBetweenPlayers(x.ID, game.LocalPlayer.ID) == DiplomacyState.ALLIED;
			   return true;
		   }));
			string propertyValue = "ui\\events\\event_combat_draw.tga";
			string str1 = App.Localize("@UI_POST_COMBAT_DRAW");
			if (flag1 && flag2)
			{
				if (playSpeech)
					this._app.PostRequestSpeech(string.Format("COMBAT_045-01_{0}_BattleIsADraw", (object)this._app.GameDatabase.GetFactionName(this._app.GameDatabase.GetPlayerFactionID(this._app.LocalPlayer.ID))), 50, 120, 0.0f);
				if (postGameEvents)
					game.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_COMBAT_DRAW,
						EventMessage = TurnEventMessage.EM_COMBAT_DRAW,
						FleetID = pendingCombat.SelectedPlayerFleets[game.LocalPlayer.ID],
						PlayerID = game.LocalPlayer.ID,
						SystemID = pendingCombat.SystemID,
						CombatID = pendingCombat.ConflictID,
						TurnNumber = game.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
			}
			else if (flag2)
			{
				if (playSpeech)
				{
					FleetInfo fleetInfo = this._selectedCombat.CombatResults.FleetsInCombat.FirstOrDefault<FleetInfo>((Func<FleetInfo, bool>)(x => x.PlayerID == game.LocalPlayer.ID));
					if (fleetInfo != null)
					{
						this._app.GameDatabase.GetAdmiralInfo(fleetInfo.AdmiralID);
						this._app.PostRequestSpeech(string.Format("COMBAT_043-01_{0}_WinningABattle", (object)this._app.GameDatabase.GetFactionName(this._app.GameDatabase.GetPlayerFactionID(this._app.LocalPlayer.ID))), 50, 120, 0.0f);
					}
				}
				propertyValue = "ui\\events\\event_combat_win.tga";
				str1 = App.Localize("@UI_POST_COMBAT_WIN");
				if (postGameEvents)
					game.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_COMBAT_WIN,
						EventMessage = TurnEventMessage.EM_COMBAT_WIN,
						FleetID = pendingCombat.SelectedPlayerFleets[game.LocalPlayer.ID],
						PlayerID = game.LocalPlayer.ID,
						SystemID = pendingCombat.SystemID,
						CombatID = pendingCombat.ConflictID,
						TurnNumber = game.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
			}
			else
			{
				propertyValue = "ui\\events\\event_combat_lose.tga";
				str1 = App.Localize("@UI_POST_COMBAT_LOSS");
				if (postGameEvents)
					game.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_COMBAT_LOSS,
						EventMessage = TurnEventMessage.EM_COMBAT_LOSS,
						FleetID = pendingCombat.SelectedPlayerFleets[game.LocalPlayer.ID],
						PlayerID = game.LocalPlayer.ID,
						SystemID = pendingCombat.SystemID,
						CombatID = pendingCombat.ConflictID,
						TurnNumber = game.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
			}
			game.UI.SetPropertyString(game.UI.Path(panelName, "imgPostCombatEvent"), "sprite", propertyValue);
			string str2 = combatResults.SystemId.HasValue ? game.GameDatabase.GetStarSystemInfo(combatResults.SystemId.Value).Name : App.Localize("@UI_POST_COMBAT_DEEP_SPACE");
			game.UI.SetPropertyString(game.UI.Path(panelName, EncounterDialog.UICombatHeaderLabel), "text", string.Format("{0} {1} - {2}", (object)App.Localize("@UI_POST_COMBAT_COMBAT_AT"), (object)str2, (object)str1));
			game.UI.ClearItems(game.UI.Path(panelName, EncounterDialog.UIInfoPanel, EncounterDialog.UIInfoList));
			for (int userItemId = 0; userItemId < combatResults.AdditionalInfo.Count; ++userItemId)
			{
				game.UI.AddItem(game.UI.Path(panelName, EncounterDialog.UIInfoPanel, EncounterDialog.UIInfoList), string.Empty, userItemId, string.Empty);
				string itemGlobalId = game.UI.GetItemGlobalID(game.UI.Path(panelName, EncounterDialog.UIInfoPanel, EncounterDialog.UIInfoList), string.Empty, userItemId, string.Empty);
				game.UI.SetPropertyString(itemGlobalId, "text", combatResults.AdditionalInfo[userItemId]);
			}
			int? selectedAlly;
			if (AlliedPlayers.Count > 1)
			{
				this._allyIndex = Math.Min(Math.Max(this._allyIndex, 0), AlliedPlayers.Count);
				selectedAlly = this._allyIndex == 0 ? new int?() : new int?(AlliedPlayers[this._allyIndex - 1]);
				this._app.UI.SetEnabled(game.UI.Path(panelName, EncounterDialog.UINextAlly), (this._allyIndex < AlliedPlayers.Count ? 1 : 0) != 0);
				this._app.UI.SetEnabled(game.UI.Path(panelName, EncounterDialog.UIPreviousAlly), (this._allyIndex >= 1 ? 1 : 0) != 0);
			}
			else
			{
				selectedAlly = new int?(AlliedPlayers.First<int>());
				this._app.UI.SetEnabled(game.UI.Path(panelName, EncounterDialog.UINextAlly), false);
				this._app.UI.SetEnabled(game.UI.Path(panelName, EncounterDialog.UIPreviousAlly), false);
			}
			int? selectedEnemy;
			if (EnemyPlayers.Count > 1)
			{
				this._enemyIndex = Math.Min(Math.Max(this._enemyIndex, 0), EnemyPlayers.Count);
				selectedEnemy = this._enemyIndex == 0 ? new int?() : new int?(EnemyPlayers[this._enemyIndex - 1]);
				this._app.UI.SetEnabled(game.UI.Path(panelName, EncounterDialog.UINextEnemy), (this._enemyIndex < EnemyPlayers.Count ? 1 : 0) != 0);
				this._app.UI.SetEnabled(game.UI.Path(panelName, EncounterDialog.UIPreviousEnemy), (this._enemyIndex >= 1 ? 1 : 0) != 0);
			}
			else
			{
				selectedEnemy = new int?(EnemyPlayers.First<int>());
				this._app.UI.SetEnabled(game.UI.Path(panelName, EncounterDialog.UINextEnemy), false);
				this._app.UI.SetEnabled(game.UI.Path(panelName, EncounterDialog.UIPreviousEnemy), false);
			}
			if (AlliedPlayers.Count > 1 && !selectedAlly.HasValue)
			{
				StarmapUI.SyncPlayerCard(game, game.UI.Path(panelName, EncounterDialog.UIAlliedPlayerCard), App.Localize("@UI_POST_COMBAT_ALLIANCE"), App.Localize("@DIPLO_RELATION_LOVE"), "", "");
				this._postCombatLocalFleetWidget.SetSyncedFleets(combatResults.FleetsInCombat.Where<FleetInfo>((Func<FleetInfo, bool>)(x => AlliedPlayers.Contains(x.PlayerID))).ToList<FleetInfo>());
				Dictionary<int, float> damageTable = new Dictionary<int, float>();
				foreach (KeyValuePair<int, Dictionary<int, float>> keyValuePair1 in combatResults.WeaponDamageTable)
				{
					if (AlliedPlayers.Contains(keyValuePair1.Key))
					{
						foreach (KeyValuePair<int, float> keyValuePair2 in keyValuePair1.Value)
						{
							if (!damageTable.ContainsKey(keyValuePair2.Key))
								damageTable.Add(keyValuePair2.Key, 0.0f);
							Dictionary<int, float> dictionary;
							int key;
							(dictionary = damageTable)[key = keyValuePair2.Key] = dictionary[key] + keyValuePair2.Value;
						}
					}
				}
				this.SyncWeaponDamageList(game, game.UI.Path(panelName, EncounterDialog.UIAlliedDamagePanel, EncounterDialog.UIAlliedDamageList), damageTable);
			}
			else
			{
				int? nullable1 = selectedAlly;
				if ((nullable1.GetValueOrDefault() != 0 ? 1 : (!nullable1.HasValue ? 1 : 0)) != 0)
				{
					StarmapUI.SyncPlayerCard(game, game.UI.Path(panelName, EncounterDialog.UIAlliedPlayerCard), selectedAlly.Value);
					this._postCombatLocalFleetWidget.SetSyncedFleets(combatResults.FleetsInCombat.Where<FleetInfo>((Func<FleetInfo, bool>)(x =>
				   {
					   int playerId = x.PlayerID;
					   int? nullable = selectedAlly;
					   if (playerId == nullable.GetValueOrDefault())
						   return nullable.HasValue;
					   return false;
				   })).ToList<FleetInfo>());
					this.SyncWeaponDamageList(game, game.UI.Path(panelName, EncounterDialog.UIAlliedDamagePanel, EncounterDialog.UIAlliedDamageList), combatResults.WeaponDamageTable[selectedAlly.Value]);
				}
			}
			if (EnemyPlayers.Count > 1 && !selectedEnemy.HasValue)
			{
				StarmapUI.SyncPlayerCard(game, game.UI.Path(panelName, EncounterDialog.UIEnemyPlayerCard), App.Localize("@UI_POST_COMBAT_ENEMIES"), App.Localize("@DIPLO_RELATION_HATE"), "", "");
				this._postCombatEncounterFleetWidget.SetSyncedFleets(combatResults.FleetsInCombat.Where<FleetInfo>((Func<FleetInfo, bool>)(x => EnemyPlayers.Contains(x.PlayerID))).ToList<FleetInfo>());
				Dictionary<int, float> damageTable = new Dictionary<int, float>();
				foreach (KeyValuePair<int, Dictionary<int, float>> keyValuePair1 in combatResults.WeaponDamageTable)
				{
					if (EnemyPlayers.Contains(keyValuePair1.Key))
					{
						foreach (KeyValuePair<int, float> keyValuePair2 in keyValuePair1.Value)
						{
							if (!damageTable.ContainsKey(keyValuePair2.Key))
								damageTable.Add(keyValuePair2.Key, 0.0f);
							Dictionary<int, float> dictionary;
							int key;
							(dictionary = damageTable)[key = keyValuePair2.Key] = dictionary[key] + keyValuePair2.Value;
						}
					}
				}
				this.SyncWeaponDamageList(game, game.UI.Path(panelName, EncounterDialog.UIEnemyDamagePanel, EncounterDialog.UIEnemyDamageList), damageTable);
			}
			else
			{
				int? nullable1 = selectedEnemy;
				if ((nullable1.GetValueOrDefault() != 0 ? 1 : (!nullable1.HasValue ? 1 : 0)) == 0)
					return;
				StarmapUI.SyncPlayerCard(game, game.UI.Path(panelName, EncounterDialog.UIEnemyPlayerCard), selectedEnemy.Value);
				this._postCombatEncounterFleetWidget.SetSyncedFleets(combatResults.FleetsInCombat.Where<FleetInfo>((Func<FleetInfo, bool>)(x =>
			   {
				   int playerId = x.PlayerID;
				   int? nullable = selectedEnemy;
				   if (playerId == nullable.GetValueOrDefault())
					   return nullable.HasValue;
				   return false;
			   })).ToList<FleetInfo>());
				this.SyncWeaponDamageList(game, game.UI.Path(panelName, EncounterDialog.UIEnemyDamagePanel, EncounterDialog.UIEnemyDamageList), combatResults.WeaponDamageTable[selectedEnemy.Value]);
			}
		}
	}
}
