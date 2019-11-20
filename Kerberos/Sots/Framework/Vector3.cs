// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Framework.Vector3
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;

namespace Kerberos.Sots.Framework
{
	public struct Vector3 : IEquatable<Vector3>
	{
		public static readonly Vector3 Zero = new Vector3(0.0f, 0.0f, 0.0f);
		public static readonly Vector3 One = new Vector3(1f, 1f, 1f);
		public static readonly Vector3 UnitX = new Vector3(1f, 0.0f, 0.0f);
		public static readonly Vector3 UnitY = new Vector3(0.0f, 1f, 0.0f);
		public static readonly Vector3 UnitZ = new Vector3(0.0f, 0.0f, 1f);
		public float X;
		public float Y;
		public float Z;

		public Vector3(float x, float y, float z)
		{
			this.X = x;
			this.Y = y;
			this.Z = z;
		}

		public Vector3(Vector3 value)
		{
			this.X = value.X;
			this.Y = value.Y;
			this.Z = value.Z;
		}

		public Vector3(float value)
		{
			this.X = this.Y = this.Z = value;
		}

		public float Length
		{
			get
			{
				return (float)Math.Sqrt((double)this.X * (double)this.X + (double)this.Y * (double)this.Y + (double)this.Z * (double)this.Z);
			}
		}

		public float LengthSquared
		{
			get
			{
				return (float)((double)this.X * (double)this.X + (double)this.Y * (double)this.Y + (double)this.Z * (double)this.Z);
			}
		}

		public static Vector3 Normalize(Vector3 value)
		{
			return value / value.Length;
		}

		public static Vector3 Lerp(Vector3 v0, Vector3 v1, float t)
		{
			return new Vector3(v0.X + (v1.X - v0.X) * t, v0.Y + (v1.Y - v0.Y) * t, v0.Z + (v1.Z - v0.Z) * t);
		}

		public static bool operator ==(Vector3 valueA, Vector3 valueB)
		{
			return valueA.Equals(valueB);
		}

		public static bool operator !=(Vector3 valueA, Vector3 valueB)
		{
			return !valueA.Equals(valueB);
		}

		public static Vector3 operator *(Vector3 v, float s)
		{
			return new Vector3(s * v.X, s * v.Y, s * v.Z);
		}

		public static Vector3 operator /(Vector3 v, float s)
		{
			return new Vector3(v.X / s, v.Y / s, v.Z / s);
		}

		public static Vector3 operator +(Vector3 v0, Vector3 v1)
		{
			return new Vector3(v0.X + v1.X, v0.Y + v1.Y, v0.Z + v1.Z);
		}

		public static Vector3 operator -(Vector3 v0, Vector3 v1)
		{
			return new Vector3(v0.X - v1.X, v0.Y - v1.Y, v0.Z - v1.Z);
		}

		public static Vector3 operator -(Vector3 v0)
		{
			return new Vector3(-v0.X, -v0.Y, -v0.Z);
		}

		public static float Dot(Vector3 v0, Vector3 v1)
		{
			return (float)((double)v0.X * (double)v1.X + (double)v0.Y * (double)v1.Y + (double)v0.Z * (double)v1.Z);
		}

		public float Normalize()
		{
			float num = (float)Math.Sqrt((double)this.X * (double)this.X + (double)this.Y * (double)this.Y + (double)this.Z * (double)this.Z);
			this.X /= num;
			this.Y /= num;
			this.Z /= num;
			return num;
		}

		public bool Equals(Vector3 other)
		{
			if ((double)this.X == (double)other.X && (double)this.Y == (double)other.Y)
				return (double)this.Z == (double)other.Z;
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is Vector3))
				return false;
			return this.Equals((Vector3)obj);
		}

		public override int GetHashCode()
		{
			return this.X.GetHashCode() ^ this.Y.GetHashCode() ^ this.Z.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format("{0}, {1}, {2}", (object)this.X, (object)this.Y, (object)this.Z);
		}

		public static Vector3 Parse(string value)
		{
			return Vector3.Parse(value, ',');
		}

		public static bool TryParse(string value, out Vector3 v)
		{
			return Vector3.TryParse(value, out v, ',');
		}

		public static bool TryParse(string value, out Vector3 v, params char[] separator)
		{
			bool flag = true;
			v = new Vector3();
			string[] strArray = new string[0];
			if (string.IsNullOrEmpty(value))
				flag = false;
			else
				strArray = value.Split(separator);
			if (flag && strArray.Length != 3)
				flag = false;
			if (flag && !float.TryParse(strArray[0], out v.X))
				flag = false;
			if (flag && !float.TryParse(strArray[1], out v.Y))
				flag = false;
			if (flag && !float.TryParse(strArray[2], out v.Z))
				flag = false;
			if (!flag)
				v = new Vector3();
			return flag;
		}

		public static Vector3 Parse(string value, params char[] separator)
		{
			string[] strArray = value.Split(separator);
			return new Vector3(float.Parse(strArray[0]), float.Parse(strArray[1]), float.Parse(strArray[2]));
		}

		public static Vector3 RadiansToDegrees(Vector3 value)
		{
			return new Vector3(MathHelper.RadiansToDegrees(value.X), MathHelper.RadiansToDegrees(value.Y), MathHelper.RadiansToDegrees(value.Z));
		}

		public static Vector3 DegreesToRadians(Vector3 value)
		{
			return new Vector3(MathHelper.DegreesToRadians(value.X), MathHelper.DegreesToRadians(value.Y), MathHelper.DegreesToRadians(value.Z));
		}

		public static Vector3 Cross(Vector3 vector1, Vector3 vector2)
		{
			Vector3 vector3;
			vector3.X = (float)((double)vector1.Y * (double)vector2.Z - (double)vector1.Z * (double)vector2.Y);
			vector3.Y = (float)((double)vector1.Z * (double)vector2.X - (double)vector1.X * (double)vector2.Z);
			vector3.Z = (float)((double)vector1.X * (double)vector2.Y - (double)vector1.Y * (double)vector2.X);
			return vector3;
		}

		public static Vector3 Transform(Vector3 vec, Matrix mat)
		{
			return new Vector3()
			{
				X = (float)((double)vec.X * (double)mat.M11 + (double)vec.Y * (double)mat.M21 + (double)vec.Z * (double)mat.M31) + mat.M41,
				Y = (float)((double)vec.X * (double)mat.M12 + (double)vec.Y * (double)mat.M22 + (double)vec.Z * (double)mat.M32) + mat.M42,
				Z = (float)((double)vec.X * (double)mat.M13 + (double)vec.Y * (double)mat.M23 + (double)vec.Z * (double)mat.M33) + mat.M43
			};
		}
	}
}
