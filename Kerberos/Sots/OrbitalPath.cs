// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.OrbitalPath
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using System;

namespace Kerberos.Sots
{
	internal struct OrbitalPath
	{
		public static readonly OrbitalPath Zero = new OrbitalPath()
		{
			Scale = Vector2.Zero,
			Rotation = Vector3.Zero,
			InitialAngle = 0.0f,
			DeltaAngle = 0.0f
		};
		public Vector2 Scale;
		public Vector3 Rotation;
		public float InitialAngle;
		public float DeltaAngle;

		public Matrix GetTransform(double t)
		{
			this.DeltaAngle = 0.2f;
			float num = (float)(((double)this.InitialAngle + t * (double)this.DeltaAngle) % 6.28318548202515);
			return Matrix.CreateRotationYPR(this.Rotation.X, this.Rotation.Y, this.Rotation.Z) * Matrix.CreateTranslation(new Vector3((float)Math.Sin(-(double)num) * this.Scale.X, 0.0f, (float)-Math.Cos(-(double)num) * this.Scale.Y));
		}

		public static OrbitalPath Parse(string fromdb)
		{
			string[] strArray = fromdb.Split(',');
			return new OrbitalPath()
			{
				Scale = new Vector2(float.Parse(strArray[0]), float.Parse(strArray[1])),
				Rotation = new Vector3(float.Parse(strArray[2]), float.Parse(strArray[3]), float.Parse(strArray[4])),
				InitialAngle = float.Parse(strArray[5]),
				DeltaAngle = float.Parse(strArray[6])
			};
		}

		public void VerifyFinite()
		{
			this.Scale.X.VerifyFinite();
			this.Scale.Y.VerifyFinite();
			this.Rotation.X.VerifyFinite();
			this.Rotation.Y.VerifyFinite();
			this.Rotation.Z.VerifyFinite();
			this.InitialAngle.VerifyFinite();
			this.DeltaAngle.VerifyFinite();
		}

		public override string ToString()
		{
			return string.Format("{0},{1},{2},{3},{4},{5},{6}", (object)this.Scale.X, (object)this.Scale.Y, (object)this.Rotation.X, (object)this.Rotation.Y, (object)this.Rotation.Z, (object)this.InitialAngle, (object)this.DeltaAngle);
		}
	}
}
