// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Framework.CsvOperations
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Kerberos.Sots.Framework
{
	public static class CsvOperations
	{
		public const char DefaultDelimiter = ',';
		public const char DefaultQuoteChar = '"';

		public static string[] SplitLine(string line, char quoteChar, char delimiter)
		{
			return CsvOperations.ParseQuotedStrings(line, quoteChar, delimiter, 0).ToArray<string>();
		}

		public static IEnumerable<string[]> Read(
		  IFileSystem fileSystem,
		  string filename,
		  char quoteChar,
		  char delimiter,
		  int firstColumn,
		  int maxColumns)
		{
			using (Stream stream = fileSystem.CreateStream(filename))
			{
				using (StreamReader csvStream = new StreamReader(stream))
					return CsvOperations.Read(csvStream, quoteChar, delimiter, firstColumn, maxColumns);
			}
		}

		public static IEnumerable<string[]> Read(
		  string filename,
		  char quoteChar,
		  char delimiter,
		  int firstColumn,
		  int maxColumns)
		{
			using (StreamReader reader = new StreamReader(filename))
			{
				foreach (string[] strArray in CsvOperations.Read(reader, quoteChar, delimiter, firstColumn, maxColumns))
					yield return strArray;
			}
		}

		public static IEnumerable<string[]> Read(
		  string filename,
		  char quoteChar,
		  char delimiter,
		  int firstColumn,
		  int maxColumns,
		  Encoding enc)
		{
			using (StreamReader reader = new StreamReader(filename, enc))
			{
				foreach (string[] strArray in CsvOperations.Read(reader, quoteChar, delimiter, firstColumn, maxColumns))
					yield return strArray;
			}
		}

		public static IEnumerable<string[]> Read(
		  StreamReader csvStream,
		  char quoteChar,
		  char delimiter,
		  int firstColumn,
		  int maxColumns)
		{
			string empty = string.Empty;
			int line = 0;
			List<string[]> strArrayList = new List<string[]>();
			string toParse;
			while ((toParse = csvStream.ReadLine()) != null)
			{
				++line;
				strArrayList.Add(CsvOperations.ParseQuotedStrings(toParse, quoteChar, delimiter, line).Skip<string>(firstColumn).Take<string>(maxColumns).ToArray<string>());
			}
			return (IEnumerable<string[]>)strArrayList;
		}

		public static string ToCSV(string[] textlines)
		{
			return CsvOperations.ToCSV(textlines, '"', ',');
		}

		public static string ToCSV(string[] textlines, char quoteChar, char delimiter)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string textline in textlines)
			{
				stringBuilder.Append(CsvOperations.QuoteStr(textline, quoteChar));
				stringBuilder.Append(delimiter);
			}
			return stringBuilder.Remove(stringBuilder.Length - 1, 1).ToString();
		}

		private static string ExtractQuotedStr(string value, char quotedChar)
		{
			string empty = string.Empty;
			int num = 0;
			if (string.IsNullOrWhiteSpace(value) || value[0] != quotedChar)
				return value;
			for (int index = 1; index < value.Length - 1; ++index)
			{
				if (value[index] == quotedChar)
				{
					++num;
					if (num == 2)
					{
						empty += quotedChar.ToString();
						num = 0;
					}
				}
				else
					empty += value[index].ToString();
			}
			return empty;
		}

		private static string QuoteStr(string value, char quoteChar)
		{
			if (!value.Contains(" ") && !value.Contains<char>(quoteChar) && !value.Contains(Environment.NewLine))
				return value;
			string str = quoteChar.ToString();
			for (int index = 0; index < value.Length; ++index)
			{
				if ((int)value[index] == (int)quoteChar)
					str += quoteChar.ToString();
				str += value[index].ToString();
			}
			return str + (object)quoteChar;
		}

		private static IEnumerable<string> ParseQuotedStrings(string toParse, char quoteChar, char delimiter, int line)
		{
			string parsedStr = string.Empty;
			bool insideQuote = false;
			for (int i = 0; i < toParse.Length; i++)
			{
				if (toParse[i] != quoteChar & toParse[i] != delimiter)
				{
					parsedStr += toParse[i];
				}
				else if (toParse[i] == delimiter)
				{
					if (!insideQuote)
					{
						yield return CsvOperations.ExtractQuotedStr(parsedStr, quoteChar);
						parsedStr = string.Empty;
					}
					else
					{
						parsedStr += toParse[i];
					}
				}
				else if (toParse[i] == quoteChar && i != 0 && i - 1 > 0 && toParse[i - 1] == delimiter)
				{
					insideQuote = true;
					parsedStr += toParse[i];
				}
				else if (toParse[i] == quoteChar & i == toParse.Length - 1)
				{
					parsedStr += toParse[i];
				}
				else if (toParse[i] == quoteChar & toParse[i + 1] == delimiter)
				{
					insideQuote = false;
					parsedStr += toParse[i];
				}
				else if (toParse[i] == quoteChar)
				{
					parsedStr += toParse[i];
				}
				else if (toParse[i] == delimiter)
				{
					parsedStr += toParse[i];
				}
			}
			yield return CsvOperations.ExtractQuotedStr(parsedStr, quoteChar);
			parsedStr = string.Empty;
			yield break;
		}
	}
}
