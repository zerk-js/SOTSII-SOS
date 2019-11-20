// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.SQLite.Row
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System.Collections;
using System.Collections.Generic;

namespace Kerberos.Sots.Data.SQLite
{
	internal class Row : IEnumerable<string>, IEnumerable
	{
		public string[] Values;

		public IEnumerator<string> GetEnumerator()
		{
			return ((IEnumerable<string>)this.Values).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.Values.GetEnumerator();
		}

		public string this[int index]
		{
			get
			{
				return this.Values[index];
			}
			set
			{
				this.Values[index] = value;
			}
		}
	}
}
