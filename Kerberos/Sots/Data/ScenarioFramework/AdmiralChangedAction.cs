// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.AdmiralChangedAction
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class AdmiralChangedAction : TriggerAction
	{
		public Admiral OldAdmiral = new Admiral();
		public Admiral NewAdmiral = new Admiral();
		internal const string XmlAdmiralChangedActionName = "AdmiralChanged";
		private const string XmlOldAdmiralName = "OldAdmiral";
		private const string XmlNewAdmiralName = "NewAdmiral";
		private const string XmlNewPlayerName = "NewPlayer";
		public int NewPlayer;

		public override string XmlName
		{
			get
			{
				return "AdmiralChanged";
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((IXmlLoadSave)this.OldAdmiral, "OldAdmiral", ref node);
			XmlHelper.AddNode((IXmlLoadSave)this.NewAdmiral, "NewAdmiral", ref node);
			XmlHelper.AddNode((object)this.NewPlayer, "NewPlayer", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			this.OldAdmiral.LoadFromXmlNode(node["OldAdmiral"]);
			this.NewAdmiral.LoadFromXmlNode(node["NewAdmiral"]);
			this.NewPlayer = XmlHelper.GetData<int>(node, "NewPlayer");
		}
	}
}
