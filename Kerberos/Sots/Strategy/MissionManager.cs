// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.MissionManager
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.PlayerFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Strategy
{
	internal class MissionManager
	{
		private readonly List<MissionManagerTargetInfo> targets = new List<MissionManagerTargetInfo>();
		private readonly StrategicAI ai;

		public IEnumerable<MissionManagerTargetInfo> Targets
		{
			get
			{
				return (IEnumerable<MissionManagerTargetInfo>)this.targets;
			}
		}

		public MissionManager(StrategicAI ai)
		{
			this.ai = ai;
		}

		private bool IsInvadeEffective(int orbitalObjectId)
		{
			foreach (CombatData combatData in this.ai.Game.CombatData.GetCombatsForPlayer(this.ai.Game.GameDatabase, this.ai.Player.ID, this.ai.Game.GameDatabase.GetOrbitalObjectInfo(orbitalObjectId).StarSystemID, 10).ToList<CombatData>())
			{
				if (combatData.GetPlayer(this.ai.Player.ID).FleetCount >= 3)
					return false;
			}
			return true;
		}

		internal void Update()
		{
			GameDatabase gameDatabase = this.ai.Game.GameDatabase;
			Player player = this.ai.Player;
			int turn = gameDatabase.GetTurnCount();
			this.targets.RemoveAll((Predicate<MissionManagerTargetInfo>)(x =>
		   {
			   if (turn <= x.ArrivalTurn && this.ai.IsValidInvasionTarget(x.OrbitalObjectID))
				   return !this.IsInvadeEffective(x.OrbitalObjectID);
			   return true;
		   }));
			Dictionary<int, float> source = new Dictionary<int, float>();
			foreach (int playerId in gameDatabase.GetPlayerIDs())
			{
				if (gameDatabase.GetDiplomacyStateBetweenPlayers(player.ID, playerId) == DiplomacyState.WAR)
				{
					foreach (AIFleetInfo aiFleetInfo in gameDatabase.GetAIFleetInfos(player.ID).Where<AIFleetInfo>((Func<AIFleetInfo, bool>)(x => x.FleetID.HasValue)))
					{
						if ((aiFleetInfo.FleetType & 1536) != 0)
						{
							foreach (TargetOrbitalObjectScore orbitalObjectScore in this.ai.GetTargetsForInvasion(aiFleetInfo.FleetID.Value, playerId))
							{
								if (source.ContainsKey(orbitalObjectScore.OrbitalObjectID))
								{
									Dictionary<int, float> dictionary;
									int orbitalObjectId;
									(dictionary = source)[orbitalObjectId = orbitalObjectScore.OrbitalObjectID] = dictionary[orbitalObjectId] + orbitalObjectScore.Score;
								}
								else
									source[orbitalObjectScore.OrbitalObjectID] = orbitalObjectScore.Score;
							}
						}
					}
				}
			}
			foreach (MissionManagerTargetInfo target in this.targets)
				source.Remove(target.OrbitalObjectID);
			List<MissionManagerTargetInfo> list = source.Select<KeyValuePair<int, float>, MissionManagerTargetInfo>((Func<KeyValuePair<int, float>, MissionManagerTargetInfo>)(x => new MissionManagerTargetInfo()
			{
				OrbitalObjectID = x.Key,
				Score = x.Value
			})).OrderByDescending<MissionManagerTargetInfo, float>((Func<MissionManagerTargetInfo, float>)(y => y.Score)).ToList<MissionManagerTargetInfo>();
			for (int index = 0; index < list.Count; ++index)
				list[index].ArrivalTurn = turn + 3 + 2 * index;
			this.targets.AddRange((IEnumerable<MissionManagerTargetInfo>)list);
		}
	}
}
