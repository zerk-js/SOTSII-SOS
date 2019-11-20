// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.StationsCache
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.SQLite;
using System;
using System.Collections.Generic;

namespace Kerberos.Sots.Data
{
	internal sealed class StationsCache : RowCache<int, StationInfo>
	{
		private readonly DesignsCache designs;
		private readonly Dictionary<int, int> designIDToStationID;

		public StationsCache(SQLiteConnection db, AssetDatabase assets, DesignsCache designs)
		  : base(db, assets)
		{
			this.designIDToStationID = new Dictionary<int, int>();
			this.designs = designs;
			designs.RowObjectDirtied += new RowObjectDirtiedEventHandler<int>(this.designs_RowObjectDirtied);
		}

		protected override void OnCleared()
		{
			base.OnCleared();
			this.designIDToStationID.Clear();
		}

		private void designs_RowObjectDirtied(object sender, int key)
		{
			if (sender != this.designs || !this.designIDToStationID.ContainsKey(key))
				return;
			this.Sync(this.designIDToStationID[key]);
		}

		public static StationInfo GetStationInfoFromRow(
		  SQLiteConnection db,
		  DesignsCache designs,
		  Row row)
		{
			return new StationInfo()
			{
				OrbitalObjectInfo = GameDatabase.GetOrbitalObjectInfo(db, row[0].SQLiteValueToInteger()),
				PlayerID = row[1].SQLiteValueToInteger(),
				DesignInfo = designs[row[2].SQLiteValueToInteger()],
				WarehousedGoods = row[3].SQLiteValueToInteger(),
				ShipID = row[4].SQLiteValueToOneBasedIndex()
			};
		}

		protected override int OnInsert(SQLiteConnection db, int? key, StationInfo value)
		{
			if (key.HasValue)
				throw new ArgumentOutOfRangeException(nameof(key), "Station insertion does not permit explicit specification of an ID.");
			int num = db.ExecuteIntegerQuery(string.Format(Queries.InsertStation, (object)value.OrbitalObjectInfo.ParentID.ToNullableSQLiteValue(), (object)value.OrbitalObjectInfo.StarSystemID.ToSQLiteValue(), (object)value.OrbitalObjectInfo.OrbitalPath.ToString().ToSQLiteValue(), (object)value.OrbitalObjectInfo.Name.ToNullableSQLiteValue(), (object)value.PlayerID.ToSQLiteValue(), (object)value.DesignInfo.ID.ToSQLiteValue(), (object)value.ShipID.ToSQLiteValue()));
			value.OrbitalObjectInfo.ID = num;
			return num;
		}

		protected override void OnRemove(SQLiteConnection db, int key)
		{
			db.ExecuteNonQuery(string.Format(Queries.RemoveStation, (object)key.ToSQLiteValue()), false, true);
		}

		protected override IEnumerable<KeyValuePair<int, StationInfo>> OnSynchronizeWithDatabase(
		  SQLiteConnection db,
		  IEnumerable<int> range)
		{
			if (range == null)
			{
				foreach (Row row in db.ExecuteTableQuery(Queries.GetStationInfos, true))
				{
					StationInfo o = StationsCache.GetStationInfoFromRow(db, this.designs, row);
					this.designIDToStationID[o.DesignInfo.ID] = o.ID;
					yield return new KeyValuePair<int, StationInfo>(o.ID, o);
				}
			}
			else
			{
				foreach (int num in range)
				{
					foreach (Row row in db.ExecuteTableQuery(string.Format(Queries.GetStationInfo, (object)num.ToSQLiteValue()), true))
					{
						StationInfo o = StationsCache.GetStationInfoFromRow(db, this.designs, row);
						this.designIDToStationID[o.DesignInfo.ID] = o.ID;
						yield return new KeyValuePair<int, StationInfo>(o.ID, o);
					}
				}
			}
		}

		protected override void OnUpdate(SQLiteConnection db, int key, StationInfo value)
		{
			db.ExecuteNonQuery(string.Format(Queries.UpdateStation, (object)value.OrbitalObjectInfo.ID.ToSQLiteValue(), (object)value.PlayerID.ToSQLiteValue(), (object)value.DesignInfo.ID.ToSQLiteValue(), (object)value.ShipID.ToSQLiteValue()), false, true);
			value.OrbitalObjectInfo = GameDatabase.GetOrbitalObjectInfo(db, value.OrbitalObjectInfo.ID);
			this.Sync(key);
		}
	}
}
