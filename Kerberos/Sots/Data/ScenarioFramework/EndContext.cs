// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.EndContext
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class EndContext : TriggerContext
	{
		internal const string XmlEndContextName = "EndContext";
		private const string XmlEndTurnName = "EndTurn";
		public int EndTurn;

		public override string XmlName
		{
			get
			{
				return nameof(EndContext);
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.EndTurn, "EndTurn", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			this.EndTurn = XmlHelper.GetData<int>(node, "EndTurn");
		}
	}
}
