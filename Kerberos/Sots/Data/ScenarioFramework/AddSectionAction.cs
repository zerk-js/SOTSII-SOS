// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.AddSectionAction
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class AddSectionAction : TriggerAction
	{
		public string SectionFile = "";
		internal const string XmlAddSectionActionName = "AddSection";
		private const string XmlPlayerName = "Player";
		private const string XmlSectionFileName = "SectionFile";
		public int Player;

		public override string XmlName
		{
			get
			{
				return "AddSection";
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.Player, "Player", ref node);
			XmlHelper.AddNode((object)this.SectionFile, "SectionFile", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			this.Player = XmlHelper.GetData<int>(node, "Player");
			this.SectionFile = XmlHelper.GetData<string>(node, "SectionFile");
		}
	}
}
