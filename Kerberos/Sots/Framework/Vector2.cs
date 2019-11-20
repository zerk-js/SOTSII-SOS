// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Framework.Vector2
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;

namespace Kerberos.Sots.Framework
{
	public struct Vector2 : IEquatable<Vector2>
	{
		public static readonly Vector2 Zero = new Vector2(0.0f, 0.0f);
		public static readonly Vector2 One = new Vector2(1f, 1f);
		public float X;
		public float Y;

		public Vector2(float x, float y)
		{
			this.X = x;
			this.Y = y;
		}

		public Vector2(Vector2 value)
		{
			this.X = value.X;
			this.Y = value.Y;
		}

		public Vector2(float value)
		{
			this.X = this.Y = value;
		}

		public float Length
		{
			get
			{
				return (float)Math.Sqrt((double)this.X * (double)this.X + (double)this.Y * (double)this.Y);
			}
		}

		public float LengthSq
		{
			get
			{
				return (float)((double)this.X * (double)this.X + (double)this.Y * (double)this.Y);
			}
		}

		public static Vector2 Lerp(Vector2 v0, Vector2 v1, float t)
		{
			return new Vector2(v0.X + (v1.X - v0.X) * t, v0.Y + (v1.Y - v0.Y) * t);
		}

		public static bool operator ==(Vector2 valueA, Vector2 valueB)
		{
			return valueA.Equals(valueB);
		}

		public static bool operator !=(Vector2 valueA, Vector2 valueB)
		{
			return !valueA.Equals(valueB);
		}

		public static Vector2 operator *(Vector2 v, float s)
		{
			return new Vector2(s * v.X, s * v.Y);
		}

		public bool Equals(Vector2 other)
		{
			if ((double)this.X == (double)other.X)
				return (double)this.Y == (double)other.Y;
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is Vector2))
				return false;
			return this.Equals((Vector2)obj);
		}

		public override int GetHashCode()
		{
			return this.X.GetHashCode() ^ this.Y.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format("{0}, {1}", (object)this.X, (object)this.Y);
		}

		public static Vector2 Parse(string value)
		{
			return Vector2.Parse(value, ',');
		}

		public static Vector2 Parse(string value, params char[] separator)
		{
			string[] strArray = value.Split(separator);
			return new Vector2(float.Parse(strArray[0]), float.Parse(strArray[1]));
		}
	}
}
