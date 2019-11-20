// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.TechnologyFramework.TechTree
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.GenericFramework;
using Kerberos.Sots.Data.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Kerberos.Sots.Data.TechnologyFramework
{
	public class TechTree : IXmlLoadSave
	{
		public List<Tech> Technologies = new List<Tech>();
		public List<BasicNameField> TechGroups = new List<BasicNameField>();
		public List<TechFamily> TechFamilies = new List<TechFamily>();
		internal const string XmlTechTreeName = "TechTree";
		private const string XmlTechnologiesName = "Technologies";
		private const string XmlTechGroupsName = "TechGroups";
		private const string XmlTechFamiliesName = "TechFamilies";
		private const string XmlGroupName = "Group";

		public Kerberos.Sots.Data.TechnologyFramework.TechFamilies GetTechFamilyEnumFromName(
		  string techFamilyName)
		{
			return (Kerberos.Sots.Data.TechnologyFramework.TechFamilies)Enum.Parse(typeof(Kerberos.Sots.Data.TechnologyFramework.TechFamilies), this.TechFamilies.First<TechFamily>((Func<TechFamily, bool>)(x => x.Id == techFamilyName)).Name);
		}

		public Kerberos.Sots.Data.TechnologyFramework.TechFamilies GetTechFamilyEnum(
		  Tech tech)
		{
			return this.GetTechFamilyEnumFromName(tech.Family);
		}

		public Tech this[string techId]
		{
			get
			{
				return this.Technologies.First<Tech>((Func<Tech, bool>)(x => techId == x.Id));
			}
		}

		private bool IsRoot(Tech value)
		{
			return !this.Technologies.Any<Tech>((Func<Tech, bool>)(x => x.Allows.Any<Allowable>((Func<Allowable, bool>)(y => y.Id == value.Id))));
		}

		public Tech[] GetRoots()
		{
			return this.Technologies.Where<Tech>((Func<Tech, bool>)(x => this.IsRoot(x))).ToArray<Tech>();
		}

		public string XmlName
		{
			get
			{
				return nameof(TechTree);
			}
		}

		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddObjectCollectionNode((IEnumerable<IXmlLoadSave>)this.Technologies, "Technologies", ref node);
			XmlHelper.AddObjectCollectionNode((IEnumerable<IXmlLoadSave>)this.TechGroups, "TechGroups", "Group", ref node);
			XmlHelper.AddObjectCollectionNode((IEnumerable<IXmlLoadSave>)this.TechFamilies, "TechFamilies", "Family", ref node);
		}

		public void LoadFromXmlNode(XmlElement node)
		{
			this.Technologies = XmlHelper.GetDataObjectCollection<Tech>(node, "Technologies", "Tech");
			this.TechGroups = XmlHelper.GetDataObjectCollection<BasicNameField>(node, "TechGroups", "Group");
			this.TechFamilies = XmlHelper.GetDataObjectCollection<TechFamily>(node, "TechFamilies", "Family");
		}
	}
}
