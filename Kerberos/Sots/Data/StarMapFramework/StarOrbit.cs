﻿// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.StarMapFramework.StarOrbit
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System.Xml;

namespace Kerberos.Sots.Data.StarMapFramework
{
	public class StarOrbit : Orbit
	{
		public const string XmlStarOrbitName = "Star";

		public string StellarClass { get; set; }

		public override string XmlName
		{
			get
			{
				return "Star";
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			base.AttachToXmlNode(ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			if (node == null)
				return;
			base.LoadFromXmlNode(node);
		}
	}
}
