// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.SplashState
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;

namespace Kerberos.Sots.GameStates
{
	internal class SplashState : GameState
	{
		private GameState _initialState;

		public SplashState(App game)
		  : base(game)
		{
		}

		protected override void OnPrepare(GameState prev, object[] parms)
		{
			this._initialState = (GameState)parms[0];
			this.App.UI.LoadScreen("Splash");
		}

		protected override void OnEnter()
		{
			this.App.UI.SetScreen("Splash");
			this.App.UI.Update();
			if (!this.App.IsInitialized())
				this.App.Initialize();
			this.App.PostRequestGuiSound("universal_kerbgrowl");
		}

		protected override void OnExit(GameState prev, ExitReason reason)
		{
			this._initialState = (GameState)null;
			this.App.UI.DeleteScreen("Splash");
		}

		protected override void OnUpdate()
		{
			if (!this.App.IsInitialized())
				return;
			this.App.SwitchGameState(this._initialState);
		}

		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}
	}
}
