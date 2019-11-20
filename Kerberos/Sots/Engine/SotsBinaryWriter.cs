// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Engine.SotsBinaryWriter
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System.IO;
using System.Text;

namespace Kerberos.Sots.Engine
{
	internal sealed class SotsBinaryWriter : BinaryWriter
	{
		public SotsBinaryWriter(Stream input, Encoding encoding)
		  : base(input, encoding)
		{
		}

		public new void Write7BitEncodedInt(int value)
		{
			base.Write7BitEncodedInt(value);
		}
	}
}
