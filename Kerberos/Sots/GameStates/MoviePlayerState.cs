// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.MoviePlayerState
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using System;

namespace Kerberos.Sots.GameStates
{
	internal class MoviePlayerState : GameState
	{
		private string _movie;
		private Action _action;
		private bool _actionPerformed;
		private bool _preparingState;
		private GameState _nextState;
		private object[] _nextStateParams;
		private bool _isMultiplayer;
		private bool _movieDone;
		private bool _switchStates;

		public bool SwitchStates
		{
			get
			{
				return this._switchStates;
			}
			set
			{
				this._switchStates = value;
			}
		}

		private bool IsNextStateReady()
		{
			if (this._nextState != null)
				return this._nextState.IsReady();
			return false;
		}

		public override bool IsTransitionState
		{
			get
			{
				return true;
			}
		}

		public MoviePlayerState(App game)
		  : base(game)
		{
		}

		protected override void OnPrepare(GameState prev, object[] parms)
		{
			if (parms.Length > 0)
			{
				this._movie = parms[0] as string;
				this._action = (Action)parms[1];
				this._nextState = (GameState)parms[2];
				this._isMultiplayer = (bool)parms[3];
				this._nextStateParams = (object[])parms[4];
			}
			else
			{
				this._movie = "movies\\Paradox_Interactive_ID_uncompressed.bik";
				this._action = (Action)null;
				this._nextState = (GameState)this.App.GetGameState<MainMenuState>();
				this._isMultiplayer = false;
			}
			this.App.UI.LoadScreen("MoviePlayer");
		}

		protected override void OnEnter()
		{
			this.App.UI.GameEvent += new UIEventGameEvent(this.UICommChannel_GameEvent);
			this.App.UI.SetScreen("MoviePlayer");
			this.App.UI.SetPropertyString("movie", "movie", this._movie);
		}

		protected override void OnExit(GameState next, ExitReason reason)
		{
			this.App.UI.SetPropertyBool("movie", "movie_done", true);
			this._movie = null;
			this._action = (Action)null;
			this._actionPerformed = false;
			this._preparingState = false;
			this._nextState = (GameState)null;
			this._nextStateParams = (object[])null;
			this.App.UI.GameEvent -= new UIEventGameEvent(this.UICommChannel_GameEvent);
		}

		protected override void OnUpdate()
		{
			if (!this._movieDone)
				return;
			if (!this._actionPerformed)
			{
				this._actionPerformed = true;
				if (this._action != null)
					this._action();
			}
			if (this._actionPerformed && !this._preparingState)
			{
				this._preparingState = true;
				this.App.PrepareGameState(this._nextState, this._nextStateParams);
			}
			if (this._nextState.IsReady())
				this.App.SwitchToPreparedGameState();
			if (!this._isMultiplayer)
				return;
			if (this._nextState.IsReady())
				this.App.Network.Ready();
			if (!this._switchStates || !this._nextState.IsReady())
				return;
			this.App.SwitchToPreparedGameState();
		}

		private void UICommChannel_GameEvent(string eventName, string[] eventParams)
		{
			if (!(eventName == "movie_done"))
				return;
			this._movieDone = true;
		}

		protected override void UICommChannel_OnPanelMessage(
		  string panelName,
		  string msgType,
		  string[] msgParams)
		{
			if (!(msgType == "button_clicked"))
				return;
			this.App.UI.SetPropertyBool("movie", "movie_done", true);
		}

		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}
	}
}
