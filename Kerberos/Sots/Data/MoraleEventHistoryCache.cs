// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.MoraleEventHistoryCache
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.SQLite;
using System;
using System.Collections.Generic;

namespace Kerberos.Sots.Data
{
	internal sealed class MoraleEventHistoryCache : RowCache<int, MoraleEventHistory>
	{
		public MoraleEventHistoryCache(SQLiteConnection db, AssetDatabase assets)
		  : base(db, assets)
		{
		}

		private static MoraleEventHistory ParseMoraleEventHistory(Row row)
		{
			return new MoraleEventHistory()
			{
				id = row[0].SQLiteValueToInteger(),
				turn = row[1].SQLiteValueToInteger(),
				moraleEvent = (MoralEvent)row[2].SQLiteValueToInteger(),
				playerId = row[3].SQLiteValueToInteger(),
				value = row[4].SQLiteValueToInteger(),
				colonyId = row[5].SQLiteValueToNullableInteger(),
				systemId = row[6].SQLiteValueToNullableInteger(),
				provinceId = row[7].SQLiteValueToNullableInteger()
			};
		}

		protected override int OnInsert(SQLiteConnection db, int? key, MoraleEventHistory value)
		{
			if (key.HasValue)
				throw new ArgumentOutOfRangeException(nameof(key), "MoraleEventHistory insertion does not permit explicit specification of an ID.");
			int num = db.ExecuteIntegerQuery(string.Format(Queries.InsertMoraleHistoryEvent, (object)value.turn.ToSQLiteValue(), (object)((int)value.moraleEvent).ToSQLiteValue(), (object)value.playerId.ToSQLiteValue(), (object)value.value.ToSQLiteValue(), (object)value.colonyId.ToNullableSQLiteValue(), (object)value.systemId.ToNullableSQLiteValue(), (object)value.provinceId.ToNullableSQLiteValue()));
			value.id = num;
			return num;
		}

		protected override void OnRemove(SQLiteConnection db, int key)
		{
			db.ExecuteTableQuery(string.Format(Queries.RemoveMoraleHistoryEvent, (object)key.ToSQLiteValue()), true);
		}

		protected override IEnumerable<KeyValuePair<int, MoraleEventHistory>> OnSynchronizeWithDatabase(
		  SQLiteConnection db,
		  IEnumerable<int> range)
		{
			if (range == null)
			{
				foreach (Row row in db.ExecuteTableQuery(Queries.GetMoraleHistoryEvents, true))
				{
					MoraleEventHistory o = MoraleEventHistoryCache.ParseMoraleEventHistory(row);
					yield return new KeyValuePair<int, MoraleEventHistory>(o.id, o);
				}
			}
			else
			{
				foreach (int num in range)
				{
					foreach (Row row in db.ExecuteTableQuery(string.Format(Queries.GetMoraleHistoryEvent, (object)num.ToSQLiteValue()), true))
					{
						MoraleEventHistory o = MoraleEventHistoryCache.ParseMoraleEventHistory(row);
						yield return new KeyValuePair<int, MoraleEventHistory>(o.id, o);
					}
				}
			}
		}

		protected override void OnUpdate(SQLiteConnection db, int key, MoraleEventHistory value)
		{
		}
	}
}
