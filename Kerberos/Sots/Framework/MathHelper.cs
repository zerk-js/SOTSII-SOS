// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Framework.MathHelper
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;

namespace Kerberos.Sots.Framework
{
	public static class MathHelper
	{
		public const double TwoPi = 6.28318530717959;
		public const double PiOverTwo = 1.5707963267949;
		public const double PiOverFour = 0.785398163397448;

		public static float DegreesToRadians(float degrees)
		{
			return (float)((double)degrees * Math.PI / 180.0);
		}

		public static float RadiansToDegrees(float radians)
		{
			return (float)((double)radians * 180.0 / Math.PI);
		}

		public static Vector3 DegreesToRadians(Vector3 degrees)
		{
			return new Vector3(MathHelper.DegreesToRadians(degrees.X), MathHelper.DegreesToRadians(degrees.Y), MathHelper.DegreesToRadians(degrees.Z));
		}

		public static Vector3 RadiansToDegrees(Vector3 radians)
		{
			return new Vector3(MathHelper.RadiansToDegrees(radians.X), MathHelper.RadiansToDegrees(radians.Y), MathHelper.RadiansToDegrees(radians.Z));
		}

		public static double Square(double value)
		{
			return value * value;
		}

		public static float Square(float value)
		{
			return value * value;
		}
	}
}
