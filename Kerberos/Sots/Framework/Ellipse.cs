// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Framework.Ellipse
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;
using System.Runtime.InteropServices;

namespace Kerberos.Sots.Framework
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct Ellipse
	{
		public static float CalcEccentricity(float semiMajorAxis, float semiMinorAxis)
		{
			return (float)Math.Sqrt((double)semiMajorAxis * (double)semiMajorAxis - (double)semiMinorAxis * (double)semiMinorAxis) / semiMajorAxis;
		}

		public static Vector2 CalcPoint(float semiMajorAxis, float semiMinorAxis, float theta)
		{
			return new Vector2(semiMajorAxis * (float)Math.Cos((double)theta), semiMinorAxis * (float)Math.Sin((double)theta));
		}

		public static float CalcSemiMinorAxis(float semiMajorAxis, float eccentricity)
		{
			if ((double)eccentricity < 0.0 || (double)eccentricity >= 1.0)
				throw new ArgumentOutOfRangeException(nameof(eccentricity), "Value must be in the range 0..1.");
			return semiMajorAxis * (float)Math.Sqrt(1.0 - (double)eccentricity * (double)eccentricity);
		}
	}
}
