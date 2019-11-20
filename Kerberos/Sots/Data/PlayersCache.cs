// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.PlayersCache
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.SQLite;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Data
{
	internal sealed class PlayersCache : RowCache<int, PlayerInfo>
	{
		private DiplomacyStatesCache diplomacy_states;

		public PlayersCache(
		  SQLiteConnection db,
		  AssetDatabase assets,
		  DiplomacyStatesCache diplomacy_states)
		  : base(db, assets)
		{
			this.diplomacy_states = diplomacy_states;
		}

		public static PlayerInfo GetPlayerInfoFromRow(SQLiteConnection db, Row row)
		{
			PlayerInfo playerInfo = new PlayerInfo()
			{
				ID = row[0].SQLiteValueToInteger(),
				Name = row[1].ToString(),
				FactionID = row[2].SQLiteValueToInteger(),
				PrimaryColor = row[4].SQLiteValueToVector3(),
				SecondaryColor = row[5].SQLiteValueToVector3(),
				BadgeAssetPath = row[6].ToString(),
				Savings = row[7].SQLiteValueToDouble(),
				Homeworld = row[8].SQLiteValueToNullableInteger(),
				AvatarAssetPath = row[9].ToString(),
				LastCombatTurn = row[10].SQLiteValueToInteger(),
				LastEncounterTurn = row[11].SQLiteValueToInteger(),
				RateGovernmentResearch = row[12].SQLiteValueToSingle(),
				RateResearchCurrentProject = row[13].SQLiteValueToSingle(),
				RateResearchSpecialProject = row[14].SQLiteValueToSingle(),
				RateResearchSalvageResearch = row[15].SQLiteValueToSingle(),
				RateGovernmentStimulus = row[16].SQLiteValueToSingle(),
				RateGovernmentSecurity = row[17].SQLiteValueToSingle(),
				RateGovernmentSavings = row[18].SQLiteValueToSingle(),
				RateStimulusMining = row[19].SQLiteValueToSingle(),
				RateStimulusColonization = row[20].SQLiteValueToSingle(),
				RateStimulusTrade = row[21].SQLiteValueToSingle(),
				RateSecurityOperations = row[22].SQLiteValueToSingle(),
				RateSecurityIntelligence = row[23].SQLiteValueToSingle(),
				RateSecurityCounterIntelligence = row[24].SQLiteValueToSingle(),
				isStandardPlayer = row[25].SQLiteValueToBoolean(),
				GenericDiplomacyPoints = row[26].SQLiteValueToInteger(),
				RateTax = row[27].SQLiteValueToSingle(),
				RateImmigration = row[28].SQLiteValueToSingle(),
				IntelPoints = row[29].SQLiteValueToInteger(),
				CounterIntelPoints = row[30].SQLiteValueToInteger(),
				OperationsPoints = row[31].SQLiteValueToInteger(),
				IntelAccumulator = row[32].SQLiteValueToInteger(),
				CounterIntelAccumulator = row[33].SQLiteValueToInteger(),
				OperationsAccumulator = row[34].SQLiteValueToInteger(),
				CivilianMiningAccumulator = row[35].SQLiteValueToInteger(),
				CivilianColonizationAccumulator = row[36].SQLiteValueToInteger(),
				CivilianTradeAccumulator = row[37].SQLiteValueToInteger(),
				SubfactionIndex = row[38].SQLiteValueToInteger(),
				AdditionalResearchPoints = row[39].SQLiteValueToInteger(),
				PsionicPotential = row[40].SQLiteValueToInteger(),
				isDefeated = row[41].SQLiteValueToBoolean(),
				CurrentTradeIncome = row[42].SQLiteValueToDouble(),
				includeInDiplomacy = row[43].SQLiteValueToBoolean(),
				isAIRebellionPlayer = row[44].SQLiteValueToBoolean(),
				AutoPlaceDefenseAssets = row[45].SQLiteValueToBoolean(),
				AutoRepairShips = row[46].SQLiteValueToBoolean(),
				AutoUseGoopModules = row[47].SQLiteValueToBoolean(),
				AutoUseJokerModules = row[48].SQLiteValueToBoolean(),
				ResearchBoostFunds = row[49].SQLiteValueToDouble(),
				AutoAoe = row[50].SQLiteValueToBoolean(),
				Team = row[51].SQLiteValueToInteger(),
				AutoPatrol = row[52].SQLiteValueToBoolean(),
				AIDifficulty = row.Count<string>() <= 53 || row[53] == null ? AIDifficulty.Normal : (AIDifficulty)Enum.Parse(typeof(AIDifficulty), row[53].ToString()),
				RateTaxPrev = row.Count<string>() <= 54 || row[54] == null ? row[27].SQLiteValueToSingle() : row[54].SQLiteValueToSingle()
			};
			playerInfo.FactionDiplomacyPoints = PlayersCache.GetFactionDiplomacyPoints(db, playerInfo.ID);
			return playerInfo;
		}

		private static void InsertGovernment(
		  SQLiteConnection db,
		  int playerID,
		  float auth,
		  float econLib)
		{
			db.ExecuteNonQuery(string.Format(Queries.InsertGovernment, (object)playerID.ToSQLiteValue(), (object)auth.ToSQLiteValue(), (object)econLib.ToSQLiteValue()), false, true);
		}

		private IEnumerable<string> GetPlayerNames()
		{
			return this.Values.Select<PlayerInfo, string>((Func<PlayerInfo, string>)(x => x.Name));
		}

		public IEnumerable<int> GetStandardPlayerIDs()
		{
			return this.Values.Where<PlayerInfo>((Func<PlayerInfo, bool>)(x => x.isStandardPlayer)).Select<PlayerInfo, int>((Func<PlayerInfo, int>)(y => y.ID));
		}

		public IEnumerable<int> GetPlayerIDs()
		{
			return this.Values.Select<PlayerInfo, int>((Func<PlayerInfo, int>)(y => y.ID));
		}

		private string MakeUniquePlayerName(string name)
		{
			string newname = name;
			List<string> list = this.GetPlayerNames().ToList<string>();
			int num = 2;
			while (list.Any<string>((Func<string, bool>)(x => x.Equals(newname, StringComparison.InvariantCultureIgnoreCase))))
			{
				newname = name + " (" + (object)num + ")";
				++num;
			}
			return newname;
		}

		public static FactionInfo GetFactionInfoFromRow(Row row)
		{
			return new FactionInfo()
			{
				ID = row[0].SQLiteValueToInteger(),
				Name = row[1].SQLiteValueToString(),
				IdealSuitability = row[2].SQLiteValueToSingle()
			};
		}

		public int GetPlayerFactionID(int playerId)
		{
			if (this.ContainsKey(playerId))
				return this[playerId].FactionID;
			return 0;
		}

		public static IEnumerable<FactionInfo> GetFactions(SQLiteConnection db)
		{
			Table t = db.ExecuteTableQuery(Queries.GetFactions, true);
			foreach (Row row in t.Rows)
				yield return PlayersCache.GetFactionInfoFromRow(row);
		}

		public int GetDefaultDiplomacyReactionValue(int player1, int player2)
		{
			int num = DiplomacyInfo.DefaultDeplomacyRelations;
			int p1Faction = this.GetPlayerFactionID(player1);
			int p2Faction = this.GetPlayerFactionID(player2);
			Faction faction1 = this.Assets.Factions.FirstOrDefault<Faction>((Func<Faction, bool>)(x => x.ID == p1Faction));
			Faction faction2 = this.Assets.Factions.FirstOrDefault<Faction>((Func<Faction, bool>)(x => x.ID == p2Faction));
			if (faction1 != null && faction2 != null)
				num = faction1.GetDefaultReactionToFaction(faction2);
			return num;
		}

		public static Dictionary<int, int> GetFactionDiplomacyPoints(
		  SQLiteConnection db,
		  int playerId)
		{
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			foreach (Row row in db.ExecuteTableQuery(string.Format(Queries.GetFactionDiplomacyPointsForPlayer, (object)playerId.ToSQLiteValue()), true))
				dictionary.Add(row[1].SQLiteValueToInteger(), row[2].SQLiteValueToInteger());
			foreach (FactionInfo faction in PlayersCache.GetFactions(db))
			{
				if (!dictionary.ContainsKey(faction.ID))
					dictionary.Add(faction.ID, 0);
			}
			return dictionary;
		}

		protected override int OnInsert(SQLiteConnection db, int? key, PlayerInfo value)
		{
			if (key.HasValue)
				throw new ArgumentOutOfRangeException(nameof(key), "Player insertion does not permit explicit specification of an ID.");
			int? homeworld = value.Homeworld;
			int num1 = 0;
			if (homeworld.GetValueOrDefault() == num1 & homeworld.HasValue)
				throw new ArgumentOutOfRangeException("value.Homeworld", "Nullable foreign key must never be 0 because this can violate database equivalence constraints. If the intent is to say that no such foreign key exists then use null instead.");
			int num2 = db.ExecuteIntegerQuery(string.Format(Queries.InsertPlayer, (object)"NULL".ToSQLiteValue(), (object)this.MakeUniquePlayerName(value.Name).ToSQLiteValue(), (object)value.FactionID.ToSQLiteValue(), (object)value.Homeworld.ToNullableSQLiteValue(), (object)value.PrimaryColor.ToSQLiteValue(), (object)value.SecondaryColor.ToSQLiteValue(), (object)value.BadgeAssetPath.ToSQLiteValue(), (object)value.Savings.ToSQLiteValue(), (object)value.AvatarAssetPath.ToSQLiteValue(), (object)value.isStandardPlayer.ToSQLiteValue(), (object)value.SubfactionIndex.ToSQLiteValue(), (object)value.includeInDiplomacy.ToSQLiteValue(), (object)value.isAIRebellionPlayer.ToSQLiteValue(), (object)value.AutoPlaceDefenseAssets.ToSQLiteValue(), (object)value.AutoRepairShips.ToSQLiteValue(), (object)value.AutoUseGoopModules.ToSQLiteValue(), (object)value.AutoUseJokerModules.ToSQLiteValue(), (object)value.AutoAoe.ToSQLiteValue(), (object)value.Team.ToSQLiteValue(), (object)value.AIDifficulty.ToString().ToSQLiteValue()));
			PlayersCache.InsertGovernment(db, num2, 0.0f, 0.0f);
			this.Sync(num2);
			foreach (int playerId in this.GetPlayerIDs())
			{
				if (playerId != num2)
				{
					this.diplomacy_states.InsertDiplomaticState(num2, playerId, value.isStandardPlayer || value.includeInDiplomacy ? DiplomacyState.NEUTRAL : DiplomacyState.WAR, this.GetDefaultDiplomacyReactionValue(num2, playerId), false, false);
					this.diplomacy_states.InsertDiplomaticState(playerId, num2, value.isStandardPlayer || value.includeInDiplomacy ? DiplomacyState.NEUTRAL : DiplomacyState.WAR, this.GetDefaultDiplomacyReactionValue(playerId, num2), false, false);
				}
			}
			value.ID = num2;
			return num2;
		}

		protected override void OnUpdate(SQLiteConnection db, int key, PlayerInfo value)
		{
			throw new NotImplementedException("There is no general PlayerInfo update.");
		}

		protected override void OnRemove(SQLiteConnection db, int key)
		{
			throw new NotImplementedException("There is no general PlayerInfo remove.");
		}

		protected override IEnumerable<KeyValuePair<int, PlayerInfo>> OnSynchronizeWithDatabase(
		  SQLiteConnection db,
		  IEnumerable<int> range)
		{
			if (range == null)
			{
				foreach (Row row in db.ExecuteTableQuery(Queries.GetPlayerInfos, true))
				{
					PlayerInfo o = PlayersCache.GetPlayerInfoFromRow(db, row);
					yield return new KeyValuePair<int, PlayerInfo>(o.ID, o);
				}
			}
			else
			{
				foreach (int num in range)
				{
					foreach (Row row in db.ExecuteTableQuery(string.Format(Queries.GetPlayerInfo, (object)num.ToSQLiteValue()), true))
					{
						PlayerInfo o = PlayersCache.GetPlayerInfoFromRow(db, row);
						yield return new KeyValuePair<int, PlayerInfo>(o.ID, o);
					}
				}
			}
		}
	}
}
