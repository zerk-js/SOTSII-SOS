// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Framework.EmptyEnumerable`1
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;
using System.Collections;
using System.Collections.Generic;

namespace Kerberos.Sots.Framework
{
	internal class EmptyEnumerable<T> : IEnumerable<T>, IEnumerable
	{
		public static readonly EmptyEnumerable<T> Default = new EmptyEnumerable<T>();

		public IEnumerator<T> GetEnumerator()
		{
			return (IEnumerator<T>)EmptyEnumerable<T>.EmptyEnumerator.Default;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator)EmptyEnumerable<T>.EmptyEnumerator.Default;
		}

		private class EmptyEnumerator : IEnumerator<T>, IDisposable, IEnumerator
		{
			public static readonly EmptyEnumerable<T>.EmptyEnumerator Default = new EmptyEnumerable<T>.EmptyEnumerator();

			public T Current
			{
				get
				{
					throw new InvalidOperationException("There is never any Current value for an EmptyEnumerator.");
				}
			}

			public void Dispose()
			{
			}

			object IEnumerator.Current
			{
				get
				{
					throw new InvalidOperationException("There is never any Current value for an EmptyEnumerator.");
				}
			}

			public bool MoveNext()
			{
				return false;
			}

			public void Reset()
			{
			}
		}
	}
}
