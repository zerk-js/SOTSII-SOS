// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Framework.GrabBag`1
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Framework
{
	internal class GrabBag<T> : IEnumerable<GrabBagItem<T>>, IEnumerable
	{
		private readonly Random _rand;
		private readonly List<GrabBagItem<T>> _items;

		public GrabBag(Random random, IEnumerable<T> items)
		{
			if (random == null)
				throw new ArgumentNullException(nameof(random));
			this._rand = random;
			this._items = new List<GrabBagItem<T>>(items.Select<T, GrabBagItem<T>>((Func<T, GrabBagItem<T>>)(x => new GrabBagItem<T>()
			{
				IsTaken = false,
				Value = x
			})));
		}

		public void Reset()
		{
			for (int index = 0; index < this._items.Count; ++index)
			{
				GrabBagItem<T> grabBagItem = this._items[index];
				grabBagItem.IsTaken = false;
				this._items[index] = grabBagItem;
			}
		}

		public bool Replace(T item)
		{
			int index = this._items.FindIndex((Predicate<GrabBagItem<T>>)(x => EqualityComparer<T>.Default.Equals(x.Value, item)));
			if (index == -1 || !this._items[index].IsTaken)
				return false;
			GrabBagItem<T> grabBagItem = this._items[index];
			grabBagItem.IsTaken = false;
			this._items[index] = grabBagItem;
			return true;
		}

		public bool IsTaken(T item)
		{
			int index = this._items.FindIndex((Predicate<GrabBagItem<T>>)(x => EqualityComparer<T>.Default.Equals(x.Value, item)));
			if (index == -1)
				return false;
			return this._items[index].IsTaken;
		}

		public bool Take(T item)
		{
			int index = this._items.FindIndex((Predicate<GrabBagItem<T>>)(x => EqualityComparer<T>.Default.Equals(x.Value, item)));
			if (index == -1 || this._items[index].IsTaken)
				return false;
			GrabBagItem<T> grabBagItem = this._items[index];
			grabBagItem.IsTaken = true;
			this._items[index] = grabBagItem;
			return true;
		}

		public T TakeRandom()
		{
			if (this._items.Count == 0)
				throw new InvalidOperationException("No items to take.");
			int num = this._rand.Next(this._items.Count);
			for (int index1 = 0; index1 < this._items.Count; ++index1)
			{
				int index2 = (index1 + num) % this._items.Count;
				if (!this._items[index2].IsTaken)
				{
					GrabBagItem<T> grabBagItem = this._items[index2];
					grabBagItem.IsTaken = true;
					this._items[index2] = grabBagItem;
					return this._items[index2].Value;
				}
			}
			throw new InvalidOperationException("All items are taken.");
		}

		public IEnumerator<GrabBagItem<T>> GetEnumerator()
		{
			return (IEnumerator<GrabBagItem<T>>)this._items.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator)this._items.GetEnumerator();
		}

		public IEnumerable<T> GetAvailableItems()
		{
			return this._items.Where<GrabBagItem<T>>((Func<GrabBagItem<T>, bool>)(x => !x.IsTaken)).Select<GrabBagItem<T>, T>((Func<GrabBagItem<T>, T>)(y => y.Value));
		}

		public IEnumerable<T> GetTakenItems()
		{
			return this._items.Where<GrabBagItem<T>>((Func<GrabBagItem<T>, bool>)(x => x.IsTaken)).Select<GrabBagItem<T>, T>((Func<GrabBagItem<T>, T>)(y => y.Value));
		}
	}
}
