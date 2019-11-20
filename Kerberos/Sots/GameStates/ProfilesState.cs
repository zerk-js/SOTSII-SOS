// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.ProfilesState
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;

namespace Kerberos.Sots.GameStates
{
	internal class ProfilesState : GameState
	{
		public ProfilesState(App game)
		  : base(game)
		{
		}

		protected override void OnPrepare(GameState prev, object[] parms)
		{
			this.App.UI.LoadScreen("Profiles");
		}

		protected override void OnEnter()
		{
			this.App.UI.SetScreen("Profiles");
		}

		protected override void OnExit(GameState prev, ExitReason reason)
		{
		}

		protected override void UICommChannel_OnPanelMessage(
		  string panelName,
		  string msgType,
		  string[] msgParams)
		{
			if (!(msgType == "button_clicked") || !(panelName == "gameBackButton"))
				return;
			this.App.SwitchGameState("MainMenuState");
		}

		protected override void OnUpdate()
		{
		}

		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}
	}
}
