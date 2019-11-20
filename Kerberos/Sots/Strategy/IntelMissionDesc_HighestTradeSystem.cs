// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.IntelMissionDesc_HighestTradeSystem
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Strategy
{
	internal sealed class IntelMissionDesc_HighestTradeSystem : IntelMissionDesc_XxxSystem
	{
		public IntelMissionDesc_HighestTradeSystem()
		{
			this.ID = IntelMission.HighestTradeSystem;
			this.Name = App.Localize("@INTEL_NAME_HIGHEST_TRADE_SYSTEM_INFO");
			this.TurnEventTypes = (IEnumerable<TurnEventType>)new TurnEventType[1]
			{
		TurnEventType.EV_INTEL_MISSION_HIGHEST_TRADE_SYSTEM
			};
		}

		public override void OnCommit(
		  GameSession game,
		  int playerId,
		  int targetPlayerId,
		  int? missionid = null)
		{
			List<CounterIntelResponse> source = new List<CounterIntelResponse>();
			if (missionid.HasValue)
				source = game.GameDatabase.GetCounterIntelResponses(missionid.Value).ToList<CounterIntelResponse>();
			if (source.Any<CounterIntelResponse>((Func<CounterIntelResponse, bool>)(x => !x.auto)))
			{
				game.GameDatabase.InsertTurnEvent(new TurnEvent()
				{
					EventType = TurnEventType.EV_INTEL_MISSION_HIGHEST_TRADE_SYSTEM,
					EventMessage = TurnEventMessage.EM_INTEL_MISSION_HIGHEST_TRADE_SYSTEM,
					PlayerID = playerId,
					TargetPlayerID = targetPlayerId,
					SystemID = int.Parse(source.First<CounterIntelResponse>().value),
					TurnNumber = game.GameDatabase.GetTurnCount(),
					ShowsDialog = true
				});
			}
			else
			{
				List<KeyValuePair<int, TradeNode>> list1 = game.GameDatabase.GetTradeResultsTable().TradeNodes.Where<KeyValuePair<int, TradeNode>>((Func<KeyValuePair<int, TradeNode>, bool>)(x =>
			   {
				   int? systemOwningPlayer = game.GameDatabase.GetSystemOwningPlayer(x.Key);
				   int num = targetPlayerId;
				   if (systemOwningPlayer.GetValueOrDefault() == num)
					   return systemOwningPlayer.HasValue;
				   return false;
			   })).ToList<KeyValuePair<int, TradeNode>>();
				if (list1.Count == 0)
				{
					game.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_INTEL_MISSION_NO_HIGHEST_TRADE_SYSTEM,
						EventMessage = TurnEventMessage.EM_INTEL_MISSION_NO_HIGHEST_TRADE_SYSTEM,
						PlayerID = playerId,
						TargetPlayerID = targetPlayerId,
						TurnNumber = game.GameDatabase.GetTurnCount()
					});
				}
				else
				{
					int num = list1.OrderByDescending<KeyValuePair<int, TradeNode>, int>((Func<KeyValuePair<int, TradeNode>, int>)(x => x.Value.GetTotalImportsAndExports())).First<KeyValuePair<int, TradeNode>>().Key;
					if (source.Any<CounterIntelResponse>((Func<CounterIntelResponse, bool>)(x => x.auto)))
					{
						List<int> list2 = game.GameDatabase.GetPlayerColonySystemIDs(targetPlayerId).ToList<int>();
						num = game.Random.Choose<int>((IList<int>)list2);
					}
					game.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_INTEL_MISSION_HIGHEST_TRADE_SYSTEM,
						EventMessage = TurnEventMessage.EM_INTEL_MISSION_HIGHEST_TRADE_SYSTEM,
						PlayerID = playerId,
						TargetPlayerID = targetPlayerId,
						SystemID = num,
						TurnNumber = game.GameDatabase.GetTurnCount(),
						ShowsDialog = true
					});
				}
			}
		}
	}
}
