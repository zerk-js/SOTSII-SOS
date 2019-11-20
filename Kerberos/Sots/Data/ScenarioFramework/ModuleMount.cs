// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.ModuleMount
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class ModuleMount : IXmlLoadSave
	{
		internal const string XmlModuleName = "Module";
		private const string XmlNameName = "Name";
		private const string XmlNodeName = "Node";
		public string NodeName;
		public string ModuleName;

		public string XmlName
		{
			get
			{
				return "Module";
			}
		}

		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.NodeName, "Name", ref node);
			XmlHelper.AddNode((object)this.ModuleName, "Node", ref node);
		}

		public void LoadFromXmlNode(XmlElement node)
		{
			this.NodeName = XmlHelper.GetData<string>(node, "Name");
			this.ModuleName = XmlHelper.GetData<string>(node, "Node");
		}
	}
}
