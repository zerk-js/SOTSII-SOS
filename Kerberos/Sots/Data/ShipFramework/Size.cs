// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ShipFramework.Size
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;

namespace Kerberos.Sots.Data.ShipFramework
{
	public class Size
	{
		public int X;
		public int Y;

		public static implicit operator Size(string rhs)
		{
			Size size = new Size();
			Size.TryParse(rhs, out size);
			return size;
		}

		public override string ToString()
		{
			return this.X.ToString() + "," + this.Y.ToString();
		}

		public static bool TryParse(string s, out Size value)
		{
			try
			{
				string[] strArray = s.Split(',');
				value = new Size()
				{
					X = int.Parse(strArray[0]),
					Y = int.Parse(strArray[1])
				};
				return true;
			}
			catch (Exception ex)
			{
				value = new Size();
				return false;
			}
		}
	}
}
