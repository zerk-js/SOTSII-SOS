// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameState
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;

namespace Kerberos.Sots
{
	internal abstract class GameState
	{
		private bool _guiLoaded;
		private bool _entered;

		protected App App { get; private set; }

		public string Name
		{
			get
			{
				return this.GetType().Name;
			}
		}

		private static void Trace(string message)
		{
			App.Log.Trace(message, "state");
		}

		private static void Warn(string message)
		{
			App.Log.Warn(message, "state");
		}

		public virtual bool IsTransitionState
		{
			get
			{
				return false;
			}
		}

		public virtual bool IsScreenState
		{
			get
			{
				return true;
			}
		}

		protected abstract void OnPrepare(GameState prev, object[] parms);

		protected abstract void OnEnter();

		protected abstract void OnExit(GameState next, ExitReason reason);

		protected abstract void OnUpdate();

		public virtual void AddGameObject(IGameObject gameObject, bool autoSetActive = false)
		{
		}

		public virtual void RemoveGameObject(IGameObject gameObject)
		{
		}

		public abstract void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr);

		protected virtual void UICommChannel_OnPanelMessage(
		  string panelName,
		  string msgType,
		  string[] msgParams)
		{
		}

		internal void Prepare(GameState prev, object[] parms)
		{
			ScriptHost.Engine.RenderingEnabled = false;
			try
			{
				GameState.Trace(string.Format("Preparing {0} for transition from {1}.", (object)this.Name, prev != null ? (object)prev.Name : (object)"nothing"));
				this.App.UI.PanelMessage += new UIEventPanelMessage(this.UICommChannel_PanelMessage);
				this.OnPrepare(prev, parms);
			}
			finally
			{
				ScriptHost.Engine.RenderingEnabled = true;
			}
		}

		internal void Enter()
		{
			ScriptHost.Engine.RenderingEnabled = false;
			try
			{
				GameState.Trace(string.Format("Entering {0}.", (object)this.Name));
				this.App.UI.UnlockUI();
				if (this.App.GameSettings.AudioEnabled)
					this.App.PostEnableAllSounds();
				this.OnEnter();
				this.App.HotKeyManager.SyncKeyProfile(this.Name);
				this.App.HotKeyManager.SetEnabled(true);
				this.App.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_EXITSTATE);
				this._entered = true;
				if (this.App.Game == null)
					return;
				this.App.Game.ShowCombatDialog(true, this);
			}
			finally
			{
				ScriptHost.Engine.RenderingEnabled = true;
			}
		}

		internal void Exit(GameState next, ExitReason reason)
		{
			GameState.Trace(string.Format("Exiting {0}.", (object)this.Name));
			this.App.UI.PanelMessage -= new UIEventPanelMessage(this.UICommChannel_PanelMessage);
			if (this.App.Game != null)
				this.App.Game.ShowCombatDialog(false, (GameState)null);
			this.OnExit(next, reason);
			this._entered = false;
		}

		internal void Update()
		{
			this.OnUpdate();
		}

		protected GameState(App game)
		{
			this.App = game;
		}

		public override string ToString()
		{
			return this.Name;
		}

		private void UICommChannel_PanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "screen_loaded")
			{
				this._guiLoaded = true;
			}
			else
			{
				if (this.IsScreenState && (!this._guiLoaded || !this._entered))
					return;
				this.UICommChannel_OnPanelMessage(panelName, msgType, msgParams);
			}
		}

		public virtual bool IsReady()
		{
			return !this.IsScreenState || this._guiLoaded;
		}
	}
}
