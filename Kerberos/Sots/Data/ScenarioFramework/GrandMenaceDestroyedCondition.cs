// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.GrandMenaceDestroyedCondition
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class GrandMenaceDestroyedCondition : TriggerCondition
	{
		internal const string XmlGrandMenaceDestroyedConditionName = "GrandMenaceDestroyed";
		private const string XmlMenaceIdName = "MenaceId";
		public int MenaceId;

		public override string XmlName
		{
			get
			{
				return "GrandMenaceDestroyed";
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.MenaceId, "MenaceId", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			this.MenaceId = XmlHelper.GetData<int>(node, "MenaceId");
		}
	}
}
