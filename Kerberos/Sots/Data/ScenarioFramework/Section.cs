// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.Section
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Collections.Generic;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class Section : IXmlLoadSave
	{
		public string SectionFile = "";
		public List<Bank> Banks = new List<Bank>();
		public List<ModuleMount> Modules = new List<ModuleMount>();
		internal const string XmlSectionName = "Section";
		private const string XmlSectionFileName = "SectionFile";
		private const string XmlBanksName = "Banks";
		private const string XmlModulesName = "Modules";

		public string XmlName
		{
			get
			{
				return nameof(Section);
			}
		}

		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.SectionFile, "SectionFile", ref node);
			XmlHelper.AddObjectCollectionNode((IEnumerable<IXmlLoadSave>)this.Banks, "Banks", ref node);
			XmlHelper.AddObjectCollectionNode((IEnumerable<IXmlLoadSave>)this.Modules, "Modules", "Module", ref node);
		}

		public void LoadFromXmlNode(XmlElement node)
		{
			this.SectionFile = XmlHelper.GetData<string>(node, "SectionFile");
			this.Banks = XmlHelper.GetDataObjectCollection<Bank>(node, "Banks", "Bank");
			this.Modules = XmlHelper.GetDataObjectCollection<ModuleMount>(node, "Modules", "Module");
		}
	}
}
