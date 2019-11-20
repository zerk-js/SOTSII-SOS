// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ShipFramework.WeaponGroup
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ShipFramework
{
	public class WeaponGroup : IXmlLoadSave
	{
		public string Name = "";
		internal const string XmlWeaponGroupName = "WeaponGroup";
		private const string XmlNameName = "Name";

		public string XmlName
		{
			get
			{
				return nameof(WeaponGroup);
			}
		}

		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.Name, "Name", ref node);
		}

		public void LoadFromXmlNode(XmlElement node)
		{
			this.Name = XmlHelper.GetData<string>(node, "Name");
		}
	}
}
