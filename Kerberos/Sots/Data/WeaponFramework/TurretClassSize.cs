// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.WeaponFramework.TurretClassSize
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.WeaponFramework
{
	public class TurretClassSize : IXmlLoadSave
	{
		public string TurretSize = "";
		public string Turret = "";
		public string Barrel = "";
		public string Base = "";
		internal const string XmlTurretClassSizeName = "TurretClassSize";
		private const string XmlTurretSizeName = "TurretSize";
		private const string XmlTurretName = "Turret";
		private const string XmlBarrelName = "Barrel";
		private const string XmlBaseName = "Base";

		public string XmlName
		{
			get
			{
				return nameof(TurretClassSize);
			}
		}

		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.TurretSize, "TurretSize", ref node);
			XmlHelper.AddNode((object)this.Turret, "Turret", ref node);
			XmlHelper.AddNode((object)this.Barrel, "Barrel", ref node);
			XmlHelper.AddNode((object)this.Base, "Base", ref node);
		}

		public void LoadFromXmlNode(XmlElement node)
		{
			this.TurretSize = XmlHelper.GetData<string>(node, "TurretSize");
			this.Turret = XmlHelper.GetData<string>(node, "Turret");
			this.Barrel = XmlHelper.GetData<string>(node, "Barrel");
			this.Base = XmlHelper.GetData<string>(node, "Base");
		}
	}
}
