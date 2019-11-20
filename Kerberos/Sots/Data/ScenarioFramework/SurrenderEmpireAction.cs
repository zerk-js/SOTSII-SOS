// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.SurrenderEmpireAction
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class SurrenderEmpireAction : TriggerAction
	{
		internal const string XmlSurrenderEmpireActionName = "SurrenderEmpire";
		private const string XmlSurrenderingPlayerName = "SurrenderingPlayer";
		private const string XmlCapturingPlayerName = "CapturingPlayer";
		public int SurrenderingPlayer;
		public int CapturingPlayer;

		public override string XmlName
		{
			get
			{
				return "SurrenderEmpire";
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.SurrenderingPlayer, "SurrenderingPlayer", ref node);
			XmlHelper.AddNode((object)this.CapturingPlayer, "CapturingPlayer", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			this.SurrenderingPlayer = XmlHelper.GetData<int>(node, "SurrenderingPlayer");
			this.CapturingPlayer = XmlHelper.GetData<int>(node, "CapturingPlayer");
		}
	}
}
