// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ShipFramework.ShipOption
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.GenericFramework;
using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ShipFramework
{
	public class ShipOption : BasicNameField, IXmlLoadSave
	{
		private static readonly string XmlNameAvailableByDefault = nameof(AvailableByDefault);
		public bool AvailableByDefault;

		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.AvailableByDefault, ShipOption.XmlNameAvailableByDefault, ref node);
			base.AttachToXmlNode(ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			this.AvailableByDefault = XmlHelper.GetData<bool>(node, ShipOption.XmlNameAvailableByDefault);
			base.LoadFromXmlNode(node);
		}
	}
}
