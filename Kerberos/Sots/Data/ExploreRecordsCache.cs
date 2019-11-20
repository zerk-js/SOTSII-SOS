// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ExploreRecordsCache
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.SQLite;
using System;
using System.Collections.Generic;

namespace Kerberos.Sots.Data
{
	internal sealed class ExploreRecordsCache : RowCache<int, ExploreRecordInfo>
	{
		public ExploreRecordsCache(SQLiteConnection db, AssetDatabase assets)
		  : base(db, assets)
		{
		}

		private static ExploreRecordInfo GetExploreRecordInfoFromRow(Row row)
		{
			return new ExploreRecordInfo()
			{
				SystemId = int.Parse(row[0]),
				PlayerId = int.Parse(row[1]),
				LastTurnExplored = int.Parse(row[2]),
				Visible = bool.Parse(row[3]),
				Explored = bool.Parse(row[4])
			};
		}

		public static int GetRecordKey(ExploreRecordInfo value)
		{
			return value.SystemId << 8 | value.PlayerId;
		}

		public static int GetSystemFromKey(int key)
		{
			return key >> 8;
		}

		public static int GetPlayerFromKey(int key)
		{
			return key & (int)byte.MaxValue;
		}

		protected override int OnInsert(SQLiteConnection db, int? key, ExploreRecordInfo value)
		{
			db.ExecuteIntegerQuery(string.Format(Queries.UpdateExploreRecord, (object)value.SystemId.ToSQLiteValue(), (object)value.PlayerId.ToSQLiteValue(), (object)value.LastTurnExplored.ToSQLiteValue(), (object)value.Visible.ToString(), (object)value.Explored.ToString()));
			return ExploreRecordsCache.GetRecordKey(value);
		}

		protected override void OnRemove(SQLiteConnection db, int key)
		{
			throw new NotImplementedException("All updating is handled via finer-grained external calls and reliance on Sync().");
		}

		protected override IEnumerable<KeyValuePair<int, ExploreRecordInfo>> OnSynchronizeWithDatabase(
		  SQLiteConnection db,
		  IEnumerable<int> range)
		{
			if (range == null)
			{
				foreach (Row row in db.ExecuteTableQuery(Queries.GetExploreRecordInfos, true))
				{
					ExploreRecordInfo o = ExploreRecordsCache.GetExploreRecordInfoFromRow(row);
					yield return new KeyValuePair<int, ExploreRecordInfo>(ExploreRecordsCache.GetRecordKey(o), o);
				}
			}
			else
			{
				foreach (int key in range)
				{
					int systemID = ExploreRecordsCache.GetSystemFromKey(key);
					int playerID = ExploreRecordsCache.GetPlayerFromKey(key);
					foreach (Row row in db.ExecuteTableQuery(string.Format(Queries.GetExploreRecordInfo, (object)systemID.ToSQLiteValue(), (object)playerID.ToSQLiteValue()), true))
					{
						ExploreRecordInfo o = ExploreRecordsCache.GetExploreRecordInfoFromRow(row);
						yield return new KeyValuePair<int, ExploreRecordInfo>(key, o);
					}
				}
			}
		}

		protected override void OnUpdate(SQLiteConnection db, int key, ExploreRecordInfo value)
		{
			db.ExecuteTableQuery(string.Format(Queries.UpdateExploreRecord, (object)value.SystemId, (object)value.PlayerId, (object)value.LastTurnExplored, (object)value.Visible, (object)value.Explored), true);
			this.Sync(ExploreRecordsCache.GetRecordKey(value));
		}
	}
}
