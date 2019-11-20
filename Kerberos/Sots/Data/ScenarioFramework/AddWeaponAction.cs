// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.AddWeaponAction
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class AddWeaponAction : TriggerAction
	{
		public string WeaponFile = "";
		internal const string XmlAddWeaponActionName = "AddWeapon";
		private const string XmlPlayerName = "Player";
		private const string XmlWeaponFileName = "WeaponFile";
		public int Player;

		public override string XmlName
		{
			get
			{
				return "AddWeapon";
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.Player, "Player", ref node);
			XmlHelper.AddNode((object)this.WeaponFile, "WeaponFile", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			this.Player = XmlHelper.GetData<int>(node, "Player");
			this.WeaponFile = XmlHelper.GetData<string>(node, "WeaponFile");
		}
	}
}
