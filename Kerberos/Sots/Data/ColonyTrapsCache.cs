// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ColonyTrapsCache
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.SQLite;
using System;
using System.Collections.Generic;

namespace Kerberos.Sots.Data
{
	internal sealed class ColonyTrapsCache : RowCache<int, ColonyTrapInfo>
	{
		public ColonyTrapsCache(SQLiteConnection db, AssetDatabase assets)
		  : base(db, assets)
		{
		}

		private static ColonyTrapInfo GetColonyTrapInfoFromRow(Row row)
		{
			return new ColonyTrapInfo()
			{
				ID = row[0].SQLiteValueToInteger(),
				SystemID = row[1].SQLiteValueToInteger(),
				PlanetID = row[2].SQLiteValueToInteger(),
				FleetID = row[3].SQLiteValueToInteger()
			};
		}

		protected override int OnInsert(SQLiteConnection db, int? key, ColonyTrapInfo value)
		{
			if (key.HasValue)
				throw new ArgumentOutOfRangeException(nameof(key), "Colony trap insertion does not permit explicit specification of an ID.");
			int num = db.ExecuteIntegerQuery(string.Format(Queries.InsertColonyTrap, (object)value.SystemID.ToSQLiteValue(), (object)value.PlanetID.ToSQLiteValue(), (object)value.FleetID.ToSQLiteValue()));
			value.ID = num;
			return num;
		}

		protected override void OnRemove(SQLiteConnection db, int key)
		{
			db.ExecuteTableQuery(string.Format(Queries.RemoveColonyTrapInfo, (object)key.ToSQLiteValue()), true);
		}

		protected override IEnumerable<KeyValuePair<int, ColonyTrapInfo>> OnSynchronizeWithDatabase(
		  SQLiteConnection db,
		  IEnumerable<int> range)
		{
			if (range == null)
			{
				foreach (Row row in db.ExecuteTableQuery(Queries.GetColonyTrapInfos, true))
				{
					ColonyTrapInfo o = ColonyTrapsCache.GetColonyTrapInfoFromRow(row);
					yield return new KeyValuePair<int, ColonyTrapInfo>(o.ID, o);
				}
			}
			else
			{
				foreach (int num in range)
				{
					foreach (Row row in db.ExecuteTableQuery(string.Format(Queries.GetColonyTrapInfo, (object)num.ToSQLiteValue()), true))
					{
						ColonyTrapInfo o = ColonyTrapsCache.GetColonyTrapInfoFromRow(row);
						yield return new KeyValuePair<int, ColonyTrapInfo>(o.ID, o);
					}
				}
			}
		}

		protected override void OnUpdate(SQLiteConnection db, int key, ColonyTrapInfo value)
		{
			throw new NotImplementedException("All updating is handled via finer-grained external calls and reliance on Sync().");
		}
	}
}
