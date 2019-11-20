// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ModuleInstancesCache
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
	internal sealed class ModuleInstancesCache : RowCache<int, ModuleInstanceInfo>
	{
		private readonly Dictionary<int, List<int>> sectionInstanceIdToModuleInstanceIds = new Dictionary<int, List<int>>();
		private bool requires_sync = true;
		private readonly SectionInstancesCache section_instances;

		private void ModuleInstanceAdded(int sectionInstanceID, int moduleInstanceID)
		{
			List<int> intList;
			if (!this.sectionInstanceIdToModuleInstanceIds.TryGetValue(sectionInstanceID, out intList))
			{
				intList = new List<int>();
				this.sectionInstanceIdToModuleInstanceIds.Add(sectionInstanceID, intList);
			}
			if (intList.Contains(moduleInstanceID))
				return;
			intList.Add(moduleInstanceID);
		}

		private void ModuleInstanceRemoved(int sectionInstanceID, int moduleInstanceID)
		{
			List<int> intList;
			if (!this.sectionInstanceIdToModuleInstanceIds.TryGetValue(sectionInstanceID, out intList))
				return;
			intList.Remove(moduleInstanceID);
			if (intList.Count != 0)
				return;
			this.sectionInstanceIdToModuleInstanceIds.Remove(sectionInstanceID);
		}

		public ModuleInstancesCache(
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
			this.sectionInstanceIdToModuleInstanceIds.Clear();
		}

		private void section_instances_RowObjectRemoving(object sender, int key)
		{
			List<int> intList;
			if (!this.sectionInstanceIdToModuleInstanceIds.TryGetValue(key, out intList))
				return;
			foreach (int key1 in intList)
				this.Remove(key1);
		}

		public IEnumerable<ModuleInstanceInfo> EnumerateBySectionInstanceID(
		  int value)
		{
			if (this.requires_sync)
				this.SynchronizeWithDatabase();
			List<int> source;
			if (this.sectionInstanceIdToModuleInstanceIds.TryGetValue(value, out source))
				return source.Select<int, ModuleInstanceInfo>((Func<int, ModuleInstanceInfo>)(x => this[x]));
			return (IEnumerable<ModuleInstanceInfo>)EmptyEnumerable<ModuleInstanceInfo>.Default;
		}

		private ModuleInstanceInfo GetModuleInstanceInfoFromRow(Row row)
		{
			return new ModuleInstanceInfo()
			{
				ID = row[0].SQLiteValueToInteger(),
				SectionInstanceID = row[1].SQLiteValueToInteger(),
				ModuleNodeID = row[2].SQLiteValueToString(),
				Structure = row[3].SQLiteValueToInteger(),
				RepairPoints = row[4].SQLiteValueToInteger()
			};
		}

		protected override IEnumerable<KeyValuePair<int, ModuleInstanceInfo>> OnSynchronizeWithDatabase(
		  SQLiteConnection db,
		  IEnumerable<int> range)
		{
			if (range == null)
			{
				foreach (Row row in db.ExecuteTableQuery(Queries.GetAllModuleInstances, true))
				{
					ModuleInstanceInfo o = this.GetModuleInstanceInfoFromRow(row);
					this.ModuleInstanceAdded(o.SectionInstanceID, o.ID);
					yield return new KeyValuePair<int, ModuleInstanceInfo>(o.ID, o);
				}
			}
			else
			{
				foreach (int num in range)
				{
					foreach (Row row in db.ExecuteTableQuery(string.Format(Queries.GetModuleInstance, (object)num.ToSQLiteValue()), true))
					{
						ModuleInstanceInfo o = this.GetModuleInstanceInfoFromRow(row);
						this.ModuleInstanceAdded(o.SectionInstanceID, o.ID);
						yield return new KeyValuePair<int, ModuleInstanceInfo>(o.ID, o);
					}
				}
			}
			this.requires_sync = false;
		}

		protected override int OnInsert(SQLiteConnection db, int? key, ModuleInstanceInfo value)
		{
			if (key.HasValue)
				throw new ArgumentOutOfRangeException(nameof(key), "ModuleInstanceInfo insertion does not allow explicit specification of an ID.");
			int moduleInstanceID = db.ExecuteIntegerQuery(string.Format(Queries.InsertModuleInstance, (object)value.SectionInstanceID.ToSQLiteValue(), (object)value.ModuleNodeID.ToSQLiteValue(), (object)value.Structure.ToSQLiteValue(), (object)value.RepairPoints.ToSQLiteValue()));
			this.ModuleInstanceAdded(value.SectionInstanceID, moduleInstanceID);
			return moduleInstanceID;
		}

		protected override void OnUpdate(SQLiteConnection db, int key, ModuleInstanceInfo value)
		{
			ModuleInstanceInfo moduleInstanceInfo = this[key];
			db.ExecuteNonQuery(string.Format(Queries.UpdateModuleInstance, (object)key.ToSQLiteValue(), (object)value.SectionInstanceID.ToSQLiteValue(), (object)value.ModuleNodeID.ToSQLiteValue(), (object)value.Structure.ToSQLiteValue(), (object)value.RepairPoints.ToSQLiteValue()), false, true);
			if (moduleInstanceInfo.SectionInstanceID == value.SectionInstanceID)
				return;
			this.ModuleInstanceRemoved(moduleInstanceInfo.SectionInstanceID, key);
			this.ModuleInstanceAdded(value.SectionInstanceID, key);
		}

		protected override void OnRemove(SQLiteConnection db, int key)
		{
		}
	}
}
