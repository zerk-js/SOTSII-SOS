// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.StellarBodyTypes
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots
{
	internal static class StellarBodyTypes
	{
		public static readonly string Barren = "barren";
		public static readonly string Gaseous = "gaseous";
		public static readonly string Normal = "normal";
		public static readonly string Pastoral = "pastoral";
		public static readonly string Volcanic = "volcanic";
		public static readonly string Cavernous = "cavernous";
		public static readonly string Tempestuous = "tempestuous";
		public static readonly string Magnar = "magnar";
		public static readonly string Primordial = "primordial";
		private static readonly string[] _terrestrialTypes = new string[8]
		{
	  StellarBodyTypes.Normal,
	  StellarBodyTypes.Pastoral,
	  StellarBodyTypes.Volcanic,
	  StellarBodyTypes.Cavernous,
	  StellarBodyTypes.Tempestuous,
	  StellarBodyTypes.Magnar,
	  StellarBodyTypes.Primordial,
	  StellarBodyTypes.Pastoral
		};
		private static readonly string[] _specialTerrestrialTypes = new string[7]
		{
	  StellarBodyTypes.Pastoral,
	  StellarBodyTypes.Volcanic,
	  StellarBodyTypes.Cavernous,
	  StellarBodyTypes.Tempestuous,
	  StellarBodyTypes.Magnar,
	  StellarBodyTypes.Primordial,
	  StellarBodyTypes.Pastoral
		};

		public static string[] TerrestrialTypes
		{
			get
			{
				return StellarBodyTypes._terrestrialTypes;
			}
		}

		public static string[] SpecialTerrestrialTypes
		{
			get
			{
				return StellarBodyTypes._specialTerrestrialTypes;
			}
		}

		public static bool IsTerrestrial(string type)
		{
			return ((IEnumerable<string>)StellarBodyTypes.TerrestrialTypes).Contains<string>(type);
		}
	}
}
