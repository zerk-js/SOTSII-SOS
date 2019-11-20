// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.LoadGameState
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.UI;

namespace Kerberos.Sots.GameStates
{
	internal class LoadGameState : GameState
	{
		public LoadGameState(App game)
		  : base(game)
		{
		}

		protected override void OnPrepare(GameState prev, object[] parms)
		{
			this.App.UI.CreateDialog((Dialog)new LoadGameDialog(this.App, "fun"), null);
		}

		protected override void OnEnter()
		{
		}

		protected override void OnExit(GameState prev, ExitReason reason)
		{
		}

		protected override void UICommChannel_OnPanelMessage(
		  string panelName,
		  string msgType,
		  string[] msgParams)
		{
		}

		protected override void OnUpdate()
		{
		}

		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}
	}
}
