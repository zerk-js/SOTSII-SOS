// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.GameSetupUI
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.Strategy;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class GameSetupUI
	{
		private static void SyncPlayerSlotControl(
		  App game,
		  string panelName,
		  int itemId,
		  PlayerSetup player,
		  bool rebuildIDs)
		{
			Vector4 vector4_1 = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
			Vector4 vector4_2 = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
			if (player != null)
			{
				string str1 = null;
				if (!string.IsNullOrEmpty(player.Faction))
					str1 = Path.GetFileNameWithoutExtension(game.AssetDatabase.GetFaction(player.Faction).NoAvatar);
				game.UI.SetVisible(game.UI.Path(panelName, "pnlPlayer"), true);
				game.UI.SetVisible(game.UI.Path(panelName, "pnlEmpty"), false);
				game.UI.SetPropertyString(game.UI.Path(panelName, "btnPlayer"), "id", string.Format("{0}|btnPlayer", (object)itemId));
				game.UI.SetPropertyString(game.UI.Path(panelName, "lblEmpireName"), "text", player.EmpireName ?? string.Empty);
				game.UI.SetPropertyString(game.UI.Path(panelName, "lblName"), "text", player.Name ?? string.Empty);
				if (player.Faction != null)
					game.UI.SetPropertyString(game.UI.Path(panelName, "lblFaction"), "text", !string.IsNullOrEmpty(player.Faction) ? App.GetLocalizedFactionName(player.Faction) ?? string.Empty : string.Empty);
				game.UI.SetPropertyString(game.UI.Path(panelName, "imgAvatar"), "sprite", !string.IsNullOrEmpty(player.Avatar) ? player.Avatar : str1 ?? string.Empty);
				game.UI.SetPropertyString(game.UI.Path(panelName, "imgBadge"), "sprite", !string.IsNullOrEmpty(player.Badge) ? player.Badge : string.Empty);
				if (player.EmpireColor.HasValue && player.EmpireColor.Value >= 0 && player.EmpireColor.Value < Player.DefaultPrimaryPlayerColors.Count<Vector3>())
					vector4_1 = new Vector4(Player.DefaultPrimaryPlayerColors[player.EmpireColor.Value] * (float)byte.MaxValue, (float)byte.MaxValue);
				vector4_2 = new Vector4(player.ShipColor * (float)byte.MaxValue, (float)byte.MaxValue);
				if (player.Ready)
					game.UI.SetVisible(game.UI.Path(panelName, "imgReady"), true);
				else
					game.UI.SetVisible(game.UI.Path(panelName, "imgReady"), false);
				if (player.Locked)
					game.UI.SetVisible(game.UI.Path(panelName, "eliminatedState"), true);
				else
					game.UI.SetVisible(game.UI.Path(panelName, "eliminatedState"), false);
				if (rebuildIDs)
					game.UI.SetPropertyString(game.UI.Path(panelName, "team_button"), "id", "team_button|" + player.slot.ToString());
				game.UI.SetText(game.UI.Path(panelName, "team_button|" + player.slot.ToString(), "team_label"), player.Team != 0 ? App.Localize("@UI_GAMESETUP_TEAM") + " " + (object)player.Team : App.Localize("@UI_GAMESETUP_NOTEAM"));
				game.UI.SetPropertyColor(game.UI.Path(panelName, "team_button|" + player.slot.ToString(), "team_label"), "color", player.Team != 0 ? Player.DefaultPrimaryTeamColors[player.Team - 1] * (float)byte.MaxValue : new Vector3((float)byte.MaxValue, (float)byte.MaxValue, (float)byte.MaxValue));
				foreach (string str2 in new List<string>()
		{
		  "TC",
		  "BC",
		  "BOL1",
		  "BOL2",
		  "BOL3",
		  "BOL4",
		  "BOL5",
		  "BOL6",
		  "BOL7",
		  "BOL8"
		})
				{
					game.UI.SetPropertyColor(game.UI.Path(panelName, "team_button|" + player.slot.ToString(), "idle", str2), "color", player.Team != 0 ? Player.DefaultPrimaryTeamColors[player.Team - 1] * (float)byte.MaxValue : new Vector3((float)byte.MaxValue, (float)byte.MaxValue, (float)byte.MaxValue));
					game.UI.SetPropertyColor(game.UI.Path(panelName, "team_button|" + player.slot.ToString(), "mouse_over", str2), "color", player.Team != 0 ? Player.DefaultPrimaryTeamColors[player.Team - 1] * (float)byte.MaxValue : new Vector3((float)byte.MaxValue, (float)byte.MaxValue, (float)byte.MaxValue));
					game.UI.SetPropertyColor(game.UI.Path(panelName, "team_button|" + player.slot.ToString(), "pressed", str2), "color", player.Team != 0 ? Player.DefaultPrimaryTeamColors[player.Team - 1] * (float)byte.MaxValue : new Vector3((float)byte.MaxValue, (float)byte.MaxValue, (float)byte.MaxValue));
					game.UI.SetPropertyColor(game.UI.Path(panelName, "team_button|" + player.slot.ToString(), "disabled", str2), "color", player.Team != 0 ? Player.DefaultPrimaryTeamColors[player.Team - 1] * (float)byte.MaxValue : new Vector3((float)byte.MaxValue, (float)byte.MaxValue, (float)byte.MaxValue));
				}
				game.UI.SetEnabled(game.UI.Path(panelName, "team_button|" + player.slot.ToString()), (game.GameSetup.IsMultiplayer && game.Network.IsHosting || !game.GameSetup.IsMultiplayer ? (!player.Fixed ? 1 : 0) : 0) != 0);
			}
			else
			{
				game.UI.SetVisible(game.UI.Path(panelName, "pnlPlayer"), false);
				game.UI.SetVisible(game.UI.Path(panelName, "pnlEmpty"), true);
				game.UI.SetPropertyString(game.UI.Path(panelName, "btnPlayer"), "id", string.Format("{0}|btnPlayer", (object)itemId));
			}
			game.UI.SetPropertyColor(game.UI.Path(panelName, "LC"), "color", vector4_1);
			game.UI.SetPropertyColor(game.UI.Path(panelName, "RC"), "color", vector4_1);
			game.UI.SetPropertyColor(game.UI.Path(panelName, "BG"), "color", vector4_1);
			game.UI.SetPropertyColor(game.UI.Path(panelName, "RC"), "color", vector4_2);
		}

		internal static void SyncPlayerSetupWidget(App game, string panelName, PlayerSetup player)
		{
			string str = null;
			if (!string.IsNullOrEmpty(player.Faction))
				str = Path.GetFileNameWithoutExtension(game.AssetDatabase.GetFaction(player.Faction).NoAvatar);
			game.UI.SetPropertyString(game.UI.Path(panelName, "imgFaction"), "sprite", App.GetFactionIcon(player.Faction));
			game.UI.SetPropertyString(game.UI.Path(panelName, "imgAvatar"), "sprite", string.IsNullOrEmpty(player.Avatar) ? str ?? string.Empty : player.Avatar);
			game.UI.SetPropertyString(game.UI.Path(panelName, "imgBadge"), "sprite", player.Badge ?? string.Empty);
			game.UI.SetPropertyString(game.UI.Path(panelName, "lblPlayerName"), "text", player.Name ?? string.Empty);
			game.UI.SetPropertyString(game.UI.Path(panelName, "lblEmpireName"), "text", string.IsNullOrEmpty(player.EmpireName) ? App.Localize("@GAMESETUP_RANDOM_EMPIRE_NAME") : player.EmpireName);
			game.UI.SetPropertyString(game.UI.Path(panelName, "lblFactionDescription"), "text", App.GetFactionDescription(player.Faction));
			Vector4 vector4_1 = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
			Vector4 vector4_2 = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
			if (player.EmpireColor.HasValue)
				vector4_1 = new Vector4(Player.DefaultPrimaryPlayerColors[player.EmpireColor.Value] * (float)byte.MaxValue, (float)byte.MaxValue);
			vector4_2 = new Vector4(player.ShipColor * (float)byte.MaxValue, (float)byte.MaxValue);
			game.UI.SetPropertyColor(game.UI.Path(panelName, "imgEmpireColor"), "color", vector4_1);
			game.UI.SetPropertyColor(game.UI.Path(panelName, "sample"), "color", vector4_2);
			game.UI.SetPropertyColor(game.UI.Path(panelName, "LC"), "color", vector4_1);
			game.UI.SetPropertyColor(game.UI.Path(panelName, "RC"), "color", vector4_1);
			game.UI.SetPropertyColor(game.UI.Path(panelName, "BG"), "color", vector4_1);
			game.UI.SetPropertyColor(game.UI.Path(panelName, "RC"), "color", vector4_2);
		}

		internal static void SyncPlayerListWidget(
		  App game,
		  string panelName,
		  List<PlayerSetup> players,
		  bool rebuildPlayerList = true)
		{
			if (rebuildPlayerList)
				game.UI.ClearItems(panelName);
			foreach (PlayerSetup player in players)
			{
				if (rebuildPlayerList)
					game.UI.AddItem(panelName, string.Empty, player.slot, string.Empty);
				string itemGlobalId = game.UI.GetItemGlobalID(panelName, string.Empty, player.slot, string.Empty);
				GameSetupUI.SyncPlayerSlotControl(game, itemGlobalId, player.slot, player, rebuildPlayerList);
			}
		}

		internal static void ClearPlayerListWidget(App game, string panelName)
		{
			game.UI.ClearItems(panelName);
		}
	}
}
