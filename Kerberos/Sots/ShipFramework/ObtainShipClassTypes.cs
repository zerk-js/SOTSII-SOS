// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.ShipFramework.ObtainShipClassTypes
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;

namespace Kerberos.Sots.ShipFramework
{
	public class ObtainShipClassTypes
	{
		public static RealShipClasses GetRealShipClass(
		  ShipClass legacyShipClass,
		  BattleRiderTypes battleRiderType,
		  string filename = "")
		{
			switch (legacyShipClass)
			{
				case ShipClass.Cruiser:
					return battleRiderType != BattleRiderTypes.battlerider ? RealShipClasses.Cruiser : RealShipClasses.BattleCruiser;
				case ShipClass.Dreadnought:
					return battleRiderType != BattleRiderTypes.battlerider ? RealShipClasses.Dreadnought : RealShipClasses.BattleShip;
				case ShipClass.Leviathan:
					return RealShipClasses.Leviathan;
				case ShipClass.BattleRider:
					switch (battleRiderType)
					{
						case BattleRiderTypes.Unspecified:
						case BattleRiderTypes.nodefighter:
						case BattleRiderTypes.patrol:
						case BattleRiderTypes.scout:
						case BattleRiderTypes.spinal:
						case BattleRiderTypes.escort:
						case BattleRiderTypes.interceptor:
						case BattleRiderTypes.torpedo:
						case BattleRiderTypes.battlerider:
							return RealShipClasses.BattleRider;
						case BattleRiderTypes.boardingpod:
							return RealShipClasses.BoardingPod;
						case BattleRiderTypes.drone:
							return RealShipClasses.Drone;
						case BattleRiderTypes.escapepod:
							return RealShipClasses.EscapePod;
						case BattleRiderTypes.assaultshuttle:
							return RealShipClasses.AssaultShuttle;
						case BattleRiderTypes.biomissile:
							return RealShipClasses.Biomissile;
						default:
							throw new ArgumentOutOfRangeException(nameof(battleRiderType));
					}
				case ShipClass.Station:
					return filename.Contains("drone") ? RealShipClasses.Platform : RealShipClasses.Station;
				default:
					throw new ArgumentOutOfRangeException(nameof(legacyShipClass));
			}
		}

		public static BattleRiderTypes GetBattleRiderTypeByName(
		  ShipClass shipClass,
		  string name)
		{
			BattleRiderTypes battleRiderTypes = BattleRiderTypes.Unspecified;
			if (shipClass == ShipClass.BattleRider)
				battleRiderTypes = !name.Contains("drone") ? (name.Contains("assaultshuttle") || name.Contains("assault_shuttle") ? BattleRiderTypes.assaultshuttle : (name.Contains("boardingpod") || name.Contains("boarding_pod") ? BattleRiderTypes.boardingpod : (name.Contains("escapepod") || name.Contains("escape_pod") ? BattleRiderTypes.escapepod : (!name.Contains("patrol") ? (!name.Contains("scout") ? (!name.Contains("spinal") ? (!name.Contains("escort") ? (!name.Contains("interceptor") ? (name.Contains("biomissile") || name.Contains("bio_missile") ? BattleRiderTypes.biomissile : (!name.Contains("torpedo") ? (!name.Contains("nodefighter") ? BattleRiderTypes.battlerider : BattleRiderTypes.nodefighter) : BattleRiderTypes.torpedo)) : BattleRiderTypes.interceptor) : BattleRiderTypes.escort) : BattleRiderTypes.spinal) : BattleRiderTypes.scout) : BattleRiderTypes.patrol)))) : BattleRiderTypes.drone;
			else if (name.ToLower().Contains("bb_") || name.ToLower().Contains("bc_"))
				battleRiderTypes = BattleRiderTypes.battlerider;
			return battleRiderTypes;
		}
	}
}
