// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ShipFramework.Bank
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Data.Xml;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Kerberos.Sots.Data.ShipFramework
{
	public class Bank : IXmlLoadSave
	{
		internal static readonly string XmlNameBank = nameof(Bank);
		private static readonly string XmlNameId = nameof(Id);
		private static readonly string XmlNameTurretSize = "TurretSize";
		private static readonly string XmlNameTurretClass = "TurretClass";
		private static readonly string XmlNameWeaponGroup = nameof(WeaponGroup);
		private static readonly string XmlNameDefaultWeapon = nameof(DefaultWeapon);
		private static readonly string XmlNameMounts = nameof(Mounts);
		private static readonly string XmlNameFrameX = nameof(FrameX);
		private static readonly string XmlNameFrameY = nameof(FrameY);
		public string Id = string.Empty;
		public string Size = WeaponEnums.WeaponSizes.Light.ToString();
		public string WeaponGroup = "";
		public string DefaultWeapon = "";
		public List<Mount> Mounts = new List<Mount>();
		public string Class;
		public int FrameX;
		public int FrameY;

		public Bank()
		{
			if (!ToolEnvironment.IsRunningInTool)
				return;
			this.Id = Guid.NewGuid().ToString();
		}

		public string XmlName
		{
			get
			{
				return Bank.XmlNameBank;
			}
		}

		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.Id, Bank.XmlNameId, ref node);
			XmlHelper.AddNode((object)this.Size, Bank.XmlNameTurretSize, ref node);
			XmlHelper.AddNode((object)this.Class, Bank.XmlNameTurretClass, ref node);
			XmlHelper.AddNode((object)this.WeaponGroup, Bank.XmlNameWeaponGroup, ref node);
			XmlHelper.AddNode((object)this.DefaultWeapon, Bank.XmlNameDefaultWeapon, ref node);
			XmlHelper.AddNode((object)this.FrameX, Bank.XmlNameFrameX, ref node);
			XmlHelper.AddNode((object)this.FrameY, Bank.XmlNameFrameY, ref node);
			XmlHelper.AddObjectCollectionNode((IEnumerable<IXmlLoadSave>)this.Mounts, Bank.XmlNameMounts, Mount.XmlNameMount, ref node);
		}

		public void LoadFromXmlNode(XmlElement node)
		{
			this.Id = XmlHelper.GetData<string>(node, Bank.XmlNameId);
			this.Size = XmlHelper.GetData<string>(node, Bank.XmlNameTurretSize);
			this.Class = XmlHelper.GetData<string>(node, Bank.XmlNameTurretClass);
			this.WeaponGroup = XmlHelper.GetData<string>(node, Bank.XmlNameWeaponGroup);
			this.DefaultWeapon = XmlHelper.GetData<string>(node, Bank.XmlNameDefaultWeapon);
			this.FrameX = XmlHelper.GetData<int>(node, Bank.XmlNameFrameX);
			this.FrameY = XmlHelper.GetData<int>(node, Bank.XmlNameFrameY);
			this.Mounts = XmlHelper.GetDataObjectCollection<Mount>(node, Bank.XmlNameMounts, Mount.XmlNameMount);
			if (!ToolEnvironment.IsRunningInTool || !string.IsNullOrEmpty(this.Id))
				return;
			this.Id = Guid.NewGuid().ToString();
		}
	}
}
