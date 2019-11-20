// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.TurnEventUI
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Strategy;

namespace Kerberos.Sots.UI
{
	internal class TurnEventUI
	{
		public const string UITurnEventImage = "turnEventImage";
		public const string UITurnEventMessage = "turnEventMessage";
		public const string UITurnEventNext = "turnEventNext";
		public const string UITurnEventPrevious = "turnEventPrevious";

		internal static void SyncTurnEventWidget(
		  GameSession game,
		  string panelName,
		  TurnEvent turnEvent)
		{
			if (turnEvent == null)
			{
				game.UI.SetPropertyString("turnEventMessage", "text", "");
				game.UI.SetPropertyString("turnEventImage", "sprite", "");
				game.UI.SetVisible("turnEventNext", false);
				game.UI.SetVisible("turnEventPrevious", false);
			}
			else
			{
				game.UI.SetPropertyString("turnEventMessage", "text", turnEvent.GetEventMessage(game));
				game.UI.SetPropertyString("turnEventImage", "texture", TurnEvent.GetTurnEventSprite(game, turnEvent));
				game.UI.SetVisible("turnEventNext", true);
				game.UI.SetVisible("turnEventPrevious", true);
			}
		}

		internal static void SyncTurnEventTicker(GameSession game, string panelName)
		{
			game.UI.ClearItems(panelName);
			game.UI.AddItem(panelName, "", 8000000, "", "tickerEvent_Spacer");
			game.UI.AddItem(panelName, "", 8000001, "", "tickerEvent_Item");
			string itemGlobalId1 = game.UI.GetItemGlobalID(panelName, "", 8000001, "");
			game.UI.SetText(game.UI.Path(itemGlobalId1, "tickerEventButton", "idle"), App.Localize("@TURN") + " " + game.GameDatabase.GetTurnCount().ToString() + " " + App.Localize("@UI_EVENTS"));
			game.UI.SetText(game.UI.Path(itemGlobalId1, "tickerEventButton", "mouse_over"), App.Localize("@TURN") + " " + game.GameDatabase.GetTurnCount().ToString() + " " + App.Localize("@UI_EVENTS"));
			game.UI.SetText(game.UI.Path(itemGlobalId1, "tickerEventButton", "pressed"), App.Localize("@TURN") + " " + game.GameDatabase.GetTurnCount().ToString() + " " + App.Localize("@UI_EVENTS"));
			game.UI.SetText(game.UI.Path(itemGlobalId1, "tickerEventButton", "disabled"), App.Localize("@TURN") + " " + game.GameDatabase.GetTurnCount().ToString() + " " + App.Localize("@UI_EVENTS"));
			foreach (TurnEvent turnEvent in game.TurnEvents)
			{
				game.UI.AddItem(panelName, "", turnEvent.ID, "", "tickerEvent_Item");
				string itemGlobalId2 = game.UI.GetItemGlobalID(panelName, "", turnEvent.ID, "");
				game.UI.SetText(game.UI.Path(itemGlobalId2, "tickerEventButton", "idle"), turnEvent.GetEventName(game));
				game.UI.SetText(game.UI.Path(itemGlobalId2, "tickerEventButton", "mouse_over"), turnEvent.GetEventName(game));
				game.UI.SetText(game.UI.Path(itemGlobalId2, "tickerEventButton", "pressed"), turnEvent.GetEventName(game));
				game.UI.SetText(game.UI.Path(itemGlobalId2, "tickerEventButton", "disabled"), turnEvent.GetEventName(game));
				game.UI.SetPropertyString(game.UI.Path(itemGlobalId2, "tickerEventButton"), "id", "tickerEventButton|" + turnEvent.ID.ToString());
			}
			game.UI.AddItem(panelName, "", 9000000, "", "tickerEvent_Spacer");
		}
	}
}
