// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.SQLite.SQLiteConnection
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Kerberos.Sots.Data.SQLite
{
	internal class SQLiteConnection : IDisposable
	{
		private static readonly string[] StatementsToLog = new string[4]
		{
	  "INSERT",
	  "UPDATE",
	  "DELETE",
	  "REPLACE"
		};
		private string QueryStack = "BEGIN TRANSACTION;";
		public bool LogQueries = true;
		public const string NullValue = "NULL";
		internal const int SQLITE_OK = 0;
		internal const int SQLITE_ROW = 100;
		internal const int SQLITE_DONE = 101;
		internal const int SQLITE_INTEGER = 1;
		internal const int SQLITE_FLOAT = 2;
		internal const int SQLITE_TEXT = 3;
		internal const int SQLITE_BLOB = 4;
		internal const int SQLITE_NULL = 5;
		private IntPtr _db;

		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int sqlite3_open(string filename, out IntPtr db);

		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int sqlite3_open(byte[] filename, out IntPtr db);

		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int sqlite3_open16(byte[] filename, out IntPtr db);

		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int sqlite3_close(IntPtr db);

		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int sqlite3_prepare_v2(
		  IntPtr db,
		  byte[] zSql,
		  int nByte,
		  out IntPtr ppStmpt,
		  IntPtr pzTail);

		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int sqlite3_prepare16_v2(
		  IntPtr db,
		  string zSql,
		  int nByte,
		  out IntPtr ppStmpt,
		  IntPtr pzTail);

		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int sqlite3_step(IntPtr stmHandle);

		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int sqlite3_finalize(IntPtr stmHandle);

		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int sqlite3_errcode(IntPtr db);

		[DllImport("sqlite3.dll", EntryPoint = "sqlite3_errmsg16", CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr _sqlite3_errmsg16(IntPtr db);

		internal static string sqlite3_errmsg16(IntPtr db)
		{
			return Marshal.PtrToStringUni(SQLiteConnection._sqlite3_errmsg16(db));
		}

		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int sqlite3_column_count(IntPtr stmHandle);

		[DllImport("sqlite3.dll", EntryPoint = "sqlite3_column_origin_name16", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr _sqlite3_column_origin_name16(IntPtr stmHandle, int iCol);

		internal static string sqlite3_column_origin_name16(IntPtr stmHandle, int iCol)
		{
			return Marshal.PtrToStringUni(SQLiteConnection._sqlite3_column_origin_name16(stmHandle, iCol));
		}

		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int sqlite3_column_type(IntPtr stmHandle, int iCol);

		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int sqlite3_trigger_rowid(IntPtr db);

		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sqlite3_clear_trigger_rowid(IntPtr db);

		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int sqlite3_column_int(IntPtr stmHandle, int iCol);

		[DllImport("sqlite3.dll", EntryPoint = "sqlite3_column_text16", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr _sqlite3_column_text16(IntPtr stmHandle, int iCol);

		internal static string sqlite3_column_text16(IntPtr stmHandle, int iCol)
		{
			return Marshal.PtrToStringAuto(SQLiteConnection._sqlite3_column_text16(stmHandle, iCol));
		}

		[DllImport("sqlite3.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int sqlite3_exec(
		  IntPtr dbHandle,
		  byte[] statement,
		  IntPtr callbackPtr,
		  IntPtr callbackArg,
		  out string errmsg);

		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern double sqlite3_column_double(IntPtr stmHandle, int iCol);

		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sqlite3_backup_init(
		  IntPtr to,
		  string todbname,
		  IntPtr from,
		  string fromdbname);

		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int sqlite3_backup_step(IntPtr backupHandle, int page);

		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int sqlite3_backup_finish(IntPtr backupHandle);

		public IntPtr GetDbPointer()
		{
			return this._db;
		}

		public void ExecuteNonQueryReferenceUTF8(string statement)
		{
			string errmsg;
			if (SQLiteConnection.sqlite3_exec(this._db, Encoding.UTF8.GetBytes(statement), IntPtr.Zero, IntPtr.Zero, out errmsg) != 0)
				throw new SQLiteException("Could not execute query: " + this.GetErrorMessage());
		}

		private static string GetResultCodeDescription(int value)
		{
			switch (value)
			{
				case 0:
					return "Successful result";
				case 1:
					return "SQL error or missing database";
				case 2:
					return "Internal logic error in SQLite";
				case 3:
					return "Access permission denied";
				case 4:
					return "Callback routine requested an abort";
				case 5:
					return "The database file is locked";
				case 6:
					return "A table in the database is locked";
				case 7:
					return "A malloc() failed";
				case 8:
					return "Attempt to write a readonly database";
				case 9:
					return "Operation terminated by sqlite3_interrupt()";
				case 10:
					return "Some kind of disk I/O error occurred";
				case 11:
					return "The database disk image is malformed";
				case 12:
					return "Unknown opcode in sqlite3_file_control()";
				case 13:
					return "Insertion failed because database is full";
				case 14:
					return "Unable to open the database file";
				case 15:
					return "Database lock protocol error";
				case 16:
					return "Database is empty";
				case 17:
					return "The database schema changed";
				case 18:
					return "String or BLOB exceeds size limit";
				case 19:
					return "Abort due to constraint violation";
				case 20:
					return "Data type mismatch";
				case 21:
					return "Library used incorrectly";
				case 22:
					return "Uses OS features not supported on host";
				case 23:
					return "Authorization denied";
				case 24:
					return "Auxiliary database format error";
				case 25:
					return "2nd parameter to sqlite3_bind out of range";
				case 26:
					return "File opened that is not a database file";
				case 100:
					return "sqlite3_step() has another row ready";
				case 101:
					return "sqlite3_step() has finished executing";
				default:
					return "Unrecognized result code " + (object)value;
			}
		}

		private string GetErrorMessage()
		{
			return SQLiteConnection.GetResultCodeDescription(SQLiteConnection.sqlite3_errcode(this._db)) + " > " + SQLiteConnection.sqlite3_errmsg16(this._db);
		}

		public SQLiteConnection(string path)
		{
			this.OpenDatabase(path);
		}

		private void PerformBackup(IntPtr from, IntPtr to)
		{
			this.ExecuteNonQueryReferenceUTF8("PRAGMA OPTIMIZE");
			IntPtr backupHandle = SQLiteConnection.sqlite3_backup_init(to, "main", from, "main");
			if (backupHandle == IntPtr.Zero)
				throw new SQLiteException("Could not initialize db backup: " + this.GetErrorMessage());
			int num = 0;
			while (num == 0)
				num = SQLiteConnection.sqlite3_backup_step(backupHandle, -1);
			if (num != 101)
				throw new SQLiteException("Could not perform backup step: " + this.GetErrorMessage());
			SQLiteConnection.sqlite3_backup_finish(backupHandle);
		}

		private static IntPtr OpenDatabaseCore(string path)
		{
			IntPtr db;
			if (SQLiteConnection.sqlite3_open(Encoding.UTF8.GetBytes(path), out db) != 0)
				throw new SQLiteException("Could not open database '" + path + "'");
			return db;
		}

		private void CloseDatabaseCore(IntPtr dbPtr)
		{
			if (SQLiteConnection.sqlite3_close(dbPtr) != 0)
				throw new SQLiteException("Could not close database: " + this.GetErrorMessage());
		}

		public void LoadBackup(string path)
		{
			IntPtr num = SQLiteConnection.OpenDatabaseCore(path);
			this.PerformBackup(num, this._db);
			this.CloseDatabaseCore(num);
		}

		public void SaveBackup(string path)
		{
			IntPtr num = SQLiteConnection.OpenDatabaseCore(path);
			this.PerformBackup(this._db, num);
			this.CloseDatabaseCore(num);
		}

		public void Reload()
		{
			IntPtr to = SQLiteConnection.OpenDatabaseCore(":memory:");
			this.PerformBackup(this._db, to);
			this.CloseDatabaseCore(this._db);
			this._db = to;
		}

		private void OpenDatabase(string path)
		{
			this._db = SQLiteConnection.OpenDatabaseCore(path);
			if (!(path == ":memory:"))
				return;
			this.ExecuteNonQueryReferenceUTF8("PRAGMA page_size = 8192");
			this.ExecuteNonQueryReferenceUTF8("PRAGMA synchronous = OFF");
			this.ExecuteNonQueryReferenceUTF8("PRAGMA journal_mode = OFF");
			this.ExecuteNonQueryReferenceUTF8("PRAGMA temp_store = 2");
			this.ExecuteNonQueryReferenceUTF8("PRAGMA cache_size = 50000");
			this.ExecuteNonQueryReferenceUTF8("PRAGMA wal_autocheckpoint = 100000");
			this.ExecuteNonQueryReferenceUTF8("PRAGMA LOCKING_MODE = EXCLUSIVE");
			this.ExecuteNonQueryReferenceUTF8("PRAGMA AUTOMATIC_INDEX = TRUE");
		}

		private void CloseDatabase()
		{
			this.CloseDatabaseCore(this._db);
			this._db = IntPtr.Zero;
		}

		private static string[] SplitQuery(string query)
		{
			return ((IEnumerable<string>)query.Split(new char[1]
			{
		';'
			}, StringSplitOptions.RemoveEmptyEntries)).Select<string, string>((Func<string, string>)(x => x.Trim())).Where<string>((Func<string, bool>)(y => y.Length > 0)).ToArray<string>();
		}

		private IntPtr PrepareStatement(string statement)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(statement);
			IntPtr ppStmpt;
			if (SQLiteConnection.sqlite3_prepare_v2(this._db, bytes, bytes.Length + 1, out ppStmpt, IntPtr.Zero) != 0)
			{
				string errorMessage = this.GetErrorMessage();
				throw new SQLiteException("Could not prepare SQL statement '" + statement + "': " + errorMessage);
			}
			return ppStmpt;
		}

		private void FinalizeStatement(IntPtr statementPtr)
		{
			if (SQLiteConnection.sqlite3_finalize(statementPtr) != 0)
				throw new SQLiteException("Could not finalize SQL statement: " + this.GetErrorMessage());
		}

		private static bool StepStatement(IntPtr statementPtr)
		{
			switch (SQLiteConnection.sqlite3_step(statementPtr))
			{
				case 100:
					return true;
				case 101:
					return false;
				default:
					throw new SQLiteException("SQL statement step failed.");
			}
		}

		private void ExecuteNonQueryStatement(string statement)
		{
			if (SQLiteConnection.IsCommentStatement(statement))
				return;
			IntPtr statementPtr = this.PrepareStatement(statement);
			try
			{
				SQLiteConnection.StepStatement(statementPtr);
			}
			catch (SQLiteException ex)
			{
				throw new SQLiteException("SQL statement failed: \n" + statement + "\n-- " + this.GetErrorMessage());
			}
			finally
			{
				try
				{
					this.FinalizeStatement(statementPtr);
				}
				catch (SQLiteException ex)
				{
					throw new SQLiteException(ex.Message + "\nin: " + statement);
				}
			}
		}

		private IEnumerable<Row> StepRows(IntPtr statementPtr)
		{
			while (SQLiteConnection.StepStatement(statementPtr))
			{
				int colcount = SQLiteConnection.sqlite3_column_count(statementPtr);
				Row row = new Row()
				{
					Values = new string[colcount]
				};
				for (int iCol = 0; iCol < colcount; ++iCol)
					row.Values[iCol] = SQLiteConnection.sqlite3_column_text16(statementPtr, iCol);
				yield return row;
			}
		}

		private Table ExecuteTableQueryStatement(string statement)
		{
			IntPtr statementPtr = this.PrepareStatement(statement);
			try
			{
				return new Table()
				{
					Rows = this.StepRows(statementPtr).ToArray<Row>()
				};
			}
			finally
			{
				try
				{
					this.FinalizeStatement(statementPtr);
				}
				catch (SQLiteException ex)
				{
					throw new SQLiteException(ex.Message + "\nin: " + statement);
				}
			}
		}

		private bool TryLogQuery(string query)
		{
			if (!this.LogQueries)
				return false;
			foreach (string str in SQLiteConnection.StatementsToLog)
			{
				if (query.Contains(str) || SQLiteConnection.IsCommentStatement(query))
				{
					this.ExecuteNonQueryStatement(string.Format(Queries.LogTransaction, (object)query.Replace("'", "''").Replace("\r\n", "")));
					return true;
				}
			}
			return false;
		}

		public void LogComment(string comment)
		{
			if (ScriptHost.AllowConsole)
				App.Log.Trace("db.LogComment: " + comment, "data");
			if (!this.LogQueries)
				return;
			comment = comment.Insert(0, "--");
			this.ExecuteNonQueryStatement(string.Format(Queries.LogTransaction, (object)comment.Replace("'", "''").Replace("\r\n", "")));
		}

		public Table ExecuteTableQuery(string query, bool splitQuery = true)
		{
			this.TryLogQuery(query);
			List<string> stringList = new List<string>();
			if (splitQuery)
				stringList.AddRange((IEnumerable<string>)SQLiteConnection.SplitQuery(query));
			else
				stringList.Add(query);
			int index;
			for (index = 0; index < stringList.Count - 1; ++index)
				this.ExecuteNonQueryStatement(stringList[index]);
			Table table;
			if (index < stringList.Count)
				table = this.ExecuteTableQueryStatement(stringList[index]);
			else
				table = new Table() { Rows = new Row[0] };
			return table;
		}

		public void StackQuery(string query)
		{
			this.QueryStack += query;
		}

		public void ExecuteQueryStack(bool ignore = false)
		{
			this.QueryStack += "COMMIT TRANSACTION;";
			this.ExecuteNonQueryStatement(this.QueryStack);
			this.QueryStack = "BEGIN DEFERRED TRANSACTION;";
		}

		public static bool IsCommentStatement(string value)
		{
			return value != null && value.StartsWith("--");
		}

		public void ExecuteNonQuery(string query, bool ignore = false, bool split = true)
		{
			if (!ignore)
				this.TryLogQuery(query);
			if (split)
			{
				foreach (string statement in SQLiteConnection.SplitQuery(query))
					this.ExecuteNonQueryStatement(statement);
			}
			else
				this.ExecuteNonQueryStatement(query);
		}

		private int ExecuteIntegerQueryStatement(string statement)
		{
			IntPtr num1 = this.PrepareStatement(statement);
			try
			{
				SQLiteConnection.StepStatement(num1);
				int num2 = SQLiteConnection.sqlite3_trigger_rowid(this._db);
				if (num2 > 0)
					return num2;
				return SQLiteConnection.sqlite3_column_int(num1, 0);
			}
			finally
			{
				try
				{
					this.FinalizeStatement(num1);
				}
				catch (SQLiteException ex)
				{
					throw new SQLiteException(ex.Message + "\nin: " + statement);
				}
			}
		}

		private int? ExecuteIntegerQueryStatementDefault(string statement, int? defaultValue)
		{
			IntPtr num1 = this.PrepareStatement(statement);
			try
			{
				if (!SQLiteConnection.StepStatement(num1))
					return defaultValue;
				int num2 = SQLiteConnection.sqlite3_trigger_rowid(this._db);
				if (num2 > 0)
					return new int?(num2);
				return new int?(SQLiteConnection.sqlite3_column_int(num1, 0));
			}
			finally
			{
				try
				{
					this.FinalizeStatement(num1);
				}
				catch (SQLiteException ex)
				{
					throw new SQLiteException(ex.Message + "\nin: " + statement);
				}
			}
		}

		public int ExecuteIntegerQuery(string query)
		{
			this.TryLogQuery(query);
			SQLiteConnection.sqlite3_clear_trigger_rowid(this._db);
			string[] strArray = SQLiteConnection.SplitQuery(query);
			int index;
			for (index = 0; index < strArray.Length - 1; ++index)
				this.ExecuteNonQueryStatement(strArray[index]);
			if (index < strArray.Length)
				return this.ExecuteIntegerQueryStatement(strArray[index]);
			return 0;
		}

		public int? ExecuteIntegerQueryDefault(string query, int? defaultValue)
		{
			this.TryLogQuery(query);
			SQLiteConnection.sqlite3_clear_trigger_rowid(this._db);
			string[] strArray = SQLiteConnection.SplitQuery(query);
			int index;
			for (index = 0; index < strArray.Length - 1; ++index)
				this.ExecuteNonQueryStatement(strArray[index]);
			if (index < strArray.Length)
				return this.ExecuteIntegerQueryStatementDefault(strArray[index], defaultValue);
			return new int?(0);
		}

		private string ExecuteStringQueryStatement(string statement)
		{
			IntPtr num = this.PrepareStatement(statement);
			try
			{
				SQLiteConnection.StepStatement(num);
				return SQLiteConnection.sqlite3_column_text16(num, 0);
			}
			finally
			{
				try
				{
					this.FinalizeStatement(num);
				}
				catch (SQLiteException ex)
				{
					throw new SQLiteException(ex.Message + "\nin: " + statement);
				}
			}
		}

		public string ExecuteStringQuery(string query)
		{
			this.TryLogQuery(query);
			string[] strArray = SQLiteConnection.SplitQuery(query);
			int index;
			for (index = 0; index < strArray.Length - 1; ++index)
				this.ExecuteNonQueryStatement(strArray[index]);
			if (index < strArray.Length)
				return this.ExecuteStringQueryStatement(strArray[index]);
			return null;
		}

		public void VacuumDatabase()
		{
			this.ExecuteNonQueryReferenceUTF8("VACUUM;");
		}

		private IEnumerable<int> ExecuteIntegerArrayQueryStatement(string statement)
		{
			IntPtr statementPtr = this.PrepareStatement(statement);
			try
			{
				while (SQLiteConnection.StepStatement(statementPtr))
					yield return SQLiteConnection.sqlite3_column_int(statementPtr, 0);
			}
			finally
			{
				this.FinalizeStatement(statementPtr);
			}
		}

		public int[] ExecuteIntegerArrayQuery(string query)
		{
			this.TryLogQuery(query);
			string[] strArray = SQLiteConnection.SplitQuery(query);
			int index;
			for (index = 0; index < strArray.Length - 1; ++index)
				this.ExecuteNonQueryStatement(strArray[index]);
			if (index < strArray.Length)
				return this.ExecuteIntegerArrayQueryStatement(strArray[index]).ToArray<int>();
			return new int[0];
		}

		public void Dispose()
		{
			this.CloseDatabase();
		}
	}
}
