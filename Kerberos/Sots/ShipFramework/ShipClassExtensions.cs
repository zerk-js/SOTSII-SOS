// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.ShipFramework.ShipClassExtensions
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;

namespace Kerberos.Sots.ShipFramework
{
	internal static class ShipClassExtensions
	{
		public static readonly Kerberos.Sots.ShipFramework.RealShipClasses[] RealShipClasses = (Kerberos.Sots.ShipFramework.RealShipClasses[])Enum.GetValues(typeof(Kerberos.Sots.ShipFramework.RealShipClasses));

		public static ShipClass ConvertToLegacyShipClass(this Kerberos.Sots.ShipFramework.RealShipClasses value)
		{
			switch (value)
			{
				case Kerberos.Sots.ShipFramework.RealShipClasses.Cruiser:
					return ShipClass.Cruiser;
				case Kerberos.Sots.ShipFramework.RealShipClasses.Dreadnought:
					return ShipClass.Dreadnought;
				case Kerberos.Sots.ShipFramework.RealShipClasses.Leviathan:
					return ShipClass.Leviathan;
				case Kerberos.Sots.ShipFramework.RealShipClasses.BattleRider:
					return ShipClass.BattleRider;
				case Kerberos.Sots.ShipFramework.RealShipClasses.BattleCruiser:
					return ShipClass.Cruiser;
				case Kerberos.Sots.ShipFramework.RealShipClasses.BattleShip:
					return ShipClass.Dreadnought;
				case Kerberos.Sots.ShipFramework.RealShipClasses.Drone:
					return ShipClass.BattleRider;
				case Kerberos.Sots.ShipFramework.RealShipClasses.BoardingPod:
					return ShipClass.BattleRider;
				case Kerberos.Sots.ShipFramework.RealShipClasses.EscapePod:
					return ShipClass.BattleRider;
				case Kerberos.Sots.ShipFramework.RealShipClasses.AssaultShuttle:
					return ShipClass.BattleRider;
				case Kerberos.Sots.ShipFramework.RealShipClasses.Biomissile:
					return ShipClass.BattleRider;
				case Kerberos.Sots.ShipFramework.RealShipClasses.Station:
					return ShipClass.Station;
				case Kerberos.Sots.ShipFramework.RealShipClasses.Platform:
					return ShipClass.Station;
				case Kerberos.Sots.ShipFramework.RealShipClasses.SystemDefenseBoat:
					return ShipClass.Cruiser;
				default:
					throw new ArgumentOutOfRangeException(nameof(value));
			}
		}

		public static string Localize(this Kerberos.Sots.ShipFramework.RealShipClasses shipClass)
		{
			switch (shipClass)
			{
				case Kerberos.Sots.ShipFramework.RealShipClasses.Cruiser:
					return App.Localize("@SHIPCLASSES_CRUISER");
				case Kerberos.Sots.ShipFramework.RealShipClasses.Dreadnought:
					return App.Localize("@SHIPCLASSES_DREADNOUGHT");
				case Kerberos.Sots.ShipFramework.RealShipClasses.Leviathan:
					return App.Localize("@SHIPCLASSES_LEVIATHAN");
				case Kerberos.Sots.ShipFramework.RealShipClasses.BattleRider:
					return App.Localize("@SHIPCLASSES_BATTLE_RIDER");
				case Kerberos.Sots.ShipFramework.RealShipClasses.BattleCruiser:
					return App.Localize("@SHIPCLASSES_BATTLE_CRUISER");
				case Kerberos.Sots.ShipFramework.RealShipClasses.BattleShip:
					return App.Localize("@SHIPCLASSES_BATTLESHIP");
				case Kerberos.Sots.ShipFramework.RealShipClasses.Drone:
					return App.Localize("@SHIPCLASSES_DRONE");
				case Kerberos.Sots.ShipFramework.RealShipClasses.BoardingPod:
					return App.Localize("@SHIPCLASSES_BOARDING_POD");
				case Kerberos.Sots.ShipFramework.RealShipClasses.EscapePod:
					return App.Localize("@SHIPCLASSES_ESCAPE_POD");
				case Kerberos.Sots.ShipFramework.RealShipClasses.AssaultShuttle:
					return App.Localize("@SHIPCLASSES_ASSAULT_SHUTTLE");
				case Kerberos.Sots.ShipFramework.RealShipClasses.Biomissile:
					return App.Localize("@SHIPCLASSES_BIOMISSLE");
				case Kerberos.Sots.ShipFramework.RealShipClasses.Station:
					return App.Localize("@SHIPCLASSES_STATION");
				case Kerberos.Sots.ShipFramework.RealShipClasses.Platform:
					return App.Localize("@SHIPCLASSES_PLATFORM");
				case Kerberos.Sots.ShipFramework.RealShipClasses.SystemDefenseBoat:
					return App.Localize("@SHIPCLASSES_SDB");
				default:
					throw new ArgumentOutOfRangeException(nameof(shipClass));
			}
		}

		public static string LocalizeAbbr(this Kerberos.Sots.ShipFramework.RealShipClasses? shipClass)
		{
			if (!shipClass.HasValue)
				return string.Empty;
			return shipClass.Value.LocalizeAbbr();
		}

		public static string LocalizeAbbr(this Kerberos.Sots.ShipFramework.RealShipClasses shipClass)
		{
			switch (shipClass)
			{
				case Kerberos.Sots.ShipFramework.RealShipClasses.Cruiser:
					return App.Localize("@SHIPCLASSES_ABBR_CR");
				case Kerberos.Sots.ShipFramework.RealShipClasses.Dreadnought:
					return App.Localize("@SHIPCLASSES_ABBR_DN");
				case Kerberos.Sots.ShipFramework.RealShipClasses.Leviathan:
					return App.Localize("@SHIPCLASSES_ABBR_LV");
				case Kerberos.Sots.ShipFramework.RealShipClasses.BattleRider:
					return App.Localize("@SHIPCLASSES_ABBR_BR");
				case Kerberos.Sots.ShipFramework.RealShipClasses.BattleCruiser:
					return App.Localize("@SHIPCLASSES_ABBR_BC");
				case Kerberos.Sots.ShipFramework.RealShipClasses.BattleShip:
					return App.Localize("@SHIPCLASSES_ABBR_BB");
				case Kerberos.Sots.ShipFramework.RealShipClasses.Drone:
					return App.Localize("@SHIPCLASSES_ABBR_DR");
				case Kerberos.Sots.ShipFramework.RealShipClasses.BoardingPod:
					return App.Localize("@SHIPCLASSES_ABBR_BP");
				case Kerberos.Sots.ShipFramework.RealShipClasses.EscapePod:
					return App.Localize("@SHIPCLASSES_ABBR_EP");
				case Kerberos.Sots.ShipFramework.RealShipClasses.AssaultShuttle:
					return App.Localize("@SHIPCLASSES_ABBR_AS");
				case Kerberos.Sots.ShipFramework.RealShipClasses.Biomissile:
					return App.Localize("@SHIPCLASSES_ABBR_BM");
				case Kerberos.Sots.ShipFramework.RealShipClasses.Station:
					return App.Localize("@SHIPCLASSES_ABBR_SN");
				case Kerberos.Sots.ShipFramework.RealShipClasses.Platform:
					return App.Localize("@SHIPCLASSES_ABBR_PT");
				case Kerberos.Sots.ShipFramework.RealShipClasses.SystemDefenseBoat:
					return App.Localize("@SHIPCLASSES_ABBR_DB");
				default:
					throw new ArgumentOutOfRangeException(nameof(shipClass));
			}
		}
	}
}
