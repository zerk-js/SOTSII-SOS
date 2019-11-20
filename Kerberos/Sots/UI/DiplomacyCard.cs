// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.DiplomacyCard
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Strategy;
using System.IO;

namespace Kerberos.Sots.UI
{
	internal class DiplomacyCard : PanelBinding
	{
		private static string UIsecondaryColor = "secondaryColor";
		private static string UIempireColor = "empireColor";
		private static string UIavatar = "avatar_doodle";
		private static string UIbadge = "badge";
		private static string UIpeace = "peace";
		private static string UIwar = "war";
		private static string UImoodHatred = "moodHatred";
		private static string UImoodHostile = "moodHostile";
		private static string UImoodDistrust = "moodDistrust";
		private static string UImoodIndifferent = "moodIndifferent";
		private static string UImoodTrust = "moodTrust";
		private static string UImoodFriend = "moodFriend";
		private static string UImoodLove = "moodLove";
		private App App;
		private PlayerInfo Player;

		public DiplomacyCard(App game, int playerid, UICommChannel ui, string id)
		  : base(ui, id)
		{
			this.App = game;
			this.Player = this.App.GameDatabase.GetPlayerInfo(playerid);
		}

		protected override void OnPanelMessage(string panelId, string msgType, string[] msgParams)
		{
			base.OnPanelMessage(panelId, msgType, msgParams);
		}

		public void Initialize()
		{
			this.App.UI.SetPropertyColor(this.UI.Path(this.ID, DiplomacyCard.UIsecondaryColor), "color", this.Player.SecondaryColor * (float)byte.MaxValue);
			this.App.UI.SetPropertyColor(this.UI.Path(this.ID, DiplomacyCard.UIempireColor), "color", this.Player.PrimaryColor * (float)byte.MaxValue);
			this.App.UI.SetPropertyString(this.UI.Path(this.ID, DiplomacyCard.UIavatar), "sprite", Path.GetFileNameWithoutExtension(this.Player.AvatarAssetPath));
			this.App.UI.SetPropertyString(this.UI.Path(this.ID, DiplomacyCard.UIbadge), "sprite", Path.GetFileNameWithoutExtension(this.Player.BadgeAssetPath));
			if (this.App.GameDatabase.GetDiplomacyStateBetweenPlayers(this.App.LocalPlayer.ID, this.Player.ID) == DiplomacyState.WAR)
			{
				this.App.UI.SetVisible(this.UI.Path(this.ID, DiplomacyCard.UIpeace), false);
				this.App.UI.SetVisible(this.UI.Path(this.ID, DiplomacyCard.UIwar), true);
			}
			else
			{
				this.App.UI.SetVisible(this.UI.Path(this.ID, DiplomacyCard.UIpeace), true);
				this.App.UI.SetVisible(this.UI.Path(this.ID, DiplomacyCard.UIwar), false);
			}
			DiplomacyInfo diplomacyInfo = this.App.GameDatabase.GetDiplomacyInfo(this.Player.ID, this.App.LocalPlayer.ID);
			this.App.UI.SetVisible(this.UI.Path(this.ID, DiplomacyCard.UImoodHatred), false);
			this.App.UI.SetVisible(this.UI.Path(this.ID, DiplomacyCard.UImoodHostile), false);
			this.App.UI.SetVisible(this.UI.Path(this.ID, DiplomacyCard.UImoodDistrust), false);
			this.App.UI.SetVisible(this.UI.Path(this.ID, DiplomacyCard.UImoodIndifferent), false);
			this.App.UI.SetVisible(this.UI.Path(this.ID, DiplomacyCard.UImoodTrust), false);
			this.App.UI.SetVisible(this.UI.Path(this.ID, DiplomacyCard.UImoodFriend), false);
			this.App.UI.SetVisible(this.UI.Path(this.ID, DiplomacyCard.UImoodLove), false);
			switch (diplomacyInfo.GetDiplomaticMood())
			{
				case DiplomaticMood.Hatred:
					this.App.UI.SetVisible(this.UI.Path(this.ID, DiplomacyCard.UImoodHatred), true);
					break;
				case DiplomaticMood.Hostility:
					this.App.UI.SetVisible(this.UI.Path(this.ID, DiplomacyCard.UImoodHostile), true);
					break;
				case DiplomaticMood.Distrust:
					this.App.UI.SetVisible(this.UI.Path(this.ID, DiplomacyCard.UImoodDistrust), true);
					break;
				case DiplomaticMood.Indifference:
					this.App.UI.SetVisible(this.UI.Path(this.ID, DiplomacyCard.UImoodIndifferent), true);
					break;
				case DiplomaticMood.Trust:
					this.App.UI.SetVisible(this.UI.Path(this.ID, DiplomacyCard.UImoodTrust), true);
					break;
				case DiplomaticMood.Friendship:
					this.App.UI.SetVisible(this.UI.Path(this.ID, DiplomacyCard.UImoodFriend), true);
					break;
				case DiplomaticMood.Love:
					this.App.UI.SetVisible(this.UI.Path(this.ID, DiplomacyCard.UImoodLove), true);
					break;
			}
		}
	}
}
