// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.PlayerWidget
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class PlayerWidget : PanelBinding
	{
		private int _prevNumBusyPlayers = -1;
		private List<PlayerDetails> _prevPlayerDetails = new List<PlayerDetails>();
		private const string WidgetPanel = "playerDropdown";
		private App App;
		private bool _visible;
		private bool _initialized;

		public bool Visible
		{
			get
			{
				return this._visible;
			}
			set
			{
				this.UI.SetVisible("playerDropdown", value);
			}
		}

		public bool Initialized
		{
			get
			{
				return this._initialized;
			}
		}

		public PlayerWidget(App game, UICommChannel ui, string id)
		  : base(ui, id)
		{
			this.App = game;
			this.App.UI.GameEvent += new UIEventGameEvent(this.UICommChannel_GameEvent);
		}

		public void Initialize()
		{
			int userItemId = 0;
			foreach (PlayerSetup player in this.App.GameSetup.Players)
			{
				PlayerDetails playerDetails = new PlayerDetails();
				playerDetails.Slot = player.slot;
				playerDetails.Status = player.Status;
				playerDetails.AI = player.AI;
				this.App.UI.AddItem(this.App.UI.Path("playerDropdown", "playerList"), "", userItemId, "");
				playerDetails.ItemID = this.App.UI.GetItemGlobalID(this.App.UI.Path("playerDropdown", "playerList"), "", userItemId, "");
				string str = player.Name;
				if (player.AI)
					str = "AI Player";
				this.App.UI.SetPropertyString(this.App.UI.Path(playerDetails.ItemID, "name"), "text", string.Format(App.Localize("@PLAYERS_WIDGET_PLAYER_NAME_OF"), (object)str, (object)player.EmpireName));
				this.App.UI.SetPropertyColorNormalized(this.App.UI.Path(playerDetails.ItemID, "name"), "color", this.App.GameSetup.GetEmpireColor(player.EmpireColor));
				this.App.UI.SetPropertyString(this.App.UI.Path(playerDetails.ItemID, "status"), "text", GameSetup.PlayerStatusToString(player.Status));
				this.App.UI.SetPropertyString(this.App.UI.Path(playerDetails.ItemID, "avatar"), "sprite", player.Avatar ?? string.Empty);
				this.App.UI.SetPropertyString(this.App.UI.Path(playerDetails.ItemID, "badge"), "sprite", player.Badge ?? string.Empty);
				this.App.UI.SetVisible(this.App.UI.Path(playerDetails.ItemID, "eliminatedState"), (player.Status == NPlayerStatus.PS_DEFEATED ? 1 : 0) != 0);
				this._prevPlayerDetails.Add(playerDetails);
				this.App.UI.ForceLayout("playerList");
				++userItemId;
			}
			this._initialized = true;
		}

		public void UpdateSlotStatus(PlayerDetails details, PlayerSetup player)
		{
			this.App.UI.SetPropertyString(this.App.UI.Path(details.ItemID, "status"), "text", GameSetup.PlayerStatusToString(player.Status));
			this.App.UI.SetVisible(this.App.UI.Path(details.ItemID, "eliminatedState"), (player.Status == NPlayerStatus.PS_DEFEATED ? 1 : 0) != 0);
			details.Status = player.Status;
		}

		public void Sync()
		{
			int num1 = 0;
			string str = "";
			for (int index = 0; index < this.App.GameSetup.Players.Count<PlayerSetup>(); ++index)
			{
				if (this.App.GameSetup.Players[index].Status != NPlayerStatus.PS_WAIT && this.App.GameSetup.Players[index].Status != NPlayerStatus.PS_DEFEATED && !this.App.GameSetup.Players[index].AI)
				{
					++num1;
					str = this.App.GameSetup.Players[index].Name;
				}
				if (this._prevPlayerDetails[index].Status != this.App.GameSetup.Players[index].Status)
					this.UpdateSlotStatus(this._prevPlayerDetails[index], this.App.GameSetup.Players[index]);
				if (this._prevPlayerDetails[index].AI != this.App.GameSetup.Players[index].AI)
				{
					if (this.App.GameSetup.Players[index].AI)
						this.App.UI.SetPropertyString(this.App.UI.Path(this._prevPlayerDetails[index].ItemID, "name"), "text", string.Format(App.Localize("@PLAYERS_WIDGET_PLAYER_NAME_OF"), (object)"AI Player", (object)this.App.GameSetup.Players[index].EmpireName));
					else
						this.App.UI.SetPropertyString(this.App.UI.Path(this._prevPlayerDetails[index].ItemID, "name"), "text", string.Format(App.Localize("@PLAYERS_WIDGET_PLAYER_NAME_OF"), (object)this.App.GameSetup.Players[index].Name, (object)this.App.GameSetup.Players[index].EmpireName));
					this._prevPlayerDetails[index].AI = this.App.GameSetup.Players[index].AI;
				}
			}
			if (num1 != this._prevNumBusyPlayers)
			{
				switch (num1)
				{
					case 0:
						this.App.UI.SetPropertyString(this.App.UI.Path("playerDropdown", "playersRemaining"), "text", App.Localize("@PLAYERS_WIDGET_PLAYER_FINISHED"));
						break;
					case 1:
						this.App.UI.SetPropertyString(this.App.UI.Path("playerDropdown", "playersRemaining"), "text", string.Format(App.Localize("@PLAYERS_WIDGET_WAITING_ON"), (object)str));
						break;
					default:
						this.App.UI.SetPropertyString(this.App.UI.Path("playerDropdown", "playersRemaining"), "text", string.Format(App.Localize("@PLAYERS_WIDGET_PLAYERS_REMAINING"), (object)num1.ToString()));
						break;
				}
			}
			this._prevNumBusyPlayers = num1;
			if (this.App.GameSetup.StrategicTurnLength != float.MaxValue)
			{
				float strategicTurnLength = this.App.GameSetup.StrategicTurnLength;
				TimeSpan turnTime = this.App.Game.TurnTimer.GetTurnTime();
				float num2 = strategicTurnLength * 60f - ((float)turnTime.Minutes * 60f + (float)turnTime.Seconds);
				int num3 = (int)((double)num2 / 60.0);
				string source = (num2 - (float)num3 * 60f).ToString();
				if (source.Count<char>() == 1)
					source = "0" + source;
				this.App.UI.SetPropertyString(this.App.UI.Path("playerDropdown", "timeRemaining"), "text", num3.ToString() + ":" + source);
				this.App.UI.SetVisible(this.App.UI.Path("playerDropdown", "timeRemaining"), true);
			}
			else
				this.App.UI.SetVisible(this.App.UI.Path("playerDropdown", "timeRemaining"), false);
		}

		public void Terminate()
		{
			this._initialized = false;
			this.App.UI.GameEvent -= new UIEventGameEvent(this.UICommChannel_GameEvent);
			this.App.UI.ClearItems(this.App.UI.Path("playerDropdown", "playerList"));
		}

		private void UICommChannel_GameEvent(string eventName, string[] eventParams)
		{
		}
	}
}
