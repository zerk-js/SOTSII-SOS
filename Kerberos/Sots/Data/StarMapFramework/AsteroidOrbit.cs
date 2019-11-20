// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.StarMapFramework.AsteroidOrbit
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.StarMapFramework
{
	public class AsteroidOrbit : Orbit
	{
		public const string XmlAsteroidOrbitName = "Asteroid";
		public const string XmlDensityName = "Density";
		public const string XmlWidthName = "Width";
		public int? Density;
		public int? Width;

		public override string XmlName
		{
			get
			{
				return "Asteroid";
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			base.AttachToXmlNode(ref node);
			XmlHelper.AddNode((object)this.Density, "Density", ref node);
			XmlHelper.AddNode((object)this.Width, "Width", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			if (node == null)
				return;
			base.LoadFromXmlNode(node);
			this.Density = XmlHelper.GetData<int?>(node, "Density");
			this.Width = XmlHelper.GetData<int?>(node, "Width");
		}
	}
}
