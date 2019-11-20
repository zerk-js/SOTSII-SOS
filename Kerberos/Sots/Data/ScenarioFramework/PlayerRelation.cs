// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.PlayerRelation
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Strategy;
using System;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class PlayerRelation : IXmlLoadSave
	{
		public DiplomacyState DiplomacyState = DiplomacyState.UNKNOWN;
		internal const string XmlPlayerRelationName = "PlayerRelation";
		private const string XmlPlayerName = "Player";
		private const string XmlRelationsName = "Relations";
		private const string XmlDiplomacyStateName = "DiplomacyState";
		public int Player;
		public int Relations;

		public string XmlName
		{
			get
			{
				return nameof(PlayerRelation);
			}
		}

		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.Player, "Player", ref node);
			XmlHelper.AddNode((object)this.Relations, "Relations", ref node);
			XmlHelper.AddNode((object)this.DiplomacyState, "DiplomacyState", ref node);
		}

		public void LoadFromXmlNode(XmlElement node)
		{
			this.Player = XmlHelper.GetData<int>(node, "Player");
			this.Relations = XmlHelper.GetData<int>(node, "Relations");
			Enum.TryParse<DiplomacyState>(XmlHelper.GetData<string>(node, "DiplomacyState"), out this.DiplomacyState);
		}
	}
}
