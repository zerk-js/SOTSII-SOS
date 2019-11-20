// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ArmorInstancesCache
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.SQLite;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Data
{
	internal sealed class ArmorInstancesCache : RowCache<int, Dictionary<ArmorSide, DamagePattern>>
	{
		public static readonly Dictionary<ArmorSide, DamagePattern> EmptyArmorInstances = new Dictionary<ArmorSide, DamagePattern>();
		private readonly SectionInstancesCache section_instances;

		static ArmorInstancesCache()
		{
			for (int index = 0; index < 4; ++index)
				ArmorInstancesCache.EmptyArmorInstances.Add((ArmorSide)index, new DamagePattern(0, 0));
		}

		public ArmorInstancesCache(
		  SQLiteConnection db,
		  AssetDatabase assets,
		  SectionInstancesCache section_instances)
		  : base(db, assets)
		{
			this.section_instances = section_instances;
			section_instances.RowObjectRemoving += new RowObjectDirtiedEventHandler<int>(this.section_instances_RowObjectRemoving);
		}

		private void section_instances_RowObjectRemoving(object sender, int key)
		{
			this.Remove(key);
		}

		private Dictionary<ArmorSide, DamagePattern> GetArmorInstancesFromRows(
		  IEnumerable<Row> rows)
		{
			Dictionary<ArmorSide, DamagePattern> dictionary = new Dictionary<ArmorSide, DamagePattern>();
			foreach (Row row in rows)
				dictionary.Add((ArmorSide)row[2].SQLiteValueToInteger(), DamagePattern.FromDatabaseString(row[3].SQLiteValueToString()));
			for (int index = 0; index < 4; ++index)
			{
				if (!dictionary.ContainsKey((ArmorSide)index))
					dictionary.Add((ArmorSide)index, new DamagePattern(0, 0));
			}
			return dictionary;
		}

		private IEnumerable<KeyValuePair<int, Dictionary<ArmorSide, DamagePattern>>> GetAllArmorInstancesFromRows(
		  IEnumerable<Row> rows)
		{
			int sectionInstanceIdIndex = 1;
			return rows.GroupBy<Row, int>((Func<Row, int>)(row => row[sectionInstanceIdIndex].SQLiteValueToInteger())).Select<IGrouping<int, Row>, KeyValuePair<int, Dictionary<ArmorSide, DamagePattern>>>((Func<IGrouping<int, Row>, KeyValuePair<int, Dictionary<ArmorSide, DamagePattern>>>)(x => new KeyValuePair<int, Dictionary<ArmorSide, DamagePattern>>(x.Key, this.GetArmorInstancesFromRows((IEnumerable<Row>)x))));
		}

		protected override IEnumerable<KeyValuePair<int, Dictionary<ArmorSide, DamagePattern>>> OnSynchronizeWithDatabase(
		  SQLiteConnection db,
		  IEnumerable<int> range)
		{
			if (range == null)
			{
				Table rows = db.ExecuteTableQuery(Queries.GetAllArmorInstances, true);
				IEnumerable<KeyValuePair<int, Dictionary<ArmorSide, DamagePattern>>> os = this.GetAllArmorInstancesFromRows((IEnumerable<Row>)rows);
				foreach (KeyValuePair<int, Dictionary<ArmorSide, DamagePattern>> keyValuePair in os)
					yield return keyValuePair;
			}
			else
			{
				foreach (int key in range)
				{
					Table rows = db.ExecuteTableQuery(string.Format(Queries.GetArmorInstances, (object)key.ToSQLiteValue()), true);
					Dictionary<ArmorSide, DamagePattern> o = this.GetArmorInstancesFromRows((IEnumerable<Row>)rows);
					yield return new KeyValuePair<int, Dictionary<ArmorSide, DamagePattern>>(key, o);
				}
			}
		}

		protected override int OnInsert(
		  SQLiteConnection db,
		  int? key,
		  Dictionary<ArmorSide, DamagePattern> value)
		{
			if (!key.HasValue)
				throw new ArgumentOutOfRangeException(nameof(key), "Armor instance insertion requires explicit specification of an existing section instance ID.");
			foreach (KeyValuePair<ArmorSide, DamagePattern> keyValuePair in value)
				db.ExecuteNonQuery(string.Format(Queries.InsertArmorInstance, (object)key.Value, (object)((int)keyValuePair.Key).ToSQLiteValue(), (object)keyValuePair.Value.ToDatabaseString()), false, true);
			return key.Value;
		}

		protected override void OnUpdate(
		  SQLiteConnection db,
		  int key,
		  Dictionary<ArmorSide, DamagePattern> value)
		{
			foreach (KeyValuePair<ArmorSide, DamagePattern> keyValuePair in value)
				db.ExecuteNonQuery(string.Format(Queries.UpdateArmorInstance, (object)key, (object)((int)keyValuePair.Key).ToSQLiteValue(), (object)keyValuePair.Value.ToDatabaseString()), false, true);
		}

		protected override void OnRemove(SQLiteConnection db, int key)
		{
		}
	}
}
