// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Orbit
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using System;

namespace Kerberos.Sots
{
	internal class Orbit
	{
		private float _position;

		public Kerberos.Sots.Data.StarMapFramework.Orbit Parent { get; set; }

		public float SemiMajorAxis { get; set; }

		public float SemiMinorAxis { get; set; }

		public float Inclination { get; set; }

		public int OrbitNumber { get; set; }

		public float Position
		{
			get
			{
				return this._position;
			}
			set
			{
				this._position = Math.Max(0.0f, Math.Min(1f, value));
			}
		}

		public static Matrix CalcTransform(Orbit orbit)
		{
			return new OrbitalPath()
			{
				Scale = {
		  X = orbit.SemiMajorAxis,
		  Y = orbit.SemiMinorAxis
		},
				InitialAngle = ((float)((double)orbit.Position * 3.14159274101257 * 2.0)),
				DeltaAngle = 0.2617994f
			}.GetTransform(0.0);
		}

		private static float CalcOrbitDistance(float orbitStep, int orbitNumber)
		{
			return (float)((double)orbitNumber * (double)orbitStep + (double)orbitNumber * 0.100000001490116 * (double)orbitStep);
		}

		public static float CalcOrbitRadius(float parentRadius, float orbitStep, int orbitNumber)
		{
			return parentRadius + Orbit.CalcOrbitDistance(orbitStep, orbitNumber);
		}
	}
}
