// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Framework.Size
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.Framework
{
	internal struct Size
	{
		public int X;
		public int Y;

		public static Size Parse(string value)
		{
			string[] strArray = value.Split(',');
			return new Size()
			{
				X = int.Parse(strArray[0]),
				Y = int.Parse(strArray[1])
			};
		}
	}
}
