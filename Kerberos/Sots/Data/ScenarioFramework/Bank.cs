// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.Bank
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class Bank : IXmlLoadSave
	{
		internal const string XmlBankName = "Bank";
		private const string XmlGUIDName = "GUID";
		private const string XmlNameName = "Name";
		private const string XmlMountSizeName = "MountSize";
		private const string XmlMountClassName = "MountClass";
		private const string XmlWeaponName = "Weapon";
		public string GUID;
		public string Name;
		public string MountSize;
		public string MountClass;
		public string Weapon;

		public string XmlName
		{
			get
			{
				return nameof(Bank);
			}
		}

		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.GUID, "GUID", ref node);
			XmlHelper.AddNode((object)this.Name, "Name", ref node);
			XmlHelper.AddNode((object)this.MountSize, "MountSize", ref node);
			XmlHelper.AddNode((object)this.MountClass, "MountClass", ref node);
			XmlHelper.AddNode((object)this.Weapon, "Weapon", ref node);
		}

		public void LoadFromXmlNode(XmlElement node)
		{
			this.GUID = XmlHelper.GetData<string>(node, "GUID");
			this.Name = XmlHelper.GetData<string>(node, "Name");
			this.MountSize = XmlHelper.GetData<string>(node, "MountSize");
			this.MountClass = XmlHelper.GetData<string>(node, "MountClass");
			this.Weapon = XmlHelper.GetData<string>(node, "Weapon");
		}
	}
}
