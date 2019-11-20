// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.PointPerColonyDeathAction
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class PointPerColonyDeathAction : TriggerAction
	{
		public string ScalarName = "";
		internal const string XmlPointPerColonyDeathActionName = "PointPerColonyDeathAction";
		private const string XmlScalarNameName = "ScalarName";
		private const string XmlAmountPerColonyName = "AmountPerColony";
		public float AmountPerColony;

		public override string XmlName
		{
			get
			{
				return nameof(PointPerColonyDeathAction);
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.ScalarName, "ScalarName", ref node);
			XmlHelper.AddNode((object)this.AmountPerColony, "AmountPerColony", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			this.ScalarName = XmlHelper.GetData<string>(node, "ScalarName");
			this.AmountPerColony = XmlHelper.GetData<float>(node, "AmountPerColony");
		}
	}
}
