// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.ShipFramework.LogicalTurretHousing
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.WeaponFramework;

namespace Kerberos.Sots.ShipFramework
{
	internal class LogicalTurretHousing
	{
		public WeaponEnums.WeaponSizes MountSize;
		public WeaponEnums.WeaponSizes WeaponSize;
		public WeaponEnums.TurretClasses Class;
		public float TrackSpeed;
		public string ModelName;
		public string BaseModelName;

		public override string ToString()
		{
			return ((int)this.Class).ToString() + "," + (object)this.MountSize + "," + (object)this.WeaponSize;
		}
	}
}
