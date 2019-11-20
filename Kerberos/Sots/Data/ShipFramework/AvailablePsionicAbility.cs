// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ShipFramework.AvailablePsionicAbility
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ShipFramework
{
	public class AvailablePsionicAbility : IXmlLoadSave
	{
		public string Name = "";
		internal const string XmlAvailablePsionicAbilityName = "PsionicAbility";
		private const string XmlNameName = "Name";
		private const string XmlModifierName = "Modifier";
		public float Modifier;

		public string XmlName
		{
			get
			{
				return "PsionicAbility";
			}
		}

		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.Name, "Name", ref node);
			XmlHelper.AddNode((object)this.Modifier, "Modifier", ref node);
		}

		public void LoadFromXmlNode(XmlElement node)
		{
			this.Name = XmlHelper.GetData<string>(node, "Name");
			this.Modifier = XmlHelper.GetDataOrDefault<float>(node["Modifier"], 1f);
		}
	}
}
