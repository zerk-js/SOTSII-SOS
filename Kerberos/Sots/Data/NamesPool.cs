// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.NamesPool
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Kerberos.Sots.Data
{
	internal class NamesPool
	{
		private static List<string> SystemNames = new List<string>();
		private static Dictionary<string, List<string>> ProvinceNames = new Dictionary<string, List<string>>();
		private static Dictionary<string, NamesPool.iNameCollection> AdmiralNames = new Dictionary<string, NamesPool.iNameCollection>();
		private static Dictionary<string, List<string>> FleetNames = new Dictionary<string, List<string>>();
		private static Dictionary<string, List<string>> SalvageProjectNames = new Dictionary<string, List<string>>();
		private static Dictionary<string, NamesPool.ShipNameCollection> ShipNames = new Dictionary<string, NamesPool.ShipNameCollection>();
		private string _lastFileName = "";
		private int planetIteration = 1;
		private readonly Dictionary<string, Type> RaceNameCollectionMap = new Dictionary<string, Type>()
	{
	  {
		"human",
		typeof (NamesPool.HumanNameCollection)
	  },
	  {
		"hiver",
		typeof (NamesPool.HiverNameCollection)
	  },
	  {
		"tarka",
		typeof (NamesPool.TarkaNameCollection)
	  },
	  {
		"morrigi",
		typeof (NamesPool.MorrigiNameCollection)
	  },
	  {
		"hordezuul",
		typeof (NamesPool.ZuulNameCollection)
	  },
	  {
		"presterzuul",
		typeof (NamesPool.ZuulNameCollection)
	  },
	  {
		"liir",
		typeof (NamesPool.LiirNameCollection)
	  },
	  {
		"loa",
		typeof (NamesPool.LoaNameCollection)
	  }
	};
		private const string XmlSet1Name = "set1";
		private const string XmlSet2Name = "set2";
		private const string XmlListAName = "ListA";
		private const string XmlListBName = "ListB";
		private const string XmlListCName = "ListC";
		private const string XmlMaleName = "male";
		private const string XmlFemaleName = "female";
		private const string XmlStringName = "string";
		private const string XmlSystemNamesName = "SystemNames";
		private const string XmlProvinceNamesName = "ProvinceNames";
		private const string XmlAdmiralNamesName = "AdmiralNames";
		private const string XmlFleetNamesName = "FleetNames";
		private const string XmlSalvageProjectNames = "SalvageProjectNames";
		private const string XmlShipsNamesName = "ShipNames";

		private static string GetExclusiveName(ref List<string> list)
		{
			int index = App.GetSafeRandom().Next(list.Count<string>());
			string str = list[index];
			list.Remove(str);
			return str;
		}

		private void GetNewSystemNameList()
		{
			XmlDocument document = new XmlDocument();
			document.Load(ScriptHost.FileSystem, this._lastFileName);
			XmlElement documentElement = document.DocumentElement;
			NamesPool.SystemNames.Clear();
			NamesPool.SystemNames = XmlHelper.GetDataCollection<string>(documentElement, "SystemNames", "string");
			for (int index1 = 0; index1 < NamesPool.SystemNames.Count; ++index1)
			{
				List<string> systemNames;
				int index2;
				(systemNames = NamesPool.SystemNames)[index2 = index1] = systemNames[index2] + " " + NamesPool.ToRomanNumeral(this.planetIteration);
			}
		}

		private static void AppendDuplicate(ref string name, ref List<string> usedNames)
		{
			if (!usedNames.Contains(name))
				return;
			int num = 1;
			string str;
			for (str = string.Format("{0} {1}", (object)name, (object)NamesPool.ToRomanNumeral(num)); usedNames.Contains(str); str = string.Format("{0} {1}", (object)name, (object)NamesPool.ToRomanNumeral(num)))
				++num;
			usedNames.Add(str);
			name = str;
		}

		private static string Choose(List<string> list)
		{
			Random safeRandom = App.GetSafeRandom();
			return list[safeRandom.Next(list.Count<string>())];
		}

		private static string ToRomanNumeral(int value)
		{
			if (value > 399 || value < 1)
				return "";
			StringBuilder stringBuilder = new StringBuilder();
			int[] numArray = new int[9]
			{
		100,
		90,
		50,
		40,
		10,
		9,
		5,
		4,
		1
			};
			string[] strArray = new string[9]
			{
		"C",
		"XC",
		"L",
		"XL",
		"X",
		"IX",
		"V",
		"IV",
		"I"
			};
			for (int index = 0; index < 9; ++index)
			{
				while (value >= numArray[index])
				{
					value -= numArray[index];
					stringBuilder.Append(strArray[index]);
				}
			}
			return stringBuilder.ToString();
		}

		public string GetSystemName()
		{
			if (NamesPool.SystemNames.Count == 0)
			{
				++this.planetIteration;
				this.GetNewSystemNameList();
			}
			return NamesPool.GetExclusiveName(ref NamesPool.SystemNames);
		}

		public string GetProvinceName(string faction)
		{
			List<string> provinceName = NamesPool.ProvinceNames[faction];
			string exclusiveName = NamesPool.GetExclusiveName(ref provinceName);
			NamesPool.ProvinceNames[faction] = provinceName;
			return exclusiveName;
		}

		public string GetAdmiralName(string race, string sex = "")
		{
			return NamesPool.AdmiralNames[race].GetName(sex);
		}

		public string GetFleetName(string faction)
		{
			return NamesPool.Choose(NamesPool.FleetNames[faction]);
		}

		public List<string> GetFleetNamesForFaction(string faction)
		{
			return NamesPool.FleetNames[faction];
		}

		public string GetSalvageProjectName(string projtype)
		{
			return App.Localize(NamesPool.Choose(NamesPool.SalvageProjectNames[projtype]));
		}

		public string GetShipName(
		  GameSession game,
		  int playerid,
		  ShipClass shipclass,
		  IEnumerable<string> checknames = null)
		{
			string name = game.AssetDatabase.GetFaction(game.GameDatabase.GetPlayerInfo(playerid).FactionID).Name;
			return NamesPool.ShipNames[name].GetName(game, playerid, shipclass, checknames);
		}

		public NamesPool(string filename)
		{
			this._lastFileName = filename;
			XmlDocument document = new XmlDocument();
			document.Load(ScriptHost.FileSystem, filename);
			XmlElement documentElement = document.DocumentElement;
			NamesPool.SystemNames.Clear();
			NamesPool.AdmiralNames.Clear();
			NamesPool.ProvinceNames.Clear();
			NamesPool.FleetNames.Clear();
			NamesPool.SalvageProjectNames.Clear();
			NamesPool.ShipNames.Clear();
			NamesPool.SystemNames = XmlHelper.GetDataCollection<string>(documentElement, nameof(SystemNames), "string");
			foreach (XmlElement e in (XmlNode)documentElement[nameof(AdmiralNames)])
			{
				NamesPool.AdmiralNames.Add(e.Name, (NamesPool.iNameCollection)Activator.CreateInstance(this.RaceNameCollectionMap[e.Name]));
				NamesPool.AdmiralNames[e.Name].LoadLists(e);
			}
			foreach (XmlElement xmlElement in (XmlNode)documentElement[nameof(ProvinceNames)])
			{
				List<string> dataCollection = XmlHelper.GetDataCollection<string>(documentElement[nameof(ProvinceNames)], xmlElement.Name, "string");
				NamesPool.ProvinceNames.Add(xmlElement.Name, dataCollection);
				foreach (string str in dataCollection)
				{
					int num = 0;
					foreach (List<string> stringList in NamesPool.ProvinceNames.Values)
					{
						if (stringList.Contains(str))
							++num;
					}
				}
			}
			foreach (XmlElement xmlElement in (XmlNode)documentElement[nameof(FleetNames)])
				NamesPool.FleetNames.Add(xmlElement.Name, XmlHelper.GetDataCollection<string>(documentElement[nameof(FleetNames)], xmlElement.Name, "string"));
			foreach (XmlElement xmlElement in (XmlNode)documentElement[nameof(SalvageProjectNames)])
				NamesPool.SalvageProjectNames.Add(xmlElement.Name, XmlHelper.GetDataCollection<string>(documentElement[nameof(SalvageProjectNames)], xmlElement.Name, "string"));
			foreach (XmlElement e in (XmlNode)documentElement[nameof(ShipNames)])
			{
				NamesPool.ShipNames.Add(e.Name, new NamesPool.ShipNameCollection());
				NamesPool.ShipNames[e.Name].LoadLists(e);
			}
		}

		private interface iNameCollection
		{
			string GetName(string sex);

			void LoadLists(XmlElement e);
		}

		private class HumanNameCollection : NamesPool.iNameCollection
		{
			private List<string> UsedNames = new List<string>();
			private Dictionary<string, List<string>> ListA = new Dictionary<string, List<string>>();
			private List<string> ListB = new List<string>();
			private List<string> ListC = new List<string>();

			public string GetName(string sex)
			{
				string name = string.Format("{0} {1}", (object)NamesPool.Choose(this.ListA[sex]), (object)NamesPool.Choose(this.ListB));
				NamesPool.AppendDuplicate(ref name, ref this.UsedNames);
				return name;
			}

			public void LoadLists(XmlElement e)
			{
				this.ListA.Add("male", XmlHelper.GetDataCollection<string>(e["ListA"], "male", "string"));
				this.ListA.Add("female", XmlHelper.GetDataCollection<string>(e["ListA"], "female", "string"));
				this.ListB = XmlHelper.GetDataCollection<string>(e, "ListB", "string");
			}
		}

		private class HiverNameCollection : NamesPool.iNameCollection
		{
			private List<string> UsedNames = new List<string>();
			private List<string> ListA = new List<string>();
			private List<string> ListB = new List<string>();
			private List<string> ListC = new List<string>();

			public string GetName(string sex)
			{
				string name = string.Format("{0} {1} {2}", (object)NamesPool.Choose(this.ListA), (object)NamesPool.Choose(this.ListB), (object)NamesPool.Choose(this.ListC));
				NamesPool.AppendDuplicate(ref name, ref this.UsedNames);
				return name;
			}

			public void LoadLists(XmlElement e)
			{
				this.ListA = XmlHelper.GetDataCollection<string>(e, "ListA", "string");
				this.ListB = XmlHelper.GetDataCollection<string>(e, "ListB", "string");
				this.ListC = XmlHelper.GetDataCollection<string>(e, "ListC", "string");
			}
		}

		private class TarkaNameCollection : NamesPool.iNameCollection
		{
			private List<string> UsedNames = new List<string>();
			private Dictionary<string, List<string>> ListA = new Dictionary<string, List<string>>();
			private List<string> ListB = new List<string>();
			private List<string> ListC = new List<string>();

			public string GetName(string sex)
			{
				string name = string.Format("{0} {1} {2}", (object)NamesPool.Choose(this.ListA[sex]), (object)NamesPool.Choose(this.ListB), (object)NamesPool.Choose(this.ListC));
				NamesPool.AppendDuplicate(ref name, ref this.UsedNames);
				return name;
			}

			public void LoadLists(XmlElement e)
			{
				this.ListA.Add("male", XmlHelper.GetDataCollection<string>(e["ListA"], "male", "string"));
				this.ListA.Add("female", XmlHelper.GetDataCollection<string>(e["ListA"], "female", "string"));
				this.ListB = XmlHelper.GetDataCollection<string>(e, "ListB", "string");
				this.ListC = XmlHelper.GetDataCollection<string>(e, "ListC", "string");
			}
		}

		private class MorrigiNameCollection : NamesPool.iNameCollection
		{
			private List<string> UsedNames = new List<string>();
			private Dictionary<string, List<string>> ListA = new Dictionary<string, List<string>>();
			private List<string> ListB = new List<string>();

			public string GetName(string sex)
			{
				string name = string.Format("{0} {1}", (object)NamesPool.Choose(this.ListA[sex]), (object)NamesPool.Choose(this.ListB));
				NamesPool.AppendDuplicate(ref name, ref this.UsedNames);
				return name;
			}

			public void LoadLists(XmlElement e)
			{
				this.ListA.Add("male", XmlHelper.GetDataCollection<string>(e["ListA"], "male", "string"));
				this.ListA.Add("female", XmlHelper.GetDataCollection<string>(e["ListA"], "female", "string"));
				this.ListB = XmlHelper.GetDataCollection<string>(e, "ListB", "string");
			}
		}

		private class ZuulNameCollection : NamesPool.iNameCollection
		{
			private List<string> UsedNames = new List<string>();
			private List<string> ListA = new List<string>();
			private List<string> ListB = new List<string>();
			private List<string> ListC = new List<string>();

			public string GetName(string sex)
			{
				string name = string.Format("{0} {1} {2}", (object)NamesPool.Choose(this.ListA), (object)NamesPool.Choose(this.ListB), (object)NamesPool.Choose(this.ListC));
				NamesPool.AppendDuplicate(ref name, ref this.UsedNames);
				return name;
			}

			public void LoadLists(XmlElement e)
			{
				this.ListA = XmlHelper.GetDataCollection<string>(e, "ListA", "string");
				this.ListB = XmlHelper.GetDataCollection<string>(e, "ListB", "string");
				this.ListC = XmlHelper.GetDataCollection<string>(e, "ListC", "string");
			}
		}

		private class LiirNameCollection : NamesPool.iNameCollection
		{
			private List<string> UsedNames = new List<string>();
			private List<string> ListA = new List<string>();

			public string GetName(string sex)
			{
				string name = NamesPool.Choose(this.ListA);
				NamesPool.AppendDuplicate(ref name, ref this.UsedNames);
				return name;
			}

			public void LoadLists(XmlElement e)
			{
				this.ListA = XmlHelper.GetDataCollection<string>(e, "ListA", "string");
			}
		}

		private class ShipNameCollection
		{
			private List<List<KeyValuePair<string, string>>> Lists = new List<List<KeyValuePair<string, string>>>();

			public string GetName(
			  GameSession game,
			  int playerid,
			  ShipClass shipclass,
			  IEnumerable<string> checknames = null)
			{
				string name = "";
				foreach (List<KeyValuePair<string, string>> list in this.Lists)
				{
					if (list.Any<KeyValuePair<string, string>>((Func<KeyValuePair<string, string>, bool>)(x =>
				   {
					   if (!(x.Key == ""))
						   return x.Key.ToLower().Contains(shipclass.ToString().ToLower());
					   return true;
				   })))
						name += string.Format("{0}", (object)NamesPool.Choose(list.Where<KeyValuePair<string, string>>((Func<KeyValuePair<string, string>, bool>)(x =>
					  {
						  if (!(x.Key == ""))
							  return x.Key.ToLower().Contains(shipclass.ToString().ToLower());
						  return true;
					  })).Select<KeyValuePair<string, string>, string>((Func<KeyValuePair<string, string>, string>)(x => x.Value)).ToList<string>()));
				}
				List<string> usedNames = new List<string>();
				usedNames.AddRange(game.GameDatabase.GetShipInfos(true).Where<ShipInfo>((Func<ShipInfo, bool>)(x => x.DesignInfo.PlayerID == playerid)).Select<ShipInfo, string>((Func<ShipInfo, string>)(x => x.ShipName)));
				foreach (InvoiceInfo invoiceInfo in game.GameDatabase.GetInvoiceInfosForPlayer(playerid))
				{
					foreach (BuildOrderInfo buildOrderInfo in game.GameDatabase.GetBuildOrdersForInvoiceInstance(invoiceInfo.ID))
						usedNames.Add(buildOrderInfo.ShipName);
				}
				if (checknames != null)
					usedNames.AddRange(checknames);
				NamesPool.AppendDuplicate(ref name, ref usedNames);
				return name;
			}

			public void LoadLists(XmlElement e)
			{
				foreach (XmlNode childNode1 in e.ChildNodes)
				{
					List<KeyValuePair<string, string>> source = new List<KeyValuePair<string, string>>();
					foreach (XmlNode childNode2 in childNode1.ChildNodes)
					{
						string key = ((XmlElement)childNode2).GetAttribute("class") ?? "";
						string innerText = childNode2.InnerText;
						source.Add(new KeyValuePair<string, string>(key, innerText));
					}
					if (source.Any<KeyValuePair<string, string>>())
						this.Lists.Add(source);
				}
			}
		}

		private class LoaNameCollection : NamesPool.iNameCollection
		{
			private List<string> UsedNames = new List<string>();
			private List<string> ListA = new List<string>();
			private List<string> ListB = new List<string>();

			public string GetName(string sex)
			{
				string name = string.Format("{0} {1}", (object)NamesPool.Choose(this.ListA), (object)NamesPool.Choose(this.ListB));
				NamesPool.AppendDuplicate(ref name, ref this.UsedNames);
				return name;
			}

			public void LoadLists(XmlElement e)
			{
				this.ListA = XmlHelper.GetDataCollection<string>(e, "ListA", "string");
				this.ListB = XmlHelper.GetDataCollection<string>(e, "ListB", "string");
			}
		}
	}
}
