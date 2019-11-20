// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.DiplomacyScreenState
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.UI;
using Kerberos.Sots.UI.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kerberos.Sots.GameStates
{
	internal class DiplomacyScreenState : GameState, IKeyBindListener
	{
		private Dictionary<int, DiplomacyCardState> LastDiplomacyCardState = new Dictionary<int, DiplomacyCardState>();
		private List<int> PlayerSlots = new List<int>();
		private bool _lobbyimprove = true;
		private const string UIBackButton = "btnBackButton";
		private const string UIEmpiresButton = "btnEmpiresButton";
		private const string UIIndependentsButton = "btnIndependentsButton";
		private int _playerId;
		private int _selectedIndy;
		private int _selectedLobbyPlayer;
		private DiplomacyScreenState.DiplomacyMode _mode;

		public DiplomacyScreenState(App game)
		  : base(game)
		{
		}

		protected override void OnPrepare(GameState prev, object[] parms)
		{
			if (this.App.GameDatabase == null)
				this.App.NewGame();
			this.App.UI.LoadScreen("Diplomacy");
		}

		public void SyncPlayerDiplomacyCard(int playerId, bool updateButtonIds)
		{
			DiplomacyUI.SyncPlayerDiplomacyCard(this.App, "Player" + (object)this.PlayerSlots.IndexOf(playerId), playerId, this.LastDiplomacyCardState[playerId], updateButtonIds);
		}

		protected override void OnEnter()
		{
			if (this.App.LocalPlayer == null)
				this.App.NewGame();
			this.App.UI.SetScreen("Diplomacy");
			this.App.UI.SetVisible("noDiploText", false);
			this.PlayerSlots.Clear();
			List<PlayerInfo> list = this.App.GameDatabase.GetPlayerInfos().ToList<PlayerInfo>();
			list.RemoveAll((Predicate<PlayerInfo>)(x => x.ID == this.App.Game.LocalPlayer.ID));
			int num = 0;
			DiplomacyUI.HideAllPlayerDiplomacyCards(this.App);
			this.App.UI.ClearItems("pnlIndyDiplomacy.pnlFactionsList.factionList");
			foreach (PlayerInfo playerInfo in list)
			{
				if (playerInfo.isStandardPlayer)
				{
					if (!this.LastDiplomacyCardState.ContainsKey(playerInfo.ID))
						this.LastDiplomacyCardState.Add(playerInfo.ID, DiplomacyCardState.PlayerStats);
					this.PlayerSlots.Add(playerInfo.ID);
					this.SyncPlayerDiplomacyCard(playerInfo.ID, true);
					bool isEncountered = this.App.GameDatabase.GetDiplomacyInfo(playerInfo.ID, this.App.LocalPlayer.ID).isEncountered;
					this.App.UI.SetVisible("Player" + (object)this.PlayerSlots.IndexOf(playerInfo.ID), isEncountered);
					if (isEncountered)
						++num;
				}
				else if (!playerInfo.isDefeated && playerInfo.includeInDiplomacy && (!this.App.AssetDatabase.GetFaction(playerInfo.FactionID).IsIndependent() || this.App.GameDatabase.GetHasPlayerStudiedIndependentRace(this.App.LocalPlayer.ID, playerInfo.ID)) && this.App.GameDatabase.GetDiplomacyInfo(playerInfo.ID, this.App.LocalPlayer.ID).isEncountered)
				{
					this.App.UI.AddItem("pnlIndyDiplomacy.pnlFactionsList.factionList", string.Empty, playerInfo.ID, string.Empty);
					string itemGlobalId = this.App.UI.GetItemGlobalID("pnlIndyDiplomacy.pnlFactionsList.factionList", string.Empty, playerInfo.ID, string.Empty);
					this.App.UI.SetEnabled(itemGlobalId, false);
					this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "txtInteraction"), "text", playerInfo.Name);
				}
			}
			if (num == 0)
				this.App.UI.SetVisible("noDiploText", true);
			this._selectedIndy = 0;
			this.App.UI.SetVisible("pnlIndyPlayerSummary", false);
			this.App.UI.ClearSelection("factionList");
			this.App.UI.SetPropertyString("Screen_Title", "text", string.Format(App.Localize("@UI_DIPLOMACY_DIPLOMACY"), (object)this.App.GameDatabase.GetPlayerInfo(this.App.Game.LocalPlayer.ID).GenericDiplomacyPoints));
			this.App.HotKeyManager.AddListener((IKeyBindListener)this);
		}

		private int GetPlayerId(string panelName)
		{
			if (this._mode != DiplomacyScreenState.DiplomacyMode.Standard)
				return this._selectedIndy;
			return this.PlayerSlots[int.Parse(panelName.Split('|')[0].Replace("Player", ""))];
		}

		protected override void OnExit(GameState prev, ExitReason reason)
		{
			this.App.HotKeyManager.RemoveListener((IKeyBindListener)this);
		}

		protected override void UICommChannel_OnPanelMessage(
		  string panelName,
		  string msgType,
		  string[] msgParams)
		{
			if (msgType == "dialog_closed")
			{
				if (this._mode == DiplomacyScreenState.DiplomacyMode.Independent)
					DiplomacyUI.SyncIndyDiplomacyCard(this.App, "pnlIndyPlayerSummary", this._selectedIndy);
				else
					this.SyncPlayerDiplomacyCard(this._playerId, false);
			}
			else if (msgType == "list_sel_changed")
			{
				if (panelName == "factionList" && !string.IsNullOrEmpty(msgParams[0]))
				{
					this._selectedIndy = int.Parse(msgParams[0]);
					this.App.UI.SetVisible("pnlIndyPlayerSummary", true);
					DiplomacyUI.SyncIndyDiplomacyCard(this.App, "pnlIndyPlayerSummary", this._selectedIndy);
				}
				else if (panelName == DiplomacyUI.UILobbyPlayerList)
				{
					int playerID = int.Parse(msgParams[0]);
					this._selectedLobbyPlayer = playerID;
					PlayerInfo playerInfo = this.App.GameDatabase.GetPlayerInfo(playerID);
					DiplomaticMood diplomaticMood = this.App.GameDatabase.GetDiplomacyInfo(playerInfo.ID, this._playerId).GetDiplomaticMood();
					this.App.UI.SetVisible(this.App.UI.Path(DiplomacyUI.UILobbyPanel, "imgOtherRelation"), true);
					switch (diplomaticMood)
					{
						case DiplomaticMood.Hatred:
							this.App.UI.SetPropertyString(this.App.UI.Path(DiplomacyUI.UILobbyPanel, "imgOtherRelation"), "sprite", "Hate");
							break;
						case DiplomaticMood.Love:
							this.App.UI.SetPropertyString(this.App.UI.Path(DiplomacyUI.UILobbyPanel, "imgOtherRelation"), "sprite", "Love");
							break;
						default:
							this.App.UI.SetVisible(this.App.UI.Path(DiplomacyUI.UILobbyPanel, "imgOtherRelation"), false);
							break;
					}
					this.App.UI.SetPropertyString(this.App.UI.Path(DiplomacyUI.UILobbyPanel, "imgOtherAvatar"), "sprite", Path.GetFileNameWithoutExtension(playerInfo.AvatarAssetPath));
					this.App.UI.SetPropertyString(this.App.UI.Path(DiplomacyUI.UILobbyPanel, "imgOtherBadge"), "sprite", Path.GetFileNameWithoutExtension(playerInfo.BadgeAssetPath));
				}
			}
			if (!(msgType == "button_clicked"))
				return;
			if (panelName == "gameTutorialButton")
				this.App.UI.SetVisible("DiplomacyScreenTutorial", true);
			else if (panelName == "diplomacyScreenTutImage")
				this.App.UI.SetVisible("DiplomacyScreenTutorial", false);
			else if (panelName == "btnEmpiresButton")
			{
				this._mode = DiplomacyScreenState.DiplomacyMode.Standard;
				this.App.UI.SetVisible("pnlStandardDiplomacy", true);
				this.App.UI.SetVisible("pnlIndyDiplomacy", false);
			}
			else if (panelName == "btnIndependentsButton")
			{
				this._mode = DiplomacyScreenState.DiplomacyMode.Independent;
				this.App.UI.SetVisible("pnlIndyDiplomacy", true);
				this.App.UI.SetVisible("pnlStandardDiplomacy", false);
			}
			else if (panelName == "btnBackButton")
				this.App.SwitchGameState<StarMapState>();
			else if (panelName == DiplomacyUI.UISurpriseAttackOk)
			{
				this.App.Game.DeclareWarInformally(this.App.Game.LocalPlayer.ID, this._playerId);
				this.App.UI.SetVisible(DiplomacyUI.UISurpriseAttackPanel, false);
				if (this._mode == DiplomacyScreenState.DiplomacyMode.Independent)
					DiplomacyUI.SyncIndyDiplomacyCard(this.App, "pnlIndyPlayerSummary", this._selectedIndy);
				else
					this.SyncPlayerDiplomacyCard(this._playerId, true);
			}
			else if (panelName == DiplomacyUI.UISurpriseAttackCancel)
				this.App.UI.SetVisible(DiplomacyUI.UISurpriseAttackPanel, false);
			else if (panelName == DiplomacyUI.UIDeclareWarOk)
			{
				this.App.Game.DeclareWarFormally(this.App.Game.LocalPlayer.ID, this._playerId);
				this.App.UI.SetVisible(DiplomacyUI.UIDeclareWarPanel, false);
				this.App.GameDatabase.SpendDiplomacyPoints(this.App.GameDatabase.GetPlayerInfo(this.App.Game.LocalPlayer.ID), this.App.GameDatabase.GetPlayerFactionID(this._playerId), this.App.Game.GetDiplomacyActionCost(DiplomacyAction.DECLARATION, new RequestType?(), new DemandType?()).Value);
				if (this._mode == DiplomacyScreenState.DiplomacyMode.Independent)
					DiplomacyUI.SyncIndyDiplomacyCard(this.App, "pnlIndyPlayerSummary", this._selectedIndy);
				else
					this.SyncPlayerDiplomacyCard(this._playerId, true);
			}
			else if (panelName == DiplomacyUI.UIDeclareWarCancel)
				this.App.UI.SetVisible(DiplomacyUI.UIDeclareWarPanel, false);
			else if (panelName == DiplomacyUI.UIDemandOk)
				this.App.UI.SetVisible(DiplomacyUI.UIDemandPanel, false);
			else if (panelName == DiplomacyUI.UIDemandCancel)
				this.App.UI.SetVisible(DiplomacyUI.UIDemandPanel, false);
			else if (panelName == DiplomacyUI.UIRequestOk)
				this.App.UI.SetVisible(DiplomacyUI.UIRequestPanel, false);
			else if (panelName == DiplomacyUI.UIRequestCancel)
				this.App.UI.SetVisible(DiplomacyUI.UIRequestPanel, false);
			else if (panelName == DiplomacyUI.UITreatyOk)
				this.App.UI.SetVisible(DiplomacyUI.UITreatyPanel, false);
			else if (panelName == DiplomacyUI.UITreatyCancel)
				this.App.UI.SetVisible(DiplomacyUI.UITreatyPanel, false);
			else if (panelName == DiplomacyUI.UILobbyRelationImprovebtn)
			{
				this._lobbyimprove = true;
				this.App.UI.SetChecked(this.App.UI.Path(DiplomacyUI.UILobbyPanel, DiplomacyUI.UILobbyRelationDegradebtn), false);
				this.App.UI.SetChecked(this.App.UI.Path(DiplomacyUI.UILobbyPanel, DiplomacyUI.UILobbyRelationImprovebtn), true);
			}
			else if (panelName == DiplomacyUI.UILobbyRelationDegradebtn)
			{
				this._lobbyimprove = false;
				this.App.UI.SetChecked(this.App.UI.Path(DiplomacyUI.UILobbyPanel, DiplomacyUI.UILobbyRelationDegradebtn), true);
				this.App.UI.SetChecked(this.App.UI.Path(DiplomacyUI.UILobbyPanel, DiplomacyUI.UILobbyRelationImprovebtn), false);
			}
			else if (panelName == DiplomacyUI.UILobbyOk)
			{
				this.App.Game.DoLobbyAction(this.App.LocalPlayer.ID, this._playerId, this._selectedLobbyPlayer, this._lobbyimprove);
				this.App.UI.SetVisible(DiplomacyUI.UILobbyPanel, false);
			}
			else if (panelName == DiplomacyUI.UILobbyCancel)
				this.App.UI.SetVisible(DiplomacyUI.UILobbyPanel, false);
			else if (panelName.EndsWith(DiplomacyUI.UIIntelButton))
			{
				IntelMissionDialog intelMissionDialog = new IntelMissionDialog(this.App.Game, this.PlayerSlots[int.Parse(panelName.Split('|')[0].Replace("Player", ""))]);
				this._playerId = this.GetPlayerId(panelName);
				this.App.UI.CreateDialog((Dialog)intelMissionDialog, null);
			}
			else if (panelName.EndsWith(DiplomacyUI.UICounterIntelButton))
			{
				int playerSlot = this.PlayerSlots[int.Parse(panelName.Split('|')[0].Replace("Player", ""))];
				this._playerId = this.GetPlayerId(panelName);
				this.App.UI.CreateDialog((Dialog)new CounterIntelMissionDialog(this.App.Game, playerSlot), null);
			}
			else if (panelName.EndsWith(DiplomacyUI.UIOperationsButton))
			{
				int playerSlot1 = this.PlayerSlots[int.Parse(panelName.Split('|')[0].Replace("Player", ""))];
			}
			else if (panelName.EndsWith(DiplomacyUI.UICardPreviousState))
			{
				string panelName1 = panelName.Split('|')[0];
				this._playerId = this.PlayerSlots[int.Parse(panelName1.Replace("Player", ""))];
				this.LastDiplomacyCardState[this._playerId] = DiplomacyUI.GetPreviousDiplomacyCardState(this.LastDiplomacyCardState[this._playerId]);
				DiplomacyUI.SyncPlayerDiplomacyCard(this.App, panelName1, this._playerId, this.LastDiplomacyCardState[this._playerId], false);
			}
			else if (panelName.EndsWith(DiplomacyUI.UICardNextState))
			{
				string panelName1 = panelName.Split('|')[0];
				this._playerId = this.PlayerSlots[int.Parse(panelName1.Replace("Player", ""))];
				this.LastDiplomacyCardState[this._playerId] = DiplomacyUI.GetNextDiplomacyCardState(this.LastDiplomacyCardState[this._playerId]);
				DiplomacyUI.SyncPlayerDiplomacyCard(this.App, panelName1, this._playerId, this.LastDiplomacyCardState[this._playerId], false);
			}
			else if (panelName.EndsWith(DiplomacyUI.UISurpriseAttackButton))
			{
				this._playerId = this.GetPlayerId(panelName);
				DiplomacyUI.SyncDiplomacyPopup(this.App, DiplomacyUI.UISurpriseAttackPanel, this._playerId);
				this.App.UI.SetVisible(DiplomacyUI.UISurpriseAttackPanel, true);
			}
			else if (panelName.EndsWith(DiplomacyUI.UIDeclareButton))
			{
				this._playerId = this.GetPlayerId(panelName);
				DiplomacyUI.SyncDiplomacyPopup(this.App, DiplomacyUI.UIDeclareWarPanel, this._playerId);
				this.App.UI.SetVisible(DiplomacyUI.UIDeclareWarPanel, true);
			}
			else if (panelName.EndsWith(DiplomacyUI.UIDemandButton))
			{
				this._playerId = this.GetPlayerId(panelName);
				this.App.UI.CreateDialog((Dialog)new DemandTypeDialog(this.App, this._playerId, "dialogDemandType"), null);
			}
			else if (panelName.EndsWith(DiplomacyUI.UIRequestButton))
			{
				this._playerId = this.GetPlayerId(panelName);
				this.App.UI.CreateDialog((Dialog)new RequestTypeDialog(this.App, this._playerId, "dialogRequestType"), null);
			}
			else if (panelName.EndsWith(DiplomacyUI.UITreatyButton))
			{
				this._playerId = this.GetPlayerId(panelName);
				this.App.UI.CreateDialog((Dialog)new TreatiesPopup(this.App, this._playerId, "TreatiesPopup"), null);
			}
			else if (panelName.EndsWith(DiplomacyUI.UILobbyButton))
			{
				this._playerId = this.GetPlayerId(panelName);
				DiplomacyUI.SyncDiplomacyPopup(this.App, DiplomacyUI.UILobbyPanel, this._playerId);
				this._lobbyimprove = true;
				this.App.UI.SetChecked(this.App.UI.Path(DiplomacyUI.UILobbyPanel, DiplomacyUI.UILobbyRelationDegradebtn), false);
				this.App.UI.SetChecked(this.App.UI.Path(DiplomacyUI.UILobbyPanel, DiplomacyUI.UILobbyRelationImprovebtn), true);
				this.App.UI.SetVisible(DiplomacyUI.UILobbyPanel, true);
			}
			else
			{
				if (!panelName.EndsWith(DiplomacyUI.UIGiveButton))
					return;
				this._playerId = this.GetPlayerId(panelName);
				this.App.UI.CreateDialog((Dialog)new GiveTypeDialog(this.App, this._playerId, "dialogGiveType"), null);
			}
		}

		protected override void OnUpdate()
		{
		}

		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
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
				}
			}
			return false;
		}

		private enum DiplomacyMode
		{
			Standard,
			Independent,
		}
	}
}
