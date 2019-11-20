// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.SQLite.Table
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System.Collections;
using System.Collections.Generic;

namespace Kerberos.Sots.Data.SQLite
{
	internal class Table : IEnumerable<Row>, IEnumerable
	{
		public Row[] Rows;

		public IEnumerator<Row> GetEnumerator()
		{
			return ((IEnumerable<Row>)this.Rows).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.Rows.GetEnumerator();
		}

		public Row this[int index]
		{
			get
			{
				return this.Rows[index];
			}
			set
			{
				this.Rows[index] = value;
			}
		}
	}
}
