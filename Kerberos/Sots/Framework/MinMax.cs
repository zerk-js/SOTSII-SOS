// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Framework.MinMax
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.Framework
{
	public struct MinMax
	{
		public float Min;
		public float Max;

		public static MinMax Parse(string value)
		{
			string[] strArray = value.Split(',');
			return new MinMax()
			{
				Min = float.Parse(strArray[0]),
				Max = float.Parse(strArray[1])
			};
		}
	}
}
