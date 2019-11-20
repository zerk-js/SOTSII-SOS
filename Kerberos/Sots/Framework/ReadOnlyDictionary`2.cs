// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Framework.ReadOnlyDictionary`2
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System.Collections.Generic;

namespace Kerberos.Sots.Framework
{
	internal class ReadOnlyDictionary<TKey, TValue>
	{
		private readonly IDictionary<TKey, TValue> _dict;

		public IEnumerable<TKey> Keys
		{
			get
			{
				return (IEnumerable<TKey>)this._dict.Keys;
			}
		}

		public IEnumerable<TValue> Values
		{
			get
			{
				return (IEnumerable<TValue>)this._dict.Values;
			}
		}

		public TValue this[TKey key]
		{
			get
			{
				return this._dict[key];
			}
		}

		public bool ContainsKey(TKey key)
		{
			return this._dict.ContainsKey(key);
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			return this._dict.TryGetValue(key, out value);
		}

		public ReadOnlyDictionary(IDictionary<TKey, TValue> dictionary)
		{
			this._dict = dictionary;
		}
	}
}
