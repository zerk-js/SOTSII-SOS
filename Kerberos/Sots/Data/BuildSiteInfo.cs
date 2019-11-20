// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.BuildSiteInfo
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.Data
{
	internal class BuildSiteInfo : IIDProvider
	{
		public int StationID;
		public int PlanetID;
		public int ShipID;
		public int Resources;

		public int ID { get; set; }

		public int OrbitalID
		{
			get
			{
				if (this.PlanetID != 0)
					return this.PlanetID;
				return this.StationID;
			}
		}
	}
}
