// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ShipsCache
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.SQLite;
using System;
using System.Collections.Generic;

namespace Kerberos.Sots.Data
{
	internal sealed class ShipsCache : RowCache<int, ShipInfo>
	{
		private readonly DesignsCache designs;
		private readonly Dictionary<int, int> designIDToShipID;

		public ShipsCache(SQLiteConnection db, AssetDatabase assets, DesignsCache designs)
		  : base(db, assets)
		{
			this.designIDToShipID = new Dictionary<int, int>();
			this.designs = designs;
			designs.RowObjectDirtied += new RowObjectDirtiedEventHandler<int>(this.designs_RowObjectDirtied);
		}

		protected override void OnCleared()
		{
			base.OnCleared();
			this.designIDToShipID.Clear();
		}

		private void designs_RowObjectDirtied(object sender, int key)
		{
			if (sender != this.designs || !this.designIDToShipID.ContainsKey(key))
				return;
			this.Sync(this.designIDToShipID[key]);
		}

		private ShipInfo GetShipInfoFromRow(Row row)
		{
			int integer = row[2].SQLiteValueToInteger();
			return new ShipInfo()
			{
				ID = row[0].SQLiteValueToInteger(),
				FleetID = row[1].SQLiteValueToOneBasedIndex(),
				DesignID = integer,
				ParentID = row[3].SQLiteValueToInteger(),
				ShipName = row[4].SQLiteValueToString(),
				SerialNumber = row[5].SQLiteValueToInteger(),
				ShipFleetPosition = row[6].SQLiteValueToNullableVector3(),
				ShipSystemPosition = row[7].SQLiteValueToNullableMatrix(),
				Params = (ShipParams)row[8].SQLiteValueToInteger(),
				RiderIndex = row[9].SQLiteValueToInteger(),
				PsionicPower = row[10].SQLiteValueToInteger(),
				AIFleetID = row[11].SQLiteValueToNullableInteger(),
				ComissionDate = row[12].SQLiteValueToInteger(),
				SlavesObtained = row[13].SQLiteValueToDouble(),
				LoaCubes = row[14].SQLiteValueToInteger(),
				DesignInfo = this.designs[integer]
			};
		}

		protected override int OnInsert(SQLiteConnection db, int? key, ShipInfo value)
		{
			if (key.HasValue)
				throw new ArgumentOutOfRangeException(nameof(key), "Mission insertion does not permit explicit specification of an ID.");
			int num = db.ExecuteIntegerQuery(string.Format(Queries.InsertShip, (object)value.FleetID.ToOneBasedSQLiteValue(), (object)value.DesignID.ToSQLiteValue(), (object)value.ParentID.ToSQLiteValue(), (object)value.ShipName.ToSQLiteValue(), (object)value.SerialNumber.ToSQLiteValue(), (object)((int)value.Params).ToSQLiteValue(), (object)value.RiderIndex.ToSQLiteValue(), (object)value.PsionicPower.ToSQLiteValue(), (object)value.AIFleetID.ToNullableSQLiteValue(), (object)value.ComissionDate.ToSQLiteValue(), (object)value.LoaCubes.ToSQLiteValue()));
			value.ID = num;
			this.designIDToShipID[value.DesignID] = value.ID;
			return num;
		}

		protected override void OnRemove(SQLiteConnection db, int key)
		{
			this.designIDToShipID.Remove(this[key].DesignID);
			db.ExecuteNonQuery(string.Format(Queries.RemoveShip, (object)key.ToSQLiteValue()), false, true);
		}

		protected override IEnumerable<KeyValuePair<int, ShipInfo>> OnSynchronizeWithDatabase(
		  SQLiteConnection db,
		  IEnumerable<int> range)
		{
			if (range == null)
			{
				foreach (Row row in db.ExecuteTableQuery(Queries.GetShipInfos, true))
				{
					ShipInfo o = this.GetShipInfoFromRow(row);
					this.designIDToShipID[o.DesignID] = o.ID;
					yield return new KeyValuePair<int, ShipInfo>(o.ID, o);
				}
			}
			else
			{
				foreach (int num in range)
				{
					foreach (Row row in db.ExecuteTableQuery(string.Format(Queries.GetShipInfo, (object)num.ToSQLiteValue()), true))
					{
						ShipInfo o = this.GetShipInfoFromRow(row);
						this.designIDToShipID[o.DesignID] = o.ID;
						yield return new KeyValuePair<int, ShipInfo>(o.ID, o);
					}
				}
			}
		}

		protected override void OnUpdate(SQLiteConnection db, int key, ShipInfo value)
		{
			throw new NotImplementedException("All updating is handled via finer-grained external calls and reliance on Sync().");
		}
	}
}
