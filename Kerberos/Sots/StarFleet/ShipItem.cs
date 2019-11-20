// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.StarFleet.ShipItem
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;

namespace Kerberos.Sots.StarFleet
{
	internal class ShipItem
	{
		public readonly string Name = string.Empty;
		public readonly int ShipID;
		public readonly int DesignID;
		public int NumAdded;

		public ShipItem(ShipInfo shipInfo)
		{
			this.ShipID = shipInfo.ID;
			this.DesignID = shipInfo.DesignID;
			this.Name = shipInfo.ShipName;
		}
	}
}
