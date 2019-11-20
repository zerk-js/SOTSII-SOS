// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.WeaponInstancesCache
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
	internal sealed class WeaponInstancesCache : RowCache<int, WeaponInstanceInfo>
	{
		private readonly Dictionary<int, List<int>> sectionInstanceIdToWeaponInstanceIds = new Dictionary<int, List<int>>();
		private bool requires_sync = true;
		private readonly SectionInstancesCache section_instances;

		private void WeaponInstanceAdded(int sectionInstanceID, int weaponInstanceID)
		{
			List<int> intList;
			if (!this.sectionInstanceIdToWeaponInstanceIds.TryGetValue(sectionInstanceID, out intList))
			{
				intList = new List<int>();
				this.sectionInstanceIdToWeaponInstanceIds.Add(sectionInstanceID, intList);
			}
			if (intList.Contains(weaponInstanceID))
				return;
			intList.Add(weaponInstanceID);
		}

		private void WeaponInstanceRemoved(int sectionInstanceID, int weaponInstanceID)
		{
			List<int> intList;
			if (!this.sectionInstanceIdToWeaponInstanceIds.TryGetValue(sectionInstanceID, out intList))
				return;
			intList.Remove(weaponInstanceID);
			if (intList.Count != 0)
				return;
			this.sectionInstanceIdToWeaponInstanceIds.Remove(sectionInstanceID);
		}

		public WeaponInstancesCache(
		  SQLiteConnection db,
		  AssetDatabase assets,
		  SectionInstancesCache section_instances)
		  : base(db, assets)
		{
			this.section_instances = section_instances;
			section_instances.RowObjectRemoving += new RowObjectDirtiedEventHandler<int>(this.section_instances_RowObjectRemoving);
		}

		protected override void OnCleared()
		{
			base.OnCleared();
			this.sectionInstanceIdToWeaponInstanceIds.Clear();
		}

		private void section_instances_RowObjectRemoving(object sender, int key)
		{
			List<int> intList;
			if (!this.sectionInstanceIdToWeaponInstanceIds.TryGetValue(key, out intList))
				return;
			foreach (int key1 in intList)
				this.Remove(key1);
		}

		public IEnumerable<WeaponInstanceInfo> EnumerateBySectionInstanceID(
		  int value)
		{
			if (this.requires_sync)
				this.SynchronizeWithDatabase();
			List<int> source;
			if (this.sectionInstanceIdToWeaponInstanceIds.TryGetValue(value, out source))
				return source.Select<int, WeaponInstanceInfo>((Func<int, WeaponInstanceInfo>)(x => this[x]));
			return (IEnumerable<WeaponInstanceInfo>)EmptyEnumerable<WeaponInstanceInfo>.Default;
		}

		private WeaponInstanceInfo GetWeaponInstanceInfoFromRow(Row row)
		{
			return new WeaponInstanceInfo()
			{
				ID = row[0].SQLiteValueToInteger(),
				SectionInstanceID = row[1].SQLiteValueToInteger(),
				WeaponID = row[2].SQLiteValueToInteger(),
				Structure = row[3].SQLiteValueToSingle(),
				MaxStructure = row[4].SQLiteValueToSingle(),
				NodeName = row[5].SQLiteValueToString(),
				ModuleInstanceID = row[6].SQLiteValueToNullableInteger()
			};
		}

		protected override IEnumerable<KeyValuePair<int, WeaponInstanceInfo>> OnSynchronizeWithDatabase(
		  SQLiteConnection db,
		  IEnumerable<int> range)
		{
			if (range == null)
			{
				foreach (Row row in db.ExecuteTableQuery(Queries.GetAllWeaponInstances, true))
				{
					WeaponInstanceInfo o = this.GetWeaponInstanceInfoFromRow(row);
					this.WeaponInstanceAdded(o.SectionInstanceID, o.ID);
					yield return new KeyValuePair<int, WeaponInstanceInfo>(o.ID, o);
				}
			}
			else
			{
				foreach (int num in range)
				{
					foreach (Row row in db.ExecuteTableQuery(string.Format(Queries.GetWeaponInstance, (object)num.ToSQLiteValue()), true))
					{
						WeaponInstanceInfo o = this.GetWeaponInstanceInfoFromRow(row);
						this.WeaponInstanceAdded(o.SectionInstanceID, o.ID);
						yield return new KeyValuePair<int, WeaponInstanceInfo>(o.ID, o);
					}
				}
			}
			this.requires_sync = false;
		}

		protected override int OnInsert(SQLiteConnection db, int? key, WeaponInstanceInfo value)
		{
			if (key.HasValue)
				throw new ArgumentOutOfRangeException(nameof(key), "WeaponInstanceInfo insertion does not allow explicit specification of an ID.");
			int weaponInstanceID = db.ExecuteIntegerQuery(string.Format(Queries.InsertWeaponInstance, (object)value.SectionInstanceID.ToSQLiteValue(), (object)value.WeaponID.ToSQLiteValue(), (object)value.Structure.ToSQLiteValue(), (object)value.MaxStructure.ToSQLiteValue(), (object)value.NodeName.ToSQLiteValue(), (object)value.ModuleInstanceID.ToNullableSQLiteValue()));
			this.WeaponInstanceAdded(value.SectionInstanceID, weaponInstanceID);
			return weaponInstanceID;
		}

		protected override void OnUpdate(SQLiteConnection db, int key, WeaponInstanceInfo value)
		{
			WeaponInstanceInfo weaponInstanceInfo = this[key];
			db.ExecuteNonQuery(string.Format(Queries.UpdateWeaponInstance, (object)key.ToSQLiteValue(), (object)value.Structure.ToSQLiteValue()), false, true);
			if (weaponInstanceInfo.SectionInstanceID == value.SectionInstanceID)
				return;
			this.WeaponInstanceRemoved(weaponInstanceInfo.SectionInstanceID, key);
			this.WeaponInstanceAdded(value.SectionInstanceID, key);
		}

		protected override void OnRemove(SQLiteConnection db, int key)
		{
		}
	}
}
