// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Engine.ScriptMessageReader
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System.IO;
using System.Text;

namespace Kerberos.Sots.Engine
{
	public sealed class ScriptMessageReader : ScriptMessageIO
	{
		private readonly SotsBinaryReader _reader;
		private bool noDebugging;

		public ScriptMessageReader()
		  : this(false, (MemoryStream)null)
		{
		}

		public ScriptMessageReader(bool noDebugging, MemoryStream stream)
		  : base(stream)
		{
			this.noDebugging = noDebugging;
			this._reader = new SotsBinaryReader((Stream)this.Stream, Encoding.UTF8);
		}

		public override void Dispose()
		{
			base.Dispose();
			this._reader.Dispose();
		}

		public string ReadString()
		{
			return this._reader.ReadString();
		}

		public float ReadSingle()
		{
			return this._reader.ReadSingle();
		}

		public int ReadInteger()
		{
			return this._reader.Read7BitEncodedInt();
		}

		public bool ReadBool()
		{
			return this.ReadInteger() != 0;
		}

		public double ReadDouble()
		{
			return this._reader.ReadDouble();
		}

		public char ReadByte()
		{
			return (char)this._reader.ReadByte();
		}
	}
}
