// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.StationInfo
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.ShipFramework;

namespace Kerberos.Sots.Data
{
	internal class StationInfo
	{
		public OrbitalObjectInfo OrbitalObjectInfo;
		public int PlayerID;
		public DesignInfo DesignInfo;
		public int WarehousedGoods;
		public int ShipID;

		public int OrbitalObjectID
		{
			get
			{
				return this.OrbitalObjectInfo.ID;
			}
		}

		public int ID
		{
			get
			{
				return this.OrbitalObjectInfo.ID;
			}
		}

		public float GetBaseStratSensorRange()
		{
			ShipSectionAsset shipSectionAsset = this.DesignInfo.DesignSections[0].ShipSectionAsset;
			if (shipSectionAsset != null)
				return shipSectionAsset.StrategicSensorRange;
			return 0.0f;
		}
	}
}
