// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Engine.NonClosingStreamWrapper
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;
using System.IO;
using System.Runtime.Remoting;

namespace Kerberos.Sots.Engine
{
	public sealed class NonClosingStreamWrapper : Stream
	{
		private Stream stream;
		private bool closed;

		public NonClosingStreamWrapper(Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));
			this.stream = stream;
		}

		public Stream BaseStream
		{
			get
			{
				return this.stream;
			}
		}

		private void CheckClosed()
		{
			if (this.closed)
				throw new InvalidOperationException("Wrapper has been closed or disposed");
		}

		public void CloseContainer()
		{
			base.Close();
			this.stream.Close();
		}

		public override IAsyncResult BeginRead(
		  byte[] buffer,
		  int offset,
		  int count,
		  AsyncCallback callback,
		  object state)
		{
			this.CheckClosed();
			return this.stream.BeginRead(buffer, offset, count, callback, state);
		}

		public override IAsyncResult BeginWrite(
		  byte[] buffer,
		  int offset,
		  int count,
		  AsyncCallback callback,
		  object state)
		{
			this.CheckClosed();
			return this.stream.BeginWrite(buffer, offset, count, callback, state);
		}

		public override bool CanRead
		{
			get
			{
				if (!this.closed)
					return this.stream.CanRead;
				return false;
			}
		}

		public override bool CanSeek
		{
			get
			{
				if (!this.closed)
					return this.stream.CanSeek;
				return false;
			}
		}

		public override bool CanWrite
		{
			get
			{
				if (!this.closed)
					return this.stream.CanWrite;
				return false;
			}
		}

		public override void Close()
		{
			if (!this.closed)
				this.stream.Flush();
			this.stream.Position = 0L;
		}

		public override ObjRef CreateObjRef(Type requestedType)
		{
			throw new NotSupportedException();
		}

		public override int EndRead(IAsyncResult asyncResult)
		{
			this.CheckClosed();
			return this.stream.EndRead(asyncResult);
		}

		public override void EndWrite(IAsyncResult asyncResult)
		{
			this.CheckClosed();
			this.stream.EndWrite(asyncResult);
		}

		public override void Flush()
		{
			this.CheckClosed();
			this.stream.Flush();
		}

		public override object InitializeLifetimeService()
		{
			throw new NotSupportedException();
		}

		public override long Length
		{
			get
			{
				this.CheckClosed();
				return this.stream.Length;
			}
		}

		public override long Position
		{
			get
			{
				this.CheckClosed();
				return this.stream.Position;
			}
			set
			{
				this.CheckClosed();
				this.stream.Position = value;
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			this.CheckClosed();
			return this.stream.Read(buffer, offset, count);
		}

		public override int ReadByte()
		{
			this.CheckClosed();
			return this.stream.ReadByte();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			this.CheckClosed();
			return this.stream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			this.CheckClosed();
			this.stream.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			this.CheckClosed();
			this.stream.Write(buffer, offset, count);
		}

		public override void WriteByte(byte value)
		{
			this.CheckClosed();
			this.stream.WriteByte(value);
		}
	}
}
