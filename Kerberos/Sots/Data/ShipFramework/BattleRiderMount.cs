// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ShipFramework.BattleRiderMount
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Collections.Generic;
using System.Xml;

namespace Kerberos.Sots.Data.ShipFramework
{
	public class BattleRiderMount : IXmlLoadSave
	{
		internal static readonly string XmlNameBattleRiderMount = nameof(BattleRiderMount);
		private static readonly string XmlNameNodeName = nameof(NodeName);
		private static readonly string XmlNameAllowableTypes = nameof(AllowableTypes);
		private static readonly string XmlNameBattleRiderType = "BattleRiderType";
		public string NodeName = "";
		public List<BattleRiderType> AllowableTypes = new List<BattleRiderType>();

		public string XmlName
		{
			get
			{
				return BattleRiderMount.XmlNameBattleRiderMount;
			}
		}

		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.NodeName, BattleRiderMount.XmlNameNodeName, ref node);
			XmlHelper.AddObjectCollectionNode((IEnumerable<IXmlLoadSave>)this.AllowableTypes, BattleRiderMount.XmlNameAllowableTypes, BattleRiderMount.XmlNameBattleRiderType, ref node);
		}

		public void LoadFromXmlNode(XmlElement node)
		{
			this.NodeName = XmlHelper.GetData<string>(node, BattleRiderMount.XmlNameNodeName);
			this.AllowableTypes = XmlHelper.GetDataObjectCollection<BattleRiderType>(node, BattleRiderMount.XmlNameAllowableTypes, BattleRiderMount.XmlNameBattleRiderType);
		}
	}
}
