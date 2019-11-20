// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.TableCache`2
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using System;
using System.Collections.Generic;

namespace Kerberos.Sots.Data
{
	internal class TableCache<TRowKey, TRowObject> : Dictionary<TRowKey, TRowObject> where TRowObject : new()
	{
		public bool IsDirty { get; set; }

		public TableCache()
		{
			this.IsDirty = true;
		}

		public new void Clear()
		{
			if (ScriptHost.AllowConsole && !this.IsDirty)
				App.Log.Trace(string.Format("{0}<{1},{2}> cleared.", (object)this.GetType().Name, (object)typeof(TRowKey).Name, (object)typeof(TRowObject).Name), "data", Kerberos.Sots.Engine.LogLevel.Verbose);
			base.Clear();
			this.IsDirty = true;
		}

		public TRowObject Find(TRowKey primaryKey)
		{
			TRowObject rowObject;
			this.TryGetValue(primaryKey, out rowObject);
			return rowObject;
		}

		public void Cache(TRowKey primaryKey, TRowObject rowObject)
		{
			if ((object)rowObject == null)
				throw new ArgumentNullException(nameof(rowObject));
			this[primaryKey] = rowObject;
		}
	}
}
