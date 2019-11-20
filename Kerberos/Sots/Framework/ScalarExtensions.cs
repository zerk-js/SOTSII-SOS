// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Framework.ScalarExtensions
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;

namespace Kerberos.Sots.Framework
{
	public static class ScalarExtensions
	{
		public static bool IsFinite(this Vector3 value)
		{
			if (value.X.IsFinite() && value.Y.IsFinite())
				return value.Z.IsFinite();
			return false;
		}

		public static bool IsFinite(this Matrix value)
		{
			if (value.M11.IsFinite() && value.M12.IsFinite() && (value.M13.IsFinite() && value.M14.IsFinite()) && (value.M21.IsFinite() && value.M22.IsFinite() && (value.M23.IsFinite() && value.M24.IsFinite())) && (value.M31.IsFinite() && value.M32.IsFinite() && (value.M33.IsFinite() && value.M34.IsFinite()) && (value.M41.IsFinite() && value.M42.IsFinite() && value.M43.IsFinite())))
				return value.M44.IsFinite();
			return false;
		}

		public static bool IsFinite(this double value)
		{
			if (!double.IsInfinity(value))
				return !double.IsNaN(value);
			return false;
		}

		public static bool IsFinite(this float value)
		{
			if (!float.IsInfinity(value))
				return !float.IsNaN(value);
			return false;
		}

		public static double Saturate(this double value)
		{
			return Math.Max(0.0, Math.Min(value, 1.0));
		}

		public static double Lerp(double x0, double x1, double t)
		{
			return x0 + t * (x1 - x0);
		}

		public static double SmoothStep(this double t)
		{
			t = t.Saturate();
			return t * t * (3.0 - 2.0 * t);
		}

		public static double SmoothStep(double x0, double x1, double t)
		{
			t = t.SmoothStep();
			return x0 + t * (x1 - x0);
		}

		public static float Saturate(this float value)
		{
			return Math.Max(0.0f, Math.Min(value, 1f));
		}

		public static int Clamp(this int value, int x0, int x1)
		{
			return Math.Max(x0, Math.Min(value, x1));
		}

		public static float Clamp(this float value, float x0, float x1)
		{
			return Math.Max(x0, Math.Min(value, x1));
		}

		public static double Clamp(this double value, double x0, double x1)
		{
			return Math.Max(x0, Math.Min(value, x1));
		}

		public static float Lerp(float x0, float x1, float t)
		{
			return x0 + t * (x1 - x0);
		}

		public static float SmoothStep(float t)
		{
			t = t.Saturate();
			return (float)((double)t * (double)t * (3.0 - 2.0 * (double)t));
		}

		public static float SmoothStep(float x0, float x1, float t)
		{
			t = ScalarExtensions.SmoothStep(t);
			return x0 + t * (x1 - x0);
		}

		public static void VerifyFinite(this float t)
		{
			if (!t.IsFinite())
				throw new ArgumentException("The given value is not finite.");
		}

		public static void VerifyFinite(this double t)
		{
			if (!t.IsFinite())
				throw new ArgumentException("The given value is not finite.");
		}
	}
}
