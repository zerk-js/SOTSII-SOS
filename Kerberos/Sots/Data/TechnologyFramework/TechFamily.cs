// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.TechnologyFramework.TechFamily
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.IO;
using System.Xml;

namespace Kerberos.Sots.Data.TechnologyFramework
{
	public class TechFamily : IXmlLoadSave
	{
		public string Name = "";
		public string Id = "";
		public string Icon = "";
		internal const string XmlFamilyName = "Family";
		private const string XmlNameName = "Name";
		private const string XmlIdName = "Id";
		private const string XmlIconName = "Icon";
		private const string XmlFactionDefinedName = "FactionDefined";
		public bool FactionDefined;

		public string GetProperIconPath()
		{
			return Path.Combine("Tech", this.Icon.Replace(".\\", string.Empty));
		}

		public string XmlName
		{
			get
			{
				return "Family";
			}
		}

		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.Name, "Name", ref node);
			XmlHelper.AddNode((object)this.Id, "Id", ref node);
			XmlHelper.AddNode((object)this.Icon, "Icon", ref node);
			XmlHelper.AddNode((object)this.FactionDefined, "FactionDefined", ref node);
		}

		public void LoadFromXmlNode(XmlElement node)
		{
			this.Name = XmlHelper.GetData<string>(node, "Name");
			this.Id = XmlHelper.GetData<string>(node, "Id");
			this.Icon = XmlHelper.GetData<string>(node, "Icon");
			this.FactionDefined = XmlHelper.GetData<bool>(node, "FactionDefined");
		}
	}
}
