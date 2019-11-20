// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.ShipFramework.LogicalBank
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.WeaponFramework;
using System;

namespace Kerberos.Sots.ShipFramework
{
	internal class LogicalBank
	{
		public ShipSectionAsset Section;
		public LogicalModule Module;
		public WeaponEnums.WeaponSizes TurretSize;
		public WeaponEnums.TurretClasses TurretClass;
		public int FrameX;
		public int FrameY;
		public Guid GUID;
		public string DefaultWeaponName;

		public override string ToString()
		{
			return this.GUID.ToString() + "," + (object)this.TurretClass + "," + (object)this.TurretSize;
		}
	}
}
