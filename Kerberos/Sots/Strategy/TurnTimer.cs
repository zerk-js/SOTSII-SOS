// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.TurnTimer
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;

namespace Kerberos.Sots.Strategy
{
	internal class TurnTimer
	{
		private DateTime _turnTimer = DateTime.Now;
		private DateTime _turnTimerSpan = DateTime.Now;
		private TimeSpan _previousTurnTime = DateTime.Now - DateTime.Now;
		private float _strategicTurnLength = float.MaxValue;
		private bool _turnTimerRunning;
		private App App;

		public float StrategicTurnLength
		{
			get
			{
				return this._strategicTurnLength;
			}
			set
			{
				this._strategicTurnLength = value;
			}
		}

		public TurnTimer(App game)
		{
			this.App = game;
		}

		public bool IsRunning()
		{
			return this._turnTimerRunning;
		}

		public void StartTurnTimer()
		{
			this._previousTurnTime = this.GetTurnTime();
			this._turnTimer = DateTime.Now;
			this._turnTimerRunning = true;
		}

		public TimeSpan GetTurnTime()
		{
			if (this.App.GameSetup != null && this.App.GameSetup.IsMultiplayer && !this.App.Network.IsHosting)
				return new TimeSpan(0, 0, (int)Math.Round((double)this.App.Network.TurnSeconds, 0, MidpointRounding.AwayFromZero));
			if (!this._turnTimerRunning)
				return this._previousTurnTime;
			return DateTime.Now - this._turnTimer;
		}

		public void StopTurnTimer()
		{
			this._previousTurnTime = this.GetTurnTime();
			this._turnTimerRunning = false;
		}
	}
}
