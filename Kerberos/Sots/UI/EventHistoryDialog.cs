// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.EventHistoryDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class EventHistoryDialog : Dialog
	{
		public const string OKButton = "buttonOK";
		public const string EventList = "eventList";
		public const string EventItemList = "eventItemList";
		public const string EventTurnList = "turnList";

		public EventHistoryDialog(App game)
		  : base(game, "dialogEventHistory")
		{
		}

		public override void Initialize()
		{
			base.Initialize();
			if (this._app.GameDatabase.GetTurnEventsByPlayerID(this._app.LocalPlayer.ID).Count<TurnEvent>() == 0)
				return;
			int turnCount = this._app.GameDatabase.GetTurnCount();
			for (int userItemId = turnCount; userItemId > 0; userItemId -= 25)
			{
				int num = userItemId - 25;
				if (num < 0)
					num = 0;
				this._app.UI.AddItem(this._app.UI.Path(this.ID, "turnList"), "", userItemId, App.Localize("@UI_GENERAL_TURN") + " " + (num + 1).ToString() + " - " + userItemId.ToString());
			}
			this.SyncTurns(turnCount - 25, turnCount);
		}

		private void SyncTurns(int from, int to)
		{
			int userItemId = 0;
			int num = -1;
			string str = "";
			this._app.UI.ClearItems(this._app.UI.Path(this.ID, "eventList"));
			IEnumerable<TurnEvent> source = this._app.GameDatabase.GetTurnEventsByPlayerID(this._app.LocalPlayer.ID).Where<TurnEvent>((Func<TurnEvent, bool>)(x =>
		   {
			   if (x.TurnNumber >= from)
				   return x.TurnNumber <= to;
			   return false;
		   }));
			if (source.Count<TurnEvent>() == 0)
				return;
			foreach (TurnEvent e in source.Reverse<TurnEvent>())
			{
				if (num != e.TurnNumber)
				{
					this._app.UI.AddItem(this._app.UI.Path(this.ID, "eventList"), "", e.TurnNumber, "");
					str = this._app.UI.GetItemGlobalID(this._app.UI.Path(this.ID, "eventList"), "", e.TurnNumber, "");
					this._app.UI.SetPropertyString(this._app.UI.Path(str, "turnNumber"), "text", App.Localize("@TURN") + " " + e.TurnNumber.ToString());
					num = e.TurnNumber;
				}
				string eventMessage = e.GetEventMessage(this._app.Game);
				this._app.UI.AddItem(this._app.UI.Path(str, "eventItemList"), "", userItemId, "");
				string itemGlobalId = this._app.UI.GetItemGlobalID(this._app.UI.Path(str, "eventItemList"), "", userItemId, "");
				this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId, "eventInfo"), "text", eventMessage);
				this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId, "eventImage"), "texture", TurnEvent.GetTurnEventSprite(this._app.Game, e));
				++userItemId;
			}
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked" && panelName == "buttonOK")
				this._app.UI.CloseDialog((Dialog)this, true);
			if (!(msgType == "list_sel_changed") || !(panelName == "turnList"))
				return;
			int to = int.Parse(msgParams[0]);
			this.SyncTurns(to - 25, to);
		}

		public override string[] CloseDialog()
		{
			return (string[])null;
		}
	}
}
