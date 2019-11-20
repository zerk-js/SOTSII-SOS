// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ShipFramework.Mount
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ShipFramework
{
	public class Mount : IXmlLoadSave
	{
		internal static readonly string XmlNameMount = nameof(Mount);
		private static readonly string XmlNameNodeName = nameof(NodeName);
		private static readonly string XmlNameTurretOverloadName = nameof(TurretOverload);
		private static readonly string XmlNameBarrelOverloadName = nameof(BarrelOverload);
		private static readonly string XmlNameBaseOverloadName = nameof(BaseOverload);
		private static readonly string XmlNameYawMin = nameof(YawMin);
		private static readonly string XmlNameYawMax = nameof(YawMax);
		private static readonly string XmlNamePitchMin = nameof(PitchMin);
		private static readonly string XmlNamePitchMax = nameof(PitchMax);
		private static readonly string XmlNameSectionFireAnimation = nameof(SectionFireAnimation);
		private static readonly string XmlNameSectionReloadAnimation = nameof(SectionReloadAnimation);
		public string NodeName = "";
		public string TurretOverload = "";
		public string BarrelOverload = "";
		public string BaseOverload = "";
		public string SectionFireAnimation = "";
		public string SectionReloadAnimation = "";
		public float YawMin;
		public float YawMax;
		public float PitchMin;
		public float PitchMax;

		public string XmlName
		{
			get
			{
				return Mount.XmlNameMount;
			}
		}

		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.NodeName, Mount.XmlNameNodeName, ref node);
			XmlHelper.AddNode((object)this.TurretOverload, Mount.XmlNameTurretOverloadName, ref node);
			XmlHelper.AddNode((object)this.BarrelOverload, Mount.XmlNameBarrelOverloadName, ref node);
			XmlHelper.AddNode((object)this.BaseOverload, Mount.XmlNameBaseOverloadName, ref node);
			XmlHelper.AddNode((object)this.YawMin, Mount.XmlNameYawMin, ref node);
			XmlHelper.AddNode((object)this.YawMax, Mount.XmlNameYawMax, ref node);
			XmlHelper.AddNode((object)this.PitchMin, Mount.XmlNamePitchMin, ref node);
			XmlHelper.AddNode((object)this.PitchMax, Mount.XmlNamePitchMax, ref node);
			XmlHelper.AddNode((object)this.SectionFireAnimation, Mount.XmlNameSectionFireAnimation, ref node);
			XmlHelper.AddNode((object)this.SectionReloadAnimation, Mount.XmlNameSectionReloadAnimation, ref node);
		}

		public void LoadFromXmlNode(XmlElement node)
		{
			this.NodeName = XmlHelper.GetData<string>(node, Mount.XmlNameNodeName);
			this.TurretOverload = XmlHelper.GetData<string>(node, Mount.XmlNameTurretOverloadName);
			this.BarrelOverload = XmlHelper.GetData<string>(node, Mount.XmlNameBarrelOverloadName);
			this.BaseOverload = XmlHelper.GetData<string>(node, Mount.XmlNameBaseOverloadName);
			this.YawMin = XmlHelper.GetData<float>(node, Mount.XmlNameYawMin);
			this.YawMax = XmlHelper.GetData<float>(node, Mount.XmlNameYawMax);
			this.PitchMin = XmlHelper.GetData<float>(node, Mount.XmlNamePitchMin);
			this.PitchMax = XmlHelper.GetData<float>(node, Mount.XmlNamePitchMax);
			this.SectionFireAnimation = XmlHelper.GetData<string>(node, Mount.XmlNameSectionFireAnimation);
			this.SectionReloadAnimation = XmlHelper.GetData<string>(node, Mount.XmlNameSectionReloadAnimation);
		}
	}
}
