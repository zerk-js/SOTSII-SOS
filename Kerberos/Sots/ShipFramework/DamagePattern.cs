// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.ShipFramework.DamagePattern
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using System;
using System.IO;

namespace Kerberos.Sots.ShipFramework
{
	internal class DamagePattern
	{
		public static readonly DamagePattern Empty = new DamagePattern(1, 1, 0, 0, new byte[4]
		{
	  byte.MaxValue,
	  byte.MaxValue,
	  byte.MaxValue,
	  byte.MaxValue
		});
		private byte[] _data;
		private int _originX;
		private int _originY;
		private int _width;
		private int _height;

		public byte[] Data
		{
			get
			{
				return this._data;
			}
		}

		public int Width
		{
			get
			{
				return this._width;
			}
		}

		public int Height
		{
			get
			{
				return this._height;
			}
		}

		private static int GetStride(int width)
		{
			return (width + 31) / 32 * 4;
		}

		private static int GetSize(int stride, int height)
		{
			return stride * height;
		}

		public void SetValue(int x, int y, bool value)
		{
			int num1 = x / 8;
			int num2 = x % 8;
			int index = y * DamagePattern.GetStride(this._width) + num1;
			byte num3 = this._data[index];
			byte num4 = (byte)(1 << num2);
			byte num5 = !value ? (byte)((uint)num3 & (uint)~num4) : (byte)((uint)num3 | (uint)num4);
			this._data[index] = num5;
		}

		public bool GetValue(int x, int y)
		{
			int num1 = x / 8;
			int num2 = x % 8;
			return ((int)this._data[y * DamagePattern.GetStride(this._width) + num1] & 1 << num2) != 0;
		}

		public int GetTotalFilled()
		{
			int num = 0;
			for (int x = 0; x < this._width; ++x)
			{
				for (int y = 0; y < this._height; ++y)
				{
					if (!this.GetValue(x, y))
						++num;
				}
			}
			return num;
		}

		public DamagePattern(int width, int height)
		  : this(width, height, 0, 0, (byte[])null)
		{
		}

		private static byte[] GetDataFromText(int w, int h, string text)
		{
			if (text == null || w == 0 || h == 0)
				return (byte[])null;
			int stride = DamagePattern.GetStride(w);
			byte[] numArray = new byte[DamagePattern.GetSize(stride, h)];
			int index1 = 0;
			for (int index2 = 0; index2 < h; ++index2)
			{
				for (int index3 = 0; index3 < w; ++index3)
				{
					if (text[index1] != '0')
					{
						int index4 = index2 * stride + index3 / 8;
						numArray[index4] |= (byte)(1 << index3 % 8);
					}
					++index1;
				}
				++index1;
			}
			return numArray;
		}

		public string ToDatabaseString()
		{
			using (ScriptMessageWriter m = new ScriptMessageWriter(true, (MemoryStream)null))
			{
				this.Write(m);
				return Convert.ToBase64String(m.GetBuffer(), 0, (int)m.GetSize(), Base64FormattingOptions.None);
			}
		}

		public static DamagePattern FromDatabaseString(string base64String)
		{
			using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(base64String)))
			{
				using (ScriptMessageReader r = new ScriptMessageReader(true, stream))
					return DamagePattern.Read(r);
			}
		}

		public int GetTotalPoints()
		{
			return this._width * this._height;
		}

		public int GetNumOfType(bool value)
		{
			int num = 0;
			for (int y = 0; y < this._height; ++y)
			{
				for (int x = 0; x < this._width; ++x)
				{
					if (this.GetValue(x, y) == value)
						++num;
				}
			}
			return num;
		}

		public DamagePattern(int width, int height, int originX, int originY, string initialDataText)
		  : this(width, height, originX, originY, DamagePattern.GetDataFromText(width, height, initialDataText))
		{
		}

		public DamagePattern(int width, int height, int originX, int originY, byte[] initialData)
		{
			this._data = initialData;
			this._width = width;
			this._height = height;
			this._originX = originX;
			this._originY = originY;
			if (this._data != null)
				return;
			this._data = new byte[DamagePattern.GetSize(DamagePattern.GetStride(this._width), this._height)];
		}

		public static void FourCCToBytes(
		  uint value,
		  out byte b1,
		  out byte b2,
		  out byte b3,
		  out byte b4)
		{
			b1 = (byte)(value & (uint)byte.MaxValue);
			b2 = (byte)((value & 65280U) >> 8);
			b3 = (byte)((value & 16711680U) >> 16);
			b4 = (byte)((value & 4278190080U) >> 24);
		}

		public static uint BytesToFourCC(byte b1, byte b2, byte b3, byte b4)
		{
			return (uint)((int)b1 | (int)b2 << 8 | (int)b3 << 16 | (int)b4 << 24);
		}

		public static DamagePattern Read(ScriptMessageReader r)
		{
			int width = r.ReadInteger();
			int height = r.ReadInteger();
			int originX = r.ReadInteger();
			int originY = r.ReadInteger();
			int size = DamagePattern.GetSize(DamagePattern.GetStride(width), height);
			byte[] initialData = new byte[size];
			for (int index = 0; index < size; index += 4)
				DamagePattern.FourCCToBytes((uint)r.ReadInteger(), out initialData[index], out initialData[index + 1], out initialData[index + 2], out initialData[index + 3]);
			return new DamagePattern(width, height, originX, originY, initialData);
		}

		public void Write(ScriptMessageWriter m)
		{
			m.WriteInteger(this._width);
			m.WriteInteger(this._height);
			m.WriteInteger(this._originX);
			m.WriteInteger(this._originY);
			int size = DamagePattern.GetSize(DamagePattern.GetStride(this._width), this._height);
			for (int index = 0; index < size; index += 4)
				m.WriteInteger((int)DamagePattern.BytesToFourCC(this._data[index], this._data[index + 1], this._data[index + 2], this._data[index + 3]));
		}
	}
}
