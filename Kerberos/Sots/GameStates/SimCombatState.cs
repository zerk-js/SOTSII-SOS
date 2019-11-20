// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.SimCombatState
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.PlayerFramework;
using System.IO;

namespace Kerberos.Sots.GameStates
{
	internal class SimCombatState : CommonCombatState
	{
		public SimCombatState(App game)
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
			int num = msgType == "button_clicked" ? 1 : 0;
		}

		protected override void OnCombatEnding()
		{
		}

		protected override void OnPrepare(GameState prev, object[] stateParams)
		{
			this.SimMode = true;
			base.OnPrepare(prev, stateParams);
			this.App.UI.LoadScreen("SimCombat");
		}

		protected override void OnEnter()
		{
			base.OnEnter();
			this.App.UI.SetScreen("SimCombat");
			this.App.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_COMBAT_CONNECT_UI, (object)this.Combat.ObjectID, (object)"SimCombat");
			this.BuildAvatarList("avatarList");
		}

		protected void BuildAvatarList(string panelName)
		{
			this.App.UI.ClearItems(panelName);
			int userItemId = 0;
			foreach (Player player in this.PlayersInCombat)
			{
				this.App.UI.AddItem(panelName, string.Empty, userItemId, string.Empty);
				string itemGlobalId = this.App.UI.GetItemGlobalID(panelName, string.Empty, userItemId, string.Empty);
				PlayerInfo playerInfo = this.App.GameDatabase.GetPlayerInfo(player.ID);
				this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "playeravatar"), "sprite", Path.GetFileNameWithoutExtension(playerInfo.AvatarAssetPath));
				this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "badge"), "sprite", Path.GetFileNameWithoutExtension(playerInfo.BadgeAssetPath));
				this.App.UI.SetPropertyColor(this.App.UI.Path(itemGlobalId, "primaryColor"), "color", playerInfo.PrimaryColor * (float)byte.MaxValue);
				this.App.UI.SetPropertyColor(this.App.UI.Path(itemGlobalId, "secondaryColor"), "color", playerInfo.SecondaryColor * (float)byte.MaxValue);
				this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "name"), "text", playerInfo.Name);
				++userItemId;
			}
		}

		protected override void OnExit(GameState prev, ExitReason reason)
		{
			base.OnExit(prev, reason);
		}

		protected override void SyncPlayerList()
		{
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
