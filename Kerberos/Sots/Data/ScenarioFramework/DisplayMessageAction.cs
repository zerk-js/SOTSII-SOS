// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.DisplayMessageAction
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class DisplayMessageAction : TriggerAction
	{
		public string Message = "";
		internal const string XmlDisplayMessageActionName = "DisplayMessage";
		private const string XmlMessageName = "Message";

		public override string XmlName
		{
			get
			{
				return "DisplayMessage";
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.Message, "Message", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			this.Message = XmlHelper.GetData<string>(node, "Message");
		}
	}
}
