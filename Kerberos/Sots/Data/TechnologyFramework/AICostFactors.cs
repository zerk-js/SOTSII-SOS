// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.TechnologyFramework.AICostFactors
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Kerberos.Sots.Data.TechnologyFramework
{
	public struct AICostFactors
	{
		public static readonly AICostFactors Default = new AICostFactors()
		{
			Hiver = 1f,
			Human = 1f,
			LiirZuul = 1f,
			Morrigi = 1f,
			Tarka = 1f,
			Zuul = 1f,
			Loa = 1f
		};
		public static readonly AICostFactors Zero = new AICostFactors()
		{
			Hiver = 0.0f,
			Human = 0.0f,
			LiirZuul = 0.0f,
			Morrigi = 0.0f,
			Tarka = 0.0f,
			Zuul = 0.0f,
			Loa = 0.0f
		};
		private static readonly string[] _factions = new string[7]
		{
	  "hiver",
	  "human",
	  "liir_zuul",
	  "morrigi",
	  "tarkas",
	  "zuul",
	  "loa"
		};
		public const string XmlHiverName = "HiverAICostFactor";
		public const string XmlHumanName = "HumanAICostFactor";
		public const string XmlLiirZuulName = "LiirZuulAICostFactor";
		public const string XmlMorrigiName = "MorrigiAICostFactor";
		public const string XmlTarkaName = "TarkaAICostFactor";
		public const string XmlZuulName = "ZuulAICostFactor";
		public const string XmlLoaName = "LoaAICostFactor";
		public float Hiver;
		public float Human;
		public float LiirZuul;
		public float Morrigi;
		public float Tarka;
		public float Zuul;
		public float Loa;

		public static IList<string> Factions
		{
			get
			{
				return (IList<string>)AICostFactors._factions;
			}
		}

		public float Faction(string faction)
		{
			switch (faction)
			{
				case "hiver":
					return this.Hiver;
				case "human":
					return this.Human;
				case "liir_zuul":
					return this.LiirZuul;
				case "morrigi":
					return this.Morrigi;
				case "tarkas":
					return this.Tarka;
				case "zuul":
					return this.Zuul;
				case "loa":
					return this.Loa;
				default:
					return 1f;
			}
		}

		public void SetFaction(string faction, float value)
		{
			switch (faction)
			{
				case "hiver":
					this.Hiver = value;
					break;
				case "human":
					this.Human = value;
					break;
				case "liir_zuul":
					this.LiirZuul = value;
					break;
				case "morrigi":
					this.Morrigi = value;
					break;
				case "tarkas":
					this.Tarka = value;
					break;
				case "zuul":
					this.Zuul = value;
					break;
				case "loa":
					this.Loa = value;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(faction));
			}
		}

		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.Hiver, "HiverAICostFactor", ref node);
			XmlHelper.AddNode((object)this.Human, "HumanAICostFactor", ref node);
			XmlHelper.AddNode((object)this.LiirZuul, "LiirZuulAICostFactor", ref node);
			XmlHelper.AddNode((object)this.Morrigi, "MorrigiAICostFactor", ref node);
			XmlHelper.AddNode((object)this.Tarka, "TarkaAICostFactor", ref node);
			XmlHelper.AddNode((object)this.Zuul, "ZuulAICostFactor", ref node);
			XmlHelper.AddNode((object)this.Loa, "LoaAICostFactor", ref node);
		}

		public void LoadFromXmlNode(XmlElement node)
		{
			this.Hiver = XmlHelper.GetDataOrDefault<float>(node["HiverAICostFactor"], 1f);
			this.Human = XmlHelper.GetDataOrDefault<float>(node["HumanAICostFactor"], 1f);
			this.LiirZuul = XmlHelper.GetDataOrDefault<float>(node["LiirZuulAICostFactor"], 1f);
			this.Morrigi = XmlHelper.GetDataOrDefault<float>(node["MorrigiAICostFactor"], 1f);
			this.Tarka = XmlHelper.GetDataOrDefault<float>(node["TarkaAICostFactor"], 1f);
			this.Zuul = XmlHelper.GetDataOrDefault<float>(node["ZuulAICostFactor"], 1f);
			this.Loa = XmlHelper.GetDataOrDefault<float>(node["LoaAICostFactor"], 1f);
		}
	}
}
