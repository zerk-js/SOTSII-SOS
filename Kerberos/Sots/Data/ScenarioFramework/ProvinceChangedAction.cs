// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.ProvinceChangedAction
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class ProvinceChangedAction : TriggerAction
	{
		internal const string XmlProvinceChangedActionName = "ProvinceChanged";
		private const string XmlProvinceIdName = "ProvinceId";
		private const string XmlNewPlayerName = "NewPlayer";
		public int ProvinceId;
		public int NewPlayer;

		public override string XmlName
		{
			get
			{
				return "ProvinceChanged";
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.ProvinceId, "ProvinceId", ref node);
			XmlHelper.AddNode((object)this.NewPlayer, "NewPlayer", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			this.ProvinceId = XmlHelper.GetData<int>(node, "ProvinceId");
			this.NewPlayer = XmlHelper.GetData<int>(node, "NewPlayer");
		}
	}
}
