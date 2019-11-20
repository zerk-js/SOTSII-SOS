// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Framework.StringInjectExtension
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Kerberos.Sots.Framework
{
	internal static class StringInjectExtension
	{
		public static string Inject(this string formatString, object injectionObject)
		{
			return StringInjectExtension.Inject(formatString, StringInjectExtension.GetPropertyHash(injectionObject));
		}

		public static string Inject(this string formatString, IDictionary dictionary)
		{
			return StringInjectExtension.Inject(formatString, new Hashtable(dictionary));
		}

		public static string Inject(this string formatString, Hashtable attributes)
		{
			string formatString1 = formatString;
			if (attributes == null || formatString == null)
				return formatString1;
			foreach (string key in (IEnumerable)attributes.Keys)
				formatString1 = formatString1.InjectSingleValue(key, attributes[(object)key]);
			return formatString1;
		}

		public static string InjectSingleValue(
		  this string formatString,
		  string key,
		  object replacementValue)
		{
			string str = formatString;
			foreach (Match match in new Regex("{(" + key + ")(?:}|(?::(.[^}]*)}))").Matches(formatString))
			{
				match.ToString();
				string newValue;
				if (match.Groups[2].Length > 0)
					newValue = string.Format((IFormatProvider)CultureInfo.CurrentCulture, string.Format((IFormatProvider)CultureInfo.InvariantCulture, "{{0:{0}}}", (object)match.Groups[2]), replacementValue);
				else
					newValue = (replacementValue ?? (object)string.Empty).ToString();
				str = str.Replace(match.ToString(), newValue);
			}
			return str;
		}

		private static Hashtable GetPropertyHash(object properties)
		{
			Hashtable hashtable = (Hashtable)null;
			if (properties != null)
			{
				hashtable = new Hashtable();
				foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(properties))
					hashtable.Add((object)property.Name, property.GetValue(properties));
			}
			return hashtable;
		}
	}
}
