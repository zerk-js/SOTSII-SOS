// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ShipFramework.ShipOptionGroup
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Collections.Generic;
using System.Xml;

namespace Kerberos.Sots.Data.ShipFramework
{
	public class ShipOptionGroup : IXmlLoadSave
	{
		internal static readonly string XmlNameShipOptionGroup = nameof(ShipOptionGroup);
		private static readonly string XmlNameShipOptions = nameof(ShipOptions);
		private static readonly string XmlNameShipOption = "ShipOption";
		public List<ShipOption> ShipOptions = new List<ShipOption>();

		public string XmlName
		{
			get
			{
				return ShipOptionGroup.XmlNameShipOptionGroup;
			}
		}

		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddObjectCollectionNode((IEnumerable<IXmlLoadSave>)this.ShipOptions, ShipOptionGroup.XmlNameShipOptions, ShipOptionGroup.XmlNameShipOption, ref node);
		}

		public void LoadFromXmlNode(XmlElement node)
		{
			this.ShipOptions = XmlHelper.GetDataObjectCollection<ShipOption>(node, ShipOptionGroup.XmlNameShipOptions, ShipOptionGroup.XmlNameShipOption);
		}
	}
}
