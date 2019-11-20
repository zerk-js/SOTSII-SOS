// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.Station
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Collections.Generic;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class Station : IXmlLoadSave
	{
		public List<ModuleMount> Modules = new List<ModuleMount>();
		internal const string XmlStationName = "Station";
		private const string XmlNameName = "Name";
		private const string XmlTypeName = "Type";
		private const string XmlStageName = "Stage";
		private const string XmlLocationName = "Location";
		private const string XmlOrbitName = "Orbit";
		private const string XmlModulesName = "Modules";
		public string Name;
		public string Type;
		public int Stage;
		public int Location;
		public int Orbit;

		public string XmlName
		{
			get
			{
				return nameof(Station);
			}
		}

		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.Name, "Name", ref node);
			XmlHelper.AddNode((object)this.Type, "Type", ref node);
			XmlHelper.AddNode((object)this.Stage, "Stage", ref node);
			XmlHelper.AddNode((object)this.Location, "Location", ref node);
			XmlHelper.AddNode((object)this.Orbit, "Orbit", ref node);
			XmlHelper.AddObjectCollectionNode((IEnumerable<IXmlLoadSave>)this.Modules, "Modules", "Module", ref node);
		}

		public void LoadFromXmlNode(XmlElement node)
		{
			this.Name = XmlHelper.GetData<string>(node, "Name");
			this.Type = XmlHelper.GetData<string>(node, "Type");
			this.Stage = XmlHelper.GetData<int>(node, "Stage");
			this.Location = XmlHelper.GetData<int>(node, "Location");
			this.Orbit = XmlHelper.GetData<int>(node, "Orbit");
			this.Modules = XmlHelper.GetDataObjectCollection<ModuleMount>(node, "Modules", "Module");
		}
	}
}
