// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Framework.Quaternion
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;

namespace Kerberos.Sots.Framework
{
	internal class Quaternion
	{
		public static readonly Quaternion Zero = new Quaternion(0.0f, 0.0f, 0.0f, 0.0f);
		public static readonly Quaternion Identity = new Quaternion(0.0f, 0.0f, 0.0f, 1f);
		public float X;
		public float Y;
		public float Z;
		public float W;

		public Quaternion()
		{
		}

		public Quaternion(float x, float y, float z, float w)
		{
			this.X = x;
			this.Y = y;
			this.Z = z;
			this.W = w;
		}

		public static Quaternion CreateFromRotationMatrix(Matrix matrix)
		{
			float num1 = matrix.M11 + matrix.M22 + matrix.M33;
			Quaternion quaternion = new Quaternion();
			if ((double)num1 > 0.0)
			{
				float num2 = (float)Math.Sqrt((double)num1 + 1.0);
				quaternion.W = num2 * 0.5f;
				float num3 = 0.5f / num2;
				quaternion.X = (matrix.M23 - matrix.M32) * num3;
				quaternion.Y = (matrix.M31 - matrix.M13) * num3;
				quaternion.Z = (matrix.M12 - matrix.M21) * num3;
				return quaternion;
			}
			if ((double)matrix.M11 >= (double)matrix.M22 && (double)matrix.M11 >= (double)matrix.M33)
			{
				float num2 = (float)Math.Sqrt(1.0 + (double)matrix.M11 - (double)matrix.M22 - (double)matrix.M33);
				float num3 = 0.5f / num2;
				quaternion.X = 0.5f * num2;
				quaternion.Y = (matrix.M12 + matrix.M21) * num3;
				quaternion.Z = (matrix.M13 + matrix.M31) * num3;
				quaternion.W = (matrix.M23 - matrix.M32) * num3;
				return quaternion;
			}
			if ((double)matrix.M22 > (double)matrix.M33)
			{
				float num2 = (float)Math.Sqrt(1.0 + (double)matrix.M22 - (double)matrix.M11 - (double)matrix.M33);
				float num3 = 0.5f / num2;
				quaternion.X = (matrix.M21 + matrix.M12) * num3;
				quaternion.Y = 0.5f * num2;
				quaternion.Z = (matrix.M32 + matrix.M23) * num3;
				quaternion.W = (matrix.M31 - matrix.M13) * num3;
				return quaternion;
			}
			float num4 = (float)Math.Sqrt(1.0 + (double)matrix.M33 - (double)matrix.M11 - (double)matrix.M22);
			float num5 = 0.5f / num4;
			quaternion.X = (matrix.M31 + matrix.M13) * num5;
			quaternion.Y = (matrix.M32 + matrix.M23) * num5;
			quaternion.Z = 0.5f * num4;
			quaternion.W = (matrix.M12 - matrix.M21) * num5;
			return quaternion;
		}
	}
}
