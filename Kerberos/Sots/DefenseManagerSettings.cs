// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.DefenseManagerSettings
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.ShipFramework;

namespace Kerberos.Sots
{
	internal struct DefenseManagerSettings
	{
		public int SDBCPCost;
		public int MineLayerCPCost;
		public int PoliceCPCost;
		public int ScanSatCPCost;
		public int DroneSatCPCost;
		public int TorpSatCPCost;
		public int BRSatCPCost;
		public int MonitorSatCPCost;
		public int MissileSatCPCost;

		private int GetPlatformCPCost(PlatformTypes type)
		{
			switch (type)
			{
				case PlatformTypes.dronesat:
					return this.DroneSatCPCost;
				case PlatformTypes.brsat:
					return this.BRSatCPCost;
				case PlatformTypes.scansat:
					return this.ScanSatCPCost;
				case PlatformTypes.torpsat:
					return this.TorpSatCPCost;
				case PlatformTypes.monitorsat:
					return this.MonitorSatCPCost;
				case PlatformTypes.missilesat:
					return this.MissileSatCPCost;
				default:
					return 0;
			}
		}

		public int GetDefenseAssetCPCost(DesignInfo design)
		{
			if (design == null)
				return 0;
			if (design.IsPlatform())
			{
				PlatformTypes? platformType = design.GetPlatformType();
				if (!platformType.HasValue)
					return 1;
				return this.GetPlatformCPCost(platformType.Value);
			}
			if (design.IsMinelayer())
				return this.MineLayerCPCost;
			if (design.IsPoliceShip())
				return this.PoliceCPCost;
			if (design.IsSDB())
				return this.SDBCPCost;
			return 0;
		}
	}
}
