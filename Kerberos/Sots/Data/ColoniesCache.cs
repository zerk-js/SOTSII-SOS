// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ColoniesCache
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.SQLite;
using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Data
{
	internal sealed class ColoniesCache : RowCache<int, ColonyInfo>
	{
		public ColoniesCache(SQLiteConnection db, AssetDatabase assets)
		  : base(db, assets)
		{
		}

		private static ColonyFactionInfo GetColonyFactionInfoFromRow(
		  Row row,
		  int orbitalObjectID)
		{
			return new ColonyFactionInfo()
			{
				OrbitalObjectID = orbitalObjectID,
				FactionID = row[0].SQLiteValueToInteger(),
				CivilianPop = row[1].SQLiteValueToDouble(),
				Morale = row[2].SQLiteValueToInteger(),
				CivPopWeight = row[3].SQLiteValueToSingle(),
				TurnEstablished = row[4].SQLiteValueToInteger(),
				LastMorale = row[5].SQLiteValueToInteger()
			};
		}

		public static IEnumerable<ColonyFactionInfo> GetColonyFactionInfosFromOrbitalObjectID(
		  SQLiteConnection db,
		  int orbitalObjectID)
		{
			foreach (Row row in db.ExecuteTableQuery(string.Format(Queries.GetCivilianPopulationsForColony, (object)orbitalObjectID), true))
				yield return ColoniesCache.GetColonyFactionInfoFromRow(row, orbitalObjectID);
		}

		private static ColonyInfo GetColonyInfoFromRow(SQLiteConnection db, Row row)
		{
			int integer = row[1].SQLiteValueToInteger();
			float single1 = row[6].SQLiteValueToSingle();
			float single2 = row[7].SQLiteValueToSingle();
			float single3 = row[8].SQLiteValueToSingle();
			float single4 = row[13].SQLiteValueToSingle();
			int num = db.ExecuteIntegerQuery(string.Format(Queries.GetOrbitalObjectStarSystemID, (object)integer.ToSQLiteValue()));
			return new ColonyInfo()
			{
				ID = row[0].SQLiteValueToInteger(),
				OrbitalObjectID = integer,
				PlayerID = row[2].SQLiteValueToInteger(),
				ImperialPop = row[3].SQLiteValueToDouble(),
				CivilianWeight = row[4].SQLiteValueToSingle(),
				TurnEstablished = row[5].SQLiteValueToInteger(),
				TerraRate = single1,
				InfraRate = single2,
				ShipConRate = single3,
				TradeRate = ((float)(1.0 - ((double)single1 + (double)single2 + (double)single3 + (double)single4))).Saturate(),
				OverharvestRate = row[9].SQLiteValueToSingle(),
				EconomyRating = row[10].SQLiteValueToSingle(),
				CurrentStage = (ColonyStage)row[11].SQLiteValueToInteger(),
				OverdevelopProgress = row[12].SQLiteValueToSingle(),
				OverdevelopRate = single4,
				PopulationBiosphereRate = row[14].SQLiteValueToSingle(),
				isHardenedStructures = row[15].SQLiteValueToBoolean(),
				RebellionType = (RebellionType)row[16].SQLiteValueToInteger(),
				RebellionTurns = row[17].SQLiteValueToInteger(),
				TurnsOverharvested = row[18].SQLiteValueToInteger(),
				RepairPoints = row[19].SQLiteValueToInteger(),
				SlaveWorkRate = row[20].SQLiteValueToSingle(),
				DamagedLastTurn = row[21].SQLiteValueToBoolean(),
				RepairPointsMax = row[22].SQLiteValueToInteger(),
				OwningColony = row[23].SQLiteValueToBoolean(),
				ReplicantsOn = row[24].SQLiteValueToBoolean(),
				Factions = ColoniesCache.GetColonyFactionInfosFromOrbitalObjectID(db, integer).ToArray<ColonyFactionInfo>(),
				CachedStarSystemID = num
			};
		}

		protected override int OnInsert(SQLiteConnection db, int? key, ColonyInfo value)
		{
			if (key.HasValue)
				throw new ArgumentOutOfRangeException(nameof(key), "Colony insertion does not permit explicit specification of an ID.");
			int num = db.ExecuteIntegerQuery(string.Format(Queries.InsertColony, (object)value.OrbitalObjectID.ToSQLiteValue(), (object)value.PlayerID.ToSQLiteValue(), (object)value.ImperialPop.ToSQLiteValue(), (object)value.CivilianWeight.ToSQLiteValue(), (object)value.TurnEstablished.ToSQLiteValue()));
			value.CachedStarSystemID = db.ExecuteIntegerQuery(string.Format(Queries.GetOrbitalObjectStarSystemID, (object)value.OrbitalObjectID.ToSQLiteValue()));
			value.ID = num;
			return num;
		}

		protected override void OnRemove(SQLiteConnection db, int key)
		{
			db.ExecuteNonQuery(string.Format(Queries.RemoveColony, (object)key), false, true);
		}

		protected override IEnumerable<KeyValuePair<int, ColonyInfo>> OnSynchronizeWithDatabase(
		  SQLiteConnection db,
		  IEnumerable<int> range)
		{
			if (range == null)
			{
				foreach (Row row in db.ExecuteTableQuery(Queries.GetColonyInfos, true))
				{
					ColonyInfo o = ColoniesCache.GetColonyInfoFromRow(db, row);
					yield return new KeyValuePair<int, ColonyInfo>(o.ID, o);
				}
			}
			else
			{
				foreach (int num in range)
				{
					foreach (Row row in db.ExecuteTableQuery(string.Format(Queries.GetColonyInfo, (object)num.ToSQLiteValue()), true))
					{
						ColonyInfo o = ColoniesCache.GetColonyInfoFromRow(db, row);
						yield return new KeyValuePair<int, ColonyInfo>(o.ID, o);
					}
				}
			}
		}

		protected override void OnUpdate(SQLiteConnection db, int key, ColonyInfo value)
		{
			db.ExecuteNonQuery(string.Format(Queries.UpdateColony, (object)key, (object)value.PlayerID, (object)value.ImperialPop, (object)value.CivilianWeight, (object)value.TerraRate, (object)value.InfraRate, (object)value.ShipConRate, (object)value.OverharvestRate, (object)value.EconomyRating, (object)((int)value.CurrentStage).ToSQLiteValue(), (object)value.OverdevelopProgress.ToSQLiteValue(), (object)value.OverdevelopRate.ToSQLiteValue(), (object)value.PopulationBiosphereRate.ToSQLiteValue(), (object)value.isHardenedStructures.ToSQLiteValue(), (object)((int)value.RebellionType).ToSQLiteValue(), (object)value.RebellionTurns.ToSQLiteValue(), (object)value.TurnsOverharvested.ToSQLiteValue(), (object)value.RepairPoints.ToSQLiteValue(), (object)value.SlaveWorkRate.ToSQLiteValue(), (object)value.DamagedLastTurn.ToSQLiteValue(), (object)value.RepairPointsMax.ToSQLiteValue(), (object)value.OwningColony.ToSQLiteValue(), (object)value.ReplicantsOn.ToSQLiteValue()), false, true);
		}
	}
}
