// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.StarmapUI
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using System.IO;

namespace Kerberos.Sots.UI
{
	internal class StarmapUI
	{
		public static readonly string UIPlayerCardName = "lblPlayerName";
		public static readonly string UIPlayerCardAvatar = "imgAvatar";
		public static readonly string UIPlayerCardBadge = "imgBadge";
		public static readonly string UIPlayerCardRelation = "imgRelation";

		public static void SyncPlayerCard(App game, string panelName, int playerId)
		{
			StarmapUI.SyncPlayerCard(game, panelName, game.GameDatabase.GetPlayerInfo(playerId));
		}

		public static void SyncPlayerCard(App game, string panelName, PlayerInfo pi)
		{
			if (pi == null)
				return;
			game.UI.SetPropertyString(game.UI.Path(panelName, StarmapUI.UIPlayerCardName), "text", pi.Name);
			game.UI.SetPropertyString(game.UI.Path(panelName, StarmapUI.UIPlayerCardAvatar), "sprite", Path.GetFileNameWithoutExtension(pi.AvatarAssetPath));
			game.UI.SetPropertyString(game.UI.Path(panelName, StarmapUI.UIPlayerCardBadge), "sprite", Path.GetFileNameWithoutExtension(pi.BadgeAssetPath));
			DiplomaticMood diplomaticMood = game.GameDatabase.GetDiplomacyInfo(pi.ID, game.Game.LocalPlayer.ID).GetDiplomaticMood();
			game.UI.SetVisible(game.UI.Path(panelName, StarmapUI.UIPlayerCardRelation), true);
			if (diplomaticMood == DiplomaticMood.Love && pi.ID != game.LocalPlayer.ID)
				game.UI.SetPropertyString(game.UI.Path(panelName, StarmapUI.UIPlayerCardRelation), "sprite", "Love");
			else if (diplomaticMood == DiplomaticMood.Hatred && pi.ID != game.LocalPlayer.ID)
				game.UI.SetPropertyString(game.UI.Path(panelName, StarmapUI.UIPlayerCardRelation), "sprite", "Hate");
			else
				game.UI.SetVisible(game.UI.Path(panelName, StarmapUI.UIPlayerCardRelation), false);
		}

		public static void SyncPlayerCard(
		  App game,
		  string panelName,
		  string playerName,
		  string avatarSprite,
		  string badgeSprite,
		  string relationSprite)
		{
			game.UI.SetPropertyString(game.UI.Path(panelName, StarmapUI.UIPlayerCardName), "text", playerName);
			game.UI.SetPropertyString(game.UI.Path(panelName, StarmapUI.UIPlayerCardAvatar), "sprite", avatarSprite);
			game.UI.SetPropertyString(game.UI.Path(panelName, StarmapUI.UIPlayerCardBadge), "sprite", badgeSprite);
			game.UI.SetPropertyString(game.UI.Path(panelName, StarmapUI.UIPlayerCardRelation), "sprite", relationSprite);
		}
	}
}
