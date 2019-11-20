// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.SectionInstancesCache
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Data
{
	internal sealed class SectionInstancesCache : RowCache<int, SectionInstanceInfo>
	{
		private readonly DesignsCache designs;
		private readonly ShipsCache ships;
		private readonly StationsCache stations;
		private ArmorInstancesCache armor_instances;
		private WeaponInstancesCache weapon_instances;
		private ModuleInstancesCache module_instances;

		public SectionInstancesCache(
		  SQLiteConnection db,
		  AssetDatabase assets,
		  DesignsCache designs,
		  ShipsCache ships,
		  StationsCache stations)
		  : base(db, assets)
		{
			this.designs = designs;
			this.ships = ships;
			this.stations = stations;
			designs.RowObjectRemoving += new RowObjectDirtiedEventHandler<int>(this.designs_RowObjectRemoving);
			ships.RowObjectRemoving += new RowObjectDirtiedEventHandler<int>(this.ships_RowObjectRemoving);
			stations.RowObjectRemoving += new RowObjectDirtiedEventHandler<int>(this.stations_RowObjectRemoving);
		}

		internal void PostInit(
		  ArmorInstancesCache armor_instances,
		  WeaponInstancesCache weapon_instances,
		  ModuleInstancesCache module_instances)
		{
			this.armor_instances = armor_instances;
			this.weapon_instances = weapon_instances;
			this.module_instances = module_instances;
			weapon_instances.RowObjectDirtied += new RowObjectDirtiedEventHandler<int>(this.weapon_instances_RowObjectDirtied);
			module_instances.RowObjectDirtied += new RowObjectDirtiedEventHandler<int>(this.module_instances_RowObjectDirtied);
		}

		private void module_instances_RowObjectDirtied(object sender, int key)
		{
			this.SyncRange(this.Values.Where<SectionInstanceInfo>((Func<SectionInstanceInfo, bool>)(x => this.module_instances[key].SectionInstanceID == x.ID)).Select<SectionInstanceInfo, int>((Func<SectionInstanceInfo, int>)(y => y.ID)));
		}

		private void weapon_instances_RowObjectDirtied(object sender, int key)
		{
			this.SyncRange(this.Values.Where<SectionInstanceInfo>((Func<SectionInstanceInfo, bool>)(x => this.weapon_instances[key].SectionInstanceID == x.ID)).Select<SectionInstanceInfo, int>((Func<SectionInstanceInfo, int>)(y => y.ID)));
		}

		private void stations_RowObjectRemoving(object sender, int key)
		{
			foreach (SectionInstanceInfo sectionInstanceInfo in this.Values.Where<SectionInstanceInfo>((Func<SectionInstanceInfo, bool>)(x =>
		   {
			   int? stationId = x.StationID;
			   int num = key;
			   if (stationId.GetValueOrDefault() == num)
				   return stationId.HasValue;
			   return false;
		   })).ToList<SectionInstanceInfo>())
				this.Remove(sectionInstanceInfo.ID);
		}

		private void ships_RowObjectRemoving(object sender, int key)
		{
			foreach (SectionInstanceInfo sectionInstanceInfo in this.Values.Where<SectionInstanceInfo>((Func<SectionInstanceInfo, bool>)(x =>
		   {
			   int? shipId = x.ShipID;
			   int num = key;
			   if (shipId.GetValueOrDefault() == num)
				   return shipId.HasValue;
			   return false;
		   })).ToList<SectionInstanceInfo>())
				this.Remove(sectionInstanceInfo.ID);
		}

		private void designs_RowObjectRemoving(object sender, int key)
		{
			foreach (SectionInstanceInfo sectionInstanceInfo in this.Values.Where<SectionInstanceInfo>((Func<SectionInstanceInfo, bool>)(x => ((IEnumerable<DesignSectionInfo>)this.designs[key].DesignSections).Any<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(y => x.SectionID == y.ID)))).ToList<SectionInstanceInfo>())
				this.Remove(sectionInstanceInfo.ID);
		}

		private SectionInstanceInfo GetSectionInstanceInfoFromRow(Row row)
		{
			int integer = row[0].SQLiteValueToInteger();
			return new SectionInstanceInfo()
			{
				ID = integer,
				SectionID = row[1].SQLiteValueToInteger(),
				ShipID = row[2].SQLiteValueToNullableInteger(),
				StationID = row[3].SQLiteValueToNullableInteger(),
				Structure = row[4].SQLiteValueToInteger(),
				Supply = row[5].SQLiteValueToInteger(),
				Crew = row[6].SQLiteValueToInteger(),
				Signature = row[7].SQLiteValueToSingle(),
				RepairPoints = row[8].SQLiteValueToInteger(),
				Armor = this.armor_instances.ContainsKey(integer) ? this.armor_instances[integer] : ArmorInstancesCache.EmptyArmorInstances,
				WeaponInstances = this.weapon_instances.EnumerateBySectionInstanceID(integer).ToList<WeaponInstanceInfo>(),
				ModuleInstances = this.module_instances.EnumerateBySectionInstanceID(integer).ToList<ModuleInstanceInfo>()
			};
		}

		protected override IEnumerable<KeyValuePair<int, SectionInstanceInfo>> OnSynchronizeWithDatabase(
		  SQLiteConnection db,
		  IEnumerable<int> range)
		{
			if (range == null)
			{
				foreach (Row row in db.ExecuteTableQuery(Queries.GetAllShipSectionInstances, true))
				{
					SectionInstanceInfo o = this.GetSectionInstanceInfoFromRow(row);
					yield return new KeyValuePair<int, SectionInstanceInfo>(o.ID, o);
				}
			}
			else
			{
				foreach (int num in range)
				{
					foreach (Row row in db.ExecuteTableQuery(string.Format(Queries.GetShipSectionInstance, (object)num.ToSQLiteValue()), true))
					{
						SectionInstanceInfo o = this.GetSectionInstanceInfoFromRow(row);
						yield return new KeyValuePair<int, SectionInstanceInfo>(o.ID, o);
					}
				}
			}
		}

		protected override int OnInsert(SQLiteConnection db, int? key, SectionInstanceInfo value)
		{
			if (key.HasValue)
				throw new ArgumentOutOfRangeException(nameof(key), "SectionInstanceInfo insertion does not allow explicit specification of an ID.");
			return db.ExecuteIntegerQuery(string.Format(Queries.InsertSectionInstance, (object)value.SectionID.ToSQLiteValue(), (object)value.ShipID.ToNullableSQLiteValue(), (object)value.StationID.ToNullableSQLiteValue(), (object)value.Structure.ToSQLiteValue(), (object)value.Supply.ToSQLiteValue(), (object)value.Crew.ToSQLiteValue(), (object)value.Signature.ToSQLiteValue(), (object)value.RepairPoints.ToSQLiteValue(), (object)string.Empty));
		}

		protected override void OnUpdate(SQLiteConnection db, int key, SectionInstanceInfo value)
		{
			if (!value.ShipID.HasValue)
				throw new ArgumentException("Exactly one of section.ShipID or section.StationID must have a value.");
			db.ExecuteNonQuery(string.Format(Queries.UpdateSectionInstance, (object)key.ToSQLiteValue(), (object)value.SectionID.ToSQLiteValue(), (object)value.ShipID.ToNullableSQLiteValue(), (object)value.StationID.ToNullableSQLiteValue(), (object)value.Structure.ToSQLiteValue(), (object)value.Supply.ToSQLiteValue(), (object)value.Crew.ToSQLiteValue(), (object)value.Signature.ToSQLiteValue(), (object)value.RepairPoints.ToSQLiteValue()), false, true);
		}

		protected override void OnRemove(SQLiteConnection db, int key)
		{
			db.ExecuteNonQuery(string.Format(Queries.RemoveSectionInstance, (object)key.ToSQLiteValue()), false, true);
		}
	}
}
