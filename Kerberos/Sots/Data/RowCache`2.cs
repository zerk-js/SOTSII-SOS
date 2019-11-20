// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.RowCache`2
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.SQLite;
using Kerberos.Sots.Engine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Kerberos.Sots.Data
{
	internal abstract class RowCache<TRowKey, TRowObject> : IEnumerable<KeyValuePair<TRowKey, TRowObject>>, IEnumerable
	  where TRowKey : struct
	  where TRowObject : new()
	{
		private readonly Dictionary<TRowKey, TRowObject> items = new Dictionary<TRowKey, TRowObject>();
		private readonly HashSet<TRowKey> staleItems = new HashSet<TRowKey>();
		private bool syncfromdb = true;
		private bool syncall = true;
		private readonly SQLiteConnection db;

		protected AssetDatabase Assets { get; private set; }

		protected static void Trace(string message)
		{
			App.Log.Trace(message, "data", Kerberos.Sots.Engine.LogLevel.Verbose);
		}

		protected static void Warn(string message)
		{
			App.Log.Warn(message, "data");
		}

		protected void SynchronizeWithDatabase()
		{
			if (!this.syncfromdb)
				return;
			if (this.syncall)
			{
				if (ScriptHost.AllowConsole)
					RowCache<TRowKey, TRowObject>.Trace(string.Format("{0}: Synchronizing all objects", (object)this.GetType().Name));
				foreach (KeyValuePair<TRowKey, TRowObject> keyValuePair in this.OnSynchronizeWithDatabase(this.db, (IEnumerable<TRowKey>)null))
					this.items.Add(keyValuePair.Key, keyValuePair.Value);
			}
			else
			{
				List<TRowKey> rowKeyList = (List<TRowKey>)null;
				if (ScriptHost.AllowConsole)
					rowKeyList = new List<TRowKey>();
				foreach (KeyValuePair<TRowKey, TRowObject> keyValuePair in this.OnSynchronizeWithDatabase(this.db, (IEnumerable<TRowKey>)this.staleItems))
				{
					this.items[keyValuePair.Key] = keyValuePair.Value;
					rowKeyList?.Add(keyValuePair.Key);
				}
				if (ScriptHost.AllowConsole)
				{
					foreach (TRowKey rowKey in rowKeyList)
						RowCache<TRowKey, TRowObject>.Trace(string.Format("{0}: Synchronized object for key {1}", (object)this.GetType().Name, (object)rowKey));
				}
			}
			this.staleItems.Clear();
			this.syncall = false;
			this.syncfromdb = false;
		}

		public RowCache(SQLiteConnection db, AssetDatabase assets)
		{
			this.db = db;
			this.Assets = assets;
		}

		public TRowObject this[TRowKey key]
		{
			get
			{
				this.SynchronizeWithDatabase();
				return this.items[key];
			}
		}

		public IEnumerable<TRowKey> Keys
		{
			get
			{
				this.SynchronizeWithDatabase();
				return (IEnumerable<TRowKey>)this.items.Keys;
			}
		}

		public IEnumerable<TRowObject> Values
		{
			get
			{
				this.SynchronizeWithDatabase();
				return (IEnumerable<TRowObject>)this.items.Values;
			}
		}

		public bool ContainsKey(TRowKey key)
		{
			this.SynchronizeWithDatabase();
			return this.items.ContainsKey(key);
		}

		public IEnumerator<KeyValuePair<TRowKey, TRowObject>> GetEnumerator()
		{
			this.SynchronizeWithDatabase();
			return (IEnumerator<KeyValuePair<TRowKey, TRowObject>>)this.items.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			this.SynchronizeWithDatabase();
			return ((IEnumerable)this.items).GetEnumerator();
		}

		public event RowObjectDirtiedEventHandler<TRowKey> RowObjectDirtied;

		public event RowObjectDirtiedEventHandler<TRowKey> RowObjectRemoving;

		protected virtual void OnCleared()
		{
		}

		public void Clear()
		{
			this.items.Clear();
			this.syncfromdb = true;
			this.syncall = true;
			this.OnCleared();
		}

		public TRowKey Insert(TRowKey? key, TRowObject value)
		{
			this.SynchronizeWithDatabase();
			TRowKey key1 = this.OnInsert(this.db, key, value);
			this.items[key1] = value;
			this.Sync(key1);
			return key1;
		}

		private void InvokeRowObjectDirtied(TRowKey key)
		{
			if (this.RowObjectDirtied == null)
				return;
			this.RowObjectDirtied((object)this, key);
		}

		public void Update(TRowKey key, TRowObject value)
		{
			this.SynchronizeWithDatabase();
			if (!this.items.ContainsKey(key))
				throw new ArgumentOutOfRangeException(nameof(key), "Cannot update. No such row key exists: " + key.ToString());
			this.OnUpdate(this.db, key, value);
			this.Sync(key);
		}

		private void InvokeRowObjectRemoving(TRowKey key)
		{
			if (this.RowObjectRemoving == null)
				return;
			this.RowObjectRemoving((object)this, key);
		}

		public void Remove(TRowKey key)
		{
			this.SynchronizeWithDatabase();
			if (!this.items.ContainsKey(key))
				return;
			this.OnRemove(this.db, key);
			this.InvokeRowObjectRemoving(key);
			this.items.Remove(key);
		}

		public void Sync(TRowKey key)
		{
			this.staleItems.Add(key);
			this.syncfromdb = true;
			this.InvokeRowObjectDirtied(key);
		}

		public void SyncRange(IEnumerable<TRowKey> keys)
		{
			foreach (TRowKey key in keys)
				this.Sync(key);
		}

		protected abstract IEnumerable<KeyValuePair<TRowKey, TRowObject>> OnSynchronizeWithDatabase(
		  SQLiteConnection db,
		  IEnumerable<TRowKey> range);

		protected abstract TRowKey OnInsert(SQLiteConnection db, TRowKey? key, TRowObject value);

		protected abstract void OnUpdate(SQLiteConnection db, TRowKey key, TRowObject value);

		protected abstract void OnRemove(SQLiteConnection db, TRowKey key);
	}
}
