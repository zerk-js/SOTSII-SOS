// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.TestNetworkState
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using System;
using System.Collections.Generic;

namespace Kerberos.Sots.GameStates
{
	internal class TestNetworkState : GameState
	{
		private readonly List<string> _networkLog = new List<string>();

		public TestNetworkState(App game)
		  : base(game)
		{
		}

		protected override void OnPrepare(GameState prev, object[] parms)
		{
			this.App.UI.LoadScreen("TestNetwork");
		}

		protected override void OnEnter()
		{
			this.App.GameSetup.IsMultiplayer = true;
			this.App.UI.SetScreen("TestNetwork");
			this.App.UI.GameEvent += new UIEventGameEvent(this.UICommChannel_GameEvent);
		}

		protected override void OnExit(GameState next, ExitReason reason)
		{
		}

		protected override void OnUpdate()
		{
		}

		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}

		protected override void UICommChannel_OnPanelMessage(
		  string panelName,
		  string msgType,
		  string[] msgParams)
		{
			if (!(msgType == "button_clicked") || !(panelName == "host_test_button"))
				return;
			this.App.SwitchGameStateViaLoadingScreen((Action)null, (LoadingFinishedDelegate)null, (GameState)this.App.GetGameState<CombatState>());
		}

		private void UICommChannel_GameEvent(string eventName, string[] eventParams)
		{
			if (!(eventName == "netlog"))
				return;
			this.ProcessGameEvent_NetworkLog(eventParams);
		}

		private void ProcessGameEvent_NetworkLog(string[] eventParams)
		{
			foreach (string eventParam in eventParams)
			{
				if (!string.IsNullOrWhiteSpace(eventParam))
				{
					this._networkLog.Add(eventParam);
					this.App.UI.AddItem("network_log", "", this._networkLog.Count, eventParam);
				}
			}
		}
	}
}
