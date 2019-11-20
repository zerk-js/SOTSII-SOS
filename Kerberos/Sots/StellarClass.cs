// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.StellarClass
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots
{
	public struct StellarClass : IComparable<StellarClass>, IEquatable<StellarClass>
	{
		private static Random _rand = new Random((int)DateTime.UtcNow.Ticks);
		public static readonly StellarClass Default = new StellarClass("G2V");
		private static readonly IEnumerable<StellarSize> StellarSizeValues = Enum.GetValues(typeof(StellarSize)).Cast<StellarSize>();
		public const int MinSubType = 0;
		public const int MaxSubType = 9;
		private byte _type;
		private byte _subtype;
		private byte _size;

		public static string Random(string Type = null, int? SubType = null, string Size = null)
		{
			string str = "";
			StellarType stellarType = (StellarType)StellarClass._rand.Next(0, 7);
			StellarSize stellarSize = (StellarSize)StellarClass._rand.Next(0, 8);
			int num = StellarClass._rand.Next(0, 9);
			return str + (Type ?? stellarType.ToString()) + (object)(SubType ?? num) + (Size ?? stellarSize.ToString());
		}

		public StellarType Type
		{
			get
			{
				return (StellarType)this._type;
			}
			set
			{
				this._type = (byte)value;
			}
		}

		public int SubType
		{
			get
			{
				return (int)this._subtype;
			}
			set
			{
				if (value < 0 || value > 9)
					throw new ArgumentOutOfRangeException(nameof(value), "Stellar sub-type must be 0-9.");
				this._subtype = (byte)value;
			}
		}

		public StellarSize Size
		{
			get
			{
				return (StellarSize)this._size;
			}
			set
			{
				this._size = (byte)value;
			}
		}

		public StellarClass(string str)
		{
			this = StellarClass.Parse(str);
		}

		public unsafe StellarClass(StellarType type, int subtype, StellarSize size)
		{
			this = default;
			this.Type = type;
			this.SubType = subtype;
			this.Size = size;
		}

		public static bool TryParseType(string str, out StellarType value)
		{
			return Enum.TryParse<StellarType>(str, out value);
		}

		public static bool TryParseSubType(string str, out int value)
		{
			if (int.TryParse(str, out value) && value >= 0)
				return value <= 9;
			return false;
		}

		public static bool TryParseSize(string str, out StellarSize value)
		{
			return Enum.TryParse<StellarSize>(str, out value);
		}

		private static void Split(string str, out string type, out string subtype, out string size)
		{
			type = str.Substring(0, 1);
			if (str.Length > 1 && char.IsDigit(str[1]))
			{
				subtype = str.Substring(1, 1);
				size = str.Length > 2 ? str.Substring(2) : string.Empty;
			}
			else
			{
				subtype = string.Empty;
				size = str.Length > 1 ? str.Substring(1) : string.Empty;
			}
		}

		private static StellarClass Parse(string str, StellarClass.ParseMode mode)
		{
			if (str == "Deepspace")
				return new StellarClass(StellarType.MAX, 0, StellarSize.MAX);
			try
			{
				string type;
				string subtype;
				string size;
				StellarClass.Split(str, out type, out subtype, out size);
				switch (mode)
				{
					case StellarClass.ParseMode.Normal:
						if (string.IsNullOrEmpty(subtype))
						{
							subtype = 0.ToString();
							break;
						}
						break;
					case StellarClass.ParseMode.MinRange:
						if (string.IsNullOrEmpty(subtype))
							subtype = 0.ToString();
						if (string.IsNullOrEmpty(size))
						{
							size = StellarClass.StellarSizeValues.First<StellarSize>().ToString();
							break;
						}
						break;
					case StellarClass.ParseMode.MaxRange:
						if (string.IsNullOrEmpty(subtype))
							subtype = 9.ToString();
						if (string.IsNullOrEmpty(size))
						{
							size = StellarClass.StellarSizeValues.Last<StellarSize>().ToString();
							break;
						}
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(mode));
				}
				return new StellarClass((StellarType)Enum.Parse(typeof(StellarType), type), int.Parse(subtype), (StellarSize)Enum.Parse(typeof(StellarSize), size));
			}
			catch (Exception ex)
			{
				throw new ArgumentOutOfRangeException(string.Format("'{0}' is not a valid StellarClass.", (object)str), ex);
			}
		}

		public static StellarClass Parse(string str)
		{
			return StellarClass.Parse(str, StellarClass.ParseMode.Normal);
		}

		public static bool Parse(string str, out StellarClass output)
		{
			output = StellarClass.Parse(str);
			return true;
		}

		public static bool Contains(string minRange, string maxRange, StellarClass value)
		{
			return StellarClass.Contains(StellarClass.Parse(minRange, StellarClass.ParseMode.MinRange), StellarClass.Parse(maxRange, StellarClass.ParseMode.MaxRange), value);
		}

		public static bool Contains(StellarClass minRange, StellarClass maxRange, StellarClass value)
		{
			if (value >= minRange)
				return value <= maxRange;
			return false;
		}

		public static bool operator <(StellarClass valueA, StellarClass valueB)
		{
			return StellarClass.Compare(valueA, valueB) < 0;
		}

		public static bool operator >(StellarClass valueA, StellarClass valueB)
		{
			return StellarClass.Compare(valueA, valueB) > 0;
		}

		public static bool operator <=(StellarClass valueA, StellarClass valueB)
		{
			return StellarClass.Compare(valueA, valueB) <= 0;
		}

		public static bool operator >=(StellarClass valueA, StellarClass valueB)
		{
			return StellarClass.Compare(valueA, valueB) >= 0;
		}

		public static bool operator ==(StellarClass valueA, StellarClass valueB)
		{
			return StellarClass.Compare(valueA, valueB) == 0;
		}

		public static bool operator !=(StellarClass valueA, StellarClass valueB)
		{
			return StellarClass.Compare(valueA, valueB) != 0;
		}

		public static bool operator <(string valueA, StellarClass valueB)
		{
			return StellarClass.Compare(new StellarClass(valueA), valueB) < 0;
		}

		public static bool operator >(string valueA, StellarClass valueB)
		{
			return StellarClass.Compare(new StellarClass(valueA), valueB) > 0;
		}

		public static bool operator <=(string valueA, StellarClass valueB)
		{
			return StellarClass.Compare(new StellarClass(valueA), valueB) <= 0;
		}

		public static bool operator >=(string valueA, StellarClass valueB)
		{
			return StellarClass.Compare(new StellarClass(valueA), valueB) >= 0;
		}

		public static bool operator ==(string valueA, StellarClass valueB)
		{
			return StellarClass.Compare(new StellarClass(valueA), valueB) == 0;
		}

		public static bool operator !=(string valueA, StellarClass valueB)
		{
			return StellarClass.Compare(new StellarClass(valueA), valueB) != 0;
		}

		public static bool operator <(StellarClass valueA, string valueB)
		{
			return StellarClass.Compare(valueA, new StellarClass(valueB)) < 0;
		}

		public static bool operator >(StellarClass valueA, string valueB)
		{
			return StellarClass.Compare(valueA, new StellarClass(valueB)) > 0;
		}

		public static bool operator <=(StellarClass valueA, string valueB)
		{
			return StellarClass.Compare(valueA, new StellarClass(valueB)) <= 0;
		}

		public static bool operator >=(StellarClass valueA, string valueB)
		{
			return StellarClass.Compare(valueA, new StellarClass(valueB)) >= 0;
		}

		public static bool operator ==(StellarClass valueA, string valueB)
		{
			return StellarClass.Compare(valueA, new StellarClass(valueB)) == 0;
		}

		public static bool operator !=(StellarClass valueA, string valueB)
		{
			return StellarClass.Compare(valueA, new StellarClass(valueB)) != 0;
		}

		bool IEquatable<StellarClass>.Equals(StellarClass other)
		{
			return StellarClass.Compare(this, other) == 0;
		}

		public bool Equals(StellarClass other)
		{
			return StellarClass.Compare(this, other) == 0;
		}

		public int CompareTo(StellarClass other)
		{
			return StellarClass.Compare(this, other);
		}

		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is StellarClass))
				return false;
			return StellarClass.Compare(this, (StellarClass)obj) == 0;
		}

		private static int Compare(StellarClass lhs, StellarClass rhs)
		{
			if ((int)lhs._type != (int)rhs._type)
				return lhs._type.CompareTo(rhs._type);
			if ((int)lhs._subtype != (int)rhs._subtype)
				return lhs._subtype.CompareTo(rhs._subtype);
			return lhs._size.CompareTo(rhs._size);
		}

		public override string ToString()
		{
			return string.Format("{0}{1}{2}", (object)this.Type, (object)this.SubType, (object)this.Size);
		}

		public override int GetHashCode()
		{
			return (int)this._type | (int)this._subtype << 8 | (int)this._size << 16;
		}

		public string GetStellarActivity()
		{
			switch (this.Type)
			{
				case StellarType.O:
					return "7-9";
				case StellarType.B:
					return "6-8";
				case StellarType.A:
					return "5-8";
				case StellarType.F:
					return "4-8";
				case StellarType.G:
					return "3-7";
				case StellarType.K:
					return "1-5";
				case StellarType.M:
					return "0-4";
				default:
					return "0";
			}
		}

		public int GetInterference()
		{
			switch (this.Type)
			{
				case StellarType.O:
					return StellarClass._rand.Next(7, 9);
				case StellarType.B:
					return StellarClass._rand.Next(6, 8);
				case StellarType.A:
					return StellarClass._rand.Next(5, 8);
				case StellarType.F:
					return StellarClass._rand.Next(4, 8);
				case StellarType.G:
					return StellarClass._rand.Next(3, 7);
				case StellarType.K:
					return StellarClass._rand.Next(1, 5);
				case StellarType.M:
					return StellarClass._rand.Next(0, 4);
				default:
					return 0;
			}
		}

		public int GetAverageInterference()
		{
			switch (this.Type)
			{
				case StellarType.O:
					return 8;
				case StellarType.B:
					return 7;
				case StellarType.A:
					return 6;
				case StellarType.F:
					return 6;
				case StellarType.G:
					return 5;
				case StellarType.K:
					return 3;
				case StellarType.M:
					return 2;
				default:
					return 0;
			}
		}

		private enum ParseMode
		{
			Normal,
			MinRange,
			MaxRange,
		}
	}
}
