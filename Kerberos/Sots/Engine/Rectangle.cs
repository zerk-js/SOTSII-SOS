// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Engine.Rectangle
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;
using System.Text;

namespace Kerberos.Sots.Engine
{
	internal struct Rectangle
	{
		public float X;
		public float Y;
		public float W;
		public float H;

		public Rectangle(float x, float y, float w, float h)
		{
			this.X = x;
			this.Y = y;
			this.W = w;
			this.H = h;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("{");
			stringBuilder.Append(this.X);
			stringBuilder.Append(",");
			stringBuilder.Append(this.Y);
			stringBuilder.Append(",");
			stringBuilder.Append(this.W);
			stringBuilder.Append(",");
			stringBuilder.Append(this.H);
			stringBuilder.Append("}");
			return stringBuilder.ToString();
		}

		public bool IsIntersecting(float x, float y)
		{
			if ((double)x > (double)this.X && (double)x < (double)this.X + (double)this.W && (double)y > (double)this.Y)
				return (double)y < (double)this.Y + (double)this.H;
			return false;
		}

		public static bool operator ==(Rectangle value1, Rectangle value2)
		{
			if ((double)value1.X == (double)value2.X && (double)value1.Y == (double)value2.Y && (double)value1.W == (double)value2.W)
				return (double)value1.H == (double)value2.H;
			return false;
		}

		public static bool operator !=(Rectangle value1, Rectangle value2)
		{
			return !(value1 == value2);
		}

		public override bool Equals(object obj)
		{
			try
			{
				return (Rectangle)obj == this;
			}
			catch (InvalidCastException ex)
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
