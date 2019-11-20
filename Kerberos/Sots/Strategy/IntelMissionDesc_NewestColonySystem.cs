// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.IntelMissionDesc_NewestColonySystem
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
	internal sealed class IntelMissionDesc_NewestColonySystem : IntelMissionDesc_XxxSystem
	{
		public IntelMissionDesc_NewestColonySystem()
		{
			this.ID = IntelMission.NewestColonySystem;
			this.Name = App.Localize("@INTEL_NAME_NEWEST_COLONY_SYSTEM_INFO");
			this.TurnEventTypes = (IEnumerable<TurnEventType>)new TurnEventType[1]
			{
		TurnEventType.EV_INTEL_MISSION_NEWEST_COLONY_SYSTEM
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
					EventType = TurnEventType.EV_INTEL_MISSION_NEWEST_COLONY_SYSTEM,
					EventMessage = TurnEventMessage.EM_INTEL_MISSION_NEWEST_COLONY_SYSTEM,
					PlayerID = playerId,
					TargetPlayerID = targetPlayerId,
					SystemID = int.Parse(source.First<CounterIntelResponse>().value),
					TurnNumber = game.GameDatabase.GetTurnCount(),
					ShowsDialog = true
				});
			}
			else
			{
				List<ColonyInfo> list1 = game.GameDatabase.GetPlayerColoniesByPlayerId(targetPlayerId).ToList<ColonyInfo>();
				if (list1.Count == 0)
				{
					game.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_INTEL_MISSION_NO_NEWEST_COLONY_SYSTEM,
						EventMessage = TurnEventMessage.EM_INTEL_MISSION_NO_NEWEST_COLONY_SYSTEM,
						PlayerID = playerId,
						TargetPlayerID = targetPlayerId,
						TurnNumber = game.GameDatabase.GetTurnCount()
					});
				}
				else
				{
					ColonyInfo colonyInfo = list1.OrderByDescending<ColonyInfo, int>((Func<ColonyInfo, int>)(x => x.TurnEstablished)).First<ColonyInfo>();
					int num = game.GameDatabase.GetOrbitalObjectInfo(colonyInfo.OrbitalObjectID).StarSystemID;
					if (source.Any<CounterIntelResponse>((Func<CounterIntelResponse, bool>)(x => x.auto)))
					{
						List<int> list2 = game.GameDatabase.GetPlayerColonySystemIDs(targetPlayerId).ToList<int>();
						num = game.Random.Choose<int>((IList<int>)list2);
					}
					game.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_INTEL_MISSION_NEWEST_COLONY_SYSTEM,
						EventMessage = TurnEventMessage.EM_INTEL_MISSION_NEWEST_COLONY_SYSTEM,
						PlayerID = playerId,
						TargetPlayerID = targetPlayerId,
						SystemID = num,
						ColonyID = colonyInfo.ID,
						TurnNumber = game.GameDatabase.GetTurnCount(),
						ShowsDialog = true
					});
				}
			}
		}
	}
}
