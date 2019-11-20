// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.CombatState
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.Strategy;
using System.Collections.Generic;
using System.IO;

namespace Kerberos.Sots.GameStates
{
	internal class CombatState : CommonCombatState
	{
		private static readonly string UIExitButton = "gameExitButton";

		public CombatState(App game)
		  : base(game)
		{
		}

		protected override GameState GetExitState()
		{
			return (GameState)this.App.GetGameState<StarMapState>();
		}

		protected override void UICommChannel_OnPanelMessage(
		  string panelName,
		  string msgType,
		  string[] msgParams)
		{
			if (!(msgType == "button_clicked") || !(panelName == CombatState.UIExitButton) || !this.EndCombat())
				return;
			this.App.UI.SetEnabled(CombatState.UIExitButton, false);
		}

		protected override void OnCombatEnding()
		{
			this.App.UI.SetEnabled(CombatState.UIExitButton, false);
		}

		protected override void OnPrepare(GameState prev, object[] stateParams)
		{
			this.SimMode = false;
			base.OnPrepare(prev, stateParams);
			this.App.UI.LoadScreen("Combat");
		}

		protected override void OnEnter()
		{
			base.OnEnter();
			this.App.UI.SetScreen("Combat");
			this.App.UI.SetEnabled(CombatState.UIExitButton, true);
			this.App.UI.SetPropertyInt("gameWeaponsPanel", "combat_input", this.Input.ObjectID);
			this.App.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_COMBAT_CONNECT_UI, (object)this.Combat.ObjectID, (object)"Combat");
			this.SyncPlayerList();
		}

		protected override void SyncPlayerList()
		{
			this.App.UI.ClearItems("combatPlayers");
			this.App.UI.ClearItems("remainingPlayers");
			List<Player> playerList1 = new List<Player>();
			List<Player> playerList2 = new List<Player>();
			List<Player> playerList3 = new List<Player>();
			foreach (Player player in this._playersInCombat)
			{
				if (player.ID == this.App.LocalPlayer.ID)
				{
					playerList1.Add(player);
				}
				else
				{
					switch (this.GetDiplomacyState(this.App.LocalPlayer.ID, player.ID))
					{
						case DiplomacyState.CEASE_FIRE:
						case DiplomacyState.NON_AGGRESSION:
						case DiplomacyState.NEUTRAL:
							playerList2.Add(player);
							continue;
						case DiplomacyState.WAR:
							playerList3.Add(player);
							continue;
						case DiplomacyState.ALLIED:
						case DiplomacyState.PEACE:
							playerList1.Add(player);
							continue;
						default:
							continue;
					}
				}
			}
			if (this._systemId > 0)
				this.App.UI.SetPropertyString("systemName", "text", this.App.GameDatabase.GetStarSystemInfo(this._systemId).Name);
			else
				this.App.UI.SetPropertyString("systemName", "text", App.Localize("@ADMIRAL_LOCATION_DEEP_SPACE"));
			foreach (Player player in playerList1)
			{
				if (player.ID != this.App.LocalPlayer.ID)
				{
					this.App.UI.AddItem("remainingPlayers", "", player.ID, "", "smallPlayerCard");
					this.SyncPlayerCard(this.App.UI.GetItemGlobalID("remainingPlayers", "", player.ID, ""), player.ID, true, false);
				}
			}
			if (playerList2.Count > 0)
			{
				foreach (Player player in playerList2)
				{
					this.App.UI.AddItem("remainingPlayers", "", player.ID, "", "smallPlayerCard");
					this.SyncPlayerCard(this.App.UI.GetItemGlobalID("remainingPlayers", "", player.ID, ""), player.ID, false, false);
				}
			}
			if (playerList3.Count > 0)
			{
				foreach (Player player in playerList3)
				{
					this.App.UI.AddItem("combatPlayers", "", player.ID, "", "smallPlayerCard");
					this.SyncPlayerCard(this.App.UI.GetItemGlobalID("combatPlayers", "", player.ID, ""), player.ID, false, false);
				}
				this.App.UI.AddItem("combatPlayers", "", 99999999, "", "smallPlayerCard");
				this.SyncPlayerCard(this.App.UI.GetItemGlobalID("combatPlayers", "", 99999999, ""), 99999999, false, true);
			}
			this.App.UI.AddItem("combatPlayers", "", this.App.LocalPlayer.ID, "", "smallPlayerCard");
			this.SyncPlayerCard(this.App.UI.GetItemGlobalID("combatPlayers", "", this.App.LocalPlayer.ID, ""), this.App.LocalPlayer.ID, false, false);
		}

		protected void SyncPlayerCard(string card, int playerID, bool isally, bool vscard = false)
		{
			if (!vscard)
			{
				PlayerInfo playerInfo = this.App.GameDatabase.GetPlayerInfo(playerID);
				this.App.UI.SetPropertyString(this.App.UI.Path(card, "smallAvatar"), "sprite", Path.GetFileNameWithoutExtension(playerInfo.AvatarAssetPath));
				this.App.UI.SetVisible(this.App.UI.Path(card, "ally"), (isally ? 1 : 0) != 0);
				CombatState.SetPlayerCardOutlineColor(this.App, this.App.UI.Path(card, "bgPlayerColor"), playerInfo.PrimaryColor);
			}
			else
			{
				this.App.UI.SetVisible(this.App.UI.Path(card, "ally"), false);
				this.App.UI.SetVisible(this.App.UI.Path(card, "smallAvatar"), false);
				this.App.UI.SetVisible(this.App.UI.Path(card, "vsText"), true);
				this.App.UI.SetVisible(this.App.UI.Path(card, "bgPlayerColor"), false);
			}
		}

		public static void SetPlayerCardOutlineColor(App game, string panelName, Vector3 color)
		{
			foreach (string str in new List<string>()
	  {
		"BOL1",
		"BOL2",
		"BOL3",
		"BOL4",
		"BOL5",
		"BOL6",
		"BOL7",
		"BOL8",
		"L_Cap",
		"R_Cap",
		"PC_OWNER_S"
	  })
				game.UI.SetPropertyColorNormalized(game.UI.Path(panelName, str), nameof(color), color);
		}

		protected override void OnExit(GameState prev, ExitReason reason)
		{
			base.OnExit(prev, reason);
		}

		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
			if (messageID == InteropMessageID.IMID_SCRIPT_MOVE_ORDER)
			{
				if (this.GetCommanderForPlayerID(mr.ReadInteger()) == null)
					;
			}
			else
				base.OnEngineMessage(messageID, mr);
		}
	}
}
