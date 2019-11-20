// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.DiplomacyStatesCache
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.SQLite;
using Kerberos.Sots.Framework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;

namespace Kerberos.Sots.Data
{
	internal sealed class DiplomacyStatesCache : RowCache<int, DiplomacyInfo>
	{
		private readonly BidirMap<KeyValuePair<int, int>, int> byPlayer = new BidirMap<KeyValuePair<int, int>, int>();
		private PlayersCache players;

		private void TryRemoveByPlayer(int playerId, int towardsPlayerId)
		{
			KeyValuePair<int, int> index = new KeyValuePair<int, int>(playerId, towardsPlayerId);
			if (!this.byPlayer.Forward.ContainsKey(index))
				return;
			this.byPlayer.Remove(index, this.byPlayer.Forward[index]);
		}

		private void TryInsertByPlayer(int playerId, int towardsPlayerId, int diplomacyInfoId)
		{
			this.byPlayer.Insert(new KeyValuePair<int, int>(playerId, towardsPlayerId), diplomacyInfoId);
		}

		private int InsertDiplomaticStateOneWay(
		  int playerID,
		  int towardPlayerID,
		  DiplomacyState type,
		  int relations,
		  bool isEncountered)
		{
			return this.Insert(new int?(), new DiplomacyInfo()
			{
				PlayerID = playerID,
				TowardsPlayerID = towardPlayerID,
				State = type,
				Relations = relations,
				isEncountered = isEncountered
			});
		}

		public int InsertDiplomaticState(
		  int playerID,
		  int towardPlayerID,
		  DiplomacyState type,
		  int relations,
		  bool isEncountered,
		  bool reciprocal)
		{
			if (reciprocal)
				this.InsertDiplomaticStateOneWay(towardPlayerID, playerID, type, relations, isEncountered);
			return this.InsertDiplomaticStateOneWay(playerID, towardPlayerID, type, relations, isEncountered);
		}

		public DiplomacyInfo GetDiplomacyInfoByPlayer(int playerId, int towardsPlayerId)
		{
			this.SynchronizeWithDatabase();
			int index;
			if (this.byPlayer.Forward.TryGetValue(new KeyValuePair<int, int>(playerId, towardsPlayerId), out index))
				return this[index];
			return (DiplomacyInfo)null;
		}

		private static DiplomacyInfo GetDiplomacyInfoFromRow(Row r)
		{
			return new DiplomacyInfo()
			{
				ID = r[0].SQLiteValueToInteger(),
				PlayerID = r[1].SQLiteValueToInteger(),
				TowardsPlayerID = r[2].SQLiteValueToInteger(),
				State = (DiplomacyState)r[3].SQLiteValueToInteger(),
				Relations = r[4].SQLiteValueToInteger(),
				isEncountered = r[5].SQLiteValueToBoolean()
			};
		}

		public DiplomacyStatesCache(SQLiteConnection db, AssetDatabase assets)
		  : base(db, assets)
		{
			this.RowObjectDirtied += new RowObjectDirtiedEventHandler<int>(this.DiplomacyStatesCache_RowObjectDirtied);
		}

		private void DiplomacyStatesCache_RowObjectDirtied(object sender, int key)
		{
			if (!this.byPlayer.Reverse.ContainsKey(key))
				return;
			this.byPlayer.Remove(this.byPlayer.Reverse[key], key);
		}

		public void PostInit(PlayersCache players)
		{
			this.players = players;
		}

		protected override void OnCleared()
		{
			base.OnCleared();
			this.byPlayer.Clear();
		}

		protected override IEnumerable<KeyValuePair<int, DiplomacyInfo>> OnSynchronizeWithDatabase(
		  SQLiteConnection db,
		  IEnumerable<int> range)
		{
			if (range == null)
			{
				foreach (Row r in db.ExecuteTableQuery(Queries.GetDiplomacyInfos, true))
				{
					DiplomacyInfo o = DiplomacyStatesCache.GetDiplomacyInfoFromRow(r);
					this.TryInsertByPlayer(o.PlayerID, o.TowardsPlayerID, o.ID);
					yield return new KeyValuePair<int, DiplomacyInfo>(o.ID, o);
				}
			}
			else
			{
				foreach (int num in range)
				{
					foreach (Row r in db.ExecuteTableQuery(string.Format(Queries.GetDiplomacyInfo, (object)num.ToSQLiteValue()), true))
					{
						DiplomacyInfo o = DiplomacyStatesCache.GetDiplomacyInfoFromRow(r);
						this.TryInsertByPlayer(o.PlayerID, o.TowardsPlayerID, o.ID);
						yield return new KeyValuePair<int, DiplomacyInfo>(o.ID, o);
					}
				}
			}
		}

		protected override int OnInsert(SQLiteConnection db, int? key, DiplomacyInfo value)
		{
			if (key.HasValue)
				throw new ArgumentOutOfRangeException(nameof(key), "DiplomacyInfo insertion does not permit explicit specification of an ID.");
			this.players.GetDefaultDiplomacyReactionValue(value.PlayerID, value.TowardsPlayerID);
			int diplomacyInfoId = db.ExecuteIntegerQuery(string.Format(Queries.InsertDiplomacyInfo, (object)value.PlayerID.ToSQLiteValue(), (object)value.TowardsPlayerID.ToSQLiteValue(), (object)((int)value.State).ToSQLiteValue(), (object)value.Relations, (object)false.ToSQLiteValue()));
			this.TryInsertByPlayer(value.PlayerID, value.TowardsPlayerID, diplomacyInfoId);
			return diplomacyInfoId;
		}

		protected override void OnRemove(SQLiteConnection db, int key)
		{
			throw new NotImplementedException("DiplomacyInfo rows are never removed.");
		}

		protected override void OnUpdate(SQLiteConnection db, int key, DiplomacyInfo value)
		{
			this.TryRemoveByPlayer(value.PlayerID, value.TowardsPlayerID);
			db.ExecuteNonQuery(string.Format(Queries.UpdateDiplomacyInfo, (object)value.PlayerID.ToSQLiteValue(), (object)value.TowardsPlayerID.ToSQLiteValue(), (object)((int)value.State).ToSQLiteValue(), (object)value.Relations.ToSQLiteValue(), (object)value.isEncountered.ToSQLiteValue()), false, true);
			this.TryInsertByPlayer(value.PlayerID, value.TowardsPlayerID, key);
		}
	}
}
