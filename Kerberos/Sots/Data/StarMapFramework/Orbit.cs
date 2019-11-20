// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.StarMapFramework.Orbit
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Collections.Generic;
using System.Xml;

namespace Kerberos.Sots.Data.StarMapFramework
{
	public class Orbit : IXmlLoadSave
	{
		public static Dictionary<string, System.Type> TypeMap = new Dictionary<string, System.Type>()
	{
	  {
		"Empty",
		typeof (EmptyOrbit)
	  },
	  {
		"Star",
		typeof (StarOrbit)
	  },
	  {
		"Artifact",
		typeof (ArtifactOrbit)
	  },
	  {
		"GasGiantSmall",
		typeof (GasGiantSmallOrbit)
	  },
	  {
		"GasGiantLarge",
		typeof (GasGiantLargeOrbit)
	  },
	  {
		"Moon",
		typeof (MoonOrbit)
	  },
	  {
		"PlanetaryRing",
		typeof (PlanetaryRingOrbit)
	  },
	  {
		"Planet",
		typeof (PlanetOrbit)
	  },
	  {
		"Asteroid",
		typeof (AsteroidOrbit)
	  }
	};
		public string Name = "";
		public string Type = "";
		public string Parent = "";
		public const string XmlOrbitName = "Orbit";
		public const string XmlNameName = "Name";
		public const string XmlTypeName = "Type";
		public const string XmlParentName = "Parent";
		public const string XmlOrbitNumberName = "OrbitNumber";
		public const string XmlEccentricityName = "Eccentricity";
		public const string XmlInclinationName = "Inclination";
		public float? Eccentricity;
		public float? Inclination;
		public int OrbitNumber;

		public virtual string XmlName
		{
			get
			{
				return nameof(Orbit);
			}
		}

		public virtual void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.Name, "Name", ref node);
			XmlHelper.AddNode((object)this.Type, "Type", ref node);
			XmlHelper.AddNode((object)this.Parent, "Parent", ref node);
			XmlHelper.AddNode((object)this.Eccentricity, "Eccentricity", ref node);
			XmlHelper.AddNode((object)this.Inclination, "Inclination", ref node);
			XmlHelper.AddNode((object)this.OrbitNumber, "OrbitNumber", ref node);
		}

		public virtual void LoadFromXmlNode(XmlElement node)
		{
			if (node == null)
				return;
			this.Name = XmlHelper.GetData<string>(node, "Name");
			this.Type = XmlHelper.GetData<string>(node, "Type");
			this.Parent = XmlHelper.GetData<string>(node, "Parent");
			this.Eccentricity = XmlHelper.GetData<float?>(node, "Eccentricity");
			this.Inclination = XmlHelper.GetData<float?>(node, "Inclination");
			this.OrbitNumber = XmlHelper.GetData<int>(node, "OrbitNumber");
		}
	}
}
