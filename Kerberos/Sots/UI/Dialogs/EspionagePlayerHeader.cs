// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.Dialogs.EspionagePlayerHeader
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Strategy;

namespace Kerberos.Sots.UI.Dialogs
{
	internal class EspionagePlayerHeader : PanelBinding
	{
		private readonly GameSession _game;
		private readonly Label _playerNameLabel;
		private readonly Image _avatarImage;
		private readonly Image _badgeImage;
		private readonly Image _relationImage;

		public EspionagePlayerHeader(GameSession game, string dialogID)
		  : base(game.UI, dialogID)
		{
			this._game = game;
			this._playerNameLabel = new Label(this.UI, this.UI.Path(this.ID, "lblPlayerName"));
			this._avatarImage = new Image(this.UI, this.UI.Path(this.ID, "imgAvatar"));
			this._badgeImage = new Image(this.UI, this.UI.Path(this.ID, "imgBadge"));
			this._relationImage = new Image(this.UI, this.UI.Path(this.ID, "imgRelation"));
			this.AddPanels((PanelBinding)this._playerNameLabel, (PanelBinding)this._avatarImage, (PanelBinding)this._badgeImage, (PanelBinding)this._relationImage);
		}

		public void UpdateFromPlayerInfo(int localPlayerID, PlayerInfo targetPlayerInfo)
		{
			this._playerNameLabel.SetText(targetPlayerInfo.Name);
			this._avatarImage.SetTexture(targetPlayerInfo.AvatarAssetPath);
			this._badgeImage.SetTexture(targetPlayerInfo.BadgeAssetPath);
			DiplomacyInfo diplomacyInfo = this._game.GameDatabase.GetDiplomacyInfo(localPlayerID, targetPlayerInfo.ID);
			string spriteName = null;
			if (diplomacyInfo != null)
				spriteName = diplomacyInfo.GetDiplomaticMoodSprite();
			if (!string.IsNullOrEmpty(spriteName))
			{
				this._relationImage.SetVisible(true);
				this._relationImage.SetSprite(spriteName);
			}
			else
				this._relationImage.SetVisible(false);
		}
	}
}
