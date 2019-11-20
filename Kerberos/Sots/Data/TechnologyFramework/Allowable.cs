// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.TechnologyFramework.Allowable
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.TechnologyFramework
{
	public class Allowable : IXmlLoadSave
	{
		public string Id = "";
		internal const string XmlAllowedTechName = "AllowedTech";
		private const string XmlIdName = "Id";
		private const string XmlIsPermanentName = "IsPermanent";
		private const string XmlHiverPercentName = "HiverPercent";
		private const string XmlHumanPercentName = "HumanPercent";
		private const string XmlLiirZuulPercentName = "LiirZuulPercent";
		private const string XmlMorrigiPercentName = "MorrigiPercent";
		private const string XmlTarkaPercentName = "TarkaPercent";
		private const string XmlZuulPercentName = "ZuulPercent";
		private const string XmlLoaPercentName = "LoaPercent";
		private const string XmlResearchPointsName = "ResearchPoints";
		public bool IsPermanent;
		public float HiverPercent;
		public float HumanPercent;
		public float LiirZuulPercent;
		public float MorrigiPercent;
		public float TarkaPercent;
		public float ZuulPercent;
		public float LoaPercent;
		public int ResearchPoints;

		public override string ToString()
		{
			return "Allows {" + this.Id + "} $" + (object)this.ResearchPoints;
		}

		public float GetFactionProbabilityPercentage(string faction)
		{
			if (this.IsPermanent)
				return 100f;
			switch (faction)
			{
				case "hiver":
					return this.HiverPercent;
				case "human":
					return this.HumanPercent;
				case "liir_zuul":
					return this.LiirZuulPercent;
				case "morrigi":
					return this.MorrigiPercent;
				case "tarkas":
					return this.TarkaPercent;
				case "zuul":
					return this.ZuulPercent;
				case "loa":
					return this.LoaPercent;
				default:
					return 0.0f;
			}
		}

		public string XmlName
		{
			get
			{
				return "AllowedTech";
			}
		}

		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.Id, "Id", ref node);
			XmlHelper.AddNode((object)this.IsPermanent, "IsPermanent", ref node);
			XmlHelper.AddNode((object)this.HiverPercent, "HiverPercent", ref node);
			XmlHelper.AddNode((object)this.HumanPercent, "HumanPercent", ref node);
			XmlHelper.AddNode((object)this.LiirZuulPercent, "LiirZuulPercent", ref node);
			XmlHelper.AddNode((object)this.MorrigiPercent, "MorrigiPercent", ref node);
			XmlHelper.AddNode((object)this.TarkaPercent, "TarkaPercent", ref node);
			XmlHelper.AddNode((object)this.ZuulPercent, "ZuulPercent", ref node);
			XmlHelper.AddNode((object)this.LoaPercent, "LoaPercent", ref node);
			XmlHelper.AddNode((object)this.ResearchPoints, "ResearchPoints", ref node);
		}

		public void LoadFromXmlNode(XmlElement node)
		{
			this.Id = XmlHelper.GetData<string>(node, "Id");
			this.IsPermanent = XmlHelper.GetData<bool>(node, "IsPermanent");
			this.HiverPercent = XmlHelper.GetData<float>(node, "HiverPercent");
			this.HumanPercent = XmlHelper.GetData<float>(node, "HumanPercent");
			this.LiirZuulPercent = XmlHelper.GetData<float>(node, "LiirZuulPercent");
			this.MorrigiPercent = XmlHelper.GetData<float>(node, "MorrigiPercent");
			this.TarkaPercent = XmlHelper.GetData<float>(node, "TarkaPercent");
			this.ZuulPercent = XmlHelper.GetData<float>(node, "ZuulPercent");
			this.LoaPercent = XmlHelper.GetData<float>(node, "LoaPercent");
			this.ResearchPoints = XmlHelper.GetData<int>(node, "ResearchPoints");
		}
	}
}
