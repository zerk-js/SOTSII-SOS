// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.AddScalarToScalarAction
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class AddScalarToScalarAction : TriggerAction
	{
		public string ScalarToAdd = "";
		public string ScalarAddedTo = "";
		internal const string XmlAddScalarToScalarActionName = "AddScalarToScalarAction";
		private const string XmlScalarToAddName = "ScalarToAdd";
		private const string XmlScalarAddedToName = "ScalarAddedTo";

		public override string XmlName
		{
			get
			{
				return nameof(AddScalarToScalarAction);
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.ScalarToAdd, "ScalarToAdd", ref node);
			XmlHelper.AddNode((object)this.ScalarAddedTo, "ScalarAddedTo", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			this.ScalarToAdd = XmlHelper.GetData<string>(node, "ScalarToAdd");
			this.ScalarAddedTo = XmlHelper.GetData<string>(node, "ScalarAddedTo");
		}
	}
}
