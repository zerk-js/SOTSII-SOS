// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GlobalSpotterRangeData
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.ShipFramework;

namespace Kerberos.Sots
{
	public class GlobalSpotterRangeData
	{
		public float[] SpotterValues = new float[5];
		public float StationLVLOffset;

		public static GlobalSpotterRangeData.SpotterValueTypes GetTypeFromShipClass(
		  ShipClass sc)
		{
			switch (sc)
			{
				case ShipClass.Cruiser:
					return GlobalSpotterRangeData.SpotterValueTypes.Cruiser;
				case ShipClass.Dreadnought:
					return GlobalSpotterRangeData.SpotterValueTypes.Dreadnought;
				case ShipClass.Leviathan:
					return GlobalSpotterRangeData.SpotterValueTypes.Leviathan;
				case ShipClass.BattleRider:
					return GlobalSpotterRangeData.SpotterValueTypes.BattleRider;
				case ShipClass.Station:
					return GlobalSpotterRangeData.SpotterValueTypes.Station;
				default:
					return GlobalSpotterRangeData.SpotterValueTypes.Cruiser;
			}
		}

		public enum SpotterValueTypes
		{
			BattleRider,
			Cruiser,
			Dreadnought,
			Leviathan,
			Station,
			NumTypes,
		}
	}
}
