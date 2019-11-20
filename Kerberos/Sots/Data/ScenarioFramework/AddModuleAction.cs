// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.AddModuleAction
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class AddModuleAction : TriggerAction
	{
		public string ModuleFile = "";
		internal const string XmlAddModuleActionName = "AddModule";
		private const string XmlPlayerName = "Player";
		private const string XmlModuleFileName = "ModuleFile";
		public int Player;

		public override string XmlName
		{
			get
			{
				return "AddModule";
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.Player, "Player", ref node);
			XmlHelper.AddNode((object)this.ModuleFile, "ModuleFile", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			this.Player = XmlHelper.GetData<int>(node, "Player");
			this.ModuleFile = XmlHelper.GetData<string>(node, "ModuleFile");
		}
	}
}
