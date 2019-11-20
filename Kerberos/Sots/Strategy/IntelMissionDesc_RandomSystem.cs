// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.IntelMissionDesc_RandomSystem
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
	internal sealed class IntelMissionDesc_RandomSystem : IntelMissionDesc_XxxSystem
	{
		public IntelMissionDesc_RandomSystem()
		{
			this.ID = IntelMission.RandomSystem;
			this.Name = App.Localize("@INTEL_NAME_RANDOM_SYSTEM_INFO");
			this.TurnEventTypes = (IEnumerable<TurnEventType>)new TurnEventType[1]
			{
		TurnEventType.EV_INTEL_MISSION_RANDOM_SYSTEM
			};
		}

		public override void OnCommit(
		  GameSession game,
		  int playerId,
		  int targetPlayerId,
		  int? missionid = null)
		{
			List<int> list1 = game.GameDatabase.GetPlayerColonySystemIDs(targetPlayerId).ToList<int>();
			if (missionid.HasValue)
			{
				List<CounterIntelResponse> list2 = game.GameDatabase.GetCounterIntelResponses(missionid.Value).ToList<CounterIntelResponse>();
				if (list2.Any<CounterIntelResponse>((Func<CounterIntelResponse, bool>)(x => !x.auto)))
				{
					list1.Clear();
					list1.Add(int.Parse(list2.First<CounterIntelResponse>().value));
				}
			}
			if (list1.Count == 0)
			{
				game.GameDatabase.InsertTurnEvent(new TurnEvent()
				{
					EventType = TurnEventType.EV_INTEL_MISSION_NO_RANDOM_SYSTEM,
					EventMessage = TurnEventMessage.EM_INTEL_MISSION_NO_RANDOM_SYSTEM,
					PlayerID = playerId,
					TargetPlayerID = targetPlayerId,
					TurnNumber = game.GameDatabase.GetTurnCount()
				});
			}
			else
			{
				int num = game.Random.Choose<int>((IList<int>)list1);
				game.GameDatabase.InsertTurnEvent(new TurnEvent()
				{
					EventType = TurnEventType.EV_INTEL_MISSION_RANDOM_SYSTEM,
					EventMessage = TurnEventMessage.EM_INTEL_MISSION_RANDOM_SYSTEM,
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
