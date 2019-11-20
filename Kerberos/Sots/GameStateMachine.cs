// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStateMachine
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.GameStates;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Kerberos.Sots
{
	internal class GameStateMachine : IEnumerable<GameState>, IEnumerable
	{
		private readonly List<GameState> _gameStates = new List<GameState>();
		private object[] _pendingParms = new object[0];
		private GameState _currentGameState;
		private GameState _pendingGameState;
		private GameState _previousGameState;
		private bool _pendingGameStateIsReady;
		private bool _switchGameStateNow;
		private bool _booted;

		public GameState CurrentState
		{
			get
			{
				return this._currentGameState;
			}
		}

		public GameState PendingState
		{
			get
			{
				return this._pendingGameState;
			}
		}

		public GameState PreviousState
		{
			get
			{
				return this._previousGameState;
			}
		}

		public bool IsBooted
		{
			get
			{
				return this._booted;
			}
		}

		public void Exit()
		{
			try
			{
				if (this._pendingGameState == null)
					return;
				GameState pendingGameState = this._pendingGameState;
				this._pendingGameState = (GameState)null;
				pendingGameState.Exit((GameState)null, ExitReason.TransitionInterrupted);
			}
			finally
			{
				if (this._currentGameState != null)
				{
					GameState currentGameState = this._currentGameState;
					this._currentGameState = (GameState)null;
					currentGameState.Exit((GameState)null, ExitReason.TransitionCompleted);
				}
			}
		}

		public void Add(GameState value)
		{
			this._gameStates.Add(value);
		}

		public GameState GetGameState(string name)
		{
			foreach (GameState gameState in this._gameStates)
			{
				if (gameState.Name.Equals(name, StringComparison.InvariantCulture))
					return gameState;
			}
			throw new ArgumentOutOfRangeException(nameof(name), "Unknown GameState: " + name + ", from: " + (object)this._gameStates + ".");
		}

		private void PrepareGameStateCore(GameState value, object[] parms)
		{
			if (value == this._currentGameState)
				return;
			if (value == this._pendingGameState)
				return;
			try
			{
				value.Prepare(this._currentGameState, parms);
			}
			catch
			{
				try
				{
					value.Exit(this._previousGameState, ExitReason.TransitionInterrupted);
				}
				finally
				{
					this._currentGameState = this._previousGameState;
					this._pendingGameState = (GameState)null;
					this._switchGameStateNow = false;
				}
				throw;
			}
			bool flag = false;
			try
			{
				flag = value.IsReady();
			}
			finally
			{
				try
				{
					if (this._pendingGameState != null)
					{
						this._switchGameStateNow = false;
						this._pendingGameState.Exit(value, ExitReason.TransitionInterrupted);
					}
				}
				finally
				{
					this._pendingGameStateIsReady = flag;
					this._pendingGameState = value;
				}
			}
		}

		public bool PrepareGameState(GameState value, object[] parms)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));
			this._pendingParms = parms ?? new object[0];
			if (this._pendingGameState != value)
				this.PrepareGameStateCore(value, parms);
			if (!this._pendingGameStateIsReady)
				return false;
			this._pendingParms = new object[0];
			return true;
		}

		public bool SwitchToPreparedGameState()
		{
			if (this._pendingGameState == null || !this._pendingGameStateIsReady)
				return false;
			this._switchGameStateNow = true;
			this._booted = true;
			return true;
		}

		public bool SwitchGameState(GameState value, object[] parms)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));
			if (!this.PrepareGameState(value, parms))
				return false;
			this.SwitchToPreparedGameState();
			return true;
		}

		public void Update()
		{
			try
			{
				if (this._pendingGameState != null)
				{
					if (!this._pendingGameStateIsReady)
					{
						this._pendingGameStateIsReady = this._pendingGameState.IsReady();
						if (this._pendingGameStateIsReady)
							this.SwitchGameState(this._pendingGameState, this._pendingParms);
					}
				}
			}
			finally
			{
				if (this._switchGameStateNow)
				{
					this._switchGameStateNow = false;
					try
					{
						if (this._currentGameState != null)
							this._currentGameState.Exit(this._pendingGameState, ExitReason.TransitionCompleted);
					}
					finally
					{
						GameState pendingGameState = this._pendingGameState;
						this._pendingGameState = (GameState)null;
						if (this._currentGameState != null)
						{
							if (this._currentGameState.IsTransitionState)
								goto label_12;
						}
						this._previousGameState = this._currentGameState;
					label_12:
						try
						{
							pendingGameState.Enter();
							this._currentGameState = pendingGameState;
						}
						catch
						{
							try
							{
								pendingGameState.Exit((GameState)null, ExitReason.TransitionInterrupted);
							}
							finally
							{
								this._currentGameState = (GameState)null;
							}
							throw;
						}
					}
				}
			}
			if (!this._booted || this._currentGameState == null)
				return;
			this._currentGameState.Update();
		}

		public void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
			switch (messageID)
			{
				case InteropMessageID.IMID_SCRIPT_SET_PAUSE_STATE:
				case InteropMessageID.IMID_SCRIPT_SET_COMBAT_ACTIVE:
				case InteropMessageID.IMID_SCRIPT_PLAYER_DIPLO_CHANGED:
				case InteropMessageID.IMID_SCRIPT_ZONE_OWNER_CHANGED:
				case InteropMessageID.IMID_SCRIPT_OBJECT_ADD:
				case InteropMessageID.IMID_SCRIPT_OBJECTS_ADD:
				case InteropMessageID.IMID_SCRIPT_OBJECT_RELEASE:
				case InteropMessageID.IMID_SCRIPT_OBJECTS_RELEASE:
				case InteropMessageID.IMID_SCRIPT_COMBAT_ENDED:
				case InteropMessageID.IMID_SCRIPT_START_SENDINGDATA:
				case InteropMessageID.IMID_SCRIPT_COMBAT_DATA_SHIP:
				case InteropMessageID.IMID_SCRIPT_COMBAT_DATA_PLANET:
				case InteropMessageID.IMID_SCRIPT_COMBAT_DATA_STAR:
				case InteropMessageID.IMID_SCRIPT_COMBAT_DESTROYED_SHIPS:
				case InteropMessageID.IMID_SCRIPT_COMBAT_CAPTURED_SHIPS:
				case InteropMessageID.IMID_SCRIPT_COMBAT_WEAPON_DAMAGE_STATS:
				case InteropMessageID.IMID_SCRIPT_COMBAT_ZONE_STATES:
				case InteropMessageID.IMID_SCRIPT_END_SENDINGDATA:
				case InteropMessageID.IMID_SCRIPT_END_DELAYCOMPLETE:
				case InteropMessageID.IMID_SCRIPT_SYNC_FLEET_POSITIONS:
				case InteropMessageID.IMID_SCRIPT_SYNC_DEFENSE_POSITIONS:
				case InteropMessageID.IMID_SCRIPT_SYNC_DEFENSEBOAT_DATA:
					this._currentGameState.OnEngineMessage(messageID, mr);
					break;
				case InteropMessageID.IMID_SCRIPT_MOVE_ORDER:
					if (!(this._currentGameState is CombatState))
						break;
					this._currentGameState.OnEngineMessage(messageID, mr);
					break;
				default:
					App.Log.Warn("Unhandled message (id=" + (object)messageID + ").", "state");
					break;
			}
		}

		public IEnumerator<GameState> GetEnumerator()
		{
			return (IEnumerator<GameState>)this._gameStates.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator)this._gameStates.GetEnumerator();
		}
	}
}
