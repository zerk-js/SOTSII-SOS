// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.StarMapFramework.Province
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.StarMapFramework
{
	public class Province : IXmlLoadSave
	{
		public string Name = "";
		internal const string XmlProvinceName = "Province";
		internal const string XmlProvinceIdName = "Id";
		internal const string XmlNameName = "Name";
		internal const string XmlCapitalIdName = "CapitalId";
		internal const string XmlPlayerName = "Player";
		public int? Id;
		public int CapitalID;
		public int Player;

		public virtual string XmlName
		{
			get
			{
				return nameof(Province);
			}
		}

		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.Id, "Id", ref node);
			XmlHelper.AddNode((object)this.Name, "Name", ref node);
			XmlHelper.AddNode((object)this.CapitalID, "CapitalId", ref node);
			XmlHelper.AddNode((object)this.Player, "Player", ref node);
		}

		public void LoadFromXmlNode(XmlElement node)
		{
			if (node == null)
				return;
			this.Id = XmlHelper.GetData<int?>(node, "Id");
			this.Name = XmlHelper.GetData<string>(node, "Name");
			this.CapitalID = XmlHelper.GetData<int>(node, "CapitalId");
			this.Player = XmlHelper.GetData<int>(node, "Player");
		}
	}
}
