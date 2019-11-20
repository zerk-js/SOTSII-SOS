// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.LoadingScreenState
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using System;

namespace Kerberos.Sots.GameStates
{
	internal class LoadingScreenState : GameState
	{
		private float _minTime;
		private string _text;
		private string _image;
		private Action _action;
		private LoadingFinishedDelegate _loadingFinishedDelegate;
		private bool _actionPerformed;
		private bool _preparingState;
		private GameState _nextState;
		private GameState _prevState;
		private object[] _nextStateParams;
		private DateTime _startTime;
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

		public GameState PreviousState
		{
			get
			{
				return this._prevState;
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

		public LoadingScreenState(App game)
		  : base(game)
		{
		}

		protected override void OnPrepare(GameState prev, object[] parms)
		{
			this._minTime = (float)parms[0];
			this._text = parms[1] as string;
			this._image = parms[2] as string;
			this._action = (Action)parms[3];
			this._loadingFinishedDelegate = (LoadingFinishedDelegate)parms[4];
			this._nextState = (GameState)parms[5];
			this._nextStateParams = (object[])parms[6];
			this._actionPerformed = false;
			this._prevState = prev;
			this.App.UI.LoadScreen("LoadingScreen");
		}

		protected override void OnEnter()
		{
			this.App.UI.SetScreen("LoadingScreen");
			this._startTime = DateTime.Now;
			this.App.UI.SetText("SplashText", this._text);
			this.App.UI.SetPropertyString("SplashImage", "texture", this._image);
			this.App.UI.Update();
		}

		protected override void OnExit(GameState next, ExitReason reason)
		{
			this.App.UI.DeleteScreen("LoadingScreen");
			this._minTime = 0.0f;
			this._text = null;
			this._image = null;
			this._action = (Action)null;
			this._actionPerformed = false;
			this._preparingState = false;
			this._nextState = (GameState)null;
			this._nextStateParams = (object[])null;
		}

		protected override void OnUpdate()
		{
			TimeSpan timeSpan = DateTime.Now - this._startTime;
			if (this._loadingFinishedDelegate != null && !this._loadingFinishedDelegate())
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
			if (!(timeSpan >= TimeSpan.FromSeconds((double)this._minTime + 2.0)) || !this._nextState.IsReady())
				return;
			this.App.SwitchToPreparedGameState();
		}

		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}
	}
}
