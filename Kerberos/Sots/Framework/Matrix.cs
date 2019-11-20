// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Framework.Matrix
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;

namespace Kerberos.Sots.Framework
{
	public struct Matrix
	{
		public static readonly Matrix Identity = new Matrix(1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f);
		public float M11;
		public float M12;
		public float M13;
		public float M14;
		public float M21;
		public float M22;
		public float M23;
		public float M24;
		public float M31;
		public float M32;
		public float M33;
		public float M34;
		public float M41;
		public float M42;
		public float M43;
		public float M44;

		public Matrix(
		  float m11,
		  float m12,
		  float m13,
		  float m14,
		  float m21,
		  float m22,
		  float m23,
		  float m24,
		  float m31,
		  float m32,
		  float m33,
		  float m34,
		  float m41,
		  float m42,
		  float m43,
		  float m44)
		{
			this.M11 = m11;
			this.M12 = m12;
			this.M13 = m13;
			this.M14 = m14;
			this.M21 = m21;
			this.M22 = m22;
			this.M23 = m23;
			this.M24 = m24;
			this.M31 = m31;
			this.M32 = m32;
			this.M33 = m33;
			this.M34 = m34;
			this.M41 = m41;
			this.M42 = m42;
			this.M43 = m43;
			this.M44 = m44;
		}

		public Matrix(Matrix value)
		{
			this.M11 = value.M11;
			this.M12 = value.M12;
			this.M13 = value.M13;
			this.M14 = value.M14;
			this.M21 = value.M21;
			this.M22 = value.M22;
			this.M23 = value.M23;
			this.M24 = value.M24;
			this.M31 = value.M31;
			this.M32 = value.M32;
			this.M33 = value.M33;
			this.M34 = value.M34;
			this.M41 = value.M41;
			this.M42 = value.M42;
			this.M43 = value.M43;
			this.M44 = value.M44;
		}

		public Vector3 Right
		{
			get
			{
				return new Vector3(this.M11, this.M12, this.M13);
			}
		}

		public Vector3 Up
		{
			get
			{
				return new Vector3(this.M21, this.M22, this.M23);
			}
		}

		public Vector3 Forward
		{
			get
			{
				return new Vector3(-this.M31, -this.M32, -this.M33);
			}
		}

		public Vector3 Position
		{
			get
			{
				return new Vector3(this.M41, this.M42, this.M43);
			}
			set
			{
				this.M41 = value.X;
				this.M42 = value.Y;
				this.M43 = value.Z;
			}
		}

		public Vector3 EulerAngles
		{
			get
			{
				float y = (float)Math.Asin((double)this.Forward.Y.Clamp(-1f, 1f));
				float x;
				float z;
				if ((double)Math.Abs(this.Forward.Y) >= 0.9)
				{
					x = (float)Math.Atan2(-(double)this.Right.Z, (double)this.Right.Y);
					z = 0.0f;
				}
				else
				{
					x = (float)Math.Atan2(-(double)this.Forward.X, -(double)this.Forward.Z);
					z = (float)Math.Atan2((double)this.Right.Y, (double)this.Up.Y);
				}
				return new Vector3(x, y, z);
			}
		}

		public static Matrix CreateWorld(Vector3 position, Vector3 forward, Vector3 up)
		{
			Vector3 vector3_1 = Vector3.Normalize(-forward);
			Vector3 vector2 = Vector3.Normalize(Vector3.Cross(up, vector3_1));
			Vector3 vector3_2 = Vector3.Cross(vector3_1, vector2);
			Matrix matrix;
			matrix.M11 = vector2.X;
			matrix.M12 = vector2.Y;
			matrix.M13 = vector2.Z;
			matrix.M14 = 0.0f;
			matrix.M21 = vector3_2.X;
			matrix.M22 = vector3_2.Y;
			matrix.M23 = vector3_2.Z;
			matrix.M24 = 0.0f;
			matrix.M31 = vector3_1.X;
			matrix.M32 = vector3_1.Y;
			matrix.M33 = vector3_1.Z;
			matrix.M34 = 0.0f;
			matrix.M41 = position.X;
			matrix.M42 = position.Y;
			matrix.M43 = position.Z;
			matrix.M44 = 1f;
			return matrix;
		}

		public static Matrix CreateScale(Vector3 scale)
		{
			return new Matrix(scale.X, 0.0f, 0.0f, 0.0f, 0.0f, scale.Y, 0.0f, 0.0f, 0.0f, 0.0f, scale.Z, 0.0f, 0.0f, 0.0f, 0.0f, 1f);
		}

		public static Matrix CreateScale(float x, float y, float z)
		{
			return new Matrix(x, 0.0f, 0.0f, 0.0f, 0.0f, y, 0.0f, 0.0f, 0.0f, 0.0f, z, 0.0f, 0.0f, 0.0f, 0.0f, 1f);
		}

		public static Matrix CreateTranslation(Vector3 trans)
		{
			return new Matrix(1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, trans.X, trans.Y, trans.Z, 1f);
		}

		public static Matrix CreateTranslation(float x, float y, float z)
		{
			return new Matrix(1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, x, y, z, 1f);
		}

		public static Matrix CreateRotationX(float radians)
		{
			float num = (float)Math.Cos((double)radians);
			float m23 = (float)Math.Sin((double)radians);
			return new Matrix(1f, 0.0f, 0.0f, 0.0f, 0.0f, num, m23, 0.0f, 0.0f, -m23, num, 0.0f, 0.0f, 0.0f, 0.0f, 1f);
		}

		public static Matrix CreateRotationY(float radians)
		{
			float num = (float)Math.Cos((double)radians);
			float m31 = (float)Math.Sin((double)radians);
			return new Matrix(num, 0.0f, -m31, 0.0f, 0.0f, 1f, 0.0f, 0.0f, m31, 0.0f, num, 0.0f, 0.0f, 0.0f, 0.0f, 1f);
		}

		public static Matrix CreateRotationZ(float radians)
		{
			float num = (float)Math.Cos((double)radians);
			float m12 = (float)Math.Sin((double)radians);
			return new Matrix(num, m12, 0.0f, 0.0f, -m12, num, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f);
		}

		public static Matrix CreateRotationYPR(Vector3 yawPitchRoll)
		{
			return Matrix.CreateRotationYPR(yawPitchRoll.X, yawPitchRoll.Y, yawPitchRoll.Z);
		}

		public static Matrix CreateRotationYPR(
		  float yawRadians,
		  float pitchRadians,
		  float rollRadians)
		{
			Matrix rotationZ = Matrix.CreateRotationZ(rollRadians);
			Matrix rotationX = Matrix.CreateRotationX(pitchRadians);
			Matrix rotationY = Matrix.CreateRotationY(yawRadians);
			Matrix output;
			Matrix.Multiply(out output, ref rotationZ, ref rotationX);
			Matrix.Multiply(out output, ref output, ref rotationY);
			return output;
		}

		public static Matrix operator *(Matrix lhs, Matrix rhs)
		{
			Matrix output;
			Matrix.Multiply(out output, ref lhs, ref rhs);
			return output;
		}

		public static void Multiply(out Matrix output, ref Matrix lhs, ref Matrix rhs)
		{
			output = new Matrix((float)((double)lhs.M11 * (double)rhs.M11 + (double)lhs.M12 * (double)rhs.M21 + (double)lhs.M13 * (double)rhs.M31 + (double)lhs.M14 * (double)rhs.M41), (float)((double)lhs.M11 * (double)rhs.M12 + (double)lhs.M12 * (double)rhs.M22 + (double)lhs.M13 * (double)rhs.M32 + (double)lhs.M14 * (double)rhs.M42), (float)((double)lhs.M11 * (double)rhs.M13 + (double)lhs.M12 * (double)rhs.M23 + (double)lhs.M13 * (double)rhs.M33 + (double)lhs.M14 * (double)rhs.M43), (float)((double)lhs.M11 * (double)rhs.M14 + (double)lhs.M12 * (double)rhs.M24 + (double)lhs.M13 * (double)rhs.M34 + (double)lhs.M14 * (double)rhs.M44), (float)((double)lhs.M21 * (double)rhs.M11 + (double)lhs.M22 * (double)rhs.M21 + (double)lhs.M23 * (double)rhs.M31 + (double)lhs.M24 * (double)rhs.M41), (float)((double)lhs.M21 * (double)rhs.M12 + (double)lhs.M22 * (double)rhs.M22 + (double)lhs.M23 * (double)rhs.M32 + (double)lhs.M24 * (double)rhs.M42), (float)((double)lhs.M21 * (double)rhs.M13 + (double)lhs.M22 * (double)rhs.M23 + (double)lhs.M23 * (double)rhs.M33 + (double)lhs.M24 * (double)rhs.M43), (float)((double)lhs.M21 * (double)rhs.M14 + (double)lhs.M22 * (double)rhs.M24 + (double)lhs.M23 * (double)rhs.M34 + (double)lhs.M24 * (double)rhs.M44), (float)((double)lhs.M31 * (double)rhs.M11 + (double)lhs.M32 * (double)rhs.M21 + (double)lhs.M33 * (double)rhs.M31 + (double)lhs.M34 * (double)rhs.M41), (float)((double)lhs.M31 * (double)rhs.M12 + (double)lhs.M32 * (double)rhs.M22 + (double)lhs.M33 * (double)rhs.M32 + (double)lhs.M34 * (double)rhs.M42), (float)((double)lhs.M31 * (double)rhs.M13 + (double)lhs.M32 * (double)rhs.M23 + (double)lhs.M33 * (double)rhs.M33 + (double)lhs.M34 * (double)rhs.M43), (float)((double)lhs.M31 * (double)rhs.M14 + (double)lhs.M32 * (double)rhs.M24 + (double)lhs.M33 * (double)rhs.M34 + (double)lhs.M34 * (double)rhs.M44), (float)((double)lhs.M41 * (double)rhs.M11 + (double)lhs.M42 * (double)rhs.M21 + (double)lhs.M43 * (double)rhs.M31 + (double)lhs.M44 * (double)rhs.M41), (float)((double)lhs.M41 * (double)rhs.M12 + (double)lhs.M42 * (double)rhs.M22 + (double)lhs.M43 * (double)rhs.M32 + (double)lhs.M44 * (double)rhs.M42), (float)((double)lhs.M41 * (double)rhs.M13 + (double)lhs.M42 * (double)rhs.M23 + (double)lhs.M43 * (double)rhs.M33 + (double)lhs.M44 * (double)rhs.M43), (float)((double)lhs.M41 * (double)rhs.M14 + (double)lhs.M42 * (double)rhs.M24 + (double)lhs.M43 * (double)rhs.M34 + (double)lhs.M44 * (double)rhs.M44));
		}

		public static float Determinant(Matrix m)
		{
			return (float)((double)m.M11 * (double)m.M22 * (double)m.M33 * (double)m.M44 + (double)m.M11 * (double)m.M23 * (double)m.M34 * (double)m.M42 + (double)m.M11 * (double)m.M24 * (double)m.M32 * (double)m.M43 + (double)m.M12 * (double)m.M21 * (double)m.M34 * (double)m.M43 + (double)m.M12 * (double)m.M23 * (double)m.M31 * (double)m.M44 + (double)m.M12 * (double)m.M24 * (double)m.M33 * (double)m.M41 + (double)m.M13 * (double)m.M21 * (double)m.M32 * (double)m.M44 + (double)m.M13 * (double)m.M22 * (double)m.M34 * (double)m.M41 + (double)m.M13 * (double)m.M24 * (double)m.M31 * (double)m.M42 + (double)m.M14 * (double)m.M21 * (double)m.M33 * (double)m.M42 + (double)m.M14 * (double)m.M22 * (double)m.M31 * (double)m.M43 + (double)m.M14 * (double)m.M23 * (double)m.M32 * (double)m.M41 - (double)m.M11 * (double)m.M22 * (double)m.M34 * (double)m.M43 - (double)m.M11 * (double)m.M23 * (double)m.M32 * (double)m.M44 - (double)m.M11 * (double)m.M24 * (double)m.M33 * (double)m.M42 - (double)m.M12 * (double)m.M21 * (double)m.M33 * (double)m.M44 - (double)m.M12 * (double)m.M23 * (double)m.M34 * (double)m.M41 - (double)m.M12 * (double)m.M24 * (double)m.M31 * (double)m.M43 - (double)m.M13 * (double)m.M21 * (double)m.M34 * (double)m.M42 - (double)m.M13 * (double)m.M22 * (double)m.M31 * (double)m.M44 - (double)m.M13 * (double)m.M24 * (double)m.M32 * (double)m.M41 - (double)m.M14 * (double)m.M21 * (double)m.M32 * (double)m.M43 - (double)m.M14 * (double)m.M22 * (double)m.M33 * (double)m.M41 - (double)m.M14 * (double)m.M23 * (double)m.M31 * (double)m.M42);
		}

		public static Matrix Inverse(Matrix m)
		{
			float num1 = Matrix.Determinant(m);
			if ((double)num1 == 0.0)
				return m;
			float num2 = 1f / num1;
			return new Matrix(num2 * (float)((double)m.M22 * (double)m.M33 * (double)m.M44 + (double)m.M23 * (double)m.M34 * (double)m.M42 + (double)m.M24 * (double)m.M32 * (double)m.M43 - (double)m.M22 * (double)m.M34 * (double)m.M43 - (double)m.M23 * (double)m.M32 * (double)m.M44 - (double)m.M24 * (double)m.M33 * (double)m.M42), num2 * (float)((double)m.M12 * (double)m.M34 * (double)m.M43 + (double)m.M13 * (double)m.M32 * (double)m.M44 + (double)m.M14 * (double)m.M33 * (double)m.M42 - (double)m.M12 * (double)m.M33 * (double)m.M44 - (double)m.M13 * (double)m.M34 * (double)m.M42 - (double)m.M14 * (double)m.M32 * (double)m.M43), num2 * (float)((double)m.M12 * (double)m.M23 * (double)m.M44 + (double)m.M13 * (double)m.M24 * (double)m.M42 + (double)m.M14 * (double)m.M22 * (double)m.M43 - (double)m.M12 * (double)m.M24 * (double)m.M43 - (double)m.M13 * (double)m.M22 * (double)m.M44 - (double)m.M14 * (double)m.M23 * (double)m.M42), num2 * (float)((double)m.M12 * (double)m.M24 * (double)m.M33 + (double)m.M13 * (double)m.M22 * (double)m.M34 + (double)m.M14 * (double)m.M23 * (double)m.M32 - (double)m.M12 * (double)m.M23 * (double)m.M34 - (double)m.M13 * (double)m.M24 * (double)m.M32 - (double)m.M14 * (double)m.M22 * (double)m.M33), num2 * (float)((double)m.M21 * (double)m.M34 * (double)m.M43 + (double)m.M23 * (double)m.M31 * (double)m.M44 + (double)m.M24 * (double)m.M33 * (double)m.M41 - (double)m.M21 * (double)m.M33 * (double)m.M44 - (double)m.M23 * (double)m.M34 * (double)m.M41 - (double)m.M24 * (double)m.M31 * (double)m.M43), num2 * (float)((double)m.M11 * (double)m.M33 * (double)m.M44 + (double)m.M13 * (double)m.M34 * (double)m.M41 + (double)m.M14 * (double)m.M31 * (double)m.M43 - (double)m.M11 * (double)m.M34 * (double)m.M43 - (double)m.M13 * (double)m.M31 * (double)m.M44 - (double)m.M14 * (double)m.M33 * (double)m.M41), num2 * (float)((double)m.M11 * (double)m.M24 * (double)m.M43 + (double)m.M13 * (double)m.M21 * (double)m.M44 + (double)m.M14 * (double)m.M23 * (double)m.M41 - (double)m.M11 * (double)m.M23 * (double)m.M44 - (double)m.M13 * (double)m.M24 * (double)m.M41 - (double)m.M14 * (double)m.M21 * (double)m.M43), num2 * (float)((double)m.M11 * (double)m.M23 * (double)m.M34 + (double)m.M13 * (double)m.M24 * (double)m.M31 + (double)m.M14 * (double)m.M21 * (double)m.M33 - (double)m.M11 * (double)m.M24 * (double)m.M33 - (double)m.M13 * (double)m.M21 * (double)m.M34 - (double)m.M14 * (double)m.M23 * (double)m.M31), num2 * (float)((double)m.M21 * (double)m.M32 * (double)m.M44 + (double)m.M22 * (double)m.M34 * (double)m.M41 + (double)m.M24 * (double)m.M31 * (double)m.M42 - (double)m.M21 * (double)m.M34 * (double)m.M42 - (double)m.M22 * (double)m.M31 * (double)m.M44 - (double)m.M24 * (double)m.M32 * (double)m.M41), num2 * (float)((double)m.M11 * (double)m.M34 * (double)m.M42 + (double)m.M12 * (double)m.M31 * (double)m.M44 + (double)m.M14 * (double)m.M32 * (double)m.M41 - (double)m.M11 * (double)m.M32 * (double)m.M44 - (double)m.M12 * (double)m.M34 * (double)m.M41 - (double)m.M14 * (double)m.M31 * (double)m.M42), num2 * (float)((double)m.M11 * (double)m.M22 * (double)m.M44 + (double)m.M12 * (double)m.M24 * (double)m.M41 + (double)m.M14 * (double)m.M21 * (double)m.M42 - (double)m.M11 * (double)m.M24 * (double)m.M42 - (double)m.M12 * (double)m.M21 * (double)m.M44 - (double)m.M14 * (double)m.M22 * (double)m.M41), num2 * (float)((double)m.M11 * (double)m.M24 * (double)m.M32 + (double)m.M12 * (double)m.M21 * (double)m.M34 + (double)m.M14 * (double)m.M22 * (double)m.M31 - (double)m.M11 * (double)m.M22 * (double)m.M34 - (double)m.M12 * (double)m.M24 * (double)m.M31 - (double)m.M14 * (double)m.M21 * (double)m.M32), num2 * (float)((double)m.M21 * (double)m.M33 * (double)m.M42 + (double)m.M22 * (double)m.M31 * (double)m.M43 + (double)m.M23 * (double)m.M32 * (double)m.M41 - (double)m.M21 * (double)m.M32 * (double)m.M43 - (double)m.M22 * (double)m.M33 * (double)m.M41 - (double)m.M23 * (double)m.M31 * (double)m.M42), num2 * (float)((double)m.M11 * (double)m.M32 * (double)m.M43 + (double)m.M12 * (double)m.M33 * (double)m.M41 + (double)m.M13 * (double)m.M31 * (double)m.M42 - (double)m.M11 * (double)m.M33 * (double)m.M42 - (double)m.M12 * (double)m.M31 * (double)m.M43 - (double)m.M13 * (double)m.M32 * (double)m.M41), num2 * (float)((double)m.M11 * (double)m.M23 * (double)m.M42 + (double)m.M12 * (double)m.M21 * (double)m.M43 + (double)m.M13 * (double)m.M22 * (double)m.M41 - (double)m.M11 * (double)m.M22 * (double)m.M43 - (double)m.M12 * (double)m.M23 * (double)m.M41 - (double)m.M13 * (double)m.M21 * (double)m.M42), num2 * (float)((double)m.M11 * (double)m.M22 * (double)m.M33 + (double)m.M12 * (double)m.M23 * (double)m.M31 + (double)m.M13 * (double)m.M21 * (double)m.M32 - (double)m.M11 * (double)m.M23 * (double)m.M32 - (double)m.M12 * (double)m.M21 * (double)m.M33 - (double)m.M13 * (double)m.M22 * (double)m.M31));
		}

		public static Matrix Parse(string value)
		{
			return Matrix.Parse(value, ',');
		}

		public static Matrix Parse(string value, params char[] separator)
		{
			string[] strArray = value.Split(separator);
			return new Matrix(float.Parse(strArray[0]), float.Parse(strArray[1]), float.Parse(strArray[2]), float.Parse(strArray[3]), float.Parse(strArray[4]), float.Parse(strArray[5]), float.Parse(strArray[6]), float.Parse(strArray[7]), float.Parse(strArray[8]), float.Parse(strArray[9]), float.Parse(strArray[10]), float.Parse(strArray[11]), float.Parse(strArray[12]), float.Parse(strArray[13]), float.Parse(strArray[14]), float.Parse(strArray[15]));
		}

		public static bool TryParse(string value, out Matrix m)
		{
			return Matrix.TryParse(value, out m, ',');
		}

		public static bool TryParse(string value, out Matrix m, params char[] separator)
		{
			bool flag = true;
			m = new Matrix();
			string[] strArray = new string[0];
			if (string.IsNullOrEmpty(value))
				flag = false;
			else
				strArray = value.Split(separator);
			if (flag && strArray.Length != 16)
				flag = false;
			if (flag && !float.TryParse(strArray[0], out m.M11))
				flag = false;
			if (flag && !float.TryParse(strArray[1], out m.M12))
				flag = false;
			if (flag && !float.TryParse(strArray[2], out m.M13))
				flag = false;
			if (flag && !float.TryParse(strArray[3], out m.M14))
				flag = false;
			if (flag && !float.TryParse(strArray[4], out m.M21))
				flag = false;
			if (flag && !float.TryParse(strArray[5], out m.M22))
				flag = false;
			if (flag && !float.TryParse(strArray[6], out m.M23))
				flag = false;
			if (flag && !float.TryParse(strArray[7], out m.M24))
				flag = false;
			if (flag && !float.TryParse(strArray[8], out m.M31))
				flag = false;
			if (flag && !float.TryParse(strArray[9], out m.M32))
				flag = false;
			if (flag && !float.TryParse(strArray[10], out m.M33))
				flag = false;
			if (flag && !float.TryParse(strArray[11], out m.M34))
				flag = false;
			if (flag && !float.TryParse(strArray[12], out m.M41))
				flag = false;
			if (flag && !float.TryParse(strArray[13], out m.M42))
				flag = false;
			if (flag && !float.TryParse(strArray[14], out m.M43))
				flag = false;
			if (flag && !float.TryParse(strArray[15], out m.M44))
				flag = false;
			if (!flag)
				m = new Matrix();
			return flag;
		}

		public override string ToString()
		{
			return this.ToString(',');
		}

		public string ToString(char separator)
		{
			return string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}{0}{10}{0}{11}{0}{12}{0}{13}{0}{14}{0}{15}{0}{16}", (object)separator, (object)this.M11, (object)this.M12, (object)this.M13, (object)this.M14, (object)this.M21, (object)this.M22, (object)this.M23, (object)this.M24, (object)this.M31, (object)this.M32, (object)this.M33, (object)this.M34, (object)this.M41, (object)this.M42, (object)this.M43, (object)this.M44);
		}

		public static Matrix PolarDeviation(Random random, float maxAngle)
		{
			if ((double)maxAngle < 9.99999974737875E-05)
				return Matrix.Identity;
			float num1 = Math.Abs(random.NextSingle());
			float num2 = random.NextInclusive(0.0f, 6.283185f);
			float z = -1f / (float)Math.Tan((double)maxAngle);
			Vector3 vector3 = new Vector3(num1 * (float)Math.Cos((double)num2), num1 * (float)Math.Sin((double)num2), z);
			double num3 = (double)vector3.Normalize();
			float num4 = Vector3.Dot(vector3, Vector3.UnitY);
			Vector3 up = (double)num4 > 0.990000009536743 ? -Vector3.UnitZ : ((double)num4 < -0.990000009536743 ? Vector3.UnitZ : Vector3.UnitY);
			return Matrix.CreateWorld(Vector3.Zero, vector3, up);
		}
	}
}
