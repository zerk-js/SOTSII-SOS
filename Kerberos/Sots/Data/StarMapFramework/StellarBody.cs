// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.StarMapFramework.StellarBody
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.StarMapFramework
{
	public class StellarBody : Feature
	{
		internal const string XmlStellarBodyName = "StellarBody";
		private const string XmlModelName = "Model";
		public string Model;

		public override string XmlName
		{
			get
			{
				return nameof(StellarBody);
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			base.AttachToXmlNode(ref node);
			XmlHelper.AddNode((object)this.Model, "Model", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			if (node == null)
				return;
			base.LoadFromXmlNode(node);
			this.Model = XmlHelper.GetData<string>(node, "Model");
		}
	}
}
