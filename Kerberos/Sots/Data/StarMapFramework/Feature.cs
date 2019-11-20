// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.StarMapFramework.Feature
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Kerberos.Sots.Data.StarMapFramework
{
	public class Feature : IXmlLoadSave
	{
		public static Dictionary<string, Type> TypeMap = new Dictionary<string, Type>()
	{
	  {
		"Terrain",
		typeof (Terrain)
	  },
	  {
		"StellarBody",
		typeof (StellarBody)
	  },
	  {
		"System",
		typeof (StarSystem)
	  }
	};
		public Matrix LocalSpace = Matrix.Identity;
		public bool isVisible = true;
		private const string XmlNameName = "Name";
		private const string XmlLocalSpaceName = "LocalSpace";
		private const string XmlInheritObject = "Inherit";
		private const string XmlIsVisible = "IsVisible";
		private const char XmlMatrixSeperator = ',';
		public string Name;
		public string InheritObject;

		public virtual string XmlName
		{
			get
			{
				throw XmlHelper.NoXmlNameException;
			}
		}

		public virtual void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.Name, "Name", ref node);
			XmlHelper.AddNode((object)this.LocalSpace.ToString(','), "LocalSpace", ref node);
			XmlHelper.AddNode((object)this.InheritObject, "Inherit", ref node);
			XmlHelper.AddNode((object)this.isVisible, "IsVisible", ref node);
		}

		public virtual void LoadFromXmlNode(XmlElement node)
		{
			if (node == null)
				return;
			this.Name = XmlHelper.GetData<string>(node, "Name");
			this.LocalSpace = Matrix.Parse(XmlHelper.GetData<string>(node, "LocalSpace"), ',');
			this.InheritObject = XmlHelper.GetData<string>(node, "Inherit");
			this.isVisible = XmlHelper.GetDataOrDefault<bool>(node["IsVisible"], true);
		}
	}
}
