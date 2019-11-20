// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.DiplomacyUI
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class DiplomacyUI
	{
		public static readonly string UIPanelBackground = "pnlBackground";
		public static readonly string UISurpriseAttackPanel = "pnlSurpriseAttack";
		public static readonly string UISurpriseAttackOk = "btnSurpriseAttackOk";
		public static readonly string UISurpriseAttackCancel = "btnSurpriseAttackCancel";
		public static readonly string UIDeclareWarPanel = "pnlDeclareWar";
		public static readonly string UIDeclareWarOk = "btnDeclareWarOk";
		public static readonly string UIDeclareWarCancel = "btnDeclareWarCancel";
		public static readonly string UIRequestPanel = "pnlRequest";
		public static readonly string UIRequestOk = "btnRequestOk";
		public static readonly string UIRequestCancel = "btnRequestCancel";
		public static readonly string UIDemandPanel = "pnlDemand";
		public static readonly string UIDemandOk = "btnDemandOk";
		public static readonly string UIDemandCancel = "btnDemandCancel";
		public static readonly string UITreatyPanel = "pnlTreaty";
		public static readonly string UITreatyOk = "btnTreatyOk";
		public static readonly string UITreatyCancel = "btnTreatyCancel";
		public static readonly string UILobbyPanel = "pnlLobby";
		public static readonly string UILobbyOk = "btnLobbyOk";
		public static readonly string UILobbyCancel = "btnLobbyCancel";
		public static readonly string UILobbyPlayerList = "LobbyselectEmpire";
		public static readonly string UILobbyRelationImprovebtn = "LobbyrelationsImprove";
		public static readonly string UILobbyRelationDegradebtn = "LobbyrelationsDegrade";
		public static readonly string UICardPreviousState = "btnPreviousState";
		public static readonly string UICardNextState = "btnNextState";
		public static readonly string UICardPlayerName = "lblPlayerName";
		public static readonly string UIAvatar = "imgAvatar";
		public static readonly string UIBadge = "imgBadge";
		public static readonly string UIRelation = "imgRelation";
		public static readonly string UIMood = "imgMood";
		public static readonly string UIRelationText = "lblPlayerRelation";
		public static readonly string UIRelationsGraph = "grphRelation";
		public static readonly string UIHazardRating = "lblHazardValue";
		public static readonly string UIDriveTech = "lblDriveTechValue";
		public static readonly string UIDriveSpecial = "lblDriveSpecialValue";
		public static readonly string UIStatRpdValue = "lblStatRpdValue";
		public static readonly string UIGovernmentType = "governmentType";
		public static readonly string UIActionRdpValue = "lblActionRpdValue";
		public static readonly string UIPendingActions = "lstPendingActions";
		public static readonly string UISurpriseAttackButton = "btnSurpriseAttack";
		public static readonly string UIDeclareButton = "btnDeclare";
		public static readonly string UIRequestButton = "btnRequest";
		public static readonly string UIDemandButton = "btnDemand";
		public static readonly string UITreatyButton = "btnTreaty";
		public static readonly string UILobbyButton = "btnLobby";
		public static readonly string UIGiveButton = "btnGive";
		public static readonly string UINewsList = "lstNews";
		public static readonly string UIInteractionsList = "lstInteractions";
		public static readonly string UIIntelButton = "btnIntel";
		public static readonly string UICounterIntelButton = "btnCounterIntel";
		public static readonly string UIOperationsButton = "btnOperations";
		public static readonly string UIIntelList = "listIntel";
		public static readonly string UICounterIntelList = "listCounterIntel";
		public static readonly string UIOperationsList = "listOperations";
		internal static Dictionary<DiplomacyCardState, DiplomacyUI.SyncCardStateDelegate> CardStateFunctionMap = new Dictionary<DiplomacyCardState, DiplomacyUI.SyncCardStateDelegate>()
	{
	  {
		DiplomacyCardState.PlayerStats,
		new DiplomacyUI.SyncCardStateDelegate(DiplomacyUI.SyncPlayerStatsState)
	  },
	  {
		DiplomacyCardState.DiplomacyActions,
		new DiplomacyUI.SyncCardStateDelegate(DiplomacyUI.SyncDiplomacyActionsState)
	  },
	  {
		DiplomacyCardState.PlayerHistory,
		new DiplomacyUI.SyncCardStateDelegate(DiplomacyUI.SyncPlayerHistoryState)
	  },
	  {
		DiplomacyCardState.Espionage,
		new DiplomacyUI.SyncCardStateDelegate(DiplomacyUI.SyncEspionageState)
	  }
	};
		public const int MaxPlayerCards = 7;

		private static bool EnableSupriseButton(GameSession game, PlayerInfo target)
		{
			return game.CanPerformLocalDiplomacyAction(target, DiplomacyAction.SURPRISEATTACK, new RequestType?(), new DemandType?());
		}

		private static bool EnableDeclareButton(GameSession game, PlayerInfo target)
		{
			return game.CanPerformLocalDiplomacyAction(target, DiplomacyAction.DECLARATION, new RequestType?(), new DemandType?());
		}

		private static bool EnableRequestButton(GameSession game, PlayerInfo target)
		{
			return game.CanPerformLocalDiplomacyAction(target, DiplomacyAction.REQUEST, new RequestType?(), new DemandType?());
		}

		private static bool EnableDemandButton(GameSession game, PlayerInfo target)
		{
			return game.CanPerformLocalDiplomacyAction(target, DiplomacyAction.DEMAND, new RequestType?(), new DemandType?());
		}

		private static bool EnableTreatyButton(GameSession game, PlayerInfo target)
		{
			return game.CanPerformLocalDiplomacyAction(target, DiplomacyAction.TREATY, new RequestType?(), new DemandType?());
		}

		private static bool EnableLobbyButton(GameSession game, PlayerInfo target)
		{
			return game.CanPerformLocalDiplomacyAction(target, DiplomacyAction.LOBBY, new RequestType?(), new DemandType?());
		}

		private static string GetDiplomacyActionString(
		  App game,
		  DiplomacyActionHistoryEntryInfo actionInfo)
		{
			string str = "";
			switch (actionInfo.Action)
			{
				case DiplomacyAction.DECLARATION:
					return App.Localize("@DIPLOMACY_DECLARE_WAR");
				case DiplomacyAction.REQUEST:
					str = App.Localize("@UI_DIPLOMACY_REQUEST") + " ";
					int? actionSubType1 = actionInfo.ActionSubType;
					int valueOrDefault1 = actionSubType1.GetValueOrDefault();
					if (actionSubType1.HasValue)
					{
						switch (valueOrDefault1)
						{
							case 0:
								str += App.Localize("@DIPLOMACY_SUBTYPE_MONEY");
								break;
							case 1:
								str += App.Localize("@DIPLOMACY_SUBTYPE_SYSTEM_INFORMATION");
								break;
							case 2:
								str += App.Localize("@DIPLOMACY_SUBTYPE_RESEARCH_POINTS");
								break;
							case 3:
								str += App.Localize("@DIPLOMACY_SUBTYPE_MILITARY_ASSISTANCE");
								break;
							case 4:
								str += App.Localize("@DIPLOMACY_SUBTYPE_BUILD_GATE");
								break;
							case 5:
								str += App.Localize("@DIPLOMACY_SUBTYPE_BUILD_WORLD");
								break;
							case 6:
								str += App.Localize("@DIPLOMACY_SUBTYPE_BUILD_ENCLAVE");
								break;
						}
					}
					break;
				case DiplomacyAction.DEMAND:
					str = App.Localize("@DIPLOMACY_DEMAND") + " ";
					int? actionSubType2 = actionInfo.ActionSubType;
					int valueOrDefault2 = actionSubType2.GetValueOrDefault();
					if (actionSubType2.HasValue)
					{
						switch (valueOrDefault2)
						{
							case 0:
								str += App.Localize("@DIPLOMACY_SUBTYPE_MONEY");
								break;
							case 1:
								str += App.Localize("@DIPLOMACY_SUBTYPE_SYSTEM_INFORMATION");
								break;
							case 2:
								str += App.Localize("@DIPLOMACY_SUBTYPE_RESEARCH_POINTS");
								break;
							case 3:
								str += App.Localize("@DIPLOMACY_SUBTYPE_SLAVES");
								break;
							case 4:
								str += App.Localize("@DIPLOMACY_SUBTYPE_SYSTEM");
								break;
							case 5:
								str += App.Localize("@DIPLOMACY_SUBTYPE_SURRENDER");
								break;
						}
					}
					break;
				case DiplomacyAction.TREATY:
					str = App.Localize("@UI_DIPLOMACY_TREATY") + " ";
					int? actionSubType3 = actionInfo.ActionSubType;
					int valueOrDefault3 = actionSubType3.GetValueOrDefault();
					if (actionSubType3.HasValue)
					{
						switch (valueOrDefault3)
						{
							case 0:
								str += App.Localize("@UI_TREATY_ARMISTICE");
								break;
							case 1:
								str += App.Localize("@UI_TREATY_TRADE");
								break;
							case 2:
								str += App.Localize("@UI_TREATY_LIMITATION");
								break;
							case 3:
								str += App.Localize("@UI_TREATY_PROTECTORATE");
								break;
							case 4:
								str += App.Localize("@UI_TREATY_INCORPORATE");
								break;
						}
					}
					break;
				case DiplomacyAction.LOBBY:
					return App.Localize("@DIPLOMACY_LOBBY");
				case DiplomacyAction.SPIN:
					return App.Localize("@DIPLOMACY_SPIN");
				case DiplomacyAction.SURPRISEATTACK:
					return App.Localize("@DIPLOMACY_SURPRISE_ATTACK");
				case DiplomacyAction.GIVE:
					int? actionSubType4 = actionInfo.ActionSubType;
					int valueOrDefault4 = actionSubType4.GetValueOrDefault();
					if (actionSubType4.HasValue)
					{
						switch (valueOrDefault4)
						{
							case 0:
								str = App.Localize("@UI_DIPLOMACY_GIVE_SAVINGS");
								break;
							case 1:
								str = App.Localize("@UI_DIPLOMACY_GIVE_RESEARCH_MONEY");
								break;
						}
					}
					break;
			}
			return str;
		}

		public static void SyncPanelColor(App game, string panelName, Vector3 color)
		{
			foreach (string str in new List<string>()
	  {
		"TLC",
		"TRC",
		"BLC",
		"BRC",
		"TC",
		"BC",
		"FILL"
	  })
				game.UI.SetPropertyColorNormalized(game.UI.Path(panelName, str), nameof(color), color);
		}

		private static string GetRelationText(DiplomacyState state)
		{
			switch (state)
			{
				case DiplomacyState.CEASE_FIRE:
					return "C/F";
				case DiplomacyState.UNKNOWN:
					return string.Empty;
				case DiplomacyState.NON_AGGRESSION:
					return "NAP";
				case DiplomacyState.WAR:
					return "War";
				case DiplomacyState.ALLIED:
					return "Ally";
				case DiplomacyState.NEUTRAL:
					return "Neutral";
				case DiplomacyState.PEACE:
					return "Peace";
				default:
					return string.Empty;
			}
		}

		private static void SyncPlayerStatsState(
		  App game,
		  string panelName,
		  PlayerInfo playerInfo,
		  bool updateButtonIds)
		{
			FactionInfo factionInfo = game.GameDatabase.GetFactionInfo(playerInfo.FactionID);
			string propertyValue1 = string.Format("{0:000}", (object)Math.Abs(game.GameDatabase.GetFactionInfo(game.GameDatabase.GetPlayerFactionID(game.Game.LocalPlayer.ID)).IdealSuitability - factionInfo.IdealSuitability));
			string enginePlantTechString = GameSession.GetBestEnginePlantTechString(game, playerInfo.ID);
			string engineTechString = GameSession.GetBestEngineTechString(game, playerInfo.ID);
			string propertyValue2 = game.GameDatabase.GetPlayerInfo(game.Game.LocalPlayer.ID).FactionDiplomacyPoints[factionInfo.ID].ToString();
			DiplomacyInfo diplomacyInfo = game.GameDatabase.GetDiplomacyInfo(playerInfo.ID, game.Game.LocalPlayer.ID);
			int numHistoryTurns = 10;
			int currentTurn = game.GameDatabase.GetTurnCount();
			List<DiplomacyReactionHistoryEntryInfo> list = game.GameDatabase.GetDiplomacyReactionHistory(playerInfo.ID, game.Game.LocalPlayer.ID, currentTurn, 10).ToList<DiplomacyReactionHistoryEntryInfo>();
			int[] numArray = new int[numHistoryTurns];
			numArray[numHistoryTurns - 1] = diplomacyInfo.Relations;
			string propertyValue3 = numArray[numHistoryTurns - 1].ToString();
			for (int i = numHistoryTurns - 2; i >= 0; --i)
			{
				numArray[i] = numArray[i + 1] + list.Where<DiplomacyReactionHistoryEntryInfo>((Func<DiplomacyReactionHistoryEntryInfo, bool>)(x =>
			   {
				   int? turnCount = x.TurnCount;
				   int num = currentTurn - (numHistoryTurns - 1 - i);
				   if (turnCount.GetValueOrDefault() == num)
					   return turnCount.HasValue;
				   return false;
			   })).Sum<DiplomacyReactionHistoryEntryInfo>((Func<DiplomacyReactionHistoryEntryInfo, int>)(y => y.Difference));
				propertyValue3 = propertyValue3 + "|" + numArray[i].ToString();
			}
			game.UI.SetPropertyString(game.UI.Path(panelName, DiplomacyUI.UIRelationsGraph), "data", propertyValue3);
			string diplomaticMoodSprite = diplomacyInfo.GetDiplomaticMoodSprite();
			if (!string.IsNullOrEmpty(diplomaticMoodSprite))
			{
				game.UI.SetVisible(DiplomacyUI.UIRelation, true);
				game.UI.SetPropertyString(DiplomacyUI.UIRelation, "sprite", diplomaticMoodSprite);
			}
			else
				game.UI.SetVisible(DiplomacyUI.UIRelation, false);
			game.UI.SetText(game.UI.Path(panelName, DiplomacyUI.UIRelationText), DiplomacyUI.GetRelationText(diplomacyInfo.State));
			string propertyValue4 = Path.GetFileNameWithoutExtension(playerInfo.AvatarAssetPath);
			if (propertyValue4 == "")
				propertyValue4 = game.AssetDatabase.GetFaction(playerInfo.FactionID).SplinterAvatarPath();
			game.UI.SetVisible(game.UI.Path(panelName, "eliminated"), (playerInfo.isDefeated ? 1 : 0) != 0);
			game.UI.SetPropertyString(game.UI.Path(panelName, DiplomacyUI.UIAvatar), "sprite", propertyValue4);
			game.UI.SetPropertyString(game.UI.Path(panelName, DiplomacyUI.UIBadge), "sprite", Path.GetFileNameWithoutExtension(playerInfo.BadgeAssetPath));
			game.UI.SetPropertyString(game.UI.Path(panelName, DiplomacyUI.UIHazardRating), "text", propertyValue1);
			game.UI.SetVisible(game.UI.Path(panelName, DiplomacyUI.UIHazardRating), (factionInfo.Name != "loa" ? 1 : 0) != 0);
			game.UI.SetVisible(game.UI.Path(panelName, "hazardtitle"), (factionInfo.Name != "loa" ? 1 : 0) != 0);
			if (playerInfo.isStandardPlayer)
			{
				game.UI.SetPropertyString(game.UI.Path(panelName, DiplomacyUI.UIDriveTech), "text", enginePlantTechString);
				game.UI.SetPropertyString(game.UI.Path(panelName, DiplomacyUI.UIDriveSpecial), "text", engineTechString);
			}
			else
			{
				game.UI.SetPropertyString(game.UI.Path(panelName, DiplomacyUI.UIDriveTech), "text", App.Localize("@UI_DIPLOMACY_TECHLEVEL_1"));
				game.UI.SetPropertyString(game.UI.Path(panelName, DiplomacyUI.UIDriveSpecial), "text", "");
			}
			game.UI.SetPropertyString(game.UI.Path(panelName, DiplomacyUI.UIStatRpdValue), "text", propertyValue2);
			GovernmentInfo governmentInfo = game.GameDatabase.GetGovernmentInfo(playerInfo.ID);
			game.UI.SetPropertyString(game.UI.Path(panelName, DiplomacyUI.UIGovernmentType), "text", App.Localize(string.Format("@UI_EMPIRESUMMARY_{0}", (object)governmentInfo.CurrentType.ToString().ToUpper())));
		}

		private static void SyncDiplomacyActionsState(
		  App game,
		  string panelName,
		  PlayerInfo playerInfo,
		  bool updateButtonIds)
		{
			int turnCount = game.GameDatabase.GetTurnCount();
			FactionInfo factionInfo = game.GameDatabase.GetFactionInfo(playerInfo.FactionID);
			string propertyValue = game.GameDatabase.GetPlayerInfo(game.Game.LocalPlayer.ID).FactionDiplomacyPoints[factionInfo.ID].ToString();
			List<DiplomacyActionHistoryEntryInfo> list = game.GameDatabase.GetDiplomacyActionHistory(game.Game.LocalPlayer.ID, playerInfo.ID, turnCount, 1).ToList<DiplomacyActionHistoryEntryInfo>();
			game.UI.ClearItems(game.UI.Path(panelName, DiplomacyUI.UIPendingActions));
			foreach (DiplomacyActionHistoryEntryInfo actionInfo in list)
			{
				game.UI.AddItem(game.UI.Path(panelName, DiplomacyUI.UIPendingActions), string.Empty, actionInfo.ID, string.Empty);
				string itemGlobalId = game.UI.GetItemGlobalID(game.UI.Path(panelName, DiplomacyUI.UIPendingActions), string.Empty, actionInfo.ID, string.Empty);
				game.UI.SetEnabled(itemGlobalId, false);
				game.UI.SetPropertyString(game.UI.Path(itemGlobalId, "txtTurn"), "text", actionInfo.TurnCount.ToString());
				game.UI.SetPropertyString(game.UI.Path(itemGlobalId, "txtInteraction"), "text", DiplomacyUI.GetDiplomacyActionString(game, actionInfo));
			}
			string str = ((IEnumerable<string>)panelName.Split('.')).First<string>();
			bool isStandardPlayer = playerInfo.isStandardPlayer;
			UICommChannel ui1 = game.UI;
			string panelId1;
			if (!isStandardPlayer)
				panelId1 = game.UI.Path(panelName, DiplomacyUI.UIDeclareButton);
			else
				panelId1 = string.Format("{0}|{1}", (object)str, (object)DiplomacyUI.UIDeclareButton);
			string text = string.Format(App.Localize("@UI_DIPLOMACY_DECLARE"), (object)game.Game.GetDiplomacyActionCost(DiplomacyAction.DECLARATION, new RequestType?(), new DemandType?()));
			ui1.SetButtonText(panelId1, text);
			UICommChannel ui2 = game.UI;
			string panelId2;
			if (!isStandardPlayer)
				panelId2 = game.UI.Path(panelName, DiplomacyUI.UISurpriseAttackButton);
			else
				panelId2 = string.Format("{0}|{1}", (object)str, (object)DiplomacyUI.UISurpriseAttackButton);
			int num1 = DiplomacyUI.EnableSupriseButton(game.Game, playerInfo) ? 1 : 0;
			ui2.SetEnabled(panelId2, num1 != 0);
			UICommChannel ui3 = game.UI;
			string panelId3;
			if (!isStandardPlayer)
				panelId3 = game.UI.Path(panelName, DiplomacyUI.UIDeclareButton);
			else
				panelId3 = string.Format("{0}|{1}", (object)str, (object)DiplomacyUI.UIDeclareButton);
			int num2 = DiplomacyUI.EnableDeclareButton(game.Game, playerInfo) ? 1 : 0;
			ui3.SetEnabled(panelId3, num2 != 0);
			UICommChannel ui4 = game.UI;
			string panelId4;
			if (!isStandardPlayer)
				panelId4 = game.UI.Path(panelName, DiplomacyUI.UIRequestButton);
			else
				panelId4 = string.Format("{0}|{1}", (object)str, (object)DiplomacyUI.UIRequestButton);
			int num3 = DiplomacyUI.EnableRequestButton(game.Game, playerInfo) ? 1 : 0;
			ui4.SetEnabled(panelId4, num3 != 0);
			UICommChannel ui5 = game.UI;
			string panelId5;
			if (!isStandardPlayer)
				panelId5 = game.UI.Path(panelName, DiplomacyUI.UIDemandButton);
			else
				panelId5 = string.Format("{0}|{1}", (object)str, (object)DiplomacyUI.UIDemandButton);
			int num4 = DiplomacyUI.EnableDemandButton(game.Game, playerInfo) ? 1 : 0;
			ui5.SetEnabled(panelId5, num4 != 0);
			UICommChannel ui6 = game.UI;
			string panelId6;
			if (!isStandardPlayer)
				panelId6 = game.UI.Path(panelName, DiplomacyUI.UITreatyButton);
			else
				panelId6 = string.Format("{0}|{1}", (object)str, (object)DiplomacyUI.UITreatyButton);
			int num5 = DiplomacyUI.EnableTreatyButton(game.Game, playerInfo) ? 1 : 0;
			ui6.SetEnabled(panelId6, num5 != 0);
			game.UI.SetPropertyString(game.UI.Path(panelName, DiplomacyUI.UIActionRdpValue), "text", propertyValue);
		}

		private static void SyncPlayerHistoryState(
		  App game,
		  string panelName,
		  PlayerInfo playerInfo,
		  bool updateButtonIds)
		{
			int turnCount = game.GameDatabase.GetTurnCount();
			game.UI.ClearItems(DiplomacyUI.UINewsList);
			List<DiplomacyReactionHistoryEntryInfo> list = game.GameDatabase.GetDiplomacyReactionHistory(playerInfo.ID, game.Game.LocalPlayer.ID, turnCount, 10).ToList<DiplomacyReactionHistoryEntryInfo>();
			Vector3 vector3_1 = new Vector3(0.2f, 0.8f, 0.2f);
			Vector3 vector3_2 = new Vector3(0.8f, 0.2f, 0.2f);
			foreach (DiplomacyReactionHistoryEntryInfo historyEntryInfo in list)
			{
				Vector3 vector3_3 = historyEntryInfo.Difference > 0 ? vector3_1 : vector3_2;
				game.UI.AddItem(game.UI.Path(panelName, DiplomacyUI.UINewsList), string.Empty, historyEntryInfo.ID, string.Empty);
				string itemGlobalId = game.UI.GetItemGlobalID(game.UI.Path(panelName, DiplomacyUI.UINewsList), string.Empty, historyEntryInfo.ID, string.Empty);
				game.UI.SetEnabled(itemGlobalId, false);
				game.UI.SetPropertyString(game.UI.Path(itemGlobalId, "txtTurn"), "text", historyEntryInfo.TurnCount.ToString());
				game.UI.SetPropertyString(game.UI.Path(itemGlobalId, "txtType"), "text", App.Localize("@" + historyEntryInfo.Reaction.ToString()));
				game.UI.SetPropertyString(game.UI.Path(itemGlobalId, "txtChange"), "text", historyEntryInfo.Difference > 0 ? string.Format("+{0}", (object)historyEntryInfo.Difference) : historyEntryInfo.Difference.ToString());
				game.UI.SetPropertyColorNormalized(game.UI.Path(itemGlobalId, "txtChange"), "color", vector3_3);
			}
			game.UI.ClearItems(DiplomacyUI.UIInteractionsList);
			foreach (DiplomacyActionHistoryEntryInfo actionInfo in game.GameDatabase.GetDiplomacyActionHistory(playerInfo.ID, game.Game.LocalPlayer.ID, turnCount - 1, 10).ToList<DiplomacyActionHistoryEntryInfo>())
			{
				game.UI.AddItem(game.UI.Path(panelName, DiplomacyUI.UIInteractionsList), string.Empty, actionInfo.ID, string.Empty);
				string itemGlobalId = game.UI.GetItemGlobalID(game.UI.Path(panelName, DiplomacyUI.UIInteractionsList), string.Empty, actionInfo.ID, string.Empty);
				game.UI.SetEnabled(itemGlobalId, false);
				game.UI.SetPropertyString(game.UI.Path(itemGlobalId, "txtTurn"), "text", actionInfo.TurnCount.ToString());
				game.UI.SetPropertyString(game.UI.Path(itemGlobalId, "txtInteraction"), "text", DiplomacyUI.GetDiplomacyActionString(game, actionInfo));
			}
		}

		private static void AddIntelListItem(App game, string panelName, string entry, int itemid)
		{
			game.UI.AddItem(game.UI.Path(panelName, DiplomacyUI.UIIntelList), string.Empty, itemid, entry);
			game.UI.GetItemGlobalID(game.UI.Path(panelName, DiplomacyUI.UIIntelList), string.Empty, itemid, string.Empty);
		}

		public static void ClearIntelList(App game, string panelName)
		{
			game.UI.ClearItems(game.UI.Path(panelName, DiplomacyUI.UIIntelList));
		}

		private static void SyncEspionageState(
		  App game,
		  string panelName,
		  PlayerInfo playerInfo,
		  bool updateButtonIds)
		{
			PlayerInfo playerInfo1 = game.GameDatabase.GetPlayerInfo(game.LocalPlayer.ID);
			game.UI.SetPropertyString(game.UI.Path(panelName, panelName.Split('.')[0] + "|btnIntel", "lblIntelLabel"), "text", string.Format(App.Localize("@UI_DIPLOMACY_INTEL"), (object)playerInfo1.IntelPoints));
			game.UI.SetPropertyString(game.UI.Path(panelName, panelName.Split('.')[0] + "|btnCounterIntel", "lblCounterIntelLabel"), "text", string.Format(App.Localize("@UI_DIPLOMACY_COUNTER_INTEL"), (object)playerInfo1.CounterIntelPoints));
			game.UI.SetPropertyString(game.UI.Path(panelName, panelName.Split('.')[0] + "|btnOperations", "lblOperationsLabel"), "text", string.Format(App.Localize("@UI_DIPLOMACY_OPERATIONS"), (object)playerInfo1.OperationsPoints));
			game.UI.SetEnabled(game.UI.Path(panelName, panelName.Split('.')[0] + "|btnCounterIntel"), (playerInfo1.CounterIntelPoints >= game.AssetDatabase.RequiredCounterIntelPointsForMission ? 1 : 0) != 0);
			DiplomacyUI.ClearIntelList(game, panelName);
			int num = 0;
			foreach (IntelMissionInfo intelMissionInfo in game.GameDatabase.GetIntelInfosForPlayer(game.LocalPlayer.ID).Where<IntelMissionInfo>((Func<IntelMissionInfo, bool>)(x => x.TargetPlayerId == playerInfo.ID)).ToList<IntelMissionInfo>())
			{
				DiplomacyUI.AddIntelListItem(game, panelName, "Pending Intel", intelMissionInfo.ID);
				num = num > intelMissionInfo.ID ? num : intelMissionInfo.ID;
			}
			int itemid = num + 1;
			foreach (TurnEvent turnEvent in game.GameDatabase.GetTurnEventsByTurnNumber(game.GameDatabase.GetTurnCount(), game.LocalPlayer.ID).Where<TurnEvent>((Func<TurnEvent, bool>)(x =>
		   {
			   if (x.EventType != TurnEventType.EV_INTEL_MISSION_CRITICAL_FAILED)
				   return x.EventType == TurnEventType.EV_INTEL_MISSION_FAILED;
			   return true;
		   })).ToList<TurnEvent>())
			{
				DiplomacyUI.AddIntelListItem(game, panelName, "Pending Intel", itemid);
				++itemid;
			}
			game.UI.ClearItems(game.UI.Path(panelName, DiplomacyUI.UICounterIntelList));
			foreach (CounterIntelStingMission intelStingMission in game.GameDatabase.GetCountIntelStingsForPlayerAgainstPlayer(game.LocalPlayer.ID, playerInfo.ID).ToList<CounterIntelStingMission>())
				game.UI.AddItem(game.UI.Path(panelName, DiplomacyUI.UICounterIntelList), string.Empty, intelStingMission.ID, string.Format(App.Localize("@UI_COUNTER_INTEL_LIST_ITEM")));
		}

		public static DiplomacyCardState GetPreviousDiplomacyCardState(
		  DiplomacyCardState cardState)
		{
			if (cardState != DiplomacyCardState.PlayerStats)
				return cardState - 1;
			DiplomacyCardState[] values = (DiplomacyCardState[])Enum.GetValues(typeof(DiplomacyCardState));
			return values[((IEnumerable<DiplomacyCardState>)values).Count<DiplomacyCardState>() - 1];
		}

		public static DiplomacyCardState GetNextDiplomacyCardState(
		  DiplomacyCardState cardState)
		{
			DiplomacyCardState[] values = (DiplomacyCardState[])Enum.GetValues(typeof(DiplomacyCardState));
			if (cardState == (DiplomacyCardState)(((IEnumerable<DiplomacyCardState>)values).Count<DiplomacyCardState>() - 1))
				return values[0];
			return cardState + 1;
		}

		public static void SyncDiplomacyPopup(App game, string panelName, int playerId)
		{
			PlayerInfo playerInfo = game.GameDatabase.GetPlayerInfo(playerId);
			DiplomaticMood diplomaticMood = game.GameDatabase.GetDiplomacyInfo(playerInfo.ID, game.Game.LocalPlayer.ID).GetDiplomaticMood();
			game.UI.SetVisible(DiplomacyUI.UIRelation, true);
			switch (diplomaticMood)
			{
				case DiplomaticMood.Hatred:
					game.UI.SetPropertyString(DiplomacyUI.UIRelation, "sprite", "Hate");
					break;
				case DiplomaticMood.Love:
					game.UI.SetPropertyString(DiplomacyUI.UIRelation, "sprite", "Love");
					break;
				default:
					game.UI.SetVisible(DiplomacyUI.UIRelation, false);
					break;
			}
			DiplomacyUI.SyncPanelColor(game, game.UI.Path(panelName, DiplomacyUI.UIPanelBackground), playerInfo.PrimaryColor);
			game.UI.SetPropertyString(game.UI.Path(panelName, DiplomacyUI.UIAvatar), "sprite", Path.GetFileNameWithoutExtension(playerInfo.AvatarAssetPath));
			game.UI.SetPropertyString(game.UI.Path(panelName, DiplomacyUI.UIBadge), "sprite", Path.GetFileNameWithoutExtension(playerInfo.BadgeAssetPath));
			game.UI.SetPropertyString(game.UI.Path(panelName, DiplomacyUI.UICardPlayerName), "text", playerInfo.Name);
			if (!(panelName == DiplomacyUI.UILobbyPanel))
				return;
			foreach (PlayerInfo standardPlayerInfo in game.GameDatabase.GetStandardPlayerInfos())
			{
				if (standardPlayerInfo.ID != playerId)
				{
					float num1 = (float)game.GameDatabase.GetDiplomacyInfo(playerInfo.ID, standardPlayerInfo.ID).Relations / 2000f;
					float num2 = ((float)((!(game.GameDatabase.GetPlayerFaction(standardPlayerInfo.ID).Name == "morrigi") || standardPlayerInfo.ID != game.Game.LocalPlayer.ID ? 0.0 : (double)game.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") * 0.5) + (game.GameDatabase.PlayerHasTech(game.Game.LocalPlayer.ID, "PSI_Lesser_Glamour") ? (double)game.AssetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering") : 0.0)) + num1) * 100f;
					game.UI.AddItem(DiplomacyUI.UILobbyPlayerList, "", standardPlayerInfo.ID, standardPlayerInfo.Name + " (" + (object)(int)num2 + "% Chance)");
				}
			}
			game.UI.SetSelection(DiplomacyUI.UILobbyPlayerList, game.LocalPlayer.ID);
		}

		public static void HideAllPlayerDiplomacyCards(App game)
		{
			for (int index = 0; index < 7; ++index)
				game.UI.SetVisible("Player" + (object)index, false);
		}

		public static void SyncIndyDiplomacyCard(App game, string panelName, int playerId)
		{
			PlayerInfo playerInfo = game.GameDatabase.GetPlayerInfo(playerId);
			game.UI.SetPropertyString(game.UI.Path(panelName, DiplomacyUI.UICardPlayerName), "text", playerInfo.Name);
			DiplomacyUI.SyncPanelColor(game, game.UI.Path(panelName, DiplomacyUI.UIPanelBackground), playerInfo.PrimaryColor);
			DiplomacyUI.CardStateFunctionMap[DiplomacyCardState.DiplomacyActions](game, game.UI.Path(panelName, "stateDiplomacyActions"), playerInfo, false);
			DiplomacyUI.CardStateFunctionMap[DiplomacyCardState.PlayerHistory](game, game.UI.Path(panelName, "statePlayerHistory"), playerInfo, false);
			DiplomacyUI.CardStateFunctionMap[DiplomacyCardState.PlayerStats](game, game.UI.Path(panelName, "statePlayerStats"), playerInfo, false);
		}

		public static void SyncPlayerDiplomacyCard(
		  App game,
		  string panelName,
		  int playerId,
		  DiplomacyCardState cardState,
		  bool updateButtonIds)
		{
			PlayerInfo playerInfo = game.GameDatabase.GetPlayerInfo(playerId);
			if (updateButtonIds)
			{
				game.UI.SetPropertyString(game.UI.Path(panelName, DiplomacyUI.UICardPreviousState), "id", string.Format("{0}|{1}", (object)panelName, (object)DiplomacyUI.UICardPreviousState));
				game.UI.SetPropertyString(game.UI.Path(panelName, DiplomacyUI.UICardNextState), "id", string.Format("{0}|{1}", (object)panelName, (object)DiplomacyUI.UICardNextState));
				game.UI.SetButtonText(game.UI.Path(panelName, "stateDiplomacyActions", DiplomacyUI.UIDeclareButton), string.Format(App.Localize("@UI_DIPLOMACY_DECLARE"), (object)game.AssetDatabase.DeclareWarPointCost));
				game.UI.SetPropertyString(game.UI.Path(panelName, "stateDiplomacyActions", DiplomacyUI.UISurpriseAttackButton), "id", string.Format("{0}|{1}", (object)panelName, (object)DiplomacyUI.UISurpriseAttackButton));
				game.UI.SetPropertyString(game.UI.Path(panelName, "stateDiplomacyActions", DiplomacyUI.UIDeclareButton), "id", string.Format("{0}|{1}", (object)panelName, (object)DiplomacyUI.UIDeclareButton));
				game.UI.SetPropertyString(game.UI.Path(panelName, "stateDiplomacyActions", DiplomacyUI.UIRequestButton), "id", string.Format("{0}|{1}", (object)panelName, (object)DiplomacyUI.UIRequestButton));
				game.UI.SetPropertyString(game.UI.Path(panelName, "stateDiplomacyActions", DiplomacyUI.UIDemandButton), "id", string.Format("{0}|{1}", (object)panelName, (object)DiplomacyUI.UIDemandButton));
				game.UI.SetPropertyString(game.UI.Path(panelName, "stateDiplomacyActions", DiplomacyUI.UITreatyButton), "id", string.Format("{0}|{1}", (object)panelName, (object)DiplomacyUI.UITreatyButton));
				game.UI.SetPropertyString(game.UI.Path(panelName, "stateDiplomacyActions", DiplomacyUI.UILobbyButton), "id", string.Format("{0}|{1}", (object)panelName, (object)DiplomacyUI.UILobbyButton));
				game.UI.SetPropertyString(game.UI.Path(panelName, "stateDiplomacyActions", DiplomacyUI.UIGiveButton), "id", string.Format("{0}|{1}", (object)panelName, (object)DiplomacyUI.UIGiveButton));
				game.UI.SetPropertyString(game.UI.Path(panelName, "stateEspionage", DiplomacyUI.UIIntelButton), "id", string.Format("{0}|{1}", (object)panelName, (object)DiplomacyUI.UIIntelButton));
				game.UI.SetPropertyString(game.UI.Path(panelName, "stateEspionage", DiplomacyUI.UICounterIntelButton), "id", string.Format("{0}|{1}", (object)panelName, (object)DiplomacyUI.UICounterIntelButton));
				game.UI.SetPropertyString(game.UI.Path(panelName, "stateEspionage", DiplomacyUI.UIOperationsButton), "id", string.Format("{0}|{1}", (object)panelName, (object)DiplomacyUI.UIOperationsButton));
			}
			game.UI.SetEnabled(game.UI.Path(panelName, string.Format("{0}|{1}", (object)panelName, (object)DiplomacyUI.UICardPreviousState)), (!playerInfo.isDefeated ? 1 : 0) != 0);
			game.UI.SetEnabled(game.UI.Path(panelName, string.Format("{0}|{1}", (object)panelName, (object)DiplomacyUI.UICardNextState)), (!playerInfo.isDefeated ? 1 : 0) != 0);
			game.UI.SetEnabled(game.UI.Path(panelName, "stateDiplomacyActions", string.Format("{0}|{1}", (object)panelName, (object)DiplomacyUI.UISurpriseAttackButton)), (!playerInfo.isDefeated ? 1 : 0) != 0);
			game.UI.SetEnabled(game.UI.Path(panelName, "stateDiplomacyActions", string.Format("{0}|{1}", (object)panelName, (object)DiplomacyUI.UIDeclareButton)), (!playerInfo.isDefeated ? 1 : 0) != 0);
			game.UI.SetEnabled(game.UI.Path(panelName, "stateDiplomacyActions", string.Format("{0}|{1}", (object)panelName, (object)DiplomacyUI.UIRequestButton)), (!playerInfo.isDefeated ? 1 : 0) != 0);
			game.UI.SetEnabled(game.UI.Path(panelName, "stateDiplomacyActions", string.Format("{0}|{1}", (object)panelName, (object)DiplomacyUI.UIDemandButton)), (!playerInfo.isDefeated ? 1 : 0) != 0);
			game.UI.SetEnabled(game.UI.Path(panelName, "stateDiplomacyActions", string.Format("{0}|{1}", (object)panelName, (object)DiplomacyUI.UITreatyButton)), (!playerInfo.isDefeated ? 1 : 0) != 0);
			game.UI.SetEnabled(game.UI.Path(panelName, "stateDiplomacyActions", string.Format("{0}|{1}", (object)panelName, (object)DiplomacyUI.UIGiveButton)), (playerInfo.isDefeated ? 0 : (game.GameDatabase.GetPlayerInfo(game.LocalPlayer.ID).Savings > 0.0 ? 1 : 0)) != 0);
			game.UI.SetEnabled(game.UI.Path(panelName, "stateDiplomacyActions", string.Format("{0}|{1}", (object)panelName, (object)DiplomacyUI.UILobbyButton)), (playerInfo.isDefeated ? 0 : (game.Game.CanPerformLocalDiplomacyAction(playerInfo, DiplomacyAction.LOBBY, new RequestType?(), new DemandType?()) ? 1 : 0)) != 0);
			bool flag = game.AssetDatabase.GetFaction(playerInfo.FactionID).Name == "loa" && game.GameDatabase.PlayerHasTech(game.LocalPlayer.ID, "CCC_Artificial_Intelligence") || game.AssetDatabase.GetFaction(playerInfo.FactionID).Name != "loa";
			game.UI.SetEnabled(game.UI.Path(panelName, "stateEspionage", string.Format("{0}|{1}", (object)panelName, (object)DiplomacyUI.UIIntelButton)), (playerInfo.isDefeated ? 0 : (flag ? 1 : 0)) != 0);
			game.UI.SetEnabled(game.UI.Path(panelName, "stateEspionage", string.Format("{0}|{1}", (object)panelName, (object)DiplomacyUI.UICounterIntelButton)), (playerInfo.isDefeated ? 0 : (flag ? 1 : 0)) != 0);
			game.UI.SetEnabled(game.UI.Path(panelName, "stateEspionage", string.Format("{0}|{1}", (object)panelName, (object)DiplomacyUI.UIOperationsButton)), (playerInfo.isDefeated ? 0 : (flag ? 1 : 0)) != 0);
			foreach (DiplomacyCardState diplomacyCardState in Enum.GetValues(typeof(DiplomacyCardState)))
				game.UI.SetVisible(game.UI.Path(panelName, string.Format("state{0}", (object)diplomacyCardState.ToString())), (playerInfo.isDefeated ? (diplomacyCardState == DiplomacyCardState.PlayerStats ? 1 : 0) : (diplomacyCardState == cardState ? 1 : 0)) != 0);
			game.UI.SetPropertyString(game.UI.Path(panelName, DiplomacyUI.UICardPlayerName), "text", playerInfo.Name);
			DiplomacyUI.SyncEspionageState(game, panelName, playerInfo, updateButtonIds);
			DiplomacyUI.SyncPanelColor(game, game.UI.Path(panelName, DiplomacyUI.UIPanelBackground), playerInfo.PrimaryColor);
			DiplomacyUI.CardStateFunctionMap[cardState](game, game.UI.Path(panelName, string.Format("state{0}", (object)cardState.ToString())), playerInfo, (updateButtonIds ? 1 : 0) != 0);
		}

		internal delegate void SyncCardStateDelegate(
		  App game,
		  string panelName,
		  PlayerInfo playerId,
		  bool updateButtonIds);
	}
}
