// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.IntelMissionDesc_CurrentResearch
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Strategy
{
	internal sealed class IntelMissionDesc_CurrentResearch : IntelMissionDesc
	{
		public IntelMissionDesc_CurrentResearch()
		{
			this.ID = IntelMission.CurrentResearch;
			this.Name = App.Localize("@INTEL_NAME_CURRENT_RESEARCH");
			this.TurnEventTypes = (IEnumerable<TurnEventType>)new TurnEventType[0];
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
					EventType = TurnEventType.EV_INTEL_MISSION_CURRENT_TECH,
					EventMessage = TurnEventMessage.EM_INTEL_MISSION_CURRENT_TECH,
					TechID = game.GameDatabase.GetTechID(source.First<CounterIntelResponse>().value),
					PlayerID = playerId,
					TargetPlayerID = targetPlayerId,
					TurnNumber = game.GameDatabase.GetTurnCount(),
					ShowsDialog = true
				});
			}
			else
			{
				int techId = game.GameDatabase.GetPlayerResearchingTechID(targetPlayerId);
				if (source.Any<CounterIntelResponse>((Func<CounterIntelResponse, bool>)(x => x.auto)))
				{
					List<PlayerTechInfo> list = game.GameDatabase.GetPlayerTechInfos(targetPlayerId).ToList<PlayerTechInfo>();
					techId = list.ToArray()[game.Random.Next(0, list.Count)].TechID;
				}
				if (techId != 0)
				{
					game.GameDatabase.GetPlayerTechInfo(targetPlayerId, techId);
					game.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_INTEL_MISSION_CURRENT_TECH,
						EventMessage = TurnEventMessage.EM_INTEL_MISSION_CURRENT_TECH,
						TechID = techId,
						PlayerID = playerId,
						TargetPlayerID = targetPlayerId,
						TurnNumber = game.GameDatabase.GetTurnCount(),
						ShowsDialog = true
					});
				}
				else
				{
					List<PlayerTechInfo> list = game.GameDatabase.GetPlayerTechInfos(targetPlayerId).Where<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x =>
				   {
					   if (x.State != TechStates.Researched)
						   return false;
					   int? turnResearched = x.TurnResearched;
					   if (turnResearched.GetValueOrDefault() > 1)
						   return turnResearched.HasValue;
					   return false;
				   })).ToList<PlayerTechInfo>();
					if (list.Count == 0)
					{
						game.GameDatabase.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_INTEL_MISSION_NO_COMPLETE_TECHS,
							EventMessage = TurnEventMessage.EM_INTEL_MISSION_NO_COMPLETE_TECHS,
							PlayerID = playerId,
							TargetPlayerID = targetPlayerId,
							TurnNumber = game.GameDatabase.GetTurnCount()
						});
					}
					else
					{
						PlayerTechInfo playerTechInfo = list.OrderByDescending<PlayerTechInfo, int?>((Func<PlayerTechInfo, int?>)(x => x.TurnResearched)).First<PlayerTechInfo>();
						game.GameDatabase.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_INTEL_MISSION_RECENT_TECH,
							EventMessage = TurnEventMessage.EM_INTEL_MISSION_RECENT_TECH,
							TechID = playerTechInfo.TechID,
							PlayerID = playerId,
							TargetPlayerID = targetPlayerId,
							TurnNumber = game.GameDatabase.GetTurnCount()
						});
					}
				}
			}
		}
	}
}
