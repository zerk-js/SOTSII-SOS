// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.MissionsCache
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.SQLite;
using Kerberos.Sots.StarFleet;
using System;
using System.Collections.Generic;

namespace Kerberos.Sots.Data
{
	internal sealed class MissionsCache : RowCache<int, MissionInfo>
	{
		public MissionsCache(SQLiteConnection db, AssetDatabase assets)
		  : base(db, assets)
		{
		}

		private static MissionInfo GetMissionInfoFromRow(Row row)
		{
			return new MissionInfo()
			{
				ID = row[0].SQLiteValueToInteger(),
				FleetID = row[1].SQLiteValueToInteger(),
				Type = (MissionType)row[2].SQLiteValueToInteger(),
				TargetSystemID = row[3].SQLiteValueToOneBasedIndex(),
				TargetOrbitalObjectID = row[4].SQLiteValueToOneBasedIndex(),
				TargetFleetID = row[5].SQLiteValueToOneBasedIndex(),
				Duration = row[6].SQLiteValueToInteger(),
				UseDirectRoute = row[7].SQLiteValueToBoolean(),
				TurnStarted = row[8].SQLiteValueToInteger(),
				StartingSystem = row[9].SQLiteValueToOneBasedIndex(),
				StationType = row[10].SQLiteValueToNullableInteger()
			};
		}

		protected override int OnInsert(SQLiteConnection db, int? key, MissionInfo value)
		{
			if (key.HasValue)
				throw new ArgumentOutOfRangeException(nameof(key), "Mission insertion does not permit explicit specification of an ID.");
			int num = db.ExecuteIntegerQuery(string.Format(Queries.InsertMission, (object)value.FleetID.ToSQLiteValue(), (object)((int)value.Type).ToSQLiteValue(), (object)value.TargetSystemID.ToOneBasedSQLiteValue(), (object)value.TargetOrbitalObjectID.ToOneBasedSQLiteValue(), (object)value.TargetFleetID.ToOneBasedSQLiteValue(), (object)value.Duration.ToSQLiteValue(), (object)value.UseDirectRoute.ToSQLiteValue(), (object)value.TurnStarted.ToSQLiteValue(), (object)value.StartingSystem.ToOneBasedSQLiteValue(), (object)value.StationType.ToNullableSQLiteValue()));
			value.ID = num;
			return num;
		}

		protected override void OnRemove(SQLiteConnection db, int key)
		{
			db.ExecuteTableQuery(string.Format(Queries.RemoveMission, (object)key.ToSQLiteValue()), true);
		}

		protected override IEnumerable<KeyValuePair<int, MissionInfo>> OnSynchronizeWithDatabase(
		  SQLiteConnection db,
		  IEnumerable<int> range)
		{
			if (range == null)
			{
				foreach (Row row in db.ExecuteTableQuery(Queries.GetMissionInfos, true))
				{
					MissionInfo o = MissionsCache.GetMissionInfoFromRow(row);
					yield return new KeyValuePair<int, MissionInfo>(o.ID, o);
				}
			}
			else
			{
				foreach (int num in range)
				{
					foreach (Row row in db.ExecuteTableQuery(string.Format(Queries.GetMissionInfo, (object)num.ToSQLiteValue()), true))
					{
						MissionInfo o = MissionsCache.GetMissionInfoFromRow(row);
						yield return new KeyValuePair<int, MissionInfo>(o.ID, o);
					}
				}
			}
		}

		protected override void OnUpdate(SQLiteConnection db, int key, MissionInfo value)
		{
			db.ExecuteTableQuery(string.Format(Queries.UpdateMission, (object)value.ID.ToSQLiteValue(), (object)((int)value.Type).ToSQLiteValue(), (object)value.TargetSystemID.ToOneBasedSQLiteValue(), (object)value.TargetOrbitalObjectID.ToOneBasedSQLiteValue(), (object)value.TargetFleetID.ToOneBasedSQLiteValue(), (object)value.Duration.ToSQLiteValue(), (object)value.UseDirectRoute.ToSQLiteValue()), true);
		}
	}
}
