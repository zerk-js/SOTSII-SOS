// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.PlanetAddedAction
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.StarMapFramework;
using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class PlanetAddedAction : TriggerAction
	{
		internal const string XmlPlanetAddedActionName = "PlanetAdded";
		private const string XmlSystemIdName = "SystemId";
		private const string XmlNewPlanetName = "NewPlanet";
		public int SystemId;
		public PlanetOrbit NewPlanet;

		public override string XmlName
		{
			get
			{
				return "PlanetAdded";
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.SystemId, "SystemId", ref node);
			XmlHelper.AddNode((IXmlLoadSave)this.NewPlanet, "NewPlanet", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			this.SystemId = XmlHelper.GetData<int>(node, "SystemId");
			this.NewPlanet.LoadFromXmlNode(node["NewPlanet"]);
		}
	}
}
