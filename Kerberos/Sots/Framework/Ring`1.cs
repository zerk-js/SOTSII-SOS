// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Framework.Ring`1
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;
using System.Collections;
using System.Collections.Generic;

namespace Kerberos.Sots.Framework
{
	internal class Ring<T> : IEnumerable<T>, IEnumerable where T : class
	{
		private IList<T> _data;
		private int _current;

		public Ring()
		{
			this._data = (IList<T>)new List<T>();
			this._current = 0;
		}

		public int Count
		{
			get
			{
				return this._data.Count;
			}
		}

		public T this[int i]
		{
			get
			{
				return this._data[i];
			}
		}

		public void Add(T t)
		{
			this._data.Add(t);
		}

		public T Current
		{
			get
			{
				return this.GetCurrent();
			}
			set
			{
				this.SetCurrent(value);
			}
		}

		public T GetCurrent()
		{
			T obj = default(T);
			if (this._data != null && this._current >= 0 && this._current < this._data.Count)
				obj = this._data[this._current];
			return obj;
		}

		public void SetCurrent(T t)
		{
			int num = this.IndexOf(t);
			if (num < 0)
				throw new ArgumentOutOfRangeException();
			this._current = num;
		}

		public int IndexOf(T t)
		{
			if (this._data != null)
			{
				for (int index = 0; index < this._data.Count; ++index)
				{
					if ((object)this._data[index] == (object)t)
						return index;
				}
			}
			return -1;
		}

		public T Next()
		{
			T obj = default(T);
			if (this._data != null)
			{
				++this._current;
				if (this._current < 0 || this._current >= this._data.Count)
					this._current = 0;
				obj = this.GetCurrent();
			}
			return obj;
		}

		public T Prev()
		{
			T obj = default(T);
			if (this._data != null)
			{
				--this._current;
				if (this._current < 0 || this._current >= this._data.Count)
					this._current = this._data.Count - 1;
				obj = this.GetCurrent();
			}
			return obj;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return this._data.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator)this._data.GetEnumerator();
		}
	}
}
