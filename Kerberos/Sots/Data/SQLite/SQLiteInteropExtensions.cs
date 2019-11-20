// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.SQLite.SQLiteInteropExtensions
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using System;

namespace Kerberos.Sots.Data.SQLite
{
	internal static class SQLiteInteropExtensions
	{
		public static string ToSQLiteValue(this string value)
		{
			return value;
		}

		public static string ToNullableSQLiteValue(this string value)
		{
			if (string.IsNullOrEmpty(value))
				return "NULL";
			return "\"" + value.ToSQLiteValue() + "\"";
		}

		public static string ToSQLiteValue(this bool value)
		{
			return value ? "\"True\"" : "\"False\"";
		}

		public static string ToNullableSQLiteValue(this bool? value)
		{
			if (!value.HasValue)
				return "NULL";
			return value.Value.ToSQLiteValue();
		}

		public static string ToSQLiteValue(this Guid value)
		{
			return "\"" + value.ToString() + "\"";
		}

		public static string ToNullableSQLiteValue(this Guid? value)
		{
			if (!value.HasValue)
				return "NULL";
			return value.Value.ToSQLiteValue();
		}

		public static string ToSQLiteValue(this int value)
		{
			return value.ToString();
		}

		public static string ToOneBasedSQLiteValue(this int value)
		{
			if (value == 0)
				return "NULL";
			return value.ToString();
		}

		public static string ToNullableSQLiteValue(this int? value)
		{
			if (!value.HasValue)
				return "NULL";
			return value.Value.ToSQLiteValue();
		}

		public static string ToSQLiteValue(this double value)
		{
			if (!value.IsFinite())
				throw new ArgumentOutOfRangeException(nameof(value));
			return value.ToString();
		}

		public static string ToNullableSQLiteValue(this double? value)
		{
			if (!value.HasValue)
				return "NULL";
			return value.Value.ToSQLiteValue();
		}

		public static string ToSQLiteValue(this float value)
		{
			if (!value.IsFinite())
				throw new ArgumentOutOfRangeException(nameof(value));
			return value.ToString();
		}

		public static string ToNullableSQLiteValue(this float? value)
		{
			if (!value.HasValue)
				return "NULL";
			return value.Value.ToSQLiteValue();
		}

		public static string ToSQLiteValue(this Vector3 value)
		{
			if (!value.IsFinite())
				throw new ArgumentOutOfRangeException(nameof(value));
			return value.ToString();
		}

		public static string ToSQLiteValue(this Vector3? value)
		{
			if (!value.HasValue)
				return "null";
			if (!value.Value.IsFinite())
				throw new ArgumentOutOfRangeException(nameof(value));
			return value.ToString();
		}

		public static string ToNullableSQLiteValue(this Vector3? value)
		{
			if (!value.HasValue)
				return "NULL";
			return value.Value.ToSQLiteValue();
		}

		public static string ToSQLiteValue(this Matrix value)
		{
			if (!value.IsFinite())
				throw new ArgumentOutOfRangeException(nameof(value));
			return value.ToString();
		}

		public static string ToNullableSQLiteValue(this Matrix? value)
		{
			if (!value.HasValue)
				return "NULL";
			return value.Value.ToSQLiteValue();
		}

		public static string ToSQLiteValue(this byte[] banana)
		{
			return Convert.ToBase64String(banana);
		}

		public static Guid SQLiteValueToGuid(this string sqliteValue)
		{
			return Guid.Parse(sqliteValue);
		}

		public static Guid? SQLiteValueToNullableGuid(this string sqliteValue)
		{
			if (sqliteValue == "NULL" || sqliteValue == null)
				return new Guid?();
			return new Guid?(sqliteValue.SQLiteValueToGuid());
		}

		public static int SQLiteValueToInteger(this string sqliteValue)
		{
			return int.Parse(sqliteValue);
		}

		public static int SQLiteValueToOneBasedIndex(this string sqliteValue)
		{
			if (sqliteValue == "NULL" || sqliteValue == null)
				return 0;
			return sqliteValue.SQLiteValueToInteger();
		}

		public static int? SQLiteValueToNullableInteger(this string sqliteValue)
		{
			if (sqliteValue == "NULL" || sqliteValue == null)
				return new int?();
			return new int?(sqliteValue.SQLiteValueToInteger());
		}

		public static float SQLiteValueToSingle(this string sqliteValue)
		{
			return float.Parse(sqliteValue);
		}

		public static float? SQLiteValueToNullableSingle(this string sqliteValue)
		{
			if (sqliteValue == "NULL" || sqliteValue == null)
				return new float?();
			return new float?(sqliteValue.SQLiteValueToSingle());
		}

		public static double SQLiteValueToDouble(this string sqliteValue)
		{
			return double.Parse(sqliteValue);
		}

		public static double? SQLiteValueToNullableDouble(this string sqliteValue)
		{
			if (sqliteValue == "NULL" || sqliteValue == null)
				return new double?();
			return new double?(sqliteValue.SQLiteValueToDouble());
		}

		public static bool SQLiteValueToBoolean(this string sqliteValue)
		{
			return !(sqliteValue.ToLower() == "false") && !(sqliteValue == "0");
		}

		public static string SQLiteValueToString(this string sqliteValue)
		{
			if (sqliteValue == null)
				throw new ArgumentNullException(nameof(sqliteValue));
			return sqliteValue;
		}

		public static Vector3 SQLiteValueToVector3(this string sqliteValue)
		{
			return Vector3.Parse(sqliteValue);
		}

		public static Vector3? SQLiteValueToNullableVector3(this string sqliteValue)
		{
			Vector3 v = new Vector3();
			if (Vector3.TryParse(sqliteValue, out v))
				return new Vector3?(v);
			return new Vector3?();
		}

		public static Matrix? SQLiteValueToNullableMatrix(this string sqliteValue)
		{
			Matrix m = new Matrix();
			if (Matrix.TryParse(sqliteValue, out m))
				return new Matrix?(m);
			return new Matrix?();
		}

		public static Matrix SQLiteValueToMatrix(this string sqliteValue)
		{
			return Matrix.Parse(sqliteValue);
		}

		public static byte[] SQLiteValueToByteArray(this string sqliteValue)
		{
			return Convert.FromBase64String(sqliteValue);
		}
	}
}
