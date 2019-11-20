// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Engine.ScriptMessageIO
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;
using System.IO;
using System.Text;

namespace Kerberos.Sots.Engine
{
	public abstract class ScriptMessageIO : IDisposable
	{
		private readonly UnicodeEncoding _encoding = new UnicodeEncoding();
		private readonly MemoryStream _data;

		protected MemoryStream Stream
		{
			get
			{
				return this._data;
			}
		}

		public ScriptMessageIO(MemoryStream stream)
		{
			this._data = stream ?? new MemoryStream(4096);
		}

		public virtual void Dispose()
		{
			this._data.Dispose();
		}

		public virtual long GetSize()
		{
			return this._data.Length;
		}

		public virtual void SetSize(long size)
		{
			this._data.SetLength(size);
		}

		public virtual byte[] GetBuffer()
		{
			return this._data.GetBuffer();
		}

		public void Clear()
		{
			this._data.SetLength(0L);
		}
	}
}
