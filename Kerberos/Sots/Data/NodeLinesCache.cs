// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.NodeLinesCache
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Data
{
	internal sealed class NodeLinesCache : RowCache<int, NodeLineInfo>
	{
		public NodeLinesCache(SQLiteConnection db, AssetDatabase assets)
		  : base(db, assets)
		{
		}

		private static NodeLineInfo GetNodeLineInfoFromRow(Row row)
		{
			return new NodeLineInfo()
			{
				ID = row[0].SQLiteValueToInteger(),
				System1ID = row[1].SQLiteValueToInteger(),
				System2ID = row[2].SQLiteValueToInteger(),
				Health = row[3].SQLiteValueToInteger()
			};
		}

		protected override int OnInsert(SQLiteConnection db, int? key, NodeLineInfo value)
		{
			if (key.HasValue)
				throw new ArgumentOutOfRangeException(nameof(key), "Node line insertion does not permit explicit specification of an ID.");
			int num = db.ExecuteIntegerQuery(string.Format(Queries.InsertNodeLine, (object)value.System1ID.ToSQLiteValue(), (object)value.System2ID.ToSQLiteValue(), (object)value.Health.ToSQLiteValue()));
			value.ID = num;
			return num;
		}

		protected override void OnRemove(SQLiteConnection db, int key)
		{
			db.ExecuteNonQuery(string.Format(Queries.RemoveNodeLine, (object)key.ToSQLiteValue()), false, true);
		}

		protected override IEnumerable<KeyValuePair<int, NodeLineInfo>> OnSynchronizeWithDatabase(
		  SQLiteConnection db,
		  IEnumerable<int> range)
		{
			if (range == null)
			{
				foreach (Row row in db.ExecuteTableQuery(Queries.GetNodeLines, true))
				{
					NodeLineInfo o = NodeLinesCache.GetNodeLineInfoFromRow(row);
					o.IsLoaLine = db.ExecuteTableQuery(string.Format("SELECT * FROM loa_line_records WHERE node_line_id = {0}", (object)o.ID.ToSQLiteValue()), true).Any<Row>();
					yield return new KeyValuePair<int, NodeLineInfo>(o.ID, o);
				}
			}
			else
			{
				foreach (int num in range)
				{
					foreach (Row row in db.ExecuteTableQuery(string.Format(Queries.GetNodeLine, (object)num.ToSQLiteValue()), true))
					{
						NodeLineInfo o = NodeLinesCache.GetNodeLineInfoFromRow(row);
						o.IsLoaLine = db.ExecuteTableQuery(string.Format("SELECT * FROM loa_line_records WHERE node_line_id = {0}", (object)o.ID.ToSQLiteValue()), true).Any<Row>();
						yield return new KeyValuePair<int, NodeLineInfo>(o.ID, o);
					}
				}
			}
		}

		protected override void OnUpdate(SQLiteConnection db, int key, NodeLineInfo value)
		{
			db.ExecuteNonQuery(string.Format(Queries.UpdateNodeLineHealth, (object)value.ID.ToSQLiteValue(), (object)value.Health.ToSQLiteValue()), false, true);
			this.Sync(key);
		}
	}
}
